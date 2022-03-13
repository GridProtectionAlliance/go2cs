// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package work -- go2cs converted at 2022 March 13 06:30:40 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\work\build.go
namespace go.cmd.go.@internal;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using build = go.build_package;
using exec = @internal.execabs_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using load = cmd.go.@internal.load_package;
using modload = cmd.go.@internal.modload_package;
using search = cmd.go.@internal.search_package;
using trace = cmd.go.@internal.trace_package;
using System;

public static partial class work_package {

public static ptr<base.Command> CmdBuild = addr(new base.Command(UsageLine:"go build [-o output] [build flags] [packages]",Short:"compile packages and dependencies",Long:`
Build compiles the packages named by the import paths,
along with their dependencies, but it does not install the results.

If the arguments to build are a list of .go files from a single directory,
build treats them as a list of source files specifying a single package.

When compiling packages, build ignores files that end in '_test.go'.

When compiling a single main package, build writes
the resulting executable to an output file named after
the first source file ('go build ed.go rx.go' writes 'ed' or 'ed.exe')
or the source code directory ('go build unix/sam' writes 'sam' or 'sam.exe').
The '.exe' suffix is added when writing a Windows executable.

When compiling multiple packages or a single non-main package,
build compiles the packages but discards the resulting object,
serving only as a check that the packages can be built.

The -o flag forces build to write the resulting executable or object
to the named output file or directory, instead of the default behavior described
in the last two paragraphs. If the named output is an existing directory or
ends with a slash or backslash, then any resulting executables
will be written to that directory.

The -i flag installs the packages that are dependencies of the target.
The -i flag is deprecated. Compiled packages are cached automatically.

The build flags are shared by the build, clean, get, install, list, run,
and test commands:

	-a
		force rebuilding of packages that are already up-to-date.
	-n
		print the commands but do not run them.
	-p n
		the number of programs, such as build commands or
		test binaries, that can be run in parallel.
		The default is GOMAXPROCS, normally the number of CPUs available.
	-race
		enable data race detection.
		Supported only on linux/amd64, freebsd/amd64, darwin/amd64, windows/amd64,
		linux/ppc64le and linux/arm64 (only for 48-bit VMA).
	-msan
		enable interoperation with memory sanitizer.
		Supported only on linux/amd64, linux/arm64
		and only with Clang/LLVM as the host C compiler.
		On linux/arm64, pie build mode will be used.
	-v
		print the names of packages as they are compiled.
	-work
		print the name of the temporary work directory and
		do not delete it when exiting.
	-x
		print the commands.

	-asmflags '[pattern=]arg list'
		arguments to pass on each go tool asm invocation.
	-buildmode mode
		build mode to use. See 'go help buildmode' for more.
	-compiler name
		name of compiler to use, as in runtime.Compiler (gccgo or gc).
	-gccgoflags '[pattern=]arg list'
		arguments to pass on each gccgo compiler/linker invocation.
	-gcflags '[pattern=]arg list'
		arguments to pass on each go tool compile invocation.
	-installsuffix suffix
		a suffix to use in the name of the package installation directory,
		in order to keep output separate from default builds.
		If using the -race flag, the install suffix is automatically set to race
		or, if set explicitly, has _race appended to it. Likewise for the -msan
		flag. Using a -buildmode option that requires non-default compile flags
		has a similar effect.
	-ldflags '[pattern=]arg list'
		arguments to pass on each go tool link invocation.
	-linkshared
		build code that will be linked against shared libraries previously
		created with -buildmode=shared.
	-mod mode
		module download mode to use: readonly, vendor, or mod.
		By default, if a vendor directory is present and the go version in go.mod
		is 1.14 or higher, the go command acts as if -mod=vendor were set.
		Otherwise, the go command acts as if -mod=readonly were set.
		See https://golang.org/ref/mod#build-commands for details.
	-modcacherw
		leave newly-created directories in the module cache read-write
		instead of making them read-only.
	-modfile file
		in module aware mode, read (and possibly write) an alternate go.mod
		file instead of the one in the module root directory. A file named
		"go.mod" must still be present in order to determine the module root
		directory, but it is not accessed. When -modfile is specified, an
		alternate go.sum file is also used: its path is derived from the
		-modfile flag by trimming the ".mod" extension and appending ".sum".
	-overlay file
		read a JSON config file that provides an overlay for build operations.
		The file is a JSON struct with a single field, named 'Replace', that
		maps each disk file path (a string) to its backing file path, so that
		a build will run as if the disk file path exists with the contents
		given by the backing file paths, or as if the disk file path does not
		exist if its backing file path is empty. Support for the -overlay flag
		has some limitations: importantly, cgo files included from outside the
		include path must be in the same directory as the Go package they are
		included from, and overlays will not appear when binaries and tests are
		run through go run and go test respectively.
	-pkgdir dir
		install and load all packages from dir instead of the usual locations.
		For example, when building with a non-standard configuration,
		use -pkgdir to keep generated packages in a separate location.
	-tags tag,list
		a comma-separated list of build tags to consider satisfied during the
		build. For more information about build tags, see the description of
		build constraints in the documentation for the go/build package.
		(Earlier versions of Go used a space-separated list, and that form
		is deprecated but still recognized.)
	-trimpath
		remove all file system paths from the resulting executable.
		Instead of absolute file system paths, the recorded file names
		will begin with either "go" (for the standard library),
		or a module path@version (when using modules),
		or a plain import path (when using GOPATH).
	-toolexec 'cmd args'
		a program to use to invoke toolchain programs like vet and asm.
		For example, instead of running asm, the go command will run
		'cmd args /path/to/asm <arguments for asm>'.
		The TOOLEXEC_IMPORTPATH environment variable will be set,
		matching 'go list -f {{.ImportPath}}' for the package being built.

The -asmflags, -gccgoflags, -gcflags, and -ldflags flags accept a
space-separated list of arguments to pass to an underlying tool
during the build. To embed spaces in an element in the list, surround
it with either single or double quotes. The argument list may be
preceded by a package pattern and an equal sign, which restricts
the use of that argument list to the building of packages matching
that pattern (see 'go help packages' for a description of package
patterns). Without a pattern, the argument list applies only to the
packages named on the command line. The flags may be repeated
with different patterns in order to specify different arguments for
different sets of packages. If a package matches patterns given in
multiple flags, the latest match on the command line wins.
For example, 'go build -gcflags=-S fmt' prints the disassembly
only for package fmt, while 'go build -gcflags=all=-S fmt'
prints the disassembly for fmt and all its dependencies.

For more about specifying packages, see 'go help packages'.
For more about where packages and binaries are installed,
run 'go help gopath'.
For more about calling between Go and C/C++, run 'go help c'.

Note: Build adheres to certain conventions such as those described
by 'go help gopath'. Not all projects can follow these conventions,
however. Installations that have their own conventions or that use
a separate software build system may choose to use lower-level
invocations such as 'go tool compile' and 'go tool link' to avoid
some of the overheads and design decisions of the build tool.

See also: go install, go get, go clean.
	`,));

private static readonly var concurrentGCBackendCompilationEnabledByDefault = true;



private static void init() { 
    // break init cycle
    CmdBuild.Run = runBuild;
    CmdInstall.Run = runInstall;

    CmdBuild.Flag.BoolVar(_addr_cfg.BuildI, "i", false, "");
    CmdBuild.Flag.StringVar(_addr_cfg.BuildO, "o", "", "output file or directory");

    CmdInstall.Flag.BoolVar(_addr_cfg.BuildI, "i", false, "");

    AddBuildFlags(_addr_CmdBuild, DefaultBuildFlags);
    AddBuildFlags(_addr_CmdInstall, DefaultBuildFlags);
}

// Note that flags consulted by other parts of the code
// (for example, buildV) are in cmd/go/internal/cfg.

private static slice<@string> forcedAsmflags = default;private static slice<@string> forcedGcflags = default;private static slice<@string> forcedLdflags = default;private static slice<@string> forcedGccgoflags = default;

public static toolchain BuildToolchain = new noToolchain();
private static @string ldBuildmode = default;

// buildCompiler implements flag.Var.
// It implements Set by updating both
// BuildToolchain and buildContext.Compiler.
private partial struct buildCompiler {
}

private static error Set(this buildCompiler c, @string value) {
    switch (value) {
        case "gc": 
            BuildToolchain = new gcToolchain();
            break;
        case "gccgo": 
            BuildToolchain = new gccgoToolchain();
            break;
        default: 
            return error.As(fmt.Errorf("unknown compiler %q", value))!;
            break;
    }
    cfg.BuildToolchainName = value;
    cfg.BuildToolchainCompiler = BuildToolchain.compiler;
    cfg.BuildToolchainLinker = BuildToolchain.linker;
    cfg.BuildContext.Compiler = value;
    return error.As(null!)!;
}

private static @string String(this buildCompiler c) {
    return cfg.BuildContext.Compiler;
}

private static void init() {
    switch (build.Default.Compiler) {
        case "gc": 

        case "gccgo": 
            new buildCompiler().Set(build.Default.Compiler);
            break;
    }
}

public partial struct BuildFlagMask { // : nint
}

public static readonly BuildFlagMask DefaultBuildFlags = 0;
public static readonly BuildFlagMask OmitModFlag = 1 << (int)(iota);
public static readonly var OmitModCommonFlags = 0;
public static readonly var OmitVFlag = 1;

// AddBuildFlags adds the flags common to the build, clean, get,
// install, list, run, and test commands.
public static void AddBuildFlags(ptr<base.Command> _addr_cmd, BuildFlagMask mask) {
    ref base.Command cmd = ref _addr_cmd.val;

    @base.AddBuildFlagsNX(_addr_cmd.Flag);
    cmd.Flag.BoolVar(_addr_cfg.BuildA, "a", false, "");
    cmd.Flag.IntVar(_addr_cfg.BuildP, "p", cfg.BuildP, "");
    if (mask & OmitVFlag == 0) {
        cmd.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
    }
    cmd.Flag.Var(_addr_load.BuildAsmflags, "asmflags", "");
    cmd.Flag.Var(new buildCompiler(), "compiler", "");
    cmd.Flag.StringVar(_addr_cfg.BuildBuildmode, "buildmode", "default", "");
    cmd.Flag.Var(_addr_load.BuildGcflags, "gcflags", "");
    cmd.Flag.Var(_addr_load.BuildGccgoflags, "gccgoflags", "");
    if (mask & OmitModFlag == 0) {
        @base.AddModFlag(_addr_cmd.Flag);
    }
    if (mask & OmitModCommonFlags == 0) {
        @base.AddModCommonFlags(_addr_cmd.Flag);
    }
    else
 { 
        // Add the overlay flag even when we don't add the rest of the mod common flags.
        // This only affects 'go get' in GOPATH mode, but add the flag anyway for
        // consistency.
        cmd.Flag.StringVar(_addr_fsys.OverlayFile, "overlay", "", "");
    }
    cmd.Flag.StringVar(_addr_cfg.BuildContext.InstallSuffix, "installsuffix", "", "");
    cmd.Flag.Var(_addr_load.BuildLdflags, "ldflags", "");
    cmd.Flag.BoolVar(_addr_cfg.BuildLinkshared, "linkshared", false, "");
    cmd.Flag.StringVar(_addr_cfg.BuildPkgdir, "pkgdir", "", "");
    cmd.Flag.BoolVar(_addr_cfg.BuildRace, "race", false, "");
    cmd.Flag.BoolVar(_addr_cfg.BuildMSan, "msan", false, "");
    cmd.Flag.Var((tagsFlag.val)(_addr_cfg.BuildContext.BuildTags), "tags", "");
    cmd.Flag.Var((@base.StringsFlag.val)(_addr_cfg.BuildToolexec), "toolexec", "");
    cmd.Flag.BoolVar(_addr_cfg.BuildTrimpath, "trimpath", false, "");
    cmd.Flag.BoolVar(_addr_cfg.BuildWork, "work", false, ""); 

    // Undocumented, unstable debugging flags.
    cmd.Flag.StringVar(_addr_cfg.DebugActiongraph, "debug-actiongraph", "", "");
    cmd.Flag.StringVar(_addr_cfg.DebugTrace, "debug-trace", "", "");
}

// tagsFlag is the implementation of the -tags flag.
private partial struct tagsFlag { // : slice<@string>
}

private static error Set(this ptr<tagsFlag> _addr_v, @string s) {
    ref tagsFlag v = ref _addr_v.val;
 
    // For compatibility with Go 1.12 and earlier, allow "-tags='a b c'" or even just "-tags='a'".
    if (strings.Contains(s, " ") || strings.Contains(s, "'")) {
        return error.As((@base.StringsFlag.val)(v).Set(s))!;
    }
    v.val = new slice<@string>(new @string[] {  });
    foreach (var (_, s) in strings.Split(s, ",")) {
        if (s != "") {
            v.val = append(v.val, s);
        }
    }    return error.As(null!)!;
}

private static @string String(this ptr<tagsFlag> _addr_v) {
    ref tagsFlag v = ref _addr_v.val;

    return "<TagsFlag>";
}

// fileExtSplit expects a filename and returns the name
// and ext (without the dot). If the file has no
// extension, ext will be empty.
private static (@string, @string) fileExtSplit(@string file) {
    @string name = default;
    @string ext = default;

    var dotExt = filepath.Ext(file);
    name = file[..(int)len(file) - len(dotExt)];
    if (dotExt != "") {
        ext = dotExt[(int)1..];
    }
    return ;
}

private static slice<ptr<load.Package>> pkgsMain(slice<ptr<load.Package>> pkgs) {
    slice<ptr<load.Package>> res = default;

    foreach (var (_, p) in pkgs) {
        if (p.Name == "main") {
            res = append(res, p);
        }
    }    return res;
}

private static slice<ptr<load.Package>> pkgsNotMain(slice<ptr<load.Package>> pkgs) {
    slice<ptr<load.Package>> res = default;

    foreach (var (_, p) in pkgs) {
        if (p.Name != "main") {
            res = append(res, p);
        }
    }    return res;
}

private static slice<ptr<load.Package>> oneMainPkg(slice<ptr<load.Package>> pkgs) {
    if (len(pkgs) != 1 || pkgs[0].Name != "main") {
        @base.Fatalf("-buildmode=%s requires exactly one main package", cfg.BuildBuildmode);
    }
    return pkgs;
}

private static Func<slice<ptr<load.Package>>, slice<ptr<load.Package>>> pkgsFilter = pkgs => pkgs;

private static var runtimeVersion = runtime.Version();

private static void runBuild(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    BuildInit();
    Builder b = default;
    b.Init();

    var pkgs = load.PackagesAndErrors(ctx, new load.PackageOpts(), args);
    load.CheckPackageErrors(pkgs);

    var explicitO = len(cfg.BuildO) > 0;

    if (len(pkgs) == 1 && pkgs[0].Name == "main" && cfg.BuildO == "") {
        cfg.BuildO = pkgs[0].DefaultExecName();
        cfg.BuildO += cfg.ExeSuffix;
    }
    switch (cfg.BuildContext.Compiler) {
        case "gccgo": 
            if (load.BuildGcflags.Present()) {
                fmt.Println("go build: when using gccgo toolchain, please pass compiler flags using -gccgoflags, not -gcflags");
            }
            if (load.BuildLdflags.Present()) {
                fmt.Println("go build: when using gccgo toolchain, please pass linker flags using -gccgoflags, not -ldflags");
            }
            break;
        case "gc": 
            if (load.BuildGccgoflags.Present()) {
                fmt.Println("go build: when using gc toolchain, please pass compile flags using -gcflags, and linker flags using -ldflags");
            }
            break;
    }

    var depMode = ModeBuild;
    if (cfg.BuildI) {
        depMode = ModeInstall;
        fmt.Fprint(os.Stderr, "go build: -i flag is deprecated\n");
    }
    pkgs = omitTestOnly(pkgsFilter(pkgs)); 

    // Special case -o /dev/null by not writing at all.
    if (cfg.BuildO == os.DevNull) {
        cfg.BuildO = "";
    }
    if (cfg.BuildO != "") { 
        // If the -o name exists and is a directory or
        // ends with a slash or backslash, then
        // write all main packages to that directory.
        // Otherwise require only a single package be built.
        {
            var (fi, err) = os.Stat(cfg.BuildO);

            if ((err == null && fi.IsDir()) || strings.HasSuffix(cfg.BuildO, "/") || strings.HasSuffix(cfg.BuildO, string(os.PathSeparator))) {
                if (!explicitO) {
                    @base.Fatalf("go build: build output %q already exists and is a directory", cfg.BuildO);
                }
                ptr<Action> a = addr(new Action(Mode:"go build"));
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in pkgs) {
                        p = __p;
                        if (p.Name != "main") {
                            continue;
                        }
                        p.Target = filepath.Join(cfg.BuildO, p.DefaultExecName());
                        p.Target += cfg.ExeSuffix;
                        p.Stale = true;
                        p.StaleReason = "build -o flag in use";
                        a.Deps = append(a.Deps, b.AutoAction(ModeInstall, depMode, p));
                    }

                    p = p__prev1;
                }

                if (len(a.Deps) == 0) {
                    @base.Fatalf("go build: no main packages to build");
                }
                b.Do(ctx, a);
                return ;
            }

        }
        if (len(pkgs) > 1) {
            @base.Fatalf("go build: cannot write multiple packages to non-directory %s", cfg.BuildO);
        }
        else if (len(pkgs) == 0) {
            @base.Fatalf("no packages to build");
        }
        var p = pkgs[0];
        p.Target = cfg.BuildO;
        p.Stale = true; // must build - not up to date
        p.StaleReason = "build -o flag in use";
        a = b.AutoAction(ModeInstall, depMode, p);
        b.Do(ctx, a);
        return ;
    }
    a = addr(new Action(Mode:"go build"));
    {
        var p__prev1 = p;

        foreach (var (_, __p) in pkgs) {
            p = __p;
            a.Deps = append(a.Deps, b.AutoAction(ModeBuild, depMode, p));
        }
        p = p__prev1;
    }

    if (cfg.BuildBuildmode == "shared") {
        a = b.buildmodeShared(ModeBuild, depMode, args, pkgs, a);
    }
    b.Do(ctx, a);
}

