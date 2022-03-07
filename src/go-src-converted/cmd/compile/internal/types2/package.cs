// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types2 -- go2cs converted at 2022 March 06 23:12:46 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\package.go
using fmt = go.fmt_package;

namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // A Package describes a Go package.
public partial struct Package {
    public @string path;
    public @string name;
    public ptr<Scope> scope;
    public bool complete;
    public slice<ptr<Package>> imports;
    public bool fake; // scope lookup errors are silently dropped if package is fake (internal use only)
    public bool cgo; // uses of this package will be rewritten into uses of declarations from _cgo_gotypes.go
}

// NewPackage returns a new Package for the given package path and name.
// The package is not complete and contains no explicit imports.
public static ptr<Package> NewPackage(@string path, @string name) {
    var scope = NewScope(Universe, nopos, nopos, fmt.Sprintf("package %q", path));
    return addr(new Package(path:path,name:name,scope:scope));
}

// Path returns the package path.
private static @string Path(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.path;
}

// Name returns the package name.
private static @string Name(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.name;
}

// SetName sets the package name.
private static void SetName(this ptr<Package> _addr_pkg, @string name) {
    ref Package pkg = ref _addr_pkg.val;

    pkg.name = name;
}

// Scope returns the (complete or incomplete) package scope
// holding the objects declared at package level (TypeNames,
// Consts, Vars, and Funcs).
private static ptr<Scope> Scope(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return _addr_pkg.scope!;
}

// A package is complete if its scope contains (at least) all
// exported objects; otherwise it is incomplete.
private static bool Complete(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.complete;
}

// MarkComplete marks a package as complete.
private static void MarkComplete(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    pkg.complete = true;
}

// Imports returns the list of packages directly imported by
// pkg; the list is in source order.
//
// If pkg was loaded from export data, Imports includes packages that
// provide package-level objects referenced by pkg. This may be more or
// less than the set of packages directly imported by pkg's source code.
private static slice<ptr<Package>> Imports(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.imports;
}

// SetImports sets the list of explicitly imported packages to list.
// It is the caller's responsibility to make sure list elements are unique.
private static void SetImports(this ptr<Package> _addr_pkg, slice<ptr<Package>> list) {
    ref Package pkg = ref _addr_pkg.val;

    pkg.imports = list;
}

private static @string String(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    return fmt.Sprintf("package %s (%q)", pkg.name, pkg.path);
}

} // end types2_package
