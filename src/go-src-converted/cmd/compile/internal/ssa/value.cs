// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:08:51 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\value.go
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using math = go.math_package;
using sort = go.sort_package;
using strings = go.strings_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // A Value represents a value in the SSA representation of the program.
    // The ID and Type fields must not be modified. The remainder may be modified
    // if they preserve the value of the Value (e.g. changing a (mul 2 x) to an (add x x)).
public partial struct Value {
    public ID ID; // The operation that computes this value. See op.go.
    public Op Op; // The type of this value. Normally this will be a Go type, but there
// are a few other pseudo-types, see ../types/type.go.
    public ptr<types.Type> Type; // Auxiliary info for this value. The type of this information depends on the opcode and type.
// AuxInt is used for integer values, Aux is used for other values.
// Floats are stored in AuxInt using math.Float64bits(f).
// Unused portions of AuxInt are filled by sign-extending the used portion,
// even if the represented value is unsigned.
// Users of AuxInt which interpret AuxInt as unsigned (e.g. shifts) must be careful.
// Use Value.AuxUnsigned to get the zero-extended value of AuxInt.
    public long AuxInt;
    public Aux Aux; // Arguments of this value
    public slice<ptr<Value>> Args; // Containing basic block
    public ptr<Block> Block; // Source position
    public src.XPos Pos; // Use count. Each appearance in Value.Args and Block.Controls counts once.
    public int Uses; // wasm: Value stays on the WebAssembly stack. This value will not get a "register" (WebAssembly variable)
// nor a slot on Go stack, and the generation of this value is delayed to its use time.
    public bool OnWasmStack; // Is this value in the per-function constant cache? If so, remove from cache before changing it or recycling it.
    public bool InCache; // Storage for the first three args
    public array<ptr<Value>> argstorage;
}

// Examples:
// Opcode          aux   args
//  OpAdd          nil      2
//  OpConst     string      0    string constant
//  OpConst      int64      0    int64 constant
//  OpAddcq      int64      1    amd64 op: v = arg[0] + constant

// short form print. Just v#.
private static @string String(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (v == null) {
        return "nil"; // should never happen, but not panicking helps with debugging
    }
    return fmt.Sprintf("v%d", v.ID);

}

private static sbyte AuxInt8(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxInt8 && opcodeTable[v.Op].auxType != auxNameOffsetInt8) {
        v.Fatalf("op %s doesn't have an int8 aux field", v.Op);
    }
    return int8(v.AuxInt);

}

private static short AuxInt16(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxInt16) {
        v.Fatalf("op %s doesn't have an int16 aux field", v.Op);
    }
    return int16(v.AuxInt);

}

private static int AuxInt32(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxInt32) {
        v.Fatalf("op %s doesn't have an int32 aux field", v.Op);
    }
    return int32(v.AuxInt);

}

// AuxUnsigned returns v.AuxInt as an unsigned value for OpConst*.
// v.AuxInt is always sign-extended to 64 bits, even if the
// represented value is unsigned. This undoes that sign extension.
private static ulong AuxUnsigned(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var c = v.AuxInt;

    if (v.Op == OpConst64) 
        return uint64(c);
    else if (v.Op == OpConst32) 
        return uint64(uint32(c));
    else if (v.Op == OpConst16) 
        return uint64(uint16(c));
    else if (v.Op == OpConst8) 
        return uint64(uint8(c));
        v.Fatalf("op %s isn't OpConst*", v.Op);
    return 0;

}

private static double AuxFloat(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxFloat32 && opcodeTable[v.Op].auxType != auxFloat64) {
        v.Fatalf("op %s doesn't have a float aux field", v.Op);
    }
    return math.Float64frombits(uint64(v.AuxInt));

}
private static ValAndOff AuxValAndOff(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxSymValAndOff) {
        v.Fatalf("op %s doesn't have a ValAndOff aux field", v.Op);
    }
    return ValAndOff(v.AuxInt);

}