public static ptr<base.Command> CmdInstall = addr(new base.Command(UsageLine:"go install [build flags] [packages]",Short:"compile and install packages and dependencies",Long:`
Install compiles and installs the packages named by the import paths.

Executables are installed in the directory named by the GOBIN environment
variable, which defaults to $GOPATH/bin or $HOME/go/bin if the GOPATH
environment variable is not set. Executables in $GOROOT
are installed in $GOROOT/bin or $GOTOOLDIR instead of $GOBIN.

If the arguments have version suffixes (like @latest or @v1.0.0), "go install"
builds packages in module-aware mode, ignoring the go.mod file in the current
directory or any parent directory, if there is one. This is useful for
installing executables without affecting the dependencies of the main module.
To eliminate ambiguity about which module versions are used in the build, the
arguments must satisfy the following constraints:

- Arguments must be package paths or package patterns (with "..." wildcards).
They must not be standard packages (like fmt), meta-patterns (std, cmd,
all), or relative or absolute file paths.

- All arguments must have the same version suffix. Different queries are not
allowed, even if they refer to the same version.

- All arguments must refer to packages in the same module at the same version.

- No module is considered the "main" module. If the module containing
packages named on the command line has a go.mod file, it must not contain
directives (replace and exclude) that would cause it to be interpreted
differently than if it were the main module. The module must not require
a higher version of itself.

- Package path arguments must refer to main packages. Pattern arguments
will only match main packages.

If the arguments don't have version suffixes, "go install" may run in
module-aware mode or GOPATH mode, depending on the GO111MODULE environment
variable and the presence of a go.mod file. See 'go help modules' for details.
If module-aware mode is enabled, "go install" runs in the context of the main
module.

When module-aware mode is disabled, other packages are installed in the
directory $GOPATH/pkg/$GOOS_$GOARCH. When module-aware mode is enabled,
other packages are built and cached but not installed.

The -i flag installs the dependencies of the named packages as well.
The -i flag is deprecated. Compiled packages are cached automatically.

For more about the build flags, see 'go help build'.
For more about specifying packages, see 'go help packages'.

See also: go build, go get, go clean.
	`,));

