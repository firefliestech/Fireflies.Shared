namespace Fireflies.Logging.Abstractions;

public class NullLoggerFactory : IFirefliesLoggerFactory {
    public IFirefliesLogger GetLogger<T>(string? prepend = null, string? append = null) {
        return new NullLogger();
    }
}