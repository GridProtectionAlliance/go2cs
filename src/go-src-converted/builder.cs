// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:09 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\builder.go
// This file implements the BUILD phase of SSA construction.
//
// SSA construction has two phases, CREATE and BUILD.  In the CREATE phase
// (create.go), all packages are constructed and type-checked and
// definitions of all package members are created, method-sets are
// computed, and wrapper methods are synthesized.
// ssa.Packages are created in arbitrary order.
//
// In the BUILD phase (builder.go), the builder traverses the AST of
// each Go source function and generates SSA instructions for the
// function body.  Initializer expressions for package-level variables
// are emitted to the package's init() function in the order specified
// by go/types.Info.InitOrder, then code for each function in the
// package is generated in lexical order.
// The BUILD phases for distinct packages are independent and are
// executed in parallel.
//
// TODO(adonovan): indeed, building functions is now embarrassingly parallel.
// Audit for concurrency then benchmark using more goroutines.
//
// The builder's and Program's indices (maps) are populated and
// mutated during the CREATE phase, but during the BUILD phase they
// remain constant.  The sole exception is Prog.methodSets and its
// related maps, which are protected by a dedicated mutex.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using sync = go.sync_package;
using System;
using System.Threading;


namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

private partial struct opaqueType : types.Type {
    public ref types.Type Type => ref Type_val;
    public @string name;
}

private static @string String(this ptr<opaqueType> _addr_t) {
    ref opaqueType t = ref _addr_t.val;

    return t.name;
}

private static var varOk = newVar("ok", tBool);private static var varIndex = newVar("index", tInt);private static var tBool = types.Typ[types.Bool];private static var tByte = types.Typ[types.Byte];private static var tInt = types.Typ[types.Int];private static var tInvalid = types.Typ[types.Invalid];private static var tString = types.Typ[types.String];private static var tUntypedNil = types.Typ[types.UntypedNil];private static ptr<opaqueType> tRangeIter = addr(new opaqueType(nil,"iter"));private static var tEface = types.NewInterface(null, null).Complete();private static var vZero = intConst(0);private static var vOne = intConst(1);private static var vTrue = NewConst(constant.MakeBool(true), tBool);

// builder holds state associated with the package currently being built.
// Its methods contain all the logic for AST-to-SSA conversion.
private partial struct builder {
}

// cond emits to fn code to evaluate boolean condition e and jump
// to t or f depending on its value, performing various simplifications.
//
// Postcondition: fn.currentBlock is nil.
//
private static void cond(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e, ptr<BasicBlock> _addr_t, ptr<BasicBlock> _addr_f) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref BasicBlock t = ref _addr_t.val;
    ref BasicBlock f = ref _addr_f.val;

    switch (e.type()) {
        case ptr<ast.ParenExpr> e:
            b.cond(fn, e.X, t, f);
            return ;
            break;
        case ptr<ast.BinaryExpr> e:

            if (e.Op == token.LAND) 
                var ltrue = fn.newBasicBlock("cond.true");
                b.cond(fn, e.X, ltrue, f);
                fn.currentBlock = ltrue;
                b.cond(fn, e.Y, t, f);
                return ;
            else if (e.Op == token.LOR) 
                var lfalse = fn.newBasicBlock("cond.false");
                b.cond(fn, e.X, t, lfalse);
                fn.currentBlock = lfalse;
                b.cond(fn, e.Y, t, f);
                return ;
                        break;
        case ptr<ast.UnaryExpr> e:
            if (e.Op == token.NOT) {
                b.cond(fn, e.X, f, t);
                return ;
            }
            break; 

        // A traditional compiler would simplify "if false" (etc) here
        // but we do not, for better fidelity to the source code.
        //
        // The value of a constant condition may be platform-specific,
        // and may cause blocks that are reachable in some configuration
        // to be hidden from subsequent analyses such as bug-finding tools.
    } 

    // A traditional compiler would simplify "if false" (etc) here
    // but we do not, for better fidelity to the source code.
    //
    // The value of a constant condition may be platform-specific,
    // and may cause blocks that are reachable in some configuration
    // to be hidden from subsequent analyses such as bug-finding tools.
    emitIf(fn, b.expr(fn, e), t, f);

}

// logicalBinop emits code to fn to evaluate e, a &&- or
// ||-expression whose reified boolean value is wanted.
// The value is returned.
//
private static Value logicalBinop(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.BinaryExpr> _addr_e) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.BinaryExpr e = ref _addr_e.val;

    var rhs = fn.newBasicBlock("binop.rhs");
    var done = fn.newBasicBlock("binop.done"); 

    // T(e) = T(e.X) = T(e.Y) after untyped constants have been
    // eliminated.
    // TODO(adonovan): not true; MyBool==MyBool yields UntypedBool.
    var t = fn.Pkg.typeOf(e);

    Value @short = default; // value of the short-circuit path

    if (e.Op == token.LAND) 
        b.cond(fn, e.X, rhs, done);
        short = NewConst(constant.MakeBool(false), t);
    else if (e.Op == token.LOR) 
        b.cond(fn, e.X, done, rhs);
        short = NewConst(constant.MakeBool(true), t);
    // Is rhs unreachable?
    if (rhs.Preds == null) { 
        // Simplify false&&y to false, true||y to true.
        fn.currentBlock = done;
        return short;

    }
    if (done.Preds == null) { 
        // Simplify true&&y (or false||y) to y.
        fn.currentBlock = rhs;
        return b.expr(fn, e.Y);

    }
    slice<Value> edges = default;
    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in done.Preds) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        edges = append(edges, short);
    }    fn.currentBlock = rhs;
    edges = append(edges, b.expr(fn, e.Y));
    emitJump(fn, done);
    fn.currentBlock = done;

    ptr<Phi> phi = addr(new Phi(Edges:edges,Comment:e.Op.String()));
    phi.pos = e.OpPos;
    phi.typ = t;
    return done.emit(phi);

}

// exprN lowers a multi-result expression e to SSA form, emitting code
// to fn and returning a single Value whose type is a *types.Tuple.
// The caller must access the components via Extract.
//
// Multi-result expressions include CallExprs in a multi-value
// assignment or return statement, and "value,ok" uses of
// TypeAssertExpr, IndexExpr (when X is a map), and UnaryExpr (when Op
// is token.ARROW).
//
private static Value exprN(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    ptr<types.Tuple> typ = fn.Pkg.typeOf(e)._<ptr<types.Tuple>>();
    switch (e.type()) {
        case ptr<ast.ParenExpr> e:
            return b.exprN(fn, e.X);
            break;
        case ptr<ast.CallExpr> e:
            ref Call c = ref heap(out ptr<Call> _addr_c);
            b.setCall(fn, e, _addr_c.Call);
            c.typ = typ;
            return fn.emit(_addr_c);
            break;
        case ptr<ast.IndexExpr> e:
            ptr<types.Map> mapt = fn.Pkg.typeOf(e.X).Underlying()._<ptr<types.Map>>();
            ptr<Lookup> lookup = addr(new Lookup(X:b.expr(fn,e.X),Index:emitConv(fn,b.expr(fn,e.Index),mapt.Key()),CommaOk:true,));
            lookup.setType(typ);
            lookup.setPos(e.Lbrack);
            return fn.emit(lookup);
            break;
        case ptr<ast.TypeAssertExpr> e:
            return emitTypeTest(fn, b.expr(fn, e.X), typ.At(0).Type(), e.Lparen);
            break;
        case ptr<ast.UnaryExpr> e:
            ptr<UnOp> unop = addr(new UnOp(Op:token.ARROW,X:b.expr(fn,e.X),CommaOk:true,));
            unop.setType(typ);
            unop.setPos(e.OpPos);
            return fn.emit(unop);
            break;
    }
    panic(fmt.Sprintf("exprN(%T) in %s", e, fn));

});

// builtin emits to fn SSA instructions to implement a call to the
// built-in function obj with the specified arguments
// and return type.  It returns the value defined by the result.
//
// The result is nil if no special handling was required; in this case
// the caller should treat this like an ordinary library function
// call.
//
private static Value builtin(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<types.Builtin> _addr_obj, slice<ast.Expr> args, types.Type typ, token.Pos pos) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref types.Builtin obj = ref _addr_obj.val;

    switch (obj.Name()) {
        case "make": 
            switch (typ.Underlying().type()) {
                case ptr<types.Slice> _:
                    var n = b.expr(fn, args[1]);
                    var m = n;
                    if (len(args) == 3) {
                        m = b.expr(fn, args[2]);
                    }
                    {
                        var m__prev1 = m;

                        ptr<Const> (m, ok) = m._<ptr<Const>>();

                        if (ok) { 
                            // treat make([]T, n, m) as new([m]T)[:n]
                            var cap = m.Int64();
                            var at = types.NewArray(typ.Underlying()._<ptr<types.Slice>>().Elem(), cap);
                            var alloc = emitNew(fn, at, pos);
                            alloc.Comment = "makeslice";
                            ptr<Slice> v = addr(new Slice(X:alloc,High:n,));
                            v.setPos(pos);
                            v.setType(typ);
                            return fn.emit(v);

                        }

                        m = m__prev1;

                    }

                    v = addr(new MakeSlice(Len:n,Cap:m,));
                    v.setPos(pos);
                    v.setType(typ);
                    return fn.emit(v);
                    break;
                case ptr<types.Map> _:
                    Value res = default;
                    if (len(args) == 2) {
                        res = b.expr(fn, args[1]);
                    }
                    v = addr(new MakeMap(Reserve:res));
                    v.setPos(pos);
                    v.setType(typ);
                    return fn.emit(v);
                    break;
                case ptr<types.Chan> _:
                    Value sz = vZero;
                    if (len(args) == 2) {
                        sz = b.expr(fn, args[1]);
                    }
                    v = addr(new MakeChan(Size:sz));
                    v.setPos(pos);
                    v.setType(typ);
                    return fn.emit(v);
                    break;

            }

            break;
        case "new": 
            alloc = emitNew(fn, deref(typ), pos);
            alloc.Comment = "new";
            return alloc;
            break;
        case "len": 
            // Special case: len or cap of an array or *array is
            // based on the type, not the value which may be nil.
            // We must still evaluate the value, though.  (If it
            // was side-effect free, the whole call would have
            // been constant-folded.)

        case "cap": 
            // Special case: len or cap of an array or *array is
            // based on the type, not the value which may be nil.
            // We must still evaluate the value, though.  (If it
            // was side-effect free, the whole call would have
            // been constant-folded.)
            var t = deref(fn.Pkg.typeOf(args[0])).Underlying();
            {
                var at__prev1 = at;

                ptr<types.Array> (at, ok) = t._<ptr<types.Array>>();

                if (ok) {
                    b.expr(fn, args[0]); // for effects only
                    return intConst(at.Len());

                } 
                // Otherwise treat as normal.

                at = at__prev1;

            } 
            // Otherwise treat as normal.
            break;
        case "panic": 
            fn.emit(addr(new Panic(X:emitConv(fn,b.expr(fn,args[0]),tEface),pos:pos,)));
            fn.currentBlock = fn.newBasicBlock("unreachable");
            return vTrue; // any non-nil Value will do
            break;
    }
    return null; // treat all others as a regular function call
}

