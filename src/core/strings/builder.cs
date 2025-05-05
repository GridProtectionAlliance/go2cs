// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using utf8 = unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using unicode;

partial class strings_package {

// A Builder is used to efficiently build a string using [Builder.Write] methods.
// It minimizes memory copying. The zero value is ready to use.
// Do not copy a non-zero Builder.
[GoType] partial struct Builder {
    internal ж<Builder> addr; // of receiver, to detect copies by value
    // External users should never get direct access to this buffer, since
    // the slice at some point will be converted to a string using unsafe, also
    // data between len(buf) and cap(buf) might be uninitialized.
    internal slice<byte> buf;
}

[GoRecv] internal static void copyCheck(this ref Builder b) {
    if (b.addr == nil){
        // This hack works around a failing of Go's escape analysis
        // that was causing b to escape and be heap allocated.
        // See issue 23382.
        // TODO: once issue 7921 is fixed, this should be reverted to
        // just "b.addr = b".
        b.addr = (ж<Builder>)(uintptr)(abi.NoEscape((uintptr)@unsafe.Pointer.FromRef(ref b)));
    } else 
    if (b.addr != b) {
        throw panic("strings: illegal use of non-zero Builder copied by value");
    }
}

// String returns the accumulated string.
[GoRecv] public static @string String(this ref Builder b) {
    return @unsafe.String(@unsafe.SliceData(b.buf), len(b.buf));
}

// Len returns the number of accumulated bytes; b.Len() == len(b.String()).
[GoRecv] public static nint Len(this ref Builder b) {
    return len(b.buf);
}

// Cap returns the capacity of the builder's underlying byte slice. It is the
// total space allocated for the string being built and includes any bytes
// already written.
[GoRecv] public static nint Cap(this ref Builder b) {
    return cap(b.buf);
}

// Reset resets the [Builder] to be empty.
[GoRecv] public static void Reset(this ref Builder b) {
    b.addr = default!;
    b.buf = default!;
}

// grow copies the buffer to a new, larger buffer so that there are at least n
// bytes of capacity beyond len(b.buf).
[GoRecv] internal static void grow(this ref Builder b, nint n) {
    var buf = bytealg.MakeNoZero(2 * cap(b.buf) + n)[..(int)(len(b.buf))];
    copy(buf, b.buf);
    b.buf = buf;
}

// Grow grows b's capacity, if necessary, to guarantee space for
// another n bytes. After Grow(n), at least n bytes can be written to b
// without another allocation. If n is negative, Grow panics.
[GoRecv] public static void Grow(this ref Builder b, nint n) {
    b.copyCheck();
    if (n < 0) {
        throw panic("strings.Builder.Grow: negative count");
    }
    if (cap(b.buf) - len(b.buf) < n) {
        b.grow(n);
    }
}

// Write appends the contents of p to b's buffer.
// Write always returns len(p), nil.
[GoRecv] public static (nint, error) Write(this ref Builder b, slice<byte> p) {
    b.copyCheck();
    b.buf = append(b.buf, p.ꓸꓸꓸ);
    return (len(p), default!);
}

// WriteByte appends the byte c to b's buffer.
// The returned error is always nil.
[GoRecv] public static error WriteByte(this ref Builder b, byte c) {
    b.copyCheck();
    b.buf = append(b.buf, c);
    return default!;
}

// WriteRune appends the UTF-8 encoding of Unicode code point r to b's buffer.
// It returns the length of r and a nil error.
[GoRecv] public static (nint, error) WriteRune(this ref Builder b, rune r) {
    b.copyCheck();
    nint n = len(b.buf);
    b.buf = utf8.AppendRune(b.buf, r);
    return (len(b.buf) - n, default!);
}

// WriteString appends the contents of s to b's buffer.
// It returns the length of s and a nil error.
[GoRecv] public static (nint, error) WriteString(this ref Builder b, @string s) {
    b.copyCheck();
    b.buf = append(b.buf, s.ꓸꓸꓸ);
    return (len(s), default!);
}

} // end strings_package
