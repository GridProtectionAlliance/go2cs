// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_linux_s390x.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct sigctxt
        {
            public ptr<siginfo> info;
            public unsafe.Pointer ctxt;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ptr<sigcontext> regs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return _addr_(sigcontext.val)(@unsafe.Pointer(_addr_(ucontext.val)(c.ctxt).uc_mcontext))!;
        }

        private static ulong r0(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[0L];
        }
        private static ulong r1(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[1L];
        }
        private static ulong r2(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[2L];
        }
        private static ulong r3(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[3L];
        }
        private static ulong r4(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[4L];
        }
        private static ulong r5(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[5L];
        }
        private static ulong r6(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[6L];
        }
        private static ulong r7(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[7L];
        }
        private static ulong r8(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[8L];
        }
        private static ulong r9(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[9L];
        }
        private static ulong r10(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[10L];
        }
        private static ulong r11(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[11L];
        }
        private static ulong r12(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[12L];
        }
        private static ulong r13(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[13L];
        }
        private static ulong r14(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[14L];
        }
        private static ulong r15(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[15L];
        }
        private static ulong link(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[14L];
        }
        private static ulong sp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().gregs[15L];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong pc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().psw_addr;
        }

        private static uint sigcode(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint32(c.info.si_code);
        }
        private static ulong sigaddr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.info.si_addr;
        }

        private static void set_r0(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().gregs[0L] = x;
        }
        private static void set_r13(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().gregs[13L] = x;
        }
        private static void set_link(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().gregs[14L] = x;
        }
        private static void set_sp(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().gregs[15L] = x;
        }
        private static void set_pc(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().psw_addr = x;
        }
        private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.info.si_code = int32(x);
        }
        private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            (uintptr.val)(add(@unsafe.Pointer(c.info), 2L * sys.PtrSize)).val;

            uintptr(x);

        }

        private static void dumpregs(ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            print("r0   ", hex(c.r0()), "\t");
            print("r1   ", hex(c.r1()), "\n");
            print("r2   ", hex(c.r2()), "\t");
            print("r3   ", hex(c.r3()), "\n");
            print("r4   ", hex(c.r4()), "\t");
            print("r5   ", hex(c.r5()), "\n");
            print("r6   ", hex(c.r6()), "\t");
            print("r7   ", hex(c.r7()), "\n");
            print("r8   ", hex(c.r8()), "\t");
            print("r9   ", hex(c.r9()), "\n");
            print("r10  ", hex(c.r10()), "\t");
            print("r11  ", hex(c.r11()), "\n");
            print("r12  ", hex(c.r12()), "\t");
            print("r13  ", hex(c.r13()), "\n");
            print("r14  ", hex(c.r14()), "\t");
            print("r15  ", hex(c.r15()), "\n");
            print("pc   ", hex(c.pc()), "\t");
            print("link ", hex(c.link()), "\n");
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

            return uintptr(c.link());
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
 
            // We arrange link, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save LINK to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.MinFrameSize;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.link();

            var pc = uintptr(gp.sigpc);

            if (shouldPushSigpanic(gp, pc, uintptr(c.link())))
            { 
                // Make it look the like faulting PC called sigpanic.
                c.set_link(uint64(pc));

            } 

            // In case we are panicking from external C code
            c.set_r0(0L);
            c.set_r13(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint64(funcPC(sigpanic)));

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Push the LR to stack, as we'll clobber it in order to
            // push the call. The function being pushed is responsible
            // for restoring the LR and setting the SP back.
            // This extra slot is known to gentraceback.
            var sp = c.sp() - 8L;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.link(); 
            // Set up PC and LR to pretend the function being signaled
            // calls targetPC at resumePC.
            c.set_link(uint64(resumePC));
            c.set_pc(uint64(targetPC));

        }
    }
}
