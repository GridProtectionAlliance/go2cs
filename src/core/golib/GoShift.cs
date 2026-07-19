//******************************************************************************************************
//  GoShift.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  07/18/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// Go-semantics shift helpers used by the converter when a shift's count is NOT provably within the
/// operand's bit width.
/// </summary>
/// <remarks>
/// Go and C# disagree when a shift count reaches or exceeds the operand's width:
/// <list type="bullet">
/// <item>Go: <c>x &gt;&gt; n</c> / <c>x &lt;&lt; n</c> with <c>n &gt;= width</c> yields 0 for an UNSIGNED
/// or LEFT shift; a SIGNED right shift sign-extends (0 for a non-negative value, -1 for a negative one).</item>
/// <item>C#: the native <c>&gt;&gt;</c>/<c>&lt;&lt;</c> operators MASK the count — <c>n &amp; 63</c> for a
/// 64-bit operand, <c>n &amp; 31</c> for a 32-bit operand, and sub-<c>int</c> operands promote to <c>int</c>
/// so they also mask by <c>&amp; 31</c>. So a native shift silently produces the wrong value once the count
/// can reach the width.</item>
/// </list>
/// The count is taken as a WIDE UNSIGNED <see cref="uint64"/> so its FULL magnitude is compared against the
/// width BEFORE any narrowing — a computed count such as <c>64 - n</c> that unsigned-wraps to a huge value
/// is then correctly seen as <c>&gt;= width</c>. (Go panics on a negative count; that is out of scope here —
/// widened into <see cref="uint64"/> a negative count reads as a huge value and yields 0, no worse than the
/// masked native form it replaces.)
/// </remarks>
public static class GoShift
{
    // ---- 64-bit: uint64 / int64 ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Rsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Lsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Rsh(this int64 x, uint64 n) => n >= 64 ? x >> 63 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Lsh(this int64 x, uint64 n) => n >= 64 ? 0L : x << (int)n;

    // ---- native word: nuint / nint (64-bit on this target) -------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Rsh(this nuint x, uint64 n) => n >= 64 ? (nuint)0 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Lsh(this nuint x, uint64 n) => n >= 64 ? (nuint)0 : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Rsh(this nint x, uint64 n) => n >= 64 ? x >> 63 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Lsh(this nint x, uint64 n) => n >= 64 ? (nint)0 : x << (int)n;

    // ---- uintptr (nuint-backed struct, 64-bit on this target) ----------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Rsh(this uintptr x, uint64 n) => n >= 64 ? default(uintptr) : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Lsh(this uintptr x, uint64 n) => n >= 64 ? default(uintptr) : x << (int)n;

    // ---- 32-bit: uint32 / int32 ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Rsh(this uint32 x, uint64 n) => n >= 32 ? 0U : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Lsh(this uint32 x, uint64 n) => n >= 32 ? 0U : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Rsh(this int32 x, uint64 n) => n >= 32 ? x >> 31 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Lsh(this int32 x, uint64 n) => n >= 32 ? 0 : x << (int)n;

    // ---- sub-int: uint16 / int16 (promote to int in the C# shift, so cast back at the operand width) --

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Rsh(this uint16 x, uint64 n) => n >= 16 ? (uint16)0 : (uint16)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Lsh(this uint16 x, uint64 n) => n >= 16 ? (uint16)0 : (uint16)(x << (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Rsh(this int16 x, uint64 n) => n >= 16 ? (int16)(x >> 15) : (int16)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Lsh(this int16 x, uint64 n) => n >= 16 ? (int16)0 : (int16)(x << (int)n);

    // ---- sub-int: uint8 / int8 -----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Rsh(this uint8 x, uint64 n) => n >= 8 ? (uint8)0 : (uint8)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Lsh(this uint8 x, uint64 n) => n >= 8 ? (uint8)0 : (uint8)(x << (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Rsh(this int8 x, uint64 n) => n >= 8 ? (int8)(x >> 7) : (int8)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Lsh(this int8 x, uint64 n) => n >= 8 ? (int8)0 : (int8)(x << (int)n);
}
