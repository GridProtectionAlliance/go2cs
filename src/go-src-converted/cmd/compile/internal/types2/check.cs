// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the Check function, which drives type-checking.

// package types2 -- go2cs converted at 2022 March 06 23:12:27 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\check.go
using syntax = go.cmd.compile.@internal.syntax_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types2_package {

private static syntax.Pos nopos = default;

// debugging/development support
private static readonly var debug = false; // leave on during development

// If forceStrict is set, the type-checker enforces additional
// rules not specified by the Go 1 spec, but which will
// catch guaranteed run-time errors if the respective
// code is executed. In other words, programs passing in
// strict mode are Go 1 compliant, but not all Go 1 programs
// will pass in strict mode. The additional rules are:
//
// - A type assertion x.(T) where T is an interface type
//   is invalid if any (statically known) method that exists
//   for both x and T have different signatures.
//
 // leave on during development

// If forceStrict is set, the type-checker enforces additional
// rules not specified by the Go 1 spec, but which will
// catch guaranteed run-time errors if the respective
// code is executed. In other words, programs passing in
// strict mode are Go 1 compliant, but not all Go 1 programs
// will pass in strict mode. The additional rules are:
//
// - A type assertion x.(T) where T is an interface type
//   is invalid if any (statically known) method that exists
//   for both x and T have different signatures.
//
private static readonly var forceStrict = false;

// exprInfo stores information about an untyped expression.


// exprInfo stores information about an untyped expression.
private partial struct exprInfo {
    public bool isLhs; // expression is lhs operand of a shift with delayed type-check
    public operandMode mode;
    public ptr<Basic> typ;
    public constant.Value val; // constant value; or nil (if not a constant)
}

// A context represents the context within which an object is type-checked.
private partial struct context {
    public ptr<declInfo> decl; // package-level declaration whose init expression/function body is checked
    public ptr<Scope> scope; // top-most scope for lookups
    public syntax.Pos pos; // if valid, identifiers are looked up as if at position pos (used by Eval)
    public constant.Value iota; // value of iota in a constant declaration; nil otherwise
    public syntax.Pos errpos; // if valid, identifier position of a constant with inherited initializer
    public ptr<Signature> sig; // function signature if inside a function; nil otherwise
    public map<ptr<syntax.CallExpr>, bool> isPanic; // set of panic call expressions (used for termination check)
    public bool hasLabel; // set if a function makes use of labels (only ~1% of functions); unused outside functions
    public bool hasCallOrRecv; // set if an expression contains a function call or channel receive operation
}

// lookup looks up name in the current context and returns the matching object, or nil.
private static Object lookup(this ptr<context> _addr_ctxt, @string name) {
    ref context ctxt = ref _addr_ctxt.val;

    var (_, obj) = ctxt.scope.LookupParent(name, ctxt.pos);
    return obj;
}

// An importKey identifies an imported package by import path and source directory
// (directory containing the file containing the import). In practice, the directory
// may always be the same, or may not matter. Given an (import path, directory), an
// importer must always return the same package (but given two different import paths,
// an importer may still return the same package by mapping them to the same package
// paths).
private partial struct importKey {
    public @string path;
    public @string dir;
}

// A dotImportKey describes a dot-imported object in the given scope.
private partial struct dotImportKey {
    public ptr<Scope> scope;
    public Object obj;
}

// A Checker maintains the state of the type checker.
// It must be created with NewChecker.
public partial struct Checker {
    public ptr<Config> conf;
    public ptr<Package> pkg;
    public ref ptr<Info> ptr<Info> => ref ptr<Info>_ptr;
    public version version; // accepted language version
    public map<Object, ptr<declInfo>> objMap; // maps package-level objects and (non-interface) methods to declaration info
    public map<importKey, ptr<Package>> impMap; // maps (import path, source directory) to (complete or fake) package
    public map<ptr<Interface>, slice<syntax.Pos>> posMap; // maps interface types to lists of embedded interface positions
    public map<@string, ptr<Named>> typMap; // maps an instantiated named type hash to a *Named type

// pkgPathMap maps package names to the set of distinct import paths we've
// seen for that name, anywhere in the import graph. It is used for
// disambiguating package names in error messages.
//
// pkgPathMap is allocated lazily, so that we don't pay the price of building
// it on the happy path. seenPkgMap tracks the packages that we've already
// walked.
    public map<@string, map<@string, bool>> pkgPathMap;
    public map<ptr<Package>, bool> seenPkgMap; // information collected during type-checking of a set of package files
// (initialized by Files, valid only for the duration of check.Files;
// maps and lists are allocated on demand)
    public slice<ptr<syntax.File>> files; // list of package files
    public slice<ptr<PkgName>> imports; // list of imported packages
    public map<dotImportKey, ptr<PkgName>> dotImportMap; // maps dot-imported objects to the package they were dot-imported through

    public error firstErr; // first error encountered
    public map<ptr<TypeName>, slice<ptr<Func>>> methods; // maps package scope type names to associated non-blank (non-interface) methods
    public map<syntax.Expr, exprInfo> untyped; // map of expressions without final type
    public slice<Action> delayed; // stack of delayed action segments; segments are processed in FIFO order
    public slice<Object> objPath; // path of object dependencies during type inference (for cycle reporting)

// context within which the current object is type-checked
// (valid only for the duration of type-checking a specific object)
    public ref context context => ref context_val; // debugging
    public nint indent; // indentation for tracing
}

// addDeclDep adds the dependency edge (check.decl -> to) if check.decl exists
private static void addDeclDep(this ptr<Checker> _addr_check, Object to) {
    ref Checker check = ref _addr_check.val;

    var from = check.decl;
    if (from == null) {
        return ; // not in a package-level init expression
    }
    {
        var (_, found) = check.objMap[to];

        if (!found) {
            return ; // to is not a package-level object
        }
    }

    from.addDep(to);

}

private static void rememberUntyped(this ptr<Checker> _addr_check, syntax.Expr e, bool lhs, operandMode mode, ptr<Basic> _addr_typ, constant.Value val) {
    ref Checker check = ref _addr_check.val;
    ref Basic typ = ref _addr_typ.val;

    var m = check.untyped;
    if (m == null) {
        m = make_map<syntax.Expr, exprInfo>();
        check.untyped = m;
    }
    m[e] = new exprInfo(lhs,mode,typ,val);

}

// later pushes f on to the stack of actions that will be processed later;
// either at the end of the current statement, or in case of a local constant
// or variable declaration, before the constant or variable is in scope
// (so that f still sees the scope before any new declarations).
private static void later(this ptr<Checker> _addr_check, Action f) {
    ref Checker check = ref _addr_check.val;

    check.delayed = append(check.delayed, f);
}

// push pushes obj onto the object path and returns its index in the path.
private static nint push(this ptr<Checker> _addr_check, Object obj) {
    ref Checker check = ref _addr_check.val;

    check.objPath = append(check.objPath, obj);
    return len(check.objPath) - 1;
}

// pop pops and returns the topmost object from the object path.
private static Object pop(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;

    var i = len(check.objPath) - 1;
    var obj = check.objPath[i];
    check.objPath[i] = null;
    check.objPath = check.objPath[..(int)i];
    return obj;
}

// NewChecker returns a new Checker instance for a given package.
// Package files may be added incrementally via checker.Files.
public static ptr<Checker> NewChecker(ptr<Config> _addr_conf, ptr<Package> _addr_pkg, ptr<Info> _addr_info) => func((_, panic, _) => {
    ref Config conf = ref _addr_conf.val;
    ref Package pkg = ref _addr_pkg.val;
    ref Info info = ref _addr_info.val;
 
    // make sure we have a configuration
    if (conf == null) {
        conf = @new<Config>();
    }
    if (info == null) {
        info = @new<Info>();
    }
    var (version, err) = parseGoVersion(conf.GoVersion);
    if (err != null) {
        panic(fmt.Sprintf("invalid Go version %q (%v)", conf.GoVersion, err));
    }
    return addr(new Checker(conf:conf,pkg:pkg,Info:info,version:version,objMap:make(map[Object]*declInfo),impMap:make(map[importKey]*Package),posMap:make(map[*Interface][]syntax.Pos),typMap:make(map[string]*Named),));

});

// initFiles initializes the files-specific portion of checker.
// The provided files must all belong to the same package.
private static void initFiles(this ptr<Checker> _addr_check, slice<ptr<syntax.File>> files) {
    ref Checker check = ref _addr_check.val;
 
    // start with a clean slate (check.Files may be called multiple times)
    check.files = null;
    check.imports = null;
    check.dotImportMap = null;

    check.firstErr = null;
    check.methods = null;
    check.untyped = null;
    check.delayed = null; 

    // determine package name and collect valid files
    var pkg = check.pkg;
    foreach (var (_, file) in files) {
        {
            var name = file.PkgName.Value;


            if (pkg.name == "")
            {
                if (name != "_") {
                    pkg.name = name;
                }
                else
 {
                    check.error(file.PkgName, "invalid package name _");
                }

                fallthrough = true;

            }
            if (fallthrough || pkg.name == name)
            {
                check.files = append(check.files, file);
                goto __switch_break0;
            }
            // default: 
                check.errorf(file, "package %s; expected %s", name, pkg.name); 
                // ignore this file

            __switch_break0:;
        }

    }
}

// A bailout panic is used for early termination.
private partial struct bailout {
}

private static void handleBailout(this ptr<Checker> _addr_check, ptr<error> _addr_err) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref error err = ref _addr_err.val;

    switch (recover().type()) {
        case bailout p:
            err = error.As(check.firstErr)!;
            break;
        default:
        {
            var p = recover().type();
            panic(p);
            break;
        }
    }

});

