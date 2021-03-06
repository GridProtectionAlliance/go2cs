//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:03:20 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial interface setNumable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static setNumable As<T>(in T target) => (setNumable<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static setNumable As<T>(ptr<T> target_ptr) => (setNumable<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static setNumable? As(object target) =>
                typeof(setNumable<>).CreateInterfaceHandler<setNumable>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private class setNumable<T> : setNumable
        {
            private T m_target = default!;
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

            public setNumable(in T target) => m_target = target;

            public setNumable(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate void setNumByPtr(ptr<T> value, long _p0);
            private delegate void setNumByVal(T value, long _p0);

            private static readonly setNumByPtr? s_setNumByPtr;
            private static readonly setNumByVal? s_setNumByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void setNum(long _p0)
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_setNumByPtr is null || !m_target_is_ptr)
                {
                    s_setNumByVal!(target, _p0);
                    return;
                }

                s_setNumByPtr(m_target_ptr, _p0);
                return;
                
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format;

            [DebuggerStepperBoundary]
            static setNumable()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("setNum");

                if (!(extensionMethod is null))
                    s_setNumByPtr = extensionMethod.CreateStaticDelegate(typeof(setNumByPtr)) as setNumByPtr;

                extensionMethod = targetType.GetExtensionMethod("setNum");

                if (!(extensionMethod is null))
                    s_setNumByVal = extensionMethod.CreateStaticDelegate(typeof(setNumByVal)) as setNumByVal;

                if (s_setNumByPtr is null && s_setNumByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement setNumable.setNum method", new Exception("setNum"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator setNumable<T>(in ptr<T> target_ptr) => new setNumable<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator setNumable<T>(in T target) => new setNumable<T>(target);

            // Enable comparisons between nil and setNumable<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(setNumable<T> value, NilType nil) => Activator.CreateInstance<setNumable<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(setNumable<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, setNumable<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, setNumable<T> value) => value != nil;
        }
    }
}}}}}

namespace go
{
    public static class ssa_setNumableExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.golang.org.x.tools.go.ssa_package.setNumable target)
        {
            try
            {
                return ((go.golang.org.x.tools.go.ssa_package.setNumable<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.golang.org.x.tools.go.ssa_package.setNumable target, out T result)
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
        public static object? _(this go.golang.org.x.tools.go.ssa_package.setNumable target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.golang.org.x.tools.go.ssa_package.setNumable<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.golang.org.x.tools.go.ssa_package.setNumable target, Type type, out object? result)
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