// addr lowers a single-result addressable expression e to SSA form,
// emitting code to fn and returning the location (an lvalue) defined
// by the expression.
//
// If escaping is true, addr marks the base variable of the
// addressable expression e as being a potentially escaping pointer
// value.  For example, in this code:
//
//   a := A{
//     b: [1]B{B{c: 1}}
//   }
//   return &a.b[0].c
//
// the application of & causes a.b[0].c to have its address taken,
// which means that ultimately the local variable a must be
// heap-allocated.  This is a simple but very conservative escape
// analysis.
//
// Operations forming potentially escaping pointers include:
// - &x, including when implicit in method call or composite literals.
// - a[:] iff a is an array (not *array)
// - references to variables in lexically enclosing functions.
//
private static lvalue addr(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e, bool escaping) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    switch (e.type()) {
        case ptr<ast.Ident> e:
            if (isBlankIdent(e)) {
                return new blank();
            }
            var obj = fn.Pkg.objectOf(e);
            var v = fn.Prog.packageLevelValue(obj); // var (address)
            if (v == null) {
                v = fn.lookup(obj, escaping);
            }

            return addr(new address(addr:v,pos:e.Pos(),expr:e));
            break;
        case ptr<ast.CompositeLit> e:
            var t = deref(fn.Pkg.typeOf(e));
            v = ;
            if (escaping) {
                v = emitNew(fn, t, e.Lbrace);
            }
            else
 {
                v = fn.addLocal(t, e.Lbrace);
            }

            v.Comment = "complit";
            ref storebuf sb = ref heap(out ptr<storebuf> _addr_sb);
            b.compLit(fn, v, e, true, _addr_sb);
            sb.emit(fn);
            return addr(new address(addr:v,pos:e.Lbrace,expr:e));
            break;
        case ptr<ast.ParenExpr> e:
            return b.addr(fn, e.X, escaping);
            break;
        case ptr<ast.SelectorExpr> e:
            var (sel, ok) = fn.Pkg.info.Selections[e];
            if (!ok) { 
                // qualified identifier
                return b.addr(fn, e.Sel, escaping);

            }

            if (sel.Kind() != types.FieldVal) {
                panic(sel);
            }

            var wantAddr = true;
            v = b.receiver(fn, e.X, wantAddr, escaping, sel);
            var last = len(sel.Index()) - 1;
            return addr(new address(addr:emitFieldSelection(fn,v,sel.Index()[last],true,e.Sel),pos:e.Sel.Pos(),expr:e.Sel,));
            break;
        case ptr<ast.IndexExpr> e:
            Value x = default;
            types.Type et = default;
            switch (fn.Pkg.typeOf(e.X).Underlying().type()) {
                case ptr<types.Array> t:
                    x = b.addr(fn, e.X, escaping).address(fn);
                    et = types.NewPointer(t.Elem());
                    break;
                case ptr<types.Pointer> t:
                    x = b.expr(fn, e.X);
                    et = types.NewPointer(t.Elem().Underlying()._<ptr<types.Array>>().Elem());
                    break;
                case ptr<types.Slice> t:
                    x = b.expr(fn, e.X);
                    et = types.NewPointer(t.Elem());
                    break;
                case ptr<types.Map> t:
                    return addr(new element(m:b.expr(fn,e.X),k:emitConv(fn,b.expr(fn,e.Index),t.Key()),t:t.Elem(),pos:e.Lbrack,));
                    break;
                default:
                {
                    var t = fn.Pkg.typeOf(e.X).Underlying().type();
                    panic("unexpected container type in IndexExpr: " + t.String());
                    break;
                }
            }
            v = addr(new IndexAddr(X:x,Index:emitConv(fn,b.expr(fn,e.Index),tInt),));
            v.setPos(e.Lbrack);
            v.setType(et);
            return addr(new address(addr:fn.emit(v),pos:e.Lbrack,expr:e));
            break;
        case ptr<ast.StarExpr> e:
            return addr(new address(addr:b.expr(fn,e.X),pos:e.Star,expr:e));
            break;

    }

    panic(fmt.Sprintf("unexpected address expression: %T", e));

});

private partial struct store {
    public lvalue lhs;
    public Value rhs;
}

private partial struct storebuf {
    public slice<store> stores;
}

private static void store(this ptr<storebuf> _addr_sb, lvalue lhs, Value rhs) {
    ref storebuf sb = ref _addr_sb.val;

    sb.stores = append(sb.stores, new store(lhs,rhs));
}

private static void emit(this ptr<storebuf> _addr_sb, ptr<Function> _addr_fn) {
    ref storebuf sb = ref _addr_sb.val;
    ref Function fn = ref _addr_fn.val;

    foreach (var (_, s) in sb.stores) {
        s.lhs.store(fn, s.rhs);
    }
}

// assign emits to fn code to initialize the lvalue loc with the value
// of expression e.  If isZero is true, assign assumes that loc holds
// the zero value for its type.
//
// This is equivalent to loc.store(fn, b.expr(fn, e)), but may generate
// better code in some cases, e.g., for composite literals in an
// addressable location.
//
// If sb is not nil, assign generates code to evaluate expression e, but
// not to update loc.  Instead, the necessary stores are appended to the
// storebuf sb so that they can be executed later.  This allows correct
// in-place update of existing variables when the RHS is a composite
// literal that may reference parts of the LHS.
//
private static void assign(this ptr<builder> _addr_b, ptr<Function> _addr_fn, lvalue loc, ast.Expr e, bool isZero, ptr<storebuf> _addr_sb) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref storebuf sb = ref _addr_sb.val;
 
    // Can we initialize it in place?
    {
        ptr<ast.CompositeLit> (e, ok) = unparen(e)._<ptr<ast.CompositeLit>>();

        if (ok) { 
            // A CompositeLit never evaluates to a pointer,
            // so if the type of the location is a pointer,
            // an &-operation is implied.
            {
                blank (_, ok) = loc._<blank>();

                if (!ok) { // avoid calling blank.typ()
                    if (isPointer(loc.typ())) {
                        var ptr = b.addr(fn, e, true).address(fn); 
                        // copy address
                        if (sb != null) {
                            sb.store(loc, ptr);
                        }
                        else
 {
                            loc.store(fn, ptr);
                        }

                        return ;

                    }

                }

            }


            {
                (_, ok) = loc._<ptr<address>>();

                if (ok) {
                    if (isInterface(loc.typ())) { 
                        // e.g. var x interface{} = T{...}
                        // Can't in-place initialize an interface value.
                        // Fall back to copying.
                    }
                    else
 { 
                        // x = T{...} or x := T{...}
                        var addr = loc.address(fn);
                        if (sb != null) {
                            b.compLit(fn, addr, e, isZero, sb);
                        }
                        else
 {
                            ref storebuf sb = ref heap(out ptr<storebuf> _addr_sb);
                            b.compLit(fn, addr, e, isZero, _addr_sb);
                            sb.emit(fn);
                        } 

                        // Subtle: emit debug ref for aggregate types only;
                        // slice and map are handled by store ops in compLit.
                        switch (loc.typ().Underlying().type()) {
                            case ptr<types.Struct> _:
                                emitDebugRef(fn, e, addr, true);
                                break;
                            case ptr<types.Array> _:
                                emitDebugRef(fn, e, addr, true);
                                break;

                        }

                        return ;

                    }

                }

            }

        }
    } 

    // simple case: just copy
    var rhs = b.expr(fn, e);
    if (sb != null) {
        sb.store(loc, rhs);
    }
    else
 {
        loc.store(fn, rhs);
    }
}

// expr lowers a single-result expression e to SSA form, emitting code
// to fn and returning the Value defined by the expression.
//
private static Value expr(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    e = unparen(e);

    var tv = fn.Pkg.info.Types[e]; 

    // Is expression a constant?
    if (tv.Value != null) {
        return NewConst(tv.Value, tv.Type);
    }
    Value v = default;
    if (tv.Addressable()) { 
        // Prefer pointer arithmetic ({Index,Field}Addr) followed
        // by Load over subelement extraction (e.g. Index, Field),
        // to avoid large copies.
        v = b.addr(fn, e, false).load(fn);

    }
    else
 {
        v = b.expr0(fn, e, tv);
    }
    if (fn.debugInfo()) {
        emitDebugRef(fn, e, v, false);
    }
    return v;

}

