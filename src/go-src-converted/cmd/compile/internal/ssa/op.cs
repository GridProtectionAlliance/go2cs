// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:50 UTC
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
            public bool zeroWidth; // op never translates into any machine code. example: copy, which may sometimes translate to machine code, is not zero-width.
            public bool unsafePoint; // this op is an unsafe point, i.e. not safe for async preemption
            public SymEffect symEffect; // effect this op has on symbol in aux
            public byte scale; // amd64/386 indexed load scale
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
            public slice<inputInfo> inputs; // clobbers encodes the set of registers that are overwritten by
// the instruction (other than the output registers).
            public regMask clobbers; // outputs is the same as inputs, but for the outputs of the instruction.
            public slice<outputInfo> outputs;
        }

        private partial struct auxType // : sbyte
        {
        }

        private static readonly auxType auxNone = (auxType)iota;
        private static readonly var auxBool = (var)0; // auxInt is 0/1 for false/true
        private static readonly var auxInt8 = (var)1; // auxInt is an 8-bit integer
        private static readonly var auxInt16 = (var)2; // auxInt is a 16-bit integer
        private static readonly var auxInt32 = (var)3; // auxInt is a 32-bit integer
        private static readonly var auxInt64 = (var)4; // auxInt is a 64-bit integer
        private static readonly var auxInt128 = (var)5; // auxInt represents a 128-bit integer.  Always 0.
        private static readonly var auxFloat32 = (var)6; // auxInt is a float32 (encoded with math.Float64bits)
        private static readonly var auxFloat64 = (var)7; // auxInt is a float64 (encoded with math.Float64bits)
        private static readonly var auxFlagConstant = (var)8; // auxInt is a flagConstant
        private static readonly var auxString = (var)9; // aux is a string
        private static readonly var auxSym = (var)10; // aux is a symbol (a *gc.Node for locals, an *obj.LSym for globals, or nil for none)
        private static readonly var auxSymOff = (var)11; // aux is a symbol, auxInt is an offset
        private static readonly var auxSymValAndOff = (var)12; // aux is a symbol, auxInt is a ValAndOff
        private static readonly var auxTyp = (var)13; // aux is a type
        private static readonly var auxTypSize = (var)14; // aux is a type, auxInt is a size, must have Aux.(Type).Size() == AuxInt
        private static readonly var auxCCop = (var)15; // aux is a ssa.Op that represents a flags-to-bool conversion (e.g. LessThan)

        // architecture specific aux types
        private static readonly var auxARM64BitField = (var)16; // aux is an arm64 bitfield lsb and width packed into auxInt
        private static readonly var auxS390XRotateParams = (var)17; // aux is a s390x rotate parameters object encoding start bit, end bit and rotate amount
        private static readonly var auxS390XCCMask = (var)18; // aux is a s390x 4-bit condition code mask
        private static readonly var auxS390XCCMaskInt8 = (var)19; // aux is a s390x 4-bit condition code mask, auxInt is a int8 immediate
        private static readonly var auxS390XCCMaskUint8 = (var)20; // aux is a s390x 4-bit condition code mask, auxInt is a uint8 immediate

        // A SymEffect describes the effect that an SSA Value has on the variable
        // identified by the symbol in its Aux field.
        public partial struct SymEffect // : sbyte
        {
        }

        public static readonly SymEffect SymRead = (SymEffect)1L << (int)(iota);
        public static readonly var SymWrite = (var)0;
        public static readonly SymRdWr SymAddr = (SymRdWr)SymRead | SymWrite;

        public static readonly SymEffect SymNone = (SymEffect)0L;


        // A Sym represents a symbolic offset from a base register.
        // Currently a Sym can be one of 3 things:
        //  - a *gc.Node, for an offset from SP (the stack pointer)
        //  - a *obj.LSym, for an offset from SB (the global pointer)
        //  - nil, for no offset
        public partial interface Sym
        {
            @string String();
            @string CanBeAnSSASym();
        }

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
        public static int Val32(this ValAndOff x)
        {
            return int32(int64(x) >> (int)(32L));
        }
        public static short Val16(this ValAndOff x)
        {
            return int16(int64(x) >> (int)(32L));
        }
        public static sbyte Val8(this ValAndOff x)
        {
            return int8(int64(x) >> (int)(32L));
        }

        public static long Off(this ValAndOff x)
        {
            return int64(int32(x));
        }
        public static int Off32(this ValAndOff x)
        {
            return int32(x);
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
        private static ValAndOff makeValAndOff32(int val, int off)
        {
            return ValAndOff(int64(val) << (int)(32L) + int64(uint32(off)));
        }

        public static bool canAdd(this ValAndOff x, long off)
        {
            var newoff = x.Off() + off;
            return newoff == int64(int32(newoff));
        }

        public static bool canAdd32(this ValAndOff x, int off)
        {
            var newoff = x.Off() + int64(off);
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

        public static ValAndOff addOffset32(this ValAndOff x, int off) => func((_, panic, __) =>
        {
            if (!x.canAdd32(off))
            {
                panic("invalid ValAndOff.add");
            }

            return ValAndOff(makeValAndOff(x.Val(), x.Off() + int64(off)));

        });

        public static ValAndOff addOffset64(this ValAndOff x, long off) => func((_, panic, __) =>
        {
            if (!x.canAdd(off))
            {
                panic("invalid ValAndOff.add");
            }

            return ValAndOff(makeValAndOff(x.Val(), x.Off() + off));

        });

        // int128 is a type that stores a 128-bit constant.
        // The only allowed constant right now is 0, so we can cheat quite a bit.
        private partial struct int128 // : long
        {
        }

        public partial struct BoundsKind // : byte
        {
        }

        public static readonly BoundsKind BoundsIndex = (BoundsKind)iota; // indexing operation, 0 <= idx < len failed
        public static readonly var BoundsIndexU = (var)0; // ... with unsigned idx
        public static readonly var BoundsSliceAlen = (var)1; // 2-arg slicing operation, 0 <= high <= len failed
        public static readonly var BoundsSliceAlenU = (var)2; // ... with unsigned high
        public static readonly var BoundsSliceAcap = (var)3; // 2-arg slicing operation, 0 <= high <= cap failed
        public static readonly var BoundsSliceAcapU = (var)4; // ... with unsigned high
        public static readonly var BoundsSliceB = (var)5; // 2-arg slicing operation, 0 <= low <= high failed
        public static readonly var BoundsSliceBU = (var)6; // ... with unsigned low
        public static readonly var BoundsSlice3Alen = (var)7; // 3-arg slicing operation, 0 <= max <= len failed
        public static readonly var BoundsSlice3AlenU = (var)8; // ... with unsigned max
        public static readonly var BoundsSlice3Acap = (var)9; // 3-arg slicing operation, 0 <= max <= cap failed
        public static readonly var BoundsSlice3AcapU = (var)10; // ... with unsigned max
        public static readonly var BoundsSlice3B = (var)11; // 3-arg slicing operation, 0 <= high <= max failed
        public static readonly var BoundsSlice3BU = (var)12; // ... with unsigned high
        public static readonly var BoundsSlice3C = (var)13; // 3-arg slicing operation, 0 <= low <= high failed
        public static readonly var BoundsSlice3CU = (var)14; // ... with unsigned low
        public static readonly var BoundsKindCount = (var)15;


        // boundsAPI determines which register arguments a bounds check call should use. For an [a:b:c] slice, we do:
        //   CMPQ c, cap
        //   JA   fail1
        //   CMPQ b, c
        //   JA   fail2
        //   CMPQ a, b
        //   JA   fail3
        //
        // fail1: CALL panicSlice3Acap (c, cap)
        // fail2: CALL panicSlice3B (b, c)
        // fail3: CALL panicSlice3C (a, b)
        //
        // When we register allocate that code, we want the same register to be used for
        // the first arg of panicSlice3Acap and the second arg to panicSlice3B. That way,
        // initializing that register once will satisfy both calls.
        // That desire ends up dividing the set of bounds check calls into 3 sets. This function
        // determines which set to use for a given panic call.
        // The first arg for set 0 should be the second arg for set 1.
        // The first arg for set 1 should be the second arg for set 2.
        private static long boundsABI(long b) => func((_, panic, __) =>
        {

            if (BoundsKind(b) == BoundsSlice3Alen || BoundsKind(b) == BoundsSlice3AlenU || BoundsKind(b) == BoundsSlice3Acap || BoundsKind(b) == BoundsSlice3AcapU) 
                return 0L;
            else if (BoundsKind(b) == BoundsSliceAlen || BoundsKind(b) == BoundsSliceAlenU || BoundsKind(b) == BoundsSliceAcap || BoundsKind(b) == BoundsSliceAcapU || BoundsKind(b) == BoundsSlice3B || BoundsKind(b) == BoundsSlice3BU) 
                return 1L;
            else if (BoundsKind(b) == BoundsIndex || BoundsKind(b) == BoundsIndexU || BoundsKind(b) == BoundsSliceB || BoundsKind(b) == BoundsSliceBU || BoundsKind(b) == BoundsSlice3C || BoundsKind(b) == BoundsSlice3CU) 
                return 2L;
            else 
                panic("bad BoundsKind");
            
        });
    }
}}}}
