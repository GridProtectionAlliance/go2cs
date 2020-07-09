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
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UnusedParameter.Local

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static go.builtin;

#pragma warning disable IDE0060, IDE1006, IDE0032

namespace go.experimental
{
    public interface ISlice : IArray
    {
        Array Array { get; }

        int Low { get; }

        int High { get; }

        int Capacity { get; }

        int Available { get; }
    }

    // Span<T> considered for slices: https://github.com/dotnet/corefxlab/blob/master/docs/specs/span.md#relationship-to-array-slicing

    [Serializable]
    public readonly unsafe struct slice<T> : ISlice, IList<T>, IReadOnlyList<T>, IEnumerable<(long, T)> where T : unmanaged
    {
        private readonly GCHandle m_handle;
        private readonly T* m_value;
        private readonly int m_capacity;
        private readonly int m_low;
        private readonly int m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(T[] array)
        {
            m_handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            m_value = (T*)m_handle.AddrOfPinnedObject();
            m_capacity = array.Length;
            m_low = 0;
            m_length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice(array<T> array, int low = 0, int high = -1) :
            this(array.Handle, array.Length) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal slice(GCHandle handle, int capacity, int low = 0, int high = -1)
        {
            m_handle = handle;
            m_value = (T*)handle.AddrOfPinnedObject();

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (high == -1)
                high = capacity;

            int length = high - low;

            if (capacity - low < length)
                throw new ArgumentException($"Indices {nameof(low)} and {nameof(high)} represent a range outside bounds of the array reference.");

            m_capacity = capacity;
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

            if (capacity <= 0)
                capacity = length;

            m_handle = GCHandle.Alloc(new T[capacity], GCHandleType.Pinned);
            m_value = (T*)m_handle.AddrOfPinnedObject();
            m_capacity = capacity;
            m_low = low;
            m_length = length;
        }

        internal GCHandle Handle => m_handle;

        internal T* Value => m_value;

        public int Low
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_low;
        }

        public int High
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_low + m_length;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_length;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_capacity - m_low;
        }

