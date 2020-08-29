// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:02:59 UTC
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

        public static readonly BuildMode BuildModeUnset = iota;
        public static readonly var BuildModeExe = 0;
        public static readonly var BuildModePIE = 1;
        public static readonly var BuildModeCArchive = 2;
        public static readonly var BuildModeCShared = 3;
        public static readonly var BuildModeShared = 4;
        public static readonly var BuildModePlugin = 5;

        private static error Set(this ref BuildMode mode, @string s)
        {
            Func<error> badmode = () =>
            {
                return error.As(fmt.Errorf("buildmode %s not supported on %s/%s", s, objabi.GOOS, objabi.GOARCH));
            }
;
            switch (s)
            {
                case "exe": 
                    mode.Value = BuildModeExe;
                    break;
                case "pie": 
                    switch (objabi.GOOS)
                    {
                        case "android": 

                        case "linux": 
                            break;
                        case "darwin": 
                            switch (objabi.GOARCH)
                            {
                                case "amd64": 
                                    break;
                                default: 
                                    return error.As(badmode());
                                    break;
                            }
                            break;
                        default: 
                            return error.As(badmode());
                            break;
                    }
                    mode.Value = BuildModePIE;
                    break;
                case "c-archive": 
                    switch (objabi.GOOS)
                    {
                        case "darwin": 

                        case "linux": 
                            break;
                        case "windows": 
                            switch (objabi.GOARCH)
                            {
                                case "amd64": 

                                case "386": 
                                    break;
                                default: 
                                    return error.As(badmode());
                                    break;
                            }
                            break;
                        default: 
                            return error.As(badmode());
                            break;
                    }
                    mode.Value = BuildModeCArchive;
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
                            return error.As(badmode());
                            break;
                    }
                    mode.Value = BuildModeCShared;
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
                                    return error.As(badmode());
                                    break;
                            }
                            break;
                        default: 
                            return error.As(badmode());
                            break;
                    }
                    mode.Value = BuildModeShared;
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
                                    return error.As(badmode());
                                    break;
                            }
                            break;
                        case "darwin": 
                            switch (objabi.GOARCH)
                            {
                                case "amd64": 
                                    break;
                                default: 
                                    return error.As(badmode());
                                    break;
                            }
                            break;
                        default: 
                            return error.As(badmode());
                            break;
                    }
                    mode.Value = BuildModePlugin;
                    break;
                default: 
                    return error.As(fmt.Errorf("invalid buildmode: %q", s));
                    break;
            }
            return error.As(null);
        }

        private static @string String(this ref BuildMode mode)
        {

            if (mode.Value == BuildModeUnset) 
                return ""; // avoid showing a default in usage message
            else if (mode.Value == BuildModeExe) 
                return "exe";
            else if (mode.Value == BuildModePIE) 
                return "pie";
            else if (mode.Value == BuildModeCArchive) 
                return "c-archive";
            else if (mode.Value == BuildModeCShared) 
                return "c-shared";
            else if (mode.Value == BuildModeShared) 
                return "shared";
            else if (mode.Value == BuildModePlugin) 
                return "plugin";
                        return fmt.Sprintf("BuildMode(%d)", uint8(mode.Value));
        }

        // LinkMode indicates whether an external linker is used for the final link.
        public partial struct LinkMode // : byte
        {
        }

        public static readonly LinkMode LinkAuto = iota;
        public static readonly var LinkInternal = 0;
        public static readonly var LinkExternal = 1;

        private static error Set(this ref LinkMode mode, @string s)
        {
            switch (s)
            {
                case "auto": 
                    mode.Value = LinkAuto;
                    break;
                case "internal": 
                    mode.Value = LinkInternal;
                    break;
                case "external": 
                    mode.Value = LinkExternal;
                    break;
                default: 
                    return error.As(fmt.Errorf("invalid linkmode: %q", s));
                    break;
            }
            return error.As(null);
        }

        private static @string String(this ref LinkMode mode)
        {

            if (mode.Value == LinkAuto) 
                return "auto";
            else if (mode.Value == LinkInternal) 
                return "internal";
            else if (mode.Value == LinkExternal) 
                return "external";
                        return fmt.Sprintf("LinkMode(%d)", uint8(mode.Value));
        }

        // mustLinkExternal reports whether the program being linked requires
        // the external linker be used to complete the link.
        private static (bool, @string) mustLinkExternal(ref Link _ctxt) => func(_ctxt, (ref Link ctxt, Defer defer, Panic _, Recover __) =>
        {
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
            switch (objabi.GOOS)
            {
                case "android": 
                    return (true, "android");
                    break;
                case "darwin": 
                    if (ctxt.Arch.InFamily(sys.ARM, sys.ARM64))
                    {
                        return (true, "iOS");
                    }
                    break;
            }

            if (flagMsan.Value)
            {
                return (true, "msan");
            } 

            // Internally linking cgo is incomplete on some architectures.
            // https://golang.org/issue/10373
            // https://golang.org/issue/14449
            // https://golang.org/issue/21961
            if (iscgo && ctxt.Arch.InFamily(sys.ARM64, sys.MIPS64, sys.MIPS, sys.PPC64))
            {
                return (true, objabi.GOARCH + " does not support internal cgo");
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
        private static void determineLinkMode(ref Link ctxt)
        {

            if (ctxt.LinkMode == LinkAuto) 
                // The environment variable GO_EXTLINK_ENABLED controls the
                // default value of -linkmode. If it is not set when the
                // linker is called we take the value it was set to when
                // cmd/link was compiled. (See make.bash.)
                switch (objabi.Getgoextlinkenabled())
                {
                    case "0": 
                        {
                            var needed__prev1 = needed;

                            var (needed, reason) = mustLinkExternal(ctxt);

                            if (needed)
                            {
                                Exitf("internal linking requested via GO_EXTLINK_ENABLED, but external linking required: %s", reason);
                            }

                            needed = needed__prev1;

                        }
                        ctxt.LinkMode = LinkInternal;
                        break;
                    case "1": 
                        ctxt.LinkMode = LinkExternal;
                        break;
                    default: 
                        {
                            var needed__prev1 = needed;

                            var (needed, _) = mustLinkExternal(ctxt);

                            if (needed)
                            {
                                ctxt.LinkMode = LinkExternal;
                            }
                            else if (iscgo && externalobj)
                            {
                                ctxt.LinkMode = LinkExternal;
                            }
                            else if (ctxt.BuildMode == BuildModePIE)
                            {
                                ctxt.LinkMode = LinkExternal; // https://golang.org/issue/18968
                            }
                            else
                            {
                                ctxt.LinkMode = LinkInternal;
                            }

                            needed = needed__prev1;

                        }
                        break;
                }
            else if (ctxt.LinkMode == LinkInternal) 
                {
                    var needed__prev1 = needed;

                    (needed, reason) = mustLinkExternal(ctxt);

                    if (needed)
                    {
                        Exitf("internal linking requested but external linking required: %s", reason);
                    }

                    needed = needed__prev1;

                }
                    }
    }
}}}}
