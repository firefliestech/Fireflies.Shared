using Fireflies.Logging.Abstractions;
using NLog;

namespace Fireflies.Logging.NLog;

public class FirefliesNLogFactory : IFirefliesLoggerFactory {
    public IFirefliesLogger GetLogger<T>(string? prepend = null, string? append = null) {
        var typeName = GetTypeName<T>();
        return new FirefliesNLogLogger(LogManager.GetLogger(typeName), prepend, append);
    }

    private static string GetTypeName<T>() {
        var type = typeof(T);
        if(!type.IsGenericType)
            return type.Name;

        var genericArguments = type.GetGenericArguments().Select(x => x.Name).Aggregate((x1, x2) => $"{x1}, {x2}");
        return $"{type.Name[..type.Name.IndexOf("`", StringComparison.Ordinal)]}<{genericArguments}>";
    }
}