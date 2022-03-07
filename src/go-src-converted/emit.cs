// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:15 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\emit.go
// Helpers for emitting SSA instructions.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

    // emitNew emits to f a new (heap Alloc) instruction allocating an
    // object of type typ.  pos is the optional source location.
    //
private static ptr<Alloc> emitNew(ptr<Function> _addr_f, types.Type typ, token.Pos pos) {
    ref Function f = ref _addr_f.val;

    ptr<Alloc> v = addr(new Alloc(Heap:true));
    v.setType(types.NewPointer(typ));
    v.setPos(pos);
    f.emit(v);
    return _addr_v!;
}

// emitLoad emits to f an instruction to load the address addr into a
// new temporary, and returns the value so defined.
//
private static ptr<UnOp> emitLoad(ptr<Function> _addr_f, Value addr) {
    ref Function f = ref _addr_f.val;

    ptr<UnOp> v = addr(new UnOp(Op:token.MUL,X:addr));
    v.setType(deref(addr.Type()));
    f.emit(v);
    return _addr_v!;
}

// emitDebugRef emits to f a DebugRef pseudo-instruction associating
// expression e with value v.
//
private static void emitDebugRef(ptr<Function> _addr_f, ast.Expr e, Value v, bool isAddr) => func((_, panic, _) => {
    ref Function f = ref _addr_f.val;

    if (!f.debugInfo()) {
        return ; // debugging not enabled
    }
    if (v == null || e == null) {
        panic("nil");
    }
    types.Object obj = default;
    e = unparen(e);
    {
        ptr<ast.Ident> (id, ok) = e._<ptr<ast.Ident>>();

        if (ok) {
            if (isBlankIdent(id)) {
                return ;
            }
            obj = f.Pkg.objectOf(id);
            switch (obj.type()) {
                case ptr<types.Nil> _:
                    return ;
                    break;
                case ptr<types.Const> _:
                    return ;
                    break;
                case ptr<types.Builtin> _:
                    return ;
                    break;
            }

        }
    }

    f.emit(addr(new DebugRef(X:v,Expr:e,IsAddr:isAddr,object:obj,)));

});

// emitArith emits to f code to compute the binary operation op(x, y)
// where op is an eager shift, logical or arithmetic operation.
// (Use emitCompare() for comparisons and Builder.logicalBinop() for
// non-eager operations.)
//
private static Value emitArith(ptr<Function> _addr_f, token.Token op, Value x, Value y, types.Type t, token.Pos pos) => func((_, panic, _) => {
    ref Function f = ref _addr_f.val;


    if (op == token.SHL || op == token.SHR) 
        x = emitConv(_addr_f, x, t); 
        // y may be signed or an 'untyped' constant.
        // TODO(adonovan): whence signed values?
        {
            ptr<types.Basic> (b, ok) = y.Type().Underlying()._<ptr<types.Basic>>();

            if (ok && b.Info() & types.IsUnsigned == 0) {
                y = emitConv(_addr_f, y, types.Typ[types.Uint64]);
            }

        }


    else if (op == token.ADD || op == token.SUB || op == token.MUL || op == token.QUO || op == token.REM || op == token.AND || op == token.OR || op == token.XOR || op == token.AND_NOT) 
        x = emitConv(_addr_f, x, t);
        y = emitConv(_addr_f, y, t);
    else 
        panic("illegal op in emitArith: " + op.String());
        ptr<BinOp> v = addr(new BinOp(Op:op,X:x,Y:y,));
    v.setPos(pos);
    v.setType(t);
    return f.emit(v);

});

