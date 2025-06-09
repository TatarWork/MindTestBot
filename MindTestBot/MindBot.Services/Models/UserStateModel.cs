using MindBot.Core.Enums;

namespace MindBot.Services.Models
{
    public class UserStateModel
    {
        public long ChatId { get; set; }

        public int CurrentQuestion { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsGetBonus { get; set; }

        public string? Result { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Username { get; set; }

        public UserStateEnum State { get; set; }

        public List<char> Answers { get; set; } = new List<char>();
    }
}
