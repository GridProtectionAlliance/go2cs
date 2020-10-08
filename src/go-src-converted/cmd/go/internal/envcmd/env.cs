// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package envcmd implements the ``go env'' command.
// package envcmd -- go2cs converted at 2020 October 08 04:33:36 UTC
// import "cmd/go/internal/envcmd" ==> using envcmd = go.cmd.go.@internal.envcmd_package
// Original source: C:\Go\src\cmd\go\internal\envcmd\env.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class envcmd_package
    {
        public static ptr<base.Command> CmdEnv = addr(new base.Command(UsageLine:"go env [-json] [-u] [-w] [var ...]",Short:"print Go environment information",Long:`
Env prints Go environment information.

By default env prints information as a shell script
(on Windows, a batch file). If one or more variable
names is given as arguments, env prints the value of
each named variable on its own line.

The -json flag prints the environment in JSON format
instead of as a shell script.

The -u flag requires one or more arguments and unsets
the default setting for the named environment variables,
if one has been set with 'go env -w'.

The -w flag requires one or more arguments of the
form NAME=VALUE and changes the default settings
of the named environment variables to the given values.

For more about environment variables, see 'go help environment'.
	`,));

        private static void init()
        {
            CmdEnv.Run = runEnv; // break init cycle
        }

        private static var envJson = CmdEnv.Flag.Bool("json", false, "");        private static var envU = CmdEnv.Flag.Bool("u", false, "");        private static var envW = CmdEnv.Flag.Bool("w", false, "");

        public static slice<cfg.EnvVar> MkEnv()
        {
            work.Builder b = default;
            b.Init();

            var (envFile, _) = cfg.EnvFile();
            cfg.EnvVar env = new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"GO111MODULE",Value:cfg.Getenv("GO111MODULE")}, {Name:"GOARCH",Value:cfg.Goarch}, {Name:"GOBIN",Value:cfg.GOBIN}, {Name:"GOCACHE",Value:cache.DefaultDir()}, {Name:"GOENV",Value:envFile}, {Name:"GOEXE",Value:cfg.ExeSuffix}, {Name:"GOFLAGS",Value:cfg.Getenv("GOFLAGS")}, {Name:"GOHOSTARCH",Value:runtime.GOARCH}, {Name:"GOHOSTOS",Value:runtime.GOOS}, {Name:"GOINSECURE",Value:cfg.GOINSECURE}, {Name:"GOMODCACHE",Value:cfg.GOMODCACHE}, {Name:"GONOPROXY",Value:cfg.GONOPROXY}, {Name:"GONOSUMDB",Value:cfg.GONOSUMDB}, {Name:"GOOS",Value:cfg.Goos}, {Name:"GOPATH",Value:cfg.BuildContext.GOPATH}, {Name:"GOPRIVATE",Value:cfg.GOPRIVATE}, {Name:"GOPROXY",Value:cfg.GOPROXY}, {Name:"GOROOT",Value:cfg.GOROOT}, {Name:"GOSUMDB",Value:cfg.GOSUMDB}, {Name:"GOTMPDIR",Value:cfg.Getenv("GOTMPDIR")}, {Name:"GOTOOLDIR",Value:base.ToolDir} });

            if (work.GccgoBin != "")
            {
                env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoBin));
            }
            else
            {
                env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoName));
            }

            var (key, val) = cfg.GetArchEnv();
            if (key != "")
            {
                env = append(env, new cfg.EnvVar(Name:key,Value:val));
            }

            var cc = cfg.DefaultCC(cfg.Goos, cfg.Goarch);
            {
                cfg.EnvVar env__prev1 = env;

                env = strings.Fields(cfg.Getenv("CC"));

                if (len(env) > 0L)
                {
                    cc = env[0L];
                }

                env = env__prev1;

            }

            var cxx = cfg.DefaultCXX(cfg.Goos, cfg.Goarch);
            {
                cfg.EnvVar env__prev1 = env;

                env = strings.Fields(cfg.Getenv("CXX"));

                if (len(env) > 0L)
                {
                    cxx = env[0L];
                }

                env = env__prev1;

            }

            env = append(env, new cfg.EnvVar(Name:"AR",Value:envOr("AR","ar")));
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

        private static @string envOr(@string name, @string def)
        {
            var val = cfg.Getenv(name);
            if (val != "")
            {
                return val;
            }

            return def;

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
            @string gomod = "";
            if (modload.HasModRoot())
            {
                gomod = filepath.Join(modload.ModRoot(), "go.mod");
            }
            else if (modload.Enabled())
            {
                gomod = os.DevNull;
            }

            return new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"GOMOD",Value:gomod} });

        }

        // ExtraEnvVarsCostly returns environment variables that should not leak into child processes
        // but are costly to evaluate.
        public static slice<cfg.EnvVar> ExtraEnvVarsCostly()
        {
            work.Builder b = default;
            b.Init();
            var (cppflags, cflags, cxxflags, fflags, ldflags, err) = b.CFlags(addr(new load.Package()));
            if (err != null)
            { 
                // Should not happen - b.CFlags was given an empty package.
                fmt.Fprintf(os.Stderr, "go: invalid cflags: %v\n", err);
                return null;

            }

            var cmd = b.GccCmd(".", "");

            return new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"CGO_CFLAGS",Value:strings.Join(cflags," ")}, {Name:"CGO_CPPFLAGS",Value:strings.Join(cppflags," ")}, {Name:"CGO_CXXFLAGS",Value:strings.Join(cxxflags," ")}, {Name:"CGO_FFLAGS",Value:strings.Join(fflags," ")}, {Name:"CGO_LDFLAGS",Value:strings.Join(ldflags," ")}, {Name:"PKG_CONFIG",Value:b.PkgconfigCmd()}, {Name:"GOGCCFLAGS",Value:strings.Join(cmd[3:]," ")} });

        }

        // argKey returns the KEY part of the arg KEY=VAL, or else arg itself.
        private static @string argKey(@string arg)
        {
            var i = strings.Index(arg, "=");
            if (i < 0L)
            {
                return arg;
            }

            return arg[..i];

        }

        private static void runEnv(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (envJson && envU.val)
            {
                @base.Fatalf("go env: cannot use -json with -u");
            }

            if (envJson && envW.val)
            {
                @base.Fatalf("go env: cannot use -json with -w");
            }

            if (envU && envW.val)
            {
                @base.Fatalf("go env: cannot use -u with -w");
            }

            var env = cfg.CmdEnv;
            env = append(env, ExtraEnvVars()); 

            // Do we need to call ExtraEnvVarsCostly, which is a bit expensive?
            // Only if we're listing all environment variables ("go env")
            // or the variables being requested are in the extra list.
            var needCostly = true;
            if (len(args) > 0L)
            {
                needCostly = false;
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in args)
                    {
                        arg = __arg;
                        switch (argKey(arg))
                        {
                            case "CGO_CFLAGS": 

                            case "CGO_CPPFLAGS": 

                            case "CGO_CXXFLAGS": 

                            case "CGO_FFLAGS": 

                            case "CGO_LDFLAGS": 

                            case "PKG_CONFIG": 

                            case "GOGCCFLAGS": 
                                needCostly = true;
                                break;
                        }

                    }

                    arg = arg__prev1;
                }
            }

            if (needCostly)
            {
                env = append(env, ExtraEnvVarsCostly());
            }

            if (envW.val)
            { 
                // Process and sanity-check command line.
                if (len(args) == 0L)
                {
                    @base.Fatalf("go env -w: no KEY=VALUE arguments given");
                }

                var osEnv = make_map<@string, @string>();
                {
                    var e__prev1 = e;

                    foreach (var (_, __e) in cfg.OrigEnv)
                    {
                        e = __e;
                        {
                            var i__prev2 = i;

                            var i = strings.Index(e, "=");

                            if (i >= 0L)
                            {
                                osEnv[e[..i]] = e[i + 1L..];
                            }

                            i = i__prev2;

                        }

                    }

                    e = e__prev1;
                }

                var add = make_map<@string, @string>();
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in args)
                    {
                        arg = __arg;
                        i = strings.Index(arg, "=");
                        if (i < 0L)
                        {
                            @base.Fatalf("go env -w: arguments must be KEY=VALUE: invalid argument: %s", arg);
                        }

                        var key = arg[..i];
                        var val = arg[i + 1L..];
                        {
                            var err__prev2 = err;

                            var err = checkEnvWrite(key, val);

                            if (err != null)
                            {
                                @base.Fatalf("go env -w: %v", err);
                            }

                            err = err__prev2;

                        }

                        {
                            var (_, ok) = add[key];

                            if (ok)
                            {
                                @base.Fatalf("go env -w: multiple values for key: %s", key);
                            }

                        }

                        add[key] = val;
                        {
                            var osVal = osEnv[key];

                            if (osVal != "" && osVal != val)
                            {
                                fmt.Fprintf(os.Stderr, "warning: go env -w %s=... does not override conflicting OS environment variable\n", key);
                            }

                        }

                    }

                    arg = arg__prev1;
                }

                var (goos, okGOOS) = add["GOOS"];
                var (goarch, okGOARCH) = add["GOARCH"];
                if (okGOOS || okGOARCH)
                {
                    if (!okGOOS)
                    {
                        goos = cfg.Goos;
                    }

                    if (!okGOARCH)
                    {
                        goarch = cfg.Goarch;
                    }

                    {
                        var err__prev3 = err;

                        err = work.CheckGOOSARCHPair(goos, goarch);

                        if (err != null)
                        {
                            @base.Fatalf("go env -w: %v", err);
                        }

                        err = err__prev3;

                    }

                }

                updateEnvFile(add, null);
                return ;

            }

            if (envU.val)
            { 
                // Process and sanity-check command line.
                if (len(args) == 0L)
                {
                    @base.Fatalf("go env -u: no arguments given");
                }

                var del = make_map<@string, bool>();
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in args)
                    {
                        arg = __arg;
                        {
                            var err__prev2 = err;

                            err = checkEnvWrite(arg, "");

                            if (err != null)
                            {
                                @base.Fatalf("go env -u: %v", err);
                            }

                            err = err__prev2;

                        }

                        del[arg] = true;

                    }

                    arg = arg__prev1;
                }

                if (del["GOOS"] || del["GOARCH"])
                {
                    var goos = cfg.Goos;
                    var goarch = cfg.Goarch;
                    if (del["GOOS"])
                    {
                        goos = getOrigEnv("GOOS");
                        if (goos == "")
                        {
                            goos = build.Default.GOOS;
                        }

                    }

                    if (del["GOARCH"])
                    {
                        goarch = getOrigEnv("GOARCH");
                        if (goarch == "")
                        {
                            goarch = build.Default.GOARCH;
                        }

                    }

                    {
                        var err__prev3 = err;

                        err = work.CheckGOOSARCHPair(goos, goarch);

                        if (err != null)
                        {
                            @base.Fatalf("go env -u: %v", err);
                        }

                        err = err__prev3;

                    }

                }

                updateEnvFile(null, del);
                return ;

            }

            if (len(args) > 0L)
            {
                if (envJson.val)
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

                return ;

            }

            if (envJson.val)
            {
                printEnvAsJSON(env);
                return ;
            }

            {
                var e__prev1 = e;

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
                                    fmt.Printf("%s='%s'\n", e.Name, strings.ReplaceAll(e.Value, "'", "''"));
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
                    @base.Fatalf("go env -json: %s", err);
                }

            }

        }

        private static @string getOrigEnv(@string key)
        {
            foreach (var (_, v) in cfg.OrigEnv)
            {
                if (strings.HasPrefix(v, key + "="))
                {
                    return strings.TrimPrefix(v, key + "=");
                }

            }
            return "";

        }

        private static error checkEnvWrite(@string key, @string val)
        {
            switch (key)
            {
                case "GOEXE": 

                case "GOGCCFLAGS": 

                case "GOHOSTARCH": 

                case "GOHOSTOS": 

                case "GOMOD": 

                case "GOTOOLDIR": 
                    return error.As(fmt.Errorf("%s cannot be modified", key))!;
                    break;
                case "GOENV": 
                    return error.As(fmt.Errorf("%s can only be set using the OS environment", key))!;
                    break;
            } 

            // To catch typos and the like, check that we know the variable.
            if (!cfg.CanGetenv(key))
            {
                return error.As(fmt.Errorf("unknown go command variable %s", key))!;
            } 

            // Some variables can only have one of a few valid values. If set to an
            // invalid value, the next cmd/go invocation might fail immediately,
            // even 'go env -w' itself.
            switch (key)
            {
                case "GO111MODULE": 
                    switch (val)
                    {
                        case "": 

                        case "auto": 

                        case "on": 

                        case "off": 
                            break;
                        default: 
                            return error.As(fmt.Errorf("invalid %s value %q", key, val))!;
                            break;
                    }
                    break;
                case "GOPATH": 
                    if (strings.HasPrefix(val, "~"))
                    {
                        return error.As(fmt.Errorf("GOPATH entry cannot start with shell metacharacter '~': %q", val))!;
                    }

                    if (!filepath.IsAbs(val) && val != "")
                    {
                        return error.As(fmt.Errorf("GOPATH entry is relative; must be absolute path: %q", val))!;
                    }

                    break;
            }

            if (!utf8.ValidString(val))
            {
                return error.As(fmt.Errorf("invalid UTF-8 in %s=... value", key))!;
            }

            if (strings.Contains(val, "\x00"))
            {
                return error.As(fmt.Errorf("invalid NUL in %s=... value", key))!;
            }

            if (strings.ContainsAny(val, "\v\r\n"))
            {
                return error.As(fmt.Errorf("invalid newline in %s=... value", key))!;
            }

            return error.As(null!)!;

        }

        private static void updateEnvFile(map<@string, @string> add, map<@string, bool> del)
        {
            var (file, err) = cfg.EnvFile();
            if (file == "")
            {
                @base.Fatalf("go env: cannot find go env config: %v", err);
            }

            var (data, err) = ioutil.ReadFile(file);
            if (err != null && (!os.IsNotExist(err) || len(add) == 0L))
            {
                @base.Fatalf("go env: reading go env config: %v", err);
            }

            var lines = strings.SplitAfter(string(data), "\n");
            if (lines[len(lines) - 1L] == "")
            {
                lines = lines[..len(lines) - 1L];
            }
            else
            {
                lines[len(lines) - 1L] += "\n";
            } 

            // Delete all but last copy of any duplicated variables,
            // since the last copy is the one that takes effect.
            var prev = make_map<@string, long>();
            foreach (var (l, line) in lines)
            {
                {
                    var key__prev1 = key;

                    var key = lineToKey(line);

                    if (key != "")
                    {
                        {
                            var p__prev2 = p;

                            var (p, ok) = prev[key];

                            if (ok)
                            {
                                lines[p] = "";
                            }

                            p = p__prev2;

                        }

                        prev[key] = l;

                    }

                    key = key__prev1;

                }

            } 

            // Add variables (go env -w). Update existing lines in file if present, add to end otherwise.
            {
                var key__prev1 = key;
                var val__prev1 = val;

                foreach (var (__key, __val) in add)
                {
                    key = __key;
                    val = __val;
                    {
                        var p__prev1 = p;

                        (p, ok) = prev[key];

                        if (ok)
                        {
                            lines[p] = key + "=" + val + "\n";
                            delete(add, key);
                        }

                        p = p__prev1;

                    }

                }

                key = key__prev1;
                val = val__prev1;
            }

            {
                var key__prev1 = key;
                var val__prev1 = val;

                foreach (var (__key, __val) in add)
                {
                    key = __key;
                    val = __val;
                    lines = append(lines, key + "=" + val + "\n");
                } 

                // Delete requested variables (go env -u).

                key = key__prev1;
                val = val__prev1;
            }

            {
                var key__prev1 = key;

                foreach (var (__key) in del)
                {
                    key = __key;
                    {
                        var p__prev1 = p;

                        (p, ok) = prev[key];

                        if (ok)
                        {
                            lines[p] = "";
                        }

                        p = p__prev1;

                    }

                } 

                // Sort runs of KEY=VALUE lines
                // (that is, blocks of lines where blocks are separated
                // by comments, blank lines, or invalid lines).

                key = key__prev1;
            }

            long start = 0L;
            for (long i = 0L; i <= len(lines); i++)
            {
                if (i == len(lines) || lineToKey(lines[i]) == "")
                {
                    sortKeyValues(lines[start..i]);
                    start = i + 1L;
                }

            }


            data = (slice<byte>)strings.Join(lines, "");
            err = ioutil.WriteFile(file, data, 0666L);
            if (err != null)
            { 
                // Try creating directory.
                os.MkdirAll(filepath.Dir(file), 0777L);
                err = ioutil.WriteFile(file, data, 0666L);
                if (err != null)
                {
                    @base.Fatalf("go env: writing go env config: %v", err);
                }

            }

        }

        // lineToKey returns the KEY part of the line KEY=VALUE or else an empty string.
        private static @string lineToKey(@string line)
        {
            var i = strings.Index(line, "=");
            if (i < 0L || strings.Contains(line[..i], "#"))
            {
                return "";
            }

            return line[..i];

        }

        // sortKeyValues sorts a sequence of lines by key.
        // It differs from sort.Strings in that GO386= sorts after GO=.
        private static void sortKeyValues(slice<@string> lines)
        {
            sort.Slice(lines, (i, j) =>
            {
                return lineToKey(lines[i]) < lineToKey(lines[j]);
            });

        }
    }
}}}}
