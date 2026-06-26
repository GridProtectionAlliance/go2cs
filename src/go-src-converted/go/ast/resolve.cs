// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements NewPackage.
namespace go.go;

using fmt = fmt_package;
using scanner = go.scanner_package;
using token = go.token_package;
using strconv = strconv_package;
using ꓸꓸꓸany = Span<any>;

partial class ast_package {

[GoType] partial struct pkgBuilder {
    internal ж<go.token_package.FileSet> fset;
    internal go.scanner_package.ErrorList errors;
}

[GoRecv] internal static void error(this ref pkgBuilder p, tokenꓸPos pos, @string msg) {
    p.errors.Add(p.fset.Position(pos), msg);
}

[GoRecv] internal static void errorf(this ref pkgBuilder p, tokenꓸPos pos, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    p.error(pos, fmt.Sprintf(format, args.ꓸꓸꓸ));
}

[GoRecv] internal static void declare(this ref pkgBuilder p, ж<Scope> Ꮡscope, ж<Scope> ᏑaltScope, ж<Object> Ꮡobj) {
    ref var scope = ref Ꮡscope.val;
    ref var altScope = ref ᏑaltScope.val;
    ref var obj = ref Ꮡobj.val;

    var alt = scope.Insert(Ꮡobj);
    if (alt == nil && altScope != nil) {
        // see if there is a conflicting declaration in altScope
        alt = altScope.Lookup(obj.Name);
    }
    if (alt != nil) {
        @string prevDecl = ""u8;
        {
            tokenꓸPos pos = alt.Pos(); if (pos.IsValid()) {
                prevDecl = fmt.Sprintf("\n\tprevious declaration at %s"u8, p.fset.Position(pos));
            }
        }
        p.error(obj.Pos(), fmt.Sprintf("%s redeclared in this block%s"u8, obj.Name, prevDecl));
    }
}

internal static bool resolve(ж<Scope> Ꮡscope, ж<Ident> Ꮡident) {
    ref var scope = ref Ꮡscope.val;
    ref var ident = ref Ꮡident.val;

    for (; scope != nil; scope = scope.Outer) {
        {
            var obj = scope.Lookup(ident.Name); if (obj != nil) {
                ident.Obj = obj;
                return true;
            }
        }
    }
    return false;
}

public delegate (ж<Object> pkg, error err) Importer(map<@string, ж<Object>> imports, @string path);

// NewPackage creates a new [Package] node from a set of [File] nodes. It resolves
// unresolved identifiers across files and updates each file's Unresolved list
// accordingly. If a non-nil importer and universe scope are provided, they are
// used to resolve identifiers not declared in any of the package files. Any
// remaining unresolved identifiers are reported as undeclared. If the files
// belong to different packages, one package name is selected and files with
// different package names are reported and then ignored.
// The result is a package node and a [scanner.ErrorList] if there were errors.
//
// Deprecated: use the type checker [go/types] instead; see [Object].
public static (ж<Package>, error) NewPackage(ж<token.FileSet> Ꮡfset, map<@string, ж<File>> files, Importer importer, ж<Scope> Ꮡuniverse) {
    ref var fset = ref Ꮡfset.val;
    ref var universe = ref Ꮡuniverse.val;

    pkgBuilder p = default!;
    p.fset = fset;
    // complete package scope
    @string pkgName = ""u8;
    var pkgScope = NewScope(Ꮡuniverse);
    foreach (var (_, file) in files) {
        // package names must match
        {
            @string name = (~file).Name.val.Name;
            switch (ᐧ) {
            case {} when pkgName == ""u8: {
                pkgName = name;
                break;
            }
            case {} when name is != pkgName: {
                p.errorf((~file).Package, "package %s; expected %s"u8, name, pkgName);
                continue;
                break;
            }}
        }

        // ignore this file
        // collect top-level file objects in package scope
        foreach (var (_, obj) in (~(~file).Scope).Objects) {
            p.declare(pkgScope, nil, obj);
        }
    }
    // package global mapping of imported package ids to package objects
    var imports = new map<@string, ж<Object>>();
    // complete file scopes with imports and resolve identifiers
    foreach (var (_, file) in files) {
        // ignore file if it belongs to a different package
        // (error has already been reported)
        if ((~(~file).Name).Name != pkgName) {
            continue;
        }
        // build file scope by processing all imports
        var importErrors = false;
        var fileScope = NewScope(pkgScope);
        foreach (var (_, spec) in (~file).Imports) {
            if (importer == default!) {
                importErrors = true;
                continue;
            }
            var (path, _) = strconv.Unquote((~(~spec).Path).Value);
            var pkg = importer(imports, path);
            var err = importer(imports, path);
            if (err != default!) {
                p.errorf((~spec).Path.Pos(), "could not import %s (%s)"u8, path, err);
                importErrors = true;
                continue;
            }
            // TODO(gri) If a local package name != "." is provided,
            // global identifier resolution could proceed even if the
            // import failed. Consider adjusting the logic here a bit.
            // local name overrides imported package name
            @string name = pkg.val.Name;
            if ((~spec).Name != nil) {
                name = (~spec).Name.val.Name;
            }
            // add import to file scope
            if (name == "."u8){
                // merge imported scope with file scope
                foreach (var (_, obj) in (~pkg).Data._<Scope.val>().Objects) {
                    p.declare(fileScope, pkgScope, obj);
                }
            } else 
            if (name != "_"u8) {
                // declare imported package object in file scope
                // (do not re-use pkg in the file scope but create
                // a new object instead; the Decl field is different
                // for different files)
                var obj = NewObj(Pkg, name);
                obj.val.Decl = spec;
                obj.val.Data = pkg.val.Data;
                p.declare(fileScope, pkgScope, obj);
            }
        }
        // resolve identifiers
        if (importErrors) {
            // don't use the universe scope without correct imports
            // (objects in the universe may be shadowed by imports;
            // with missing imports, identifiers might get resolved
            // incorrectly to universe objects)
            pkgScope.val.Outer = default!;
        }
        nint i = 0;
        foreach (var (_, ident) in (~file).Unresolved) {
            if (!resolve(fileScope, ident)) {
                p.errorf(ident.Pos(), "undeclared name: %s"u8, (~ident).Name);
                (~file).Unresolved[i] = ident;
                i++;
            }
        }
        file.val.Unresolved = (~file).Unresolved[0..(int)(i)];
        pkgScope.val.Outer = universe;
    }
    // reset universe scope
    p.errors.Sort();
    return (Ꮡ(new Package(pkgName, pkgScope, imports, files)), p.errors.Err());
}

} // end ast_package
