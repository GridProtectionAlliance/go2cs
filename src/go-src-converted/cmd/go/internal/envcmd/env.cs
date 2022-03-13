// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package envcmd implements the ``go env'' command.

// package envcmd -- go2cs converted at 2022 March 13 06:29:30 UTC
// import "cmd/go/internal/envcmd" ==> using envcmd = go.cmd.go.@internal.envcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\envcmd\env.go
namespace go.cmd.go.@internal;

using context = context_package;
using json = encoding.json_package;
using fmt = fmt_package;
using build = go.build_package;
using buildcfg = @internal.buildcfg_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;

using @base = cmd.go.@internal.@base_package;
using cache = cmd.go.@internal.cache_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using load = cmd.go.@internal.load_package;
using modload = cmd.go.@internal.modload_package;
using work = cmd.go.@internal.work_package;
using System;

public static partial class envcmd_package {

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

private static void init() {
    CmdEnv.Run = runEnv; // break init cycle
}

private static var envJson = CmdEnv.Flag.Bool("json", false, "");private static var envU = CmdEnv.Flag.Bool("u", false, "");private static var envW = CmdEnv.Flag.Bool("w", false, "");

public static slice<cfg.EnvVar> MkEnv() {
    var (envFile, _) = cfg.EnvFile();
    cfg.EnvVar env = new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"GO111MODULE",Value:cfg.Getenv("GO111MODULE")}, {Name:"GOARCH",Value:cfg.Goarch}, {Name:"GOBIN",Value:cfg.GOBIN}, {Name:"GOCACHE",Value:cache.DefaultDir()}, {Name:"GOENV",Value:envFile}, {Name:"GOEXE",Value:cfg.ExeSuffix}, {Name:"GOEXPERIMENT",Value:buildcfg.GOEXPERIMENT()}, {Name:"GOFLAGS",Value:cfg.Getenv("GOFLAGS")}, {Name:"GOHOSTARCH",Value:runtime.GOARCH}, {Name:"GOHOSTOS",Value:runtime.GOOS}, {Name:"GOINSECURE",Value:cfg.GOINSECURE}, {Name:"GOMODCACHE",Value:cfg.GOMODCACHE}, {Name:"GONOPROXY",Value:cfg.GONOPROXY}, {Name:"GONOSUMDB",Value:cfg.GONOSUMDB}, {Name:"GOOS",Value:cfg.Goos}, {Name:"GOPATH",Value:cfg.BuildContext.GOPATH}, {Name:"GOPRIVATE",Value:cfg.GOPRIVATE}, {Name:"GOPROXY",Value:cfg.GOPROXY}, {Name:"GOROOT",Value:cfg.GOROOT}, {Name:"GOSUMDB",Value:cfg.GOSUMDB}, {Name:"GOTMPDIR",Value:cfg.Getenv("GOTMPDIR")}, {Name:"GOTOOLDIR",Value:base.ToolDir}, {Name:"GOVCS",Value:cfg.GOVCS}, {Name:"GOVERSION",Value:runtime.Version()} });

    if (work.GccgoBin != "") {
        env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoBin));
    }
    else
 {
        env = append(env, new cfg.EnvVar(Name:"GCCGO",Value:work.GccgoName));
    }
    var (key, val) = cfg.GetArchEnv();
    if (key != "") {
        env = append(env, new cfg.EnvVar(Name:key,Value:val));
    }
    var cc = cfg.DefaultCC(cfg.Goos, cfg.Goarch);
    {
        cfg.EnvVar env__prev1 = env;

        env = strings.Fields(cfg.Getenv("CC"));

        if (len(env) > 0) {
            cc = env[0];
        }
        env = env__prev1;

    }
    var cxx = cfg.DefaultCXX(cfg.Goos, cfg.Goarch);
    {
        cfg.EnvVar env__prev1 = env;

        env = strings.Fields(cfg.Getenv("CXX"));

        if (len(env) > 0) {
            cxx = env[0];
        }
        env = env__prev1;

    }
    env = append(env, new cfg.EnvVar(Name:"AR",Value:envOr("AR","ar")));
    env = append(env, new cfg.EnvVar(Name:"CC",Value:cc));
    env = append(env, new cfg.EnvVar(Name:"CXX",Value:cxx));

    if (cfg.BuildContext.CgoEnabled) {
        env = append(env, new cfg.EnvVar(Name:"CGO_ENABLED",Value:"1"));
    }
    else
 {
        env = append(env, new cfg.EnvVar(Name:"CGO_ENABLED",Value:"0"));
    }
    return env;
}

