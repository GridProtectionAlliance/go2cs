// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:28 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\value.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using math = go.math_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // A Value represents a value in the SSA representation of the program.
        // The ID and Type fields must not be modified. The remainder may be modified
        // if they preserve the value of the Value (e.g. changing a (mul 2 x) to an (add x x)).
        public partial struct Value
        {
            public ID ID; // The operation that computes this value. See op.go.
            public Op Op; // The type of this value. Normally this will be a Go type, but there
// are a few other pseudo-types, see type.go.
            public ptr<types.Type> Type; // Auxiliary info for this value. The type of this information depends on the opcode and type.
// AuxInt is used for integer values, Aux is used for other values.
// Floats are stored in AuxInt using math.Float64bits(f).
            public long AuxInt;
            public slice<ref Value> Args; // Containing basic block
            public ptr<Block> Block; // Source position
            public src.XPos Pos; // Use count. Each appearance in Value.Args and Block.Control counts once.
            public int Uses; // Storage for the first three args
            public array<ref Value> argstorage;
        }

        // Examples:
        // Opcode          aux   args
        //  OpAdd          nil      2
        //  OpConst     string      0    string constant
        //  OpConst      int64      0    int64 constant
        //  OpAddcq      int64      1    amd64 op: v = arg[0] + constant

        // short form print. Just v#.
        private static @string String(this ref Value v)
        {
            if (v == null)
            {
                return "nil"; // should never happen, but not panicking helps with debugging
            }
            return fmt.Sprintf("v%d", v.ID);
        }

        private static sbyte AuxInt8(this ref Value v)
        {
            if (opcodeTable[v.Op].auxType != auxInt8)
            {
                v.Fatalf("op %s doesn't have an int8 aux field", v.Op);
            }
            return int8(v.AuxInt);
        }

        private static short AuxInt16(this ref Value v)
        {
            if (opcodeTable[v.Op].auxType != auxInt16)
            {
                v.Fatalf("op %s doesn't have an int16 aux field", v.Op);
            }
            return int16(v.AuxInt);
        }

        private static int AuxInt32(this ref Value v)
        {
            if (opcodeTable[v.Op].auxType != auxInt32)
            {
                v.Fatalf("op %s doesn't have an int32 aux field", v.Op);
            }
            return int32(v.AuxInt);
        }

        private static double AuxFloat(this ref Value v)
        {
            if (opcodeTable[v.Op].auxType != auxFloat32 && opcodeTable[v.Op].auxType != auxFloat64)
            {
                v.Fatalf("op %s doesn't have a float aux field", v.Op);
            }
            return math.Float64frombits(uint64(v.AuxInt));
        }
        private static ValAndOff AuxValAndOff(this ref Value v)
        {
            if (opcodeTable[v.Op].auxType != auxSymValAndOff)
            {
                v.Fatalf("op %s doesn't have a ValAndOff aux field", v.Op);
            }
            return ValAndOff(v.AuxInt);
        }

        // long form print.  v# = opcode <type> [aux] args [: reg] (names)
        private static @string LongString(this ref Value v)
        {
            var s = fmt.Sprintf("v%d = %s", v.ID, v.Op);
            s += " <" + v.Type.String() + ">";
            s += v.auxString();
            foreach (var (_, a) in v.Args)
            {
                s += fmt.Sprintf(" %v", a);
            }
            var r = v.Block.Func.RegAlloc;
            if (int(v.ID) < len(r) && r[v.ID] != null)
            {
                s += " : " + r[v.ID].String();
            }
            slice<@string> names = default;
            foreach (var (name, values) in v.Block.Func.NamedValues)
            {
                foreach (var (_, value) in values)
                {
                    if (value == v)
                    {
                        names = append(names, name.String());
                        break; // drop duplicates.
                    }
                }
            }
            if (len(names) != 0L)
            {
                sort.Strings(names); // Otherwise a source of variation in debugging output.
                s += " (" + strings.Join(names, ", ") + ")";
            }
            return s;
        }

        private static @string auxString(this ref Value v)
        {

            if (opcodeTable[v.Op].auxType == auxBool) 
                if (v.AuxInt == 0L)
                {
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
            else if (opcodeTable[v.Op].auxType == auxFloat32 || opcodeTable[v.Op].auxType == auxFloat64) 
                return fmt.Sprintf(" [%g]", v.AuxFloat());
            else if (opcodeTable[v.Op].auxType == auxString) 
                return fmt.Sprintf(" {%q}", v.Aux);
            else if (opcodeTable[v.Op].auxType == auxSym || opcodeTable[v.Op].auxType == auxTyp) 
                if (v.Aux != null)
                {
                    return fmt.Sprintf(" {%v}", v.Aux);
                }
            else if (opcodeTable[v.Op].auxType == auxSymOff || opcodeTable[v.Op].auxType == auxSymInt32 || opcodeTable[v.Op].auxType == auxTypSize) 
                @string s = "";
                if (v.Aux != null)
                {
                    s = fmt.Sprintf(" {%v}", v.Aux);
                }
                if (v.AuxInt != 0L)
                {
                    s += fmt.Sprintf(" [%v]", v.AuxInt);
                }
                return s;
            else if (opcodeTable[v.Op].auxType == auxSymValAndOff) 
                s = "";
                if (v.Aux != null)
                {
                    s = fmt.Sprintf(" {%v}", v.Aux);
                }
                return s + fmt.Sprintf(" [%s]", v.AuxValAndOff());
                        return "";
        }

        private static void AddArg(this ref Value v, ref Value w)
        {
            if (v.Args == null)
            {
                v.resetArgs(); // use argstorage
            }
            v.Args = append(v.Args, w);
            w.Uses++;
        }
        private static void AddArgs(this ref Value v, params ptr<Value>[] a)
        {
            if (v.Args == null)
            {
                v.resetArgs(); // use argstorage
            }
            v.Args = append(v.Args, a);
            foreach (var (_, x) in a)
            {
                x.Uses++;
            }
        }
        private static void SetArg(this ref Value v, long i, ref Value w)
        {
            v.Args[i].Uses--;
            v.Args[i] = w;
            w.Uses++;
        }
        private static void RemoveArg(this ref Value v, long i)
        {
            v.Args[i].Uses--;
            copy(v.Args[i..], v.Args[i + 1L..]);
            v.Args[len(v.Args) - 1L] = null; // aid GC
            v.Args = v.Args[..len(v.Args) - 1L];
        }
        private static void SetArgs1(this ref Value v, ref Value a)
        {
            v.resetArgs();
            v.AddArg(a);
        }
        private static void SetArgs2(this ref Value v, ref Value a, ref Value b)
        {
            v.resetArgs();
            v.AddArg(a);
            v.AddArg(b);
        }

        private static void resetArgs(this ref Value v)
        {
            foreach (var (_, a) in v.Args)
            {
                a.Uses--;
            }
            v.argstorage[0L] = null;
            v.argstorage[1L] = null;
            v.argstorage[2L] = null;
            v.Args = v.argstorage[..0L];
        }

        private static void reset(this ref Value v, Op op)
        {
            v.Op = op;
            v.resetArgs();
            v.AuxInt = 0L;
            v.Aux = null;
        }

        // copyInto makes a new value identical to v and adds it to the end of b.
        private static ref Value copyInto(this ref Value v, ref Block b)
        {
            var c = b.NewValue0(v.Pos, v.Op, v.Type); // Lose the position, this causes line number churn otherwise.
            c.Aux = v.Aux;
            c.AuxInt = v.AuxInt;
            c.AddArgs(v.Args);
            foreach (var (_, a) in v.Args)
            {
                if (a.Type.IsMemory())
                {
                    v.Fatalf("can't move a value with a memory arg %s", v.LongString());
                }
            }
            return c;
        }

        // copyIntoNoXPos makes a new value identical to v and adds it to the end of b.
        // The copied value receives no source code position to avoid confusing changes
        // in debugger information (the intended user is the register allocator).
        private static ref Value copyIntoNoXPos(this ref Value v, ref Block b)
        {
            return v.copyIntoWithXPos(b, src.NoXPos);
        }

        // copyIntoWithXPos makes a new value identical to v and adds it to the end of b.
        // The supplied position is used as the position of the new value.
        private static ref Value copyIntoWithXPos(this ref Value v, ref Block b, src.XPos pos)
        {
            var c = b.NewValue0(pos, v.Op, v.Type);
            c.Aux = v.Aux;
            c.AuxInt = v.AuxInt;
            c.AddArgs(v.Args);
            foreach (var (_, a) in v.Args)
            {
                if (a.Type.IsMemory())
                {
                    v.Fatalf("can't move a value with a memory arg %s", v.LongString());
                }
            }
            return c;
        }

        private static void Logf(this ref Value v, @string msg, params object[] args)
        {
            v.Block.Logf(msg, args);

        }
        private static bool Log(this ref Value v)
        {
            return v.Block.Log();
        }
        private static void Fatalf(this ref Value v, @string msg, params object[] args)
        {
            v.Block.Func.fe.Fatalf(v.Pos, msg, args);
        }

        // isGenericIntConst returns whether v is a generic integer constant.
        private static bool isGenericIntConst(this ref Value v)
        {
            return v != null && (v.Op == OpConst64 || v.Op == OpConst32 || v.Op == OpConst16 || v.Op == OpConst8);
        }

        // Reg returns the register assigned to v, in cmd/internal/obj/$ARCH numbering.
        private static short Reg(this ref Value v)
        {
            var reg = v.Block.Func.RegAlloc[v.ID];
            if (reg == null)
            {
                v.Fatalf("nil register for value: %s\n%s\n", v.LongString(), v.Block.Func);
            }
            return reg._<ref Register>().objNum;
        }

        // Reg0 returns the register assigned to the first output of v, in cmd/internal/obj/$ARCH numbering.
        private static short Reg0(this ref Value v)
        {
            LocPair reg = v.Block.Func.RegAlloc[v.ID]._<LocPair>()[0L];
            if (reg == null)
            {
                v.Fatalf("nil first register for value: %s\n%s\n", v.LongString(), v.Block.Func);
            }
            return reg._<ref Register>().objNum;
        }

        // Reg1 returns the register assigned to the second output of v, in cmd/internal/obj/$ARCH numbering.
        private static short Reg1(this ref Value v)
        {
            LocPair reg = v.Block.Func.RegAlloc[v.ID]._<LocPair>()[1L];
            if (reg == null)
            {
                v.Fatalf("nil second register for value: %s\n%s\n", v.LongString(), v.Block.Func);
            }
            return reg._<ref Register>().objNum;
        }

        private static @string RegName(this ref Value v)
        {
            var reg = v.Block.Func.RegAlloc[v.ID];
            if (reg == null)
            {
                v.Fatalf("nil register for value: %s\n%s\n", v.LongString(), v.Block.Func);
            }
            return reg._<ref Register>().name;
        }

        // MemoryArg returns the memory argument for the Value.
        // The returned value, if non-nil, will be memory-typed (or a tuple with a memory-typed second part).
        // Otherwise, nil is returned.
        private static ref Value MemoryArg(this ref Value v)
        {
            if (v.Op == OpPhi)
            {
                v.Fatalf("MemoryArg on Phi");
            }
            var na = len(v.Args);
            if (na == 0L)
            {
                return null;
            }
            {
                var m = v.Args[na - 1L];

                if (m.Type.IsMemory())
                {
                    return m;
                }

            }
            return null;
        }

        // LackingPos indicates whether v is a value that is unlikely to have a correct
        // position assigned to it.  Ignoring such values leads to more user-friendly positions
        // assigned to nearby values and the blocks containing them.
        private static bool LackingPos(this ref Value v)
        { 
            // The exact definition of LackingPos is somewhat heuristically defined and may change
            // in the future, for example if some of these operations are generated more carefully
            // with respect to their source position.
            return v.Op == OpVarDef || v.Op == OpVarKill || v.Op == OpVarLive || v.Op == OpPhi || (v.Op == OpFwdRef || v.Op == OpCopy) && v.Type == types.TypeMem;
        }
    }
}}}}
