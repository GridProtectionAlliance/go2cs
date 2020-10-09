// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 09 05:19:21 UTC
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
        private static void reportAltDecl(this ptr<Checker> _addr_check, Object obj)
        {
            ref Checker check = ref _addr_check.val;

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

        private static void declare(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope, ptr<ast.Ident> _addr_id, Object obj, token.Pos pos)
        {
            ref Checker check = ref _addr_check.val;
            ref Scope scope = ref _addr_scope.val;
            ref ast.Ident id = ref _addr_id.val;
 
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
                        return ;
                    }

                }

                obj.setScopePos(pos);

            }

            if (id != null)
            {
                check.recordDef(id, obj);
            }

        }

        // pathString returns a string of the form a->b-> ... ->g for a path [a, b, ... g].
        private static @string pathString(slice<Object> path)
        {
            @string s = default;
            foreach (var (i, p) in path)
            {
                if (i > 0L)
                {
                    s += "->";
                }

                s += p.Name();

            }
            return s;

        }

        // objDecl type-checks the declaration of obj in its respective (file) context.
        // For the meaning of def, see Checker.definedType, in typexpr.go.
        private static void objDecl(this ptr<Checker> _addr_check, Object obj, ptr<Named> _addr_def) => func((defer, _, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref Named def = ref _addr_def.val;

            if (trace)
            {
                check.trace(obj.Pos(), "-- checking %s (%s, objPath = %s)", obj, obj.color(), pathString(check.objPath));
                check.indent++;
                defer(() =>
                {
                    check.indent--;
                    check.trace(obj.Pos(), "=> %s (%s)", obj, obj.color());
                }());

            } 

            // Checking the declaration of obj means inferring its type
            // (and possibly its value, for constants).
            // An object's type (and thus the object) may be in one of
            // three states which are expressed by colors:
            //
            // - an object whose type is not yet known is painted white (initial color)
            // - an object whose type is in the process of being inferred is painted grey
            // - an object whose type is fully inferred is painted black
            //
            // During type inference, an object's color changes from white to grey
            // to black (pre-declared objects are painted black from the start).
            // A black object (i.e., its type) can only depend on (refer to) other black
            // ones. White and grey objects may depend on white and black objects.
            // A dependency on a grey object indicates a cycle which may or may not be
            // valid.
            //
            // When objects turn grey, they are pushed on the object path (a stack);
            // they are popped again when they turn black. Thus, if a grey object (a
            // cycle) is encountered, it is on the object path, and all the objects
            // it depends on are the remaining objects on that path. Color encoding
            // is such that the color value of a grey object indicates the index of
            // that object in the object path.

            // During type-checking, white objects may be assigned a type without
            // traversing through objDecl; e.g., when initializing constants and
            // variables. Update the colors of those objects here (rather than
            // everywhere where we set the type) to satisfy the color invariants.
            if (obj.color() == white && obj.Type() != null)
            {
                obj.setColor(black);
                return ;
            }


            if (obj.color() == white) 
                assert(obj.Type() == null); 
                // All color values other than white and black are considered grey.
                // Because black and white are < grey, all values >= grey are grey.
                // Use those values to encode the object's index into the object path.
                obj.setColor(grey + color(check.push(obj)));
                defer(() =>
                {
                    check.pop().setColor(black);
                }());
            else if (obj.color() == black) 
                assert(obj.Type() != null);
                return ;
            else if (obj.color() == grey) 
                // We have a cycle.
                // In the existing code, this is marked by a non-nil type
                // for the object except for constants and variables whose
                // type may be non-nil (known), or nil if it depends on the
                // not-yet known initialization value.
                // In the former case, set the type to Typ[Invalid] because
                // we have an initialization cycle. The cycle error will be
                // reported later, when determining initialization order.
                // TODO(gri) Report cycle here and simplify initialization
                // order code.
                switch (obj.type())
                {
                    case ptr<Const> obj:
                        if (check.cycle(obj) || obj.typ == null)
                        {
                            obj.typ = Typ[Invalid];
                        }

                        break;
                    case ptr<Var> obj:
                        if (check.cycle(obj) || obj.typ == null)
                        {
                            obj.typ = Typ[Invalid];
                        }

                        break;
                    case ptr<TypeName> obj:
                        if (check.cycle(obj))
                        { 
                            // break cycle
                            // (without this, calling underlying()
                            // below may lead to an endless loop
                            // if we have a cycle for a defined
                            // (*Named) type)
                            obj.typ = Typ[Invalid];

                        }

                        break;
                    case ptr<Func> obj:
                        if (check.cycle(obj))
                        { 
                            // Don't set obj.typ to Typ[Invalid] here
                            // because plenty of code type-asserts that
                            // functions have a *Signature type. Grey
                            // functions have their type set to an empty
                            // signature which makes it impossible to
                            // initialize a variable with the function.
                        }

                        break;
                    default:
                    {
                        var obj = obj.type();
                        unreachable();
                        break;
                    }
                }
                assert(obj.Type() != null);
                return ;
            else 
                // Color values other than white or black are considered grey.
                        var d = check.objMap[obj];
            if (d == null)
            {
                check.dump("%v: %s should have been declared", obj.Pos(), obj);
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
                case ptr<Const> obj:
                    check.decl = d; // new package-level const decl
                    check.constDecl(obj, d.typ, d.init);
                    break;
                case ptr<Var> obj:
                    check.decl = d; // new package-level var decl
                    check.varDecl(obj, d.lhs, d.typ, d.init);
                    break;
                case ptr<TypeName> obj:
                    check.typeDecl(obj, d.typ, def, d.alias);
                    break;
                case ptr<Func> obj:
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

        // cycle checks if the cycle starting with obj is valid and
        // reports an error if it is not.
        private static bool cycle(this ptr<Checker> _addr_check, Object obj) => func((defer, _, __) =>
        {
            bool isCycle = default;
            ref Checker check = ref _addr_check.val;
 
            // The object map contains the package scope objects and the non-interface methods.
            if (debug)
            {
                var info = check.objMap[obj];
                var inObjMap = info != null && (info.fdecl == null || info.fdecl.Recv == null); // exclude methods
                var isPkgObj = obj.Parent() == check.pkg.scope;
                if (isPkgObj != inObjMap)
                {
                    check.dump("%v: inconsistent object map for %s (isPkgObj = %v, inObjMap = %v)", obj.Pos(), obj, isPkgObj, inObjMap);
                    unreachable();
                }

            } 

            // Count cycle objects.
            assert(obj.color() >= grey);
            var start = obj.color() - grey; // index of obj in objPath
            var cycle = check.objPath[start..];
            long nval = 0L; // number of (constant or variable) values in the cycle
            long ndef = 0L; // number of type definitions in the cycle
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in cycle)
                {
                    obj = __obj;
                    switch (obj.type())
                    {
                        case ptr<Const> obj:
                            nval++;
                            break;
                        case ptr<Var> obj:
                            nval++;
                            break;
                        case ptr<TypeName> obj:
                            bool alias = default;
                            {
                                var d = check.objMap[obj];

                                if (d != null)
                                {
                                    alias = d.alias; // package-level object
                                }
                                else
                                {
                                    alias = obj.IsAlias(); // function local object
                                }

                            }

                            if (!alias)
                            {
                                ndef++;
                            }

                            break;
                        case ptr<Func> obj:
                            break;
                        default:
                        {
                            var obj = obj.type();
                            unreachable();
                            break;
                        }
                    }

                }

                obj = obj__prev1;
            }

            if (trace)
            {
                check.trace(obj.Pos(), "## cycle detected: objPath = %s->%s (len = %d)", pathString(cycle), obj.Name(), len(cycle));
                check.trace(obj.Pos(), "## cycle contains: %d values, %d type definitions", nval, ndef);
                defer(() =>
                {
                    if (isCycle)
                    {
                        check.trace(obj.Pos(), "=> error: cycle is invalid");
                    }

                }());

            } 

            // A cycle involving only constants and variables is invalid but we
            // ignore them here because they are reported via the initialization
            // cycle check.
            if (nval == len(cycle))
            {
                return false;
            } 

            // A cycle involving only types (and possibly functions) must have at least
            // one type definition to be permitted: If there is no type definition, we
            // have a sequence of alias type names which will expand ad infinitum.
            if (nval == 0L && ndef > 0L)
            {
                return false; // cycle is permitted
            }

            check.cycleError(cycle);

            return true;

        });

        private partial struct typeInfo // : ulong
        {
        }

        // validType verifies that the given type does not "expand" infinitely
        // producing a cycle in the type graph. Cycles are detected by marking
        // defined types.
        // (Cycles involving alias types, as in "type A = [10]A" are detected
        // earlier, via the objDecl cycle detection mechanism.)
        private static typeInfo validType(this ptr<Checker> _addr_check, Type typ, slice<Object> path) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;

            const typeInfo unknown = (typeInfo)iota;
            const var marked = 0;
            const var valid = 1;
            const var invalid = 2;

            switch (typ.type())
            {
                case ptr<Array> t:
                    return check.validType(t.elem, path);
                    break;
                case ptr<Struct> t:
                    foreach (var (_, f) in t.fields)
                    {
                        if (check.validType(f.typ, path) == invalid)
                        {
                            return invalid;
                        }

                    }
                    break;
                case ptr<Interface> t:
                    foreach (var (_, etyp) in t.embeddeds)
                    {
                        if (check.validType(etyp, path) == invalid)
                        {
                            return invalid;
                        }

                    }
                    break;
                case ptr<Named> t:
                    if (t.obj.pkg != check.pkg)
                    {
                        return valid;
                    } 

                    // don't report a 2nd error if we already know the type is invalid
                    // (e.g., if a cycle was detected earlier, via Checker.underlying).
                    if (t.underlying == Typ[Invalid])
                    {
                        t.info = invalid;
                        return invalid;
                    }


                    if (t.info == unknown) 
                        t.info = marked;
                        t.info = check.validType(t.orig, append(path, t.obj)); // only types of current package added to path
                    else if (t.info == marked) 
                        // cycle detected
                        foreach (var (i, tn) in path)
                        {
                            if (t.obj.pkg != check.pkg)
                            {
                                panic("internal error: type cycle via package-external type");
                            }

                            if (tn == t.obj)
                            {
                                check.cycleError(path[i..]);
                                t.info = invalid;
                                t.underlying = Typ[Invalid];
                                return t.info;
                            }

                        }
                        panic("internal error: cycle start not found");
                                        return t.info;
                    break;

            }

            return valid;

        });

        // cycleError reports a declaration cycle starting with
        // the object in cycle that is "first" in the source.
        private static void cycleError(this ptr<Checker> _addr_check, slice<Object> cycle)
        {
            ref Checker check = ref _addr_check.val;
 
            // TODO(gri) Should we start with the last (rather than the first) object in the cycle
            //           since that is the earliest point in the source where we start seeing the
            //           cycle? That would be more consistent with other error messages.
            var i = firstInSrc(cycle);
            var obj = cycle[i];
            check.errorf(obj.Pos(), "illegal cycle in declaration of %s", obj.Name());
            foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in cycle)
            {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                check.errorf(obj.Pos(), "\t%s refers to", obj.Name()); // secondary error, \t indented
                i++;
                if (i >= len(cycle))
                {
                    i = 0L;
                }

                obj = cycle[i];

            }
            check.errorf(obj.Pos(), "\t%s", obj.Name());

        }

        // firstInSrc reports the index of the object with the "smallest"
        // source position in path. path must not be empty.
        private static long firstInSrc(slice<Object> path)
        {
            long fst = 0L;
            var pos = path[0L].Pos();
            foreach (var (i, t) in path[1L..])
            {
                if (t.Pos() < pos)
                {
                    fst = i + 1L;
                    pos = t.Pos();

                }

            }
            return fst;

        }

        private static void constDecl(this ptr<Checker> _addr_check, ptr<Const> _addr_obj, ast.Expr typ, ast.Expr init) => func((defer, _, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref Const obj = ref _addr_obj.val;

            assert(obj.typ == null); 

            // use the correct value of iota
            defer(iota =>
            {
                check.iota = iota;
            }(check.iota));
            check.iota = obj.val; 

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
                    return ;

                }

                obj.typ = t;

            } 

            // check initialization
            ref operand x = ref heap(out ptr<operand> _addr_x);
            if (init != null)
            {
                check.expr(_addr_x, init);
            }

            check.initConst(obj, _addr_x);

        });

        private static void varDecl(this ptr<Checker> _addr_check, ptr<Var> _addr_obj, slice<ptr<Var>> lhs, ast.Expr typ, ast.Expr init) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref Var obj = ref _addr_obj.val;

            assert(obj.typ == null); 

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

                return ;

            }

            if (lhs == null || len(lhs) == 1L)
            {
                assert(lhs == null || lhs[0L] == obj);
                ref operand x = ref heap(out ptr<operand> _addr_x);
                check.expr(_addr_x, init);
                check.initVar(obj, _addr_x, "variable declaration");
                return ;
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
        // are incomplete. If an underlying type is found, resolve the chain by
        // setting the underlying type for each defined type in the chain before
        // returning it.
        //
        // If no underlying type is found, a cycle error is reported and Typ[Invalid]
        // is used as underlying type for each defined type in the chain and returned
        // as result.
        private static Type underlying(this ptr<Checker> _addr_check, Type typ) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
 
            // If typ is not a defined type, its underlying type is itself.
            ptr<Named> (n0, _) = typ._<ptr<Named>>();
            if (n0 == null)
            {
                return typ; // nothing to do
            } 

            // If the underlying type of a defined type is not a defined
            // type, then that is the desired underlying type.
            typ = n0.underlying;
            ptr<Named> (n, _) = typ._<ptr<Named>>();
            if (n == null)
            {
                return typ; // common case
            } 

            // Otherwise, follow the forward chain.
            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Named>, long>{n0:0};
            Object path = new slice<Object>(new Object[] { n0.obj });
            while (true)
            {
                typ = n.underlying;
                ptr<Named> (n1, _) = typ._<ptr<Named>>();
                if (n1 == null)
                {
                    break; // end of chain
                }

                seen[n] = len(seen);
                path = append(path, n.obj);
                n = n1;

                {
                    var (i, ok) = seen[n];

                    if (ok)
                    { 
                        // cycle
                        check.cycleError(path[i..]);
                        typ = Typ[Invalid];
                        break;

                    }

                }

            }


            {
                ptr<Named> n__prev1 = n;

                foreach (var (__n) in seen)
                {
                    n = __n; 
                    // We should never have to update the underlying type of an imported type;
                    // those underlying types should have been resolved during the import.
                    // Also, doing so would lead to a race condition (was issue #31749).
                    if (n.obj.pkg != check.pkg)
                    {
                        panic("internal error: imported type with unresolved underlying type");
                    }

                    n.underlying = typ;

                }

                n = n__prev1;
            }

            return typ;

        });

        private static void setUnderlying(this ptr<Named> _addr_n, Type typ)
        {
            ref Named n = ref _addr_n.val;

            if (n != null)
            {
                n.underlying = typ;
            }

        }

        private static void typeDecl(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj, ast.Expr typ, ptr<Named> _addr_def, bool alias)
        {
            ref Checker check = ref _addr_check.val;
            ref TypeName obj = ref _addr_obj.val;
            ref Named def = ref _addr_def.val;

            assert(obj.typ == null);

            check.later(() =>
            {
                check.validType(obj.typ, null);
            });

            if (alias)
            {
                obj.typ = Typ[Invalid];
                obj.typ = check.typ(typ);
            }
            else
            {
                ptr<Named> named = addr(new Named(obj:obj));
                def.setUnderlying(named);
                obj.typ = named; // make sure recursive type declarations terminate

                // determine underlying type of named
                named.orig = check.definedType(typ, named); 

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
                // any forward chain.
                named.underlying = check.underlying(named);


            }

            check.addMethodDecls(obj);

        }

        private static void addMethodDecls(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj)
        {
            ref Checker check = ref _addr_check.val;
            ref TypeName obj = ref _addr_obj.val;
 
            // get associated methods
            // (Checker.collectObjects only collects methods with non-blank names;
            // Checker.resolveBaseTypeName ensures that obj is not an alias name
            // if it has attached methods.)
            var methods = check.methods[obj];
            if (methods == null)
            {
                return ;
            }

            delete(check.methods, obj);
            assert(!check.objMap[obj].alias); // don't use TypeName.IsAlias (requires fully set up object)

            // use an objset to check for name conflicts
            objset mset = default; 

            // spec: "If the base type is a struct type, the non-blank method
            // and field names must be distinct."
            ptr<Named> (base, _) = obj.typ._<ptr<Named>>(); // shouldn't fail but be conservative
            if (base != null)
            {
                {
                    ptr<Struct> (t, _) = @base.underlying._<ptr<Struct>>();

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

            // add valid methods
            {
                var m__prev1 = m;

                foreach (var (_, __m) in methods)
                {
                    m = __m; 
                    // spec: "For a base type, the non-blank names of methods bound
                    // to it must be unique."
                    assert(m.name != "_");
                    {
                        var alt = mset.insert(m);

                        if (alt != null)
                        {
                            switch (alt.type())
                            {
                                case ptr<Var> _:
                                    check.errorf(m.pos, "field and method with the same name %s", m.name);
                                    break;
                                case ptr<Func> _:
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


                    if (base != null)
                    {
                        @base.methods = append(@base.methods, m);
                    }

                }

                m = m__prev1;
            }
        }

        private static void funcDecl(this ptr<Checker> _addr_check, ptr<Func> _addr_obj, ptr<declInfo> _addr_decl)
        {
            ref Checker check = ref _addr_check.val;
            ref Func obj = ref _addr_obj.val;
            ref declInfo decl = ref _addr_decl.val;

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
                check.later(() =>
                {
                    check.funcBody(decl, obj.name, sig, fdecl.Body, null);
                });

            }

        }

        private static void declStmt(this ptr<Checker> _addr_check, ast.Decl decl)
        {
            ref Checker check = ref _addr_check.val;

            var pkg = check.pkg;

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
                            case ptr<ast.ValueSpec> s:

                                if (d.Tok == token.CONST) 
                                    var top = len(check.delayed); 

                                    // determine which init exprs to use

                                    if (s.Type != null || len(s.Values) > 0L) 
                                        last = s;
                                    else if (last == null) 
                                        last = @new<ast.ValueSpec>(); // make sure last exists
                                    // declare all constants
                                    var lhs = make_slice<ptr<Const>>(len(s.Names));
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

                                    // process function literals in init expressions before scope changes
                                    check.processDelayed(top); 

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
                                    top = len(check.delayed);

                                    var lhs0 = make_slice<ptr<Var>>(len(s.Names));
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

                                    // process function literals in init expressions before scope changes
                                    check.processDelayed(top); 

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
                            case ptr<ast.TypeSpec> s:
                                obj = NewTypeName(s.Name.Pos(), pkg, s.Name.Name, null); 
                                // spec: "The scope of a type identifier declared inside a function
                                // begins at the identifier in the TypeSpec and ends at the end of
                                // the innermost containing block."
                                scopePos = s.Name.Pos();
                                check.declare(check.scope, s.Name, obj, scopePos); 
                                // mark and unmark type before calling typeDecl; its type is still nil (see Checker.objDecl)
                                obj.setColor(grey + color(check.push(obj)));
                                check.typeDecl(obj, s.Type, null, s.Assign.IsValid());
                                check.pop().setColor(black);
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
