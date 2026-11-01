namespace ObeAunQa.Modules.Knowledge.Domain;
public sealed class ChatThread
{
    public Guid Id { get; set; }
    public long OwnerId { get; set; }
    public string Title { get; set; } = "Cuộc trò chuyện mới";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

