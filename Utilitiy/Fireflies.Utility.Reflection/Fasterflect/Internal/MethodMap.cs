﻿#region License

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

using System.Collections;
using System.Reflection;
using System.Text;
using Fireflies.Utility.Reflection.Fasterflect.Extensions;
using Fireflies.Utility.Reflection.Fasterflect.Probing;

namespace Fireflies.Utility.Reflection.Fasterflect.Internal; 

/// <summary>
///     This class wraps a single invokable method call. It contains information on the method to call as well as
///     the parameters to use in the method call.
///     This intermediary class is used by the various other classes to select the best match to call
///     from a given set of available methods/constructors (and a set of parameter names and types).
/// </summary>
internal class MethodMap {
    #region Parameter Preparation

    /// <summary>
    ///     Perform parameter reordering, null handling and type conversion in preparation
    ///     of executing the method call.
    /// </summary>
    /// <param name="row">The callers row of data.</param>
    /// <returns>The parameter array to use in the actual invocation.</returns>
    protected object[] PrepareParameters(object[] row) {
        var methodParams = new object[parameters.Count];
        //int firstPotentialDefaultValueIndex = 0;
        for(var i = 0; i < row.Length; ++i)
            // only include columns in constructor
            if(parameterUsageMask[i]) {
                var index = parameterOrderMap[i];
                // check whether we need to type convert the input value
                var value = row[i];
                var convert = parameterTypeConvertMask[i];
                convert |= value != null && value.GetType() != paramTypes[i];
                if(convert) {
                    value = TypeConverter.Get(parameters[index].ParameterType, row[i]);
                    if(value == null) {
                        var sb = new StringBuilder();
                        sb.AppendFormat("Input parameter {0} of type {1} could unexpectedly not be converted to type {2}.{3}",
                            paramNames[i], paramTypes[i], parameters[index].ParameterType, Environment.NewLine);
                        sb.AppendFormat("Conversion was previously possible. Bad input value: {0}", row[i]);
                        throw new ArgumentException(sb.ToString(), paramNames[i]);
                    }
                }

                methodParams[index] = value;
                // advance counter of sequential fields used to save some time in the loop below
                //if(i == 1 + firstPotentialDefaultValueIndex)
                //{
                //    firstPotentialDefaultValueIndex++;
                //}
            }

        // TODO decide whether to support injecting default values
        //for (int i = firstPotentialDefaultValueIndex; i < methodParams.Length; ++i)
        //{
        //    if (parameterDefaultValueMask[i])
        //    {
        //        methodParams[i] = parameterDefaultValues[parameters[i].Name];
        //    }
        //}
        return methodParams;
    }

    #endregion

    internal virtual void InitializeInvoker() {
        var mi = (MethodInfo)method;
        invoker = mi.DelegateForCallMethod();
    }

    #region Fields

    private readonly bool mustUseAllParameters;
    protected long cost;
    protected bool isPerfectMatch;
    protected bool isValid;
    protected MemberInfo[] members;
    protected MethodBase method;
    protected BitArray methodParameterUsageMask; // marks method parameters for which a source was found
    protected string[] paramNames;
    protected Type[] paramTypes;
    protected BitArray parameterDefaultValueMask; // marks fields where default values will be used

    protected IDictionary<string, object> parameterDefaultValues;

    // protected BitArray parameterInjectionValueMask; // marks fields where injected values will be used
    // protected BitArray parameterNullValueMask; // marks fields where null values will be used
    protected int[] parameterOrderMap;
    protected int[] parameterOrderMapReverse;
    protected BitArray parameterReflectionMask; // marks parameters set using reflection
    protected BitArray parameterTypeConvertMask; // marks columns that may need type conversion
    protected BitArray parameterUnusedMask; // marks unused fields (columns with no target)
    protected long parameterUsageCount; // number of parameters used in constructor call
    protected BitArray parameterUsageMask; // marks parameters used in method call

    protected IList<ParameterInfo> parameters;

    // method call information
    protected int requiredFoundCount;
    protected int requiredParameterCount;
    protected Type type;
    private MethodInvoker invoker;

