using Microsoft.Extensions.Logging;
using MindBot.Core;
using MindBot.Core.Extensions;
using MindBot.Core.Helpers;
using MindBot.EF.Interfaces;
using MindBot.Services.Interfaces;
using MindBot.Services.Models;

namespace MindBot.Services.Services
{
    public class UserStateService : IUserStateService
    {
        private readonly IUserStateRepository _userStateRepository;
        private readonly IQuestionService _questionService;
        private readonly ILogger<UserStateService> _logger;
        private readonly Type _thisType;

        public UserStateService(IUserStateRepository userStateRepository,
            IQuestionService questionService,
            ILogger<UserStateService> logger)
        {
            _userStateRepository = userStateRepository;
            _questionService = questionService;
            _logger = logger;
            _thisType = GetType();
        }

        public async Task<UserStateModel> GetUserState(long chatId)
        {
            try
            {
                var userState = await _userStateRepository.GetUserState(chatId);

                if (userState == null)
                    throw new Exception("Не удалось получить текущую сессию пользователя");

                var result = new UserStateModel
                {
                    Answers = userState.Answers,
                    ChatId = chatId,
                    CurrentQuestion = userState.CurrentQuestion,
                    IsCompleted = userState.IsCompleted,
                    IsGetBonus = userState.IsGetBonus,
                    State = userState.State,
                    IsSendConsultNotifier = userState.IsSendConsultNotifier,
                    PhoneForConsulting = userState.PhoneForConsulting,
                };

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(GetUserState))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        public async Task RestartUserState(long chatId)
        {
            await _userStateRepository.BeginTransactionAsync();

            try
            {
                await _userStateRepository.DeleteUserState(chatId, true);
                await _userStateRepository.SaveAsync();
                await _userStateRepository.CreateUserState(chatId);
                await _userStateRepository.SaveAsync();
                await _userStateRepository.CommitAsync();
            }
            catch(Exception ex)
            {
                await _userStateRepository.RollbackAsync();

                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(RestartUserState))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        public bool CheckIsAdmin(long chatId)
        {
            return Settings.UserIdAdmin.Contains(chatId);
        }

        public async Task<(int Current, int Total)> GetUserProgressAsync(long chatId)
        {
            try
            {
                var userState = await _userStateRepository.GetUserState(chatId);

                if (userState == null)
                    throw new Exception("Не удалось получить текущую сессию пользователя");

                var questions = _questionService.GetQuestions();

                return (userState.CurrentQuestion, questions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(GetUserProgressAsync))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        public async Task AddAnswer(long chatId, char answer)
        {
            try
            {
                var userState = await _userStateRepository.GetUserState(chatId);

                if (userState == null)
                    throw new Exception("Не удалось получить текущую сессию пользователя");

                var answers = userState.Answers;

                if (answers == null)
                    answers = new List<char>();

                answers.Add(answer);

                userState.Answers = answers;

                await _userStateRepository.UpdateUserState(chatId, userState);
                await _userStateRepository.SaveAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(AddAnswer))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateUserState(long chatId, UserStateModel userState)
        {
            try
            {
                if (userState == null)
                    throw new ArgumentNullException(nameof(userState));

                var userStateEntity = await _userStateRepository.GetUserState(chatId);

                if (userStateEntity == null)
                    throw new Exception("Не удалось получить текущую сессию пользователя");

                userStateEntity.State = userState.State;
                userStateEntity.CurrentQuestion = userState.CurrentQuestion;
                userStateEntity.IsCompleted = userState.IsCompleted;
                userStateEntity.IsGetBonus = userState.IsGetBonus;
                userStateEntity.PhoneForConsulting = userState.PhoneForConsulting;

                await _userStateRepository.UpdateUserState(chatId, userStateEntity);
                await _userStateRepository.SaveAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(UpdateUserState))}: {ex.GetFullException()}");

                throw new Exception(ex.Message);
            }
        }
    }
}
