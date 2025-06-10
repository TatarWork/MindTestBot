using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MindBot.Core;
using MindBot.Core.Enums;
using MindBot.Core.Extensions;
using MindBot.Core.Helpers;
using MindBot.Core.Options;
using MindBot.Services.Interfaces;
using MindBot.Services.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MindBot.Services.Services
{
    public class ScriptService : IScriptService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<ScriptService> _logger;
        private readonly IUserStateService _userStateService;
        private readonly IQuestionService _questionService;
        private readonly TelegramOption _telegramOption;
        private readonly Type _thisType;

        public ScriptService(ITelegramBotClient botClient,
            ILogger<ScriptService> logger,
            IUserStateService userStateService,
            IQuestionService questionService,
            IOptions<TelegramOption> telegramOptions)
        {
            _botClient = botClient;
            _logger = logger;
            _userStateService = userStateService;
            _questionService = questionService;
            _telegramOption = telegramOptions.Value;
            _thisType = GetType();
        }

        public async Task HandleMessageAsync(Message message)
        {
            try
            {
                var chatId = message.Chat.Id;

                switch (message.Text?.Trim())
                {
                    /// Приветствие
                    case Settings.CommandBotStart:
                        await SendCommandWelcome(chatId);
                        break;

                    /// Запуск тестирования
                    case Settings.CommandTestStart:
                        await SendCommandTestStart(chatId);
                        break;

                    /// Обработка ответов в ходе тестирования
                    default:
                        await SendCommandTestAnswer(chatId, message.Text);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(HandleMessageAsync))}: {ex.GetFullException()}");

                throw new Exception($"Возникла ошибка при обработке сообщений Телеграм: {ex.Message}");
            }
        }

        #region Welcome
        public async Task SendCommandWelcome(long chatId)
        {
            try
            {
                _logger.LogInformation("Запуск бота для пользователя: {chatId}", chatId);

                await _userStateService.RestartUserState(chatId);

                if (_userStateService.CheckIsAdmin(chatId))
                {
                    await SendMessageWelcomeAdmin(chatId);
                }
                else
                {
                    await SendMessageWelcomeUser(chatId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(SendCommandWelcome))}: {ex.GetFullException()}");

                throw new Exception($"Не удалось отправить сообщение-приветствие при запуску бота: {ex.Message}");
            }
        }

        public async Task SendMessageWelcomeUser(long chatId)
        {
            try
            {
                var replyMarkup =
                    new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton(Settings.CommandTestStart),
                    })
                    {
                        ResizeKeyboard = true,
                    };

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: SystemEnum.WelcomeMessageUser.ToStringValue(),
                    replyMarkup: replyMarkup);

                _logger.LogInformation("Приветствие пользователя: {chatId}", chatId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessageWelcomeAdmin(long chatId)
        {
            try
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: SystemEnum.WelcomeMessageAdmin.ToStringValue());

                _logger.LogInformation("Приветствие пользователя с ролью администратор: {chatId}", chatId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion Welcome

        #region Testing
        public async Task SendCommandTestStart(long chatId)
        {
            try
            {
                var questions = _questionService.GetQuestions();

                if (questions.Any() == false)
                    throw new Exception("Не удалось получить список вопросов для тестирования");

                var userState = await _userStateService.GetUserState(chatId);

                userState.State = UserStateEnum.Questions;

                await _userStateService.UpdateUserState(chatId, userState);

                /// Отправляем первый вопрос теста пользователю

                await SendCommandQuestion(chatId, questions[0]);

                _logger.LogInformation("Тест начат для пользователя: {chatId}", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(SendCommandTestStart))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        public async Task SendCommandTestAnswer(long chatId, string? answer)
        {
            try
            {
                var userState = await _userStateService.GetUserState(chatId);

                if (userState == null || userState.IsCompleted)
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

                var questions = _questionService.GetQuestions();
                var currentQuestion = questions[userState.CurrentQuestion - 1];

                if (!currentQuestion.Options.ContainsKey(answer[0]))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "⚠️ Пожалуйста, используйте кнопки для ответа");
                    return;
                }

                /// Сохраняем полученный ответ

                await _userStateService.AddAnswer(chatId, answer[0]);

                _logger.LogInformation("Сохранен ответ '{Answer}' для вопроса {QuestionNumber} пользователя {chatId}",
                    answer, userState.CurrentQuestion, chatId);

                /// Проверяем завершение теста

                if (userState.CurrentQuestion >= questions.Count)
                {
                    userState.IsCompleted = true;
                    userState.State = UserStateEnum.Result;

                    await _userStateService.UpdateUserState(chatId, userState);

                    var loadingMessage = await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Нейросеть анализирует ответы...",
                        replyMarkup: new ReplyKeyboardRemove());

                    await Task.Delay(4000);
                    await _botClient.DeleteMessage(chatId, loadingMessage.MessageId);

                    var messageResult = "🎉 Спасибо, что приняли участие в исследовании это очень ценно для меня, в качестве благодарности выберите любой подарок:" +
                        "\n\n1 безоплатная консультация 15 минут - как нанять нейросети на работу\n2 доступ в закрытый канал «бизнес без игрушек»";

                    var getResultBonusKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(BonusTypeEnum.Consulting.ToStringValue(), BonusTypeEnum.Consulting.ToCodeValue()),
                        InlineKeyboardButton.WithCallbackData(BonusTypeEnum.VipChannel.ToStringValue(), BonusTypeEnum.VipChannel.ToCodeValue())
                    });

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: messageResult,
                        replyMarkup: getResultBonusKeyboard);

                    _logger.LogInformation("Тест заверешен для пользователя {ChatId}", chatId);
                }
                else
                {
                    /// Переходим к следующему вопросу

                    userState.CurrentQuestion++;

                    await _userStateService.UpdateUserState(chatId, userState);

                    await SendCommandQuestion(chatId, questions[userState.CurrentQuestion - 1]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(SendCommandTestAnswer))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        private async Task SendCommandQuestion(long chatId, QuestionModel question)
        {
            try
            {
                var buttons = question.Options
                   .OrderBy(o => o.Key)
                   .Select(o => new KeyboardButton(o.Key.ToString()))
                   .Chunk(2);

                var (current, total) = await _userStateService.GetUserProgressAsync(chatId);

                if (question.Order != current)
                    throw new Exception("Текущий вопрос не соотносится с полученным из базы данных по порядку");

                var answers = string.Empty;

                foreach(var item in question.Options)
                {
                    answers += item.Key.ToString() + ")  " + item.Value + "\n\n";
                }

                var message = $"❓ Вопрос {current}/{total}:\n\n{question.Text}\n\n{answers}";

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: message,
                    replyMarkup: new ReplyKeyboardMarkup(buttons)
                    {
                        ResizeKeyboard = true,
                    });

                _logger.LogInformation("Отправлен вопрос {QuestionNumber} в чат с ИД: {ChatId}", question.Order, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(SendCommandQuestion))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }
        #endregion Testing

        #region BonusResult
        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            try
            {
                var chatId = callbackQuery.Message!.Chat.Id;
                var callbackData = callbackQuery.Data;
                var linkToUser = $"tg://user?id={chatId}";

                var userState = await _userStateService.GetUserState(chatId);

                if (userState.IsGetBonus)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Вы уже выбрали бонус, благодарю за прохождение теста",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                else
                {
                    if (callbackData == BonusTypeEnum.Consulting.ToCodeValue())
                    {
                        userState.IsGetBonus = true;
                        userState.State = UserStateEnum.SendBonusConsult;

                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Вы выбрали консультацию, в течение получаса вам напишет Дарья Татар, чтобы договориться об удобном для вас времени",
                            replyMarkup: new ReplyKeyboardRemove());

                        /// Уведомление админам о консультации

                        foreach (var adminId in Settings.UserIdAdmin)
                        {                            
                            var message = $"👤 Пользователь {linkToUser} \n хочет консультацию";

                            await _botClient.SendMessage(
                                chatId: adminId,
                                text: message);
                        }

                        _logger.LogInformation($"Пользователь выбрал бонус \"консультация\" chatId: {userState.ChatId}");
                    }
                    else if (callbackData == BonusTypeEnum.VipChannel.ToCodeValue())
                    {
                        userState.IsGetBonus = true;
                        userState.State = UserStateEnum.SendBonusVipChannel;

                        var subscribeVipChannelKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithUrl("Перейти", _telegramOption.ChannelInviteLink)
                        });

                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "Закрытый телеграм канал «бизнес без игрушек»",
                            replyMarkup: subscribeVipChannelKeyboard);

                        /// Уведомление админам о консультации

                        foreach (var adminId in Settings.UserIdAdmin)
                        {
                            var message = $"👤 Пользователь {linkToUser} \n получил ссылку на закрытый телеграм-канал";

                            await _botClient.SendMessage(
                                chatId: adminId,
                                text: message);
                        }

                        _logger.LogInformation($"Пользователь выбрал бонус \"закрытый телеграм канал\" chatId: {userState.ChatId}");
                    }
                    else
                    {
                        throw new Exception("Возникла ошибка при отправке команды получения бонусов по итогам тестирования");
                    }
                }

                await _userStateService.UpdateUserState(chatId, userState);
                await _botClient.AnswerCallbackQuery(callbackQuery.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(HandleCallbackQueryAsync))}: {ex.GetFullException()}");

                throw new Exception($"Возникла ошибка при обработке коллбеков команд Телеграм: {ex.Message}");
            }
        }
        #endregion BonusResult
    }
}
