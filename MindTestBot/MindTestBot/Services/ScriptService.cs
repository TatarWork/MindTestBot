using Microsoft.EntityFrameworkCore;
using MindTestBot.Entities;
using MindTestBot.Enums;
using MindTestBot.Extensions;
using MindTestBot.Helpers;
using MindTestBot.Interfaces;
using MindTestBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MindTestBot.Services
{
    public class ScriptService : IScriptService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<ScriptService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Type _thisType;

        public ScriptService(ITelegramBotClient botClient,
            ILogger<ScriptService> logger,
            IConfiguration configuration)
        {
            _botClient = botClient;
            _logger = logger;
            _configuration = configuration;
            _thisType = GetType();
        }

        public Task CheckGetBonus(AppDbContext db, long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAnalysisResultAsync(AppDbContext db, long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var options = optionsBuilder
                    .UseNpgsql(_configuration.GetConnectionString(SettingModel.DatabaseConnectionName))
                    .Options;

                using var db = new AppDbContext(options);
                var chatId = callbackQuery.Message!.Chat.Id;                

                var callbackData = callbackQuery.Data;

                if (callbackData == ResultBonusEnum.Consulting.ToCodeValue())
                {

                }

                if(callbackData == ResultBonusEnum.VipChannel.ToCodeValue())
                {

                }                

                await _botClient.AnswerCallbackQuery(callbackQuery.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during operation {LogHelper.GetMethodName(_thisType, nameof(HandleCallbackQueryAsync))}: {ex.GetFullException()}");

                throw new Exception($"An exception occurred while trying to process a Telegram callback request: {ex.Message}");
            }
        }

        public async Task HandleMessageAsync(Message message)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var options = optionsBuilder
                    .UseNpgsql(_configuration.GetConnectionString(SettingModel.DatabaseConnectionName))
                    .Options;

                using var db = new AppDbContext(options);
                var chatId = message.Chat.Id;
                var username = message.From?.Username ?? "anonymous";

                _logger.LogInformation("Processing message from {Username} (ChatID: {ChatId})", username, chatId);

                switch (message.Text?.Trim())
                {
                    case "/start":
                        await HandleStartTestCommand(db, chatId, username);
                        break;

                    case "Начать тест":
                        await HandleStartTestCommand(db, chatId, username);
                        break;

                    default:
                        await HandleTestAnswerCommand(db, chatId, message.Text);
                        break;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error during operation {LogHelper.GetMethodName(_thisType, nameof(HandleMessageAsync))}: {ex.GetFullException()}");

                throw new Exception($"An exception occurred while trying to process a message: {ex.Message}");
            }
        }

        public async Task HandleStartTestCommand(AppDbContext db, long chatId, string username)
        {
            // Получаем первый вопрос
            var testService = EntrepreneurTestService.Instance;
            var questions = await testService.GetQuestionsAsync(db);
            await SendQuestion(db, chatId, questions[0]);

            _logger.LogDebug("Test started for {Username}", username);            
        }

        public async Task HandleTestAnswerCommand(AppDbContext db, long chatId, string? answer)
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

            _logger.LogDebug("Saved answer '{Answer}' for question {QuestionNumber}, chatId: {ChatId}",
                answer, state.CurrentQuestion, chatId);

            _logger.LogInformation("Saved answer '{Answer}' for question {QuestionNumber}, chatId: {ChatId}",
                answer, state.CurrentQuestion, chatId);

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

                                        1 консультация 15 минут - как нанять нейросети на работу
                                        2 доступ в закрытый канал «бизнес без игрушек»";

                var getResultBonusKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("1", "consulting"),
                    InlineKeyboardButton.WithCallbackData("2", "vipchannel"),
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

        public Task SendErrorMessage(long chatId)
        {
            throw new NotImplementedException();
        }

        public Task SendWelcomeMessageAdminAsync(AppDbContext db, long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public async Task SendWelcomeMessageAsync(AppDbContext db, long chatId, string username)
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
                    new KeyboardButton("Начать тест")
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

        private void InitUserState(CallbackQuery? callbackQuery, UserTestState userState)
        {
            userState.Username = callbackQuery?.From.Username;
            userState.FirstName = callbackQuery?.From.FirstName;
            userState.LastName = callbackQuery?.From.LastName;
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
    }
}
