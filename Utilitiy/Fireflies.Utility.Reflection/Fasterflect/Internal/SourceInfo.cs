#region License

// Copyright © 2010 Buu Nguyen, Morten Mertner
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

using System.Reflection;
using Fireflies.Utility.Reflection.Fasterflect.Extensions;

namespace Fireflies.Utility.Reflection.Fasterflect.Internal; 

internal class SourceInfo {
    #region Properties

    private Type type { get; }
    private bool[] paramKinds { get; }
    private string[] paramNames { get; }
    private Type[] paramTypes { get; }
    private MemberGetter[] paramValueReaders { get; set; }

    #endregion

    #region Constructors

    public SourceInfo(Type type, string[] names, Type[] types) {
        this.type = type;
        paramNames = names;
        paramTypes = types;
        paramKinds = new bool[names.Length];
        // this overload assumes that all names refer to fields on the given type
        for(var i = 0; i < paramKinds.Length; ++i) paramKinds[i] = true;
    }

    public SourceInfo(Type type, string[] names, Type[] types, bool[] kinds) {
        this.type = type;
        paramNames = names;
        paramTypes = types;
        paramKinds = kinds;
    }

    public static SourceInfo CreateFromType(Type type) {
        var members = type.Members(MemberTypes.Field | MemberTypes.Property, FasterflectFlags.InstanceAnyVisibility);
        var names = new List<string>(members.Count);
        var types = new List<Type>(members.Count);
        var kinds = new List<bool>(members.Count);
        for(var i = 0; i < members.Count; ++i) {
            var mi = members[i];
            Type memberType;
            bool isField;
            if(mi is FieldInfo field) {
                if(mi.Name[0] == '<')
                    continue;

                memberType = field.FieldType;
                isField = true;
            } else if(mi is PropertyInfo property) {
                if(!property.CanRead)
                    continue;

                memberType = property.PropertyType;
                isField = false;
            } else {
                continue;
            }

            names.Add(mi.Name);
            kinds.Add(isField);
            types.Add(memberType);
        }

        return new SourceInfo(type, names.ToArray(), types.ToArray(), kinds.ToArray());
    }

    #endregion

    #region Properties

    public Type Type => type;

    public string[] ParamNames => paramNames;

    public Type[] ParamTypes => paramTypes;

    public bool[] ParamKinds => paramKinds;

    public MemberGetter[] ParamValueReaders {
        get {
            InitializeParameterValueReaders();
            return paramValueReaders;
        }
    }

    #endregion

    #region Parameter Value Access

    public object[] GetParameterValues(object source) {
        InitializeParameterValueReaders();
        var paramValues = new object[paramNames.Length];
        for(var i = 0; i < paramNames.Length; ++i) paramValues[i] = paramValueReaders[i](source);
        return paramValues;
    }

    internal MemberGetter GetReader(string memberName) {
        var index = Array.IndexOf(paramNames, memberName);
        var reader = paramValueReaders[index];
        if(reader == null) {
            reader = paramKinds[index] ? type.DelegateForGetFieldValue(memberName) : type.DelegateForGetPropertyValue(memberName);
            paramValueReaders[index] = reader;
        }

        return reader;
    }

    private void InitializeParameterValueReaders() {
        if(paramValueReaders == null) {
            paramValueReaders = new MemberGetter[paramNames.Length];
            for(var i = 0; i < paramNames.Length; ++i) {
                var name = paramNames[i];
                paramValueReaders[i] = paramKinds[i] ? type.DelegateForGetFieldValue(name) : type.DelegateForGetPropertyValue(name);
            }
        }
    }

    #endregion

    #region Equals + GetHashCode

    public override bool Equals(object obj) {
        if(obj == this)
            return true;
        if(!(obj is SourceInfo other) || type != other.Type || paramNames.Length != other.ParamNames.Length)
            return false;

        for(var i = 0; i < paramNames.Length; ++i)
            if(paramNames[i] != other.ParamNames[i] || paramTypes[i] != other.ParamTypes[i] || paramKinds[i] != other.ParamKinds[i])
                return false;

        return true;
    }

    public override int GetHashCode() {
        var hash = type.GetHashCode();
        for(var i = 0; i < paramNames.Length; ++i)
            hash += ((i + 31) * paramNames[i].GetHashCode()) ^ paramTypes[i].GetHashCode();
        return hash;
    }

    #endregion
}