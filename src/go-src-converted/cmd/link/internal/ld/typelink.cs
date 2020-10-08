// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:39:46 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\typelink.go
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private partial struct byTypeStr // : slice<typelinkSortKey>
        {
        }

        private partial struct typelinkSortKey
        {
            public @string TypeStr;
            public loader.Sym Type;
        }

        private static bool Less(this byTypeStr s, long i, long j)
        {
            return s[i].TypeStr < s[j].TypeStr;
        }
        private static long Len(this byTypeStr s)
        {
            return len(s);
        }
        private static void Swap(this byTypeStr s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }

        // typelink generates the typelink table which is used by reflect.typelinks().
        // Types that should be added to the typelinks table are marked with the
        // MakeTypelink attribute by the compiler.
        private static void typelink(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            byTypeStr typelinks = new byTypeStr();
            {
                var s__prev1 = s;

                for (var s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
                {
                    if (ldr.AttrReachable(s) && ldr.IsTypelink(s))
                    {
                        typelinks = append(typelinks, new typelinkSortKey(decodetypeStr(ldr,ctxt.Arch,s),s));
                    }

                }


                s = s__prev1;
            }
            sort.Sort(typelinks);

            var tl = ldr.CreateSymForUpdate("runtime.typelink", 0L);
            tl.SetType(sym.STYPELINK);
            ldr.SetAttrReachable(tl.Sym(), true);
            ldr.SetAttrLocal(tl.Sym(), true);
            tl.SetSize(int64(4L * len(typelinks)));
            tl.Grow(tl.Size());
            var relocs = tl.AddRelocs(len(typelinks));
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in typelinks)
                {
                    i = __i;
                    s = __s;
                    var r = relocs.At2(i);
                    r.SetSym(s.Type);
                    r.SetOff(int32(i * 4L));
                    r.SetSiz(4L);
                    r.SetType(objabi.R_ADDROFF);
                }

                s = s__prev1;
            }
        }
    }
}}}}
