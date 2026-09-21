using System.ComponentModel.DataAnnotations;

namespace SportsClubApi.Dtos;

// Who a manually-sent notification goes to.
public enum NotificationAudience
{
    Players,
    Volunteers,
    Everyone
}

public class SendNotificationRequest
{
    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public NotificationAudience Audience { get; set; } = NotificationAudience.Players;
}
