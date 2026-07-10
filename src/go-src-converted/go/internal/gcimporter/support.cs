// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements support functionality for iimport.go.
namespace go.go.@internal;

using fmt = fmt_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using pkgbits = global::go.@internal.pkgbits_package;
using sync = sync_package;
using global::go.@internal;
using global::go.go;
using ꓸꓸꓸany = Span<any>;

partial class gcimporter_package {

internal static void assert(bool b) {
    if (!b) {
        throw panic("assertion failed");
    }
}

internal static void errorf(@string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    throw panic(fmt.Sprintf(format, args.ꓸꓸꓸ));
}

// deltaNewFile is a magic line delta offset indicating a new file.
// We use -64 because it is rare; see issue 20080 and CL 41619.
// -64 is the smallest int that fits in a single byte as a varint.
internal static readonly UntypedInt deltaNewFile = -64;

// Synthesize a token.Pos
[GoType] partial struct fakeFileSet {
    internal ж<token.FileSet> fset;
    internal map<@string, ж<fileInfo>> files;
}

[GoType] partial struct fileInfo {
    internal ж<tokenꓸFile> @file;
    internal nint lastline;
}

internal static readonly UntypedInt maxlines = /* 64 * 1024 */ 65536;

[GoRecv] internal static tokenꓸPos pos(this ref fakeFileSet s, @string @file, nint line, nint column) {
    // TODO(mdempsky): Make use of column.
    // Since we don't know the set of needed file positions, we reserve
    // maxlines positions per file. We delay calling token.File.SetLines until
    // all positions have been calculated (by way of fakeFileSet.setLines), so
    // that we can avoid setting unnecessary lines. See also golang/go#46586.
    var f = s.files[@file];
    if (f == nil) {
        f = Ꮡ(new fileInfo(@file: s.fset.AddFile(@file, -1, maxlines)));
        s.files[@file] = f;
    }
    if (line > maxlines) {
        line = 1;
    }
    if (line > (~f).lastline) {
        f.Value.lastline = line;
    }
    // Return a fake position assuming that f.file consists only of newlines.
    return ((tokenꓸPos)((~f).@file.Base() + line - 1));
}

[GoRecv] internal static void setLines(this ref fakeFileSet s) {
    ᏑfakeLinesOnce.Do(() => {
        fakeLines = new slice<nint>(maxlines);
        foreach (var (i, _) in fakeLines) {
            fakeLines[i] = i;
        }
    });
    foreach (var (_, f) in s.files) {
        (~f).@file.SetLines(fakeLines[..(int)((~f).lastline)]);
    }
}

internal static slice<nint> fakeLines;
internal static ж<sync.Once> ᏑfakeLinesOnce = new(default(sync.Once));
internal static ref sync.Once fakeLinesOnce => ref ᏑfakeLinesOnce.Value;

internal static types.ChanDir chanDir(nint d) {
    // tag values must match the constants in cmd/compile/internal/gc/go.go
    switch (d) {
    case 1: {
        return types.RecvOnly;
    }
    case 2: {
        return types.SendOnly;
    }
    case 3: {
        return types.SendRecv;
    }
    default: {
        errorf("unexpected channel dir %d"u8, /* Crecv */
 /* Csend */
 /* Cboth */
 d);
        return 0;
    }}

}

// basic types
// basic type aliases
// error
// untyped types
// package unsafe
// invalid type
// only appears in packages with errors
// used internally by gc; never used by this package or in .a files
// not to be confused with the universe any
// comparable
// "any" has special handling: see usage of predeclared.
internal static slice<typesꓸType> predeclared = new typesꓸType[]{new types.BasicжΔType(types.Typ[types.Bool]), new types.BasicжΔType(types.Typ[types.Int]), new types.BasicжΔType(types.Typ[types.Int8]), new types.BasicжΔType(types.Typ[types.Int16]), new types.BasicжΔType(types.Typ[types.Int32]), new types.BasicжΔType(types.Typ[types.Int64]), new types.BasicжΔType(types.Typ[types.Uint]), new types.BasicжΔType(types.Typ[types.Uint8]), new types.BasicжΔType(types.Typ[types.Uint16]), new types.BasicжΔType(types.Typ[types.Uint32]), new types.BasicжΔType(types.Typ[types.Uint64]), new types.BasicжΔType(types.Typ[types.Uintptr]), new types.BasicжΔType(types.Typ[types.Float32]), new types.BasicжΔType(types.Typ[types.Float64]), new types.BasicжΔType(types.Typ[types.Complex64]), new types.BasicжΔType(types.Typ[types.Complex128]), new types.BasicжΔType(types.Typ[types.ΔString]), types.Universe.Lookup("byte"u8).Type(), types.Universe.Lookup("rune"u8).Type(), types.Universe.Lookup("error"u8).Type(), new types.BasicжΔType(types.Typ[types.UntypedBool]), new types.BasicжΔType(types.Typ[types.ΔUntypedInt]), new types.BasicжΔType(types.Typ[types.UntypedRune]), new types.BasicжΔType(types.Typ[types.ΔUntypedFloat]), new types.BasicжΔType(types.Typ[types.ΔUntypedComplex]), new types.BasicжΔType(types.Typ[types.UntypedString]), new types.BasicжΔType(types.Typ[types.UntypedNil]), new types.BasicжΔType(types.Typ[types.UnsafePointer]), new types.BasicжΔType(types.Typ[types.Invalid]), new anyType(nil), types.Universe.Lookup("comparable"u8).Type()
}.slice();

[GoType] partial struct anyType {
}

internal static typesꓸType Underlying(this anyType t) {
    return t;
}

internal static @string String(this anyType t) {
    return "any"u8;
}

// See cmd/compile/internal/noder.derivedInfo.
[GoType] partial struct derivedInfo {
    internal pkgbits.Index idx;
    internal bool needed;
}

// See cmd/compile/internal/noder.typeInfo.
[GoType] partial struct typeInfo {
    internal pkgbits.Index idx;
    internal bool derived;
}

// See cmd/compile/internal/types.SplitVargenSuffix.
internal static (@string @base, @string suffix) splitVargenSuffix(@string name) {
    @string @base = default!;
    @string suffix = default!;

    nint i = len(name);
    while (i > 0 && name[i - 1] >= (rune)'0' && name[i - 1] <= (rune)'9') {
        i--;
    }
    @string dot = "·"u8;
    if (i >= len(dot) && name[(int)(i - len(dot))..(int)(i)] == dot) {
        i -= len(dot);
        return (name[..(int)(i)], name[(int)(i)..]);
    }
    return (name, "");
}

} // end gcimporter_package
