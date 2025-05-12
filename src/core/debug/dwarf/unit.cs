// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using sort = sort_package;
using strconv = strconv_package;

partial class dwarf_package {

// DWARF debug info is split into a sequence of compilation units.
// Each unit has its own abbreviation table and address size.
[GoType] partial struct unit {
    internal Offset @base; // byte offset of header within the aggregate info
    internal Offset off; // byte offset of data within the aggregate info
    internal slice<byte> data;
    internal abbrevTable atable;
    internal nint asize;
    internal nint vers;
    internal uint8 utype; // DWARF 5 unit type
    internal bool is64;  // True for 64-bit DWARF format
}

// Implement the dataFormat interface.
[GoRecv] internal static nint version(this ref unit u) {
    return u.vers;
}

[GoRecv] internal static (bool, bool) dwarf64(this ref unit u) {
    return (u.is64, true);
}

[GoRecv] internal static nint addrsize(this ref unit u) {
    return u.asize;
}

[GoRecv] internal static (slice<unit>, error) parseUnits(this ref Data d) {
    // Count units.
    nint nunit = 0;
    var b = makeBuf(d, new unknownFormat(nil), "info"u8, 0, d.info);
    while (len(b.data) > 0) {
        var (len, _) = b.unitLength();
        if (len != ((Offset)((uint32)len))) {
            b.error("unit length overflow"u8);
            break;
        }
        b.skip(((nint)len));
        if (len > 0) {
            nunit++;
        }
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    // Again, this time writing them down.
    b = makeBuf(d, new unknownFormat(nil), "info"u8, 0, d.info);
    var units = new slice<unit>(nunit);
    foreach (var (i, _) in units) {
        var u = Ꮡ(units, i);
        u.val.@base = b.off;
        Offset n = default!;
        if (b.err != default!) {
            return (default!, b.err);
        }
        while (n == 0) {
            (n, u.val.is64) = b.unitLength();
        }
        var dataOff = b.off;
        var vers = b.uint16();
        if (vers < 2 || vers > 5) {
            b.error("unsupported DWARF version "u8 + strconv.Itoa(((nint)vers)));
            break;
        }
        u.val.vers = ((nint)vers);
        if (vers >= 5) {
            u.val.utype = b.uint8();
            u.val.asize = ((nint)b.uint8());
        }
        uint64 abbrevOff = default!;
        if ((~u).is64){
            abbrevOff = b.uint64();
        } else {
            abbrevOff = ((uint64)b.uint32());
        }
        (atable, err) = d.parseAbbrev(abbrevOff, (~u).vers);
        if (err != default!) {
            if (b.err == default!) {
                b.err = err;
            }
            break;
        }
        u.val.atable = atable;
        if (vers < 5) {
            u.val.asize = ((nint)b.uint8());
        }
        var exprᴛ1 = (~u).utype;
        if (exprᴛ1 == utSkeleton || exprᴛ1 == utSplitCompile) {
            b.uint64();
        }
        else if (exprᴛ1 == utType || exprᴛ1 == utSplitType) {
            b.uint64();
            if ((~u).is64){
                // unit ID
                // type signature
                // type offset
                b.uint64();
            } else {
                b.uint32();
            }
        }

        u.val.off = b.off;
        u.val.data = b.bytes(((nint)(n - (b.off - dataOff))));
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    return (units, default!);
}

// offsetToUnit returns the index of the unit containing offset off.
// It returns -1 if no unit contains this offset.
[GoRecv] internal static nint offsetToUnit(this ref Data d, Offset off) {
    // Find the unit after off
    nint next = sort.Search(len(d.unit), (nint i) => d.unit[i].off > off);
    if (next == 0) {
        return -1;
    }
    var u = Ꮡ(d.unit[next - 1]);
    if ((~u).off <= off && off < (~u).off + ((Offset)len((~u).data))) {
        return next - 1;
    }
    return -1;
}

} // end dwarf_package
