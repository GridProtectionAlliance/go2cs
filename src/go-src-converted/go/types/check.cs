// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the Check function, which drives type-checking.

// package types -- go2cs converted at 2020 August 29 08:47:24 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\check.go
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

        // funcInfo stores the information required for type-checking a function.
        private partial struct funcInfo
        {
            public @string name; // for debugging/tracing only
            public ptr<declInfo> decl; // for cycle detection
            public ptr<Signature> sig;
            public ptr<ast.BlockStmt> body;
        }

        // A context represents the context within which an object is type-checked.
        private partial struct context
        {
            public ptr<declInfo> decl; // package-level declaration whose init expression/function body is checked
            public ptr<Scope> scope; // top-most scope for lookups
            public constant.Value iota; // value of iota in a constant declaration; nil otherwise
            public ptr<Signature> sig; // function signature if inside a function; nil otherwise
            public bool hasLabel; // set if a function makes use of labels (only ~1% of functions); unused outside functions
            public bool hasCallOrRecv; // set if an expression contains a function call or channel receive operation
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
            public ref Info Info => ref Info_ptr;
            public map<Object, ref declInfo> objMap; // maps package-level object to declaration info
            public map<importKey, ref Package> impMap; // maps (import path, source directory) to (complete or fake) package

// information collected during type-checking of a set of package files
// (initialized by Files, valid only for the duration of check.Files;
// maps and lists are allocated on demand)
            public slice<ref ast.File> files; // package files
            public map<ref Scope, map<ref Package, token.Pos>> unusedDotImports; // positions of unused dot-imported packages for each file scope

            public error firstErr; // first error encountered
            public map<@string, slice<ref Func>> methods; // maps type names to associated methods
            public map<ast.Expr, exprInfo> untyped; // map of expressions without final type
            public slice<funcInfo> funcs; // list of functions to type-check
            public slice<Action> delayed; // delayed checks requiring fully setup types

// context within which the current object is type-checked
// (valid only for the duration of type-checking a specific object)
            public ref context context => ref context_val;
            public token.Pos pos; // if valid, identifiers are looked up as if at position pos (used by Eval)

// debugging
            public long indent; // indentation for tracing
        }

        // addUnusedImport adds the position of a dot-imported package
        // pkg to the map of dot imports for the given file scope.
        private static void addUnusedDotImport(this ref Checker check, ref Scope scope, ref Package pkg, token.Pos pos)
        {
            var mm = check.unusedDotImports;
            if (mm == null)
            {
                mm = make_map<ref Scope, map<ref Package, token.Pos>>();
                check.unusedDotImports = mm;
            }
            var m = mm[scope];
            if (m == null)
            {
                m = make_map<ref Package, token.Pos>();
                mm[scope] = m;
            }
            m[pkg] = pos;
        }

        // addDeclDep adds the dependency edge (check.decl -> to) if check.decl exists
        private static void addDeclDep(this ref Checker check, Object to)
        {
            var from = check.decl;
            if (from == null)
            {
                return; // not in a package-level init expression
            }
            {
                var (_, found) = check.objMap[to];

                if (!found)
                {
                    return; // to is not a package-level object
                }

            }
            from.addDep(to);
        }

        private static void assocMethod(this ref Checker check, @string tname, ref Func meth)
        {
            var m = check.methods;
            if (m == null)
            {
                m = make_map<@string, slice<ref Func>>();
                check.methods = m;
            }
            m[tname] = append(m[tname], meth);
        }

        private static void rememberUntyped(this ref Checker check, ast.Expr e, bool lhs, operandMode mode, ref Basic typ, constant.Value val)
        {
            var m = check.untyped;
            if (m == null)
            {
                m = make_map<ast.Expr, exprInfo>();
                check.untyped = m;
            }
            m[e] = new exprInfo(lhs,mode,typ,val);
        }

        private static void later(this ref Checker check, @string name, ref declInfo decl, ref Signature sig, ref ast.BlockStmt body)
        {
            check.funcs = append(check.funcs, new funcInfo(name,decl,sig,body));
        }

        private static void delay(this ref Checker check, Action f)
        {
            check.delayed = append(check.delayed, f);
        }

        // NewChecker returns a new Checker instance for a given package.
        // Package files may be added incrementally via checker.Files.
        public static ref Checker NewChecker(ref Config conf, ref token.FileSet fset, ref Package pkg, ref Info info)
        { 
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
            return ref new Checker(conf:conf,fset:fset,pkg:pkg,Info:info,objMap:make(map[Object]*declInfo),impMap:make(map[importKey]*Package),);
        }

        // initFiles initializes the files-specific portion of checker.
        // The provided files must all belong to the same package.
        private static void initFiles(this ref Checker check, slice<ref ast.File> files)
        { 
            // start with a clean slate (check.Files may be called multiple times)
            check.files = null;
            check.unusedDotImports = null;

            check.firstErr = null;
            check.methods = null;
            check.untyped = null;
            check.funcs = null;
            check.delayed = null; 

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

        private static void handleBailout(this ref Checker _check, ref error _err) => func(_check, _err, (ref Checker check, ref error err, Defer _, Panic panic, Recover __) =>
        {
            switch (recover().type())
            {
                case bailout p:
                    err.Value = check.firstErr;
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
        private static error Files(this ref Checker check, slice<ref ast.File> files)
        {
            return error.As(check.checkFiles(files));
        }

        private static error checkFiles(this ref Checker _check, slice<ref ast.File> files) => func(_check, (ref Checker check, Defer defer, Panic _, Recover __) =>
        {
            defer(check.handleBailout(ref err));

            check.initFiles(files);

            check.collectObjects();

            check.packageObjects(check.resolveOrder());

            check.functionBodies();

            check.initOrder();

            if (!check.conf.DisableUnusedImportCheck)
            {
                check.unusedImports();
            } 

            // perform delayed checks
            foreach (var (_, f) in check.delayed)
            {
                f();
            }
            check.recordUntyped();

            check.pkg.complete = true;
            return;
        });

        private static void recordUntyped(this ref Checker check)
        {
            if (!debug && check.Types == null)
            {
                return; // nothing to do
            }
            foreach (var (x, info) in check.untyped)
            {
                if (debug && isTyped(info.typ))
                {
                    check.dump("%s: %s (type %s) is typed", x.Pos(), x, info.typ);
                    unreachable();
                }
                check.recordTypeAndValue(x, info.mode, info.typ, info.val);
            }
        }

        private static void recordTypeAndValue(this ref Checker check, ast.Expr x, operandMode mode, Type typ, constant.Value val)
        {
            assert(x != null);
            assert(typ != null);
            if (mode == invalid)
            {
                return; // omit
            }
            assert(typ != null);
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

        private static void recordBuiltinType(this ref Checker check, ast.Expr f, ref Signature sig)
        { 
            // f must be a (possibly parenthesized) identifier denoting a built-in
            // (built-ins in package unsafe always produce a constant result and
            // we don't record their signatures, so we don't see qualified idents
            // here): record the signature for f and possible children.
            while (true)
            {
                check.recordTypeAndValue(f, builtin, sig, null);
                switch (f.type())
                {
                    case ref ast.Ident p:
                        return; // we're done
                        break;
                    case ref ast.ParenExpr p:
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

        private static void recordCommaOkTypes(this ref Checker check, ast.Expr x, array<Type> a)
        {
            assert(x != null);
            if (a[0L] == null || a[1L] == null)
            {
                return;
            }
            assert(isTyped(a[0L]) && isTyped(a[1L]) && isBoolean(a[1L]));
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
                        ref ast.ParenExpr (p, _) = x._<ref ast.ParenExpr>();
                        if (p == null)
                        {
                            break;
                        }
                        x = p.X;
                    }

                }

            }
        }

        private static void recordDef(this ref Checker check, ref ast.Ident id, Object obj)
        {
            assert(id != null);
            {
                var m = check.Defs;

                if (m != null)
                {
                    m[id] = obj;
                }

            }
        }

        private static void recordUse(this ref Checker check, ref ast.Ident id, Object obj)
        {
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

        private static void recordImplicit(this ref Checker check, ast.Node node, Object obj)
        {
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

        private static void recordSelection(this ref Checker check, ref ast.SelectorExpr x, SelectionKind kind, Type recv, Object obj, slice<long> index, bool indirect)
        {
            assert(obj != null && (recv == null || len(index) > 0L));
            check.recordUse(x.Sel, obj);
            {
                var m = check.Selections;

                if (m != null)
                {
                    m[x] = ref new Selection(kind,recv,obj,index,indirect);
                }

            }
        }

        private static void recordScope(this ref Checker check, ast.Node node, ref Scope scope)
        {
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
