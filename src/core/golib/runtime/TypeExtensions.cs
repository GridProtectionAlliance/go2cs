﻿//******************************************************************************************************
//  TypeHelpers.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable InconsistentlySynchronizedField

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable IL2075
#pragma warning disable IL2067
#pragma warning disable IL2055
#pragma warning disable IL2060
#pragma warning disable IL2080
#pragma warning disable IL2070
#pragma warning disable IL2026

namespace go.runtime;

/// <summary>
/// Defines type related helper functions.
/// </summary>
public static class TypeExtensions
{
    private static (MethodInfo, Type)[]? s_extensionMethods;
    private static readonly Lock s_loadLock = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo[]> s_typeExtensionMethods = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_typeExtensionMethodNames = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_interfaceMethodNames = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_structFieldNames = [];
    private static readonly ConcurrentDictionary<Type, MethodInfo?> s_typeEqualityOperators = [];
    private static readonly ConcurrentDictionary<Type, MethodInfo?> s_onesComplementOperators = [];
    private static int s_registeredAssemblyLoadEvent;

    private static (MethodInfo, Type)[] GetExtensionMethods()
    {
        if (Interlocked.CompareExchange(ref s_extensionMethods, null, null) is not null)
            return s_extensionMethods!;

        // Register assembly load event only once, used to clear extension method caches
        if (Interlocked.CompareExchange(ref s_registeredAssemblyLoadEvent, 1, 0) == 0)
            AppDomain.CurrentDomain.AssemblyLoad += ClearTypeCaches;

        lock (s_loadLock)
        {
            // Check if another thread already loaded the extension methods
            if (Volatile.Read(ref s_extensionMethods) is not null)
                return s_extensionMethods!;

            List<(MethodInfo, Type)> extensionMethods = [];

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                LoadAssemblyExtensionMethods(assembly, extensionMethods);

            s_extensionMethods = extensionMethods.ToArray();
        }

        return s_extensionMethods;
    }

    private static void ClearTypeCaches(object? sender, EventArgs e)
    {
        // Since not all assemblies may be loaded when initial type caches
        // are created, we need to clear caches when any new assemblies are
        // loaded so that caches can be recreated
        lock (s_loadLock)
            Volatile.Write(ref s_extensionMethods, null);

        s_typeExtensionMethods.Clear();
        s_typeExtensionMethodNames.Clear();
        s_interfaceMethodNames.Clear();
    }

    private static void LoadAssemblyExtensionMethods(Assembly assembly, List<(MethodInfo, Type)> extensionMethods)
    {
        string? name = assembly.FullName;

        if (string.IsNullOrEmpty(name))
            return;

        // Ignore extensions methods from the .NET framework
        if (name.StartsWith("System.") || name.StartsWith("netstandard") || name.StartsWith("Microsoft.") || name.StartsWith("WindowsBase") || name.StartsWith("go.runtime."))
            return;

        Debug.WriteLine($"Scanning extensions for assembly \"{assembly.FullName}\"...");

        foreach (Type type in assembly.GetTypes())
        foreach (MethodInfo extensionMethod in getExtensionMethods(type))
            extensionMethods.Add((extensionMethod, extensionMethod.GetExtensionTargetType()));
        
        return;

        static IEnumerable<MethodInfo> getExtensionMethods(Type type)
        {
            if (!type.IsSealed || type.IsNested || type.IsGenericType)
                return Array.Empty<MethodInfo>();

            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
        }
    }

    private sealed class TypePrecedenceComparer : Comparer<Type>
    {
        private readonly Type m_targetType;

        public TypePrecedenceComparer(Type targetType)
        {
            m_targetType = targetType;
        }

        public override int Compare(Type? x, Type? y)
        {
            return Comparer<int>.Default.Compare(RelationDistance(x), RelationDistance(y));
        }

        private int RelationDistance(Type? type)
        {
            if (type is null)
                return int.MaxValue;

            int distance = 0;

            while (!IsDirectEquivalent(type))
            {
                type = type.BaseType;
                distance++;

                if (type is null || type == typeof(object))
                {
                    // No direct relation exists
                    distance = int.MaxValue;
                    break;
                }
            }

            return distance;
        }

