//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:45:57 UTC
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
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial interface ImportPathError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static ImportPathError As<T>(in T target) => (ImportPathError<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static ImportPathError As<T>(ptr<T> target_ptr) => (ImportPathError<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static ImportPathError? As(object target) =>
                typeof(ImportPathError<>).CreateInterfaceHandler<ImportPathError>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public class ImportPathError<T> : ImportPathError
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

            public ImportPathError(in T target) => m_target = target;

            public ImportPathError(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate @string ImportPathByPtr(ptr<T> value);
            private delegate @string ImportPathByVal(T value);

            private static readonly ImportPathByPtr? s_ImportPathByPtr;
            private static readonly ImportPathByVal? s_ImportPathByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string ImportPath()
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_ImportPathByPtr is null || !m_target_is_ptr)
                    return s_ImportPathByVal!(target);

                return s_ImportPathByPtr(m_target_ptr);
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

                return s_ErrorByPtr(m_target_ptr);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format;

            [DebuggerStepperBoundary]
            static ImportPathError()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("ImportPath");

                if (!(extensionMethod is null))
                    s_ImportPathByPtr = extensionMethod.CreateStaticDelegate(typeof(ImportPathByPtr)) as ImportPathByPtr;

                extensionMethod = targetType.GetExtensionMethod("ImportPath");

                if (!(extensionMethod is null))
                    s_ImportPathByVal = extensionMethod.CreateStaticDelegate(typeof(ImportPathByVal)) as ImportPathByVal;

                if (s_ImportPathByPtr is null && s_ImportPathByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement ImportPathError.ImportPath method", new Exception("ImportPath"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Error");

                if (!(extensionMethod is null))
                    s_ErrorByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorByPtr)) as ErrorByPtr;

                extensionMethod = targetType.GetExtensionMethod("Error");

                if (!(extensionMethod is null))
                    s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;

                if (s_ErrorByPtr is null && s_ErrorByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement ImportPathError.Error method", new Exception("Error"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator ImportPathError<T>(in ptr<T> target_ptr) => new ImportPathError<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator ImportPathError<T>(in T target) => new ImportPathError<T>(target);

            // Enable comparisons between nil and ImportPathError<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ImportPathError<T> value, NilType nil) => Activator.CreateInstance<ImportPathError<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ImportPathError<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ImportPathError<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ImportPathError<T> value) => value != nil;
        }
    }
}}}}

namespace go
{
    public static class load_ImportPathErrorExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.cmd.go.@internal.load_package.ImportPathError target)
        {
            try
            {
                return ((go.cmd.go.@internal.load_package.ImportPathError<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.cmd.go.@internal.load_package.ImportPathError target, out T result)
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
        public static object? _(this go.cmd.go.@internal.load_package.ImportPathError target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.cmd.go.@internal.load_package.ImportPathError<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.cmd.go.@internal.load_package.ImportPathError target, Type type, out object? result)
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