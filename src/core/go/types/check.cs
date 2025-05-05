// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements the Check function, which drives type-checking.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constant = go.constant_package;
using token = go.token_package;
using godebug = @internal.godebug_package;
using static @internal.types.errors_package;
using strings = strings_package;
using atomic = sync.atomic_package;
using @internal;
using sync;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

// nopos, noposn indicate an unknown position
internal static tokenꓸPos nopos;

internal static atPos noposn = ((atPos)nopos);

// debugging/development support
internal const bool debug = false; // leave on during development

// gotypesalias controls the use of Alias types.
// As of Apr 16 2024 they are used by default.
// To disable their use, set GODEBUG to gotypesalias=0.
// This GODEBUG flag will be removed in the near future (tentatively Go 1.24).
internal static ж<godebug.Setting> gotypesalias = godebug.New("gotypesalias"u8);

// _aliasAny changes the behavior of [Scope.Lookup] for "any" in the
// [Universe] scope.
//
// This is necessary because while Alias creation is controlled by
// [Config._EnableAlias], based on the gotypealias variable, the representation
// of "any" is a global. In [Scope.Lookup], we select this global
// representation based on the result of [aliasAny], but as a result need to
// guard against this behavior changing during the type checking pass.
// Therefore we implement the following rule: any number of goroutines can type
// check concurrently with the same EnableAlias value, but if any goroutine
// tries to type check concurrently with a different EnableAlias value, we
// panic.
//
// To achieve this, _aliasAny is a state machine:
//
//	0:        no type checking is occurring
//	negative: type checking is occurring without _EnableAlias set
//	positive: type checking is occurring with _EnableAlias set
internal static int32 _aliasAny;

internal static bool aliasAny() {
    @string v = gotypesalias.Value();
    var useAlias = v != "0"u8;
    var inuse = atomic.LoadInt32(Ꮡ(_aliasAny));
    if (inuse != 0 && useAlias != (inuse > 0)) {
        throw panic(fmt.Sprintf("gotypealias mutated during type checking, gotypesalias=%s, inuse=%d"u8, v, inuse));
    }
    return useAlias;
}

// exprInfo stores information about an untyped expression.
[GoType] partial struct exprInfo {
    internal bool isLhs; // expression is lhs operand of a shift with delayed type-check
    internal operandMode mode;
    internal ж<Basic> typ;
    internal go.constant_package.Value val; // constant value; or nil (if not a constant)
}

// An environment represents the environment within which an object is
// type-checked.
[GoType] partial struct environment {
    internal ж<declInfo> decl;           // package-level declaration whose init expression/function body is checked
    internal ж<ΔScope> scope;            // top-most scope for lookups
    internal go.token_package.ΔPos pos;            // if valid, identifiers are looked up as if at position pos (used by Eval)
    internal go.constant_package.Value iota;         // value of iota in a constant declaration; nil otherwise
    internal positioner errpos;             // if set, identifier position of a constant with inherited initializer
    internal bool inTParamList;                   // set if inside a type parameter list
    internal ж<ΔSignature> sig;        // function signature if inside a function; nil otherwise
    internal ast.CallExpr>bool isPanic; // set of panic call expressions (used for termination check)
    internal bool hasLabel;                   // set if a function makes use of labels (only ~1% of functions); unused outside functions
    internal bool hasCallOrRecv;                   // set if an expression contains a function call or channel receive operation
}

// lookup looks up name in the current environment and returns the matching object, or nil.
[GoRecv] internal static Object lookup(this ref environment env, @string name) {
    (_, obj) = env.scope.LookupParent(name, env.pos);
    return obj;
}

// An importKey identifies an imported package by import path and source directory
// (directory containing the file containing the import). In practice, the directory
// may always be the same, or may not matter. Given an (import path, directory), an
// importer must always return the same package (but given two different import paths,
// an importer may still return the same package by mapping them to the same package
// paths).
[GoType] partial struct importKey {
    internal @string path;
    internal @string dir;
}