private static @string envOr(@string name, @string def) {
    var val = cfg.Getenv(name);
    if (val != "") {
        return val;
    }
    return def;
}

private static @string findEnv(slice<cfg.EnvVar> env, @string name) {
    foreach (var (_, e) in env) {
        if (e.Name == name) {
            return e.Value;
        }
    }    return "";
}

// ExtraEnvVars returns environment variables that should not leak into child processes.
public static slice<cfg.EnvVar> ExtraEnvVars() {
    @string gomod = "";
    if (modload.HasModRoot()) {
        gomod = filepath.Join(modload.ModRoot(), "go.mod");
    }
    else if (modload.Enabled()) {
        gomod = os.DevNull;
    }
    return new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"GOMOD",Value:gomod} });
}

// ExtraEnvVarsCostly returns environment variables that should not leak into child processes
// but are costly to evaluate.
public static slice<cfg.EnvVar> ExtraEnvVarsCostly() {
    work.Builder b = default;
    b.Init();
    var (cppflags, cflags, cxxflags, fflags, ldflags, err) = b.CFlags(addr(new load.Package()));
    if (err != null) { 
        // Should not happen - b.CFlags was given an empty package.
        fmt.Fprintf(os.Stderr, "go: invalid cflags: %v\n", err);
        return null;
    }
    var cmd = b.GccCmd(".", "");

    return new slice<cfg.EnvVar>(new cfg.EnvVar[] { {Name:"CGO_CFLAGS",Value:strings.Join(cflags," ")}, {Name:"CGO_CPPFLAGS",Value:strings.Join(cppflags," ")}, {Name:"CGO_CXXFLAGS",Value:strings.Join(cxxflags," ")}, {Name:"CGO_FFLAGS",Value:strings.Join(fflags," ")}, {Name:"CGO_LDFLAGS",Value:strings.Join(ldflags," ")}, {Name:"PKG_CONFIG",Value:b.PkgconfigCmd()}, {Name:"GOGCCFLAGS",Value:strings.Join(cmd[3:]," ")} });
}

// argKey returns the KEY part of the arg KEY=VAL, or else arg itself.
private static @string argKey(@string arg) {
    var i = strings.Index(arg, "=");
    if (i < 0) {
        return arg;
    }
    return arg[..(int)i];
}

private static void runEnv(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (envJson && envU.val) {
        @base.Fatalf("go env: cannot use -json with -u");
    }
    if (envJson && envW.val) {
        @base.Fatalf("go env: cannot use -json with -w");
    }
    if (envU && envW.val) {
        @base.Fatalf("go env: cannot use -u with -w");
    }
    if (envW.val) {
        runEnvW(args);
        return ;
    }
    if (envU.val) {
        runEnvU(args);
        return ;
    }
    buildcfg.Check();

    var env = cfg.CmdEnv;
    env = append(env, ExtraEnvVars());

    {
        var err = fsys.Init(@base.Cwd());

        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
    } 

    // Do we need to call ExtraEnvVarsCostly, which is a bit expensive?
    var needCostly = false;
    if (len(args) == 0) { 
        // We're listing all environment variables ("go env"),
        // including the expensive ones.
        needCostly = true;
    }
    else
 {
        needCostly = false;
checkCostly:
        foreach (var (_, arg) in args) {
            switch (argKey(arg)) {
                case "CGO_CFLAGS": 

                case "CGO_CPPFLAGS": 

                case "CGO_CXXFLAGS": 

                case "CGO_FFLAGS": 

                case "CGO_LDFLAGS": 

                case "PKG_CONFIG": 

                case "GOGCCFLAGS": 
                    needCostly = true;
                    _breakcheckCostly = true;
                    break;
                    break;
            }
        }
    }
    if (needCostly) {
        env = append(env, ExtraEnvVarsCostly());
    }
    if (len(args) > 0) {
        if (envJson.val) {
            slice<cfg.EnvVar> es = default;
            {
                var name__prev1 = name;

                foreach (var (_, __name) in args) {
                    name = __name;
                    cfg.EnvVar e = new cfg.EnvVar(Name:name,Value:findEnv(env,name));
                    es = append(es, e);
                }
        else

                name = name__prev1;
            }

            printEnvAsJSON(es);
        } {
            {
                var name__prev1 = name;

                foreach (var (_, __name) in args) {
                    name = __name;
                    fmt.Printf("%s\n", findEnv(env, name));
                }

                name = name__prev1;
            }
        }
        return ;
    }
    if (envJson.val) {
        printEnvAsJSON(env);
        return ;
    }
    PrintEnv(os.Stdout, env);
}

