// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflectdata -- go2cs converted at 2022 March 13 06:22:16 UTC
// import "cmd/compile/internal/reflectdata" ==> using reflectdata = go.cmd.compile.@internal.reflectdata_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\reflectdata\alg.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using bits = math.bits_package;
using sort = sort_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;


// isRegularMemory reports whether t can be compared/hashed as regular memory.

using System;
public static partial class reflectdata_package {

private static bool isRegularMemory(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var (a, _) = types.AlgType(t);
    return a == types.AMEM;
}

// eqCanPanic reports whether == on type t could panic (has an interface somewhere).
// t must be comparable.
private static bool eqCanPanic(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (t.Kind() == types.TINTER) 
        return true;
    else if (t.Kind() == types.TARRAY) 
        return eqCanPanic(_addr_t.Elem());
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, f) in t.FieldSlice()) {
            if (!f.Sym.IsBlank() && eqCanPanic(_addr_f.Type)) {
                return true;
            }
        }        return false;
    else 
        return false;
    }

// AlgType returns the fixed-width AMEMxx variants instead of the general
// AMEM kind when possible.
public static types.AlgKind AlgType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var (a, _) = types.AlgType(t);
    if (a == types.AMEM) {
        if (t.Alignment() < int64(@base.Ctxt.Arch.Alignment) && t.Alignment() < t.Width) { 
            // For example, we can't treat [2]int16 as an int32 if int32s require
            // 4-byte alignment. See issue 46283.
            return a;
        }
        switch (t.Width) {
            case 0: 
                return types.AMEM0;
                break;
            case 1: 
                return types.AMEM8;
                break;
            case 2: 
                return types.AMEM16;
                break;
            case 4: 
                return types.AMEM32;
                break;
            case 8: 
                return types.AMEM64;
                break;
            case 16: 
                return types.AMEM128;
                break;
        }
    }
    return a;
}

