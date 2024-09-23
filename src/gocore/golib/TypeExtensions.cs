//******************************************************************************************************
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// Defines type related helper functions.
/// </summary>
public static class TypeExtensions
{
    private static readonly List<(MethodInfo method, Type type)> s_extensionMethods;

    private sealed class TypePrecedenceComparer : Comparer<Type>
    {
        private readonly Type m_targetType;

        public TypePrecedenceComparer(Type targetType) => m_targetType = targetType;

        public override int Compare(Type? x, Type? y) => 
            Comparer<int>.Default.Compare(RelationDistance(x), RelationDistance(y));

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

    static TypeExtensions()
    {
        s_extensionMethods = new List<(MethodInfo, Type)>();

        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyLoad += (_, e) => LoadAssemblyExtensionMethods(e.LoadedAssembly);

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            LoadAssemblyExtensionMethods(assembly);
    }

    private static void LoadAssemblyExtensionMethods(Assembly assembly)
    {
        static IEnumerable<MethodInfo> getExtensionMethods(Type type)
        {
            // TODO: With addition of Golang generics, type.IsGenericType is now allowable
            if (!type.IsSealed || type.IsNested) /* || type.IsGenericType */
                return Array.Empty<MethodInfo>();

            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                       .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
        }

        string? name = assembly.FullName;

        if (string.IsNullOrEmpty(name))
            return;

        // Ignore extensions methods from the .NET framework
        if (name.StartsWith("System.") || name.StartsWith("netstandard"))
            return;

        Debug.WriteLine($"Scanning extensions for assembly \"{assembly.FullName}\"...");

        lock (s_extensionMethods)
        {
            foreach (Type type in assembly.GetTypes())
            foreach (MethodInfo extensionMethod in getExtensionMethods(type))
                s_extensionMethods.Add((extensionMethod, extensionMethod.GetExtensionTargetType()));
        }
    }

    /// <summary>
    /// Gets an object's pointer value, for display purposes, in hexadecimal format.
    /// </summary>
    /// <param name="ptr"></param>
    /// <returns>Object pointer value as string in hexadecimal format.</returns>
    public static string PrintPointer<T>(this ptr<T> ptr) => ptr.val.PrintPointer();

    /// <summary>
    /// Gets an object's pointer value, for display purposes, in hexadecimal format.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns>Object pointer value as string in hexadecimal format.</returns>
    public static unsafe string PrintPointer(this object? instance)
    {
        if (instance is null)
            return "nil";

        try
        {
            // Do not attempt to use this pointer, its address is unfixed
            // and being accessed for display purposes only. The GC will
            // move this pointer location at will.
            TypedReference reference = __makeref(instance);
            IntPtr ptr = **(IntPtr**)&reference;
            return $"0x{ptr:x}";
        }
        catch
        {
            return "nil";
        }
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
    /// Type of the extension target, i.e., type of the first parameter; otherwise, <c>void</c>
    /// if <paramref name="methodInfo"/> is <c>null</c> or defines no parameters.
    /// </returns>
    public static Type GetExtensionTargetType(this MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();
        return parameters.Length > 0 ? parameters[0].ParameterType : typeof(void);
    }

    /// <summary>
    /// Finds all the extensions methods for <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <returns>Enumeration of reflected method metadata of <paramref name="targetType"/> extension methods.</returns>
    public static IEnumerable<MethodInfo> GetExtensionMethods(this Type targetType)
    {
        // TODO: Since Go restricts receiver functions (extensions in C#) to the same package, a lookup per package (namespace in C#) will be optimal here
        lock (s_extensionMethods)
        {
            bool isGenericType = (targetType == typeof(ptr<>) ? targetType.GetGenericArguments()[0] : targetType).IsGenericType;

            if (isGenericType)
                targetType = targetType.GetGenericTypeDefinition();

            bool isGenericMatch(Type methodType)
            {
                if (methodType.IsGenericType)
                    return methodType.GetGenericTypeDefinition() == targetType;

                return methodType == targetType;
            }

            return isGenericType ?
                s_extensionMethods.Where(value => isGenericMatch(value.type)).Select(value => value.method) :
                s_extensionMethods.Where(value => value.type.IsAssignableFrom(targetType)).Select(value => value.method);
        }
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
            if (delegateType.IsGenericType && methodInfo.IsGenericMethod)
            {
                Type extensionTarget = delegateType.GetGenericArguments()[0];

                if (extensionTarget.IsGenericType)
                    return Delegate.CreateDelegate(delegateType, methodInfo.MakeGenericMethod(extensionTarget.GetGenericArguments()[0]));

                return Delegate.CreateDelegate(delegateType, methodInfo.MakeGenericMethod(extensionTarget));
            }
                    

            return Delegate.CreateDelegate(delegateType, methodInfo);
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
            return default;

        try
        {
            Type constructedType = handlerType.MakeGenericType(target.GetType());
            return Activator.CreateInstance(constructedType, target) as T;
        }
        catch
        {
            return default;
        }
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

        integer = default;
        return false;
    }

    /// <summary>
    /// Tries to cast input value as an integer.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="value">Value to try to cast.</param>
    /// <param name="integer">Casted value.</param>
    /// <returns><c>true</c> if cast succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryCastAsInteger<T>(this T value, out ulong integer) where T : unmanaged, IConvertible => 
        ((object)value).TryCastAsInteger(out integer);

    /// <summary>
    /// Determines if <see cref="IConvertible"/> <paramref name="value"/> is a numeric type.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><c>true</c> is <paramref name="value"/> is a numeric type; othwerwise, <c>false</c>.</returns>
    public static bool IsNumeric(this IConvertible? value) => 
        value is not null && IsNumericType(value.GetTypeCode());

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
            TypeCode.SByte   => true,
            TypeCode.Byte    => true,
            TypeCode.Int16   => true,
            TypeCode.UInt16  => true,
            TypeCode.Int32   => true,
            TypeCode.UInt32  => true,
            TypeCode.Int64   => true,
            TypeCode.UInt64  => true,
            TypeCode.Single  => true,
            TypeCode.Double  => true,
            TypeCode.Decimal => true,
            _                => false
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
