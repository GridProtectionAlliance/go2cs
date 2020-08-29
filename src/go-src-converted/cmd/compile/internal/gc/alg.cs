// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 08:53:03 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\alg.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
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

 
        // These values are known by runtime.
        public static readonly AlgKind ANOEQ = iota;
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
        public static readonly AlgKind AMEM = 100L; 

        // Type needs special comparison/hashing functions.
        public static readonly AlgKind ASPECIAL = -1L;

        // IsComparable reports whether t is a comparable type.
        public static bool IsComparable(ref types.Type t)
        {
            var (a, _) = algtype1(t);
            return a != ANOEQ;
        }

        // IsRegularMemory reports whether t can be compared/hashed as regular memory.
        public static bool IsRegularMemory(ref types.Type t)
        {
            var (a, _) = algtype1(t);
            return a == AMEM;
        }

        // IncomparableField returns an incomparable Field of struct Type t, if any.
        public static ref types.Field IncomparableField(ref types.Type t)
        {
            foreach (var (_, f) in t.FieldSlice())
            {
                if (!IsComparable(f.Type))
                {
                    return f;
                }
            }
            return null;
        }

        // algtype is like algtype1, except it returns the fixed-width AMEMxx variants
        // instead of the general AMEM kind when possible.
        private static AlgKind algtype(ref types.Type t)
        {
            var (a, _) = algtype1(t);
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
        private static (AlgKind, ref types.Type) algtype1(ref types.Type t)
        {
            if (t.Broke())
            {
                return (AMEM, null);
            }
            if (t.Noalg())
            {
                return (ANOEQ, t);
            }

            if (t.Etype == TANY || t.Etype == TFORW) 
                // will be defined later.
                return (ANOEQ, t);
            else if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR || t.Etype == TBOOL || t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TCHAN || t.Etype == TUNSAFEPTR) 
                return (AMEM, null);
            else if (t.Etype == TFUNC || t.Etype == TMAP) 
                return (ANOEQ, t);
            else if (t.Etype == TFLOAT32) 
                return (AFLOAT32, null);
            else if (t.Etype == TFLOAT64) 
                return (AFLOAT64, null);
            else if (t.Etype == TCOMPLEX64) 
                return (ACPLX64, null);
            else if (t.Etype == TCOMPLEX128) 
                return (ACPLX128, null);
            else if (t.Etype == TSTRING) 
                return (ASTRING, null);
            else if (t.Etype == TINTER) 
                if (t.IsEmptyInterface())
                {
                    return (ANILINTER, null);
                }
                return (AINTER, null);
            else if (t.Etype == TSLICE) 
                return (ANOEQ, t);
            else if (t.Etype == TARRAY) 
                var (a, bad) = algtype1(t.Elem());

                if (a == AMEM) 
                    return (AMEM, null);
                else if (a == ANOEQ) 
                    return (ANOEQ, bad);
                                switch (t.NumElem())
                {
                    case 0L: 
                        // We checked above that the element type is comparable.
                        return (AMEM, null);
                        break;
                    case 1L: 
                        // Single-element array is same as its lone element.
                        return (a, null);
                        break;
                }

                return (ASPECIAL, null);
            else if (t.Etype == TSTRUCT) 
                var fields = t.FieldSlice(); 

                // One-field struct is same as that one field alone.
                if (len(fields) == 1L && !fields[0L].Sym.IsBlank())
                {
                    return algtype1(fields[0L].Type);
                }
                var ret = AMEM;
                foreach (var (i, f) in fields)
                { 
                    // All fields must be comparable.
                    (a, bad) = algtype1(f.Type);
                    if (a == ANOEQ)
                    {
                        return (ANOEQ, bad);
                    } 

                    // Blank fields, padded fields, fields with non-memory
                    // equality need special compare.
                    if (a != AMEM || f.Sym.IsBlank() || ispaddedfield(t, i))
                    {
                        ret = ASPECIAL;
                    }
                }
                return (ret, null);
                        Fatalf("algtype1: unexpected type %v", t);
            return (0L, null);
        }

        // Generate a helper function to compute the hash of a value of type t.
        private static void genhash(ref types.Sym sym, ref types.Type t)
        {
            if (Debug['r'] != 0L)
            {
                fmt.Printf("genhash %v %v\n", sym, t);
            }
            lineno = autogeneratedPos; // less confusing than end of input
            dclcontext = PEXTERN;
            types.Markdcl(); 

            // func sym(p *T, h uintptr) uintptr
            var tfn = nod(OTFUNC, null, null);
            var n = namedfield("p", types.NewPtr(t));
            tfn.List.Append(n);
            var np = n.Left;
            n = namedfield("h", types.Types[TUINTPTR]);
            tfn.List.Append(n);
            var nh = n.Left;
            n = anonfield(types.Types[TUINTPTR]); // return value
            tfn.Rlist.Append(n);

            var fn = dclfunc(sym, tfn); 

            // genhash is only called for types that have equality but
            // cannot be handled by the standard algorithms,
            // so t must be either an array or a struct.

            if (t.Etype == types.TARRAY) 
                // An array of pure memory would be handled by the
                // standard algorithm, so the element type must not be
                // pure memory.
                var hashel = hashfor(t.Elem());

                n = nod(ORANGE, null, nod(OIND, np, null));
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
                na.Etype = 1L; // no escape to heap
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
                        if (!IsRegularMemory(f.Type))
                        {
                            hashel = hashfor(f.Type);
                            call = nod(OCALL, hashel, null);
                            nx = nodSym(OXDOT, np, f.Sym); // TODO: fields from other packages?
                            na = nod(OADDR, nx, null);
                            na.Etype = 1L; // no escape to heap
                            call.List.Append(na);
                            call.List.Append(nh);
                            fn.Nbody.Append(nod(OAS, nh, call));
                            i++;
                            continue;
                        } 

                        // Otherwise, hash a maximal length run of raw memory.
                        var (size, next) = memrun(t, i); 

                        // h = hashel(&p.first, size, h)
                        hashel = hashmem(f.Type);
                        call = nod(OCALL, hashel, null);
                        nx = nodSym(OXDOT, np, f.Sym); // TODO: fields from other packages?
                        na = nod(OADDR, nx, null);
                        na.Etype = 1L; // no escape to heap
                        call.List.Append(na);
                        call.List.Append(nh);
                        call.List.Append(nodintconst(size));
                        fn.Nbody.Append(nod(OAS, nh, call));

                        i = next;
                    }

                }
            else 
                Fatalf("genhash %v", t);
                        var r = nod(ORETURN, null, null);
            r.List.Append(nh);
            fn.Nbody.Append(r);

            if (Debug['r'] != 0L)
            {
                dumplist("genhash body", fn.Nbody);
            }
            funcbody();
            Curfn = fn;
            fn.Func.SetDupok(true);
            fn = typecheck(fn, Etop);
            typecheckslice(fn.Nbody.Slice(), Etop);
            Curfn = null;
            types.Popdcl();
            if (debug_dclstack != 0L)
            {
                testdclstack();
            } 

            // Disable safemode while compiling this code: the code we
            // generate internally can refer to unsafe.Pointer.
            // In this case it can happen if we need to generate an ==
            // for a struct containing a reflect.Value, which itself has
            // an unexported field of type unsafe.Pointer.
            var old_safemode = safemode;
            safemode = false;

            fn.Func.SetNilCheckDisabled(true);
            funccompile(fn);

            safemode = old_safemode;
        }

        private static ref Node hashfor(ref types.Type t)
        {
            ref types.Sym sym = default;

            {
                var (a, _) = algtype1(t);


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
                    sym = typesymprefix(".hash", t);

            }

            var n = newname(sym);
            n.SetClass(PFUNC);
            var tfn = nod(OTFUNC, null, null);
            tfn.List.Append(anonfield(types.NewPtr(t)));
            tfn.List.Append(anonfield(types.Types[TUINTPTR]));
            tfn.Rlist.Append(anonfield(types.Types[TUINTPTR]));
            tfn = typecheck(tfn, Etype);
            n.Type = tfn.Type;
            return n;
        }

        // geneq generates a helper function to
        // check equality of two values of type t.
        private static void geneq(ref types.Sym sym, ref types.Type t)
        {
            if (Debug['r'] != 0L)
            {
                fmt.Printf("geneq %v %v\n", sym, t);
            }
            lineno = autogeneratedPos; // less confusing than end of input
            dclcontext = PEXTERN;
            types.Markdcl(); 

            // func sym(p, q *T) bool
            var tfn = nod(OTFUNC, null, null);
            var n = namedfield("p", types.NewPtr(t));
            tfn.List.Append(n);
            var np = n.Left;
            n = namedfield("q", types.NewPtr(t));
            tfn.List.Append(n);
            var nq = n.Left;
            n = anonfield(types.Types[TBOOL]);
            tfn.Rlist.Append(n);

            var fn = dclfunc(sym, tfn); 

            // geneq is only called for types that have equality but
            // cannot be handled by the standard algorithms,
            // so t must be either an array or a struct.

            if (t.Etype == TARRAY) 
                // An array of pure memory would be handled by the
                // standard memequal, so the element type must not be
                // pure memory. Even if we unrolled the range loop,
                // each iteration would be a function call, so don't bother
                // unrolling.
                var nrange = nod(ORANGE, null, nod(OIND, np, null));

                var ni = newname(lookup("i"));
                ni.Type = types.Types[TINT];
                nrange.List.Set1(ni);
                nrange.SetColas(true);
                colasdefn(nrange.List.Slice(), nrange);
                ni = nrange.List.First(); 

                // if p[i] != q[i] { return false }
                var nx = nod(OINDEX, np, ni);

                nx.SetBounded(true);
                var ny = nod(OINDEX, nq, ni);
                ny.SetBounded(true);

                var nif = nod(OIF, null, null);
                nif.Left = nod(ONE, nx, ny);
                var r = nod(ORETURN, null, null);
                r.List.Append(nodbool(false));
                nif.Nbody.Append(r);
                nrange.Nbody.Append(nif);
                fn.Nbody.Append(nrange); 

                // return true
                var ret = nod(ORETURN, null, null);
                ret.List.Append(nodbool(true));
                fn.Nbody.Append(ret);
            else if (t.Etype == TSTRUCT) 
                ref Node cond = default;
                Action<ref Node> and = n =>
                {
                    if (cond == null)
                    {
                        cond = n;
                        return;
                    }
                    cond = nod(OANDAND, cond, n);
                } 

                // Walk the struct using memequal for runs of AMEM
                // and calling specific equality tests for the others.
; 

                // Walk the struct using memequal for runs of AMEM
                // and calling specific equality tests for the others.
                {
                    long i = 0L;
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
                        if (!IsRegularMemory(f.Type))
                        {
                            and(eqfield(np, nq, f.Sym));
                            i++;
                            continue;
                        } 

                        // Find maximal length run of memory-only fields.
                        var (size, next) = memrun(t, i); 

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
                                        and(eqfield(np, nq, f.Sym));
                                    }
                            else

                                    f = f__prev2;
                                }

                            }                            { 
                                // More than two fields: use memequal.
                                and(eqmem(np, nq, f.Sym, size));
                            }

                        }
                        i = next;
                    }

                }

                if (cond == null)
                {
                    cond = nodbool(true);
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
            Curfn = fn;
            fn.Func.SetDupok(true);
            fn = typecheck(fn, Etop);
            typecheckslice(fn.Nbody.Slice(), Etop);
            Curfn = null;
            types.Popdcl();
            if (debug_dclstack != 0L)
            {
                testdclstack();
            } 

            // Disable safemode while compiling this code: the code we
            // generate internally can refer to unsafe.Pointer.
            // In this case it can happen if we need to generate an ==
            // for a struct containing a reflect.Value, which itself has
            // an unexported field of type unsafe.Pointer.
            var old_safemode = safemode;
            safemode = false; 

            // Disable checknils while compiling this code.
            // We are comparing a struct or an array,
            // neither of which can be nil, and our comparisons
            // are shallow.
            fn.Func.SetNilCheckDisabled(true);
            funccompile(fn);

            safemode = old_safemode;
        }

        // eqfield returns the node
        //     p.field == q.field
        private static ref Node eqfield(ref Node p, ref Node q, ref types.Sym field)
        {
            var nx = nodSym(OXDOT, p, field);
            var ny = nodSym(OXDOT, q, field);
            var ne = nod(OEQ, nx, ny);
            return ne;
        }

        // eqmem returns the node
        //     memequal(&p.field, &q.field [, size])
        private static ref Node eqmem(ref Node p, ref Node q, ref types.Sym field, long size)
        {
            var nx = nod(OADDR, nodSym(OXDOT, p, field), null);
            nx.Etype = 1L; // does not escape
            var ny = nod(OADDR, nodSym(OXDOT, q, field), null);
            ny.Etype = 1L; // does not escape
            nx = typecheck(nx, Erv);
            ny = typecheck(ny, Erv);

            var (fn, needsize) = eqmemfunc(size, nx.Type.Elem());
            var call = nod(OCALL, fn, null);
            call.List.Append(nx);
            call.List.Append(ny);
            if (needsize)
            {
                call.List.Append(nodintconst(size));
            }
            return call;
        }

        private static (ref Node, bool) eqmemfunc(long size, ref types.Type t)
        {
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
            return (fn, needsize);
        }

        // memrun finds runs of struct fields for which memory-only algs are appropriate.
        // t is the parent struct type, and start is the field index at which to start the run.
        // size is the length in bytes of the memory included in the run.
        // next is the index just after the end of the memory run.
        private static (long, long) memrun(ref types.Type t, long start)
        {
            next = start;
            while (true)
            {
                next++;
                if (next == t.NumFields())
                {
                    break;
                } 
                // Stop run after a padded field.
                if (ispaddedfield(t, next - 1L))
                {
                    break;
                } 
                // Also, stop before a blank or non-memory field.
                {
                    var f = t.Field(next);

                    if (f.Sym.IsBlank() || !IsRegularMemory(f.Type))
                    {
                        break;
                    }

                }
            }

            return (t.Field(next - 1L).End() - t.Field(start).Offset, next);
        }

        // ispaddedfield reports whether the i'th field of struct type t is followed
        // by padding.
        private static bool ispaddedfield(ref types.Type t, long i)
        {
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