// genhash returns a symbol which is the closure used to compute
// the hash of a value of type t.
// Note: the generated function must match runtime.typehash exactly.
private static ptr<obj.LSym> genhash(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (AlgType(_addr_t) == types.AMEM0) 
        return _addr_sysClosure("memhash0")!;
    else if (AlgType(_addr_t) == types.AMEM8) 
        return _addr_sysClosure("memhash8")!;
    else if (AlgType(_addr_t) == types.AMEM16) 
        return _addr_sysClosure("memhash16")!;
    else if (AlgType(_addr_t) == types.AMEM32) 
        return _addr_sysClosure("memhash32")!;
    else if (AlgType(_addr_t) == types.AMEM64) 
        return _addr_sysClosure("memhash64")!;
    else if (AlgType(_addr_t) == types.AMEM128) 
        return _addr_sysClosure("memhash128")!;
    else if (AlgType(_addr_t) == types.ASTRING) 
        return _addr_sysClosure("strhash")!;
    else if (AlgType(_addr_t) == types.AINTER) 
        return _addr_sysClosure("interhash")!;
    else if (AlgType(_addr_t) == types.ANILINTER) 
        return _addr_sysClosure("nilinterhash")!;
    else if (AlgType(_addr_t) == types.AFLOAT32) 
        return _addr_sysClosure("f32hash")!;
    else if (AlgType(_addr_t) == types.AFLOAT64) 
        return _addr_sysClosure("f64hash")!;
    else if (AlgType(_addr_t) == types.ACPLX64) 
        return _addr_sysClosure("c64hash")!;
    else if (AlgType(_addr_t) == types.ACPLX128) 
        return _addr_sysClosure("c128hash")!;
    else if (AlgType(_addr_t) == types.AMEM) 
        // For other sizes of plain memory, we build a closure
        // that calls memhash_varlen. The size of the memory is
        // encoded in the first slot of the closure.
        var closure = TypeLinksymLookup(fmt.Sprintf(".hashfunc%d", t.Width));
        if (len(closure.P) > 0) { // already generated
            return _addr_closure!;
        }
        if (memhashvarlen == null) {
            memhashvarlen = typecheck.LookupRuntimeFunc("memhash_varlen");
        }
        nint ot = 0;
        ot = objw.SymPtr(closure, ot, memhashvarlen, 0);
        ot = objw.Uintptr(closure, ot, uint64(t.Width)); // size encoded in closure
        objw.Global(closure, int32(ot), obj.DUPOK | obj.RODATA);
        return _addr_closure!;
    else if (AlgType(_addr_t) == types.ASPECIAL) 
        break;
    else 
        // genhash is only called for types that have equality
        @base.Fatalf("genhash %v", t);
        closure = TypeLinksymPrefix(".hashfunc", t);
    if (len(closure.P) > 0) { // already generated
        return _addr_closure!;
    }

    if (t.Kind() == types.TARRAY) 
        genhash(_addr_t.Elem());
    else if (t.Kind() == types.TSTRUCT) 
        {
            var f__prev1 = f;

            foreach (var (_, __f) in t.FieldSlice()) {
                f = __f;
                genhash(_addr_f.Type);
            }

            f = f__prev1;
        }
        var sym = TypeSymPrefix(".hash", t);
    if (@base.Flag.LowerR != 0) {
        fmt.Printf("genhash %v %v %v\n", closure, sym, t);
    }
    @base.Pos = @base.AutogeneratedPos; // less confusing than end of input
    typecheck.DeclContext = ir.PEXTERN; 

    // func sym(p *T, h uintptr) uintptr
    ptr<ir.Field> args = new slice<ptr<ir.Field>>(new ptr<ir.Field>[] { ir.NewField(base.Pos,typecheck.Lookup("p"),nil,types.NewPtr(t)), ir.NewField(base.Pos,typecheck.Lookup("h"),nil,types.Types[types.TUINTPTR]) });
    ptr<ir.Field> results = new slice<ptr<ir.Field>>(new ptr<ir.Field>[] { ir.NewField(base.Pos,nil,nil,types.Types[types.TUINTPTR]) });
    var tfn = ir.NewFuncType(@base.Pos, null, args, results);

    var fn = typecheck.DeclFunc(sym, tfn);
    var np = ir.AsNode(tfn.Type().Params().Field(0).Nname);
    var nh = ir.AsNode(tfn.Type().Params().Field(1).Nname);


    if (t.Kind() == types.TARRAY) 
        // An array of pure memory would be handled by the
        // standard algorithm, so the element type must not be
        // pure memory.
        var hashel = hashfor(_addr_t.Elem()); 

        // for i := 0; i < nelem; i++
        var ni = typecheck.Temp(types.Types[types.TINT]);
        var init = ir.NewAssignStmt(@base.Pos, ni, ir.NewInt(0));
        var cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, ni, ir.NewInt(t.NumElem()));
        var post = ir.NewAssignStmt(@base.Pos, ni, ir.NewBinaryExpr(@base.Pos, ir.OADD, ni, ir.NewInt(1)));
        var loop = ir.NewForStmt(@base.Pos, null, cond, post, null);
        loop.PtrInit().Append(init); 

        // h = hashel(&p[i], h)
        var call = ir.NewCallExpr(@base.Pos, ir.OCALL, hashel, null);

        var nx = ir.NewIndexExpr(@base.Pos, np, ni);
        nx.SetBounded(true);
        var na = typecheck.NodAddr(nx);
        call.Args.Append(na);
        call.Args.Append(nh);
        loop.Body.Append(ir.NewAssignStmt(@base.Pos, nh, call));

        fn.Body.Append(loop);
    else if (t.Kind() == types.TSTRUCT) 
        // Walk the struct using memhash for runs of AMEM
        // and calling specific hash functions for the others.
        {
            nint i = 0;
            var fields = t.FieldSlice();

            while (i < len(fields)) {
                var f = fields[i]; 

                // Skip blank fields.
                if (f.Sym.IsBlank()) {
                    i++;
                    continue;
                } 

                // Hash non-memory fields with appropriate hash function.
                if (!isRegularMemory(_addr_f.Type)) {
                    hashel = hashfor(_addr_f.Type);
                    call = ir.NewCallExpr(@base.Pos, ir.OCALL, hashel, null);
                    nx = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, np, f.Sym); // TODO: fields from other packages?
                    na = typecheck.NodAddr(nx);
                    call.Args.Append(na);
                    call.Args.Append(nh);
                    fn.Body.Append(ir.NewAssignStmt(@base.Pos, nh, call));
                    i++;
                    continue;
                } 

                // Otherwise, hash a maximal length run of raw memory.
                var (size, next) = memrun(_addr_t, i); 

                // h = hashel(&p.first, size, h)
                hashel = hashmem(_addr_f.Type);
                call = ir.NewCallExpr(@base.Pos, ir.OCALL, hashel, null);
                nx = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, np, f.Sym); // TODO: fields from other packages?
                na = typecheck.NodAddr(nx);
                call.Args.Append(na);
                call.Args.Append(nh);
                call.Args.Append(ir.NewInt(size));
                fn.Body.Append(ir.NewAssignStmt(@base.Pos, nh, call));

                i = next;
            }

        }
        var r = ir.NewReturnStmt(@base.Pos, null);
    r.Results.Append(nh);
    fn.Body.Append(r);

    if (@base.Flag.LowerR != 0) {
        ir.DumpList("genhash body", fn.Body);
    }
    typecheck.FinishFuncBody();

    fn.SetDupok(true);
    typecheck.Func(fn);

    ir.CurFunc = fn;
    typecheck.Stmts(fn.Body);
    ir.CurFunc = null;

    if (@base.Debug.DclStack != 0) {
        types.CheckDclstack();
    }
    fn.SetNilCheckDisabled(true);
    typecheck.Target.Decls = append(typecheck.Target.Decls, fn); 

    // Build closure. It doesn't close over any variables, so
    // it contains just the function pointer.
    objw.SymPtr(closure, 0, fn.Linksym(), 0);
    objw.Global(closure, int32(types.PtrSize), obj.DUPOK | obj.RODATA);

    return _addr_closure!;
}

