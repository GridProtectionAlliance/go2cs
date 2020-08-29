// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:07 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\gen.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ref obj.LSym sysfunc(@string name)
        {
            return Runtimepkg.Lookup(name).Linksym();
        }

        // isParamStackCopy reports whether this is the on-stack copy of a
        // function parameter that moved to the heap.
        private static bool isParamStackCopy(this ref Node n)
        {
            return n.Op == ONAME && (n.Class() == PPARAM || n.Class() == PPARAMOUT) && n.Name.Param.Heapaddr != null;
        }

        // isParamHeapCopy reports whether this is the on-heap copy of
        // a function parameter that moved to the heap.
        private static bool isParamHeapCopy(this ref Node n)
        {
            return n.Op == ONAME && n.Class() == PAUTOHEAP && n.Name.Param.Stackcopy != null;
        }

        // autotmpname returns the name for an autotmp variable numbered n.
        private static @string autotmpname(long n)
        { 
            // Give each tmp a different name so that they can be registerized.
            // Add a preceding . to avoid clashing with legal names.
            const @string prefix = ".autotmp_"; 
            // Start with a buffer big enough to hold a large n.
 
            // Start with a buffer big enough to hold a large n.
            slice<byte> b = (slice<byte>)prefix + "      "[..len(prefix)];
            b = strconv.AppendInt(b, int64(n), 10L);
            return types.InternString(b);
        }

        // make a new Node off the books
        private static ref Node tempAt(src.XPos pos, ref Node curfn, ref types.Type t)
        {
            if (curfn == null)
            {
                Fatalf("no curfn for tempname");
            }
            if (curfn.Func.Closure != null && curfn.Op == OCLOSURE)
            {
                Dump("tempname", curfn);
                Fatalf("adding tempname to wrong closure function");
            }
            if (t == null)
            {
                Fatalf("tempname called with nil type");
            }
            types.Sym s = ref new types.Sym(Name:autotmpname(len(curfn.Func.Dcl)),Pkg:localpkg,);
            var n = newnamel(pos, s);
            s.Def = asTypesNode(n);
            n.Type = t;
            n.SetClass(PAUTO);
            n.Esc = EscNever;
            n.Name.Curfn = curfn;
            n.Name.SetUsed(true);
            n.Name.SetAutoTemp(true);
            curfn.Func.Dcl = append(curfn.Func.Dcl, n);

            dowidth(t);

            return n.Orig;
        }

        private static ref Node temp(ref types.Type t)
        {
            return tempAt(lineno, Curfn, t);
        }
    }
}}}}
