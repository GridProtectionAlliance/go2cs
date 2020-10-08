using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go;

#pragma warning disable CS0660, CS0661

public static partial class main_package
{
    public partial interface I : IFormattable
    {
        [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static I As<T>(in T target) => (I<T>)target;

        [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static I As<T>(ptr<T> target_ptr) => (I<T>)target_ptr;
    }

    [GeneratedCode("go2cs", "0.1.1.0")]
    public class I<T> : I
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

        private I(in T target) => m_target = target;
        
        private I(ptr<T> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }

        private delegate void MByRef(ref T value);
        private delegate void MByVal(T value);

        private static readonly MByRef? s_MByRef;
        private static readonly MByVal? s_MByVal;

        [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void M()
        {
            T target = m_target;

            if (m_target_is_ptr && !(m_target_ptr is null))
                target = m_target_ptr.val;

            if (s_MByRef is null)
                s_MByVal!(target);
            else
                s_MByRef(ref target);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            switch (format)
            {
                case "T":
                {
                    string typeName = typeof(T).FullName!.Replace("_package+", ".");
                    return m_target_is_ptr ? $"*{typeName}" : typeName;
                }
                case "v":
                {
                    if (m_target_is_ptr)
                        return m_target_ptr is null ? "<nil>" : $"&{m_target_ptr.val}";

                    return m_target?.ToString() ?? "<nil>";
                }
                default:
                    return ToString() ?? "<nil>";
            }
        }

        [DebuggerStepperBoundary]
        static I()
        {
            Type targetType = typeof(T);
            Type targetTypeByRef = targetType.MakeByRefType();

            MethodInfo extensionMethod = targetTypeByRef.GetExtensionMethod("M");

            if (!(extensionMethod is null))
                s_MByRef = extensionMethod.CreateStaticDelegate(typeof(MByRef)) as MByRef;

            if (s_MByRef is null)
            { 
                extensionMethod = targetType.GetExtensionMethod("M");
                
                if (!(extensionMethod is null))
                    s_MByVal = extensionMethod.CreateStaticDelegate(typeof(MByVal)) as MByVal;
            }

            if (s_MByRef is null && s_MByVal is null)
                throw new NotImplementedException($"{targetType.Name} does not implement I.M method", new Exception("M"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator I<T>(in ptr<T> target_ptr) => new I<T>(target_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static explicit operator I<T>(in T target) => new I<T>(target);

        // Enable comparisons between nil and I<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(I<T> value, NilType nil) => Activator.CreateInstance<I<T>>().Equals(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(I<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, I<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, I<T> value) => value != nil;
    }
}

public static class main_IExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static ref T TypeAssert<T>(this main_package.I target)
    {
        try
        {
            return ref ((main_package.I<T>)target).Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"panic: interface conversion: {target.GetType().FullName} is not {typeof(T).FullName}: missing method {ex.InnerException?.Message}");
        }
    }

    [GeneratedCode("go2cs", "0.1.1.0"), MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
    public static bool TryTypeAssert<T>(this main_package.I target, out T result)
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
    public static object? TypeAssert(this main_package.I target, Type type)
    {
        try
        {
            MethodInfo conversionOperator = s_conversionOperators.GetOrAdd(type, _ => 
                typeof(main_package.I<>).GetExplicitGenericConversionOperator(type));

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
    public static bool TryTypeAssert(this main_package.I target, Type type, out object? result)
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