private static ir.Node hashfor(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    ptr<types.Sym> sym;

    {
        var (a, _) = types.AlgType(t);


        if (a == types.AMEM) 
            @base.Fatalf("hashfor with AMEM type");
        else if (a == types.AINTER) 
            sym = ir.Pkgs.Runtime.Lookup("interhash");
        else if (a == types.ANILINTER) 
            sym = ir.Pkgs.Runtime.Lookup("nilinterhash");
        else if (a == types.ASTRING) 
            sym = ir.Pkgs.Runtime.Lookup("strhash");
        else if (a == types.AFLOAT32) 
            sym = ir.Pkgs.Runtime.Lookup("f32hash");
        else if (a == types.AFLOAT64) 
            sym = ir.Pkgs.Runtime.Lookup("f64hash");
        else if (a == types.ACPLX64) 
            sym = ir.Pkgs.Runtime.Lookup("c64hash");
        else if (a == types.ACPLX128) 
            sym = ir.Pkgs.Runtime.Lookup("c128hash");
        else 
            // Note: the caller of hashfor ensured that this symbol
            // exists and has a body by calling genhash for t.
            sym = TypeSymPrefix(".hash", t);

    } 

    // TODO(austin): This creates an ir.Name with a nil Func.
    var n = typecheck.NewName(sym);
    ir.MarkFunc(n);
    n.SetType(types.NewSignature(types.NoPkg, null, null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.NewPtr(t)), types.NewField(base.Pos,nil,types.Types[types.TUINTPTR]) }), new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.Types[types.TUINTPTR]) })));
    return n;
}

// sysClosure returns a closure which will call the
// given runtime function (with no closed-over variables).
private static ptr<obj.LSym> sysClosure(@string name) {
    var s = typecheck.LookupRuntimeVar(name + "·f");
    if (len(s.P) == 0) {
        var f = typecheck.LookupRuntimeFunc(name);
        objw.SymPtr(s, 0, f, 0);
        objw.Global(s, int32(types.PtrSize), obj.DUPOK | obj.RODATA);
    }
    return _addr_s!;
}