private static arm64BitField AuxArm64BitField(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (opcodeTable[v.Op].auxType != auxARM64BitField) {
        v.Fatalf("op %s doesn't have a ValAndOff aux field", v.Op);
    }
    return arm64BitField(v.AuxInt);

}

// long form print.  v# = opcode <type> [aux] args [: reg] (names)
private static @string LongString(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (v == null) {
        return "<NIL VALUE>";
    }
    var s = fmt.Sprintf("v%d = %s", v.ID, v.Op);
    s += " <" + v.Type.String() + ">";
    s += v.auxString();
    foreach (var (_, a) in v.Args) {
        s += fmt.Sprintf(" %v", a);
    }    slice<Location> r = default;
    if (v.Block != null) {
        r = v.Block.Func.RegAlloc;
    }
    if (int(v.ID) < len(r) && r[v.ID] != null) {
        s += " : " + r[v.ID].String();
    }
    slice<@string> names = default;
    if (v.Block != null) {
        foreach (var (name, values) in v.Block.Func.NamedValues) {
            foreach (var (_, value) in values) {
                if (value == v) {
                    names = append(names, name.String());
                    break; // drop duplicates.
                }

            }

        }
    }
    if (len(names) != 0) {
        sort.Strings(names); // Otherwise a source of variation in debugging output.
        s += " (" + strings.Join(names, ", ") + ")";

    }
    return s;

}

private static @string auxString(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (opcodeTable[v.Op].auxType == auxBool) 
        if (v.AuxInt == 0) {
            return " [false]";
        }
        else
 {
            return " [true]";
        }
    else if (opcodeTable[v.Op].auxType == auxInt8) 
        return fmt.Sprintf(" [%d]", v.AuxInt8());
    else if (opcodeTable[v.Op].auxType == auxInt16) 
        return fmt.Sprintf(" [%d]", v.AuxInt16());
    else if (opcodeTable[v.Op].auxType == auxInt32) 
        return fmt.Sprintf(" [%d]", v.AuxInt32());
    else if (opcodeTable[v.Op].auxType == auxInt64 || opcodeTable[v.Op].auxType == auxInt128) 
        return fmt.Sprintf(" [%d]", v.AuxInt);
    else if (opcodeTable[v.Op].auxType == auxARM64BitField) 
        var lsb = v.AuxArm64BitField().getARM64BFlsb();
        var width = v.AuxArm64BitField().getARM64BFwidth();
        return fmt.Sprintf(" [lsb=%d,width=%d]", lsb, width);
    else if (opcodeTable[v.Op].auxType == auxFloat32 || opcodeTable[v.Op].auxType == auxFloat64) 
        return fmt.Sprintf(" [%g]", v.AuxFloat());
    else if (opcodeTable[v.Op].auxType == auxString) 
        return fmt.Sprintf(" {%q}", v.Aux);
    else if (opcodeTable[v.Op].auxType == auxSym || opcodeTable[v.Op].auxType == auxCall || opcodeTable[v.Op].auxType == auxTyp) 
        if (v.Aux != null) {
            return fmt.Sprintf(" {%v}", v.Aux);
        }
    else if (opcodeTable[v.Op].auxType == auxSymOff || opcodeTable[v.Op].auxType == auxCallOff || opcodeTable[v.Op].auxType == auxTypSize || opcodeTable[v.Op].auxType == auxNameOffsetInt8) 
        @string s = "";
        if (v.Aux != null) {
            s = fmt.Sprintf(" {%v}", v.Aux);
        }
        if (v.AuxInt != 0 || opcodeTable[v.Op].auxType == auxNameOffsetInt8) {
            s += fmt.Sprintf(" [%v]", v.AuxInt);
        }
        return s;
    else if (opcodeTable[v.Op].auxType == auxSymValAndOff) 
        s = "";
        if (v.Aux != null) {
            s = fmt.Sprintf(" {%v}", v.Aux);
        }
        return s + fmt.Sprintf(" [%s]", v.AuxValAndOff());
    else if (opcodeTable[v.Op].auxType == auxCCop) 
        return fmt.Sprintf(" {%s}", Op(v.AuxInt));
    else if (opcodeTable[v.Op].auxType == auxS390XCCMask || opcodeTable[v.Op].auxType == auxS390XRotateParams) 
        return fmt.Sprintf(" {%v}", v.Aux);
    else if (opcodeTable[v.Op].auxType == auxFlagConstant) 
        return fmt.Sprintf("[%s]", flagConstant(v.AuxInt));
        return "";

}

