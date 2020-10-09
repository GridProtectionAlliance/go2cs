// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 09 05:19:36 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\resolver.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using sort = go.sort_package;
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
            public slice<ptr<Var>> lhs; // lhs of n:1 variable declarations, or nil
            public ast.Expr typ; // type, or nil
            public ast.Expr init; // init/orig expression, or nil
            public ptr<ast.FuncDecl> fdecl; // func declaration, or nil
            public bool alias; // type alias declaration

// The deps field tracks initialization expression dependencies.
            public map<Object, bool> deps; // lazily initialized
        }

        // hasInitializer reports whether the declared object has an initialization
        // expression or function body.
        private static bool hasInitializer(this ptr<declInfo> _addr_d)
        {
            ref declInfo d = ref _addr_d.val;

            return d.init != null || d.fdecl != null && d.fdecl.Body != null;
        }

        // addDep adds obj to the set of objects d's init expression depends on.
        private static void addDep(this ptr<declInfo> _addr_d, Object obj)
        {
            ref declInfo d = ref _addr_d.val;

            var m = d.deps;
            if (m == null)
            {
                m = make_map<Object, bool>();
                d.deps = m;
            }

            m[obj] = true;

        }

        // arityMatch checks that the lhs and rhs of a const or var decl
        // have the appropriate number of names and init exprs. For const
        // decls, init is the value spec providing the init exprs; for
        // var decls, init is nil (the init exprs are in s in this case).
        private static void arityMatch(this ptr<Checker> _addr_check, ptr<ast.ValueSpec> _addr_s, ptr<ast.ValueSpec> _addr_init)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.ValueSpec s = ref _addr_s.val;
            ref ast.ValueSpec init = ref _addr_init.val;

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
            @string _p0 = default;
            error _p0 = default!;

            var (s, err) = strconv.Unquote(path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (s == "")
            {
                return ("", error.As(fmt.Errorf("empty string"))!);
            }

            const @string illegalChars = (@string)"!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

            foreach (var (_, r) in s)
            {
                if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r))
                {
                    return (s, error.As(fmt.Errorf("invalid character %#U", r))!);
                }

            }
            return (s, error.As(null!)!);

        }

        // declarePkgObj declares obj in the package scope, records its ident -> obj mapping,
        // and updates check.objMap. The object must not be a function or method.
        private static void declarePkgObj(this ptr<Checker> _addr_check, ptr<ast.Ident> _addr_ident, Object obj, ptr<declInfo> _addr_d)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.Ident ident = ref _addr_ident.val;
            ref declInfo d = ref _addr_d.val;

            assert(ident.Name == obj.Name()); 

            // spec: "A package-scope or file-scope identifier with name init
            // may only be declared to be a function with this (func()) signature."
            if (ident.Name == "init")
            {
                check.errorf(ident.Pos(), "cannot declare init - must be func");
                return ;
            } 

            // spec: "The main package must have package name main and declare
            // a function main that takes no arguments and returns no value."
            if (ident.Name == "main" && check.pkg.name == "main")
            {
                check.errorf(ident.Pos(), "cannot declare main - must be func");
                return ;
            }

            check.declare(check.pkg.scope, ident, obj, token.NoPos);
            check.objMap[obj] = d;
            obj.setOrder(uint32(len(check.objMap)));

        }

        // filename returns a filename suitable for debugging output.
        private static @string filename(this ptr<Checker> _addr_check, long fileNo)
        {
            ref Checker check = ref _addr_check.val;

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

        private static ptr<Package> importPackage(this ptr<Checker> _addr_check, token.Pos pos, @string path, @string dir)
        {
            ref Checker check = ref _addr_check.val;
 
            // If we already have a package for the given (path, dir)
            // pair, use it instead of doing a full import.
            // Checker.impMap only caches packages that are marked Complete
            // or fake (dummy packages for failed imports). Incomplete but
            // non-fake packages do require an import to complete them.
            importKey key = new importKey(path,dir);
            var imp = check.impMap[key];
            if (imp != null)
            {
                return _addr_imp!;
            } 

            // no package yet => import it
            if (path == "C" && (check.conf.FakeImportC || check.conf.go115UsesCgo))
            {
                imp = NewPackage("C", "C");
                imp.fake = true; // package scope is not populated
                imp.cgo = check.conf.go115UsesCgo;

            }
            else
            { 
                // ordinary import
                error err = default!;
                {
                    var importer = check.conf.Importer;

                    if (importer == null)
                    {
                        err = error.As(fmt.Errorf("Config.Importer not installed"))!;
                    }                    {
                        ImporterFrom (importerFrom, ok) = importer._<ImporterFrom>();


                        else if (ok)
                        {
                            imp, err = importerFrom.ImportFrom(path, dir, 0L);
                            if (imp == null && err == null)
                            {
                                err = error.As(fmt.Errorf("Config.Importer.ImportFrom(%s, %s, 0) returned nil but no error", path, dir))!;
                            }

                        }
                        else
                        {
                            imp, err = importer.Import(path);
                            if (imp == null && err == null)
                            {
                                err = error.As(fmt.Errorf("Config.Importer.Import(%s) returned nil but no error", path))!;
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
                    err = error.As(fmt.Errorf("invalid package name: %q", imp.name))!;
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
                check.pkgCnt[imp.name]++;
                return _addr_imp!;
            } 

            // something went wrong (importer may have returned incomplete package without error)
            return _addr_null!;

        }

        // collectObjects collects all file and package objects and inserts them
        // into their respective scopes. It also performs imports and associates
        // methods with receiver base type names.
        private static void collectObjects(this ptr<Checker> _addr_check)
        {
            ref Checker check = ref _addr_check.val;

            var pkg = check.pkg; 

            // pkgImports is the set of packages already imported by any package file seen
            // so far. Used to avoid duplicate entries in pkg.imports. Allocate and populate
            // it (pkg.imports may not be empty if we are checking test files incrementally).
            // Note that pkgImports is keyed by package (and thus package path), not by an
            // importKey value. Two different importKey values may map to the same package
            // which is why we cannot use the check.impMap here.
            var pkgImports = make_map<ptr<Package>, bool>();
            {
                var imp__prev1 = imp;

                foreach (var (_, __imp) in pkg.imports)
                {
                    imp = __imp;
                    pkgImports[imp] = true;
                }

                imp = imp__prev1;
            }

            slice<ptr<Func>> methods = default; // list of methods with non-blank _ names
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
                    var f__prev1 = f;

                    var f = check.fset.File(file.Pos());

                    if (f != null)
                    {
                        pos = token.Pos(f.Base());
                        end = token.Pos(f.Base() + f.Size());

                    }

                    f = f__prev1;

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
                        case ptr<ast.BadDecl> d:
                            break;
                        case ptr<ast.GenDecl> d:
                            ptr<ast.ValueSpec> last; // last ValueSpec with type or init exprs seen
                            foreach (var (iota, spec) in d.Specs)
                            {
                                switch (spec.type())
                                {
                                    case ptr<ast.ImportSpec> s:
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
                                                        // declare dot-imported object
                                                        // (Do not use check.declare because it modifies the object
                                                        // via Object.setScopePos, which leads to a race condition;
                                                        // the object may be imported into more than one file scope
                                                        // concurrently. See issue #32154.)
                                                        {
                                                            var alt__prev3 = alt;

                                                            var alt = fileScope.Insert(obj);

                                                            if (alt != null)
                                                            {
                                                                check.errorf(s.Name.Pos(), "%s redeclared in this block", obj.Name());
                                                                check.reportAltDecl(alt);
                                                            }

                                                            alt = alt__prev3;

                                                        }

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
                                            // (no need to provide s.Name since we called check.recordDef earlier)
                                            check.declare(fileScope, null, obj, token.NoPos);

                                        }

                                        break;
                                    case ptr<ast.ValueSpec> s:

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

                                                    ptr<declInfo> d = addr(new declInfo(file:fileScope,typ:last.Type,init:init));
                                                    check.declarePkgObj(name, obj, d);

                                                }

                                                i = i__prev4;
                                                name = name__prev4;
                                            }

                                            check.arityMatch(s, last);
                                        else if (d.Tok == token.VAR) 
                                            var lhs = make_slice<ptr<Var>>(len(s.Names)); 
                                            // If there's exactly one rhs initializer, use
                                            // the same declInfo d1 for all lhs variables
                                            // so that each lhs variable depends on the same
                                            // rhs initializer (n:1 var declaration).
                                            ptr<declInfo> d1;
                                            if (len(s.Values) == 1L)
                                            { 
                                                // The lhs elements are only set up after the for loop below,
                                                // but that's ok because declareVar only collects the declInfo
                                                // for a later phase.
                                                d1 = addr(new declInfo(file:fileScope,lhs:lhs,typ:s.Type,init:s.Values[0]));

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

                                                        d = addr(new declInfo(file:fileScope,typ:s.Type,init:init));

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
                                    case ptr<ast.TypeSpec> s:
                                        obj = NewTypeName(s.Name.Pos(), pkg, s.Name.Name, null);
                                        check.declarePkgObj(s.Name, obj, addr(new declInfo(file:fileScope,typ:s.Type,alias:s.Assign.IsValid())));
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
                        case ptr<ast.FuncDecl> d:
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
                                // (Methods with blank _ names are never found; no need to collect
                                // them for later type association. They will still be type-checked
                                // with all the other functions.)
                                if (name != "_")
                                {
                                    methods = append(methods, obj);
                                }

                                check.recordDef(d.Name, obj);

                            }

                            ptr<declInfo> info = addr(new declInfo(file:fileScope,fdecl:d)); 
                            // Methods are not package-level objects but we still track them in the
                            // object map so that we can handle them like regular functions (if the
                            // receiver is invalid); also we need their fdecl info when associating
                            // them with their receiver base type, below.
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
                            var alt__prev1 = alt;

                            alt = pkg.scope.Lookup(obj.Name());

                            if (alt != null)
                            {
                                {
                                    var pkg__prev2 = pkg;

                                    ptr<PkgName> (pkg, ok) = obj._<ptr<PkgName>>();

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

                            alt = alt__prev1;

                        }

                    }

                    obj = obj__prev2;
                }
            } 

            // Now that we have all package scope objects and all methods,
            // associate methods with receiver base type name where possible.
            // Ignore methods that have an invalid receiver. They will be
            // type-checked later, with regular functions.
            if (methods == null)
            {
                return ; // nothing to do
            }

            check.methods = make_map<ptr<TypeName>, slice<ptr<Func>>>();
            {
                var f__prev1 = f;

                foreach (var (_, __f) in methods)
                {
                    f = __f;
                    var fdecl = check.objMap[f].fdecl;
                    {
                        var list = fdecl.Recv.List;

                        if (len(list) > 0L)
                        { 
                            // f is a method.
                            // Determine the receiver base type and associate f with it.
                            var (ptr, base) = check.resolveBaseTypeName(list[0L].Type);
                            if (base != null)
                            {
                                f.hasPtrRecv = ptr;
                                check.methods[base] = append(check.methods[base], f);
                            }

                        }

                    }

                }

                f = f__prev1;
            }
        }

        // resolveBaseTypeName returns the non-alias base type name for typ, and whether
        // there was a pointer indirection to get to it. The base type name must be declared
        // in package scope, and there can be at most one pointer indirection. If no such type
        // name exists, the returned base is nil.
        private static (bool, ptr<TypeName>) resolveBaseTypeName(this ptr<Checker> _addr_check, ast.Expr typ)
        {
            bool ptr = default;
            ptr<TypeName> @base = default!;
            ref Checker check = ref _addr_check.val;
 
            // Algorithm: Starting from a type expression, which may be a name,
            // we follow that type through alias declarations until we reach a
            // non-alias type name. If we encounter anything but pointer types or
            // parentheses we're done. If we encounter more than one pointer type
            // we're done.
            map<ptr<TypeName>, bool> seen = default;
            while (true)
            {
                typ = unparen(typ); 

                // check if we have a pointer type
                {
                    ptr<ast.StarExpr> (pexpr, _) = typ._<ptr<ast.StarExpr>>();

                    if (pexpr != null)
                    { 
                        // if we've already seen a pointer, we're done
                        if (ptr)
                        {
                            return (false, _addr_null!);
                        }

                        ptr = true;
                        typ = unparen(pexpr.X); // continue with pointer base type
                    } 

                    // typ must be a name

                } 

                // typ must be a name
                ptr<ast.Ident> (name, _) = typ._<ptr<ast.Ident>>();
                if (name == null)
                {
                    return (false, _addr_null!);
                } 

                // name must denote an object found in the current package scope
                // (note that dot-imported objects are not in the package scope!)
                var obj = check.pkg.scope.Lookup(name.Name);
                if (obj == null)
                {
                    return (false, _addr_null!);
                } 

                // the object must be a type name...
                ptr<TypeName> (tname, _) = obj._<ptr<TypeName>>();
                if (tname == null)
                {
                    return (false, _addr_null!);
                } 

                // ... which we have not seen before
                if (seen[tname])
                {
                    return (false, _addr_null!);
                } 

                // we're done if tdecl defined tname as a new type
                // (rather than an alias)
                var tdecl = check.objMap[tname]; // must exist for objects in package scope
                if (!tdecl.alias)
                {
                    return (ptr, _addr_tname!);
                } 

                // otherwise, continue resolving
                typ = tdecl.typ;
                if (seen == null)
                {
                    seen = make_map<ptr<TypeName>, bool>();
                }

                seen[tname] = true;

            }


        }

        // packageObjects typechecks all package objects, but not function bodies.
        private static void packageObjects(this ptr<Checker> _addr_check)
        {
            ref Checker check = ref _addr_check.val;
 
            // process package objects in source order for reproducible results
            var objList = make_slice<Object>(len(check.objMap));
            long i = 0L;
            {
                var obj__prev1 = obj;

                foreach (var (__obj) in check.objMap)
                {
                    obj = __obj;
                    objList[i] = obj;
                    i++;
                }

                obj = obj__prev1;
            }

            sort.Sort(inSourceOrder(objList)); 

            // add new methods to already type-checked types (from a prior Checker.Files call)
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in objList)
                {
                    obj = __obj;
                    {
                        var obj__prev1 = obj;

                        ptr<TypeName> (obj, _) = obj._<ptr<TypeName>>();

                        if (obj != null && obj.typ != null)
                        {
                            check.addMethodDecls(obj);
                        }

                        obj = obj__prev1;

                    }

                } 

                // We process non-alias declarations first, in order to avoid situations where
                // the type of an alias declaration is needed before it is available. In general
                // this is still not enough, as it is possible to create sufficiently convoluted
                // recursive type definitions that will cause a type alias to be needed before it
                // is available (see issue #25838 for examples).
                // As an aside, the cmd/compiler suffers from the same problem (#25838).

                obj = obj__prev1;
            }

            slice<ptr<TypeName>> aliasList = default; 
            // phase 1
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in objList)
                {
                    obj = __obj; 
                    // If we have a type alias, collect it for the 2nd phase.
                    {
                        ptr<TypeName> (tname, _) = obj._<ptr<TypeName>>();

                        if (tname != null && check.objMap[tname].alias)
                        {
                            aliasList = append(aliasList, tname);
                            continue;
                        }

                    }


                    check.objDecl(obj, null);

                } 
                // phase 2

                obj = obj__prev1;
            }

            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in aliasList)
                {
                    obj = __obj;
                    check.objDecl(obj, null);
                } 

                // At this point we may have a non-empty check.methods map; this means that not all
                // entries were deleted at the end of typeDecl because the respective receiver base
                // types were not found. In that case, an error was reported when declaring those
                // methods. We can now safely discard this map.

                obj = obj__prev1;
            }

            check.methods = null;

        }

        // inSourceOrder implements the sort.Sort interface.
        private partial struct inSourceOrder // : slice<Object>
        {
        }

        private static long Len(this inSourceOrder a)
        {
            return len(a);
        }
        private static bool Less(this inSourceOrder a, long i, long j)
        {
            return a[i].order() < a[j].order();
        }
        private static void Swap(this inSourceOrder a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];
        }

        // unusedImports checks for unused imports.
        private static void unusedImports(this ptr<Checker> _addr_check)
        {
            ref Checker check = ref _addr_check.val;
 
            // if function bodies are not checked, packages' uses are likely missing - don't check
            if (check.conf.IgnoreFuncBodies)
            {
                return ;
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

                            ptr<PkgName> (obj, ok) = obj._<ptr<PkgName>>();

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
