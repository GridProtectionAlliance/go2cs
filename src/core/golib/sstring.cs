//******************************************************************************************************
//  sstring.cs - Gbtc
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
//  07/12/2026 - J. Ritchie Carroll
//       Promoted from experimental to a first-class stack string; rewritten for correctness and to
//       shed ref-struct-incompatible surface.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using go.golib;

namespace go;

/// <summary>
/// Represents a stack-only value that behaves like a Go <c>string</c>: a read-only view over a span
/// of UTF-8 bytes (<see cref="ReadOnlySpan{T}"/>) rather than the heap-allocated <see cref="@string"/>.
/// </summary>
/// <remarks>
/// <para>
/// <c>sstring</c> is the stack analog of <see cref="@string"/>. Because it is a <c>ref struct</c> it can
/// live only on the stack — it cannot be boxed, stored in a field/array/map, captured by a lambda, or
/// used as a type argument. Those are exactly the ways a Go string "escapes", so the .NET compiler turns
/// every escape into a compile error, and the go2cs converter emits <c>sstring</c> only for a
/// <c>string([]byte)</c>/<c>string([]rune)</c> conversion whose result provably does not escape and whose
/// source is not mutated while the view is alive. Where a string does escape, the converter emits
/// <see cref="@string"/> instead; the implicit <c>sstring</c>-to-<see cref="@string"/> conversion copies
/// the bytes to the heap at that boundary.
/// </para>
/// <para>
/// Unlike <see cref="@string"/>, whose constructors copy their source, <c>sstring</c> <em>views</em> its
/// source with no allocation — that is the whole point. Correctness of that aliasing is the converter's
/// responsibility (the escape/mutation analysis above), not this type's.
/// </para>
/// <para>
/// Conversions to heap types (<c>[]rune</c>, <c>[]byte</c>, <see cref="@string"/>) produce a detached copy
/// and therefore represent an escape; conversions that would only make sense for an already-escaping value
/// (e.g. rune-slice materialization) are intentionally not provided so such code falls back to
/// <see cref="@string"/>.
/// </para>
/// </remarks>
public readonly ref struct sstring
{
    // A read-only view over UTF-8 bytes. NOT owned/copied: sstring aliases whatever backing the
    // source span points at (a []byte, a u8 literal, an @string's bytes, a stackalloc, ...).
    internal readonly ReadOnlySpan<byte> m_value;

    public sstring()
    {
        m_value = [];
    }

    // Primary constructor: a zero-copy view over the given UTF-8 bytes.
    public sstring(ReadOnlySpan<byte> bytes)
    {
        m_value = bytes;
    }

    public sstring(byte[]? bytes)
    {
        m_value = bytes ?? [];
    }

    // A `[]byte`-to-string conversion. Views the slice's window directly (no copy) — the converter only
    // routes here when it has proven the slice is not mutated for the lifetime of the view.
    public sstring(in slice<byte> value) : this(value.ToSpan()) { }

    // From a C# string (e.g. a string literal). This necessarily allocates the UTF-8 encoding; the view
    // then covers that fresh buffer.
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

            // The bounds check above already validated the index, so read straight through the span's
            // reference and skip the ReadOnlySpan indexer's OWN (now-redundant) bounds check — which
            // Native AOT does not elide, making the plain `m_value[index]` measurably slower there than
            // the equivalent byte[] index. Matches @string's effective cost.
            return Unsafe.Add(ref MemoryMarshal.GetReference(m_value), index);
        }
    }

    public byte this[nint index] => this[(int)index];

    public byte this[ulong index] => this[(int)index];

    // Slicing a Go string yields a string, so the range indexer returns an sstring — a zero-copy
    // sub-view over the same backing span (mirrors @string.this[Range] returning @string, but without
    // the copy, since a stack string never outlives its source).
    public sstring this[Range range] => new(m_value[range]);

    // `[]byte(s[a:b])`: converting a string to a byte slice copies in Go, so materialize a detached
    // slice<byte> over a fresh array rather than aliasing the view.
    public slice<byte> Slice(int start, int length)
    {
        return new slice<byte>(m_value.Slice(start, length).ToArray());
    }

    public slice<byte> Slice(nint start, nint length)
    {
        return Slice((int)start, (int)length);
    }

    public ReadOnlySpan<byte> ToSpan()
    {
        return m_value;
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(m_value);
    }

    // Go compares strings as raw bytes (for valid UTF-8 this is also code-point order), matching
    // @string. SequenceEqual / SequenceCompareTo are SIMD-vectorized on the backing spans.
    public bool Equals(sstring other)
    {
        return m_value.SequenceEqual(other.m_value);
    }

    public int CompareTo(sstring other)
    {
        return m_value.SequenceCompareTo(other.m_value);
    }

    // Byte-based, so an sstring and an @string over the same bytes hash identically. (A ref struct can
    // never be a dictionary key, but keeping this correct and consistent costs nothing.)
    public override int GetHashCode()
    {
        System.HashCode hash = new();
        hash.AddBytes(m_value);
        return hash.ToHashCode();
    }

    // Copying a Go string shares its (immutable) data; the sstring copy shares the same view.
    public sstring Clone()
    {
        return new sstring(this);
    }

    #region [ Operators ]

    // Enable implicit conversions between C# string and sstring.
    public static implicit operator sstring(string value)
    {
        return new sstring(value);
    }

    public static implicit operator string(sstring value)
    {
        return value.ToString();
    }

    // sstring <-> @string. From @string is a zero-copy view (an @string's bytes are immutable, so
    // viewing them is safe); to @string copies the bytes to the heap — this is the escape boundary
    // where a stack string becomes a heap string.
    public static implicit operator sstring(@string value)
    {
        return new sstring(value.m_value);
    }

    public static implicit operator @string(sstring value)
    {
        return new @string(value.m_value);
    }

    // ReadOnlySpan<byte> (most importantly a u8 literal) is EXPLICIT on purpose: an implicit form would
    // clash with @string's implicit ReadOnlySpan<byte> operator and make every "..."u8 literal
    // CS0034-ambiguous. The converter target-types such literals to sstring where it wants them.
    public static explicit operator sstring(ReadOnlySpan<byte> value)
    {
        return new sstring(value);
    }

    public static explicit operator ReadOnlySpan<byte>(sstring value)
    {
        return value.m_value;
    }

    // []byte <-> string. From a byte slice is a zero-copy view (the converter only routes a non-mutated
    // source here); to a byte slice copies, since Go's []byte(string) always copies.
    public static implicit operator sstring(in slice<byte> value)
    {
        return new sstring(value);
    }

    public static implicit operator slice<byte>(sstring value)
    {
        return new slice<byte>(value.m_value.ToArray());
    }

    public static implicit operator sstring(byte[] value)
    {
        return new sstring(value);
    }

    public static explicit operator byte[](sstring value)
    {
        return value.m_value.ToArray();
    }

    // Enable comparisons/assignment against nil, mirroring @string (a Go string's zero value is "").
    public static implicit operator sstring(NilType _)
    {
        return new sstring();
    }

    public static bool operator ==(sstring value, NilType _)
    {
        return value.Length == 0;
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
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(sstring a, sstring b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator >(sstring a, sstring b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(sstring a, sstring b)
    {
        return a.CompareTo(b) >= 0;
    }

    // Comparisons directly against a `ReadOnlySpan<byte>` — most importantly a `u8` string literal
    // (`s == "…"u8`), which is zero-allocation static ROM. These compare the backing spans in place;
    // without them the converter would have to render the literal as a plain C# string and let it
    // convert to sstring (a per-comparison `UTF8.GetBytes` allocation) just to reuse the sstring-vs-
    // sstring operators. The go2cs converter emits stack-string comparisons in this form.
    public static bool operator ==(sstring a, ReadOnlySpan<byte> b)
    {
        return a.m_value.SequenceEqual(b);
    }

    public static bool operator !=(sstring a, ReadOnlySpan<byte> b)
    {
        return !a.m_value.SequenceEqual(b);
    }

    public static bool operator ==(ReadOnlySpan<byte> a, sstring b)
    {
        return a.SequenceEqual(b.m_value);
    }

    public static bool operator !=(ReadOnlySpan<byte> a, sstring b)
    {
        return !a.SequenceEqual(b.m_value);
    }

    public static bool operator <(sstring a, ReadOnlySpan<byte> b)
    {
        return a.m_value.SequenceCompareTo(b) < 0;
    }

    public static bool operator <=(sstring a, ReadOnlySpan<byte> b)
    {
        return a.m_value.SequenceCompareTo(b) <= 0;
    }

    public static bool operator >(sstring a, ReadOnlySpan<byte> b)
    {
        return a.m_value.SequenceCompareTo(b) > 0;
    }

    public static bool operator >=(sstring a, ReadOnlySpan<byte> b)
    {
        return a.m_value.SequenceCompareTo(b) >= 0;
    }

    public static bool operator <(ReadOnlySpan<byte> a, sstring b)
    {
        return a.SequenceCompareTo(b.m_value) < 0;
    }

    public static bool operator <=(ReadOnlySpan<byte> a, sstring b)
    {
        return a.SequenceCompareTo(b.m_value) <= 0;
    }

    public static bool operator >(ReadOnlySpan<byte> a, sstring b)
    {
        return a.SequenceCompareTo(b.m_value) > 0;
    }

    public static bool operator >=(ReadOnlySpan<byte> a, sstring b)
    {
        return a.SequenceCompareTo(b.m_value) >= 0;
    }

    // sstring <-> @string comparisons — a stack string against a heap string, compared byte-ordinal
    // with NO allocation (neither operand is copied). These explicit operators are REQUIRED: without
    // them the comparison is ambiguous, because sstring and @string each convert implicitly to the
    // other, so C# cannot choose sstring-vs-sstring over @string-vs-@string — and the @string route
    // would force a heap COPY of the sstring operand (the very copy the stack string exists to avoid).
    // The go2cs converter emits this form when a non-escaping stack string is compared against a heap
    // string variable (`s == other`), the mixed idiom the literal-only comparison could not cover.
    public static bool operator ==(sstring a, @string b)
    {
        return a.m_value.SequenceEqual(b.m_value);
    }

    public static bool operator !=(sstring a, @string b)
    {
        return !a.m_value.SequenceEqual(b.m_value);
    }

    public static bool operator ==(@string a, sstring b)
    {
        return b.m_value.SequenceEqual(a.m_value);
    }

    public static bool operator !=(@string a, sstring b)
    {
        return !b.m_value.SequenceEqual(a.m_value);
    }

    public static bool operator <(sstring a, @string b)
    {
        return a.m_value.SequenceCompareTo(b.m_value) < 0;
    }

    public static bool operator <=(sstring a, @string b)
    {
        return a.m_value.SequenceCompareTo(b.m_value) <= 0;
    }

    public static bool operator >(sstring a, @string b)
    {
        return a.m_value.SequenceCompareTo(b.m_value) > 0;
    }

    public static bool operator >=(sstring a, @string b)
    {
        return a.m_value.SequenceCompareTo(b.m_value) >= 0;
    }

    public static bool operator <(@string a, sstring b)
    {
        return new ReadOnlySpan<byte>(a.m_value).SequenceCompareTo(b.m_value) < 0;
    }

    public static bool operator <=(@string a, sstring b)
    {
        return new ReadOnlySpan<byte>(a.m_value).SequenceCompareTo(b.m_value) <= 0;
    }

    public static bool operator >(@string a, sstring b)
    {
        return new ReadOnlySpan<byte>(a.m_value).SequenceCompareTo(b.m_value) > 0;
    }

    public static bool operator >=(@string a, sstring b)
    {
        return new ReadOnlySpan<byte>(a.m_value).SequenceCompareTo(b.m_value) >= 0;
    }

    // Concatenation. A Go string concatenation always allocates a fresh result, so joining a stack
    // string with another string yields a heap @string (which may itself escape — only the OPERANDS
    // are stack values). Binding these overloads skips the intermediate @string the sstring operand
    // would otherwise be copied into first (`((@string)view) + b`): the operand spans are block-copied
    // straight into the single result buffer, saving one allocation per concatenation. The converter
    // emits an sstring operand here only when it does not escape and its source is not mutated before
    // the concatenation reads it. Mirrors @string's own `+` overloads (incl. the `u8`-literal span form).
    public static @string operator +(sstring a, @string b)
    {
        return concat(a.m_value, b.m_value);
    }

    public static @string operator +(@string a, sstring b)
    {
        return concat(a.m_value, b.m_value);
    }

    public static @string operator +(sstring a, sstring b)
    {
        return concat(a.m_value, b.m_value);
    }

    public static @string operator +(sstring a, ReadOnlySpan<byte> b)
    {
        return concat(a.m_value, b);
    }

    public static @string operator +(ReadOnlySpan<byte> a, sstring b)
    {
        return concat(a, b.m_value);
    }

    // A concatenation whose OTHER operand is a plain C# `string` — a string literal the converter
    // rendered WITHOUT the `u8` suffix because an object/vararg context (e.g. `panic("…" + string(x))`)
    // suppressed it. Without these explicit overloads `string + sstring` is CS0034-ambiguous, since
    // `string` and `sstring` each convert implicitly to the other (mirrors why the comparison form keeps
    // the literal as `u8`). UTF-8-encoding the C# string matches what its implicit `@string` conversion
    // would have done.
    public static @string operator +(string a, sstring b)
    {
        return concat(Encoding.UTF8.GetBytes(a ?? ""), b.m_value);
    }

    public static @string operator +(sstring a, string b)
    {
        return concat(a.m_value, Encoding.UTF8.GetBytes(b ?? ""));
    }

    private static @string concat(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        byte[] bytes = new byte[a.Length + b.Length];
        a.CopyTo(bytes);
        b.CopyTo(new Span<byte>(bytes, a.Length, b.Length));
        return new @string(bytes);
    }

    #endregion
}
