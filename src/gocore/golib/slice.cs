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
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static go.builtin;

#pragma warning disable IDE1006, IDE0032

namespace go
{
    public interface ISlice : IArray
    {
        Array Array { get; }

        nint Low { get; }

        nint High { get; }

        nint Capacity { get; }

        nint Available { get; }

        ISlice? Append(object[] elems);
    }

    [Serializable]
    public readonly struct slice<T> : ISlice, IList<T>, IReadOnlyList<T>, IEnumerable<(nint, T)>, IEquatable<slice<T>>, IEquatable<ISlice>
    {
        private readonly T[] m_array;
        private readonly nint m_low;
        private readonly nint m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(T[] array)
        {
            m_array = array ?? throw new ArgumentNullException(nameof(array), "slice array reference is null.");
            m_low = 0;
            m_length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(array<T> array) : this((T[])array) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(T[] array, nint low = 0, nint high = -1)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array), "slice array reference is null.");

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (high == -1)
                high = array.Length;

            nint length = high - low;

            if (array.Length - low < length)
                throw new ArgumentException($"Indices {nameof(low)} and {nameof(high)} represent a range outside bounds of the array reference.");

            m_array = array;
            m_low = low;
            m_length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(array<T> array, nint low = 0, nint high = -1) : this((T[])array, low, high) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(nint length, nint capacity = -1, nint low = 0)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Value is less than zero.");

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (capacity <= 0)
                capacity = length;

