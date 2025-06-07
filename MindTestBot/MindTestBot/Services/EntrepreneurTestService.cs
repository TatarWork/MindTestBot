using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MindTestBot.Entities;

namespace MindTestBot.Services
{
    public sealed class EntrepreneurTestService
    {
        private const string QuestionsCacheKey = "test_questions";
        private readonly IMemoryCache _cache;
        private readonly ILogger<EntrepreneurTestService> _logger;
        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        private EntrepreneurTestService(IMemoryCache cache, ILogger<EntrepreneurTestService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private static readonly Lazy<EntrepreneurTestService> _instance = new(
            () => new EntrepreneurTestService(
                new MemoryCache(new MemoryCacheOptions()), LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EntrepreneurTestService>()));

        public static EntrepreneurTestService Instance => _instance.Value;

        public async Task<List<TestQuestion>> GetQuestionsAsync(AppDbContext dbContext)
        {
            if (_cache.TryGetValue(QuestionsCacheKey, out List<TestQuestion>? cachedQuestions))
            {
                _logger.LogDebug("Retrieved {Count} questions from cache", cachedQuestions?.Count);
                return cachedQuestions ?? new List<TestQuestion>();
            }

            await _cacheLock.WaitAsync();
            try
            {
                // Double-check locking
                if (_cache.TryGetValue(QuestionsCacheKey, out cachedQuestions))
                {
                    return cachedQuestions ?? new List<TestQuestion>();
                }

                _logger.LogInformation("Loading questions from database...");
                var questions = await dbContext.TestQuestions
                    .AsNoTracking()
                    .Where(q => q.IsActive)
                    .OrderBy(q => q.Order)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .RegisterPostEvictionCallback(OnCacheEviction);

                _cache.Set(QuestionsCacheKey, questions, cacheOptions);
                _logger.LogInformation("Cached {Count} questions", questions.Count);

                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load questions from database");
                throw;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private void OnCacheEviction(object key, object? value, EvictionReason reason, object? state)
        {
            _logger.LogInformation("Cache evicted: {Key} (Reason: {Reason})", key, reason);
        }

        /// <summary>
        /// Инициализирует тестовые вопросы в БД (выполнять один раз при первом запуске)
        /// </summary>
        public async Task InitializeQuestionsAsync(AppDbContext db)
        {
            if (await db.TestQuestions.AnyAsync())
                return;

            var questions = new List<TestQuestion>
            {
                new() {
                    Text = "Ты открываешь почту и видишь письмо от налоговой. Твоя первая мысль:",
                    Order = 1,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Наверняка какая-то ерунда\" — открою потом, когда будет время"},
                        {'b', "\"Интересно, что там\" — сразу проверяю, даже если это выходной"},
                        {'c', "\"О боже, я что-то нарушила?!\" — паника, сердце колотится"}
                    }
                },
                new() {
                    Text = "Как ты реагируешь на фразу клиента \"Это слишком дорого\"?",
                    Order = 2,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Я могу сделать скидку!\" — сразу предлагаю уступку"},
                        {'b', "\"Давайте обсудим, какие именно критерии ценности для вас важны\" — включаю переговоры"},
                        {'c', "\"Наверное, я действительно завысил(а) цену...\" — начинаю сомневаться в себе"}
                    }
                },
                new() {
                    Text = "Твой подрядчик сорвал сроки. Ты:",
                    Order = 3,
                    Options = new Dictionary<char, string> {
                        {'a', "Делаю вид, что ничего не произошло — \"бывает\""},
                        {'b', "Отправляю официальную претензию по договору"},
                        {'c', "Кричу в подушку, но ему ничего не говорю — \"а вдруг обидится\""}
                    }
                },
                new() {
                    Text = "Что ты чувствуешь, когда видишь чужой успешный бизнес в твоей нише?",
                    Order = 4,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Они просто везунчики/у них блат/украли мои идеи\" — раздражение"},
                        {'b', "\"Интересно, как они это сделали?\" — изучаю их стратегию"},
                        {'c', "\"Я никогда так не смогу\" — грусть и зависть"}
                    }
                },
                new() {
                    Text = "Ты выделяешь деньги на обучение. Куда они уйдут в первую очередь?",
                    Order = 5,
                    Options = new Dictionary<char, string> {
                        {'a', "На красивый лендинг/новый логотип — \"чтобы выглядеть солидно\""},
                        {'b', "На наём ассистента или бухгалтера — \"чтобы разгрузить себя\""},
                        {'c', "На курс \"Как выйти на 1 млн\" — \"волшебную таблетку\""}
                    }
                },
                new() {
                    Text = "Как ты отмечаешь неудачи?",
                    Order = 6,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Ну вот, опять ничего не получилось!\" — шоколад и сериалы"},
                        {'b', "\"Что я могу извлечь из этого?\" — записываю уроки"},
                        {'c', "\"Это знак, что надо всё бросить\" — неделя бездействия"}
                    }
                },
                new() {
                    Text = "Твой знакомый просит \"просто проконсультировать бесплатно\". Ты:",
                    Order = 7,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Конечно!\" — трачу 2 часа своего времени"},
                        {'b', "\"У меня есть гайд за 500 руб. — могу прислать его\""},
                        {'c', "\"Я не беру деньги с друзей\" — но внутри злюсь"}
                    }
                },
                new() {
                    Text = "Что чаще всего звучит в твоей голове перед сном?",
                    Order = 8,
                    Options = new Dictionary<char, string> {
                        {'a', "\"Завтра точно начну...\""},
                        {'b', "\"По пунктам: что сделал(а) сегодня, что сделаю завтра\""},
                        {'c', "\"Почему у всех получается, а у меня нет?!\""}
                    }
                },
            };

            try
            {
                await db.TestQuestions.AddRangeAsync(questions);
                await db.SaveChangesAsync();
                _logger.LogInformation("Initialized {Count} default questions", questions.Count);
                InvalidateCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize questions");
                throw;
            }
        }

        public async Task<(int Current, int Total)> GetUserProgressAsync(AppDbContext dbContext, long chatId)
        {
            var state = await dbContext.UserTestStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ChatId == chatId);

            if (state == null)
            {
                _logger.LogDebug("No progress found for chat {ChatId}", chatId);
                return (0, 0);
            }

            var questions = await GetQuestionsAsync(dbContext);
            return (state.CurrentQuestion, questions.Count);
        }

        public void InvalidateCache()
        {
            _logger.LogInformation("Invalidating questions cache");
            _cache.Remove(QuestionsCacheKey);
        }
    }
}