private static Value expr0(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e, types.TypeAndValue tv) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    switch (e.type()) {
        case ptr<ast.BasicLit> e:
            panic("non-constant BasicLit"); // unreachable
            break;
        case ptr<ast.FuncLit> e:
            ptr<Function> fn2 = addr(new Function(name:fmt.Sprintf("%s$%d",fn.Name(),1+len(fn.AnonFuncs)),Signature:fn.Pkg.typeOf(e.Type).Underlying().(*types.Signature),pos:e.Type.Func,parent:fn,Pkg:fn.Pkg,Prog:fn.Prog,syntax:e,));
            fn.AnonFuncs = append(fn.AnonFuncs, fn2);
            b.buildFunction(fn2);
            if (fn2.FreeVars == null) {
                return fn2;
            }
            ptr<MakeClosure> v = addr(new MakeClosure(Fn:fn2));
            v.setType(tv.Type);
            foreach (var (_, fv) in fn2.FreeVars) {
                v.Bindings = append(v.Bindings, fv.outer);
                fv.outer = null;
            }
            return fn.emit(v);
            break;
        case ptr<ast.TypeAssertExpr> e:
            return emitTypeAssert(fn, b.expr(fn, e.X), tv.Type, e.Lparen);
            break;
        case ptr<ast.CallExpr> e:
            if (fn.Pkg.info.Types[e.Fun].IsType()) { 
                // Explicit type conversion, e.g. string(x) or big.Int(x)
                var x = b.expr(fn, e.Args[0]);
                var y = emitConv(fn, x, tv.Type);
                if (y != x) {
                    switch (y.type()) {
                        case ptr<Convert> y:
                            y.pos = e.Lparen;
                            break;
                        case ptr<ChangeType> y:
                            y.pos = e.Lparen;
                            break;
                        case ptr<MakeInterface> y:
                            y.pos = e.Lparen;
                            break;
                    }

                }

                return y;

            } 
            // Call to "intrinsic" built-ins, e.g. new, make, panic.
            {
                ptr<ast.Ident> (id, ok) = unparen(e.Fun)._<ptr<ast.Ident>>();

                if (ok) {
                    {
                        ptr<types.Builtin> obj__prev2 = obj;

                        ptr<types.Builtin> (obj, ok) = fn.Pkg.info.Uses[id]._<ptr<types.Builtin>>();

                        if (ok) {
                            {
                                ptr<MakeClosure> v__prev3 = v;

                                v = b.builtin(fn, obj, e.Args, tv.Type, e.Lparen);

                                if (v != null) {
                                    return v;
                                }

                                v = v__prev3;

                            }

                        }

                        obj = obj__prev2;

                    }

                } 
                // Regular function call.

            } 
            // Regular function call.
            v = default;
            b.setCall(fn, e, _addr_v.Call);
            v.setType(tv.Type);
            return fn.emit(_addr_v);
            break;
        case ptr<ast.UnaryExpr> e:

            if (e.Op == token.AND) // &X --- potentially escaping.
                var addr = b.addr(fn, e.X, true);
                {
                    ptr<ast.StarExpr> (_, ok) = unparen(e.X)._<ptr<ast.StarExpr>>();

                    if (ok) { 
                        // &*p must panic if p is nil (http://golang.org/s/go12nil).
                        // For simplicity, we'll just (suboptimally) rely
                        // on the side effects of a load.
                        // TODO(adonovan): emit dedicated nilcheck.
                        addr.load(fn);

                    }

                }

                return addr.address(fn);
            else if (e.Op == token.ADD) 
                return b.expr(fn, e.X);
            else if (e.Op == token.NOT || e.Op == token.ARROW || e.Op == token.SUB || e.Op == token.XOR) // ! <- - ^
                v = addr(new UnOp(Op:e.Op,X:b.expr(fn,e.X),));
                v.setPos(e.OpPos);
                v.setType(tv.Type);
                return fn.emit(v);
            else 
                panic(e.Op);
                        break;
        case ptr<ast.BinaryExpr> e:

            if (e.Op == token.LAND || e.Op == token.LOR)
            {
                return b.logicalBinop(fn, e);
                goto __switch_break0;
            }
            if (e.Op == token.SHL || e.Op == token.SHR)
            {
                fallthrough = true;
            }
            if (fallthrough || e.Op == token.ADD || e.Op == token.SUB || e.Op == token.MUL || e.Op == token.QUO || e.Op == token.REM || e.Op == token.AND || e.Op == token.OR || e.Op == token.XOR || e.Op == token.AND_NOT)
            {
                return emitArith(fn, e.Op, b.expr(fn, e.X), b.expr(fn, e.Y), tv.Type, e.OpPos);
                goto __switch_break0;
            }
            if (e.Op == token.EQL || e.Op == token.NEQ || e.Op == token.GTR || e.Op == token.LSS || e.Op == token.LEQ || e.Op == token.GEQ)
            {
                var cmp = emitCompare(fn, e.Op, b.expr(fn, e.X), b.expr(fn, e.Y), e.OpPos); 
                // The type of x==y may be UntypedBool.
                return emitConv(fn, cmp, types.Default(tv.Type));
                goto __switch_break0;
            }
            // default: 
                panic("illegal op in BinaryExpr: " + e.Op.String());

            __switch_break0:;
            break;
        case ptr<ast.SliceExpr> e:
            Value low = default;            Value high = default;            Value max = default;

            x = default;
            switch (fn.Pkg.typeOf(e.X).Underlying().type()) {
                case ptr<types.Array> _:
                    x = b.addr(fn, e.X, true).address(fn);
                    break;
                case ptr<types.Basic> _:
                    x = b.expr(fn, e.X);
                    break;
                case ptr<types.Slice> _:
                    x = b.expr(fn, e.X);
                    break;
                case ptr<types.Pointer> _:
                    x = b.expr(fn, e.X);
                    break;
                default:
                {
                    panic("unreachable");
                    break;
                }
            }
            if (e.High != null) {
                high = b.expr(fn, e.High);
            }

            if (e.Low != null) {
                low = b.expr(fn, e.Low);
            }

            if (e.Slice3) {
                max = b.expr(fn, e.Max);
            }

            v = addr(new Slice(X:x,Low:low,High:high,Max:max,));
            v.setPos(e.Lbrack);
            v.setType(tv.Type);
            return fn.emit(v);
            break;
        case ptr<ast.Ident> e:
            var obj = fn.Pkg.info.Uses[e]; 
            // Universal built-in or nil?
            switch (obj.type()) {
                case ptr<types.Builtin> obj:
                    return addr(new Builtin(name:obj.Name(),sig:tv.Type.(*types.Signature)));
                    break;
                case ptr<types.Nil> obj:
                    return nilConst(tv.Type);
                    break; 
                // Package-level func or var?
            } 
            // Package-level func or var?
            {
                ptr<MakeClosure> v__prev1 = v;

                v = fn.Prog.packageLevelValue(obj);

                if (v != null) {
                    {
                        (_, ok) = obj._<ptr<types.Var>>();

                        if (ok) {
                            return emitLoad(fn, v); // var (address)
                        }

                    }

                    return v; // (func)
                } 
                // Local var.

                v = v__prev1;

            } 
            // Local var.
            return emitLoad(fn, fn.lookup(obj, false)); // var (address)
            break;
        case ptr<ast.SelectorExpr> e:
            var (sel, ok) = fn.Pkg.info.Selections[e];
            if (!ok) { 
                // qualified identifier
                return b.expr(fn, e.Sel);

            }


            if (sel.Kind() == types.MethodExpr) 
                // (*T).f or T.f, the method f from the method-set of type T.
                // The result is a "thunk".
                return emitConv(fn, makeThunk(fn.Prog, sel), tv.Type);
            else if (sel.Kind() == types.MethodVal) 
                // e.f where e is an expression and f is a method.
                // The result is a "bound".
                obj = sel.Obj()._<ptr<types.Func>>();
                var rt = recvType(obj);
                var wantAddr = isPointer(rt);
                var escaping = true;
                v = b.receiver(fn, e.X, wantAddr, escaping, sel);
                if (isInterface(rt)) { 
                    // If v has interface type I,
                    // we must emit a check that v is non-nil.
                    // We use: typeassert v.(I).
                    emitTypeAssert(fn, v, rt, token.NoPos);

                }

                ptr<MakeClosure> c = addr(new MakeClosure(Fn:makeBound(fn.Prog,obj),Bindings:[]Value{v},));
                c.setPos(e.Sel.Pos());
                c.setType(tv.Type);
                return fn.emit(c);
            else if (sel.Kind() == types.FieldVal) 
                var indices = sel.Index();
                var last = len(indices) - 1;
                v = b.expr(fn, e.X);
                v = emitImplicitSelections(fn, v, indices[..(int)last]);
                v = emitFieldSelection(fn, v, indices[last], false, e.Sel);
                return v;
                        panic("unexpected expression-relative selector");
            break;
        case ptr<ast.IndexExpr> e:
            switch (fn.Pkg.typeOf(e.X).Underlying().type()) {
                case ptr<types.Array> t:
                    v = addr(new Index(X:b.expr(fn,e.X),Index:emitConv(fn,b.expr(fn,e.Index),tInt),));
                    v.setPos(e.Lbrack);
                    v.setType(t.Elem());
                    return fn.emit(v);
                    break;
                case ptr<types.Map> t:
                    ptr<types.Map> mapt = fn.Pkg.typeOf(e.X).Underlying()._<ptr<types.Map>>();
                    v = addr(new Lookup(X:b.expr(fn,e.X),Index:emitConv(fn,b.expr(fn,e.Index),mapt.Key()),));
                    v.setPos(e.Lbrack);
                    v.setType(mapt.Elem());
                    return fn.emit(v);
                    break;
                case ptr<types.Basic> t:
                    v = addr(new Lookup(X:b.expr(fn,e.X),Index:b.expr(fn,e.Index),));
                    v.setPos(e.Lbrack);
                    v.setType(tByte);
                    return fn.emit(v);
                    break;
                case ptr<types.Slice> t:
                    return b.addr(fn, e, false).load(fn);
                    break;
                case ptr<types.Pointer> t:
                    return b.addr(fn, e, false).load(fn);
                    break;
                default:
                {
                    var t = fn.Pkg.typeOf(e.X).Underlying().type();
                    panic("unexpected container type in IndexExpr: " + t.String());
                    break;
                }

            }
            break;
        case ptr<ast.CompositeLit> e:
            return b.addr(fn, e, false).load(fn);
            break;
        case ptr<ast.StarExpr> e:
            return b.addr(fn, e, false).load(fn);
            break;

    }

    panic(fmt.Sprintf("unexpected expr: %T", e));

});

// stmtList emits to fn code for all statements in list.
private static void stmtList(this ptr<builder> _addr_b, ptr<Function> _addr_fn, slice<ast.Stmt> list) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    foreach (var (_, s) in list) {
        b.stmt(fn, s);
    }
}

// receiver emits to fn code for expression e in the "receiver"
// position of selection e.f (where f may be a field or a method) and
// returns the effective receiver after applying the implicit field
// selections of sel.
//
// wantAddr requests that the result is an an address.  If
// !sel.Indirect(), this may require that e be built in addr() mode; it
// must thus be addressable.
//
// escaping is defined as per builder.addr().
//
private static Value receiver(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Expr e, bool wantAddr, bool escaping, ptr<types.Selection> _addr_sel) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref types.Selection sel = ref _addr_sel.val;

    Value v = default;
    if (wantAddr && !sel.Indirect() && !isPointer(fn.Pkg.typeOf(e))) {
        v = b.addr(fn, e, escaping).address(fn);
    }
    else
 {
        v = b.expr(fn, e);
    }
    var last = len(sel.Index()) - 1;
    v = emitImplicitSelections(fn, v, sel.Index()[..(int)last]);
    if (!wantAddr && isPointer(v.Type())) {
        v = emitLoad(fn, v);
    }
    return v;

}

// setCallFunc populates the function parts of a CallCommon structure
// (Func, Method, Recv, Args[0]) based on the kind of invocation
// occurring in e.
//
private static void setCallFunc(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.CallExpr> _addr_e, ptr<CallCommon> _addr_c) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.CallExpr e = ref _addr_e.val;
    ref CallCommon c = ref _addr_c.val;

    c.pos = e.Lparen; 

    // Is this a method call?
    {
        ptr<ast.SelectorExpr> (selector, ok) = unparen(e.Fun)._<ptr<ast.SelectorExpr>>();

        if (ok) {
            var (sel, ok) = fn.Pkg.info.Selections[selector];
            if (ok && sel.Kind() == types.MethodVal) {
                ptr<types.Func> obj = sel.Obj()._<ptr<types.Func>>();
                var recv = recvType(obj);
                var wantAddr = isPointer(recv);
                var escaping = true;
                var v = b.receiver(fn, selector.X, wantAddr, escaping, sel);
                if (isInterface(recv)) { 
                    // Invoke-mode call.
                    c.Value = v;
                    c.Method = obj;

                }
                else
 { 
                    // "Call"-mode call.
                    c.Value = fn.Prog.declaredFunc(obj);
                    c.Args = append(c.Args, v);

                }

                return ;

            } 

            // sel.Kind()==MethodExpr indicates T.f() or (*T).f():
            // a statically dispatched call to the method f in the
            // method-set of T or *T.  T may be an interface.
            //
            // e.Fun would evaluate to a concrete method, interface
            // wrapper function, or promotion wrapper.
            //
            // For now, we evaluate it in the usual way.
            //
            // TODO(adonovan): opt: inline expr() here, to make the
            // call static and to avoid generation of wrappers.
            // It's somewhat tricky as it may consume the first
            // actual parameter if the call is "invoke" mode.
            //
            // Examples:
            //  type T struct{}; func (T) f() {}   // "call" mode
            //  type T interface { f() }           // "invoke" mode
            //
            //  type S struct{ T }
            //
            //  var s S
            //  S.f(s)
            //  (*S).f(&s)
            //
            // Suggested approach:
            // - consume the first actual parameter expression
            //   and build it with b.expr().
            // - apply implicit field selections.
            // - use MethodVal logic to populate fields of c.
        }
    } 

    // Evaluate the function operand in the usual way.
    c.Value = b.expr(fn, e.Fun);

}