// libname returns the filename to use for the shared library when using
// -buildmode=shared. The rules we use are:
// Use arguments for special 'meta' packages:
//    std --> libstd.so
//    std cmd --> libstd,cmd.so
// A single non-meta argument with trailing "/..." is special cased:
//    foo/... --> libfoo.so
//    (A relative path like "./..."  expands the "." first)
// Use import paths for other cases, changing '/' to '-':
//    somelib --> libsubdir-somelib.so
//    ./ or ../ --> libsubdir-somelib.so
//    gopkg.in/tomb.v2 -> libgopkg.in-tomb.v2.so
//    a/... b/... ---> liba/c,b/d.so - all matching import paths
// Name parts are joined with ','.
private static (@string, error) libname(slice<@string> args, slice<ptr<load.Package>> pkgs) {
    @string _p0 = default;
    error _p0 = default!;

    @string libname = default;
    Action<@string> appendName = arg => {
        if (libname == "") {
            libname = arg;
        }
        else
 {
            libname += "," + arg;
        }
    };
    bool haveNonMeta = default;
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in args) {
            arg = __arg;
            if (search.IsMetaPackage(arg)) {
                appendName(arg);
            }
            else
 {
                haveNonMeta = true;
            }
        }
        arg = arg__prev1;
    }

    if (len(libname) == 0) { // non-meta packages only. use import paths
        if (len(args) == 1 && strings.HasSuffix(args[0], "/...")) { 
            // Special case of "foo/..." as mentioned above.
            var arg = strings.TrimSuffix(args[0], "/...");
            if (build.IsLocalImport(arg)) {
                var (cwd, _) = os.Getwd();
                var (bp, _) = cfg.BuildContext.ImportDir(filepath.Join(cwd, arg), build.FindOnly);
                if (bp.ImportPath != "" && bp.ImportPath != ".") {
                    arg = bp.ImportPath;
                }
            }
            appendName(strings.ReplaceAll(arg, "/", "-"));
        }
        else
 {
            foreach (var (_, pkg) in pkgs) {
                appendName(strings.ReplaceAll(pkg.ImportPath, "/", "-"));
            }
        }
    }
    else if (haveNonMeta) { // have both meta package and a non-meta one
        return ("", error.As(errors.New("mixing of meta and non-meta packages is not allowed"))!);
    }
    return ("lib" + libname + ".so", error.As(null!)!);
}

