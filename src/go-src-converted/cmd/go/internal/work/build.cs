// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package work -- go2cs converted at 2020 August 29 10:01:12 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\build.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using exec = go.os.exec_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        public static base.Command CmdBuild = ref new base.Command(UsageLine:"build [-o output] [-i] [build flags] [packages]",Short:"compile packages and dependencies",Long:`
Build compiles the packages named by the import paths,
along with their dependencies, but it does not install the results.

If the arguments to build are a list of .go files, build treats
them as a list of source files specifying a single package.

When compiling a single main package, build writes
the resulting executable to an output file named after
the first source file ('go build ed.go rx.go' writes 'ed' or 'ed.exe')
or the source code directory ('go build unix/sam' writes 'sam' or 'sam.exe').
The '.exe' suffix is added when writing a Windows executable.

When compiling multiple packages or a single non-main package,
build compiles the packages but discards the resulting object,
serving only as a check that the packages can be built.

When compiling packages, build ignores files that end in '_test.go'.

The -o flag, only allowed when compiling a single package,
forces build to write the resulting executable or object
to the named output file, instead of the default behavior described
in the last two paragraphs.

The -i flag installs the packages that are dependencies of the target.

The build flags are shared by the build, clean, get, install, list, run,
and test commands:

	-a
		force rebuilding of packages that are already up-to-date.
	-n
		print the commands but do not run them.
	-p n
		the number of programs, such as build commands or
		test binaries, that can be run in parallel.
		The default is the number of CPUs available.
	-race
		enable data race detection.
		Supported only on linux/amd64, freebsd/amd64, darwin/amd64 and windows/amd64.
	-msan
		enable interoperation with memory sanitizer.
		Supported only on linux/amd64,
		and only with Clang/LLVM as the host C compiler.
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
		link against shared libraries previously created with
		-buildmode=shared.
	-pkgdir dir
		install and load all packages from dir instead of the usual locations.
		For example, when building with a non-standard configuration,
		use -pkgdir to keep generated packages in a separate location.
	-tags 'tag list'
		a space-separated list of build tags to consider satisfied during the
		build. For more information about build tags, see the description of
		build constraints in the documentation for the go/build package.
	-toolexec 'cmd args'
		a program to use to invoke toolchain programs like vet and asm.
		For example, instead of running asm, the go command will run
		'cmd args /path/to/asm <arguments for asm>'.

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
	`,);

        private static readonly var concurrentGCBackendCompilationEnabledByDefault = true;



        private static void init()
        { 
            // break init cycle
            CmdBuild.Run = runBuild;
            CmdInstall.Run = runInstall;

            CmdBuild.Flag.BoolVar(ref cfg.BuildI, "i", false, "");
            CmdBuild.Flag.StringVar(ref cfg.BuildO, "o", "", "output file");

            CmdInstall.Flag.BoolVar(ref cfg.BuildI, "i", false, "");

            AddBuildFlags(CmdBuild);
            AddBuildFlags(CmdInstall);
        }

        // Note that flags consulted by other parts of the code
        // (for example, buildV) are in cmd/go/internal/cfg.

        private static slice<@string> forcedAsmflags = default;        private static slice<@string> forcedGcflags = default;        private static slice<@string> forcedLdflags = default;        private static slice<@string> forcedGccgoflags = default;

        public static toolchain BuildToolchain = new noToolchain();
        private static @string ldBuildmode = default;

        // buildCompiler implements flag.Var.
        // It implements Set by updating both
        // BuildToolchain and buildContext.Compiler.
        private partial struct buildCompiler
        {
        }

        private static error Set(this buildCompiler c, @string value)
        {
            switch (value)
            {
                case "gc": 
                    BuildToolchain = new gcToolchain();
                    break;
                case "gccgo": 
                    BuildToolchain = new gccgoToolchain();
                    break;
                default: 
                    return error.As(fmt.Errorf("unknown compiler %q", value));
                    break;
            }
            cfg.BuildToolchainName = value;
            cfg.BuildToolchainCompiler = BuildToolchain.compiler;
            cfg.BuildToolchainLinker = BuildToolchain.linker;
            cfg.BuildContext.Compiler = value;
            return error.As(null);
        }

        private static @string String(this buildCompiler c)
        {
            return cfg.BuildContext.Compiler;
        }

        private static void init()
        {
            switch (build.Default.Compiler)
            {
                case "gc": 

                case "gccgo": 
                    new buildCompiler().Set(build.Default.Compiler);
                    break;
            }
        }

        // addBuildFlags adds the flags common to the build, clean, get,
        // install, list, run, and test commands.
        public static void AddBuildFlags(ref base.Command cmd)
        {
            cmd.Flag.BoolVar(ref cfg.BuildA, "a", false, "");
            cmd.Flag.BoolVar(ref cfg.BuildN, "n", false, "");
            cmd.Flag.IntVar(ref cfg.BuildP, "p", cfg.BuildP, "");
            cmd.Flag.BoolVar(ref cfg.BuildV, "v", false, "");
            cmd.Flag.BoolVar(ref cfg.BuildX, "x", false, "");

            cmd.Flag.Var(ref load.BuildAsmflags, "asmflags", "");
            cmd.Flag.Var(new buildCompiler(), "compiler", "");
            cmd.Flag.StringVar(ref cfg.BuildBuildmode, "buildmode", "default", "");
            cmd.Flag.Var(ref load.BuildGcflags, "gcflags", "");
            cmd.Flag.Var(ref load.BuildGccgoflags, "gccgoflags", "");
            cmd.Flag.StringVar(ref cfg.BuildContext.InstallSuffix, "installsuffix", "", "");
            cmd.Flag.Var(ref load.BuildLdflags, "ldflags", "");
            cmd.Flag.BoolVar(ref cfg.BuildLinkshared, "linkshared", false, "");
            cmd.Flag.StringVar(ref cfg.BuildPkgdir, "pkgdir", "", "");
            cmd.Flag.BoolVar(ref cfg.BuildRace, "race", false, "");
            cmd.Flag.BoolVar(ref cfg.BuildMSan, "msan", false, "");
            cmd.Flag.Var((@base.StringsFlag.Value)(ref cfg.BuildContext.BuildTags), "tags", "");
            cmd.Flag.Var((@base.StringsFlag.Value)(ref cfg.BuildToolexec), "toolexec", "");
            cmd.Flag.BoolVar(ref cfg.BuildWork, "work", false, ""); 

            // Undocumented, unstable debugging flags.
            cmd.Flag.StringVar(ref cfg.DebugActiongraph, "debug-actiongraph", "", "");
            cmd.Flag.Var(ref load.DebugDeprecatedImportcfg, "debug-deprecated-importcfg", "");
        }

        // fileExtSplit expects a filename and returns the name
        // and ext (without the dot). If the file has no
        // extension, ext will be empty.
        private static (@string, @string) fileExtSplit(@string file)
        {
            var dotExt = filepath.Ext(file);
            name = file[..len(file) - len(dotExt)];
            if (dotExt != "")
            {
                ext = dotExt[1L..];
            }
            return;
        }

        private static slice<ref load.Package> pkgsMain(slice<ref load.Package> pkgs)
        {
            foreach (var (_, p) in pkgs)
            {
                if (p.Name == "main")
                {
                    res = append(res, p);
                }
            }
            return res;
        }

        private static slice<ref load.Package> pkgsNotMain(slice<ref load.Package> pkgs)
        {
            foreach (var (_, p) in pkgs)
            {
                if (p.Name != "main")
                {
                    res = append(res, p);
                }
            }
            return res;
        }

        private static slice<ref load.Package> oneMainPkg(slice<ref load.Package> pkgs)
        {
            if (len(pkgs) != 1L || pkgs[0L].Name != "main")
            {
                @base.Fatalf("-buildmode=%s requires exactly one main package", cfg.BuildBuildmode);
            }
            return pkgs;
        }

        private static Func<slice<ref load.Package>, slice<ref load.Package>> pkgsFilter = pkgs => pkgs;

        private static var runtimeVersion = runtime.Version();

        private static void runBuild(ref base.Command cmd, slice<@string> args)
        {
            BuildInit();
            Builder b = default;
            b.Init();

            var pkgs = load.PackagesForBuild(args);

            if (len(pkgs) == 1L && pkgs[0L].Name == "main" && cfg.BuildO == "")
            {
                _, cfg.BuildO = path.Split(pkgs[0L].ImportPath);
                cfg.BuildO += cfg.ExeSuffix;
            } 

            // Special case -o /dev/null by not writing at all.
            if (cfg.BuildO == os.DevNull)
            {
                cfg.BuildO = "";
            } 

            // sanity check some often mis-used options
            switch (cfg.BuildContext.Compiler)
            {
                case "gccgo": 
                    if (load.BuildGcflags.Present())
                    {
                        fmt.Println("go build: when using gccgo toolchain, please pass compiler flags using -gccgoflags, not -gcflags");
                    }
                    if (load.BuildLdflags.Present())
                    {
                        fmt.Println("go build: when using gccgo toolchain, please pass linker flags using -gccgoflags, not -ldflags");
                    }
                    break;
                case "gc": 
                    if (load.BuildGccgoflags.Present())
                    {
                        fmt.Println("go build: when using gc toolchain, please pass compile flags using -gcflags, and linker flags using -ldflags");
                    }
                    break;
            }

            var depMode = ModeBuild;
            if (cfg.BuildI)
            {
                depMode = ModeInstall;
            }
            pkgs = pkgsFilter(load.Packages(args));

            if (cfg.BuildO != "")
            {
                if (len(pkgs) > 1L)
                {
                    @base.Fatalf("go build: cannot use -o with multiple packages");
                }
                else if (len(pkgs) == 0L)
                {
                    @base.Fatalf("no packages to build");
                }
                var p = pkgs[0L];
                p.Target = cfg.BuildO;
                p.Stale = true; // must build - not up to date
                p.StaleReason = "build -o flag in use";
                var a = b.AutoAction(ModeInstall, depMode, p);
                b.Do(a);
                return;
            }
            a = ref new Action(Mode:"go build");
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs)
                {
                    p = __p;
                    a.Deps = append(a.Deps, b.AutoAction(ModeBuild, depMode, p));
                }

                p = p__prev1;
            }

            if (cfg.BuildBuildmode == "shared")
            {
                a = b.buildmodeShared(ModeBuild, depMode, args, pkgs, a);
            }
            b.Do(a);
        }

        public static base.Command CmdInstall = ref new base.Command(UsageLine:"install [-i] [build flags] [packages]",Short:"compile and install packages and dependencies",Long:`
