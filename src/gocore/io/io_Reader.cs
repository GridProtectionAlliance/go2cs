//******************************************************************************************************
//  Reader.cs - Gbtc
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
using static go.io_package;

#pragma warning disable IDE0044, CS8618

namespace go
{
    public static partial class io_package
    {
        /// <summary>
        /// The Reader interface type wraps the basic Read method.
        /// </summary>
        public partial interface Reader : IFormattable
        {
        #if NET5_0
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Reader As<T>(ref T target) =>
                (Reader<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Reader As<T>(ptr<T> target_ptr) =>
                (Reader<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Reader? As(object target) =>
                typeof(Reader<>).CreateInterfaceHandler<Reader>(target);
        #endif
        }

        public class Reader<T> : Reader
        {
            private T m_target;
            private readonly ptr<T>? m_target_ptr;
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

            public Reader(in T target) => m_target = target;

            public Reader(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate (int, error) ReadByRef(ref T value, in slice<byte> p);
            private delegate (int, error) ReadByVal(T value, in slice<byte> p);

            private static readonly ReadByRef? s_ReadByRef;
            private static readonly ReadByVal? s_ReadByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (int, error) Read(in slice<byte> p)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ReadByRef is null)
                    return s_ReadByVal!(target, p);

                return s_ReadByRef(ref target, p);
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
            static Reader()
            {
                Type targetType = typeof(T);
                Type targetTypeByRef = targetType.MakeByRefType();

                MethodInfo extensionMethod = targetTypeByRef.GetExtensionMethod("Read");

                if (extensionMethod is not null)
                    s_ReadByRef = extensionMethod.CreateStaticDelegate(typeof(ReadByRef)) as ReadByRef;

                if (s_ReadByRef is null)
                {
                    extensionMethod = targetType.GetExtensionMethod("Read");

                    if (extensionMethod is not null)
                        s_ReadByVal = extensionMethod.CreateStaticDelegate(typeof(ReadByVal)) as ReadByVal;
                }

                if (s_ReadByRef is null && s_ReadByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Reader.Read method", new Exception("Read"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Reader<T>(in ptr<T> target_ptr) => new Reader<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Reader<T>(in T target) => new Reader<T>(target);

            // Enable comparisons between nil and Reader<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Reader<T> value, NilType _) => Activator.CreateInstance<Reader<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Reader<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Reader<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Reader<T> value) => value != nil;
        }
    }

    public static class ReaderExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this Reader target)
        {
            try
            {
                return ((Reader<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}", ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this Reader target, out T result)
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
        public static object? _(this Reader target, Type type)
        {
            try
            {
                MethodInfo conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(Reader<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this Reader target, Type type, out object? result)
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
