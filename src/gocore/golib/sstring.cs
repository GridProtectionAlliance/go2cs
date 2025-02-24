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
// ReSharper disable BuiltInTypeReferenceStyle

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using go.runtime;

namespace go;

// This is an experiment with a stack allocated string type which should improve performance
// of string operations by avoiding heap allocations. This works fine in stack oriented code
// operations, but any time heap escape is needed, @string will need to be used. Added a
// `str` function to `builtin` to do the conversion from @string to sstring, but this would
// require proper heap type parameter and heap escape detection during the conversion
// process. This is a work in progress. See `EXPERIMENTAL` region which controls implicit
// vs. explicit control of ReadOnlySpan<byte> conversions, compared to what is defined in
// heap allocated `@string` implementation. When `EXPERIMENTAL` preprocessor directive is
// defined, `sstring` can be made to take precedence for u8 string handling.

// Go automatically allows heap escape, converting things from stack to heap as needed, but
// this is not the case in .NET, here stack only types, e.g., `ref struct`, are very explicit
// and compiler constrained.

// Although this is only an experiment with strings, a similar approach may be possible for
// a small subset of other types, see note below on restrictions.

// --- Note ---
// The key difference between struct and ref struct is that ref struct types are forced to
// be stack only, i.e., they cannot be boxed and stored on the heap. In general this prevents
// unnecessary allocations which speeds processing and removes GC burden. Keeping the type on
// the stack is enforced by .NET compiler -- the type cannot escape to the heap. This means
// types cannot be stored in the `object` type, be a field in a class or common struct, etc.
// In context of Go to C# conversions, in order to make use of these types, stack to heap
// escapes which happen implicitly in Go will have to be detected during the conversion
// process and managed explicitly in converted C# code. New Go based go2cs converter already
// detects heap escapes, so it can be more easily modified to handle types like this one.
//
// In general all ref struct restrictions would apply to converted Go struct types which may
// make "general" use very impractical, the key pain points specifically are:
// * ref struct can't be the element type of an array:
//      This means a slice, array or map of type would not be allowed
// * ref struct can't be boxed to System.ValueType or System.Object:
//      This means type cannot be stored in interface{} value, headaches.

