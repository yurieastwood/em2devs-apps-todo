namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// An invite link for a guild with a unique token and expiration date.
/// </summary>
public sealed record GuildInviteLink
{
    public const int DefaultExpiryDays = 7;

    public GuildId GuildId { get; }
    public string Token { get; }
    public DateOnly ExpiresOn { get; }

    public GuildInviteLink(GuildId guildId, string token, DateOnly expiresOn)
    {
        ArgumentNullException.ThrowIfNull(guildId);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exceptions.DomainException("Invite link token cannot be empty.");
        }

        GuildId = guildId;
        Token = token;
        ExpiresOn = expiresOn;
    }

    public static GuildInviteLink Create(GuildId guildId, DateOnly today)
    {
        string token = Guid.NewGuid().ToString("N");
        DateOnly expiresOn = today.AddDays(DefaultExpiryDays);
        return new GuildInviteLink(guildId, token, expiresOn);
    }

    public bool IsExpired(DateOnly today) => today > ExpiresOn;
}
