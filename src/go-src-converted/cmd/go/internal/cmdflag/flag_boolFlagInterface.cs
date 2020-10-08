//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 08 04:35:05 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using go;

#pragma warning disable CS0660, CS0661

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class cmdflag_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial interface boolFlag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static boolFlag As<T>(in T target) => (boolFlag<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static boolFlag As<T>(ptr<T> target_ptr) => (boolFlag<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static boolFlag? As(object target) =>
                typeof(boolFlag<>).CreateInterfaceHandler<boolFlag>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private class boolFlag<T> : boolFlag
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

            public boolFlag(in T target) => m_target = target;

            public boolFlag(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate bool IsBoolFlagByPtr(ptr<T> value);
            private delegate bool IsBoolFlagByVal(T value);

            private static readonly IsBoolFlagByPtr s_IsBoolFlagByPtr;
            private static readonly IsBoolFlagByVal s_IsBoolFlagByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsBoolFlag()
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_IsBoolFlagByPtr is null || !m_target_is_ptr)
                    return s_IsBoolFlagByVal!(target);

                return s_IsBoolFlagByPtr(m_target_ptr);
            }
            
            public string ToString(string format, IFormatProvider formatProvider) => format;

            [DebuggerStepperBoundary]
            static boolFlag()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("IsBoolFlag");

                if (!(extensionMethod is null))
                    s_IsBoolFlagByPtr = extensionMethod.CreateStaticDelegate(typeof(IsBoolFlagByPtr)) as IsBoolFlagByPtr;

                extensionMethod = targetType.GetExtensionMethod("IsBoolFlag");

                if (!(extensionMethod is null))
                    s_IsBoolFlagByVal = extensionMethod.CreateStaticDelegate(typeof(IsBoolFlagByVal)) as IsBoolFlagByVal;

                if (s_IsBoolFlagByPtr is null && s_IsBoolFlagByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement boolFlag.IsBoolFlag method", new Exception("IsBoolFlag"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator boolFlag<T>(in ptr<T> target_ptr) => new boolFlag<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator boolFlag<T>(in T target) => new boolFlag<T>(target);

            // Enable comparisons between nil and boolFlag<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(boolFlag<T> value, NilType nil) => Activator.CreateInstance<boolFlag<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(boolFlag<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, boolFlag<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, boolFlag<T> value) => value != nil;
        }
    }
}}}}

namespace go
{
    public static class cmdflag_boolFlagExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.cmd.go.@internal.cmdflag_package.boolFlag target)
        {
            try
            {
                return ((go.cmd.go.@internal.cmdflag_package.boolFlag<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.cmd.go.@internal.cmdflag_package.boolFlag target, out T result)
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

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static object? _(this go.cmd.go.@internal.cmdflag_package.boolFlag target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.cmd.go.@internal.cmdflag_package.boolFlag<>).GetExplicitGenericConversionOperator(type));

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

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _(this go.cmd.go.@internal.cmdflag_package.boolFlag target, Type type, out object? result)
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