private static void runEnvW(slice<@string> args) { 
    // Process and sanity-check command line.
    if (len(args) == 0) {
        @base.Fatalf("go env -w: no KEY=VALUE arguments given");
    }
    var osEnv = make_map<@string, @string>();
    foreach (var (_, e) in cfg.OrigEnv) {
        {
            var i__prev1 = i;

            var i = strings.Index(e, "=");

            if (i >= 0) {
                osEnv[e[..(int)i]] = e[(int)i + 1..];
            }

            i = i__prev1;

        }
    }    var add = make_map<@string, @string>();
    foreach (var (_, arg) in args) {
        i = strings.Index(arg, "=");
        if (i < 0) {
            @base.Fatalf("go env -w: arguments must be KEY=VALUE: invalid argument: %s", arg);
        }
        var key = arg[..(int)i];
        var val = arg[(int)i + 1..];
        {
            var err__prev1 = err;

            var err = checkEnvWrite(key, val);

            if (err != null) {
                @base.Fatalf("go env -w: %v", err);
            }

            err = err__prev1;

        }
        {
            var (_, ok) = add[key];

            if (ok) {
                @base.Fatalf("go env -w: multiple values for key: %s", key);
            }

        }
        add[key] = val;
        {
            var osVal = osEnv[key];

            if (osVal != "" && osVal != val) {
                fmt.Fprintf(os.Stderr, "warning: go env -w %s=... does not override conflicting OS environment variable\n", key);
            }

        }
    }    {
        var err__prev1 = err;

        err = checkBuildConfig(add, null);

        if (err != null) {
            @base.Fatalf("go env -w: %v", err);
        }
        err = err__prev1;

    }

    var (gotmp, okGOTMP) = add["GOTMPDIR"];
    if (okGOTMP) {
        if (!filepath.IsAbs(gotmp) && gotmp != "") {
            @base.Fatalf("go env -w: GOTMPDIR must be an absolute path");
        }
    }
    updateEnvFile(add, null);
}

private static void runEnvU(slice<@string> args) { 
    // Process and sanity-check command line.
    if (len(args) == 0) {
        @base.Fatalf("go env -u: no arguments given");
    }
    var del = make_map<@string, bool>();
    foreach (var (_, arg) in args) {
        {
            var err__prev1 = err;

            var err = checkEnvWrite(arg, "");

            if (err != null) {
                @base.Fatalf("go env -u: %v", err);
            }

            err = err__prev1;

        }
        del[arg] = true;
    }    {
        var err__prev1 = err;

        err = checkBuildConfig(null, del);

        if (err != null) {
            @base.Fatalf("go env -u: %v", err);
        }
        err = err__prev1;

    }

    updateEnvFile(null, del);
}