// geneq returns a symbol which is the closure used to compute
// equality for two objects of type t.
private static ptr<obj.LSym> geneq(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (AlgType(_addr_t) == types.ANOEQ) 
        // The runtime will panic if it tries to compare
        // a type with a nil equality function.
        return _addr_null!;
    else if (AlgType(_addr_t) == types.AMEM0) 
        return _addr_sysClosure("memequal0")!;
    else if (AlgType(_addr_t) == types.AMEM8) 
        return _addr_sysClosure("memequal8")!;
    else if (AlgType(_addr_t) == types.AMEM16) 
        return _addr_sysClosure("memequal16")!;
    else if (AlgType(_addr_t) == types.AMEM32) 
        return _addr_sysClosure("memequal32")!;
    else if (AlgType(_addr_t) == types.AMEM64) 
        return _addr_sysClosure("memequal64")!;
    else if (AlgType(_addr_t) == types.AMEM128) 
        return _addr_sysClosure("memequal128")!;
    else if (AlgType(_addr_t) == types.ASTRING) 
        return _addr_sysClosure("strequal")!;
    else if (AlgType(_addr_t) == types.AINTER) 
        return _addr_sysClosure("interequal")!;
    else if (AlgType(_addr_t) == types.ANILINTER) 
        return _addr_sysClosure("nilinterequal")!;
    else if (AlgType(_addr_t) == types.AFLOAT32) 
        return _addr_sysClosure("f32equal")!;
    else if (AlgType(_addr_t) == types.AFLOAT64) 
        return _addr_sysClosure("f64equal")!;
    else if (AlgType(_addr_t) == types.ACPLX64) 
        return _addr_sysClosure("c64equal")!;
    else if (AlgType(_addr_t) == types.ACPLX128) 
        return _addr_sysClosure("c128equal")!;
    else if (AlgType(_addr_t) == types.AMEM) 
        // make equality closure. The size of the type
        // is encoded in the closure.
        var closure = TypeLinksymLookup(fmt.Sprintf(".eqfunc%d", t.Width));
        if (len(closure.P) != 0) {
            return _addr_closure!;
        }
        if (memequalvarlen == null) {
            memequalvarlen = typecheck.LookupRuntimeFunc("memequal_varlen");
        }
        nint ot = 0;
        ot = objw.SymPtr(closure, ot, memequalvarlen, 0);
        ot = objw.Uintptr(closure, ot, uint64(t.Width));
        objw.Global(closure, int32(ot), obj.DUPOK | obj.RODATA);
        return _addr_closure!;
    else if (AlgType(_addr_t) == types.ASPECIAL) 
        break;
        closure = TypeLinksymPrefix(".eqfunc", t);
    if (len(closure.P) > 0) { // already generated
        return _addr_closure!;
    }
    var sym = TypeSymPrefix(".eq", t);
    if (@base.Flag.LowerR != 0) {
        fmt.Printf("geneq %v\n", t);
    }
    @base.Pos = @base.AutogeneratedPos; // less confusing than end of input
    typecheck.DeclContext = ir.PEXTERN; 

    // func sym(p, q *T) bool
    var tfn = ir.NewFuncType(@base.Pos, null, new slice<ptr<ir.Field>>(new ptr<ir.Field>[] { ir.NewField(base.Pos,typecheck.Lookup("p"),nil,types.NewPtr(t)), ir.NewField(base.Pos,typecheck.Lookup("q"),nil,types.NewPtr(t)) }), new slice<ptr<ir.Field>>(new ptr<ir.Field>[] { ir.NewField(base.Pos,typecheck.Lookup("r"),nil,types.Types[types.TBOOL]) }));

    var fn = typecheck.DeclFunc(sym, tfn);
    var np = ir.AsNode(tfn.Type().Params().Field(0).Nname);
    var nq = ir.AsNode(tfn.Type().Params().Field(1).Nname);
    var nr = ir.AsNode(tfn.Type().Results().Field(0).Nname); 

    // Label to jump to if an equality test fails.
    var neq = typecheck.AutoLabel(".neq"); 

    // We reach here only for types that have equality but
    // cannot be handled by the standard algorithms,
    // so t must be either an array or a struct.

    if (t.Kind() == types.TARRAY) 
        var nelem = t.NumElem(); 

        // checkAll generates code to check the equality of all array elements.
        // If unroll is greater than nelem, checkAll generates:
        //
        // if eq(p[0], q[0]) && eq(p[1], q[1]) && ... {
        // } else {
        //   return
        // }
        //
        // And so on.
        //
        // Otherwise it generates:
        //
        // for i := 0; i < nelem; i++ {
        //   if eq(p[i], q[i]) {
        //   } else {
        //     goto neq
        //   }
        // }
        //
        // TODO(josharian): consider doing some loop unrolling
        // for larger nelem as well, processing a few elements at a time in a loop.
        Func<long, bool, Func<ir.Node, ir.Node, ir.Node>, ir.Node> checkAll = (unroll, last, eq) => { 
            // checkIdx generates a node to check for equality at index i.
            Func<ir.Node, ir.Node> checkIdx = i => { 
                // pi := p[i]
                var pi = ir.NewIndexExpr(@base.Pos, np, i);
                pi.SetBounded(true);
                pi.SetType(t.Elem()); 
                // qi := q[i]
                var qi = ir.NewIndexExpr(@base.Pos, nq, i);
                qi.SetBounded(true);
                qi.SetType(t.Elem());
                return _addr_eq(pi, qi)!;
            }
;

            if (nelem <= unroll) {
                if (last) { 
                    // Do last comparison in a different manner.
                    nelem--;
                } 
                // Generate a series of checks.
                {
                    var i__prev1 = i;

                    for (var i = int64(0); i < nelem; i++) { 
                        // if check {} else { goto neq }
                        var nif = ir.NewIfStmt(@base.Pos, checkIdx(ir.NewInt(i)), null, null);
                        nif.Else.Append(ir.NewBranchStmt(@base.Pos, ir.OGOTO, neq));
                        fn.Body.Append(nif);
                    }
            else


                    i = i__prev1;
                }
                if (last) {
                    fn.Body.Append(ir.NewAssignStmt(@base.Pos, nr, checkIdx(ir.NewInt(nelem))));
                }
            } { 
                // Generate a for loop.
                // for i := 0; i < nelem; i++
                i = typecheck.Temp(types.Types[types.TINT]);
                var init = ir.NewAssignStmt(@base.Pos, i, ir.NewInt(0));
                var cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, i, ir.NewInt(nelem));
                var post = ir.NewAssignStmt(@base.Pos, i, ir.NewBinaryExpr(@base.Pos, ir.OADD, i, ir.NewInt(1)));
                var loop = ir.NewForStmt(@base.Pos, null, cond, post, null);
                loop.PtrInit().Append(init); 
                // if eq(pi, qi) {} else { goto neq }
                nif = ir.NewIfStmt(@base.Pos, checkIdx(i), null, null);
                nif.Else.Append(ir.NewBranchStmt(@base.Pos, ir.OGOTO, neq));
                loop.Body.Append(nif);
                fn.Body.Append(loop);
                if (last) {
                    fn.Body.Append(ir.NewAssignStmt(@base.Pos, nr, ir.NewBool(true)));
                }
            }
        };


        if (t.Elem().Kind() == types.TSTRING) 
            // Do two loops. First, check that all the lengths match (cheap).
            // Second, check that all the contents match (expensive).
            // TODO: when the array size is small, unroll the length match checks.
            checkAll(3, false, (pi, qi) => { 
                // Compare lengths.
                var (eqlen, _) = EqString(pi, qi);
                return _addr_eqlen!;
            });
            checkAll(1, true, (pi, qi) => { 
                // Compare contents.
                var (_, eqmem) = EqString(pi, qi);
                return _addr_eqmem!;
            });
        else if (t.Elem().Kind() == types.TFLOAT32 || t.Elem().Kind() == types.TFLOAT64) 
            checkAll(2, true, (pi, qi) => { 
                // p[i] == q[i]
                return _addr_ir.NewBinaryExpr(@base.Pos, ir.OEQ, pi, qi)!;
            }); 
            // TODO: pick apart structs, do them piecemeal too
        else 
            checkAll(1, true, (pi, qi) => { 
                // p[i] == q[i]
                return _addr_ir.NewBinaryExpr(@base.Pos, ir.OEQ, pi, qi)!;
            });
            else if (t.Kind() == types.TSTRUCT) 
        // Build a list of conditions to satisfy.
        // The conditions are a list-of-lists. Conditions are reorderable
        // within each inner list. The outer lists must be evaluated in order.
        slice<slice<ir.Node>> conds = default;
        conds = append(conds, new slice<ir.Node>(new ir.Node[] {  }));
        Action<ir.Node> and = n => {
            i = len(conds) - 1;
            conds[i] = append(conds[i], n);
        }; 

        // Walk the struct using memequal for runs of AMEM
        // and calling specific equality tests for the others.
        {
            var i__prev1 = i;

            i = 0;
            var fields = t.FieldSlice();

            while (i < len(fields)) {
                var f = fields[i]; 

                // Skip blank-named fields.
                if (f.Sym.IsBlank()) {
                    i++;
                    continue;
                } 

                // Compare non-memory fields with field equality.
                if (!isRegularMemory(_addr_f.Type)) {
                    if (eqCanPanic(_addr_f.Type)) { 
                        // Enforce ordering by starting a new set of reorderable conditions.
                        conds = append(conds, new slice<ir.Node>(new ir.Node[] {  }));
                    }
                    var p = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, np, f.Sym);
                    var q = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, nq, f.Sym);

                    if (f.Type.IsString()) 
                        var (eqlen, eqmem) = EqString(p, q);
                        and(eqlen);
                        and(eqmem);
                    else 
                        and(ir.NewBinaryExpr(@base.Pos, ir.OEQ, p, q));
                                        if (eqCanPanic(_addr_f.Type)) { 
                        // Also enforce ordering after something that can panic.
                        conds = append(conds, new slice<ir.Node>(new ir.Node[] {  }));
                    }
                    i++;
                    continue;
                } 

                // Find maximal length run of memory-only fields.
                var (size, next) = memrun(_addr_t, i); 

                // TODO(rsc): All the calls to newname are wrong for
                // cross-package unexported fields.
                {
                    var s = fields[(int)i..(int)next];

                    if (len(s) <= 2) { 
                        // Two or fewer fields: use plain field equality.
                        {
                            var f__prev2 = f;

                            foreach (var (_, __f) in s) {
                                f = __f;
                                and(eqfield(np, nq, _addr_f.Sym));
                            }
                    else

                            f = f__prev2;
                        }
                    } { 
                        // More than two fields: use memequal.
                        and(eqmem(np, nq, _addr_f.Sym, size));
                    }

                }
                i = next;
            } 

            // Sort conditions to put runtime calls last.
            // Preserve the rest of the ordering.


            i = i__prev1;
        } 

        // Sort conditions to put runtime calls last.
        // Preserve the rest of the ordering.
        slice<ir.Node> flatConds = default;
        {
            var c__prev1 = c;

            foreach (var (_, __c) in conds) {
                c = __c;
                Func<ir.Node, bool> isCall = n => _addr_n.Op() == ir.OCALL || n.Op() == ir.OCALLFUNC!;
                sort.SliceStable(c, (i, j) => _addr_!isCall(c[i]) && isCall(c[j])!);
                flatConds = append(flatConds, c);
            }

            c = c__prev1;
        }

        if (len(flatConds) == 0) {
            fn.Body.Append(ir.NewAssignStmt(@base.Pos, nr, ir.NewBool(true)));
        }
        else
 {
            {
                var c__prev1 = c;

                foreach (var (_, __c) in flatConds[..(int)len(flatConds) - 1]) {
                    c = __c; 
                    // if cond {} else { goto neq }
                    var n = ir.NewIfStmt(@base.Pos, c, null, null);
                    n.Else.Append(ir.NewBranchStmt(@base.Pos, ir.OGOTO, neq));
                    fn.Body.Append(n);
                }

                c = c__prev1;
            }

            fn.Body.Append(ir.NewAssignStmt(@base.Pos, nr, flatConds[len(flatConds) - 1]));
        }
    else 
        @base.Fatalf("geneq %v", t);
    // ret:
    //   return
    var ret = typecheck.AutoLabel(".ret");
    fn.Body.Append(ir.NewLabelStmt(@base.Pos, ret));
    fn.Body.Append(ir.NewReturnStmt(@base.Pos, null)); 

    // neq:
    //   r = false
    //   return (or goto ret)
    fn.Body.Append(ir.NewLabelStmt(@base.Pos, neq));
    fn.Body.Append(ir.NewAssignStmt(@base.Pos, nr, ir.NewBool(false)));
    if (eqCanPanic(_addr_t) || anyCall(_addr_fn)) { 
        // Epilogue is large, so share it with the equal case.
        fn.Body.Append(ir.NewBranchStmt(@base.Pos, ir.OGOTO, ret));
    }
    else
 { 
        // Epilogue is small, so don't bother sharing.
        fn.Body.Append(ir.NewReturnStmt(@base.Pos, null));
    }
    if (@base.Flag.LowerR != 0) {
        ir.DumpList("geneq body", fn.Body);
    }
    typecheck.FinishFuncBody();

    fn.SetDupok(true);
    typecheck.Func(fn);

    ir.CurFunc = fn;
    typecheck.Stmts(fn.Body);
    ir.CurFunc = null;

    if (@base.Debug.DclStack != 0) {
        types.CheckDclstack();
    }
    fn.SetNilCheckDisabled(true);
    typecheck.Target.Decls = append(typecheck.Target.Decls, fn); 

    // Generate a closure which points at the function we just generated.
    objw.SymPtr(closure, 0, fn.Linksym(), 0);
    objw.Global(closure, int32(types.PtrSize), obj.DUPOK | obj.RODATA);
    return _addr_closure!;
}

