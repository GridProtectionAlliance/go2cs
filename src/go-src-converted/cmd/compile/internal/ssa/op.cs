// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:11 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\op.go
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // An Op encodes the specific operation that a Value performs.
        // Opcodes' semantics can be modified by the type and aux fields of the Value.
        // For instance, OpAdd can be 32 or 64 bit, signed or unsigned, float or complex, depending on Value.Type.
        // Semantics of each op are described in the opcode files in gen/*Ops.go.
        // There is one file for generic (architecture-independent) ops and one file
        // for each architecture.
        public partial struct Op // : int
        {
        }

        private partial struct opInfo
        {
            public @string name;
            public regInfo reg;
            public auxType auxType;
            public int argLen; // the number of arguments, -1 if variable length
            public obj.As asm;
            public bool generic; // this is a generic (arch-independent) opcode
            public bool rematerializeable; // this op is rematerializeable
            public bool commutative; // this operation is commutative (e.g. addition)
            public bool resultInArg0; // (first, if a tuple) output of v and v.Args[0] must be allocated to the same register
            public bool resultNotInArgs; // outputs must not be allocated to the same registers as inputs
            public bool clobberFlags; // this op clobbers flags register
            public bool call; // is a function call
            public bool nilCheck; // this op is a nil check on arg0
            public bool faultOnNilArg0; // this op will fault if arg0 is nil (and aux encodes a small offset)
            public bool faultOnNilArg1; // this op will fault if arg1 is nil (and aux encodes a small offset)
            public bool usesScratch; // this op requires scratch memory space
            public bool hasSideEffects; // for "reasons", not to be eliminated.  E.g., atomic store, #19182.
            public SymEffect symEffect; // effect this op has on symbol in aux
        }

        private partial struct inputInfo
        {
            public long idx; // index in Args array
            public regMask regs; // allowed input registers
        }

        private partial struct outputInfo
        {
            public long idx; // index in output tuple
            public regMask regs; // allowed output registers
        }

        private partial struct regInfo
        {
            public slice<inputInfo> inputs; // ordered in register allocation order
            public regMask clobbers;
            public slice<outputInfo> outputs; // ordered in register allocation order
        }

        private partial struct auxType // : sbyte
        {
        }

        private static readonly auxType auxNone = iota;
        private static readonly var auxBool = 0; // auxInt is 0/1 for false/true
        private static readonly var auxInt8 = 1; // auxInt is an 8-bit integer
        private static readonly var auxInt16 = 2; // auxInt is a 16-bit integer
        private static readonly var auxInt32 = 3; // auxInt is a 32-bit integer
        private static readonly var auxInt64 = 4; // auxInt is a 64-bit integer
        private static readonly var auxInt128 = 5; // auxInt represents a 128-bit integer.  Always 0.
        private static readonly var auxFloat32 = 6; // auxInt is a float32 (encoded with math.Float64bits)
        private static readonly var auxFloat64 = 7; // auxInt is a float64 (encoded with math.Float64bits)
        private static readonly var auxString = 8; // aux is a string
        private static readonly var auxSym = 9; // aux is a symbol (a *gc.Node for locals or an *obj.LSym for globals)
        private static readonly var auxSymOff = 10; // aux is a symbol, auxInt is an offset
        private static readonly var auxSymValAndOff = 11; // aux is a symbol, auxInt is a ValAndOff
        private static readonly var auxTyp = 12; // aux is a type
        private static readonly var auxTypSize = 13; // aux is a type, auxInt is a size, must have Aux.(Type).Size() == AuxInt

        private static readonly var auxSymInt32 = 14; // aux is a symbol, auxInt is a 32-bit integer

        // A SymEffect describes the effect that an SSA Value has on the variable
        // identified by the symbol in its Aux field.
        public partial struct SymEffect // : sbyte
        {
        }

        public static readonly SymEffect SymRead = 1L << (int)(iota);
        public static readonly var SymWrite = 0;
        public static readonly SymRdWr SymAddr = SymRead | SymWrite;

        public static readonly SymEffect SymNone = 0L;

        // A ValAndOff is used by the several opcodes. It holds
        // both a value and a pointer offset.
        // A ValAndOff is intended to be encoded into an AuxInt field.
        // The zero ValAndOff encodes a value of 0 and an offset of 0.
        // The high 32 bits hold a value.
        // The low 32 bits hold a pointer offset.
        public partial struct ValAndOff // : long
        {
        }

        public static long Val(this ValAndOff x)
        {
            return int64(x) >> (int)(32L);
        }
        public static long Off(this ValAndOff x)
        {
            return int64(int32(x));
        }
        public static long Int64(this ValAndOff x)
        {
            return int64(x);
        }
        public static @string String(this ValAndOff x)
        {
            return fmt.Sprintf("val=%d,off=%d", x.Val(), x.Off());
        }

        // validVal reports whether the value can be used
        // as an argument to makeValAndOff.
        private static bool validVal(long val)
        {
            return val == int64(int32(val));
        }

        // validOff reports whether the offset can be used
        // as an argument to makeValAndOff.
        private static bool validOff(long off)
        {
            return off == int64(int32(off));
        }

        // validValAndOff reports whether we can fit the value and offset into
        // a ValAndOff value.
        private static bool validValAndOff(long val, long off)
        {
            if (!validVal(val))
            {
                return false;
            }
            if (!validOff(off))
            {
                return false;
            }
            return true;
        }

        // makeValAndOff encodes a ValAndOff into an int64 suitable for storing in an AuxInt field.
        private static long makeValAndOff(long val, long off) => func((_, panic, __) =>
        {
            if (!validValAndOff(val, off))
            {
                panic("invalid makeValAndOff");
            }
            return ValAndOff(val << (int)(32L) + int64(uint32(off))).Int64();
        });

        public static bool canAdd(this ValAndOff x, long off)
        {
            var newoff = x.Off() + off;
            return newoff == int64(int32(newoff));
        }

        public static long add(this ValAndOff x, long off) => func((_, panic, __) =>
        {
            if (!x.canAdd(off))
            {
                panic("invalid ValAndOff.add");
            }
            return makeValAndOff(x.Val(), x.Off() + off);
        });
    }
}}}}
