// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using math = runtime.@internal.math_package;
using @unsafe = unsafe_package;
using runtime.@internal;

partial class runtime_package {

internal static void unsafestring(@unsafe.Pointer ptr, nint len) {
    if (len < 0) {
        panicunsafestringlen();
    }
    if (((uintptr)len) > -((uintptr)ptr)) {
        if (ptr == nil) {
            panicunsafestringnilptr();
        }
        panicunsafestringlen();
    }
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeString
internal static void unsafestring64(@unsafe.Pointer ptr, int64 len64) {
    nint len = ((nint)len64);
    if (((int64)len) != len64) {
        panicunsafestringlen();
    }
    unsafestring(ptr.val, len);
}

internal static void unsafestringcheckptr(@unsafe.Pointer ptr, int64 len64) {
    unsafestring64(ptr.val, len64);
    // Check that underlying array doesn't straddle multiple heap objects.
    // unsafestring64 has already checked for overflow.
    if (checkptrStraddles(ptr.val, ((uintptr)len64))) {
        @throw("checkptr: unsafe.String result straddles multiple allocations"u8);
    }
}

internal static void panicunsafestringlen() {
    throw panic(((errorString)"unsafe.String: len out of range"u8));
}

internal static void panicunsafestringnilptr() {
    throw panic(((errorString)"unsafe.String: ptr is nil and len is not zero"u8));
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeSlice
internal static void unsafeslice(ж<_type> Ꮡet, @unsafe.Pointer ptr, nint len) {
    ref var et = ref Ꮡet.val;

    if (len < 0) {
        panicunsafeslicelen1(getcallerpc());
    }
    if (et.Size_ == 0) {
        if (ptr == nil && len > 0) {
            panicunsafeslicenilptr1(getcallerpc());
        }
    }
    var (mem, overflow) = math.MulUintptr(et.Size_, ((uintptr)len));
    if (overflow || mem > -((uintptr)ptr)) {
        if (ptr == nil) {
            panicunsafeslicenilptr1(getcallerpc());
        }
        panicunsafeslicelen1(getcallerpc());
    }
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeSlice
internal static void unsafeslice64(ж<_type> Ꮡet, @unsafe.Pointer ptr, int64 len64) {
    ref var et = ref Ꮡet.val;

    nint len = ((nint)len64);
    if (((int64)len) != len64) {
        panicunsafeslicelen1(getcallerpc());
    }
    unsafeslice(Ꮡet, ptr.val, len);
}

internal static void unsafeslicecheckptr(ж<_type> Ꮡet, @unsafe.Pointer ptr, int64 len64) {
    ref var et = ref Ꮡet.val;

    unsafeslice64(Ꮡet, ptr.val, len64);
    // Check that underlying array doesn't straddle multiple heap objects.
    // unsafeslice64 has already checked for overflow.
    if (checkptrStraddles(ptr.val, ((uintptr)len64) * et.Size_)) {
        @throw("checkptr: unsafe.Slice result straddles multiple allocations"u8);
    }
}

internal static void panicunsafeslicelen() {
    // This is called only from compiler-generated code, so we can get the
    // source of the panic.
    panicunsafeslicelen1(getcallerpc());
}

//go:yeswritebarrierrec
internal static void panicunsafeslicelen1(uintptr pc) {
    panicCheck1(pc, "unsafe.Slice: len out of range"u8);
    throw panic(((errorString)"unsafe.Slice: len out of range"u8));
}

internal static void panicunsafeslicenilptr() {
    // This is called only from compiler-generated code, so we can get the
    // source of the panic.
    panicunsafeslicenilptr1(getcallerpc());
}

//go:yeswritebarrierrec
internal static void panicunsafeslicenilptr1(uintptr pc) {
    panicCheck1(pc, "unsafe.Slice: ptr is nil and len is not zero"u8);
    throw panic(((errorString)"unsafe.Slice: ptr is nil and len is not zero"u8));
}

//go:linkname reflect_unsafeslice reflect.unsafeslice
internal static void reflect_unsafeslice(ж<_type> Ꮡet, @unsafe.Pointer ptr, nint len) {
    ref var et = ref Ꮡet.val;

    unsafeslice(Ꮡet, ptr.val, len);
}

} // end runtime_package
