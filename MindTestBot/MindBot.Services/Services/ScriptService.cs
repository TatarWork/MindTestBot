using MindBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MindBot.Services.Services
{
    public class ScriptService : IScriptService
    {

        public ScriptService()
        {
            
        }
        public Task CheckGetBonus(long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAnalysisResultAsync(long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            throw new NotImplementedException();
        }

        public Task HandleMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task HandleStartTestCommand(long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public Task HandleTestAnswerCommand(long chatId, string? answer)
        {
            throw new NotImplementedException();
        }

        public Task SendErrorMessage(long chatId)
        {
            throw new NotImplementedException();
        }

        public Task SendWelcomeMessageAdminAsync(long chatId, string username)
        {
            throw new NotImplementedException();
        }

        public Task SendWelcomeMessageAsync(long chatId, string username)
        {
            throw new NotImplementedException();
        }
    }
}
