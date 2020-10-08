// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package runtime -- go2cs converted at 2020 October 08 03:22:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_386.go
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
        private static System.UIntPtr sigpc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.eip());
        }

        private static System.UIntPtr sigsp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.esp());
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

            var pc = uintptr(c.eip());
            var sp = uintptr(c.esp());

            if (shouldPushSigpanic(gp, pc, new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(sp))))
            {
                c.pushCall(funcPC(sigpanic), pc);
            }
            else
            { 
                // Not safe to push the call. Just clobber the frame.
                c.set_eip(uint32(funcPC(sigpanic)));

            }

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Make it look like we called target at resumePC.
            var sp = uintptr(c.esp());
            sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

            resumePC;
            c.set_esp(uint32(sp));
            c.set_eip(uint32(targetPC));

        }
    }
}
