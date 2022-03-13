// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:41 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\block.go
namespace go.cmd.compile.@internal;

using src = cmd.@internal.src_package;
using fmt = fmt_package;


// Block represents a basic block in the control flow graph of a function.

public static partial class ssa_package {

public partial struct Block {
    public ID ID; // Source position for block's control operation
    public src.XPos Pos; // The kind of block this is.
    public BlockKind Kind; // Likely direction for branches.
// If BranchLikely, Succs[0] is the most likely branch taken.
// If BranchUnlikely, Succs[1] is the most likely branch taken.
// Ignored if len(Succs) < 2.
// Fatal if not BranchUnknown and len(Succs) > 2.
    public BranchPrediction Likely; // After flagalloc, records whether flags are live at the end of the block.
    public bool FlagsLiveAtEnd; // Subsequent blocks, if any. The number and order depend on the block kind.
    public slice<Edge> Succs; // Inverse of successors.
// The order is significant to Phi nodes in the block.
// TODO: predecessors is a pain to maintain. Can we somehow order phi
// arguments by block id and have this field computed explicitly when needed?
    public slice<Edge> Preds; // A list of values that determine how the block is exited. The number
// and type of control values depends on the Kind of the block. For
// instance, a BlockIf has a single boolean control value and BlockExit
// has a single memory control value.
//
// The ControlValues() method may be used to get a slice with the non-nil
// control values that can be ranged over.
//
// Controls[1] must be nil if Controls[0] is nil.
    public array<ptr<Value>> Controls; // Auxiliary info for the block. Its value depends on the Kind.
    public Aux Aux;
    public long AuxInt; // The unordered set of Values that define the operation of this block.
// After the scheduling pass, this list is ordered.
    public slice<ptr<Value>> Values; // The containing function
    public ptr<Func> Func; // Storage for Succs, Preds and Values.
    public array<Edge> succstorage;
    public array<Edge> predstorage;
    public array<ptr<Value>> valstorage;
}

// Edge represents a CFG edge.
// Example edges for b branching to either c or d.
// (c and d have other predecessors.)
//   b.Succs = [{c,3}, {d,1}]
//   c.Preds = [?, ?, ?, {b,0}]
//   d.Preds = [?, {b,1}, ?]
// These indexes allow us to edit the CFG in constant time.
// In addition, it informs phi ops in degenerate cases like:
// b:
//    if k then c else c
// c:
//    v = Phi(x, y)
// Then the indexes tell you whether x is chosen from
// the if or else branch from b.
//   b.Succs = [{c,0},{c,1}]
//   c.Preds = [{b,0},{b,1}]
// means x is chosen if k is true.
public partial struct Edge {
    public ptr<Block> b; // index of reverse edge.  Invariant:
//   e := x.Succs[idx]
//   e.b.Preds[e.i] = Edge{x,idx}
// and similarly for predecessors.
    public nint i;
}

public static ptr<Block> Block(this Edge e) {
    return _addr_e.b!;
}
public static nint Index(this Edge e) {
    return e.i;
}
public static @string String(this Edge e) {
    return fmt.Sprintf("{%v,%d}", e.b, e.i);
}

//     kind          controls        successors
//   ------------------------------------------
//     Exit      [return mem]                []
//    Plain                []            [next]
//       If   [boolean Value]      [then, else]
//    Defer             [mem]  [nopanic, panic]  (control opcode should be OpStaticCall to runtime.deferproc)
public partial struct BlockKind { // : sbyte
}

// short form print
private static @string String(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return fmt.Sprintf("b%d", b.ID);
}

// long form print
private static @string LongString(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    var s = b.Kind.String();
    if (b.Aux != null) {
        s += fmt.Sprintf(" {%s}", b.Aux);
    }
    {
        var t = b.AuxIntString();

        if (t != "") {
            s += fmt.Sprintf(" [%s]", t);
        }
    }
    {
        var c__prev1 = c;

        foreach (var (_, __c) in b.ControlValues()) {
            c = __c;
            s += fmt.Sprintf(" %s", c);
        }
        c = c__prev1;
    }

    if (len(b.Succs) > 0) {
        s += " ->";
        {
            var c__prev1 = c;

            foreach (var (_, __c) in b.Succs) {
                c = __c;
                s += " " + c.b.String();
            }

            c = c__prev1;
        }
    }

    if (b.Likely == BranchUnlikely) 
        s += " (unlikely)";
    else if (b.Likely == BranchLikely) 
        s += " (likely)";
        return s;
}

// NumControls returns the number of non-nil control values the
// block has.
private static nint NumControls(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Controls[0] == null) {
        return 0;
    }
    if (b.Controls[1] == null) {
        return 1;
    }
    return 2;
}

