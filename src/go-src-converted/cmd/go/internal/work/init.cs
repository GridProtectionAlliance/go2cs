// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Build initialization (after flag parsing).

// package work -- go2cs converted at 2020 October 09 05:46:24 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\init.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
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
            load.ModInit();
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
                    @base.SetExitStatus(2L);
                    @base.Exit();
                }
                cfg.BuildPkgdir = p;

            }
            var exp = objabi.Expstring()[2L..];
            if (exp != "none")
            {
                var experiments = strings.Split(exp, ",");
                foreach (var (_, expt) in experiments)
                {
                    cfg.BuildContext.BuildTags = append(cfg.BuildContext.BuildTags, "goexperiment." + expt);
                }
            }
        }

        private static void instrumentInit()
        {
            if (!cfg.BuildRace && !cfg.BuildMSan)
            {
                return ;
            }

            if (cfg.BuildRace && cfg.BuildMSan)
            {
                fmt.Fprintf(os.Stderr, "go %s: may not use -race and -msan simultaneously\n", flag.Args()[0L]);
                @base.SetExitStatus(2L);
                @base.Exit();
            }

            if (cfg.BuildMSan && !sys.MSanSupported(cfg.Goos, cfg.Goarch))
            {
                fmt.Fprintf(os.Stderr, "-msan is not supported on %s/%s\n", cfg.Goos, cfg.Goarch);
                @base.SetExitStatus(2L);
                @base.Exit();
            }

            if (cfg.BuildRace)
            {
                if (!sys.RaceDetectorSupported(cfg.Goos, cfg.Goarch))
                {
                    fmt.Fprintf(os.Stderr, "go %s: -race is only supported on linux/amd64, linux/ppc64le, linux/arm64, freebsd/amd64, netbsd/amd64, darwin/amd64 and windows/amd64\n", flag.Args()[0L]);
                    @base.SetExitStatus(2L);
                    @base.Exit();
                }

            }

            @string mode = "race";
            if (cfg.BuildMSan)
            {
                mode = "msan"; 
                // MSAN does not support non-PIE binaries on ARM64.
                // See issue #33712 for details.
                if (cfg.Goos == "linux" && cfg.Goarch == "arm64" && cfg.BuildBuildmode == "default")
                {
                    cfg.BuildBuildmode = "pie";
                }

            }

            @string modeFlag = "-" + mode;

            if (!cfg.BuildContext.CgoEnabled)
            {
                if (runtime.GOOS != cfg.Goos || runtime.GOARCH != cfg.Goarch)
                {
                    fmt.Fprintf(os.Stderr, "go %s: %s requires cgo\n", flag.Args()[0L], modeFlag);
                }
                else
                {
                    fmt.Fprintf(os.Stderr, "go %s: %s requires cgo; enable cgo by setting CGO_ENABLED=1\n", flag.Args()[0L], modeFlag);
                }

                @base.SetExitStatus(2L);
                @base.Exit();

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

            // Configure the build mode first, then verify that it is supported.
            // That way, if the flag is completely bogus we will prefer to error out with
            // "-buildmode=%s not supported" instead of naming the specific platform.

            switch (cfg.BuildBuildmode)
            {
                case "archive": 
                    pkgsFilter = pkgsNotMain;
                    break;
                case "c-archive": 
                    pkgsFilter = oneMainPkg;
                    if (gccgo)
                    {
                        codegenArg = "-fPIC";
                    }
                    else
                    {
                        switch (cfg.Goos)
                        {
                            case "darwin": 
                                switch (cfg.Goarch)
                                {
                                    case "arm64": 
                                        codegenArg = "-shared";
                                        break;
                                }
                                break;
                            case "dragonfly": 
                                // Use -shared so that the result is
                                // suitable for inclusion in a PIE or
                                // shared library.

                            case "freebsd": 
                                // Use -shared so that the result is
                                // suitable for inclusion in a PIE or
                                // shared library.

                            case "illumos": 
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
                        switch (cfg.Goos)
                        {
                            case "linux": 

                            case "android": 

                            case "freebsd": 
                                codegenArg = "-shared";
                                break;
                            case "windows": 
                                // Do not add usual .exe suffix to the .dll file.
                                cfg.ExeSuffix = "";
                                break;
                        }

                    }

                    ldBuildmode = "c-shared";
                    break;
                case "default": 

                    if (cfg.Goos == "android")
                    {
                        codegenArg = "-shared";
                        ldBuildmode = "pie";
                        goto __switch_break0;
                    }
                    if (cfg.Goos == "windows")
                    {
                        ldBuildmode = "pie";
                        goto __switch_break0;
                    }
                    if (cfg.Goos == "darwin")
                    {
                        switch (cfg.Goarch)
                        {
                            case "arm64": 
                                codegenArg = "-shared";
                                break;
                        }
                    }
                    // default: 
                        ldBuildmode = "exe";

                    __switch_break0:;
                    if (gccgo)
                    {
                        codegenArg = "";
                    }

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
                        codegenArg = "-fPIE";
                    }
                    else
                    {
                        switch (cfg.Goos)
                        {
                            case "aix": 

                            case "windows": 
                                break;
                            default: 
                                codegenArg = "-shared";
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
                        codegenArg = "-dynlink";
                    }

                    cfg.ExeSuffix = ".so";
                    ldBuildmode = "plugin";
                    break;
                default: 
                    @base.Fatalf("buildmode=%s not supported", cfg.BuildBuildmode);
                    break;
            }

            if (!sys.BuildModeSupported(cfg.BuildToolchainName, cfg.BuildBuildmode, cfg.Goos, cfg.Goarch))
            {
                @base.Fatalf("-buildmode=%s not supported on %s/%s\n", cfg.BuildBuildmode, cfg.Goos, cfg.Goarch);
            }

            if (cfg.BuildLinkshared)
            {
                if (!sys.BuildModeSupported(cfg.BuildToolchainName, "shared", cfg.Goos, cfg.Goarch))
                {
                    @base.Fatalf("-linkshared not supported on %s/%s\n", cfg.Goos, cfg.Goarch);
                }

                if (gccgo)
                {
                    codegenArg = "-fPIC";
                }
                else
                {
                    forcedAsmflags = append(forcedAsmflags, "-D=GOBUILDMODE_shared=1");
                    codegenArg = "-dynlink";
                    forcedGcflags = append(forcedGcflags, "-linkshared"); 
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

            switch (cfg.BuildMod)
            {
                case "": 
                    break;
                case "readonly": 

                case "vendor": 

                case "mod": 
                    if (!cfg.ModulesEnabled && !inGOFLAGS("-mod"))
                    {
                        @base.Fatalf("build flag -mod=%s only valid when using modules", cfg.BuildMod);
                    }

                    break;
                default: 
                    @base.Fatalf("-mod=%s not supported (can be '', 'mod', 'readonly', or 'vendor')", cfg.BuildMod);
                    break;
            }
            if (!cfg.ModulesEnabled)
            {
                if (cfg.ModCacheRW && !inGOFLAGS("-modcacherw"))
                {
                    @base.Fatalf("build flag -modcacherw only valid when using modules");
                }

                if (cfg.ModFile != "" && !inGOFLAGS("-mod"))
                {
                    @base.Fatalf("build flag -modfile only valid when using modules");
                }

            }

        }

        private static bool inGOFLAGS(@string flag)
        {
            foreach (var (_, goflag) in @base.GOFLAGS())
            {
                var name = goflag;
                if (strings.HasPrefix(name, "--"))
                {
                    name = name[1L..];
                }

                {
                    var i = strings.Index(name, "=");

                    if (i >= 0L)
                    {
                        name = name[..i];
                    }

                }

                if (name == flag)
                {
                    return true;
                }

            }
            return false;

        }
    }
}}}}
