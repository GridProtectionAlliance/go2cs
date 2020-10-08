// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// CPU affinity functions

// package unix -- go2cs converted at 2020 October 08 04:46:08 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\affinity_linux.go
using bits = go.math.bits_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static readonly var cpuSetSize = (var)_CPU_SETSIZE / _NCPUBITS;

        // CPUSet represents a CPU affinity mask.


        // CPUSet represents a CPU affinity mask.
        public partial struct CPUSet // : array<cpuMask>
        {
        }

        private static error schedAffinity(System.UIntPtr trap, long pid, ptr<CPUSet> _addr_set)
        {
            ref CPUSet set = ref _addr_set.val;

            var (_, _, e) = RawSyscall(trap, uintptr(pid), uintptr(@unsafe.Sizeof(set)), uintptr(@unsafe.Pointer(set)));
            if (e != 0L)
            {
                return error.As(errnoErr(e))!;
            }

            return error.As(null!)!;

        }

        // SchedGetaffinity gets the CPU affinity mask of the thread specified by pid.
        // If pid is 0 the calling thread is used.
        public static error SchedGetaffinity(long pid, ptr<CPUSet> _addr_set)
        {
            ref CPUSet set = ref _addr_set.val;

            return error.As(schedAffinity(SYS_SCHED_GETAFFINITY, pid, _addr_set))!;
        }

        // SchedSetaffinity sets the CPU affinity mask of the thread specified by pid.
        // If pid is 0 the calling thread is used.
        public static error SchedSetaffinity(long pid, ptr<CPUSet> _addr_set)
        {
            ref CPUSet set = ref _addr_set.val;

            return error.As(schedAffinity(SYS_SCHED_SETAFFINITY, pid, _addr_set))!;
        }

        // Zero clears the set s, so that it contains no CPUs.
        private static void Zero(this ptr<CPUSet> _addr_s)
        {
            ref CPUSet s = ref _addr_s.val;

            foreach (var (i) in s)
            {
                s[i] = 0L;
            }

        }

        private static long cpuBitsIndex(long cpu)
        {
            return cpu / _NCPUBITS;
        }

        private static cpuMask cpuBitsMask(long cpu)
        {
            return cpuMask(1L << (int)((uint(cpu) % _NCPUBITS)));
        }

        // Set adds cpu to the set s.
        private static void Set(this ptr<CPUSet> _addr_s, long cpu)
        {
            ref CPUSet s = ref _addr_s.val;

            var i = cpuBitsIndex(cpu);
            if (i < len(s))
            {
                s[i] |= cpuBitsMask(cpu);
            }

        }

        // Clear removes cpu from the set s.
        private static void Clear(this ptr<CPUSet> _addr_s, long cpu)
        {
            ref CPUSet s = ref _addr_s.val;

            var i = cpuBitsIndex(cpu);
            if (i < len(s))
            {
                s[i] &= cpuBitsMask(cpu);
            }

        }

        // IsSet reports whether cpu is in the set s.
        private static bool IsSet(this ptr<CPUSet> _addr_s, long cpu)
        {
            ref CPUSet s = ref _addr_s.val;

            var i = cpuBitsIndex(cpu);
            if (i < len(s))
            {
                return s[i] & cpuBitsMask(cpu) != 0L;
            }

            return false;

        }

        // Count returns the number of CPUs in the set s.
        private static long Count(this ptr<CPUSet> _addr_s)
        {
            ref CPUSet s = ref _addr_s.val;

            long c = 0L;
            foreach (var (_, b) in s)
            {
                c += bits.OnesCount64(uint64(b));
            }
            return c;

        }
    }
}}}}}}
