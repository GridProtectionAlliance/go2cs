//******************************************************************************************************
//  SparseArray.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
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

namespace go.golib;

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
    /// <remarks>
    /// A Go sparse composite (<c>[]string{5: "x"}</c>) produces a DENSE array of length
    /// max-index+1 with zero-valued gaps, so the count reflects the dense length.
    /// </remarks>
    public int Count => m_items.Count == 0 ? 0 : m_items.Keys.Max() + 1;

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
        foreach (T item in this)
            array[arrayIndex++] = item;
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
        // Enumerate the DENSE Go array this sparse initializer denotes: index order from 0
        // through the max key, GAPS INCLUDED as zero values. Skipping gaps mis-positions every
        // element after the first hole and shortens the materialized slice (syscall zerrors'
        // invented-error table, index-out-of-range at runtime).
        if (m_items.Count == 0)
            yield break;

        int max = m_items.Keys.Max();

        for (int i = 0; i <= max; i++)
            yield return m_items.TryGetValue(i, out T? value) ? value! : default!;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