// If/when midstack inlining is enabled (-l=4), the compiler gets both larger and slower.
// Not-inlining this method is a help (*Value.reset and *Block.NewValue0 are similar).
//go:noinline
private static void AddArg(this ptr<Value> _addr_v, ptr<Value> _addr_w) {
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    if (v.Args == null) {
        v.resetArgs(); // use argstorage
    }
    v.Args = append(v.Args, w);
    w.Uses++;

}

//go:noinline
private static void AddArg2(this ptr<Value> _addr_v, ptr<Value> _addr_w1, ptr<Value> _addr_w2) {
    ref Value v = ref _addr_v.val;
    ref Value w1 = ref _addr_w1.val;
    ref Value w2 = ref _addr_w2.val;

    if (v.Args == null) {
        v.resetArgs(); // use argstorage
    }
    v.Args = append(v.Args, w1, w2);
    w1.Uses++;
    w2.Uses++;

}

//go:noinline
private static void AddArg3(this ptr<Value> _addr_v, ptr<Value> _addr_w1, ptr<Value> _addr_w2, ptr<Value> _addr_w3) {
    ref Value v = ref _addr_v.val;
    ref Value w1 = ref _addr_w1.val;
    ref Value w2 = ref _addr_w2.val;
    ref Value w3 = ref _addr_w3.val;

    if (v.Args == null) {
        v.resetArgs(); // use argstorage
    }
    v.Args = append(v.Args, w1, w2, w3);
    w1.Uses++;
    w2.Uses++;
    w3.Uses++;

}

//go:noinline
private static void AddArg4(this ptr<Value> _addr_v, ptr<Value> _addr_w1, ptr<Value> _addr_w2, ptr<Value> _addr_w3, ptr<Value> _addr_w4) {
    ref Value v = ref _addr_v.val;
    ref Value w1 = ref _addr_w1.val;
    ref Value w2 = ref _addr_w2.val;
    ref Value w3 = ref _addr_w3.val;
    ref Value w4 = ref _addr_w4.val;

    v.Args = append(v.Args, w1, w2, w3, w4);
    w1.Uses++;
    w2.Uses++;
    w3.Uses++;
    w4.Uses++;
}

//go:noinline
private static void AddArg5(this ptr<Value> _addr_v, ptr<Value> _addr_w1, ptr<Value> _addr_w2, ptr<Value> _addr_w3, ptr<Value> _addr_w4, ptr<Value> _addr_w5) {
    ref Value v = ref _addr_v.val;
    ref Value w1 = ref _addr_w1.val;
    ref Value w2 = ref _addr_w2.val;
    ref Value w3 = ref _addr_w3.val;
    ref Value w4 = ref _addr_w4.val;
    ref Value w5 = ref _addr_w5.val;

    v.Args = append(v.Args, w1, w2, w3, w4, w5);
    w1.Uses++;
    w2.Uses++;
    w3.Uses++;
    w4.Uses++;
    w5.Uses++;
}

//go:noinline
private static void AddArg6(this ptr<Value> _addr_v, ptr<Value> _addr_w1, ptr<Value> _addr_w2, ptr<Value> _addr_w3, ptr<Value> _addr_w4, ptr<Value> _addr_w5, ptr<Value> _addr_w6) {
    ref Value v = ref _addr_v.val;
    ref Value w1 = ref _addr_w1.val;
    ref Value w2 = ref _addr_w2.val;
    ref Value w3 = ref _addr_w3.val;
    ref Value w4 = ref _addr_w4.val;
    ref Value w5 = ref _addr_w5.val;
    ref Value w6 = ref _addr_w6.val;

    v.Args = append(v.Args, w1, w2, w3, w4, w5, w6);
    w1.Uses++;
    w2.Uses++;
    w3.Uses++;
    w4.Uses++;
    w5.Uses++;
    w6.Uses++;
}

