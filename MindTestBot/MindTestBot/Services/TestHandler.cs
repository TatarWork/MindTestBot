using MindTestBot.Entities;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using MindTestBot.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace MindTestBot.Services
{
    public class TestHandler : IDisposable
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TestHandler> _logger;
        private bool _disposed;
        private readonly IConfiguration _configuration;

        public TestHandler(ITelegramBotClient botClient,
            ILogger<TestHandler> logger, 
            IConfiguration configuration)
        {
            _botClient = botClient;            
            _logger = logger;
            _configuration = configuration;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Message is not { } message)
            {
                _logger.LogDebug("Received non-message update");
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            var options = optionsBuilder
                .UseNpgsql(_configuration.GetConnectionString(SettingModel.DatabaseConnectionName))
                .Options;

            using var db = new AppDbContext(options);
            var chatId = message.Chat.Id;
            var username = message.From?.Username ?? "anonymous";

            try
            {
                _logger.LogInformation("Processing message from {Username} (ChatID: {ChatId})", username, chatId);

                switch (message.Text?.Trim())
                {
                    case "/start":
                        await HandleStartCommand(db, chatId, username);
                        break;

                    case "Начать тест":
                        await HandleStartTestCommand(db, chatId, username);
                        break;

                    case "/reset":
                        await HandleResetCommand(db, chatId, username);
                        break;

                    case "/progress":
                        await HandleProgressCommand(db, chatId);
                        break;

                    default:
                        await HandleTestAnswer(db, chatId, message.Text);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing update for {Username} (ChatID: {ChatId})", username, chatId);
                await SendErrorMessage(chatId);
            }
        }

        private async Task HandleStartCommand(AppDbContext db, long chatId, string username)
        {
            _logger.LogInformation("Starting bot for {Username}", username);

            // Сбрасываем предыдущее состояние
            var existingState = await db.UserTestStates.FindAsync(chatId);
            if (existingState != null)
            {
                db.UserTestStates.Remove(existingState);
                await db.SaveChangesAsync();
            }

            // Создаем новое состояние
            var newState = new UserTestState { ChatId = chatId };
            await db.UserTestStates.AddAsync(newState);
            await db.SaveChangesAsync();

            //Приветствуем пользователя
            var message = $"я Дарья Татар, маркетолог и эксперт созданию продуктов, спасибо что откликнулись и выделили время на прохождение теста, в конце вас ждет подарок на выбор от меня, тест займет не более 10 минут вашего времени";

            var replyMarkup =
                new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Начать тест"),
                    new KeyboardButton("Позже")
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                };

            await _botClient.SendMessage(
                chatId: chatId,
                text: message,
                replyMarkup: replyMarkup);

            _logger.LogDebug("Bot welcome for {Username}", username);
        }

        private async Task HandleStartTestCommand(AppDbContext db, long chatId, string username)
        {
            // Получаем первый вопрос
            var testService = EntrepreneurTestService.Instance;
            var questions = await testService.GetQuestionsAsync(db);
            await SendQuestion(db, chatId, questions[0]);

            _logger.LogDebug("Test started for {Username}", username);
        }

        private async Task HandleResetCommand(AppDbContext db, long chatId, string username)
        {
            _logger.LogInformation("Resetting test for {Username}", username);

            var state = await db.UserTestStates.FindAsync(chatId);
            if (state != null)
            {
                db.UserTestStates.Remove(state);
                await db.SaveChangesAsync();
            }

            await _botClient.SendMessage(
                chatId: chatId,
                text: "Тест сброшен. Нажмите /start чтобы начать заново.",
                replyMarkup: new ReplyKeyboardRemove());

            _logger.LogDebug("Test reset for {Username}", username);
        }

        private async Task HandleProgressCommand(AppDbContext db, long chatId)
        {
            var testService = EntrepreneurTestService.Instance;
            var (current, total) = await testService.GetUserProgressAsync(db, chatId);
            await _botClient.SendMessage(
                chatId: chatId,
                text: $"Прогресс: {current}/{total} вопросов",
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task HandleTestAnswer(AppDbContext db, long chatId, string? answer)
        {
            var state = await db.UserTestStates.FindAsync(chatId);
            if (state == null || state.IsCompleted)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Начните тест с команды /start");
                return;
            }

            if (string.IsNullOrEmpty(answer))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Пожалуйста, выберите вариант ответа");
                return;
            }

            var testService = EntrepreneurTestService.Instance;
            var questions = await testService.GetQuestionsAsync(db);
            var currentQuestion = questions[state.CurrentQuestion - 1];

            if (!currentQuestion.Options.ContainsKey(answer[0]))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "⚠️ Пожалуйста, используйте кнопки для ответа");
                return;
            }

            // Сохраняем ответ
            state.AddAnswer(answer[0]);

            _logger.LogDebug("Saved answer '{Answer}' for question {QuestionNumber}",
                answer, state.CurrentQuestion);

            // Проверяем завершение теста
            if (state.CurrentQuestion >= questions.Count)
            {
                state.IsCompleted = true;

                db.Update(state);
                await db.SaveChangesAsync();

                var loadingMessage = await _botClient.SendMessage(
                chatId: chatId,
                text: "Нейросеть анализирует ответы...",
                replyMarkup: new ReplyKeyboardRemove());

                await Task.Delay(4000);
                await _botClient.DeleteMessage(chatId, loadingMessage.MessageId);

                var messageResult = @$"🎉 Спасибо, что приняли участие в исследовании это очень ценно для меня, в качестве благодарности выберите любой подарок:

                                        1 книга «ошибки, которые продают» 
                                        2 консультация 15 минут - как нанять нейросети на работу
                                        3 доступ в закрытый канал «бизнес без игрушек»";

                var getResultBonusKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("1", "book"),
                    InlineKeyboardButton.WithCallbackData("2", "consulting"),
                    InlineKeyboardButton.WithCallbackData("3", "vipchannel"),
                });

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: messageResult,
                    replyMarkup: getResultBonusKeyboard);

                _logger.LogInformation("Test completed for chat {ChatId}", chatId);
            }
            else
            {
                // Переходим к следующему вопросу
                state.CurrentQuestion++;

                db.Update(state);
                await db.SaveChangesAsync();

                await SendQuestion(db, chatId, questions[state.CurrentQuestion - 1]);
            }
        }

        private async Task SendQuestion(AppDbContext db, long chatId, TestQuestion question)
        {
            var buttons = question.Options
                .OrderBy(o => o.Key)
                .Select(o => new KeyboardButton(o.Key.ToString()))
                .Chunk(2);

            var (current, total) = await EntrepreneurTestService.Instance.GetUserProgressAsync(db, chatId);

            if (question.Order != current)
                throw new Exception("Текущий вопрос не соотносится с полученным из базы данных по порядку");

            var message = $"❓ Вопрос {current}/{total}:\n\n{question.Text}";

            await _botClient.SendMessage(
                chatId: chatId,
                text: message,
                replyMarkup: new ReplyKeyboardMarkup(buttons)
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                });

            _logger.LogDebug("Sent question {QuestionNumber} to chat {ChatId}", question.Order, chatId);
        }

        private async Task SendErrorMessage(long chatId)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "⚠️ Произошла ошибка. Пожалуйста, попробуйте позже.",
                replyMarkup: new ReplyKeyboardRemove());
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~TestHandler()
        {
            Dispose();
        }
    }
}