// A dotImportKey describes a dot-imported object in the given scope.
[GoType] partial struct dotImportKey {
    internal ж<ΔScope> scope;
    internal @string name;
}

// An action describes a (delayed) action.
[GoType] partial struct action {
    internal Action f;      // action to be executed
    internal ж<actionDesc> desc; // action description; may be nil, requires debug to be set
}

// If debug is set, describef sets a printf-formatted description for action a.
// Otherwise, it is a no-op.
[GoRecv] internal static void describef(this ref action a, positioner pos, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (debug) {
        a.desc = Ꮡ(new actionDesc(pos, format, args));
    }
}

// An actionDesc provides information on an action.
// For debugging only.
[GoType] partial struct actionDesc {
    internal positioner pos;
    internal @string format;
    internal slice<any> args;
}

// A Checker maintains the state of the type checker.
// It must be created with [NewChecker].
[GoType] partial struct Checker {
    // package information
    // (initialized by NewChecker, valid for the life-time of checker)
    internal ж<Config> conf;
    internal ж<Context> ctxt; // context for de-duplicating instances
    internal ж<go.token_package.FileSet> fset;
    internal ж<Package> pkg;
    public partial ref ж<ΔInfo> Info { get; }
    internal goVersion version;              // accepted language version
    internal uint64 nextID;                 // unique Id for type parameters (first valid Id is 1)
    internal types.declInfo objMap;   // maps package-level objects and (non-interface) methods to declaration info
    internal types.Package impMap; // maps (import path, source directory) to (complete or fake) package
// see TODO in validtype.go
// valids instanceLookup // valid *Named (incl. instantiated) types per the validType check

    // pkgPathMap maps package names to the set of distinct import paths we've
    // seen for that name, anywhere in the import graph. It is used for
    // disambiguating package names in error messages.
    //
    // pkgPathMap is allocated lazily, so that we don't pay the price of building
    // it on the happy path. seenPkgMap tracks the packages that we've already
    // walked.
    internal map<@string, map<@string, bool>> pkgPathMap;
    internal map<ж<Package>, bool> seenPkgMap;
    // information collected during type-checking of a set of package files
    // (initialized by Files, valid only for the duration of check.Files;
    // maps and lists are allocated on demand)
    internal ast.File files;               // package files
    internal ast.File>string versions;      // maps files to version strings (each file has an entry); shared with Info.FileVersions if present
    internal slice<ж<PkgName>> imports;        // list of imported packages
    internal types.PkgName dotImportMap; // maps dot-imported objects to the package they were dot-imported through
    internal ast.Ident>*TypeParam recvTParamMap; // maps blank receiver type parameters to their type
    internal map<ж<TypeName>, bool> brokenAliases;   // set of aliases with broken (not yet determined) types
    internal types._TypeSet unionTypeSets;      // computed type sets for union types
    internal monoGraph mono;                 // graph for detecting non-monomorphizable instantiation loops
    internal error firstErr;                 // first error encountered
    internal types.Func methods; // maps package scope type names to associated non-blank (non-interface) methods
    internal ast.Expr>exprInfo untyped; // map of expressions without final type
    internal slice<action> delayed;         // stack of delayed action segments; segments are processed in FIFO order
    internal slice<Object> objPath;         // path of object dependencies during type inference (for cycle reporting)
    internal slice<cleaner> cleaners;        // list of types that may need a final cleanup at the end of type-checking
    // environment within which the current object is type-checked (valid only
    // for the duration of type-checking a specific object)
    internal partial ref environment environment { get; }
    // debugging
    internal nint indent; // indentation for tracing
}

// addDeclDep adds the dependency edge (check.decl -> to) if check.decl exists
[GoRecv] internal static void addDeclDep(this ref Checker check, Object to) {
    var from = check.decl;
    if (from == nil) {
        return;
    }
    // not in a package-level init expression
    {
        var _ = check.objMap[to];
        var found = check.objMap[to]; if (!found) {
            return;
        }
    }
    // to is not a package-level object
    from.addDep(to);
}

