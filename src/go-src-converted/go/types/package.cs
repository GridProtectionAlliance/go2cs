// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:47:47 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\package.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // A Package describes a Go package.
        public partial struct Package
        {
            public @string path;
            public @string name;
            public ptr<Scope> scope;
            public bool complete;
            public slice<ref Package> imports;
            public bool fake; // scope lookup errors are silently dropped if package is fake (internal use only)
        }

        // NewPackage returns a new Package for the given package path and name.
        // The package is not complete and contains no explicit imports.
        public static ref Package NewPackage(@string path, @string name)
        {
            var scope = NewScope(Universe, token.NoPos, token.NoPos, fmt.Sprintf("package %q", path));
            return ref new Package(path:path,name:name,scope:scope);
        }

        // Path returns the package path.
        private static @string Path(this ref Package pkg)
        {
            return pkg.path;
        }

        // Name returns the package name.
        private static @string Name(this ref Package pkg)
        {
            return pkg.name;
        }

        // SetName sets the package name.
        private static void SetName(this ref Package pkg, @string name)
        {
            pkg.name = name;

        }

        // Scope returns the (complete or incomplete) package scope
        // holding the objects declared at package level (TypeNames,
        // Consts, Vars, and Funcs).
        private static ref Scope Scope(this ref Package pkg)
        {
            return pkg.scope;
        }

        // A package is complete if its scope contains (at least) all
        // exported objects; otherwise it is incomplete.
        private static bool Complete(this ref Package pkg)
        {
            return pkg.complete;
        }

        // MarkComplete marks a package as complete.
        private static void MarkComplete(this ref Package pkg)
        {
            pkg.complete = true;

        }

        // Imports returns the list of packages directly imported by
        // pkg; the list is in source order.
        //
        // If pkg was loaded from export data, Imports includes packages that
        // provide package-level objects referenced by pkg. This may be more or
        // less than the set of packages directly imported by pkg's source code.
        private static slice<ref Package> Imports(this ref Package pkg)
        {
            return pkg.imports;
        }

        // SetImports sets the list of explicitly imported packages to list.
        // It is the caller's responsibility to make sure list elements are unique.
        private static void SetImports(this ref Package pkg, slice<ref Package> list)
        {
            pkg.imports = list;

        }

        private static @string String(this ref Package pkg)
        {
            return fmt.Sprintf("package %s (%q)", pkg.name, pkg.path);
        }
    }
}}