Install compiles and installs the packages named by the import paths.

The -i flag installs the dependencies of the named packages as well.

For more about the build flags, see 'go help build'.
For more about specifying packages, see 'go help packages'.

See also: go build, go get, go clean.
	`,);

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
        private static (@string, error) libname(slice<@string> args, slice<ref load.Package> pkgs)
        {
            @string libname = default;
            Action<@string> appendName = arg =>
            {
                if (libname == "")
                {
                    libname = arg;
                }
                else
                {
                    libname += "," + arg;
                }
            }
;
            bool haveNonMeta = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in args)
                {
                    arg = __arg;
                    if (load.IsMetaPackage(arg))
                    {
                        appendName(arg);
                    }
                    else
                    {
                        haveNonMeta = true;
                    }
                }

                arg = arg__prev1;
            }

            if (len(libname) == 0L)
            { // non-meta packages only. use import paths
                if (len(args) == 1L && strings.HasSuffix(args[0L], "/..."))
                { 
                    // Special case of "foo/..." as mentioned above.
                    var arg = strings.TrimSuffix(args[0L], "/...");
                    if (build.IsLocalImport(arg))
                    {
                        var (cwd, _) = os.Getwd();
                        var (bp, _) = cfg.BuildContext.ImportDir(filepath.Join(cwd, arg), build.FindOnly);
                        if (bp.ImportPath != "" && bp.ImportPath != ".")
                        {
                            arg = bp.ImportPath;
                        }
                    }
                    appendName(strings.Replace(arg, "/", "-", -1L));
                }
                else
                {
                    foreach (var (_, pkg) in pkgs)
                    {
                        appendName(strings.Replace(pkg.ImportPath, "/", "-", -1L));
                    }
                }
            }
            else if (haveNonMeta)
            { // have both meta package and a non-meta one
                return ("", errors.New("mixing of meta and non-meta packages is not allowed"));
            } 
            // TODO(mwhudson): Needs to change for platforms that use different naming
            // conventions...
            return ("lib" + libname + ".so", null);
        }

        private static void runInstall(ref base.Command cmd, slice<@string> args)
        {
            BuildInit();
            InstallPackages(args, false);
        }

        public static void InstallPackages(slice<@string> args, bool forGet)
        {
            if (cfg.GOBIN != "" && !filepath.IsAbs(cfg.GOBIN))
            {
                @base.Fatalf("cannot install, GOBIN must be an absolute path");
            }
            var pkgs = pkgsFilter(load.PackagesForBuild(args));

            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs)
                {
                    p = __p;
                    if (p.Target == "" && (!p.Standard || p.ImportPath != "unsafe"))
                    {

                        if (p.Internal.GobinSubdir) 
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
            if (cfg.BuildI)
            {
                depMode = ModeInstall;
            }
            Action a = ref new Action(Mode:"go install");
            slice<ref Action> tools = default;
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs)
                {
                    p = __p; 
                    // During 'go get', don't attempt (and fail) to install packages with only tests.
                    // TODO(rsc): It's not clear why 'go get' should be different from 'go install' here. See #20760.
                    if (forGet && len(p.GoFiles) + len(p.CgoFiles) == 0L && len(p.TestGoFiles) + len(p.XTestGoFiles) > 0L)
                    {
                        continue;
                    } 
                    // If p is a tool, delay the installation until the end of the build.
                    // This avoids installing assemblers/compilers that are being executed
                    // by other steps in the build.
                    var a1 = b.AutoAction(ModeInstall, depMode, p);
                    if (load.InstallTargetDir(p) == load.ToTool)
                    {
                        a.Deps = append(a.Deps, a1.Deps);
                        a1.Deps = append(a1.Deps, a);
                        tools = append(tools, a1);
                        continue;
                    }
                    a.Deps = append(a.Deps, a1);
                }

                p = p__prev1;
            }

            if (len(tools) > 0L)
            {
                a = ref new Action(Mode:"go install (tools)",Deps:tools,);
            }
            if (cfg.BuildBuildmode == "shared")
            { 
                // Note: If buildmode=shared then only non-main packages
                // are present in the pkgs list, so all the special case code about
                // tools above did not apply, and a is just a simple Action
                // with a list of Deps, one per package named in pkgs,
                // the same as in runBuild.
                a = b.buildmodeShared(ModeInstall, ModeInstall, args, pkgs, a);
            }
            b.Do(a);
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
            if (len(args) == 0L && len(pkgs) == 1L && pkgs[0L].Name == "main")
            { 
                // Compute file 'go build' would have created.
                // If it exists and is an executable file, remove it.
                var (_, targ) = filepath.Split(pkgs[0L].ImportPath);
                targ += cfg.ExeSuffix;
                if (filepath.Join(pkgs[0L].Dir, targ) != pkgs[0L].Target)
                { // maybe $GOBIN is the current directory
                    var (fi, err) = os.Stat(targ);
                    if (err == null)
                    {
                        var m = fi.Mode();
                        if (m.IsRegular())
                        {
                            if (m & 0111L != 0L || cfg.Goos == "windows")
                            { // windows never sets executable bit
                                os.Remove(targ);
                            }
                        }
                    }
                }
            }
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
        public static slice<@string> FindExecCmd()
        {
            if (ExecCmd != null)
            {
                return ExecCmd;
            }
            ExecCmd = new slice<@string>(new @string[] {  }); // avoid work the second time
            if (cfg.Goos == runtime.GOOS && cfg.Goarch == runtime.GOARCH)
            {
                return ExecCmd;
            }
            var (path, err) = exec.LookPath(fmt.Sprintf("go_%s_%s_exec", cfg.Goos, cfg.Goarch));
            if (err == null)
            {
                ExecCmd = new slice<@string>(new @string[] { path });
            }
            return ExecCmd;
        }
    }
}}}}
