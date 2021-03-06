//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:01:48 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using fmt = go.fmt_package;
using sort = go.sort_package;
using time = go.time_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event {
namespace export
{
    public static partial class metric_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial interface Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Data As<T>(in T target) => (Data<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Data As<T>(ptr<T> target_ptr) => (Data<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Data? As(object target) =>
                typeof(Data<>).CreateInterfaceHandler<Data>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public class Data<T> : Data
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

            public Data(in T target) => m_target = target;

            public Data(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate slice<slice<label.Label>> HandleByPtr(ptr<T> value);
            private delegate slice<slice<label.Label>> HandleByVal(T value);

            private static readonly HandleByPtr? s_HandleByPtr;
            private static readonly HandleByVal? s_HandleByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public slice<slice<label.Label>> Handle()
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_HandleByPtr is null || !m_target_is_ptr)
                    return s_HandleByVal!(target);

                return s_HandleByPtr(m_target_ptr);
            }

            private delegate slice<slice<label.Label>> GroupsByPtr(ptr<T> value);
            private delegate slice<slice<label.Label>> GroupsByVal(T value);

            private static readonly GroupsByPtr? s_GroupsByPtr;
            private static readonly GroupsByVal? s_GroupsByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public slice<slice<label.Label>> Groups()
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_GroupsByPtr is null || !m_target_is_ptr)
                    return s_GroupsByVal!(target);

                return s_GroupsByPtr(m_target_ptr);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format;

            [DebuggerStepperBoundary]
            static Data()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Handle");

                if (!(extensionMethod is null))
                    s_HandleByPtr = extensionMethod.CreateStaticDelegate(typeof(HandleByPtr)) as HandleByPtr;

                extensionMethod = targetType.GetExtensionMethod("Handle");

                if (!(extensionMethod is null))
                    s_HandleByVal = extensionMethod.CreateStaticDelegate(typeof(HandleByVal)) as HandleByVal;

                if (s_HandleByPtr is null && s_HandleByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Data.Handle method", new Exception("Handle"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Groups");

                if (!(extensionMethod is null))
                    s_GroupsByPtr = extensionMethod.CreateStaticDelegate(typeof(GroupsByPtr)) as GroupsByPtr;

                extensionMethod = targetType.GetExtensionMethod("Groups");

                if (!(extensionMethod is null))
                    s_GroupsByVal = extensionMethod.CreateStaticDelegate(typeof(GroupsByVal)) as GroupsByVal;

                if (s_GroupsByPtr is null && s_GroupsByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Data.Groups method", new Exception("Groups"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Data<T>(in ptr<T> target_ptr) => new Data<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Data<T>(in T target) => new Data<T>(target);

            // Enable comparisons between nil and Data<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Data<T> value, NilType nil) => Activator.CreateInstance<Data<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Data<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Data<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Data<T> value) => value != nil;
        }
    }
}}}}}}}

namespace go
{
    public static class metric_DataExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.golang.org.x.tools.@internal.@event.export.metric_package.Data target)
        {
            try
            {
                return ((go.golang.org.x.tools.@internal.@event.export.metric_package.Data<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.golang.org.x.tools.@internal.@event.export.metric_package.Data target, out T result)
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
        public static object? _(this go.golang.org.x.tools.@internal.@event.export.metric_package.Data target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.golang.org.x.tools.@internal.@event.export.metric_package.Data<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.golang.org.x.tools.@internal.@event.export.metric_package.Data target, Type type, out object? result)
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