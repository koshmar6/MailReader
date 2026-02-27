using MailReader.Domain.Primitives;

namespace MailReader.Domain.ValueObjects;

public sealed record ImapCredentials
{
    public string Host { get; }
    public int Port { get; }
    public string Username { get; }
    public string Password { get; }
    public bool UseSsl { get; }

    private ImapCredentials(string host, int port, string username, string password, bool useSsl)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        UseSsl = useSsl;
    }

    public static Result<ImapCredentials> Create(
        string host,
        int port,
        string username,
        string password,
        bool useSsl = true)
    {
        if (string.IsNullOrWhiteSpace(host))
            return Result<ImapCredentials>.Failure(CredentialsErrors.HostEmpty);

        if (port is < 1 or > 65535)
            return Result<ImapCredentials>.Failure(CredentialsErrors.InvalidPort);

        if (string.IsNullOrWhiteSpace(username))
            return Result<ImapCredentials>.Failure(CredentialsErrors.UsernameEmpty);

        if (string.IsNullOrWhiteSpace(password))
            return Result<ImapCredentials>.Failure(CredentialsErrors.PasswordEmpty);

        return Result<ImapCredentials>.Success(new ImapCredentials(host, port, username, password, useSsl));
    }

    // Скрываем пароль при логировании
    public override string ToString() => $"{Username}@{Host}:{Port} (SSL={UseSsl})";
}

public static class CredentialsErrors
{
    public static readonly Error HostEmpty = new("Credentials.HostEmpty", "IMAP host cannot be empty.");
    public static readonly Error InvalidPort = new("Credentials.InvalidPort", "Port must be between 1 and 65535.");
    public static readonly Error UsernameEmpty = new("Credentials.UsernameEmpty", "Username cannot be empty.");
    public static readonly Error PasswordEmpty = new("Credentials.PasswordEmpty", "Password cannot be empty.");
}
