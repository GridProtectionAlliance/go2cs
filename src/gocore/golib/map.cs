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
using System.Runtime.CompilerServices;

#pragma warning disable 8600, IDE1006

namespace go
{
    public interface IMap : IEnumerable
    {
        nint Length { get; }

        object? this[object key] { get; set; }
    }

    public struct map<TKey, TValue> : IMap, IDictionary, IDictionary<TKey, TValue> where TKey : notnull
    {
        private Dictionary<TKey, TValue>? m_map;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public map(int size) => m_map = new Dictionary<TKey, TValue>(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public map(IEnumerable<KeyValuePair<TKey, TValue>> map)
        {
        #if NET5_0
            m_map = new Dictionary<TKey, TValue>(map);
        #else
            m_map = map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        #endif
        }

        private Dictionary<TKey, TValue> Map
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_map ??= new Dictionary<TKey, TValue>();
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Map.Count;
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Map.TryGetValue(key, out TValue value) ? value : default!;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Map[key] = value;
        }

        public (TValue, bool) this[TKey key, bool _]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Map.TryGetValue(key, out TValue value) ? (value!, true) : (default!, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value) => Map.Add(key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key) => Map.Remove(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Map.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (m_map is not null)
                return m_map.TryGetValue(key, out value!);

            value = default!;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => Map.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(map<TKey, TValue> other) => Map.Equals(other.m_map);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is map<TKey, TValue> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"map[{(m_map is null ? "nil" : string.Join(" ", m_map.Select(kvp => $"{kvp.Key}:{kvp.Value}").Take(20)))}{(Count > 20 ? " ..." : "")}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Map.GetHashCode();

#region [ Operators ]

        // Enable implicit conversions between map<TKey, TValue> and IDictionary<T>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator map<TKey, TValue>(Dictionary<TKey, TValue> value) => new map<TKey, TValue>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Dictionary<TKey, TValue>(map<TKey, TValue> value) => value.m_map!;

        // map<TKey, TValue> to map<TKey, TValue> comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(map<TKey, TValue> a, map<TKey, TValue> b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(map<TKey, TValue> a, map<TKey, TValue> b) => !(a == b);

        // map<T> to IMap comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IMap a, map<TKey, TValue> b) => b.Equals(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IMap a, map<TKey, TValue> b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(map<TKey, TValue> a, IMap b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(map<TKey, TValue> a, IMap b) => !(a == b);

        // map<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(map<TKey, TValue> map, NilType _) => map.Count == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(map<TKey, TValue> map, NilType nil) => !(map == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, map<TKey, TValue> map) => map == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, map<TKey, TValue> map) => map != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator map<TKey, TValue>(NilType _) => default;

#endregion

#region [ Interface Implementations ]

        nint IMap.Length => Count;

        object? IMap.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value!;
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }

        void IDictionary.Add(object key, object? value) => ((IDictionary)m_map)?.Add(key, value);

        void IDictionary.Remove(object key) => ((IDictionary)m_map)?.Remove(key);

        bool IDictionary.Contains(object key) => ((IDictionary)m_map)?.Contains(key) ?? false;

        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)m_map)?.GetEnumerator()!;

        bool IDictionary.IsFixedSize => ((IDictionary)m_map)?.IsFixedSize ?? false;

        bool IDictionary.IsReadOnly => ((IDictionary)m_map)?.IsReadOnly ?? false;

        object? IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value!;
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Map.Values!;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Map.Keys!;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Remove(item) ?? false;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.IsReadOnly ?? false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Add(item);

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Clear();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.Contains(item) ?? false;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)m_map)?.CopyTo(array, arrayIndex);

        ICollection IDictionary.Keys => ((IDictionary)m_map)?.Keys!;

        ICollection IDictionary.Values => ((IDictionary)m_map)?.Values!;

        bool ICollection.IsSynchronized => ((ICollection)m_map)?.IsSynchronized ?? false;

        object ICollection.SyncRoot => ((ICollection)m_map)?.SyncRoot ?? this;

        void ICollection.CopyTo(Array array, int index) => ((ICollection)m_map)?.CopyTo(array, index);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)m_map)?.GetEnumerator()!;

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_map)?.GetEnumerator()!;

#endregion
    }
}
