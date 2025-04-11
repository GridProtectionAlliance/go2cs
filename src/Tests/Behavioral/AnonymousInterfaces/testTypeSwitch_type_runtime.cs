using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using go.runtime;

namespace go;

partial class main_package
{
    internal partial interface testTypeSwitch_type
    {
        public static testTypeSwitch_type As<T>(in T target) =>
            (testTypeSwitch_type<T>)target!;

        public static testTypeSwitch_type As<T>(ж<T> target_ptr) =>
            (testTypeSwitch_type<T>)target_ptr;

        public static testTypeSwitch_type? As(object target) =>
            typeof(testTypeSwitch_type<>).CreateInterfaceHandler<testTypeSwitch_type>(target);
    }

    [GeneratedCode("go2cs", "0.1.0.0")]
    public class testTypeSwitch_type<T> : testTypeSwitch_type
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

        public testTypeSwitch_type(in T target)
        {
            m_target = target;
        }

        public testTypeSwitch_type(ж<T> target_ptr)
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

        static testTypeSwitch_type()
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
                throw new NotImplementedException($"{targetType.FullName} does not implement '{nameof(testTypeSwitch_type)}.{nameof(Unwrap)}' method", new Exception(nameof(Unwrap)));
        }

        public static explicit operator testTypeSwitch_type<T>(in ж<T> target_ptr) => new(target_ptr);

        public static explicit operator testTypeSwitch_type<T>(in T target) => new(target);

        // Enable comparisons between nil and testTypeSwitch_type<T> interface instance
        public static bool operator ==(testTypeSwitch_type<T> value, NilType nil) => Activator.CreateInstance<testTypeSwitch_type<T>>().Equals(value);

        public static bool operator !=(testTypeSwitch_type<T> value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, testTypeSwitch_type<T> value) => value == nil;

        public static bool operator !=(NilType nil, testTypeSwitch_type<T> value) => value != nil;
    }
}
