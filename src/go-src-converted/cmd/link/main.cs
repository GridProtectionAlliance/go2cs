// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:02:13 UTC
// Original source: C:\Go\src\cmd\link\main.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using amd64 = go.cmd.link.@internal.amd64_package;
using arm = go.cmd.link.@internal.arm_package;
using arm64 = go.cmd.link.@internal.arm64_package;
using ld = go.cmd.link.@internal.ld_package;
using mips = go.cmd.link.@internal.mips_package;
using mips64 = go.cmd.link.@internal.mips64_package;
using ppc64 = go.cmd.link.@internal.ppc64_package;
using s390x = go.cmd.link.@internal.s390x_package;
using x86 = go.cmd.link.@internal.x86_package;
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // The bulk of the linker implementation lives in cmd/link/internal/ld.
        // Architecture-specific code lives in cmd/link/internal/GOARCH.
        //
        // Program initialization:
        //
        // Before any argument parsing is done, the Init function of relevant
        // architecture package is called. The only job done in Init is
        // configuration of the architecture-specific variables.
        //
        // Then control flow passes to ld.Main, which parses flags, makes
        // some configuration decisions, and then gives the architecture
        // packages a second chance to modify the linker's configuration
        // via the ld.Thearch.Archinit function.
        private static void Main()
        {
            ref sys.Arch arch = default;
            ld.Arch theArch = default;

            switch (objabi.GOARCH)
            {
                case "386": 
                    arch, theArch = x86.Init();
                    break;
                case "amd64": 

                case "amd64p32": 
                    arch, theArch = amd64.Init();
                    break;
                case "arm": 
                    arch, theArch = arm.Init();
                    break;
                case "arm64": 
                    arch, theArch = arm64.Init();
                    break;
                case "mips": 

                case "mipsle": 
                    arch, theArch = mips.Init();
                    break;
                case "mips64": 

                case "mips64le": 
                    arch, theArch = mips64.Init();
                    break;
                case "ppc64": 

                case "ppc64le": 
                    arch, theArch = ppc64.Init();
                    break;
                case "s390x": 
                    arch, theArch = s390x.Init();
                    break;
                default: 
                    fmt.Fprintf(os.Stderr, "link: unknown architecture %q\n", objabi.GOARCH);
                    os.Exit(2L);
                    break;
            }
            ld.Main(arch, theArch);
        }
    }
}
