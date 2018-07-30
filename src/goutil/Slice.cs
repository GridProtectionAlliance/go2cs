//******************************************************************************************************
//  slice.cs - Gbtc
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
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static go.builtin;

namespace go
{
    public interface ISlice : EmptyInterface
    {
        Array Array { get; }

        int Low { get; }

        int High { get; }

        int Length { get; }

        int Capacity { get; }

        int Available { get; }

        object this[int index] { get; set; }
    }

    // Span<T> considered for slices: https://github.com/dotnet/corefxlab/blob/master/docs/specs/span.md#relationship-to-array-slicing

    [Serializable]
    public struct slice<T> : ISlice, IList<T>, IReadOnlyList<T>, IEquatable<slice<T>>, IEquatable<ISlice>
    {
        private readonly T[] m_array;
        private readonly int m_low;
        private readonly int m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(T[] array)
        {
            if ((object)array == null)
                throw new ArgumentNullException(nameof(array), "Slice array reference is null.");

            m_array = array;
            m_low = 0;
            m_length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(T[] array, int low = 0, int high = -1)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(int length, int capacity = -1, int low = 0)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Value is less than zero.");

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (capacity == -1)
                capacity = length;

            m_array = new T[capacity];
            m_low = low;
            m_length = length;
        }

        public T[] Array => m_array;

        public int Low => m_low;

        public int High => m_low + m_length;

        public int Length => m_length;

        public int Capacity => (object)m_array == null ? 0 : m_array.Length - m_low;

        public int Available => (object)m_array == null ? 0 : m_array.Length - m_length;

        // Returning by-ref value allows slice to be a struct instead of a class and still allow read and write
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((object)m_array == null)
                    throw new InvalidOperationException("Slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ref m_array[m_low + index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            int index = System.Array.IndexOf(m_array, item, m_low, m_length);

            return index >= 0 ? index - m_low : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            return System.Array.IndexOf(m_array, item, m_low, m_length) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((object)m_array == null)
                throw new InvalidOperationException("Slice array reference is null.");

            System.Array.Copy(m_array, m_low, array, arrayIndex, m_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            T[] array = new T[Length];
            CopyTo(array, 0);
            return array;
        }

        public override string ToString() => $"[{string.Join(" ", this.Take(5))}{(Length > 5 ? " ..." : "")}]";

        public override int GetHashCode() => (object)m_array == null ? 0 : m_array.GetHashCode() ^ m_low ^ m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => Equals(obj as ISlice);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ISlice other) => other?.Array == m_array && other?.Low == m_low && other.Length == m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(slice<T> other) => other.m_array == m_array && other.m_low == m_low && other.m_length == m_length;

        #region [ Equality Operators ]

        // Slice<T> to Slice<T> comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> a, slice<T> b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> a, slice<T> b) => !(a == b);

        // Slice<T> to ISlice comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ISlice a, slice<T> b) => a?.Equals(b) ?? false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ISlice a, slice<T> b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> a, ISlice b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> a, ISlice b) => !(a == b);

        // Slice<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> slice, NilType nil) => slice.Length == 0 && slice.Capacity == 0 && slice.Array == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> slice, NilType nil) => !(slice == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, slice<T> slice) => slice == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, slice<T> slice) => slice != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<T>(NilType nil) => default;

        #endregion

        #region [ Interface Implementations ]

        Array ISlice.Array => m_array;

        object ISlice.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
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

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        int IReadOnlyCollection<T>.Count => m_length;

        T IReadOnlyList<T>.this[int index] => this[m_low + index];

        bool ICollection<T>.IsReadOnly => false;

        int ICollection<T>.Count => m_length;

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        void ICollection<T>.Clear() => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SliceEnumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator() => new SliceEnumerator(ref this);

        [Serializable]
        private sealed class SliceEnumerator : IEnumerator<T>
        {
            private readonly T[] m_array;
            private readonly int m_start;
            private readonly int m_end;
            private int m_current;

            internal SliceEnumerator(ref slice<T> slice)
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

        public static readonly slice<T> Nil = default;

        public static slice<T> Append(ref slice<T> slice, params object[] elems)
        {
            T[] newArray;

            if (slice == nil)
            {
                newArray = new T[elems.Length];
                System.Array.Copy(elems, newArray, elems.Length);
                return new slice<T>(newArray);
            }

            if (elems.Length <= slice.Available)
            {
                System.Array.Copy(elems, 0, slice.Array, slice.High, elems.Length);
                return slice.slice(high: slice.High + elems.Length);
            }

            int newCapacity = CalculateNewCapacity(ref slice, slice.Array.Length + elems.Length);
            newArray = new T[newCapacity];

            System.Array.Copy(slice.Array, newArray, slice.Length);
            System.Array.Copy(elems, 0, newArray, slice.Length, elems.Length);

            return new slice<T>(newArray, slice.Low, slice.High + elems.Length);
        }

        private static int CalculateNewCapacity(ref slice<T> slice, int neededCapacity)
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
        //      s = s[2:]    => s = s.Slice(2)
        //      s = s[3:5]   => s = s.Slice(3, 5);
        //      s = s[:4]    => s = s.Slice(high:4)
        //      s = s[1:3:5] => s = s.Slice(1, 3, 5) // Full slice expression
        public static slice<T> slice<T>(this ref slice<T> slice, int low = -1, int high = -1, int max = -1)
        {
            return slice.Array.slice(low == -1 ? slice.Low : low, high == -1 ? slice.High : high, max);
        }

        // Slice of an array helper function
        public static slice<T> slice<T>(this T[] array , int low = -1, int high = -1, int max = -1)
        {
            if (low == -1)
                low = 0;

            if (high > -1 && max > -1)
            {
                int length = high - low;
                int capacity = max - low;

                slice<T> fullSlice = new slice<T>(length, capacity, low);
                Array.Copy(array, low, fullSlice.Array, low, length);
                return fullSlice;
            }

            if (high == -1)
                high = array.Length;

            return new slice<T>(array, low, high);
        }

        // Slice of a string helper function
        public static slice<@byte> slice(this @string source, int low = -1, int high = -1, int max = -1)
        {
            return ((IReadOnlyList<@byte>)source).ToArray().slice(low, high, max);
        }
    }
}