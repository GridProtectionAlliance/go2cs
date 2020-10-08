//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 08 04:54:23 UTC
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
using format = go.go.format_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;
using analysis = go.golang.org.x.tools.go.analysis_package;
using checker = go.golang.org.x.tools.go.analysis.@internal.checker_package;
using packages = go.golang.org.x.tools.go.packages_package;
using diff = go.golang.org.x.tools.@internal.lsp.diff_package;
using myers = go.golang.org.x.tools.@internal.lsp.diff.myers_package;
using span = go.golang.org.x.tools.@internal.span_package;
using testenv = go.golang.org.x.tools.@internal.testenv_package;
using txtar = go.golang.org.x.tools.txtar_package;
using go;

#pragma warning disable CS0660, CS0661

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis
{
    public static partial class analysistest_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial interface Testing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Testing As<T>(in T target) => (Testing<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Testing As<T>(ptr<T> target_ptr) => (Testing<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static Testing? As(object target) =>
                typeof(Testing<>).CreateInterfaceHandler<Testing>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public class Testing<T> : Testing
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

            public Testing(in T target) => m_target = target;

            public Testing(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate void ErrorfByPtr(ptr<T> value, @string format, params object[] args);
            private delegate void ErrorfByVal(T value, @string format, params object[] args);

            private static readonly ErrorfByPtr s_ErrorfByPtr;
            private static readonly ErrorfByVal s_ErrorfByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Errorf(@string format, params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_ErrorfByPtr is null || !m_target_is_ptr)
                {
                    s_ErrorfByVal!(target, format, args);
                    return;
                }

                s_ErrorfByPtr(m_target_ptr, format, args);
                return;
                
            }
            
            public string ToString(string format, IFormatProvider formatProvider) => format;

            [DebuggerStepperBoundary]
            static Testing()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Errorf");

                if (!(extensionMethod is null))
                    s_ErrorfByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorfByPtr)) as ErrorfByPtr;

                extensionMethod = targetType.GetExtensionMethod("Errorf");

                if (!(extensionMethod is null))
                    s_ErrorfByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorfByVal)) as ErrorfByVal;

                if (s_ErrorfByPtr is null && s_ErrorfByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement Testing.Errorf method", new Exception("Errorf"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Testing<T>(in ptr<T> target_ptr) => new Testing<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator Testing<T>(in T target) => new Testing<T>(target);

            // Enable comparisons between nil and Testing<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Testing<T> value, NilType nil) => Activator.CreateInstance<Testing<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Testing<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Testing<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Testing<T> value) => value != nil;
        }
    }
}}}}}}

namespace go
{
    public static class analysistest_TestingExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.golang.org.x.tools.go.analysis.analysistest_package.Testing target)
        {
            try
            {
                return ((go.golang.org.x.tools.go.analysis.analysistest_package.Testing<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.golang.org.x.tools.go.analysis.analysistest_package.Testing target, out T result)
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
        public static object? _(this go.golang.org.x.tools.go.analysis.analysistest_package.Testing target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.golang.org.x.tools.go.analysis.analysistest_package.Testing<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.golang.org.x.tools.go.analysis.analysistest_package.Testing target, Type type, out object? result)
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