// ControlValues returns a slice containing the non-nil control
// values of the block. The index of each control value will be
// the same as it is in the Controls property and can be used
// in ReplaceControl calls.
private static slice<ptr<Value>> ControlValues(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Controls[0] == null) {
        return b.Controls[..(int)0];
    }
    if (b.Controls[1] == null) {
        return b.Controls[..(int)1];
    }
    return b.Controls[..(int)2];
}

// SetControl removes all existing control values and then adds
// the control value provided. The number of control values after
// a call to SetControl will always be 1.
private static void SetControl(this ptr<Block> _addr_b, ptr<Value> _addr_v) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;

    b.ResetControls();
    b.Controls[0] = v;
    v.Uses++;
}

// ResetControls sets the number of controls for the block to 0.
private static void ResetControls(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Controls[0] != null) {
        b.Controls[0].Uses--;
    }
    if (b.Controls[1] != null) {
        b.Controls[1].Uses--;
    }
    b.Controls = new array<ptr<Value>>(new ptr<Value>[] {  }); // reset both controls to nil
}

// AddControl appends a control value to the existing list of control values.
private static void AddControl(this ptr<Block> _addr_b, ptr<Value> _addr_v) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;

    var i = b.NumControls();
    b.Controls[i] = v; // panics if array is full
    v.Uses++;
}

// ReplaceControl exchanges the existing control value at the index provided
// for the new value. The index must refer to a valid control value.
private static void ReplaceControl(this ptr<Block> _addr_b, nint i, ptr<Value> _addr_v) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;

    b.Controls[i].Uses--;
    b.Controls[i] = v;
    v.Uses++;
}

// CopyControls replaces the controls for this block with those from the
// provided block. The provided block is not modified.
private static void CopyControls(this ptr<Block> _addr_b, ptr<Block> _addr_from) {
    ref Block b = ref _addr_b.val;
    ref Block from = ref _addr_from.val;

    if (b == from) {
        return ;
    }
    b.ResetControls();
    foreach (var (_, c) in from.ControlValues()) {
        b.AddControl(c);
    }
}

// Reset sets the block to the provided kind and clears all the blocks control
// and auxiliary values. Other properties of the block, such as its successors,
// predecessors and values are left unmodified.
private static void Reset(this ptr<Block> _addr_b, BlockKind kind) {
    ref Block b = ref _addr_b.val;

    b.Kind = kind;
    b.ResetControls();
    b.Aux = null;
    b.AuxInt = 0;
}

// resetWithControl resets b and adds control v.
// It is equivalent to b.Reset(kind); b.AddControl(v),
// except that it is one call instead of two and avoids a bounds check.
// It is intended for use by rewrite rules, where this matters.
private static void resetWithControl(this ptr<Block> _addr_b, BlockKind kind, ptr<Value> _addr_v) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;

    b.Kind = kind;
    b.ResetControls();
    b.Aux = null;
    b.AuxInt = 0;
    b.Controls[0] = v;
    v.Uses++;
}

// resetWithControl2 resets b and adds controls v and w.
// It is equivalent to b.Reset(kind); b.AddControl(v); b.AddControl(w),
// except that it is one call instead of three and avoids two bounds checks.
// It is intended for use by rewrite rules, where this matters.
private static void resetWithControl2(this ptr<Block> _addr_b, BlockKind kind, ptr<Value> _addr_v, ptr<Value> _addr_w) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    b.Kind = kind;
    b.ResetControls();
    b.Aux = null;
    b.AuxInt = 0;
    b.Controls[0] = v;
    b.Controls[1] = w;
    v.Uses++;
    w.Uses++;
}

// truncateValues truncates b.Values at the ith element, zeroing subsequent elements.
// The values in b.Values after i must already have had their args reset,
// to maintain correct value uses counts.
private static void truncateValues(this ptr<Block> _addr_b, nint i) {
    ref Block b = ref _addr_b.val;

    var tail = b.Values[(int)i..];
    foreach (var (j) in tail) {
        tail[j] = null;
    }    b.Values = b.Values[..(int)i];
}

