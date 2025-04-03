//******************************************************************************************************
//  SparseArray.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  03/31/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace go.runtime;

/// <summary>
/// Represents a sparse array.
/// </summary>
/// <typeparam name="T">Type of sparse array elements.</typeparam>
public class SparseArray<T> : IList<T>
{
    private readonly Dictionary<int, T> m_items = [];

    /// <inheritdoc />
    public T this[int index]
    {
        get => m_items[index];
        set => m_items[index] = value;
    }

    /// <inheritdoc />
    public int Count => m_items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(T item)
    {
        m_items.Add(m_items.Count, item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        m_items.Clear();
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        return m_items.ContainsValue(item);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        m_items.Values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        KeyValuePair<int, T> record = m_items.FirstOrDefault(entry => entry.Value?.Equals(item) ?? false, new KeyValuePair<int, T>(-1, default!));
        return record.Key > -1 && m_items.Remove(record.Key);
    }

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        KeyValuePair<int, T> record = m_items.FirstOrDefault(entry => entry.Value?.Equals(item) ?? false, new KeyValuePair<int, T>(-1, default!));
        return (int)record.Key;
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        m_items.Add(index, item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        m_items.Remove(index);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Items are enumerated in index order.
    /// </remarks>
    public IEnumerator<T> GetEnumerator()
    {
        // Ensure items are enumerated in order by key
        return m_items
            .OrderBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
