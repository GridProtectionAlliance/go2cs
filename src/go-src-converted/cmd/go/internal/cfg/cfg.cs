// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cfg holds configuration shared by multiple parts
// of the go command.
// package cfg -- go2cs converted at 2020 October 08 04:33:32 UTC
// import "cmd/go/internal/cfg" ==> using cfg = go.cmd.go.@internal.cfg_package
// Original source: C:\Go\src\cmd\go\internal\cfg\cfg.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using cfg = go.@internal.cfg_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using sync = go.sync_package;

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
        public static bool BuildA = default;        public static @string BuildBuildmode = default;        public static var BuildContext = defaultContext();        public static @string BuildMod = default;        public static @string BuildModReason = default;        public static bool BuildI = default;        public static bool BuildLinkshared = default;        public static bool BuildMSan = default;        public static bool BuildN = default;        public static @string BuildO = default;        public static var BuildP = runtime.NumCPU();        public static @string BuildPkgdir = default;        public static bool BuildRace = default;        public static slice<@string> BuildToolexec = default;        public static @string BuildToolchainName = default;        public static Func<@string> BuildToolchainCompiler = default;        public static Func<@string> BuildToolchainLinker = default;        public static bool BuildTrimpath = default;        public static bool BuildV = default;        public static bool BuildWork = default;        public static bool BuildX = default;        public static bool ModCacheRW = default;        public static @string ModFile = default;        public static @string CmdName = default;        public static @string DebugActiongraph = default;

        private static build.Context defaultContext()
        {
            var ctxt = build.Default;
            ctxt.JoinPath = filepath.Join; // back door to say "do not use go command"

            ctxt.GOROOT = findGOROOT();
            if (runtime.Compiler != "gccgo")
            { 
                // Note that we must use runtime.GOOS and runtime.GOARCH here,
                // as the tool directory does not move based on environment
                // variables. This matches the initialization of ToolDir in
                // go/build, except for using ctxt.GOROOT rather than
                // runtime.GOROOT.
                build.ToolDir = filepath.Join(ctxt.GOROOT, "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);

            }

            ctxt.GOPATH = envOr("GOPATH", ctxt.GOPATH); 

            // Override defaults computed in go/build with defaults
            // from go environment configuration file, if known.
            ctxt.GOOS = envOr("GOOS", ctxt.GOOS);
            ctxt.GOARCH = envOr("GOARCH", ctxt.GOARCH); 

            // The go/build rule for whether cgo is enabled is:
            //    1. If $CGO_ENABLED is set, respect it.
            //    2. Otherwise, if this is a cross-compile, disable cgo.
            //    3. Otherwise, use built-in default for GOOS/GOARCH.
            // Recreate that logic here with the new GOOS/GOARCH setting.
            {
                var v = Getenv("CGO_ENABLED");

                if (v == "0" || v == "1")
                {
                    ctxt.CgoEnabled = v[0L] == '1';
                }
                else if (ctxt.GOOS != runtime.GOOS || ctxt.GOARCH != runtime.GOARCH)
                {
                    ctxt.CgoEnabled = false;
                }
                else
                { 
                    // Use built-in default cgo setting for GOOS/GOARCH.
                    // Note that ctxt.GOOS/GOARCH are derived from the preference list
                    // (1) environment, (2) go/env file, (3) runtime constants,
                    // while go/build.Default.GOOS/GOARCH are derived from the preference list
                    // (1) environment, (2) runtime constants.
                    // We know ctxt.GOOS/GOARCH == runtime.GOOS/GOARCH;
                    // no matter how that happened, go/build.Default will make the
                    // same decision (either the environment variables are set explicitly
                    // to match the runtime constants, or else they are unset, in which
                    // case go/build falls back to the runtime constants), so
                    // go/build.Default.GOOS/GOARCH == runtime.GOOS/GOARCH.
                    // So ctxt.CgoEnabled (== go/build.Default.CgoEnabled) is correct
                    // as is and can be left unmodified.
                    // Nothing to do here.
                }


            }


            return ctxt;

        }

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
        public static var Goarch = BuildContext.GOARCH;        public static var Goos = BuildContext.GOOS;        public static var ExeSuffix = exeSuffix();        public static bool ModulesEnabled = default;

        private static @string exeSuffix()
        {
            if (Goos == "windows")
            {
                return ".exe";
            }

            return "";

        }

        private static var envCache = default;

        // EnvFile returns the name of the Go environment configuration file.
        public static (@string, error) EnvFile()
        {
            @string _p0 = default;
            error _p0 = default!;

            {
                var file = os.Getenv("GOENV");

                if (file != "")
                {
                    if (file == "off")
                    {
                        return ("", error.As(fmt.Errorf("GOENV=off"))!);
                    }

                    return (file, error.As(null!)!);

                }

            }

            var (dir, err) = os.UserConfigDir();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (dir == "")
            {
                return ("", error.As(fmt.Errorf("missing user-config dir"))!);
            }

            return (filepath.Join(dir, "go/env"), error.As(null!)!);

        }

        private static void initEnvCache()
        {
            envCache.m = make_map<@string, @string>();
            var (file, _) = EnvFile();
            if (file == "")
            {
                return ;
            }

            var (data, err) = ioutil.ReadFile(file);
            if (err != null)
            {
                return ;
            }

            while (len(data) > 0L)
            { 
                // Get next line.
                var line = data;
                var i = bytes.IndexByte(data, '\n');
                if (i >= 0L)
                {
                    line = line[..i];
                    data = data[i + 1L..];

                }
                else
                {
                    data = null;
                }

                i = bytes.IndexByte(line, '=');
                if (i < 0L || line[0L] < 'A' || 'Z' < line[0L])
                { 
                    // Line is missing = (or empty) or a comment or not a valid env name. Ignore.
                    // (This should not happen, since the file should be maintained almost
                    // exclusively by "go env -w", but better to silently ignore than to make
                    // the go command unusable just because somehow the env file has
                    // gotten corrupted.)
                    continue;

                }

                var key = line[..i];
                var val = line[i + 1L..];
                envCache.m[string(key)] = string(val);

            }


        }

        // Getenv gets the value for the configuration key.
        // It consults the operating system environment
        // and then the go/env file.
        // If Getenv is called for a key that cannot be set
        // in the go/env file (for example GODEBUG), it panics.
        // This ensures that CanGetenv is accurate, so that
        // 'go env -w' stays in sync with what Getenv can retrieve.
        public static @string Getenv(@string key) => func((_, panic, __) =>
        {
            if (!CanGetenv(key))
            {
                switch (key)
                {
                    case "CGO_TEST_ALLOW": 

                    case "CGO_TEST_DISALLOW": 

                    case "CGO_test_ALLOW": 

                    case "CGO_test_DISALLOW": 
                        break;
                    default: 
                        panic("internal error: invalid Getenv " + key);
                        break;
                }

            }

            var val = os.Getenv(key);
            if (val != "")
            {
                return val;
            }

            envCache.once.Do(initEnvCache);
            return envCache.m[key];

        });

        // CanGetenv reports whether key is a valid go/env configuration key.
        public static bool CanGetenv(@string key)
        {
            return strings.Contains(cfg.KnownEnv, "\t" + key + "\n");
        }

        public static var GOROOT = BuildContext.GOROOT;        public static var GOBIN = Getenv("GOBIN");        public static var GOROOTbin = filepath.Join(GOROOT, "bin");        public static var GOROOTpkg = filepath.Join(GOROOT, "pkg");        public static var GOROOTsrc = filepath.Join(GOROOT, "src");        public static var GOROOT_FINAL = findGOROOT_FINAL();        public static var GOMODCACHE = envOr("GOMODCACHE", gopathDir("pkg/mod"));        public static var GOARM = envOr("GOARM", fmt.Sprint(objabi.GOARM));        public static var GO386 = envOr("GO386", objabi.GO386);        public static var GOMIPS = envOr("GOMIPS", objabi.GOMIPS);        public static var GOMIPS64 = envOr("GOMIPS64", objabi.GOMIPS64);        public static var GOPPC64 = envOr("GOPPC64", fmt.Sprintf("%s%d", "power", objabi.GOPPC64));        public static var GOWASM = envOr("GOWASM", fmt.Sprint(objabi.GOWASM));        public static var GOPROXY = envOr("GOPROXY", "https://proxy.golang.org,direct");        public static var GOSUMDB = envOr("GOSUMDB", "sum.golang.org");        public static var GOPRIVATE = Getenv("GOPRIVATE");        public static var GONOPROXY = envOr("GONOPROXY", GOPRIVATE);        public static var GONOSUMDB = envOr("GONOSUMDB", GOPRIVATE);        public static var GOINSECURE = Getenv("GOINSECURE");

        public static var SumdbDir = gopathDir("pkg/sumdb");

        // GetArchEnv returns the name and setting of the
        // GOARCH-specific architecture environment variable.
        // If the current architecture has no GOARCH-specific variable,
        // GetArchEnv returns empty key and value.
        public static (@string, @string) GetArchEnv()
        {
            @string key = default;
            @string val = default;

            switch (Goarch)
            {
                case "arm": 
                    return ("GOARM", GOARM);
                    break;
                case "386": 
                    return ("GO386", GO386);
                    break;
                case "mips": 

                case "mipsle": 
                    return ("GOMIPS", GOMIPS);
                    break;
                case "mips64": 

                case "mips64le": 
                    return ("GOMIPS64", GOMIPS64);
                    break;
                case "ppc64": 

                case "ppc64le": 
                    return ("GOPPC64", GOPPC64);
                    break;
                case "wasm": 
                    return ("GOWASM", GOWASM);
                    break;
            }
            return ("", "");

        }

        // envOr returns Getenv(key) if set, or else def.
        private static @string envOr(@string key, @string def)
        {
            var val = Getenv(key);
            if (val == "")
            {
                val = def;
            }

            return val;

        }

        // There is a copy of findGOROOT, isSameDir, and isGOROOT in
        // x/tools/cmd/godoc/goroot.go.
        // Try to keep them in sync for now.

        // findGOROOT returns the GOROOT value, using either an explicitly
        // provided environment variable, a GOROOT that contains the current
        // os.Executable value, or else the GOROOT that the binary was built
        // with from runtime.GOROOT().
        //
        // There is a copy of this code in x/tools/cmd/godoc/goroot.go.
        private static @string findGOROOT()
        {
            {
                var env = Getenv("GOROOT");

                if (env != "")
                {
                    return filepath.Clean(env);
                }

            }

            var def = filepath.Clean(runtime.GOROOT());
            if (runtime.Compiler == "gccgo")
            { 
                // gccgo has no real GOROOT, and it certainly doesn't
                // depend on the executable's location.
                return def;

            }

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
            // $GOROOT_FINAL is only for use during make.bash
            // so it is not settable using go/env, so we use os.Getenv here.
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
        //
        // There is a copy of this code in x/tools/cmd/godoc/goroot.go.
        private static bool isGOROOT(@string path)
        {
            var (stat, err) = os.Stat(filepath.Join(path, "pkg", "tool"));
            if (err != null)
            {
                return false;
            }

            return stat.IsDir();

        }

        private static @string gopathDir(@string rel)
        {
            var list = filepath.SplitList(BuildContext.GOPATH);
            if (len(list) == 0L || list[0L] == "")
            {
                return "";
            }

            return filepath.Join(list[0L], rel);

        }
    }
}}}}
