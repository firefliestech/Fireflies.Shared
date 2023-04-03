using System.Collections.Concurrent;
using System.Reflection;

namespace Fireflies.Utility.Reflection;

public static class AttributeReflectionExtensions {
    private static ConcurrentDictionary<MemberInfo, AttributeCache> _memberCache = new();
    private static ConcurrentDictionary<ParameterInfo, AttributeCache> _parameterCache = new();

    public static bool HasCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute {
        return memberInfo.HasCustomAttribute<T>(out _);
    }

    public static bool HasCustomAttribute<T>(this MemberInfo memberInfo, out T? attribute) where T : Attribute {
        var attributeCache = _memberCache.GetOrAdd(memberInfo, _ => new AttributeCache(memberInfo));
        return attributeCache.TryGet(out attribute);
    }

    public static bool HasCustomAttribute<T>(this ParameterInfo parameterInfo) where T : Attribute {
        return parameterInfo.HasCustomAttribute<T>(out _);
    }

    public static bool HasCustomAttribute<T>(this ParameterInfo parameterInfo, out T? attribute) where T : Attribute {
        var attributeCache = _parameterCache.GetOrAdd(parameterInfo, _ => new AttributeCache(parameterInfo));
        return attributeCache.TryGet(out attribute);
    }

    private class AttributeCache {
        private readonly Dictionary<Type, Attribute> _attributes = new();

        public AttributeCache(MemberInfo memberInfo) {
            foreach(var attribute in memberInfo.GetCustomAttributes(true)) {
                _attributes[attribute.GetType()] = (Attribute)attribute;
            }
        }

        public AttributeCache(ParameterInfo parameterInfo) {
            foreach(var attribute in parameterInfo.GetCustomAttributes(true)) {
                _attributes[attribute.GetType()] = (Attribute)attribute;
            }
        }

        public bool TryGet<T>(out T? attribute) where T : Attribute {
            if(_attributes.TryGetValue(typeof(T), out var existingAttribute)) {
                attribute = (T)existingAttribute;
                return true;
            }

            attribute = default;
            return false;
        }
    }
}