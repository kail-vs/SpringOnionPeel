namespace SpringOnion.Contracts;

public class RelaySendRequest
{
    public string ConversationId { get; set; } = null!;
    public string ToUserId { get; set; } = null!;
    public string SenderUserId { get; set; } = null!;
    public string CipherTextBase64 { get; set; } = null!;
    public string? ContentType { get; set; }
    public string LocalMessageId { get; set; } = null!;
}

public class RelayIncomingMessage
{
    public string ConversationId { get; set; } = null!;
    public string SenderUserId { get; set; } = null!;
    public string CipherTextBase64 { get; set; } = null!;
    public string? ContentType { get; set; }
    public string? RemoteMessageId { get; set; }
    public string? LocalMessageIdFromSender { get; set; }
}

public class RelayAck
{
    public string LocalMessageId { get; set; } = null!;
    public string? RemoteMessageId { get; set; }
}
