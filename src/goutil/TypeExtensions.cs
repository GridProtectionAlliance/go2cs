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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace go
{
    /// <summary>
    /// Defines type related helper functions.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Type[] s_types;

        private class TypePrecedenceComparer : Comparer<Type>
        {
            private Type m_targetType;

            public TypePrecedenceComparer(Type targetType) => m_targetType = targetType;

            public override int Compare(Type x, Type y)
            {
                return Comparer<int>.Default.Compare(RelationDistance(x), RelationDistance(y));
            }

            private int RelationDistance(Type type)
            {
                int count = 0;

                while (type != m_targetType && type != typeof(object))
                {
                    type = type.BaseType;
                    count++;
                }

                return count;
            }
        }

        static TypeExtensions()
        {
            List<Type> types = new List<Type>();

            // TODO: Not all assemblies may be loaded when this type is initialized - may need to refresh this list
            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
                types.AddRange(item.GetTypes());

            s_types = types.ToArray();
        }

        /// <summary>
        /// Gets the type of the extension target.
        /// </summary>
        /// <param name="methodInfo">Method info.</param>
        /// <returns>Type of the extension target.</returns>
        public static Type GetExtensionTargetType(this MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length > 0)
                return parameters[0].ParameterType;

            return typeof(void);
        }

        /// <summary>
        /// Finds all the extensions methods for <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <returns>Enumeration of reflected method metadata of <paramref name="targetType"/> extension methods.</returns>
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type targetType)
        {
            return s_types
                .Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false))
                .Where(methodInfo => methodInfo.GetExtensionTargetType().IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Attempts to find the specified extension method, <paramref name="methodName"/>, for <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <param name="methodName">Name of extension method to find.</param>
        /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
        public static MethodInfo GetExtensionMethod(this Type targetType, string methodName)
        {
            return targetType.GetExtensionMethods().OrderBy(GetExtensionTargetType, new TypePrecedenceComparer(targetType)).FirstOrDefault(methodInfo => methodInfo.Name == methodName);
        }

        /// <summary>
        /// Attempts to find the specified extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> or any of its promotions.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <param name="methodName">Name of extension method to find.</param>
        /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
        public static MethodInfo GetExtensionMethodSearchingPromotions(this Type targetType, string methodName)
        {
            MethodInfo method = targetType.GetExtensionMethod(methodName);

            if ((object)method == null)
            {
                object[] customAttributes = targetType.GetCustomAttributes(typeof(PromotedStructAttribute), true);

                if (customAttributes.Length > 0)
                {
                    foreach (PromotedStructAttribute attribute in customAttributes.Cast<PromotedStructAttribute>())
                    {
                        method = attribute.PromotedType.GetExtensionMethodSearchingPromotions(methodName);

                        if ((object)method != null)
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
        public static Delegate CreateStaticDelegate(this MethodInfo methodInfo, Type delegateType = null)
        {
            if ((object)delegateType == null)
                return methodInfo.CreateStaticDelegate(null, out bool _);

            try
            {
                return Delegate.CreateDelegate(delegateType, methodInfo);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a delegate for the given static method metadata.
        /// </summary>
        /// <param name="methodInfo">Method metadata of extension method.</param>
        /// <param name="delegateType">Specific delegate type to apply; set to <c>null</c> to use an auto-derived Func or Action delegate.</param>
        /// <param name="isByRef">Determines if extension target is accessed by reference.</param>
        /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
        public static Delegate CreateStaticDelegate(this MethodInfo methodInfo, Type delegateType, out bool isByRef)
        {
            Func<Type[], Type> getMethodType;
            List<Type> types = methodInfo.GetParameters().Select(paramInfo => paramInfo.ParameterType).ToList();

            if ((object)delegateType == null)
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
    }
}