private static void runInstall(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;
 
    // TODO(golang.org/issue/41696): print a deprecation message for the -i flag
    // whenever it's set (or just remove it). For now, we don't print a message
    // if all named packages are in GOROOT. cmd/dist (run by make.bash) uses
    // 'go install -i' when bootstrapping, and we don't want to show deprecation
    // messages in that case.
    foreach (var (_, arg) in args) {
        if (strings.Contains(arg, "@") && !build.IsLocalImport(arg) && !filepath.IsAbs(arg)) {
            if (cfg.BuildI) {
                fmt.Fprint(os.Stderr, "go install: -i flag is deprecated\n");
            }
            installOutsideModule(ctx, args);
            return ;
        }
    }    BuildInit();
    var pkgs = load.PackagesAndErrors(ctx, new load.PackageOpts(), args);
    if (cfg.ModulesEnabled && !modload.HasModRoot()) {
        var haveErrors = false;
        var allMissingErrors = true;
        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in pkgs) {
                pkg = __pkg;
                if (pkg.Error == null) {
                    continue;
                }
                haveErrors = true;
                {
                    ref var missingErr = ref heap((modload.ImportMissingError.val)(null), out ptr<var> _addr_missingErr);

                    if (!errors.As(pkg.Error, _addr_missingErr)) {
                        allMissingErrors = false;
                        break;
                    }

                }
            }

            pkg = pkg__prev1;
        }

        if (haveErrors && allMissingErrors) {
            var latestArgs = make_slice<@string>(len(args));
            foreach (var (i) in args) {
                latestArgs[i] = args[i] + "@latest";
            }
            var hint = strings.Join(latestArgs, " ");
            @base.Fatalf("go install: version is required when current directory is not in a module\n\tTry 'go install %s' to install the latest version", hint);
        }
    }
    load.CheckPackageErrors(pkgs);
    if (cfg.BuildI) {
        var allGoroot = true;
        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in pkgs) {
                pkg = __pkg;
                if (!pkg.Goroot) {
                    allGoroot = false;
                    break;
                }
            }

            pkg = pkg__prev1;
        }

        if (!allGoroot) {
            fmt.Fprint(os.Stderr, "go install: -i flag is deprecated\n");
        }
    }
    InstallPackages(ctx, args, pkgs);
}

