// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_netbsd_386.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private partial struct sigctxt
        {
            public ptr<siginfo> info;
            public unsafe.Pointer ctxt;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ref mcontextt regs(this ref sigctxt c)
        {
            return ref (ucontextt.Value)(c.ctxt).uc_mcontext;
        }

        private static uint eax(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EAX];
        }
        private static uint ebx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EBX];
        }
        private static uint ecx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_ECX];
        }
        private static uint edx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EDX];
        }
        private static uint edi(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EDI];
        }
        private static uint esi(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_ESI];
        }
        private static uint ebp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EBP];
        }
        private static uint esp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_UESP];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint eip(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EIP];
        }

        private static uint eflags(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_EFL];
        }
        private static uint cs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_CS];
        }
        private static uint fs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_FS];
        }
        private static uint gs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_GS];
        }
        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info._code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return @unsafe.Pointer(ref c.info._reason[0L]).Value;
        }

        private static void set_eip(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_EIP] = x;

        }
        private static void set_esp(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_UESP] = x;

        }
        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info._code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            (uint32.Value)(@unsafe.Pointer(ref c.info._reason[0L])).Value;

            x;
        }
    }
}
