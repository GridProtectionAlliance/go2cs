using System;
using System.Reflection;
using go;

using float64 = System.Double;

#pragma warning disable CS0660, CS0661

public static partial class main_package
{
    public partial interface Abser
    {
        public static Abser As<T>(in T target) => (Abser<T>)target;

        public static Abser As<T>(ptr<T> target_ptr) => (Abser<T>)target_ptr;
    }

    public class Abser<T> : Abser
    {
        private T m_target;
        private readonly ptr<T>? m_target_ptr;

        public ref T Target
        {
            get
            {
                if (m_target_ptr is null)
                    return ref m_target;

                return ref m_target_ptr.Value;
            }
        }

        private Abser(in T target) => m_target = target;
        private Abser(ptr<T> target_ptr) => m_target_ptr = target_ptr;

        private delegate float64 AbsByRef(ref T value);
        private delegate float64 AbsByVal(T value);

        private static readonly AbsByRef? s_AbsByRef;
        private static readonly AbsByVal? s_AbsByVal;

        public float64 Abs()
        {
            if (m_target_ptr is null)
                return s_AbsByRef?.Invoke(ref m_target) ??
                       s_AbsByVal!(m_target);

            return s_AbsByRef?.Invoke(ref m_target_ptr.Value) ??
                       s_AbsByVal!(m_target_ptr.Value);
        }

        static Abser()
        {
            Type targetType = typeof(T);
            Type targetTypeByRef = targetType.MakeByRefType();

            MethodInfo extensionMethod = targetTypeByRef.GetExtensionMethod("Abs");

            if (!(extensionMethod is null))
                s_AbsByRef = extensionMethod.CreateStaticDelegate(typeof(AbsByRef)) as AbsByRef;

            if (s_AbsByRef is null)
            { 
                extensionMethod = targetType.GetExtensionMethod("Abs");
                
                if (!(extensionMethod is null))
                    s_AbsByVal = extensionMethod.CreateStaticDelegate(typeof(AbsByVal)) as AbsByVal;
            }

            if (s_AbsByRef is null && s_AbsByVal is null)
                throw new NotImplementedException($"{targetType.Name} does not implement Abser.Abs method", new Exception("Abs"));
        }

        public static explicit operator Abser<T>(in ptr<T> target_ptr) => new Abser<T>(target_ptr);

        public static explicit operator Abser<T>(in T target) => new Abser<T>(target);
    }
}