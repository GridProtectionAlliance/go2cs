//******************************************************************************************************
//  error.cs - Gbtc
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
using static go.builtin;

#pragma warning disable IDE0044, CS8618

// TODO: Keep error implementation updated to match best interface template pattern

namespace go
{
    /// <summary>
    /// The built-in error interface type is the conventional interface for representing an
    /// error condition, with the nil value representing no error.
    /// </summary>
    public interface error : IFormattable
    {
        /// <summary>
        /// Get string that represents an error.
        /// </summary>
        @string Error();

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static error As<T>(in T target) => (error<T>)target!;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static error As<T>(ptr<T> target_ptr) => (error<T>)target_ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static error? As(object target) =>
            typeof(error<>).CreateInterfaceHandler<error>(target);
    }

    public class error<T> : error
    {
        private T m_target;
        private readonly ptr<T>? m_target_ptr;
        private readonly bool m_target_is_ptr;

        public ref T Target
        {
            get
            {
                if (m_target_is_ptr && !(m_target_ptr is null))
                    return ref m_target_ptr.val;

                return ref m_target;
            }
        }

        public error(in T target) => m_target = target;

        public error(ptr<T> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }

        private delegate @string ErrorByPtr(ptr<T> value);
        private delegate @string ErrorByVal(T value);

        private static readonly ErrorByPtr? s_ErrorByPtr;
        private static readonly ErrorByVal? s_ErrorByVal;

        [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string Error()
        {
            T target = m_target;

            if (m_target_is_ptr && !(m_target_ptr is null))
                target = m_target_ptr.val;

            if (s_ErrorByPtr is null || !m_target_is_ptr)
                return s_ErrorByVal!(target);

            return s_ErrorByPtr(m_target_ptr!);
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
        static error()
        {
            Type targetType = typeof(T);
            Type targetTypeByPtr = typeof(ptr<T>);

            MethodInfo extensionMethod = targetTypeByPtr.GetExtensionMethod("Error");

            if (!(extensionMethod is null))
                s_ErrorByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorByPtr)) as ErrorByPtr;

            extensionMethod = targetType.GetExtensionMethod("Error");

            if (!(extensionMethod is null))
                s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;

            if (s_ErrorByPtr is null && s_ErrorByVal is null)
                throw new NotImplementedException($"{targetType.FullName} does not implement error.Error method", new Exception("Error"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator error<T>(in ptr<T> target_ptr) => new error<T>(target_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator error<T>(in T target) => new error<T>(target);

        // Enable comparisons between nil and error<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(error<T> value, NilType nil) => Activator.CreateInstance<error<T>>().Equals(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(error<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, error<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, error<T> value) => value != nil;
    }

    public static class errorExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this error target)
        {
            try
            {
                return ((error<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}", ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this error target, out T result)
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
        public static object? _(this error target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(error<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this error target, Type type, out object? result)
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
}