// omitTestOnly returns pkgs with test-only packages removed.
private static slice<ptr<load.Package>> omitTestOnly(slice<ptr<load.Package>> pkgs) {
    slice<ptr<load.Package>> list = default;
    foreach (var (_, p) in pkgs) {
        if (len(p.GoFiles) + len(p.CgoFiles) == 0 && !p.Internal.CmdlinePkgLiteral) { 
            // Package has no source files,
            // perhaps due to build tags or perhaps due to only having *_test.go files.
            // Also, it is only being processed as the result of a wildcard match
            // like ./..., not because it was listed as a literal path on the command line.
            // Ignore it.
            continue;
        }
        list = append(list, p);
    }    return list;
}

public static void InstallPackages(context.Context ctx, slice<@string> patterns, slice<ptr<load.Package>> pkgs) => func((defer, _, _) => {
    var (ctx, span) = trace.StartSpan(ctx, "InstallPackages " + strings.Join(patterns, " "));
    defer(span.Done());

    if (cfg.GOBIN != "" && !filepath.IsAbs(cfg.GOBIN)) {
        @base.Fatalf("cannot install, GOBIN must be an absolute path");
    }
    pkgs = omitTestOnly(pkgsFilter(pkgs));
    {
        var p__prev1 = p;

        foreach (var (_, __p) in pkgs) {
            p = __p;
            if (p.Target == "") {

                if (p.Standard && p.ImportPath == "unsafe")                 else if (p.Name != "main" && p.Internal.Local && p.ConflictDir == "")                 else if (p.Name != "main" && p.Module != null)                 else if (p.Internal.GobinSubdir) 
                    @base.Errorf("go %s: cannot install cross-compiled binaries when GOBIN is set", cfg.CmdName);
                else if (p.Internal.CmdlineFiles) 
                    @base.Errorf("go %s: no install location for .go files listed on command line (GOBIN not set)", cfg.CmdName);
                else if (p.ConflictDir != "") 
                    @base.Errorf("go %s: no install location for %s: hidden by %s", cfg.CmdName, p.Dir, p.ConflictDir);
                else 
                    @base.Errorf("go %s: no install location for directory %s outside GOPATH\n" + "\tFor more details see: 'go help gopath'", cfg.CmdName, p.Dir);
                            }
        }
        p = p__prev1;
    }

    @base.ExitIfErrors();

    Builder b = default;
    b.Init();
    var depMode = ModeBuild;
    if (cfg.BuildI) {
        depMode = ModeInstall;
    }
    ptr<Action> a = addr(new Action(Mode:"go install"));
    slice<ptr<Action>> tools = default;
    {
        var p__prev1 = p;

        foreach (var (_, __p) in pkgs) {
            p = __p; 
            // If p is a tool, delay the installation until the end of the build.
            // This avoids installing assemblers/compilers that are being executed
            // by other steps in the build.
            var a1 = b.AutoAction(ModeInstall, depMode, p);
            if (load.InstallTargetDir(p) == load.ToTool) {
                a.Deps = append(a.Deps, a1.Deps);
                a1.Deps = append(a1.Deps, a);
                tools = append(tools, a1);
                continue;
            }
            a.Deps = append(a.Deps, a1);
        }
        p = p__prev1;
    }

    if (len(tools) > 0) {
        a = addr(new Action(Mode:"go install (tools)",Deps:tools,));
    }
    if (cfg.BuildBuildmode == "shared") { 
        // Note: If buildmode=shared then only non-main packages
        // are present in the pkgs list, so all the special case code about
        // tools above did not apply, and a is just a simple Action
        // with a list of Deps, one per package named in pkgs,
        // the same as in runBuild.
        a = b.buildmodeShared(ModeInstall, ModeInstall, patterns, pkgs, a);
    }
    b.Do(ctx, a);
    @base.ExitIfErrors(); 

    // Success. If this command is 'go install' with no arguments
    // and the current directory (the implicit argument) is a command,
    // remove any leftover command binary from a previous 'go build'.
    // The binary is installed; it's not needed here anymore.
    // And worse it might be a stale copy, which you don't want to find
    // instead of the installed one if $PATH contains dot.
    // One way to view this behavior is that it is as if 'go install' first
    // runs 'go build' and the moves the generated file to the install dir.
    // See issue 9645.
    if (len(patterns) == 0 && len(pkgs) == 1 && pkgs[0].Name == "main") { 
        // Compute file 'go build' would have created.
        // If it exists and is an executable file, remove it.
        var targ = pkgs[0].DefaultExecName();
        targ += cfg.ExeSuffix;
        if (filepath.Join(pkgs[0].Dir, targ) != pkgs[0].Target) { // maybe $GOBIN is the current directory
            var (fi, err) = os.Stat(targ);
            if (err == null) {
                var m = fi.Mode();
                if (m.IsRegular()) {
                    if (m & 0111 != 0 || cfg.Goos == "windows") { // windows never sets executable bit
                        os.Remove(targ);
                    }
                }
            }
        }
    }
});

