using Microsoft.Extensions.Logging;

namespace Stockroom.Infrastructure.Persistence;

internal static partial class DatabaseInitializerLog
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Information, Message = "Database schema created")]
    public static partial void SchemaCreated(this ILogger logger);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Database schema already present")]
    public static partial void SchemaAlreadyPresent(this ILogger logger);
}
