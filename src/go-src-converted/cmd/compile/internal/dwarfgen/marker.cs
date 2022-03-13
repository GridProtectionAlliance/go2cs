// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarfgen -- go2cs converted at 2022 March 13 06:27:53 UTC
// import "cmd/compile/internal/dwarfgen" ==> using dwarfgen = go.cmd.compile.@internal.dwarfgen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\dwarfgen\marker.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using src = cmd.@internal.src_package;


// A ScopeMarker tracks scope nesting and boundaries for later use
// during DWARF generation.

public static partial class dwarfgen_package {

public partial struct ScopeMarker {
    public slice<ir.ScopeID> parents;
    public slice<ir.Mark> marks;
}

// checkPos validates the given position and returns the current scope.
private static ir.ScopeID checkPos(this ptr<ScopeMarker> _addr_m, src.XPos pos) {
    ref ScopeMarker m = ref _addr_m.val;

    if (!pos.IsKnown()) {
        @base.Fatalf("unknown scope position");
    }
    if (len(m.marks) == 0) {
        return 0;
    }
    var last = _addr_m.marks[len(m.marks) - 1];
    if (xposBefore(pos, last.Pos)) {
        @base.FatalfAt(pos, "non-monotonic scope positions\n\t%v: previous scope position", @base.FmtPos(last.Pos));
    }
    return last.Scope;
}

// Push records a transition to a new child scope of the current scope.
private static void Push(this ptr<ScopeMarker> _addr_m, src.XPos pos) {
    ref ScopeMarker m = ref _addr_m.val;

    var current = m.checkPos(pos);

    m.parents = append(m.parents, current);
    var child = ir.ScopeID(len(m.parents));

    m.marks = append(m.marks, new ir.Mark(Pos:pos,Scope:child));
}

// Pop records a transition back to the current scope's parent.
private static void Pop(this ptr<ScopeMarker> _addr_m, src.XPos pos) {
    ref ScopeMarker m = ref _addr_m.val;

    var current = m.checkPos(pos);

    var parent = m.parents[current - 1];

    m.marks = append(m.marks, new ir.Mark(Pos:pos,Scope:parent));
}

// Unpush removes the current scope, which must be empty.
private static void Unpush(this ptr<ScopeMarker> _addr_m) {
    ref ScopeMarker m = ref _addr_m.val;

    var i = len(m.marks) - 1;
    var current = m.marks[i].Scope;

    if (current != ir.ScopeID(len(m.parents))) {
        @base.FatalfAt(m.marks[i].Pos, "current scope is not empty");
    }
    m.parents = m.parents[..(int)current - 1];
    m.marks = m.marks[..(int)i];
}

// WriteTo writes the recorded scope marks to the given function,
// and resets the marker for reuse.
private static void WriteTo(this ptr<ScopeMarker> _addr_m, ptr<ir.Func> _addr_fn) {
    ref ScopeMarker m = ref _addr_m.val;
    ref ir.Func fn = ref _addr_fn.val;

    m.compactMarks();

    fn.Parents = make_slice<ir.ScopeID>(len(m.parents));
    copy(fn.Parents, m.parents);
    m.parents = m.parents[..(int)0];

    fn.Marks = make_slice<ir.Mark>(len(m.marks));
    copy(fn.Marks, m.marks);
    m.marks = m.marks[..(int)0];
}

private static void compactMarks(this ptr<ScopeMarker> _addr_m) {
    ref ScopeMarker m = ref _addr_m.val;

    nint n = 0;
    foreach (var (_, next) in m.marks) {
        if (n > 0 && next.Pos == m.marks[n - 1].Pos) {
            m.marks[n - 1].Scope = next.Scope;
            continue;
        }
        m.marks[n] = next;
        n++;
    }    m.marks = m.marks[..(int)n];
}

} // end dwarfgen_package
