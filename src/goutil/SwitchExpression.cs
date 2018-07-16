//******************************************************************************************************
//  Switch.cs - Gbtc
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
//  07/14/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;

namespace go
{
    public delegate SwitchExpression<T> SwitchCaseExpression<T>(Action action, bool fallThrough = false);

    public class SwitchExpression<T>
    {
        public SwitchExpression(T value) => Value = value;

        public T Value { get; }
    }

    public static class SwitchExpressionExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_interfaceTypeAsserts = new ConcurrentDictionary<Type, MethodInfo>();

        private static bool IsTypeMatch<T>(SwitchExpression<T> target, Type type)
        {
            // If type is nil, test for target value is nil
            if (type == typeof(NilType))
                return target.Value == nil;

            Type targetType = target.Value.GetType();

            // Check if target value type is or is assignable from specified test type
            if (targetType.IsAssignableFrom(type))
                return true;

            // Check if test type is an interface
            if (!type.IsInterface)
                return false;

            // For interfaces, you match even when you simply define matching extension methods,
            // check for match by calling interface's TypeAssert extension method
            MethodInfo typeAssert = s_interfaceTypeAsserts.GetOrAdd(type, _ =>
                type.GetExtensionMethods().FirstOrDefault(IsTypeAssertWithTypeParam)
            );

            if ((object)typeAssert == null)
                return false;

            try
            {
                return (bool)typeAssert.Invoke(null, new object[] { target.Value, type });
            }
            catch
            {
                return false;
            }            
        }

        private static bool IsTypeAssertWithTypeParam(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return
                method.Name == "TypeAssert" &&
                parameters.Length == 2 &&
                parameters[1].ParameterType == typeof(Type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchCaseExpression<T> Case<T>(this SwitchExpression<T> target, params Type[] typeValues)
        {
            return (action, fallThrough) => target.Case(() => typeValues.Any(typeValue => IsTypeMatch(target, typeValue)), action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchCaseExpression<T> Case<T>(this SwitchExpression<T> target, params bool[] testValues)
        {
            return (action, fallThrough) => target.Case(() => testValues.Any(testValue => testValue), action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchCaseExpression<T> Case<T>(this SwitchExpression<T> target, params T[] testValues)
        {
            return (action, fallThrough) => target.Case(testValues, action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<T> Case<T>(this SwitchExpression<T> target, IEnumerable<T> testValues, Action action, bool fallThrough)
        {
            return target.Case(() => testValues.Any(testValue => testValue.Equals(target.Value)), action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<T> Case<T>(this SwitchExpression<T> target, T testValue, Action action, bool fallThrough)
        {
            return target.Case(() => testValue.Equals(target.Value), action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Action, SwitchExpression<T>> Case<T>(this SwitchExpression<T> target, params Func<bool>[] conditions)
        {
            return action => target.Case(conditions, action, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Action, SwitchExpression<T>> CaseFallthrough<T>(this SwitchExpression<T> target, params Func<bool>[] conditions)
        {
            return action => target.Case(conditions, action, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<T> Case<T>(this SwitchExpression<T> target, IEnumerable<Func<bool>> conditions, Action action, bool fallThrough)
        {
            return target.Case(() => conditions.Any(condition => condition()), action, fallThrough);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<T> Case<T>(this SwitchExpression<T> target, Func<bool> condition, Action action, bool fallThrough)
        {
            if (target == null)
                return null;

            if (!condition())
                return target;

            action();

            return fallThrough ? target : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Default<T>(this SwitchExpression<T> target, Action action)
        {
            if (target != null)
                action();
        }
    }
}