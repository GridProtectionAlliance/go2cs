﻿//******************************************************************************************************
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

namespace go;

public interface ISlice : IArray
{
    nint Low { get; }

    nint High { get; }

    nint Capacity { get; }

    nint Available { get; }

    ISlice? Append(object[] elems);
}

public interface ISlice<T> : ISlice
{
    ISlice<T> Append(params T[] elems);
}

[Serializable]
public readonly struct slice<T> : ISlice<T>, IList<T>, IReadOnlyList<T>, IEnumerable<(nint, T)>, IEquatable<slice<T>>, IEquatable<ISlice>
{
    internal readonly T[] m_array;
    private readonly nint m_low;
    private readonly nint m_length;

    public slice()
    {
        m_array = [];
        m_low = m_length = 0;
    }
    
    public slice(T[]? array)
    {
        m_array = array ?? throw new ArgumentNullException(nameof(array), "slice array reference is null.");
        m_low = 0;
        m_length = array.Length;
    }

    public slice(Span<T> source)
    {
        m_array = source.ToArray();
        m_low = 0;
        m_length = m_array.Length;
    }

    public slice(ReadOnlySpan<T> source)
    {
        m_array = source.ToArray();
        m_low = 0;
        m_length = m_array.Length;
    }

    public slice(Memory<T> source)
    {
        m_array = source.ToArray();
        m_low = 0;
        m_length = m_array.Length;
    }

    public slice(ReadOnlyMemory<T> source)
    {
        m_array = source.ToArray();
        m_low = 0;
        m_length = m_array.Length;
    }

    public slice(array<T> array) : this((T[])array) { }
    
    public T[] Source
    {
        get
        {
            return m_array;
        }
    }

    public slice(T[]? array, (int, int) offsetAndLength)
    {
        (int low, int length) = offsetAndLength;

        if (low < 0)
            throw new ArgumentOutOfRangeException(nameof(offsetAndLength), "Offset value is less than zero.");

        m_array = array ?? throw new ArgumentNullException(nameof(array), "slice array reference is null.");

        if (array.Length - low < length)
            throw new ArgumentException($"Offset {nameof(low)} and {nameof(length)} represent a range outside bounds of the array reference.");

        m_low = low;
        m_length = length;
    }

    public slice(T[]? array, nint low = 0, nint high = -1)
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

    public slice(array<T> array, nint low = 0, nint high = -1) : this((T[])array, low, high) { }

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

    public nint Low
    {
        get
        {
            return m_low;
        }
    }

    public nint High
    {
        get
        {
            return m_low + m_length;
        }
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public nint Length
    {
        get
        {
            return m_length;
        }
    }

    public nint Capacity
    {
        get
        {
            return m_array.Length - m_low;
        }
    }

    public nint Available
    {
        get
        {
            return m_array.Length - m_length;
        }
    }

    // Returning by-ref value allows slice to be a struct instead of a class and still allow read and write
    // Allows for implicit index support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support
    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref m_array[m_low + index];
        }
    }

    public ref T this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref m_array[m_low + index];
        }
    }

    public ref T this[ulong index]
    {
        get
        {
            return ref this[(nint)index];
        }
    }

    // Allows for implicit range support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-range-support
    public slice<T> Slice(int start, int length)
    {
        return m_array.slice(start, start + length, Capacity);
    }

    public nint IndexOf(in T item)
    {
        int index = Array.IndexOf(m_array, item, (int)m_low, (int)m_length);
        return index >= 0 ? index - m_low : -1;
    }

    public bool Contains(in T item)
    {
        return Array.IndexOf(m_array, item, (int)m_low, (int)m_length) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(m_array, m_low, array, arrayIndex, m_length);
    }

    public T[] ToArray()
    {
        T[] array = new T[Length];
        CopyTo(array, 0);
        return array;
    }

    public slice<T> Append(T[] elems)
    {
        return Append(this, elems);
    }

    public slice<T> Clone()
    {
        return m_array.slice();
    }

    public IEnumerator<(nint, T)> GetEnumerator()
    {
        SliceEnumerator enumerator = new(this);
        nint index = 0;

        while (enumerator.MoveNext())
            yield return (index++, enumerator.Current);
    }

    // Returns an enumerator for just index enumeration
    public IEnumerable<nint> Range
    {
        get
        {
            for (nint i = Low; i < High; i++)
                yield return i;
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";
    }

    public override int GetHashCode()
    {
        return m_array.GetHashCode() ^ (int)m_low ^ (int)m_length;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ISlice);
    }

    public bool Equals(ISlice? other)
    {
        return other?.Source == m_array && other.Low == m_low && other.Length == m_length;
    }

    public bool Equals(slice<T> other)
    {
        return other.m_array == m_array && other.m_low == m_low && other.m_length == m_length;
    }

    #region [ Operators ]

    // Enable implicit conversions between slice<T> and T[]
    public static implicit operator slice<T>(T[] value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(Span<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(ReadOnlySpan<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(Memory<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(ReadOnlyMemory<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator T[](slice<T> value)
    {
        return value.ToArray();
    }

    // Enable implicit conversions between slice<T> and array<T>
    public static implicit operator slice<T>(array<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator array<T>(slice<T> value)
    {
        return new array<T>(value.ToArray());
    }

    // slice<T> to slice<T> comparisons
    public static bool operator ==(slice<T> a, slice<T> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(slice<T> a, slice<T> b)
    {
        return !(a == b);
    }

    // slice<T> to ISlice comparisons
    public static bool operator ==(ISlice? a, slice<T> b)
    {
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(ISlice? a, slice<T> b)
    {
        return !(a == b);
    }

    public static bool operator ==(slice<T> a, ISlice? b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(slice<T> a, ISlice? b)
    {
        return !(a == b);
    }

    // slice<T> to nil comparisons
    public static bool operator ==(slice<T> slice, NilType _)
    {
        return slice.Length == 0 && slice.Capacity == 0;
    }

    public static bool operator !=(slice<T> slice, NilType nil)
    {
        return !(slice == nil);
    }

    public static bool operator ==(NilType nil, slice<T> slice)
    {
        return slice == nil;
    }

    public static bool operator !=(NilType nil, slice<T> slice)
    {
        return slice != nil;
    }

    public static implicit operator slice<T>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        return MemberwiseClone();
    }

    ISlice ISlice.Append(object[] elems)
    {
        return Append(elems.Cast<T>().ToArray());
    }

    ISlice<T> ISlice<T>.Append(params T[] elems)
    {
        return Append(elems);
    }

    Array IArray.Source
    {
        get
        {
            return m_array;
        }
    }

    object? IArray.this[nint index]
    {
        get
        {
            return this[index];
        }
        set
        {
            this[index] = (T)value!;
        }
    }

    T IList<T>.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
            if (index < 0 || index >= m_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            m_array[m_low + index] = value;
        }
    }

    int IList<T>.IndexOf(T item)
    {
        return (int)IndexOf(item);
    }

    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException();
    }

    void IList<T>.RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    int IReadOnlyCollection<T>.Count
    {
        get
        {
            return (int)m_length;
        }
    }

    T IReadOnlyList<T>.this[int index]
    {
        get
        {
            return this[m_low + index];
        }
    }

    bool ICollection<T>.IsReadOnly
    {
        get
        {
            return false;
        }
    }

    int ICollection<T>.Count
    {
        get
        {
            return (int)m_length;
        }
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Contains(T item)
    {
        return Contains(item);
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new SliceEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new SliceEnumerator(this);
    }

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

        object? IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            m_current = m_start - 1;
        }

        public void Dispose()
        {
        }
    }

    #endregion

    public static slice<T> From<TSource>(TSource[]? array)
    {
        if (array is null)
            return new slice<T>(Array.Empty<T>());

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
            Array.Copy(elems, newArray, elems.Length);
            return new slice<T>(newArray);
        }

        if (elems.Length <= slice.Available)
        {
            Array.Copy(elems, 0, slice.m_array, slice.High, elems.Length);
            return slice.slice(high: slice.High + elems.Length);
        }

        nint newCapacity = CalculateNewCapacity(slice, slice.m_array.Length + elems.Length);
        newArray = new T[newCapacity];

        Array.Copy(slice.m_array, newArray, slice.Length);
        Array.Copy(elems, 0, newArray, slice.Length, elems.Length);

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
        return slice.m_array.slice(low == -1 ? slice.Low : low, high == -1 ? slice.High : high, max);
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

            slice<T> fullSlice = new(length, capacity);
            Array.Copy(array, low, fullSlice.m_array, 0, length);
            return fullSlice;
        }

        if (high == -1)
            high = array.Length;

        return new slice<T>(array, low, high);
    }

    public static slice<T> slice<T>(this array<T> array, nint low = -1, nint high = -1, nint max = -1)
    {
        return array.m_array.slice(low, high, max);
    }

    // slice of a string helper function
    public static slice<byte> slice(this @string source, nint low = -1, nint high = -1, nint max = -1)
    {
        return source.m_value.slice(low, high, max);
    }
}
