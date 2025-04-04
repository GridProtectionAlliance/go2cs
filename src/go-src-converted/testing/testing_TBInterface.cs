//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:43:10 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using race = go.@internal.race_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using trace = go.runtime.trace_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go
{
    public static partial class testing_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial interface TB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static TB As<T>(in T target) => (TB<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static TB As<T>(ptr<T> target_ptr) => (TB<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static TB? As(object target) =>
                typeof(TB<>).CreateInterfaceHandler<TB>(target);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public class TB<T> : TB
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

            public TB(in T target) => m_target = target;

            public TB(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate @string CleanupByPtr(ptr<T> value, Action _p0);
            private delegate @string CleanupByVal(T value, Action _p0);

            private static readonly CleanupByPtr? s_CleanupByPtr;
            private static readonly CleanupByVal? s_CleanupByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Cleanup(Action _p0)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_CleanupByPtr is null || !m_target_is_ptr)
                    return s_CleanupByVal!(target, _p0);

                return s_CleanupByPtr(m_target_ptr!, _p0);
            }

            private delegate @string ErrorByPtr(ptr<T> value, params object[] args);
            private delegate @string ErrorByVal(T value, params object[] args);

            private static readonly ErrorByPtr? s_ErrorByPtr;
            private static readonly ErrorByVal? s_ErrorByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Error(params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ErrorByPtr is null || !m_target_is_ptr)
                    return s_ErrorByVal!(target, args);

                return s_ErrorByPtr(m_target_ptr!, args);
            }

            private delegate @string ErrorfByPtr(ptr<T> value, @string format, params object[] args);
            private delegate @string ErrorfByVal(T value, @string format, params object[] args);

            private static readonly ErrorfByPtr? s_ErrorfByPtr;
            private static readonly ErrorfByVal? s_ErrorfByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Errorf(@string format, params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ErrorfByPtr is null || !m_target_is_ptr)
                    return s_ErrorfByVal!(target, format, args);

                return s_ErrorfByPtr(m_target_ptr!, format, args);
            }

            private delegate @string FailByPtr(ptr<T> value);
            private delegate @string FailByVal(T value);

            private static readonly FailByPtr? s_FailByPtr;
            private static readonly FailByVal? s_FailByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Fail()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_FailByPtr is null || !m_target_is_ptr)
                    return s_FailByVal!(target);

                return s_FailByPtr(m_target_ptr!);
            }

            private delegate @string FailNowByPtr(ptr<T> value);
            private delegate @string FailNowByVal(T value);

            private static readonly FailNowByPtr? s_FailNowByPtr;
            private static readonly FailNowByVal? s_FailNowByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string FailNow()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_FailNowByPtr is null || !m_target_is_ptr)
                    return s_FailNowByVal!(target);

                return s_FailNowByPtr(m_target_ptr!);
            }

            private delegate @string FailedByPtr(ptr<T> value);
            private delegate @string FailedByVal(T value);

            private static readonly FailedByPtr? s_FailedByPtr;
            private static readonly FailedByVal? s_FailedByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Failed()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_FailedByPtr is null || !m_target_is_ptr)
                    return s_FailedByVal!(target);

                return s_FailedByPtr(m_target_ptr!);
            }

            private delegate @string FatalByPtr(ptr<T> value, params object[] args);
            private delegate @string FatalByVal(T value, params object[] args);

            private static readonly FatalByPtr? s_FatalByPtr;
            private static readonly FatalByVal? s_FatalByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Fatal(params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_FatalByPtr is null || !m_target_is_ptr)
                    return s_FatalByVal!(target, args);

                return s_FatalByPtr(m_target_ptr!, args);
            }

            private delegate @string FatalfByPtr(ptr<T> value, @string format, params object[] args);
            private delegate @string FatalfByVal(T value, @string format, params object[] args);

            private static readonly FatalfByPtr? s_FatalfByPtr;
            private static readonly FatalfByVal? s_FatalfByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Fatalf(@string format, params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_FatalfByPtr is null || !m_target_is_ptr)
                    return s_FatalfByVal!(target, format, args);

                return s_FatalfByPtr(m_target_ptr!, format, args);
            }

            private delegate @string HelperByPtr(ptr<T> value);
            private delegate @string HelperByVal(T value);

            private static readonly HelperByPtr? s_HelperByPtr;
            private static readonly HelperByVal? s_HelperByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Helper()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_HelperByPtr is null || !m_target_is_ptr)
                    return s_HelperByVal!(target);

                return s_HelperByPtr(m_target_ptr!);
            }

            private delegate @string LogByPtr(ptr<T> value, params object[] args);
            private delegate @string LogByVal(T value, params object[] args);

            private static readonly LogByPtr? s_LogByPtr;
            private static readonly LogByVal? s_LogByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Log(params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_LogByPtr is null || !m_target_is_ptr)
                    return s_LogByVal!(target, args);

                return s_LogByPtr(m_target_ptr!, args);
            }

            private delegate @string LogfByPtr(ptr<T> value, @string format, params object[] args);
            private delegate @string LogfByVal(T value, @string format, params object[] args);

            private static readonly LogfByPtr? s_LogfByPtr;
            private static readonly LogfByVal? s_LogfByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Logf(@string format, params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_LogfByPtr is null || !m_target_is_ptr)
                    return s_LogfByVal!(target, format, args);

                return s_LogfByPtr(m_target_ptr!, format, args);
            }

            private delegate @string NameByPtr(ptr<T> value);
            private delegate @string NameByVal(T value);

            private static readonly NameByPtr? s_NameByPtr;
            private static readonly NameByVal? s_NameByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Name()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_NameByPtr is null || !m_target_is_ptr)
                    return s_NameByVal!(target);

                return s_NameByPtr(m_target_ptr!);
            }

            private delegate @string SetenvByPtr(ptr<T> value, @string key, @string value);
            private delegate @string SetenvByVal(T value, @string key, @string value);

            private static readonly SetenvByPtr? s_SetenvByPtr;
            private static readonly SetenvByVal? s_SetenvByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Setenv(@string key, @string value)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SetenvByPtr is null || !m_target_is_ptr)
                    return s_SetenvByVal!(target, key, value);

                return s_SetenvByPtr(m_target_ptr!, key, value);
            }

            private delegate @string SkipByPtr(ptr<T> value, params object[] args);
            private delegate @string SkipByVal(T value, params object[] args);

            private static readonly SkipByPtr? s_SkipByPtr;
            private static readonly SkipByVal? s_SkipByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Skip(params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SkipByPtr is null || !m_target_is_ptr)
                    return s_SkipByVal!(target, args);

                return s_SkipByPtr(m_target_ptr!, args);
            }

            private delegate @string SkipNowByPtr(ptr<T> value);
            private delegate @string SkipNowByVal(T value);

            private static readonly SkipNowByPtr? s_SkipNowByPtr;
            private static readonly SkipNowByVal? s_SkipNowByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string SkipNow()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SkipNowByPtr is null || !m_target_is_ptr)
                    return s_SkipNowByVal!(target);

                return s_SkipNowByPtr(m_target_ptr!);
            }

            private delegate @string SkipfByPtr(ptr<T> value, @string format, params object[] args);
            private delegate @string SkipfByVal(T value, @string format, params object[] args);

            private static readonly SkipfByPtr? s_SkipfByPtr;
            private static readonly SkipfByVal? s_SkipfByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Skipf(@string format, params object[] args)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SkipfByPtr is null || !m_target_is_ptr)
                    return s_SkipfByVal!(target, format, args);

                return s_SkipfByPtr(m_target_ptr!, format, args);
            }

            private delegate @string SkippedByPtr(ptr<T> value);
            private delegate @string SkippedByVal(T value);

            private static readonly SkippedByPtr? s_SkippedByPtr;
            private static readonly SkippedByVal? s_SkippedByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string Skipped()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_SkippedByPtr is null || !m_target_is_ptr)
                    return s_SkippedByVal!(target);

                return s_SkippedByPtr(m_target_ptr!);
            }

            private delegate @string TempDirByPtr(ptr<T> value);
            private delegate @string TempDirByVal(T value);

            private static readonly TempDirByPtr? s_TempDirByPtr;
            private static readonly TempDirByVal? s_TempDirByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string TempDir()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_TempDirByPtr is null || !m_target_is_ptr)
                    return s_TempDirByVal!(target);

                return s_TempDirByPtr(m_target_ptr!);
            }

            private delegate @string privateByPtr(ptr<T> value);
            private delegate @string privateByVal(T value);

            private static readonly privateByPtr? s_privateByPtr;
            private static readonly privateByVal? s_privateByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string private()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_privateByPtr is null || !m_target_is_ptr)
                    return s_privateByVal!(target);

                return s_privateByPtr(m_target_ptr!);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format ?? GetGoTypeName(typeof(T));

            [DebuggerStepperBoundary]
            static TB()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Cleanup");

                if (extensionMethod is not null)
                    s_CleanupByPtr = extensionMethod.CreateStaticDelegate(typeof(CleanupByPtr)) as CleanupByPtr;

                extensionMethod = targetType.GetExtensionMethod("Cleanup");

                if (extensionMethod is not null)
                    s_CleanupByVal = extensionMethod.CreateStaticDelegate(typeof(CleanupByVal)) as CleanupByVal;

                if (s_CleanupByPtr is null && s_CleanupByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Cleanup method", new Exception("Cleanup"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Error");

                if (extensionMethod is not null)
                    s_ErrorByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorByPtr)) as ErrorByPtr;

                extensionMethod = targetType.GetExtensionMethod("Error");

                if (extensionMethod is not null)
                    s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;

                if (s_ErrorByPtr is null && s_ErrorByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Error method", new Exception("Error"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Errorf");

                if (extensionMethod is not null)
                    s_ErrorfByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorfByPtr)) as ErrorfByPtr;

                extensionMethod = targetType.GetExtensionMethod("Errorf");

                if (extensionMethod is not null)
                    s_ErrorfByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorfByVal)) as ErrorfByVal;

                if (s_ErrorfByPtr is null && s_ErrorfByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Errorf method", new Exception("Errorf"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Fail");

                if (extensionMethod is not null)
                    s_FailByPtr = extensionMethod.CreateStaticDelegate(typeof(FailByPtr)) as FailByPtr;

                extensionMethod = targetType.GetExtensionMethod("Fail");

                if (extensionMethod is not null)
                    s_FailByVal = extensionMethod.CreateStaticDelegate(typeof(FailByVal)) as FailByVal;

                if (s_FailByPtr is null && s_FailByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Fail method", new Exception("Fail"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("FailNow");

                if (extensionMethod is not null)
                    s_FailNowByPtr = extensionMethod.CreateStaticDelegate(typeof(FailNowByPtr)) as FailNowByPtr;

                extensionMethod = targetType.GetExtensionMethod("FailNow");

                if (extensionMethod is not null)
                    s_FailNowByVal = extensionMethod.CreateStaticDelegate(typeof(FailNowByVal)) as FailNowByVal;

                if (s_FailNowByPtr is null && s_FailNowByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.FailNow method", new Exception("FailNow"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Failed");

                if (extensionMethod is not null)
                    s_FailedByPtr = extensionMethod.CreateStaticDelegate(typeof(FailedByPtr)) as FailedByPtr;

                extensionMethod = targetType.GetExtensionMethod("Failed");

                if (extensionMethod is not null)
                    s_FailedByVal = extensionMethod.CreateStaticDelegate(typeof(FailedByVal)) as FailedByVal;

                if (s_FailedByPtr is null && s_FailedByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Failed method", new Exception("Failed"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Fatal");

                if (extensionMethod is not null)
                    s_FatalByPtr = extensionMethod.CreateStaticDelegate(typeof(FatalByPtr)) as FatalByPtr;

                extensionMethod = targetType.GetExtensionMethod("Fatal");

                if (extensionMethod is not null)
                    s_FatalByVal = extensionMethod.CreateStaticDelegate(typeof(FatalByVal)) as FatalByVal;

                if (s_FatalByPtr is null && s_FatalByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Fatal method", new Exception("Fatal"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Fatalf");

                if (extensionMethod is not null)
                    s_FatalfByPtr = extensionMethod.CreateStaticDelegate(typeof(FatalfByPtr)) as FatalfByPtr;

                extensionMethod = targetType.GetExtensionMethod("Fatalf");

                if (extensionMethod is not null)
                    s_FatalfByVal = extensionMethod.CreateStaticDelegate(typeof(FatalfByVal)) as FatalfByVal;

                if (s_FatalfByPtr is null && s_FatalfByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Fatalf method", new Exception("Fatalf"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Helper");

                if (extensionMethod is not null)
                    s_HelperByPtr = extensionMethod.CreateStaticDelegate(typeof(HelperByPtr)) as HelperByPtr;

                extensionMethod = targetType.GetExtensionMethod("Helper");

                if (extensionMethod is not null)
                    s_HelperByVal = extensionMethod.CreateStaticDelegate(typeof(HelperByVal)) as HelperByVal;

                if (s_HelperByPtr is null && s_HelperByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Helper method", new Exception("Helper"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Log");

                if (extensionMethod is not null)
                    s_LogByPtr = extensionMethod.CreateStaticDelegate(typeof(LogByPtr)) as LogByPtr;

                extensionMethod = targetType.GetExtensionMethod("Log");

                if (extensionMethod is not null)
                    s_LogByVal = extensionMethod.CreateStaticDelegate(typeof(LogByVal)) as LogByVal;

                if (s_LogByPtr is null && s_LogByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Log method", new Exception("Log"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Logf");

                if (extensionMethod is not null)
                    s_LogfByPtr = extensionMethod.CreateStaticDelegate(typeof(LogfByPtr)) as LogfByPtr;

                extensionMethod = targetType.GetExtensionMethod("Logf");

                if (extensionMethod is not null)
                    s_LogfByVal = extensionMethod.CreateStaticDelegate(typeof(LogfByVal)) as LogfByVal;

                if (s_LogfByPtr is null && s_LogfByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Logf method", new Exception("Logf"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Name");

                if (extensionMethod is not null)
                    s_NameByPtr = extensionMethod.CreateStaticDelegate(typeof(NameByPtr)) as NameByPtr;

                extensionMethod = targetType.GetExtensionMethod("Name");

                if (extensionMethod is not null)
                    s_NameByVal = extensionMethod.CreateStaticDelegate(typeof(NameByVal)) as NameByVal;

                if (s_NameByPtr is null && s_NameByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Name method", new Exception("Name"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Setenv");

                if (extensionMethod is not null)
                    s_SetenvByPtr = extensionMethod.CreateStaticDelegate(typeof(SetenvByPtr)) as SetenvByPtr;

                extensionMethod = targetType.GetExtensionMethod("Setenv");

                if (extensionMethod is not null)
                    s_SetenvByVal = extensionMethod.CreateStaticDelegate(typeof(SetenvByVal)) as SetenvByVal;

                if (s_SetenvByPtr is null && s_SetenvByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Setenv method", new Exception("Setenv"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Skip");

                if (extensionMethod is not null)
                    s_SkipByPtr = extensionMethod.CreateStaticDelegate(typeof(SkipByPtr)) as SkipByPtr;

                extensionMethod = targetType.GetExtensionMethod("Skip");

                if (extensionMethod is not null)
                    s_SkipByVal = extensionMethod.CreateStaticDelegate(typeof(SkipByVal)) as SkipByVal;

                if (s_SkipByPtr is null && s_SkipByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Skip method", new Exception("Skip"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("SkipNow");

                if (extensionMethod is not null)
                    s_SkipNowByPtr = extensionMethod.CreateStaticDelegate(typeof(SkipNowByPtr)) as SkipNowByPtr;

                extensionMethod = targetType.GetExtensionMethod("SkipNow");

                if (extensionMethod is not null)
                    s_SkipNowByVal = extensionMethod.CreateStaticDelegate(typeof(SkipNowByVal)) as SkipNowByVal;

                if (s_SkipNowByPtr is null && s_SkipNowByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.SkipNow method", new Exception("SkipNow"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Skipf");

                if (extensionMethod is not null)
                    s_SkipfByPtr = extensionMethod.CreateStaticDelegate(typeof(SkipfByPtr)) as SkipfByPtr;

                extensionMethod = targetType.GetExtensionMethod("Skipf");

                if (extensionMethod is not null)
                    s_SkipfByVal = extensionMethod.CreateStaticDelegate(typeof(SkipfByVal)) as SkipfByVal;

                if (s_SkipfByPtr is null && s_SkipfByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Skipf method", new Exception("Skipf"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Skipped");

                if (extensionMethod is not null)
                    s_SkippedByPtr = extensionMethod.CreateStaticDelegate(typeof(SkippedByPtr)) as SkippedByPtr;

                extensionMethod = targetType.GetExtensionMethod("Skipped");

                if (extensionMethod is not null)
                    s_SkippedByVal = extensionMethod.CreateStaticDelegate(typeof(SkippedByVal)) as SkippedByVal;

                if (s_SkippedByPtr is null && s_SkippedByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.Skipped method", new Exception("Skipped"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("TempDir");

                if (extensionMethod is not null)
                    s_TempDirByPtr = extensionMethod.CreateStaticDelegate(typeof(TempDirByPtr)) as TempDirByPtr;

                extensionMethod = targetType.GetExtensionMethod("TempDir");

                if (extensionMethod is not null)
                    s_TempDirByVal = extensionMethod.CreateStaticDelegate(typeof(TempDirByVal)) as TempDirByVal;

                if (s_TempDirByPtr is null && s_TempDirByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.TempDir method", new Exception("TempDir"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("private");

                if (extensionMethod is not null)
                    s_privateByPtr = extensionMethod.CreateStaticDelegate(typeof(privateByPtr)) as privateByPtr;

                extensionMethod = targetType.GetExtensionMethod("private");

                if (extensionMethod is not null)
                    s_privateByVal = extensionMethod.CreateStaticDelegate(typeof(privateByVal)) as privateByVal;

                if (s_privateByPtr is null && s_privateByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement TB.private method", new Exception("private"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator TB<T>(in ptr<T> target_ptr) => new TB<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator TB<T>(in T target) => new TB<T>(target);

            // Enable comparisons between nil and TB<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(TB<T> value, NilType nil) => Activator.CreateInstance<TB<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(TB<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, TB<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, TB<T> value) => value != nil;
        }
    }
}

namespace go
{
    public static class testing_TBExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.testing_package.TB target)
        {
            try
            {
                return ((go.testing_package.TB<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.testing_package.TB target, out T result)
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
        public static object? _(this go.testing_package.TB target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.testing_package.TB<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.testing_package.TB target, Type type, out object? result)
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