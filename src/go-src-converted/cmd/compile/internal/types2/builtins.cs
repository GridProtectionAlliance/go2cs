// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of builtin function calls.

// package types2 -- go2cs converted at 2022 March 13 06:25:45 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\builtins.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;
using constant = go.constant_package;
using token = go.token_package;


// builtin type-checks a call to the built-in specified by id and
// reports whether the call is valid, with *x holding the result;
// but x.expr is not set. If the call is invalid, the result is
// false, and *x is undefined.
//

using System;
public static partial class types2_package {

private static bool builtin(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.CallExpr> _addr_call, builtinId id) => func((defer, _, _) => {
    bool _ = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.CallExpr call = ref _addr_call.val;
 
    // append is the only built-in that permits the use of ... for the last argument
    var bin = predeclaredFuncs[id];
    if (call.HasDots && id != _Append) { 
        //check.errorf(call.Ellipsis, invalidOp + "invalid use of ... with built-in %s", bin.name)
        check.errorf(call, invalidOp + "invalid use of ... with built-in %s", bin.name);
        check.use(call.ArgList);
        return ;
    }
    if (id == _Len || id == _Cap) {
        defer(b => {
            check.hasCallOrRecv = b;
        }(check.hasCallOrRecv));
        check.hasCallOrRecv = false;
    }
    Action<ptr<operand>, nint> arg = default; // TODO(gri) remove use of arg getter in favor of using xlist directly
    var nargs = len(call.ArgList);

    if (id == _Make || id == _New || id == _Offsetof || id == _Trace)     else 
        // make argument getter
        var (xlist, _) = check.exprList(call.ArgList, false);
        arg = (x, i) => {
            x = xlist[i].val;

            x.typ = expand(x.typ);
        };
        nargs = len(xlist); 
        // evaluate first argument, if present
        if (nargs > 0) {
            arg(x, 0);
            if (x.mode == invalid) {
                return ;
            }
        }
    // check argument count
 {
        @string msg = "";
        if (nargs < bin.nargs) {
            msg = "not enough";
        }
        else if (!bin.variadic && nargs > bin.nargs) {
            msg = "too many";
        }
        if (msg != "") {
            check.errorf(call, invalidOp + "%s arguments for %v (expected %d, found %d)", msg, call, bin.nargs, nargs);
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
            var s = asSlice(S);

            if (s != null) {
                T = s.elem;
            }
            else
 {
                check.errorf(x, invalidArg + "%s is not a slice", x);
                return ;
            }
        } 

        // remember arguments that have been evaluated already
        operand alist = new slice<operand>(new operand[] { *x }); 

        // spec: "As a special case, append also accepts a first argument assignable
        // to type []byte with a second argument of string type followed by ... .
        // This form appends the bytes of the string.
        if (nargs == 2 && call.HasDots) {
            {
                var (ok, _) = x.assignableTo(check, NewSlice(universeByte), null);

                if (ok) {
                    arg(x, 1);
                    if (x.mode == invalid) {
                        return ;
                    }
                    if (isString(x.typ)) {
                        if (check.Types != null) {
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
            }
        }
        sig = makeSig(S, S, NewSlice(T)); // []T required for variadic signature
        sig.variadic = true;
        slice<ptr<operand>> xlist = default; 
        // convert []operand to []*operand
        {
            var i__prev1 = i;

            foreach (var (__i) in alist) {
                i = __i;
                xlist = append(xlist, _addr_alist[i]);
            }
            i = i__prev1;
        }

        {
            var i__prev1 = i;

            for (var i = len(alist); i < nargs; i++) {
                ref operand x = ref heap(out ptr<operand> _addr_x);
                arg(_addr_x, i);
                xlist = append(xlist, _addr_x);
            }

            i = i__prev1;
        }
        check.arguments(call, sig, null, xlist); // discard result (we know the result type)
        // ok to continue even if check.arguments reported errors

        x.mode = value;
        x.typ = S;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, sig);
        }
    else if (id == _Cap || id == _Len) 
        // cap(x)
        // len(x)
        var mode = invalid;
        Type typ = default;
        constant.Value val = default;
            typ = implicitArrayDeref(optype(x.typ));

        switch (typ.type()) {
            case ptr<Basic> t:
                if (isString(t) && id == _Len) {
                    if (x.mode == constant_) {
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
                if (!check.hasCallOrRecv) {
                    mode = constant_;
                    if (t.len >= 0) {
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
                if (id == _Len) {
                    mode = value;
                }
                break;
            case ptr<Sum> t:
                if (t.@is(t => {
                    switch (under(t).type()) {
                        case ptr<Basic> t:
                            if (isString(t) && id == _Len) {
                                return true;
                            }
                            break;
                        case ptr<Array> t:
                            return true;
                            break;
                        case ptr<Slice> t:
                            return true;
                            break;
                        case ptr<Chan> t:
                            return true;
                            break;
                        case ptr<Map> t:
                            if (id == _Len) {
                                return true;
                            }
                            break;
                    }
                    return false;
                })) {
                    mode = value;
                }
                break;

        }

        if (mode == invalid && typ != Typ[Invalid]) {
            check.errorf(x, invalidArg + "%s for %s", x, bin.name);
            return ;
        }
        x.mode = mode;
        x.typ = Typ[Int];
        x.val = val;
        if (check.Types != null && mode != constant_) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ, typ));
        }
    else if (id == _Close) 
        // close(c)
        var c = asChan(x.typ);
        if (c == null) {
            check.errorf(x, invalidArg + "%s is not a channel", x);
            return ;
        }
        if (c.dir == RecvOnly) {
            check.errorf(x, invalidArg + "%s must not be a receive-only channel", x);
            return ;
        }
        x.mode = novalue;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(null, c));
        }
    else if (id == _Complex) 
        // complex(x, y floatT) complexT
        ref operand y = ref heap(out ptr<operand> _addr_y);
        arg(_addr_y, 1);
        if (y.mode == invalid) {
            return ;
        }
        nint d = 0;
        if (isUntyped(x.typ)) {
            d |= 1;
        }
        if (isUntyped(y.typ)) {
            d |= 2;
        }
        switch (d) {
            case 0: 

                break;
            case 1: 
                // only x is untyped => convert to type of y
                check.convertUntyped(x, y.typ);
                break;
            case 2: 
                // only y is untyped => convert to type of x
                check.convertUntyped(_addr_y, x.typ);
                break;
            case 3: 
                // x and y are untyped =>
                // 1) if both are constants, convert them to untyped
                //    floating-point numbers if possible,
                // 2) if one of them is not constant (possible because
                //    it contains a shift that is yet untyped), convert
                //    both of them to float64 since they must have the
                //    same type to succeed (this will result in an error
                //    because shifts of floats are not permitted)
                            if (x.mode == constant_ && y.mode == constant_) {
                                Action<ptr<operand>> toFloat = x => {
                                    if (isNumeric(x.typ) && constant.Sign(constant.Imag(x.val)) == 0) {
                                        x.typ = Typ[UntypedFloat];
                                    }
                                }
                            else
                ;
                                toFloat(x);
                                toFloat(_addr_y);
                            } {
                                check.convertUntyped(x, Typ[Float64]);
                                check.convertUntyped(_addr_y, Typ[Float64]); 
                                // x and y should be invalid now, but be conservative
                                // and check below
                            }
                break;
        }
        if (x.mode == invalid || y.mode == invalid) {
            return ;
        }
        if (!check.identical(x.typ, y.typ)) {
            check.errorf(x, invalidOp + "%v (mismatched types %s and %s)", call, x.typ, y.typ);
            return ;
        }
        Func<Type, Type> f = x => {
            {
                var t__prev1 = t;

                ref var t = ref heap(asBasic(x), out ptr<var> _addr_t);

                if (t != null) {

                    if (t.kind == Float32) 
                        return Typ[Complex64];
                    else if (t.kind == Float64) 
                        return Typ[Complex128];
                    else if (t.kind == UntypedFloat) 
                        return Typ[UntypedComplex];
                                    }
                t = t__prev1;

            }
            return null;
        };
        var resTyp = check.applyTypeFunc(f, x.typ);
        if (resTyp == null) {
            check.errorf(x, invalidArg + "arguments have type %s, expected floating-point", x.typ);
            return ;
        }
        if (x.mode == constant_ && y.mode == constant_) {
            x.val = constant.BinaryOp(constant.ToFloat(x.val), token.ADD, constant.MakeImag(constant.ToFloat(y.val)));
        }
        else
 {
            x.mode = value;
        }
        if (check.Types != null && x.mode != constant_) {
            check.recordBuiltinType(call.Fun, makeSig(resTyp, x.typ, x.typ));
        }
        x.typ = resTyp;
    else if (id == _Copy) 
        // copy(x, y []T) int
        Type dst = default;
        {
            var t__prev1 = t;

            t = asSlice(x.typ);

            if (t != null) {
                dst = t.elem;
            }
            t = t__prev1;

        }

        y = default;
        arg(_addr_y, 1);
        if (y.mode == invalid) {
            return ;
        }
        Type src = default;
        switch (optype(y.typ).type()) {
            case ptr<Basic> t:
                if (isString(y.typ)) {
                    src = universeByte;
                }
                break;
            case ptr<Slice> t:
                src = t.elem;
                break;

        }

        if (dst == null || src == null) {
            check.errorf(x, invalidArg + "copy expects slice arguments; found %s and %s", x, _addr_y);
            return ;
        }
        if (!check.identical(dst, src)) {
            check.errorf(x, invalidArg + "arguments to copy %s and %s have different element types %s and %s", x, _addr_y, dst, src);
            return ;
        }
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(Typ[Int], x.typ, y.typ));
        }
        x.mode = value;
        x.typ = Typ[Int];
    else if (id == _Delete) 
        // delete(m, k)
        var m = asMap(x.typ);
        if (m == null) {
            check.errorf(x, invalidArg + "%s is not a map", x);
            return ;
        }
        arg(x, 1); // k
        if (x.mode == invalid) {
            return ;
        }
        check.assignment(x, m.key, "argument to delete");
        if (x.mode == invalid) {
            return ;
        }
        x.mode = novalue;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(null, m, m.key));
        }
    else if (id == _Imag || id == _Real) 
        // imag(complexT) floatT
        // real(complexT) floatT

        // convert or check untyped argument
        if (isUntyped(x.typ)) {
            if (x.mode == constant_) { 
                // an untyped constant number can always be considered
                // as a complex constant
                if (isNumeric(x.typ)) {
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
                if (x.mode == invalid) {
                    return ;
                }
            }
        }
        f = x => {
            {
                var t__prev1 = t;

                t = asBasic(x);

                if (t != null) {

                    if (t.kind == Complex64) 
                        return Typ[Float32];
                    else if (t.kind == Complex128) 
                        return Typ[Float64];
                    else if (t.kind == UntypedComplex) 
                        return Typ[UntypedFloat];
                                    }
                t = t__prev1;

            }
            return null;
        };
        resTyp = check.applyTypeFunc(f, x.typ);
        if (resTyp == null) {
            check.errorf(x, invalidArg + "argument has type %s, expected complex type", x.typ);
            return ;
        }
        if (x.mode == constant_) {
            if (id == _Real) {
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
        if (check.Types != null && x.mode != constant_) {
            check.recordBuiltinType(call.Fun, makeSig(resTyp, x.typ));
        }
        x.typ = resTyp;
    else if (id == _Make) 
        // make(T, n)
        // make(T, n, m)
        // (no argument evaluated yet)
        var arg0 = call.ArgList[0];
        T = check.varType(arg0);
        if (T == Typ[Invalid]) {
            return ;
        }
        nint min = -1;
        nint max = 10;
        Func<Type, bool> valid = default;
        valid = t => {
            m = default;
            switch (optype(t).type()) {
                case ptr<Slice> t:
                    m = 2;
                    break;
                case ptr<Map> t:
                    m = 1;
                    break;
                case ptr<Chan> t:
                    m = 1;
                    break;
                case ptr<Sum> t:
                    return t.@is(valid);
                    break;
                default:
                {
                    var t = optype(t).type();
                    return false;
                    break;
                }
            }
            if (m > min) {
                min = m;
            }
            if (m + 1 < max) {
                max = m + 1;
            }
            return true;
        };

        if (!valid(T)) {
            check.errorf(arg0, invalidArg + "cannot make %s; type must be slice, map, or channel", arg0);
            return ;
        }
        if (nargs < min || max < nargs) {
            if (min == max) {
                check.errorf(call, "%v expects %d arguments; found %d", call, min, nargs);
            }
            else
 {
                check.errorf(call, "%v expects %d or %d arguments; found %d", call, min, max, nargs);
            }
            return ;
        }
        Type types = new slice<Type>(new Type[] { T });
        slice<long> sizes = default; // constant integer arguments, if any
        {
            Action<ptr<operand>, nint> arg__prev1 = arg;

            foreach (var (_, __arg) in call.ArgList[(int)1..]) {
                arg = __arg;
                var (typ, size) = check.index(arg, -1); // ok to continue with typ == Typ[Invalid]
                types = append(types, typ);
                if (size >= 0) {
                    sizes = append(sizes, size);
                }
            }
            arg = arg__prev1;
        }

        if (len(sizes) == 2 && sizes[0] > sizes[1]) {
            check.error(call.ArgList[1], invalidArg + "length and capacity swapped"); 
            // safe to continue
        }
        x.mode = value;
        x.typ = T;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ, types));
        }
    else if (id == _New) 
        // new(T)
        // (no argument evaluated yet)
        T = check.varType(call.ArgList[0]);
        if (T == Typ[Invalid]) {
            return ;
        }
        x.mode = value;
        x.typ = addr(new Pointer(base:T));
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ, T));
        }
    else if (id == _Panic) 
        // panic(x)
        // record panic call if inside a function with result parameters
        // (for use in Checker.isTerminating)
        if (check.sig != null && check.sig.results.Len() > 0) { 
            // function has result parameters
            var p = check.isPanic;
            if (p == null) { 
                // allocate lazily
                p = make_map<ptr<syntax.CallExpr>, bool>();
                check.isPanic = p;
            }
            p[call] = true;
        }
        check.assignment(x, _addr_emptyInterface, "argument to panic");
        if (x.mode == invalid) {
            return ;
        }
        x.mode = novalue;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(null, _addr_emptyInterface));
        }
    else if (id == _Print || id == _Println) 
        // print(x, y, ...)
        // println(x, y, ...)
        slice<Type> @params = default;
        if (nargs > 0) {
            params = make_slice<Type>(nargs);
            {
                var i__prev1 = i;

                for (i = 0; i < nargs; i++) {
                    if (i > 0) {
                        arg(x, i); // first argument already evaluated
                    }
                    check.assignment(x, null, "argument to " + predeclaredFuncs[id].name);
                    if (x.mode == invalid) { 
                        // TODO(gri) "use" all arguments?
                        return ;
                    }
                    params[i] = x.typ;
                }

                i = i__prev1;
            }
        }
        x.mode = novalue;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(null, params));
        }
    else if (id == _Recover) 
        // recover() interface{}
        x.mode = value;
        x.typ = _addr_emptyInterface;
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ));
        }
    else if (id == _Add) 
        // unsafe.Add(ptr unsafe.Pointer, len IntegerType) unsafe.Pointer
        if (!check.allowVersion(check.pkg, 1, 17)) {
            check.error(call.Fun, "unsafe.Add requires go1.17 or later");
            return ;
        }
        check.assignment(x, Typ[UnsafePointer], "argument to unsafe.Add");
        if (x.mode == invalid) {
            return ;
        }
        y = default;
        arg(_addr_y, 1);
        if (!check.isValidIndex(_addr_y, "length", true)) {
            return ;
        }
        x.mode = value;
        x.typ = Typ[UnsafePointer];
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ, x.typ, y.typ));
        }
    else if (id == _Alignof) 
        // unsafe.Alignof(x T) uintptr
        if (asTypeParam(x.typ) != null) {
            check.errorf(call, invalidOp + "unsafe.Alignof undefined for %s", x);
            return ;
        }
        check.assignment(x, null, "argument to unsafe.Alignof");
        if (x.mode == invalid) {
            return ;
        }
        x.mode = constant_;
        x.val = constant.MakeInt64(check.conf.alignof(x.typ));
        x.typ = Typ[Uintptr]; 
        // result is constant - no need to record signature
    else if (id == _Offsetof) 
        // unsafe.Offsetof(x T) uintptr, where x must be a selector
        // (no argument evaluated yet)
        arg0 = call.ArgList[0];
        ptr<syntax.SelectorExpr> (selx, _) = unparen(arg0)._<ptr<syntax.SelectorExpr>>();
        if (selx == null) {
            check.errorf(arg0, invalidArg + "%s is not a selector expression", arg0);
            check.use(arg0);
            return ;
        }
        check.expr(x, selx.X);
        if (x.mode == invalid) {
            return ;
        }
        var @base = derefStructPtr(x.typ);
        var sel = selx.Sel.Value;
        var (obj, index, indirect) = check.lookupFieldOrMethod(base, false, check.pkg, sel);
        switch (obj.type()) {
            case 
                check.errorf(x, invalidArg + "%s has no single field %s", base, sel);
                return ;
                break;
            case ptr<Func> _:
                check.errorf(arg0, invalidArg + "%s is a method value", arg0);
                return ;
                break;
        }
        if (indirect) {
            check.errorf(x, invalidArg + "field %s is embedded via a pointer in %s", sel, base);
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
        if (asTypeParam(x.typ) != null) {
            check.errorf(call, invalidOp + "unsafe.Sizeof undefined for %s", x);
            return ;
        }
        check.assignment(x, null, "argument to unsafe.Sizeof");
        if (x.mode == invalid) {
            return ;
        }
        x.mode = constant_;
        x.val = constant.MakeInt64(check.conf.@sizeof(x.typ));
        x.typ = Typ[Uintptr]; 
        // result is constant - no need to record signature
    else if (id == _Slice) 
        // unsafe.Slice(ptr *T, len IntegerType) []T
        if (!check.allowVersion(check.pkg, 1, 17)) {
            check.error(call.Fun, "unsafe.Slice requires go1.17 or later");
            return ;
        }
        typ = asPointer(x.typ);
        if (typ == null) {
            check.errorf(x, invalidArg + "%s is not a pointer", x);
            return ;
        }
        y = default;
        arg(_addr_y, 1);
        if (!check.isValidIndex(_addr_y, "length", false)) {
            return ;
        }
        x.mode = value;
        x.typ = NewSlice(typ.@base);
        if (check.Types != null) {
            check.recordBuiltinType(call.Fun, makeSig(x.typ, typ, y.typ));
        }
    else if (id == _Assert) 
        // assert(pred) causes a typechecker error if pred is false.
        // The result of assert is the value of pred if there is no error.
        // Note: assert is only available in self-test mode.
        if (x.mode != constant_ || !isBoolean(x.typ)) {
            check.errorf(x, invalidArg + "%s is not a boolean constant", x);
            return ;
        }
        if (x.val.Kind() != constant.Bool) {
            check.errorf(x, "internal error: value of %s should be a boolean constant", x);
            return ;
        }
        if (!constant.BoolVal(x.val)) {
            check.errorf(call, "%v failed", call); 
            // compile-time assertion failure - safe to continue
        }
    else if (id == _Trace) 
        // trace(x, y, z, ...) dumps the positions, expressions, and
        // values of its arguments. The result of trace is the value
        // of the first argument.
        // Note: trace is only available in self-test mode.
        // (no argument evaluated yet)
        if (nargs == 0) {
            check.dump("%v: trace() without arguments", posFor(call));
            x.mode = novalue;
            break;
        }
        t = default;
        var x1 = x;
        {
            Action<ptr<operand>, nint> arg__prev1 = arg;

            foreach (var (_, __arg) in call.ArgList) {
                arg = __arg;
                check.rawExpr(x1, arg, null); // permit trace for types, e.g.: new(trace(T))
                check.dump("%v: %s", posFor(x1), x1);
                _addr_x1 = _addr_t;
                x1 = ref _addr_x1.val; // use incoming x only for first argument
            }
            arg = arg__prev1;
        }
    else 
        unreachable();
        return true;
});

