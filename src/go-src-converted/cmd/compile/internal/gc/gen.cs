// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:30 UTC
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
        // sysfunc looks up Go function name in package runtime. This function
        // must follow the internal calling convention.
        private static ptr<obj.LSym> sysfunc(@string name)
        {
            var s = Runtimepkg.Lookup(name);
            s.SetFunc(true);
            return _addr_s.Linksym()!;
        }

        // sysvar looks up a variable (or assembly function) name in package
        // runtime. If this is a function, it may have a special calling
        // convention.
        private static ptr<obj.LSym> sysvar(@string name)
        {
            return _addr_Runtimepkg.Lookup(name).Linksym()!;
        }

        // isParamStackCopy reports whether this is the on-stack copy of a
        // function parameter that moved to the heap.
        private static bool isParamStackCopy(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == ONAME && (n.Class() == PPARAM || n.Class() == PPARAMOUT) && n.Name.Param.Heapaddr != null;
        }

        // isParamHeapCopy reports whether this is the on-heap copy of
        // a function parameter that moved to the heap.
        private static bool isParamHeapCopy(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == ONAME && n.Class() == PAUTOHEAP && n.Name.Param.Stackcopy != null;
        }

        // autotmpname returns the name for an autotmp variable numbered n.
        private static @string autotmpname(long n)
        { 
            // Give each tmp a different name so that they can be registerized.
            // Add a preceding . to avoid clashing with legal names.
            const @string prefix = (@string)".autotmp_"; 
            // Start with a buffer big enough to hold a large n.
 
            // Start with a buffer big enough to hold a large n.
            slice<byte> b = (slice<byte>)prefix + "      "[..len(prefix)];
            b = strconv.AppendInt(b, int64(n), 10L);
            return types.InternString(b);

        }

        // make a new Node off the books
        private static ptr<Node> tempAt(src.XPos pos, ptr<Node> _addr_curfn, ptr<types.Type> _addr_t)
        {
            ref Node curfn = ref _addr_curfn.val;
            ref types.Type t = ref _addr_t.val;

            if (curfn == null)
            {
                Fatalf("no curfn for tempAt");
            }

            if (curfn.Func.Closure != null && curfn.Op == OCLOSURE)
            {
                Dump("tempAt", curfn);
                Fatalf("adding tempAt to wrong closure function");
            }

            if (t == null)
            {
                Fatalf("tempAt called with nil type");
            }

            ptr<types.Sym> s = addr(new types.Sym(Name:autotmpname(len(curfn.Func.Dcl)),Pkg:localpkg,));
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

            return _addr_n.Orig!;

        }

        private static ptr<Node> temp(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return _addr_tempAt(lineno, _addr_Curfn, _addr_t)!;
        }
    }
}}}}