// emitCompare emits to f code compute the boolean result of
// comparison comparison 'x op y'.
//
private static Value emitCompare(ptr<Function> _addr_f, token.Token op, Value x, Value y, token.Pos pos) {
    ref Function f = ref _addr_f.val;

    var xt = x.Type().Underlying();
    var yt = y.Type().Underlying(); 

    // Special case to optimise a tagless SwitchStmt so that
    // these are equivalent
    //   switch { case e: ...}
    //   switch true { case e: ... }
    //   if e==true { ... }
    // even in the case when e's type is an interface.
    // TODO(adonovan): opt: generalise to x==true, false!=y, etc.
    if (x == vTrue && op == token.EQL) {
        {
            var yt__prev2 = yt;

            ptr<types.Basic> (yt, ok) = yt._<ptr<types.Basic>>();

            if (ok && yt.Info() & types.IsBoolean != 0) {
                return y;
            }

            yt = yt__prev2;

        }

    }
    if (types.Identical(xt, yt)) { 
        // no conversion necessary
    }    {
        ptr<types.Interface> (_, ok) = xt._<ptr<types.Interface>>();


        else if (ok) {
            y = emitConv(_addr_f, y, x.Type());
        }        {
            (_, ok) = yt._<ptr<types.Interface>>();


            else if (ok) {
                x = emitConv(_addr_f, x, y.Type());
            }            {
                (_, ok) = x._<ptr<Const>>();


                else if (ok) {
                    x = emitConv(_addr_f, x, y.Type());
                }                {
                    (_, ok) = y._<ptr<Const>>();


                    else if (ok) {
                        y = emitConv(_addr_f, y, x.Type());
                    }
                    else
 { 
                        // other cases, e.g. channels.  No-op.
                    }

                }



            }



        }



    }


    ptr<BinOp> v = addr(new BinOp(Op:op,X:x,Y:y,));
    v.setPos(pos);
    v.setType(tBool);
    return f.emit(v);

}

// isValuePreserving returns true if a conversion from ut_src to
// ut_dst is value-preserving, i.e. just a change of type.
// Precondition: neither argument is a named type.
//
private static bool isValuePreserving(types.Type ut_src, types.Type ut_dst) { 
    // Identical underlying types?
    if (structTypesIdentical(ut_dst, ut_src)) {
        return true;
    }
    switch (ut_dst.type()) {
        case ptr<types.Chan> _:
            ptr<types.Chan> (_, ok) = ut_src._<ptr<types.Chan>>();
            return ok;
            break;
        case ptr<types.Pointer> _:
            (_, ok) = ut_src._<ptr<types.Pointer>>();
            return ok;
            break;
    }
    return false;

}

// emitConv emits to f code to convert Value val to exactly type typ,
// and returns the converted value.  Implicit conversions are required
// by language assignability rules in assignments, parameter passing,
// etc.  Conversions cannot fail dynamically.
//
private static Value emitConv(ptr<Function> _addr_f, Value val, types.Type typ) => func((_, panic, _) => {
    ref Function f = ref _addr_f.val;

    var t_src = val.Type(); 

    // Identical types?  Conversion is a no-op.
    if (types.Identical(t_src, typ)) {
        return val;
    }
    var ut_dst = typ.Underlying();
    var ut_src = t_src.Underlying(); 

    // Just a change of type, but not value or representation?
    if (isValuePreserving(ut_src, ut_dst)) {
        ptr<ChangeType> c = addr(new ChangeType(X:val));
        c.setType(typ);
        return f.emit(c);
    }
    {
        ptr<types.Interface> (_, ok) = ut_dst._<ptr<types.Interface>>();

        if (ok) { 
            // Assignment from one interface type to another?
            {
                (_, ok) = ut_src._<ptr<types.Interface>>();

                if (ok) {
                    c = addr(new ChangeInterface(X:val));
                    c.setType(typ);
                    return f.emit(c);
                } 

                // Untyped nil constant?  Return interface-typed nil constant.

            } 

            // Untyped nil constant?  Return interface-typed nil constant.
            if (ut_src == tUntypedNil) {
                return nilConst(typ);
            } 

            // Convert (non-nil) "untyped" literals to their default type.
            {
                ptr<types.Basic> (t, ok) = ut_src._<ptr<types.Basic>>();

                if (ok && t.Info() & types.IsUntyped != 0) {
                    val = emitConv(_addr_f, val, types.Default(ut_src));
                }

            }


            f.Pkg.Prog.needMethodsOf(val.Type());
            ptr<MakeInterface> mi = addr(new MakeInterface(X:val));
            mi.setType(typ);
            return f.emit(mi);

        }
    } 

    // Conversion of a compile-time constant value?
    {
        ptr<ChangeType> c__prev1 = c;

        ptr<Const> (c, ok) = val._<ptr<Const>>();

        if (ok) {
            {
                (_, ok) = ut_dst._<ptr<types.Basic>>();

                if (ok || c.IsNil()) { 
                    // Conversion of a compile-time constant to
                    // another constant type results in a new
                    // constant of the destination type and
                    // (initially) the same abstract value.
                    // We don't truncate the value yet.
                    return NewConst(c.Value, typ);

                } 

                // We're converting from constant to non-constant type,
                // e.g. string -> []byte/[]rune.

            } 

            // We're converting from constant to non-constant type,
            // e.g. string -> []byte/[]rune.
        }
        c = c__prev1;

    } 

    // A representation-changing conversion?
    // At least one of {ut_src,ut_dst} must be *Basic.
    // (The other may be []byte or []rune.)
    ptr<types.Basic> (_, ok1) = ut_src._<ptr<types.Basic>>();
    ptr<types.Basic> (_, ok2) = ut_dst._<ptr<types.Basic>>();
    if (ok1 || ok2) {
        c = addr(new Convert(X:val));
        c.setType(typ);
        return f.emit(c);
    }
    panic(fmt.Sprintf("in %s: cannot convert %s (%s) to %s", f, val, val.Type(), typ));

});

