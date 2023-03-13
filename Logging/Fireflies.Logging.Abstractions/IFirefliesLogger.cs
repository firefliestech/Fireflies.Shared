using System.ComponentModel;

namespace Fireflies.Logging.Abstractions;

public interface IFirefliesLogger {
    void Fatal(Func<string> message);
    void Fatal(string message);

    void Error(Exception exception, [Localizable(false)] string message);
    void Error(Exception ex, Func<string> message);
    void Error(Func<string> message);
    void Error(string message);

    void Warn(Func<string> message);
    void Warn(string message);

    void Info(string message);
    void Info(Func<string> message);

    void Debug([Localizable(false)] string message);
    void Debug(Func<string> message);

    void Trace(string message);
    void Trace(Func<string> message);
}