// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpers
{
    public static T[] GetSubArray<T>(T[] array, Range range)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        (int offset, int length) = range.GetOffsetAndLength(array.Length);

        if (default(T) != null || typeof(T[]) == array.GetType())
        {
            if (length == 0)
                return [];

            T[] dest = new T[length];
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
        else
        {
            T[] dest = (T[])Array.CreateInstance(array.GetType().GetElementType()!, length);
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}