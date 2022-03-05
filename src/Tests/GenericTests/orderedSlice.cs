using System;
using System.Collections;
using System.Linq;
using go;

namespace ConstraintTests
{
    public static partial class Program
    {
        // Generic slice type definition (inheriting slice functionality)
        public readonly partial struct orderedSlice<T> : ISlice where T : IComparable<T>
        {
            private readonly slice<T> m_source;

            public orderedSlice(slice<T> source) => m_source = source;

            public T this[nint index]
            {
                get => m_source[index];
                set => m_source[index] = value;
            }

            object IArray.this[nint index]
            {
                get => m_source[index];
                set => m_source[index] = (T)value;
            }

            public IEnumerator GetEnumerator() => m_source.GetEnumerator();

            public object Clone() => m_source.Clone();

            public nint Length => m_source.Length;

            public ISlice Append(object[] elems) => append(m_source, elems.Cast<T>().ToArray());

            public Array Array => m_source.Array;

            public nint Low => m_source.Low;

            public nint High => m_source.High;

            public nint Capacity => m_source.Capacity;

            public nint Available => m_source.Available;
        }
    }
}
