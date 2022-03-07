// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testenv contains helper functions for skipping tests
// based on which tools are present in the environment.
// package testenv -- go2cs converted at 2022 March 06 23:31:19 UTC
// import "golang.org/x/tools/internal/testenv" ==> using testenv = go.golang.org.x.tools.@internal.testenv_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\testenv\testenv.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using sync = go.sync_package;
using System;


namespace go.golang.org.x.tools.@internal;

public static partial class testenv_package {

    // Testing is an abstraction of a *testing.T.
public partial interface Testing {
    void Skipf(@string format, params object[] args);
    void Fatalf(@string format, params object[] args);
}

private partial interface helperer {
    void Helper();
}

// packageMainIsDevel reports whether the module containing package main
// is a development version (if module information is available).
//
// Builds in GOPATH mode and builds that lack module information are assumed to
// be development versions.
private static Func<bool> packageMainIsDevel = () => true;

private static var checkGoGoroot = default;

private static error hasTool(@string tool) => func((defer, _, _) => {
    if (tool == "cgo") {
        var (enabled, err) = cgoEnabled(false);
        if (err != null) {
            return error.As(fmt.Errorf("checking cgo: %v", err))!;
        }
        if (!enabled) {
            return error.As(fmt.Errorf("cgo not enabled"))!;
        }
        return error.As(null!)!;

    }
    var (_, err) = exec.LookPath(tool);
    if (err != null) {
        return error.As(err)!;
    }
    switch (tool) {
        case "patch": 
            // check that the patch tools supports the -o argument
            var (temp, err) = ioutil.TempFile("", "patch-test");
            if (err != null) {
                return error.As(err)!;
            }
            temp.Close();
            defer(os.Remove(temp.Name()));
            var cmd = exec.Command(tool, "-o", temp.Name());
            {
                var err = cmd.Run();

                if (err != null) {
                    return error.As(err)!;
                }

            }


            break;
        case "go": 
            checkGoGoroot.once.Do(() => { 
                // Ensure that the 'go' command found by exec.LookPath is from the correct
                // GOROOT. Otherwise, 'some/path/go test ./...' will test against some
                // version of the 'go' binary other than 'some/path/go', which is almost
                // certainly not what the user intended.
                var (out, err) = exec.Command(tool, "env", "GOROOT").CombinedOutput();
                if (err != null) {
                    checkGoGoroot.err = err;
                    return ;
                }

                var GOROOT = strings.TrimSpace(string(out));
                if (GOROOT != runtime.GOROOT()) {
                    checkGoGoroot.err = fmt.Errorf("'go env GOROOT' does not match runtime.GOROOT:\n\tgo env: %s\n\tGOROOT: %s", GOROOT, runtime.GOROOT());
                }

            });
            if (checkGoGoroot.err != null) {
                return error.As(checkGoGoroot.err)!;
            }
            break;
        case "diff": 
            // Check that diff is the GNU version, needed for the -u argument and
            // to report missing newlines at the end of files.
            (out, err) = exec.Command(tool, "-version").Output();
            if (err != null) {
                return error.As(err)!;
            }
            if (!bytes.Contains(out, (slice<byte>)"GNU diffutils")) {
                return error.As(fmt.Errorf("diff is not the GNU version"))!;
            }
            break;
    }

    return error.As(null!)!;

});

private static (bool, error) cgoEnabled(bool bypassEnvironment) {
    bool _p0 = default;
    error _p0 = default!;

    var cmd = exec.Command("go", "env", "CGO_ENABLED");
    if (bypassEnvironment) {
        cmd.Env = append(append((slice<@string>)null, os.Environ()), "CGO_ENABLED=");
    }
    var (out, err) = cmd.CombinedOutput();
    if (err != null) {
        return (false, error.As(err)!);
    }
    var enabled = strings.TrimSpace(string(out));
    return (enabled == "1", error.As(null!)!);

}

private static bool allowMissingTool(@string tool) {
    if (runtime.GOOS == "android") { 
        // Android builds generally run tests on a separate machine from the build,
        // so don't expect any external tools to be available.
        return true;

    }
    switch (tool) {
        case "cgo": 
            if (strings.HasSuffix(os.Getenv("GO_BUILDER_NAME"), "-nocgo")) { 
                // Explicitly disabled on -nocgo builders.
                return true;

            }
            {
                var (enabled, err) = cgoEnabled(true);

                if (err == null && !enabled) { 
                    // No platform support.
                    return true;

                }

            }

            break;
        case "go": 
            if (os.Getenv("GO_BUILDER_NAME") == "illumos-amd64-joyent") { 
                // Work around a misconfigured builder (see https://golang.org/issue/33950).
                return true;

            }
            break;
        case "diff": 
            if (os.Getenv("GO_BUILDER_NAME") != "") {
                return true;
            }
            break;
        case "patch": 
            if (os.Getenv("GO_BUILDER_NAME") != "") {
                return true;
            }
            break;
    } 

    // If a developer is actively working on this test, we expect them to have all
    // of its dependencies installed. However, if it's just a dependency of some
    // other module (for example, being run via 'go test all'), we should be more
    // tolerant of unusual environments.
    return !packageMainIsDevel();

}

// NeedsTool skips t if the named tool is not present in the path.
// As a special case, "cgo" means "go" is present and can compile cgo programs.
public static void NeedsTool(Testing t, @string tool) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }

    var err = hasTool(tool);
    if (err == null) {
        return ;
    }
    if (allowMissingTool(tool)) {
        t.Skipf("skipping because %s tool not available: %v", tool, err);
    }
    else
 {
        t.Fatalf("%s tool not available: %v", tool, err);
    }
}

