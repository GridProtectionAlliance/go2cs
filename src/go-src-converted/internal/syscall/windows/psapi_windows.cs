// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 August 29 08:22:31 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Go\src\internal\syscall\windows\psapi_windows.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class windows_package
    {
        public partial struct PROCESS_MEMORY_COUNTERS
        {
            public uint CB;
            public uint PageFaultCount;
            public System.UIntPtr PeakWorkingSetSize;
            public System.UIntPtr WorkingSetSize;
            public System.UIntPtr QuotaPeakPagedPoolUsage;
            public System.UIntPtr QuotaPagedPoolUsage;
            public System.UIntPtr QuotaPeakNonPagedPoolUsage;
            public System.UIntPtr QuotaNonPagedPoolUsage;
            public System.UIntPtr PagefileUsage;
            public System.UIntPtr PeakPagefileUsage;
        }

        //sys    GetProcessMemoryInfo(handle syscall.Handle, memCounters *PROCESS_MEMORY_COUNTERS, cb uint32) (err error) = psapi.GetProcessMemoryInfo
    }
}}}
