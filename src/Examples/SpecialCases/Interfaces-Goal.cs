using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using MyFloat = System.Double;

public static partial class main_package
{
    public interface Abser
    {
        double Abs();
    }

    public struct Abser<T> : Abser where T : struct
    {
        internal T m_target;
        internal Func<T, double> m_abs;

        public double Abs() => m_abs(m_target);

        internal static Abser<T> s_empty;

        static Abser()
        {
            Type targetType = typeof(T);

            s_empty.m_abs = targetType.GetExtensionDelegate("Abs") as Func<T, double>;

            if (s_empty.m_abs == null)
                throw new NotImplementedException($"{targetType.Name} does not implement Abser.Abs method");
        }
    }

    public static Abser<T> _asAbser<T>(T target) where T : struct
    {
        Abser<T> value = Abser<T>.s_empty;
        value.m_target = target;
        return value;
    }

    public static double Abs(this MyFloat f)
    {
        if (f < 0)
            return -f;

        return f;
    }

    public struct Vertex
    {
        public double X;
        public double Y;
    }

    public static double Abs(this Vertex v)
    {
        return Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    public static void Main()
    {
        Abser a;
        MyFloat f = -Math.Sqrt(2);
        Vertex v = new Vertex { X = 3, Y = 4 };

        a = _asAbser(f);
        a = _asAbser(v);

        Console.WriteLine(a.Abs());
        Console.ReadLine();
    }
}

public static class TypeExtensions
{
    private static readonly Type[] s_types;

    static TypeExtensions()
    {
        List<Type> types = new List<Type>();

        foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            types.AddRange(item.GetTypes());

        s_types = types.ToArray();
    }

    public static IEnumerable<MethodInfo> GetExtensionMethods(this Type targetType)
    {
        return s_types
            .Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false))
            .Where(methodInfo => methodInfo.GetParameters()[0].ParameterType == targetType);
    }

    public static MethodInfo GetExtensionMethod(this Type targetType, string methodName)
    {
        return targetType.GetExtensionMethods().FirstOrDefault(methodInfo => methodInfo.Name == methodName);
    }

    public static Delegate CreateStaticDelegate(this MethodInfo methodInfo)
    {
        Func<Type[], Type> getMethodType;
        List<Type> types = methodInfo.GetParameters().Select(paramInfo => paramInfo.ParameterType).ToList();

        if (methodInfo.ReturnType == typeof(void))
        {
            getMethodType = Expression.GetActionType;
        }
        else
        {
            getMethodType = Expression.GetFuncType;
            types.Add(methodInfo.ReturnType);
        }

        return Delegate.CreateDelegate(getMethodType(types.ToArray()), methodInfo);
    }

    public static Delegate GetExtensionDelegate(this Type targetType, string methodName)
    {
        return targetType.GetExtensionMethod(methodName)?.CreateStaticDelegate();
    }
}