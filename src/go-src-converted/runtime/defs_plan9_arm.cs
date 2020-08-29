// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_plan9_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _PAGESIZE = 0x1000UL;



        private partial struct ureg
        {
            public uint r0; /* general registers */
            public uint r1; /* ... */
            public uint r2; /* ... */
            public uint r3; /* ... */
            public uint r4; /* ... */
            public uint r5; /* ... */
            public uint r6; /* ... */
            public uint r7; /* ... */
            public uint r8; /* ... */
            public uint r9; /* ... */
            public uint r10; /* ... */
            public uint r11; /* ... */
            public uint r12; /* ... */
            public uint sp;
            public uint link; /* ... */
            public uint trap; /* trap type */
            public uint psr;
            public uint pc; /* interrupted addr */
        }

        private partial struct sigctxt
        {
            public ptr<ureg> u;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr pc(this ref sigctxt c)
        {
            return uintptr(c.u.pc);
        }

        private static System.UIntPtr sp(this ref sigctxt c)
        {
            return uintptr(c.u.sp);
        }
        private static System.UIntPtr lr(this ref sigctxt c)
        {
            return uintptr(c.u.link);
        }

        private static void setpc(this ref sigctxt c, System.UIntPtr x)
        {
            c.u.pc = uint32(x);

        }
        private static void setsp(this ref sigctxt c, System.UIntPtr x)
        {
            c.u.sp = uint32(x);

        }
        private static void setlr(this ref sigctxt c, System.UIntPtr x)
        {
            c.u.link = uint32(x);

        }
        private static void savelr(this ref sigctxt c, System.UIntPtr x)
        {
            c.u.r0 = uint32(x);

        }

        private static void dumpregs(ref ureg u)
        {
            print("r0    ", hex(u.r0), "\n");
            print("r1    ", hex(u.r1), "\n");
            print("r2    ", hex(u.r2), "\n");
            print("r3    ", hex(u.r3), "\n");
            print("r4    ", hex(u.r4), "\n");
            print("r5    ", hex(u.r5), "\n");
            print("r6    ", hex(u.r6), "\n");
            print("r7    ", hex(u.r7), "\n");
            print("r8    ", hex(u.r8), "\n");
            print("r9    ", hex(u.r9), "\n");
            print("r10   ", hex(u.r10), "\n");
            print("r11   ", hex(u.r11), "\n");
            print("r12   ", hex(u.r12), "\n");
            print("sp    ", hex(u.sp), "\n");
            print("link  ", hex(u.link), "\n");
            print("pc    ", hex(u.pc), "\n");
            print("psr   ", hex(u.psr), "\n");
        }

        private static void sigpanictramp()
;
    }
}