private static void AddArgs(this ptr<Value> _addr_v, params ptr<ptr<Value>>[] _addr_a) {
    a = a.Clone();
    ref Value v = ref _addr_v.val;
    ref Value a = ref _addr_a.val;

    if (v.Args == null) {
        v.resetArgs(); // use argstorage
    }
    v.Args = append(v.Args, a);
    foreach (var (_, x) in a) {
        x.Uses++;
    }
}
private static void SetArg(this ptr<Value> _addr_v, nint i, ptr<Value> _addr_w) {
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    v.Args[i].Uses--;
    v.Args[i] = w;
    w.Uses++;
}
private static void RemoveArg(this ptr<Value> _addr_v, nint i) {
    ref Value v = ref _addr_v.val;

    v.Args[i].Uses--;
    copy(v.Args[(int)i..], v.Args[(int)i + 1..]);
    v.Args[len(v.Args) - 1] = null; // aid GC
    v.Args = v.Args[..(int)len(v.Args) - 1];

}
private static void SetArgs1(this ptr<Value> _addr_v, ptr<Value> _addr_a) {
    ref Value v = ref _addr_v.val;
    ref Value a = ref _addr_a.val;

    v.resetArgs();
    v.AddArg(a);
}
private static void SetArgs2(this ptr<Value> _addr_v, ptr<Value> _addr_a, ptr<Value> _addr_b) {
    ref Value v = ref _addr_v.val;
    ref Value a = ref _addr_a.val;
    ref Value b = ref _addr_b.val;

    v.resetArgs();
    v.AddArg(a);
    v.AddArg(b);
}
private static void SetArgs3(this ptr<Value> _addr_v, ptr<Value> _addr_a, ptr<Value> _addr_b, ptr<Value> _addr_c) {
    ref Value v = ref _addr_v.val;
    ref Value a = ref _addr_a.val;
    ref Value b = ref _addr_b.val;
    ref Value c = ref _addr_c.val;

    v.resetArgs();
    v.AddArg(a);
    v.AddArg(b);
    v.AddArg(c);
}

private static void resetArgs(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    foreach (var (_, a) in v.Args) {
        a.Uses--;
    }    v.argstorage[0] = null;
    v.argstorage[1] = null;
    v.argstorage[2] = null;
    v.Args = v.argstorage[..(int)0];
}

// reset is called from most rewrite rules.
// Allowing it to be inlined increases the size
// of cmd/compile by almost 10%, and slows it down.
//go:noinline
private static void reset(this ptr<Value> _addr_v, Op op) {
    ref Value v = ref _addr_v.val;

    if (v.InCache) {
        v.Block.Func.unCache(v);
    }
    v.Op = op;
    v.resetArgs();
    v.AuxInt = 0;
    v.Aux = null;

}

// invalidateRecursively marks a value as invalid (unused)
// and after decrementing reference counts on its Args,
// also recursively invalidates any of those whose use
// count goes to zero.
//
// BEWARE of doing this *before* you've applied intended
// updates to SSA.
private static void invalidateRecursively(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (v.InCache) {
        v.Block.Func.unCache(v);
    }
    v.Op = OpInvalid;

    foreach (var (_, a) in v.Args) {
        a.Uses--;
        if (a.Uses == 0) {
            a.invalidateRecursively();
        }
    }    v.argstorage[0] = null;
    v.argstorage[1] = null;
    v.argstorage[2] = null;
    v.Args = v.argstorage[..(int)0];

    v.AuxInt = 0;
    v.Aux = null;

}

