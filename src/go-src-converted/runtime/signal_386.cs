// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd

// package runtime -- go2cs converted at 2020 August 29 08:19:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_386.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static void dumpregs(ref sigctxt c)
        {
            print("eax    ", hex(c.eax()), "\n");
            print("ebx    ", hex(c.ebx()), "\n");
            print("ecx    ", hex(c.ecx()), "\n");
            print("edx    ", hex(c.edx()), "\n");
            print("edi    ", hex(c.edi()), "\n");
            print("esi    ", hex(c.esi()), "\n");
            print("ebp    ", hex(c.ebp()), "\n");
            print("esp    ", hex(c.esp()), "\n");
            print("eip    ", hex(c.eip()), "\n");
            print("eflags ", hex(c.eflags()), "\n");
            print("cs     ", hex(c.cs()), "\n");
            print("fs     ", hex(c.fs()), "\n");
            print("gs     ", hex(c.gs()), "\n");
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr sigpc(this ref sigctxt c)
        {
            return uintptr(c.eip());
        }

        private static System.UIntPtr sigsp(this ref sigctxt c)
        {
            return uintptr(c.esp());
        }
        private static System.UIntPtr siglr(this ref sigctxt c)
        {
            return 0L;
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.sigaddr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ref sigctxt c, uint sig, ref g gp)
        {
            if (GOOS == "darwin")
            { 
                // Work around Leopard bug that doesn't set FPE_INTDIV.
                // Look at instruction to see if it is a divide.
                // Not necessary in Snow Leopard (si_code will be != 0).
                if (sig == _SIGFPE && gp.sigcode0 == 0L)
                {
                    ref array<byte> pc = new ptr<ref array<byte>>(@unsafe.Pointer(gp.sigpc));
                    long i = 0L;
                    if (pc[i] == 0x66UL)
                    { // 16-bit instruction prefix
                        i++;
                    }
                    if (pc[i] == 0xF6UL || pc[i] == 0xF7UL)
                    {
                        gp.sigcode0 = _FPE_INTDIV;
                    }
                }
            }
            pc = uintptr(c.eip());
            var sp = uintptr(c.esp()); 

            // If we don't recognize the PC as code
            // but we do recognize the top pointer on the stack as code,
            // then assume this was a call to non-code and treat like
            // pc == 0, to make unwinding show the context.
            if (pc != 0L && !findfunc(pc).valid() && findfunc(@unsafe.Pointer(sp).Value).valid())
            {
                pc = 0L;
            } 

            // Only push runtime.sigpanic if pc != 0.
            // If pc == 0, probably panicked because of a
            // call to a nil func. Not pushing that onto sp will
            // make the trace look like a call to runtime.sigpanic instead.
            // (Otherwise the trace will end at runtime.sigpanic and we
            // won't get to see who faulted.)
            if (pc != 0L)
            {
                if (sys.RegSize > sys.PtrSize)
                {
                    sp -= sys.PtrSize * (uintptr.Value)(@unsafe.Pointer(sp));

                    0L;
                }
                sp -= sys.PtrSize * (uintptr.Value)(@unsafe.Pointer(sp));

                pc;
                c.set_esp(uint32(sp));
            }
            c.set_eip(uint32(funcPC(sigpanic)));
        }
    }
}
