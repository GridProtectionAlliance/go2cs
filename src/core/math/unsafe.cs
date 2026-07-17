// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted unsafe.go output). Go implements the
// Float32/Float64 bits conversions as raw pointer reinterpretations — *(*uint32)(unsafe.Pointer(&f)) — a
// pure same-size bit cast. The literal conversion renders that as a ж<T>/uintptr round-trip that compiles
// but cannot reinterpret the bits at runtime (it boxes a copy and casts the box handle, not the value).
// .NET provides the exact operation directly: the BitConverter bit-cast intrinsics preserve every bit —
// sign of zero, NaN payload and signaling bit, subnormals — with no byte-array staging and no endianness
// dependence (a value-to-value cast, not a memory serialization). The float32 conversions use the Single
// variants exclusively so no value ever transits double, which could quiet a signaling NaN.
using System;

// Hand-owned native replacement of the converted unsafe.go output — the converter skips regenerating a
// file that carries this marker, so a -stdlib reconvert preserves it (see containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class math_package {

// Despite being an exported symbol,
// Float32bits is linknamed by widely used packages.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/num
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
// Note that this comment is not part of the doc comment.
//
//go:linkname Float32bits

// Float32bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position.
// Float32bits(Float32frombits(x)) == x.
public static uint32 Float32bits(float32 f) => BitConverter.SingleToUInt32Bits(f);

// Float32frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float32frombits(Float32bits(x)) == x.
public static float32 Float32frombits(uint32 b) => BitConverter.UInt32BitsToSingle(b);

// Float64bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position,
// and Float64bits(Float64frombits(x)) == x.
public static uint64 Float64bits(float64 f) => BitConverter.DoubleToUInt64Bits(f);

// Float64frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float64frombits(Float64bits(x)) == x.
public static float64 Float64frombits(uint64 b) => BitConverter.UInt64BitsToDouble(b);

} // end math_package
