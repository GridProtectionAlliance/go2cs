// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2022 March 13 05:23:51 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Program Files\Go\src\strings\builder.go
namespace go;

using utf8 = unicode.utf8_package;
using @unsafe = @unsafe_package;


// A Builder is used to efficiently build a string using Write methods.
// It minimizes memory copying. The zero value is ready to use.
// Do not copy a non-zero Builder.

public static partial class strings_package {

public partial struct Builder {
    public ptr<Builder> addr; // of receiver, to detect copies by value
    public slice<byte> buf;
}

// noescape hides a pointer from escape analysis.  noescape is
// the identity function but escape analysis doesn't think the
// output depends on the input. noescape is inlined and currently
// compiles down to zero instructions.
// USE CAREFULLY!
// This was copied from the runtime; see issues 23382 and 7921.
//go:nosplit
//go:nocheckptr
private static unsafe.Pointer noescape(unsafe.Pointer p) {
    var x = uintptr(p);
    return @unsafe.Pointer(x ^ 0);
}

private static void copyCheck(this ptr<Builder> _addr_b) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;

    if (b.addr == null) { 
        // This hack works around a failing of Go's escape analysis
        // that was causing b to escape and be heap allocated.
        // See issue 23382.
        // TODO: once issue 7921 is fixed, this should be reverted to
        // just "b.addr = b".
        b.addr = (Builder.val)(noescape(@unsafe.Pointer(b)));
    }
    else if (b.addr != b) {
        panic("strings: illegal use of non-zero Builder copied by value");
    }
});

// String returns the accumulated string.
private static @string String(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    return new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_b.buf));
}

// Len returns the number of accumulated bytes; b.Len() == len(b.String()).
private static nint Len(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    return len(b.buf);
}

// Cap returns the capacity of the builder's underlying byte slice. It is the
// total space allocated for the string being built and includes any bytes
// already written.
private static nint Cap(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    return cap(b.buf);
}

// Reset resets the Builder to be empty.
private static void Reset(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    b.addr = null;
    b.buf = null;
}

// grow copies the buffer to a new, larger buffer so that there are at least n
// bytes of capacity beyond len(b.buf).
private static void grow(this ptr<Builder> _addr_b, nint n) {
    ref Builder b = ref _addr_b.val;

    var buf = make_slice<byte>(len(b.buf), 2 * cap(b.buf) + n);
    copy(buf, b.buf);
    b.buf = buf;
}

// Grow grows b's capacity, if necessary, to guarantee space for
// another n bytes. After Grow(n), at least n bytes can be written to b
// without another allocation. If n is negative, Grow panics.
private static void Grow(this ptr<Builder> _addr_b, nint n) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;

    b.copyCheck();
    if (n < 0) {
        panic("strings.Builder.Grow: negative count");
    }
    if (cap(b.buf) - len(b.buf) < n) {
        b.grow(n);
    }
});

// Write appends the contents of p to b's buffer.
// Write always returns len(p), nil.
private static (nint, error) Write(this ptr<Builder> _addr_b, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref Builder b = ref _addr_b.val;

    b.copyCheck();
    b.buf = append(b.buf, p);
    return (len(p), error.As(null!)!);
}

// WriteByte appends the byte c to b's buffer.
// The returned error is always nil.
private static error WriteByte(this ptr<Builder> _addr_b, byte c) {
    ref Builder b = ref _addr_b.val;

    b.copyCheck();
    b.buf = append(b.buf, c);
    return error.As(null!)!;
}

// WriteRune appends the UTF-8 encoding of Unicode code point r to b's buffer.
// It returns the length of r and a nil error.
private static (nint, error) WriteRune(this ptr<Builder> _addr_b, int r) {
    nint _p0 = default;
    error _p0 = default!;
    ref Builder b = ref _addr_b.val;

    b.copyCheck(); 
    // Compare as uint32 to correctly handle negative runes.
    if (uint32(r) < utf8.RuneSelf) {
        b.buf = append(b.buf, byte(r));
        return (1, error.As(null!)!);
    }
    var l = len(b.buf);
    if (cap(b.buf) - l < utf8.UTFMax) {
        b.grow(utf8.UTFMax);
    }
    var n = utf8.EncodeRune(b.buf[(int)l..(int)l + utf8.UTFMax], r);
    b.buf = b.buf[..(int)l + n];
    return (n, error.As(null!)!);
}

// WriteString appends the contents of s to b's buffer.
// It returns the length of s and a nil error.
private static (nint, error) WriteString(this ptr<Builder> _addr_b, @string s) {
    nint _p0 = default;
    error _p0 = default!;
    ref Builder b = ref _addr_b.val;

    b.copyCheck();
    b.buf = append(b.buf, s);
    return (len(s), error.As(null!)!);
}

} // end strings_package
