// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sysinfo implements high level hardware information gathering
// that can be used for debugging or information purposes.

// package sysinfo -- go2cs converted at 2022 March 13 06:43:00 UTC
// import "internal/sysinfo" ==> using sysinfo = go.@internal.sysinfo_package
// Original source: C:\Program Files\Go\src\internal\sysinfo\sysinfo.go
namespace go.@internal;

using internalcpu = @internal.cpu_package;
using sync = sync_package;
using System;

public static partial class sysinfo_package {

private partial struct cpuInfo {
    public sync.Once once;
    public @string name;
}

public static cpuInfo CPU = default;

private static @string Name(this ptr<cpuInfo> _addr_cpu) {
    ref cpuInfo cpu = ref _addr_cpu.val;

    cpu.once.Do(() => { 
        // Try to get the information from internal/cpu.
        {
            var name = internalcpu.Name();

            if (name != "") {
                cpu.name = name;
                return ;
            } 
            // TODO(martisch): use /proc/cpuinfo and /sys/devices/system/cpu/ on Linux as fallback.

        } 
        // TODO(martisch): use /proc/cpuinfo and /sys/devices/system/cpu/ on Linux as fallback.
    });
    return cpu.name;
}

} // end sysinfo_package
