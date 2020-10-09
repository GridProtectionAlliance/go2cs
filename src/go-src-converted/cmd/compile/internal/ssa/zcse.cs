// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:39:43 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\zcse.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // zcse does an initial pass of common-subexpression elimination on the
        // function for values with zero arguments to allow the more expensive cse
        // to begin with a reduced number of values. Values are just relinked,
        // nothing is deleted. A subsequent deadcode pass is required to actually
        // remove duplicate expressions.
        private static void zcse(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var vals = make_map<vkey, ptr<Value>>();

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        long i__prev2 = i;

                        for (long i = 0L; i < len(b.Values); i++)
                        {
                            var v = b.Values[i];
                            if (opcodeTable[v.Op].argLen == 0L)
                            {
                                vkey key = new vkey(v.Op,keyFor(v),v.Aux,v.Type);
                                if (vals[key] == null)
                                {
                                    vals[key] = v;
                                    if (b != f.Entry)
                                    { 
                                        // Move v to the entry block so it will dominate every block
                                        // where we might use it. This prevents the need for any dominator
                                        // calculations in this pass.
                                        v.Block = f.Entry;
                                        f.Entry.Values = append(f.Entry.Values, v);
                                        var last = len(b.Values) - 1L;
                                        b.Values[i] = b.Values[last];
                                        b.Values[last] = null;
                                        b.Values = b.Values[..last];

                                        i--; // process b.Values[i] again
                                    }
                                }
                            }
                        }

                        i = i__prev2;
                    }

                }
                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            {
                                long i__prev3 = i;

                                foreach (var (__i, __a) in v.Args)
                                {
                                    i = __i;
                                    a = __a;
                                    if (opcodeTable[a.Op].argLen == 0L)
                                    {
                                        key = new vkey(a.Op,keyFor(a),a.Aux,a.Type);
                                        {
                                            var (rv, ok) = vals[key];

                                            if (ok)
                                            {
                                                v.SetArg(i, rv);
                                            }
                                        }

                                    }
                                }
                                i = i__prev3;
                            }
                        }
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }
        }

        // vkey is a type used to uniquely identify a zero arg value.
        private partial struct vkey
        {
            public Op op;
            public long ai; // aux int
            public ptr<types.Type> t; // type
        }

        // keyFor returns the AuxInt portion of a  key structure uniquely identifying a
        // zero arg value for the supported ops.
        private static long keyFor(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Op == OpConst64 || v.Op == OpConst64F || v.Op == OpConst32F) 
                return v.AuxInt;
            else if (v.Op == OpConst32) 
                return int64(int32(v.AuxInt));
            else if (v.Op == OpConst16) 
                return int64(int16(v.AuxInt));
            else if (v.Op == OpConst8 || v.Op == OpConstBool) 
                return int64(int8(v.AuxInt));
            else 
                return v.AuxInt;
            
        }
    }
}}}}