            m_array = new T[capacity];
            m_low = low;
            m_length = length;
        }

        public T[] Array
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_array;
        }

        public nint Low
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_low;
        }

        public nint High
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_low + m_length;
        }

        public nint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_length;
        }

        public nint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_array?.Length - m_low ?? 0;
        }

        public nint Available
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_array?.Length - m_length ?? 0;
        }

        // Returning by-ref value allows slice to be a struct instead of a class and still allow read and write
        // Allows for implicit index support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_array is null)
                    throw new InvalidOperationException("slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ref m_array[m_low + index];
            }
        }

        public ref T this[nint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_array is null)
                    throw new InvalidOperationException("slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

                return ref m_array[m_low + index];
            }
        }

        public ref T this[ulong index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[(nint)index];
        }

        // Allows for implicit range support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-range-support
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Slice(int start, int length) => m_array.slice(start, start + length, Capacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint IndexOf(in T item)
        {
            if (m_array is null)
                throw new InvalidOperationException("slice array reference is null.");

            int index = System.Array.IndexOf(m_array, item, (int)m_low, (int)m_length);

            return index >= 0 ? index - m_low : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            if (m_array is null)
                throw new InvalidOperationException("slice array reference is null.");

            return System.Array.IndexOf(m_array, item, (int)m_low, (int)m_length) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (m_array is null)
                throw new InvalidOperationException("slice array reference is null.");

            System.Array.Copy(m_array, m_low, array, arrayIndex, m_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            T[] array = new T[Length];
            CopyTo(array, 0);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Clone() => Array?.slice() ?? new slice<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<(nint, T)> GetEnumerator()
        {
            SliceEnumerator enumerator = new SliceEnumerator(this);
            nint index = 0;

            while (enumerator.MoveNext())
                yield return (index++, enumerator.Current);
        }

        // Returns an enumerator for just index enumeration
        public IEnumerable<nint> Range
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                for (nint i = Low; i < High; i++)
                    yield return i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_array is null ? 0 : m_array.GetHashCode() ^ (int)m_low ^ (int)m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => Equals(obj as ISlice);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ISlice? other) => other?.Array == m_array && other?.Low == m_low && other.Length == m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(slice<T> other) => other.m_array == m_array && other.m_low == m_low && other.m_length == m_length;

        #region [ Operators ]

        // Enable implicit conversions between slice<T> and T[]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<T>(T[] value) => new slice<T>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T[](slice<T> value) => value.ToArray();

        // Enable implicit conversions between slice<T> and array<T>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<T>(array<T> value) => new slice<T>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator array<T>(slice<T> value) => new array<T>(value.ToArray());

        // slice<T> to slice<T> comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> a, slice<T> b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> a, slice<T> b) => !(a == b);

        // slice<T> to ISlice comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ISlice a, slice<T> b) => a?.Equals(b) ?? false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ISlice a, slice<T> b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> a, ISlice b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> a, ISlice b) => !(a == b);

        // slice<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(slice<T> slice, NilType _) => slice.Length == 0 && slice.Capacity == 0 && slice.Array is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(slice<T> slice, NilType nil) => !(slice == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, slice<T> slice) => slice == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, slice<T> slice) => slice != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<T>(NilType _) => default;

        #endregion

        #region [ Interface Implementations ]

        object ICloneable.Clone() => MemberwiseClone();

        Array ISlice.Array => m_array;

        ISlice? ISlice.Append(object[] elems) => Append(this, elems.Cast<T>().ToArray());

        object? IArray.this[nint index]
        {
            get => this[index];
            set => this[index] = (T)value!;
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set
            {
                if (m_array is null)
                    throw new InvalidOperationException("slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                m_array[m_low + index] = value;
            }
        }

        int IList<T>.IndexOf(T item) => (int)IndexOf(item);

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        int IReadOnlyCollection<T>.Count => (int)m_length;

        T IReadOnlyList<T>.this[int index] => this[m_low + index];

        bool ICollection<T>.IsReadOnly => false;

        int ICollection<T>.Count => (int)m_length;

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        bool ICollection<T>.Contains(T item) => Contains(item);

        void ICollection<T>.Clear() => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SliceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new SliceEnumerator(this);

        [Serializable]
        private sealed class SliceEnumerator : IEnumerator<T>
        {
            private readonly T[] m_array;
            private readonly nint m_start;
            private readonly nint m_end;
            private nint m_current;

            internal SliceEnumerator(slice<T> slice)
            {
                if (slice != nil && slice.m_array is null)
                    throw new InvalidOperationException("slice array reference is null.");

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
                        throw new InvalidOperationException("slice enumeration not started.");

                    if (m_current >= m_end)
                        throw new InvalidOperationException("slice enumeration has ended.");

                    return m_array[m_current];
                }
            }

            object? IEnumerator.Current => Current;

            void IEnumerator.Reset() => m_current = m_start - 1;

            public void Dispose()
            {
            }
        }

        #endregion

        public static slice<T> From<TSource>(TSource[] array)
        {
            if (array is null)
                return new slice<T>(new T[0]);

            if (array is T[] baseTypeArray)
                return new slice<T>(baseTypeArray);

            baseTypeArray = new T[array.Length];

            for (int i = 0; i < array.Length; i++)
                baseTypeArray[i] = (T)ConvertToType((IConvertible)array[i]!);

            return baseTypeArray;
        }

        public static slice<T> Append(in slice<T> slice, params T[] elems)
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

            nint newCapacity = CalculateNewCapacity(slice, slice.Array.Length + elems.Length);
            newArray = new T[newCapacity];

            System.Array.Copy(slice.Array, newArray, slice.Length);
            System.Array.Copy(elems, 0, newArray, slice.Length, elems.Length);

            return new slice<T>(newArray, slice.Low, slice.High + elems.Length);
        }

        private static nint CalculateNewCapacity(in slice<T> slice, int neededCapacity)
        {
            nint capacity = slice.Capacity;

            if (capacity > 1 && capacity % 2 != 0)
                capacity++;

            nint doubleCapacity = capacity + capacity;

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
        // slice of a slice helper function:
        //      s = s[2:]    => s = s.slice(2)
        //      s = s[3:5]   => s = s.slice(3, 5);
        //      s = s[:4]    => s = s.slice(high:4)
        //      s = s[1:3:5] => s = s.slice(1, 3, 5) // Full slice expression
        public static slice<T> slice<T>(this in slice<T> slice, nint low = -1, nint high = -1, nint max = -1)
        {
            return slice.Array.slice(low == -1 ? slice.Low : low, high == -1 ? slice.High : high, max);
        }

        // slice of an array helper function
        public static slice<T> slice<T>(this T[] array, nint low = -1, nint high = -1, nint max = -1)
        {
            if (low == -1)
                low = 0;

            if (high > -1 && max > -1)
            {
                nint length = high - low;
                nint capacity = max - low;

                if (capacity == array.Length)
                    return new slice<T>(array, low, high);

                slice<T> fullSlice = new slice<T>(length, capacity);
                Array.Copy(array, low, fullSlice.Array, 0, length);
                return fullSlice;
            }

            if (high == -1)
                high = array.Length;

            return new slice<T>(array, low, high);
        }

        public static slice<T> slice<T>(this array<T> array, nint low = -1, nint high = -1, nint max = -1) =>
            ((T[])array).slice(low, high, max);

        // slice of a string helper function
        public static slice<byte> slice(this @string source, nint low = -1, nint high = -1, nint max = -1) =>
            ((IReadOnlyList<byte>)source).ToArray().slice(low, high, max);
    }
}