// AddEdgeTo adds an edge from block b to block c. Used during building of the
// SSA graph; do not use on an already-completed SSA graph.
private static void AddEdgeTo(this ptr<Block> _addr_b, ptr<Block> _addr_c) {
    ref Block b = ref _addr_b.val;
    ref Block c = ref _addr_c.val;

    var i = len(b.Succs);
    var j = len(c.Preds);
    b.Succs = append(b.Succs, new Edge(c,j));
    c.Preds = append(c.Preds, new Edge(b,i));
    b.Func.invalidateCFG();
}

// removePred removes the ith input edge from b.
// It is the responsibility of the caller to remove
// the corresponding successor edge.
private static void removePred(this ptr<Block> _addr_b, nint i) {
    ref Block b = ref _addr_b.val;

    var n = len(b.Preds) - 1;
    if (i != n) {
        var e = b.Preds[n];
        b.Preds[i] = e; 
        // Update the other end of the edge we moved.
        e.b.Succs[e.i].i = i;
    }
    b.Preds[n] = new Edge();
    b.Preds = b.Preds[..(int)n];
    b.Func.invalidateCFG();
}

// removeSucc removes the ith output edge from b.
// It is the responsibility of the caller to remove
// the corresponding predecessor edge.
private static void removeSucc(this ptr<Block> _addr_b, nint i) {
    ref Block b = ref _addr_b.val;

    var n = len(b.Succs) - 1;
    if (i != n) {
        var e = b.Succs[n];
        b.Succs[i] = e; 
        // Update the other end of the edge we moved.
        e.b.Preds[e.i].i = i;
    }
    b.Succs[n] = new Edge();
    b.Succs = b.Succs[..(int)n];
    b.Func.invalidateCFG();
}

private static void swapSuccessors(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (len(b.Succs) != 2) {
        b.Fatalf("swapSuccessors with len(Succs)=%d", len(b.Succs));
    }
    var e0 = b.Succs[0];
    var e1 = b.Succs[1];
    b.Succs[0] = e1;
    b.Succs[1] = e0;
    e0.b.Preds[e0.i].i = 1;
    e1.b.Preds[e1.i].i = 0;
    b.Likely *= -1;
}

// LackingPos indicates whether b is a block whose position should be inherited
// from its successors.  This is true if all the values within it have unreliable positions
// and if it is "plain", meaning that there is no control flow that is also very likely
// to correspond to a well-understood source position.
private static bool LackingPos(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;
 
    // Non-plain predecessors are If or Defer, which both (1) have two successors,
    // which might have different line numbers and (2) correspond to statements
    // in the source code that have positions, so this case ought not occur anyway.
    if (b.Kind != BlockPlain) {
        return false;
    }
    if (b.Pos != src.NoXPos) {
        return false;
    }
    foreach (var (_, v) in b.Values) {
        if (v.LackingPos()) {
            continue;
        }
        return false;
    }    return true;
}

private static @string AuxIntString(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    switch (b.Kind.AuxIntType()) {
        case "int8": 
            return fmt.Sprintf("%v", int8(b.AuxInt));
            break;
        case "uint8": 
            return fmt.Sprintf("%v", uint8(b.AuxInt));
            break;
        case "": // no aux int type
            return "";
            break;
        default: // type specified but not implemented - print as int64
            return fmt.Sprintf("%v", b.AuxInt);
            break;
    }
}

// likelyBranch reports whether block b is the likely branch of all of its predecessors.
private static bool likelyBranch(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (len(b.Preds) == 0) {
        return false;
    }
    foreach (var (_, e) in b.Preds) {
        var p = e.b;
        if (len(p.Succs) == 1 || len(p.Succs) == 2 && (p.Likely == BranchLikely && p.Succs[0].b == b || p.Likely == BranchUnlikely && p.Succs[1].b == b)) {
            continue;
        }
        return false;
    }    return true;
}

private static void Logf(this ptr<Block> _addr_b, @string msg, params object[] args) {
    args = args.Clone();
    ref Block b = ref _addr_b.val;

    b.Func.Logf(msg, args);
}
private static bool Log(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return b.Func.Log();
}
private static void Fatalf(this ptr<Block> _addr_b, @string msg, params object[] args) {
    args = args.Clone();
    ref Block b = ref _addr_b.val;

    b.Func.Fatalf(msg, args);
}

public partial struct BranchPrediction { // : sbyte
}

public static readonly var BranchUnlikely = BranchPrediction(-1);
public static readonly var BranchUnknown = BranchPrediction(0);
public static readonly var BranchLikely = BranchPrediction(+1);

} // end ssa_package
