// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:49 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\location.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // A place that an ssa variable can reside.
        public partial interface Location
        {
            @string String(); // name to use in assembly templates: AX, 16(SP), ...
        }

        // A Register is a machine register, like AX.
        // They are numbered densely from 0 (for each architecture).
        public partial struct Register
        {
            public int num; // dense numbering
            public short objNum; // register number from cmd/internal/obj/$ARCH
            public short gcNum; // GC register map number (dense numbering of registers that can contain pointers)
            public @string name;
        }

        private static @string String(this ptr<Register> _addr_r)
        {
            ref Register r = ref _addr_r.val;

            return r.name;
        }

        // ObjNum returns the register number from cmd/internal/obj/$ARCH that
        // corresponds to this register.
        private static short ObjNum(this ptr<Register> _addr_r)
        {
            ref Register r = ref _addr_r.val;

            return r.objNum;
        }

        // GCNum returns the runtime GC register index of r, or -1 if this
        // register can't contain pointers.
        private static short GCNum(this ptr<Register> _addr_r)
        {
            ref Register r = ref _addr_r.val;

            return r.gcNum;
        }

        // A LocalSlot is a location in the stack frame, which identifies and stores
        // part or all of a PPARAM, PPARAMOUT, or PAUTO ONAME node.
        // It can represent a whole variable, part of a larger stack slot, or part of a
        // variable that has been decomposed into multiple stack slots.
        // As an example, a string could have the following configurations:
        //
        //           stack layout              LocalSlots
        //
        // Optimizations are disabled. s is on the stack and represented in its entirety.
        // [ ------- s string ---- ] { N: s, Type: string, Off: 0 }
        //
        // s was not decomposed, but the SSA operates on its parts individually, so
        // there is a LocalSlot for each of its fields that points into the single stack slot.
        // [ ------- s string ---- ] { N: s, Type: *uint8, Off: 0 }, {N: s, Type: int, Off: 8}
        //
        // s was decomposed. Each of its fields is in its own stack slot and has its own LocalSLot.
        // [ ptr *uint8 ] [ len int] { N: ptr, Type: *uint8, Off: 0, SplitOf: parent, SplitOffset: 0},
        //                           { N: len, Type: int, Off: 0, SplitOf: parent, SplitOffset: 8}
        //                           parent = &{N: s, Type: string}
        public partial struct LocalSlot
        {
            public GCNode N; // an ONAME *gc.Node representing a stack location.
            public ptr<types.Type> Type; // type of slot
            public long Off; // offset of slot in N

            public ptr<LocalSlot> SplitOf; // slot is a decomposition of SplitOf
            public long SplitOffset; // .. at this offset.
        }

        public static @string String(this LocalSlot s)
        {
            if (s.Off == 0L)
            {
                return fmt.Sprintf("%v[%v]", s.N, s.Type);
            }

            return fmt.Sprintf("%v+%d[%v]", s.N, s.Off, s.Type);

        }

        public partial struct LocPair // : array<Location>
        {
        }

        public static @string String(this LocPair t)
        {
            @string n0 = "nil";
            @string n1 = "nil";
            if (t[0L] != null)
            {
                n0 = t[0L].String();
            }

            if (t[1L] != null)
            {
                n1 = t[1L].String();
            }

            return fmt.Sprintf("<%s,%s>", n0, n1);

        }
    }
}}}}