    #endregion

    #region Constructors and Initialization

    public MethodMap(MethodBase method, string[] paramNames, Type[] paramTypes, object[] sampleParamValues, bool mustUseAllParameters) {
        type = method.DeclaringType;
        this.method = method;
        this.paramNames = paramNames;
        this.paramTypes = paramTypes;
        requiredParameterCount = method.Parameters().Count;
        this.mustUseAllParameters = mustUseAllParameters;
        parameters = method.Parameters();
        InitializeBitArrays(Math.Max(parameters.Count, paramNames.Length));
        InitializeMethodMap(sampleParamValues);
    }

    private void InitializeBitArrays(int length) {
        methodParameterUsageMask = new BitArray(parameters.Count);
        parameterUsageMask = new BitArray(length);
        parameterUnusedMask = new BitArray(length);
        parameterTypeConvertMask = new BitArray(length);
        parameterReflectionMask = new BitArray(length);
        parameterDefaultValueMask = new BitArray(length);
    }

    #region Map Initialization

    private void InitializeMethodMap(object[] sampleParamValues) {
        #region Field initialization

        //int normalCount = 0; // number of fields filled with regular parameter values
        var defaultCount = 0; // number of fields filled using default values
        var nullCount = 0; // number of fields filled using null
        var injectionCount = 0; // number of fields filled using external values (dependency injection aka IoC)
        parameterOrderMap = new int[paramNames.Length];
        for(var i = 0; i < paramNames.Length; ++i) parameterOrderMap[i] = -1;
        parameterUsageCount = 0;
        members = new MemberInfo[paramNames.Length];
        // use a counter to determine whether we have a column for every parameter
        var noColumnForParameter = parameters.Count;
        // keep a reverse index for later when we check for default values
        parameterOrderMapReverse = new int[noColumnForParameter];
        // explicitly mark unused entries as we may have more parameters than columns
        for(var i = 0; i < noColumnForParameter; ++i) parameterOrderMapReverse[i] = -1;
        var isPerfectColumnOrder = true;

        #endregion

        #region Input parameters loop

        for(var invokeParamIndex = 0; invokeParamIndex < paramNames.Length; invokeParamIndex++) {
            #region Method parameters loop

            var paramName = paramNames[invokeParamIndex];
            var paramType = paramTypes[invokeParamIndex];
            var foundParam = false;
            var methodParameterIndex = 0;
            string errorText = null;
            for(var methodParamIndex = 0; methodParamIndex < parameters.Count; methodParamIndex++) {
                if(methodParameterUsageMask[methodParamIndex]) // ignore input if we already have an appropriate source
                    continue;

                methodParameterIndex = methodParamIndex; // preserve loop variable outside loop
                var parameter = parameters[methodParamIndex];
                // permit casing differences to allow for matching lower-case parameters to upper-case properties
                if(parameter.HasName(paramName)) {
                    var compatible = parameter.ParameterType.IsAssignableFrom(paramType);
                    // avoid checking if implicit conversion is possible
                    var convertible = !compatible && IsConvertible(paramType, parameter.ParameterType, sampleParamValues[invokeParamIndex]);
                    if(compatible || convertible) {
                        foundParam = true;
                        methodParameterUsageMask[methodParamIndex] = true;
                        noColumnForParameter--;
                        parameterUsageCount++;
                        parameterUsageMask[invokeParamIndex] = true;
                        parameterOrderMap[invokeParamIndex] = methodParamIndex;
                        parameterOrderMapReverse[methodParamIndex] = invokeParamIndex;
                        isPerfectColumnOrder &= invokeParamIndex == methodParamIndex;
                        // type conversion required for nullable columns mapping to not-nullable system type
                        // or when the supplied value type is different from member/parameter type
                        if(convertible) {
                            parameterTypeConvertMask[invokeParamIndex] = true;
                            cost += 1;
                        }

                        break;
                    }

                    // save a partial exception message in case there is also not a matching member we can set
                    errorText = string.Format("constructor parameter {0} of type {1}", parameter.Name, parameter.ParameterType);
                }
            }

            // method can only be invoked if we have the required number of parameters
            // parameters are checked from left to right (so any required number wont be enough)
            if(foundParam && methodParameterIndex < requiredParameterCount) requiredFoundCount++;

            #endregion

            #region No parameter handling (member check)

            if(!foundParam && method is ConstructorInfo) {
                // check if we can use reflection to set some members
                MemberInfo member = type.Property(paramName, FasterflectFlags.InstanceAnyVisibility | FasterflectFlags.IgnoreCase);
                // try again using leading underscore if nothing was found
                member = member ?? type.Property("_" + paramName, FasterflectFlags.InstanceAnyVisibility | FasterflectFlags.IgnoreCase);
                // look for fields if we still got no match or property was readonly
                if(member == null || !member.IsWritable()) {
                    member = type.Field(paramName, FasterflectFlags.InstanceAnyVisibility | FasterflectFlags.IgnoreCase);
                    // try again using leading underscore if nothing was found
                    member = member ?? type.Field("_" + paramName, FasterflectFlags.InstanceAnyVisibility | FasterflectFlags.IgnoreCase);
                }

                var exists = member != null;
                var memberType = member?.Type();
                var compatible = exists && memberType.IsAssignableFrom(paramType);
                // avoid checking if implicit conversion is possible
                var convertible = exists && !compatible && IsConvertible(paramType, memberType, sampleParamValues[invokeParamIndex]);
                if(method.IsConstructor && (compatible || convertible)) {
                    members[invokeParamIndex] = member;
                    // input not included in method call but member field or property is present
                    parameterUsageCount++;
                    parameterReflectionMask[invokeParamIndex] = true;
                    cost += 10;
                    // flag input parameter for type conversion
                    if(convertible) {
                        parameterTypeConvertMask[invokeParamIndex] = true;
                        cost += 1;
                    }
                } else {
                    // unused column - not in constructor or as member field
                    parameterUnusedMask[invokeParamIndex] = true;
                    if(exists || errorText != null) {
                        errorText = errorText ?? string.Format("member {0} of type {1}", member.Name, memberType);
                        var message = "Input parameter {0} of type {1} is incompatible with {2} (conversion was not possible).";
                        message = string.Format(message, paramName, paramType, errorText);
                        throw new ArgumentException(message, paramName);
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Default value injection

        // check whether method has unused parameters
        /*
        if(noColumnForParameter > 0)
        {
            for(int methodParamIndex = 0; methodParamIndex < parameters.Count; methodParamIndex++)
            {
                int invokeIndex = parameterOrderMapReverse[ methodParamIndex ];
                bool hasValue = invokeIndex != -1;
                if(hasValue)
                {
                    hasValue = parameterUsageMask[ invokeIndex ];
                }
                // only try to supply default values for parameters that do not already have a value
                if(! hasValue)
                {
                    ParameterInfo parameter = parameters[ methodParamIndex ];
                    bool hasDefaultValue = parameter.HasDefaultValue();
                    // default value can be a null value, but not for required parameters
                    bool isDefaultAllowed = methodParamIndex >= requiredParameterCount || hasDefaultValue;
                    if(isDefaultAllowed)
                    {
                        // prefer any explicitly defined default parameter value for the parameter
                        if(hasDefaultValue)
                        {
                            SaveDefaultValue(parameter.Name, parameter.DefaultValue);
                            parameterDefaultValueMask[ methodParamIndex ] = true;
                            defaultCount++;
                            noColumnForParameter--;
                        }
                        else if(HasExternalDefaultValue(parameter)) // external values (dependency injection)
                        {
                            SaveDefaultValue(parameter.Name, GetExternalDefaultValue(parameter));
                            parameterDefaultValueMask[ methodParamIndex ] = true;
                            injectionCount++;
                            noColumnForParameter--;
                        }
                        else // see if we can use null as the default value
                        {
                            if(parameter.ParameterType != null && parameter.IsNullable())
                            {
                                SaveDefaultValue(parameter.Name, null);
                                parameterDefaultValueMask[ methodParamIndex ] = true;
                                nullCount++;
                                noColumnForParameter--;
                            }
                        }
                    }
                }
            }
        }
         */

        #endregion

        #region Cost calculation and map validity checks

        // score 100 if parameter and column count differ
        cost += parameterUsageCount == parameters.Count ? 0 : 100;
        // score 300 if column order does not match parameter order
        cost += isPerfectColumnOrder ? 0 : 300;
        // score 600 if type conversion for any column is required
        cost += AllUnset(parameterTypeConvertMask) ? 0 : 600;
        // score additinal points if we need to use any kind of default value
        cost += defaultCount * 1000 + injectionCount * 1000 + nullCount * 1000;
        // determine whether we have a perfect match (can use direct constructor invocation)
        isPerfectMatch = isPerfectColumnOrder && parameterUsageCount == parameters.Count;
        isPerfectMatch &= parameterUsageCount == paramNames.Length;
        isPerfectMatch &= AllUnset(parameterUnusedMask) && AllUnset(parameterTypeConvertMask);
        isPerfectMatch &= cost == 0;
        // isValid tells whether this CM can be used with the given columns
        isValid = requiredFoundCount == requiredParameterCount && parameterUsageCount >= requiredParameterCount;
        isValid &= !mustUseAllParameters || parameterUsageCount == paramNames.Length;
        isValid &= noColumnForParameter == 0;
        isValid &= AllSet(methodParameterUsageMask);
        // this last specifies that we must use all of the supplied parameters to construct the object
        // isValid &= parameterUnusedMask == 0;

        #endregion
    }

    private bool IsConvertible(Type sourceType, Type targetType, object sampleValue) {
        // determine from sample value whether type conversion is needed
        var convertedValue = TypeConverter.Get(targetType, sampleValue);
        return convertedValue != null && sourceType != convertedValue.GetType();
    }

    private void SaveDefaultValue(string parameterName, object parameterValue) {
        // perform late initialization of the dictionary for default values
        if(parameterDefaultValues == null) parameterDefaultValues = new Dictionary<string, object>();
        parameterDefaultValues[parameterName] = parameterValue;
    }

    #endregion

    #endregion

    #region Method Invocation

    // this method is used for invoking static methods and constructors
    public virtual object Invoke(object[] row) {
        var methodParameters = isPerfectMatch ? row : PrepareParameters(row);
        return invoker.Invoke(null, methodParameters);
    }

    // this method is used for invoking instance methods
    public virtual object Invoke(object target, object[] row) {
        var methodParameters = isPerfectMatch ? row : PrepareParameters(row);
        return invoker.Invoke(target, methodParameters);
    }

    internal Type[] GetParamTypes() {
        var paramTypes = new Type[parameters.Count];
        for(var i = 0; i < parameters.Count; ++i) {
            var pi = parameters[i];
            paramTypes[i] = pi.ParameterType;
        }

        return paramTypes;
    }

    #endregion

    #region BitArray Helpers

    /// <summary>
    ///     Test whether at least one bit is set in the array. Replaces the old "long != 0" check.
    /// </summary>
    protected bool AnySet(BitArray bits) {
        return !AllUnset(bits);
    }

    /// <summary>
    ///     Test whether no bits are set in the array. Replaces the old "long == 0" check.
    /// </summary>
    protected bool AllUnset(BitArray bits) {
        foreach(bool bit in bits)
            if(bit)
                return false;

        return true;
    }

    /// <summary>
    ///     Test whether no bits are set in the array. Replaces the old "long == 0" check.
    /// </summary>
    protected bool AllSet(BitArray bits) {
        foreach(bool bit in bits)
            if(!bit)
                return false;

        return true;
    }

    #endregion

    #region Properties

    public IDictionary<string, object> ParameterDefaultValues {
        get => parameterDefaultValues;
        set => parameterDefaultValues = value;
    }

    public int ParameterCount => parameters.Count;

    public int RequiredParameterCount => requiredParameterCount;

    public virtual long Cost => cost;

    public bool IsValid => isValid;

    public bool IsPerfectMatch => isPerfectMatch;

    #endregion
}