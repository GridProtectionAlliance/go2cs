//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:31:51 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using fs = go.io.fs_package;
using os = go.os_package;
using pathpkg = go.path_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using cfg = go.cmd.go.@internal.cfg_package;
using imports = go.cmd.go.@internal.imports_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using trace = go.cmd.go.@internal.trace_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial interface versionRepo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static versionRepo As<T>(in T target) => (versionRepo<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static versionRepo As<T>(ptr<T> target_ptr) => (versionRepo<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static versionRepo? As(object target) =>
                typeof(versionRepo<>).CreateInterfaceHandler<versionRepo>(target);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private class versionRepo<T> : versionRepo
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

            public versionRepo(in T target) => m_target = target;

            public versionRepo(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate (ptr<modfetch.RevInfo>, error) ModulePathByPtr(ptr<T> value);
            private delegate (ptr<modfetch.RevInfo>, error) ModulePathByVal(T value);

            private static readonly ModulePathByPtr? s_ModulePathByPtr;
            private static readonly ModulePathByVal? s_ModulePathByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (ptr<modfetch.RevInfo>, error) ModulePath()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_ModulePathByPtr is null || !m_target_is_ptr)
                    return s_ModulePathByVal!(target);

                return s_ModulePathByPtr(m_target_ptr!);
            }

            private delegate (ptr<modfetch.RevInfo>, error) VersionsByPtr(ptr<T> value, @string prefix);
            private delegate (ptr<modfetch.RevInfo>, error) VersionsByVal(T value, @string prefix);

            private static readonly VersionsByPtr? s_VersionsByPtr;
            private static readonly VersionsByVal? s_VersionsByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (ptr<modfetch.RevInfo>, error) Versions(@string prefix)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_VersionsByPtr is null || !m_target_is_ptr)
                    return s_VersionsByVal!(target, prefix);

                return s_VersionsByPtr(m_target_ptr!, prefix);
            }

            private delegate (ptr<modfetch.RevInfo>, error) StatByPtr(ptr<T> value, @string rev);
            private delegate (ptr<modfetch.RevInfo>, error) StatByVal(T value, @string rev);

            private static readonly StatByPtr? s_StatByPtr;
            private static readonly StatByVal? s_StatByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (ptr<modfetch.RevInfo>, error) Stat(@string rev)
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_StatByPtr is null || !m_target_is_ptr)
                    return s_StatByVal!(target, rev);

                return s_StatByPtr(m_target_ptr!, rev);
            }

            private delegate (ptr<modfetch.RevInfo>, error) LatestByPtr(ptr<T> value);
            private delegate (ptr<modfetch.RevInfo>, error) LatestByVal(T value);

            private static readonly LatestByPtr? s_LatestByPtr;
            private static readonly LatestByVal? s_LatestByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (ptr<modfetch.RevInfo>, error) Latest()
            {
                T target = m_target;

                if (m_target_is_ptr && m_target_ptr is not null)
                    target = m_target_ptr.val;

                if (s_LatestByPtr is null || !m_target_is_ptr)
                    return s_LatestByVal!(target);

                return s_LatestByPtr(m_target_ptr!);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format ?? GetGoTypeName(typeof(T));

            [DebuggerStepperBoundary]
            static versionRepo()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("ModulePath");

                if (extensionMethod is not null)
                    s_ModulePathByPtr = extensionMethod.CreateStaticDelegate(typeof(ModulePathByPtr)) as ModulePathByPtr;

                extensionMethod = targetType.GetExtensionMethod("ModulePath");

                if (extensionMethod is not null)
                    s_ModulePathByVal = extensionMethod.CreateStaticDelegate(typeof(ModulePathByVal)) as ModulePathByVal;

                if (s_ModulePathByPtr is null && s_ModulePathByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement versionRepo.ModulePath method", new Exception("ModulePath"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Versions");

                if (extensionMethod is not null)
                    s_VersionsByPtr = extensionMethod.CreateStaticDelegate(typeof(VersionsByPtr)) as VersionsByPtr;

                extensionMethod = targetType.GetExtensionMethod("Versions");

                if (extensionMethod is not null)
                    s_VersionsByVal = extensionMethod.CreateStaticDelegate(typeof(VersionsByVal)) as VersionsByVal;

                if (s_VersionsByPtr is null && s_VersionsByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement versionRepo.Versions method", new Exception("Versions"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Stat");

                if (extensionMethod is not null)
                    s_StatByPtr = extensionMethod.CreateStaticDelegate(typeof(StatByPtr)) as StatByPtr;

                extensionMethod = targetType.GetExtensionMethod("Stat");

                if (extensionMethod is not null)
                    s_StatByVal = extensionMethod.CreateStaticDelegate(typeof(StatByVal)) as StatByVal;

                if (s_StatByPtr is null && s_StatByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement versionRepo.Stat method", new Exception("Stat"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Latest");

                if (extensionMethod is not null)
                    s_LatestByPtr = extensionMethod.CreateStaticDelegate(typeof(LatestByPtr)) as LatestByPtr;

                extensionMethod = targetType.GetExtensionMethod("Latest");

                if (extensionMethod is not null)
                    s_LatestByVal = extensionMethod.CreateStaticDelegate(typeof(LatestByVal)) as LatestByVal;

                if (s_LatestByPtr is null && s_LatestByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement versionRepo.Latest method", new Exception("Latest"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator versionRepo<T>(in ptr<T> target_ptr) => new versionRepo<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator versionRepo<T>(in T target) => new versionRepo<T>(target);

            // Enable comparisons between nil and versionRepo<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(versionRepo<T> value, NilType nil) => Activator.CreateInstance<versionRepo<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(versionRepo<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, versionRepo<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, versionRepo<T> value) => value != nil;
        }
    }
}}}}

namespace go
{
    public static class modload_versionRepoExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.cmd.go.@internal.modload_package.versionRepo target)
        {
            try
            {
                return ((go.cmd.go.@internal.modload_package.versionRepo<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.2.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.cmd.go.@internal.modload_package.versionRepo target, out T result)
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
        public static object? _(this go.cmd.go.@internal.modload_package.versionRepo target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.cmd.go.@internal.modload_package.versionRepo<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.cmd.go.@internal.modload_package.versionRepo target, Type type, out object? result)
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