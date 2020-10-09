// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:40:36 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\align.go
using types = go.cmd.compile.@internal.types_package;
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
        private static void expandiface(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var seen = make_map<ptr<types.Sym>, ptr<types.Field>>();
            slice<ptr<types.Field>> methods = default;

            Action<ptr<types.Field>, bool> addMethod = (m, @explicit) =>
            {
                {
                    var prev = seen[m.Sym];


                    if (prev == null) 
                        seen[m.Sym] = m;
                    else if (langSupported(1L, 14L, t.Pkg()) && !explicit && types.Identical(m.Type, prev.Type)) 
                        return ;
                    else 
                        yyerrorl(m.Pos, "duplicate method %s", m.Sym.Name);

                }
                methods = append(methods, m);

            }
;

            {
                var m__prev1 = m;

                foreach (var (_, __m) in t.Methods().Slice())
                {
                    m = __m;
                    if (m.Sym == null)
                    {
                        continue;
                    }

                    checkwidth(_addr_m.Type);
                    addMethod(m, true);

                }

                m = m__prev1;
            }

            {
                var m__prev1 = m;

                foreach (var (_, __m) in t.Methods().Slice())
                {
                    m = __m;
                    if (m.Sym != null)
                    {
                        continue;
                    }

                    if (!m.Type.IsInterface())
                    {
                        yyerrorl(m.Pos, "interface contains embedded non-interface %v", m.Type);
                        m.SetBroke(true);
                        t.SetBroke(true); 
                        // Add to fields so that error messages
                        // include the broken embedded type when
                        // printing t.
                        // TODO(mdempsky): Revisit this.
                        methods = append(methods, m);
                        continue;

                    } 

                    // Embedded interface: duplicate all methods
                    // (including broken ones, if any) and add to t's
                    // method set.
                    foreach (var (_, t1) in m.Type.Fields().Slice())
                    {
                        var f = types.NewField();
                        f.Pos = m.Pos; // preserve embedding position
                        f.Sym = t1.Sym;
                        f.Type = t1.Type;
                        f.SetBroke(t1.Broke());
                        addMethod(f, false);

                    }

                }

                m = m__prev1;
            }

            sort.Sort(methcmp(methods));

            if (int64(len(methods)) >= thearch.MAXWIDTH / int64(Widthptr))
            {
                yyerror("interface too large");
            }

            {
                var m__prev1 = m;

                foreach (var (__i, __m) in methods)
                {
                    i = __i;
                    m = __m;
                    m.Offset = int64(i) * int64(Widthptr);
                } 

                // Access fields directly to avoid recursively calling dowidth
                // within Type.Fields().

                m = m__prev1;
            }

            t.Extra._<ptr<types.Interface>>().Fields.Set(methods);

        }

        private static long widstruct(ptr<types.Type> _addr_errtype, ptr<types.Type> _addr_t, long o, long flag)
        {
            ref types.Type errtype = ref _addr_errtype.val;
            ref types.Type t = ref _addr_t.val;

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

                dowidth(_addr_f.Type);
                if (int32(f.Type.Align) > maxalign)
                {
                    maxalign = int32(f.Type.Align);
                }

                if (f.Type.Align > 0L)
                {
                    o = Rnd(o, int64(f.Type.Align));
                }

                f.Offset = o;
                {
                    var n = asNode(f.Nname);

                    if (n != null)
                    { 
                        // addrescapes has similar code to update these offsets.
                        // Usually addrescapes runs after widstruct,
                        // in which case we could drop this,
                        // but function closure functions are the exception.
                        // NOTE(rsc): This comment may be stale.
                        // It's possible the ordering has changed and this is
                        // now the common case. I'm not sure.
                        if (n.Name.Param.Stackcopy != null)
                        {
                            n.Name.Param.Stackcopy.Xoffset = o;
                            n.Xoffset = 0L;
                        }
                        else
                        {
                            n.Xoffset = o;
                        }

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
        private static void dowidth(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
            // Calling dowidth when typecheck tracing enabled is not safe.
            // See issue #33658.
            if (enableTrace && skipDowidthForTracing)
            {
                return ;
            }

            if (Widthptr == 0L)
            {
                Fatalf("dowidth without betypeinit");
            }

            if (t == null)
            {
                return ;
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
                return ;

            }

            if (t.WidthCalculated())
            {
                return ;
            }

            if (sizeCalculationDisabled)
            {
                if (t.Broke())
                { 
                    // break infinite recursion from Fatal call below
                    return ;

                }

                t.SetBroke(true);
                Fatalf("width not calculated: %v", t);

            } 

            // break infinite recursion if the broken recursive type
            // is referenced again
            if (t.Broke() && t.Width == 0L)
            {
                return ;
            } 

            // defer checkwidth calls until after we're done
            defercheckwidth();

            var lno = lineno;
            if (asNode(t.Nod) != null)
            {
                lineno = asNode(t.Nod).Pos;
            }

            t.Width = -2L;
            t.Align = 0L; // 0 means use t.Width, below

            var et = t.Etype;

            if (et == TFUNC || et == TCHAN || et == TMAP || et == TSTRING) 
                break; 

                // simtype == 0 during bootstrap
            else 
                if (simtype[t.Etype] != 0L)
                {
                    et = simtype[t.Etype];
                }

                        long w = default;

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
            else if (et == TPTR) 
                w = int64(Widthptr);
                checkwidth(_addr_t.Elem());
            else if (et == TUNSAFEPTR) 
                w = int64(Widthptr);
            else if (et == TINTER) // implemented as 2 pointers
                w = 2L * int64(Widthptr);
                t.Align = uint8(Widthptr);
                expandiface(_addr_t);
            else if (et == TCHAN) // implemented as pointer
                w = int64(Widthptr);

                checkwidth(_addr_t.Elem()); 

                // make fake type to check later to
                // trigger channel argument check.
                var t1 = types.NewChanArgs(t);
                checkwidth(_addr_t1);
            else if (et == TCHANARGS) 
                t1 = t.ChanArgs();
                dowidth(_addr_t1); // just in case
                if (t1.Elem().Width >= 1L << (int)(16L))
                {
                    yyerror("channel element type too large (>64kB)");
                }

                w = 1L; // anything will do
            else if (et == TMAP) // implemented as pointer
                w = int64(Widthptr);
                checkwidth(_addr_t.Elem());
                checkwidth(_addr_t.Key());
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
                if (sizeofString == 0L)
                {
                    Fatalf("early dowidth string");
                }

                w = sizeofString;
                t.Align = uint8(Widthptr);
            else if (et == TARRAY) 
                if (t.Elem() == null)
                {
                    break;
                }

                dowidth(_addr_t.Elem());
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

                w = sizeofSlice;
                checkwidth(_addr_t.Elem());
                t.Align = uint8(Widthptr);
            else if (et == TSTRUCT) 
                if (t.IsFuncArgStruct())
                {
                    Fatalf("dowidth fn struct %v", t);
                }

                w = widstruct(_addr_t, _addr_t, 0L, 1L); 

                // make fake type to check later to
                // trigger function argument computation.
            else if (et == TFUNC) 
                t1 = types.NewFuncArgs(t);
                checkwidth(_addr_t1);
                w = int64(Widthptr); // width of func type is pointer

                // function is 3 cated structures;
                // compute their widths as side-effect.
            else if (et == TFUNCARGS) 
                t1 = t.FuncArgs();
                w = widstruct(_addr_t1, _addr_t1.Recvs(), 0L, 0L);
                w = widstruct(_addr_t1, _addr_t1.Params(), w, Widthreg);
                w = widstruct(_addr_t1, _addr_t1.Results(), w, Widthreg);
                t1.Extra._<ptr<types.Func>>().Argwid = w;
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
                if (w == 0L || w > 8L || w & (w - 1L) != 0L)
                {
                    Fatalf("invalid alignment for %v", t);
                }

                t.Align = uint8(w);

            }

            lineno = lno;

            resumecheckwidth();

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

        private static slice<ptr<types.Type>> deferredTypeStack = default;

        private static void checkwidth(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return ;
            } 

            // function arg structs should not be checked
            // outside of the enclosing function.
            if (t.IsFuncArgStruct())
            {
                Fatalf("checkwidth %v", t);
            }

            if (defercalc == 0L)
            {
                dowidth(_addr_t);
                return ;
            } 

            // if type has not yet been pushed on deferredTypeStack yet, do it now
            if (!t.Deferwidth())
            {
                t.SetDeferwidth(true);
                deferredTypeStack = append(deferredTypeStack, t);
            }

        }

        private static void defercheckwidth()
        {
            defercalc++;
        }

        private static void resumecheckwidth()
        {
            if (defercalc == 1L)
            {
                while (len(deferredTypeStack) > 0L)
                {
                    var t = deferredTypeStack[len(deferredTypeStack) - 1L];
                    deferredTypeStack = deferredTypeStack[..len(deferredTypeStack) - 1L];
                    t.SetDeferwidth(false);
                    dowidth(_addr_t);
                }


            }

            defercalc--;

        }
    }
}}}}
