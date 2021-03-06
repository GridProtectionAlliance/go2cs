//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:56:05 UTC
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
using errors = go.errors_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using textproto = go.net.textproto_package;
using os = go.os_package;
using go;

#nullable enable
#pragma warning disable CS0660, CS0661

namespace go {
namespace mime
{
    public static partial class multipart_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial interface File
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static File As<T>(in T target) => (File<T>)target!;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static File As<T>(ptr<T> target_ptr) => (File<T>)target_ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static File? As(object target) =>
                typeof(File<>).CreateInterfaceHandler<File>(target);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public class File<T> : File
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

            public File(in T target) => m_target = target;

            public File(ptr<T> target_ptr)
            {
                m_target_ptr = target_ptr;
                m_target_is_ptr = true;
            }

            private delegate (long, error) ReadByPtr(ptr<T> value, slice<byte> p);
            private delegate (long, error) ReadByVal(T value, slice<byte> p);

            private static readonly ReadByPtr? s_ReadByPtr;
            private static readonly ReadByVal? s_ReadByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (long, error) Read(slice<byte> p)
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_ReadByPtr is null || !m_target_is_ptr)
                    return s_ReadByVal!(target, p);

                return s_ReadByPtr(m_target_ptr, p);
            }

            private delegate (long, error) ReadAtByPtr(ptr<T> value, slice<byte> p, long off);
            private delegate (long, error) ReadAtByVal(T value, slice<byte> p, long off);

            private static readonly ReadAtByPtr? s_ReadAtByPtr;
            private static readonly ReadAtByVal? s_ReadAtByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (long, error) ReadAt(slice<byte> p, long off)
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_ReadAtByPtr is null || !m_target_is_ptr)
                    return s_ReadAtByVal!(target, p, off);

                return s_ReadAtByPtr(m_target_ptr, p, off);
            }

            private delegate (long, error) SeekByPtr(ptr<T> value, long offset, long whence);
            private delegate (long, error) SeekByVal(T value, long offset, long whence);

            private static readonly SeekByPtr? s_SeekByPtr;
            private static readonly SeekByVal? s_SeekByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (long, error) Seek(long offset, long whence)
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_SeekByPtr is null || !m_target_is_ptr)
                    return s_SeekByVal!(target, offset, whence);

                return s_SeekByPtr(m_target_ptr, offset, whence);
            }

            private delegate error CloseByPtr(ptr<T> value);
            private delegate error CloseByVal(T value);

            private static readonly CloseByPtr? s_CloseByPtr;
            private static readonly CloseByVal? s_CloseByVal;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public error Close()
            {
                T target = m_target;

                if (m_target_is_ptr && !(m_target_ptr is null))
                    target = m_target_ptr.val;

                if (s_CloseByPtr is null || !m_target_is_ptr)
                    return s_CloseByVal!(target);

                return s_CloseByPtr(m_target_ptr);
            }
            
            public string ToString(string? format, IFormatProvider? formatProvider) => format;

            [DebuggerStepperBoundary]
            static File()
            {
                Type targetType = typeof(T);
                Type targetTypeByPtr = typeof(ptr<T>);
                MethodInfo extensionMethod;

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Read");

                if (!(extensionMethod is null))
                    s_ReadByPtr = extensionMethod.CreateStaticDelegate(typeof(ReadByPtr)) as ReadByPtr;

                extensionMethod = targetType.GetExtensionMethod("Read");

                if (!(extensionMethod is null))
                    s_ReadByVal = extensionMethod.CreateStaticDelegate(typeof(ReadByVal)) as ReadByVal;

                if (s_ReadByPtr is null && s_ReadByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement File.Read method", new Exception("Read"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("ReadAt");

                if (!(extensionMethod is null))
                    s_ReadAtByPtr = extensionMethod.CreateStaticDelegate(typeof(ReadAtByPtr)) as ReadAtByPtr;

                extensionMethod = targetType.GetExtensionMethod("ReadAt");

                if (!(extensionMethod is null))
                    s_ReadAtByVal = extensionMethod.CreateStaticDelegate(typeof(ReadAtByVal)) as ReadAtByVal;

                if (s_ReadAtByPtr is null && s_ReadAtByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement File.ReadAt method", new Exception("ReadAt"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Seek");

                if (!(extensionMethod is null))
                    s_SeekByPtr = extensionMethod.CreateStaticDelegate(typeof(SeekByPtr)) as SeekByPtr;

                extensionMethod = targetType.GetExtensionMethod("Seek");

                if (!(extensionMethod is null))
                    s_SeekByVal = extensionMethod.CreateStaticDelegate(typeof(SeekByVal)) as SeekByVal;

                if (s_SeekByPtr is null && s_SeekByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement File.Seek method", new Exception("Seek"));

               extensionMethod = targetTypeByPtr.GetExtensionMethod("Close");

                if (!(extensionMethod is null))
                    s_CloseByPtr = extensionMethod.CreateStaticDelegate(typeof(CloseByPtr)) as CloseByPtr;

                extensionMethod = targetType.GetExtensionMethod("Close");

                if (!(extensionMethod is null))
                    s_CloseByVal = extensionMethod.CreateStaticDelegate(typeof(CloseByVal)) as CloseByVal;

                if (s_CloseByPtr is null && s_CloseByVal is null)
                    throw new NotImplementedException($"{targetType.FullName} does not implement File.Close method", new Exception("Close"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator File<T>(in ptr<T> target_ptr) => new File<T>(target_ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator File<T>(in T target) => new File<T>(target);

            // Enable comparisons between nil and File<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(File<T> value, NilType nil) => Activator.CreateInstance<File<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(File<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, File<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, File<T> value) => value != nil;
        }
    }
}}

namespace go
{
    public static class multipart_FileExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T _<T>(this go.mime.multipart_package.File target)
        {
            try
            {
                return ((go.mime.multipart_package.File<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}");
            }
        }

        [GeneratedCode("go2cs", "0.1.0.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool _<T>(this go.mime.multipart_package.File target, out T result)
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
        public static object? _(this go.mime.multipart_package.File target, Type type)
        {
            try
            {
                MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(go.mime.multipart_package.File<>).GetExplicitGenericConversionOperator(type));

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
        public static bool _(this go.mime.multipart_package.File target, Type type, out object? result)
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