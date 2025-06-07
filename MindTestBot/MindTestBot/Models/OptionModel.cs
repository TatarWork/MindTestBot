namespace MindTestBot.Models
{
    public class OptionModel
    {
        public required string Token { get; set; }

        public string WebhookUrl { get; set; } = string.Empty;

        public string ChannelName { get; set; } = string.Empty;

        public string ChannelInviteLink { get; set; } = string.Empty;

        public string BonusResultLink { get; set; } = string.Empty;

        public string BonusForInviteLink { get; set; } = string.Empty;

        public string BonusBook { get; set; } = string.Empty;
    }
}
