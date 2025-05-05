// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// This package contains APIs and helpers for encoding the meta-data
// "blob" for a single Go package, created when coverage
// instrumentation is turned on.
using bytes = bytes_package;
using md5 = crypto.md5_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using hash = hash_package;
using coverage = @internal.coverage_package;
using stringtab = @internal.coverage.stringtab_package;
using uleb128 = @internal.coverage.uleb128_package;
using io = io_package;
using os = os_package;
using @internal;
using crypto;
using encoding;

partial class encodemeta_package {

[GoType] partial struct CoverageMetaDataBuilder {
    internal @internal.coverage.stringtab_package.Writer stab;
    internal slice<funcDesc> funcs;
    internal slice<byte> tmp; // temp work slice
    internal hash_package.Hash h;
    internal uint32 pkgpath;
    internal uint32 pkgname;
    internal uint32 modpath;
    internal bool debug;
    internal error werr;
}

public static (ж<CoverageMetaDataBuilder>, error) NewCoverageMetaDataBuilder(@string pkgpath, @string pkgname, @string modulepath) {
    if (pkgpath == ""u8) {
        return (default!, fmt.Errorf("invalid empty package path"u8));
    }
    var x = Ꮡ(new CoverageMetaDataBuilder(
        tmp: new slice<byte>(0, 256),
        h: md5.New()
    ));
    (~x).stab.InitWriter();
    (~x).stab.Lookup(""u8);
    x.val.pkgpath = (~x).stab.Lookup(pkgpath);
    x.val.pkgname = (~x).stab.Lookup(pkgname);
    x.val.modpath = (~x).stab.Lookup(modulepath);
    io.WriteString((~x).h, pkgpath);
    io.WriteString((~x).h, pkgname);
    io.WriteString((~x).h, modulepath);
    return (x, default!);
}

internal static void h32(uint32 x, hash.Hash h, slice<byte> tmp) {
    tmp = tmp[..0];
    tmp = append(tmp, 0, 0, 0, 0);
    binary.LittleEndian.PutUint32(tmp, x);
    h.Write(tmp);
}

[GoType] partial struct funcDesc {
    internal slice<byte> encoded;
}

// AddFunc registers a new function with the meta data builder.
[GoRecv] public static nuint AddFunc(this ref CoverageMetaDataBuilder b, coverage.FuncDesc f) {
    hashFuncDesc(b.h, Ꮡ(f), b.tmp);
    var fd = new funcDesc(nil);
    b.tmp = b.tmp[..0];
    b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)len(f.Units)));
    b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)b.stab.Lookup(f.Funcname)));
    b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)b.stab.Lookup(f.Srcfile)));
    foreach (var (_, u) in f.Units) {
        b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)u.StLine));
        b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)u.StCol));
        b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)u.EnLine));
        b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)u.EnCol));
        b.tmp = uleb128.AppendUleb128(b.tmp, ((nuint)u.NxStmts));
    }
    nuint lit = ((nuint)0);
    if (f.Lit) {
        lit = 1;
    }
    b.tmp = uleb128.AppendUleb128(b.tmp, lit);
    fd.encoded = bytes.Clone(b.tmp);
    nuint rv = ((nuint)len(b.funcs));
    b.funcs = append(b.funcs, fd);
    return rv;
}

[GoRecv] internal static int64 emitFuncOffsets(this ref CoverageMetaDataBuilder b, io.WriteSeeker w, int64 off) {
    nint nFuncs = len(b.funcs);
    int64 foff = coverage.CovMetaHeaderSize + ((int64)b.stab.Size()) + ((int64)nFuncs) * 4;
    for (nint idx = 0; idx < nFuncs; idx++) {
        b.wrUint32(w, ((uint32)foff));
        foff += ((int64)len(b.funcs[idx].encoded));
    }
    return off + (((int64)len(b.funcs)) * 4);
}

[GoRecv] internal static (int64, error) emitFunc(this ref CoverageMetaDataBuilder b, io.WriteSeeker w, int64 off, funcDesc f) {
    nint ew = len(f.encoded);
    {
        var (nw, err) = w.Write(f.encoded); if (err != default!){
            return (0, err);
        } else 
        if (ew != nw) {
            return (0, fmt.Errorf("short write emitting coverage meta-data"u8));
        }
    }
    return (off + ((int64)ew), default!);
}

