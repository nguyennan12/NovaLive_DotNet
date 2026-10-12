using Microsoft.Extensions.Logging;

namespace NovaLive.Application.Tests.UseCases.Auth.Common;

internal sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public List<object?> Values { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception) + exception?.ToString());
            if (state is IEnumerable<KeyValuePair<string, object?>> fields)
                Values.AddRange(fields.Select(field => field.Value));
        }
    }
