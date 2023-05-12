using System.Collections.Concurrent;
using System.Reflection;

namespace Fireflies.Utility.Reflection;

public static class ReflectionExtensions {
    private static readonly ConcurrentDictionary<Type, Type?> IsTaskCache = new();

    public static Type DiscardTaskFromReturnType(this MethodInfo methodInfo) {
        return methodInfo.ReturnType.DiscardTask();
    }

    public static Type DiscardTask(this Type type) {
        if(type.IsGenericType) {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if(genericTypeDefinition == typeof(Task<>) || genericTypeDefinition == typeof(ValueTask<>) || genericTypeDefinition == typeof(IAsyncEnumerable<>)) {
                return type.GetGenericArguments()[0];
            }
        }

        return type;
    }

    public static bool IsTask(this Type type, out Type? returnType) {
        returnType = type;

        var cachedEntry = IsTaskCache.GetOrAdd(type, _ => {
            if(type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Task<>) || type.GetGenericTypeDefinition() == typeof(ValueTask<>)))
                return type.GetGenericArguments()[0];

            return null;
        });

        if(cachedEntry == null)
            return false;

        returnType = cachedEntry;
        return true;
    }

    public static bool IsTask(this Type type) {
        return IsTask(type, out _);
    }

    public static bool IsAsyncEnumerable(this Type type, out Type? returnType) {
        returnType = type;

        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)) {
            returnType = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    public static bool IsAsyncEnumerable(this Type type) {
        return IsAsyncEnumerable(type, out _);
    }

    public static bool IsTaskOrAsyncEnumerable(this Type type, out Type? genericType) {
        if(IsTask(type, out genericType) || IsAsyncEnumerable(type, out genericType))
            return true;

        genericType = type;
        return false;
    }
}