        private bool IsDirectEquivalent(Type type)
        {
            if (m_targetType.IsInterface)
            {
                if (type.IsInterface)
                    return type.ImplementsInterface(m_targetType) || m_targetType.ImplementsInterface(type);

                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == m_targetType || interfaceType.ImplementsInterface(m_targetType))
                        return true;
                }

                return false;
            }

            if (!type.IsInterface)
                return type == m_targetType;

            foreach (Type interfaceType in m_targetType.GetInterfaces())
            {
                if (interfaceType == type || interfaceType.ImplementsInterface(type))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets an object's pointer value, for display purposes, in hexadecimal format.
    /// </summary>
    /// <param name="ptr"></param>
    /// <returns>Object pointer value as string in hexadecimal format.</returns>
    public static string PrintPointer<T>(this ж<T> ptr)
    {
        return ptr == nil ? "<nil>" : ptr.val.PrintPointer();
    }

    /// <summary>
    /// Gets an object's pointer value, for display purposes, in hexadecimal format.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns>Object pointer value as string in hexadecimal format.</returns>
    public static unsafe string PrintPointer(this object? instance)
    {
        if (instance is null)
            return "<nil>";

        try
        {
            // Do not attempt to use this pointer, its address is unfixed
            // and being accessed for display purposes only. The GC will
            // move this pointer location at will.
            TypedReference reference = __makeref(instance);
            nint ptr = **(nint**)&reference;
            return $"0x{ptr:x}";
        }
        catch
        {
            return "<nil>";
        }
    }

    /// <summary>
    /// Get the "==" equality operator for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to search for equality operator.</param>
    /// <returns>Equality operator for <paramref name="type"/> if found; otherwise, <c>null</c>.</returns>
    public static MethodInfo? GetEqualityOperator(this Type type)
    {
        return s_typeEqualityOperators.GetOrAdd(type, _ => type.GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public, [type, type]));
    }

    /// <summary>
    /// Get the "~" ones complement operator for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to search for equality operator.</param>
    /// <returns>Ones complement operator for <paramref name="type"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// For go2cs pointers, <see cref="go.ж{T}"/>, the ones complement operator is used to dereference
    /// the pointer, i.e., to get the value that the pointer points to, its element. Since the pointer
    /// class is a generic type, this function can be used to get the pointer value using the static
    /// ones complement operator without needing to know the pointer type.
    /// </remarks>
    public static MethodInfo? GetOnesComplementOperator(this Type type)
    {
        return s_onesComplementOperators.GetOrAdd(type, _ => type.GetMethod("op_OnesComplement", BindingFlags.Static | BindingFlags.Public, [type]));
    }

    /// <summary>
    /// Determines if <paramref name="targetType"/> implements specified <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="targetType">Target type to test.</param>
    /// <param name="interfaceType">Interface to search.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="targetType"/> implements specified <paramref name="interfaceType"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool ImplementsInterface(this Type? targetType, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            return false;

