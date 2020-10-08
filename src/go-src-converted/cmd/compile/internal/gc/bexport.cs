// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:27:58 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bexport.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private partial struct exporter
        {
            public map<ptr<types.Type>, bool> marked; // types already seen by markType
        }

        // markType recursively visits types reachable from t to identify
        // functions whose inline bodies may be needed.
        private static void markType(this ptr<exporter> _addr_p, ptr<types.Type> _addr_t)
        {
            ref exporter p = ref _addr_p.val;
            ref types.Type t = ref _addr_t.val;

            if (p.marked[t])
            {
                return ;
            }

            p.marked[t] = true; 

            // If this is a named type, mark all of its associated
            // methods. Skip interface types because t.Methods contains
            // only their unexpanded method set (i.e., exclusive of
            // interface embeddings), and the switch statement below
            // handles their full method set.
            if (t.Sym != null && t.Etype != TINTER)
            {
                foreach (var (_, m) in t.Methods().Slice())
                {
                    if (types.IsExported(m.Sym.Name))
                    {
                        p.markType(m.Type);
                    }

                }

            } 

            // Recursively mark any types that can be produced given a
            // value of type t: dereferencing a pointer; indexing or
            // iterating over an array, slice, or map; receiving from a
            // channel; accessing a struct field or interface method; or
            // calling a function.
            //
            // Notably, we don't mark function parameter types, because
            // the user already needs some way to construct values of
            // those types.

            if (t.Etype == TPTR || t.Etype == TARRAY || t.Etype == TSLICE) 
                p.markType(t.Elem());
            else if (t.Etype == TCHAN) 
                if (t.ChanDir().CanRecv())
                {
                    p.markType(t.Elem());
                }

            else if (t.Etype == TMAP) 
                p.markType(t.Key());
                p.markType(t.Elem());
            else if (t.Etype == TSTRUCT) 
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        if (types.IsExported(f.Sym.Name) || f.Embedded != 0L)
                        {
                            p.markType(f.Type);
                        }

                    }

                    f = f__prev1;
                }
            else if (t.Etype == TFUNC) 
                // If t is the type of a function or method, then
                // t.Nname() is its ONAME. Mark its inline body and
                // any recursively called functions for export.
                inlFlood(asNode(t.Nname()));

                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.Results().FieldSlice())
                    {
                        f = __f;
                        p.markType(f.Type);
                    }

                    f = f__prev1;
                }
            else if (t.Etype == TINTER) 
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        if (types.IsExported(f.Sym.Name))
                        {
                            p.markType(f.Type);
                        }

                    }

                    f = f__prev1;
                }
                    }

        // deltaNewFile is a magic line delta offset indicating a new file.
        // We use -64 because it is rare; see issue 20080 and CL 41619.
        // -64 is the smallest int that fits in a single byte as a varint.
        private static readonly long deltaNewFile = (long)-64L;

        // ----------------------------------------------------------------------------
        // Export format

        // Tags. Must be < 0.


        // ----------------------------------------------------------------------------
        // Export format

        // Tags. Must be < 0.
 
        // Objects
        private static readonly var packageTag = (var)-(iota + 1L);
        private static readonly var constTag = (var)0;
        private static readonly var typeTag = (var)1;
        private static readonly var varTag = (var)2;
        private static readonly var funcTag = (var)3;
        private static readonly var endTag = (var)4; 

        // Types
        private static readonly var namedTag = (var)5;
        private static readonly var arrayTag = (var)6;
        private static readonly var sliceTag = (var)7;
        private static readonly var dddTag = (var)8;
        private static readonly var structTag = (var)9;
        private static readonly var pointerTag = (var)10;
        private static readonly var signatureTag = (var)11;
        private static readonly var interfaceTag = (var)12;
        private static readonly var mapTag = (var)13;
        private static readonly var chanTag = (var)14; 

        // Values
        private static readonly var falseTag = (var)15;
        private static readonly var trueTag = (var)16;
        private static readonly var int64Tag = (var)17;
        private static readonly var floatTag = (var)18;
        private static readonly var fractionTag = (var)19; // not used by gc
        private static readonly var complexTag = (var)20;
        private static readonly var stringTag = (var)21;
        private static readonly var nilTag = (var)22;
        private static readonly var unknownTag = (var)23; // not used by gc (only appears in packages with errors)

        // Type aliases
        private static readonly var aliasTag = (var)24;


        // untype returns the "pseudo" untyped type for a Ctype (import/export use only).
        // (we can't use a pre-initialized array because we must be sure all types are
        // set up)
        private static ptr<types.Type> untype(Ctype ctype)
        {

            if (ctype == CTINT) 
                return _addr_types.Idealint!;
            else if (ctype == CTRUNE) 
                return _addr_types.Idealrune!;
            else if (ctype == CTFLT) 
                return _addr_types.Idealfloat!;
            else if (ctype == CTCPLX) 
                return _addr_types.Idealcomplex!;
            else if (ctype == CTSTR) 
                return _addr_types.Idealstring!;
            else if (ctype == CTBOOL) 
                return _addr_types.Idealbool!;
            else if (ctype == CTNIL) 
                return _addr_types.Types[TNIL]!;
                        Fatalf("exporter: unknown Ctype");
            return _addr_null!;

        }

        private static slice<ptr<types.Type>> predecl = default; // initialized lazily

        private static slice<ptr<types.Type>> predeclared()
        {
            if (predecl == null)
            { 
                // initialize lazily to be sure that all
                // elements have been initialized before
                predecl = new slice<ptr<types.Type>>(new ptr<types.Type>[] { types.Types[TBOOL], types.Types[TINT], types.Types[TINT8], types.Types[TINT16], types.Types[TINT32], types.Types[TINT64], types.Types[TUINT], types.Types[TUINT8], types.Types[TUINT16], types.Types[TUINT32], types.Types[TUINT64], types.Types[TUINTPTR], types.Types[TFLOAT32], types.Types[TFLOAT64], types.Types[TCOMPLEX64], types.Types[TCOMPLEX128], types.Types[TSTRING], types.Bytetype, types.Runetype, types.Errortype, untype(CTBOOL), untype(CTINT), untype(CTRUNE), untype(CTFLT), untype(CTCPLX), untype(CTSTR), untype(CTNIL), types.Types[TUNSAFEPTR], types.Types[Txxx], types.Types[TANY] });

            }

            return predecl;

        }
    }
}}}}
