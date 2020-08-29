// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements NewPackage.

// package ast -- go2cs converted at 2020 August 29 08:48:35 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\resolve.go
using fmt = go.fmt_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        private partial struct pkgBuilder
        {
            public ptr<token.FileSet> fset;
            public scanner.ErrorList errors;
        }

        private static void error(this ref pkgBuilder p, token.Pos pos, @string msg)
        {
            p.errors.Add(p.fset.Position(pos), msg);
        }

        private static void errorf(this ref pkgBuilder p, token.Pos pos, @string format, params object[] args)
        {
            p.error(pos, fmt.Sprintf(format, args));
        }

        private static void declare(this ref pkgBuilder p, ref Scope scope, ref Scope altScope, ref Object obj)
        {
            var alt = scope.Insert(obj);
            if (alt == null && altScope != null)
            { 
                // see if there is a conflicting declaration in altScope
                alt = altScope.Lookup(obj.Name);
            }
            if (alt != null)
            {
                @string prevDecl = "";
                {
                    var pos = alt.Pos();

                    if (pos.IsValid())
                    {
                        prevDecl = fmt.Sprintf("\n\tprevious declaration at %s", p.fset.Position(pos));
                    }

                }
                p.error(obj.Pos(), fmt.Sprintf("%s redeclared in this block%s", obj.Name, prevDecl));
            }
        }

        private static bool resolve(ref Scope scope, ref Ident ident)
        {
            while (scope != null)
            {
                {
                    var obj = scope.Lookup(ident.Name);

                    if (obj != null)
                    {
                        ident.Obj = obj;
                        return true;
                scope = scope.Outer;
                    }

                }
            }

            return false;
        }

        // An Importer resolves import paths to package Objects.
        // The imports map records the packages already imported,
        // indexed by package id (canonical import path).
        // An Importer must determine the canonical import path and
        // check the map to see if it is already present in the imports map.
        // If so, the Importer can return the map entry. Otherwise, the
        // Importer should load the package data for the given path into
        // a new *Object (pkg), record pkg in the imports map, and then
        // return pkg.
        public delegate  error) Importer(map<@string,  ref Object>,  @string,  (ref Object);

        // NewPackage creates a new Package node from a set of File nodes. It resolves
        // unresolved identifiers across files and updates each file's Unresolved list
        // accordingly. If a non-nil importer and universe scope are provided, they are
        // used to resolve identifiers not declared in any of the package files. Any
        // remaining unresolved identifiers are reported as undeclared. If the files
        // belong to different packages, one package name is selected and files with
        // different package names are reported and then ignored.
        // The result is a package node and a scanner.ErrorList if there were errors.
        //
        public static (ref Package, error) NewPackage(ref token.FileSet fset, map<@string, ref File> files, Importer importer, ref Scope universe)
        {
            pkgBuilder p = default;
            p.fset = fset; 

            // complete package scope
            @string pkgName = "";
            var pkgScope = NewScope(universe);
            {
                var file__prev1 = file;

                foreach (var (_, __file) in files)
                {
                    file = __file; 
                    // package names must match
                    {
                        var name__prev1 = name;

                        var name = file.Name.Name;


                        if (pkgName == "") 
                            pkgName = name;
                        else if (name != pkgName) 
                            p.errorf(file.Package, "package %s; expected %s", name, pkgName);
                            continue; // ignore this file


                        name = name__prev1;
                    } 

                    // collect top-level file objects in package scope
                    {
                        var obj__prev2 = obj;

                        foreach (var (_, __obj) in file.Scope.Objects)
                        {
                            obj = __obj;
                            p.declare(pkgScope, null, obj);
                        }

                        obj = obj__prev2;
                    }

                } 

                // package global mapping of imported package ids to package objects

                file = file__prev1;
            }

            var imports = make_map<@string, ref Object>(); 

            // complete file scopes with imports and resolve identifiers
            {
                var file__prev1 = file;

                foreach (var (_, __file) in files)
                {
                    file = __file; 
                    // ignore file if it belongs to a different package
                    // (error has already been reported)
                    if (file.Name.Name != pkgName)
                    {
                        continue;
                    } 

                    // build file scope by processing all imports
                    var importErrors = false;
                    var fileScope = NewScope(pkgScope);
                    foreach (var (_, spec) in file.Imports)
                    {
                        if (importer == null)
                        {
                            importErrors = true;
                            continue;
                        }
                        var (path, _) = strconv.Unquote(spec.Path.Value);
                        var (pkg, err) = importer(imports, path);
                        if (err != null)
                        {
                            p.errorf(spec.Path.Pos(), "could not import %s (%s)", path, err);
                            importErrors = true;
                            continue;
                        } 
                        // TODO(gri) If a local package name != "." is provided,
                        // global identifier resolution could proceed even if the
                        // import failed. Consider adjusting the logic here a bit.

                        // local name overrides imported package name
                        name = pkg.Name;
                        if (spec.Name != null)
                        {
                            name = spec.Name.Name;
                        } 

                        // add import to file scope
                        if (name == ".")
                        { 
                            // merge imported scope with file scope
                            {
                                var obj__prev3 = obj;

                                foreach (var (_, __obj) in pkg.Data._<ref Scope>().Objects)
                                {
                                    obj = __obj;
                                    p.declare(fileScope, pkgScope, obj);
                                }

                                obj = obj__prev3;
                            }

                        }
                        else if (name != "_")
                        { 
                            // declare imported package object in file scope
                            // (do not re-use pkg in the file scope but create
                            // a new object instead; the Decl field is different
                            // for different files)
                            var obj = NewObj(Pkg, name);
                            obj.Decl = spec;
                            obj.Data = pkg.Data;
                            p.declare(fileScope, pkgScope, obj);
                        }
                    } 

                    // resolve identifiers
                    if (importErrors)
                    { 
                        // don't use the universe scope without correct imports
                        // (objects in the universe may be shadowed by imports;
                        // with missing imports, identifiers might get resolved
                        // incorrectly to universe objects)
                        pkgScope.Outer = null;
                    }
                    long i = 0L;
                    foreach (var (_, ident) in file.Unresolved)
                    {
                        if (!resolve(fileScope, ident))
                        {
                            p.errorf(ident.Pos(), "undeclared name: %s", ident.Name);
                            file.Unresolved[i] = ident;
                            i++;
                        }
                    }
                    file.Unresolved = file.Unresolved[0L..i];
                    pkgScope.Outer = universe; // reset universe scope
                }

                file = file__prev1;
            }

            p.errors.Sort();
            return (ref new Package(pkgName,pkgScope,imports,files), p.errors.Err());
        }
    }
}}
