using MindBot.EF.Interfaces;
using MindBot.Services.Interfaces;
using MindBot.Services.Models;

namespace MindBot.Services.Services
{
    public class UserStateService : IUserStateService
    {
        private readonly IUserStateRepository _userStateRepository;
        private readonly IQuestionService _questionService;

        public UserStateService(IUserStateRepository userStateRepository,
            IQuestionService questionService)
        {
            _userStateRepository = userStateRepository;
            _questionService = questionService;
        }

        public async Task<UserStateModel> AddAnswer(long chatId, string answer)
        {
            throw new NotImplementedException();
        }

        public bool CheckIsAdmin(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserStateModel> CreateUserState(long chatId)
        {
            throw new NotImplementedException();
        }

        public Task<(int Current, int Total)> GetUserProgressAsync(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserStateModel> GetUserState(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserStateModel> UpdateUserState(long chatId, UserStateModel userState)
        {
            throw new NotImplementedException();
        }
    }
}
