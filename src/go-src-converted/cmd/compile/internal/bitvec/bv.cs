// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bitvec -- go2cs converted at 2022 March 06 22:47:45 UTC
// import "cmd/compile/internal/bitvec" ==> using bitvec = go.cmd.compile.@internal.bitvec_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\bitvec\bv.go
using bits = go.math.bits_package;

using @base = go.cmd.compile.@internal.@base_package;

namespace go.cmd.compile.@internal;

public static partial class bitvec_package {

private static readonly nint wordBits = 32;
private static readonly var wordMask = wordBits - 1;
private static readonly nint wordShift = 5;


// A BitVec is a bit vector.
public partial struct BitVec {
    public int N; // number of bits in vector
    public slice<uint> B; // words holding bits
}

public static BitVec New(int n) {
    var nword = (n + wordBits - 1) / wordBits;
    return new BitVec(n,make([]uint32,nword));
}

public partial struct Bulk {
    public slice<uint> words;
    public int nbit;
    public int nword;
}

public static Bulk NewBulk(int nbit, int count) {
    var nword = (nbit + wordBits - 1) / wordBits;
    var size = int64(nword) * int64(count);
    if (int64(int32(size * 4)) != size * 4) {
        @base.Fatalf("NewBulk too big: nbit=%d count=%d nword=%d size=%d", nbit, count, nword, size);
    }
    return new Bulk(words:make([]uint32,size),nbit:nbit,nword:nword,);

}

private static BitVec Next(this ptr<Bulk> _addr_b) {
    ref Bulk b = ref _addr_b.val;

    BitVec @out = new BitVec(b.nbit,b.words[:b.nword]);
    b.words = b.words[(int)b.nword..];
    return out;
}

public static bool Eq(this BitVec bv1, BitVec bv2) {
    if (bv1.N != bv2.N) {
        @base.Fatalf("bvequal: lengths %d and %d are not equal", bv1.N, bv2.N);
    }
    foreach (var (i, x) in bv1.B) {
        if (x != bv2.B[i]) {
            return false;
        }
    }    return true;

}

public static void Copy(this BitVec dst, BitVec src) {
    copy(dst.B, src.B);
}

public static bool Get(this BitVec bv, int i) {
    if (i < 0 || i >= bv.N) {
        @base.Fatalf("bvget: index %d is out of bounds with length %d\n", i, bv.N);
    }
    var mask = uint32(1 << (int)(uint(i % wordBits)));
    return bv.B[i >> (int)(wordShift)] & mask != 0;

}

public static void Set(this BitVec bv, int i) {
    if (i < 0 || i >= bv.N) {
        @base.Fatalf("bvset: index %d is out of bounds with length %d\n", i, bv.N);
    }
    var mask = uint32(1 << (int)(uint(i % wordBits)));
    bv.B[i / wordBits] |= mask;

}

public static void Unset(this BitVec bv, int i) {
    if (i < 0 || i >= bv.N) {
        @base.Fatalf("bvunset: index %d is out of bounds with length %d\n", i, bv.N);
    }
    var mask = uint32(1 << (int)(uint(i % wordBits)));
    bv.B[i / wordBits] &= mask;

}

// bvnext returns the smallest index >= i for which bvget(bv, i) == 1.
// If there is no such index, bvnext returns -1.
public static int Next(this BitVec bv, int i) {
    if (i >= bv.N) {
        return -1;
    }
    if (bv.B[i >> (int)(wordShift)] >> (int)(uint(i & wordMask)) == 0) {
        i &= wordMask;
        i += wordBits;
        while (i < bv.N && bv.B[i >> (int)(wordShift)] == 0) {
            i += wordBits;
        }
    }
    if (i >= bv.N) {
        return -1;
    }
    var w = bv.B[i >> (int)(wordShift)] >> (int)(uint(i & wordMask));
    i += int32(bits.TrailingZeros32(w));

    return i;

}

public static bool IsEmpty(this BitVec bv) {
    foreach (var (_, x) in bv.B) {
        if (x != 0) {
            return false;
        }
    }    return true;

}

public static void Not(this BitVec bv) {
    foreach (var (i, x) in bv.B) {
        bv.B[i] = ~x;
    }
}

// union
public static void Or(this BitVec dst, BitVec src1, BitVec src2) {
    if (len(src1.B) == 0) {
        return ;
    }
    (_, _) = (dst.B[len(src1.B) - 1], src2.B[len(src1.B) - 1]);    foreach (var (i, x) in src1.B) {
        dst.B[i] = x | src2.B[i];
    }
}

// intersection
public static void And(this BitVec dst, BitVec src1, BitVec src2) {
    if (len(src1.B) == 0) {
        return ;
    }
    (_, _) = (dst.B[len(src1.B) - 1], src2.B[len(src1.B) - 1]);    foreach (var (i, x) in src1.B) {
        dst.B[i] = x & src2.B[i];
    }
}

// difference
public static void AndNot(this BitVec dst, BitVec src1, BitVec src2) {
    if (len(src1.B) == 0) {
        return ;
    }
    (_, _) = (dst.B[len(src1.B) - 1], src2.B[len(src1.B) - 1]);    foreach (var (i, x) in src1.B) {
        dst.B[i] = x & ~src2.B[i];
    }
}

public static @string String(this BitVec bv) {
    var s = make_slice<byte>(2 + bv.N);
    copy(s, "#*");
    for (var i = int32(0); i < bv.N; i++) {
        var ch = byte('0');
        if (bv.Get(i)) {
            ch = '1';
        }
        s[2 + i] = ch;

    }
    return string(s);

}

public static void Clear(this BitVec bv) {
    foreach (var (i) in bv.B) {
        bv.B[i] = 0;
    }
}

} // end bitvec_package
