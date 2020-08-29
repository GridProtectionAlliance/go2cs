// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:47:52 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\resolver.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // A declInfo describes a package-level const, type, var, or func declaration.
        private partial struct declInfo
        {
            public ptr<Scope> file; // scope of file containing this declaration
            public slice<ref Var> lhs; // lhs of n:1 variable declarations, or nil
            public ast.Expr typ; // type, or nil
            public ast.Expr init; // init/orig expression, or nil
            public ptr<ast.FuncDecl> fdecl; // func declaration, or nil
            public bool alias; // type alias declaration

// The deps field tracks initialization expression dependencies.
// As a special (overloaded) case, it also tracks dependencies of
// interface types on embedded interfaces (see ordering.go).
            public objSet deps; // lazily initialized
        }

        // An objSet is simply a set of objects.
        private partial struct objSet // : map<Object, bool>
        {
        }

        // hasInitializer reports whether the declared object has an initialization
        // expression or function body.
        private static bool hasInitializer(this ref declInfo d)
        {
            return d.init != null || d.fdecl != null && d.fdecl.Body != null;
        }

        // addDep adds obj to the set of objects d's init expression depends on.
        private static void addDep(this ref declInfo d, Object obj)
        {
            var m = d.deps;
            if (m == null)
            {
                m = make(objSet);
                d.deps = m;
            }
            m[obj] = true;
        }

        // arityMatch checks that the lhs and rhs of a const or var decl
        // have the appropriate number of names and init exprs. For const
        // decls, init is the value spec providing the init exprs; for
        // var decls, init is nil (the init exprs are in s in this case).
        private static void arityMatch(this ref Checker check, ref ast.ValueSpec s, ref ast.ValueSpec init)
        {
            var l = len(s.Names);
            var r = len(s.Values);
            if (init != null)
            {
                r = len(init.Values);
            }

            if (init == null && r == 0L) 
                // var decl w/o init expr
                if (s.Type == null)
                {
                    check.errorf(s.Pos(), "missing type or init expr");
                }
            else if (l < r) 
                if (l < len(s.Values))
                { 
                    // init exprs from s
                    var n = s.Values[l];
                    check.errorf(n.Pos(), "extra init expr %s", n); 
                    // TODO(gri) avoid declared but not used error here
                }
                else
                { 
                    // init exprs "inherited"
                    check.errorf(s.Pos(), "extra init expr at %s", check.fset.Position(init.Pos())); 
                    // TODO(gri) avoid declared but not used error here
                }
            else if (l > r && (init != null || r != 1L)) 
                n = s.Names[r];
                check.errorf(n.Pos(), "missing init expr for %s", n);
                    }

        private static (@string, error) validatedImportPath(@string path)
        {
            var (s, err) = strconv.Unquote(path);
            if (err != null)
            {
                return ("", err);
            }
            if (s == "")
            {
                return ("", fmt.Errorf("empty string"));
            }
            const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

            foreach (var (_, r) in s)
            {
                if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r))
                {
                    return (s, fmt.Errorf("invalid character %#U", r));
                }
            }
            return (s, null);
        }

        // declarePkgObj declares obj in the package scope, records its ident -> obj mapping,
        // and updates check.objMap. The object must not be a function or method.
        private static void declarePkgObj(this ref Checker check, ref ast.Ident ident, Object obj, ref declInfo d)
        {
            assert(ident.Name == obj.Name()); 

            // spec: "A package-scope or file-scope identifier with name init
            // may only be declared to be a function with this (func()) signature."
            if (ident.Name == "init")
            {
                check.errorf(ident.Pos(), "cannot declare init - must be func");
                return;
            } 

            // spec: "The main package must have package name main and declare
            // a function main that takes no arguments and returns no value."
            if (ident.Name == "main" && check.pkg.name == "main")
            {
                check.errorf(ident.Pos(), "cannot declare main - must be func");
                return;
            }
            check.declare(check.pkg.scope, ident, obj, token.NoPos);
            check.objMap[obj] = d;
            obj.setOrder(uint32(len(check.objMap)));
        }

        // filename returns a filename suitable for debugging output.
        private static @string filename(this ref Checker check, long fileNo)
        {
            var file = check.files[fileNo];
            {
                var pos = file.Pos();

                if (pos.IsValid())
                {
                    return check.fset.File(pos).Name();
                }

            }
            return fmt.Sprintf("file[%d]", fileNo);
        }

        private static ref Package importPackage(this ref Checker check, token.Pos pos, @string path, @string dir)
        { 
            // If we already have a package for the given (path, dir)
            // pair, use it instead of doing a full import.
            // Checker.impMap only caches packages that are marked Complete
            // or fake (dummy packages for failed imports). Incomplete but
            // non-fake packages do require an import to complete them.
            importKey key = new importKey(path,dir);
            var imp = check.impMap[key];
            if (imp != null)
            {
                return imp;
            } 

            // no package yet => import it
            if (path == "C" && check.conf.FakeImportC)
            {
                imp = NewPackage("C", "C");
                imp.fake = true;
            }
            else
            { 
                // ordinary import
                error err = default;
                {
                    var importer = check.conf.Importer;

                    if (importer == null)
                    {
                        err = error.As(fmt.Errorf("Config.Importer not installed"));
                    }                    {
                        ImporterFrom (importerFrom, ok) = importer._<ImporterFrom>();


                        else if (ok)
                        {
                            imp, err = importerFrom.ImportFrom(path, dir, 0L);
                            if (imp == null && err == null)
                            {
                                err = error.As(fmt.Errorf("Config.Importer.ImportFrom(%s, %s, 0) returned nil but no error", path, dir));
                            }
                        }
                        else
                        {
                            imp, err = importer.Import(path);
                            if (imp == null && err == null)
                            {
                                err = error.As(fmt.Errorf("Config.Importer.Import(%s) returned nil but no error", path));
                            }
                        } 
                        // make sure we have a valid package name
                        // (errors here can only happen through manipulation of packages after creation)

                    } 
                    // make sure we have a valid package name
                    // (errors here can only happen through manipulation of packages after creation)

                } 
                // make sure we have a valid package name
                // (errors here can only happen through manipulation of packages after creation)
                if (err == null && imp != null && (imp.name == "_" || imp.name == ""))
                {
                    err = error.As(fmt.Errorf("invalid package name: %q", imp.name));
                    imp = null; // create fake package below
                }
                if (err != null)
                {
                    check.errorf(pos, "could not import %s (%s)", path, err);
                    if (imp == null)
                    { 
                        // create a new fake package
                        // come up with a sensible package name (heuristic)
                        var name = path;
                        {
                            var i__prev4 = i;

                            var i = len(name);

                            if (i > 0L && name[i - 1L] == '/')
                            {
                                name = name[..i - 1L];
                            }

                            i = i__prev4;

                        }
                        {
                            var i__prev4 = i;

                            i = strings.LastIndex(name, "/");

                            if (i >= 0L)
                            {
                                name = name[i + 1L..];
                            }

                            i = i__prev4;

                        }
                        imp = NewPackage(path, name);
                    } 
                    // continue to use the package as best as we can
                    imp.fake = true; // avoid follow-up lookup failures
                }
            } 

            // package should be complete or marked fake, but be cautious
            if (imp.complete || imp.fake)
            {
                check.impMap[key] = imp;
                return imp;
            } 

            // something went wrong (importer may have returned incomplete package without error)
            return null;
        }

        // collectObjects collects all file and package objects and inserts them
        // into their respective scopes. It also performs imports and associates
        // methods with receiver base type names.
        private static void collectObjects(this ref Checker check)
        {
            var pkg = check.pkg; 

            // pkgImports is the set of packages already imported by any package file seen
            // so far. Used to avoid duplicate entries in pkg.imports. Allocate and populate
            // it (pkg.imports may not be empty if we are checking test files incrementally).
            // Note that pkgImports is keyed by package (and thus package path), not by an
            // importKey value. Two different importKey values may map to the same package
            // which is why we cannot use the check.impMap here.
            var pkgImports = make_map<ref Package, bool>();
            {
                var imp__prev1 = imp;

                foreach (var (_, __imp) in pkg.imports)
                {
                    imp = __imp;
                    pkgImports[imp] = true;
                }

                imp = imp__prev1;
            }

            foreach (var (fileNo, file) in check.files)
            { 
                // The package identifier denotes the current package,
                // but there is no corresponding package object.
                check.recordDef(file.Name, null); 

                // Use the actual source file extent rather than *ast.File extent since the
                // latter doesn't include comments which appear at the start or end of the file.
                // Be conservative and use the *ast.File extent if we don't have a *token.File.
                var pos = file.Pos();
                var end = file.End();
                {
                    var f = check.fset.File(file.Pos());

                    if (f != null)
                    {
                        pos = token.Pos(f.Base());
                        end = token.Pos(f.Base() + f.Size());
                    }

                }
                var fileScope = NewScope(check.pkg.scope, pos, end, check.filename(fileNo));
                check.recordScope(file, fileScope); 

                // determine file directory, necessary to resolve imports
                // FileName may be "" (typically for tests) in which case
                // we get "." as the directory which is what we would want.
                var fileDir = dir(check.fset.Position(file.Name.Pos()).Filename);

                foreach (var (_, decl) in file.Decls)
                {
                    switch (decl.type())
                    {
                        case ref ast.BadDecl d:
                            break;
                        case ref ast.GenDecl d:
                            ref ast.ValueSpec last = default; // last ValueSpec with type or init exprs seen
                            foreach (var (iota, spec) in d.Specs)
                            {
                                switch (spec.type())
                                {
                                    case ref ast.ImportSpec s:
                                        var (path, err) = validatedImportPath(s.Path.Value);
                                        if (err != null)
                                        {
                                            check.errorf(s.Path.Pos(), "invalid import path (%s)", err);
                                            continue;
                                        }
                                        var imp = check.importPackage(s.Path.Pos(), path, fileDir);
                                        if (imp == null)
                                        {
                                            continue;
                                        } 

                                        // add package to list of explicit imports
                                        // (this functionality is provided as a convenience
                                        // for clients; it is not needed for type-checking)
                                        if (!pkgImports[imp])
                                        {
                                            pkgImports[imp] = true;
                                            pkg.imports = append(pkg.imports, imp);
                                        } 

                                        // local name overrides imported package name
                                        var name = imp.name;
                                        if (s.Name != null)
                                        {
                                            name = s.Name.Name;
                                            if (path == "C")
                                            { 
                                                // match cmd/compile (not prescribed by spec)
                                                check.errorf(s.Name.Pos(), "cannot rename import \"C\"");
                                                continue;
                                            }
                                            if (name == "init")
                                            {
                                                check.errorf(s.Name.Pos(), "cannot declare init - must be func");
                                                continue;
                                            }
                                        }
                                        var obj = NewPkgName(s.Pos(), pkg, name, imp);
                                        if (s.Name != null)
                                        { 
                                            // in a dot-import, the dot represents the package
                                            check.recordDef(s.Name, obj);
                                        }
                                        else
                                        {
                                            check.recordImplicit(s, obj);
                                        }
                                        if (path == "C")
                                        { 
                                            // match cmd/compile (not prescribed by spec)
                                            obj.used = true;
                                        } 

                                        // add import to file scope
                                        if (name == ".")
                                        { 
                                            // merge imported scope with file scope
                                            {
                                                var obj__prev4 = obj;

                                                foreach (var (_, __obj) in imp.scope.elems)
                                                {
                                                    obj = __obj; 
                                                    // A package scope may contain non-exported objects,
                                                    // do not import them!
                                                    if (obj.Exported())
                                                    { 
                                                        // TODO(gri) When we import a package, we create
                                                        // a new local package object. We should do the
                                                        // same for each dot-imported object. That way
                                                        // they can have correct position information.
                                                        // (We must not modify their existing position
                                                        // information because the same package - found
                                                        // via Config.Packages - may be dot-imported in
                                                        // another package!)
                                                        check.declare(fileScope, null, obj, token.NoPos);
                                                    }
                                                }
                                        else
 
                                                // add position to set of dot-import positions for this file
                                                // (this is only needed for "imported but not used" errors)

                                                obj = obj__prev4;
                                            }

                                            check.addUnusedDotImport(fileScope, imp, s.Pos());
                                        }                                        { 
                                            // declare imported package object in file scope
                                            check.declare(fileScope, null, obj, token.NoPos);
                                        }
                                        break;
                                    case ref ast.ValueSpec s:

                                        if (d.Tok == token.CONST) 
                                            // determine which initialization expressions to use

                                            if (s.Type != null || len(s.Values) > 0L) 
                                                last = s;
                                            else if (last == null) 
                                                last = @new<ast.ValueSpec>(); // make sure last exists
                                            // declare all constants
                                            {
                                                var i__prev4 = i;
                                                var name__prev4 = name;

                                                foreach (var (__i, __name) in s.Names)
                                                {
                                                    i = __i;
                                                    name = __name;
                                                    obj = NewConst(name.Pos(), pkg, name.Name, null, constant.MakeInt64(int64(iota)));

                                                    ast.Expr init = default;
                                                    if (i < len(last.Values))
                                                    {
                                                        init = last.Values[i];
                                                    }
                                                    declInfo d = ref new declInfo(file:fileScope,typ:last.Type,init:init);
                                                    check.declarePkgObj(name, obj, d);
                                                }

                                                i = i__prev4;
                                                name = name__prev4;
                                            }

                                            check.arityMatch(s, last);
                                        else if (d.Tok == token.VAR) 
                                            var lhs = make_slice<ref Var>(len(s.Names)); 
                                            // If there's exactly one rhs initializer, use
                                            // the same declInfo d1 for all lhs variables
                                            // so that each lhs variable depends on the same
                                            // rhs initializer (n:1 var declaration).
                                            ref declInfo d1 = default;
                                            if (len(s.Values) == 1L)
                                            { 
                                                // The lhs elements are only set up after the for loop below,
                                                // but that's ok because declareVar only collects the declInfo
                                                // for a later phase.
                                                d1 = ref new declInfo(file:fileScope,lhs:lhs,typ:s.Type,init:s.Values[0]);
                                            } 

                                            // declare all variables
                                            {
                                                var i__prev4 = i;
                                                var name__prev4 = name;

                                                foreach (var (__i, __name) in s.Names)
                                                {
                                                    i = __i;
                                                    name = __name;
                                                    obj = NewVar(name.Pos(), pkg, name.Name, null);
                                                    lhs[i] = obj;

                                                    d = d1;
                                                    if (d == null)
                                                    { 
                                                        // individual assignments
                                                        init = default;
                                                        if (i < len(s.Values))
                                                        {
                                                            init = s.Values[i];
                                                        }
                                                        d = ref new declInfo(file:fileScope,typ:s.Type,init:init);
                                                    }
                                                    check.declarePkgObj(name, obj, d);
                                                }

                                                i = i__prev4;
                                                name = name__prev4;
                                            }

                                            check.arityMatch(s, null);
                                        else 
                                            check.invalidAST(s.Pos(), "invalid token %s", d.Tok);
                                                                                break;
                                    case ref ast.TypeSpec s:
                                        obj = NewTypeName(s.Name.Pos(), pkg, s.Name.Name, null);
                                        check.declarePkgObj(s.Name, obj, ref new declInfo(file:fileScope,typ:s.Type,alias:s.Assign.IsValid()));
                                        break;
                                    default:
                                    {
                                        var s = spec.type();
                                        check.invalidAST(s.Pos(), "unknown ast.Spec node %T", s);
                                        break;
                                    }
                                }
                            }
                            break;
                        case ref ast.FuncDecl d:
                            name = d.Name.Name;
                            obj = NewFunc(d.Name.Pos(), pkg, name, null);
                            if (d.Recv == null)
                            { 
                                // regular function
                                if (name == "init")
                                { 
                                    // don't declare init functions in the package scope - they are invisible
                                    obj.parent = pkg.scope;
                                    check.recordDef(d.Name, obj); 
                                    // init functions must have a body
                                    if (d.Body == null)
                                    {
                                        check.softErrorf(obj.pos, "missing function body");
                                    }
                                }
                                else
                                {
                                    check.declare(pkg.scope, d.Name, obj, token.NoPos);
                                }
                            }
                            else
                            { 
                                // method
                                check.recordDef(d.Name, obj); 
                                // Associate method with receiver base type name, if possible.
                                // Ignore methods that have an invalid receiver, or a blank _
                                // receiver name. They will be type-checked later, with regular
                                // functions.
                                {
                                    var list = d.Recv.List;

                                    if (len(list) > 0L)
                                    {
                                        var typ = unparen(list[0L].Type);
                                        {
                                            ref ast.StarExpr (ptr, _) = typ._<ref ast.StarExpr>();

                                            if (ptr != null)
                                            {
                                                typ = unparen(ptr.X);
                                            }

                                        }
                                        {
                                            ref ast.Ident (base, _) = typ._<ref ast.Ident>();

                                            if (base != null && @base.Name != "_")
                                            {
                                                check.assocMethod(@base.Name, obj);
                                            }

                                        }
                                    }

                                }
                            }
                            declInfo info = ref new declInfo(file:fileScope,fdecl:d);
                            check.objMap[obj] = info;
                            obj.setOrder(uint32(len(check.objMap)));
                            break;
                        default:
                        {
                            var d = decl.type();
                            check.invalidAST(d.Pos(), "unknown ast.Decl node %T", d);
                            break;
                        }
                    }
                }
            } 

            // verify that objects in package and file scopes have different names
            foreach (var (_, scope) in check.pkg.scope.children)
            {
                {
                    var obj__prev2 = obj;

                    foreach (var (_, __obj) in scope.elems)
                    {
                        obj = __obj;
                        {
                            var alt = pkg.scope.Lookup(obj.Name());

                            if (alt != null)
                            {
                                {
                                    var pkg__prev2 = pkg;

                                    ref PkgName (pkg, ok) = obj._<ref PkgName>();

                                    if (ok)
                                    {
                                        check.errorf(alt.Pos(), "%s already declared through import of %s", alt.Name(), pkg.Imported());
                                        check.reportAltDecl(pkg);
                                    }
                                    else
                                    {
                                        check.errorf(alt.Pos(), "%s already declared through dot-import of %s", alt.Name(), obj.Pkg()); 
                                        // TODO(gri) dot-imported objects don't have a position; reportAltDecl won't print anything
                                        check.reportAltDecl(obj);
                                    }

                                    pkg = pkg__prev2;

                                }
                            }

                        }
                    }

                    obj = obj__prev2;
                }

            }
        }

        // packageObjects typechecks all package objects in objList, but not function bodies.
        private static void packageObjects(this ref Checker check, slice<Object> objList)
        { 
            // add new methods to already type-checked types (from a prior Checker.Files call)
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in objList)
                {
                    obj = __obj;
                    {
                        var obj__prev1 = obj;

                        ref TypeName (obj, _) = obj._<ref TypeName>();

                        if (obj != null && obj.typ != null)
                        {
                            check.addMethodDecls(obj);
                        }

                        obj = obj__prev1;

                    }
                } 

                // pre-allocate space for type declaration paths so that the underlying array is reused

                obj = obj__prev1;
            }

            var typePath = make_slice<ref TypeName>(0L, 8L);

            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in objList)
                {
                    obj = __obj;
                    check.objDecl(obj, null, typePath);
                } 

                // At this point we may have a non-empty check.methods map; this means that not all
                // entries were deleted at the end of typeDecl because the respective receiver base
                // types were not found. In that case, an error was reported when declaring those
                // methods. We can now safely discard this map.

                obj = obj__prev1;
            }

            check.methods = null;
        }

        // functionBodies typechecks all function bodies.
        private static void functionBodies(this ref Checker check)
        {
            foreach (var (_, f) in check.funcs)
            {
                check.funcBody(f.decl, f.name, f.sig, f.body);
            }
        }

        // unusedImports checks for unused imports.
        private static void unusedImports(this ref Checker check)
        { 
            // if function bodies are not checked, packages' uses are likely missing - don't check
            if (check.conf.IgnoreFuncBodies)
            {
                return;
            } 

            // spec: "It is illegal (...) to directly import a package without referring to
            // any of its exported identifiers. To import a package solely for its side-effects
            // (initialization), use the blank identifier as explicit package name."

            // check use of regular imported packages
            foreach (var (_, scope) in check.pkg.scope.children)
            {
                {
                    var obj__prev2 = obj;

                    foreach (var (_, __obj) in scope.elems)
                    {
                        obj = __obj;
                        {
                            var obj__prev1 = obj;

                            ref PkgName (obj, ok) = obj._<ref PkgName>();

                            if (ok)
                            { 
                                // Unused "blank imports" are automatically ignored
                                // since _ identifiers are not entered into scopes.
                                if (!obj.used)
                                {
                                    var path = obj.imported.path;
                                    var @base = pkgName(path);
                                    if (obj.name == base)
                                    {
                                        check.softErrorf(obj.pos, "%q imported but not used", path);
                                    }
                                    else
                                    {
                                        check.softErrorf(obj.pos, "%q imported but not used as %s", path, obj.name);
                                    }
                                }
                            }

                            obj = obj__prev1;

                        }
                    }

                    obj = obj__prev2;
                }

            } 

            // check use of dot-imported packages
            foreach (var (_, unusedDotImports) in check.unusedDotImports)
            {
                foreach (var (pkg, pos) in unusedDotImports)
                {
                    check.softErrorf(pos, "%q imported but not used", pkg.path);
                }
            }
        }

        // pkgName returns the package name (last element) of an import path.
        private static @string pkgName(@string path)
        {
            {
                var i = strings.LastIndex(path, "/");

                if (i >= 0L)
                {
                    path = path[i + 1L..];
                }

            }
            return path;
        }

        // dir makes a good-faith attempt to return the directory
        // portion of path. If path is empty, the result is ".".
        // (Per the go/build package dependency tests, we cannot import
        // path/filepath and simply use filepath.Dir.)
        private static @string dir(@string path)
        {
            {
                var i = strings.LastIndexAny(path, "/\\");

                if (i > 0L)
                {
                    return path[..i];
                } 
                // i <= 0

            } 
            // i <= 0
            return ".";
        }
    }
}}
