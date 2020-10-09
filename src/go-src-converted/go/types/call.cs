// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of call and selector expressions.

// package types -- go2cs converted at 2020 October 09 05:19:18 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\call.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        private static exprKind call(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.CallExpr> _addr_e)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.CallExpr e = ref _addr_e.val;

            check.exprOrType(x, e.Fun);


            if (x.mode == invalid) 
                check.use(e.Args);
                x.mode = invalid;
                x.expr = e;
                return statement;
            else if (x.mode == typexpr) 
                // conversion
                var T = x.typ;
                x.mode = invalid;
                {
                    var n = len(e.Args);

                    switch (n)
                    {
                        case 0L: 
                            check.errorf(e.Rparen, "missing argument in conversion to %s", T);
                            break;
                        case 1L: 
                            check.expr(x, e.Args[0L]);
                            if (x.mode != invalid)
                            {
                                check.conversion(x, T);
                            }
                            break;
                        default: 
                            check.use(e.Args);
                            check.errorf(e.Args[n - 1L].Pos(), "too many arguments in conversion to %s", T);
                            break;
                    }
                }
                x.expr = e;
                return conversion;
            else if (x.mode == builtin) 
                var id = x.id;
                if (!check.builtin(x, e, id))
                {
                    x.mode = invalid;
                }
                x.expr = e; 
                // a non-constant result implies a function call
                if (x.mode != invalid && x.mode != constant_)
                {
                    check.hasCallOrRecv = true;
                }
                return predeclaredFuncs[id].kind;
            else 
                // function/method call
                var cgocall = x.mode == cgofunc;

                ptr<Signature> (sig, _) = x.typ.Underlying()._<ptr<Signature>>();
                if (sig == null)
                {
                    check.invalidOp(x.pos(), "cannot call non-function %s", x);
                    x.mode = invalid;
                    x.expr = e;
                    return statement;
                }
                var (arg, n, _) = unpack((x, i) =>
                {
                    check.multiExpr(x, e.Args[i]);
                }, len(e.Args), false);
                if (arg != null)
                {
                    check.arguments(x, e, sig, arg, n);
                }
                else
                {
                    x.mode = invalid;
                }
                switch (sig.results.Len())
                {
                    case 0L: 
                        x.mode = novalue;
                        break;
                    case 1L: 
                        if (cgocall)
                        {
                            x.mode = commaerr;
                        }
                        else
                        {
                            x.mode = value;
                        }
                        x.typ = sig.results.vars[0L].typ; // unpack tuple
                        break;
                    default: 
                        x.mode = value;
                        x.typ = sig.results;
                        break;
                }

                x.expr = e;
                check.hasCallOrRecv = true;

                return statement;
            
        }

        // use type-checks each argument.
        // Useful to make sure expressions are evaluated
        // (and variables are "used") in the presence of other errors.
        // The arguments may be nil.
        private static void use(this ptr<Checker> _addr_check, params ast.Expr[] arg)
        {
            arg = arg.Clone();
            ref Checker check = ref _addr_check.val;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            foreach (var (_, e) in arg)
            { 
                // The nil check below is necessary since certain AST fields
                // may legally be nil (e.g., the ast.SliceExpr.High field).
                if (e != null)
                {
                    check.rawExpr(_addr_x, e, null);
                }

            }

        }

        // useLHS is like use, but doesn't "use" top-level identifiers.
        // It should be called instead of use if the arguments are
        // expressions on the lhs of an assignment.
        // The arguments must not be nil.
        private static void useLHS(this ptr<Checker> _addr_check, params ast.Expr[] arg)
        {
            arg = arg.Clone();
            ref Checker check = ref _addr_check.val;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            foreach (var (_, e) in arg)
            { 
                // If the lhs is an identifier denoting a variable v, this assignment
                // is not a 'use' of v. Remember current value of v.used and restore
                // after evaluating the lhs via check.rawExpr.
                ptr<Var> v;
                bool v_used = default;
                {
                    ptr<ast.Ident> (ident, _) = unparen(e)._<ptr<ast.Ident>>();

                    if (ident != null)
                    { 
                        // never type-check the blank name on the lhs
                        if (ident.Name == "_")
                        {
                            continue;
                        }

                        {
                            var (_, obj) = check.scope.LookupParent(ident.Name, token.NoPos);

                            if (obj != null)
                            { 
                                // It's ok to mark non-local variables, but ignore variables
                                // from other packages to avoid potential race conditions with
                                // dot-imported variables.
                                {
                                    ptr<Var> (w, _) = obj._<ptr<Var>>();

                                    if (w != null && w.pkg == check.pkg)
                                    {
                                        v = w;
                                        v_used = v.used;
                                    }

                                }

                            }

                        }

                    }

                }

                check.rawExpr(_addr_x, e, null);
                if (v != null)
                {
                    v.used = v_used; // restore v.used
                }

            }

        }

        // useGetter is like use, but takes a getter instead of a list of expressions.
        // It should be called instead of use if a getter is present to avoid repeated
        // evaluation of the first argument (since the getter was likely obtained via
        // unpack, which may have evaluated the first argument already).
        private static void useGetter(this ptr<Checker> _addr_check, getter get, long n)
        {
            ref Checker check = ref _addr_check.val;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            for (long i = 0L; i < n; i++)
            {
                get(_addr_x, i);
            }


        }

        // A getter sets x as the i'th operand, where 0 <= i < n and n is the total
        // number of operands (context-specific, and maintained elsewhere). A getter
        // type-checks the i'th operand; the details of the actual check are getter-
        // specific.
        public delegate void getter(ptr<operand>, long);

        // unpack takes a getter get and a number of operands n. If n == 1, unpack
        // calls the incoming getter for the first operand. If that operand is
        // invalid, unpack returns (nil, 0, false). Otherwise, if that operand is a
        // function call, or a comma-ok expression and allowCommaOk is set, the result
        // is a new getter and operand count providing access to the function results,
        // or comma-ok values, respectively. The third result value reports if it
        // is indeed the comma-ok case. In all other cases, the incoming getter and
        // operand count are returned unchanged, and the third result value is false.
        //
        // In other words, if there's exactly one operand that - after type-checking
        // by calling get - stands for multiple operands, the resulting getter provides
        // access to those operands instead.
        //
        // If the returned getter is called at most once for a given operand index i
        // (including i == 0), that operand is guaranteed to cause only one call of
        // the incoming getter with that i.
        //
        private static (getter, long, bool) unpack(getter get, long n, bool allowCommaOk)
        {
            getter _p0 = default;
            long _p0 = default;
            bool _p0 = default;

            if (n != 1L)
            { 
                // zero or multiple values
                return (get, n, false);

            } 
            // possibly result of an n-valued function call or comma,ok value
            ref operand x0 = ref heap(out ptr<operand> _addr_x0);
            get(_addr_x0, 0L);
            if (x0.mode == invalid)
            {
                return (null, 0L, false);
            }

            {
                ptr<Tuple> (t, ok) = x0.typ._<ptr<Tuple>>();

                if (ok)
                { 
                    // result of an n-valued function call
                    return ((x, i) =>
                    {
                        x.mode = value;
                        x.expr = x0.expr;
                        x.typ = t.At(i).typ;
                    }, t.Len(), false);

                }

            }


            if (x0.mode == mapindex || x0.mode == commaok || x0.mode == commaerr)
            { 
                // comma-ok value
                if (allowCommaOk)
                {
                    array<Type> a = new array<Type>(new Type[] { x0.typ, Typ[UntypedBool] });
                    if (x0.mode == commaerr)
                    {
                        a[1L] = universeError;
                    }

                    return ((x, i) =>
                    {
                        x.mode = value;
                        x.expr = x0.expr;
                        x.typ = a[i];
                    }, 2L, true);

                }

                x0.mode = value;

            } 

            // single value
            return ((x, i) =>
            {
                if (i != 0L)
                {
                    unreachable();
                }

                x.val = x0;

            }, 1L, false);

        }

        // arguments checks argument passing for the call with the given signature.
        // The arg function provides the operand for the i'th argument.
        private static void arguments(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.CallExpr> _addr_call, ptr<Signature> _addr_sig, getter arg, long n)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref Signature sig = ref _addr_sig.val;

            if (call.Ellipsis.IsValid())
            { 
                // last argument is of the form x...
                if (!sig.variadic)
                {
                    check.errorf(call.Ellipsis, "cannot use ... in call to non-variadic %s", call.Fun);
                    check.useGetter(arg, n);
                    return ;
                }

                if (len(call.Args) == 1L && n > 1L)
                { 
                    // f()... is not permitted if f() is multi-valued
                    check.errorf(call.Ellipsis, "cannot use ... with %d-valued %s", n, call.Args[0L]);
                    check.useGetter(arg, n);
                    return ;

                }

            } 

            // evaluate arguments
            var context = check.sprintf("argument to %s", call.Fun);
            for (long i = 0L; i < n; i++)
            {
                arg(x, i);
                if (x.mode != invalid)
                {
                    token.Pos ellipsis = default;
                    if (i == n - 1L && call.Ellipsis.IsValid())
                    {
                        ellipsis = call.Ellipsis;
                    }

                    check.argument(sig, i, x, ellipsis, context);

                }

            } 

            // check argument count
 

            // check argument count
            if (sig.variadic)
            { 
                // a variadic function accepts an "empty"
                // last argument: count one extra
                n++;

            }

            if (n < sig.@params.Len())
            {
                check.errorf(call.Rparen, "too few arguments in call to %s", call.Fun); 
                // ok to continue
            }

        }

        // argument checks passing of argument x to the i'th parameter of the given signature.
        // If ellipsis is valid, the argument is followed by ... at that position in the call.
        private static void argument(this ptr<Checker> _addr_check, ptr<Signature> _addr_sig, long i, ptr<operand> _addr_x, token.Pos ellipsis, @string context)
        {
            ref Checker check = ref _addr_check.val;
            ref Signature sig = ref _addr_sig.val;
            ref operand x = ref _addr_x.val;

            check.singleValue(x);
            if (x.mode == invalid)
            {
                return ;
            }

            var n = sig.@params.Len(); 

            // determine parameter type
            Type typ = default;

            if (i < n) 
                typ = sig.@params.vars[i].typ;
            else if (sig.variadic) 
                typ = sig.@params.vars[n - 1L].typ;
                if (debug)
                {
                    {
                        ptr<Slice> (_, ok) = typ._<ptr<Slice>>();

                        if (!ok)
                        {
                            check.dump("%v: expected unnamed slice type, got %s", sig.@params.vars[n - 1L].Pos(), typ);
                        }

                    }

                }

            else 
                check.errorf(x.pos(), "too many arguments");
                return ;
                        if (ellipsis.IsValid())
            { 
                // argument is of the form x... and x is single-valued
                if (i != n - 1L)
                {
                    check.errorf(ellipsis, "can only use ... with matching parameter");
                    return ;
                }

                {
                    (_, ok) = x.typ.Underlying()._<ptr<Slice>>();

                    if (!ok && x.typ != Typ[UntypedNil])
                    { // see issue #18268
                        check.errorf(x.pos(), "cannot use %s as parameter of type %s", x, typ);
                        return ;

                    }

                }

            }
            else if (sig.variadic && i >= n - 1L)
            { 
                // use the variadic parameter slice's element type
                typ = typ._<ptr<Slice>>().elem;

            }

            check.assignment(x, typ, context);

        }

        private static array<@string> cgoPrefixes = new array<@string>(new @string[] { "_Ciconst_", "_Cfconst_", "_Csconst_", "_Ctype_", "_Cvar_", "_Cfpvar_fp_", "_Cfunc_", "_Cmacro_" });

        private static void selector(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.SelectorExpr> _addr_e) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.SelectorExpr e = ref _addr_e.val;
 
            // these must be declared before the "goto Error" statements
            Object obj = default;            slice<long> index = default;            bool indirect = default;

            var sel = e.Sel.Name; 
            // If the identifier refers to a package, handle everything here
            // so we don't need a "package" mode for operands: package names
            // can only appear in qualified identifiers which are mapped to
            // selector expressions.
            {
                ptr<ast.Ident> (ident, ok) = e.X._<ptr<ast.Ident>>();

                if (ok)
                {
                    obj = check.lookup(ident.Name);
                    {
                        ptr<PkgName> (pname, _) = obj._<ptr<PkgName>>();

                        if (pname != null)
                        {
                            assert(pname.pkg == check.pkg);
                            check.recordUse(ident, pname);
                            pname.used = true;
                            var pkg = pname.imported;

                            Object exp = default;
                            var funcMode = value;
                            if (pkg.cgo)
                            { 
                                // cgo special cases C.malloc: it's
                                // rewritten to _CMalloc and does not
                                // support two-result calls.
                                if (sel == "malloc")
                                {
                                    sel = "_CMalloc";
                                }
                                else
                                {
                                    funcMode = cgofunc;
                                }

                                foreach (var (_, prefix) in cgoPrefixes)
                                { 
                                    // cgo objects are part of the current package (in file
                                    // _cgo_gotypes.go). Use regular lookup.
                                    _, exp = check.scope.LookupParent(prefix + sel, check.pos);
                                    if (exp != null)
                                    {
                                        break;
                                    }

                                }
                            else
                                if (exp == null)
                                {
                                    check.errorf(e.Sel.Pos(), "%s not declared by package C", sel);
                                    goto Error;
                                }

                                check.objDecl(exp, null);

                            }                            {
                                exp = pkg.scope.Lookup(sel);
                                if (exp == null)
                                {
                                    if (!pkg.fake)
                                    {
                                        check.errorf(e.Sel.Pos(), "%s not declared by package %s", sel, pkg.name);
                                    }

                                    goto Error;

                                }

                                if (!exp.Exported())
                                {
                                    check.errorf(e.Sel.Pos(), "%s not exported by package %s", sel, pkg.name); 
                                    // ok to continue
                                }

                            }

                            check.recordUse(e.Sel, exp); 

                            // Simplified version of the code for *ast.Idents:
                            // - imported objects are always fully initialized
                            switch (exp.type())
                            {
                                case ptr<Const> exp:
                                    assert(exp.Val() != null);
                                    x.mode = constant_;
                                    x.typ = exp.typ;
                                    x.val = exp.val;
                                    break;
                                case ptr<TypeName> exp:
                                    x.mode = typexpr;
                                    x.typ = exp.typ;
                                    break;
                                case ptr<Var> exp:
                                    x.mode = variable;
                                    x.typ = exp.typ;
                                    if (pkg.cgo && strings.HasPrefix(exp.name, "_Cvar_"))
                                    {
                                        x.typ = x.typ._<ptr<Pointer>>().@base;
                                    }

                                    break;
                                case ptr<Func> exp:
                                    x.mode = funcMode;
                                    x.typ = exp.typ;
                                    if (pkg.cgo && strings.HasPrefix(exp.name, "_Cmacro_"))
                                    {
                                        x.mode = value;
                                        x.typ = x.typ._<ptr<Signature>>().results.vars[0L].typ;
                                    }

                                    break;
                                case ptr<Builtin> exp:
                                    x.mode = builtin;
                                    x.typ = exp.typ;
                                    x.id = exp.id;
                                    break;
                                default:
                                {
                                    var exp = exp.type();
                                    check.dump("unexpected object %v", exp);
                                    unreachable();
                                    break;
                                }
                            }
                            x.expr = e;
                            return ;

                        }

                    }

                }

            }


            check.exprOrType(x, e.X);
            if (x.mode == invalid)
            {
                goto Error;
            }

            obj, index, indirect = check.lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel);
            if (obj == null)
            {

                if (index != null) 
                    // TODO(gri) should provide actual type where the conflict happens
                    check.errorf(e.Sel.Pos(), "ambiguous selector %s.%s", x.expr, sel);
                else if (indirect) 
                    check.errorf(e.Sel.Pos(), "cannot call pointer method %s on %s", sel, x.typ);
                else 
                    // Check if capitalization of sel matters and provide better error
                    // message in that case.
                    if (len(sel) > 0L)
                    {
                        @string changeCase = default;
                        {
                            var r = rune(sel[0L]);

                            if (unicode.IsUpper(r))
                            {
                                changeCase = string(unicode.ToLower(r)) + sel[1L..];
                            }
                            else
                            {
                                changeCase = string(unicode.ToUpper(r)) + sel[1L..];
                            }

                        }

                        obj, _, _ = check.lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, changeCase);

                        if (obj != null)
                        {
                            check.errorf(e.Sel.Pos(), "%s.%s undefined (type %s has no field or method %s, but does have %s)", x.expr, sel, x.typ, sel, changeCase);
                            break;
                        }

                    }

                    check.errorf(e.Sel.Pos(), "%s.%s undefined (type %s has no field or method %s)", x.expr, sel, x.typ, sel);
                                goto Error;

            } 

            // methods may not have a fully set up signature yet
            {
                ptr<Func> m__prev1 = m;

                ptr<Func> (m, _) = obj._<ptr<Func>>();

                if (m != null)
                {
                    check.objDecl(m, null);
                }

                m = m__prev1;

            }


            if (x.mode == typexpr)
            { 
                // method expression
                (m, _) = obj._<ptr<Func>>();
                if (m == null)
                { 
                    // TODO(gri) should check if capitalization of sel matters and provide better error message in that case
                    check.errorf(e.Sel.Pos(), "%s.%s undefined (type %s has no method %s)", x.expr, sel, x.typ, sel);
                    goto Error;

                }

                check.recordSelection(e, MethodExpr, x.typ, m, index, indirect); 

                // the receiver type becomes the type of the first function
                // argument of the method expression's function type
                slice<ptr<Var>> @params = default;
                ptr<Signature> sig = m.typ._<ptr<Signature>>();
                if (sig.@params != null)
                {
                    params = sig.@params.vars;
                }

                x.mode = value;
                x.typ = addr(new Signature(params:NewTuple(append([]*Var{NewVar(token.NoPos,check.pkg,"",x.typ)},params...)...),results:sig.results,variadic:sig.variadic,));

                check.addDeclDep(m);


            }
            else
            { 
                // regular selector
                switch (obj.type())
                {
                    case ptr<Var> obj:
                        check.recordSelection(e, FieldVal, x.typ, obj, index, indirect);
                        if (x.mode == variable || indirect)
                        {
                            x.mode = variable;
                        }
                        else
                        {
                            x.mode = value;
                        }

                        x.typ = obj.typ;
                        break;
                    case ptr<Func> obj:
                        check.recordSelection(e, MethodVal, x.typ, obj, index, indirect);

                        if (debug)
                        { 
                            // Verify that LookupFieldOrMethod and MethodSet.Lookup agree.
                            // TODO(gri) This only works because we call LookupFieldOrMethod
                            // _before_ calling NewMethodSet: LookupFieldOrMethod completes
                            // any incomplete interfaces so they are available to NewMethodSet
                            // (which assumes that interfaces have been completed already).
                            var typ = x.typ;
                            if (x.mode == variable)
                            { 
                                // If typ is not an (unnamed) pointer or an interface,
                                // use *typ instead, because the method set of *typ
                                // includes the methods of typ.
                                // Variables are addressable, so we can always take their
                                // address.
                                {
                                    ptr<Pointer> (_, ok) = typ._<ptr<Pointer>>();

                                    if (!ok && !IsInterface(typ))
                                    {
                                        typ = addr(new Pointer(base:typ));
                                    }

                                }

                            } 
                            // If we created a synthetic pointer type above, we will throw
                            // away the method set computed here after use.
                            // TODO(gri) Method set computation should probably always compute
                            // both, the value and the pointer receiver method set and represent
                            // them in a single structure.
                            // TODO(gri) Consider also using a method set cache for the lifetime
                            // of checker once we rely on MethodSet lookup instead of individual
                            // lookup.
                            var mset = NewMethodSet(typ);
                            {
                                ptr<Func> m__prev3 = m;

                                var m = mset.Lookup(check.pkg, sel);

                                if (m == null || m.obj != obj)
                                {
                                    check.dump("%v: (%s).%v -> %s", e.Pos(), typ, obj.name, m);
                                    check.dump("%s\n", mset); 
                                    // Caution: MethodSets are supposed to be used externally
                                    // only (after all interface types were completed). It's
                                    // now possible that we get here incorrectly. Not urgent
                                    // to fix since we only run this code in debug mode.
                                    // TODO(gri) fix this eventually.
                                    panic("method sets and lookup don't agree");

                                }

                                m = m__prev3;

                            }

                        }

                        x.mode = value; 

                        // remove receiver
                        sig = obj.typ._<ptr<Signature>>().val;
                        sig.recv = null;
                        x.typ = _addr_sig;

                        check.addDeclDep(obj);
                        break;
                    default:
                    {
                        var obj = obj.type();
                        unreachable();
                        break;
                    }
                }

            } 

            // everything went well
            x.expr = e;
            return ;

Error:
            x.mode = invalid;
            x.expr = e;

        });
    }
}}