        while (targetType is not null)
        {
            if (targetType.GetInterfaces().Any(targetInterface => targetInterface == interfaceType || targetInterface.ImplementsInterface(interfaceType)))
                return true;

            targetType = targetType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets the type of the extension target.
    /// </summary>
    /// <param name="methodInfo">Method info.</param>
    /// <returns>
    /// Type of the extension target, i.e., type of the first parameter.
    /// </returns>
    /// <exception cref="InvalidOperationException">Method has no parameters and cannot be an extension method.</exception>
    public static Type GetExtensionTargetType(this MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        if (parameters.Length == 0)
            throw new InvalidOperationException("Method has no parameters and cannot be an extension method.");

        return parameters[0].ParameterType;
    }

    /// <summary>
    /// Finds all the extensions methods for <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <returns>Enumeration of reflected method metadata of <paramref name="targetType"/> extension methods.</returns>
    public static MethodInfo[] GetExtensionMethods(this Type targetType)
    {
        return s_typeExtensionMethods.GetOrAdd(targetType, _ =>
        {
            (MethodInfo method, Type type)[] extensionMethods = GetExtensionMethods();

            bool isGenericType = (targetType == typeof(ж<>) ? targetType.GetGenericArguments()[0] : targetType).IsGenericType;

            if (isGenericType)
                targetType = targetType.GetGenericTypeDefinition();

            IEnumerable<MethodInfo> methods = isGenericType ?
                extensionMethods.Where(value => isGenericMatch(value.type)).Select(value => value.method) :
                extensionMethods.Where(value => value.type.IsAssignableFrom(targetType)).Select(value => value.method);

            return methods.ToArray();

            bool isGenericMatch(Type methodType)
            {
                if (methodType.IsGenericType)
                    return methodType.GetGenericTypeDefinition() == targetType;

                return methodType == targetType;
            }
        });
    }

    /// <summary>
    /// Gets all the extension method names for <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <returns>A collection of extension method names for <paramref name="targetType"/>.</returns>
    public static ImmutableHashSet<string> GetExtensionMethodNames(this Type targetType)
    {
        return s_typeExtensionMethodNames.GetOrAdd(targetType, _ => [.. targetType.GetExtensionMethods().Select(info => info.Name)]);
    }

    /// <summary>
    /// Determines if an extension method with the specified <paramref name="methodName"/> exists for the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <param name="methodName">Name of extension method to find.</param>
    /// <returns><c>true</c> if extension method exists; otherwise, <c>false</c>.</returns>
    public static bool ExtensionMethodExists(this Type targetType, string methodName)
    {
        // Note that match by function name alone is sufficient as Go does not currently support function overloading by adjusting signature:
        // https://golang.org/doc/faq#overloading
        return targetType.GetExtensionMethods().Any(methodInfo => methodInfo.Name == methodName);
    }

    /// <summary>
    /// Attempts to find the best precedence-wise matching extension method called <paramref name="methodName"/> for the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <param name="methodName">Name of extension method to find.</param>
    /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
    public static MethodInfo? GetExtensionMethod(this Type targetType, string methodName)
    {
        // Note that match by function name alone is sufficient as Go does not currently support function overloading by adjusting signature:
        // https://golang.org/doc/faq#overloading
        return targetType.GetExtensionMethods().Where(methodInfo => methodInfo.Name == methodName).MinBy(GetExtensionTargetType, new TypePrecedenceComparer(targetType));
    }

    /// <summary>
    /// Returns all method names defined in an interface type, including those inherited from other interfaces.
    /// </summary>
    /// <param name="interfaceType">The interface type to examine. Must be an interface.</param>
    /// <returns>A collection of method names defined in the interface and its base interfaces.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not an interface.</exception>
    public static ImmutableHashSet<string> GetInterfaceMethodNames(this Type interfaceType)
    {
        return s_interfaceMethodNames.GetOrAdd(interfaceType, _ => [..interfaceType.GetInterfaceMethods().Select(info => info.Name)]);
    }

    /// <summary>
    /// Returns detailed information about methods defined in an interface type, including those inherited from other interfaces.
    /// </summary>
    /// <param name="interfaceType">The interface type to examine. Must be an interface.</param>
    /// <returns>A collection of MethodInfo objects for methods defined in the interface and its base interfaces.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not an interface.</exception>
    public static IEnumerable<MethodInfo> GetInterfaceMethods(this Type interfaceType)
    {
        // Verify the type is an interface
        if (!interfaceType.IsInterface)
            throw new ArgumentException($"The type '{interfaceType.FullName}' is not an interface.", nameof(interfaceType));

        // Get all methods directly defined on this interface
        MethodInfo[] methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        // Get all base interfaces
        Type[] baseInterfaces = interfaceType.GetInterfaces();

        // If there are no base interfaces, return just the direct methods
        if (baseInterfaces.Length == 0)
            return methods;

        // Otherwise, combine the direct methods with inherited methods
        HashSet<MethodInfo> allMethods = [..methods];

        // Add methods from all base interfaces
        foreach (Type baseInterface in baseInterfaces)
        {
            MethodInfo[] baseMethods = baseInterface.GetMethods();
            
            foreach (MethodInfo method in baseMethods)
                allMethods.Add(method);
        }

        return allMethods;
    }

    /// <summary>
    /// Gets the names of all fields in a struct type.
    /// </summary>
    /// <param name="valueType">Struct type to search.</param>
    /// <returns>Names of all fields in the struct type.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not a value type.</exception>
    public static ImmutableHashSet<string> GetStructFieldNames(this Type valueType)
    {
        return s_structFieldNames.GetOrAdd(valueType, _ =>
        {
            if (!valueType.IsValueType)
                throw new ArgumentException($"Type '{valueType.FullName}' is not a value type.", nameof(valueType));

            FieldInfo[] fields = valueType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return [..fields.Select(field => field.Name)];
        });
    }

    /// <summary>
    /// Creates a delegate for the given static method metadata.
    /// </summary>
    /// <param name="methodInfo">Method metadata of extension method.</param>
    /// <param name="delegateType">Specific delegate type to apply; otherwise, defaults to an auto-derived Func or Action delegate.</param>
    /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
    public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type? delegateType = null)
    {
        if (delegateType is null)
            return methodInfo.CreateStaticDelegate(null!, out bool _);

        try
        {
            if (!delegateType.IsGenericType || !methodInfo.IsGenericMethod)
                return Delegate.CreateDelegate(delegateType, methodInfo);

            Type extensionTarget = delegateType.GetGenericArguments()[0];

            return Delegate.CreateDelegate(delegateType, extensionTarget.IsGenericType ?
                methodInfo.MakeGenericMethod(extensionTarget.GetGenericArguments()[0]) :
                methodInfo.MakeGenericMethod(extensionTarget));
        }
        catch (ArgumentException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Creates a delegate for the given static method metadata.
    /// </summary>
    /// <param name="methodInfo">Method metadata of extension method.</param>
    /// <param name="delegateType">Specific delegate type to apply; set to <c>null</c> to use an auto-derived Func or Action delegate.</param>
    /// <param name="isByRef">Determines if extension target is accessed by reference.</param>
    /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
    public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type? delegateType, out bool isByRef)
    {
        Func<Type[], Type> getMethodType;
        List<Type> types = methodInfo.GetParameters().Select(paramInfo => paramInfo.ParameterType).ToList();

        if (delegateType is null)
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                getMethodType = Expression.GetActionType;
            }
            else
            {
                getMethodType = Expression.GetFuncType;
                types.Add(methodInfo.ReturnType);
            }
        }
        else
        {
            getMethodType = _ => delegateType;
        }

