// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cfg holds configuration shared by multiple parts
// of the go command.
// package cfg -- go2cs converted at 2020 August 29 10:00:30 UTC
// import "cmd/go/internal/cfg" ==> using cfg = go.cmd.go.@internal.cfg_package
// Original source: C:\Go\src\cmd\go\internal\cfg\cfg.go
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;

using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class cfg_package
    {
        // These are general "build flags" used by build and other commands.
        public static bool BuildA = default;        public static @string BuildBuildmode = default;        public static var BuildContext = build.Default;        public static bool BuildI = default;        public static bool BuildLinkshared = default;        public static bool BuildMSan = default;        public static bool BuildN = default;        public static @string BuildO = default;        public static var BuildP = runtime.NumCPU();        public static @string BuildPkgdir = default;        public static bool BuildRace = default;        public static slice<@string> BuildToolexec = default;        public static @string BuildToolchainName = default;        public static Func<@string> BuildToolchainCompiler = default;        public static Func<@string> BuildToolchainLinker = default;        public static bool BuildV = default;        public static bool BuildWork = default;        public static bool BuildX = default;        public static @string CmdName = default;        public static @string DebugActiongraph = default;

        private static void init()
        {
            BuildToolchainCompiler = () => "missing-compiler";
            BuildToolchainLinker = () => "missing-linker";
        }

        // An EnvVar is an environment variable Name=Value.
        public partial struct EnvVar
        {
            public @string Name;
            public @string Value;
        }

        // OrigEnv is the original environment of the program at startup.
        public static slice<@string> OrigEnv = default;

        // CmdEnv is the new environment for running go tool commands.
        // User binaries (during go test or go run) are run with OrigEnv,
        // not CmdEnv.
        public static slice<EnvVar> CmdEnv = default;

        // Global build parameters (used during package load)
        public static var Goarch = BuildContext.GOARCH;        public static var Goos = BuildContext.GOOS;        public static @string ExeSuffix = default;        public static var Gopath = filepath.SplitList(BuildContext.GOPATH);

        private static void init()
        {
            if (Goos == "windows")
            {
                ExeSuffix = ".exe";
            }
        }

        public static var GOROOT = findGOROOT();        public static var GOBIN = os.Getenv("GOBIN");        public static var GOROOTbin = filepath.Join(GOROOT, "bin");        public static var GOROOTpkg = filepath.Join(GOROOT, "pkg");        public static var GOROOTsrc = filepath.Join(GOROOT, "src");        public static var GOROOT_FINAL = findGOROOT_FINAL();        public static var GOARM = fmt.Sprint(objabi.GOARM);        public static var GO386 = objabi.GO386;        public static var GOMIPS = objabi.GOMIPS;

        // Update build context to use our computed GOROOT.
        private static void init()
        {
            BuildContext.GOROOT = GOROOT; 
            // Note that we must use runtime.GOOS and runtime.GOARCH here,
            // as the tool directory does not move based on environment variables.
            // This matches the initialization of ToolDir in go/build,
            // except for using GOROOT rather than runtime.GOROOT().
            build.ToolDir = filepath.Join(GOROOT, "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);
        }

        private static @string findGOROOT()
        {
            {
                var env = os.Getenv("GOROOT");

                if (env != "")
                {
                    return filepath.Clean(env);
                }

            }
            var def = filepath.Clean(runtime.GOROOT());
            var (exe, err) = os.Executable();
            if (err == null)
            {
                exe, err = filepath.Abs(exe);
                if (err == null)
                {
                    {
                        var dir__prev3 = dir;

                        var dir = filepath.Join(exe, "../..");

                        if (isGOROOT(dir))
                        { 
                            // If def (runtime.GOROOT()) and dir are the same
                            // directory, prefer the spelling used in def.
                            if (isSameDir(def, dir))
                            {
                                return def;
                            }
                            return dir;
                        }

                        dir = dir__prev3;

                    }
                    exe, err = filepath.EvalSymlinks(exe);
                    if (err == null)
                    {
                        {
                            var dir__prev4 = dir;

                            dir = filepath.Join(exe, "../..");

                            if (isGOROOT(dir))
                            {
                                if (isSameDir(def, dir))
                                {
                                    return def;
                                }
                                return dir;
                            }

                            dir = dir__prev4;

                        }
                    }
                }
            }
            return def;
        }

        private static @string findGOROOT_FINAL()
        {
            var def = GOROOT;
            {
                var env = os.Getenv("GOROOT_FINAL");

                if (env != "")
                {
                    def = filepath.Clean(env);
                }

            }
            return def;
        }

        // isSameDir reports whether dir1 and dir2 are the same directory.
        private static bool isSameDir(@string dir1, @string dir2)
        {
            if (dir1 == dir2)
            {
                return true;
            }
            var (info1, err1) = os.Stat(dir1);
            var (info2, err2) = os.Stat(dir2);
            return err1 == null && err2 == null && os.SameFile(info1, info2);
        }

        // isGOROOT reports whether path looks like a GOROOT.
        //
        // It does this by looking for the path/pkg/tool directory,
        // which is necessary for useful operation of the cmd/go tool,
        // and is not typically present in a GOPATH.
        private static bool isGOROOT(@string path)
        {
            var (stat, err) = os.Stat(filepath.Join(path, "pkg", "tool"));
            if (err != null)
            {
                return false;
            }
            return stat.IsDir();
        }
    }
}}}}
