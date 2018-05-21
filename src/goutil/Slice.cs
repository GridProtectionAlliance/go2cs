//******************************************************************************************************
//  Slice.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2018 - J. Ritchie Carroll
//       Generated original version of source code inspired by .NET ArraySegment<T> source:
//          https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/arraysegment.cs
//          Copyright (c) Microsoft Corporation.  All rights reserved.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using static goutil.BuiltInFunctions;

namespace goutil
{
    public interface ISlice
    {
        Array Array { get; }

        int Low { get; }

        int High { get; }

        int Length { get; }

        int Capacity { get; }

        int Available { get; }

        object this[int index] { get; set; }
    }

    [Serializable]
    public class Slice<T> : ISlice, IList<T>, IReadOnlyList<T>, IEquatable<Slice<T>>
    {
        private readonly T[] m_array;
        private readonly int m_low;
        private readonly int m_length;

        public Slice()
        {
        }

        public Slice(T[] array)
        {
            if ((object)array == null)
                throw new ArgumentNullException(nameof(array), "Slice array reference is null.");

            m_array = array;
            m_low = 0;
            m_length = array.Length;
        }

        public Slice(T[] array, int low = 0, int high = -1)
        {
            if ((object)array == null)
                throw new ArgumentNullException(nameof(array), "Slice array reference is null.");

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (high == -1)
                high = array.Length;

            int length = high - low;

            if (array.Length - low < length)
                throw new ArgumentException($"Indices {nameof(low)} and {nameof(high)} represent a range outside bounds of the array reference.");

            m_array = array;
            m_low = low;
            m_length = length;
        }

        public Slice(int length, int capacity = -1)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Value is less than zero.");

            if (capacity == -1)
                capacity = length;

            m_array = new T[capacity];
            m_low = 0;
            m_length = length;
        }

        public T[] Array => m_array;

        public int Low => m_low;

        public int High => m_low + m_length;

        public int Length => m_length;

        public int Capacity => (object)m_array == null ? 0 : m_array.Length - m_low;

        public int Available => (object)m_array == null ? 0 : m_array.Length - m_length;

        public T this[int index]
        {
            get
            {
                if ((object)m_array == null)
                    throw new InvalidOperationException("Slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return m_array[m_low + index];
            }

            set
            {
                if ((object)m_array == null)
                    throw new InvalidOperationException("Slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                m_array[m_low + index] = value;
            }
        }

        public int IndexOf(T item)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            int index = System.Array.IndexOf(m_array, item, m_low, m_length);

            return index >= 0 ? index - m_low : -1;
        }

        public bool Contains(T item)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            int index = System.Array.IndexOf(m_array, item, m_low, m_length);

            return index >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            System.Array.Copy(m_array, m_low, array, arrayIndex, m_length);
        }

        public override int GetHashCode() => (object)m_array == null ? 0 : m_array.GetHashCode() ^ m_low ^ m_length;

        public override bool Equals(object obj) => Equals(obj as Slice<T>);

        public bool Equals(Slice<T> obj) => (object)obj != null && obj.m_array == m_array && obj.m_low == m_low && obj.m_length == m_length;

        public static bool operator ==(Slice<T> a, Slice<T> b) => ((object)a == null && (object)b == null) || (a?.Equals(b) ?? false);

        public static bool operator !=(Slice<T> a, Slice<T> b) => !(a == b);

        public override string ToString() => $"[{string.Join(" ", this)}]";

        #region [ Interface Implementations ]

        Array ISlice.Array => m_array;

        object ISlice.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        int IReadOnlyCollection<T>.Count => m_length;

        T IReadOnlyList<T>.this[int index] => this[m_low + index];

        bool ICollection<T>.IsReadOnly => false;

        int ICollection<T>.Count => m_length;

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        void ICollection<T>.Clear() => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SliceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new SliceEnumerator(this);

        [Serializable]
        private sealed class SliceEnumerator : IEnumerator<T>
        {
            private readonly T[] m_array;
            private readonly int m_start;
            private readonly int m_end;
            private int m_current;

            internal SliceEnumerator(Slice<T> slice)
            {
                if (slice != nil && (object)slice.m_array == null)
                    throw new InvalidOperationException("Slice array reference is null.");

                m_array = slice.m_array;
                m_start = slice.m_low;
                m_end = m_start + slice.m_length;
                m_current = m_start - 1;
            }

            public bool MoveNext()
            {
                if (m_current >= m_end)
                    return false;

                m_current++;
                return m_current < m_end;
            }

            public T Current
            {
                get
                {
                    if (m_current < m_start)
                        throw new InvalidOperationException("Slice enumeration not started.");

                    if (m_current >= m_end)
                        throw new InvalidOperationException("Slice enumeration has ended.");

                    return m_array[m_current];
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset() => m_current = m_start - 1;

            public void Dispose()
            {
            }
        }

        #endregion

        public static readonly Slice<T> Nil = null;

        public static Slice<T> Append(Slice<T> slice, params object[] elems)
        {
            T[] newArray;

            if (slice == nil)
            {
                newArray = new T[elems.Length];
                System.Array.Copy(elems, newArray, elems.Length);
                return new Slice<T>(newArray);
            }

            if (elems.Length <= slice.Available)
            {
                System.Array.Copy(elems, 0, slice.Array, slice.High, elems.Length);
                return slice.Slice(high: slice.High + elems.Length);
            }

            int newCapacity = CalculateNewCapacity(slice, slice.Array.Length + elems.Length);
            newArray = new T[newCapacity];

            System.Array.Copy(slice.Array, newArray, slice.Length);
            System.Array.Copy(elems, 0, newArray, slice.Length, elems.Length);

            return new Slice<T>(newArray, slice.Low, slice.High + elems.Length);
        }

        private static int CalculateNewCapacity(Slice<T> slice, int neededCapacity)
        {
            int capacity = slice.Capacity;

            if (capacity % 2 != 0)
                capacity++;

            int doubleCapacity = capacity + capacity;

            if (neededCapacity > doubleCapacity)
            {
                if (neededCapacity % 2 != 0)
                    neededCapacity++;

                capacity = neededCapacity;
            }
            else
            {
                if (slice.Length < 1024)
                {
                    capacity = doubleCapacity;
                }
                else
                {
                    while (capacity < neededCapacity)
                        capacity += capacity / 4;
                }
            }

            return capacity;
        }
    }

    public static class SliceExtensions
    {
        // Slice of a slice helper function:
        //      s = s[2:]  => s = s.Slice(2)
        //      s = s[3:5] => s = s.Slice(3, 5);
        //      s = s[:4]  => s = s.Slice(high:4)
        public static Slice<T> Slice<T>(this Slice<T> slice, int low = -1, int high = -1)
        {
            if (low == -1)
                low = slice.Low;

            if (high == -1)
                high = slice.High;

            return new Slice<T>(slice.Array, low, high);
        }
    }
}