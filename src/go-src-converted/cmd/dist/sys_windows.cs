// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:15:23 UTC
// Original source: C:\Program Files\Go\src\cmd\dist\sys_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class main_package {

private static var modkernel32 = syscall.NewLazyDLL("kernel32.dll");private static var procGetSystemInfo = modkernel32.NewProc("GetSystemInfo");

// see https://msdn.microsoft.com/en-us/library/windows/desktop/ms724958(v=vs.85).aspx
private partial struct systeminfo {
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

// See https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-system_info
public static readonly nint PROCESSOR_ARCHITECTURE_AMD64 = 9;
public static readonly nint PROCESSOR_ARCHITECTURE_INTEL = 0;
public static readonly nint PROCESSOR_ARCHITECTURE_ARM = 5;
public static readonly nint PROCESSOR_ARCHITECTURE_ARM64 = 12;
public static readonly nint PROCESSOR_ARCHITECTURE_IA64 = 6;


private static systeminfo sysinfo = default;

private static void sysinit() {
    syscall.Syscall(procGetSystemInfo.Addr(), 1, uintptr(@unsafe.Pointer(_addr_sysinfo)), 0, 0);

    if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64) 
        gohostarch = "amd64";
    else if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL) 
        gohostarch = "386";
    else if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_ARM) 
        gohostarch = "arm";
    else if (sysinfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_ARM64) 
        gohostarch = "arm64";
    else 
        fatalf("unknown processor architecture");
    
}

} // end main_package