// emitStore emits to f an instruction to store value val at location
// addr, applying implicit conversions as required by assignability rules.
//
private static ptr<Store> emitStore(ptr<Function> _addr_f, Value addr, Value val, token.Pos pos) {
    ref Function f = ref _addr_f.val;

    ptr<Store> s = addr(new Store(Addr:addr,Val:emitConv(f,val,deref(addr.Type())),pos:pos,));
    f.emit(s);
    return _addr_s!;
}

// emitJump emits to f a jump to target, and updates the control-flow graph.
// Postcondition: f.currentBlock is nil.
//
private static void emitJump(ptr<Function> _addr_f, ptr<BasicBlock> _addr_target) {
    ref Function f = ref _addr_f.val;
    ref BasicBlock target = ref _addr_target.val;

    var b = f.currentBlock;
    b.emit(@new<Jump>());
    addEdge(b, target);
    f.currentBlock = null;
}

// emitIf emits to f a conditional jump to tblock or fblock based on
// cond, and updates the control-flow graph.
// Postcondition: f.currentBlock is nil.
//
private static void emitIf(ptr<Function> _addr_f, Value cond, ptr<BasicBlock> _addr_tblock, ptr<BasicBlock> _addr_fblock) {
    ref Function f = ref _addr_f.val;
    ref BasicBlock tblock = ref _addr_tblock.val;
    ref BasicBlock fblock = ref _addr_fblock.val;

    var b = f.currentBlock;
    b.emit(addr(new If(Cond:cond)));
    addEdge(b, tblock);
    addEdge(b, fblock);
    f.currentBlock = null;
}

// emitExtract emits to f an instruction to extract the index'th
// component of tuple.  It returns the extracted value.
//
private static Value emitExtract(ptr<Function> _addr_f, Value tuple, nint index) {
    ref Function f = ref _addr_f.val;

    ptr<Extract> e = addr(new Extract(Tuple:tuple,Index:index));
    e.setType(tuple.Type()._<ptr<types.Tuple>>().At(index).Type());
    return f.emit(e);
}

// emitTypeAssert emits to f a type assertion value := x.(t) and
// returns the value.  x.Type() must be an interface.
//
private static Value emitTypeAssert(ptr<Function> _addr_f, Value x, types.Type t, token.Pos pos) {
    ref Function f = ref _addr_f.val;

    ptr<TypeAssert> a = addr(new TypeAssert(X:x,AssertedType:t));
    a.setPos(pos);
    a.setType(t);
    return f.emit(a);
}

// emitTypeTest emits to f a type test value,ok := x.(t) and returns
// a (value, ok) tuple.  x.Type() must be an interface.
//
private static Value emitTypeTest(ptr<Function> _addr_f, Value x, types.Type t, token.Pos pos) {
    ref Function f = ref _addr_f.val;

    ptr<TypeAssert> a = addr(new TypeAssert(X:x,AssertedType:t,CommaOk:true,));
    a.setPos(pos);
    a.setType(types.NewTuple(newVar("value", t), varOk));
    return f.emit(a);
}

// emitTailCall emits to f a function call in tail position.  The
// caller is responsible for all fields of 'call' except its type.
// Intended for wrapper methods.
// Precondition: f does/will not use deferred procedure calls.
// Postcondition: f.currentBlock is nil.
//
private static void emitTailCall(ptr<Function> _addr_f, ptr<Call> _addr_call) {
    ref Function f = ref _addr_f.val;
    ref Call call = ref _addr_call.val;

    var tresults = f.Signature.Results();
    var nr = tresults.Len();
    if (nr == 1) {
        call.typ = tresults.At(0).Type();
    }
    else
 {
        call.typ = tresults;
    }
    var tuple = f.emit(call);
    ref Return ret = ref heap(out ptr<Return> _addr_ret);
    switch (nr) {
        case 0: 

            break;
        case 1: 
            ret.Results = new slice<Value>(new Value[] { tuple });
            break;
        default: 
            for (nint i = 0; i < nr; i++) {
                var v = emitExtract(_addr_f, tuple, i); 
                // TODO(adonovan): in principle, this is required:
                //   v = emitConv(f, o.Type, f.Signature.Results[i].Type)
                // but in practice emitTailCall is only used when
                // the types exactly match.
                ret.Results = append(ret.Results, v);

            }

            break;
    }
    f.emit(_addr_ret);
    f.currentBlock = null;

}

