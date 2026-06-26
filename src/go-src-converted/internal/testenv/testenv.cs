// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testenv provides information about what functionality
// is available in different testing environments run by the Go team.
//
// It is an internal package because these details are specific
// to the Go team's test setup (on build.golang.org) and not
// fundamental to tests in general.
namespace go.@internal;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using cfg = @internal.cfg_package;
using goarch = @internal.goarch_package;
using platform = @internal.platform_package;
using os = os_package;
using exec = os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using os;
using path;
using ꓸꓸꓸ@string = Span<@string>;

partial class testenv_package {

// Save the original environment during init for use in checks. A test
// binary may modify its environment before calling HasExec to change its
// behavior (such as mimicking a command-line tool), and that modified
// environment might cause environment checks to behave erratically.
internal static slice<@string> origEnv = os.Environ();

// Builder reports the name of the builder running this test
// (for example, "linux-amd64" or "windows-386-gce").
// If the test is not running on the build infrastructure,
// Builder returns the empty string.
public static @string Builder() {
    return os.Getenv("GO_BUILDER_NAME"u8);
}

// HasGoBuild reports whether the current system can build programs with “go build”
// and then run them with os.StartProcess or exec.Command.
public static bool HasGoBuild() {
    if (os.Getenv("GO_GCFLAGS"u8) != ""u8) {
        // It's too much work to require every caller of the go command
        // to pass along "-gcflags="+os.Getenv("GO_GCFLAGS").
        // For now, if $GO_GCFLAGS is set, report that we simply can't
        // run go build.
        return false;
    }
    goBuildOnce.Do(
    var origEnvʗ2 = origEnv;
    () => {
        var cmd = exec.Command("go"u8, "tool"u8, "-n", "compile");
        cmd.val.Env = origEnvʗ2;
        (@out, err) = cmd.Output();
        if (err != default!) {
            goBuildErr = fmt.Errorf("%v: %w"u8, cmd, err);
            return;
        }
        @out = bytes.TrimSpace(@out);
        if (len(@out) == 0) {
            goBuildErr = fmt.Errorf("%v: no tool reported"u8, cmd);
            return;
        }
        {
            var (_, errΔ1) = exec.LookPath(((@string)@out)); if (errΔ1 != default!) {
                goBuildErr = errΔ1;
                return;
            }
        }
        if (platform.MustLinkExternal(runtime.GOOS, runtime.GOARCH, false)) {
            if (os.Getenv("CC"u8) == ""u8) {
                var cmdΔ1 = exec.Command("go"u8, "env"u8, "CC");
                cmdΔ1.val.Env = origEnvʗ2;
                (outΔ1, errΔ2) = cmdΔ1.Output();
                if (errΔ2 != default!) {
                    goBuildErr = fmt.Errorf("%v: %w"u8, cmdΔ1, errΔ2);
                    return;
                }
                outΔ1 = bytes.TrimSpace(outΔ1);
                if (len(outΔ1) == 0) {
                    goBuildErr = fmt.Errorf("%v: no CC reported"u8, cmdΔ1);
                    return;
                }
                (_, goBuildErr) = exec.LookPath(((@string)outΔ1));
            }
        }
    });
    return goBuildErr == default!;
}

internal static sync.Once goBuildOnce;
internal static error goBuildErr;

// MustHaveGoBuild checks that the current system can build programs with “go build”
// and then run them with os.StartProcess or exec.Command.
// If not, MustHaveGoBuild calls t.Skip with an explanation.
public static void MustHaveGoBuild(testing.TB t) {
    if (os.Getenv("GO_GCFLAGS"u8) != ""u8) {
        t.Helper();
        t.Skipf("skipping test: 'go build' not compatible with setting $GO_GCFLAGS"u8);
    }
    if (!HasGoBuild()) {
        t.Helper();
        t.Skipf("skipping test: 'go build' unavailable: %v"u8, goBuildErr);
    }
}

// HasGoRun reports whether the current system can run programs with “go run”.
public static bool HasGoRun() {
    // For now, having go run and having go build are the same.
    return HasGoBuild();
}

// MustHaveGoRun checks that the current system can run programs with “go run”.
// If not, MustHaveGoRun calls t.Skip with an explanation.
public static void MustHaveGoRun(testing.TB t) {
    if (!HasGoRun()) {
        t.Skipf("skipping test: 'go run' not available on %s/%s"u8, runtime.GOOS, runtime.GOARCH);
    }
}

// HasParallelism reports whether the current system can execute multiple
// threads in parallel.
// There is a copy of this function in cmd/dist/test.go.
public static bool HasParallelism() {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        return false;
    }