// Note: The following three alias-related functions are only used
//       when Alias types are not enabled.

// brokenAlias records that alias doesn't have a determined type yet.
// It also sets alias.typ to Typ[Invalid].
// Not used if check.conf._EnableAlias is set.
[GoRecv] public static void brokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) {
    ref var alias = ref Ꮡalias.val;

    assert(!check.conf._EnableAlias);
    if (check.brokenAliases == default!) {
        check.brokenAliases = new map<ж<TypeName>, bool>();
    }
    check.brokenAliases[alias] = true;
    alias.typ = Typ[Invalid];
}

// validAlias records that alias has the valid type typ (possibly Typ[Invalid]).
[GoRecv] public static void validAlias(this ref Checker check, ж<TypeName> Ꮡalias, ΔType typ) {
    ref var alias = ref Ꮡalias.val;

    assert(!check.conf._EnableAlias);
    delete(check.brokenAliases, Ꮡalias);
    alias.typ = typ;
}

// isBrokenAlias reports whether alias doesn't have a determined type yet.
[GoRecv] public static bool isBrokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) {
    ref var alias = ref Ꮡalias.val;

    assert(!check.conf._EnableAlias);
    return check.brokenAliases[alias];
}

[GoRecv] public static void rememberUntyped(this ref Checker check, ast.Expr e, bool lhs, operandMode mode, ж<Basic> Ꮡtyp, constant.Value val) {
    ref var typ = ref Ꮡtyp.val;

    var m = check.untyped;
    if (m == default!) {
        m = new ast.Expr>exprInfo();
        check.untyped = m;
    }
    m[e] = new exprInfo(lhs, mode, Ꮡtyp, val);
}

// later pushes f on to the stack of actions that will be processed later;
// either at the end of the current statement, or in case of a local constant
// or variable declaration, before the constant or variable is in scope
// (so that f still sees the scope before any new declarations).
// later returns the pushed action so one can provide a description
// via action.describef for debugging, if desired.
[GoRecv] internal static ж<action> later(this ref Checker check, Action f) {
    nint i = len(check.delayed);
    check.delayed = append(check.delayed, new action(f: f));
    return Ꮡ(check.delayed[i]);
}

// push pushes obj onto the object path and returns its index in the path.
[GoRecv] internal static nint push(this ref Checker check, Object obj) {
    check.objPath = append(check.objPath, obj);
    return len(check.objPath) - 1;
}

// pop pops and returns the topmost object from the object path.
[GoRecv] internal static Object pop(this ref Checker check) {
    nint i = len(check.objPath) - 1;
    var obj = check.objPath[i];
    check.objPath[i] = default!;
    check.objPath = check.objPath[..(int)(i)];
    return obj;
}

[GoType] partial interface cleaner {
    void cleanup();
}

// needsCleanup records objects/types that implement the cleanup method
// which will be called at the end of type-checking.
[GoRecv] internal static void needsCleanup(this ref Checker check, cleaner c) {
    check.cleaners = append(check.cleaners, c);
}

// NewChecker returns a new [Checker] instance for a given package.
// [Package] files may be added incrementally via checker.Files.
public static ж<Checker> NewChecker(ж<Config> Ꮡconf, ж<token.FileSet> Ꮡfset, ж<Package> Ꮡpkg, ж<ΔInfo> Ꮡinfo) {
    ref var conf = ref Ꮡconf.val;
    ref var fset = ref Ꮡfset.val;
    ref var pkg = ref Ꮡpkg.val;
    ref var info = ref Ꮡinfo.val;

    // make sure we have a configuration
    if (conf == nil) {
        conf = @new<Config>();
    }
    // make sure we have an info struct
    if (info == nil) {
        info = @new<ΔInfo>();
    }
    // Note: clients may call NewChecker with the Unsafe package, which is
    // globally shared and must not be mutated. Therefore NewChecker must not
    // mutate *pkg.
    //
    // (previously, pkg.goVersion was mutated here: go.dev/issue/61212)
    // In go/types, conf._EnableAlias is controlled by gotypesalias.
    conf._EnableAlias = gotypesalias.Value() != "0"u8;
    return Ꮡ(new Checker(
        conf: conf,
        ctxt: conf.Context,
        fset: fset,
        pkg: pkg,
        ΔInfo: info,
        version: asGoVersion(conf.GoVersion),
        objMap: new types.declInfo(),
        impMap: new types.Package()
    ));
}

