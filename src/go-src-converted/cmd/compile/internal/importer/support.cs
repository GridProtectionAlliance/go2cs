// UNREVIEWED
// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements support functionality for iimport.go.

// package importer -- go2cs converted at 2022 March 06 23:13:55 UTC
// import "cmd/compile/internal/importer" ==> using importer = go.cmd.compile.@internal.importer_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\support.go
using types2 = go.cmd.compile.@internal.types2_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using sync = go.sync_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class importer_package {

private static void errorf(@string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    panic(fmt.Sprintf(format, args));
});

private static readonly nint deltaNewFile = -64; // see cmd/compile/internal/gc/bexport.go

// Synthesize a token.Pos
 // see cmd/compile/internal/gc/bexport.go

// Synthesize a token.Pos
private partial struct fakeFileSet {
    public ptr<token.FileSet> fset;
    public map<@string, ptr<token.File>> files;
}

private static token.Pos pos(this ptr<fakeFileSet> _addr_s, @string file, nint line, nint column) {
    ref fakeFileSet s = ref _addr_s.val;
 
    // TODO(mdempsky): Make use of column.

    // Since we don't know the set of needed file positions, we
    // reserve maxlines positions per file.
    const nint maxlines = 64 * 1024;

    var f = s.files[file];
    if (f == null) {
        f = s.fset.AddFile(file, -1, maxlines);
        s.files[file] = f; 
        // Allocate the fake linebreak indices on first use.
        // TODO(adonovan): opt: save ~512KB using a more complex scheme?
        fakeLinesOnce.Do(() => {
            fakeLines = make_slice<nint>(maxlines);
            foreach (var (i) in fakeLines) {
                fakeLines[i] = i;
            }
        });
        f.SetLines(fakeLines);

    }
    if (line > maxlines) {
        line = 1;
    }
    return f.Pos(line - 1);

}

private static slice<nint> fakeLines = default;private static sync.Once fakeLinesOnce = default;

private static types2.ChanDir chanDir(nint d) { 
    // tag values must match the constants in cmd/compile/internal/gc/go.go
    switch (d) {
        case 1: 
            return types2.RecvOnly;
            break;
        case 2: 
            return types2.SendOnly;
            break;
        case 3: 
            return types2.SendRecv;
            break;
        default: 
            errorf("unexpected channel dir %d", d);
            return 0;
            break;
    }

}

private static types2.Type predeclared = new slice<types2.Type>(new types2.Type[] { types2.Typ[types2.Bool], types2.Typ[types2.Int], types2.Typ[types2.Int8], types2.Typ[types2.Int16], types2.Typ[types2.Int32], types2.Typ[types2.Int64], types2.Typ[types2.Uint], types2.Typ[types2.Uint8], types2.Typ[types2.Uint16], types2.Typ[types2.Uint32], types2.Typ[types2.Uint64], types2.Typ[types2.Uintptr], types2.Typ[types2.Float32], types2.Typ[types2.Float64], types2.Typ[types2.Complex64], types2.Typ[types2.Complex128], types2.Typ[types2.String], types2.Universe.Lookup("byte").Type(), types2.Universe.Lookup("rune").Type(), types2.Universe.Lookup("error").Type(), types2.Typ[types2.UntypedBool], types2.Typ[types2.UntypedInt], types2.Typ[types2.UntypedRune], types2.Typ[types2.UntypedFloat], types2.Typ[types2.UntypedComplex], types2.Typ[types2.UntypedString], types2.Typ[types2.UntypedNil], types2.Typ[types2.UnsafePointer], types2.Typ[types2.Invalid], anyType{} });

private partial struct anyType {
}

private static types2.Type Underlying(this anyType t) {
    return t;
}
private static @string String(this anyType t) {
    return "any";
}

} // end importer_package