// checkBuildConfig checks whether the build configuration is valid
// after the specified configuration environment changes are applied.
private static error checkBuildConfig(map<@string, @string> add, map<@string, bool> del) { 
    // get returns the value for key after applying add and del and
    // reports whether it changed. cur should be the current value
    // (i.e., before applying changes) and def should be the default
    // value (i.e., when no environment variables are provided at all).
    Func<@string, @string, @string, (@string, bool)> get = (key, cur, def) => {
        {
            var val__prev1 = val;

            var (val, ok) = add[key];

            if (ok) {
                return (error.As(val)!, true);
            }

            val = val__prev1;

        }
        if (del[key]) {
            var val = getOrigEnv(key);
            if (val == "") {
                val = def;
            }
            return (error.As(val)!, true);
        }
        return (error.As(cur)!, false);
    };

    var (goos, okGOOS) = get("GOOS", cfg.Goos, build.Default.GOOS);
    var (goarch, okGOARCH) = get("GOARCH", cfg.Goarch, build.Default.GOARCH);
    if (okGOOS || okGOARCH) {
        {
            var err = work.CheckGOOSARCHPair(goos, goarch);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }
    var (goexperiment, okGOEXPERIMENT) = get("GOEXPERIMENT", buildcfg.GOEXPERIMENT(), "");
    if (okGOEXPERIMENT) {
        {
            var (_, _, err) = buildcfg.ParseGOEXPERIMENT(goos, goarch, goexperiment);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }
    return error.As(null!)!;
}

// PrintEnv prints the environment variables to w.
public static void PrintEnv(io.Writer w, slice<cfg.EnvVar> env) {
    foreach (var (_, e) in env) {
        if (e.Name != "TERM") {
            switch (runtime.GOOS) {
                case "plan9": 
                                   if (strings.IndexByte(e.Value, '\x00') < 0) {
                                       fmt.Fprintf(w, "%s='%s'\n", e.Name, strings.ReplaceAll(e.Value, "'", "''"));
                                   }
                                   else
                    {
                                       var v = strings.Split(e.Value, "\x00");
                                       fmt.Fprintf(w, "%s=(", e.Name);
                                       foreach (var (x, s) in v) {
                                           if (x > 0) {
                                               fmt.Fprintf(w, " ");
                                           }
                                           fmt.Fprintf(w, "%s", s);
                                       }
                                       fmt.Fprintf(w, ")\n");
                                   }
                    break;
                case "windows": 
                    fmt.Fprintf(w, "set %s=%s\n", e.Name, e.Value);
                    break;
                default: 
                    fmt.Fprintf(w, "%s=\"%s\"\n", e.Name, e.Value);
                    break;
            }
        }
    }
}

private static void printEnvAsJSON(slice<cfg.EnvVar> env) {
    var m = make_map<@string, @string>();
    foreach (var (_, e) in env) {
        if (e.Name == "TERM") {
            continue;
        }
        m[e.Name] = e.Value;
    }    var enc = json.NewEncoder(os.Stdout);
    enc.SetIndent("", "\t");
    {
        var err = enc.Encode(m);

        if (err != null) {
            @base.Fatalf("go env -json: %s", err);
        }
    }
}

private static @string getOrigEnv(@string key) {
    foreach (var (_, v) in cfg.OrigEnv) {
        if (strings.HasPrefix(v, key + "=")) {
            return strings.TrimPrefix(v, key + "=");
        }
    }    return "";
}

private static error checkEnvWrite(@string key, @string val) {
    switch (key) {
        case "GOEXE": 

        case "GOGCCFLAGS": 

        case "GOHOSTARCH": 

        case "GOHOSTOS": 

        case "GOMOD": 

        case "GOTOOLDIR": 

        case "GOVERSION": 
            return error.As(fmt.Errorf("%s cannot be modified", key))!;
            break;
        case "GOENV": 
            return error.As(fmt.Errorf("%s can only be set using the OS environment", key))!;
            break;
    } 

    // To catch typos and the like, check that we know the variable.
    if (!cfg.CanGetenv(key)) {
        return error.As(fmt.Errorf("unknown go command variable %s", key))!;
    }
    switch (key) {
        case "GO111MODULE": 
            switch (val) {
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
            if (strings.HasPrefix(val, "~")) {
                return error.As(fmt.Errorf("GOPATH entry cannot start with shell metacharacter '~': %q", val))!;
            }
            if (!filepath.IsAbs(val) && val != "") {
                return error.As(fmt.Errorf("GOPATH entry is relative; must be absolute path: %q", val))!;
            }
            break;
        case "CC": 

        case "CXX": 

        case "GOMODCACHE": 
            if (!filepath.IsAbs(val) && val != "" && val != filepath.Base(val)) {
                return error.As(fmt.Errorf("%s entry is relative; must be absolute path: %q", key, val))!;
            }
            break;
    }

    if (!utf8.ValidString(val)) {
        return error.As(fmt.Errorf("invalid UTF-8 in %s=... value", key))!;
    }
    if (strings.Contains(val, "\x00")) {
        return error.As(fmt.Errorf("invalid NUL in %s=... value", key))!;
    }
    if (strings.ContainsAny(val, "\v\r\n")) {
        return error.As(fmt.Errorf("invalid newline in %s=... value", key))!;
    }
    return error.As(null!)!;
}

private static void updateEnvFile(map<@string, @string> add, map<@string, bool> del) {
    var (file, err) = cfg.EnvFile();
    if (file == "") {
        @base.Fatalf("go env: cannot find go env config: %v", err);
    }
    var (data, err) = os.ReadFile(file);
    if (err != null && (!os.IsNotExist(err) || len(add) == 0)) {
        @base.Fatalf("go env: reading go env config: %v", err);
    }
    var lines = strings.SplitAfter(string(data), "\n");
    if (lines[len(lines) - 1] == "") {
        lines = lines[..(int)len(lines) - 1];
    }
    else
 {
        lines[len(lines) - 1] += "\n";
    }
    var prev = make_map<@string, nint>();
    foreach (var (l, line) in lines) {
        {
            var key__prev1 = key;

            var key = lineToKey(line);

            if (key != "") {
                {
                    var p__prev2 = p;

                    var (p, ok) = prev[key];

                    if (ok) {
                        lines[p] = "";
                    }

                    p = p__prev2;

                }
                prev[key] = l;
            }

            key = key__prev1;

        }
    }    {
        var key__prev1 = key;
        var val__prev1 = val;

        foreach (var (__key, __val) in add) {
            key = __key;
            val = __val;
            {
                var p__prev1 = p;

                (p, ok) = prev[key];

                if (ok) {
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

        foreach (var (__key, __val) in add) {
            key = __key;
            val = __val;
            lines = append(lines, key + "=" + val + "\n");
        }
        key = key__prev1;
        val = val__prev1;
    }

    {
        var key__prev1 = key;

        foreach (var (__key) in del) {
            key = __key;
            {
                var p__prev1 = p;

                (p, ok) = prev[key];

                if (ok) {
                    lines[p] = "";
                }

                p = p__prev1;

            }
        }
        key = key__prev1;
    }

    nint start = 0;
    for (nint i = 0; i <= len(lines); i++) {
        if (i == len(lines) || lineToKey(lines[i]) == "") {
            sortKeyValues(lines[(int)start..(int)i]);
            start = i + 1;
        }
    }

    data = (slice<byte>)strings.Join(lines, "");
    err = os.WriteFile(file, data, 0666);
    if (err != null) { 
        // Try creating directory.
        os.MkdirAll(filepath.Dir(file), 0777);
        err = os.WriteFile(file, data, 0666);
        if (err != null) {
            @base.Fatalf("go env: writing go env config: %v", err);
        }
    }
}

// lineToKey returns the KEY part of the line KEY=VALUE or else an empty string.
private static @string lineToKey(@string line) {
    var i = strings.Index(line, "=");
    if (i < 0 || strings.Contains(line[..(int)i], "#")) {
        return "";
    }
    return line[..(int)i];
}

// sortKeyValues sorts a sequence of lines by key.
// It differs from sort.Strings in that keys which are GOx where x is an ASCII
// character smaller than = sort after GO=.
// (There are no such keys currently. It used to matter for GO386 which was
// removed in Go 1.16.)
private static void sortKeyValues(slice<@string> lines) {
    sort.Slice(lines, (i, j) => lineToKey(lines[i]) < lineToKey(lines[j]));
}

} // end envcmd_package
