using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.runtime;

namespace go;

partial class errors_package
{
    internal partial interface @is_typeᴛ1
    {
        public static @is_typeᴛ1 As<T>(in T target) =>
            (@is_typeᴛ1<T>)target!;

        public static @is_typeᴛ1 As<T>(ж<T> target_ptr) =>
            (@is_typeᴛ1<T>)target_ptr;

        public static @is_typeᴛ1? As(object target) =>
            typeof(@is_typeᴛ1<>).CreateInterfaceHandler<@is_typeᴛ1>(target);
    }

    [GeneratedCode("go2cs", "0.1.0.0")]
    public class @is_typeᴛ1<T> : @is_typeᴛ1
    {
        private T m_target = default!;
        private readonly ж<T>? m_target_ptr;
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

        public @is_typeᴛ1(in T target) => m_target = target;

        public @is_typeᴛ1(ж<T> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }

        private delegate error UnwrapByPtr(ж<T> value);
        private delegate error UnwrapByVal(T value);

        private static readonly UnwrapByPtr? s_UnwrapByPtr;
        private static readonly UnwrapByVal? s_UnwrapByVal;

        [DebuggerNonUserCode]
        public error Unwrap()
        {
            T target = m_target;

            if (m_target_is_ptr && m_target_ptr is not null)
                target = m_target_ptr.val;

            if (s_UnwrapByPtr is null || !m_target_is_ptr)
                return s_UnwrapByVal!(target);

            return s_UnwrapByPtr(m_target_ptr!);
        }

        public string? ToString(string? format, IFormatProvider? formatProvider) => format;

        static @is_typeᴛ1()
        {
            Type targetType = typeof(T);
            Type targetTypeByPtr = typeof(ж<T>);

            MethodInfo? extensionMethod = targetTypeByPtr.GetExtensionMethod(nameof(Unwrap));

            if (extensionMethod is not null)
                s_UnwrapByPtr = extensionMethod.CreateStaticDelegate(typeof(UnwrapByPtr)) as UnwrapByPtr;

            extensionMethod = targetType.GetExtensionMethod(nameof(Unwrap));

            if (extensionMethod is not null)
                s_UnwrapByVal = extensionMethod.CreateStaticDelegate(typeof(UnwrapByVal)) as UnwrapByVal;

            if (s_UnwrapByPtr is null && s_UnwrapByVal is null)
                throw new NotImplementedException($"{targetType.FullName} does not implement @is_typeᴛ1.Is method", new Exception(nameof(Unwrap)));
        }

        public static explicit operator @is_typeᴛ1<T>(in ж<T> target_ptr) => new(target_ptr);

        public static explicit operator @is_typeᴛ1<T>(in T target) => new(target);

        // Enable comparisons between nil and @is_typeᴛ1<T> interface instance
        public static bool operator ==(@is_typeᴛ1<T> value, NilType nil) => Activator.CreateInstance<@is_typeᴛ1<T>>().Equals(value);

        public static bool operator !=(@is_typeᴛ1<T> value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, @is_typeᴛ1<T> value) => value == nil;

        public static bool operator !=(NilType nil, @is_typeᴛ1<T> value) => value != nil;
    }
}
