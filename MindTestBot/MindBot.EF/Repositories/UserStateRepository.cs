using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MindBot.Core;
using MindBot.Core.Enums;
using MindBot.Core.Extensions;
using MindBot.Core.Helpers;
using MindBot.EF.Entities;
using MindBot.EF.Interfaces;

namespace MindBot.EF.Repositories
{
    public class UserStateRepository : IUserStateRepository
    {
        private readonly ILogger<UserStateRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly Type _thisType;

        private readonly MindBotDbContext _db;
        private IDbContextTransaction? _dbTransaction { get; set; } = null;

        public UserStateRepository(ILogger<UserStateRepository> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var optionsBuilder = new DbContextOptionsBuilder<MindBotDbContext>();

            var options = optionsBuilder
                .UseNpgsql(_configuration.GetConnectionString(Settings.DatabaseConnectionName))
                .Options;

            _db = new MindBotDbContext(options);

            _thisType = GetType();            
        }

        public async Task<UserStateEntity?> GetUserState(long chatId)
        {
            try
            {
                var result = await _db.UserStates
                    .SingleOrDefaultAsync(x => x.ChatId == chatId &&
                        x.IsDeleted == false);

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(GetUserState))}: {ex.GetFullException()}");

                throw new Exception($"Не удалось получить состояние пользователя из БД: {ex.Message}");
            }
        }

        public async Task CreateUserState(long chatId)
        {
            try
            {
                var existEntity = await GetUserState(chatId);

                if (existEntity != null)
                    throw new Exception("Невозможно создать новое состояние пользователя, в базе есть актуальная запись");

                var entity = new UserStateEntity
                {
                    ChatId = chatId,
                    State = UserStateEnum.WelcomeMessage
                };

                await _db.AddAsync(entity);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(CreateUserState))}: {ex.GetFullException()}");

                throw new Exception($"Не удалось создать новое состояние пользователя в БД: {ex.Message}");
            }
        }

        public async Task UpdateUserState(long chatId, UserStateEntity entity)
        {
            try
            {
                if (entity == null)
                    throw new Exception($"Не передан объект в качесве параметра {entity}");

                var existEntity = await GetUserState(chatId);

                if (existEntity == null)
                    throw new Exception("Не удалось получить состояние пользователя из базы данных");

                existEntity.State = entity.State;
                existEntity.Answers = entity.Answers;
                existEntity.CurrentQuestion = entity.CurrentQuestion;
                existEntity.IsCompleted = entity.IsCompleted;
                existEntity.IsGetBonus = entity.IsGetBonus;
                existEntity.UpdatedAt = DateTime.Now;

                _db.Update(existEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(UpdateUserState))}: {ex.GetFullException()}");

                throw new Exception($"Не удалось обновить состояние пользователя в БД: {ex.Message}");
            }
        }

        public async Task DeleteUserState(long chatId, bool forseDelete = false)
        {
            try
            {
                var existEntity = await GetUserState(chatId);

                if (existEntity == null)
                    return;

                if (forseDelete)
                {
                    _db.Remove(existEntity);
                }
                else
                {
                    existEntity.IsDeleted = true;

                    _db.Update(existEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в методе {LogHelper.GetMethodName(_thisType, nameof(UpdateUserState))}: {ex.GetFullException()}");

                throw new Exception($"Не удалось обновить состояние пользователя в БД: {ex.Message}");
            }
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _dbTransaction = await _db.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_dbTransaction != null)
            {
                await _dbTransaction.CommitAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_dbTransaction != null)
            {
                await _dbTransaction.RollbackAsync();
            }
        }
    }
}