// emitCallArgs emits to f code for the actual parameters of call e to
// a (possibly built-in) function of effective type sig.
// The argument values are appended to args, which is then returned.
//
private static slice<Value> emitCallArgs(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<types.Signature> _addr_sig, ptr<ast.CallExpr> _addr_e, slice<Value> args) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref types.Signature sig = ref _addr_sig.val;
    ref ast.CallExpr e = ref _addr_e.val;
 
    // f(x, y, z...): pass slice z straight through.
    if (e.Ellipsis != 0) {
        {
            var i__prev1 = i;
            var arg__prev1 = arg;

            foreach (var (__i, __arg) in e.Args) {
                i = __i;
                arg = __arg;
                var v = emitConv(fn, b.expr(fn, arg), sig.Params().At(i).Type());
                args = append(args, v);
            }

            i = i__prev1;
            arg = arg__prev1;
        }

        return args;

    }
    var offset = len(args); // 1 if call has receiver, 0 otherwise

    // Evaluate actual parameter expressions.
    //
    // If this is a chained call of the form f(g()) where g has
    // multiple return values (MRV), they are flattened out into
    // args; a suffix of them may end up in a varargs slice.
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in e.Args) {
            arg = __arg;
            v = b.expr(fn, arg);
            {
                ptr<types.Tuple> (ttuple, ok) = v.Type()._<ptr<types.Tuple>>();

                if (ok) { // MRV chain
                    {
                        var i__prev2 = i;

                        for (nint i = 0;
                        var n = ttuple.Len(); i < n; i++) {
                            args = append(args, emitExtract(fn, v, i));
                        }
                else


                        i = i__prev2;
                    }

                } {
                    args = append(args, v);
                }

            }

        }
        arg = arg__prev1;
    }

    var np = sig.Params().Len(); // number of normal parameters
    if (sig.Variadic()) {
        np--;
    }
    {
        var i__prev1 = i;

        for (i = 0; i < np; i++) {
            args[offset + i] = emitConv(fn, args[offset + i], sig.Params().At(i).Type());
        }

        i = i__prev1;
    } 

    // Actual->formal assignability conversions for variadic parameter,
    // and construction of slice.
    if (sig.Variadic()) {
        var varargs = args[(int)offset + np..];
        ptr<types.Slice> st = sig.Params().At(np).Type()._<ptr<types.Slice>>();
        var vt = st.Elem();
        if (len(varargs) == 0) {
            args = append(args, nilConst(st));
        }
        else
 { 
            // Replace a suffix of args with a slice containing it.
            var at = types.NewArray(vt, int64(len(varargs)));
            var a = emitNew(fn, at, token.NoPos);
            a.setPos(e.Rparen);
            a.Comment = "varargs";
            {
                var i__prev1 = i;
                var arg__prev1 = arg;

                foreach (var (__i, __arg) in varargs) {
                    i = __i;
                    arg = __arg;
                    ptr<IndexAddr> iaddr = addr(new IndexAddr(X:a,Index:intConst(int64(i)),));
                    iaddr.setType(types.NewPointer(vt));
                    fn.emit(iaddr);
                    emitStore(fn, iaddr, arg, arg.Pos());
                }

                i = i__prev1;
                arg = arg__prev1;
            }

            ptr<Slice> s = addr(new Slice(X:a));
            s.setType(st);
            args[offset + np] = fn.emit(s);
            args = args[..(int)offset + np + 1];

        }
    }
    return args;

}

// setCall emits to fn code to evaluate all the parameters of a function
// call e, and populates *c with those values.
//
private static void setCall(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.CallExpr> _addr_e, ptr<CallCommon> _addr_c) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.CallExpr e = ref _addr_e.val;
    ref CallCommon c = ref _addr_c.val;
 
    // First deal with the f(...) part and optional receiver.
    b.setCallFunc(fn, e, c); 

    // Then append the other actual parameters.
    ptr<types.Signature> (sig, _) = fn.Pkg.typeOf(e.Fun).Underlying()._<ptr<types.Signature>>();
    if (sig == null) {
        panic(fmt.Sprintf("no signature for call of %s", e.Fun));
    }
    c.Args = b.emitCallArgs(fn, sig, e, c.Args);

});

// assignOp emits to fn code to perform loc <op>= val.
private static void assignOp(this ptr<builder> _addr_b, ptr<Function> _addr_fn, lvalue loc, Value val, token.Token op, token.Pos pos) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    var oldv = loc.load(fn);
    loc.store(fn, emitArith(fn, op, oldv, emitConv(fn, val, oldv.Type()), loc.typ(), pos));
}

// localValueSpec emits to fn code to define all of the vars in the
// function-local ValueSpec, spec.
//
private static void localValueSpec(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.ValueSpec> _addr_spec) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.ValueSpec spec = ref _addr_spec.val;


    if (len(spec.Values) == len(spec.Names)) 
        // e.g. var x, y = 0, 1
        // 1:1 assignment
        {
            var i__prev1 = i;
            var id__prev1 = id;

            foreach (var (__i, __id) in spec.Names) {
                i = __i;
                id = __id;
                if (!isBlankIdent(id)) {
                    fn.addLocalForIdent(id);
                }
                var lval = b.addr(fn, id, false); // non-escaping
                b.assign(fn, lval, spec.Values[i], true, null);

            }

            i = i__prev1;
            id = id__prev1;
        }
    else if (len(spec.Values) == 0) 
        // e.g. var x, y int
        // Locals are implicitly zero-initialized.
        {
            var id__prev1 = id;

            foreach (var (_, __id) in spec.Names) {
                id = __id;
                if (!isBlankIdent(id)) {
                    var lhs = fn.addLocalForIdent(id);
                    if (fn.debugInfo()) {
                        emitDebugRef(fn, id, lhs, true);
                    }
                }
            }

            id = id__prev1;
        }
    else 
        // e.g. var x, y = pos()
        var tuple = b.exprN(fn, spec.Values[0]);
        {
            var i__prev1 = i;
            var id__prev1 = id;

            foreach (var (__i, __id) in spec.Names) {
                i = __i;
                id = __id;
                if (!isBlankIdent(id)) {
                    fn.addLocalForIdent(id);
                    lhs = b.addr(fn, id, false); // non-escaping
                    lhs.store(fn, emitExtract(fn, tuple, i));

                }

            }

            i = i__prev1;
            id = id__prev1;
        }
    }

// assignStmt emits code to fn for a parallel assignment of rhss to lhss.
// isDef is true if this is a short variable declaration (:=).
//
// Note the similarity with localValueSpec.
//
private static void assignStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, slice<ast.Expr> lhss, slice<ast.Expr> rhss, bool isDef) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
 
    // Side effects of all LHSs and RHSs must occur in left-to-right order.
    var lvals = make_slice<lvalue>(len(lhss));
    var isZero = make_slice<bool>(len(lhss));
    {
        var i__prev1 = i;

        foreach (var (__i, __lhs) in lhss) {
            i = __i;
            lhs = __lhs;
            lvalue lval = new blank();
            if (!isBlankIdent(lhs)) {
                if (isDef) {
                    {
                        var obj = fn.Pkg.info.Defs[lhs._<ptr<ast.Ident>>()];

                        if (obj != null) {
                            fn.addNamedLocal(obj);
                            isZero[i] = true;
                        }

                    }

                }

                lval = b.addr(fn, lhs, false); // non-escaping
            }

            lvals[i] = lval;

        }
        i = i__prev1;
    }

    if (len(lhss) == len(rhss)) { 
        // Simple assignment:   x     = f()        (!isDef)
        // Parallel assignment: x, y  = f(), g()   (!isDef)
        // or short var decl:   x, y := f(), g()   (isDef)
        //
        // In all cases, the RHSs may refer to the LHSs,
        // so we need a storebuf.
        ref storebuf sb = ref heap(out ptr<storebuf> _addr_sb);
        {
            var i__prev1 = i;

            foreach (var (__i) in rhss) {
                i = __i;
                b.assign(fn, lvals[i], rhss[i], isZero[i], _addr_sb);
            }
    else

            i = i__prev1;
        }

        sb.emit(fn);

    } { 
        // e.g. x, y = pos()
        var tuple = b.exprN(fn, rhss[0]);
        emitDebugRef(fn, rhss[0], tuple, false);
        {
            var i__prev1 = i;
            lvalue lval__prev1 = lval;

            foreach (var (__i, __lval) in lvals) {
                i = __i;
                lval = __lval;
                lval.store(fn, emitExtract(fn, tuple, i));
            }

            i = i__prev1;
            lval = lval__prev1;
        }
    }
}

// arrayLen returns the length of the array whose composite literal elements are elts.
private static long arrayLen(this ptr<builder> _addr_b, ptr<Function> _addr_fn, slice<ast.Expr> elts) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    long max = -1;
    long i = -1;
    foreach (var (_, e) in elts) {
        {
            ptr<ast.KeyValueExpr> (kv, ok) = e._<ptr<ast.KeyValueExpr>>();

            if (ok) {
                i = b.expr(fn, kv.Key)._<ptr<Const>>().Int64();
            }
            else
 {
                i++;
            }

        }

        if (i > max) {
            max = i;
        }
    }    return max + 1;

}

