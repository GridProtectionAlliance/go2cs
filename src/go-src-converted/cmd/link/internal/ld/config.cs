// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:37:59 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\config.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // A BuildMode indicates the sort of object we are building.
        //
        // Possible build modes are the same as those for the -buildmode flag
        // in cmd/go, and are documented in 'go help buildmode'.
        public partial struct BuildMode // : byte
        {
        }

        public static readonly BuildMode BuildModeUnset = (BuildMode)iota;
        public static readonly var BuildModeExe = (var)0;
        public static readonly var BuildModePIE = (var)1;
        public static readonly var BuildModeCArchive = (var)2;
        public static readonly var BuildModeCShared = (var)3;
        public static readonly var BuildModeShared = (var)4;
        public static readonly var BuildModePlugin = (var)5;


        private static error Set(this ptr<BuildMode> _addr_mode, @string s)
        {
            ref BuildMode mode = ref _addr_mode.val;

            Func<error> badmode = () =>
            {
                return error.As(fmt.Errorf("buildmode %s not supported on %s/%s", s, objabi.GOOS, objabi.GOARCH))!;
            }
;
            switch (s)
            {
                case "exe": 
                    mode.val = BuildModeExe;
                    break;
                case "pie": 
                    switch (objabi.GOOS)
                    {
                        case "aix": 

                        case "android": 

                        case "linux": 

                        case "windows": 
                            break;
                        case "darwin": 

                        case "freebsd": 
                            switch (objabi.GOARCH)
                            {
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
                    switch (objabi.GOOS)
                    {
                        case "aix": 

                        case "darwin": 

                        case "linux": 
                            break;
                        case "freebsd": 
                            switch (objabi.GOARCH)
                            {
                                case "amd64": 
                                    break;
                                default: 
                                    return error.As(badmode())!;
                                    break;
                            }
                            break;
                        case "windows": 
                            switch (objabi.GOARCH)
                            {
                                case "amd64": 

                                case "386": 

                                case "arm": 
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
                    switch (objabi.GOARCH)
                    {
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
                    switch (objabi.GOOS)
                    {
                        case "linux": 
                            switch (objabi.GOARCH)
                            {
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
                    switch (objabi.GOOS)
                    {
                        case "linux": 
                            switch (objabi.GOARCH)
                            {
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

                        case "freebsd": 
                            switch (objabi.GOARCH)
                            {
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

        private static @string String(this ptr<BuildMode> _addr_mode)
        {
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
        public partial struct LinkMode // : byte
        {
        }

        public static readonly LinkMode LinkAuto = (LinkMode)iota;
        public static readonly var LinkInternal = (var)0;
        public static readonly var LinkExternal = (var)1;


        private static error Set(this ptr<LinkMode> _addr_mode, @string s)
        {
            ref LinkMode mode = ref _addr_mode.val;

            switch (s)
            {
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

        private static @string String(this ptr<LinkMode> _addr_mode)
        {
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
        private static (bool, @string) mustLinkExternal(ptr<Link> _addr_ctxt) => func((defer, _, __) =>
        {
            bool res = default;
            @string reason = default;
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.Debugvlog > 1L)
            {
                defer(() =>
                {
                    if (res)
                    {
                        log.Printf("external linking is forced by: %s\n", reason);
                    }

                }());

            }

            if (sys.MustLinkExternal(objabi.GOOS, objabi.GOARCH))
            {
                return (true, fmt.Sprintf("%s/%s requires external linking", objabi.GOOS, objabi.GOARCH));
            }

            if (flagMsan.val)
            {
                return (true, "msan");
            } 

            // Internally linking cgo is incomplete on some architectures.
            // https://golang.org/issue/14449
            // https://golang.org/issue/21961
            if (iscgo && ctxt.Arch.InFamily(sys.MIPS64, sys.MIPS, sys.PPC64))
            {
                return (true, objabi.GOARCH + " does not support internal cgo");
            }

            if (iscgo && objabi.GOOS == "android")
            {
                return (true, objabi.GOOS + " does not support internal cgo");
            } 

            // When the race flag is set, the LLVM tsan relocatable file is linked
            // into the final binary, which means external linking is required because
            // internal linking does not support it.
            if (flagRace && ctxt.Arch.InFamily(sys.PPC64).val)
            {
                return (true, "race on " + objabi.GOARCH);
            } 

            // Some build modes require work the internal linker cannot do (yet).

            if (ctxt.BuildMode == BuildModeCArchive) 
                return (true, "buildmode=c-archive");
            else if (ctxt.BuildMode == BuildModeCShared) 
                return (true, "buildmode=c-shared");
            else if (ctxt.BuildMode == BuildModePIE) 
                switch (objabi.GOOS + "/" + objabi.GOARCH)
                {
                    case "linux/amd64": 

                    case "linux/arm64": 

                    case "android/arm64": 
                        break;
                    case "windows/386": 

                    case "windows/amd64": 

                    case "windows/arm": 
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
                        if (ctxt.linkShared)
            {
                return (true, "dynamically linking with a shared library");
            }

            return (false, "");

        });

        // determineLinkMode sets ctxt.LinkMode.
        //
        // It is called after flags are processed and inputs are processed,
        // so the ctxt.LinkMode variable has an initial value from the -linkmode
        // flag and the iscgo externalobj variables are set.
        private static void determineLinkMode(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var (extNeeded, extReason) = mustLinkExternal(_addr_ctxt);
            @string via = "";

            if (ctxt.LinkMode == LinkAuto)
            { 
                // The environment variable GO_EXTLINK_ENABLED controls the
                // default value of -linkmode. If it is not set when the
                // linker is called we take the value it was set to when
                // cmd/link was compiled. (See make.bash.)
                switch (objabi.Getgoextlinkenabled())
                {
                    case "0": 
                        ctxt.LinkMode = LinkInternal;
                        via = "via GO_EXTLINK_ENABLED ";
                        break;
                    case "1": 
                        ctxt.LinkMode = LinkExternal;
                        via = "via GO_EXTLINK_ENABLED ";
                        break;
                    default: 
                        if (extNeeded || (iscgo && externalobj))
                        {
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
                if (extNeeded)
                {
                    Exitf("internal linking requested %sbut external linking required: %s", via, extReason);
                }

            else if (ctxt.LinkMode == LinkExternal) 

                if (objabi.GOARCH == "riscv64") 
                    Exitf("external linking not supported for %s/riscv64", objabi.GOOS);
                else if (objabi.GOARCH == "ppc64" && objabi.GOOS != "aix") 
                    Exitf("external linking not supported for %s/ppc64", objabi.GOOS);
                            
        }
    }
}}}}