// initFiles initializes the files-specific portion of checker.
// The provided files must all belong to the same package.
[GoRecv] internal static void initFiles(this ref Checker check, slice<ast.File> files) {
    // start with a clean slate (check.Files may be called multiple times)
    check.files = default!;
    check.imports = default!;
    check.dotImportMap = default!;
    check.firstErr = default!;
    check.methods = default!;
    check.untyped = default!;
    check.delayed = default!;
    check.objPath = default!;
    check.cleaners = default!;
    // determine package name and collect valid files
    var pkg = check.pkg;
    foreach (var (_, file) in files) {
        {
            @string name = (~file).Name.val.Name;
            var exprᴛ1 = (~pkg).name;
            var matchᴛ1 = false;
            if (exprᴛ1 == ""u8) { matchᴛ1 = true;
                if (name != "_"u8){
                    pkg.val.name = name;
                } else {
                    check.error(~(~file).Name, BlankPkgName, "invalid package name _"u8);
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 is name) {
                check.files = append(check.files, file);
            }
            else { /* default: */
                check.errorf(((atPos)(~file).Package), MismatchedPkgName, "package %s; expected package %s"u8, name, (~pkg).name);
            }
        }

    }
    // ignore this file
    // reuse Info.FileVersions if provided
    var versions = check.Info.FileVersions;
    if (versions == default!) {
        versions = new ast.File>string();
    }
    check.versions = versions;
    var pkgVersionOk = check.version.isValid();
    if (pkgVersionOk && len(files) > 0 && check.version.cmp(go_current) > 0) {
        check.errorf(~files[0], TooNew, "package requires newer Go version %v (application built with %v)"u8,
            check.version, go_current);
    }
    // determine Go version for each file
    foreach (var (_, file) in check.files) {
        // use unaltered Config.GoVersion by default
        // (This version string may contain dot-release numbers as in go1.20.1,
        // unlike file versions which are Go language versions only, if valid.)
        @string v = check.conf.GoVersion;
        // If the file specifies a version, use max(fileVersion, go1.21).
        {
            @string fileVersion = asGoVersion((~file).GoVersion); if (fileVersion.isValid()) {
                // Go 1.21 introduced the feature of setting the go.mod
                // go line to an early version of Go and allowing //go:build lines
                // to set the Go version in a given file. Versions Go 1.21 and later
                // can be set backwards compatibly as that was the first version
                // files with go1.21 or later build tags could be built with.
                //
                // Set the version to max(fileVersion, go1.21): That will allow a
                // downgrade to a version before go1.22, where the for loop semantics
                // change was made, while being backwards compatible with versions of
                // go before the new //go:build semantics were introduced.
                v = ((@string)versionMax(fileVersion, go1_21));
                // Report a specific error for each tagged file that's too new.
                // (Normally the build system will have filtered files by version,
                // but clients can present arbitrary files to the type checker.)
                if (fileVersion.cmp(go_current) > 0) {
                    // Use position of 'package [p]' for types/types2 consistency.
                    // (Ideally we would use the //build tag itself.)
                    check.errorf(~(~file).Name, TooNew, "file requires newer Go version %v (application built with %v)"u8, fileVersion, go_current);
                }
            }
        }
        versions[file] = v;
    }
}

internal static goVersion versionMax(goVersion a, goVersion b) {
    if (a.cmp(b) < 0) {
        return b;
    }
    return a;
}

// A bailout panic is used for early termination.
[GoType] partial struct bailout {
}

[GoRecv] public static void handleBailout(this ref Checker check, ж<error> Ꮡerr) {
    ref var err = ref Ꮡerr.val;

    switch (recover().type()) {
    case default! p: {
        err = check.firstErr;
        break;
    }
    case bailout p: {
        err = check.firstErr;
        break;
    }
    default: {
        var p = recover().type();
        throw panic(p);
        break;
    }}
}

// normal return or early exit
// re-panic

// Files checks the provided files as part of the checker's package.
[GoRecv] public static error /*err*/ Files(this ref Checker check, slice<ast.File> files) => func((defer, _) => {
    error err = default!;

    if (check.pkg == Unsafe) {
        // Defensive handling for Unsafe, which cannot be type checked, and must
        // not be mutated. See https://go.dev/issue/61212 for an example of where
        // Unsafe is passed to NewChecker.
        return default!;
    }
    // Avoid early returns here! Nearly all errors can be
    // localized to a piece of syntax and needn't prevent
    // type-checking of the rest of the package.
    deferǃ(check.handleBailout, Ꮡ(err), defer);
    check.checkFiles(files);
    return err;
});

// checkFiles type-checks the specified files. Errors are reported as
// a side effect, not by returning early, to ensure that well-formed
// syntax is properly type annotated even in a package containing
// errors.
[GoRecv] internal static void checkFiles(this ref Checker check, slice<ast.File> files) => func((defer, _) => {
    // Ensure that _EnableAlias is consistent among concurrent type checking
    // operations. See the documentation of [_aliasAny] for details.
    if (check.conf._EnableAlias){
        if (atomic.AddInt32(Ꮡ(_aliasAny), 1) <= 0) {
            throw panic("EnableAlias set while !EnableAlias type checking is ongoing");
        }
        deferǃ(atomic.AddInt32, Ꮡ(_aliasAny), -1, defer);
    } else {
        if (atomic.AddInt32(Ꮡ(_aliasAny), -1) >= 0) {
            throw panic("!EnableAlias set while EnableAlias type checking is ongoing");
        }
        deferǃ(atomic.AddInt32, Ꮡ(_aliasAny), 1, defer);
    }
    var print = (@string msg) => {
        if (check.conf._Trace) {
            fmt.Println();
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
    check.processDelayed(0);
    // incl. all functions
    print("== cleanup ==");
    check.cleanup();
    print("== initOrder ==");
    check.initOrder();
    if (!check.conf.DisableUnusedImportCheck) {
        print("== unusedImports ==");
        check.unusedImports();
    }
    print("== recordUntyped ==");
    check.recordUntyped();
    if (check.firstErr == default!) {
        // TODO(mdempsky): Ensure monomorph is safe when errors exist.
        check.monomorph();
    }
    check.pkg.goVersion = check.conf.GoVersion;
    check.pkg.complete = true;
    // no longer needed - release memory
    check.imports = default!;
    check.dotImportMap = default!;
    check.pkgPathMap = default!;
    check.seenPkgMap = default!;
    check.recvTParamMap = default!;
    check.brokenAliases = default!;
    check.unionTypeSets = default!;
    check.ctxt = default!;
});

// TODO(rFindley) There's more memory we should release at this point.

// processDelayed processes all delayed actions pushed after top.
[GoRecv] internal static void processDelayed(this ref Checker check, nint top) {
    // If each delayed action pushes a new action, the
    // stack will continue to grow during this loop.
    // However, it is only processing functions (which
    // are processed in a delayed fashion) that may
    // add more actions (such as nested functions), so
    // this is a sufficiently bounded process.
    for (nint i = top; i < len(check.delayed); i++) {
        var a = Ꮡ(check.delayed[i]);
        if (check.conf._Trace) {
            if ((~a).desc != nil){
                check.trace((~(~a).desc).pos.Pos(), "-- "u8 + (~(~a).desc).format, (~(~a).desc).args.ꓸꓸꓸ);
            } else {
                check.trace(nopos, "-- delayed %p"u8, (~a).f);
            }
        }
        (~a).f();
        // may append to check.delayed
        if (check.conf._Trace) {
            fmt.Println();
        }
    }
    assert(top <= len(check.delayed));
    // stack must not have shrunk
    check.delayed = check.delayed[..(int)(top)];
}

// cleanup runs cleanup for all collected cleaners.
[GoRecv] internal static void cleanup(this ref Checker check) {
    // Don't use a range clause since Named.cleanup may add more cleaners.
    for (nint i = 0; i < len(check.cleaners); i++) {
        check.cleaners[i].cleanup();
    }
    check.cleaners = default!;
}

[GoRecv] public static void record(this ref Checker check, ж<operand> Ꮡx) {
    ref var x = ref Ꮡx.val;

    // convert x into a user-friendly set of values
    // TODO(gri) this code can be simplified
    ΔType typ = default!;
    constant.Value val = default!;
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == invalid) {
        typ = ~Typ[Invalid];
    }
    else if (exprᴛ1 == novalue) {
        typ = ~(ж<Tuple>)(default!);
    }
    else if (exprᴛ1 == constant_) {
        typ = x.typ;
        val = x.val;
    }
    else { /* default: */
        typ = x.typ;
    }

    assert(x.expr != default! && typ != default!);
    if (isUntyped(typ)){
        // delay type and value recording until we know the type
        // or until the end of type checking
        check.rememberUntyped(x.expr, false, x.mode, typ._<Basic.val>(), val);
    } else {
        check.recordTypeAndValue(x.expr, x.mode, typ, val);
    }
}

[GoRecv] internal static void recordUntyped(this ref Checker check) {
    if (!debug && check.Types == default!) {
        return;
    }
    // nothing to do
    foreach (var (x, info) in check.untyped) {
        if (debug && isTyped(~info.typ)) {
            check.dump("%v: %s (type %s) is typed"u8, x.Pos(), x, info.typ);
            throw panic("unreachable");
        }
        check.recordTypeAndValue(x, info.mode, ~info.typ, info.val);
    }
}

[GoRecv] internal static void recordTypeAndValue(this ref Checker check, ast.Expr x, operandMode mode, ΔType typ, constant.Value val) {
    assert(x != default!);
    assert(typ != default!);
    if (mode == invalid) {
        return;
    }
    // omit
    if (mode == constant_) {
        assert(val != default!);
        // We check allBasic(typ, IsConstType) here as constant expressions may be
        // recorded as type parameters.
        assert(!isValid(typ) || allBasic(typ, IsConstType));
    }
    {
        var m = check.Types; if (m != default!) {
            m[x] = new TypeAndValue(mode, typ, val);
        }
    }
}

[GoRecv] public static void recordBuiltinType(this ref Checker check, ast.Expr f, ж<ΔSignature> Ꮡsig) {
    ref var sig = ref Ꮡsig.val;

    // f must be a (possibly parenthesized, possibly qualified)
    // identifier denoting a built-in (including unsafe's non-constant
    // functions Add and Slice): record the signature for f and possible
    // children.
    while (ᐧ) {
        check.recordTypeAndValue(f, Δbuiltin, ~sig, default!);
        switch (f.type()) {
        case ж<ast.Ident> p: {
            return;
        }
        case ж<ast.SelectorExpr> p: {
            return;
        }
        case ж<ast.ParenExpr> p: {
            f = p.val.X;
            break;
        }
        default: {
            var p = f.type();
            throw panic("unreachable");
            break;
        }}
    }
}

// we're done

// recordCommaOkTypes updates recorded types to reflect that x is used in a commaOk context
// (and therefore has tuple type).
[GoRecv] internal static void recordCommaOkTypes(this ref Checker check, ast.Expr x, slice<ж<operand>> a) {
    assert(x != default!);
    assert(len(a) == 2);
    if (a[0].mode == invalid) {
        return;
    }
    var t0 = a[0].typ;
    var t1 = a[1].typ;
    assert(isTyped(t0) && isTyped(t1) && (allBoolean(t1) || AreEqual(t1, universeError)));
    {
        var m = check.Types; if (m != default!) {
            while (ᐧ) {
                var tv = m[x];
                assert(tv.Type != default!);
                // should have been recorded already
                tokenꓸPos pos = x.Pos();
                tv.Type = NewTuple(
                    NewVar(pos, check.pkg, ""u8, t0),
                    NewVar(pos, check.pkg, ""u8, t1));
                m[x] = tv;
                // if x is a parenthesized expression (p.X), update p.X
                var (p, _) = x._<ж<ast.ParenExpr>>(ᐧ);
                if (p == nil) {
                    break;
                }
                x = p.val.X;
            }
        }
    }
}

// recordInstance records instantiation information into check.Info, if the
// Instances map is non-nil. The given expr must be an ident, selector, or
// index (list) expr with ident or selector operand.
//
// TODO(rfindley): the expr parameter is fragile. See if we can access the
// instantiated identifier in some other way.
[GoRecv] internal static void recordInstance(this ref Checker check, ast.Expr expr, slice<ΔType> targs, ΔType typ) {
    var ident = instantiatedIdent(expr);
    assert(ident != nil);
    assert(typ != default!);
    {
        var m = check.Instances; if (m != default!) {
            m[ident] = new Instance(newTypeList(targs), typ);
        }
    }
}

internal static ж<ast.Ident> instantiatedIdent(ast.Expr expr) {
    ast.Expr selOrIdent = default!;
    switch (expr.type()) {
    case ж<ast.IndexExpr> e: {
        selOrIdent = e.val.X;
        break;
    }
    case ж<ast.IndexListExpr> e: {
        selOrIdent = e.val.X;
        break;
    }
    case ж<ast.SelectorExpr> e: {
        selOrIdent = e;
        break;
    }
    case ж<ast.Ident> e: {
        selOrIdent = e;
        break;
    }}
    switch (selOrIdent.type()) {
    case ж<ast.Ident> x: {
        return x;
    }
    case ж<ast.SelectorExpr> x: {
        return (~x).Sel;
    }}
    // extra debugging of #63933
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    buf.WriteString("instantiated ident not found; please report: "u8);
    ast.Fprint(~Ꮡbuf, token.NewFileSet(), expr, ast.NotNilFilter);
    throw panic(buf.String());
}

[GoRecv] public static void recordDef(this ref Checker check, ж<ast.Ident> Ꮡid, Object obj) {
    ref var id = ref Ꮡid.val;

    assert(id != nil);
    {
        var m = check.Defs; if (m != default!) {
            m[id] = obj;
        }
    }
}

[GoRecv] public static void recordUse(this ref Checker check, ж<ast.Ident> Ꮡid, Object obj) {
    ref var id = ref Ꮡid.val;

    assert(id != nil);
    assert(obj != default!);
    {
        var m = check.Uses; if (m != default!) {
            m[id] = obj;
        }
    }
}

[GoRecv] internal static void recordImplicit(this ref Checker check, ast.Node node, Object obj) {
    assert(node != default!);
    assert(obj != default!);
    {
        var m = check.Implicits; if (m != default!) {
            m[node] = obj;
        }
    }
}

[GoRecv] public static void recordSelection(this ref Checker check, ж<ast.SelectorExpr> Ꮡx, SelectionKind kind, ΔType recv, Object obj, slice<nint> index, bool indirect) {
    ref var x = ref Ꮡx.val;

    assert(obj != default! && (recv == default! || len(index) > 0));
    check.recordUse(x.Sel, obj);
    {
        var m = check.Selections; if (m != default!) {
            m[x] = Ꮡ(new Selection(kind, recv, obj, index, indirect));
        }
    }
}

[GoRecv] public static void recordScope(this ref Checker check, ast.Node node, ж<ΔScope> Ꮡscope) {
    ref var scope = ref Ꮡscope.val;

    assert(node != default!);
    assert(scope != nil);
    {
        var m = check.Scopes; if (m != default!) {
            m[node] = scope;
        }
    }
}

} // end types_package
