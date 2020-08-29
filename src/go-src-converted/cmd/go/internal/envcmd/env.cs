// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package envcmd implements the ``go env'' command.
// package envcmd -- go2cs converted at 2020 August 29 10:00:34 UTC
// import "cmd/go/internal/envcmd" ==> using envcmd = go.cmd.go.@internal.envcmd_package
// Original source: C:\Go\src\cmd\go\internal\envcmd\env.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class envcmd_package
    {
        public static base.Command CmdEnv = ref new base.Command(UsageLine:"env [-json] [var ...]",Short:"print Go environment information",Long:`
Env prints Go environment information.

By default env prints information as a shell script
(on Windows, a batch file). If one or more variable
names is given as arguments, env prints the value of
each named variable on its own line.

The -json flag prints the environment in JSON format
instead of as a shell script.

For more about environment variables, see 'go help environment'.
	`,);

        private static void init()
        {
            CmdEnv.Run = runEnv; // break init cycle
        }

        private static var envJson = CmdEnv.Flag.Bool("json", false, "");

        public static slice<cfg.EnvVar> MkEnv()
        {
            work.Builder b = default;
            b.Init();

            cfg.EnvVar env = new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"GOARCH",Value:cfg.Goarch}, {Name:"GOBIN",Value:cfg.GOBIN}, {Name:"GOCACHE",Value:cache.DefaultDir()}, {Name:"GOEXE",Value:cfg.ExeSuffix}, {Name:"GOHOSTARCH",Value:runtime.GOARCH}, {Name:"GOHOSTOS",Value:runtime.GOOS}, {Name:"GOOS",Value:cfg.Goos}, {Name:"GOPATH",Value:cfg.BuildContext.GOPATH}, {Name:"GORACE",Value:os.Getenv("GORACE")}, {Name:"GOROOT",Value:cfg.GOROOT}, {Name:"GOTMPDIR",Value:os.Getenv("GOTMPDIR")}, {Name:"GOTOOLDIR",Value:base.ToolDir}, {Name:"TERM",Value:"dumb"} });

            if (work.GccgoBin != "")
            {
                env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoBin));
            }
            else
            {
                env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoName));
            }
            switch (cfg.Goarch)
            {
                case "arm": 
                    env = append(env, new cfg.EnvVar(Name:"GOARM",Value:cfg.GOARM));
                    break;
                case "386": 
                    env = append(env, new cfg.EnvVar(Name:"GO386",Value:cfg.GO386));
                    break;
                case "mips": 

                case "mipsle": 
                    env = append(env, new cfg.EnvVar(Name:"GOMIPS",Value:cfg.GOMIPS));
                    break;
            }

            var cc = cfg.DefaultCC(cfg.Goos, cfg.Goarch);
            {
                cfg.EnvVar env__prev1 = env;

                env = strings.Fields(os.Getenv("CC"));

                if (len(env) > 0L)
                {
                    cc = env[0L];
                }

                env = env__prev1;

            }
            var cxx = cfg.DefaultCXX(cfg.Goos, cfg.Goarch);
            {
                cfg.EnvVar env__prev1 = env;

                env = strings.Fields(os.Getenv("CXX"));

                if (len(env) > 0L)
                {
                    cxx = env[0L];
                }

                env = env__prev1;

            }
            env = append(env, new cfg.EnvVar(Name:"CC",Value:cc));
            env = append(env, new cfg.EnvVar(Name:"CXX",Value:cxx));

            if (cfg.BuildContext.CgoEnabled)
            {
                env = append(env, new cfg.EnvVar(Name:"CGO_ENABLED",Value:"1"));
            }
            else
            {
                env = append(env, new cfg.EnvVar(Name:"CGO_ENABLED",Value:"0"));
            }
            return env;
        }

        private static @string findEnv(slice<cfg.EnvVar> env, @string name)
        {
            foreach (var (_, e) in env)
            {
                if (e.Name == name)
                {
                    return e.Value;
                }
            }
            return "";
        }

        // ExtraEnvVars returns environment variables that should not leak into child processes.
        public static slice<cfg.EnvVar> ExtraEnvVars()
        {
            work.Builder b = default;
            b.Init();
            var (cppflags, cflags, cxxflags, fflags, ldflags, err) = b.CFlags(ref new load.Package());
            if (err != null)
            { 
                // Should not happen - b.CFlags was given an empty package.
                fmt.Fprintf(os.Stderr, "go: invalid cflags: %v\n", err);
                return null;
            }
            var cmd = b.GccCmd(".", "");
            return new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"CGO_CFLAGS",Value:strings.Join(cflags," ")}, {Name:"CGO_CPPFLAGS",Value:strings.Join(cppflags," ")}, {Name:"CGO_CXXFLAGS",Value:strings.Join(cxxflags," ")}, {Name:"CGO_FFLAGS",Value:strings.Join(fflags," ")}, {Name:"CGO_LDFLAGS",Value:strings.Join(ldflags," ")}, {Name:"PKG_CONFIG",Value:b.PkgconfigCmd()}, {Name:"GOGCCFLAGS",Value:strings.Join(cmd[3:]," ")} });
        }

        private static void runEnv(ref base.Command cmd, slice<@string> args)
        {
            var env = cfg.CmdEnv; 

            // Do we need to call ExtraEnvVars, which is a bit expensive?
            // Only if we're listing all environment variables ("go env")
            // or the variables being requested are in the extra list.
            var needExtra = true;
            if (len(args) > 0L)
            {
                needExtra = false;
                foreach (var (_, arg) in args)
                {
                    switch (arg)
                    {
                        case "CGO_CFLAGS": 

                        case "CGO_CPPFLAGS": 

                        case "CGO_CXXFLAGS": 

                        case "CGO_FFLAGS": 

                        case "CGO_LDFLAGS": 

                        case "PKG_CONFIG": 

                        case "GOGCCFLAGS": 
                            needExtra = true;
                            break;
                    }
                }
            }
            if (needExtra)
            {
                env = append(env, ExtraEnvVars());
            }
            if (len(args) > 0L)
            {
                if (envJson.Value)
                {
                    slice<cfg.EnvVar> es = default;
                    {
                        var name__prev1 = name;

                        foreach (var (_, __name) in args)
                        {
                            name = __name;
                            cfg.EnvVar e = new cfg.EnvVar(Name:name,Value:findEnv(env,name));
                            es = append(es, e);
                        }
                else

                        name = name__prev1;
                    }

                    printEnvAsJSON(es);
                }                {
                    {
                        var name__prev1 = name;

                        foreach (var (_, __name) in args)
                        {
                            name = __name;
                            fmt.Printf("%s\n", findEnv(env, name));
                        }

                        name = name__prev1;
                    }

                }
                return;
            }
            if (envJson.Value)
            {
                printEnvAsJSON(env);
                return;
            }
            {
                cfg.EnvVar e__prev1 = e;

                foreach (var (_, __e) in env)
                {
                    e = __e;
                    if (e.Name != "TERM")
                    {
                        switch (runtime.GOOS)
                        {
                            case "plan9": 
                                if (strings.IndexByte(e.Value, '\x00') < 0L)
                                {
                                    fmt.Printf("%s='%s'\n", e.Name, strings.Replace(e.Value, "'", "''", -1L));
                                }
                                else
                                {
                                    var v = strings.Split(e.Value, "\x00");
                                    fmt.Printf("%s=(", e.Name);
                                    foreach (var (x, s) in v)
                                    {
                                        if (x > 0L)
                                        {
                                            fmt.Printf(" ");
                                        }
                                        fmt.Printf("%s", s);
                                    }
                                    fmt.Printf(")\n");
                                }
                                break;
                            case "windows": 
                                fmt.Printf("set %s=%s\n", e.Name, e.Value);
                                break;
                            default: 
                                fmt.Printf("%s=\"%s\"\n", e.Name, e.Value);
                                break;
                        }
                    }
                }

                e = e__prev1;
            }

        }

        private static void printEnvAsJSON(slice<cfg.EnvVar> env)
        {
            var m = make_map<@string, @string>();
            foreach (var (_, e) in env)
            {
                if (e.Name == "TERM")
                {
                    continue;
                }
                m[e.Name] = e.Value;
            }
            var enc = json.NewEncoder(os.Stdout);
            enc.SetIndent("", "\t");
            {
                var err = enc.Encode(m);

                if (err != null)
                {
                    @base.Fatalf("%s", err);
                }

            }
        }
    }
}}}}