private static bool anyCall(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    return ir.Any(fn, n => { 
        // TODO(rsc): No methods?
        var op = n.Op();
        return op == ir.OCALL || op == ir.OCALLFUNC;
    });
}

// eqfield returns the node
//     p.field == q.field
private static ir.Node eqfield(ir.Node p, ir.Node q, ptr<types.Sym> _addr_field) {
    ref types.Sym field = ref _addr_field.val;

    var nx = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, p, field);
    var ny = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, q, field);
    var ne = ir.NewBinaryExpr(@base.Pos, ir.OEQ, nx, ny);
    return ne;
}

// EqString returns the nodes
//   len(s) == len(t)
// and
//   memequal(s.ptr, t.ptr, len(s))
// which can be used to construct string equality comparison.
// eqlen must be evaluated before eqmem, and shortcircuiting is required.
public static (ptr<ir.BinaryExpr>, ptr<ir.CallExpr>) EqString(ir.Node s, ir.Node t) {
    ptr<ir.BinaryExpr> eqlen = default!;
    ptr<ir.CallExpr> eqmem = default!;

    s = typecheck.Conv(s, types.Types[types.TSTRING]);
    t = typecheck.Conv(t, types.Types[types.TSTRING]);
    var sptr = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, s);
    var tptr = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, t);
    var slen = typecheck.Conv(ir.NewUnaryExpr(@base.Pos, ir.OLEN, s), types.Types[types.TUINTPTR]);
    var tlen = typecheck.Conv(ir.NewUnaryExpr(@base.Pos, ir.OLEN, t), types.Types[types.TUINTPTR]);

    var fn = typecheck.LookupRuntime("memequal");
    fn = typecheck.SubstArgTypes(fn, types.Types[types.TUINT8], types.Types[types.TUINT8]);
    var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, new slice<ir.Node>(new ir.Node[] { sptr, tptr, ir.Copy(slen) }));
    typecheck.Call(call);

    var cmp = ir.NewBinaryExpr(@base.Pos, ir.OEQ, slen, tlen);
    cmp = typecheck.Expr(cmp)._<ptr<ir.BinaryExpr>>();
    cmp.SetType(types.Types[types.TBOOL]);
    return (_addr_cmp!, _addr_call!);
}

