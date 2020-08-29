// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:47:27 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\decl.go
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
        private static void reportAltDecl(this ref Checker check, Object obj)
        {
            {
                var pos = obj.Pos();

                if (pos.IsValid())
                { 
                    // We use "other" rather than "previous" here because
                    // the first declaration seen may not be textually
                    // earlier in the source.
                    check.errorf(pos, "\tother declaration of %s", obj.Name()); // secondary error, \t indented
                }
            }
        }

        private static void declare(this ref Checker check, ref Scope scope, ref ast.Ident id, Object obj, token.Pos pos)
        { 
            // spec: "The blank identifier, represented by the underscore
            // character _, may be used in a declaration like any other
            // identifier but the declaration does not introduce a new
            // binding."
            if (obj.Name() != "_")
            {
                {
                    var alt = scope.Insert(obj);

                    if (alt != null)
                    {
                        check.errorf(obj.Pos(), "%s redeclared in this block", obj.Name());
                        check.reportAltDecl(alt);
                        return;
                    }

                }
                obj.setScopePos(pos);
            }
            if (id != null)
            {
                check.recordDef(id, obj);
            }
        }

        // objDecl type-checks the declaration of obj in its respective (file) context.
        // See check.typ for the details on def and path.
        private static void objDecl(this ref Checker _check, Object obj, ref Named _def, slice<ref TypeName> path) => func(_check, _def, (ref Checker check, ref Named def, Defer defer, Panic _, Recover __) =>
        {
            if (obj.Type() != null)
            {
                return; // already checked - nothing to do
            }
            if (trace)
            {
                check.trace(obj.Pos(), "-- declaring %s", obj.Name());
                check.indent++;
                defer(() =>
                {
                    check.indent--;
                    check.trace(obj.Pos(), "=> %s", obj);
                }());
            }
            var d = check.objMap[obj];
            if (d == null)
            {
                check.dump("%s: %s should have been declared", obj.Pos(), obj.Name());
                unreachable();
            } 

            // save/restore current context and setup object context
            defer(ctxt =>
            {
                check.context = ctxt;
            }(check.context));
            check.context = new context(scope:d.file,); 

            // Const and var declarations must not have initialization
            // cycles. We track them by remembering the current declaration
            // in check.decl. Initialization expressions depending on other
            // consts, vars, or functions, add dependencies to the current
            // check.decl.
            switch (obj.type())
            {
                case ref Const obj:
                    check.decl = d; // new package-level const decl
                    check.constDecl(obj, d.typ, d.init);
                    break;
                case ref Var obj:
                    check.decl = d; // new package-level var decl
                    check.varDecl(obj, d.lhs, d.typ, d.init);
                    break;
                case ref TypeName obj:
                    check.typeDecl(obj, d.typ, def, path, d.alias);
                    break;
                case ref Func obj:
                    check.funcDecl(obj, d);
                    break;
                default:
                {
                    var obj = obj.type();
                    unreachable();
                    break;
                }
            }
        });

        private static void constDecl(this ref Checker _check, ref Const _obj, ast.Expr typ, ast.Expr init) => func(_check, _obj, (ref Checker check, ref Const obj, Defer defer, Panic _, Recover __) =>
        {
            assert(obj.typ == null);

            if (obj.visited)
            {
                obj.typ = Typ[Invalid];
                return;
            }
            obj.visited = true; 

            // use the correct value of iota
            assert(check.iota == null);
            check.iota = obj.val;
            defer(() =>
            {
                check.iota = null;

            }()); 

            // provide valid constant value under all circumstances
            obj.val = constant.MakeUnknown(); 

            // determine type, if any
            if (typ != null)
            {
                var t = check.typ(typ);
                if (!isConstType(t))
                { 
                    // don't report an error if the type is an invalid C (defined) type
                    // (issue #22090)
                    if (t.Underlying() != Typ[Invalid])
                    {
                        check.errorf(typ.Pos(), "invalid constant type %s", t);
                    }
                    obj.typ = Typ[Invalid];
                    return;
                }
                obj.typ = t;
            } 

            // check initialization
            operand x = default;
            if (init != null)
            {
                check.expr(ref x, init);
            }
            check.initConst(obj, ref x);
        });

        private static void varDecl(this ref Checker _check, ref Var _obj, slice<ref Var> lhs, ast.Expr typ, ast.Expr init) => func(_check, _obj, (ref Checker check, ref Var obj, Defer _, Panic panic, Recover __) =>
        {
            assert(obj.typ == null);

            if (obj.visited)
            {
                obj.typ = Typ[Invalid];
                return;
            }
            obj.visited = true; 

            // var declarations cannot use iota
            assert(check.iota == null); 

            // determine type, if any
            if (typ != null)
            {
                obj.typ = check.typ(typ); 
                // We cannot spread the type to all lhs variables if there
                // are more than one since that would mark them as checked
                // (see Checker.objDecl) and the assignment of init exprs,
                // if any, would not be checked.
                //
                // TODO(gri) If we have no init expr, we should distribute
                // a given type otherwise we need to re-evalate the type
                // expr for each lhs variable, leading to duplicate work.
            } 

            // check initialization
            if (init == null)
            {
                if (typ == null)
                { 
                    // error reported before by arityMatch
                    obj.typ = Typ[Invalid];
                }
                return;
            }
            if (lhs == null || len(lhs) == 1L)
            {
                assert(lhs == null || lhs[0L] == obj);
                operand x = default;
                check.expr(ref x, init);
                check.initVar(obj, ref x, "variable declaration");
                return;
            }
            if (debug)
            { 
                // obj must be one of lhs
                var found = false;
                {
                    var lhs__prev1 = lhs;

                    foreach (var (_, __lhs) in lhs)
                    {
                        lhs = __lhs;
                        if (obj == lhs)
                        {
                            found = true;
                            break;
                        }
                    }

                    lhs = lhs__prev1;
                }

                if (!found)
                {
                    panic("inconsistent lhs");
                }
            } 

            // We have multiple variables on the lhs and one init expr.
            // Make sure all variables have been given the same type if
            // one was specified, otherwise they assume the type of the
            // init expression values (was issue #15755).
            if (typ != null)
            {
                {
                    var lhs__prev1 = lhs;

                    foreach (var (_, __lhs) in lhs)
                    {
                        lhs = __lhs;
                        lhs.typ = obj.typ;
                    }

                    lhs = lhs__prev1;
                }

            }
            check.initVars(lhs, new slice<ast.Expr>(new ast.Expr[] { init }), token.NoPos);
        });

        // underlying returns the underlying type of typ; possibly by following
        // forward chains of named types. Such chains only exist while named types
        // are incomplete.
        private static Type underlying(Type typ)
        {
            while (true)
            {
                ref Named (n, _) = typ._<ref Named>();
                if (n == null)
                {
                    break;
                }
                typ = n.underlying;
            }

            return typ;
        }

        private static void setUnderlying(this ref Named n, Type typ)
        {
            if (n != null)
            {
                n.underlying = typ;
            }
        }

        private static void typeDecl(this ref Checker check, ref TypeName obj, ast.Expr typ, ref Named def, slice<ref TypeName> path, bool alias)
        {
            assert(obj.typ == null); 

            // type declarations cannot use iota
            assert(check.iota == null);

            if (alias)
            {
                obj.typ = Typ[Invalid];
                obj.typ = check.typExpr(typ, null, append(path, obj));

            }
            else
            {
                Named named = ref new Named(obj:obj);
                def.setUnderlying(named);
                obj.typ = named; // make sure recursive type declarations terminate

                // determine underlying type of named
                check.typExpr(typ, named, append(path, obj)); 

                // The underlying type of named may be itself a named type that is
                // incomplete:
                //
                //    type (
                //        A B
                //        B *C
                //        C A
                //    )
                //
                // The type of C is the (named) type of A which is incomplete,
                // and which has as its underlying type the named type B.
                // Determine the (final, unnamed) underlying type by resolving
                // any forward chain (they always end in an unnamed type).
                named.underlying = underlying(named.underlying);

            } 

            // check and add associated methods
            // TODO(gri) It's easy to create pathological cases where the
            // current approach is incorrect: In general we need to know
            // and add all methods _before_ type-checking the type.
            // See https://play.golang.org/p/WMpE0q2wK8
            check.addMethodDecls(obj);
        }

        private static void addMethodDecls(this ref Checker check, ref TypeName obj)
        { 
            // get associated methods
            var methods = check.methods[obj.name];
            if (len(methods) == 0L)
            {
                return; // no methods
            }
            delete(check.methods, obj.name); 

            // use an objset to check for name conflicts
            objset mset = default; 

            // spec: "If the base type is a struct type, the non-blank method
            // and field names must be distinct."
            ref Named (base, _) = obj.typ._<ref Named>(); // nil if receiver base type is type alias
            if (base != null)
            {
                {
                    ref Struct (t, _) = @base.underlying._<ref Struct>();

                    if (t != null)
                    {
                        foreach (var (_, fld) in t.fields)
                        {
                            if (fld.name != "_")
                            {
                                assert(mset.insert(fld) == null);
                            }
                        }
                    } 

                    // Checker.Files may be called multiple times; additional package files
                    // may add methods to already type-checked types. Add pre-existing methods
                    // so that we can detect redeclarations.

                } 

                // Checker.Files may be called multiple times; additional package files
                // may add methods to already type-checked types. Add pre-existing methods
                // so that we can detect redeclarations.
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in @base.methods)
                    {
                        m = __m;
                        assert(m.name != "_");
                        assert(mset.insert(m) == null);
                    }

                    m = m__prev1;
                }

            } 

            // type-check methods
            {
                var m__prev1 = m;

                foreach (var (_, __m) in methods)
                {
                    m = __m; 
                    // spec: "For a base type, the non-blank names of methods bound
                    // to it must be unique."
                    if (m.name != "_")
                    {
                        {
                            var alt = mset.insert(m);

                            if (alt != null)
                            {
                                switch (alt.type())
                                {
                                    case ref Var _:
                                        check.errorf(m.pos, "field and method with the same name %s", m.name);
                                        break;
                                    case ref Func _:
                                        check.errorf(m.pos, "method %s already declared for %s", m.name, obj);
                                        break;
                                    default:
                                    {
                                        unreachable();
                                        break;
                                    }
                                }
                                check.reportAltDecl(alt);
                                continue;
                            }

                        }
                    } 

                    // type-check
                    check.objDecl(m, null, null); 

                    // methods with blank _ names cannot be found - don't keep them
                    if (base != null && m.name != "_")
                    {
                        @base.methods = append(@base.methods, m);
                    }
                }

                m = m__prev1;
            }

        }

        private static void funcDecl(this ref Checker check, ref Func obj, ref declInfo decl)
        {
            assert(obj.typ == null); 

            // func declarations cannot use iota
            assert(check.iota == null);

            ptr<object> sig = @new<Signature>();
            obj.typ = sig; // guard against cycles
            var fdecl = decl.fdecl;
            check.funcType(sig, fdecl.Recv, fdecl.Type);
            if (sig.recv == null && obj.name == "init" && (sig.@params.Len() > 0L || sig.results.Len() > 0L))
            {
                check.errorf(fdecl.Pos(), "func init must have no arguments and no return values"); 
                // ok to continue
            } 

            // function body must be type-checked after global declarations
            // (functions implemented elsewhere have no body)
            if (!check.conf.IgnoreFuncBodies && fdecl.Body != null)
            {
                check.later(obj.name, decl, sig, fdecl.Body);
            }
        }

        private static void declStmt(this ref Checker check, ast.Decl decl)
        {
            var pkg = check.pkg;

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
                            case ref ast.ValueSpec s:

                                if (d.Tok == token.CONST) 
                                    // determine which init exprs to use

                                    if (s.Type != null || len(s.Values) > 0L) 
                                        last = s;
                                    else if (last == null) 
                                        last = @new<ast.ValueSpec>(); // make sure last exists
                                    // declare all constants
                                    var lhs = make_slice<ref Const>(len(s.Names));
                                    {
                                        var i__prev2 = i;
                                        var name__prev2 = name;

                                        foreach (var (__i, __name) in s.Names)
                                        {
                                            i = __i;
                                            name = __name;
                                            var obj = NewConst(name.Pos(), pkg, name.Name, null, constant.MakeInt64(int64(iota)));
                                            lhs[i] = obj;

                                            ast.Expr init = default;
                                            if (i < len(last.Values))
                                            {
                                                init = last.Values[i];
                                            }
                                            check.constDecl(obj, last.Type, init);
                                        }

                                        i = i__prev2;
                                        name = name__prev2;
                                    }

                                    check.arityMatch(s, last); 

                                    // spec: "The scope of a constant or variable identifier declared
                                    // inside a function begins at the end of the ConstSpec or VarSpec
                                    // (ShortVarDecl for short variable declarations) and ends at the
                                    // end of the innermost containing block."
                                    var scopePos = s.End();
                                    {
                                        var i__prev2 = i;
                                        var name__prev2 = name;

                                        foreach (var (__i, __name) in s.Names)
                                        {
                                            i = __i;
                                            name = __name;
                                            check.declare(check.scope, name, lhs[i], scopePos);
                                        }

                                        i = i__prev2;
                                        name = name__prev2;
                                    }
                                else if (d.Tok == token.VAR) 
                                    var lhs0 = make_slice<ref Var>(len(s.Names));
                                    {
                                        var i__prev2 = i;
                                        var name__prev2 = name;

                                        foreach (var (__i, __name) in s.Names)
                                        {
                                            i = __i;
                                            name = __name;
                                            lhs0[i] = NewVar(name.Pos(), pkg, name.Name, null);
                                        } 

                                        // initialize all variables

                                        i = i__prev2;
                                        name = name__prev2;
                                    }

                                    {
                                        var i__prev2 = i;
                                        var obj__prev2 = obj;

                                        foreach (var (__i, __obj) in lhs0)
                                        {
                                            i = __i;
                                            obj = __obj;
                                            lhs = default;
                                            init = default;

                                            if (len(s.Values) == len(s.Names)) 
                                                // lhs and rhs match
                                                init = s.Values[i];
                                            else if (len(s.Values) == 1L) 
                                                // rhs is expected to be a multi-valued expression
                                                lhs = lhs0;
                                                init = s.Values[0L];
                                            else 
                                                if (i < len(s.Values))
                                                {
                                                    init = s.Values[i];
                                                }
                                                                                        check.varDecl(obj, lhs, s.Type, init);
                                            if (len(s.Values) == 1L)
                                            { 
                                                // If we have a single lhs variable we are done either way.
                                                // If we have a single rhs expression, it must be a multi-
                                                // valued expression, in which case handling the first lhs
                                                // variable will cause all lhs variables to have a type
                                                // assigned, and we are done as well.
                                                if (debug)
                                                {
                                                    {
                                                        var obj__prev3 = obj;

                                                        foreach (var (_, __obj) in lhs0)
                                                        {
                                                            obj = __obj;
                                                            assert(obj.typ != null);
                                                        }

                                                        obj = obj__prev3;
                                                    }

                                                }
                                                break;
                                            }
                                        }

                                        i = i__prev2;
                                        obj = obj__prev2;
                                    }

                                    check.arityMatch(s, null); 

                                    // declare all variables
                                    // (only at this point are the variable scopes (parents) set)
                                    scopePos = s.End(); // see constant declarations
                                    {
                                        var i__prev2 = i;
                                        var name__prev2 = name;

                                        foreach (var (__i, __name) in s.Names)
                                        {
                                            i = __i;
                                            name = __name; 
                                            // see constant declarations
                                            check.declare(check.scope, name, lhs0[i], scopePos);
                                        }

                                        i = i__prev2;
                                        name = name__prev2;
                                    }
                                else 
                                    check.invalidAST(s.Pos(), "invalid token %s", d.Tok);
                                                                break;
                            case ref ast.TypeSpec s:
                                obj = NewTypeName(s.Name.Pos(), pkg, s.Name.Name, null); 
                                // spec: "The scope of a type identifier declared inside a function
                                // begins at the identifier in the TypeSpec and ends at the end of
                                // the innermost containing block."
                                scopePos = s.Name.Pos();
                                check.declare(check.scope, s.Name, obj, scopePos);
                                check.typeDecl(obj, s.Type, null, null, s.Assign.IsValid());
                                break;
                            default:
                            {
                                var s = spec.type();
                                check.invalidAST(s.Pos(), "const, type, or var declaration expected");
                                break;
                            }
                        }
                    }
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
}}
