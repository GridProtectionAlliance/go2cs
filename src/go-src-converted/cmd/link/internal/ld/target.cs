// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 13 06:35:28 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\target.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using binary = encoding.binary_package;


// Target holds the configuration we're building for.

public static partial class ld_package {

public partial struct Target {
    public ptr<sys.Arch> Arch;
    public objabi.HeadType HeadType;
    public LinkMode LinkMode;
    public BuildMode BuildMode;
    public bool linkShared;
    public bool canUsePlugins;
    public bool IsELF;
}

//
// Target type functions
//

private static bool IsExe(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.BuildMode == BuildModeExe;
}

private static bool IsShared(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.BuildMode == BuildModeShared;
}

private static bool IsPlugin(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.BuildMode == BuildModePlugin;
}

private static bool IsInternal(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.LinkMode == LinkInternal;
}

private static bool IsExternal(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.LinkMode == LinkExternal;
}

private static bool IsPIE(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.BuildMode == BuildModePIE;
}

private static bool IsSharedGoLink(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.linkShared;
}

private static bool CanUsePlugins(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.canUsePlugins;
}

private static bool IsElf(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.IsELF;
}

private static bool IsDynlinkingGo(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.IsShared() || t.IsSharedGoLink() || t.IsPlugin() || t.CanUsePlugins();
}

// UseRelro reports whether to make use of "read only relocations" aka
// relro.
private static bool UseRelro(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;


    if (t.BuildMode == BuildModeCArchive || t.BuildMode == BuildModeCShared || t.BuildMode == BuildModeShared || t.BuildMode == BuildModePIE || t.BuildMode == BuildModePlugin) 
        return t.IsELF || t.HeadType == objabi.Haix || t.HeadType == objabi.Hdarwin;
    else 
        if (t.HeadType == objabi.Hdarwin && t.IsARM64()) { 
            // On darwin/ARM64, everything is PIE.
            return true;
        }
        return t.linkShared || (t.HeadType == objabi.Haix && t.LinkMode == LinkExternal);
    }

//
// Processor functions
//

private static bool Is386(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.I386;
}

private static bool IsARM(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.ARM;
}

private static bool IsARM64(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.ARM64;
}

private static bool IsAMD64(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.AMD64;
}

private static bool IsMIPS(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.MIPS;
}

private static bool IsMIPS64(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.MIPS64;
}

private static bool IsPPC64(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.PPC64;
}

private static bool IsRISCV64(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.RISCV64;
}

private static bool IsS390X(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.S390X;
}

private static bool IsWasm(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.Family == sys.Wasm;
}

//
// OS Functions
//

private static bool IsLinux(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hlinux;
}

private static bool IsDarwin(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hdarwin;
}

private static bool IsWindows(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hwindows;
}

private static bool IsPlan9(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hplan9;
}

private static bool IsAIX(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Haix;
}

private static bool IsSolaris(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hsolaris;
}

private static bool IsNetbsd(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hnetbsd;
}

private static bool IsOpenbsd(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    t.mustSetHeadType();
    return t.HeadType == objabi.Hopenbsd;
}

private static void mustSetHeadType(this ptr<Target> _addr_t) => func((_, panic, _) => {
    ref Target t = ref _addr_t.val;

    if (t.HeadType == objabi.Hunknown) {
        panic("HeadType is not set");
    }
});

//
// MISC
//

private static bool IsBigEndian(this ptr<Target> _addr_t) {
    ref Target t = ref _addr_t.val;

    return t.Arch.ByteOrder == binary.BigEndian;
}

} // end ld_package