    return true;
}

// MustHaveParallelism checks that the current system can execute multiple
// threads in parallel. If not, MustHaveParallelism calls t.Skip with an explanation.
public static void MustHaveParallelism(testing.TB t) {
    if (!HasParallelism()) {
        t.Skipf("skipping test: no parallelism available on %s/%s"u8, runtime.GOOS, runtime.GOARCH);
    }
}

// GoToolPath reports the path to the Go tool.
// It is a convenience wrapper around GoTool.
// If the tool is unavailable GoToolPath calls t.Skip.
// If the tool should be available and isn't, GoToolPath calls t.Fatal.
public static @string GoToolPath(testing.TB t) {
    MustHaveGoBuild(t);
    var (path, err) = GoTool();
    if (err != default!) {
        t.Fatal(err);
    }
    // Add all environment variables that affect the Go command to test metadata.
    // Cached test results will be invalidate when these variables change.
    // See golang.org/issue/32285.
    foreach (var (_, envVar) in strings.Fields(cfg.KnownEnv)) {
        os.Getenv(envVar);
    }
    return path;
}

internal static sync.Once gorootOnce;
internal static @string gorootPath;
internal static error gorootErr;

internal static (@string, error) findGOROOT() {
    gorootOnce.Do(() => {
        gorootPath = runtime.GOROOT();
        if (gorootPath != ""u8) {
            return;
        }
        var (cwd, err) = os.Getwd();
        if (err != default!) {
            gorootErr = fmt.Errorf("finding GOROOT: %w"u8, err);
            return;
        }
        @string dir = cwd;
        while (ᐧ) {
            @string parent = filepath.Dir(dir);
            if (parent == dir) {
                gorootErr = fmt.Errorf("failed to locate GOROOT/src in any parent directory"u8);
                return;
            }
            {
                @string @base = filepath.Base(dir); if (@base != "src"u8) {
                    dir = parent;
                    continue;
                }
            }
            (b, errΔ1) = os.ReadFile(filepath.Join(dir, "go.mod"));
            if (errΔ1 != default!) {
                if (os.IsNotExist(errΔ1)) {
                    dir = parent;
                    continue;
                }
                gorootErr = fmt.Errorf("finding GOROOT: %w"u8, errΔ1);
                return;
            }
            @string goMod = ((@string)b);
            while (goMod != ""u8) {
                @string line = default!;
                (line, goMod, _) = strings.Cut(goMod, "\n"u8);
                var fields = strings.Fields(line);
                if (len(fields) >= 2 && fields[0] == "module" && fields[1] == "std") {
                    gorootPath = parent;
                    return;
                }
            }
        }
    });
    return (gorootPath, gorootErr);
}

// GOROOT reports the path to the directory containing the root of the Go
// project source tree. This is normally equivalent to runtime.GOROOT, but
// works even if the test binary was built with -trimpath and cannot exec
// 'go env GOROOT'.
//
// If GOROOT cannot be found, GOROOT skips t if t is non-nil,
// or panics otherwise.
public static @string GOROOT(testing.TB t) {
    var (path, err) = findGOROOT();
    if (err != default!) {
        if (t == default!) {
            throw panic(err);
        }
        t.Helper();
        t.Skip(err);
    }
    return path;
}

// GoTool reports the path to the Go tool.
public static (@string, error) GoTool() {
    if (!HasGoBuild()) {
        return ("", errors.New("platform cannot run go tool"u8));
    }
    goToolOnce.Do(() => {
        (goToolPath, goToolErr) = exec.LookPath("go"u8);
    });
    return (goToolPath, goToolErr);
}

internal static sync.Once goToolOnce;
internal static @string goToolPath;
internal static error goToolErr;

// HasSrc reports whether the entire source tree is available under GOROOT.
public static bool HasSrc() {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "ios"u8) {
        return false;
    }

    return true;
}

