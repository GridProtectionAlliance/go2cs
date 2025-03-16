//******************************************************************************************************
//  @string.cs - Gbtc
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
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UseSymbolAlias

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using go.runtime;

namespace go;

/// <summary>
/// Represents a structure with heap allocated data that behaves like a Go string.
/// </summary>
public readonly struct @string : 
    IConvertible, 
    IEquatable<@string>, 
    IComparable<@string>, 
    IReadOnlyList<byte>, 
    IEnumerable<rune>, 
    IEnumerable<(nint, rune)>, 
    IEnumerable<char>, 
    ICloneable, 
    IComparisonOperators<@string, @string, bool>,
    IAdditionOperators<@string, @string, @string>
{
    internal readonly byte[] m_value;

    // If @string needs to match sizeof in Go, it would need to be 16 bytes,
    // in this case 8-byte length value would need to be added here:
    //      private readonly int64 m_length;
    // Currently goal is to match Go's string behavior, not memory layout.

    public @string()
    {
        m_value = [];
    }

    public @string(byte[]? bytes)
    {
        m_value = bytes ?? [];
    }

    public @string(in ReadOnlySpan<byte> bytes)
    {
        m_value = bytes.ToArray();
    }

    public @string(char[] value) : this(new string(value)) { }

    public @string(in ReadOnlySpan<rune> value) : this(value.ToUTF8Bytes()) { }

    public @string(in slice<byte> value) : this(value.ToArray()) { }

    public @string(in slice<char> value) : this(value.ToArray()) { }

    public @string(in slice<rune> value) : this(value.ToSpan()) { }

    public @string(string? value)
    {
        m_value = Encoding.UTF8.GetBytes(value ?? "");
    }

    public @string(@string value) : this(value.m_value) { }

    public int Length => m_value.Length;

    public byte this[int index]
    {
        get
        {
            if (index < 0 || index >= m_value.Length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_value.Length);

            return m_value[index];
        }
    }

    public byte this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_value.Length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_value.Length);

            return m_value[index];
        }
    }

    public byte this[ulong index] => this[(nint)index];
    
    public slice<byte> this[Range range] => new(m_value, range.Start.GetOffset(m_value.Length), range.End.GetOffset(m_value.Length));

    public slice<byte> Slice(int start, int length)
    {
        return new slice<byte>(m_value, start, start + length);
    }

    public slice<byte> Slice(nint start, nint length)
    {
        return new slice<byte>(m_value, start, start + length);
    }

    public Span<byte> ToSpan()
    {
        return new Span<byte>(m_value);
    }

    public Span<byte> ꓸꓸꓸ => ToSpan(); // Spread operator

    internal PinnedBuffer buffer => new(m_value, Length);

    public override string ToString()
    {
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(m_value));
    }

    public bool Equals(@string other)
    {
        return BytesAreEqual(m_value, other.m_value);
    }

    public int CompareTo(@string other)
    {
        return StringComparer.Ordinal.Compare(ToString(), other);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null          => false,
            @string gostr => Equals(gostr),
            string str    => Equals(str),
            _             => false
        };
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public string ToString(IFormatProvider? provider)
    {
        return ToString().ToString(provider);
    }

    public TypeCode GetTypeCode()
    {
        return TypeCode.String;
    }

    public @string Clone()
    {
        return new @string(this);
    }

    public IEnumerator<(nint, rune)> GetEnumerator()
    {
        return new RuneSpanEnumerator(m_value);
    }

    private class RuneSpanEnumerator(byte[] bytes) : IEnumerator<(nint, rune)>
    {
        private readonly byte[] m_bytes = bytes;
        private int m_byteIndex;
        private int m_runeIndex = -1;
        private (nint, rune) m_current;

        public (nint, rune) Current => m_current;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public bool MoveNext()
        {
            if (m_byteIndex >= m_bytes.Length)
                return false;

            ReadOnlySpan<byte> remainingBytes = new(m_bytes, m_byteIndex, m_bytes.Length - m_byteIndex);
            OperationStatus status = Rune.DecodeFromUtf8(remainingBytes, out Rune rune, out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                m_current = (++m_runeIndex, rune.Value);
            }
            else
            {
                // When status is InvalidData or NeedMoreData or DestinationTooSmall, follow
                // Go behavior: Replace invalid sequences with Unicode replacement character
                m_current = (++m_runeIndex, RuneReplacementChar);
                bytesConsumed = bytesConsumed == 0 ? 1 : bytesConsumed;
            }

            m_byteIndex += bytesConsumed;
            return true;
        }

        public void Reset()
        {
            m_byteIndex = 0;
            m_runeIndex = -1;
            m_current = default;
        }
    }

    public rune[] ToRunes()
    {
        // Estimate the rune length (1 rune per byte as worst case)
        int estimatedLength = m_value.Length;

        Span<rune> runes = estimatedLength <= StackAllocThreshold / 4 ?
            stackalloc rune[estimatedLength] :
            new rune[estimatedLength];

        int runesDecoded = DecodeRunes(runes);
        return runes[..runesDecoded].ToArray();
    }

    private int DecodeRunes(Span<rune> runes)
    {
        if (m_value.Length == 0)
            return 0;

        int index = 0;
        ReadOnlySpan<byte> bytes = m_value;

        while (!bytes.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf8(bytes, out Rune rune, out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                runes[index++] = rune.Value;
            }
            else
            {
                // When status is InvalidData or NeedMoreData or DestinationTooSmall, follow
                // Go behavior: Replace invalid sequences with Unicode replacement character
                runes[index++] = RuneReplacementChar;
                bytesConsumed = bytesConsumed == 0 ? 1 : bytesConsumed;
            }

            bytes = bytes[bytesConsumed..];
        }

        return index;
    }

    public static @string Default => new("");

    #region [ Operators ]

    // Enable implicit conversions between string and @string struct
    public static implicit operator @string(string value)
    {
        return new @string(value);
    }

    public static implicit operator string(@string value)
    {
        return value.ToString();
    }

    public static implicit operator @string(ReadOnlySpan<byte> value)
    {
        return new @string(value);
    }

    public static implicit operator @string(slice<byte> value)
    {
        return new @string(value);
    }

    public static implicit operator slice<byte>(@string value)
    {
        return new slice<byte>(value.m_value);
    }

    public static implicit operator @string(slice<rune> value)
    {
        return new @string(value);
    }

    public static implicit operator slice<rune>(@string value)
    {
        return new slice<rune>(((IEnumerable<rune>)value).ToArray());
    }

    public static implicit operator @string(rune value)
    {
        return new @string([value]);
    }

    public static explicit operator rune(@string value)
    {
        return value.ToRunes().FirstOrDefault();
    }

    public static implicit operator @string(slice<char> value)
    {
        return new @string(value);
    }

    public static implicit operator slice<char>(@string value)
    {
        return new slice<char>(((IEnumerable<char>)value).ToArray());
    }

    public static implicit operator byte[](@string value)
    {
        return value.m_value;
    }

    public static implicit operator @string(byte[] value)
    {
        return new @string(value);
    }

    public static implicit operator rune[](@string value)
    {
        return value.ToRunes();
    }

    public static implicit operator @string(rune[] value)
    {
        return new @string(new ReadOnlySpan<rune>(value));
    }

    public static implicit operator ReadOnlySpan<rune>(@string value)
    {
        return value.ToRunes();
    }

    public static implicit operator @string(ReadOnlySpan<rune> value)
    {
        return new @string(value);
    }

    public static explicit operator char[](@string value)
    {
        return ((IEnumerable<char>)value).ToArray();
    }

    public static implicit operator @string(char[] value)
    {
        return new @string(value);
    }

    // Enable comparisons between nil and @string struct
    public static bool operator ==(@string value, NilType _)
    {
        return value.Equals(Default);
    }

    public static bool operator !=(@string value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, @string value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, @string value)
    {
        return value != nil;
    }

    // Enable @string to @string comparisons
    public static implicit operator @string(NilType _)
    {
        return Default;
    }

    public static bool operator ==(@string a, @string b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(@string a, @string b)
    {
        return !a.Equals(b);
    }

    public static bool operator <(@string a, @string b)
    {
        return string.CompareOrdinal(a, b) < 0;
    }

    public static bool operator <=(@string a, @string b)
    {
        return string.CompareOrdinal(a, b) <= 0;
    }

    public static bool operator >(@string a, @string b)
    {
        return string.CompareOrdinal(a, b) > 0;
    }

    public static bool operator >=(@string a, @string b)
    {
        return string.CompareOrdinal(a, b) >= 0;
    }

    public static @string operator +(@string a, @string b)
    {
        byte[] bytes = new byte[a.m_value.Length + b.m_value.Length];

        Buffer.BlockCopy(a.m_value, 0, bytes, 0, a.m_value.Length);
        Buffer.BlockCopy(b.m_value, 0, bytes, a.m_value.Length, b.m_value.Length);

        return new @string(bytes);
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        return Clone();
    }

    int IReadOnlyCollection<byte>.Count => Length;

    bool IConvertible.ToBoolean(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToBoolean(provider);
    }

    char IConvertible.ToChar(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToChar(provider);
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToSByte(provider);
    }

    byte IConvertible.ToByte(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToByte(provider);
    }

    short IConvertible.ToInt16(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt16(provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt16(provider);
    }

    int IConvertible.ToInt32(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt32(provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt32(provider);
    }

    long IConvertible.ToInt64(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt64(provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt64(provider);
    }

    float IConvertible.ToSingle(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToSingle(provider);
    }

    double IConvertible.ToDouble(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDouble(provider);
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDecimal(provider);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDateTime(provider);
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToType(conversionType, provider);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_value.GetEnumerator();
    }

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        foreach (byte item in m_value)
            yield return item;
    }

    IEnumerator<rune> IEnumerable<rune>.GetEnumerator()
    {
        foreach (rune codePoint in ToRunes())
            yield return codePoint;
    }

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        return ToString().GetEnumerator();
    }

    private static bool BytesAreEqual(byte[] data1, byte[] data2)
    {
        if (data1 == data2)
            return true;

        return data1.Length == data2.Length && new ReadOnlySpan<byte>(data1).SequenceEqual(new ReadOnlySpan<byte>(data2));
    }

    private const rune RuneReplacementChar = 0xFFFD;

    #endregion
}
