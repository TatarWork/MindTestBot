using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MindBot.Core.Extensions;
using MindBot.Core.Helpers;
using MindBot.EF;
using Telegram.Bot;

namespace MindBot.Services.BackgroundServices
{
    public class ConsultationNotifierBackgroundService : BackgroundService
    {
        private readonly ILogger<ConsultationNotifierBackgroundService> _logger;
        private readonly Type _thisType;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotClient _botClient;

        public ConsultationNotifierBackgroundService(ILogger<ConsultationNotifierBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            ITelegramBotClient botClient)
        {
            _botClient = botClient;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _thisType = GetType();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("ConsultationNotifierBackgroundService запущен");

                await CheckIsSendConsultNotifyEveryHour(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(ExecuteAsync))}: {ex.GetFullException()}");
            }
        }

        private async Task CheckIsSendConsultNotifyEveryHour(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetService<MindBotDbContext>();

                    if (db == null)
                        throw new Exception($"Не удалось получить контекст базы данных в фоновой задаче {nameof(ConsultationNotifierBackgroundService)}");

                    var listUserForNotifier = await db.UserStates
                        .Where(x => x.IsSendConsultNotifier == false &&
                            (x.State == Core.Enums.UserStateEnum.Result ||
                                x.State == Core.Enums.UserStateEnum.SendBonusVipChannel))
                        .ToListAsync();

                    foreach (var userForNotifier in listUserForNotifier)
                    {
                        var dbTransaction = await db.Database.BeginTransactionAsync();

                        try
                        {
                            var chatInfo = await _botClient.GetChat(userForNotifier.ChatId);
                            var username = chatInfo.FirstName;
                            var message = @"У меня есть 1 свободное место на разбор твоей ситуации, где мы построим план - как за лето сделать доход х2 за лето 2025. Если хочешь участвовать — отвечай «Хочу». Стоимость — 3000₽.";

                            await _botClient.SendMessage(chatId: userForNotifier.ChatId,
                                text: message);

                            userForNotifier.IsSendConsultNotifier = true;
                            db.Update(userForNotifier);

                            await db.SaveChangesAsync();
                            await dbTransaction.CommitAsync();
                        }
                        catch(Exception ex)
                        {
                            await dbTransaction.RollbackAsync();

                            throw new Exception(ex.Message, ex);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.GetFullException());

                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
        }
    }
}
