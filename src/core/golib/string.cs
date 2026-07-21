// string.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
using go.golib;

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
    IAdditionOperators<@string, @string, @string>,
    IByteSeq<byte>
{
    internal readonly byte[] m_value;

    // Null-safe view of the backing bytes: `default(@string)` runs no constructor, so m_value is
    // null; treat that zero value as Go's empty string ("") for all reads (length, index, concat,
    // print, range) instead of throwing NRE. Mirrors the nil-map / nil-slice null-safe approach.
    private byte[] Bytes => m_value ?? [];

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

    public @string(in IArray<byte> value) : this(value.Source) { }

    // From a `string | []byte`-constrained byte sequence (IByteSeq<byte>): a generic body slices
    // such a value and wraps the result in @string (e.g. `string(s[a:b])` in bytealg's Rabin-Karp).
    // A concrete slice<byte>/@string argument still prefers its own more-specific constructor.
    public @string(IByteSeq<byte> value)
    {
        byte[] bytes = new byte[value.Length];

        for (nint i = 0; i < value.Length; i++)
            bytes[i] = value[i];

        m_value = bytes;
    }

    public @string(string? value)
    {
        m_value = Encoding.UTF8.GetBytes(value ?? "");
    }

    public @string(@string value) : this(value.Bytes) { }

    public int Length => Bytes.Length;

    public byte this[int index]
    {
        get
        {
            if (index < 0 || index >= Bytes.Length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, Bytes.Length);

            return Bytes[index];
        }
    }

    public byte this[nint index]
    {
        get
        {
            if (index < 0 || index >= Bytes.Length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, Bytes.Length);

            return Bytes[index];
        }
    }

    public byte this[ulong index] => this[(nint)index];
    
    // Slicing a Go string yields a string (e.g. `s[a:b]`), so the range indexer
    // returns @string. Returning slice<byte> here would break string comparisons
    // (slice<byte> != string) and put a ref-struct-convertible value into tuples.
    public @string this[Range range] => new(new slice<byte>(Bytes, range.Start.GetOffset(Bytes.Length), range.End.GetOffset(Bytes.Length)));

    // IByteSeq<byte> — models Go's `string | []byte` union constraint. The byte indexer
    // (this[nint]) implicitly implements IByteSeq<byte>.this[nint]; Length (int) and the
    // @string range indexer need explicit forms to match the interface's nint/IByteSeq types.
    nint IByteSeq.Length => Bytes.Length;

    IByteSeq<byte> IByteSeq<byte>.this[Range range] => this[range];

    public slice<byte> Slice(int start, int length)
    {
        return new slice<byte>(Bytes, start, start + length);
    }

    public slice<byte> Slice(nint start, nint length)
    {
        return new slice<byte>(Bytes, start, start + length);
    }

    public Span<byte> ToSpan()
    {
        return new Span<byte>(Bytes);
    }

    public Span<byte> ꓸꓸꓸ => ToSpan(); // Spread operator

    internal PinnedBuffer buffer => new(Bytes, Length);

    public override string ToString()
    {
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(Bytes));
    }

    public bool Equals(@string other)
    {
        return BytesAreEqual(Bytes, other.Bytes);
    }

    // Go compares strings as raw bytes; for valid UTF-8 this is also code-point order. Comparing the
    // backing bytes directly avoids transcoding both sides to UTF-16 strings per comparison.
    public int CompareTo(@string other)
    {
        return Bytes.AsSpan().SequenceCompareTo(other.Bytes);
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
        System.HashCode hash = new();
        hash.AddBytes(Bytes);
        return hash.ToHashCode();
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
        return new RuneSpanEnumerator(Bytes);
    }

    private class RuneSpanEnumerator(byte[] bytes) : IEnumerator<(nint, rune)>
    {
        private readonly byte[] m_bytes = bytes;
        private int m_byteIndex;
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
                // Go `for i, r := range s` yields the BYTE index of each rune's first byte, not
                // the rune ordinal — a multi-byte rune advances the next index by its encoded
                // length (unicode/utf8 TestSequencing walks exactly this contract).
                m_current = (m_byteIndex, rune.Value);
            }
            else
            {
                // Invalid sequence: Go yields U+FFFD at the byte's index and advances a SINGLE
                // byte (spec: range and DecodeRune consume one byte per invalid sequence) — never
                // .NET's maximal-subpart consumption, which can swallow several bytes at once.
                m_current = (m_byteIndex, RuneReplacementChar);
                bytesConsumed = 1;
            }

            m_byteIndex += bytesConsumed;
            return true;
        }

        public void Reset()
        {
            m_byteIndex = 0;
            m_current = default;
        }
    }

    public rune[] ToRunes()
    {
        // Estimate the rune length (1 rune per byte as worst case)
        int estimatedLength = Bytes.Length;

        Span<rune> runes = estimatedLength <= StackAllocThreshold / 4 ?
            stackalloc rune[estimatedLength] :
            new rune[estimatedLength];

        int runesDecoded = DecodeRunes(runes);
        return runes[..runesDecoded].ToArray();
    }

    private int DecodeRunes(Span<rune> runes)
    {
        if (Bytes.Length == 0)
            return 0;

        int index = 0;
        ReadOnlySpan<byte> bytes = Bytes;

        while (!bytes.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf8(bytes, out Rune rune, out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                runes[index++] = rune.Value;
            }
            else
            {
                // Invalid sequence: Go's []rune(string) yields one U+FFFD PER INVALID BYTE
                // (same single-byte advance as range/DecodeRune) — never .NET's maximal-subpart
                // consumption, which can swallow several bytes as one replacement.
                runes[index++] = RuneReplacementChar;
                bytesConsumed = 1;
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
        return new slice<byte>(value.Bytes);
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
        // Go: `[]byte(s)` COPIES — strings are immutable and the receiver may freely mutate the
        // result. Returning the backing array let a converted `b := []byte(m.str); b[0] = 0x80`
        // write THROUGH into the string (unicode/utf8's TestDecodeRune corrupted the package's
        // utf8map string table for every later test). Zero-copy read-only access uses sstring
        // views / ToSpan() internally — never this conversion.
        return value.Bytes.ToArray();
    }

    // NOTE: stores WITHOUT copying — every LIVE Go []byte value is a slice<byte> in converted
    // code, so `string(b)` conversions route through the copying slice<byte> constructor above;
    // a raw byte[] reaches this operator only as a freshly allocated array (emitted literals,
    // golib internals), where a defensive copy would be pure waste.
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
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(@string a, @string b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator >(@string a, @string b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(@string a, @string b)
    {
        return a.CompareTo(b) >= 0;
    }

    public static @string operator +(@string a, @string b)
    {
        byte[] bytes = new byte[a.Bytes.Length + b.Bytes.Length];

        Buffer.BlockCopy(a.Bytes, 0, bytes, 0, a.Bytes.Length);
        Buffer.BlockCopy(b.Bytes, 0, bytes, a.Bytes.Length, b.Bytes.Length);

        return new @string(bytes);
    }

    // Concatenation directly against a `ReadOnlySpan<byte>` operand — most importantly a `u8` string
    // literal (`s + "-"u8`), which is zero-allocation static ROM. Binding these overloads avoids the
    // intermediate `@string` the span would otherwise be copied into (via the implicit
    // `@string(ReadOnlySpan<byte>)` conversion) before `operator +(@string, @string)` ran: the
    // literal's bytes are block-copied straight into the single result buffer instead. Only the
    // per-concat path is affected — no change to the far hotter `[]byte`→`@string` conversion path.
    public static @string operator +(@string a, ReadOnlySpan<byte> b)
    {
        byte[] a1 = a.Bytes;
        byte[] bytes = new byte[a1.Length + b.Length];

        Buffer.BlockCopy(a1, 0, bytes, 0, a1.Length);
        b.CopyTo(new Span<byte>(bytes, a1.Length, b.Length));

        return new @string(bytes);
    }

    public static @string operator +(ReadOnlySpan<byte> a, @string b)
    {
        byte[] b1 = b.Bytes;
        byte[] bytes = new byte[a.Length + b1.Length];

        a.CopyTo(new Span<byte>(bytes, 0, a.Length));
        Buffer.BlockCopy(b1, 0, bytes, a.Length, b1.Length);

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
        return Bytes.GetEnumerator();
    }

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        foreach (byte item in Bytes)
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
        return data1 == data2 || new ReadOnlySpan<byte>(data1).SequenceEqual(new ReadOnlySpan<byte>(data2));
    }

    private const rune RuneReplacementChar = 0xFFFD;

    #endregion
}
