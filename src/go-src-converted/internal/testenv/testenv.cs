// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testenv provides information about what functionality
// is available in different testing environments run by the Go team.
//
// It is an internal package because these details are specific
// to the Go team's test setup (on build.golang.org) and not
// fundamental to tests in general.
// package testenv -- go2cs converted at 2022 March 06 23:36:29 UTC
// import "internal/testenv" ==> using testenv = go.@internal.testenv_package
// Original source: C:\Program Files\Go\src\internal\testenv\testenv.go
using errors = go.errors_package;
using flag = go.flag_package;
using cfg = go.@internal.cfg_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using testing = go.testing_package;

namespace go.@internal;

public static partial class testenv_package {

    // Builder reports the name of the builder running this test
    // (for example, "linux-amd64" or "windows-386-gce").
    // If the test is not running on the build infrastructure,
    // Builder returns the empty string.
public static @string Builder() {
    return os.Getenv("GO_BUILDER_NAME");
}

// HasGoBuild reports whether the current system can build programs with ``go build''
// and then run them with os.StartProcess or exec.Command.
public static bool HasGoBuild() {
    if (os.Getenv("GO_GCFLAGS") != "") { 
        // It's too much work to require every caller of the go command
        // to pass along "-gcflags="+os.Getenv("GO_GCFLAGS").
        // For now, if $GO_GCFLAGS is set, report that we simply can't
        // run go build.
        return false;

    }
    switch (runtime.GOOS) {
        case "android": 

        case "js": 

        case "ios": 
            return false;
            break;
    }
    return true;

}

// MustHaveGoBuild checks that the current system can build programs with ``go build''
// and then run them with os.StartProcess or exec.Command.
// If not, MustHaveGoBuild calls t.Skip with an explanation.
public static void MustHaveGoBuild(testing.TB t) {
    if (os.Getenv("GO_GCFLAGS") != "") {
        t.Skipf("skipping test: 'go build' not compatible with setting $GO_GCFLAGS");
    }
    if (!HasGoBuild()) {
        t.Skipf("skipping test: 'go build' not available on %s/%s", runtime.GOOS, runtime.GOARCH);
    }
}

// HasGoRun reports whether the current system can run programs with ``go run.''
public static bool HasGoRun() { 
    // For now, having go run and having go build are the same.
    return HasGoBuild();

}

// MustHaveGoRun checks that the current system can run programs with ``go run.''
// If not, MustHaveGoRun calls t.Skip with an explanation.
public static void MustHaveGoRun(testing.TB t) {
    if (!HasGoRun()) {
        t.Skipf("skipping test: 'go run' not available on %s/%s", runtime.GOOS, runtime.GOARCH);
    }
}

// GoToolPath reports the path to the Go tool.
// It is a convenience wrapper around GoTool.
// If the tool is unavailable GoToolPath calls t.Skip.
// If the tool should be available and isn't, GoToolPath calls t.Fatal.
public static @string GoToolPath(testing.TB t) {
    MustHaveGoBuild(t);
    var (path, err) = GoTool();
    if (err != null) {
        t.Fatal(err);
    }
    foreach (var (_, envVar) in strings.Fields(cfg.KnownEnv)) {
        os.Getenv(envVar);
    }    return path;

}

// GoTool reports the path to the Go tool.
public static (@string, error) GoTool() {
    @string _p0 = default;
    error _p0 = default!;

    if (!HasGoBuild()) {
        return ("", error.As(errors.New("platform cannot run go tool"))!);
    }
    @string exeSuffix = default;
    if (runtime.GOOS == "windows") {
        exeSuffix = ".exe";
    }
    var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
    {
        var (_, err) = os.Stat(path);

        if (err == null) {
            return (path, error.As(null!)!);
        }
    }

    var (goBin, err) = exec.LookPath("go" + exeSuffix);
    if (err != null) {
        return ("", error.As(errors.New("cannot find go tool: " + err.Error()))!);
    }
    return (goBin, error.As(null!)!);

}

// HasExec reports whether the current system can start new processes
// using os.StartProcess or (more commonly) exec.Command.
public static bool HasExec() {
    switch (runtime.GOOS) {
        case "js": 

        case "ios": 
            return false;
            break;
    }
    return true;

}

// HasSrc reports whether the entire source tree is available under GOROOT.
public static bool HasSrc() {
    switch (runtime.GOOS) {
        case "ios": 
            return false;
            break;
    }
    return true;

}

// MustHaveExec checks that the current system can start new processes
// using os.StartProcess or (more commonly) exec.Command.
// If not, MustHaveExec calls t.Skip with an explanation.
public static void MustHaveExec(testing.TB t) {
    if (!HasExec()) {
        t.Skipf("skipping test: cannot exec subprocess on %s/%s", runtime.GOOS, runtime.GOARCH);
    }
}

private static sync.Map execPaths = default; // path -> error

// MustHaveExecPath checks that the current system can start the named executable
// using os.StartProcess or (more commonly) exec.Command.
// If not, MustHaveExecPath calls t.Skip with an explanation.
public static void MustHaveExecPath(testing.TB t, @string path) {
    MustHaveExec(t);

    var (err, found) = execPaths.Load(path);
    if (!found) {
        _, err = exec.LookPath(path);
        err, _ = execPaths.LoadOrStore(path, err);
    }
    if (err != null) {
        t.Skipf("skipping test: %s: %s", path, err);
    }
}

// HasExternalNetwork reports whether the current system can use
// external (non-localhost) networks.
public static bool HasExternalNetwork() {
    return !testing.Short() && runtime.GOOS != "js";
}

// MustHaveExternalNetwork checks that the current system can use
// external (non-localhost) networks.
// If not, MustHaveExternalNetwork calls t.Skip with an explanation.
public static void MustHaveExternalNetwork(testing.TB t) {
    if (runtime.GOOS == "js") {
        t.Skipf("skipping test: no external network on %s", runtime.GOOS);
    }
    if (testing.Short()) {
        t.Skipf("skipping test: no external network in -short mode");
    }
}

private static bool haveCGO = default;

// HasCGO reports whether the current system can use cgo.
public static bool HasCGO() {
    return haveCGO;
}

// MustHaveCGO calls t.Skip if cgo is not available.
public static void MustHaveCGO(testing.TB t) {
    if (!haveCGO) {
        t.Skipf("skipping test: no cgo");
    }
}

// CanInternalLink reports whether the current system can link programs with
// internal linking.
// (This is the opposite of cmd/internal/sys.MustLinkExternal. Keep them in sync.)
public static bool CanInternalLink() {
    switch (runtime.GOOS) {
        case "android": 
            if (runtime.GOARCH != "arm64") {
                return false;
            }
            break;
        case "ios": 
            if (runtime.GOARCH == "arm64") {
                return false;
            }
            break;
    }
    return true;

}

// MustInternalLink checks that the current system can link programs with internal
// linking.
// If not, MustInternalLink calls t.Skip with an explanation.
public static void MustInternalLink(testing.TB t) {
    if (!CanInternalLink()) {
        t.Skipf("skipping test: internal linking on %s/%s is not supported", runtime.GOOS, runtime.GOARCH);
    }
}

// HasSymlink reports whether the current system can use os.Symlink.
public static bool HasSymlink() {
    var (ok, _) = hasSymlink();
    return ok;
}

// MustHaveSymlink reports whether the current system can use os.Symlink.
// If not, MustHaveSymlink calls t.Skip with an explanation.
public static void MustHaveSymlink(testing.TB t) {
    var (ok, reason) = hasSymlink();
    if (!ok) {
        t.Skipf("skipping test: cannot make symlinks on %s/%s%s", runtime.GOOS, runtime.GOARCH, reason);
    }
}

// HasLink reports whether the current system can use os.Link.
public static bool HasLink() { 
    // From Android release M (Marshmallow), hard linking files is blocked
    // and an attempt to call link() on a file will return EACCES.
    // - https://code.google.com/p/android-developer-preview/issues/detail?id=3150
    return runtime.GOOS != "plan9" && runtime.GOOS != "android";

}

// MustHaveLink reports whether the current system can use os.Link.
// If not, MustHaveLink calls t.Skip with an explanation.
public static void MustHaveLink(testing.TB t) {
    if (!HasLink()) {
        t.Skipf("skipping test: hardlinks are not supported on %s/%s", runtime.GOOS, runtime.GOARCH);
    }
}

private static var flaky = flag.Bool("flaky", false, "run known-flaky tests too");

public static void SkipFlaky(testing.TB t, nint issue) {
    t.Helper();
    if (!flaky.val) {
        t.Skipf("skipping known flaky test without the -flaky flag; see golang.org/issue/%d", issue);
    }
}

public static void SkipFlakyNet(testing.TB t) {
    t.Helper();
    {
        var (v, _) = strconv.ParseBool(os.Getenv("GO_BUILDER_FLAKY_NET"));

        if (v) {
            t.Skip("skipping test on builder known to have frequent network failures");
        }
    }

}

// CleanCmdEnv will fill cmd.Env with the environment, excluding certain
// variables that could modify the behavior of the Go tools such as
// GODEBUG and GOTRACEBACK.
public static ptr<exec.Cmd> CleanCmdEnv(ptr<exec.Cmd> _addr_cmd) => func((_, panic, _) => {
    ref exec.Cmd cmd = ref _addr_cmd.val;

    if (cmd.Env != null) {
        panic("environment already set");
    }
    foreach (var (_, env) in os.Environ()) { 
        // Exclude GODEBUG from the environment to prevent its output
        // from breaking tests that are trying to parse other command output.
        if (strings.HasPrefix(env, "GODEBUG=")) {
            continue;
        }
        if (strings.HasPrefix(env, "GOTRACEBACK=")) {
            continue;
        }
        cmd.Env = append(cmd.Env, env);

    }    return _addr_cmd!;

});

// CPUIsSlow reports whether the CPU running the test is suspected to be slow.
public static bool CPUIsSlow() {
    switch (runtime.GOARCH) {
        case "arm": 

        case "mips": 

        case "mipsle": 

        case "mips64": 

        case "mips64le": 
            return true;
            break;
    }
    return false;

}

// SkipIfShortAndSlow skips t if -short is set and the CPU running the test is
// suspected to be slow.
//
// (This is useful for CPU-intensive tests that otherwise complete quickly.)
public static void SkipIfShortAndSlow(testing.TB t) {
    if (testing.Short() && CPUIsSlow()) {
        t.Helper();
        t.Skipf("skipping test in -short mode on %s", runtime.GOARCH);
    }
}

} // end testenv_package
