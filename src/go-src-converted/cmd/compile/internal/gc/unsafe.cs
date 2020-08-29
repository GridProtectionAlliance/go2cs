// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:52 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\unsafe.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // evalunsafe evaluates a package unsafe operation and returns the result.
        private static long evalunsafe(ref Node n)
        {

            if (n.Op == OALIGNOF || n.Op == OSIZEOF) 
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                var tr = n.Left.Type;
                if (tr == null)
                {
                    return 0L;
                }
                dowidth(tr);
                if (n.Op == OALIGNOF)
                {
                    return int64(tr.Align);
                }
                return tr.Width;
            else if (n.Op == OOFFSETOF) 
                // must be a selector.
                if (n.Left.Op != OXDOT)
                {
                    yyerror("invalid expression %v", n);
                    return 0L;
                }
                n.Left.Left = typecheck(n.Left.Left, Erv);
                var @base = n.Left.Left;

                n.Left = typecheck(n.Left, Erv);
                if (n.Left.Type == null)
                {
                    return 0L;
                }

                if (n.Left.Op == ODOT || n.Left.Op == ODOTPTR) 
                    break;
                else if (n.Left.Op == OCALLPART) 
                    yyerror("invalid expression %v: argument is a method value", n);
                    return 0L;
                else 
                    yyerror("invalid expression %v", n);
                    return 0L;
                // Sum offsets for dots until we reach base.
                long v = default;
                {
                    var r = n.Left;

                    while (r != base)
                    {

                        if (r.Op == ODOTPTR) 
                        {
                            // For Offsetof(s.f), s may itself be a pointer,
                            // but accessing f must not otherwise involve
                            // indirection via embedded pointer types.
                            if (r.Left != base)
                            {
                                yyerror("invalid expression %v: selector implies indirection of embedded %v", n, r.Left);
                                return 0L;
                        r = r.Left;
                            }
                            fallthrough = true;
                        }
                        if (fallthrough || r.Op == ODOT)
                        {
                            v += r.Xoffset;
                            goto __switch_break0;
                        }
                        // default: 
                            Dump("unsafenmagic", n.Left);
                            Fatalf("impossible %#v node after dot insertion", r.Op);

                        __switch_break0:;
                    }
                }
                return v;
                        Fatalf("unexpected op %v", n.Op);
            return 0L;
        }
    }
}}}}