        public int Available
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_capacity - m_length;
        }

        // Returning by-ref value allows slice to be a struct instead of a class and still allow read and write
        // Allows for implicit index support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value == null)
                    throw new InvalidOperationException("slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

                return ref m_value[m_low + index];
            }
        }

        public ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value == null)
                    throw new InvalidOperationException("slice array reference is null.");

                if (index < 0 || index >= m_length)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

                return ref m_value[m_low + index];
            }
        }

        public ref T this[ulong index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[(long)index];
        }

        // Allows for implicit range support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-range-support
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Slice(int start, int length)
        {
            slice<T> target = this;
            return target.slice(start, start + length, Capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Slice(long start, long length)
        {
            slice<T> target = this;
            return target.slice((int)start, (int)(start + length), Capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            if (m_value == null)
                return -1;

            for (int i = m_low; i < m_length; i++)
            {
                if (m_value[i].Equals(item))
                    return i;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) =>
            m_value != null && IndexOf(item) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (m_value == null)
                return;

            CopyTo(0, array, arrayIndex, m_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            T[] array = new T[m_length];
            CopyTo(array, 0);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Clone() => new slice<T>(m_handle, m_capacity, m_low, High);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<(long, T)> GetEnumerator()
        {
            SliceEnumerator enumerator = new SliceEnumerator(this);
            int index = 0;

            while (enumerator.MoveNext())
                yield return (index++, enumerator.Current);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_value == null ? 0 : m_handle.GetHashCode() ^ m_low ^ m_length;

        internal void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int length)
        {
            if (m_value == null)
                throw new InvalidOperationException("slice array reference is null.");

            for (int i = 0; i < m_length; i++)
                destinationArray[destinationIndex + i] = m_value[sourceIndex + i];
        }

        internal void CopyTo(int sourceIndex, void* destinationArray, int destinationIndex, int length)
        {
            if (m_value == null)
                throw new InvalidOperationException("slice array reference is null.");

            for (int i = 0; i < m_length; i++)
                ((T*)destinationArray)[destinationIndex + i] = m_value[sourceIndex + i];
        }

        internal void CopyFrom(void* sourceArray, int sourceIndex, int destinationIndex, int length)
        {
            if (m_value == null)
                throw new InvalidOperationException("slice array reference is null.");

            for (int i = 0; i < m_length; i++)
                m_value[destinationIndex + i] = ((T*)sourceArray)[sourceIndex + i];
        }

        internal void CopyFrom(T[] sourceArray, int sourceIndex, int destinationIndex, int length)
        {
            if (m_value == null)
                throw new InvalidOperationException("slice array reference is null.");

            for (int i = 0; i < m_length; i++)
                m_value[destinationIndex + i] = sourceArray[sourceIndex + i];
        }

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
        public static bool operator ==(slice<T> slice, NilType _) => slice.Length == 0 && slice.Capacity == 0 &&  slice.m_value == null;

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

        Array ISlice.Array => ToArray();

        object IArray.this[int index]
        {
            get => this[index]!;
            set => this[index] = (T)value;
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => this[index] = value;
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

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this == nil ? Enumerable.Empty<T>().GetEnumerator() : new SliceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this == nil ? Enumerable.Empty<T>().GetEnumerator() : new SliceEnumerator(this);

        [Serializable]
        private sealed class SliceEnumerator : IEnumerator<T>
        {
            private readonly T* m_value;
            private readonly int m_start;
            private readonly int m_end;
            private int m_current;

            internal SliceEnumerator(slice<T> slice)
            {
                m_value = slice.m_value;
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

                    return m_value[m_current];
                }
            }

            object IEnumerator.Current => Current!;

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
                baseTypeArray[i] = (T)ConvertToType(array[i] as IConvertible);

            return baseTypeArray;
        }

        public static slice<T> Append(ref slice<T> slice, params T[] elems)
        {
            T[] newArray;

            if (slice == nil)
            {
                newArray = new T[elems.Length];
                Array.Copy(elems, newArray, elems.Length);
                return new slice<T>(newArray);
            }

            if (elems.Length <= slice.Available)
            {
                slice.CopyFrom(elems, 0, slice.High, elems.Length);
                return slice.slice(high: slice.High + elems.Length);
            }

            int newCapacity = CalculateNewCapacity(ref slice, slice.m_capacity + elems.Length);
            newArray = new T[newCapacity];

            slice.CopyTo(0, newArray, 0, slice.Length);
            slice.CopyFrom(elems, 0, slice.Length, elems.Length);

            return new slice<T>(newArray, slice.Low, slice.High + elems.Length);
        }

        private static int CalculateNewCapacity(ref slice<T> slice, int neededCapacity)
        {
            int capacity = slice.Capacity;

            if (capacity > 1 && capacity % 2 != 0)
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
        // slice of a slice helper function:
        //      s = s[2:]    => s = s.slice(2)
        //      s = s[3:5]   => s = s.slice(3, 5);
        //      s = s[:4]    => s = s.slice(high:4)
        //      s = s[1:3:5] => s = s.slice(1, 3, 5) // Full slice expression
        public static slice<T> slice<T>(this in slice<T> source, int low = -1, int high = -1, int max = -1) where T : unmanaged =>
            slice(source.Handle, source.Length, source, low, high, max);

        // slice of an array helper function
        public static slice<T> slice<T>(this in array<T> source, int low = -1, int high = -1, int max = -1) where T : unmanaged =>
            slice(source.Handle, source.Length, source, low, high, max);

        public static slice<T> slice<T>(this T[] source, int low = -1, int high = -1, int max = -1) where T : unmanaged =>
            (new array<T>(source)).slice(low, high, max);

        // slice of a string helper function
        public static slice<byte> slice(this in @string source, int low = -1, int high = -1, int max = -1) =>
            slice(source.Handle, source.Length, source, low, high, max);

        private static slice<T> slice<T>(GCHandle handle, int sourceLength, IReadOnlyList<T> source, int low = -1, int high = -1, int max = -1) where T : unmanaged
        {
            if (low == -1)
                low = 0;

            if (high > -1 && max > -1)
            {
                int length = high - low;
                int capacity = max - low;

                slice<T> fullSlice = new slice<T>(length, capacity);

                for (int i = 0; i < length; i++)
                    fullSlice[i] = source[low + i];

                return fullSlice;
            }

            if (high == -1)
                high = sourceLength;

            return new slice<T>(handle, sourceLength, low, high);
        }
    }
}