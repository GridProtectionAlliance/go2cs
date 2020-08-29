// package runtime -- go2cs converted at 2020 August 29 08:16:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_nacl_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // These values are referred to in the source code
        // but really don't matter. Even so, use the standard numbers.
        private static readonly long _SIGQUIT = 3L;
        private static readonly long _SIGSEGV = 11L;
        private static readonly long _SIGPROF = 27L;

        private partial struct timespec
        {
            public long tv_sec;
            public int tv_nsec;
        }

        private partial struct excregsarm
        {
            public uint r0;
            public uint r1;
            public uint r2;
            public uint r3;
            public uint r4;
            public uint r5;
            public uint r6;
            public uint r7;
            public uint r8;
            public uint r9; // the value reported here is undefined.
            public uint r10;
            public uint r11;
            public uint r12;
            public uint sp; /* r13 */
            public uint lr; /* r14 */
            public uint pc; /* r15 */
            public uint cpsr;
        }

        private partial struct exccontext
        {
            public uint size;
            public uint portable_context_offset;
            public uint portable_context_size;
            public uint arch;
            public uint regs_size;
            public array<uint> reserved;
            public excregsarm regs;
        }

        private partial struct excportablecontext
        {
            public uint pc;
            public uint sp;
            public uint fp;
        }
    }
}
