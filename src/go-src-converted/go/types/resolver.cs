// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:42:12 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\resolver.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using typeparams = go.go.@internal.typeparams_package;
using token = go.go.token_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using System;


namespace go.go;

public static partial class types_package {

    // A declInfo describes a package-level const, type, var, or func declaration.
private partial struct declInfo {
    public ptr<Scope> file; // scope of file containing this declaration
    public slice<ptr<Var>> lhs; // lhs of n:1 variable declarations, or nil
    public ast.Expr vtyp; // type, or nil (for const and var declarations only)
    public ast.Expr init; // init/orig expression, or nil (for const and var declarations only)
    public bool inherited; // if set, the init expression is inherited from a previous constant declaration
    public ptr<ast.TypeSpec> tdecl; // type declaration, or nil
    public ptr<ast.FuncDecl> fdecl; // func declaration, or nil

// The deps field tracks initialization expression dependencies.
    public map<Object, bool> deps; // lazily initialized
}

// hasInitializer reports whether the declared object has an initialization
// expression or function body.
private static bool hasInitializer(this ptr<declInfo> _addr_d) {
    ref declInfo d = ref _addr_d.val;

    return d.init != null || d.fdecl != null && d.fdecl.Body != null;
}

// addDep adds obj to the set of objects d's init expression depends on.
private static void addDep(this ptr<declInfo> _addr_d, Object obj) {
    ref declInfo d = ref _addr_d.val;

    var m = d.deps;
    if (m == null) {
        m = make_map<Object, bool>();
        d.deps = m;
    }
    m[obj] = true;

}

// arityMatch checks that the lhs and rhs of a const or var decl
// have the appropriate number of names and init exprs. For const
// decls, init is the value spec providing the init exprs; for
// var decls, init is nil (the init exprs are in s in this case).
private static void arityMatch(this ptr<Checker> _addr_check, ptr<ast.ValueSpec> _addr_s, ptr<ast.ValueSpec> _addr_init) {
    ref Checker check = ref _addr_check.val;
    ref ast.ValueSpec s = ref _addr_s.val;
    ref ast.ValueSpec init = ref _addr_init.val;

    var l = len(s.Names);
    var r = len(s.Values);
    if (init != null) {
        r = len(init.Values);
    }
    const var code = _WrongAssignCount;


    if (init == null && r == 0) 
        // var decl w/o init expr
        if (s.Type == null) {
            check.errorf(s, code, "missing type or init expr");
        }
    else if (l < r) 
        if (l < len(s.Values)) { 
            // init exprs from s
            var n = s.Values[l];
            check.errorf(n, code, "extra init expr %s", n); 
            // TODO(gri) avoid declared but not used error here
        }
        else
 { 
            // init exprs "inherited"
            check.errorf(s, code, "extra init expr at %s", check.fset.Position(init.Pos())); 
            // TODO(gri) avoid declared but not used error here
        }
    else if (l > r && (init != null || r != 1)) 
        n = s.Names[r];
        check.errorf(n, code, "missing init expr for %s", n);
    
}

private static (@string, error) validatedImportPath(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    var (s, err) = strconv.Unquote(path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (s == "") {
        return ("", error.As(fmt.Errorf("empty string"))!);
    }
    const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return (s, error.As(fmt.Errorf("invalid character %#U", r))!);
        }
    }    return (s, error.As(null!)!);

}

// declarePkgObj declares obj in the package scope, records its ident -> obj mapping,
// and updates check.objMap. The object must not be a function or method.
private static void declarePkgObj(this ptr<Checker> _addr_check, ptr<ast.Ident> _addr_ident, Object obj, ptr<declInfo> _addr_d) {
    ref Checker check = ref _addr_check.val;
    ref ast.Ident ident = ref _addr_ident.val;
    ref declInfo d = ref _addr_d.val;

    assert(ident.Name == obj.Name()); 

    // spec: "A package-scope or file-scope identifier with name init
    // may only be declared to be a function with this (func()) signature."
    if (ident.Name == "init") {
        check.errorf(ident, _InvalidInitDecl, "cannot declare init - must be func");
        return ;
    }
    if (ident.Name == "main" && check.pkg.name == "main") {
        check.errorf(ident, _InvalidMainDecl, "cannot declare main - must be func");
        return ;
    }
    check.declare(check.pkg.scope, ident, obj, token.NoPos);
    check.objMap[obj] = d;
    obj.setOrder(uint32(len(check.objMap)));

}