// NeedsGoPackages skips t if the go/packages driver (or 'go' tool) implied by
// the current process environment is not present in the path.
public static void NeedsGoPackages(Testing t) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }


    var tool = os.Getenv("GOPACKAGESDRIVER");
    switch (tool) {
        case "off": 
            // "off" forces go/packages to use the go command.
            tool = "go";
            break;
        case "": 
                   {
                       var (_, err) = exec.LookPath("gopackagesdriver");

                       if (err == null) {
                           tool = "gopackagesdriver";
                       }
                       else
            {
                           tool = "go";
                       }

                   }

            break;
    }

    NeedsTool(t, tool);

}

// NeedsGoPackagesEnv skips t if the go/packages driver (or 'go' tool) implied
// by env is not present in the path.
public static void NeedsGoPackagesEnv(Testing t, slice<@string> env) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }


    foreach (var (_, v) in env) {
        if (strings.HasPrefix(v, "GOPACKAGESDRIVER=")) {
            var tool = strings.TrimPrefix(v, "GOPACKAGESDRIVER=");
            if (tool == "off") {
                NeedsTool(t, "go");
            }
            else
 {
                NeedsTool(t, tool);
            }

            return ;

        }
    }    NeedsGoPackages(t);

}

// NeedsGoBuild skips t if the current system can't build programs with ``go build''
// and then run them with os.StartProcess or exec.Command.
// android, and darwin/arm systems don't have the userspace go build needs to run,
// and js/wasm doesn't support running subprocesses.
public static void NeedsGoBuild(Testing t) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }


    NeedsTool(t, "go");

    switch (runtime.GOOS) {
        case "android": 

        case "js": 
            t.Skipf("skipping test: %v can't build and run Go binaries", runtime.GOOS);
            break;
        case "darwin": 
            if (strings.HasPrefix(runtime.GOARCH, "arm")) {
                t.Skipf("skipping test: darwin/arm can't build and run Go binaries");
            }
            break;
    }

}

// ExitIfSmallMachine emits a helpful diagnostic and calls os.Exit(0) if the
// current machine is a builder known to have scarce resources.
//
// It should be called from within a TestMain function.
public static void ExitIfSmallMachine() {
    switch (os.Getenv("GO_BUILDER_NAME")) {
        case "linux-arm": 
            fmt.Fprintln(os.Stderr, "skipping test: linux-arm builder lacks sufficient memory (https://golang.org/issue/32834)");
            os.Exit(0);
            break;
        case "plan9-arm": 
            fmt.Fprintln(os.Stderr, "skipping test: plan9-arm builder lacks sufficient memory (https://golang.org/issue/38772)");
            os.Exit(0);
            break;
    }

}

// Go1Point returns the x in Go 1.x.
public static nint Go1Point() => func((_, panic, _) => {
    for (var i = len(build.Default.ReleaseTags) - 1; i >= 0; i--) {
        ref nint version = ref heap(out ptr<nint> _addr_version);
        {
            var (_, err) = fmt.Sscanf(build.Default.ReleaseTags[i], "go1.%d", _addr_version);

            if (err != null) {
                continue;
            }

        }

        return version;

    }
    panic("bad release tags");

});

// NeedsGo1Point skips t if the Go version used to run the test is older than
// 1.x.
public static void NeedsGo1Point(Testing t, nint x) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }

    if (Go1Point() < x) {
        t.Skipf("running Go version %q is version 1.%d, older than required 1.%d", runtime.Version(), Go1Point(), x);
    }
}

// SkipAfterGo1Point skips t if the Go version used to run the test is newer than
// 1.x.
public static void SkipAfterGo1Point(Testing t, nint x) {
    {
        helperer (t, ok) = helperer.As(t._<helperer>())!;

        if (ok) {
            t.Helper();
        }
    }

    if (Go1Point() > x) {
        t.Skipf("running Go version %q is version 1.%d, newer than maximum 1.%d", runtime.Version(), Go1Point(), x);
    }
}

} // end testenv_package
