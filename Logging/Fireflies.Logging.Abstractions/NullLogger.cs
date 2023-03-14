namespace Fireflies.Logging.Abstractions;

internal class NullLogger : IFirefliesLogger {
    public void Fatal(Func<string> message) {
    }

    public void Fatal(string message) {
    }

    public void Error(Exception exception, string message) {
    }

    public void Error(Exception ex, Func<string> message) {
    }

    public void Error(Func<string> message) {
    }

    public void Error(string message) {
    }

    public void Warn(Func<string> message) {
    }

    public void Warn(string message) {
    }

    public void Info(string message) {
    }

    public void Info(Func<string> message) {
    }

    public void Debug(string message) {
    }

    public void Debug(Func<string> message) {
    }

    public void Trace(string message) {
    }

    public void Trace(Func<string> message) {
    }
}