// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:24:02 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\alg.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // AlgKind describes the kind of algorithms used for comparing and
        // hashing a Type.
        public partial struct AlgKind // : long
        {
        }

        //go:generate stringer -type AlgKind -trimprefix A

 
        // These values are known by runtime.
        public static readonly AlgKind ANOEQ = (AlgKind)iota;
        public static readonly var AMEM0 = 0;
        public static readonly var AMEM8 = 1;
        public static readonly var AMEM16 = 2;
        public static readonly var AMEM32 = 3;
        public static readonly var AMEM64 = 4;
        public static readonly var AMEM128 = 5;
        public static readonly var ASTRING = 6;
        public static readonly var AINTER = 7;
        public static readonly var ANILINTER = 8;
        public static readonly var AFLOAT32 = 9;
        public static readonly var AFLOAT64 = 10;
        public static readonly var ACPLX64 = 11;
        public static readonly var ACPLX128 = 12; 

        // Type can be compared/hashed as regular memory.
        public static readonly AlgKind AMEM = (AlgKind)100L; 

        // Type needs special comparison/hashing functions.
        public static readonly AlgKind ASPECIAL = (AlgKind)-1L;


        // IsComparable reports whether t is a comparable type.
        public static bool IsComparable(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var (a, _) = algtype1(_addr_t);
            return a != ANOEQ;
        }

        // IsRegularMemory reports whether t can be compared/hashed as regular memory.
        public static bool IsRegularMemory(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var (a, _) = algtype1(_addr_t);
            return a == AMEM;
        }

        // IncomparableField returns an incomparable Field of struct Type t, if any.
        public static ptr<types.Field> IncomparableField(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            foreach (var (_, f) in t.FieldSlice())
            {
                if (!IsComparable(_addr_f.Type))
                {
                    return _addr_f!;
                }

            }
            return _addr_null!;

        }

        // EqCanPanic reports whether == on type t could panic (has an interface somewhere).
        // t must be comparable.
        public static bool EqCanPanic(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TINTER) 
                return true;
            else if (t.Etype == TARRAY) 
                return EqCanPanic(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, f) in t.FieldSlice())
                {
                    if (!f.Sym.IsBlank() && EqCanPanic(_addr_f.Type))
                    {
                        return true;
                    }

                }
                return false;
            else 
                return false;
            
        }

        // algtype is like algtype1, except it returns the fixed-width AMEMxx variants
        // instead of the general AMEM kind when possible.
        private static AlgKind algtype(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var (a, _) = algtype1(_addr_t);
            if (a == AMEM)
            {
                switch (t.Width)
                {
                    case 0L: 
                        return AMEM0;
                        break;
                    case 1L: 
                        return AMEM8;
                        break;
                    case 2L: 
                        return AMEM16;
                        break;
                    case 4L: 
                        return AMEM32;
                        break;
                    case 8L: 
                        return AMEM64;
                        break;
                    case 16L: 
                        return AMEM128;
                        break;
                }

            }

            return a;

        }

        // algtype1 returns the AlgKind used for comparing and hashing Type t.
        // If it returns ANOEQ, it also returns the component type of t that
        // makes it incomparable.
        private static (AlgKind, ptr<types.Type>) algtype1(ptr<types.Type> _addr_t)
        {
            AlgKind _p0 = default;
            ptr<types.Type> _p0 = default!;
            ref types.Type t = ref _addr_t.val;

            if (t.Broke())
            {
                return (AMEM, _addr_null!);
            }

            if (t.Noalg())
            {
                return (ANOEQ, _addr_t!);
            }


            if (t.Etype == TANY || t.Etype == TFORW) 
                // will be defined later.
                return (ANOEQ, _addr_t!);
            else if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR || t.Etype == TBOOL || t.Etype == TPTR || t.Etype == TCHAN || t.Etype == TUNSAFEPTR) 
                return (AMEM, _addr_null!);
            else if (t.Etype == TFUNC || t.Etype == TMAP) 
                return (ANOEQ, _addr_t!);
            else if (t.Etype == TFLOAT32) 
                return (AFLOAT32, _addr_null!);
            else if (t.Etype == TFLOAT64) 
                return (AFLOAT64, _addr_null!);
            else if (t.Etype == TCOMPLEX64) 
                return (ACPLX64, _addr_null!);
            else if (t.Etype == TCOMPLEX128) 
                return (ACPLX128, _addr_null!);
            else if (t.Etype == TSTRING) 
                return (ASTRING, _addr_null!);
            else if (t.Etype == TINTER) 
                if (t.IsEmptyInterface())
                {
                    return (ANILINTER, _addr_null!);
                }

                return (AINTER, _addr_null!);
            else if (t.Etype == TSLICE) 
                return (ANOEQ, _addr_t!);
            else if (t.Etype == TARRAY) 
                var (a, bad) = algtype1(_addr_t.Elem());

                if (a == AMEM) 
                    return (AMEM, _addr_null!);
                else if (a == ANOEQ) 
                    return (ANOEQ, _addr_bad!);
                                switch (t.NumElem())
                {
                    case 0L: 
                        // We checked above that the element type is comparable.
                        return (AMEM, _addr_null!);
                        break;
                    case 1L: 
                        // Single-element array is same as its lone element.
                        return (a, _addr_null!);
                        break;
                }

                return (ASPECIAL, _addr_null!);
            else if (t.Etype == TSTRUCT) 
                var fields = t.FieldSlice(); 

                // One-field struct is same as that one field alone.
                if (len(fields) == 1L && !fields[0L].Sym.IsBlank())
                {
                    return algtype1(_addr_fields[0L].Type);
                }

                var ret = AMEM;
                foreach (var (i, f) in fields)
                { 
                    // All fields must be comparable.
                    (a, bad) = algtype1(_addr_f.Type);
                    if (a == ANOEQ)
                    {
                        return (ANOEQ, _addr_bad!);
                    } 

                    // Blank fields, padded fields, fields with non-memory
                    // equality need special compare.
                    if (a != AMEM || f.Sym.IsBlank() || ispaddedfield(_addr_t, i))
                    {
                        ret = ASPECIAL;
                    }

                }
                return (ret, _addr_null!);
                        Fatalf("algtype1: unexpected type %v", t);
            return (0L, _addr_null!);

        }

        // genhash returns a symbol which is the closure used to compute
        // the hash of a value of type t.
        // Note: the generated function must match runtime.typehash exactly.
        private static ptr<obj.LSym> genhash(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (algtype(_addr_t) == AMEM0) 
                return _addr_sysClosure("memhash0")!;
            else if (algtype(_addr_t) == AMEM8) 
                return _addr_sysClosure("memhash8")!;
            else if (algtype(_addr_t) == AMEM16) 
                return _addr_sysClosure("memhash16")!;
            else if (algtype(_addr_t) == AMEM32) 
                return _addr_sysClosure("memhash32")!;
            else if (algtype(_addr_t) == AMEM64) 
                return _addr_sysClosure("memhash64")!;
            else if (algtype(_addr_t) == AMEM128) 
                return _addr_sysClosure("memhash128")!;
            else if (algtype(_addr_t) == ASTRING) 
                return _addr_sysClosure("strhash")!;
            else if (algtype(_addr_t) == AINTER) 
                return _addr_sysClosure("interhash")!;
            else if (algtype(_addr_t) == ANILINTER) 
                return _addr_sysClosure("nilinterhash")!;
            else if (algtype(_addr_t) == AFLOAT32) 
                return _addr_sysClosure("f32hash")!;
            else if (algtype(_addr_t) == AFLOAT64) 
                return _addr_sysClosure("f64hash")!;
            else if (algtype(_addr_t) == ACPLX64) 
                return _addr_sysClosure("c64hash")!;
            else if (algtype(_addr_t) == ACPLX128) 
                return _addr_sysClosure("c128hash")!;
            else if (algtype(_addr_t) == AMEM) 
                // For other sizes of plain memory, we build a closure
                // that calls memhash_varlen. The size of the memory is
                // encoded in the first slot of the closure.
                var closure = typeLookup(fmt.Sprintf(".hashfunc%d", t.Width)).Linksym();
                if (len(closure.P) > 0L)
                { // already generated
                    return _addr_closure!;

                }

                if (memhashvarlen == null)
                {
                    memhashvarlen = sysfunc("memhash_varlen");
                }

                long ot = 0L;
                ot = dsymptr(closure, ot, memhashvarlen, 0L);
                ot = duintptr(closure, ot, uint64(t.Width)); // size encoded in closure
                ggloblsym(closure, int32(ot), obj.DUPOK | obj.RODATA);
                return _addr_closure!;
            else if (algtype(_addr_t) == ASPECIAL) 
                break;
            else 
                // genhash is only called for types that have equality
                Fatalf("genhash %v", t);
                        closure = typesymprefix(".hashfunc", t).Linksym();
            if (len(closure.P) > 0L)
            { // already generated
                return _addr_closure!;

            } 

            // Generate hash functions for subtypes.
            // There are cases where we might not use these hashes,
            // but in that case they will get dead-code eliminated.
            // (And the closure generated by genhash will also get
            // dead-code eliminated, as we call the subtype hashers
            // directly.)

            if (t.Etype == types.TARRAY) 
                genhash(_addr_t.Elem());
            else if (t.Etype == types.TSTRUCT) 
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        genhash(_addr_f.Type);
                    }

                    f = f__prev1;
                }
                        var sym = typesymprefix(".hash", t);
            if (Debug['r'] != 0L)
            {
                fmt.Printf("genhash %v %v %v\n", closure, sym, t);
            }

            lineno = autogeneratedPos; // less confusing than end of input
            dclcontext = PEXTERN; 

            // func sym(p *T, h uintptr) uintptr
            var tfn = nod(OTFUNC, null, null);
            tfn.List.Set2(namedfield("p", types.NewPtr(t)), namedfield("h", types.Types[TUINTPTR]));
            tfn.Rlist.Set1(anonfield(types.Types[TUINTPTR]));

            var fn = dclfunc(sym, tfn);
            var np = asNode(tfn.Type.Params().Field(0L).Nname);
            var nh = asNode(tfn.Type.Params().Field(1L).Nname);


            if (t.Etype == types.TARRAY) 
                // An array of pure memory would be handled by the
                // standard algorithm, so the element type must not be
                // pure memory.
                var hashel = hashfor(_addr_t.Elem());

                var n = nod(ORANGE, null, nod(ODEREF, np, null));
                var ni = newname(lookup("i"));
                ni.Type = types.Types[TINT];
                n.List.Set1(ni);
                n.SetColas(true);
                colasdefn(n.List.Slice(), n);
                ni = n.List.First(); 

                // h = hashel(&p[i], h)
                var call = nod(OCALL, hashel, null);

                var nx = nod(OINDEX, np, ni);
                nx.SetBounded(true);
                var na = nod(OADDR, nx, null);
                call.List.Append(na);
                call.List.Append(nh);
                n.Nbody.Append(nod(OAS, nh, call));

                fn.Nbody.Append(n);
            else if (t.Etype == types.TSTRUCT) 
                // Walk the struct using memhash for runs of AMEM
                // and calling specific hash functions for the others.
                {
                    long i = 0L;
                    var fields = t.FieldSlice();

                    while (i < len(fields))
                    {
                        var f = fields[i]; 

                        // Skip blank fields.
                        if (f.Sym.IsBlank())
                        {
                            i++;
                            continue;
                        } 

                        // Hash non-memory fields with appropriate hash function.
                        if (!IsRegularMemory(_addr_f.Type))
                        {
                            hashel = hashfor(_addr_f.Type);
                            call = nod(OCALL, hashel, null);
                            nx = nodSym(OXDOT, np, f.Sym); // TODO: fields from other packages?
                            na = nod(OADDR, nx, null);
                            call.List.Append(na);
                            call.List.Append(nh);
                            fn.Nbody.Append(nod(OAS, nh, call));
                            i++;
                            continue;

                        } 

                        // Otherwise, hash a maximal length run of raw memory.
                        var (size, next) = memrun(_addr_t, i); 

                        // h = hashel(&p.first, size, h)
                        hashel = hashmem(f.Type);
                        call = nod(OCALL, hashel, null);
                        nx = nodSym(OXDOT, np, f.Sym); // TODO: fields from other packages?
                        na = nod(OADDR, nx, null);
                        call.List.Append(na);
                        call.List.Append(nh);
                        call.List.Append(nodintconst(size));
                        fn.Nbody.Append(nod(OAS, nh, call));

                        i = next;

                    }

                }
                        var r = nod(ORETURN, null, null);
            r.List.Append(nh);
            fn.Nbody.Append(r);

            if (Debug['r'] != 0L)
            {
                dumplist("genhash body", fn.Nbody);
            }

            funcbody();

            fn.Func.SetDupok(true);
            fn = typecheck(fn, ctxStmt);

            Curfn = fn;
            typecheckslice(fn.Nbody.Slice(), ctxStmt);
            Curfn = null;

            if (debug_dclstack != 0L)
            {
                testdclstack();
            }

            fn.Func.SetNilCheckDisabled(true);
            funccompile(fn); 

            // Build closure. It doesn't close over any variables, so
            // it contains just the function pointer.
            dsymptr(closure, 0L, sym.Linksym(), 0L);
            ggloblsym(closure, int32(Widthptr), obj.DUPOK | obj.RODATA);

            return _addr_closure!;

        }

        private static ptr<Node> hashfor(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            ptr<types.Sym> sym;

            {
                var (a, _) = algtype1(_addr_t);


                if (a == AMEM) 
                    Fatalf("hashfor with AMEM type");
                else if (a == AINTER) 
                    sym = Runtimepkg.Lookup("interhash");
                else if (a == ANILINTER) 
                    sym = Runtimepkg.Lookup("nilinterhash");
                else if (a == ASTRING) 
                    sym = Runtimepkg.Lookup("strhash");
                else if (a == AFLOAT32) 
                    sym = Runtimepkg.Lookup("f32hash");
                else if (a == AFLOAT64) 
                    sym = Runtimepkg.Lookup("f64hash");
                else if (a == ACPLX64) 
                    sym = Runtimepkg.Lookup("c64hash");
                else if (a == ACPLX128) 
                    sym = Runtimepkg.Lookup("c128hash");
                else 
                    // Note: the caller of hashfor ensured that this symbol
                    // exists and has a body by calling genhash for t.
                    sym = typesymprefix(".hash", t);

            }

            var n = newname(sym);
            n.SetClass(PFUNC);
            n.Sym.SetFunc(true);
            n.Type = functype(null, new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.NewPtr(t)), anonfield(types.Types[TUINTPTR]) }), new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.Types[TUINTPTR]) }));
            return _addr_n!;

        }

        // sysClosure returns a closure which will call the
        // given runtime function (with no closed-over variables).
        private static ptr<obj.LSym> sysClosure(@string name)
        {
            var s = sysvar(name + "·f");
            if (len(s.P) == 0L)
            {
                var f = sysfunc(name);
                dsymptr(s, 0L, f, 0L);
                ggloblsym(s, int32(Widthptr), obj.DUPOK | obj.RODATA);
            }

            return _addr_s!;

        }

        // geneq returns a symbol which is the closure used to compute
        // equality for two objects of type t.
        private static ptr<obj.LSym> geneq(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (algtype(_addr_t) == ANOEQ) 
                // The runtime will panic if it tries to compare
                // a type with a nil equality function.
                return _addr_null!;
            else if (algtype(_addr_t) == AMEM0) 
                return _addr_sysClosure("memequal0")!;
            else if (algtype(_addr_t) == AMEM8) 
                return _addr_sysClosure("memequal8")!;
            else if (algtype(_addr_t) == AMEM16) 
                return _addr_sysClosure("memequal16")!;
            else if (algtype(_addr_t) == AMEM32) 
                return _addr_sysClosure("memequal32")!;
            else if (algtype(_addr_t) == AMEM64) 
                return _addr_sysClosure("memequal64")!;
            else if (algtype(_addr_t) == AMEM128) 
                return _addr_sysClosure("memequal128")!;
            else if (algtype(_addr_t) == ASTRING) 
                return _addr_sysClosure("strequal")!;
            else if (algtype(_addr_t) == AINTER) 
                return _addr_sysClosure("interequal")!;
            else if (algtype(_addr_t) == ANILINTER) 
                return _addr_sysClosure("nilinterequal")!;
            else if (algtype(_addr_t) == AFLOAT32) 
                return _addr_sysClosure("f32equal")!;
            else if (algtype(_addr_t) == AFLOAT64) 
                return _addr_sysClosure("f64equal")!;
            else if (algtype(_addr_t) == ACPLX64) 
                return _addr_sysClosure("c64equal")!;
            else if (algtype(_addr_t) == ACPLX128) 
                return _addr_sysClosure("c128equal")!;
            else if (algtype(_addr_t) == AMEM) 
                // make equality closure. The size of the type
                // is encoded in the closure.
                var closure = typeLookup(fmt.Sprintf(".eqfunc%d", t.Width)).Linksym();
                if (len(closure.P) != 0L)
                {
                    return _addr_closure!;
                }

                if (memequalvarlen == null)
                {
                    memequalvarlen = sysvar("memequal_varlen"); // asm func
                }

                long ot = 0L;
                ot = dsymptr(closure, ot, memequalvarlen, 0L);
                ot = duintptr(closure, ot, uint64(t.Width));
                ggloblsym(closure, int32(ot), obj.DUPOK | obj.RODATA);
                return _addr_closure!;
            else if (algtype(_addr_t) == ASPECIAL) 
                break;
                        closure = typesymprefix(".eqfunc", t).Linksym();
            if (len(closure.P) > 0L)
            { // already generated
                return _addr_closure!;

            }

            var sym = typesymprefix(".eq", t);
            if (Debug['r'] != 0L)
            {
                fmt.Printf("geneq %v\n", t);
            } 

            // Autogenerate code for equality of structs and arrays.
            lineno = autogeneratedPos; // less confusing than end of input
            dclcontext = PEXTERN; 

            // func sym(p, q *T) bool
            var tfn = nod(OTFUNC, null, null);
            tfn.List.Set2(namedfield("p", types.NewPtr(t)), namedfield("q", types.NewPtr(t)));
            tfn.Rlist.Set1(namedfield("r", types.Types[TBOOL]));

            var fn = dclfunc(sym, tfn);
            var np = asNode(tfn.Type.Params().Field(0L).Nname);
            var nq = asNode(tfn.Type.Params().Field(1L).Nname); 

            // We reach here only for types that have equality but
            // cannot be handled by the standard algorithms,
            // so t must be either an array or a struct.

            if (t.Etype == TARRAY) 
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
                //     return
                //   }
                // }
                //
                // TODO(josharian): consider doing some loop unrolling
                // for larger nelem as well, processing a few elements at a time in a loop.
                Func<long, Func<ptr<Node>, ptr<Node>, ptr<Node>>, ptr<Node>> checkAll = (unroll, eq) =>
                { 
                    // checkIdx generates a node to check for equality at index i.
                    Func<ptr<Node>, ptr<Node>> checkIdx = i =>
                    { 
                        // pi := p[i]
                        var pi = nod(OINDEX, np, i);
                        pi.SetBounded(true);
                        pi.Type = t.Elem(); 
                        // qi := q[i]
                        var qi = nod(OINDEX, nq, i);
                        qi.SetBounded(true);
                        qi.Type = t.Elem();
                        return _addr_eq(pi, qi)!;

                    }
;

                    if (nelem <= unroll)
                    { 
                        // Generate a series of checks.
                        ptr<Node> cond;
                        {
                            var i__prev1 = i;

                            for (var i = int64(0L); i < nelem; i++)
                            {
                                var c = nodintconst(i);
                                var check = checkIdx(c);
                                if (cond == null)
                                {
                                    cond = check;
                                    continue;
                                }

                                cond = nod(OANDAND, cond, check);

                            }


                            i = i__prev1;
                        }
                        var nif = nod(OIF, cond, null);
                        nif.Rlist.Append(nod(ORETURN, null, null));
                        fn.Nbody.Append(nif);
                        return ;

                    } 

                    // Generate a for loop.
                    // for i := 0; i < nelem; i++
                    i = temp(types.Types[TINT]);
                    var init = nod(OAS, i, nodintconst(0L));
                    cond = nod(OLT, i, nodintconst(nelem));
                    var post = nod(OAS, i, nod(OADD, i, nodintconst(1L)));
                    var loop = nod(OFOR, cond, post);
                    loop.Ninit.Append(init); 
                    // if eq(pi, qi) {} else { return }
                    check = checkIdx(i);
                    nif = nod(OIF, check, null);
                    nif.Rlist.Append(nod(ORETURN, null, null));
                    loop.Nbody.Append(nif);
                    fn.Nbody.Append(loop);

                }
;


                if (t.Elem().Etype == TSTRING) 
                    // Do two loops. First, check that all the lengths match (cheap).
                    // Second, check that all the contents match (expensive).
                    // TODO: when the array size is small, unroll the length match checks.
                    checkAll(3L, (pi, qi) =>
                    { 
                        // Compare lengths.
                        var (eqlen, _) = eqstring(_addr_pi, _addr_qi);
                        return _addr_eqlen!;

                    });
                    checkAll(1L, (pi, qi) =>
                    { 
                        // Compare contents.
                        var (_, eqmem) = eqstring(_addr_pi, _addr_qi);
                        return _addr_eqmem!;

                    });
                else if (t.Elem().Etype == TFLOAT32 || t.Elem().Etype == TFLOAT64) 
                    checkAll(2L, (pi, qi) =>
                    { 
                        // p[i] == q[i]
                        return _addr_nod(OEQ, pi, qi)!;

                    }); 
                    // TODO: pick apart structs, do them piecemeal too
                else 
                    checkAll(1L, (pi, qi) =>
                    { 
                        // p[i] == q[i]
                        return _addr_nod(OEQ, pi, qi)!;

                    });
                // return true
                var ret = nod(ORETURN, null, null);
                ret.List.Append(nodbool(true));
                fn.Nbody.Append(ret);
            else if (t.Etype == TSTRUCT) 
                // Build a list of conditions to satisfy.
                // The conditions are a list-of-lists. Conditions are reorderable
                // within each inner list. The outer lists must be evaluated in order.
                // Even within each inner list, track their order so that we can preserve
                // aspects of that order. (TODO: latter part needed?)
                private partial struct nodeIdx
                {
                    public ptr<Node> n;
                    public long idx;
                }
                slice<slice<nodeIdx>> conds = default;
                conds = append(conds, new slice<nodeIdx>(new nodeIdx[] {  }));
                Action<ptr<Node>> and = n =>
                {
                    i = len(conds) - 1L;
                    conds[i] = append(conds[i], new nodeIdx(n:n,idx:len(conds[i])));
                } 

                // Walk the struct using memequal for runs of AMEM
                // and calling specific equality tests for the others.
; 

                // Walk the struct using memequal for runs of AMEM
                // and calling specific equality tests for the others.
                {
                    var i__prev1 = i;

                    i = 0L;
                    var fields = t.FieldSlice();

                    while (i < len(fields))
                    {
                        var f = fields[i]; 

                        // Skip blank-named fields.
                        if (f.Sym.IsBlank())
                        {
                            i++;
                            continue;
                        } 

                        // Compare non-memory fields with field equality.
                        if (!IsRegularMemory(_addr_f.Type))
                        {
                            if (EqCanPanic(_addr_f.Type))
                            { 
                                // Enforce ordering by starting a new set of reorderable conditions.
                                conds = append(conds, new slice<nodeIdx>(new nodeIdx[] {  }));

                            }

                            var p = nodSym(OXDOT, np, f.Sym);
                            var q = nodSym(OXDOT, nq, f.Sym);

                            if (f.Type.IsString()) 
                                var (eqlen, eqmem) = eqstring(_addr_p, _addr_q);
                                and(eqlen);
                                and(eqmem);
                            else 
                                and(nod(OEQ, p, q));
                                                        if (EqCanPanic(_addr_f.Type))
                            { 
                                // Also enforce ordering after something that can panic.
                                conds = append(conds, new slice<nodeIdx>(new nodeIdx[] {  }));

                            }

                            i++;
                            continue;

                        } 

                        // Find maximal length run of memory-only fields.
                        var (size, next) = memrun(_addr_t, i); 

                        // TODO(rsc): All the calls to newname are wrong for
                        // cross-package unexported fields.
                        {
                            var s = fields[i..next];

                            if (len(s) <= 2L)
                            { 
                                // Two or fewer fields: use plain field equality.
                                {
                                    var f__prev2 = f;

                                    foreach (var (_, __f) in s)
                                    {
                                        f = __f;
                                        and(eqfield(_addr_np, _addr_nq, _addr_f.Sym));
                                    }
                            else

                                    f = f__prev2;
                                }
                            }                            { 
                                // More than two fields: use memequal.
                                and(eqmem(_addr_np, _addr_nq, _addr_f.Sym, size));

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
                slice<nodeIdx> flatConds = default;
                {
                    var c__prev1 = c;

                    foreach (var (_, __c) in conds)
                    {
                        c = __c;
                        sort.SliceStable(c, (i, j) =>
                        {
                            var x = c[i];
                            var y = c[j];
                            if ((x.n.Op != OCALL) == (y.n.Op != OCALL))
                            {
                                return _addr_x.idx < y.idx!;
                            }

                            return _addr_x.n.Op != OCALL!;

                        });
                        flatConds = append(flatConds, c);

                    }

                    c = c__prev1;
                }

                cond = ;
                if (len(flatConds) == 0L)
                {
                    cond = nodbool(true);
                }
                else
                {
                    cond = flatConds[0L].n;
                    {
                        var c__prev1 = c;

                        foreach (var (_, __c) in flatConds[1L..])
                        {
                            c = __c;
                            cond = nod(OANDAND, cond, c.n);
                        }

                        c = c__prev1;
                    }
                }

                ret = nod(ORETURN, null, null);
                ret.List.Append(cond);
                fn.Nbody.Append(ret);
            else 
                Fatalf("geneq %v", t);
                        if (Debug['r'] != 0L)
            {
                dumplist("geneq body", fn.Nbody);
            }

            funcbody();

            fn.Func.SetDupok(true);
            fn = typecheck(fn, ctxStmt);

            Curfn = fn;
            typecheckslice(fn.Nbody.Slice(), ctxStmt);
            Curfn = null;

            if (debug_dclstack != 0L)
            {
                testdclstack();
            } 

            // Disable checknils while compiling this code.
            // We are comparing a struct or an array,
            // neither of which can be nil, and our comparisons
            // are shallow.
            fn.Func.SetNilCheckDisabled(true);
            funccompile(fn); 

            // Generate a closure which points at the function we just generated.
            dsymptr(closure, 0L, sym.Linksym(), 0L);
            ggloblsym(closure, int32(Widthptr), obj.DUPOK | obj.RODATA);
            return _addr_closure!;

        }

        // eqfield returns the node
        //     p.field == q.field
        private static ptr<Node> eqfield(ptr<Node> _addr_p, ptr<Node> _addr_q, ptr<types.Sym> _addr_field)
        {
            ref Node p = ref _addr_p.val;
            ref Node q = ref _addr_q.val;
            ref types.Sym field = ref _addr_field.val;

            var nx = nodSym(OXDOT, p, field);
            var ny = nodSym(OXDOT, q, field);
            var ne = nod(OEQ, nx, ny);
            return _addr_ne!;
        }

        // eqstring returns the nodes
        //   len(s) == len(t)
        // and
        //   memequal(s.ptr, t.ptr, len(s))
        // which can be used to construct string equality comparison.
        // eqlen must be evaluated before eqmem, and shortcircuiting is required.
        private static (ptr<Node>, ptr<Node>) eqstring(ptr<Node> _addr_s, ptr<Node> _addr_t)
        {
            ptr<Node> eqlen = default!;
            ptr<Node> eqmem = default!;
            ref Node s = ref _addr_s.val;
            ref Node t = ref _addr_t.val;

            s = conv(s, types.Types[TSTRING]);
            t = conv(t, types.Types[TSTRING]);
            var sptr = nod(OSPTR, s, null);
            var tptr = nod(OSPTR, t, null);
            var slen = conv(nod(OLEN, s, null), types.Types[TUINTPTR]);
            var tlen = conv(nod(OLEN, t, null), types.Types[TUINTPTR]);

            var fn = syslook("memequal");
            fn = substArgTypes(fn, types.Types[TUINT8], types.Types[TUINT8]);
            var call = nod(OCALL, fn, null);
            call.List.Append(sptr, tptr, slen.copy());
            call = typecheck(call, ctxExpr | ctxMultiOK);

            var cmp = nod(OEQ, slen, tlen);
            cmp = typecheck(cmp, ctxExpr);
            cmp.Type = types.Types[TBOOL];
            return (_addr_cmp!, _addr_call!);
        }

        // eqinterface returns the nodes
        //   s.tab == t.tab (or s.typ == t.typ, as appropriate)
        // and
        //   ifaceeq(s.tab, s.data, t.data) (or efaceeq(s.typ, s.data, t.data), as appropriate)
        // which can be used to construct interface equality comparison.
        // eqtab must be evaluated before eqdata, and shortcircuiting is required.
        private static (ptr<Node>, ptr<Node>) eqinterface(ptr<Node> _addr_s, ptr<Node> _addr_t)
        {
            ptr<Node> eqtab = default!;
            ptr<Node> eqdata = default!;
            ref Node s = ref _addr_s.val;
            ref Node t = ref _addr_t.val;

            if (!types.Identical(s.Type, t.Type))
            {
                Fatalf("eqinterface %v %v", s.Type, t.Type);
            } 
            // func ifaceeq(tab *uintptr, x, y unsafe.Pointer) (ret bool)
            // func efaceeq(typ *uintptr, x, y unsafe.Pointer) (ret bool)
            ptr<Node> fn;
            if (s.Type.IsEmptyInterface())
            {
                fn = syslook("efaceeq");
            }
            else
            {
                fn = syslook("ifaceeq");
            }

            var stab = nod(OITAB, s, null);
            var ttab = nod(OITAB, t, null);
            var sdata = nod(OIDATA, s, null);
            var tdata = nod(OIDATA, t, null);
            sdata.Type = types.Types[TUNSAFEPTR];
            tdata.Type = types.Types[TUNSAFEPTR];
            sdata.SetTypecheck(1L);
            tdata.SetTypecheck(1L);

            var call = nod(OCALL, fn, null);
            call.List.Append(stab, sdata, tdata);
            call = typecheck(call, ctxExpr | ctxMultiOK);

            var cmp = nod(OEQ, stab, ttab);
            cmp = typecheck(cmp, ctxExpr);
            cmp.Type = types.Types[TBOOL];
            return (_addr_cmp!, _addr_call!);

        }

        // eqmem returns the node
        //     memequal(&p.field, &q.field [, size])
        private static ptr<Node> eqmem(ptr<Node> _addr_p, ptr<Node> _addr_q, ptr<types.Sym> _addr_field, long size)
        {
            ref Node p = ref _addr_p.val;
            ref Node q = ref _addr_q.val;
            ref types.Sym field = ref _addr_field.val;

            var nx = nod(OADDR, nodSym(OXDOT, p, field), null);
            var ny = nod(OADDR, nodSym(OXDOT, q, field), null);
            nx = typecheck(nx, ctxExpr);
            ny = typecheck(ny, ctxExpr);

            var (fn, needsize) = eqmemfunc(size, _addr_nx.Type.Elem());
            var call = nod(OCALL, fn, null);
            call.List.Append(nx);
            call.List.Append(ny);
            if (needsize)
            {
                call.List.Append(nodintconst(size));
            }

            return _addr_call!;

        }

        private static (ptr<Node>, bool) eqmemfunc(long size, ptr<types.Type> _addr_t)
        {
            ptr<Node> fn = default!;
            bool needsize = default;
            ref types.Type t = ref _addr_t.val;

            switch (size)
            {
                case 1L: 

                case 2L: 

                case 4L: 

                case 8L: 

                case 16L: 
                    var buf = fmt.Sprintf("memequal%d", int(size) * 8L);
                    fn = syslook(buf);
                    break;
                default: 
                    fn = syslook("memequal");
                    needsize = true;
                    break;
            }

            fn = substArgTypes(fn, t, t);
            return (_addr_fn!, needsize);

        }

        // memrun finds runs of struct fields for which memory-only algs are appropriate.
        // t is the parent struct type, and start is the field index at which to start the run.
        // size is the length in bytes of the memory included in the run.
        // next is the index just after the end of the memory run.
        private static (long, long) memrun(ptr<types.Type> _addr_t, long start)
        {
            long size = default;
            long next = default;
            ref types.Type t = ref _addr_t.val;

            next = start;
            while (true)
            {
                next++;
                if (next == t.NumFields())
                {
                    break;
                } 
                // Stop run after a padded field.
                if (ispaddedfield(_addr_t, next - 1L))
                {
                    break;
                } 
                // Also, stop before a blank or non-memory field.
                {
                    var f = t.Field(next);

                    if (f.Sym.IsBlank() || !IsRegularMemory(_addr_f.Type))
                    {
                        break;
                    }

                }

            }

            return (t.Field(next - 1L).End() - t.Field(start).Offset, next);

        }

        // ispaddedfield reports whether the i'th field of struct type t is followed
        // by padding.
        private static bool ispaddedfield(ptr<types.Type> _addr_t, long i)
        {
            ref types.Type t = ref _addr_t.val;

            if (!t.IsStruct())
            {
                Fatalf("ispaddedfield called non-struct %v", t);
            }

            var end = t.Width;
            if (i + 1L < t.NumFields())
            {
                end = t.Field(i + 1L).Offset;
            }

            return t.Field(i).End() != end;

        }
    }
}}}}
