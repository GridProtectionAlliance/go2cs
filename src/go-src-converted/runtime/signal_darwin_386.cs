// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:03 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_darwin_386.go
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
        private static ref regs32 regs(this ref sigctxt c)
        {
            return ref (ucontext.Value)(c.ctxt).uc_mcontext.ss;
        }

        private static uint eax(this ref sigctxt c)
        {
            return c.regs().eax;
        }
        private static uint ebx(this ref sigctxt c)
        {
            return c.regs().ebx;
        }
        private static uint ecx(this ref sigctxt c)
        {
            return c.regs().ecx;
        }
        private static uint edx(this ref sigctxt c)
        {
            return c.regs().edx;
        }
        private static uint edi(this ref sigctxt c)
        {
            return c.regs().edi;
        }
        private static uint esi(this ref sigctxt c)
        {
            return c.regs().esi;
        }
        private static uint ebp(this ref sigctxt c)
        {
            return c.regs().ebp;
        }
        private static uint esp(this ref sigctxt c)
        {
            return c.regs().esp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint eip(this ref sigctxt c)
        {
            return c.regs().eip;
        }

        private static uint eflags(this ref sigctxt c)
        {
            return c.regs().eflags;
        }
        private static uint cs(this ref sigctxt c)
        {
            return c.regs().cs;
        }
        private static uint fs(this ref sigctxt c)
        {
            return c.regs().fs;
        }
        private static uint gs(this ref sigctxt c)
        {
            return c.regs().gs;
        }
        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return c.info.si_addr;
        }

        private static void set_eip(this ref sigctxt c, uint x)
        {
            c.regs().eip = x;

        }
        private static void set_esp(this ref sigctxt c, uint x)
        {
            c.regs().esp = x;

        }
        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            c.info.si_addr = x;

        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {

            if (sig == _SIGTRAP) 
                // OS X sets c.sigcode() == TRAP_BRKPT unconditionally for all SIGTRAPs,
                // leaving no way to distinguish a breakpoint-induced SIGTRAP
                // from an asynchronous signal SIGTRAP.
                // They all look breakpoint-induced by default.
                // Try looking at the code to see if it's a breakpoint.
                // The assumption is that we're very unlikely to get an
                // asynchronous SIGTRAP at just the moment that the
                // PC started to point at unmapped memory.
                var pc = uintptr(c.eip()); 
                // OS X will leave the pc just after the INT 3 instruction.
                // INT 3 is usually 1 byte, but there is a 2-byte form.
                ref array<byte> code = new ptr<ref array<byte>>(@unsafe.Pointer(pc - 2L));
                if (code[1L] != 0xCCUL && (code[0L] != 0xCDUL || code[1L] != 3L))
                { 
                    // SIGTRAP on something other than INT 3.
                    c.set_sigcode(_SI_USER);
                }
                    }
    }
}
