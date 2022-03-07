// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:26 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\typelink.go
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using sort = go.sort_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

private partial struct byTypeStr { // : slice<typelinkSortKey>
}

private partial struct typelinkSortKey {
    public @string TypeStr;
    public loader.Sym Type;
}

private static bool Less(this byTypeStr s, nint i, nint j) {
    return s[i].TypeStr < s[j].TypeStr;
}
private static nint Len(this byTypeStr s) {
    return len(s);
}
private static void Swap(this byTypeStr s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

// typelink generates the typelink table which is used by reflect.typelinks().
// Types that should be added to the typelinks table are marked with the
// MakeTypelink attribute by the compiler.
private static void typelink(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    byTypeStr typelinks = new byTypeStr();
    slice<loader.Sym> itabs = default;
    {
        var s__prev1 = s;

        for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            if (ldr.IsTypelink(s)) {
                typelinks = append(typelinks, new typelinkSortKey(decodetypeStr(ldr,ctxt.Arch,s),s));
            }
            else if (ldr.IsItab(s)) {
                itabs = append(itabs, s);
            }

        }

        s = s__prev1;
    }
    sort.Sort(typelinks);

    var tl = ldr.CreateSymForUpdate("runtime.typelink", 0);
    tl.SetType(sym.STYPELINK);
    ldr.SetAttrLocal(tl.Sym(), true);
    tl.SetSize(int64(4 * len(typelinks)));
    tl.Grow(tl.Size());
    var relocs = tl.AddRelocs(len(typelinks));
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in typelinks) {
            i = __i;
            s = __s;
            var r = relocs.At(i);
            r.SetSym(s.Type);
            r.SetOff(int32(i * 4));
            r.SetSiz(4);
            r.SetType(objabi.R_ADDROFF);
        }
        i = i__prev1;
        s = s__prev1;
    }

    var ptrsize = ctxt.Arch.PtrSize;
    var il = ldr.CreateSymForUpdate("runtime.itablink", 0);
    il.SetType(sym.SITABLINK);
    ldr.SetAttrLocal(il.Sym(), true);
    il.SetSize(int64(ptrsize * len(itabs)));
    il.Grow(il.Size());
    relocs = il.AddRelocs(len(itabs));
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in itabs) {
            i = __i;
            s = __s;
            r = relocs.At(i);
            r.SetSym(s);
            r.SetOff(int32(i * ptrsize));
            r.SetSiz(uint8(ptrsize));
            r.SetType(objabi.R_ADDR);
        }
        i = i__prev1;
        s = s__prev1;
    }
}

} // end ld_package
