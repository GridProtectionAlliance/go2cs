// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:49:24 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\check.go
using s390x = go.cmd.@internal.obj.s390x_package;
using math = go.math_package;
using bits = go.math.bits_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // checkFunc checks invariants of f.
private static void checkFunc(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var blockMark = make_slice<bool>(f.NumBlocks());
    var valueMark = make_slice<bool>(f.NumValues());

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (blockMark[b.ID]) {
                f.Fatalf("block %s appears twice in %s!", b, f.Name);
            }
            blockMark[b.ID] = true;
            if (b.Func != f) {
                f.Fatalf("%s.Func=%s, want %s", b, b.Func.Name, f.Name);
            }
            {
                var i__prev2 = i;
                var e__prev2 = e;

                foreach (var (__i, __e) in b.Preds) {
                    i = __i;
                    e = __e;
                    {
                        var se = e.b.Succs[e.i];

                        if (se.b != b || se.i != i) {
                            f.Fatalf("block pred/succ not crosslinked correctly %d:%s %d:%s", i, b, se.i, se.b);
                        }
                    }

                }
                i = i__prev2;
                e = e__prev2;
            }

            {
                var i__prev2 = i;
                var e__prev2 = e;

                foreach (var (__i, __e) in b.Succs) {
                    i = __i;
                    e = __e;
                    {
                        var pe = e.b.Preds[e.i];

                        if (pe.b != b || pe.i != i) {
                            f.Fatalf("block succ/pred not crosslinked correctly %d:%s %d:%s", i, b, pe.i, pe.b);
                        }
                    }

                }
                i = i__prev2;
                e = e__prev2;
            }


            if (b.Kind == BlockExit) 
                if (len(b.Succs) != 0) {
                    f.Fatalf("exit block %s has successors", b);
                }
                if (b.NumControls() != 1) {
                    f.Fatalf("exit block %s has no control value", b);
                }
                if (!b.Controls[0].Type.IsMemory()) {
                    f.Fatalf("exit block %s has non-memory control value %s", b, b.Controls[0].LongString());
                }
            else if (b.Kind == BlockRet) 
                if (len(b.Succs) != 0) {
                    f.Fatalf("ret block %s has successors", b);
                }
                if (b.NumControls() != 1) {
                    f.Fatalf("ret block %s has nil control", b);
                }
                if (!b.Controls[0].Type.IsMemory()) {
                    f.Fatalf("ret block %s has non-memory control value %s", b, b.Controls[0].LongString());
                }
            else if (b.Kind == BlockRetJmp) 
                if (len(b.Succs) != 0) {
                    f.Fatalf("retjmp block %s len(Succs)==%d, want 0", b, len(b.Succs));
                }
                if (b.NumControls() != 1) {
                    f.Fatalf("retjmp block %s has nil control", b);
                }
                if (!b.Controls[0].Type.IsMemory()) {
                    f.Fatalf("retjmp block %s has non-memory control value %s", b, b.Controls[0].LongString());
                }
                if (b.Aux == null) {
                    f.Fatalf("retjmp block %s has nil Aux field", b);
                }
            else if (b.Kind == BlockPlain) 
                if (len(b.Succs) != 1) {
                    f.Fatalf("plain block %s len(Succs)==%d, want 1", b, len(b.Succs));
                }
                if (b.NumControls() != 0) {
                    f.Fatalf("plain block %s has non-nil control %s", b, b.Controls[0].LongString());
                }
            else if (b.Kind == BlockIf) 
                if (len(b.Succs) != 2) {
                    f.Fatalf("if block %s len(Succs)==%d, want 2", b, len(b.Succs));
                }
                if (b.NumControls() != 1) {
                    f.Fatalf("if block %s has no control value", b);
                }
                if (!b.Controls[0].Type.IsBoolean()) {
                    f.Fatalf("if block %s has non-bool control value %s", b, b.Controls[0].LongString());
                }
            else if (b.Kind == BlockDefer) 
                if (len(b.Succs) != 2) {
                    f.Fatalf("defer block %s len(Succs)==%d, want 2", b, len(b.Succs));
                }
                if (b.NumControls() != 1) {
                    f.Fatalf("defer block %s has no control value", b);
                }
                if (!b.Controls[0].Type.IsMemory()) {
                    f.Fatalf("defer block %s has non-memory control value %s", b, b.Controls[0].LongString());
                }
            else if (b.Kind == BlockFirst) 
                if (len(b.Succs) != 2) {
                    f.Fatalf("plain/dead block %s len(Succs)==%d, want 2", b, len(b.Succs));
                }
                if (b.NumControls() != 0) {
                    f.Fatalf("plain/dead block %s has a control value", b);
                }
                        if (len(b.Succs) != 2 && b.Likely != BranchUnknown) {
                f.Fatalf("likeliness prediction %d for block %s with %d successors", b.Likely, b, len(b.Succs));
            }
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v; 
                    // Check to make sure argument count makes sense (argLen of -1 indicates
                    // variable length args)
                    var nArgs = opcodeTable[v.Op].argLen;
                    if (nArgs != -1 && int32(len(v.Args)) != nArgs) {
                        f.Fatalf("value %s has %d args, expected %d", v.LongString(), len(v.Args), nArgs);
                    }
                    var canHaveAux = false;
                    var canHaveAuxInt = false; 
                    // TODO: enforce types of Aux in this switch (like auxString does below)

                    if (opcodeTable[v.Op].auxType == auxNone)
                    {
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxBool)
                    {
                        if (v.AuxInt < 0 || v.AuxInt > 1) {
                            f.Fatalf("bad bool AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxInt8)
                    {
                        if (v.AuxInt != int64(int8(v.AuxInt))) {
                            f.Fatalf("bad int8 AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxInt16)
                    {
                        if (v.AuxInt != int64(int16(v.AuxInt))) {
                            f.Fatalf("bad int16 AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxInt32)
                    {
                        if (v.AuxInt != int64(int32(v.AuxInt))) {
                            f.Fatalf("bad int32 AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxInt64 || opcodeTable[v.Op].auxType == auxARM64BitField)
                    {
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxInt128)
                    {
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxUInt8)
                    {
                        if (v.AuxInt != int64(uint8(v.AuxInt))) {
                            f.Fatalf("bad uint8 AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxFloat32)
                    {
                        canHaveAuxInt = true;
                        if (math.IsNaN(v.AuxFloat())) {
                            f.Fatalf("value %v has an AuxInt that encodes a NaN", v);
                        }
                        if (!isExactFloat32(v.AuxFloat())) {
                            f.Fatalf("value %v has an AuxInt value that is not an exact float32", v);
                        }
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxFloat64)
                    {
                        canHaveAuxInt = true;
                        if (math.IsNaN(v.AuxFloat())) {
                            f.Fatalf("value %v has an AuxInt that encodes a NaN", v);
                        }
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxString)
                    {
                        {
                            stringAux (_, ok) = v.Aux._<stringAux>();

                            if (!ok) {
                                f.Fatalf("value %v has Aux type %T, want string", v, v.Aux);
                            }
                        }

                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxCallOff)
                    {
                        canHaveAuxInt = true;
                        fallthrough = true;
                    }
                    if (fallthrough || opcodeTable[v.Op].auxType == auxCall)
                    {
                        {
                            ptr<AuxCall> (ac, ok) = v.Aux._<ptr<AuxCall>>();

                            if (ok) {
                                if (v.Op == OpStaticCall && ac.Fn == null) {
                                    f.Fatalf("value %v has *AuxCall with nil Fn", v);
                                }
                            }
                            else
 {
                                f.Fatalf("value %v has Aux type %T, want *AuxCall", v, v.Aux);
                            }
                        }

                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxNameOffsetInt8)
                    {
                        {
                            (_, ok) = v.Aux._<ptr<AuxNameOffset>>();

                            if (!ok) {
                                f.Fatalf("value %v has Aux type %T, want *AuxNameOffset", v, v.Aux);
                            }
                        }

                        canHaveAux = true;
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxSym || opcodeTable[v.Op].auxType == auxTyp)
                    {
                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxSymOff || opcodeTable[v.Op].auxType == auxSymValAndOff || opcodeTable[v.Op].auxType == auxTypSize)
                    {
                        canHaveAuxInt = true;
                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxCCop)
                    {
                        if (opcodeTable[Op(v.AuxInt)].name == "OpInvalid") {
                            f.Fatalf("value %v has an AuxInt value that is a valid opcode", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxS390XCCMask)
                    {
                        {
                            (_, ok) = v.Aux._<s390x.CCMask>();

                            if (!ok) {
                                f.Fatalf("bad type %T for S390XCCMask in %v", v.Aux, v);
                            }
                        }

                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxS390XRotateParams)
                    {
                        {
                            (_, ok) = v.Aux._<s390x.RotateParams>();

                            if (!ok) {
                                f.Fatalf("bad type %T for S390XRotateParams in %v", v.Aux, v);
                            }
                        }

                        canHaveAux = true;
                        goto __switch_break0;
                    }
                    if (opcodeTable[v.Op].auxType == auxFlagConstant)
                    {
                        if (v.AuxInt < 0 || v.AuxInt > 15) {
                            f.Fatalf("bad FlagConstant AuxInt value for %v", v);
                        }
                        canHaveAuxInt = true;
                        goto __switch_break0;
                    }
                    // default: 
                        f.Fatalf("unknown aux type for %s", v.Op);

                    __switch_break0:;
                    if (!canHaveAux && v.Aux != null) {
                        f.Fatalf("value %s has an Aux value %v but shouldn't", v.LongString(), v.Aux);
                    }
                    if (!canHaveAuxInt && v.AuxInt != 0) {
                        f.Fatalf("value %s has an AuxInt value %d but shouldn't", v.LongString(), v.AuxInt);
                    }
                    {
                        var i__prev3 = i;
                        var arg__prev3 = arg;

                        foreach (var (__i, __arg) in v.Args) {
                            i = __i;
                            arg = __arg;
                            if (arg == null) {
                                f.Fatalf("value %s has nil arg", v.LongString());
                            }
                            if (v.Op != OpPhi) { 
                                // For non-Phi ops, memory args must be last, if present
                                if (arg.Type.IsMemory() && i != len(v.Args) - 1) {
                                    f.Fatalf("value %s has non-final memory arg (%d < %d)", v.LongString(), i, len(v.Args) - 1);
                                }
                            }
                        }
                        i = i__prev3;
                        arg = arg__prev3;
                    }

                    if (valueMark[v.ID]) {
                        f.Fatalf("value %s appears twice!", v.LongString());
                    }
                    valueMark[v.ID] = true;

                    if (v.Block != b) {
                        f.Fatalf("%s.block != %s", v, b);
                    }
                    if (v.Op == OpPhi && len(v.Args) != len(b.Preds)) {
                        f.Fatalf("phi length %s does not match pred length %d for block %s", v.LongString(), len(b.Preds), b);
                    }
                    if (v.Op == OpAddr) {
                        if (len(v.Args) == 0) {
                            f.Fatalf("no args for OpAddr %s", v.LongString());
                        }
                        if (v.Args[0].Op != OpSB) {
                            f.Fatalf("bad arg to OpAddr %v", v);
                        }
                    }
                    if (v.Op == OpLocalAddr) {
                        if (len(v.Args) != 2) {
                            f.Fatalf("wrong # of args for OpLocalAddr %s", v.LongString());
                        }
                        if (v.Args[0].Op != OpSP) {
                            f.Fatalf("bad arg 0 to OpLocalAddr %v", v);
                        }
                        if (!v.Args[1].Type.IsMemory()) {
                            f.Fatalf("bad arg 1 to OpLocalAddr %v", v);
                        }
                    }
                    if (f.RegAlloc != null && f.Config.SoftFloat && v.Type.IsFloat()) {
                        f.Fatalf("unexpected floating-point type %v", v.LongString());
                    }
                    {
                        var c__prev1 = c;

                        var c = f.Config;


                        if (v.Op == OpSP || v.Op == OpSB) 
                            if (v.Type != c.Types.Uintptr) {
                                f.Fatalf("bad %s type: want uintptr, have %s", v.Op, v.Type.String());
                            }
                        else if (v.Op == OpStringLen) 
                            if (v.Type != c.Types.Int) {
                                f.Fatalf("bad %s type: want int, have %s", v.Op, v.Type.String());
                            }
                        else if (v.Op == OpLoad) 
                            if (!v.Args[1].Type.IsMemory()) {
                                f.Fatalf("bad arg 1 type to %s: want mem, have %s", v.Op, v.Args[1].Type.String());
                            }
                        else if (v.Op == OpStore) 
                            if (!v.Type.IsMemory()) {
                                f.Fatalf("bad %s type: want mem, have %s", v.Op, v.Type.String());
                            }
                            if (!v.Args[2].Type.IsMemory()) {
                                f.Fatalf("bad arg 2 type to %s: want mem, have %s", v.Op, v.Args[2].Type.String());
                            }
                        else if (v.Op == OpCondSelect) 
                            if (!v.Args[2].Type.IsBoolean()) {
                                f.Fatalf("bad arg 2 type to %s: want boolean, have %s", v.Op, v.Args[2].Type.String());
                            }
                        else if (v.Op == OpAddPtr) 
                            if (!v.Args[0].Type.IsPtrShaped() && v.Args[0].Type != c.Types.Uintptr) {
                                f.Fatalf("bad arg 0 type to %s: want ptr, have %s", v.Op, v.Args[0].LongString());
                            }
                            if (!v.Args[1].Type.IsInteger()) {
                                f.Fatalf("bad arg 1 type to %s: want integer, have %s", v.Op, v.Args[1].LongString());
                            }


                        c = c__prev1;
                    } 

                    // TODO: check for cycles in values
                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    if (!blockMark[f.Entry.ID]) {
        f.Fatalf("entry block %v is missing", f.Entry);
    }
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var c__prev2 = c;

                foreach (var (_, __c) in b.Preds) {
                    c = __c;
                    if (!blockMark[c.b.ID]) {
                        f.Fatalf("predecessor block %v for %v is missing", c, b);
                    }
                }
                c = c__prev2;
            }

            {
                var c__prev2 = c;

                foreach (var (_, __c) in b.Succs) {
                    c = __c;
                    if (!blockMark[c.b.ID]) {
                        f.Fatalf("successor block %v for %v is missing", c, b);
                    }
                }
                c = c__prev2;
            }
        }
        b = b__prev1;
    }

    if (len(f.Entry.Preds) > 0) {
        f.Fatalf("entry block %s of %s has predecessor(s) %v", f.Entry, f.Name, f.Entry.Preds);
    }
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    {
                        var i__prev3 = i;
                        var a__prev3 = a;

                        foreach (var (__i, __a) in v.Args) {
                            i = __i;
                            a = __a;
                            if (!valueMark[a.ID]) {
                                f.Fatalf("%v, arg %d of %s, is missing", a, i, v.LongString());
                            }
                        }
                        i = i__prev3;
                        a = a__prev3;
                    }
                }
                v = v__prev2;
            }

            {
                var c__prev2 = c;

                foreach (var (_, __c) in b.ControlValues()) {
                    c = __c;
                    if (!valueMark[c.ID]) {
                        f.Fatalf("control value for %s is missing: %v", b, c);
                    }
                }
                c = c__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        var b = f.freeBlocks;

        while (b != null) {
            if (blockMark[b.ID]) {
                f.Fatalf("used block b%d in free list", b.ID);
            b = b.succstorage[0].b;
            }
        }

        b = b__prev1;
    }
    {
        var v__prev1 = v;

        var v = f.freeValues;

        while (v != null) {
            if (valueMark[v.ID]) {
                f.Fatalf("used value v%d in free list", v.ID);
            v = v.argstorage[0];
            }
        }

        v = v__prev1;
    } 

    // Check to make sure all args dominate uses.
    if (f.RegAlloc == null) { 
        // Note: regalloc introduces non-dominating args.
        // See TODO in regalloc.go.
        var sdom = f.Sdom();
        {
            var b__prev1 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        {
                            var i__prev3 = i;
                            var arg__prev3 = arg;

                            foreach (var (__i, __arg) in v.Args) {
                                i = __i;
                                arg = __arg;
                                var x = arg.Block;
                                var y = b;
                                if (v.Op == OpPhi) {
                                    y = b.Preds[i].b;
                                }
                                if (!domCheck(_addr_f, sdom, _addr_x, _addr_y)) {
                                    f.Fatalf("arg %d of value %s does not dominate, arg=%s", i, v.LongString(), arg.LongString());
                                }
                            }
                            i = i__prev3;
                            arg = arg__prev3;
                        }
                    }
                    v = v__prev2;
                }

                {
                    var c__prev2 = c;

                    foreach (var (_, __c) in b.ControlValues()) {
                        c = __c;
                        if (!domCheck(_addr_f, sdom, _addr_c.Block, _addr_b)) {
                            f.Fatalf("control value %s for %s doesn't dominate", c, b);
                        }
                    }
                    c = c__prev2;
                }
            }
            b = b__prev1;
        }
    }
    if (f.RegAlloc == null && f.pass != null) { // non-nil pass allows better-targeted debug printing
        var ln = f.loopnest();
        if (!ln.hasIrreducible) {
            var po = f.postorder(); // use po to avoid unreachable blocks.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in po) {
                    b = __b;
                    foreach (var (_, s) in b.Succs) {
                        var bb = s.Block();
                        if (ln.b2l[b.ID] == null && ln.b2l[bb.ID] != null && bb != ln.b2l[bb.ID].header) {
                            f.Fatalf("block %s not in loop branches to non-header block %s in loop", b.String(), bb.String());
                        }
                        if (ln.b2l[b.ID] != null && ln.b2l[bb.ID] != null && bb != ln.b2l[bb.ID].header && !ln.b2l[b.ID].isWithinOrEq(ln.b2l[bb.ID])) {
                            f.Fatalf("block %s in loop branches to non-header block %s in non-containing loop", b.String(), bb.String());
                        }
                    }
                }
                b = b__prev1;
            }
        }
    }
    var uses = make_slice<int>(f.NumValues());
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    {
                        var a__prev3 = a;

                        foreach (var (_, __a) in v.Args) {
                            a = __a;
                            uses[a.ID]++;
                        }
                        a = a__prev3;
                    }
                }
                v = v__prev2;
            }

            {
                var c__prev2 = c;

                foreach (var (_, __c) in b.ControlValues()) {
                    c = __c;
                    uses[c.ID]++;
                }
                c = c__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Uses != uses[v.ID]) {
                        f.Fatalf("%s has %d uses, but has Uses=%d", v, uses[v.ID], v.Uses);
                    }
                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    memCheck(_addr_f);

}

private static void memCheck(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Check that if a tuple has a memory type, it is second.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Type.IsTuple() && v.Type.FieldType(0).IsMemory()) {
                        f.Fatalf("memory is first in a tuple: %s\n", v.LongString());
                    }
                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if ((v.Op == OpCopy || v.Uses == 0) && v.Type.IsMemory()) {
                        return ;
                    }
                }

                v = v__prev2;
            }

            if (b != f.Entry && len(b.Preds) == 0) {
                return ;
            }

        }
        b = b__prev1;
    }

    var lastmem = make_slice<ptr<Value>>(f.NumBlocks());
    var ss = newSparseSet(f.NumValues());
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b; 
            // Mark overwritten memory values. Those are args of other
            // ops that generate memory values.
            ss.clear();
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op == OpPhi || !v.Type.IsMemory()) {
                        continue;
                    }
                    {
                        var m__prev1 = m;

                        var m = v.MemoryArg();

                        if (m != null) {
                            ss.add(m.ID);
                        }

                        m = m__prev1;

                    }

                } 
                // There should be at most one remaining unoverwritten memory value.

                v = v__prev2;
            }

            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (!v.Type.IsMemory()) {
                        continue;
                    }
                    if (ss.contains(v.ID)) {
                        continue;
                    }
                    if (lastmem[b.ID] != null) {
                        f.Fatalf("two live memory values in %s: %s and %s", b, lastmem[b.ID], v);
                    }
                    lastmem[b.ID] = v;
                } 
                // If there is no remaining memory value, that means there was no memory update.
                // Take any memory arg.

                v = v__prev2;
            }

            if (lastmem[b.ID] == null) {
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        if (v.Op == OpPhi) {
                            continue;
                        }
                        m = v.MemoryArg();
                        if (m == null) {
                            continue;
                        }
                        if (lastmem[b.ID] != null && lastmem[b.ID] != m) {
                            f.Fatalf("two live memory values in %s: %s and %s", b, lastmem[b.ID], m);
                        }
                        lastmem[b.ID] = m;
                    }

                    v = v__prev2;
                }
            }

        }
        b = b__prev1;
    }

    while (true) {
        var changed = false;
        {
            var b__prev2 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                if (lastmem[b.ID] != null) {
                    continue;
                }
                foreach (var (_, e) in b.Preds) {
                    var p = e.b;
                    if (lastmem[p.ID] != null) {
                        lastmem[b.ID] = lastmem[p.ID];
                        changed = true;
                        break;
                    }
                }
            }

            b = b__prev2;
        }

        if (!changed) {
            break;
        }
    } 
    // Check merge points.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op == OpPhi && v.Type.IsMemory()) {
                        {
                            var a__prev3 = a;

                            foreach (var (__i, __a) in v.Args) {
                                i = __i;
                                a = __a;
                                if (a != lastmem[b.Preds[i].b.ID]) {
                                    f.Fatalf("inconsistent memory phi %s %d %s %s", v.LongString(), i, a, lastmem[b.Preds[i].b.ID]);
                                }
                            }

                            a = a__prev3;
                        }
                    }

                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    if (f.scheduled) {
        {
            var b__prev1 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                ptr<Value> mem; // the current live memory in the block
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        if (v.Op == OpPhi) {
                            if (v.Type.IsMemory()) {
                                mem = v;
                            }
                            continue;
                        }
                        if (mem == null && len(b.Preds) > 0) { 
                            // If no mem phi, take mem of any predecessor.
                            mem = lastmem[b.Preds[0].b.ID];

                        }

                        {
                            var a__prev3 = a;

                            foreach (var (_, __a) in v.Args) {
                                a = __a;
                                if (a.Type.IsMemory() && a != mem) {
                                    f.Fatalf("two live mems @ %s: %s and %s", v, mem, a);
                                }
                            }

                            a = a__prev3;
                        }

                        if (v.Type.IsMemory()) {
                            mem = v;
                        }

                    }

                    v = v__prev2;
                }
            }

            b = b__prev1;
        }
    }
    if (f.scheduled) {
        {
            var b__prev1 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                var seenNonPhi = false;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;

                        if (v.Op == OpPhi) 
                            if (seenNonPhi) {
                                f.Fatalf("phi after non-phi @ %s: %s", b, v);
                            }
                        else 
                            seenNonPhi = true;
                        
                    }

                    v = v__prev2;
                }
            }

            b = b__prev1;
        }
    }
}

// domCheck reports whether x dominates y (including x==y).
private static bool domCheck(ptr<Func> _addr_f, SparseTree sdom, ptr<Block> _addr_x, ptr<Block> _addr_y) {
    ref Func f = ref _addr_f.val;
    ref Block x = ref _addr_x.val;
    ref Block y = ref _addr_y.val;

    if (!sdom.IsAncestorEq(f.Entry, y)) { 
        // unreachable - ignore
        return true;

    }
    return sdom.IsAncestorEq(x, y);

}

// isExactFloat32 reports whether x can be exactly represented as a float32.
private static bool isExactFloat32(double x) { 
    // Check the mantissa is in range.
    if (bits.TrailingZeros64(math.Float64bits(x)) < 52 - 23) {
        return false;
    }
    return math.IsNaN(x) || x == float64(float32(x));

}

} // end ssa_package
