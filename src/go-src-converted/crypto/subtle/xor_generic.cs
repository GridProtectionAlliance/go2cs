// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !ppc64 && !ppc64le) || purego
namespace go.crypto;

using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class subtle_package {

internal static readonly uintptr wordSize = /* unsafe.Sizeof(uintptr(0)) */ 8;

internal const bool supportsUnaligned = /* runtime.GOARCH == "386" ||
	runtime.GOARCH == "amd64" ||
	runtime.GOARCH == "ppc64" ||
	runtime.GOARCH == "ppc64le" ||
	runtime.GOARCH == "s390x" */ true;

internal static void xorBytes(ж<byte> Ꮡdstb, ж<byte> Ꮡxb, ж<byte> Ꮡyb, nint n) {
    // xorBytes assembly is written using pointers and n. Back to slices.
    var dst = @unsafe.Slice(Ꮡdstb, n);
    var x = @unsafe.Slice(Ꮡxb, n);
    var y = @unsafe.Slice(Ꮡyb, n);
    if (supportsUnaligned || aligned(Ꮡdstb, Ꮡxb, Ꮡyb)) {
        xorLoop(words(dst), words(x), words(y));
        if ((uintptr)n % wordSize == 0) {
            return;
        }
        nint done = (nint)(n & ~(nint)(nint)(wordSize - 1));
        dst = dst[(int)(done)..];
        x = x[(int)(done)..];
        y = y[(int)(done)..];
    }
    xorLoop(dst, x, y);
}

// aligned reports whether dst, x, and y are all word-aligned pointers.
internal static bool aligned(ж<byte> Ꮡdst, ж<byte> Ꮡx, ж<byte> Ꮡy) {
    return (uintptr)(((uintptr)((uintptr)((uintptr)new @unsafe.Pointer(Ꮡdst) | (uintptr)new @unsafe.Pointer(Ꮡx)) | (uintptr)new @unsafe.Pointer(Ꮡy))) & (uintptr)(wordSize - 1)) == 0;
}

// words returns a []uintptr pointing at the same data as x,
// with any trailing partial word removed.
internal static slice<uintptr> words(slice<byte> x) {
    var n = (uintptr)len(x) / wordSize;
    if (n == 0) {
        // Avoid creating a *uintptr that refers to data smaller than a uintptr;
        // see issue 59334.
        return default!;
    }
    return @unsafe.Slice((ж<uintptr>)(uintptr)(new @unsafe.Pointer(Ꮡ(x, 0))), n);
}

internal static void xorLoop<T>(slice<T> dst, slice<T> x, slice<T> y)
    where T : /* byte | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    x = x[..(int)(len(dst))];
    // remove bounds check in loop
    y = y[..(int)(len(dst))];
    // remove bounds check in loop
    foreach (var (i, _) in dst) {
        dst[i] = (T)(x[i] ^ y[i]);
    }
}

} // end subtle_package
