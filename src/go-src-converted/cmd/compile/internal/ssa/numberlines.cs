// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:41 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\numberlines.go
namespace go.cmd.compile.@internal;

using src = cmd.@internal.src_package;
using fmt = fmt_package;
using sort = sort_package;
using System;

public static partial class ssa_package {

private static bool isPoorStatementOp(Op op) {

    // Note that Nilcheck often vanishes, but when it doesn't, you'd love to start the statement there
    // so that a debugger-user sees the stop before the panic, and can examine the value.
    if (op == OpAddr || op == OpLocalAddr || op == OpOffPtr || op == OpStructSelect || op == OpPhi || op == OpITab || op == OpIData || op == OpIMake || op == OpStringMake || op == OpSliceMake || op == OpStructMake0 || op == OpStructMake1 || op == OpStructMake2 || op == OpStructMake3 || op == OpStructMake4 || op == OpConstBool || op == OpConst8 || op == OpConst16 || op == OpConst32 || op == OpConst64 || op == OpConst32F || op == OpConst64F || op == OpSB || op == OpSP || op == OpArgIntReg || op == OpArgFloatReg) 
        return true;
        return false;
}

// nextGoodStatementIndex returns an index at i or later that is believed
// to be a good place to start the statement for b.  This decision is
// based on v's Op, the possibility of a better later operation, and
// whether the values following i are the same line as v.
// If a better statement index isn't found, then i is returned.
private static nint nextGoodStatementIndex(ptr<Value> _addr_v, nint i, ptr<Block> _addr_b) {
    ref Value v = ref _addr_v.val;
    ref Block b = ref _addr_b.val;
 
    // If the value is the last one in the block, too bad, it will have to do
    // (this assumes that the value ordering vaguely corresponds to the source
    // program execution order, which tends to be true directly after ssa is
    // first built.
    if (i >= len(b.Values) - 1) {
        return i;
    }
    if (!isPoorStatementOp(v.Op)) {
        return i;
    }
    for (var j = i + 1; j < len(b.Values); j++) {
        var u = b.Values[j];
        if (u.Pos.IsStmt() == src.PosNotStmt) { // ignore non-statements
            continue;
        }
        if (u.Pos.SameFileAndLine(v.Pos)) {
            if (isPoorStatementOp(u.Op)) {
                continue; // Keep looking, this is also not a good statement op
            }
            return j;
        }
        return i;
    }
    return i;
}

// notStmtBoundary reports whether a value with opcode op can never be a statement
// boundary. Such values don't correspond to a user's understanding of a
// statement boundary.
private static bool notStmtBoundary(Op op) {

    if (op == OpCopy || op == OpPhi || op == OpVarKill || op == OpVarDef || op == OpVarLive || op == OpUnknown || op == OpFwdRef || op == OpArg || op == OpArgIntReg || op == OpArgFloatReg) 
        return true;
        return false;
}

private static ptr<Value> FirstPossibleStmtValue(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    foreach (var (_, v) in b.Values) {
        if (notStmtBoundary(v.Op)) {
            continue;
        }
        return _addr_v!;
    }    return _addr_null!;
}

private static @string flc(src.XPos p) {
    if (p == src.NoXPos) {
        return "none";
    }
    return fmt.Sprintf("(%d):%d:%d", p.FileIndex(), p.Line(), p.Col());
}

private partial struct fileAndPair {
    public int f;
    public lineRange lp;
}

private partial struct fileAndPairs { // : slice<fileAndPair>
}

private static nint Len(this fileAndPairs fap) {
    return len(fap);
}
private static bool Less(this fileAndPairs fap, nint i, nint j) {
    return fap[i].f < fap[j].f;
}
private static void Swap(this fileAndPairs fap, nint i, nint j) {
    (fap[i], fap[j]) = (fap[j], fap[i]);
}

// -d=ssa/number_lines/stats=1 (that bit) for line and file distribution statistics
// -d=ssa/number_lines/debug for information about why particular values are marked as statements.
private static void numberLines(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var po = f.Postorder();
    var endlines = make_map<ID, src.XPos>();
    var ranges = make_map<nint, lineRange>();
    Action<src.XPos> note = p => {
        var line = uint32(p.Line());
        var i = int(p.FileIndex());
        var (lp, found) = ranges[i];
        var change = false;
        if (line < lp.first || !found) {
            lp.first = line;
            change = true;
        }
        if (line > lp.last) {
            lp.last = line;
            change = true;
        }
        if (change) {
            ranges[i] = lp;
        }
    }; 

    // Visit in reverse post order so that all non-loop predecessors come first.
    for (var j = len(po) - 1; j >= 0; j--) {
        var b = po[j]; 
        // Find the first interesting position and check to see if it differs from any predecessor
        var firstPos = src.NoXPos;
        nint firstPosIndex = -1;
        if (b.Pos.IsStmt() != src.PosNotStmt) {
            note(b.Pos);
        }
        {
            var i__prev2 = i;

            for (i = 0; i < len(b.Values); i++) {
                var v = b.Values[i];
                if (v.Pos.IsStmt() != src.PosNotStmt) {
                    note(v.Pos); 
                    // skip ahead to better instruction for this line if possible
                    i = nextGoodStatementIndex(_addr_v, i, _addr_b);
                    v = b.Values[i];
                    firstPosIndex = i;
                    firstPos = v.Pos;
                    v.Pos = firstPos.WithDefaultStmt(); // default to default
                    break;
                }
            }


            i = i__prev2;
        }

        if (firstPosIndex == -1) { // Effectively empty block, check block's own Pos, consider preds.
            line = src.NoXPos;
            {
                var p__prev2 = p;

                foreach (var (_, __p) in b.Preds) {
                    p = __p;
                    var pbi = p.Block().ID;
                    if (!endlines[pbi].SameFileAndLine(line)) {
                        if (line == src.NoXPos) {
                            line = endlines[pbi];
                            continue;
                        }
                        else
 {
                            line = src.NoXPos;
                            break;
                        }
                    }
                } 
                // If the block has no statement itself and is effectively empty, tag it w/ predecessor(s) but not as a statement

                p = p__prev2;
            }

            if (b.Pos.IsStmt() == src.PosNotStmt) {
                b.Pos = line;
                endlines[b.ID] = line;
                continue;
            } 
            // If the block differs from its predecessors, mark it as a statement
            if (line == src.NoXPos || !line.SameFileAndLine(b.Pos)) {
                b.Pos = b.Pos.WithIsStmt();
                if (f.pass.debug > 0) {
                    fmt.Printf("Mark stmt effectively-empty-block %s %s %s\n", f.Name, b, flc(b.Pos));
                }
            }
            endlines[b.ID] = b.Pos;
            continue;
        }
        if (len(b.Preds) == 0) { // Don't forget the entry block
            b.Values[firstPosIndex].Pos = firstPos.WithIsStmt();
            if (f.pass.debug > 0) {
                fmt.Printf("Mark stmt entry-block %s %s %s %s\n", f.Name, b, b.Values[firstPosIndex], flc(firstPos));
            }
        }
        else
 { // differing pred
            {
                var p__prev2 = p;

                foreach (var (_, __p) in b.Preds) {
                    p = __p;
                    pbi = p.Block().ID;
                    if (!endlines[pbi].SameFileAndLine(firstPos)) {
                        b.Values[firstPosIndex].Pos = firstPos.WithIsStmt();
                        if (f.pass.debug > 0) {
                            fmt.Printf("Mark stmt differing-pred %s %s %s %s, different=%s ending %s\n", f.Name, b, b.Values[firstPosIndex], flc(firstPos), p.Block(), flc(endlines[pbi]));
                        }
                        break;
                    }
                }

                p = p__prev2;
            }
        }
        {
            var i__prev2 = i;

            for (i = firstPosIndex + 1; i < len(b.Values); i++) {
                v = b.Values[i];
                if (v.Pos.IsStmt() == src.PosNotStmt) {
                    continue;
                }
                note(v.Pos); 
                // skip ahead if possible
                i = nextGoodStatementIndex(_addr_v, i, _addr_b);
                v = b.Values[i];
                if (!v.Pos.SameFileAndLine(firstPos)) {
                    if (f.pass.debug > 0) {
                        fmt.Printf("Mark stmt new line %s %s %s %s prev pos = %s\n", f.Name, b, v, flc(v.Pos), flc(firstPos));
                    }
                    firstPos = v.Pos;
                    v.Pos = v.Pos.WithIsStmt();
                }
                else
 {
                    v.Pos = v.Pos.WithDefaultStmt();
                }
            }


            i = i__prev2;
        }
        if (b.Pos.IsStmt() != src.PosNotStmt && !b.Pos.SameFileAndLine(firstPos)) {
            if (f.pass.debug > 0) {
                fmt.Printf("Mark stmt end of block differs %s %s %s prev pos = %s\n", f.Name, b, flc(b.Pos), flc(firstPos));
            }
            b.Pos = b.Pos.WithIsStmt();
            firstPos = b.Pos;
        }
        endlines[b.ID] = firstPos;
    }
    if (f.pass.stats & 1 != 0) { 
        // Report summary statistics on the shape of the sparse map about to be constructed
        // TODO use this information to make sparse maps faster.
        fileAndPairs entries = default;
        {
            var v__prev1 = v;

            foreach (var (__k, __v) in ranges) {
                k = __k;
                v = __v;
                entries = append(entries, new fileAndPair(int32(k),v));
            }

            v = v__prev1;
        }

        sort.Sort(entries);
        var total = uint64(0); // sum over files of maxline(file) - minline(file)
        var maxfile = int32(0); // max(file indices)
        var minline = uint32(0xffffffff); // min over files of minline(file)
        var maxline = uint32(0); // max over files of maxline(file)
        {
            var v__prev1 = v;

            foreach (var (_, __v) in entries) {
                v = __v;
                if (f.pass.stats > 1) {
                    f.LogStat("file", v.f, "low", v.lp.first, "high", v.lp.last);
                }
                total += uint64(v.lp.last - v.lp.first);
                if (maxfile < v.f) {
                    maxfile = v.f;
                }
                if (minline > v.lp.first) {
                    minline = v.lp.first;
                }
                if (maxline < v.lp.last) {
                    maxline = v.lp.last;
                }
            }

            v = v__prev1;
        }

        f.LogStat("SUM_LINE_RANGE", total, "MAXMIN_LINE_RANGE", maxline - minline, "MAXFILE", maxfile, "NFILES", len(entries));
    }
    f.cachedLineStarts = newXposmap(ranges);
}

} // end ssa_package
