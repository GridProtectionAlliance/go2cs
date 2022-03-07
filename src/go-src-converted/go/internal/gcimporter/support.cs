// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements support functionality for iimport.go.

// package gcimporter -- go2cs converted at 2022 March 06 23:32:40 UTC
// import "go/internal/gcimporter" ==> using gcimporter = go.go.@internal.gcimporter_package
// Original source: C:\Program Files\Go\src\go\internal\gcimporter\support.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using sync = go.sync_package;
using System;


namespace go.go.@internal;

public static partial class gcimporter_package {

private static void errorf(@string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    panic(fmt.Sprintf(format, args));
});

// deltaNewFile is a magic line delta offset indicating a new file.
// We use -64 because it is rare; see issue 20080 and CL 41619.
// -64 is the smallest int that fits in a single byte as a varint.
private static readonly nint deltaNewFile = -64;

// Synthesize a token.Pos


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

private static types.ChanDir chanDir(nint d) { 
    // tag values must match the constants in cmd/compile/internal/gc/go.go
    switch (d) {
        case 1: 
            return types.RecvOnly;
            break;
        case 2: 
            return types.SendOnly;
            break;
        case 3: 
            return types.SendRecv;
            break;
        default: 
            errorf("unexpected channel dir %d", d);
            return 0;
            break;
    }

}

private static types.Type predeclared = new slice<types.Type>(new types.Type[] { types.Typ[types.Bool], types.Typ[types.Int], types.Typ[types.Int8], types.Typ[types.Int16], types.Typ[types.Int32], types.Typ[types.Int64], types.Typ[types.Uint], types.Typ[types.Uint8], types.Typ[types.Uint16], types.Typ[types.Uint32], types.Typ[types.Uint64], types.Typ[types.Uintptr], types.Typ[types.Float32], types.Typ[types.Float64], types.Typ[types.Complex64], types.Typ[types.Complex128], types.Typ[types.String], types.Universe.Lookup("byte").Type(), types.Universe.Lookup("rune").Type(), types.Universe.Lookup("error").Type(), types.Typ[types.UntypedBool], types.Typ[types.UntypedInt], types.Typ[types.UntypedRune], types.Typ[types.UntypedFloat], types.Typ[types.UntypedComplex], types.Typ[types.UntypedString], types.Typ[types.UntypedNil], types.Typ[types.UnsafePointer], types.Typ[types.Invalid], anyType{} });

private partial struct anyType {
}

private static types.Type Underlying(this anyType t) {
    return t;
}
private static @string String(this anyType t) {
    return "any";
}

} // end gcimporter_package
