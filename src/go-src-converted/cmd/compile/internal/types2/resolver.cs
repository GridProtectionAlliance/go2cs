// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types2 -- go2cs converted at 2022 March 13 06:26:13 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\resolver.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;
using fmt = fmt_package;
using constant = go.constant_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;


// A declInfo describes a package-level const, type, var, or func declaration.

public static partial class types2_package {

private partial struct declInfo {
    public ptr<Scope> file; // scope of file containing this declaration
    public slice<ptr<Var>> lhs; // lhs of n:1 variable declarations, or nil
    public syntax.Expr vtyp; // type, or nil (for const and var declarations only)
    public syntax.Expr init; // init/orig expression, or nil (for const and var declarations only)
    public bool inherited; // if set, the init expression is inherited from a previous constant declaration
    public ptr<syntax.TypeDecl> tdecl; // type declaration, or nil
    public ptr<syntax.FuncDecl> fdecl; // func declaration, or nil

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

// arity checks that the lhs and rhs of a const or var decl
// have a matching number of names and initialization values.
// If inherited is set, the initialization values are from
// another (constant) declaration.
private static void arity(this ptr<Checker> _addr_check, syntax.Pos pos, slice<ptr<syntax.Name>> names, slice<syntax.Expr> inits, bool constDecl, bool inherited) {
    ref Checker check = ref _addr_check.val;

    var l = len(names);
    var r = len(inits);


    if (l < r) 
        var n = inits[l];
        if (inherited) {
            check.errorf(pos, "extra init expr at %s", n.Pos());
        }
        else
 {
            check.errorf(n, "extra init expr %s", n);
        }
    else if (l > r && (constDecl || r != 1)) // if r == 1 it may be a multi-valued function and we can't say anything yet
        n = names[r];
        check.errorf(n, "missing init expr for %s", n.Value);
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
private static void declarePkgObj(this ptr<Checker> _addr_check, ptr<syntax.Name> _addr_ident, Object obj, ptr<declInfo> _addr_d) {
    ref Checker check = ref _addr_check.val;
    ref syntax.Name ident = ref _addr_ident.val;
    ref declInfo d = ref _addr_d.val;

    assert(ident.Value == obj.Name()); 

    // spec: "A package-scope or file-scope identifier with name init
    // may only be declared to be a function with this (func()) signature."
    if (ident.Value == "init") {
        check.error(ident, "cannot declare init - must be func");
        return ;
    }
    if (ident.Value == "main" && check.pkg.name == "main") {
        check.error(ident, "cannot declare main - must be func");
        return ;
    }
    check.declare(check.pkg.scope, ident, obj, nopos);
    check.objMap[obj] = d;
    obj.setOrder(uint32(len(check.objMap)));
}

// filename returns a filename suitable for debugging output.
private static @string filename(this ptr<Checker> _addr_check, nint fileNo) {
    ref Checker check = ref _addr_check.val;

    var file = check.files[fileNo];
    {
        var pos = file.Pos();

        if (pos.IsKnown()) { 
            // return check.fset.File(pos).Name()
            // TODO(gri) do we need the actual file name here?
            return pos.RelFilename();
        }
    }
    return fmt.Sprintf("file[%d]", fileNo);
}

private static ptr<Package> importPackage(this ptr<Checker> _addr_check, syntax.Pos pos, @string path, @string dir) {
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
            check.errorf(pos, "could not import %s (%s)", path, err);
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
        public ptr<syntax.Name> recv; // receiver type name
    }
    slice<methodInfo> methods = default; // collected methods with valid receivers and non-blank _ names
    slice<ptr<Scope>> fileScopes = default;
    foreach (var (fileNo, file) in check.files) { 
        // The package identifier denotes the current package,
        // but there is no corresponding package object.
        check.recordDef(file.PkgName, null);

        var fileScope = NewScope(check.pkg.scope, syntax.StartPos(file), syntax.EndPos(file), check.filename(fileNo));
        fileScopes = append(fileScopes, fileScope);
        check.recordScope(file, fileScope); 

        // determine file directory, necessary to resolve imports
        // FileName may be "" (typically for tests) in which case
        // we get "." as the directory which is what we would want.
        var fileDir = dir(file.PkgName.Pos().RelFilename()); // TODO(gri) should this be filename?

        nint first = -1; // index of first ConstDecl in the current group, or -1
        ptr<syntax.ConstDecl> last; // last ConstDecl with init expressions, or nil
        foreach (var (index, decl) in file.DeclList) {
            {
                ptr<syntax.ConstDecl> (_, ok) = decl._<ptr<syntax.ConstDecl>>();

                if (!ok) {
                    first = -1; // we're not in a constant declaration
                }

            }

            switch (decl.type()) {
                case ptr<syntax.ImportDecl> s:
                    if (s.Path == null || s.Path.Bad) {
                        continue; // error reported during parsing
                    }
                    var (path, err) = validatedImportPath(s.Path.Value);
                    if (err != null) {
                        check.errorf(s.Path, "invalid import path (%s)", err);
                        continue;
                    }
                    var imp = check.importPackage(s.Path.Pos(), path, fileDir);
                    if (imp == null) {
                        continue;
                    } 

                    // local name overrides imported package name
                    var name = imp.name;
                    if (s.LocalPkgName != null) {
                        name = s.LocalPkgName.Value;
                        if (path == "C") { 
                            // match cmd/compile (not prescribed by spec)
                            check.error(s.LocalPkgName, "cannot rename import \"C\"");
                            continue;
                        }
                    }
                    if (name == "init") {
                        check.error(s.LocalPkgName, "cannot import package as init - init must be a func");
                        continue;
                    } 

                    // add package to list of explicit imports
                    // (this functionality is provided as a convenience
                    // for clients; it is not needed for type-checking)
                    if (!pkgImports[imp]) {
                        pkgImports[imp] = true;
                        pkg.imports = append(pkg.imports, imp);
                    }
                    var pkgName = NewPkgName(s.Pos(), pkg, name, imp);
                    if (s.LocalPkgName != null) { 
                        // in a dot-import, the dot represents the package
                        check.recordDef(s.LocalPkgName, pkgName);
                    }
                    else
 {
                        check.recordImplicit(s, pkgName);
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
                            var obj__prev3 = obj;

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
                                            ref error_ err = ref heap(out ptr<error_> _addr_err);
                                            err.errorf(s.LocalPkgName, "%s redeclared in this block", obj.Name());
                                            err.recordAltDecl(alt);
                                            check.report(_addr_err);
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

                            obj = obj__prev3;
                        }
                    } { 
                        // declare imported package object in file scope
                        // (no need to provide s.LocalPkgName since we called check.recordDef earlier)
                        check.declare(fileScope, null, pkgName, nopos);
                    }
                    break;
                case ptr<syntax.ConstDecl> s:
                    if (first < 0 || file.DeclList[index - 1]._<ptr<syntax.ConstDecl>>().Group != s.Group) {
                        first = index;
                        last = null;
                    }
                    var iota = constant.MakeInt64(int64(index - first)); 

                    // determine which initialization expressions to use
                    var inherited = true;

                    if (s.Type != null || s.Values != null) 
                        last = s;
                        inherited = false;
                    else if (last == null) 
                        last = @new<syntax.ConstDecl>(); // make sure last exists
                        inherited = false;
                    // declare all constants
                    var values = unpackExpr(last.Values);
                    {
                        var i__prev3 = i;
                        var name__prev3 = name;

                        foreach (var (__i, __name) in s.NameList) {
                            i = __i;
                            name = __name;
                            var obj = NewConst(name.Pos(), pkg, name.Value, null, iota);

                            syntax.Expr init = default;
                            if (i < len(values)) {
                                init = values[i];
                            }
                            ptr<declInfo> d = addr(new declInfo(file:fileScope,vtyp:last.Type,init:init,inherited:inherited));
                            check.declarePkgObj(name, obj, d);
                        } 

                        // Constants must always have init values.

                        i = i__prev3;
                        name = name__prev3;
                    }

                    check.arity(s.Pos(), s.NameList, values, true, inherited);
                    break;
                case ptr<syntax.VarDecl> s:
                    var lhs = make_slice<ptr<Var>>(len(s.NameList)); 
                    // If there's exactly one rhs initializer, use
                    // the same declInfo d1 for all lhs variables
                    // so that each lhs variable depends on the same
                    // rhs initializer (n:1 var declaration).
                    ptr<declInfo> d1;
                    {
                        (_, ok) = s.Values._<ptr<syntax.ListExpr>>();

                        if (!ok) { 
                            // The lhs elements are only set up after the for loop below,
                            // but that's ok because declarePkgObj only collects the declInfo
                            // for a later phase.
                            d1 = addr(new declInfo(file:fileScope,lhs:lhs,vtyp:s.Type,init:s.Values));
                        } 

                        // declare all variables

                    } 

                    // declare all variables
                    values = unpackExpr(s.Values);
                    {
                        var i__prev3 = i;
                        var name__prev3 = name;

                        foreach (var (__i, __name) in s.NameList) {
                            i = __i;
                            name = __name;
                            obj = NewVar(name.Pos(), pkg, name.Value, null);
                            lhs[i] = obj;

                            d = d1;
                            if (d == null) { 
                                // individual assignments
                                init = default;
                                if (i < len(values)) {
                                    init = values[i];
                                }
                                d = addr(new declInfo(file:fileScope,vtyp:s.Type,init:init));
                            }
                            check.declarePkgObj(name, obj, d);
                        } 

                        // If we have no type, we must have values.

                        i = i__prev3;
                        name = name__prev3;
                    }

                    if (s.Type == null || values != null) {
                        check.arity(s.Pos(), s.NameList, values, false, false);
                    }
                    break;
                case ptr<syntax.TypeDecl> s:
                    obj = NewTypeName(s.Name.Pos(), pkg, s.Name.Value, null);
                    check.declarePkgObj(s.Name, obj, addr(new declInfo(file:fileScope,tdecl:s)));
                    break;
                case ptr<syntax.FuncDecl> s:
                    d = s; // TODO(gri) get rid of this
                    name = d.Name.Value;
                    obj = NewFunc(d.Name.Pos(), pkg, name, null);
                    if (d.Recv == null) { 
                        // regular function
                        if (name == "init" || name == "main" && pkg.name == "main") {
                            if (d.TParamList != null) {
                                check.softErrorf(d, "func %s must have no type parameters", name);
                            }
                            {
                                var t = d.Type;

                                if (len(t.ParamList) != 0 || len(t.ResultList) != 0) {
                                    check.softErrorf(d, "func %s must have no arguments and no return values", name);
                                }

                            }
                        } 
                        // don't declare init functions in the package scope - they are invisible
                        if (name == "init") {
                            obj.parent = pkg.scope;
                            check.recordDef(d.Name, obj); 
                            // init functions must have a body
                            if (d.Body == null) { 
                                // TODO(gri) make this error message consistent with the others above
                                check.softErrorf(obj.pos, "missing function body");
                            }
                        }
                        else
 {
                            check.declare(pkg.scope, d.Name, obj, nopos);
                        }
                    }
                    else
 { 
                        // method
                        // d.Recv != nil
                        if (!acceptMethodTypeParams && len(d.TParamList) != 0) { 
                            //check.error(d.TParamList.Pos(), invalidAST + "method must have no type parameters")
                            check.error(d, invalidAST + "method must have no type parameters");
                        }
                        var (ptr, recv, _) = check.unpackRecv(d.Recv.Type, false); 
                        // (Methods with invalid receiver cannot be associated to a type, and
                        // methods with blank _ names are never found; no need to collect any
                        // of them. They will still be type-checked with all the other functions.)
                        if (recv != null && name != "_") {
                            methods = append(methods, new methodInfo(obj,ptr,recv));
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
                    var s = decl.type();
                    check.errorf(s, invalidAST + "unknown syntax.Decl node %T", s);
                    break;
                }
            }
        }
    }    foreach (var (_, scope) in fileScopes) {
        {
            var obj__prev2 = obj;

            foreach (var (_, __obj) in scope.elems) {
                obj = __obj;
                {
                    var alt__prev1 = alt;

                    alt = pkg.scope.Lookup(obj.Name());

                    if (alt != null) {
                        err = default;
                        {
                            var pkg__prev2 = pkg;

                            ptr<PkgName> (pkg, ok) = obj._<ptr<PkgName>>();

                            if (ok) {
                                err.errorf(alt, "%s already declared through import of %s", alt.Name(), pkg.Imported());
                                err.recordAltDecl(pkg);
                            }
                            else
 {
                                err.errorf(alt, "%s already declared through dot-import of %s", alt.Name(), obj.Pkg()); 
                                // TODO(gri) dot-imported objects don't have a position; recordAltDecl won't print anything
                                err.recordAltDecl(obj);
                            }

                            pkg = pkg__prev2;

                        }
                        check.report(_addr_err);
                    }

                    alt = alt__prev1;

                }
            }

            obj = obj__prev2;
        }
    }    if (methods != null) {
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
}

// unpackRecv unpacks a receiver type and returns its components: ptr indicates whether
// rtyp is a pointer receiver, rname is the receiver type name, and tparams are its
// type parameters, if any. The type parameters are only unpacked if unpackParams is
// set. If rname is nil, the receiver is unusable (i.e., the source has a bug which we
// cannot easily work around).
private static (bool, ptr<syntax.Name>, slice<ptr<syntax.Name>>) unpackRecv(this ptr<Checker> _addr_check, syntax.Expr rtyp, bool unpackParams) {
    bool ptr = default;
    ptr<syntax.Name> rname = default!;
    slice<ptr<syntax.Name>> tparams = default;
    ref Checker check = ref _addr_check.val;

L: 

    // unpack type parameters, if any
    while (true) {
        switch (rtyp.type()) {
            case ptr<syntax.ParenExpr> t:
                rtyp = t.X; 
                // case *ast.StarExpr:
                //      ptr = true
                //     rtyp = t.X
                break;
            case ptr<syntax.Operation> t:
                if (t.Op != syntax.Mul || t.Y != null) {
                    break;
                }
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
        ptr<syntax.IndexExpr> (ptyp, _) = rtyp._<ptr<syntax.IndexExpr>>();

        if (ptyp != null) {
            rtyp = ptyp.X;
            if (unpackParams) {
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in unpackExpr(ptyp.Index)) {
                        arg = __arg;
                        ptr<syntax.Name> par;
                        switch (arg.type()) {
                            case ptr<syntax.Name> arg:
                                par = arg;
                                break;
                            case ptr<syntax.BadExpr> arg:
                                break;
                            case 
                                check.error(ptyp, invalidAST + "parameterized receiver contains nil parameters");
                                break;
                            default:
                            {
                                var arg = arg.type();
                                check.errorf(arg, "receiver type parameter %s must be an identifier", arg);
                                break;
                            }
                        }
                        if (par == null) {
                            par = syntax.NewName(arg.Pos(), "_");
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
        ptr<syntax.Name> (name, _) = rtyp._<ptr<syntax.Name>>();

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
private static (bool, ptr<TypeName>) resolveBaseTypeName(this ptr<Checker> _addr_check, bool seenPtr, syntax.Expr typ) {
    bool ptr = default;
    ptr<TypeName> @base = default!;
    ref Checker check = ref _addr_check.val;
 
    // Algorithm: Starting from a type expression, which may be a name,
    // we follow that type through alias declarations until we reach a
    // non-alias type name. If we encounter anything but pointer types or
    // parentheses we're done. If we encounter more than one pointer type
    // we're done.
    ptr = seenPtr;
    map<ptr<TypeName>, bool> seen = default;
    while (true) {
        typ = unparen(typ); 

        // check if we have a pointer type
        // if pexpr, _ := typ.(*ast.StarExpr); pexpr != nil {
        {
            ptr<syntax.Operation> (pexpr, _) = typ._<ptr<syntax.Operation>>();

            if (pexpr != null && pexpr.Op == syntax.Mul && pexpr.Y == null) { 
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
        ptr<syntax.Name> (name, _) = typ._<ptr<syntax.Name>>();
        if (name == null) {
            return (false, _addr_null!);
        }
        var obj = check.pkg.scope.Lookup(name.Value);
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
        if (!tdecl.Alias) {
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

                if (tname != null && check.objMap[tname].tdecl.Alias) {
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
        if (check.conf.CompilerErrorMessages) {
            check.softErrorf(obj, "imported and not used: %q", path);
        }
        else
 {
            check.softErrorf(obj, "%q imported but not used", path);
        }
    }
    else
 {
        if (check.conf.CompilerErrorMessages) {
            check.softErrorf(obj, "imported and not used: %q as %s", path, obj.name);
        }
        else
 {
            check.softErrorf(obj, "%q imported but not used as %s", path, obj.name);
        }
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

} // end types2_package