// compLit emits to fn code to initialize a composite literal e at
// address addr with type typ.
//
// Nested composite literals are recursively initialized in place
// where possible. If isZero is true, compLit assumes that addr
// holds the zero value for typ.
//
// Because the elements of a composite literal may refer to the
// variables being updated, as in the second line below,
//    x := T{a: 1}
//    x = T{a: x.a}
// all the reads must occur before all the writes.  Thus all stores to
// loc are emitted to the storebuf sb for later execution.
//
// A CompositeLit may have pointer type only in the recursive (nested)
// case when the type name is implicit.  e.g. in []*T{{}}, the inner
// literal has type *T behaves like &T{}.
// In that case, addr must hold a T, not a *T.
//
private static void compLit(this ptr<builder> _addr_b, ptr<Function> _addr_fn, Value addr, ptr<ast.CompositeLit> _addr_e, bool isZero, ptr<storebuf> _addr_sb) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.CompositeLit e = ref _addr_e.val;
    ref storebuf sb = ref _addr_sb.val;

    var typ = deref(fn.Pkg.typeOf(e));
    switch (typ.Underlying().type()) {
        case ptr<types.Struct> t:
            if (!isZero && len(e.Elts) != t.NumFields()) { 
                // memclear
                sb.store(addr(new address(addr,e.Lbrace,nil)), zeroValue(fn, deref(addr.Type())));
                isZero = true;

            }

            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in e.Elts) {
                    i = __i;
                    e = __e;
                    var fieldIndex = i;
                    var pos = e.Pos();
                    {
                        ptr<ast.KeyValueExpr> kv__prev1 = kv;

                        ptr<ast.KeyValueExpr> (kv, ok) = e._<ptr<ast.KeyValueExpr>>();

                        if (ok) {
                            ptr<ast.Ident> fname = kv.Key._<ptr<ast.Ident>>().Name;
                            {
                                var i__prev2 = i;

                                for (nint i = 0;
                                var n = t.NumFields(); i < n; i++) {
                                    var sf = t.Field(i);
                                    if (sf.Name() == fname) {
                                        fieldIndex = i;
                                        pos = kv.Colon;
                                        e = kv.Value;
                                        break;
                                    }
                                }


                                i = i__prev2;
                            }

                        }

                        kv = kv__prev1;

                    }

                    sf = t.Field(fieldIndex);
                    ptr<FieldAddr> faddr = addr(new FieldAddr(X:addr,Field:fieldIndex,));
                    faddr.setType(types.NewPointer(sf.Type()));
                    fn.emit(faddr);
                    b.assign(fn, addr(new address(addr:faddr,pos:pos,expr:e)), e, isZero, sb);

                }

                i = i__prev1;
                e = e__prev1;
            }
            break;
        case ptr<types.Array> t:
            ptr<types.Array> at;
            Value array = default;
            switch (t.type()) {
                case ptr<types.Slice> t:
                    at = types.NewArray(t.Elem(), b.arrayLen(fn, e.Elts));
                    var alloc = emitNew(fn, at, e.Lbrace);
                    alloc.Comment = "slicelit";
                    array = alloc;
                    break;
                case ptr<types.Array> t:
                    at = t;
                    array = addr;

                    if (!isZero && int64(len(e.Elts)) != at.Len()) { 
                        // memclear
                        sb.store(addr(new address(array,e.Lbrace,nil)), zeroValue(fn, deref(array.Type())));

                    }

                    break;

            }

            ptr<Const> idx;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in e.Elts) {
                    e = __e;
                    pos = e.Pos();
                    {
                        ptr<ast.KeyValueExpr> kv__prev1 = kv;

                        (kv, ok) = e._<ptr<ast.KeyValueExpr>>();

                        if (ok) {
                            idx = b.expr(fn, kv.Key)._<ptr<Const>>();
                            pos = kv.Colon;
                            e = kv.Value;
                        }
                        else
 {
                            long idxval = default;
                            if (idx != null) {
                                idxval = idx.Int64() + 1;
                            }
                            idx = intConst(idxval);
                        }

                        kv = kv__prev1;

                    }

                    ptr<IndexAddr> iaddr = addr(new IndexAddr(X:array,Index:idx,));
                    iaddr.setType(types.NewPointer(at.Elem()));
                    fn.emit(iaddr);
                    if (t != at) { // slice
                        // backing array is unaliased => storebuf not needed.
                        b.assign(fn, addr(new address(addr:iaddr,pos:pos,expr:e)), e, true, null);

                    }
                    else
 {
                        b.assign(fn, addr(new address(addr:iaddr,pos:pos,expr:e)), e, true, sb);
                    }

                }

                e = e__prev1;
            }

            if (t != at) { // slice
                ptr<Slice> s = addr(new Slice(X:array));
                s.setPos(e.Lbrace);
                s.setType(typ);
                sb.store(addr(new address(addr:addr,pos:e.Lbrace,expr:e)), fn.emit(s));

            }

            break;
        case ptr<types.Slice> t:
            ptr<types.Array> at;
            Value array = default;
            switch (t.type()) {
                case ptr<types.Slice> t:
                    at = types.NewArray(t.Elem(), b.arrayLen(fn, e.Elts));
                    var alloc = emitNew(fn, at, e.Lbrace);
                    alloc.Comment = "slicelit";
                    array = alloc;
                    break;
                case ptr<types.Array> t:
                    at = t;
                    array = addr;

                    if (!isZero && int64(len(e.Elts)) != at.Len()) { 
                        // memclear
                        sb.store(addr(new address(array,e.Lbrace,nil)), zeroValue(fn, deref(array.Type())));

                    }

                    break;

            }

            ptr<Const> idx;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in e.Elts) {
                    e = __e;
                    pos = e.Pos();
                    {
                        ptr<ast.KeyValueExpr> kv__prev1 = kv;

                        (kv, ok) = e._<ptr<ast.KeyValueExpr>>();

                        if (ok) {
                            idx = b.expr(fn, kv.Key)._<ptr<Const>>();
                            pos = kv.Colon;
                            e = kv.Value;
                        }
                        else
 {
                            long idxval = default;
                            if (idx != null) {
                                idxval = idx.Int64() + 1;
                            }
                            idx = intConst(idxval);
                        }

                        kv = kv__prev1;

                    }

                    ptr<IndexAddr> iaddr = addr(new IndexAddr(X:array,Index:idx,));
                    iaddr.setType(types.NewPointer(at.Elem()));
                    fn.emit(iaddr);
                    if (t != at) { // slice
                        // backing array is unaliased => storebuf not needed.
                        b.assign(fn, addr(new address(addr:iaddr,pos:pos,expr:e)), e, true, null);

                    }
                    else
 {
                        b.assign(fn, addr(new address(addr:iaddr,pos:pos,expr:e)), e, true, sb);
                    }

                }

                e = e__prev1;
            }

            if (t != at) { // slice
                ptr<Slice> s = addr(new Slice(X:array));
                s.setPos(e.Lbrace);
                s.setType(typ);
                sb.store(addr(new address(addr:addr,pos:e.Lbrace,expr:e)), fn.emit(s));

            }

            break;
        case ptr<types.Map> t:
            ptr<MakeMap> m = addr(new MakeMap(Reserve:intConst(int64(len(e.Elts)))));
            m.setPos(e.Lbrace);
            m.setType(typ);
            fn.emit(m);
            {
                var e__prev1 = e;

                foreach (var (_, __e) in e.Elts) {
                    e = __e;
                    ptr<ast.KeyValueExpr> e = e._<ptr<ast.KeyValueExpr>>(); 

                    // If a key expression in a map literal is itself a
                    // composite literal, the type may be omitted.
                    // For example:
                    //    map[*struct{}]bool{{}: true}
                    // An &-operation may be implied:
                    //    map[*struct{}]bool{&struct{}{}: true}
                    Value key = default;
                    {
                        ptr<ast.CompositeLit> (_, ok) = unparen(e.Key)._<ptr<ast.CompositeLit>>();

                        if (ok && isPointer(t.Key())) { 
                            // A CompositeLit never evaluates to a pointer,
                            // so if the type of the location is a pointer,
                            // an &-operation is implied.
                            key = b.addr(fn, e.Key, true).address(fn);

                        }
                        else
 {
                            key = b.expr(fn, e.Key);
                        }

                    }


                    ref element loc = ref heap(new element(m:m,k:emitConv(fn,key,t.Key()),t:t.Elem(),pos:e.Colon,), out ptr<element> _addr_loc); 

                    // We call assign() only because it takes care
                    // of any &-operation required in the recursive
                    // case, e.g.,
                    // map[int]*struct{}{0: {}} implies &struct{}{}.
                    // In-place update is of course impossible,
                    // and no storebuf is needed.
                    b.assign(fn, _addr_loc, e.Value, true, null);

                }

                e = e__prev1;
            }

            sb.store(addr(new address(addr:addr,pos:e.Lbrace,expr:e)), m);
            break;
        default:
        {
            var t = typ.Underlying().type();
            panic("unexpected CompositeLit type: " + t.String());
            break;
        }
    }

});

// switchStmt emits to fn code for the switch statement s, optionally
// labelled by label.
//
private static void switchStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.SwitchStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.SwitchStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    // We treat SwitchStmt like a sequential if-else chain.
    // Multiway dispatch can be recovered later by ssautil.Switches()
    // to those cases that are free of side effects.
    if (s.Init != null) {
        b.stmt(fn, s.Init);
    }
    Value tag = vTrue;
    if (s.Tag != null) {
        tag = b.expr(fn, s.Tag);
    }
    var done = fn.newBasicBlock("switch.done");
    if (label != null) {
        label._break = done;
    }
    ptr<slice<ast.Stmt>> dfltBody;
    ptr<BasicBlock> dfltFallthrough;
    ptr<BasicBlock> fallthru;    ptr<BasicBlock> dfltBlock;

    var ncases = len(s.Body.List);
    foreach (var (i, clause) in s.Body.List) {
        var body = fallthru;
        if (body == null) {
            body = fn.newBasicBlock("switch.body"); // first case only
        }
        fallthru = done;
        if (i + 1 < ncases) {
            fallthru = fn.newBasicBlock("switch.body");
        }
        ptr<ast.CaseClause> cc = clause._<ptr<ast.CaseClause>>();
        if (cc.List == null) { 
            // Default case.
            dfltBody = _addr_cc.Body;
            dfltFallthrough = addr(fallthru);
            dfltBlock = body;
            continue;

        }
        ptr<BasicBlock> nextCond;
        {
            var cond__prev2 = cond;

            foreach (var (_, __cond) in cc.List) {
                cond = __cond;
                nextCond = fn.newBasicBlock("switch.next"); 
                // TODO(adonovan): opt: when tag==vTrue, we'd
                // get better code if we use b.cond(cond)
                // instead of BinOp(EQL, tag, b.expr(cond))
                // followed by If.  Don't forget conversions
                // though.
                var cond = emitCompare(fn, token.EQL, tag, b.expr(fn, cond), token.NoPos);
                emitIf(fn, cond, body, nextCond);
                fn.currentBlock = nextCond;

            }

            cond = cond__prev2;
        }

        fn.currentBlock = body;
        fn.targets = addr(new targets(tail:fn.targets,_break:done,_fallthrough:fallthru,));
        b.stmtList(fn, cc.Body);
        fn.targets = fn.targets.tail;
        emitJump(fn, done);
        fn.currentBlock = nextCond;

    }    if (dfltBlock != null) {
        emitJump(fn, dfltBlock);
        fn.currentBlock = dfltBlock;
        fn.targets = addr(new targets(tail:fn.targets,_break:done,_fallthrough:dfltFallthrough,));
        b.stmtList(fn, dfltBody.val);
        fn.targets = fn.targets.tail;
    }
    emitJump(fn, done);
    fn.currentBlock = done;

}

// typeSwitchStmt emits to fn code for the type switch statement s, optionally
// labelled by label.
//
private static void typeSwitchStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.TypeSwitchStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.TypeSwitchStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    // We treat TypeSwitchStmt like a sequential if-else chain.
    // Multiway dispatch can be recovered later by ssautil.Switches().

    // Typeswitch lowering:
    //
    // var x X
    // switch y := x.(type) {
    // case T1, T2: S1                  // >1     (y := x)
    // case nil:    SN                  // nil     (y := x)
    // default:     SD                  // 0 types     (y := x)
    // case T3:     S3                  // 1 type     (y := x.(T3))
    // }
    //
    //      ...s.Init...
    //     x := eval x
    // .caseT1:
    //     t1, ok1 := typeswitch,ok x <T1>
    //     if ok1 then goto S1 else goto .caseT2
    // .caseT2:
    //     t2, ok2 := typeswitch,ok x <T2>
    //     if ok2 then goto S1 else goto .caseNil
    // .S1:
    //      y := x
    //     ...S1...
    //     goto done
    // .caseNil:
    //     if t2, ok2 := typeswitch,ok x <T2>
    //     if x == nil then goto SN else goto .caseT3
    // .SN:
    //      y := x
    //     ...SN...
    //     goto done
    // .caseT3:
    //     t3, ok3 := typeswitch,ok x <T3>
    //     if ok3 then goto S3 else goto default
    // .S3:
    //      y := t3
    //     ...S3...
    //     goto done
    // .default:
    //      y := x
    //     ...SD...
    //     goto done
    // .done:

    if (s.Init != null) {
        b.stmt(fn, s.Init);
    }
    Value x = default;
    switch (s.Assign.type()) {
        case ptr<ast.ExprStmt> ass:
            x = b.expr(fn, unparen(ass.X)._<ptr<ast.TypeAssertExpr>>().X);
            break;
        case ptr<ast.AssignStmt> ass:
            x = b.expr(fn, unparen(ass.Rhs[0])._<ptr<ast.TypeAssertExpr>>().X);
            break;

    }

    var done = fn.newBasicBlock("typeswitch.done");
    if (label != null) {
        label._break = done;
    }
    ptr<ast.CaseClause> default_;
    foreach (var (_, clause) in s.Body.List) {
        ptr<ast.CaseClause> cc = clause._<ptr<ast.CaseClause>>();
        if (cc.List == null) {
            default_ = addr(cc);
            continue;
        }
        var body = fn.newBasicBlock("typeswitch.body");
        ptr<BasicBlock> next;
        types.Type casetype = default;
        Value ti = default; // ti, ok := typeassert,ok x <Ti>
        foreach (var (_, cond) in cc.List) {
            next = fn.newBasicBlock("typeswitch.next");
            casetype = fn.Pkg.typeOf(cond);
            Value condv = default;
            if (casetype == tUntypedNil) {
                condv = emitCompare(fn, token.EQL, x, nilConst(x.Type()), token.NoPos);
                ti = x;
            }
            else
 {
                var yok = emitTypeTest(fn, x, casetype, cc.Case);
                ti = emitExtract(fn, yok, 0);
                condv = emitExtract(fn, yok, 1);
            }

            emitIf(fn, condv, body, next);
            fn.currentBlock = next;

        }        if (len(cc.List) != 1) {
            ti = x;
        }
        fn.currentBlock = body;
        b.typeCaseBody(fn, cc, ti, done);
        fn.currentBlock = next;

    }    if (default_ != null) {
        b.typeCaseBody(fn, default_, x, done);
    }
    else
 {
        emitJump(fn, done);
    }
    fn.currentBlock = done;

}

