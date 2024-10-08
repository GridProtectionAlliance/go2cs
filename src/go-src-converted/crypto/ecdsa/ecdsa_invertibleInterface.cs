//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:30:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using crypto = go.crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using randutil = go.crypto.@internal.randutil_package;
using sha512 = go.crypto.sha512_package;
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;
using cryptobyte = go.golang.org.x.crypto.cryptobyte_package;
using asn1 = go.golang.org.x.crypto.cryptobyte.asn1_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace crypto
{
    public static partial class ecdsa_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial interface invertible
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static invertible As<T>(in T target) => (invertible<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static invertible As<T>(ptr<T> target_ptr) => (invertible<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static invertible? As(object target) =>
                typeof(invertible<>).CreateInterfaceHandler<invertible>(target);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private class invertible<T> : invertible
        {
            private T m_target = default!;
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

            public invertible(in T target) => m_target = target;

            public invertible(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate ptr<big.Int> InverseByPtr(ptr<T> value, ptr<big.Int> k);
            private delegate ptr<big.Int> InverseByVal(T value, ptr<big.Int> k);

            private static readonly InverseByPtr? s_InverseByPtr;
            private static readonly InverseByVal? s_InverseByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<big.Int> Inverse(ptr<big.Int> k)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_InverseByPtr is null || !m_target_is_ptr)
                    return s_InverseByVal!(target, k);

                return s_InverseByPtr(m_target_ptr!, k);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format ?? GetGoTypeName(typeof(T));

            [DebuggerStepperBoundary]
            static invertible()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Inverse");

                if (extensionMethod is not null)
                    s_InverseByPtr = extensionMethod.CreateStaticDelegate(typeof(InverseByPtr)) as InverseByPtr;

                extensionMethod = targetType.GetExtensionMethod("Inverse");

                if (extensionMethod is not null)
                    s_InverseByVal = extensionMethod.CreateStaticDelegate(typeof(InverseByVal)) as InverseByVal;

                if (s_InverseByPtr is null && s_InverseByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement invertible.Inverse method", new Exception("Inverse"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator invertible<T>(in ptr<T> target_ptr) => new invertible<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator invertible<T>(in T target) => new invertible<T>(target);

            // Enable comparisons between nil and invertible<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(invertible<T> value, NilType nil) => Activator.CreateInstance<invertible<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(invertible<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, invertible<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, invertible<T> value) => value != nil;
        }
    }
}}

namespace go
{
    public static class ecdsa_invertibleExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.crypto.ecdsa_package.invertible target)
        {
            try
            {
                return ((go.crypto.ecdsa_package.invertible<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.crypto.ecdsa_package.invertible target, out T result)
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

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static object? _(this go.crypto.ecdsa_package.invertible target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.crypto.ecdsa_package.invertible<>).GetExplicitGenericConversionOperator(type));

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

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _(this go.crypto.ecdsa_package.invertible target, Type type, out object? result)
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