// copyOf is called from rewrite rules.
// It modifies v to be (Copy a).
//go:noinline
private static void copyOf(this ptr<Value> _addr_v, ptr<Value> _addr_a) {
    ref Value v = ref _addr_v.val;
    ref Value a = ref _addr_a.val;

    if (v == a) {
        return ;
    }
    if (v.InCache) {
        v.Block.Func.unCache(v);
    }
    v.Op = OpCopy;
    v.resetArgs();
    v.AddArg(a);
    v.AuxInt = 0;
    v.Aux = null;
    v.Type = a.Type;

}

// copyInto makes a new value identical to v and adds it to the end of b.
// unlike copyIntoWithXPos this does not check for v.Pos being a statement.
private static ptr<Value> copyInto(this ptr<Value> _addr_v, ptr<Block> _addr_b) {
    ref Value v = ref _addr_v.val;
    ref Block b = ref _addr_b.val;

    var c = b.NewValue0(v.Pos.WithNotStmt(), v.Op, v.Type); // Lose the position, this causes line number churn otherwise.
    c.Aux = v.Aux;
    c.AuxInt = v.AuxInt;
    c.AddArgs(v.Args);
    foreach (var (_, a) in v.Args) {
        if (a.Type.IsMemory()) {
            v.Fatalf("can't move a value with a memory arg %s", v.LongString());
        }
    }    return _addr_c!;

}

// copyIntoWithXPos makes a new value identical to v and adds it to the end of b.
// The supplied position is used as the position of the new value.
// Because this is used for rematerialization, check for case that (rematerialized)
// input to value with position 'pos' carried a statement mark, and that the supplied
// position (of the instruction using the rematerialized value) is not marked, and
// preserve that mark if its line matches the supplied position.
private static ptr<Value> copyIntoWithXPos(this ptr<Value> _addr_v, ptr<Block> _addr_b, src.XPos pos) {
    ref Value v = ref _addr_v.val;
    ref Block b = ref _addr_b.val;

    if (v.Pos.IsStmt() == src.PosIsStmt && pos.IsStmt() != src.PosIsStmt && v.Pos.SameFileAndLine(pos)) {
        pos = pos.WithIsStmt();
    }
    var c = b.NewValue0(pos, v.Op, v.Type);
    c.Aux = v.Aux;
    c.AuxInt = v.AuxInt;
    c.AddArgs(v.Args);
    foreach (var (_, a) in v.Args) {
        if (a.Type.IsMemory()) {
            v.Fatalf("can't move a value with a memory arg %s", v.LongString());
        }
    }    return _addr_c!;

}

private static void Logf(this ptr<Value> _addr_v, @string msg, params object[] args) {
    args = args.Clone();
    ref Value v = ref _addr_v.val;

    v.Block.Logf(msg, args);
}
private static bool Log(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    return v.Block.Log();
}
private static void Fatalf(this ptr<Value> _addr_v, @string msg, params object[] args) {
    args = args.Clone();
    ref Value v = ref _addr_v.val;

    v.Block.Func.fe.Fatalf(v.Pos, msg, args);
}

// isGenericIntConst reports whether v is a generic integer constant.
private static bool isGenericIntConst(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    return v != null && (v.Op == OpConst64 || v.Op == OpConst32 || v.Op == OpConst16 || v.Op == OpConst8);
}

