// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 13 05:29:16 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\protobuf.go
namespace go.runtime;

public static partial class pprof_package {

// A protobuf is a simple protocol buffer encoder.
private partial struct protobuf {
    public slice<byte> data;
    public array<byte> tmp;
    public nint nest;
}

private static void varint(this ptr<protobuf> _addr_b, ulong x) {
    ref protobuf b = ref _addr_b.val;

    while (x >= 128) {
        b.data = append(b.data, byte(x) | 0x80);
        x>>=7;
    }
    b.data = append(b.data, byte(x));
}

private static void length(this ptr<protobuf> _addr_b, nint tag, nint len) {
    ref protobuf b = ref _addr_b.val;

    b.varint(uint64(tag) << 3 | 2);
    b.varint(uint64(len));
}

private static void uint64(this ptr<protobuf> _addr_b, nint tag, ulong x) {
    ref protobuf b = ref _addr_b.val;
 
    // append varint to b.data
    b.varint(uint64(tag) << 3 | 0);
    b.varint(x);
}

private static void uint64s(this ptr<protobuf> _addr_b, nint tag, slice<ulong> x) {
    ref protobuf b = ref _addr_b.val;

    if (len(x) > 2) { 
        // Use packed encoding
        var n1 = len(b.data);
        {
            var u__prev1 = u;

            foreach (var (_, __u) in x) {
                u = __u;
                b.varint(u);
            }

            u = u__prev1;
        }

        var n2 = len(b.data);
        b.length(tag, n2 - n1);
        var n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)n2..(int)n3]);
        copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
        copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
        return ;
    }
    {
        var u__prev1 = u;

        foreach (var (_, __u) in x) {
            u = __u;
            b.uint64(tag, u);
        }
        u = u__prev1;
    }
}

private static void uint64Opt(this ptr<protobuf> _addr_b, nint tag, ulong x) {
    ref protobuf b = ref _addr_b.val;

    if (x == 0) {
        return ;
    }
    b.uint64(tag, x);
}

private static void int64(this ptr<protobuf> _addr_b, nint tag, long x) {
    ref protobuf b = ref _addr_b.val;

    var u = uint64(x);
    b.uint64(tag, u);
}

private static void int64Opt(this ptr<protobuf> _addr_b, nint tag, long x) {
    ref protobuf b = ref _addr_b.val;

    if (x == 0) {
        return ;
    }
    b.int64(tag, x);
}

private static void int64s(this ptr<protobuf> _addr_b, nint tag, slice<long> x) {
    ref protobuf b = ref _addr_b.val;

    if (len(x) > 2) { 
        // Use packed encoding
        var n1 = len(b.data);
        {
            var u__prev1 = u;

            foreach (var (_, __u) in x) {
                u = __u;
                b.varint(uint64(u));
            }

            u = u__prev1;
        }

        var n2 = len(b.data);
        b.length(tag, n2 - n1);
        var n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)n2..(int)n3]);
        copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
        copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
        return ;
    }
    {
        var u__prev1 = u;

        foreach (var (_, __u) in x) {
            u = __u;
            b.int64(tag, u);
        }
        u = u__prev1;
    }
}

private static void @string(this ptr<protobuf> _addr_b, nint tag, @string x) {
    ref protobuf b = ref _addr_b.val;

    b.length(tag, len(x));
    b.data = append(b.data, x);
}

private static void strings(this ptr<protobuf> _addr_b, nint tag, slice<@string> x) {
    ref protobuf b = ref _addr_b.val;

    foreach (var (_, s) in x) {
        b.@string(tag, s);
    }
}

private static void stringOpt(this ptr<protobuf> _addr_b, nint tag, @string x) {
    ref protobuf b = ref _addr_b.val;

    if (x == "") {
        return ;
    }
    b.@string(tag, x);
}

private static void @bool(this ptr<protobuf> _addr_b, nint tag, bool x) {
    ref protobuf b = ref _addr_b.val;

    if (x) {
        b.uint64(tag, 1);
    }
    else
 {
        b.uint64(tag, 0);
    }
}

private static void boolOpt(this ptr<protobuf> _addr_b, nint tag, bool x) {
    ref protobuf b = ref _addr_b.val;

    if (x == false) {
        return ;
    }
    b.@bool(tag, x);
}

private partial struct msgOffset { // : nint
}

private static msgOffset startMessage(this ptr<protobuf> _addr_b) {
    ref protobuf b = ref _addr_b.val;

    b.nest++;
    return msgOffset(len(b.data));
}

private static void endMessage(this ptr<protobuf> _addr_b, nint tag, msgOffset start) {
    ref protobuf b = ref _addr_b.val;

    var n1 = int(start);
    var n2 = len(b.data);
    b.length(tag, n2 - n1);
    var n3 = len(b.data);
    copy(b.tmp[..], b.data[(int)n2..(int)n3]);
    copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
    copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
    b.nest--;
}

} // end pprof_package
