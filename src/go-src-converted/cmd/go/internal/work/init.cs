// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Build initialization (after flag parsing).

// package work -- go2cs converted at 2020 August 29 10:01:37 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\init.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        public static void BuildInit()
        {
            instrumentInit();
            buildModeInit(); 

            // Make sure -pkgdir is absolute, because we run commands
            // in different directories.
            if (cfg.BuildPkgdir != "" && !filepath.IsAbs(cfg.BuildPkgdir))
            {
                var (p, err) = filepath.Abs(cfg.BuildPkgdir);
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go %s: evaluating -pkgdir: %v\n", flag.Args()[0L], err);
                    os.Exit(2L);
                }
                cfg.BuildPkgdir = p;
            }
        }

        private static void instrumentInit()
        {
            if (!cfg.BuildRace && !cfg.BuildMSan)
            {
                return;
            }
            if (cfg.BuildRace && cfg.BuildMSan)
            {
                fmt.Fprintf(os.Stderr, "go %s: may not use -race and -msan simultaneously\n", flag.Args()[0L]);
                os.Exit(2L);
            }
            if (cfg.BuildMSan && (cfg.Goos != "linux" || cfg.Goarch != "amd64"))
            {
                fmt.Fprintf(os.Stderr, "-msan is not supported on %s/%s\n", cfg.Goos, cfg.Goarch);
                os.Exit(2L);
            }
            if (cfg.Goarch != "amd64" || cfg.Goos != "linux" && cfg.Goos != "freebsd" && cfg.Goos != "darwin" && cfg.Goos != "windows")
            {
                fmt.Fprintf(os.Stderr, "go %s: -race and -msan are only supported on linux/amd64, freebsd/amd64, darwin/amd64 and windows/amd64\n", flag.Args()[0L]);
                os.Exit(2L);
            }
            @string mode = "race";
            if (cfg.BuildMSan)
            {
                mode = "msan";
            }
            @string modeFlag = "-" + mode;

            if (!cfg.BuildContext.CgoEnabled)
            {
                fmt.Fprintf(os.Stderr, "go %s: %s requires cgo; enable cgo by setting CGO_ENABLED=1\n", flag.Args()[0L], modeFlag);
                os.Exit(2L);
            }
            forcedGcflags = append(forcedGcflags, modeFlag);
            forcedLdflags = append(forcedLdflags, modeFlag);

            if (cfg.BuildContext.InstallSuffix != "")
            {
                cfg.BuildContext.InstallSuffix += "_";
            }
            cfg.BuildContext.InstallSuffix += mode;
            cfg.BuildContext.BuildTags = append(cfg.BuildContext.BuildTags, mode);
        }

        private static void buildModeInit()
        {
            var gccgo = cfg.BuildToolchainName == "gccgo";
            @string codegenArg = default;
            var platform = cfg.Goos + "/" + cfg.Goarch;
            switch (cfg.BuildBuildmode)
            {
                case "archive": 
                    pkgsFilter = pkgsNotMain;
                    break;
                case "c-archive": 
                    pkgsFilter = oneMainPkg;
                    switch (platform)
                    {
                        case "darwin/arm": 

                        case "darwin/arm64": 
                            codegenArg = "-shared";
                            break;
                        default: 
                            switch (cfg.Goos)
                            {
                                case "dragonfly": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.

                                case "freebsd": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.

                                case "linux": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.

                                case "netbsd": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.

                                case "openbsd": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.

                                case "solaris": 
                                    // Use -shared so that the result is
                                    // suitable for inclusion in a PIE or
                                    // shared library.
                                    codegenArg = "-shared";
                                    break;
                            }
                            break;
                    }
                    cfg.ExeSuffix = ".a";
                    ldBuildmode = "c-archive";
                    break;
                case "c-shared": 
                    pkgsFilter = oneMainPkg;
                    if (gccgo)
                    {
                        codegenArg = "-fPIC";
                    }
                    else
                    {
                        switch (platform)
                        {
                            case "linux/amd64": 

                            case "linux/arm": 

                            case "linux/arm64": 

                            case "linux/386": 

                            case "linux/ppc64le": 

                            case "linux/s390x": 

                            case "android/amd64": 

                            case "android/arm": 

                            case "android/arm64": 

                            case "android/386": 
                                codegenArg = "-shared";
                                break;
                            case "darwin/amd64": 

                            case "darwin/386": 
                                break;
                            case "windows/amd64": 
                                // Do not add usual .exe suffix to the .dll file.

                            case "windows/386": 
                                // Do not add usual .exe suffix to the .dll file.
                                cfg.ExeSuffix = "";
                                break;
                            default: 
                                @base.Fatalf("-buildmode=c-shared not supported on %s\n", platform);
                                break;
                        }
                    }
                    ldBuildmode = "c-shared";
                    break;
                case "default": 

                    if (platform == "android/arm" || platform == "android/arm64" || platform == "android/amd64" || platform == "android/386")
                    {
                        codegenArg = "-shared";
                        ldBuildmode = "pie";
                        goto __switch_break0;
                    }
                    if (platform == "darwin/arm" || platform == "darwin/arm64")
                    {
                        codegenArg = "-shared";
                    }
                    // default: 
                        ldBuildmode = "exe";

                    __switch_break0:;
                    break;
                case "exe": 
                    pkgsFilter = pkgsMain;
                    ldBuildmode = "exe"; 
                    // Set the pkgsFilter to oneMainPkg if the user passed a specific binary output
                    // and is using buildmode=exe for a better error message.
                    // See issue #20017.
                    if (cfg.BuildO != "")
                    {
                        pkgsFilter = oneMainPkg;
                    }
                    break;
                case "pie": 
                    if (cfg.BuildRace)
                    {
                        @base.Fatalf("-buildmode=pie not supported when -race is enabled");
                    }
                    if (gccgo)
                    {
                        @base.Fatalf("-buildmode=pie not supported by gccgo");
                    }
                    else
                    {
                        switch (platform)
                        {
                            case "linux/386": 

                            case "linux/amd64": 

                            case "linux/arm": 

                            case "linux/arm64": 

                            case "linux/ppc64le": 

                            case "linux/s390x": 

                            case "android/amd64": 

                            case "android/arm": 

                            case "android/arm64": 

                            case "android/386": 
                                codegenArg = "-shared";
                                break;
                            case "darwin/amd64": 
                                codegenArg = "-shared";
                                break;
                            default: 
                                @base.Fatalf("-buildmode=pie not supported on %s\n", platform);
                                break;
                        }
                    }
                    ldBuildmode = "pie";
                    break;
                case "shared": 
                    pkgsFilter = pkgsNotMain;
                    if (gccgo)
                    {
                        codegenArg = "-fPIC";
                    }
                    else
                    {
                        switch (platform)
                        {
                            case "linux/386": 

                            case "linux/amd64": 

                            case "linux/arm": 

                            case "linux/arm64": 

                            case "linux/ppc64le": 

                            case "linux/s390x": 
                                break;
                            default: 
                                @base.Fatalf("-buildmode=shared not supported on %s\n", platform);
                                break;
                        }
                        codegenArg = "-dynlink";
                    }
                    if (cfg.BuildO != "")
                    {
                        @base.Fatalf("-buildmode=shared and -o not supported together");
                    }
                    ldBuildmode = "shared";
                    break;
                case "plugin": 
                    pkgsFilter = oneMainPkg;
                    if (gccgo)
                    {
                        codegenArg = "-fPIC";
                    }
                    else
                    {
                        switch (platform)
                        {
                            case "linux/amd64": 

                            case "linux/arm": 

                            case "linux/arm64": 

                            case "linux/386": 

                            case "linux/s390x": 

                            case "linux/ppc64le": 

                            case "android/amd64": 

                            case "android/arm": 

                            case "android/arm64": 

                            case "android/386": 
                                break;
                            case "darwin/amd64": 
                                // Skip DWARF generation due to #21647
                                forcedLdflags = append(forcedLdflags, "-w");
                                break;
                            default: 
                                @base.Fatalf("-buildmode=plugin not supported on %s\n", platform);
                                break;
                        }
                        codegenArg = "-dynlink";
                    }
                    cfg.ExeSuffix = ".so";
                    ldBuildmode = "plugin";
                    break;
                default: 
                    @base.Fatalf("buildmode=%s not supported", cfg.BuildBuildmode);
                    break;
            }
            if (cfg.BuildLinkshared)
            {
                if (gccgo)
                {
                    codegenArg = "-fPIC";
                }
                else
                {
                    switch (platform)
                    {
                        case "linux/386": 

                        case "linux/amd64": 

                        case "linux/arm": 

                        case "linux/arm64": 

                        case "linux/ppc64le": 

                        case "linux/s390x": 
                            forcedAsmflags = append(forcedAsmflags, "-D=GOBUILDMODE_shared=1");
                            break;
                        default: 
                            @base.Fatalf("-linkshared not supported on %s\n", platform);
                            break;
                    }
                    codegenArg = "-dynlink"; 
                    // TODO(mwhudson): remove -w when that gets fixed in linker.
                    forcedLdflags = append(forcedLdflags, "-linkshared", "-w");
                }
            }
            if (codegenArg != "")
            {
                if (gccgo)
                {
                    forcedGccgoflags = append(new slice<@string>(new @string[] { codegenArg }), forcedGccgoflags);
                }
                else
                {
                    forcedAsmflags = append(new slice<@string>(new @string[] { codegenArg }), forcedAsmflags);
                    forcedGcflags = append(new slice<@string>(new @string[] { codegenArg }), forcedGcflags);
                } 
                // Don't alter InstallSuffix when modifying default codegen args.
                if (cfg.BuildBuildmode != "default" || cfg.BuildLinkshared)
                {
                    if (cfg.BuildContext.InstallSuffix != "")
                    {
                        cfg.BuildContext.InstallSuffix += "_";
                    }
                    cfg.BuildContext.InstallSuffix += codegenArg[1L..];
                }
            }
        }
    }
}}}}