// EqInterface returns the nodes
//   s.tab == t.tab (or s.typ == t.typ, as appropriate)
// and
//   ifaceeq(s.tab, s.data, t.data) (or efaceeq(s.typ, s.data, t.data), as appropriate)
// which can be used to construct interface equality comparison.
// eqtab must be evaluated before eqdata, and shortcircuiting is required.
public static (ptr<ir.BinaryExpr>, ptr<ir.CallExpr>) EqInterface(ir.Node s, ir.Node t) {
    ptr<ir.BinaryExpr> eqtab = default!;
    ptr<ir.CallExpr> eqdata = default!;

    if (!types.Identical(s.Type(), t.Type())) {
        @base.Fatalf("EqInterface %v %v", s.Type(), t.Type());
    }
    ir.Node fn = default;
    if (s.Type().IsEmptyInterface()) {
        fn = typecheck.LookupRuntime("efaceeq");
    }
    else
 {
        fn = typecheck.LookupRuntime("ifaceeq");
    }
    var stab = ir.NewUnaryExpr(@base.Pos, ir.OITAB, s);
    var ttab = ir.NewUnaryExpr(@base.Pos, ir.OITAB, t);
    var sdata = ir.NewUnaryExpr(@base.Pos, ir.OIDATA, s);
    var tdata = ir.NewUnaryExpr(@base.Pos, ir.OIDATA, t);
    sdata.SetType(types.Types[types.TUNSAFEPTR]);
    tdata.SetType(types.Types[types.TUNSAFEPTR]);
    sdata.SetTypecheck(1);
    tdata.SetTypecheck(1);

    var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, new slice<ir.Node>(new ir.Node[] { stab, sdata, tdata }));
    typecheck.Call(call);

    var cmp = ir.NewBinaryExpr(@base.Pos, ir.OEQ, stab, ttab);
    cmp = typecheck.Expr(cmp)._<ptr<ir.BinaryExpr>>();
    cmp.SetType(types.Types[types.TBOOL]);
    return (_addr_cmp!, _addr_call!);
}

