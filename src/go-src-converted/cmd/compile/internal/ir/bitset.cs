// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:47:42 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\bitset.go


namespace go.cmd.compile.@internal;

public static partial class ir_package {

private partial struct bitset8 { // : byte
}

private static void set(this ptr<bitset8> _addr_f, byte mask, bool b) {
    ref bitset8 f = ref _addr_f.val;

    if (b) {
        (uint8.val)(f).val;

        mask;

    }
    else
 {
        (uint8.val)(f).val;

        mask;

    }
}

private static byte get2(this bitset8 f, byte shift) {
    return uint8(f >> (int)(shift)) & 3;
}

// set2 sets two bits in f using the bottom two bits of b.
private static void set2(this ptr<bitset8> _addr_f, byte shift, byte b) {
    ref bitset8 f = ref _addr_f.val;
 
    // Clear old bits.
    (uint8.val)(f).val;

    3 << (int)(shift) * (uint8.val)(f);

    uint8(b & 3) << (int)(shift);

}

private partial struct bitset16 { // : ushort
}

private static void set(this ptr<bitset16> _addr_f, ushort mask, bool b) {
    ref bitset16 f = ref _addr_f.val;

    if (b) {
        (uint16.val)(f).val;

        mask;

    }
    else
 {
        (uint16.val)(f).val;

        mask;

    }
}

private partial struct bitset32 { // : uint
}

private static void set(this ptr<bitset32> _addr_f, uint mask, bool b) {
    ref bitset32 f = ref _addr_f.val;

    if (b) {
        (uint32.val)(f).val;

        mask;

    }
    else
 {
        (uint32.val)(f).val;

        mask;

    }
}

private static byte get2(this bitset32 f, byte shift) {
    return uint8(f >> (int)(shift)) & 3;
}

// set2 sets two bits in f using the bottom two bits of b.
private static void set2(this ptr<bitset32> _addr_f, byte shift, byte b) {
    ref bitset32 f = ref _addr_f.val;
 
    // Clear old bits.
    (uint32.val)(f).val;

    3 << (int)(shift) * (uint32.val)(f);

    uint32(b & 3) << (int)(shift);

}

private static byte get3(this bitset32 f, byte shift) {
    return uint8(f >> (int)(shift)) & 7;
}

// set3 sets three bits in f using the bottom three bits of b.
private static void set3(this ptr<bitset32> _addr_f, byte shift, byte b) {
    ref bitset32 f = ref _addr_f.val;
 
    // Clear old bits.
    (uint32.val)(f).val;

    7 << (int)(shift) * (uint32.val)(f);

    uint32(b & 7) << (int)(shift);

}

} // end ir_package