// applyTypeFunc applies f to x. If x is a type parameter,
// the result is a type parameter constrained by an new
// interface bound. The type bounds for that interface
// are computed by applying f to each of the type bounds
// of x. If any of these applications of f return nil,
// applyTypeFunc returns nil.
// If x is not a type parameter, the result is f(x).
private static Type applyTypeFunc(this ptr<Checker> _addr_check, Func<Type, Type> f, Type x) {
    ref Checker check = ref _addr_check.val;

    {
        var tp = asTypeParam(x);

        if (tp != null) { 
            // Test if t satisfies the requirements for the argument
            // type and collect possible result types at the same time.
            slice<Type> rtypes = default;
            if (!tp.Bound().@is(x => {
                {
                    var r = f(x);

                    if (r != null) {
                        rtypes = append(rtypes, r);
                        return true;
                    }

                }
                return false;
            })) {
                return null;
            } 

            // TODO(gri) Would it be ok to return just the one type
            //           if len(rtypes) == 1? What about top-level
            //           uses of real() where the result is used to
            //           define type and initialize a variable?

            // construct a suitable new type parameter
            var tpar = NewTypeName(nopos, null, "<type parameter>", null);
            var ptyp = check.NewTypeParam(tpar, 0, _addr_emptyInterface); // assigns type to tpar as a side-effect
            var tsum = NewSum(rtypes);
            ptyp.bound = addr(new Interface(types:tsum,allMethods:markComplete,allTypes:tsum));

            return ptyp;
        }
    }

    return f(x);
}