        isByRef = types[0].IsByRef;

        try
        {
            return Delegate.CreateDelegate(getMethodType(types.ToArray()), methodInfo);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds the explicit conversion operator for converting <paramref name="targetType"/> to
    /// <paramref name="genericType"/> of <paramref name="targetType"/>.
    /// </summary>
    /// <param name="genericType">Generic type to search.</param>
    /// <param name="targetType">Target type of generic type.</param>
    /// <returns>Explicit conversion operator, if found; otherwise <c>null</c>.</returns>
    public static MethodInfo GetExplicitGenericConversionOperator(this Type genericType, Type targetType)
    {
        Type genericOfType = genericType.MakeGenericType(targetType);

        return genericOfType
               .GetMethods(BindingFlags.Static | BindingFlags.Public)
               .FirstOrDefault(method => IsConversionOperator(method, genericOfType, targetType))!;
    }

    /// <summary>
    /// Create an interface handler for <typeparamref name="T"/> interface from
    /// an object-based target.
    /// </summary>
    /// <param name="handlerType">Generic handler type.</param>
    /// <param name="target">Target value for interface handler.</param>
    /// <returns>Interface handler for <typeparamref name="T"/> interface</returns>
    /// <typeparam name="T">Target interface.</typeparam>
    public static T? CreateInterfaceHandler<T>(this Type handlerType, object? target) where T : class
    {
        if (target is null)
            return null;

        try
        {
            Type constructedType = handlerType.MakeGenericType(target.GetType());
            return Activator.CreateInstance(constructedType, target) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if the specified <paramref name="valueType"/> is a dynamic type.
    /// </summary>
    /// <param name="valueType">Type to check.</param>
    /// <returns><c>true</c> if <paramref name="valueType"/> is a dynamic type; otherwise, <c>false</c>.</returns>
    public static bool IsDynamicType(this Type valueType)
    {
        GoTypeAttribute? goType = valueType.GetCustomAttribute<GoTypeAttribute>();
        return goType is not null && goType.Definition == "dyn";
    }

    /// <summary>
    /// Returns a Go type equivalent to the specified value.
    /// </summary>
    /// <param name="value">An object that implements the <see cref="IConvertible" /> interface.</param>
    /// <returns>A Go type whose value is equivalent to <paramref name="value"/>.</returns>
    public static object ConvertToType<T>(in T? value) where T : IConvertible
    {
        if (value is null)
            return nil;

        return value.GetTypeCode() switch
        {
            TypeCode.Boolean => value.ToBoolean(null),
            TypeCode.Char => (rune)value.ToChar(null),
            TypeCode.SByte => value.ToSByte(null),
            TypeCode.Byte => value.ToByte(null),
            TypeCode.Int16 => value.ToInt16(null),
            TypeCode.UInt16 => value.ToUInt16(null),
            TypeCode.Int32 => value.ToInt32(null),
            TypeCode.UInt32 => value.ToUInt32(null),
            TypeCode.Int64 => value.ToInt64(null),
            TypeCode.UInt64 => value.ToUInt64(null),
            TypeCode.Single => value.ToSingle(null),
            TypeCode.Double => value.ToDouble(null),
            _ => (@string)value.ToString(null)
        };
    }

    /// <summary>
    /// Tries to cast input value as an integer.
    /// </summary>
    /// <param name="value">Value to try to cast.</param>
    /// <param name="integer">Casted value.</param>
    /// <returns><c>true</c> if cast succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryCastAsInteger(this object value, out ulong integer)
    {
        switch (value)
        {
            case char charVal:
                integer = charVal;
                return true;
            case bool boolVal:
                integer = boolVal ? 1UL : 0UL;
                return true;
            case sbyte sbyteVal:
                integer = (ulong)sbyteVal;
                return true;
            case byte byteVal:
                integer = byteVal;
                return true;
            case short shortVal:
                integer = (ulong)shortVal;
                return true;
            case ushort ushortVal:
                integer = ushortVal;
                return true;
            case int intVal:
                integer = (ulong)intVal;
                return true;
            case uint uintVal:
                integer = uintVal;
                return true;
            case long longVal:
                integer = (ulong)longVal;
                return true;
            case ulong ulongVal:
                integer = ulongVal;
                return true;
        }

        integer = 0;
        return false;
    }

    /// <summary>
    /// Tries to cast input value as an integer.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="value">Value to try to cast.</param>
    /// <param name="integer">Casted value.</param>
    /// <returns><c>true</c> if cast succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryCastAsInteger<T>(this T value, out ulong integer) where T : unmanaged, IConvertible
    {
        return ((object)value).TryCastAsInteger(out integer);
    }

    /// <summary>
    /// Determines if <see cref="IConvertible"/> <paramref name="value"/> is a numeric type.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><c>true</c> is <paramref name="value"/> is a numeric type; otherwise, <c>false</c>.</returns>
    public static bool IsNumeric(this IConvertible? value)
    {
        return value is not null && value.GetTypeCode().IsNumericType();
    }

    /// <summary>
    /// Determines if <paramref name="typeCode"/> is a numeric type, i.e., one of:
    /// <see cref="TypeCode.Boolean"/>, <see cref="TypeCode.SByte"/>, <see cref="TypeCode.Byte"/>,
    /// <see cref="TypeCode.Int16"/>, <see cref="TypeCode.UInt16"/>, <see cref="TypeCode.Int32"/>,
    /// <see cref="TypeCode.UInt32"/>, <see cref="TypeCode.Int64"/>, <see cref="TypeCode.UInt64"/>
    /// <see cref="TypeCode.Single"/>, <see cref="TypeCode.Double"/> or <see cref="TypeCode.Decimal"/>.
    /// </summary>
    /// <param name="typeCode"><see cref="TypeCode"/> value to check.</param>
    /// <returns><c>true</c> if <paramref name="typeCode"/> is a numeric type; otherwise, <c>false</c>.</returns>
    public static bool IsNumericType(this TypeCode typeCode)
    {
        return typeCode switch
        {
            TypeCode.Boolean => true,
            TypeCode.SByte => true,
            TypeCode.Byte => true,
            TypeCode.Int16 => true,
            TypeCode.UInt16 => true,
            TypeCode.Int32 => true,
            TypeCode.UInt32 => true,
            TypeCode.Int64 => true,
            TypeCode.UInt64 => true,
            TypeCode.Single => true,
            TypeCode.Double => true,
            TypeCode.Decimal => true,
            _ => false
        };
    }

    private static bool IsConversionOperator(MethodInfo method, Type genericOfType, Type targetType)
    {
        ParameterInfo[] parameters = method.GetParameters();

        return method.Name == "op_Explicit" &&
               method.ReturnType == genericOfType &&
               parameters.Length == 1 &&
               parameters[0].ParameterType == targetType;
    }
}