// Files checks the provided files as part of the checker's package.
private static error Files(this ptr<Checker> _addr_check, slice<ptr<syntax.File>> files) {
    ref Checker check = ref _addr_check.val;

    return error.As(check.checkFiles(files))!;
}

private static var errBadCgo = errors.New("cannot use FakeImportC and go115UsesCgo together");

private static error checkFiles(this ptr<Checker> _addr_check, slice<ptr<syntax.File>> files) => func((defer, _, _) => {
    error err = default!;
    ref Checker check = ref _addr_check.val;

    if (check.conf.FakeImportC && check.conf.go115UsesCgo) {
        return error.As(errBadCgo)!;
    }
    defer(check.handleBailout(_addr_err));

    Action<@string> print = msg => {
        if (check.conf.Trace) {
            fmt.Println(msg);
        }
    };

    print("== initFiles ==");
    check.initFiles(files);

    print("== collectObjects ==");
    check.collectObjects();

    print("== packageObjects ==");
    check.packageObjects();

    print("== processDelayed ==");
    check.processDelayed(0); // incl. all functions

    print("== initOrder ==");
    check.initOrder();

    if (!check.conf.DisableUnusedImportCheck) {
        print("== unusedImports ==");
        check.unusedImports();
    }
    print("== recordUntyped ==");
    check.recordUntyped();

    if (check.Info != null) {
        print("== sanitizeInfo ==");
        sanitizeInfo(check.Info);
    }
    check.pkg.complete = true; 

    // no longer needed - release memory
    check.imports = null;
    check.dotImportMap = null;
    check.pkgPathMap = null;
    check.seenPkgMap = null; 

    // TODO(gri) There's more memory we should release at this point.

    return ;

});

