// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:47:32 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\main.go
using amd64 = go.cmd.compile.@internal.amd64_package;
using arm = go.cmd.compile.@internal.arm_package;
using arm64 = go.cmd.compile.@internal.arm64_package;
using @base = go.cmd.compile.@internal.@base_package;
using gc = go.cmd.compile.@internal.gc_package;
using mips = go.cmd.compile.@internal.mips_package;
using mips64 = go.cmd.compile.@internal.mips64_package;
using ppc64 = go.cmd.compile.@internal.ppc64_package;
using riscv64 = go.cmd.compile.@internal.riscv64_package;
using s390x = go.cmd.compile.@internal.s390x_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using wasm = go.cmd.compile.@internal.wasm_package;
using x86 = go.cmd.compile.@internal.x86_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using log = go.log_package;
using os = go.os_package;
using System;


namespace go;

public static partial class main_package {

private static map archInits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Action<ptr<ssagen.ArchInfo>>>{"386":x86.Init,"amd64":amd64.Init,"arm":arm.Init,"arm64":arm64.Init,"mips":mips.Init,"mipsle":mips.Init,"mips64":mips64.Init,"mips64le":mips64.Init,"ppc64":ppc64.Init,"ppc64le":ppc64.Init,"riscv64":riscv64.Init,"s390x":s390x.Init,"wasm":wasm.Init,};

private static void Main() { 
    // disable timestamps for reproducible output
    log.SetFlags(0);
    log.SetPrefix("compile: ");

    buildcfg.Check();
    var (archInit, ok) = archInits[buildcfg.GOARCH];
    if (!ok) {
        fmt.Fprintf(os.Stderr, "compile: unknown architecture %q\n", buildcfg.GOARCH);
        os.Exit(2);
    }
    gc.Main(archInit);
    @base.Exit(0);

}

} // end main_package