[GoRecv] internal static void reportWriteError(this ref CoverageMetaDataBuilder b, error err) {
    if (b.werr != default!) {
        b.werr = err;
    }
}

[GoRecv] internal static void wrUint32(this ref CoverageMetaDataBuilder b, io.WriteSeeker w, uint32 v) {
    b.tmp = b.tmp[..0];
    b.tmp = append(b.tmp, 0, 0, 0, 0);
    binary.LittleEndian.PutUint32(b.tmp, v);
    {
        var (nw, err) = w.Write(b.tmp); if (err != default!){
            b.reportWriteError(err);
        } else 
        if (nw != 4) {
            b.reportWriteError(fmt.Errorf("short write"u8));
        }
    }
}

// Emit writes the meta-data accumulated so far in this builder to 'w'.
// Returns a hash of the meta-data payload and an error.
[GoRecv] public static (array<byte>, error) Emit(this ref CoverageMetaDataBuilder b, io.WriteSeeker w) {
    // Emit header.  Length will initially be zero, we'll
    // back-patch it later.
    array<byte> digest = new(16);
    copy(digest[..], b.h.Sum(default!));
    var mh = new coverage.MetaSymbolHeader( // hash and length initially zero, will be back-patched

        PkgPath: ((uint32)b.pkgpath),
        PkgName: ((uint32)b.pkgname),
        ModulePath: ((uint32)b.modpath),
        NumFiles: ((uint32)b.stab.Nentries()),
        NumFuncs: ((uint32)len(b.funcs)),
        MetaHash: digest
    );
    if (b.debug) {
        fmt.Fprintf(~os.Stderr, "=-= writing header: %+v\n"u8, mh);
    }
    {
        var errΔ1 = binary.Write(w, binary.LittleEndian, mh); if (errΔ1 != default!) {
            return (digest, fmt.Errorf("error writing meta-file header: %v"u8, errΔ1));
        }
    }
    var off = ((int64)coverage.CovMetaHeaderSize);
    // Write function offsets section
    off = b.emitFuncOffsets(w, off);
    // Check for any errors up to this point.
    if (b.werr != default!) {
        return (digest, b.werr);
    }
    // Write string table.
    {
        var errΔ2 = b.stab.Write(w); if (errΔ2 != default!) {
            return (digest, errΔ2);
        }
    }
    off += ((int64)b.stab.Size());
    // Write functions
    foreach (var (_, f) in b.funcs) {
        error err = default!;
        (off, err) = b.emitFunc(w, off, f);
        if (err != default!) {
            return (digest, err);
        }
    }
    // Back-patch the length.
    var totalLength = ((uint32)off);
    {
        var (_, err) = w.Seek(0, io.SeekStart); if (err != default!) {
            return (digest, err);
        }
    }
    b.wrUint32(w, totalLength);
    if (b.werr != default!) {
        return (digest, b.werr);
    }
    return (digest, default!);
}

// HashFuncDesc computes an md5 sum of a coverage.FuncDesc and returns
// a digest for it.
public static array<byte> HashFuncDesc(ж<coverage.FuncDesc> Ꮡf) {
    ref var f = ref Ꮡf.val;

    var h = md5.New();
    var tmp = new slice<byte>(0, 32);
    hashFuncDesc(h, Ꮡf, tmp);
    array<byte> r = new(16);
    copy(r[..], h.Sum(default!));
    return r;
}

// hashFuncDesc incorporates a given function 'f' into the hash 'h'.
internal static void hashFuncDesc(hash.Hash h, ж<coverage.FuncDesc> Ꮡf, slice<byte> tmp) {
    ref var f = ref Ꮡf.val;

    io.WriteString(h, f.Funcname);
    io.WriteString(h, f.Srcfile);
    foreach (var (_, u) in f.Units) {
        h32(u.StLine, h, tmp);
        h32(u.StCol, h, tmp);
        h32(u.EnLine, h, tmp);
        h32(u.EnCol, h, tmp);
        h32(u.NxStmts, h, tmp);
    }
    var lit = ((uint32)0);
    if (f.Lit) {
        lit = 1;
    }
    h32(lit, h, tmp);
}

} // end encodemeta_package
