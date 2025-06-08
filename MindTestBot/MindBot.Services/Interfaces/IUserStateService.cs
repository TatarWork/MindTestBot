using MindBot.Services.Models;

namespace MindBot.Services.Interfaces
{
    public interface IUserStateService
    {
        Task<UserStateModel> GetUserState(long chatId);

        Task<UserStateModel> CreateUserState(long chatId);

        Task<UserStateModel> AddAnswer(long chatId, string answer);

        Task<UserStateModel> UpdateUserState(long chatId, UserStateModel userState);

        bool CheckIsAdmin(long chatId);

        Task<(int Current, int Total)> GetUserProgressAsync(long chatId);
    }
}