// ResultReg returns the result register assigned to v, in cmd/internal/obj/$ARCH numbering.
// It is similar to Reg and Reg0, except that it is usable interchangeably for all Value Ops.
// If you know v.Op, using Reg or Reg0 (as appropriate) will be more efficient.
private static short ResultReg(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var reg = v.Block.Func.RegAlloc[v.ID];
    if (reg == null) {
        v.Fatalf("nil reg for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    {
        LocPair (pair, ok) = reg._<LocPair>();

        if (ok) {
            reg = pair[0];
        }
    }

    if (reg == null) {
        v.Fatalf("nil reg0 for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    return reg._<ptr<Register>>().objNum;

}

// Reg returns the register assigned to v, in cmd/internal/obj/$ARCH numbering.
private static short Reg(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var reg = v.Block.Func.RegAlloc[v.ID];
    if (reg == null) {
        v.Fatalf("nil register for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    return reg._<ptr<Register>>().objNum;

}

// Reg0 returns the register assigned to the first output of v, in cmd/internal/obj/$ARCH numbering.
private static short Reg0(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    LocPair reg = v.Block.Func.RegAlloc[v.ID]._<LocPair>()[0];
    if (reg == null) {
        v.Fatalf("nil first register for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    return reg._<ptr<Register>>().objNum;

}

// Reg1 returns the register assigned to the second output of v, in cmd/internal/obj/$ARCH numbering.
private static short Reg1(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    LocPair reg = v.Block.Func.RegAlloc[v.ID]._<LocPair>()[1];
    if (reg == null) {
        v.Fatalf("nil second register for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    return reg._<ptr<Register>>().objNum;

}

private static @string RegName(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var reg = v.Block.Func.RegAlloc[v.ID];
    if (reg == null) {
        v.Fatalf("nil register for value: %s\n%s\n", v.LongString(), v.Block.Func);
    }
    return reg._<ptr<Register>>().name;

}

// MemoryArg returns the memory argument for the Value.
// The returned value, if non-nil, will be memory-typed (or a tuple with a memory-typed second part).
// Otherwise, nil is returned.
private static ptr<Value> MemoryArg(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (v.Op == OpPhi) {
        v.Fatalf("MemoryArg on Phi");
    }
    var na = len(v.Args);
    if (na == 0) {
        return _addr_null!;
    }
    {
        var m = v.Args[na - 1];

        if (m.Type.IsMemory()) {
            return _addr_m!;
        }
    }

    return _addr_null!;

}

// LackingPos indicates whether v is a value that is unlikely to have a correct
// position assigned to it.  Ignoring such values leads to more user-friendly positions
// assigned to nearby values and the blocks containing them.
private static bool LackingPos(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // The exact definition of LackingPos is somewhat heuristically defined and may change
    // in the future, for example if some of these operations are generated more carefully
    // with respect to their source position.
    return v.Op == OpVarDef || v.Op == OpVarKill || v.Op == OpVarLive || v.Op == OpPhi || (v.Op == OpFwdRef || v.Op == OpCopy) && v.Type == types.TypeMem;

}

// removeable reports whether the value v can be removed from the SSA graph entirely
// if its use count drops to 0.
private static bool removeable(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (v.Type.IsVoid()) { 
        // Void ops, like nil pointer checks, must stay.
        return false;

    }
    if (v.Type.IsMemory()) { 
        // We don't need to preserve all memory ops, but we do need
        // to keep calls at least (because they might have
        // synchronization operations we can't see).
        return false;

    }
    if (v.Op.HasSideEffects()) { 
        // These are mostly synchronization operations.
        return false;

    }
    return true;

}

// TODO(mdempsky): Shouldn't be necessary; see discussion at golang.org/cl/275756
private static void CanBeAnSSAAux(this ptr<Value> _addr__p0) {
    ref Value _p0 = ref _addr__p0.val;

}

// AutoVar returns a *Name and int64 representing the auto variable and offset within it
// where v should be spilled.
public static (ptr<ir.Name>, long) AutoVar(ptr<Value> _addr_v) {
    ptr<ir.Name> _p0 = default!;
    long _p0 = default;
    ref Value v = ref _addr_v.val;

    {
        LocalSlot (loc, ok) = v.Block.Func.RegAlloc[v.ID]._<LocalSlot>();

        if (ok) {
            if (v.Type.Size() > loc.Type.Size()) {
                v.Fatalf("spill/restore type %s doesn't fit in slot type %s", v.Type, loc.Type);
            }
            return (_addr_loc.N!, loc.Off);
        }
    } 
    // Assume it is a register, return its spill slot, which needs to be live
    ptr<AuxNameOffset> nameOff = v.Aux._<ptr<AuxNameOffset>>();
    return (_addr_nameOff.Name!, nameOff.Offset);

}

} // end ssa_package
