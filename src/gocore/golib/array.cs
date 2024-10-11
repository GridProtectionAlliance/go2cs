//******************************************************************************************************
//  array.cs - Gbtc
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
//  08/12/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace go;

public interface IArray : IEnumerable, ICloneable
{
    Array? Source { get; }
    
    nint Length { get; }

    object? this[nint index] { get; set; }
}

public interface IArray<T> : IArray
{
    new T[] Source { get; }
    
    new ref T this[nint index] { get; }
}

[Serializable]
public readonly struct array<T> : IArray<T>, IList<T>, IReadOnlyList<T>, IEnumerable<(nint, T)>, IEquatable<array<T>>, IEquatable<IArray>
{
    internal readonly T[] m_array;

    public array()
    {
        m_array = [];
    }

    public array(int length)
    {
        m_array = new T[length];
    }

    public array(nint length)
    {
        m_array = new T[length];
    }

    public array(ulong length)
    {
        m_array = new T[length];
    }

    public array(T[]? array)
    {
        m_array = array ?? [];
    }

    public array(Span<T> source)
    {
        m_array = source.ToArray();
    }

    public array(ReadOnlySpan<T> source)
    {
        m_array = source.ToArray();
    }

    public array(Memory<T> source)
    {
        m_array = source.ToArray();
    }

    public array(ReadOnlyMemory<T> source)
    {
        m_array = source.ToArray();
    }

    public T[] Source
    {
        get
        {
            return m_array;
        }
    }

    public nint Length
    {
        get
        {
            return m_array.Length;
        }
    }

    // Returning by-ref value allows array to be a struct instead of a class and still allow read and write
    public ref T this[int index]
    {
        get
        {
            return ref m_array[index];
        }
    }

    public ref T this[nint index]
    {
        get
        {
            return ref m_array[index];
        }
    }

    public ref T this[ulong index]
    {
        get
        {
            return ref this[(nint)index];
        }
    }

    public slice<T> this[Range range]
    {
        get
        {
            return new slice<T>(m_array, range.GetOffsetAndLength(m_array.Length));
        }
    }

    public slice<T> Slice(int start, int length)
    {
        return new slice<T>(m_array, start, start + length);
    }

    public slice<T> Slice(nint start, nint length)
    {
        return new slice<T>(m_array, start, start + length);
    }

    public int IndexOf(in T item)
    {
        int index = Array.IndexOf(m_array, item, 0, m_array.Length);
        return index >= 0 ? index : -1;
    }

    public bool Contains(in T item)
    {
        return Array.IndexOf(m_array, item, 0, m_array.Length) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        m_array.CopyTo(array, arrayIndex);
    }

    public array<T> Clone()
    {
        return new array<T>(m_array.Clone() as T[]);
    }

    public IEnumerator<(nint, T)> GetEnumerator()
    {
        nint index = 0;

        foreach (T item in m_array)
            yield return (index++, item);
    }

    // Returns an enumerator for just index enumeration
    public IEnumerable<nint> Range
    {
        get
        {
            for (nint i = 0; i < m_array.Length; i++)
                yield return i;
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";
    }

    public override int GetHashCode()
    {
        return m_array.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IArray);
    }

    public bool Equals(IArray? other)
    {
        return m_array.Equals(other as T[]);
    }

    public bool Equals(array<T> other)
    {
        return m_array.Equals(other.m_array);
    }

    #region [ Operators ]

    // Enable implicit conversions between array<T> and T[]
    public static implicit operator array<T>(T[] value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(Span<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(ReadOnlySpan<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(Memory<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(ReadOnlyMemory<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator T[](array<T> value)
    {
        return value.m_array;
    }

    // array<T> to array<T> comparisons
    public static bool operator ==(array<T> a, array<T> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(array<T> a, array<T> b)
    {
        return !(a == b);
    }

    // Slice<T> to IArray comparisons
    public static bool operator ==(IArray a, array<T> b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(IArray a, array<T> b)
    {
        return !(a == b);
    }

    public static bool operator ==(array<T> a, IArray b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(array<T> a, IArray b)
    {
        return !(a == b);
    }

    // array<T> to nil comparisons
    public static bool operator ==(array<T> array, NilType _)
    {
        return array.Length == 0;
    }

    public static bool operator !=(array<T> array, NilType nil)
    {
        return !(array == nil);
    }

    public static bool operator ==(NilType nil, array<T> array)
    {
        return array == nil;
    }

    public static bool operator !=(NilType nil, array<T> array)
    {
        return array != nil;
    }

    public static implicit operator array<T>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        return m_array.Clone();
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
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            m_array[index] = value;
        }
    }

    int IList<T>.IndexOf(T item)
    {
        return IndexOf(item);
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
            return (int)Length;
        }
    }

    T IReadOnlyList<T>.this[int index]
    {
        get
        {
            return this[index];
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
            return (int)Length;
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
        return m_array.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_array.GetEnumerator();
    }

    #endregion
}
