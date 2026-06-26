// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// This package contains APIs and helpers for decoding a single package's
// meta data "blob" emitted by the compiler when coverage instrumentation
// is turned on.
using binary = encoding.binary_package;
using fmt = fmt_package;
using coverage = @internal.coverage_package;
using slicereader = @internal.coverage.slicereader_package;
using stringtab = @internal.coverage.stringtab_package;
using io = io_package;
using os = os_package;
using @internal;
using encoding;

partial class decodemeta_package {

// See comments in the encodecovmeta package for details on the format.
[GoType] partial struct CoverageMetaDataDecoder {
    internal ж<@internal.coverage.slicereader_package.Reader> r;
    internal @internal.coverage_package.MetaSymbolHeader hdr;
    internal ж<@internal.coverage.stringtab_package.Reader> strtab;
    internal slice<byte> tmp;
    internal bool debug;
}

public static (ж<CoverageMetaDataDecoder>, error) NewCoverageMetaDataDecoder(slice<byte> b, bool @readonly) {
    var slr = slicereader.NewReader(b, @readonly);
    var x = Ꮡ(new CoverageMetaDataDecoder(
        r: slr,
        tmp: new slice<byte>(0, 256)
    ));
    {
        var err = x.readHeader(); if (err != default!) {
            return (default!, err);
        }
    }
    {
        var err = x.readStringTable(); if (err != default!) {
            return (default!, err);
        }
    }
    return (x, default!);
}

[GoRecv] internal static error readHeader(this ref CoverageMetaDataDecoder d) {
    {
        var err = binary.Read(~d.r, binary.LittleEndian, Ꮡ(d.hdr)); if (err != default!) {
            return err;
        }
    }
    if (d.debug) {
        fmt.Fprintf(~os.Stderr, "=-= after readHeader: %+v\n"u8, d.hdr);
    }
    return default!;
}

[GoRecv] internal static error readStringTable(this ref CoverageMetaDataDecoder d) {
    // Seek to the correct location to read the string table.
    var stringTableLocation = ((int64)(coverage.CovMetaHeaderSize + 4 * d.hdr.NumFuncs));
    {
        var (_, err) = d.r.Seek(stringTableLocation, io.SeekStart); if (err != default!) {
            return err;
        }
    }
    // Read the table itself.
    d.strtab = stringtab.NewReader(d.r);
    d.strtab.Read();
    return default!;
}

[GoRecv] public static @string PackagePath(this ref CoverageMetaDataDecoder d) {
    return d.strtab.Get(d.hdr.PkgPath);
}

[GoRecv] public static @string PackageName(this ref CoverageMetaDataDecoder d) {
    return d.strtab.Get(d.hdr.PkgName);
}

[GoRecv] public static @string ModulePath(this ref CoverageMetaDataDecoder d) {
    return d.strtab.Get(d.hdr.ModulePath);
}

[GoRecv] public static uint32 NumFuncs(this ref CoverageMetaDataDecoder d) {
    return d.hdr.NumFuncs;
}

// ReadFunc reads the coverage meta-data for the function with index
// 'findex', filling it into the FuncDesc pointed to by 'f'.
[GoRecv] public static error ReadFunc(this ref CoverageMetaDataDecoder d, uint32 fidx, ж<coverage.FuncDesc> Ꮡf) {
    ref var f = ref Ꮡf.val;

    if (fidx >= d.hdr.NumFuncs) {
        return fmt.Errorf("illegal function index"u8);
    }
    // Seek to the correct location to read the function offset and read it.
    var funcOffsetLocation = ((int64)(coverage.CovMetaHeaderSize + 4 * fidx));
    {
        var (_, err) = d.r.Seek(funcOffsetLocation, io.SeekStart); if (err != default!) {
            return err;
        }
    }
    var foff = d.r.ReadUint32();
    // Check assumptions
    if (foff < ((uint32)funcOffsetLocation) || foff > d.hdr.Length) {
        return fmt.Errorf("malformed func offset %d"u8, foff);
    }
    // Seek to the correct location to read the function.
    var floc = ((int64)foff);
    {
        var (_, err) = d.r.Seek(floc, io.SeekStart); if (err != default!) {
            return err;
        }
    }
    // Preamble containing number of units, file, and function.
    var numUnits = ((uint32)d.r.ReadULEB128());
    var fnameidx = ((uint32)d.r.ReadULEB128());
    var fileidx = ((uint32)d.r.ReadULEB128());
    f.Srcfile = d.strtab.Get(fileidx);
    f.Funcname = d.strtab.Get(fnameidx);
    // Now the units
    f.Units = f.Units[..0];
    if (cap(f.Units) < ((nint)numUnits)) {
        f.Units = new slice<coverage.CoverableUnit>(0, numUnits);
    }
    for (var k = ((uint32)0); k < numUnits; k++) {
        f.Units = append(f.Units,
            new coverage.CoverableUnit(
                StLine: ((uint32)d.r.ReadULEB128()),
                StCol: ((uint32)d.r.ReadULEB128()),
                EnLine: ((uint32)d.r.ReadULEB128()),
                EnCol: ((uint32)d.r.ReadULEB128()),
                NxStmts: ((uint32)d.r.ReadULEB128())
            ));
    }
    var lit = d.r.ReadULEB128();
    f.Lit = lit != 0;
    return default!;
}

} // end decodemeta_package