// filename returns a filename suitable for debugging output.
private static @string filename(this ptr<Checker> _addr_check, nint fileNo) {
    ref Checker check = ref _addr_check.val;

    var file = check.files[fileNo];
    {
        var pos = file.Pos();

        if (pos.IsValid()) {
            return check.fset.File(pos).Name();
        }
    }

    return fmt.Sprintf("file[%d]", fileNo);

}

private static ptr<Package> importPackage(this ptr<Checker> _addr_check, positioner at, @string path, @string dir) {
    ref Checker check = ref _addr_check.val;
 
    // If we already have a package for the given (path, dir)
    // pair, use it instead of doing a full import.
    // Checker.impMap only caches packages that are marked Complete
    // or fake (dummy packages for failed imports). Incomplete but
    // non-fake packages do require an import to complete them.
    importKey key = new importKey(path,dir);
    var imp = check.impMap[key];
    if (imp != null) {
        return _addr_imp!;
    }
    if (path == "C" && (check.conf.FakeImportC || check.conf.go115UsesCgo)) {
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

            if (importer == null) {
                err = error.As(fmt.Errorf("Config.Importer not installed"))!;
            }            {
                ImporterFrom (importerFrom, ok) = importer._<ImporterFrom>();


                else if (ok) {
                    imp, err = importerFrom.ImportFrom(path, dir, 0);
                    if (imp == null && err == null) {
                        err = error.As(fmt.Errorf("Config.Importer.ImportFrom(%s, %s, 0) returned nil but no error", path, dir))!;
                    }
                }
                else
 {
                    imp, err = importer.Import(path);
                    if (imp == null && err == null) {
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
        if (err == null && imp != null && (imp.name == "_" || imp.name == "")) {
            err = error.As(fmt.Errorf("invalid package name: %q", imp.name))!;
            imp = null; // create fake package below
        }
        if (err != null) {
            check.errorf(at, _BrokenImport, "could not import %s (%s)", path, err);
            if (imp == null) { 
                // create a new fake package
                // come up with a sensible package name (heuristic)
                var name = path;
                {
                    var i__prev4 = i;

                    var i = len(name);

                    if (i > 0 && name[i - 1] == '/') {
                        name = name[..(int)i - 1];
                    }

                    i = i__prev4;

                }

                {
                    var i__prev4 = i;

                    i = strings.LastIndex(name, "/");

                    if (i >= 0) {
                        name = name[(int)i + 1..];
                    }

                    i = i__prev4;

                }

                imp = NewPackage(path, name);

            } 
            // continue to use the package as best as we can
            imp.fake = true; // avoid follow-up lookup failures
        }
    }
    if (imp.complete || imp.fake) {
        check.impMap[key] = imp; 
        // Once we've formatted an error message once, keep the pkgPathMap
        // up-to-date on subsequent imports.
        if (check.pkgPathMap != null) {
            check.markImports(imp);
        }
        return _addr_imp!;

    }
    return _addr_null!;

}

// collectObjects collects all file and package objects and inserts them
// into their respective scopes. It also performs imports and associates
// methods with receiver base type names.
private static void collectObjects(this ptr<Checker> _addr_check) {
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

        foreach (var (_, __imp) in pkg.imports) {
            imp = __imp;
            pkgImports[imp] = true;
        }
        imp = imp__prev1;
    }

    private partial struct methodInfo {
        public ptr<Func> obj; // method
        public bool ptr; // true if pointer receiver
        public ptr<ast.Ident> recv; // receiver type name
    }
    slice<methodInfo> methods = default; // collected methods with valid receivers and non-blank _ names
    slice<ptr<Scope>> fileScopes = default;
    foreach (var (fileNo, file) in check.files) { 
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

            if (f != null) {
                (pos, end) = (token.Pos(f.Base()), token.Pos(f.Base() + f.Size()));
            }

        }

        var fileScope = NewScope(check.pkg.scope, pos, end, check.filename(fileNo));
        fileScopes = append(fileScopes, fileScope);
        check.recordScope(file, fileScope); 

        // determine file directory, necessary to resolve imports
        // FileName may be "" (typically for tests) in which case
        // we get "." as the directory which is what we would want.
        var fileDir = dir(check.fset.Position(file.Name.Pos()).Filename);

        check.walkDecls(file.Decls, d => {
            switch (d.type()) {
                case importDecl d:
                    var (path, err) = validatedImportPath(d.spec.Path.Value);
                    if (err != null) {
                        check.errorf(d.spec.Path, _BadImportPath, "invalid import path (%s)", err);
                        return ;
                    }
                    var imp = check.importPackage(d.spec.Path, path, fileDir);
                    if (imp == null) {
                        return ;
                    } 

                    // local name overrides imported package name
                    var name = imp.name;
                    if (d.spec.Name != null) {
                        name = d.spec.Name.Name;
                        if (path == "C") { 
                            // match cmd/compile (not prescribed by spec)
                            check.errorf(d.spec.Name, _ImportCRenamed, "cannot rename import \"C\"");
                            return ;

                        }

                    }

                    if (name == "init") {
                        check.errorf(d.spec, _InvalidInitDecl, "cannot import package as init - init must be a func");
                        return ;
                    } 

                    // add package to list of explicit imports
                    // (this functionality is provided as a convenience
                    // for clients; it is not needed for type-checking)
                    if (!pkgImports[imp]) {
                        pkgImports[imp] = true;
                        pkg.imports = append(pkg.imports, imp);
                    }

                    var pkgName = NewPkgName(d.spec.Pos(), pkg, name, imp);
                    if (d.spec.Name != null) { 
                        // in a dot-import, the dot represents the package
                        check.recordDef(d.spec.Name, pkgName);

                    }
                    else
 {
                        check.recordImplicit(d.spec, pkgName);
                    }

                    if (path == "C") { 
                        // match cmd/compile (not prescribed by spec)
                        pkgName.used = true;

                    } 

                    // add import to file scope
                    check.imports = append(check.imports, pkgName);
                    if (name == ".") { 
                        // dot-import
                        if (check.dotImportMap == null) {
                            check.dotImportMap = make_map<dotImportKey, ptr<PkgName>>();
                        } 
                        // merge imported scope with file scope
                        {
                            var obj__prev2 = obj;

                            foreach (var (_, __obj) in imp.scope.elems) {
                                obj = __obj; 
                                // A package scope may contain non-exported objects,
                                // do not import them!
                                if (obj.Exported()) { 
                                    // declare dot-imported object
                                    // (Do not use check.declare because it modifies the object
                                    // via Object.setScopePos, which leads to a race condition;
                                    // the object may be imported into more than one file scope
                                    // concurrently. See issue #32154.)
                                    {
                                        var alt__prev3 = alt;

                                        var alt = fileScope.Insert(obj);

                                        if (alt != null) {
                                            check.errorf(d.spec.Name, _DuplicateDecl, "%s redeclared in this block", obj.Name());
                                            check.reportAltDecl(alt);
                                        }
                                        else
 {
                                            check.dotImportMap[new dotImportKey(fileScope,obj)] = pkgName;
                                        }

                                        alt = alt__prev3;

                                    }

                                }

                            }
                    else

                            obj = obj__prev2;
                        }
                    } { 
                        // declare imported package object in file scope
                        // (no need to provide s.Name since we called check.recordDef earlier)
                        check.declare(fileScope, null, pkgName, token.NoPos);

                    }

                    break;
                case constDecl d:
                    {
                        var i__prev2 = i;
                        var name__prev2 = name;

                        foreach (var (__i, __name) in d.spec.Names) {
                            i = __i;
                            name = __name;
                            var obj = NewConst(name.Pos(), pkg, name.Name, null, constant.MakeInt64(int64(d.iota)));

                            ast.Expr init = default;
                            if (i < len(d.init)) {
                                init = d.init[i];
                            }
                            ptr<declInfo> d = addr(new declInfo(file:fileScope,vtyp:d.typ,init:init,inherited:d.inherited));
                            check.declarePkgObj(name, obj, d);
                        }

                        i = i__prev2;
                        name = name__prev2;
                    }
                    break;
                case varDecl d:
                    var lhs = make_slice<ptr<Var>>(len(d.spec.Names)); 
                    // If there's exactly one rhs initializer, use
                    // the same declInfo d1 for all lhs variables
                    // so that each lhs variable depends on the same
                    // rhs initializer (n:1 var declaration).
                    ptr<declInfo> d1;
                    if (len(d.spec.Values) == 1) { 
                        // The lhs elements are only set up after the for loop below,
                        // but that's ok because declareVar only collects the declInfo
                        // for a later phase.
                        d1 = addr(new declInfo(file:fileScope,lhs:lhs,vtyp:d.spec.Type,init:d.spec.Values[0]));

                    } 

                    // declare all variables
                    {
                        var i__prev2 = i;
                        var name__prev2 = name;

                        foreach (var (__i, __name) in d.spec.Names) {
                            i = __i;
                            name = __name;
                            obj = NewVar(name.Pos(), pkg, name.Name, null);
                            lhs[i] = obj;

                            var di = d1;
                            if (di == null) { 
                                // individual assignments
                                init = default;
                                if (i < len(d.spec.Values)) {
                                    init = d.spec.Values[i];
                                }

                                di = addr(new declInfo(file:fileScope,vtyp:d.spec.Type,init:init));

                            }

                            check.declarePkgObj(name, obj, di);

                        }

                        i = i__prev2;
                        name = name__prev2;
                    }
                    break;
                case typeDecl d:
                    obj = NewTypeName(d.spec.Name.Pos(), pkg, d.spec.Name.Name, null);
                    check.declarePkgObj(d.spec.Name, obj, addr(new declInfo(file:fileScope,tdecl:d.spec)));
                    break;
                case funcDecl d:
                    ptr<declInfo> info = addr(new declInfo(file:fileScope,fdecl:d.decl));
                    name = d.decl.Name.Name;
                    obj = NewFunc(d.decl.Name.Pos(), pkg, name, null);
                    if (d.decl.Recv.NumFields() == 0) { 
                        // regular function
                        if (d.decl.Recv != null) {
                            check.error(d.decl.Recv, _BadRecv, "method is missing receiver"); 
                            // treat as function
                        }

                        if (name == "init" || (name == "main" && check.pkg.name == "main")) {
                            var code = _InvalidInitDecl;
                            if (name == "main") {
                                code = _InvalidMainDecl;
                            }
                            {
                                var tparams = typeparams.Get(d.decl.Type);

                                if (tparams != null) {
                                    check.softErrorf(tparams, code, "func %s must have no type parameters", name);
                                }

                            }

                            {
                                var t = d.decl.Type;

                                if (t.Params.NumFields() != 0 || t.Results != null) { 
                                    // TODO(rFindley) Should this be a hard error?
                                    check.softErrorf(d.decl, code, "func %s must have no arguments and no return values", name);

                                }

                            }

                        }

                        if (name == "init") { 
                            // don't declare init functions in the package scope - they are invisible
                            obj.parent = pkg.scope;
                            check.recordDef(d.decl.Name, obj); 
                            // init functions must have a body
                            if (d.decl.Body == null) { 
                                // TODO(gri) make this error message consistent with the others above
                                check.softErrorf(obj, _MissingInitBody, "missing function body");

                            }

                        }
                        else
 {
                            check.declare(pkg.scope, d.decl.Name, obj, token.NoPos);
                        }

                    }
                    else
 { 
                        // method

                        // TODO(rFindley) earlier versions of this code checked that methods
                        //                have no type parameters, but this is checked later
                        //                when type checking the function type. Confirm that
                        //                we don't need to check tparams here.

                        var (ptr, recv, _) = check.unpackRecv(d.decl.Recv.List[0].Type, false); 
                        // (Methods with invalid receiver cannot be associated to a type, and
                        // methods with blank _ names are never found; no need to collect any
                        // of them. They will still be type-checked with all the other functions.)
                        if (recv != null && name != "_") {
                            methods = append(methods, new methodInfo(obj,ptr,recv));
                        }

                        check.recordDef(d.decl.Name, obj);

                    } 
                    // Methods are not package-level objects but we still track them in the
                    // object map so that we can handle them like regular functions (if the
                    // receiver is invalid); also we need their fdecl info when associating
                    // them with their receiver base type, below.
                    check.objMap[obj] = info;
                    obj.setOrder(uint32(len(check.objMap)));
                    break;
            }

        });

    }    foreach (var (_, scope) in fileScopes) {
        {
            var obj__prev2 = obj;

            foreach (var (_, __obj) in scope.elems) {
                obj = __obj;
                {
                    var alt__prev1 = alt;

                    alt = pkg.scope.Lookup(obj.Name());

                    if (alt != null) {
                        {
                            var pkg__prev2 = pkg;

                            ptr<PkgName> (pkg, ok) = obj._<ptr<PkgName>>();

                            if (ok) {
                                check.errorf(alt, _DuplicateDecl, "%s already declared through import of %s", alt.Name(), pkg.Imported());
                                check.reportAltDecl(pkg);
                            }
                            else
 {
                                check.errorf(alt, _DuplicateDecl, "%s already declared through dot-import of %s", alt.Name(), obj.Pkg()); 
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
    }    if (methods == null) {
        return ; // nothing to do
    }
    check.methods = make_map<ptr<TypeName>, slice<ptr<Func>>>();
    {
        var i__prev1 = i;

        foreach (var (__i) in methods) {
            i = __i;
            var m = _addr_methods[i]; 
            // Determine the receiver base type and associate m with it.
            var (ptr, base) = check.resolveBaseTypeName(m.ptr, m.recv);
            if (base != null) {
                m.obj.hasPtrRecv = ptr;
                check.methods[base] = append(check.methods[base], m.obj);
            }

        }
        i = i__prev1;
    }
}

// unpackRecv unpacks a receiver type and returns its components: ptr indicates whether
// rtyp is a pointer receiver, rname is the receiver type name, and tparams are its
// type parameters, if any. The type parameters are only unpacked if unpackParams is
// set. If rname is nil, the receiver is unusable (i.e., the source has a bug which we
// cannot easily work around).
private static (bool, ptr<ast.Ident>, slice<ptr<ast.Ident>>) unpackRecv(this ptr<Checker> _addr_check, ast.Expr rtyp, bool unpackParams) {
    bool ptr = default;
    ptr<ast.Ident> rname = default!;
    slice<ptr<ast.Ident>> tparams = default;
    ref Checker check = ref _addr_check.val;

L: 

    // unpack type parameters, if any
    while (true) {
        switch (rtyp.type()) {
            case ptr<ast.ParenExpr> t:
                rtyp = t.X;
                break;
            case ptr<ast.StarExpr> t:
                ptr = true;
                rtyp = t.X;
                break;
            default:
            {
                var t = rtyp.type();
                _breakL = true;
                break;
                break;
            }
        }

    } 

    // unpack type parameters, if any
    {
        ptr<ast.IndexExpr> (ptyp, _) = rtyp._<ptr<ast.IndexExpr>>();

        if (ptyp != null) {
            rtyp = ptyp.X;
            if (unpackParams) {
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in typeparams.UnpackExpr(ptyp.Index)) {
                        arg = __arg;
                        ptr<ast.Ident> par;
                        switch (arg.type()) {
                            case ptr<ast.Ident> arg:
                                par = arg;
                                break;
                            case ptr<ast.BadExpr> arg:
                                break;
                            case 
                                check.invalidAST(ptyp, "parameterized receiver contains nil parameters");
                                break;
                            default:
                            {
                                var arg = arg.type();
                                check.errorf(arg, _Todo, "receiver type parameter %s must be an identifier", arg);
                                break;
                            }
                        }
                        if (par == null) {
                            par = addr(new ast.Ident(NamePos:arg.Pos(),Name:"_"));
                        }

                        tparams = append(tparams, par);

                    }

                    arg = arg__prev1;
                }
            }

        }
    } 

    // unpack receiver name
    {
        ptr<ast.Ident> (name, _) = rtyp._<ptr<ast.Ident>>();

        if (name != null) {
            rname = name;
        }
    }


    return ;

}

// resolveBaseTypeName returns the non-alias base type name for typ, and whether
// there was a pointer indirection to get to it. The base type name must be declared
// in package scope, and there can be at most one pointer indirection. If no such type
// name exists, the returned base is nil.
private static (bool, ptr<TypeName>) resolveBaseTypeName(this ptr<Checker> _addr_check, bool seenPtr, ptr<ast.Ident> _addr_name) {
    bool ptr = default;
    ptr<TypeName> @base = default!;
    ref Checker check = ref _addr_check.val;
    ref ast.Ident name = ref _addr_name.val;
 
    // Algorithm: Starting from a type expression, which may be a name,
    // we follow that type through alias declarations until we reach a
    // non-alias type name. If we encounter anything but pointer types or
    // parentheses we're done. If we encounter more than one pointer type
    // we're done.
    ptr = seenPtr;
    map<ptr<TypeName>, bool> seen = default;
    ast.Expr typ = name;
    while (true) {
        typ = unparen(typ); 

        // check if we have a pointer type
        {
            ptr<ast.StarExpr> (pexpr, _) = typ._<ptr<ast.StarExpr>>();

            if (pexpr != null) { 
                // if we've already seen a pointer, we're done
                if (ptr) {
                    return (false, _addr_null!);
                }

                ptr = true;
                typ = unparen(pexpr.X); // continue with pointer base type
            } 

            // typ must be a name

        } 

        // typ must be a name
        ptr<ast.Ident> (name, _) = typ._<ptr<ast.Ident>>();
        if (name == null) {
            return (false, _addr_null!);
        }
        var obj = check.pkg.scope.Lookup(name.Name);
        if (obj == null) {
            return (false, _addr_null!);
        }
        ptr<TypeName> (tname, _) = obj._<ptr<TypeName>>();
        if (tname == null) {
            return (false, _addr_null!);
        }
        if (seen[tname]) {
            return (false, _addr_null!);
        }
        var tdecl = check.objMap[tname].tdecl; // must exist for objects in package scope
        if (!tdecl.Assign.IsValid()) {
            return (ptr, _addr_tname!);
        }
        typ = tdecl.Type;
        if (seen == null) {
            seen = make_map<ptr<TypeName>, bool>();
        }
        seen[tname] = true;

    }

}

// packageObjects typechecks all package objects, but not function bodies.
private static void packageObjects(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;
 
    // process package objects in source order for reproducible results
    var objList = make_slice<Object>(len(check.objMap));
    nint i = 0;
    {
        var obj__prev1 = obj;

        foreach (var (__obj) in check.objMap) {
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

        foreach (var (_, __obj) in objList) {
            obj = __obj;
            {
                var obj__prev1 = obj;

                ptr<TypeName> (obj, _) = obj._<ptr<TypeName>>();

                if (obj != null && obj.typ != null) {
                    check.collectMethods(obj);
                }

                obj = obj__prev1;

            }

        }
        obj = obj__prev1;
    }

    slice<ptr<TypeName>> aliasList = default; 
    // phase 1
    {
        var obj__prev1 = obj;

        foreach (var (_, __obj) in objList) {
            obj = __obj; 
            // If we have a type alias, collect it for the 2nd phase.
            {
                ptr<TypeName> (tname, _) = obj._<ptr<TypeName>>();

                if (tname != null && check.objMap[tname].tdecl.Assign.IsValid()) {
                    aliasList = append(aliasList, tname);
                    continue;
                }

            }


            check.objDecl(obj, null);

        }
        obj = obj__prev1;
    }

    {
        var obj__prev1 = obj;

        foreach (var (_, __obj) in aliasList) {
            obj = __obj;
            check.objDecl(obj, null);
        }
        obj = obj__prev1;
    }

    check.methods = null;

}

// inSourceOrder implements the sort.Sort interface.
private partial struct inSourceOrder { // : slice<Object>
}

private static nint Len(this inSourceOrder a) {
    return len(a);
}
private static bool Less(this inSourceOrder a, nint i, nint j) {
    return a[i].order() < a[j].order();
}
private static void Swap(this inSourceOrder a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

// unusedImports checks for unused imports.
private static void unusedImports(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;
 
    // if function bodies are not checked, packages' uses are likely missing - don't check
    if (check.conf.IgnoreFuncBodies) {
        return ;
    }
    foreach (var (_, obj) in check.imports) {
        if (!obj.used && obj.name != "_") {
            check.errorUnusedPkg(obj);
        }
    }
}

private static void errorUnusedPkg(this ptr<Checker> _addr_check, ptr<PkgName> _addr_obj) {
    ref Checker check = ref _addr_check.val;
    ref PkgName obj = ref _addr_obj.val;
 
    // If the package was imported with a name other than the final
    // import path element, show it explicitly in the error message.
    // Note that this handles both renamed imports and imports of
    // packages containing unconventional package declarations.
    // Note that this uses / always, even on Windows, because Go import
    // paths always use forward slashes.
    var path = obj.imported.path;
    var elem = path;
    {
        var i = strings.LastIndex(elem, "/");

        if (i >= 0) {
            elem = elem[(int)i + 1..];
        }
    }

    if (obj.name == "" || obj.name == "." || obj.name == elem) {
        check.softErrorf(obj, _UnusedImport, "%q imported but not used", path);
    }
    else
 {
        check.softErrorf(obj, _UnusedImport, "%q imported but not used as %s", path, obj.name);
    }
}

// dir makes a good-faith attempt to return the directory
// portion of path. If path is empty, the result is ".".
// (Per the go/build package dependency tests, we cannot import
// path/filepath and simply use filepath.Dir.)
private static @string dir(@string path) {
    {
        var i = strings.LastIndexAny(path, "/\\");

        if (i > 0) {
            return path[..(int)i];
        }
    } 
    // i <= 0
    return ".";

}

} // end types_package
