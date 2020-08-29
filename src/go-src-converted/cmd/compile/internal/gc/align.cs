// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:25:31 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\align.go
using types = go.cmd.compile.@internal.types_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // sizeCalculationDisabled indicates whether it is safe
        // to calculate Types' widths and alignments. See dowidth.
        private static bool sizeCalculationDisabled = default;

        // machine size and rounding alignment is dictated around
        // the size of a pointer, set in betypeinit (see ../amd64/galign.go).
        private static long defercalc = default;

        public static long Rnd(long o, long r)
        {
            if (r < 1L || r > 8L || r & (r - 1L) != 0L)
            {
                Fatalf("rnd %d", r);
            }
            return (o + r - 1L) & ~(r - 1L);
        }

        // expandiface computes the method set for interface type t by
        // expanding embedded interfaces.
        private static void expandiface(ref types.Type t)
        {
            slice<ref types.Field> fields = default;
            foreach (var (_, m) in t.Methods().Slice())
            {
                if (m.Sym != null)
                {
                    fields = append(fields, m);
                    checkwidth(m.Type);
                    continue;
                }
                if (!m.Type.IsInterface())
                {
                    yyerrorl(asNode(m.Nname).Pos, "interface contains embedded non-interface %v", m.Type);
                    m.SetBroke(true);
                    t.SetBroke(true); 
                    // Add to fields so that error messages
                    // include the broken embedded type when
                    // printing t.
                    // TODO(mdempsky): Revisit this.
                    fields = append(fields, m);
                    continue;
                } 

                // Embedded interface: duplicate all methods
                // (including broken ones, if any) and add to t's
                // method set.
                foreach (var (_, t1) in m.Type.Fields().Slice())
                {
                    var f = types.NewField();
                    f.Type = t1.Type;
                    f.SetBroke(t1.Broke());
                    f.Sym = t1.Sym;
                    f.Nname = m.Nname; // preserve embedding position
                    fields = append(fields, f);
                }
            }
            sort.Sort(methcmp(fields)); 

            // Access fields directly to avoid recursively calling dowidth
            // within Type.Fields().
            t.Extra._<ref types.Interface>().Fields.Set(fields);
        }

        private static void offmod(ref types.Type t)
        {
            var o = int32(0L);
            foreach (var (_, f) in t.Fields().Slice())
            {
                f.Offset = int64(o);
                o += int32(Widthptr);
                if (int64(o) >= thearch.MAXWIDTH)
                {
                    yyerror("interface too large");
                    o = int32(Widthptr);
                }
            }
        }

        private static long widstruct(ref types.Type errtype, ref types.Type t, long o, long flag)
        {
            var starto = o;
            var maxalign = int32(flag);
            if (maxalign < 1L)
            {
                maxalign = 1L;
            }
            var lastzero = int64(0L);
            foreach (var (_, f) in t.Fields().Slice())
            {
                if (f.Type == null)
                { 
                    // broken field, just skip it so that other valid fields
                    // get a width.
                    continue;
                }
                dowidth(f.Type);
                if (int32(f.Type.Align) > maxalign)
                {
                    maxalign = int32(f.Type.Align);
                }
                if (f.Type.Align > 0L)
                {
                    o = Rnd(o, int64(f.Type.Align));
                }
                f.Offset = o;
                if (asNode(f.Nname) != null)
                { 
                    // addrescapes has similar code to update these offsets.
                    // Usually addrescapes runs after widstruct,
                    // in which case we could drop this,
                    // but function closure functions are the exception.
                    // NOTE(rsc): This comment may be stale.
                    // It's possible the ordering has changed and this is
                    // now the common case. I'm not sure.
                    if (asNode(f.Nname).Name.Param.Stackcopy != null)
                    {
                        asNode(f.Nname).Name.Param.Stackcopy.Xoffset;

                        o;
                        asNode(f.Nname).Xoffset;

                        0L;
                    }
                    else
                    {
                        asNode(f.Nname).Xoffset;

                        o;
                    }
                }
                var w = f.Type.Width;
                if (w < 0L)
                {
                    Fatalf("invalid width %d", f.Type.Width);
                }
                if (w == 0L)
                {
                    lastzero = o;
                }
                o += w;
                var maxwidth = thearch.MAXWIDTH; 
                // On 32-bit systems, reflect tables impose an additional constraint
                // that each field start offset must fit in 31 bits.
                if (maxwidth < 1L << (int)(32L))
                {
                    maxwidth = 1L << (int)(31L) - 1L;
                }
                if (o >= maxwidth)
                {
                    yyerror("type %L too large", errtype);
                    o = 8L; // small but nonzero
                }
            } 

            // For nonzero-sized structs which end in a zero-sized thing, we add
            // an extra byte of padding to the type. This padding ensures that
            // taking the address of the zero-sized thing can't manufacture a
            // pointer to the next object in the heap. See issue 9401.
            if (flag == 1L && o > starto && o == lastzero)
            {
                o++;
            } 

            // final width is rounded
            if (flag != 0L)
            {
                o = Rnd(o, int64(maxalign));
            }
            t.Align = uint8(maxalign); 

            // type width only includes back to first field's offset
            t.Width = o - starto;

            return o;
        }

        // dowidth calculates and stores the size and alignment for t.
        // If sizeCalculationDisabled is set, and the size/alignment
        // have not already been calculated, it calls Fatal.
        // This is used to prevent data races in the back end.
        private static void dowidth(ref types.Type t)
        {
            if (Widthptr == 0L)
            {
                Fatalf("dowidth without betypeinit");
            }
            if (t == null)
            {
                return;
            }
            if (t.Width == -2L)
            {
                if (!t.Broke())
                {
                    t.SetBroke(true);
                    yyerrorl(asNode(t.Nod).Pos, "invalid recursive type %v", t);
                }
                t.Width = 0L;
                t.Align = 1L;
                return;
            }
            if (t.WidthCalculated())
            {
                return;
            }
            if (sizeCalculationDisabled)
            {
                if (t.Broke())
                { 
                    // break infinite recursion from Fatal call below
                    return;
                }
                t.SetBroke(true);
                Fatalf("width not calculated: %v", t);
            } 

            // break infinite recursion if the broken recursive type
            // is referenced again
            if (t.Broke() && t.Width == 0L)
            {
                return;
            } 

            // defer checkwidth calls until after we're done
            defercalc++;

            var lno = lineno;
            if (asNode(t.Nod) != null)
            {
                lineno = asNode(t.Nod).Pos;
            }
            t.Width = -2L;
            t.Align = 0L;

            var et = t.Etype;

            if (et == TFUNC || et == TCHAN || et == TMAP || et == TSTRING) 
                break; 

                // simtype == 0 during bootstrap
            else 
                if (simtype[t.Etype] != 0L)
                {
                    et = simtype[t.Etype];
                }
                        var w = int64(0L);

            if (et == TINT8 || et == TUINT8 || et == TBOOL) 
                // bool is int8
                w = 1L;
            else if (et == TINT16 || et == TUINT16) 
                w = 2L;
            else if (et == TINT32 || et == TUINT32 || et == TFLOAT32) 
                w = 4L;
            else if (et == TINT64 || et == TUINT64 || et == TFLOAT64) 
                w = 8L;
                t.Align = uint8(Widthreg);
            else if (et == TCOMPLEX64) 
                w = 8L;
                t.Align = 4L;
            else if (et == TCOMPLEX128) 
                w = 16L;
                t.Align = uint8(Widthreg);
            else if (et == TPTR32) 
                w = 4L;
                checkwidth(t.Elem());
            else if (et == TPTR64) 
                w = 8L;
                checkwidth(t.Elem());
            else if (et == TUNSAFEPTR) 
                w = int64(Widthptr);
            else if (et == TINTER) // implemented as 2 pointers
                w = 2L * int64(Widthptr);
                t.Align = uint8(Widthptr);
                expandiface(t);
            else if (et == TCHAN) // implemented as pointer
                w = int64(Widthptr);

                checkwidth(t.Elem()); 

                // make fake type to check later to
                // trigger channel argument check.
                var t1 = types.NewChanArgs(t);
                checkwidth(t1);
            else if (et == TCHANARGS) 
                t1 = t.ChanArgs();
                dowidth(t1); // just in case
                if (t1.Elem().Width >= 1L << (int)(16L))
                {
                    yyerror("channel element type too large (>64kB)");
                }
                w = 1L; // anything will do
            else if (et == TMAP) // implemented as pointer
                w = int64(Widthptr);
                checkwidth(t.Val());
                checkwidth(t.Key());
            else if (et == TFORW) // should have been filled in
                if (!t.Broke())
                {
                    t.SetBroke(true);
                    yyerror("invalid recursive type %v", t);
                }
                w = 1L; // anything will do
            else if (et == TANY) 
                // dummy type; should be replaced before use.
                Fatalf("dowidth any");
            else if (et == TSTRING) 
                if (sizeof_String == 0L)
                {
                    Fatalf("early dowidth string");
                }
                w = int64(sizeof_String);
                t.Align = uint8(Widthptr);
            else if (et == TARRAY) 
                if (t.Elem() == null)
                {
                    break;
                }
                if (t.IsDDDArray())
                {
                    if (!t.Broke())
                    {
                        yyerror("use of [...] array outside of array literal");
                        t.SetBroke(true);
                    }
                    break;
                }
                dowidth(t.Elem());
                if (t.Elem().Width != 0L)
                {
                    var cap = (uint64(thearch.MAXWIDTH) - 1L) / uint64(t.Elem().Width);
                    if (uint64(t.NumElem()) > cap)
                    {
                        yyerror("type %L larger than address space", t);
                    }
                }
                w = t.NumElem() * t.Elem().Width;
                t.Align = t.Elem().Align;
            else if (et == TSLICE) 
                if (t.Elem() == null)
                {
                    break;
                }
                w = int64(sizeof_Array);
                checkwidth(t.Elem());
                t.Align = uint8(Widthptr);
            else if (et == TSTRUCT) 
                if (t.IsFuncArgStruct())
                {
                    Fatalf("dowidth fn struct %v", t);
                }
                w = widstruct(t, t, 0L, 1L); 

                // make fake type to check later to
                // trigger function argument computation.
            else if (et == TFUNC) 
                t1 = types.NewFuncArgs(t);
                checkwidth(t1);
                w = int64(Widthptr); // width of func type is pointer

                // function is 3 cated structures;
                // compute their widths as side-effect.
            else if (et == TFUNCARGS) 
                t1 = t.FuncArgs();
                w = widstruct(t1, t1.Recvs(), 0L, 0L);
                w = widstruct(t1, t1.Params(), w, Widthreg);
                w = widstruct(t1, t1.Results(), w, Widthreg);
                t1.Extra._<ref types.Func>().Argwid = w;
                if (w % int64(Widthreg) != 0L)
                {
                    Warn("bad type %v %d\n", t1, w);
                }
                t.Align = 1L;
            else 
                Fatalf("dowidth: unknown type: %v", t); 

                // compiler-specific stuff
                        if (Widthptr == 4L && w != int64(int32(w)))
            {
                yyerror("type %v too large", t);
            }
            t.Width = w;
            if (t.Align == 0L)
            {
                if (w > 8L || w & (w - 1L) != 0L || w == 0L)
                {
                    Fatalf("invalid alignment for %v", t);
                }
                t.Align = uint8(w);
            }
            if (t.Etype == TINTER)
            { 
                // We defer calling these functions until after
                // setting t.Width and t.Align so the recursive calls
                // to dowidth within t.Fields() will succeed.
                checkdupfields("method", t);
                offmod(t);
            }
            lineno = lno;

            if (defercalc == 1L)
            {
                resumecheckwidth();
            }
            else
            {
                defercalc--;
            }
        }

        // when a type's width should be known, we call checkwidth
        // to compute it.  during a declaration like
        //
        //    type T *struct { next T }
        //
        // it is necessary to defer the calculation of the struct width
        // until after T has been initialized to be a pointer to that struct.
        // similarly, during import processing structs may be used
        // before their definition.  in those situations, calling
        // defercheckwidth() stops width calculations until
        // resumecheckwidth() is called, at which point all the
        // checkwidths that were deferred are executed.
        // dowidth should only be called when the type's size
        // is needed immediately.  checkwidth makes sure the
        // size is evaluated eventually.

        private static slice<ref types.Type> deferredTypeStack = default;

        private static void checkwidth(ref types.Type t)
        {
            if (t == null)
            {
                return;
            } 

            // function arg structs should not be checked
            // outside of the enclosing function.
            if (t.IsFuncArgStruct())
            {
                Fatalf("checkwidth %v", t);
            }
            if (defercalc == 0L)
            {
                dowidth(t);
                return;
            }
            if (t.Deferwidth())
            {
                return;
            }
            t.SetDeferwidth(true);

            deferredTypeStack = append(deferredTypeStack, t);
        }

        private static void defercheckwidth()
        { 
            // we get out of sync on syntax errors, so don't be pedantic.
            if (defercalc != 0L && nerrors == 0L)
            {
                Fatalf("defercheckwidth");
            }
            defercalc = 1L;
        }

        private static void resumecheckwidth()
        {
            if (defercalc == 0L)
            {
                Fatalf("resumecheckwidth");
            }
            while (len(deferredTypeStack) > 0L)
            {
                var t = deferredTypeStack[len(deferredTypeStack) - 1L];
                deferredTypeStack = deferredTypeStack[..len(deferredTypeStack) - 1L];
                t.SetDeferwidth(false);
                dowidth(t);
            }


            defercalc = 0L;
        }
    }
}}}}
