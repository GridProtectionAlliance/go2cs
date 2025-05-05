// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buffer provides a pool-allocated byte buffer.
namespace go.log.slog.@internal;

using sync = sync_package;

partial class buffer_package {

[GoType("[]byte")] partial struct Buffer;

// Having an initial size gives a dramatic speedup.
internal static sync.Pool bufPool = new sync.Pool(
    New: () => {
        var b = new slice<byte>(0, 1024);
        return ((ж<Buffer>)(Ꮡ(b)));
    }
);

public static ж<Buffer> New() {
    return bufPool.Get()._<Buffer.val>();
}

[GoRecv] public static unsafe void Free(this ref Buffer b) {
    // To reduce peak allocation, return only smaller buffers to the pool.
    static readonly UntypedInt maxBufferSize = /* 16 << 10 */ 16384;
    if (cap(b) <= maxBufferSize) {
        b = new Span<ж<Buffer>>((Buffer**), 0);
        bufPool.Put(b);
    }
}

[GoRecv] public static void Reset(this ref Buffer b) {
    b.SetLen(0);
}

[GoRecv] public static (nint, error) Write(this ref Buffer b, slice<byte> p) {
    b = append(b, p.ꓸꓸꓸ);
    return (len(p), default!);
}

[GoRecv] public static (nint, error) WriteString(this ref Buffer b, @string s) {
    b = append(b, s.ꓸꓸꓸ);
    return (len(s), default!);
}

[GoRecv] public static error WriteByte(this ref Buffer b, byte c) {
    b = append(b, c);
    return default!;
}

[GoRecv] public static @string String(this ref Buffer b) {
    return ((@string)(b));
}

[GoRecv] public static nint Len(this ref Buffer b) {
    return len(b);
}

[GoRecv] public static unsafe void SetLen(this ref Buffer b, nint n) {
    b = new Span<ж<Buffer>>((Buffer**), n);
}

} // end buffer_package
