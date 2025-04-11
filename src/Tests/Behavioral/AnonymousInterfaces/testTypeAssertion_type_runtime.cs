using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using go.runtime;

namespace go;

partial class main_package
{
    internal partial interface testTypeAssertion_type
    {
        public static testTypeAssertion_type As<T>(in T target) =>
            (testTypeAssertion_type<T>)target!;

        public static testTypeAssertion_type As<T>(ж<T> target_ptr) =>
            (testTypeAssertion_type<T>)target_ptr;

        public static testTypeAssertion_type? As(object target) =>
            typeof(testTypeAssertion_type<>).CreateInterfaceHandler<testTypeAssertion_type>(target);
    }

    [GeneratedCode("go2cs", "0.1.0.0")]
    public class testTypeAssertion_type<T> : testTypeAssertion_type
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

        public testTypeAssertion_type(in T target)
        {
            m_target = target;
        }

        public testTypeAssertion_type(ж<T> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }

        private delegate bool IsByPtr(ж<T> value, error p1);
        private delegate bool IsByVal(T value, error p1);

        private static readonly IsByPtr? s_IsByPtr;
        private static readonly IsByVal? s_IsByVal;

        [DebuggerNonUserCode]
        public bool Is(error p1)
        {
            T target = m_target;

            if (m_target_is_ptr && m_target_ptr is not null)
                target = m_target_ptr.val;

            if (s_IsByPtr is null || !m_target_is_ptr)
                return s_IsByVal!(target, p1);

            return s_IsByPtr(m_target_ptr!, p1);
        }

        public string? ToString(string? format, IFormatProvider? formatProvider) => format;

        static testTypeAssertion_type()
        {
            Type targetType = typeof(T);
            Type targetTypeByPtr = typeof(ж<T>);

            MethodInfo? extensionMethod = targetTypeByPtr.GetExtensionMethod(nameof(Is));

            if (extensionMethod is not null)
                s_IsByPtr = extensionMethod.CreateStaticDelegate(typeof(IsByPtr)) as IsByPtr;

            extensionMethod = targetType.GetExtensionMethod(nameof(Is));

            if (extensionMethod is not null)
                s_IsByVal = extensionMethod.CreateStaticDelegate(typeof(IsByVal)) as IsByVal;

            if (s_IsByPtr is null && s_IsByVal is null)
                throw new NotImplementedException($"{targetType.FullName} does not implement '{nameof(testTypeAssertion_type)}.{nameof(Is)}' method", new Exception(nameof(Is)));
        }

        public static explicit operator testTypeAssertion_type<T>(in ж<T> target_ptr) => new(target_ptr);

        public static explicit operator testTypeAssertion_type<T>(in T target) => new(target);

        // Enable comparisons between nil and testTypeAssertion_type<T> interface instance
        public static bool operator ==(testTypeAssertion_type<T> value, NilType nil) => Activator.CreateInstance<testTypeAssertion_type<T>>().Equals(value);

        public static bool operator !=(testTypeAssertion_type<T> value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, testTypeAssertion_type<T> value) => value == nil;

        public static bool operator !=(NilType nil, testTypeAssertion_type<T> value) => value != nil;
    }
}
