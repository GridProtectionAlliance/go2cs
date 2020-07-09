using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go;

using float64 = System.Double;

#pragma warning disable CS0660, CS0661

public static partial class main_package
{
    public partial interface Abser
    {
        [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static Abser As<T>(in T target) => (Abser<T>)target;

        [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static Abser As<T>(ptr<T> target_ptr) => (Abser<T>)target_ptr;
    }

    [GeneratedCode("go2cs", "0.1.1.0")]
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

        [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float64 Abs()
        {
            if (m_target_ptr is null)
                return s_AbsByRef?.Invoke(ref m_target) ??
                       s_AbsByVal!(m_target);

            return s_AbsByRef?.Invoke(ref m_target_ptr.Value) ??
                       s_AbsByVal!(m_target_ptr.Value);
        }

        [DebuggerStepperBoundary]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator Abser<T>(in ptr<T> target_ptr) => new Abser<T>(target_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator Abser<T>(in T target) => new Abser<T>(target);

        // Enable comparisons between nil and Abser<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Abser<T> value, NilType nil) => Activator.CreateInstance<Abser<T>>().Equals(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Abser<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, Abser<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, Abser<T> value) => value != nil;
    }
}

public static class main_AbserExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static ref T TypeAssert<T>(this main_package.Abser target)
    {
        try
        {
            return ref ((main_package.Abser<T>)target).Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"panic: interface conversion: {target.GetType().FullName} is not {typeof(T).FullName}: missing method {ex.InnerException?.Message}");
        }
    }

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static bool TryTypeAssert<T>(this main_package.Abser target, out T result)
    {
        try
        {
            result = target.TypeAssert<T>();
            return true;
        }
        catch (PanicException)
        {
            result = default!;
            return false;
        }
    }

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static object? TypeAssert(this main_package.Abser target, Type type)
    {
        try
        {
            MethodInfo conversionOperator = s_conversionOperators.GetOrAdd(type, _ => 
                typeof(main_package.Abser<>).GetExplicitGenericConversionOperator(type));

            if (conversionOperator is null)
                throw new PanicException($"panic: interface conversion: failed to create converter for {target.GetType().FullName} to {type.FullName}");

            dynamic? result = conversionOperator.Invoke(null, new object[] { target });
            return result?.Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"panic: interface conversion: {target.GetType().FullName} is not {type.FullName}: missing method {ex.InnerException?.Message}");
        }
    }

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static bool TryTypeAssert(this main_package.Abser target, Type type, out object? result)
    {
        try
        {
            result = target.TypeAssert(type);
            return true;
        }
        catch (PanicException)
        {
            result = type.IsValueType ? Activator.CreateInstance(type) : null;
            return false;
        }
    }
}