private static void typeCaseBody(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.CaseClause> _addr_cc, Value x, ptr<BasicBlock> _addr_done) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.CaseClause cc = ref _addr_cc.val;
    ref BasicBlock done = ref _addr_done.val;

    {
        var obj = fn.Pkg.info.Implicits[cc];

        if (obj != null) { 
            // In a switch y := x.(type), each case clause
            // implicitly declares a distinct object y.
            // In a single-type case, y has that type.
            // In multi-type cases, 'case nil' and default,
            // y has the same type as the interface operand.
            emitStore(fn, fn.addNamedLocal(obj), x, obj.Pos());

        }
    }

    fn.targets = addr(new targets(tail:fn.targets,_break:done,));
    b.stmtList(fn, cc.Body);
    fn.targets = fn.targets.tail;
    emitJump(fn, done);

}

// selectStmt emits to fn code for the select statement s, optionally
// labelled by label.
//
private static void selectStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.SelectStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.SelectStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    // A blocking select of a single case degenerates to a
    // simple send or receive.
    // TODO(adonovan): opt: is this optimization worth its weight?
    if (len(s.Body.List) == 1) {
        ptr<ast.CommClause> clause = s.Body.List[0]._<ptr<ast.CommClause>>();
        if (clause.Comm != null) {
            b.stmt(fn, clause.Comm);
            var done = fn.newBasicBlock("select.done");
            if (label != null) {
                label._break = done;
            }
            fn.targets = addr(new targets(tail:fn.targets,_break:done,));
            b.stmtList(fn, clause.Body);
            fn.targets = fn.targets.tail;
            emitJump(fn, done);
            fn.currentBlock = done;
            return ;
        }
    }
    slice<ptr<SelectState>> states = default;
    var blocking = true;
    var debugInfo = fn.debugInfo();
    {
        ptr<ast.CommClause> clause__prev1 = clause;

        foreach (var (_, __clause) in s.Body.List) {
            clause = __clause;
            ptr<SelectState> st;
            switch (clause._<ptr<ast.CommClause>>().Comm.type()) {
                case 
                    blocking = false;
                    continue;
                    break;
                case ptr<ast.SendStmt> comm:
                    var ch = b.expr(fn, comm.Chan);
                    st = addr(new SelectState(Dir:types.SendOnly,Chan:ch,Send:emitConv(fn,b.expr(fn,comm.Value),ch.Type().Underlying().(*types.Chan).Elem()),Pos:comm.Arrow,));
                    if (debugInfo) {
                        st.DebugNode = comm;
                    }
                    break;
                case ptr<ast.AssignStmt> comm:
                    ptr<ast.UnaryExpr> recv = unparen(comm.Rhs[0])._<ptr<ast.UnaryExpr>>();
                    st = addr(new SelectState(Dir:types.RecvOnly,Chan:b.expr(fn,recv.X),Pos:recv.OpPos,));
                    if (debugInfo) {
                        st.DebugNode = recv;
                    }
                    break;
                case ptr<ast.ExprStmt> comm:
                    recv = unparen(comm.X)._<ptr<ast.UnaryExpr>>();
                    st = addr(new SelectState(Dir:types.RecvOnly,Chan:b.expr(fn,recv.X),Pos:recv.OpPos,));
                    if (debugInfo) {
                        st.DebugNode = recv;
                    }
                    break;
            }
            states = append(states, st);

        }
        clause = clause__prev1;
    }

    ptr<Select> sel = addr(new Select(States:states,Blocking:blocking,));
    sel.setPos(s.Select);
    slice<ptr<types.Var>> vars = default;
    vars = append(vars, varIndex, varOk);
    {
        ptr<SelectState> st__prev1 = st;

        foreach (var (_, __st) in states) {
            st = __st;
            if (st.Dir == types.RecvOnly) {
                ptr<types.Chan> tElem = st.Chan.Type().Underlying()._<ptr<types.Chan>>().Elem();
                vars = append(vars, anonVar(tElem));
            }
        }
        st = st__prev1;
    }

    sel.setType(types.NewTuple(vars));

    fn.emit(sel);
    var idx = emitExtract(fn, sel, 0);

    done = fn.newBasicBlock("select.done");
    if (label != null) {
        label._break = done;
    }
    ptr<slice<ast.Stmt>> defaultBody;
    nint state = 0;
    nint r = 2; // index in 'sel' tuple of value; increments if st.Dir==RECV
    foreach (var (_, cc) in s.Body.List) {
        clause = cc._<ptr<ast.CommClause>>();
        if (clause.Comm == null) {
            defaultBody = _addr_clause.Body;
            continue;
        }
        var body = fn.newBasicBlock("select.body");
        var next = fn.newBasicBlock("select.next");
        emitIf(fn, emitCompare(fn, token.EQL, idx, intConst(int64(state)), token.NoPos), body, next);
        fn.currentBlock = body;
        fn.targets = addr(new targets(tail:fn.targets,_break:done,));
        switch (clause.Comm.type()) {
            case ptr<ast.ExprStmt> comm:
                if (debugInfo) {
                    var v = emitExtract(fn, sel, r);
                    emitDebugRef(fn, states[state].DebugNode._<ast.Expr>(), v, false);
                }
                r++;
                break;
            case ptr<ast.AssignStmt> comm:
                if (comm.Tok == token.DEFINE) {
                    fn.addLocalForIdent(comm.Lhs[0]._<ptr<ast.Ident>>());
                }
                var x = b.addr(fn, comm.Lhs[0], false); // non-escaping
                v = emitExtract(fn, sel, r);
                if (debugInfo) {
                    emitDebugRef(fn, states[state].DebugNode._<ast.Expr>(), v, false);
                }

                x.store(fn, v);

                if (len(comm.Lhs) == 2) { // x, ok := ...
                    if (comm.Tok == token.DEFINE) {
                        fn.addLocalForIdent(comm.Lhs[1]._<ptr<ast.Ident>>());
                    }

                    var ok = b.addr(fn, comm.Lhs[1], false); // non-escaping
                    ok.store(fn, emitExtract(fn, sel, 1));

                }

                r++;
                break;
        }
        b.stmtList(fn, clause.Body);
        fn.targets = fn.targets.tail;
        emitJump(fn, done);
        fn.currentBlock = next;
        state++;

    }    if (defaultBody != null) {
        fn.targets = addr(new targets(tail:fn.targets,_break:done,));
        b.stmtList(fn, defaultBody.val);
        fn.targets = fn.targets.tail;
    }
    else
 { 
        // A blocking select must match some case.
        // (This should really be a runtime.errorString, not a string.)
        fn.emit(addr(new Panic(X:emitConv(fn,stringConst("blocking select matched no case"),tEface),)));
        fn.currentBlock = fn.newBasicBlock("unreachable");

    }
    emitJump(fn, done);
    fn.currentBlock = done;

}

// forStmt emits to fn code for the for statement s, optionally
// labelled by label.
//
private static void forStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.ForStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.ForStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    //    ...init...
    //      jump loop
    // loop:
    //      if cond goto body else done
    // body:
    //      ...body...
    //      jump post
    // post:                 (target of continue)
    //      ...post...
    //      jump loop
    // done:                                 (target of break)
    if (s.Init != null) {
        b.stmt(fn, s.Init);
    }
    var body = fn.newBasicBlock("for.body");
    var done = fn.newBasicBlock("for.done"); // target of 'break'
    var loop = body; // target of back-edge
    if (s.Cond != null) {
        loop = fn.newBasicBlock("for.loop");
    }
    var cont = loop; // target of 'continue'
    if (s.Post != null) {
        cont = fn.newBasicBlock("for.post");
    }
    if (label != null) {
        label._break = done;
        label._continue = cont;
    }
    emitJump(fn, loop);
    fn.currentBlock = loop;
    if (loop != body) {
        b.cond(fn, s.Cond, body, done);
        fn.currentBlock = body;
    }
    fn.targets = addr(new targets(tail:fn.targets,_break:done,_continue:cont,));
    b.stmt(fn, s.Body);
    fn.targets = fn.targets.tail;
    emitJump(fn, cont);

    if (s.Post != null) {
        fn.currentBlock = cont;
        b.stmt(fn, s.Post);
        emitJump(fn, loop); // back-edge
    }
    fn.currentBlock = done;

}

// rangeIndexed emits to fn the header for an integer-indexed loop
// over array, *array or slice value x.
// The v result is defined only if tv is non-nil.
// forPos is the position of the "for" token.
//
private static (Value, Value, ptr<BasicBlock>, ptr<BasicBlock>) rangeIndexed(this ptr<builder> _addr_b, ptr<Function> _addr_fn, Value x, types.Type tv, token.Pos pos) => func((_, panic, _) => {
    Value k = default;
    Value v = default;
    ptr<BasicBlock> loop = default!;
    ptr<BasicBlock> done = default!;
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
 
    //
    //      length = len(x)
    //      index = -1
    // loop:                                   (target of continue)
    //      index++
    //     if index < length goto body else done
    // body:
    //      k = index
    //      v = x[index]
    //      ...body...
    //     jump loop
    // done:                                   (target of break)

    // Determine number of iterations.
    Value length = default;
    {
        ptr<types.Array> (arr, ok) = deref(x.Type()).Underlying()._<ptr<types.Array>>();

        if (ok) { 
            // For array or *array, the number of iterations is
            // known statically thanks to the type.  We avoid a
            // data dependence upon x, permitting later dead-code
            // elimination if x is pure, static unrolling, etc.
            // Ranging over a nil *array may have >0 iterations.
            // We still generate code for x, in case it has effects.
            length = intConst(arr.Len());

        }
        else
 { 
            // length = len(x).
            ref Call c = ref heap(out ptr<Call> _addr_c);
            c.Call.Value = makeLen(x.Type());
            c.Call.Args = new slice<Value>(new Value[] { x });
            c.setType(tInt);
            length = fn.emit(_addr_c);

        }
    }


    var index = fn.addLocal(tInt, token.NoPos);
    emitStore(fn, index, intConst(-1), pos);

    loop = fn.newBasicBlock("rangeindex.loop");
    emitJump(fn, loop);
    fn.currentBlock = loop;

    ptr<BinOp> incr = addr(new BinOp(Op:token.ADD,X:emitLoad(fn,index),Y:vOne,));
    incr.setType(tInt);
    emitStore(fn, index, fn.emit(incr), pos);

    var body = fn.newBasicBlock("rangeindex.body");
    done = fn.newBasicBlock("rangeindex.done");
    emitIf(fn, emitCompare(fn, token.LSS, incr, length, token.NoPos), body, done);
    fn.currentBlock = body;

    k = emitLoad(fn, index);
    if (tv != null) {
        switch (x.Type().Underlying().type()) {
            case ptr<types.Array> t:
                ptr<Index> instr = addr(new Index(X:x,Index:k,));
                instr.setType(t.Elem());
                instr.setPos(x.Pos());
                v = fn.emit(instr);
                break;
            case ptr<types.Pointer> t:
                instr = addr(new IndexAddr(X:x,Index:k,));
                instr.setType(types.NewPointer(t.Elem().Underlying()._<ptr<types.Array>>().Elem()));
                instr.setPos(x.Pos());
                v = emitLoad(fn, fn.emit(instr));
                break;
            case ptr<types.Slice> t:
                instr = addr(new IndexAddr(X:x,Index:k,));
                instr.setType(types.NewPointer(t.Elem()));
                instr.setPos(x.Pos());
                v = emitLoad(fn, fn.emit(instr));
                break;
            default:
            {
                var t = x.Type().Underlying().type();
                panic("rangeIndexed x:" + t.String());
                break;
            }
        }

    }
    return ;

});

