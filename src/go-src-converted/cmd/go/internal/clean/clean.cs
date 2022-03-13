// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package clean implements the ``go clean'' command.

// package clean -- go2cs converted at 2022 March 13 06:29:27 UTC
// import "cmd/go/internal/clean" ==> using clean = go.cmd.go.@internal.clean_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\clean\clean.go
namespace go.cmd.go.@internal;

using context = context_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;

using @base = cmd.go.@internal.@base_package;
using cache = cmd.go.@internal.cache_package;
using cfg = cmd.go.@internal.cfg_package;
using load = cmd.go.@internal.load_package;
using lockedfile = cmd.go.@internal.lockedfile_package;
using modfetch = cmd.go.@internal.modfetch_package;
using modload = cmd.go.@internal.modload_package;
using work = cmd.go.@internal.work_package;
using System;

public static partial class clean_package {

public static ptr<base.Command> CmdClean = addr(new base.Command(UsageLine:"go clean [clean flags] [build flags] [packages]",Short:"remove object files and cached files",Long:`
Clean removes object files from package source directories.
The go command builds most objects in a temporary directory,
so go clean is mainly concerned with object files left by other
tools or by manual invocations of go build.

If a package argument is given or the -i or -r flag is set,
clean removes the following files from each of the
source directories corresponding to the import paths:

	_obj/            old object directory, left from Makefiles
	_test/           old test directory, left from Makefiles
	_testmain.go     old gotest file, left from Makefiles
	test.out         old test log, left from Makefiles
	build.out        old test log, left from Makefiles
	*.[568ao]        object files, left from Makefiles

	DIR(.exe)        from go build
	DIR.test(.exe)   from go test -c
	MAINFILE(.exe)   from go build MAINFILE.go
	*.so             from SWIG

In the list, DIR represents the final path element of the
directory, and MAINFILE is the base name of any Go source
file in the directory that is not included when building
the package.

The -i flag causes clean to remove the corresponding installed
archive or binary (what 'go install' would create).

The -n flag causes clean to print the remove commands it would execute,
but not run them.

The -r flag causes clean to be applied recursively to all the
dependencies of the packages named by the import paths.

The -x flag causes clean to print remove commands as it executes them.

The -cache flag causes clean to remove the entire go build cache.

The -testcache flag causes clean to expire all test results in the
go build cache.

The -modcache flag causes clean to remove the entire module
download cache, including unpacked source code of versioned
dependencies.

For more about build flags, see 'go help build'.

For more about specifying packages, see 'go help packages'.
	`,));

private static bool cleanI = default;private static bool cleanR = default;private static bool cleanCache = default;private static bool cleanModcache = default;private static bool cleanTestcache = default;

private static void init() { 
    // break init cycle
    CmdClean.Run = runClean;

    CmdClean.Flag.BoolVar(_addr_cleanI, "i", false, "");
    CmdClean.Flag.BoolVar(_addr_cleanR, "r", false, "");
    CmdClean.Flag.BoolVar(_addr_cleanCache, "cache", false, "");
    CmdClean.Flag.BoolVar(_addr_cleanModcache, "modcache", false, "");
    CmdClean.Flag.BoolVar(_addr_cleanTestcache, "testcache", false, ""); 

    // -n and -x are important enough to be
    // mentioned explicitly in the docs but they
    // are part of the build flags.

    work.AddBuildFlags(CmdClean, work.DefaultBuildFlags);
}

private static void runClean(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;
 
    // golang.org/issue/29925: only load packages before cleaning if
    // either the flags and arguments explicitly imply a package,
    // or no other target (such as a cache) was requested to be cleaned.
    var cleanPkg = len(args) > 0 || cleanI || cleanR;
    if ((!modload.Enabled() || modload.HasModRoot()) && !cleanCache && !cleanModcache && !cleanTestcache) {
        cleanPkg = true;
    }
    if (cleanPkg) {
        foreach (var (_, pkg) in load.PackagesAndErrors(ctx, new load.PackageOpts(), args)) {
            clean(_addr_pkg);
        }
    }
    work.Builder b = default;
    b.Print = fmt.Print;

    if (cleanCache) {
        var dir = cache.DefaultDir();
        if (dir != "off") { 
            // Remove the cache subdirectories but not the top cache directory.
            // The top cache directory may have been created with special permissions
            // and not something that we want to remove. Also, we'd like to preserve
            // the access log for future analysis, even if the cache is cleared.
            var (subdirs, _) = filepath.Glob(filepath.Join(dir, "[0-9a-f][0-9a-f]"));
            var printedErrors = false;
            if (len(subdirs) > 0) {
                if (cfg.BuildN || cfg.BuildX) {
                    b.Showcmd("", "rm -r %s", strings.Join(subdirs, " "));
                }
                if (!cfg.BuildN) {
                    foreach (var (_, d) in subdirs) { 
                        // Only print the first error - there may be many.
                        // This also mimics what os.RemoveAll(dir) would do.
                        {
                            var err__prev5 = err;

                            var err = os.RemoveAll(d);

                            if (err != null && !printedErrors) {
                                printedErrors = true;
                                @base.Errorf("go clean -cache: %v", err);
                            }

                            err = err__prev5;

                        }
                    }
                }
            }
            var logFile = filepath.Join(dir, "log.txt");
            if (cfg.BuildN || cfg.BuildX) {
                b.Showcmd("", "rm -f %s", logFile);
            }
            if (!cfg.BuildN) {
                {
                    var err__prev4 = err;

                    err = os.RemoveAll(logFile);

                    if (err != null && !printedErrors) {
                        printedErrors = true;
                        @base.Errorf("go clean -cache: %v", err);
                    }

                    err = err__prev4;

                }
            }
        }
    }
    if (cleanTestcache && !cleanCache) { 
        // Instead of walking through the entire cache looking for test results,
        // we write a file to the cache indicating that all test results from before
        // right now are to be ignored.
        dir = cache.DefaultDir();
        if (dir != "off") {
            var (f, err) = lockedfile.Edit(filepath.Join(dir, "testexpire.txt"));
            if (err == null) {
                var now = time.Now().UnixNano();
                var (buf, _) = io.ReadAll(f);
                var (prev, _) = strconv.ParseInt(strings.TrimSpace(string(buf)), 10, 64);
                if (now > prev) {
                    err = f.Truncate(0);

                    if (err == null) {
                        _, err = f.Seek(0, 0);

                        if (err == null) {
                            _, err = fmt.Fprintf(f, "%d\n", now);
                        }
                    }
                }
                {
                    var closeErr = f.Close();

                    if (err == null) {
                        err = closeErr;
                    }

                }
            }
            if (err != null) {
                {
                    var (_, statErr) = os.Stat(dir);

                    if (!os.IsNotExist(statErr)) {
                        @base.Errorf("go clean -testcache: %v", err);
                    }

                }
            }
        }
    }
    if (cleanModcache) {
        if (cfg.GOMODCACHE == "") {
            @base.Fatalf("go clean -modcache: no module cache");
        }
        if (cfg.BuildN || cfg.BuildX) {
            b.Showcmd("", "rm -rf %s", cfg.GOMODCACHE);
        }
        if (!cfg.BuildN) {
            {
                var err__prev3 = err;

                err = modfetch.RemoveAll(cfg.GOMODCACHE);

                if (err != null) {
                    @base.Errorf("go clean -modcache: %v", err);
                }

                err = err__prev3;

            }
        }
    }
}

private static map cleaned = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<load.Package>, bool>{};

// TODO: These are dregs left by Makefile-based builds.
// Eventually, can stop deleting these.
private static map cleanDir = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"_test":true,"_obj":true,};

