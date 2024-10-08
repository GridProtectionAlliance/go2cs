//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:27:37 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using unsafeheader = go.@internal.unsafeheader_package;
using @unsafe = go.@unsafe_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace @internal
{
    public static partial class reflectlite_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial interface Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Type As<T>(in T target) => (Type<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Type As<T>(ptr<T> target_ptr) => (Type<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Type? As(object target) =>
                typeof(Type<>).CreateInterfaceHandler<Type>(target);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public class Type<T> : Type
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

            public Type(in T target) => m_target = target;

            public Type(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate ptr<uncommonType> NameByPtr(ptr<T> value);
            private delegate ptr<uncommonType> NameByVal(T value);

            private static readonly NameByPtr? s_NameByPtr;
            private static readonly NameByVal? s_NameByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Name()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_NameByPtr is null || !m_target_is_ptr)
                    return s_NameByVal!(target);

                return s_NameByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> PkgPathByPtr(ptr<T> value);
            private delegate ptr<uncommonType> PkgPathByVal(T value);

            private static readonly PkgPathByPtr? s_PkgPathByPtr;
            private static readonly PkgPathByVal? s_PkgPathByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> PkgPath()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_PkgPathByPtr is null || !m_target_is_ptr)
                    return s_PkgPathByVal!(target);

                return s_PkgPathByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> SizeByPtr(ptr<T> value);
            private delegate ptr<uncommonType> SizeByVal(T value);

            private static readonly SizeByPtr? s_SizeByPtr;
            private static readonly SizeByVal? s_SizeByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Size()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SizeByPtr is null || !m_target_is_ptr)
                    return s_SizeByVal!(target);

                return s_SizeByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> KindByPtr(ptr<T> value);
            private delegate ptr<uncommonType> KindByVal(T value);

            private static readonly KindByPtr? s_KindByPtr;
            private static readonly KindByVal? s_KindByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Kind()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_KindByPtr is null || !m_target_is_ptr)
                    return s_KindByVal!(target);

                return s_KindByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> ImplementsByPtr(ptr<T> value, Type u);
            private delegate ptr<uncommonType> ImplementsByVal(T value, Type u);

            private static readonly ImplementsByPtr? s_ImplementsByPtr;
            private static readonly ImplementsByVal? s_ImplementsByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Implements(Type u)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ImplementsByPtr is null || !m_target_is_ptr)
                    return s_ImplementsByVal!(target, u);

                return s_ImplementsByPtr(m_target_ptr!, u);
            }

            private delegate ptr<uncommonType> AssignableToByPtr(ptr<T> value, Type u);
            private delegate ptr<uncommonType> AssignableToByVal(T value, Type u);

            private static readonly AssignableToByPtr? s_AssignableToByPtr;
            private static readonly AssignableToByVal? s_AssignableToByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> AssignableTo(Type u)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_AssignableToByPtr is null || !m_target_is_ptr)
                    return s_AssignableToByVal!(target, u);

                return s_AssignableToByPtr(m_target_ptr!, u);
            }

            private delegate ptr<uncommonType> ComparableByPtr(ptr<T> value);
            private delegate ptr<uncommonType> ComparableByVal(T value);

            private static readonly ComparableByPtr? s_ComparableByPtr;
            private static readonly ComparableByVal? s_ComparableByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Comparable()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ComparableByPtr is null || !m_target_is_ptr)
                    return s_ComparableByVal!(target);

                return s_ComparableByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> StringByPtr(ptr<T> value);
            private delegate ptr<uncommonType> StringByVal(T value);

            private static readonly StringByPtr? s_StringByPtr;
            private static readonly StringByVal? s_StringByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> String()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_StringByPtr is null || !m_target_is_ptr)
                    return s_StringByVal!(target);

                return s_StringByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> ElemByPtr(ptr<T> value);
            private delegate ptr<uncommonType> ElemByVal(T value);

            private static readonly ElemByPtr? s_ElemByPtr;
            private static readonly ElemByVal? s_ElemByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> Elem()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ElemByPtr is null || !m_target_is_ptr)
                    return s_ElemByVal!(target);

                return s_ElemByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> commonByPtr(ptr<T> value);
            private delegate ptr<uncommonType> commonByVal(T value);

            private static readonly commonByPtr? s_commonByPtr;
            private static readonly commonByVal? s_commonByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> common()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_commonByPtr is null || !m_target_is_ptr)
                    return s_commonByVal!(target);

                return s_commonByPtr(m_target_ptr!);
            }

            private delegate ptr<uncommonType> uncommonByPtr(ptr<T> value);
            private delegate ptr<uncommonType> uncommonByVal(T value);

            private static readonly uncommonByPtr? s_uncommonByPtr;
            private static readonly uncommonByVal? s_uncommonByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ptr<uncommonType> uncommon()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_uncommonByPtr is null || !m_target_is_ptr)
                    return s_uncommonByVal!(target);

                return s_uncommonByPtr(m_target_ptr!);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format ?? GetGoTypeName(typeof(T));

            [DebuggerStepperBoundary]
            static Type()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Name");

                if (extensionMethod is not null)
                    s_NameByPtr = extensionMethod.CreateStaticDelegate(typeof(NameByPtr)) as NameByPtr;

                extensionMethod = targetType.GetExtensionMethod("Name");

                if (extensionMethod is not null)
                    s_NameByVal = extensionMethod.CreateStaticDelegate(typeof(NameByVal)) as NameByVal;

                if (s_NameByPtr is null && s_NameByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Name method", new Exception("Name"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("PkgPath");

                if (extensionMethod is not null)
                    s_PkgPathByPtr = extensionMethod.CreateStaticDelegate(typeof(PkgPathByPtr)) as PkgPathByPtr;

                extensionMethod = targetType.GetExtensionMethod("PkgPath");

                if (extensionMethod is not null)
                    s_PkgPathByVal = extensionMethod.CreateStaticDelegate(typeof(PkgPathByVal)) as PkgPathByVal;

                if (s_PkgPathByPtr is null && s_PkgPathByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.PkgPath method", new Exception("PkgPath"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Size");

                if (extensionMethod is not null)
                    s_SizeByPtr = extensionMethod.CreateStaticDelegate(typeof(SizeByPtr)) as SizeByPtr;

                extensionMethod = targetType.GetExtensionMethod("Size");

                if (extensionMethod is not null)
                    s_SizeByVal = extensionMethod.CreateStaticDelegate(typeof(SizeByVal)) as SizeByVal;

                if (s_SizeByPtr is null && s_SizeByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Size method", new Exception("Size"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Kind");

                if (extensionMethod is not null)
                    s_KindByPtr = extensionMethod.CreateStaticDelegate(typeof(KindByPtr)) as KindByPtr;

                extensionMethod = targetType.GetExtensionMethod("Kind");

                if (extensionMethod is not null)
                    s_KindByVal = extensionMethod.CreateStaticDelegate(typeof(KindByVal)) as KindByVal;

                if (s_KindByPtr is null && s_KindByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Kind method", new Exception("Kind"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Implements");

                if (extensionMethod is not null)
                    s_ImplementsByPtr = extensionMethod.CreateStaticDelegate(typeof(ImplementsByPtr)) as ImplementsByPtr;

                extensionMethod = targetType.GetExtensionMethod("Implements");

                if (extensionMethod is not null)
                    s_ImplementsByVal = extensionMethod.CreateStaticDelegate(typeof(ImplementsByVal)) as ImplementsByVal;

                if (s_ImplementsByPtr is null && s_ImplementsByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Implements method", new Exception("Implements"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("AssignableTo");

                if (extensionMethod is not null)
                    s_AssignableToByPtr = extensionMethod.CreateStaticDelegate(typeof(AssignableToByPtr)) as AssignableToByPtr;

                extensionMethod = targetType.GetExtensionMethod("AssignableTo");

                if (extensionMethod is not null)
                    s_AssignableToByVal = extensionMethod.CreateStaticDelegate(typeof(AssignableToByVal)) as AssignableToByVal;

                if (s_AssignableToByPtr is null && s_AssignableToByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.AssignableTo method", new Exception("AssignableTo"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Comparable");

                if (extensionMethod is not null)
                    s_ComparableByPtr = extensionMethod.CreateStaticDelegate(typeof(ComparableByPtr)) as ComparableByPtr;

                extensionMethod = targetType.GetExtensionMethod("Comparable");

                if (extensionMethod is not null)
                    s_ComparableByVal = extensionMethod.CreateStaticDelegate(typeof(ComparableByVal)) as ComparableByVal;

                if (s_ComparableByPtr is null && s_ComparableByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Comparable method", new Exception("Comparable"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("String");

                if (extensionMethod is not null)
                    s_StringByPtr = extensionMethod.CreateStaticDelegate(typeof(StringByPtr)) as StringByPtr;

                extensionMethod = targetType.GetExtensionMethod("String");

                if (extensionMethod is not null)
                    s_StringByVal = extensionMethod.CreateStaticDelegate(typeof(StringByVal)) as StringByVal;

                if (s_StringByPtr is null && s_StringByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.String method", new Exception("String"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Elem");

                if (extensionMethod is not null)
                    s_ElemByPtr = extensionMethod.CreateStaticDelegate(typeof(ElemByPtr)) as ElemByPtr;

                extensionMethod = targetType.GetExtensionMethod("Elem");

                if (extensionMethod is not null)
                    s_ElemByVal = extensionMethod.CreateStaticDelegate(typeof(ElemByVal)) as ElemByVal;

                if (s_ElemByPtr is null && s_ElemByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.Elem method", new Exception("Elem"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("common");

                if (extensionMethod is not null)
                    s_commonByPtr = extensionMethod.CreateStaticDelegate(typeof(commonByPtr)) as commonByPtr;

                extensionMethod = targetType.GetExtensionMethod("common");

                if (extensionMethod is not null)
                    s_commonByVal = extensionMethod.CreateStaticDelegate(typeof(commonByVal)) as commonByVal;

                if (s_commonByPtr is null && s_commonByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.common method", new Exception("common"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("uncommon");

                if (extensionMethod is not null)
                    s_uncommonByPtr = extensionMethod.CreateStaticDelegate(typeof(uncommonByPtr)) as uncommonByPtr;

                extensionMethod = targetType.GetExtensionMethod("uncommon");

                if (extensionMethod is not null)
                    s_uncommonByVal = extensionMethod.CreateStaticDelegate(typeof(uncommonByVal)) as uncommonByVal;

                if (s_uncommonByPtr is null && s_uncommonByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Type.uncommon method", new Exception("uncommon"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Type<T>(in ptr<T> target_ptr) => new Type<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Type<T>(in T target) => new Type<T>(target);

            // Enable comparisons between nil and Type<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Type<T> value, NilType nil) => Activator.CreateInstance<Type<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Type<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Type<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Type<T> value) => value != nil;
        }
    }
}}

namespace go
{
    public static class reflectlite_TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.@internal.reflectlite_package.Type target)
        {
            try
            {
                return ((go.@internal.reflectlite_package.Type<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.@internal.reflectlite_package.Type target, out T result)
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
        public static object? _(this go.@internal.reflectlite_package.Type target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.@internal.reflectlite_package.Type<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.@internal.reflectlite_package.Type target, Type type, out object? result)
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