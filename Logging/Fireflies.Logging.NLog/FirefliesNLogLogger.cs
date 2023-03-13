using Fireflies.Logging.Abstractions;
using NLog;

namespace Fireflies.Logging.NLog;

public class FirefliesNLogLogger : IFirefliesLogger {
    private readonly Logger _logger;

    public FirefliesNLogLogger(Logger logger) {
        _logger = logger;
    }

    public void Fatal(Func<string> message) {
        _logger.Fatal(message());
    }

    public void Fatal(string message) {
        _logger.Fatal(message);
    }

    public void Error(Exception exception, string message) {
        _logger.Error(exception, message);
    }

    public void Error(Exception ex, Func<string> message) {
        _logger.Error(ex, message());
    }

    public void Error(Func<string> message) {
        _logger.Error(message());
    }

    public void Error(string message) {
        _logger.Error(message);
    }

    public void Warn(string message) {
        _logger.Warn(message);
    }

    public void Warn(Func<string> message) {
        _logger.Warn(message());
    }

    public void Info(string message) {
        _logger.Info(message);
    }

    public void Info(Func<string> message) {
        _logger.Info(message());
    }

    public void Trace(string message) {
        _logger.Trace(message);
    }

    public void Debug(Func<string> message) {
        _logger.Debug(message());
    }

    public void Debug(string message) {
        _logger.Debug(message);
    }

    public void Trace(Func<string> message) {
        _logger.Trace(message());
    }
}