// makeSig makes a signature for the given argument and result types.
// Default types are used for untyped arguments, and res may be nil.
private static ptr<Signature> makeSig(Type res, params Type[] args) {
    args = args.Clone();

    var list = make_slice<ptr<Var>>(len(args));
    foreach (var (i, param) in args) {
        list[i] = NewVar(nopos, null, "", Default(param));
    }    var @params = NewTuple(list);
    ptr<Tuple> result;
    if (res != null) {
        assert(!isUntyped(res));
        result = NewTuple(NewVar(nopos, null, "", res));
    }
    return addr(new Signature(params:params,results:result));
}

// implicitArrayDeref returns A if typ is of the form *A and A is an array;
// otherwise it returns typ.
//
private static Type implicitArrayDeref(Type typ) {
    {
        ptr<Pointer> (p, ok) = typ._<ptr<Pointer>>();

        if (ok) {
            {
                var a = asArray(p.@base);

                if (a != null) {
                    return a;
                }

            }
        }
    }
    return typ;
}

// unparen returns e with any enclosing parentheses stripped.
private static syntax.Expr unparen(syntax.Expr e) {
    while (true) {
        ptr<syntax.ParenExpr> (p, ok) = e._<ptr<syntax.ParenExpr>>();
        if (!ok) {
            return e;
        }
        e = p.X;
    }
}

} // end types2_package
