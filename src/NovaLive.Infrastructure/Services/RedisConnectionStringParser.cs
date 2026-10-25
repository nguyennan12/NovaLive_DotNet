using StackExchange.Redis;

namespace NovaLive.Infrastructure.Services;

public static class RedisConnectionStringParser
{
    public static ConfigurationOptions Parse(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || (uri.Scheme != "redis" && uri.Scheme != "rediss"))
        {
            return ConfigurationOptions.Parse(connectionString);
        }

        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Ssl = uri.Scheme == "rediss"
        };
        options.EndPoints.Add(uri.Host, uri.IsDefaultPort ? 6379 : uri.Port);

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var credentials = Uri.UnescapeDataString(uri.UserInfo).Split(':', 2);
            if (credentials.Length == 2)
            {
                options.User = credentials[0];
                options.Password = credentials[1];
            }
            else
            {
                options.Password = credentials[0];
            }
        }

        var database = uri.AbsolutePath.Trim('/');
        if (int.TryParse(database, out var databaseNumber))
            options.DefaultDatabase = databaseNumber;

        return options;
    }
}
