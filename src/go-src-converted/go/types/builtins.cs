// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of builtin function calls.

// package types -- go2cs converted at 2020 October 09 05:19:16 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\builtins.go
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
        // builtin type-checks a call to the built-in specified by id and
        // reports whether the call is valid, with *x holding the result;
        // but x.expr is not set. If the call is invalid, the result is
        // false, and *x is undefined.
        //
        private static bool builtin(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.CallExpr> _addr_call, builtinId id) => func((defer, _, __) =>
        {
            bool _ = default;
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.CallExpr call = ref _addr_call.val;
 
            // append is the only built-in that permits the use of ... for the last argument
            var bin = predeclaredFuncs[id];
            if (call.Ellipsis.IsValid() && id != _Append)
            {
                check.invalidOp(call.Ellipsis, "invalid use of ... with built-in %s", bin.name);
                check.use(call.Args);
                return ;
            }
            if (id == _Len || id == _Cap)
            {
                defer(b =>
                {
                    check.hasCallOrRecv = b;
                }(check.hasCallOrRecv));
                check.hasCallOrRecv = false;

            }
            getter arg = default;
            var nargs = len(call.Args);

            if (id == _Make || id == _New || id == _Offsetof || id == _Trace)             else 
                // make argument getter
                arg, nargs, _ = unpack((x, i) =>
                {
                    check.multiExpr(x, call.Args[i]);
                }, nargs, false);
                if (arg == null)
                {
                    return ;
                }
                if (nargs > 0L)
                {
                    arg(x, 0L);
                    if (x.mode == invalid)
                    {
                        return ;
                    }
                }
            // check argument count
            {
                @string msg = "";
                if (nargs < bin.nargs)
                {
                    msg = "not enough";
                }
                else if (!bin.variadic && nargs > bin.nargs)
                {
                    msg = "too many";
                }
                if (msg != "")
                {
                    check.invalidOp(call.Rparen, "%s arguments for %s (expected %d, found %d)", msg, call, bin.nargs, nargs);
                    return ;
                }
            }
            if (id == _Append) 
                // append(s S, x ...T) S, where T is the element type of S
                // spec: "The variadic function append appends zero or more values x to s of type
                // S, which must be a slice type, and returns the resulting slice, also of type S.
                // The values x are passed to a parameter of type ...T where T is the element type
                // of S and the respective parameter passing rules apply."
                var S = x.typ;
                Type T = default;
                {
                    ptr<Slice> (s, _) = S.Underlying()._<ptr<Slice>>();

                    if (s != null)
                    {
                        T = s.elem;
                    }
                    else
                    {
                        check.invalidArg(x.pos(), "%s is not a slice", x);
                        return ;
                    }
                } 

                // remember arguments that have been evaluated already
                operand alist = new slice<operand>(new operand[] { *x }); 

                // spec: "As a special case, append also accepts a first argument assignable
                // to type []byte with a second argument of string type followed by ... .
                // This form appends the bytes of the string.
                if (nargs == 2L && call.Ellipsis.IsValid() && x.assignableTo(check, NewSlice(universeByte), null))
                {
                    arg(x, 1L);
                    if (x.mode == invalid)
                    {
                        return ;
                    }
                    if (isString(x.typ))
                    {
                        if (check.Types != null)
                        {
                            var sig = makeSig(S, S, x.typ);
                            sig.variadic = true;
                            check.recordBuiltinType(call.Fun, sig);
                        }
                        x.mode = value;
                        x.typ = S;
                        break;

                    }
                    alist = append(alist, x); 
                    // fallthrough
                }
                sig = makeSig(S, S, NewSlice(T)); // []T required for variadic signature
                sig.variadic = true;
                check.arguments(x, call, sig, (x, i) =>
                { 
                    // only evaluate arguments that have not been evaluated before
                    if (i < len(alist))
                    {
                        x = alist[i];
                        return ;
                    }
                    arg(x, i);

                }, nargs); 
                // ok to continue even if check.arguments reported errors

                x.mode = value;
                x.typ = S;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, sig);
                }
            else if (id == _Cap || id == _Len) 
                // cap(x)
                // len(x)
                var mode = invalid;
                Type typ = default;
                constant.Value val = default;
                    typ = implicitArrayDeref(x.typ.Underlying());

                switch (typ.type())
                {
                    case ptr<Basic> t:
                        if (isString(t) && id == _Len)
                        {
                            if (x.mode == constant_)
                            {
                                mode = constant_;
                                val = constant.MakeInt64(int64(len(constant.StringVal(x.val))));
                            }
                            else
                            {
                                mode = value;
                            }
                        }
                        break;
                    case ptr<Array> t:
                        mode = value; 
                        // spec: "The expressions len(s) and cap(s) are constants
                        // if the type of s is an array or pointer to an array and
                        // the expression s does not contain channel receives or
                        // function calls; in this case s is not evaluated."
                        if (!check.hasCallOrRecv)
                        {
                            mode = constant_;
                            if (t.len >= 0L)
                            {
                                val = constant.MakeInt64(t.len);
                            }
                            else
                            {
                                val = constant.MakeUnknown();
                            }
                        }
                        break;
                    case ptr<Slice> t:
                        mode = value;
                        break;
                    case ptr<Chan> t:
                        mode = value;
                        break;
                    case ptr<Map> t:
                        if (id == _Len)
                        {
                            mode = value;
                        }
                        break;

                }

                if (mode == invalid && typ != Typ[Invalid])
                {
                    check.invalidArg(x.pos(), "%s for %s", x, bin.name);
                    return ;
                }
                x.mode = mode;
                x.typ = Typ[Int];
                x.val = val;
                if (check.Types != null && mode != constant_)
                {
                    check.recordBuiltinType(call.Fun, makeSig(x.typ, typ));
                }
            else if (id == _Close) 
                // close(c)
                ptr<Chan> (c, _) = x.typ.Underlying()._<ptr<Chan>>();
                if (c == null)
                {
                    check.invalidArg(x.pos(), "%s is not a channel", x);
                    return ;
                }
                if (c.dir == RecvOnly)
                {
                    check.invalidArg(x.pos(), "%s must not be a receive-only channel", x);
                    return ;
                }
                x.mode = novalue;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(null, c));
                }
            else if (id == _Complex) 
                // complex(x, y floatT) complexT
                ref operand y = ref heap(out ptr<operand> _addr_y);
                arg(_addr_y, 1L);
                if (y.mode == invalid)
                {
                    return ;
                }
                long d = 0L;
                if (isUntyped(x.typ))
                {
                    d |= 1L;
                }
                if (isUntyped(y.typ))
                {
                    d |= 2L;
                }
                switch (d)
                {
                    case 0L: 
                        break;
                    case 1L: 
                        // only x is untyped => convert to type of y
                        check.convertUntyped(x, y.typ);
                        break;
                    case 2L: 
                        // only y is untyped => convert to type of x
                        check.convertUntyped(_addr_y, x.typ);
                        break;
                    case 3L: 
                        // x and y are untyped =>
                        // 1) if both are constants, convert them to untyped
                        //    floating-point numbers if possible,
                        // 2) if one of them is not constant (possible because
                        //    it contains a shift that is yet untyped), convert
                        //    both of them to float64 since they must have the
                        //    same type to succeed (this will result in an error
                        //    because shifts of floats are not permitted)
                                            if (x.mode == constant_ && y.mode == constant_)
                                            {
                                                Action<ptr<operand>> toFloat = x =>
                                                {
                                                    if (isNumeric(x.typ) && constant.Sign(constant.Imag(x.val)) == 0L)
                                                    {
                                                        x.typ = Typ[UntypedFloat];
                                                    }
                                                }
                                            else
                        ;
                                                toFloat(x);
                                                toFloat(_addr_y);

                                            }                    {
                                                check.convertUntyped(x, Typ[Float64]);
                                                check.convertUntyped(_addr_y, Typ[Float64]); 
                                                // x and y should be invalid now, but be conservative
                                                // and check below
                                            }
                        break;
                }
                if (x.mode == invalid || y.mode == invalid)
                {
                    return ;
                }
                if (!check.identical(x.typ, y.typ))
                {
                    check.invalidArg(x.pos(), "mismatched types %s and %s", x.typ, y.typ);
                    return ;
                }
                if (!isFloat(x.typ))
                {
                    check.invalidArg(x.pos(), "arguments have type %s, expected floating-point", x.typ);
                    return ;
                }
                if (x.mode == constant_ && y.mode == constant_)
                {
                    x.val = constant.BinaryOp(constant.ToFloat(x.val), token.ADD, constant.MakeImag(constant.ToFloat(y.val)));
                }
                else
                {
                    x.mode = value;
                }
                BasicKind res = default;

                if (x.typ.Underlying()._<ptr<Basic>>().kind == Float32) 
                    res = Complex64;
                else if (x.typ.Underlying()._<ptr<Basic>>().kind == Float64) 
                    res = Complex128;
                else if (x.typ.Underlying()._<ptr<Basic>>().kind == UntypedFloat) 
                    res = UntypedComplex;
                else 
                    unreachable();
                                var resTyp = Typ[res];

                if (check.Types != null && x.mode != constant_)
                {
                    check.recordBuiltinType(call.Fun, makeSig(resTyp, x.typ, x.typ));
                }
                x.typ = resTyp;
            else if (id == _Copy) 
                // copy(x, y []T) int
                Type dst = default;
                {
                    var t__prev1 = t;

                    ptr<Slice> (t, _) = x.typ.Underlying()._<ptr<Slice>>();

                    if (t != null)
                    {
                        dst = t.elem;
                    }
                    t = t__prev1;

                }


                y = default;
                arg(_addr_y, 1L);
                if (y.mode == invalid)
                {
                    return ;
                }
                Type src = default;
                switch (y.typ.Underlying().type())
                {
                    case ptr<Basic> t:
                        if (isString(y.typ))
                        {
                            src = universeByte;
                        }
                        break;
                    case ptr<Slice> t:
                        src = t.elem;
                        break;

                }

                if (dst == null || src == null)
                {
                    check.invalidArg(x.pos(), "copy expects slice arguments; found %s and %s", x, _addr_y);
                    return ;
                }
                if (!check.identical(dst, src))
                {
                    check.invalidArg(x.pos(), "arguments to copy %s and %s have different element types %s and %s", x, _addr_y, dst, src);
                    return ;
                }
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(Typ[Int], x.typ, y.typ));
                }
                x.mode = value;
                x.typ = Typ[Int];
            else if (id == _Delete) 
                // delete(m, k)
                ptr<Map> (m, _) = x.typ.Underlying()._<ptr<Map>>();
                if (m == null)
                {
                    check.invalidArg(x.pos(), "%s is not a map", x);
                    return ;
                }
                arg(x, 1L); // k
                if (x.mode == invalid)
                {
                    return ;
                }
                if (!x.assignableTo(check, m.key, null))
                {
                    check.invalidArg(x.pos(), "%s is not assignable to %s", x, m.key);
                    return ;
                }
                x.mode = novalue;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(null, m, m.key));
                }
            else if (id == _Imag || id == _Real) 
                // imag(complexT) floatT
                // real(complexT) floatT

                // convert or check untyped argument
                if (isUntyped(x.typ))
                {
                    if (x.mode == constant_)
                    { 
                        // an untyped constant number can always be considered
                        // as a complex constant
                        if (isNumeric(x.typ))
                        {
                            x.typ = Typ[UntypedComplex];
                        }
                    }
                    else
                    { 
                        // an untyped non-constant argument may appear if
                        // it contains a (yet untyped non-constant) shift
                        // expression: convert it to complex128 which will
                        // result in an error (shift of complex value)
                        check.convertUntyped(x, Typ[Complex128]); 
                        // x should be invalid now, but be conservative and check
                        if (x.mode == invalid)
                        {
                            return ;
                        }
                    }
                }
                if (!isComplex(x.typ))
                {
                    check.invalidArg(x.pos(), "argument has type %s, expected complex type", x.typ);
                    return ;
                }
                if (x.mode == constant_)
                {
                    if (id == _Real)
                    {
                        x.val = constant.Real(x.val);
                    }
                    else
                    {
                        x.val = constant.Imag(x.val);
                    }
                }
                else
                {
                    x.mode = value;
                }
                res = default;

                if (x.typ.Underlying()._<ptr<Basic>>().kind == Complex64) 
                    res = Float32;
                else if (x.typ.Underlying()._<ptr<Basic>>().kind == Complex128) 
                    res = Float64;
                else if (x.typ.Underlying()._<ptr<Basic>>().kind == UntypedComplex) 
                    res = UntypedFloat;
                else 
                    unreachable();
                                resTyp = Typ[res];

                if (check.Types != null && x.mode != constant_)
                {
                    check.recordBuiltinType(call.Fun, makeSig(resTyp, x.typ));
                }
                x.typ = resTyp;
            else if (id == _Make) 
                // make(T, n)
                // make(T, n, m)
                // (no argument evaluated yet)
                var arg0 = call.Args[0L];
                T = check.typ(arg0);
                if (T == Typ[Invalid])
                {
                    return ;
                }
                long min = default; // minimum number of arguments
                switch (T.Underlying().type())
                {
                    case ptr<Slice> _:
                        min = 2L;
                        break;
                    case ptr<Map> _:
                        min = 1L;
                        break;
                    case ptr<Chan> _:
                        min = 1L;
                        break;
                    default:
                    {
                        check.invalidArg(arg0.Pos(), "cannot make %s; type must be slice, map, or channel", arg0);
                        return ;
                        break;
                    }
                }
                if (nargs < min || min + 1L < nargs)
                {
                    check.errorf(call.Pos(), "%v expects %d or %d arguments; found %d", call, min, min + 1L, nargs);
                    return ;
                }
                Type types = new slice<Type>(new Type[] { T });
                slice<long> sizes = default; // constant integer arguments, if any
                {
                    getter arg__prev1 = arg;

                    foreach (var (_, __arg) in call.Args[1L..])
                    {
                        arg = __arg;
                        var (typ, size) = check.index(arg, -1L); // ok to continue with typ == Typ[Invalid]
                        types = append(types, typ);
                        if (size >= 0L)
                        {
                            sizes = append(sizes, size);
                        }
                    }
                    arg = arg__prev1;
                }

                if (len(sizes) == 2L && sizes[0L] > sizes[1L])
                {
                    check.invalidArg(call.Args[1L].Pos(), "length and capacity swapped"); 
                    // safe to continue
                }
                x.mode = value;
                x.typ = T;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(x.typ, types));
                }
            else if (id == _New) 
                // new(T)
                // (no argument evaluated yet)
                T = check.typ(call.Args[0L]);
                if (T == Typ[Invalid])
                {
                    return ;
                }
                x.mode = value;
                x.typ = addr(new Pointer(base:T));
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(x.typ, T));
                }
            else if (id == _Panic) 
                // panic(x)
                // record panic call if inside a function with result parameters
                // (for use in Checker.isTerminating)
                if (check.sig != null && check.sig.results.Len() > 0L)
                { 
                    // function has result parameters
                    var p = check.isPanic;
                    if (p == null)
                    { 
                        // allocate lazily
                        p = make_map<ptr<ast.CallExpr>, bool>();
                        check.isPanic = p;

                    }
                    p[call] = true;

                }
                check.assignment(x, _addr_emptyInterface, "argument to panic");
                if (x.mode == invalid)
                {
                    return ;
                }
                x.mode = novalue;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(null, _addr_emptyInterface));
                }
            else if (id == _Print || id == _Println) 
                // print(x, y, ...)
                // println(x, y, ...)
                slice<Type> @params = default;
                if (nargs > 0L)
                {
                    params = make_slice<Type>(nargs);
                    for (long i = 0L; i < nargs; i++)
                    {
                        if (i > 0L)
                        {
                            arg(x, i); // first argument already evaluated
                        }
                        check.assignment(x, null, "argument to " + predeclaredFuncs[id].name);
                        if (x.mode == invalid)
                        { 
                            // TODO(gri) "use" all arguments?
                            return ;

                        }
                        params[i] = x.typ;

                    }

                }
                x.mode = novalue;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(null, params));
                }
            else if (id == _Recover) 
                // recover() interface{}
                x.mode = value;
                x.typ = _addr_emptyInterface;
                if (check.Types != null)
                {
                    check.recordBuiltinType(call.Fun, makeSig(x.typ));
                }
            else if (id == _Alignof) 
                // unsafe.Alignof(x T) uintptr
                check.assignment(x, null, "argument to unsafe.Alignof");
                if (x.mode == invalid)
                {
                    return ;
                }
                x.mode = constant_;
                x.val = constant.MakeInt64(check.conf.alignof(x.typ));
                x.typ = Typ[Uintptr]; 
                // result is constant - no need to record signature
            else if (id == _Offsetof) 
                // unsafe.Offsetof(x T) uintptr, where x must be a selector
                // (no argument evaluated yet)
                arg0 = call.Args[0L];
                ptr<ast.SelectorExpr> (selx, _) = unparen(arg0)._<ptr<ast.SelectorExpr>>();
                if (selx == null)
                {
                    check.invalidArg(arg0.Pos(), "%s is not a selector expression", arg0);
                    check.use(arg0);
                    return ;
                }
                check.expr(x, selx.X);
                if (x.mode == invalid)
                {
                    return ;
                }
                var @base = derefStructPtr(x.typ);
                var sel = selx.Sel.Name;
                var (obj, index, indirect) = check.lookupFieldOrMethod(base, false, check.pkg, sel);
                switch (obj.type())
                {
                    case 
                        check.invalidArg(x.pos(), "%s has no single field %s", base, sel);
                        return ;
                        break;
                    case ptr<Func> _:
                        check.invalidArg(arg0.Pos(), "%s is a method value", arg0);
                        return ;
                        break;
                }
                if (indirect)
                {
                    check.invalidArg(x.pos(), "field %s is embedded via a pointer in %s", sel, base);
                    return ;
                }
                check.recordSelection(selx, FieldVal, base, obj, index, false);

                var offs = check.conf.offsetof(base, index);
                x.mode = constant_;
                x.val = constant.MakeInt64(offs);
                x.typ = Typ[Uintptr]; 
                // result is constant - no need to record signature
            else if (id == _Sizeof) 
                // unsafe.Sizeof(x T) uintptr
                check.assignment(x, null, "argument to unsafe.Sizeof");
                if (x.mode == invalid)
                {
                    return ;
                }
                x.mode = constant_;
                x.val = constant.MakeInt64(check.conf.@sizeof(x.typ));
                x.typ = Typ[Uintptr]; 
                // result is constant - no need to record signature
            else if (id == _Assert) 
                // assert(pred) causes a typechecker error if pred is false.
                // The result of assert is the value of pred if there is no error.
                // Note: assert is only available in self-test mode.
                if (x.mode != constant_ || !isBoolean(x.typ))
                {
                    check.invalidArg(x.pos(), "%s is not a boolean constant", x);
                    return ;
                }
                if (x.val.Kind() != constant.Bool)
                {
                    check.errorf(x.pos(), "internal error: value of %s should be a boolean constant", x);
                    return ;
                }
                if (!constant.BoolVal(x.val))
                {
                    check.errorf(call.Pos(), "%v failed", call); 
                    // compile-time assertion failure - safe to continue
                }
            else if (id == _Trace) 
                // trace(x, y, z, ...) dumps the positions, expressions, and
                // values of its arguments. The result of trace is the value
                // of the first argument.
                // Note: trace is only available in self-test mode.
                // (no argument evaluated yet)
                if (nargs == 0L)
                {
                    check.dump("%v: trace() without arguments", call.Pos());
                    x.mode = novalue;
                    break;
                }
                ref operand t = ref heap(out ptr<operand> _addr_t);
                var x1 = x;
                {
                    getter arg__prev1 = arg;

                    foreach (var (_, __arg) in call.Args)
                    {
                        arg = __arg;
                        check.rawExpr(x1, arg, null); // permit trace for types, e.g.: new(trace(T))
                        check.dump("%v: %s", x1.pos(), x1);
                        _addr_x1 = _addr_t;
                        x1 = ref _addr_x1.val; // use incoming x only for first argument
                    }
                    arg = arg__prev1;
                }
            else 
                unreachable();
                        return true;

        });

        // makeSig makes a signature for the given argument and result types.
        // Default types are used for untyped arguments, and res may be nil.
        private static ptr<Signature> makeSig(Type res, params Type[] args)
        {
            args = args.Clone();

            var list = make_slice<ptr<Var>>(len(args));
            foreach (var (i, param) in args)
            {
                list[i] = NewVar(token.NoPos, null, "", Default(param));
            }
            var @params = NewTuple(list);
            ptr<Tuple> result;
            if (res != null)
            {
                assert(!isUntyped(res));
                result = NewTuple(NewVar(token.NoPos, null, "", res));
            }

            return addr(new Signature(params:params,results:result));

        }

        // implicitArrayDeref returns A if typ is of the form *A and A is an array;
        // otherwise it returns typ.
        //
        private static Type implicitArrayDeref(Type typ)
        {
            {
                ptr<Pointer> (p, ok) = typ._<ptr<Pointer>>();

                if (ok)
                {
                    {
                        ptr<Array> (a, ok) = p.@base.Underlying()._<ptr<Array>>();

                        if (ok)
                        {
                            return a;
                        }

                    }

                }

            }

            return typ;

        }

        // unparen returns e with any enclosing parentheses stripped.
        private static ast.Expr unparen(ast.Expr e)
        {
            while (true)
            {
                ptr<ast.ParenExpr> (p, ok) = e._<ptr<ast.ParenExpr>>();
                if (!ok)
                {
                    return e;
                }

                e = p.X;

            }


        }
    }
}}
