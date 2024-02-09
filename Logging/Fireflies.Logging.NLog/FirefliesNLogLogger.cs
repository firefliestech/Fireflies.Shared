using Fireflies.Logging.Abstractions;
using NLog;

namespace Fireflies.Logging.NLog;

public class FirefliesNLogLogger(Logger logger, string? prepend, string? append) : IFirefliesLogger {
    public void Fatal(Func<string> message) {
        if(logger.IsFatalEnabled)
            Fatal(message());
    }

    public void Fatal(string message) {
        logger.Fatal(AddFixedStrings(message));
    }

    public void Error(Exception ex, Func<string> message) {
        if(logger.IsErrorEnabled)
            Error(ex, message());
    }

    public void Error(Exception exception, string message) {
        logger.Error(exception, AddFixedStrings(message));
    }

    public void Error(string message) {
        logger.Error(AddFixedStrings(message));
    }

    public void Error(Func<string> message) {
        if(logger.IsErrorEnabled)
            Error(message());
    }

    public void Warn(string message) {
        logger.Warn(AddFixedStrings(message));
    }

    public void Warn(Func<string> message) {
        if(logger.IsWarnEnabled)
            Warn(message());
    }

    public void Info(string message) {
        logger.Info(AddFixedStrings(message));
    }

    public void Info(Func<string> message) {
        if(logger.IsInfoEnabled)
            Info(message());
    }

    public void Trace(string message) {
        logger.Trace(AddFixedStrings(message));
    }

    public void Debug(Func<string> message) {
        if(logger.IsDebugEnabled)
            Debug(message());
    }

    public void Debug(string message) {
        logger.Debug(AddFixedStrings(message));
    }

    public void Trace(Func<string> message) {
        if(logger.IsTraceEnabled)
            Trace(message());
    }

    private string AddFixedStrings(string message) {
        return $"{prepend}{message}{append}";
    }
}