// installOutsideModule implements 'go install pkg@version'. It builds and
// installs one or more main packages in module mode while ignoring any go.mod
// in the current directory or parent directories.
//
// See golang.org/issue/40276 for details and rationale.
private static void installOutsideModule(context.Context ctx, slice<@string> args) {
    modload.ForceUseModules = true;
    modload.RootMode = modload.NoRoot;
    modload.AllowMissingModuleImports();
    modload.Init();
    BuildInit(); 

    // Load packages. Ignore non-main packages.
    // Print a warning if an argument contains "..." and matches no main packages.
    // PackagesAndErrors already prints warnings for patterns that don't match any
    // packages, so be careful not to double print.
    // TODO(golang.org/issue/40276): don't report errors loading non-main packages
    // matched by a pattern.
    load.PackageOpts pkgOpts = new load.PackageOpts(MainOnly:true);
    var (pkgs, err) = load.PackagesAndErrorsOutsideModule(ctx, pkgOpts, args);
    if (err != null) {
        @base.Fatalf("go install: %v", err);
    }
    load.CheckPackageErrors(pkgs);
    var patterns = make_slice<@string>(len(args));
    foreach (var (i, arg) in args) {
        patterns[i] = arg[..(int)strings.Index(arg, "@")];
    }    InstallPackages(ctx, patterns, pkgs);
}

// ExecCmd is the command to use to run user binaries.
// Normally it is empty, meaning run the binaries directly.
// If cross-compiling and running on a remote system or
// simulator, it is typically go_GOOS_GOARCH_exec, with
// the target GOOS and GOARCH substituted.
// The -exec flag overrides these defaults.
public static slice<@string> ExecCmd = default;

// FindExecCmd derives the value of ExecCmd to use.
// It returns that value and leaves ExecCmd set for direct use.
public static slice<@string> FindExecCmd() {
    if (ExecCmd != null) {
        return ExecCmd;
    }
    ExecCmd = new slice<@string>(new @string[] {  }); // avoid work the second time
    if (cfg.Goos == runtime.GOOS && cfg.Goarch == runtime.GOARCH) {
        return ExecCmd;
    }
    var (path, err) = exec.LookPath(fmt.Sprintf("go_%s_%s_exec", cfg.Goos, cfg.Goarch));
    if (err == null) {
        ExecCmd = new slice<@string>(new @string[] { path });
    }
    return ExecCmd;
}

} // end work_package
