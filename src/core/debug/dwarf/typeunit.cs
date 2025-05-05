// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using fmt = fmt_package;
using strconv = strconv_package;

partial class dwarf_package {

// Parse the type units stored in a DWARF4 .debug_types section. Each
// type unit defines a single primary type and an 8-byte signature.
// Other sections may then use formRefSig8 to refer to the type.

// The typeUnit format is a single type with a signature. It holds
// the same data as a compilation unit.
[GoType] partial struct typeUnit {
    internal partial ref unit unit { get; }
    internal Offset toff; // Offset to signature type within data.
    internal @string name; // Name of .debug_type section.
    internal ΔType cache; // Cache the type, nil to start.
}

// Parse a .debug_types section.
[GoRecv] internal static error parseTypes(this ref Data d, @string name, slice<byte> types) {
    var b = makeBuf(d, new unknownFormat(nil), name, 0, types);
    while (len(b.data) > 0) {
        var @base = b.off;
        var (n, dwarf64) = b.unitLength();
        if (n != ((Offset)((uint32)n))) {
            b.error("type unit length overflow"u8);
            return b.err;
        }
        var hdroff = b.off;
        nint vers = ((nint)b.uint16());
        if (vers != 4) {
            b.error("unsupported DWARF version "u8 + strconv.Itoa(vers));
            return b.err;
        }
        uint64 ao = default!;
        if (!dwarf64){
            ao = ((uint64)b.uint32());
        } else {
            ao = b.uint64();
        }
        (atable, err) = d.parseAbbrev(ao, vers);
        if (err != default!) {
            return err;
        }
        var asize = b.uint8();
        var sig = b.uint64();
        uint32 toff = default!;
        if (!dwarf64){
            toff = b.uint32();
        } else {
            var to64 = b.uint64();
            if (to64 != ((uint64)((uint32)to64))) {
                b.error("type unit type offset overflow"u8);
                return b.err;
            }
            toff = ((uint32)to64);
        }
        var boff = b.off;
        d.typeSigs[sig] = Ꮡ(new typeUnit(
            unit: new unit(
                @base: @base,
                off: boff,
                data: b.bytes(((nint)(n - (b.off - hdroff)))),
                atable: atable,
                asize: ((nint)asize),
                vers: vers,
                is64: dwarf64
            ),
            toff: ((Offset)toff),
            name: name
        ));
        if (b.err != default!) {
            return b.err;
        }
    }
    return default!;
}

// Return the type for a type signature.
[GoRecv] internal static (ΔType, error) sigToType(this ref Data d, uint64 sig) {
    var tu = d.typeSigs[sig];
    if (tu == nil) {
        return (default!, fmt.Errorf("no type unit with signature %v"u8, sig));
    }
    if ((~tu).cache != default!) {
        return ((~tu).cache, default!);
    }
    ref var b = ref heap<buf>(out var Ꮡb);
    b = makeBuf(d, ~tu, (~tu).name, tu.off, tu.data);
    var r = Ꮡ(new typeUnitReader(d: d, tu: tu, b: b));
    (t, err) = d.readType((~tu).name, ~r, (~tu).toff, new dwarf.Type(), nil);
    if (err != default!) {
        return (default!, err);
    }
    tu.val.cache = t;
    return (t, default!);
}

// typeUnitReader is a typeReader for a tagTypeUnit.
[GoType] partial struct typeUnitReader {
    internal ж<Data> d;
    internal ж<typeUnit> tu;
    internal buf b;
    internal error err;
}

// Seek to a new position in the type unit.
[GoRecv] internal static void Seek(this ref typeUnitReader tur, Offset off) {
    tur.err = default!;
    var doff = off - tur.tu.off;
    if (doff < 0 || doff >= ((Offset)len(tur.tu.data))) {
        tur.err = fmt.Errorf("%s: offset %d out of range; max %d"u8, tur.tu.name, doff, len(tur.tu.data));
        return;
    }
    tur.b = makeBuf(tur.d, ~tur.tu, tur.tu.name, off, tur.tu.data[(int)(doff)..]);
}

// AddressSize returns the size in bytes of addresses in the current type unit.
[GoRecv] internal static nint AddressSize(this ref typeUnitReader tur) {
    return tur.tu.unit.asize;
}

// Next reads the next [Entry] from the type unit.
[GoRecv] internal static (ж<Entry>, error) Next(this ref typeUnitReader tur) {
    if (tur.err != default!) {
        return (default!, tur.err);
    }
    if (len(tur.tu.data) == 0) {
        return (default!, default!);
    }
    var e = tur.b.entry(nil, tur.tu.atable, tur.tu.@base, tur.tu.vers);
    if (tur.b.err != default!) {
        tur.err = tur.b.err;
        return (default!, tur.err);
    }
    return (e, default!);
}

// clone returns a new reader for the type unit.
[GoRecv] internal static typeReader clone(this ref typeUnitReader tur) {
    return new typeUnitReader(
        d: tur.d,
        tu: tur.tu,
        b: makeBuf(tur.d, ~tur.tu, tur.tu.name, tur.tu.off, tur.tu.data)
    );
}

// offset returns the current offset.
[GoRecv] internal static Offset offset(this ref typeUnitReader tur) {
    return tur.b.off;
}

} // end dwarf_package
