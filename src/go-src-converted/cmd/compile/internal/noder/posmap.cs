// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:34 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\posmap.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using syntax = cmd.compile.@internal.syntax_package;
using src = cmd.@internal.src_package;


// A posMap handles mapping from syntax.Pos to src.XPos.

public static partial class noder_package {

private partial struct posMap {
    public map<ptr<syntax.PosBase>, ptr<src.PosBase>> bases;
}

private partial interface poser {
    syntax.Pos Pos();
}
private partial interface ender {
    syntax.Pos End();
}

private static src.XPos pos(this ptr<posMap> _addr_m, poser p) {
    ref posMap m = ref _addr_m.val;

    return m.makeXPos(p.Pos());
}
private static src.XPos end(this ptr<posMap> _addr_m, ender p) {
    ref posMap m = ref _addr_m.val;

    return m.makeXPos(p.End());
}

private static src.XPos makeXPos(this ptr<posMap> _addr_m, syntax.Pos pos) {
    ref posMap m = ref _addr_m.val;

    if (!pos.IsKnown()) { 
        // TODO(mdempsky): Investigate restoring base.Fatalf.
        return src.NoXPos;
    }
    var posBase = m.makeSrcPosBase(pos.Base());
    return @base.Ctxt.PosTable.XPos(src.MakePos(posBase, pos.Line(), pos.Col()));
}

// makeSrcPosBase translates from a *syntax.PosBase to a *src.PosBase.
private static ptr<src.PosBase> makeSrcPosBase(this ptr<posMap> _addr_m, ptr<syntax.PosBase> _addr_b0) => func((_, panic, _) => {
    ref posMap m = ref _addr_m.val;
    ref syntax.PosBase b0 = ref _addr_b0.val;
 
    // fast path: most likely PosBase hasn't changed
    if (m.cache.last == b0) {
        return _addr_m.cache.@base!;
    }
    var (b1, ok) = m.bases[b0];
    if (!ok) {
        var fn = b0.Filename();
        if (b0.IsFileBase()) {
            b1 = src.NewFileBase(fn, absFilename(fn));
        }
        else
 { 
            // line directive base
            var p0 = b0.Pos();
            var p0b = p0.Base();
            if (p0b == b0) {
                panic("infinite recursion in makeSrcPosBase");
            }
            var p1 = src.MakePos(m.makeSrcPosBase(p0b), p0.Line(), p0.Col());
            b1 = src.NewLinePragmaBase(p1, fn, fileh(fn), b0.Line(), b0.Col());
        }
        if (m.bases == null) {
            m.bases = make_map<ptr<syntax.PosBase>, ptr<src.PosBase>>();
        }
        m.bases[b0] = b1;
    }
    m.cache.last = b0;
    m.cache.@base = b1;

    return _addr_b1!;
});

private static void join(this ptr<posMap> _addr_m, ptr<posMap> _addr_other) {
    ref posMap m = ref _addr_m.val;
    ref posMap other = ref _addr_other.val;

    if (m.bases == null) {
        m.bases = make_map<ptr<syntax.PosBase>, ptr<src.PosBase>>();
    }
    foreach (var (k, v) in other.bases) {
        if (m.bases[k] != null) {
            @base.Fatalf("duplicate posmap bases");
        }
        m.bases[k] = v;
    }
}

} // end noder_package
