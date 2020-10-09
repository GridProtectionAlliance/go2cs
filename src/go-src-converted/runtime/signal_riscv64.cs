// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,riscv64

// package runtime -- go2cs converted at 2020 October 09 04:48:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_riscv64.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void dumpregs(ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            print("ra  ", hex(c.ra()), "\t");
            print("sp  ", hex(c.sp()), "\n");
            print("gp  ", hex(c.gp()), "\t");
            print("tp  ", hex(c.tp()), "\n");
            print("t0  ", hex(c.t0()), "\t");
            print("t1  ", hex(c.t1()), "\n");
            print("t2  ", hex(c.t2()), "\t");
            print("s0  ", hex(c.s0()), "\n");
            print("s1  ", hex(c.s1()), "\t");
            print("a0  ", hex(c.a0()), "\n");
            print("a1  ", hex(c.a1()), "\t");
            print("a2  ", hex(c.a2()), "\n");
            print("a3  ", hex(c.a3()), "\t");
            print("a4  ", hex(c.a4()), "\n");
            print("a5  ", hex(c.a5()), "\t");
            print("a6  ", hex(c.a6()), "\n");
            print("a7  ", hex(c.a7()), "\t");
            print("s2  ", hex(c.s2()), "\n");
            print("s3  ", hex(c.s3()), "\t");
            print("s4  ", hex(c.s4()), "\n");
            print("s5  ", hex(c.s5()), "\t");
            print("s6  ", hex(c.s6()), "\n");
            print("s7  ", hex(c.s7()), "\t");
            print("s8  ", hex(c.s8()), "\n");
            print("s9  ", hex(c.s9()), "\t");
            print("s10 ", hex(c.s10()), "\n");
            print("s11 ", hex(c.s11()), "\t");
            print("t3  ", hex(c.t3()), "\n");
            print("t4  ", hex(c.t4()), "\t");
            print("t5  ", hex(c.t5()), "\n");
            print("t6  ", hex(c.t6()), "\t");
            print("pc  ", hex(c.pc()), "\n");
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr sigpc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.pc());
        }

        private static System.UIntPtr sigsp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.sp());
        }
        private static System.UIntPtr siglr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.ra());
        }
        private static System.UIntPtr fault(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.sigaddr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ptr<sigctxt> _addr_c, uint sig, ptr<g> _addr_gp)
        {
            ref sigctxt c = ref _addr_c.val;
            ref g gp = ref _addr_gp.val;
 
            // We arrange RA, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save RA to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.PtrSize;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.ra();

            var pc = gp.sigpc;

            if (shouldPushSigpanic(gp, pc, uintptr(c.ra())))
            { 
                // Make it look the like faulting PC called sigpanic.
                c.set_ra(uint64(pc));

            } 

            // In case we are panicking from external C code
            c.set_gp(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint64(funcPC(sigpanic)));

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Push the LR to stack, as we'll clobber it in order to
            // push the call. The function being pushed is responsible
            // for restoring the LR and setting the SP back.
            // This extra slot is known to gentraceback.
            var sp = c.sp() - sys.PtrSize;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.ra(); 
            // Set up PC and LR to pretend the function being signaled
            // calls targetPC at resumePC.
            c.set_ra(uint64(resumePC));
            c.set_pc(uint64(targetPC));

        }
    }
}
