//******************************************************************************************************
//  string.cs - Gbtc
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
//  06/28/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using rune = System.Int32;

#pragma warning disable IDE0032, IDE1006

namespace go.experimental
{
    /// <summary>
    /// Represents a structure that behaves like a Go string.
    /// </summary>
    public readonly unsafe struct @string : IConvertible, IReadOnlyList<byte>, IEnumerable<rune>, IEnumerable<(long, rune)>, IEnumerable<char>, ICloneable
    {
        private readonly GCHandle m_handle;
        private readonly byte* m_value;
        private readonly int m_length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(byte[]? bytes)
        {
            if (bytes is null)
            {
                m_handle = default;
                m_value = null;
                m_length = 0;
            }
            else
            {
                m_handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                m_value = (byte*)m_handle.AddrOfPinnedObject();
                m_length = bytes.Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(char[] value) : this(new string(value)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(rune[] value) : this(new string(value.Select(item => (char)item).ToArray())) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(slice<byte> value) : this(value.ToArray()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(slice<char> value) : this(value.ToArray()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(slice<rune> value) : this(value.ToArray()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(string value) : this(Encoding.UTF8.GetBytes(value ?? "")) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string(@string value) : this((byte[])value) { }

        internal GCHandle Handle => m_handle;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_length;
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value is null)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, Length);

                return m_value[index];
            }
        }

        public byte this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_value is null)
                    throw RuntimeErrorPanic.IndexOutOfRange(index, Length);

                return m_value[index];
            }
        }

        public byte this[ulong index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[(long)index];
        }

        // Allows for implicit range support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-range-support
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<byte> Slice(int start, int length) => 
            new slice<byte>(m_handle, m_length, start, start + length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public slice<byte> Slice(long start, long length) =>
            new slice<byte>(m_handle, m_length, (int)start, (int)(start + length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => 
            m_length == 0 ? "" : Encoding.UTF8.GetString(m_value, m_length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(@string other) => BytesAreEqual(m_value,  other.m_value, Length, other.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            return obj switch
            {
                null => false,
                @string gostr => Equals(gostr),
                string str => Equals(str),
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => ToString().GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => ToString().ToString(provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeCode GetTypeCode() => TypeCode.String;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @string Clone() => new @string(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<(long, rune)> GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] rune = new char[1];
            int byteCount;

            for (long index = 0; index < m_length; index += byteCount)
            {
                byteCount = 1;
                bool completed = Decode(decoder, index, byteCount, rune);

                if (!completed)
                {
                    byteCount = 2;
                    completed = Decode(decoder, index, byteCount, rune);
                }

                if (completed)
                    yield return (index, rune[0]);
                else
                    yield return (index, '\uFFFD');
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Decode(Decoder decoder, long index, int byteCount, char[] rune)
        {
            bool completed;
            byte* bytes = &m_value[index];

            fixed (char* chars = rune)
                decoder.Convert(bytes, byteCount, chars, 1, true, out _, out _, out completed);

            return completed;
        }

        public static @string Default { get; } = new @string("");

        #region [ Operators ]

        // Enable implicit conversions between string and @string struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(string value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(@string value) => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(slice<byte> value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<byte>(@string value) => value.slice();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(slice<rune> value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<rune>(@string value) =>  new slice<rune>(((IEnumerable<rune>)value).ToArray());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(slice<char> value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator slice<char>(@string value) => new slice<char>(((IEnumerable<char>)value).ToArray());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator byte[](@string value) => ((IEnumerable<byte>)value).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(byte[] value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator rune[](@string value) => ((IEnumerable<rune>)value).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(rune[] value) => new @string(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator char[](@string value) => ((IEnumerable<char>)value).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(char[] value) => new @string(value);

        // Enable comparisons between nil and @string struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@string value, NilType _) => value.Equals(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@string value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @string value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @string value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @string(NilType _) => Default;

        #endregion

        #region [ Interface Implementations ]

        object ICloneable.Clone() => Clone();

        int IReadOnlyCollection<byte>.Count => Length;

        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)ToString()).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible)ToString()).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible)ToString()).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible)ToString()).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible)ToString()).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible)ToString()).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible)ToString()).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible)ToString()).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible)ToString()).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible)ToString()).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible)ToString()).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible)ToString()).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible)ToString()).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible)ToString()).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible)ToString()).ToType(conversionType, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            BytePointerEnumerator enumerator = new BytePointerEnumerator(this);

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            BytePointerEnumerator enumerator = new BytePointerEnumerator(this);

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<rune> IEnumerable<rune>.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            foreach (char item in ToString())
                yield return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            if (m_length == 0)
                yield break;

            foreach (char item in ToString())
                yield return item;
        }

        private sealed class BytePointerEnumerator : IEnumerator<byte>
        {
            private readonly byte* m_value;
            private readonly int m_length;
            private int m_index;

            internal BytePointerEnumerator(@string str)
            {
                m_value = str.m_value;
                m_length = str.m_length;
                m_index = -1;
            }

            public bool MoveNext()
            {
                if (m_index >= m_length)
                    return false;

                m_index++;
                return m_index < m_length;
            }

            public byte Current
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool BytesAreEqual(byte* bytes1, byte* bytes2, int length1, int length2)
        {
            if (bytes1 == bytes2)
                return true;

            if (length1 != length2)
                return false;

            if (length1 == 0)
                return true;

            int len = length1;
            int rem = len % (sizeof(long) * 16);
            long* b1 = (long*)bytes1;
            long* b2 = (long*)bytes2;
            long* e1 = (long*)(bytes1 + len - rem);

            while (b1 < e1)
            {
                if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
                    *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
                    *(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
                    *(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
                    *(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
                    *(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
                    *(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
                    *(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
                    return false;
                b1 += 16;
                b2 += 16;
            }

            for (int i = 0; i < rem; i++)
                if (bytes1[len - 1 - i] != bytes2[len - 1 - i])
                    return false;

            return true;
        }

        #endregion
    }
}