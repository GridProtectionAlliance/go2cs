// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package importer provides access to export data importers.
namespace go.go;

using build = go.build_package;
using gccgoimporter = go.@internal.gccgoimporter_package;
using gcimporter = go.@internal.gcimporter_package;
using srcimporter = go.@internal.srcimporter_package;
using token = go.token_package;
using types = go.types_package;
using io = io_package;
using runtime = runtime_package;
using go.@internal;

partial class importer_package {

public delegate (io.ReadCloser, error) Lookup(@string path);

// ForCompiler returns an Importer for importing from installed packages
// for the compilers "gc" and "gccgo", or for importing directly
// from the source if the compiler argument is "source". In this
// latter case, importing may fail under circumstances where the
// exported API is not entirely defined in pure Go source code
// (if the package API depends on cgo-defined entities, the type
// checker won't have access to those).
//
// The lookup function is called each time the resulting importer needs
// to resolve an import path. In this mode the importer can only be
// invoked with canonical import paths (not relative or absolute ones);
// it is assumed that the translation to canonical import paths is being
// done by the client of the importer.
//
// A lookup function must be provided for correct module-aware operation.
// Deprecated: If lookup is nil, for backwards-compatibility, the importer
// will attempt to resolve imports in the $GOPATH workspace.
public static types.Importer ForCompiler(ж<token.FileSet> Ꮡfset, @string compiler, Lookup lookup) {
    ref var fset = ref Ꮡfset.val;

    var exprᴛ1 = compiler;
    if (exprᴛ1 == "gc"u8) {
        return new gcimports(
            fset: fset,
            packages: new types.Package(),
            lookup: lookup
        );
    }
    if (exprᴛ1 == "gccgo"u8) {
        gccgoimporter.GccgoInstallation inst = default!;
        {
            var err = inst.InitFromDriver("gccgo"u8); if (err != default!) {
                return default!;
            }
        }
        return new gccgoimports(
            packages: new types.Package(),
            importer: inst.GetImporter(default!, default!),
            lookup: lookup
        );
    }
    if (exprᴛ1 == "source"u8) {
        if (lookup != default!) {
            throw panic("source importer for custom import path lookup not supported (issue #13847).");
        }
        return ~srcimporter.New(Ꮡ(build.Default), Ꮡfset, new types.Package());
    }

    // compiler not supported
    return default!;
}

// For calls [ForCompiler] with a new FileSet.
//
// Deprecated: Use [ForCompiler], which populates a FileSet
// with the positions of objects created by the importer.
public static types.Importer For(@string compiler, Lookup lookup) {
    return ForCompiler(token.NewFileSet(), compiler, lookup);
}

// Default returns an Importer for the compiler that built the running binary.
// If available, the result implements [types.ImporterFrom].
public static types.Importer Default() {
    return For(runtime.Compiler, default!);
}

// gc importer
[GoType] partial struct gcimports {
    internal ж<go.token_package.FileSet> fset;
    internal types.Package packages;
    internal Lookup lookup;
}

[GoRecv] internal static (ж<types.Package>, error) Import(this ref gcimports m, @string path) {
    return m.ImportFrom(path, ""u8, /* no vendoring */
 0);
}

[GoRecv] internal static (ж<types.Package>, error) ImportFrom(this ref gcimports m, @string path, @string srcDir, types.ImportMode mode) {
    if (mode != 0) {
        throw panic("mode must be 0");
    }
    return gcimporter.Import(m.fset, m.packages, path, srcDir, m.lookup);
}

// gccgo importer
[GoType] partial struct gccgoimports {
    internal types.Package packages;
    internal go.@internal.gccgoimporter_package.Importer importer;
    internal Lookup lookup;
}

[GoRecv] internal static (ж<types.Package>, error) Import(this ref gccgoimports m, @string path) {
    return m.ImportFrom(path, ""u8, /* no vendoring */
 0);
}

[GoRecv] internal static (ж<types.Package>, error) ImportFrom(this ref gccgoimports m, @string path, @string srcDir, types.ImportMode mode) {
    if (mode != 0) {
        throw panic("mode must be 0");
    }
    return m.importer(m.packages, path, srcDir, m.lookup);
}

} // end importer_package
