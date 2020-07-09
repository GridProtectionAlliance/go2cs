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
// ReSharper disable SuggestBaseTypeForParameter

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE0032, IDE1006

namespace go.experimental
{
    public interface IArray : IEnumerable, ICloneable
    {
        int Length { get; }

        object this[int index] { get; set; }
    }

    [Serializable]
    public readonly unsafe struct array<T> : IArray, IList<T>, IReadOnlyList<T>, IEnumerable<(long, T)> where T : unmanaged
    {
        private readonly GCHandle m_handle;
        private readonly T* m_value;
        private readonly int m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array(int length) : this(new T[length]) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array(T[] array)
        {
            m_handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            m_value = (T*)m_handle.AddrOfPinnedObject();
            m_length = array.Length;
        }

        internal GCHandle Handle => m_handle;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_length;
        }

        // Returning by-ref value allows array to be a struct instead of a class and still allow read and write
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value == null)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

                return ref m_value[index];

            }
        }

        public ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value == null)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

                return ref m_value[index];
            }
        }

        public ref T this[ulong index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[(long)index];
        }

        // Allows for implicit range support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-range-support
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Slice(int start, int length) => 
            new slice<T>(m_handle, m_length, start, start + length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<T> Slice(long start, long length) =>
            new slice<T>(m_handle, m_length, (int)start, (int)(start + length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            if (m_value == null)
                return -1;

            for (int i = 0; i < m_length; i++)
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

            for (int i = 0; i < m_length; i++)
                array[arrayIndex + i] = m_value[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array<T> Clone() => new array<T>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<(long, T)> GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            long index = 0;

            ArrayEnumerator enumerator = new ArrayEnumerator(this);

            while (enumerator.MoveNext())
                yield return (index++, enumerator.Current);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_handle.GetHashCode();

        #region [ Operators ]

        // Enable implicit conversions between array<T> and T[]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator array<T>(T[] value) => new array<T>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T[](array<T> value)
        {
            T[] result = new T[value.m_length];
            value.CopyTo(result, 0);
            return result;
        }

        // array<T> to array<T> comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> a, array<T> b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> a, array<T> b) => !(a == b);

        // array<T> to IArray comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IArray a, array<T> b) => b.Equals(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IArray a, array<T> b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> a, IArray b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> a, IArray b) => !(a == b);

        // array<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> array, NilType _) => array.Length == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> array, NilType nil) => !(array == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, array<T> array) => array == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, array<T> array) => array != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator array<T>(NilType _) => default;

        #endregion

        #region [ Interface Implementations ]

        object ICloneable.Clone() => Clone();

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

        int IReadOnlyCollection<T>.Count => Length;

        T IReadOnlyList<T>.this[int index] => this[index];

        bool ICollection<T>.IsReadOnly => false;

        int ICollection<T>.Count => Length;

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        void ICollection<T>.Clear() => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            ArrayEnumerator enumerator = new ArrayEnumerator(this);

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            ArrayEnumerator enumerator = new ArrayEnumerator(this);

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        private sealed class ArrayEnumerator : IEnumerator<T>
        {
            private readonly T* m_value;
            private readonly int m_length;
            private int m_index;

            internal ArrayEnumerator(array<T> array)
            {
                m_value = array.m_value;
                m_length = array.m_length;
                m_index = -1;
            }

            public bool MoveNext()
            {
                if (m_index >= m_length)
                    return false;

                m_index++;
                return m_index < m_length;
            }

            public T Current
            {
                get
                {
                    if (m_index < 0)
                        throw new InvalidOperationException("enumeration not started.");

                    if (m_index >= m_length)
                        throw new InvalidOperationException("enumeration has ended.");

                    return m_value[m_index];
                }
            }

            object IEnumerator.Current => Current!;

            void IEnumerator.Reset() => m_index = -1;

            public void Dispose() { }
        }

        #endregion
    }
}