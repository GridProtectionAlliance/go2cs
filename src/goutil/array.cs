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
#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace go
{
    public interface IArray : EmptyInterface, IEnumerable, ICloneable
    {
        int Length { get; }

        object this[int index] { get; set; }
    }

    [Serializable]
    public readonly struct array<T> : IArray, IList<T>, IReadOnlyList<T>, IEnumerable<(int index, T value)>, IEquatable<array<T>>, IEquatable<IArray>
    {
        private readonly T[] m_array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array(int length) => m_array = new T[length];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array(T[] array) => m_array = array ?? new T[0];

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_array.Length;
        }

        // Returning by-ref value allows array to be a struct instead of a class and still allow read and write
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref m_array[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            int index = Array.IndexOf(m_array, item, 0, m_array.Length);
            return index >= 0 ? index : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => Array.IndexOf(m_array, item, 0, m_array.Length) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex) => m_array.CopyTo(array, arrayIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public array<T> Clone() => new array<T>(m_array.Clone() as T[]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<(int index, T value)> GetEnumerator()
        {
            int index = 0;

            foreach (T item in m_array)
                yield return (index++, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_array.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => Equals(obj as IArray);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IArray other) => m_array.Equals(other as T[]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(array<T> other) => m_array.Equals(other.m_array);

        #region [ Operators ]

        // Enable implicit conversions between array<T> and T[]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator array<T>(T[] value) => new array<T>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T[](array<T> value) => value.m_array;

        // array<T> to array<T> comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> a, array<T> b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> a, array<T> b) => !(a == b);

        // Slice<T> to IArray comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IArray a, array<T> b) => b.Equals(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IArray a, array<T> b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> a, IArray b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> a, IArray b) => !(a == b);

        // Slice<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(array<T> array, NilType nil) => array.Length == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(array<T> array, NilType nil) => !(array == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, array<T> array) => array == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, array<T> array) => array != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator array<T>(NilType nil) => default;

        #endregion

        #region [ Interface Implementations ]

        object ICloneable.Clone() => m_array.Clone();

        object IArray.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
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

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        int IReadOnlyCollection<T>.Count => Length;

        T IReadOnlyList<T>.this[int index] => this[index];

        bool ICollection<T>.IsReadOnly => false;

        int ICollection<T>.Count => Length;

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        void ICollection<T>.Clear() => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => m_array.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_array.GetEnumerator();

        #endregion
    }
}