// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package edit implements buffered position-based editing of byte slices.
// package edit -- go2cs converted at 2022 March 06 22:47:22 UTC
// import "cmd/internal/edit" ==> using edit = go.cmd.@internal.edit_package
// Original source: C:\Program Files\Go\src\cmd\internal\edit\edit.go
using fmt = go.fmt_package;
using sort = go.sort_package;

namespace go.cmd.@internal;

public static partial class edit_package {

    // A Buffer is a queue of edits to apply to a given byte slice.
public partial struct Buffer {
    public slice<byte> old;
    public edits q;
}

// An edit records a single text modification: change the bytes in [start,end) to new.
private partial struct edit {
    public nint start;
    public nint end;
    public @string @new;
}

// An edits is a list of edits that is sortable by start offset, breaking ties by end offset.
private partial struct edits { // : slice<edit>
}

private static nint Len(this edits x) {
    return len(x);
}
private static void Swap(this edits x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}
private static bool Less(this edits x, nint i, nint j) {
    if (x[i].start != x[j].start) {
        return x[i].start < x[j].start;
    }
    return x[i].end < x[j].end;

}

// NewBuffer returns a new buffer to accumulate changes to an initial data slice.
// The returned buffer maintains a reference to the data, so the caller must ensure
// the data is not modified until after the Buffer is done being used.
public static ptr<Buffer> NewBuffer(slice<byte> data) {
    return addr(new Buffer(old:data));
}

private static void Insert(this ptr<Buffer> _addr_b, nint pos, @string @new) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    if (pos < 0 || pos > len(b.old)) {
        panic("invalid edit position");
    }
    b.q = append(b.q, new edit(pos,pos,new));

});

private static void Delete(this ptr<Buffer> _addr_b, nint start, nint end) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    if (end < start || start < 0 || end > len(b.old)) {
        panic("invalid edit position");
    }
    b.q = append(b.q, new edit(start,end,""));

});

private static void Replace(this ptr<Buffer> _addr_b, nint start, nint end, @string @new) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    if (end < start || start < 0 || end > len(b.old)) {
        panic("invalid edit position");
    }
    b.q = append(b.q, new edit(start,end,new));

});

// Bytes returns a new byte slice containing the original data
// with the queued edits applied.
private static slice<byte> Bytes(this ptr<Buffer> _addr_b) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;
 
    // Sort edits by starting position and then by ending position.
    // Breaking ties by ending position allows insertions at point x
    // to be applied before a replacement of the text at [x, y).
    sort.Stable(b.q);

    slice<byte> @new = default;
    nint offset = 0;
    foreach (var (i, e) in b.q) {
        if (e.start < offset) {
            var e0 = b.q[i - 1];
            panic(fmt.Sprintf("overlapping edits: [%d,%d)->%q, [%d,%d)->%q", e0.start, e0.end, e0.@new, e.start, e.end, e.@new));
        }
        new = append(new, b.old[(int)offset..(int)e.start]);
        offset = e.end;
        new = append(new, e.@new);

    }    new = append(new, b.old[(int)offset..]);
    return new;

});

// String returns a string containing the original data
// with the queued edits applied.
private static @string String(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    return string(b.Bytes());
}

} // end edit_package
