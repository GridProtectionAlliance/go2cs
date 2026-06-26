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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace go;

public interface IMap
{
    nint Length { get; }
}

public interface IMap<TKey, TValue> : IMap, IDictionary<TKey, TValue> where TKey : notnull
{
    (TValue, bool) this[TKey key, bool _] { get; }
}

public readonly struct map<TKey, TValue> : IMap<TKey, TValue>, ISupportMake<map<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> m_map;

    public map()
    {
        m_map = new Dictionary<TKey, TValue>();
    }

    public map(nint size)
    {
        m_map = new Dictionary<TKey, TValue>((int)size);
    }

    public map(IEnumerable<KeyValuePair<TKey, TValue>> map)
    {
        m_map = new Dictionary<TKey, TValue>(map);
    }

    /// <inheritdoc />
    public int Count => m_map?.Count ?? 0;

    public TValue this[TKey key]
    {
        // Reading from a nil map yields the zero value in Go, so route through the
        // null-safe TryGetValue rather than dereferencing m_map directly.
        get => TryGetValue(key, out TValue? value) ? value : default!;
        set
        {
            // Writing to a nil map panics in Go ("assignment to entry in nil map").
            if (m_map is null)
                throw new PanicException("assignment to entry in nil map");

            m_map[key] = value;
        }
    }

    public (TValue, bool) this[TKey key, bool _]
    {
        // Comma-ok read of a nil (or absent) key yields (zero, false).
        get => TryGetValue(key, out TValue? value) ? (value!, true) : (default!, false);
    }

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        // Adding to a nil map panics in Go, the same as an index assignment.
        if (m_map is null)
            throw new PanicException("assignment to entry in nil map");

        m_map.Add(key, value);
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        // delete() on a nil map is a no-op in Go (no panic).
        return m_map?.Remove(key) ?? false;
    }

    public void Clear()
    {
        // clear() on a nil map is a no-op in Go.
        m_map?.Clear();
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (m_map is not null)
            return m_map.TryGetValue(key, out value!);

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
        // A nil map contains no keys.
        return m_map?.ContainsKey(key) ?? false;
    }

    public bool Equals(map<TKey, TValue> other)
    {
        // Go maps are reference types: `m == nil` is true only for the nil map, and two
        // map values are equal only when they share the same backing store. Comparing by
        // reference identity captures both (and is null-safe — two nil maps share a null
        // backing store and so compare equal, while a nil map differs from an empty one).
        return ReferenceEquals(m_map, other.m_map);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is map<TKey, TValue> other && Equals(other);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"map[{(m_map is null ? "<nil>" : string.Join(" ", m_map.Select(kvp => $"{kvp.Key}:{kvp.Value}").Take(20)))}{(Count > 20 ? " ..." : "")}]";
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Reference-based hash, consistent with the identity Equals above; a nil map hashes to 0.
        return m_map?.GetHashCode() ?? 0;
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
        // A map is nil only when it has no backing store — an empty but allocated map is not nil.
        return map.m_map is null;
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

    nint IMap.Length => Count;

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values => m_map.Values;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => m_map.Keys;

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Remove(item) ?? false;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.IsReadOnly ?? false;

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

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        // Ranging over a nil map performs zero iterations in Go.
        return (m_map ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)(m_map ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>())).GetEnumerator();
    }

    #endregion
    
    /// <inheritdoc />
    public static map<TKey, TValue> Make(nint p1 = 0, nint p2 = -1)
    {
        return new map<TKey, TValue>(p1);
    }
}
