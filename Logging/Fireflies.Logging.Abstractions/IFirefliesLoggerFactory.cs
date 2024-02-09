namespace Fireflies.Logging.Abstractions;

public interface IFirefliesLoggerFactory {
    public IFirefliesLogger GetLogger<T>(string? prepend = null, string? append = null);
}