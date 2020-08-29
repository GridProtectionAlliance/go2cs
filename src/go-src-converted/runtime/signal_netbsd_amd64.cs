// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_netbsd_amd64.go
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
            return (mcontextt.Value)(@unsafe.Pointer(ref (ucontextt.Value)(c.ctxt).uc_mcontext));
        }

        private static ulong rax(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RAX];
        }
        private static ulong rbx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RBX];
        }
        private static ulong rcx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RCX];
        }
        private static ulong rdx(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RDX];
        }
        private static ulong rdi(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RDI];
        }
        private static ulong rsi(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RSI];
        }
        private static ulong rbp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RBP];
        }
        private static ulong rsp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RSP];
        }
        private static ulong r8(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R8];
        }
        private static ulong r9(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R8];
        }
        private static ulong r10(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R10];
        }
        private static ulong r11(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R11];
        }
        private static ulong r12(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R12];
        }
        private static ulong r13(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R13];
        }
        private static ulong r14(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R14];
        }
        private static ulong r15(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R15];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong rip(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RIP];
        }

        private static ulong rflags(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_RFLAGS];
        }
        private static ulong cs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_CS];
        }
        private static ulong fs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_FS];
        }
        private static ulong gs(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_GS];
        }
        private static ulong sigcode(this ref sigctxt c)
        {
            return uint64(c.info._code);
        }
        private static ulong sigaddr(this ref sigctxt c)
        {
            return @unsafe.Pointer(ref c.info._reason[0L]).Value;
        }

        private static void set_rip(this ref sigctxt c, ulong x)
        {
            c.regs().__gregs[_REG_RIP] = x;

        }
        private static void set_rsp(this ref sigctxt c, ulong x)
        {
            c.regs().__gregs[_REG_RSP] = x;

        }
        private static void set_sigcode(this ref sigctxt c, ulong x)
        {
            c.info._code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, ulong x)
        {
            (uint64.Value)(@unsafe.Pointer(ref c.info._reason[0L])).Value;

            x;
        }
    }
}