private static map cleanFile = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"_testmain.go":true,"test.out":true,"build.out":true,"a.out":true,};

private static map cleanExt = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{".5":true,".6":true,".8":true,".a":true,".o":true,".so":true,};

private static void clean(ptr<load.Package> _addr_p) {
    ref load.Package p = ref _addr_p.val;

    if (cleaned[p]) {
        return ;
    }
    cleaned[p] = true;

    if (p.Dir == "") {
        @base.Errorf("%v", p.Error);
        return ;
    }
    var (dirs, err) = os.ReadDir(p.Dir);
    if (err != null) {
        @base.Errorf("go clean %s: %v", p.Dir, err);
        return ;
    }
    work.Builder b = default;
    b.Print = fmt.Print;

    map packageFile = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    if (p.Name != "main") { 
        // Record which files are not in package main.
        // The others are.
        Action<slice<@string>> keep = list => {
            foreach (var (_, f) in list) {
                packageFile[f] = true;
            }
        };
        keep(p.GoFiles);
        keep(p.CgoFiles);
        keep(p.TestGoFiles);
        keep(p.XTestGoFiles);
    }
    var (_, elem) = filepath.Split(p.Dir);
    slice<@string> allRemove = default; 

    // Remove dir-named executable only if this is package main.
    if (p.Name == "main") {
        allRemove = append(allRemove, elem, elem + ".exe", p.DefaultExecName(), p.DefaultExecName() + ".exe");
    }
    allRemove = append(allRemove, elem + ".test", elem + ".test.exe", p.DefaultExecName() + ".test", p.DefaultExecName() + ".test.exe"); 

    // Remove a potential executable, test executable for each .go file in the directory that
    // is not part of the directory's package.
    {
        var dir__prev1 = dir;

        foreach (var (_, __dir) in dirs) {
            dir = __dir;
            var name = dir.Name();
            if (packageFile[name]) {
                continue;
            }
            if (dir.IsDir()) {
                continue;
            }
            if (strings.HasSuffix(name, "_test.go")) {
                var @base = name[..(int)len(name) - len("_test.go")];
                allRemove = append(allRemove, base + ".test", base + ".test.exe");
            }
            if (strings.HasSuffix(name, ".go")) { 
                // TODO(adg,rsc): check that this .go file is actually
                // in "package main", and therefore capable of building
                // to an executable file.
                @base = name[..(int)len(name) - len(".go")];
                allRemove = append(allRemove, base, base + ".exe");
            }
        }
        dir = dir__prev1;
    }

    if (cfg.BuildN || cfg.BuildX) {
        b.Showcmd(p.Dir, "rm -f %s", strings.Join(allRemove, " "));
    }
    map toRemove = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    {
        var name__prev1 = name;

        foreach (var (_, __name) in allRemove) {
            name = __name;
            toRemove[name] = true;
        }
        name = name__prev1;
    }

    {
        var dir__prev1 = dir;

        foreach (var (_, __dir) in dirs) {
            dir = __dir;
            name = dir.Name();
            if (dir.IsDir()) { 
                // TODO: Remove once Makefiles are forgotten.
                if (cleanDir[name]) {
                    if (cfg.BuildN || cfg.BuildX) {
                        b.Showcmd(p.Dir, "rm -r %s", name);
                        if (cfg.BuildN) {
                            continue;
                        }
                    }
                    {
                        var err = os.RemoveAll(filepath.Join(p.Dir, name));

                        if (err != null) {
                            @base.Errorf("go clean: %v", err);
                        }

                    }
                }
                continue;
            }
            if (cfg.BuildN) {
                continue;
            }
            if (cleanFile[name] || cleanExt[filepath.Ext(name)] || toRemove[name]) {
                removeFile(filepath.Join(p.Dir, name));
            }
        }
        dir = dir__prev1;
    }

    if (cleanI && p.Target != "") {
        if (cfg.BuildN || cfg.BuildX) {
            b.Showcmd("", "rm -f %s", p.Target);
        }
        if (!cfg.BuildN) {
            removeFile(p.Target);
        }
    }
    if (cleanR) {
        foreach (var (_, p1) in p.Internal.Imports) {
            clean(_addr_p1);
        }
    }
}

// removeFile tries to remove file f, if error other than file doesn't exist
// occurs, it will report the error.
private static void removeFile(@string f) {
    var err = os.Remove(f);
    if (err == null || os.IsNotExist(err)) {
        return ;
    }
    if (@base.ToolIsWindows) { 
        // Remove lingering ~ file from last attempt.
        {
            var (_, err2) = os.Stat(f + "~");

            if (err2 == null) {
                os.Remove(f + "~");
            } 
            // Try to move it out of the way. If the move fails,
            // which is likely, we'll try again the
            // next time we do an install of this binary.

        } 
        // Try to move it out of the way. If the move fails,
        // which is likely, we'll try again the
        // next time we do an install of this binary.
        {
            var err2 = os.Rename(f, f + "~");

            if (err2 == null) {
                os.Remove(f + "~");
                return ;
            }

        }
    }
    @base.Errorf("go clean: %v", err);
}

} // end clean_package
