// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:59:45 UTC
// Original source: C:\Go\src\cmd\dist\sys_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var modkernel32 = syscall.NewLazyDLL("kernel32.dll");        private static var procGetSystemInfo = modkernel32.NewProc("GetSystemInfo");

        // see http://msdn.microsoft.com/en-us/library/windows/desktop/ms724958(v=vs.85).aspx
        private partial struct systeminfo
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public System.UIntPtr lpMinimumApplicationAddress;
            public System.UIntPtr lpMaximumApplicationAddress;
            public System.UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        public static readonly long PROCESSOR_ARCHITECTURE_AMD64 = 9L;
        public static readonly long PROCESSOR_ARCHITECTURE_INTEL = 0L;

        private static systeminfo sysinfo = default;

        private static void sysinit()
        {
            syscall.Syscall(procGetSystemInfo.Addr(), 1L, uintptr(@unsafe.Pointer(ref sysinfo)), 0L, 0L);

            if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64) 
                gohostarch = "amd64";
            else if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL) 
                gohostarch = "386";
            else 
                fatalf("unknown processor architecture");
                    }
    }
}
