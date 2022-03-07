// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:20:40 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\config.go
using sys = go.cmd.@internal.sys_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // A BuildMode indicates the sort of object we are building.
    //
    // Possible build modes are the same as those for the -buildmode flag
    // in cmd/go, and are documented in 'go help buildmode'.
public partial struct BuildMode { // : byte
}

public static readonly BuildMode BuildModeUnset = iota;
public static readonly var BuildModeExe = 0;
public static readonly var BuildModePIE = 1;
public static readonly var BuildModeCArchive = 2;
public static readonly var BuildModeCShared = 3;
public static readonly var BuildModeShared = 4;
public static readonly var BuildModePlugin = 5;


private static error Set(this ptr<BuildMode> _addr_mode, @string s) {
    ref BuildMode mode = ref _addr_mode.val;

    Func<error> badmode = () => {
        return error.As(fmt.Errorf("buildmode %s not supported on %s/%s", s, buildcfg.GOOS, buildcfg.GOARCH))!;
    };
    switch (s) {
        case "exe": 
            switch (buildcfg.GOOS + "/" + buildcfg.GOARCH) {
                case "darwin/arm64": // On these platforms, everything is PIE

                case "windows/arm": // On these platforms, everything is PIE

                case "windows/arm64": // On these platforms, everything is PIE
                    mode.val = BuildModePIE;
                    break;
                default: 
                    mode.val = BuildModeExe;
                    break;
            }

            break;
        case "pie": 
            switch (buildcfg.GOOS) {
                case "aix": 

                case "android": 

                case "linux": 

                case "windows": 

                case "darwin": 

                case "ios": 

                    break;
                case "freebsd": 
                    switch (buildcfg.GOARCH) {
                        case "amd64": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                default: 
                    return error.As(badmode())!;
                    break;
            }
            mode.val = BuildModePIE;

            break;
        case "c-archive": 
            switch (buildcfg.GOOS) {
                case "aix": 

                case "darwin": 

                case "ios": 

                case "linux": 

                    break;
                case "freebsd": 
                    switch (buildcfg.GOARCH) {
                        case "amd64": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                case "windows": 
                    switch (buildcfg.GOARCH) {
                        case "amd64": 

                        case "386": 

                        case "arm": 

                        case "arm64": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                default: 
                    return error.As(badmode())!;
                    break;
            }
            mode.val = BuildModeCArchive;

            break;
        case "c-shared": 
            switch (buildcfg.GOARCH) {
                case "386": 

                case "amd64": 

                case "arm": 

                case "arm64": 

                case "ppc64le": 

                case "s390x": 

                    break;
                default: 
                    return error.As(badmode())!;
                    break;
            }
            mode.val = BuildModeCShared;

            break;
        case "shared": 
            switch (buildcfg.GOOS) {
                case "linux": 
                    switch (buildcfg.GOARCH) {
                        case "386": 

                        case "amd64": 

                        case "arm": 

                        case "arm64": 

                        case "ppc64le": 

                        case "s390x": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                default: 
                    return error.As(badmode())!;
                    break;
            }
            mode.val = BuildModeShared;

            break;
        case "plugin": 
            switch (buildcfg.GOOS) {
                case "linux": 
                    switch (buildcfg.GOARCH) {
                        case "386": 

                        case "amd64": 

                        case "arm": 

                        case "arm64": 

                        case "s390x": 

                        case "ppc64le": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                case "darwin": 
                    switch (buildcfg.GOARCH) {
                        case "amd64": 

                        case "arm64": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                case "freebsd": 
                    switch (buildcfg.GOARCH) {
                        case "amd64": 

                            break;
                        default: 
                            return error.As(badmode())!;
                            break;
                    }

                    break;
                default: 
                    return error.As(badmode())!;
                    break;
            }
            mode.val = BuildModePlugin;

            break;
        default: 
            return error.As(fmt.Errorf("invalid buildmode: %q", s))!;
            break;
    }
    return error.As(null!)!;

}

private static @string String(this ptr<BuildMode> _addr_mode) {
    ref BuildMode mode = ref _addr_mode.val;


    if (mode.val == BuildModeUnset) 
        return ""; // avoid showing a default in usage message
    else if (mode.val == BuildModeExe) 
        return "exe";
    else if (mode.val == BuildModePIE) 
        return "pie";
    else if (mode.val == BuildModeCArchive) 
        return "c-archive";
    else if (mode.val == BuildModeCShared) 
        return "c-shared";
    else if (mode.val == BuildModeShared) 
        return "shared";
    else if (mode.val == BuildModePlugin) 
        return "plugin";
        return fmt.Sprintf("BuildMode(%d)", uint8(mode.val));

}

// LinkMode indicates whether an external linker is used for the final link.
public partial struct LinkMode { // : byte
}

public static readonly LinkMode LinkAuto = iota;
public static readonly var LinkInternal = 0;
public static readonly var LinkExternal = 1;


private static error Set(this ptr<LinkMode> _addr_mode, @string s) {
    ref LinkMode mode = ref _addr_mode.val;

    switch (s) {
        case "auto": 
            mode.val = LinkAuto;
            break;
        case "internal": 
            mode.val = LinkInternal;
            break;
        case "external": 
            mode.val = LinkExternal;
            break;
        default: 
            return error.As(fmt.Errorf("invalid linkmode: %q", s))!;
            break;
    }
    return error.As(null!)!;

}

private static @string String(this ptr<LinkMode> _addr_mode) {
    ref LinkMode mode = ref _addr_mode.val;


    if (mode.val == LinkAuto) 
        return "auto";
    else if (mode.val == LinkInternal) 
        return "internal";
    else if (mode.val == LinkExternal) 
        return "external";
        return fmt.Sprintf("LinkMode(%d)", uint8(mode.val));

}

// mustLinkExternal reports whether the program being linked requires
// the external linker be used to complete the link.
private static (bool, @string) mustLinkExternal(ptr<Link> _addr_ctxt) => func((defer, _, _) => {
    bool res = default;
    @string reason = default;
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.Debugvlog > 1) {
        defer(() => {
            if (res) {
                ctxt.Logf("external linking is forced by: %s\n", reason);
            }
        }());
    }
    if (sys.MustLinkExternal(buildcfg.GOOS, buildcfg.GOARCH)) {
        return (true, fmt.Sprintf("%s/%s requires external linking", buildcfg.GOOS, buildcfg.GOARCH));
    }
    if (flagMsan.val) {
        return (true, "msan");
    }
    if (iscgo && ctxt.Arch.InFamily(sys.MIPS64, sys.MIPS, sys.PPC64, sys.RISCV64)) {
        return (true, buildcfg.GOARCH + " does not support internal cgo");
    }
    if (iscgo && (buildcfg.GOOS == "android" || buildcfg.GOOS == "dragonfly")) { 
        // It seems that on Dragonfly thread local storage is
        // set up by the dynamic linker, so internal cgo linking
        // doesn't work. Test case is "go test runtime/cgo".
        return (true, buildcfg.GOOS + " does not support internal cgo");

    }
    if (iscgo && buildcfg.GOOS == "windows" && buildcfg.GOARCH == "arm64") { 
        // windows/arm64 internal linking is not implemented.
        return (true, buildcfg.GOOS + "/" + buildcfg.GOARCH + " does not support internal cgo");

    }
    if (flagRace && ctxt.Arch.InFamily(sys.PPC64).val) {
        return (true, "race on " + buildcfg.GOARCH);
    }

    if (ctxt.BuildMode == BuildModeCArchive) 
        return (true, "buildmode=c-archive");
    else if (ctxt.BuildMode == BuildModeCShared) 
        return (true, "buildmode=c-shared");
    else if (ctxt.BuildMode == BuildModePIE) 
        switch (buildcfg.GOOS + "/" + buildcfg.GOARCH) {
            case "linux/amd64": 

            case "linux/arm64": 

            case "android/arm64": 

                break;
            case "windows/386": 

            case "windows/amd64": 

            case "windows/arm": 

            case "windows/arm64": 

                break;
            case "darwin/amd64": 

            case "darwin/arm64": 

                break;
            default: 
                // Internal linking does not support TLS_IE.
                return (true, "buildmode=pie");
                break;
        }
    else if (ctxt.BuildMode == BuildModePlugin) 
        return (true, "buildmode=plugin");
    else if (ctxt.BuildMode == BuildModeShared) 
        return (true, "buildmode=shared");
        if (ctxt.linkShared) {
        return (true, "dynamically linking with a shared library");
    }
    if (unknownObjFormat) {
        return (true, "some input objects have an unrecognized file format");
    }
    return (false, "");

});

// determineLinkMode sets ctxt.LinkMode.
//
// It is called after flags are processed and inputs are processed,
// so the ctxt.LinkMode variable has an initial value from the -linkmode
// flag and the iscgo, externalobj, and unknownObjFormat variables are set.
private static void determineLinkMode(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var (extNeeded, extReason) = mustLinkExternal(_addr_ctxt);
    @string via = "";

    if (ctxt.LinkMode == LinkAuto) { 
        // The environment variable GO_EXTLINK_ENABLED controls the
        // default value of -linkmode. If it is not set when the
        // linker is called we take the value it was set to when
        // cmd/link was compiled. (See make.bash.)
        switch (buildcfg.Getgoextlinkenabled()) {
            case "0": 
                ctxt.LinkMode = LinkInternal;
                via = "via GO_EXTLINK_ENABLED ";
                break;
            case "1": 
                ctxt.LinkMode = LinkExternal;
                via = "via GO_EXTLINK_ENABLED ";
                break;
            default: 
                           if (extNeeded || (iscgo && externalobj)) {
                               ctxt.LinkMode = LinkExternal;
                           }
                           else
                {
                               ctxt.LinkMode = LinkInternal;
                           }

                break;
        }

    }

    if (ctxt.LinkMode == LinkInternal) 
        if (extNeeded) {
            Exitf("internal linking requested %sbut external linking required: %s", via, extReason);
        }
    else if (ctxt.LinkMode == LinkExternal) 

        if (buildcfg.GOARCH == "ppc64" && buildcfg.GOOS != "aix") 
            Exitf("external linking not supported for %s/ppc64", buildcfg.GOOS);
            
}

} // end ld_package