// processDelayed processes all delayed actions pushed after top.
private static void processDelayed(this ptr<Checker> _addr_check, nint top) {
    ref Checker check = ref _addr_check.val;
 
    // If each delayed action pushes a new action, the
    // stack will continue to grow during this loop.
    // However, it is only processing functions (which
    // are processed in a delayed fashion) that may
    // add more actions (such as nested functions), so
    // this is a sufficiently bounded process.
    for (var i = top; i < len(check.delayed); i++) {
        check.delayed[i](); // may append to check.delayed
    }
    assert(top <= len(check.delayed)); // stack must not have shrunk
    check.delayed = check.delayed[..(int)top];

}

private static void record(this ptr<Checker> _addr_check, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
 
    // convert x into a user-friendly set of values
    // TODO(gri) this code can be simplified
    Type typ = default;
    constant.Value val = default;

    if (x.mode == invalid) 
        typ = Typ[Invalid];
    else if (x.mode == novalue) 
        typ = (Tuple.val)(null);
    else if (x.mode == constant_) 
        typ = x.typ;
        val = x.val;
    else 
        typ = x.typ;
        assert(x.expr != null && typ != null);

    if (isUntyped(typ)) { 
        // delay type and value recording until we know the type
        // or until the end of type checking
        check.rememberUntyped(x.expr, false, x.mode, typ._<ptr<Basic>>(), val);

    }
    else
 {
        check.recordTypeAndValue(x.expr, x.mode, typ, val);
    }
}

private static void recordUntyped(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;

    if (!debug && check.Types == null) {
        return ; // nothing to do
    }
    foreach (var (x, info) in check.untyped) {
        if (debug && isTyped(info.typ)) {
            check.dump("%v: %s (type %s) is typed", posFor(x), x, info.typ);
            unreachable();
        }
        check.recordTypeAndValue(x, info.mode, info.typ, info.val);

    }
}

private static void recordTypeAndValue(this ptr<Checker> _addr_check, syntax.Expr x, operandMode mode, Type typ, constant.Value val) {
    ref Checker check = ref _addr_check.val;

    assert(x != null);
    assert(typ != null);
    if (mode == invalid) {
        return ; // omit
    }
    if (mode == constant_) {
        assert(val != null); 
        // We check is(typ, IsConstType) here as constant expressions may be
        // recorded as type parameters.
        assert(typ == Typ[Invalid] || is(typ, IsConstType));

    }
    {
        var m = check.Types;

        if (m != null) {
            m[x] = new TypeAndValue(mode,typ,val);
        }
    }

}

