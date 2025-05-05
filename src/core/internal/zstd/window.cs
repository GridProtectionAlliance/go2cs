// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class zstd_package {

// window stores up to size bytes of data.
// It is implemented as a circular buffer:
// sequential save calls append to the data slice until
// its length reaches configured size and after that,
// save calls overwrite previously saved data at off
// and update off such that it always points at
// the byte stored before others.
[GoType] partial struct window {
    internal nint size;
    internal slice<byte> data;
    internal nint off;
}

// reset clears stored data and configures window size.
[GoRecv] internal static void reset(this ref window w, nint size) {
    var b = w.data[..0];
    if (cap(b) < size) {
        b = new slice<byte>(0, size);
    }
    w.data = b;
    w.off = 0;
    w.size = size;
}

// len returns the number of stored bytes.
[GoRecv] internal static uint32 len(this ref window w) {
    return ((uint32)len(w.data));
}

// save stores up to size last bytes from the buf.
[GoRecv] internal static void save(this ref window w, slice<byte> buf) {
    if (w.size == 0) {
        return;
    }
    if (len(buf) == 0) {
        return;
    }
    if (len(buf) >= w.size) {
        nint from = len(buf) - w.size;
        w.data = append(w.data[..0], buf[(int)(from)..].ꓸꓸꓸ);
        w.off = 0;
        return;
    }
    // Update off to point to the oldest remaining byte.
    nint free = w.size - len(w.data);
    if (free == 0){
        nint n = copy(w.data[(int)(w.off)..], buf);
        if (n == len(buf)){
            w.off += n;
        } else {
            w.off = copy(w.data, buf[(int)(n)..]);
        }
    } else {
        if (free >= len(buf)){
            w.data = append(w.data, buf.ꓸꓸꓸ);
        } else {
            w.data = append(w.data, buf[..(int)(free)].ꓸꓸꓸ);
            w.off = copy(w.data, buf[(int)(free)..]);
        }
    }
}

// appendTo appends stored bytes between from and to indices to the buf.
// Index from must be less or equal to index to and to must be less or equal to w.len().
[GoRecv] internal static slice<byte> appendTo(this ref window w, slice<byte> buf, uint32 from, uint32 to) {
    var dataLen = ((uint32)len(w.data));
    from += ((uint32)w.off);
    to += ((uint32)w.off);
    var wrap = false;
    if (from > dataLen) {
        from -= dataLen;
        wrap = !wrap;
    }
    if (to > dataLen) {
        to -= dataLen;
        wrap = !wrap;
    }
    if (wrap){
        buf = append(buf, w.data[(int)(from)..].ꓸꓸꓸ);
        return append(buf, w.data[..(int)(to)].ꓸꓸꓸ);
    } else {
        return append(buf, w.data[(int)(from)..(int)(to)].ꓸꓸꓸ);
    }
}

} // end zstd_package