// HasExternalNetwork reports whether the current system can use
// external (non-localhost) networks.
public static bool HasExternalNetwork() {
    return !testing.Short() && runtime.GOOS != "js"u8 && runtime.GOOS != "wasip1"u8;
}

// MustHaveExternalNetwork checks that the current system can use
// external (non-localhost) networks.
// If not, MustHaveExternalNetwork calls t.Skip with an explanation.
public static void MustHaveExternalNetwork(testing.TB t) {
    if (runtime.GOOS == "js"u8 || runtime.GOOS == "wasip1"u8) {
        t.Helper();
        t.Skipf("skipping test: no external network on %s"u8, runtime.GOOS);
    }
    if (testing.Short()) {
        t.Helper();
        t.Skipf("skipping test: no external network in -short mode"u8);
    }
}

// HasCGO reports whether the current system can use cgo.
public static bool HasCGO() {
    hasCgoOnce.Do(
    var origEnvʗ2 = origEnv;
    () => {
        var (goTool, err) = GoTool();
        if (err != default!) {
            return;
        }
        var cmd = exec.Command(goTool, "env"u8, "CGO_ENABLED");
        cmd.val.Env = origEnvʗ2;
        (@out, err) = cmd.Output();
        if (err != default!) {
            throw panic(fmt.Sprintf("%v: %v"u8, cmd, @out));
        }
        (hasCgo, err) = strconv.ParseBool(((@string)bytes.TrimSpace(@out)));
        if (err != default!) {
            throw panic(fmt.Sprintf("%v: non-boolean output %q"u8, cmd, @out));
        }
    });
    return hasCgo;
}

internal static sync.Once hasCgoOnce;
internal static bool hasCgo;

// MustHaveCGO calls t.Skip if cgo is not available.
public static void MustHaveCGO(testing.TB t) {
    if (!HasCGO()) {
        t.Skipf("skipping test: no cgo"u8);
    }
}

// CanInternalLink reports whether the current system can link programs with
// internal linking.
public static bool CanInternalLink(bool withCgo) {
    return !platform.MustLinkExternal(runtime.GOOS, runtime.GOARCH, withCgo);
}

// MustInternalLink checks that the current system can link programs with internal
// linking.
// If not, MustInternalLink calls t.Skip with an explanation.
public static void MustInternalLink(testing.TB t, bool withCgo) {
    if (!CanInternalLink(withCgo)) {
        if (withCgo && CanInternalLink(false)) {
            t.Skipf("skipping test: internal linking on %s/%s is not supported with cgo"u8, runtime.GOOS, runtime.GOARCH);
        }
        t.Skipf("skipping test: internal linking on %s/%s is not supported"u8, runtime.GOOS, runtime.GOARCH);
    }
}

// MustInternalLinkPIE checks whether the current system can link PIE binary using
// internal linking.
// If not, MustInternalLinkPIE calls t.Skip with an explanation.
public static void MustInternalLinkPIE(testing.TB t) {
    if (!platform.InternalLinkPIESupported(runtime.GOOS, runtime.GOARCH)) {
        t.Skipf("skipping test: internal linking for buildmode=pie on %s/%s is not supported"u8, runtime.GOOS, runtime.GOARCH);
    }
}

// MustHaveBuildMode reports whether the current system can build programs in
// the given build mode.
// If not, MustHaveBuildMode calls t.Skip with an explanation.
public static void MustHaveBuildMode(testing.TB t, @string buildmode) {
    if (!platform.BuildModeSupported(runtime.Compiler, buildmode, runtime.GOOS, runtime.GOARCH)) {
        t.Skipf("skipping test: build mode %s on %s/%s is not supported by the %s compiler"u8, buildmode, runtime.GOOS, runtime.GOARCH, runtime.Compiler);
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
        t.Skipf("skipping test: cannot make symlinks on %s/%s: %s"u8, runtime.GOOS, runtime.GOARCH, reason);
    }
}

// HasLink reports whether the current system can use os.Link.
public static bool HasLink() {
    // From Android release M (Marshmallow), hard linking files is blocked
    // and an attempt to call link() on a file will return EACCES.
    // - https://code.google.com/p/android-developer-preview/issues/detail?id=3150
    return runtime.GOOS != "plan9"u8 && runtime.GOOS != "android"u8;
}