/// <summary>
/// Represents a stack only structure that behaves like a Go string.
/// </summary>
#if NET9_0_OR_GREATER
public readonly ref struct sstring : IConvertible, IEquatable<sstring>, IComparable<sstring>, IReadOnlyList<byte>, IEnumerable<rune>, IEnumerable<(nint, rune)>, IEnumerable<char>, ICloneable
#else
public readonly ref struct sstring
#endif
{
    internal readonly ReadOnlySpan<byte> m_value;

    public sstring()
    {
        m_value = [];
    }

    public sstring(byte[]? bytes)
    {
        m_value = bytes ?? [];
    }

    public sstring(ReadOnlySpan<byte> bytes)
    {
        m_value = bytes;
    }

    public sstring(char[] value) : this(new string(value)) { }

    public sstring(rune[] value) : this(new string(value.Select(rune => (char)rune).ToArray())) { }

    public sstring(in slice<byte> value) : this(value.ToArray()) { }

    public sstring(in slice<char> value) : this(value.ToArray()) { }

    public sstring(in slice<rune> value) : this(value.ToArray()) { }

    public sstring(string? value)
    {
        m_value = Encoding.UTF8.GetBytes(value ?? "");
    }

    public sstring(sstring value) : this(value.m_value) { }

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

    public byte this[nint index] => this[(int)index];

    public byte this[ulong index] => this[(int)index];

    public slice<byte> this[Range range] => new(m_value.ToArray(), range.Start.GetOffset(m_value.Length), range.End.GetOffset(m_value.Length));

    public slice<byte> Slice(int start, int length)
    {
        return new slice<byte>(m_value, start, start + length);
    }

    public slice<byte> Slice(nint start, nint length)
    {
        return new slice<byte>(m_value, (int)start, (int)(start + length));
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(m_value);
    }

    public bool Equals(sstring other)
    {
        return BytesAreEqual(m_value, other.m_value);
    }

    public int CompareTo(sstring other)
    {
        return StringComparer.Ordinal.Compare(ToString(), other.ToString());
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

    public sstring Clone()
    {
        return new sstring(this);
    }

    public IEnumerator<(nint, rune)> GetEnumerator()
    {
        return GetEnumerator(DecodeRunes());
    }

    private char[] DecodeRunes()
    {
        if (m_value.Length == 0)
            return [];

        Decoder decoder = Encoding.UTF8.GetDecoder();
        Span<char> chars = new char[m_value.Length];

        decoder.Convert(m_value, chars, true, out _, out int used, out bool completed);

        if (!completed)
            chars[used++] = '\uFFFD';

        return chars[..used].ToArray();
    }

    #region [ Operators ]

    // Enable implicit conversions between string and sstring struct
    public static implicit operator sstring(string value)
    {
        return new sstring(value);
    }

    public static implicit operator sstring(@string value)
    {
        return new sstring(value.m_value);
    }

    public static implicit operator string(sstring value)
    {
        return value.ToString();
    }

    public static implicit operator @string(sstring value)
    {
        return new @string(value.m_value);
    }

#if EXPERIMENTAL

    public static implicit operator sstring(ReadOnlySpan<byte> value) => new(value);

#else
        
    public static explicit operator sstring(ReadOnlySpan<byte> value)
    {
        return new sstring(value);
    }

#endif

    public static implicit operator sstring(in slice<byte> value)
    {
        return new sstring(value);
    }

    public static implicit operator slice<byte>(sstring value)
    {
        return new slice<byte>(value.m_value);
    }

    public static implicit operator sstring(slice<rune> value)
    {
        return new sstring(value.ToArray());
    }

    public static implicit operator slice<rune>(sstring value)
    {
        return new slice<rune>(value.ToString().Select(ch => (rune)ch).ToArray());
    }

    public static implicit operator sstring(in slice<char> value)
    {
        return new sstring(value);
    }

    public static implicit operator slice<char>(sstring value)
    {
        return new slice<char>(value.ToString().ToCharArray());
    }

    public static explicit operator byte[](sstring value)
    {
        return value.m_value.ToArray();
    }

    public static explicit operator ReadOnlySpan<byte>(sstring value)
    {
        return value.m_value;
    }

    public static implicit operator sstring(byte[] value)
    {
        return new sstring(value);
    }

    public static implicit operator rune[](sstring value)
    {
        return value.ToString().Select(ch => (rune)ch).ToArray();
    }

    public static implicit operator sstring(rune[] value)
    {
        return new sstring(value);
    }

    public static explicit operator char[](sstring value)
    {
        return value.ToString().ToCharArray();
    }

    public static implicit operator sstring(char[] value)
    {
        return new sstring(value);
    }

    // Enable comparisons between nil and sstring struct
    public static bool operator ==(sstring value, NilType _)
    {
        return value.Equals(default);
    }

    public static bool operator !=(sstring value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, sstring value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, sstring value)
    {
        return value != nil;
    }

    // Enable sstring to sstring comparisons
    public static implicit operator sstring(NilType _)
    {
        return new sstring();
    }

    public static bool operator ==(sstring a, sstring b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(sstring a, sstring b)
    {
        return !a.Equals(b);
    }

    public static bool operator <(sstring a, sstring b)
    {
        return string.CompareOrdinal(a, b) < 0;
    }

    public static bool operator <=(sstring a, sstring b)
    {
        return string.CompareOrdinal(a, b) <= 0;
    }

    public static bool operator >(sstring a, sstring b)
    {
        return string.CompareOrdinal(a, b) > 0;
    }

    public static bool operator >=(sstring a, sstring b)
    {
        return string.CompareOrdinal(a, b) >= 0;
    }

    #endregion

    #region [ Interface Implementations ]

    private static IEnumerator<(nint, rune)> GetEnumerator(char[] runes)
    {
        for (int i = 0; i < runes.Length; i++)
            yield return (i, runes[i]);
    }

    private static IEnumerator<rune> GetEnumerator(string value)
    {
        foreach (rune item in value)
            yield return item;
    }

    private static IEnumerator<byte> GetEnumerator(byte[] bytes)
    {
        foreach (byte item in bytes)
            yield return item;
    }

#if NET9_0_OR_GREATER

    object ICloneable.Clone()
    {
        return (@string)Clone();
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
        return GetEnumerator(m_value.ToArray());
    }

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        return GetEnumerator(m_value.ToArray());
    }

    IEnumerator<rune> IEnumerable<rune>.GetEnumerator()
    {
        return GetEnumerator(ToString());
    }

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        return ToString().GetEnumerator();
    }

#endif

    private static unsafe bool BytesAreEqual(in ReadOnlySpan<byte> data1, in ReadOnlySpan<byte> data2)
    {
        if (data1 == data2)
            return true;

        if (data1.Length != data2.Length)
            return false;

        if (data1.Length == 0)
            return true;

        fixed (byte* bytes1 = data1, bytes2 = data2)
        {
            int len = data1.Length;
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
                if (data1[len - 1 - i] != data2[len - 1 - i])
                    return false;

            return true;
        }
    }

    #endregion
}