// rangeIter emits to fn the header for a loop using
// Range/Next/Extract to iterate over map or string value x.
// tk and tv are the types of the key/value results k and v, or nil
// if the respective component is not wanted.
//
private static (Value, Value, ptr<BasicBlock>, ptr<BasicBlock>) rangeIter(this ptr<builder> _addr_b, ptr<Function> _addr_fn, Value x, types.Type tk, types.Type tv, token.Pos pos) {
    Value k = default;
    Value v = default;
    ptr<BasicBlock> loop = default!;
    ptr<BasicBlock> done = default!;
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
 
    //
    //    it = range x
    // loop:                                   (target of continue)
    //    okv = next it                      (ok, key, value)
    //      ok = extract okv #0
    //     if ok goto body else done
    // body:
    //     k = extract okv #1
    //     v = extract okv #2
    //      ...body...
    //     jump loop
    // done:                                   (target of break)
    //

    if (tk == null) {
        tk = tInvalid;
    }
    if (tv == null) {
        tv = tInvalid;
    }
    ptr<Range> rng = addr(new Range(X:x));
    rng.setPos(pos);
    rng.setType(tRangeIter);
    var it = fn.emit(rng);

    loop = fn.newBasicBlock("rangeiter.loop");
    emitJump(fn, loop);
    fn.currentBlock = loop;

    ptr<types.Basic> (_, isString) = x.Type().Underlying()._<ptr<types.Basic>>();

    ptr<Next> okv = addr(new Next(Iter:it,IsString:isString,));
    okv.setType(types.NewTuple(varOk, newVar("k", tk), newVar("v", tv)));
    fn.emit(okv);

    var body = fn.newBasicBlock("rangeiter.body");
    done = fn.newBasicBlock("rangeiter.done");
    emitIf(fn, emitExtract(fn, okv, 0), body, done);
    fn.currentBlock = body;

    if (tk != tInvalid) {
        k = emitExtract(fn, okv, 1);
    }
    if (tv != tInvalid) {
        v = emitExtract(fn, okv, 2);
    }
    return ;

}

// rangeChan emits to fn the header for a loop that receives from
// channel x until it fails.
// tk is the channel's element type, or nil if the k result is
// not wanted
// pos is the position of the '=' or ':=' token.
//
private static (Value, ptr<BasicBlock>, ptr<BasicBlock>) rangeChan(this ptr<builder> _addr_b, ptr<Function> _addr_fn, Value x, types.Type tk, token.Pos pos) {
    Value k = default;
    ptr<BasicBlock> loop = default!;
    ptr<BasicBlock> done = default!;
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
 
    //
    // loop:                                   (target of continue)
    //      ko = <-x                           (key, ok)
    //      ok = extract ko #1
    //      if ok goto body else done
    // body:
    //      k = extract ko #0
    //      ...
    //      goto loop
    // done:                                   (target of break)

    loop = fn.newBasicBlock("rangechan.loop");
    emitJump(fn, loop);
    fn.currentBlock = loop;
    ptr<UnOp> recv = addr(new UnOp(Op:token.ARROW,X:x,CommaOk:true,));
    recv.setPos(pos);
    recv.setType(types.NewTuple(newVar("k", x.Type().Underlying()._<ptr<types.Chan>>().Elem()), varOk));
    var ko = fn.emit(recv);
    var body = fn.newBasicBlock("rangechan.body");
    done = fn.newBasicBlock("rangechan.done");
    emitIf(fn, emitExtract(fn, ko, 1), body, done);
    fn.currentBlock = body;
    if (tk != null) {
        k = emitExtract(fn, ko, 0);
    }
    return ;

}

// rangeStmt emits to fn code for the range statement s, optionally
// labelled by label.
//
private static void rangeStmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ptr<ast.RangeStmt> _addr_s, ptr<lblock> _addr_label) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
    ref ast.RangeStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;

    types.Type tk = default;    types.Type tv = default;

    if (s.Key != null && !isBlankIdent(s.Key)) {
        tk = fn.Pkg.typeOf(s.Key);
    }
    if (s.Value != null && !isBlankIdent(s.Value)) {
        tv = fn.Pkg.typeOf(s.Value);
    }
    if (s.Tok == token.DEFINE) {
        if (tk != null) {
            fn.addLocalForIdent(s.Key._<ptr<ast.Ident>>());
        }
        if (tv != null) {
            fn.addLocalForIdent(s.Value._<ptr<ast.Ident>>());
        }
    }
    var x = b.expr(fn, s.X);

    Value k = default;    Value v = default;

    ptr<BasicBlock> loop;    ptr<BasicBlock> done;

    switch (x.Type().Underlying().type()) {
        case ptr<types.Slice> rt:
            k, v, loop, done = b.rangeIndexed(fn, x, tv, s.For);
            break;
        case ptr<types.Array> rt:
            k, v, loop, done = b.rangeIndexed(fn, x, tv, s.For);
            break;
        case ptr<types.Pointer> rt:
            k, v, loop, done = b.rangeIndexed(fn, x, tv, s.For);
            break;
        case ptr<types.Chan> rt:
            k, loop, done = b.rangeChan(fn, x, tk, s.For);
            break;
        case ptr<types.Map> rt:
            k, v, loop, done = b.rangeIter(fn, x, tk, tv, s.For);
            break;
        case ptr<types.Basic> rt:
            k, v, loop, done = b.rangeIter(fn, x, tk, tv, s.For);
            break;
        default:
        {
            var rt = x.Type().Underlying().type();
            panic("Cannot range over: " + rt.String());
            break;
        } 

        // Evaluate both LHS expressions before we update either.
    } 

    // Evaluate both LHS expressions before we update either.
    lvalue kl = default;    lvalue vl = default;

    if (tk != null) {
        kl = b.addr(fn, s.Key, false); // non-escaping
    }
    if (tv != null) {
        vl = b.addr(fn, s.Value, false); // non-escaping
    }
    if (tk != null) {
        kl.store(fn, k);
    }
    if (tv != null) {
        vl.store(fn, v);
    }
    if (label != null) {
        label._break = done;
        label._continue = loop;
    }
    fn.targets = addr(new targets(tail:fn.targets,_break:done,_continue:loop,));
    b.stmt(fn, s.Body);
    fn.targets = fn.targets.tail;
    emitJump(fn, loop); // back-edge
    fn.currentBlock = done;

});

// stmt lowers statement s to SSA form, emitting code to fn.
private static void stmt(this ptr<builder> _addr_b, ptr<Function> _addr_fn, ast.Stmt _s) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;
 
    // The label of the current statement.  If non-nil, its _goto
    // target is always set; its _break and _continue are set only
    // within the body of switch/typeswitch/select/for/range.
    // It is effectively an additional default-nil parameter of stmt().
    ptr<lblock> label;
start:
    switch (_s.type()) {
        case ptr<ast.EmptyStmt> s:
            break;
        case ptr<ast.DeclStmt> s:
            ptr<ast.GenDecl> d = s.Decl._<ptr<ast.GenDecl>>();
            if (d.Tok == token.VAR) {
                foreach (var (_, spec) in d.Specs) {
                    {
                        ptr<ast.ValueSpec> (vs, ok) = spec._<ptr<ast.ValueSpec>>();

                        if (ok) {
                            b.localValueSpec(fn, vs);
                        }

                    }

                }

            }

            break;
        case ptr<ast.LabeledStmt> s:
            label = fn.labelledBlock(s.Label);
            emitJump(fn, label._goto);
            fn.currentBlock = label._goto;
            _s = s.Stmt;
            goto start; // effectively: tailcall stmt(fn, s.Stmt, label)
            break;
        case ptr<ast.ExprStmt> s:
            b.expr(fn, s.X);
            break;
        case ptr<ast.SendStmt> s:
            fn.emit(addr(new Send(Chan:b.expr(fn,s.Chan),X:emitConv(fn,b.expr(fn,s.Value),fn.Pkg.typeOf(s.Chan).Underlying().(*types.Chan).Elem()),pos:s.Arrow,)));
            break;
        case ptr<ast.IncDecStmt> s:
            var op = token.ADD;
            if (s.Tok == token.DEC) {
                op = token.SUB;
            }
            var loc = b.addr(fn, s.X, false);
            b.assignOp(fn, loc, NewConst(constant.MakeInt64(1), loc.typ()), op, s.Pos());
            break;
        case ptr<ast.AssignStmt> s:

            if (s.Tok == token.ASSIGN || s.Tok == token.DEFINE) 
                b.assignStmt(fn, s.Lhs, s.Rhs, s.Tok == token.DEFINE);
            else // +=, etc.
                op = s.Tok + token.ADD - token.ADD_ASSIGN;
                b.assignOp(fn, b.addr(fn, s.Lhs[0], false), b.expr(fn, s.Rhs[0]), op, s.Pos());
                        break;
        case ptr<ast.GoStmt> s:
            ref Go v = ref heap(new Go(pos:s.Go), out ptr<Go> _addr_v);
            b.setCall(fn, s.Call, _addr_v.Call);
            fn.emit(_addr_v);
            break;
        case ptr<ast.DeferStmt> s:
            v = new Defer(pos:s.Defer);
            b.setCall(fn, s.Call, _addr_v.Call);
            fn.emit(_addr_v); 

            // A deferred call can cause recovery from panic,
            // and control resumes at the Recover block.
            createRecoverBlock(fn);
            break;
        case ptr<ast.ReturnStmt> s:
            slice<Value> results = default;
            if (len(s.Results) == 1 && fn.Signature.Results().Len() > 1) { 
                // Return of one expression in a multi-valued function.
                var tuple = b.exprN(fn, s.Results[0]);
                ptr<types.Tuple> ttuple = tuple.Type()._<ptr<types.Tuple>>();
                {
                    nint i__prev1 = i;

                    for (nint i = 0;
                    var n = ttuple.Len(); i < n; i++) {
                        results = append(results, emitConv(fn, emitExtract(fn, tuple, i), fn.Signature.Results().At(i).Type()));
                    }
            else


                    i = i__prev1;
                }

            } { 
                // 1:1 return, or no-arg return in non-void function.
                {
                    nint i__prev1 = i;
                    var r__prev1 = r;

                    foreach (var (__i, __r) in s.Results) {
                        i = __i;
                        r = __r;
                        v = emitConv(fn, b.expr(fn, r), fn.Signature.Results().At(i).Type());
                        results = append(results, v);
                    }

                    i = i__prev1;
                    r = r__prev1;
                }
            }

            if (fn.namedResults != null) { 
                // Function has named result parameters (NRPs).
                // Perform parallel assignment of return operands to NRPs.
                {
                    nint i__prev1 = i;
                    var r__prev1 = r;

                    foreach (var (__i, __r) in results) {
                        i = __i;
                        r = __r;
                        emitStore(fn, fn.namedResults[i], r, s.Return);
                    }

                    i = i__prev1;
                    r = r__prev1;
                }
            } 
            // Run function calls deferred in this
            // function when explicitly returning from it.
            fn.emit(@new<RunDefers>());
            if (fn.namedResults != null) { 
                // Reload NRPs to form the result tuple.
                results = results[..(int)0];
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in fn.namedResults) {
                        r = __r;
                        results = append(results, emitLoad(fn, r));
                    }

                    r = r__prev1;
                }
            }

            fn.emit(addr(new Return(Results:results,pos:s.Return)));
            fn.currentBlock = fn.newBasicBlock("unreachable");
            break;
        case ptr<ast.BranchStmt> s:
            ptr<BasicBlock> block;

            if (s.Tok == token.BREAK) 
                if (s.Label != null) {
                    block = fn.labelledBlock(s.Label)._break;
                }
                else
 {
                    {
                        var t__prev1 = t;

                        var t = fn.targets;

                        while (t != null && block == null) {
                            block = t._break;
                            t = t.tail;
                        }


                        t = t__prev1;
                    }

                }

            else if (s.Tok == token.CONTINUE) 
                if (s.Label != null) {
                    block = fn.labelledBlock(s.Label)._continue;
                }
                else
 {
                    {
                        var t__prev1 = t;

                        t = fn.targets;

                        while (t != null && block == null) {
                            block = t._continue;
                            t = t.tail;
                        }


                        t = t__prev1;
                    }

                }

            else if (s.Tok == token.FALLTHROUGH) 
                {
                    var t__prev1 = t;

                    t = fn.targets;

                    while (t != null && block == null) {
                        block = t._fallthrough;
                        t = t.tail;
                    }


                    t = t__prev1;
                }
            else if (s.Tok == token.GOTO) 
                block = fn.labelledBlock(s.Label)._goto;
                        emitJump(fn, block);
            fn.currentBlock = fn.newBasicBlock("unreachable");
            break;
        case ptr<ast.BlockStmt> s:
            b.stmtList(fn, s.List);
            break;
        case ptr<ast.IfStmt> s:
            if (s.Init != null) {
                b.stmt(fn, s.Init);
            }
            var then = fn.newBasicBlock("if.then");
            var done = fn.newBasicBlock("if.done");
            var els = done;
            if (s.Else != null) {
                els = fn.newBasicBlock("if.else");
            }
            b.cond(fn, s.Cond, then, els);
            fn.currentBlock = then;
            b.stmt(fn, s.Body);
            emitJump(fn, done);

            if (s.Else != null) {
                fn.currentBlock = els;
                b.stmt(fn, s.Else);
                emitJump(fn, done);
            }
            fn.currentBlock = done;
            break;
        case ptr<ast.SwitchStmt> s:
            b.switchStmt(fn, s, label);
            break;
        case ptr<ast.TypeSwitchStmt> s:
            b.typeSwitchStmt(fn, s, label);
            break;
        case ptr<ast.SelectStmt> s:
            b.selectStmt(fn, s, label);
            break;
        case ptr<ast.ForStmt> s:
            b.forStmt(fn, s, label);
            break;
        case ptr<ast.RangeStmt> s:
            b.rangeStmt(fn, s, label);
            break;
        default:
        {
            var s = _s.type();
            panic(fmt.Sprintf("unexpected statement kind: %T", s));
            break;
        }
    }

});

