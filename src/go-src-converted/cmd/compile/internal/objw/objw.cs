// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objw -- go2cs converted at 2022 March 13 05:59:00 UTC
// import "cmd/compile/internal/objw" ==> using objw = go.cmd.compile.@internal.objw_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\objw\objw.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using bitvec = cmd.compile.@internal.bitvec_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;


// Uint8 writes an unsigned byte v into s at offset off,
// and returns the next unused offset (i.e., off+1).

public static partial class objw_package {

public static nint Uint8(ptr<obj.LSym> _addr_s, nint off, byte v) {
    ref obj.LSym s = ref _addr_s.val;

    return UintN(_addr_s, off, uint64(v), 1);
}

public static nint Uint16(ptr<obj.LSym> _addr_s, nint off, ushort v) {
    ref obj.LSym s = ref _addr_s.val;

    return UintN(_addr_s, off, uint64(v), 2);
}

public static nint Uint32(ptr<obj.LSym> _addr_s, nint off, uint v) {
    ref obj.LSym s = ref _addr_s.val;

    return UintN(_addr_s, off, uint64(v), 4);
}

public static nint Uintptr(ptr<obj.LSym> _addr_s, nint off, ulong v) {
    ref obj.LSym s = ref _addr_s.val;

    return UintN(_addr_s, off, v, types.PtrSize);
}

// UintN writes an unsigned integer v of size wid bytes into s at offset off,
// and returns the next unused offset.
public static nint UintN(ptr<obj.LSym> _addr_s, nint off, ulong v, nint wid) {
    ref obj.LSym s = ref _addr_s.val;

    if (off & (wid - 1) != 0) {
        @base.Fatalf("duintxxLSym: misaligned: v=%d wid=%d off=%d", v, wid, off);
    }
    s.WriteInt(@base.Ctxt, int64(off), wid, int64(v));
    return off + wid;
}

public static nint SymPtr(ptr<obj.LSym> _addr_s, nint off, ptr<obj.LSym> _addr_x, nint xoff) {
    ref obj.LSym s = ref _addr_s.val;
    ref obj.LSym x = ref _addr_x.val;

    off = int(types.Rnd(int64(off), int64(types.PtrSize)));
    s.WriteAddr(@base.Ctxt, int64(off), types.PtrSize, x, int64(xoff));
    off += types.PtrSize;
    return off;
}

public static nint SymPtrWeak(ptr<obj.LSym> _addr_s, nint off, ptr<obj.LSym> _addr_x, nint xoff) {
    ref obj.LSym s = ref _addr_s.val;
    ref obj.LSym x = ref _addr_x.val;

    off = int(types.Rnd(int64(off), int64(types.PtrSize)));
    s.WriteWeakAddr(@base.Ctxt, int64(off), types.PtrSize, x, int64(xoff));
    off += types.PtrSize;
    return off;
}

public static nint SymPtrOff(ptr<obj.LSym> _addr_s, nint off, ptr<obj.LSym> _addr_x) {
    ref obj.LSym s = ref _addr_s.val;
    ref obj.LSym x = ref _addr_x.val;

    s.WriteOff(@base.Ctxt, int64(off), x, 0);
    off += 4;
    return off;
}

public static nint SymPtrWeakOff(ptr<obj.LSym> _addr_s, nint off, ptr<obj.LSym> _addr_x) {
    ref obj.LSym s = ref _addr_s.val;
    ref obj.LSym x = ref _addr_x.val;

    s.WriteWeakOff(@base.Ctxt, int64(off), x, 0);
    off += 4;
    return off;
}

public static void Global(ptr<obj.LSym> _addr_s, int width, short flags) {
    ref obj.LSym s = ref _addr_s.val;

    if (flags & obj.LOCAL != 0) {
        s.Set(obj.AttrLocal, true);
        flags &= obj.LOCAL;
    }
    @base.Ctxt.Globl(s, int64(width), int(flags));
}

// Bitvec writes the contents of bv into s as sequence of bytes
// in little-endian order, and returns the next unused offset.
public static nint BitVec(ptr<obj.LSym> _addr_s, nint off, bitvec.BitVec bv) {
    ref obj.LSym s = ref _addr_s.val;
 
    // Runtime reads the bitmaps as byte arrays. Oblige.
    {
        nint j = 0;

        while (int32(j) < bv.N) {
            var word = bv.B[j / 32];
            off = Uint8(_addr_s, off, uint8(word >> (int)((uint(j) % 32))));
            j += 8;
        }
    }
    return off;
}

} // end objw_package
