// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64
// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 October 08 03:23:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_amd64.go
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

            print("rax    ", hex(c.rax()), "\n");
            print("rbx    ", hex(c.rbx()), "\n");
            print("rcx    ", hex(c.rcx()), "\n");
            print("rdx    ", hex(c.rdx()), "\n");
            print("rdi    ", hex(c.rdi()), "\n");
            print("rsi    ", hex(c.rsi()), "\n");
            print("rbp    ", hex(c.rbp()), "\n");
            print("rsp    ", hex(c.rsp()), "\n");
            print("r8     ", hex(c.r8()), "\n");
            print("r9     ", hex(c.r9()), "\n");
            print("r10    ", hex(c.r10()), "\n");
            print("r11    ", hex(c.r11()), "\n");
            print("r12    ", hex(c.r12()), "\n");
            print("r13    ", hex(c.r13()), "\n");
            print("r14    ", hex(c.r14()), "\n");
            print("r15    ", hex(c.r15()), "\n");
            print("rip    ", hex(c.rip()), "\n");
            print("rflags ", hex(c.rflags()), "\n");
            print("cs     ", hex(c.cs()), "\n");
            print("fs     ", hex(c.fs()), "\n");
            print("gs     ", hex(c.gs()), "\n");
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr sigpc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.rip());
        }

        private static System.UIntPtr sigsp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.rsp());
        }
        private static System.UIntPtr siglr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return 0L;
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
 
            // Work around Leopard bug that doesn't set FPE_INTDIV.
            // Look at instruction to see if it is a divide.
            // Not necessary in Snow Leopard (si_code will be != 0).
            if (GOOS == "darwin" && sig == _SIGFPE && gp.sigcode0 == 0L)
            {
                ptr<array<byte>> pc = new ptr<ptr<array<byte>>>(@unsafe.Pointer(gp.sigpc));
                long i = 0L;
                if (pc[i] & 0xF0UL == 0x40UL)
                { // 64-bit REX prefix
                    i++;

                }
                else if (pc[i] == 0x66UL)
                { // 16-bit instruction prefix
                    i++;

                }

                if (pc[i] == 0xF6UL || pc[i] == 0xF7UL)
                {
                    gp.sigcode0 = _FPE_INTDIV;
                }

            }

            pc = uintptr(c.rip());
            var sp = uintptr(c.rsp());

            if (shouldPushSigpanic(gp, pc, new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(sp))))
            {
                c.pushCall(funcPC(sigpanic), pc);
            }
            else
            { 
                // Not safe to push the call. Just clobber the frame.
                c.set_rip(uint64(funcPC(sigpanic)));

            }

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Make it look like we called target at resumePC.
            var sp = uintptr(c.rsp());
            sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

            resumePC;
            c.set_rsp(uint64(sp));
            c.set_rip(uint64(targetPC));

        }
    }
}