// eqmem returns the node
//     memequal(&p.field, &q.field [, size])
private static ir.Node eqmem(ir.Node p, ir.Node q, ptr<types.Sym> _addr_field, long size) {
    ref types.Sym field = ref _addr_field.val;

    var nx = typecheck.Expr(typecheck.NodAddr(ir.NewSelectorExpr(@base.Pos, ir.OXDOT, p, field)));
    var ny = typecheck.Expr(typecheck.NodAddr(ir.NewSelectorExpr(@base.Pos, ir.OXDOT, q, field)));

    var (fn, needsize) = eqmemfunc(size, _addr_nx.Type().Elem());
    var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, null);
    call.Args.Append(nx);
    call.Args.Append(ny);
    if (needsize) {
        call.Args.Append(ir.NewInt(size));
    }
    return call;
}

private static (ptr<ir.Name>, bool) eqmemfunc(long size, ptr<types.Type> _addr_t) {
    ptr<ir.Name> fn = default!;
    bool needsize = default;
    ref types.Type t = ref _addr_t.val;

    switch (size) {
        case 1: 

        case 2: 

        case 4: 

        case 8: 

        case 16: 
            var buf = fmt.Sprintf("memequal%d", int(size) * 8);
            fn = typecheck.LookupRuntime(buf);
            break;
        default: 
            fn = typecheck.LookupRuntime("memequal");
            needsize = true;
            break;
    }

    fn = typecheck.SubstArgTypes(fn, t, t);
    return (_addr_fn!, needsize);
}

