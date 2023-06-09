#region License

// Copyright � 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion

using System.ComponentModel;
using System.Reflection;
using TypeConverter = Fireflies.Utility.Reflection.Fasterflect.Probing.TypeConverter;

namespace Fireflies.Utility.Reflection.Fasterflect.Extensions; 

/// <summary>
///     Extension methods for inspecting and working with method parameters.
/// </summary>
public static partial class ParameterInfoExtensions {
    /// <summary>
    ///     Determines whether null can be assigned to the given <paramref name="parameter" />.
    /// </summary>
    /// <returns>True if null can be assigned, false otherwise.</returns>
    public static bool IsNullable(this ParameterInfo parameter) {
        return !parameter.ParameterType.IsValueType || parameter.ParameterType.IsSubclassOf(typeof(Nullable));
    }
}

/// <summary>
///     Extension methods for inspecting and working with method parameters.
/// </summary>
public static partial class ParameterInfoExtensions {
    #region Internal

    /// <summary>
    ///     Determines whether the given <paramref name="parameter" /> has the given <paramref name="name" />.
    ///     The comparison uses OrdinalIgnoreCase and allows for a leading underscore in either name
    ///     to be ignored.
    /// </summary>
    /// <returns>
    ///     True if the name is considered identical, false otherwise. If either parameter
    ///     is null an exception will be thrown.
    /// </returns>
    internal static bool HasName(this ParameterInfo parameter, string name) {
        var parameterName = parameter.Name.Length > 0 && parameter.Name[0] == '_'
            ? parameter.Name.Substring(1)
            : parameter.Name;
        name = name.Length > 0 && name[0] == '_' ? name.Substring(1) : name;
        return parameterName.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines whether the given <paramref name="parameter" /> has an associated default value as
    ///     supplied by an <see cref="DefaultValueAttribute" />. This method does not read the value of
    ///     the attribute. It also does not support C# 4.0 default parameter specifications.
    /// </summary>
    /// <returns>True if the attribute was detected, false otherwise.</returns>
    internal static bool HasDefaultValue(this ParameterInfo parameter) {
        var defaultValue = parameter.Attribute<DefaultValueAttribute>();
        return defaultValue != null;
    }

    /// <summary>
    ///     Gets the default value associated with the given <paramref name="parameter" />. The value is
    ///     obtained from the <see cref="DefaultValueAttribute" /> if present on the parameter. This method
    ///     does not support C# 4.0 default parameter specifications.
    /// </summary>
    /// <returns>
    ///     The default value if one could be obtained and converted into the type of the parameter,
    ///     and null otherwise.
    /// </returns>
    internal static object DefaultValue(this ParameterInfo parameter) {
        var defaultValue = parameter.Attribute<DefaultValueAttribute>();
        return defaultValue != null
            ? TypeConverter.Get(parameter.ParameterType, defaultValue.Value)
            : null;
    }

    #endregion Internal
}