// emitImplicitSelections emits to f code to apply the sequence of
// implicit field selections specified by indices to base value v, and
// returns the selected value.
//
// If v is the address of a struct, the result will be the address of
// a field; if it is the value of a struct, the result will be the
// value of a field.
//
private static Value emitImplicitSelections(ptr<Function> _addr_f, Value v, slice<nint> indices) {
    ref Function f = ref _addr_f.val;

    foreach (var (_, index) in indices) {
        ptr<types.Struct> fld = deref(v.Type()).Underlying()._<ptr<types.Struct>>().Field(index);

        if (isPointer(v.Type())) {
            ptr<FieldAddr> instr = addr(new FieldAddr(X:v,Field:index,));
            instr.setType(types.NewPointer(fld.Type()));
            v = f.emit(instr); 
            // Load the field's value iff indirectly embedded.
            if (isPointer(fld.Type())) {
                v = emitLoad(_addr_f, v);
            }

        }
        else
 {
            instr = addr(new Field(X:v,Field:index,));
            instr.setType(fld.Type());
            v = f.emit(instr);
        }
    }    return v;

}

// emitFieldSelection emits to f code to select the index'th field of v.
//
// If wantAddr, the input must be a pointer-to-struct and the result
// will be the field's address; otherwise the result will be the
// field's value.
// Ident id is used for position and debug info.
//
private static Value emitFieldSelection(ptr<Function> _addr_f, Value v, nint index, bool wantAddr, ptr<ast.Ident> _addr_id) {
    ref Function f = ref _addr_f.val;
    ref ast.Ident id = ref _addr_id.val;

    ptr<types.Struct> fld = deref(v.Type()).Underlying()._<ptr<types.Struct>>().Field(index);
    if (isPointer(v.Type())) {
        ptr<FieldAddr> instr = addr(new FieldAddr(X:v,Field:index,));
        instr.setPos(id.Pos());
        instr.setType(types.NewPointer(fld.Type()));
        v = f.emit(instr); 
        // Load the field's value iff we don't want its address.
        if (!wantAddr) {
            v = emitLoad(_addr_f, v);
        }
    }
    else
 {
        instr = addr(new Field(X:v,Field:index,));
        instr.setPos(id.Pos());
        instr.setType(fld.Type());
        v = f.emit(instr);
    }
    emitDebugRef(_addr_f, id, v, wantAddr);
    return v;

}

// zeroValue emits to f code to produce a zero value of type t,
// and returns it.
//
private static Value zeroValue(ptr<Function> _addr_f, types.Type t) {
    ref Function f = ref _addr_f.val;

    switch (t.Underlying().type()) {
        case ptr<types.Struct> _:
            return emitLoad(_addr_f, f.addLocal(t, token.NoPos));
            break;
        case ptr<types.Array> _:
            return emitLoad(_addr_f, f.addLocal(t, token.NoPos));
            break;
        default:
        {
            return zeroConst(t);
            break;
        }
    }

}

// createRecoverBlock emits to f a block of code to return after a
// recovered panic, and sets f.Recover to it.
//
// If f's result parameters are named, the code loads and returns
// their current values, otherwise it returns the zero values of their
// type.
//
// Idempotent.
//
private static void createRecoverBlock(ptr<Function> _addr_f) {
    ref Function f = ref _addr_f.val;

    if (f.Recover != null) {
        return ; // already created
    }
    var saved = f.currentBlock;

    f.Recover = f.newBasicBlock("recover");
    f.currentBlock = f.Recover;

    slice<Value> results = default;
    if (f.namedResults != null) { 
        // Reload NRPs to form value tuple.
        foreach (var (_, r) in f.namedResults) {
            results = append(results, emitLoad(_addr_f, r));
        }
    else
    } {
        var R = f.Signature.Results();
        for (nint i = 0;
        var n = R.Len(); i < n; i++) {
            var T = R.At(i).Type(); 

            // Return zero value of each result type.
            results = append(results, zeroValue(_addr_f, T));

        }

    }
    f.emit(addr(new Return(Results:results)));

    f.currentBlock = saved;

}

} // end ssa_package