// memrun finds runs of struct fields for which memory-only algs are appropriate.
// t is the parent struct type, and start is the field index at which to start the run.
// size is the length in bytes of the memory included in the run.
// next is the index just after the end of the memory run.
private static (long, nint) memrun(ptr<types.Type> _addr_t, nint start) {
    long size = default;
    nint next = default;
    ref types.Type t = ref _addr_t.val;

    next = start;
    while (true) {
        next++;
        if (next == t.NumFields()) {
            break;
        }
        if (types.IsPaddedField(t, next - 1)) {
            break;
        }
        {
            var f = t.Field(next);

            if (f.Sym.IsBlank() || !isRegularMemory(_addr_f.Type)) {
                break;
            } 
            // For issue 46283, don't combine fields if the resulting load would
            // require a larger alignment than the component fields.

        } 
        // For issue 46283, don't combine fields if the resulting load would
        // require a larger alignment than the component fields.
        if (@base.Ctxt.Arch.Alignment > 1) {
            var align = t.Alignment();
            {
                var off = t.Field(start).Offset;

                if (off & (align - 1) != 0) { 
                    // Offset is less aligned than the containing type.
                    // Use offset to determine alignment.
                    align = 1 << (int)(uint(bits.TrailingZeros64(uint64(off))));
                }

            }
            var size = t.Field(next).End() - t.Field(start).Offset;
            if (size > align) {
                break;
            }
        }
    }
    return (t.Field(next - 1).End() - t.Field(start).Offset, next);
}

private static ir.Node hashmem(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var sym = ir.Pkgs.Runtime.Lookup("memhash"); 

    // TODO(austin): This creates an ir.Name with a nil Func.
    var n = typecheck.NewName(sym);
    ir.MarkFunc(n);
    n.SetType(types.NewSignature(types.NoPkg, null, null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.NewPtr(t)), types.NewField(base.Pos,nil,types.Types[types.TUINTPTR]), types.NewField(base.Pos,nil,types.Types[types.TUINTPTR]) }), new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.Types[types.TUINTPTR]) })));
    return n;
}

} // end reflectdata_package
