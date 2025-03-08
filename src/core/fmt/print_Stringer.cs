//******************************************************************************************************
//  Stringer.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  06/21/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.runtime;
using static go.fmt_package;

#pragma warning disable IDE0044, CS8618

namespace go;

public static partial class fmt_package
{
    /// <summary>
    /// The Stringer interface type is the conventional interface for representing
    /// formatted string output.
    /// </summary>
    public partial interface Stringer //: IFormattable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static Stringer As<T>(in T target) =>
            (Stringer<T>)target!;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static Stringer As<T>(ж<T> target_ptr) =>
            (Stringer<T>)target_ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static Stringer? As(object target) =>
            typeof(Stringer<>).CreateInterfaceHandler<Stringer>(target);
    }

    public class Stringer<T> : Stringer
    {
        private T m_target;
        private readonly ж<T>? m_target_ptr;
        private readonly bool m_target_is_ptr;

        public ref T Target
        {
            get
            {
                if (m_target_is_ptr && m_target_ptr is not null)
                    return ref m_target_ptr.val;

                return ref m_target;
            }
        }

        public Stringer(in T target) => m_target = target;

        public Stringer(ж<T> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }

        private delegate @string StringByRef(ref T value);
        private delegate @string StringByVal(T value);

        private static readonly StringByRef? s_StringByRef;
        private static readonly StringByVal? s_StringByVal;

        [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string String()
        {
            T target = m_target;

            if (m_target_is_ptr && m_target_ptr is not null)
                target = m_target_ptr.val;

            if (s_StringByRef is null)
                return s_StringByVal!(target);

            return s_StringByRef(ref target);
        }

        public string ToString(string? format, IFormatProvider? _)
        {
            switch (format)
            {
                case "T":
                {
                    string typeName = GetGoTypeName<T>().Replace("_package+", ".");
                    return m_target_is_ptr ? $"*{typeName}" : typeName;
                }
                case "v":
                {
                    if (m_target_is_ptr)
                        return m_target_ptr is null ? "<nil>" : $"&{m_target_ptr.val}";

                    return m_target?.ToString() ?? "<nil>";
                }
                default:
                    return ToString() ?? "<nil>";
            }
        }

        [DebuggerStepperBoundary]
        static Stringer()
        {
            Type targetType = typeof(T);
            Type targetTypeByRef = targetType.MakeByRefType();

            MethodInfo? extensionMethod = targetTypeByRef.GetExtensionMethod("String");

            if (extensionMethod is not null)
                s_StringByRef = extensionMethod.CreateStaticDelegate(typeof(StringByRef)) as StringByRef;

            if (s_StringByRef is null)
            {
                extensionMethod = targetType.GetExtensionMethod("String");

                if (extensionMethod is not null)
                    s_StringByVal = extensionMethod.CreateStaticDelegate(typeof(StringByVal)) as StringByVal;
            }

            if (s_StringByRef is null && s_StringByVal is null)
                throw new NotImplementedException($"{targetType.FullName} does not implement Stringer.String method", new Exception("String"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator Stringer<T>(in ж<T> target_ptr) => new Stringer<T>(target_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator Stringer<T>(in T target) => new Stringer<T>(target);

        // Enable comparisons between nil and Stringer<T> interface instance
        public static bool operator ==(Stringer<T> value, NilType _) => Activator.CreateInstance<Stringer<T>>().Equals(value);

        public static bool operator !=(Stringer<T> value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, Stringer<T> value) => value == nil;

        public static bool operator !=(NilType nil, Stringer<T> value) => value != nil;
    }
}

public static class StringerExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static T _<T>(this Stringer target)
    {
        try
        {
            return ((Stringer<T>)target).Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}", ex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static bool _<T>(this Stringer target, out T result)
    {
        try
        {
            result = target._<T>();
            return true;
        }
        catch (PanicException)
        {
            result = default!;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static object? _(this Stringer target, Type type)
    {
        try
        {
            MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(Stringer<>).GetExplicitGenericConversionOperator(type));

            if (conversionOperator is null)
                throw new PanicException($"interface conversion: failed to create converter for {GetGoTypeName(target.GetType())} to {GetGoTypeName(type)}");

            dynamic? result = conversionOperator.Invoke(null, new object[] { target });
            return result?.Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(type)}: missing method {ex.InnerException?.Message}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static bool _(this Stringer target, Type type, out object? result)
    {
        try
        {
            result = target._(type);
            return true;
        }
        catch (PanicException)
        {
            result = type.IsValueType ? Activator.CreateInstance(type) : null;
            return false;
        }
    }
}
