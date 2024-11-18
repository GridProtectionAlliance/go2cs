//******************************************************************************************************
//  map.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  06/17/2020 - J. Ritchie Carroll
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

public interface IMap : IEnumerable
{
    nint Length { get; }

    object? this[object key] { get; set; }
}

public readonly struct map<TKey, TValue> : IMap, IDictionary, IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> m_map;

    public map()
    {
        m_map = new Dictionary<TKey, TValue>();
    }

    public map(int size)
    {
        m_map = new Dictionary<TKey, TValue>(size);
    }

    public map(IEnumerable<KeyValuePair<TKey, TValue>> map)
    {
        m_map = new Dictionary<TKey, TValue>(map);
    }

    public int Count => m_map.Count;

    public TValue this[TKey key]
    {
        get => m_map.TryGetValue(key, out TValue? value) ? value : default!;
        set => m_map[key] = value;
    }

    public (TValue, bool) this[TKey key, bool _]
    {
        get => m_map.TryGetValue(key, out TValue? value) ? (value!, true) : (default!, false);
    }

    public void Add(TKey key, TValue value)
    {
        m_map.Add(key, value);
    }

    public bool Remove(TKey key)
    {
        return m_map.Remove(key);
    }

    public void Clear()
    {
        m_map.Clear();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (m_map is not null)
            return m_map.TryGetValue(key, out value!);

        value = default!;
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return m_map.ContainsKey(key);
    }

    public bool Equals(map<TKey, TValue> other)
    {
        return m_map.Equals(other.m_map);
    }

    public override bool Equals(object? obj)
    {
        return obj is map<TKey, TValue> other && Equals(other);
    }

    public override string ToString()
    {
        return $"map[{(m_map is null ? "nil" : string.Join(" ", m_map.Select(kvp => $"{kvp.Key}:{kvp.Value}").Take(20)))}{(Count > 20 ? " ..." : "")}]";
    }

    public override int GetHashCode()
    {
        return m_map.GetHashCode();
    }

    #region [ Operators ]

    // Enable implicit conversions between map<TKey, TValue> and IDictionary<T>
    public static implicit operator map<TKey, TValue>(Dictionary<TKey, TValue> value)
    {
        return new map<TKey, TValue>(value);
    }

    public static implicit operator Dictionary<TKey, TValue>(map<TKey, TValue> value)
    {
        return value.m_map;
    }

    // map<TKey, TValue> to map<TKey, TValue> comparisons
    public static bool operator ==(map<TKey, TValue> a, map<TKey, TValue> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(map<TKey, TValue> a, map<TKey, TValue> b)
    {
        return !(a == b);
    }

    // map<T> to IMap comparisons
    public static bool operator ==(IMap a, map<TKey, TValue> b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(IMap a, map<TKey, TValue> b)
    {
        return !(a == b);
    }

    public static bool operator ==(map<TKey, TValue> a, IMap b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(map<TKey, TValue> a, IMap b)
    {
        return !(a == b);
    }

    // map<T> to nil comparisons
    public static bool operator ==(map<TKey, TValue> map, NilType _)
    {
        return map.Count == 0;
    }

    public static bool operator !=(map<TKey, TValue> map, NilType nil)
    {
        return !(map == nil);
    }

    public static bool operator ==(NilType nil, map<TKey, TValue> map)
    {
        return map == nil;
    }

    public static bool operator !=(NilType nil, map<TKey, TValue> map)
    {
        return map != nil;
    }

    public static implicit operator map<TKey, TValue>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    nint IMap.Length
    {
        get
        {
            return Count;
        }
    }

    object? IMap.this[object key]
    {
        get
        {
            return this[(TKey)key];
        }
        set
        {
            this[(TKey)key] = (TValue)value!;
        }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get
        {
            return this[key];
        }
        set
        {
            this[key] = value;
        }
    }

    void IDictionary.Add(object key, object? value)
    {
        ((IDictionary)m_map)?.Add(key, value);
    }

    void IDictionary.Remove(object key)
    {
        ((IDictionary)m_map)?.Remove(key);
    }

    bool IDictionary.Contains(object key)
    {
        return ((IDictionary)m_map)?.Contains(key) ?? false;
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)m_map)?.GetEnumerator()!;
    }

    bool IDictionary.IsFixedSize
    {
        get
        {
            return ((IDictionary)m_map)?.IsFixedSize ?? false;
        }
    }

    bool IDictionary.IsReadOnly
    {
        get
        {
            return ((IDictionary)m_map)?.IsReadOnly ?? false;
        }
    }

    object? IDictionary.this[object key]
    {
        get
        {
            return this[(TKey)key];
        }
        set
        {
            this[(TKey)key] = (TValue)value!;
        }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            return m_map.Values;
        }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get
        {
            return m_map.Keys;
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Remove(item) ?? false;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
        get
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.IsReadOnly ?? false;
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Add(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Clear();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Contains(item) ?? false;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.CopyTo(array, arrayIndex);
    }

    ICollection IDictionary.Keys
    {
        get
        {
            return ((IDictionary)m_map)?.Keys!;
        }
    }

    ICollection IDictionary.Values
    {
        get
        {
            return ((IDictionary)m_map)?.Values!;
        }
    }

    bool ICollection.IsSynchronized
    {
        get
        {
            return ((ICollection)m_map)?.IsSynchronized ?? false;
        }
    }

    object ICollection.SyncRoot
    {
        get
        {
            return ((ICollection)m_map)?.SyncRoot ?? this;
        }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)m_map)?.CopyTo(array, index);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)m_map)?.GetEnumerator()!;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)m_map)?.GetEnumerator()!;
    }

    #endregion
}