private static void recordBuiltinType(this ptr<Checker> _addr_check, syntax.Expr f, ptr<Signature> _addr_sig) {
    ref Checker check = ref _addr_check.val;
    ref Signature sig = ref _addr_sig.val;
 
    // f must be a (possibly parenthesized, possibly qualified)
    // identifier denoting a built-in (including unsafe's non-constant
    // functions Add and Slice): record the signature for f and possible
    // children.
    while (true) {
        check.recordTypeAndValue(f, builtin, sig, null);
        switch (f.type()) {
            case ptr<syntax.Name> p:
                return ; // we're done
                break;
            case ptr<syntax.SelectorExpr> p:
                return ; // we're done
                break;
            case ptr<syntax.ParenExpr> p:
                f = p.X;
                break;
            default:
            {
                var p = f.type();
                unreachable();
                break;
            }
        }

    }

}

private static void recordCommaOkTypes(this ptr<Checker> _addr_check, syntax.Expr x, array<Type> a) {
    a = a.Clone();
    ref Checker check = ref _addr_check.val;

    assert(x != null);
    if (a[0] == null || a[1] == null) {
        return ;
    }
    assert(isTyped(a[0]) && isTyped(a[1]) && (isBoolean(a[1]) || a[1] == universeError));
    {
        var m = check.Types;

        if (m != null) {
            while (true) {
                var tv = m[x];
                assert(tv.Type != null); // should have been recorded already
                var pos = x.Pos();
                tv.Type = NewTuple(NewVar(pos, check.pkg, "", a[0]), NewVar(pos, check.pkg, "", a[1]));
                m[x] = tv; 
                // if x is a parenthesized expression (p.X), update p.X
                ptr<syntax.ParenExpr> (p, _) = x._<ptr<syntax.ParenExpr>>();
                if (p == null) {
                    break;
                }

                x = p.X;

            }


        }
    }

}

private static void recordInferred(this ptr<Checker> _addr_check, syntax.Expr call, slice<Type> targs, ptr<Signature> _addr_sig) {
    ref Checker check = ref _addr_check.val;
    ref Signature sig = ref _addr_sig.val;

    assert(call != null);
    assert(sig != null);
    {
        var m = check.Inferred;

        if (m != null) {
            m[call] = new Inferred(targs,sig);
        }
    }

}

private static void recordDef(this ptr<Checker> _addr_check, ptr<syntax.Name> _addr_id, Object obj) {
    ref Checker check = ref _addr_check.val;
    ref syntax.Name id = ref _addr_id.val;

    assert(id != null);
    {
        var m = check.Defs;

        if (m != null) {
            m[id] = obj;
        }
    }

}

private static void recordUse(this ptr<Checker> _addr_check, ptr<syntax.Name> _addr_id, Object obj) {
    ref Checker check = ref _addr_check.val;
    ref syntax.Name id = ref _addr_id.val;

    assert(id != null);
    assert(obj != null);
    {
        var m = check.Uses;

        if (m != null) {
            m[id] = obj;
        }
    }

}

private static void recordImplicit(this ptr<Checker> _addr_check, syntax.Node node, Object obj) {
    ref Checker check = ref _addr_check.val;

    assert(node != null);
    assert(obj != null);
    {
        var m = check.Implicits;

        if (m != null) {
            m[node] = obj;
        }
    }

}

private static void recordSelection(this ptr<Checker> _addr_check, ptr<syntax.SelectorExpr> _addr_x, SelectionKind kind, Type recv, Object obj, slice<nint> index, bool indirect) {
    ref Checker check = ref _addr_check.val;
    ref syntax.SelectorExpr x = ref _addr_x.val;

    assert(obj != null && (recv == null || len(index) > 0));
    check.recordUse(x.Sel, obj);
    {
        var m = check.Selections;

        if (m != null) {
            m[x] = addr(new Selection(kind,recv,obj,index,indirect));
        }
    }

}

private static void recordScope(this ptr<Checker> _addr_check, syntax.Node node, ptr<Scope> _addr_scope) {
    ref Checker check = ref _addr_check.val;
    ref Scope scope = ref _addr_scope.val;

    assert(node != null);
    assert(scope != null);
    {
        var m = check.Scopes;

        if (m != null) {
            m[node] = scope;
        }
    }

}

} // end types2_package