// MustHaveLink reports whether the current system can use os.Link.
// If not, MustHaveLink calls t.Skip with an explanation.
public static void MustHaveLink(testing.TB t) {
    if (!HasLink()) {
        t.Skipf("skipping test: hardlinks are not supported on %s/%s"u8, runtime.GOOS, runtime.GOARCH);
    }
}

internal static ж<bool> flaky = flag.Bool("flaky"u8, false, "run known-flaky tests too"u8);

public static void SkipFlaky(testing.TB t, nint issue) {
    t.Helper();
    if (!flaky.val) {
        t.Skipf("skipping known flaky test without the -flaky flag; see golang.org/issue/%d"u8, issue);
    }
}

public static void SkipFlakyNet(testing.TB t) {
    t.Helper();
    {
        var (v, _) = strconv.ParseBool(os.Getenv("GO_BUILDER_FLAKY_NET"u8)); if (v) {
            t.Skip("skipping test on builder known to have frequent network failures");
        }
    }
}

// CPUIsSlow reports whether the CPU running the test is suspected to be slow.
public static bool CPUIsSlow() {
    var exprᴛ1 = runtime.GOARCH;
    if (exprᴛ1 == "arm"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "wasm"u8) {
        return true;
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
        t.Skipf("skipping test in -short mode on %s"u8, runtime.GOARCH);
    }
}

// SkipIfOptimizationOff skips t if optimization is disabled.
public static void SkipIfOptimizationOff(testing.TB t) {
    if (OptimizationOff()) {
        t.Helper();
        t.Skip("skipping test with optimization disabled");
    }
}

// WriteImportcfg writes an importcfg file used by the compiler or linker to
// dstPath containing entries for the file mappings in packageFiles, as well
// as for the packages transitively imported by the package(s) in pkgs.
//
// pkgs may include any package pattern that is valid to pass to 'go list',
// so it may also be a list of Go source files all in the same directory.
public static void WriteImportcfg(testing.TB t, @string dstPath, map<@string, @string> packageFiles, params ꓸꓸꓸ@string pkgsʗp) {
    var pkgs = pkgsʗp.slice();

    t.Helper();
    var icfg = @new<bytes.Buffer>();
    icfg.WriteString("# import config\n"u8);
    foreach (var (k, v) in packageFiles) {
        fmt.Fprintf(~icfg, "packagefile %s=%s\n"u8, k, v);
    }
    if (len(pkgs) > 0) {
        // Use 'go list' to resolve any missing packages and rewrite the import map.
        var cmd = Command(t, GoToolPath(t), "list"u8, "-export", "-deps", "-f", @"{{if ne .ImportPath ""command-line-arguments""}}{{if .Export}}{{.ImportPath}}={{.Export}}{{end}}{{end}}");
        cmd.val.Args = append((~cmd).Args, pkgs.ꓸꓸꓸ);
        cmd.val.Stderr = @new<strings.Builder>();
        (@out, err) = cmd.Output();
        if (err != default!) {
            t.Fatalf("%v: %v\n%s"u8, cmd, err, (~cmd).Stderr);
        }
        foreach (var (_, line) in strings.Split(((@string)@out), "\n"u8)) {
            if (line == ""u8) {
                continue;
            }
            var (importPath, export, ok) = strings.Cut(line, "="u8);
            if (!ok) {
                t.Fatalf("invalid line in output from %v:\n%s"u8, cmd, line);
            }
            if (packageFiles[importPath] == "") {
                fmt.Fprintf(~icfg, "packagefile %s=%s\n"u8, importPath, export);
            }
        }
    }
    {
        var err = os.WriteFile(dstPath, icfg.Bytes(), 438); if (err != default!) {
            t.Fatal(err);
        }
    }
}

// SyscallIsNotSupported reports whether err may indicate that a system call is
// not supported by the current platform or execution environment.
public static bool SyscallIsNotSupported(error err) {
    return syscallIsNotSupported(err);
}

// ParallelOn64Bit calls t.Parallel() unless there is a case that cannot be parallel.
// This function should be used when it is necessary to avoid t.Parallel on
// 32-bit machines, typically because the test uses lots of memory.
public static void ParallelOn64Bit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.val;

    if (goarch.PtrSize == 4) {
        return;
    }
    t.Parallel();
}

} // end testenv_package
