// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the Check function, which drives type-checking.

// package types -- go2cs converted at 2020 October 09 05:19:18 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\check.go
using errors = go.errors_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // debugging/development support
        private static readonly var debug = false; // leave on during development
        private static readonly var trace = false; // turn on for detailed type resolution traces

        // If Strict is set, the type-checker enforces additional
        // rules not specified by the Go 1 spec, but which will
        // catch guaranteed run-time errors if the respective
        // code is executed. In other words, programs passing in
        // Strict mode are Go 1 compliant, but not all Go 1 programs
        // will pass in Strict mode. The additional rules are:
        //
        // - A type assertion x.(T) where T is an interface type
        //   is invalid if any (statically known) method that exists
        //   for both x and T have different signatures.
        //
        private static readonly var strict = false;

        // exprInfo stores information about an untyped expression.


        // exprInfo stores information about an untyped expression.
        private partial struct exprInfo
        {
            public bool isLhs; // expression is lhs operand of a shift with delayed type-check
            public operandMode mode;
            public ptr<Basic> typ;
            public constant.Value val; // constant value; or nil (if not a constant)
        }

        // A context represents the context within which an object is type-checked.
        private partial struct context
        {
            public ptr<declInfo> decl; // package-level declaration whose init expression/function body is checked
            public ptr<Scope> scope; // top-most scope for lookups
            public token.Pos pos; // if valid, identifiers are looked up as if at position pos (used by Eval)
            public constant.Value iota; // value of iota in a constant declaration; nil otherwise
            public ptr<Signature> sig; // function signature if inside a function; nil otherwise
            public map<ptr<ast.CallExpr>, bool> isPanic; // set of panic call expressions (used for termination check)
            public bool hasLabel; // set if a function makes use of labels (only ~1% of functions); unused outside functions
            public bool hasCallOrRecv; // set if an expression contains a function call or channel receive operation
        }

        // lookup looks up name in the current context and returns the matching object, or nil.
        private static Object lookup(this ptr<context> _addr_ctxt, @string name)
        {
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
        private partial struct importKey
        {
            public @string path;
            public @string dir;
        }

        // A Checker maintains the state of the type checker.
        // It must be created with NewChecker.
        public partial struct Checker
        {
            public ptr<Config> conf;
            public ptr<token.FileSet> fset;
            public ptr<Package> pkg;
            public ref ptr<Info> ptr<Info> => ref ptr<Info>_ptr;
            public map<Object, ptr<declInfo>> objMap; // maps package-level objects and (non-interface) methods to declaration info
            public map<importKey, ptr<Package>> impMap; // maps (import path, source directory) to (complete or fake) package
            public map<ptr<Interface>, slice<token.Pos>> posMap; // maps interface types to lists of embedded interface positions
            public map<@string, long> pkgCnt; // counts number of imported packages with a given name (for better error messages)

// information collected during type-checking of a set of package files
// (initialized by Files, valid only for the duration of check.Files;
// maps and lists are allocated on demand)
            public slice<ptr<ast.File>> files; // package files
            public map<ptr<Scope>, map<ptr<Package>, token.Pos>> unusedDotImports; // positions of unused dot-imported packages for each file scope

            public error firstErr; // first error encountered
            public map<ptr<TypeName>, slice<ptr<Func>>> methods; // maps package scope type names to associated non-blank (non-interface) methods
            public map<ast.Expr, exprInfo> untyped; // map of expressions without final type
            public slice<Action> delayed; // stack of delayed action segments; segments are processed in FIFO order
            public slice<Action> finals; // list of final actions; processed at the end of type-checking the current set of files
            public slice<Object> objPath; // path of object dependencies during type inference (for cycle reporting)

// context within which the current object is type-checked
// (valid only for the duration of type-checking a specific object)
            public ref context context => ref context_val; // debugging
            public long indent; // indentation for tracing
        }

        // addUnusedImport adds the position of a dot-imported package
        // pkg to the map of dot imports for the given file scope.
        private static void addUnusedDotImport(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope, ptr<Package> _addr_pkg, token.Pos pos)
        {
            ref Checker check = ref _addr_check.val;
            ref Scope scope = ref _addr_scope.val;
            ref Package pkg = ref _addr_pkg.val;

            var mm = check.unusedDotImports;
            if (mm == null)
            {
                mm = make_map<ptr<Scope>, map<ptr<Package>, token.Pos>>();
                check.unusedDotImports = mm;
            }

            var m = mm[scope];
            if (m == null)
            {
                m = make_map<ptr<Package>, token.Pos>();
                mm[scope] = m;
            }

            m[pkg] = pos;

        }

        // addDeclDep adds the dependency edge (check.decl -> to) if check.decl exists
        private static void addDeclDep(this ptr<Checker> _addr_check, Object to)
        {
            ref Checker check = ref _addr_check.val;

            var from = check.decl;
            if (from == null)
            {
                return ; // not in a package-level init expression
            }

            {
                var (_, found) = check.objMap[to];

                if (!found)
                {
                    return ; // to is not a package-level object
                }

            }

            from.addDep(to);

        }

        private static void rememberUntyped(this ptr<Checker> _addr_check, ast.Expr e, bool lhs, operandMode mode, ptr<Basic> _addr_typ, constant.Value val)
        {
            ref Checker check = ref _addr_check.val;
            ref Basic typ = ref _addr_typ.val;

            var m = check.untyped;
            if (m == null)
            {
                m = make_map<ast.Expr, exprInfo>();
                check.untyped = m;
            }

            m[e] = new exprInfo(lhs,mode,typ,val);

        }

        // later pushes f on to the stack of actions that will be processed later;
        // either at the end of the current statement, or in case of a local constant
        // or variable declaration, before the constant or variable is in scope
        // (so that f still sees the scope before any new declarations).
        private static void later(this ptr<Checker> _addr_check, Action f)
        {
            ref Checker check = ref _addr_check.val;

            check.delayed = append(check.delayed, f);
        }

        // atEnd adds f to the list of actions processed at the end
        // of type-checking, before initialization order computation.
        // Actions added by atEnd are processed after any actions
        // added by later.
        private static void atEnd(this ptr<Checker> _addr_check, Action f)
        {
            ref Checker check = ref _addr_check.val;

            check.finals = append(check.finals, f);
        }

        // push pushes obj onto the object path and returns its index in the path.
        private static long push(this ptr<Checker> _addr_check, Object obj)
        {
            ref Checker check = ref _addr_check.val;

            check.objPath = append(check.objPath, obj);
            return len(check.objPath) - 1L;
        }

        // pop pops and returns the topmost object from the object path.
        private static Object pop(this ptr<Checker> _addr_check)
        {
            ref Checker check = ref _addr_check.val;

            var i = len(check.objPath) - 1L;
            var obj = check.objPath[i];
            check.objPath[i] = null;
            check.objPath = check.objPath[..i];
            return obj;
        }

        // NewChecker returns a new Checker instance for a given package.
        // Package files may be added incrementally via checker.Files.
        public static ptr<Checker> NewChecker(ptr<Config> _addr_conf, ptr<token.FileSet> _addr_fset, ptr<Package> _addr_pkg, ptr<Info> _addr_info)
        {
            ref Config conf = ref _addr_conf.val;
            ref token.FileSet fset = ref _addr_fset.val;
            ref Package pkg = ref _addr_pkg.val;
            ref Info info = ref _addr_info.val;
 
            // make sure we have a configuration
            if (conf == null)
            {
                conf = @new<Config>();
            } 

            // make sure we have an info struct
            if (info == null)
            {
                info = @new<Info>();
            }

            return addr(new Checker(conf:conf,fset:fset,pkg:pkg,Info:info,objMap:make(map[Object]*declInfo),impMap:make(map[importKey]*Package),posMap:make(map[*Interface][]token.Pos),pkgCnt:make(map[string]int),));

        }

        // initFiles initializes the files-specific portion of checker.
        // The provided files must all belong to the same package.
        private static void initFiles(this ptr<Checker> _addr_check, slice<ptr<ast.File>> files)
        {
            ref Checker check = ref _addr_check.val;
 
            // start with a clean slate (check.Files may be called multiple times)
            check.files = null;
            check.unusedDotImports = null;

            check.firstErr = null;
            check.methods = null;
            check.untyped = null;
            check.delayed = null;
            check.finals = null; 

            // determine package name and collect valid files
            var pkg = check.pkg;
            foreach (var (_, file) in files)
            {
                {
                    var name = file.Name.Name;


                    if (pkg.name == "")
                    {
                        if (name != "_")
                        {
                            pkg.name = name;
                        }
                        else
                        {
                            check.errorf(file.Name.Pos(), "invalid package name _");
                        }

                        fallthrough = true;

                    }
                    if (fallthrough || pkg.name == name)
                    {
                        check.files = append(check.files, file);
                        goto __switch_break0;
                    }
                    // default: 
                        check.errorf(file.Package, "package %s; expected %s", name, pkg.name); 
                        // ignore this file

                    __switch_break0:;
                }

            }

        }

        // A bailout panic is used for early termination.
        private partial struct bailout
        {
        }

        private static void handleBailout(this ptr<Checker> _addr_check, ptr<error> _addr_err) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref error err = ref _addr_err.val;

            switch (recover().type())
            {
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
        private static error Files(this ptr<Checker> _addr_check, slice<ptr<ast.File>> files)
        {
            ref Checker check = ref _addr_check.val;

            return error.As(check.checkFiles(files))!;
        }

        private static var errBadCgo = errors.New("cannot use FakeImportC and go115UsesCgo together");

        private static error checkFiles(this ptr<Checker> _addr_check, slice<ptr<ast.File>> files) => func((defer, _, __) =>
        {
            error err = default!;
            ref Checker check = ref _addr_check.val;

            if (check.conf.FakeImportC && check.conf.go115UsesCgo)
            {
                return error.As(errBadCgo)!;
            }

            defer(check.handleBailout(_addr_err));

            check.initFiles(files);

            check.collectObjects();

            check.packageObjects();

            check.processDelayed(0L); // incl. all functions
            check.processFinals();

            check.initOrder();

            if (!check.conf.DisableUnusedImportCheck)
            {
                check.unusedImports();
            }

            check.recordUntyped();

            check.pkg.complete = true;
            return ;

        });

        // processDelayed processes all delayed actions pushed after top.
        private static void processDelayed(this ptr<Checker> _addr_check, long top)
        {
            ref Checker check = ref _addr_check.val;
 
            // If each delayed action pushes a new action, the
            // stack will continue to grow during this loop.
            // However, it is only processing functions (which
            // are processed in a delayed fashion) that may
            // add more actions (such as nested functions), so
            // this is a sufficiently bounded process.
            for (var i = top; i < len(check.delayed); i++)
            {
                check.delayed[i](); // may append to check.delayed
            }

            assert(top <= len(check.delayed)); // stack must not have shrunk
            check.delayed = check.delayed[..top];

        }

        private static void processFinals(this ptr<Checker> _addr_check) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;

            var n = len(check.finals);
            foreach (var (_, f) in check.finals)
            {
                f(); // must not append to check.finals
            }
            if (len(check.finals) != n)
            {
                panic("internal error: final action list grew");
            }

        });

        private static void recordUntyped(this ptr<Checker> _addr_check)
        {
            ref Checker check = ref _addr_check.val;

            if (!debug && check.Types == null)
            {
                return ; // nothing to do
            }

            foreach (var (x, info) in check.untyped)
            {
                if (debug && isTyped(info.typ))
                {
                    check.dump("%v: %s (type %s) is typed", x.Pos(), x, info.typ);
                    unreachable();
                }

                check.recordTypeAndValue(x, info.mode, info.typ, info.val);

            }

        }

        private static void recordTypeAndValue(this ptr<Checker> _addr_check, ast.Expr x, operandMode mode, Type typ, constant.Value val)
        {
            ref Checker check = ref _addr_check.val;

            assert(x != null);
            assert(typ != null);
            if (mode == invalid)
            {
                return ; // omit
            }

            if (mode == constant_)
            {
                assert(val != null);
                assert(typ == Typ[Invalid] || isConstType(typ));
            }

            {
                var m = check.Types;

                if (m != null)
                {
                    m[x] = new TypeAndValue(mode,typ,val);
                }

            }

        }

        private static void recordBuiltinType(this ptr<Checker> _addr_check, ast.Expr f, ptr<Signature> _addr_sig)
        {
            ref Checker check = ref _addr_check.val;
            ref Signature sig = ref _addr_sig.val;
 
            // f must be a (possibly parenthesized) identifier denoting a built-in
            // (built-ins in package unsafe always produce a constant result and
            // we don't record their signatures, so we don't see qualified idents
            // here): record the signature for f and possible children.
            while (true)
            {
                check.recordTypeAndValue(f, builtin, sig, null);
                switch (f.type())
                {
                    case ptr<ast.Ident> p:
                        return ; // we're done
                        break;
                    case ptr<ast.ParenExpr> p:
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

        private static void recordCommaOkTypes(this ptr<Checker> _addr_check, ast.Expr x, array<Type> a)
        {
            a = a.Clone();
            ref Checker check = ref _addr_check.val;

            assert(x != null);
            if (a[0L] == null || a[1L] == null)
            {
                return ;
            }

            assert(isTyped(a[0L]) && isTyped(a[1L]) && (isBoolean(a[1L]) || a[1L] == universeError));
            {
                var m = check.Types;

                if (m != null)
                {
                    while (true)
                    {
                        var tv = m[x];
                        assert(tv.Type != null); // should have been recorded already
                        var pos = x.Pos();
                        tv.Type = NewTuple(NewVar(pos, check.pkg, "", a[0L]), NewVar(pos, check.pkg, "", a[1L]));
                        m[x] = tv; 
                        // if x is a parenthesized expression (p.X), update p.X
                        ptr<ast.ParenExpr> (p, _) = x._<ptr<ast.ParenExpr>>();
                        if (p == null)
                        {
                            break;
                        }

                        x = p.X;

                    }


                }

            }

        }

        private static void recordDef(this ptr<Checker> _addr_check, ptr<ast.Ident> _addr_id, Object obj)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.Ident id = ref _addr_id.val;

            assert(id != null);
            {
                var m = check.Defs;

                if (m != null)
                {
                    m[id] = obj;
                }

            }

        }

        private static void recordUse(this ptr<Checker> _addr_check, ptr<ast.Ident> _addr_id, Object obj)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.Ident id = ref _addr_id.val;

            assert(id != null);
            assert(obj != null);
            {
                var m = check.Uses;

                if (m != null)
                {
                    m[id] = obj;
                }

            }

        }

        private static void recordImplicit(this ptr<Checker> _addr_check, ast.Node node, Object obj)
        {
            ref Checker check = ref _addr_check.val;

            assert(node != null);
            assert(obj != null);
            {
                var m = check.Implicits;

                if (m != null)
                {
                    m[node] = obj;
                }

            }

        }

        private static void recordSelection(this ptr<Checker> _addr_check, ptr<ast.SelectorExpr> _addr_x, SelectionKind kind, Type recv, Object obj, slice<long> index, bool indirect)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.SelectorExpr x = ref _addr_x.val;

            assert(obj != null && (recv == null || len(index) > 0L));
            check.recordUse(x.Sel, obj);
            {
                var m = check.Selections;

                if (m != null)
                {
                    m[x] = addr(new Selection(kind,recv,obj,index,indirect));
                }

            }

        }

        private static void recordScope(this ptr<Checker> _addr_check, ast.Node node, ptr<Scope> _addr_scope)
        {
            ref Checker check = ref _addr_check.val;
            ref Scope scope = ref _addr_scope.val;

            assert(node != null);
            assert(scope != null);
            {
                var m = check.Scopes;

                if (m != null)
                {
                    m[node] = scope;
                }

            }

        }
    }
}}
