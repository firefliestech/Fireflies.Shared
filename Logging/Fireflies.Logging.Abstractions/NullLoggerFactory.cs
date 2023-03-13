namespace Fireflies.Logging.Abstractions;

public class NullLoggerFactory : IFirefliesLoggerFactory {
    public IFirefliesLogger GetLogger<T>() {
        return new NullLogger();
    }
}