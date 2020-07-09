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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA1031

namespace go
{
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Compare(Type x, Type y) => 
                Comparer<int>.Default.Compare(RelationDistance(x), RelationDistance(y));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                if (!type.IsSealed || type.IsGenericType || type.IsNested)
                    return Array.Empty<MethodInfo>();

                return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
            }

            string name = assembly.FullName;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PrintPointer<T>(this ptr<T> ptr) => ptr.Value.PrintPointer();

        /// <summary>
        /// Gets an object's pointer value, for display purposes, in hexadecimal format.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Object pointer value as string in hexadecimal format.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return $"0x{ptr.ToString("x")}";
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ImplementsInterface(this Type targetType, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                return false;

            while (!(targetType is null))
            {
                if (targetType.GetInterfaces().Any(targetInterface => targetInterface == interfaceType || targetInterface.ImplementsInterface(interfaceType)))
                    return true;

                targetType = targetType.BaseType!;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type targetType)
        {
            lock (s_extensionMethods)
                return s_extensionMethods.Where(value => value.type.IsAssignableFrom(targetType)).Select(value => value.method);
        }

        /// <summary>
        /// Attempts to find the best precedence-wise matching extension method called <paramref name="methodName"/> for the <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <param name="methodName">Name of extension method to find.</param>
        /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetExtensionMethod(this Type targetType, string methodName)
        {
            // Note that match by function name alone is sufficient as Go does not currently support function overloading by adjusting signature:
            // https://golang.org/doc/faq#overloading
            return targetType.GetExtensionMethods().Where(methodInfo => methodInfo.Name == methodName).OrderBy(GetExtensionTargetType, new TypePrecedenceComparer(targetType)).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to find the best precedence-wise matching extension method called <paramref name="methodName"/> for the <paramref name="targetType"/> or
        /// for any of its promotions, see <see cref="PromotedStructAttribute"/>.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <param name="methodName">Name of extension method to find.</param>
        /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo? GetExtensionMethodSearchingPromotions(this Type targetType, string methodName)
        {
            MethodInfo? method = targetType.GetExtensionMethod(methodName);

            if (method is null)
            {
                object[] customAttributes = targetType.GetCustomAttributes(typeof(PromotedStructAttribute), true);

                if (customAttributes.Length > 0)
                {
                    foreach (PromotedStructAttribute attribute in customAttributes.Cast<PromotedStructAttribute>())
                    {
                        method = attribute.PromotedType.GetExtensionMethodSearchingPromotions(methodName);

                        if (!(method is null))
                            break;
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Creates a delegate for the given static method metadata.
        /// </summary>
        /// <param name="methodInfo">Method metadata of extension method.</param>
        /// <param name="delegateType">Specific delegate type to apply; otherwise, defaults to an auto-derived Func or Action delegate.</param>
        /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type? delegateType = null)
        {
            if (delegateType is null)
                return methodInfo.CreateStaticDelegate(null!, out bool _);

            try
            {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type delegateType, out bool isByRef)
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
                getMethodType = sourceTypes => delegateType;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetExplicitGenericConversionOperator(this Type genericType, Type targetType)
        {
            Type genericOfType = genericType.MakeGenericType(targetType);

            return genericOfType
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .FirstOrDefault(method => IsConversionOperator(method, genericOfType, targetType));
        }

        // FYI:
        //       IsUnmanaged<bool>() is true, but
        //       IsBlittable<bool>() is false

        // ReSharper disable once UnusedTypeParameter
        private class UnmanagedTypeTester<T> where T : unmanaged { }

        /// <summary>
        /// Determines if type is unmanaged.
        /// </summary>
        /// <typeparam name="T">Type to check.</typeparam>
        /// <returns><c>true</c> if type is unmanaged; otherwise, <c>false</c>.</returns>
        public static bool IsUnmanaged<T>() => IsUnmanaged(typeof(T));

        /// <summary>
        /// Determines if type is unmanaged.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns><c>true</c> if type is unmanaged; otherwise, <c>false</c>.</returns>
        public static bool IsUnmanaged(this Type type)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                typeof(UnmanagedTypeTester<>).MakeGenericType(type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if type is blittable, i.e., can be instantiated via <see cref="GCHandle.Alloc(object, GCHandleType)"/>
        /// when <see cref="GCHandleType"/> parameter is <see cref="GCHandleType.Pinned"/>.
        /// </summary>
        /// <typeparam name="T">Type to check.</typeparam>
        /// <returns><c>true</c> if type is blittable; otherwise, <c>false</c>.</returns>
        public static bool IsBlittable<T>() where T : new()
        {
            try
            {
                GCHandle.Alloc(new T(), GCHandleType.Pinned).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create a interface handler for <typeparamref name="T"/> interface from
        /// an object-based target.
        /// </summary>
        /// <param name="handlerType">Generic handler type.</param>
        /// <param name="target">Target value for interface handler.</param>
        /// <returns>Interface handler for <typeparamref name="T"/> interface</returns>
        /// <typeparam name="T">Target interface.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? CreateInterfaceHandler<T>(this Type handlerType, object target) where T : class
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsConversionOperator(MethodInfo method, Type genericOfType, Type targetType)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return method.Name == "op_Explicit" && 
                   method.ReturnType == genericOfType && 
                   parameters.Length == 1 && 
                   parameters[0].ParameterType == targetType;
        }
    }
}