// buildFunction builds SSA code for the body of function fn.  Idempotent.
private static void buildFunction(this ptr<builder> _addr_b, ptr<Function> _addr_fn) => func((defer, panic, _) => {
    ref builder b = ref _addr_b.val;
    ref Function fn = ref _addr_fn.val;

    if (fn.Blocks != null) {
        return ; // building already started
    }
    ptr<ast.FieldList> recvField;
    ptr<ast.BlockStmt> body;
    ptr<ast.FuncType> functype;
    switch (fn.syntax.type()) {
        case 
            return ; // not a Go source function.  (Synthetic, or from object file.)
            break;
        case ptr<ast.FuncDecl> n:
            functype = n.Type;
            recvField = n.Recv;
            body = n.Body;
            break;
        case ptr<ast.FuncLit> n:
            functype = n.Type;
            body = n.Body;
            break;
        default:
        {
            var n = fn.syntax.type();
            panic(n);
            break;
        }

    }

    if (body == null) { 
        // External function.
        if (fn.Params == null) { 
            // This condition ensures we add a non-empty
            // params list once only, but we may attempt
            // the degenerate empty case repeatedly.
            // TODO(adonovan): opt: don't do that.

            // We set Function.Params even though there is no body
            // code to reference them.  This simplifies clients.
            {
                var recv = fn.Signature.Recv();

                if (recv != null) {
                    fn.addParamObj(recv);
                }

            }

            var @params = fn.Signature.Params();
            {
                var n__prev1 = n;

                for (nint i = 0;
                var n = @params.Len(); i < n; i++) {
                    fn.addParamObj(@params.At(i));
                }


                n = n__prev1;
            }

        }
        return ;

    }
    if (fn.Prog.mode & LogSource != 0) {
        defer(logStack("build function %s @ %s", fn, fn.Prog.Fset.Position(fn.pos))());
    }
    fn.startBody();
    fn.createSyntacticParams(recvField, functype);
    b.stmt(fn, body);
    {
        var cb = fn.currentBlock;

        if (cb != null && (cb == fn.Blocks[0] || cb == fn.Recover || cb.Preds != null)) { 
            // Control fell off the end of the function's body block.
            //
            // Block optimizations eliminate the current block, if
            // unreachable.  It is a builder invariant that
            // if this no-arg return is ill-typed for
            // fn.Signature.Results, this block must be
            // unreachable.  The sanity checker checks this.
            fn.emit(@new<RunDefers>());
            fn.emit(@new<Return>());

        }
    }

    fn.finishBody();

});

// buildFuncDecl builds SSA code for the function or method declared
// by decl in package pkg.
//
private static void buildFuncDecl(this ptr<builder> _addr_b, ptr<Package> _addr_pkg, ptr<ast.FuncDecl> _addr_decl) {
    ref builder b = ref _addr_b.val;
    ref Package pkg = ref _addr_pkg.val;
    ref ast.FuncDecl decl = ref _addr_decl.val;

    var id = decl.Name;
    if (isBlankIdent(id)) {
        return ; // discard
    }
    ptr<Function> fn = pkg.values[pkg.info.Defs[id]]._<ptr<Function>>();
    if (decl.Recv == null && id.Name == "init") {
        ref Call v = ref heap(out ptr<Call> _addr_v);
        v.Call.Value = fn;
        v.setType(types.NewTuple());
        pkg.init.emit(_addr_v);
    }
    b.buildFunction(fn);

}

// Build calls Package.Build for each package in prog.
// Building occurs in parallel unless the BuildSerially mode flag was set.
//
// Build is intended for whole-program analysis; a typical compiler
// need only build a single package.
//
// Build is idempotent and thread-safe.
//
private static void Build(this ptr<Program> _addr_prog) {
    ref Program prog = ref _addr_prog.val;

    sync.WaitGroup wg = default;
    foreach (var (_, p) in prog.packages) {
        if (prog.mode & BuildSerially != 0) {
            p.Build();
        }
        else
 {
            wg.Add(1);
            go_(() => p => {
                p.Build();
                wg.Done();
            }(p));
        }
    }    wg.Wait();

}

// Build builds SSA code for all functions and vars in package p.
//
// Precondition: CreatePackage must have been called for all of p's
// direct imports (and hence its direct imports must have been
// error-free).
//
// Build is idempotent and thread-safe.
//
private static void Build(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    p.buildOnce.Do(p.build);
}

private static void build(this ptr<Package> _addr_p) => func((defer, panic, _) => {
    ref Package p = ref _addr_p.val;

    if (p.info == null) {
        return ; // synthetic package, e.g. "testmain"
    }
    foreach (var (name, mem) in p.Members) {
        if (ast.IsExported(name)) {
            p.Prog.needMethodsOf(mem.Type());
        }
    }    if (p.Prog.mode & LogSource != 0) {
        defer(logStack("build %s", p)());
    }
    var init = p.init;
    init.startBody();

    ptr<BasicBlock> done;

    if (p.Prog.mode & BareInits == 0) { 
        // Make init() skip if package is already initialized.
        var initguard = p.Var("init$guard");
        var doinit = init.newBasicBlock("init.start");
        done = init.newBasicBlock("init.done");
        emitIf(init, emitLoad(init, initguard), done, doinit);
        init.currentBlock = doinit;
        emitStore(init, initguard, vTrue, token.NoPos); 

        // Call the init() function of each package we import.
        foreach (var (_, pkg) in p.Pkg.Imports()) {
            var prereq = p.Prog.packages[pkg];
            if (prereq == null) {
                panic(fmt.Sprintf("Package(%q).Build(): unsatisfied import: Program.CreatePackage(%q) was not called", p.Pkg.Path(), pkg.Path()));
            }
            ref Call v = ref heap(out ptr<Call> _addr_v);
            v.Call.Value = prereq.init;
            v.Call.pos = init.pos;
            v.setType(types.NewTuple());
            init.emit(_addr_v);
        }
    }
    builder b = default; 

    // Initialize package-level vars in correct order.
    foreach (var (_, varinit) in p.info.InitOrder) {
        if (init.Prog.mode & LogSource != 0) {
            fmt.Fprintf(os.Stderr, "build global initializer %v @ %s\n", varinit.Lhs, p.Prog.Fset.Position(varinit.Rhs.Pos()));
        }
        if (len(varinit.Lhs) == 1) { 
            // 1:1 initialization: var x, y = a(), b()
            lvalue lval = default;
            {
                Call v__prev2 = v;

                v = varinit.Lhs[0];

                if (v.Name() != "_") {
                    lval = addr(new address(addr:p.values[v].(*Global),pos:v.Pos()));
                }
                else
 {
                    lval = new blank();
                }

                v = v__prev2;

            }

            b.assign(init, lval, varinit.Rhs, true, null);

        }
        else
 { 
            // n:1 initialization: var x, y :=  f()
            var tuple = b.exprN(init, varinit.Rhs);
            {
                Call v__prev2 = v;

                foreach (var (__i, __v) in varinit.Lhs) {
                    i = __i;
                    v = __v;
                    if (v.Name() == "_") {
                        continue;
                    }
                    emitStore(init, p.values[v]._<ptr<Global>>(), emitExtract(init, tuple, i), v.Pos());
                }

                v = v__prev2;
            }
        }
    }    foreach (var (_, file) in p.files) {
        {
            var decl__prev2 = decl;

            foreach (var (_, __decl) in file.Decls) {
                decl = __decl;
                {
                    var decl__prev1 = decl;

                    ptr<ast.FuncDecl> (decl, ok) = decl._<ptr<ast.FuncDecl>>();

                    if (ok) {
                        b.buildFuncDecl(p, decl);
                    }

                    decl = decl__prev1;

                }

            }

            decl = decl__prev2;
        }
    }    if (p.Prog.mode & BareInits == 0) {
        emitJump(init, done);
        init.currentBlock = done;
    }
    init.emit(@new<Return>());
    init.finishBody();

    p.info = null; // We no longer need ASTs or go/types deductions.

    if (p.Prog.mode & SanityCheckFunctions != 0) {
        sanityCheckPackage(p);
    }
});

// Like ObjectOf, but panics instead of returning nil.
// Only valid during p's create and build phases.
private static types.Object objectOf(this ptr<Package> _addr_p, ptr<ast.Ident> _addr_id) => func((_, panic, _) => {
    ref Package p = ref _addr_p.val;
    ref ast.Ident id = ref _addr_id.val;

    {
        var o = p.info.ObjectOf(id);

        if (o != null) {
            return o;
        }
    }

    panic(fmt.Sprintf("no types.Object for ast.Ident %s @ %s", id.Name, p.Prog.Fset.Position(id.Pos())));

});

// Like TypeOf, but panics instead of returning nil.
// Only valid during p's create and build phases.
private static types.Type typeOf(this ptr<Package> _addr_p, ast.Expr e) => func((_, panic, _) => {
    ref Package p = ref _addr_p.val;

    {
        var T = p.info.TypeOf(e);

        if (T != null) {
            return T;
        }
    }

    panic(fmt.Sprintf("no type for %T @ %s", e, p.Prog.Fset.Position(e.Pos())));

});

} // end ssa_package
