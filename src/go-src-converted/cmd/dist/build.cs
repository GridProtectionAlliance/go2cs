// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:32:43 UTC
// Original source: C:\Go\src\cmd\dist\build.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        // Initialization for any invocation.

        // The usual variables.
        private static @string goarch = default;        private static @string gobin = default;        private static @string gohostarch = default;        private static @string gohostos = default;        private static @string goos = default;        private static @string goarm = default;        private static @string go386 = default;        private static @string gomips = default;        private static @string gomips64 = default;        private static @string goppc64 = default;        private static @string goroot = default;        private static @string goroot_final = default;        private static @string goextlinkenabled = default;        private static @string gogcflags = default;        private static @string goldflags = default;        private static @string workdir = default;        private static @string tooldir = default;        private static @string oldgoos = default;        private static @string oldgoarch = default;        private static @string exe = default;        private static map<@string, @string> defaultcc = default;        private static map<@string, @string> defaultcxx = default;        private static @string defaultcflags = default;        private static @string defaultldflags = default;        private static @string defaultpkgconfig = default;        private static @string defaultldso = default;        private static bool rebuildall = default;        private static bool defaultclang = default;        private static long vflag = default;

        // The known architectures.
        private static @string okgoarch = new slice<@string>(new @string[] { "386", "amd64", "arm", "arm64", "mips", "mipsle", "mips64", "mips64le", "ppc64", "ppc64le", "riscv64", "s390x", "sparc64", "wasm" });

        // The known operating systems.
        private static @string okgoos = new slice<@string>(new @string[] { "darwin", "dragonfly", "illumos", "js", "linux", "android", "solaris", "freebsd", "nacl", "netbsd", "openbsd", "plan9", "windows", "aix" });

        // find reports the first index of p in l[0:n], or else -1.
        private static long find(@string p, slice<@string> l)
        {
            foreach (var (i, s) in l)
            {
                if (p == s)
                {
                    return i;
                }

            }
            return -1L;

        }

        // xinit handles initialization of the various global state, like goroot and goarch.
        private static void xinit()
        {
            var b = os.Getenv("GOROOT");
            if (b == "")
            {
                fatalf("$GOROOT must be set");
            }

            goroot = filepath.Clean(b);
            {
                var modRoot = findModuleRoot(goroot);

                if (modRoot != "")
                {
                    fatalf("found go.mod file in %s: $GOROOT must not be inside a module", modRoot);
                }

            }


            b = os.Getenv("GOROOT_FINAL");
            if (b == "")
            {
                b = goroot;
            }

            goroot_final = b;

            b = os.Getenv("GOBIN");
            if (b == "")
            {
                b = pathf("%s/bin", goroot);
            }

            gobin = b;

            b = os.Getenv("GOOS");
            if (b == "")
            {
                b = gohostos;
            }

            goos = b;
            if (find(goos, okgoos) < 0L)
            {
                fatalf("unknown $GOOS %s", goos);
            }

            b = os.Getenv("GOARM");
            if (b == "")
            {
                b = xgetgoarm();
            }

            goarm = b;

            b = os.Getenv("GO386");
            if (b == "")
            {
                if (cansse2())
                {
                    b = "sse2";
                }
                else
                {
                    b = "387";
                }

            }

            go386 = b;

            b = os.Getenv("GOMIPS");
            if (b == "")
            {
                b = "hardfloat";
            }

            gomips = b;

            b = os.Getenv("GOMIPS64");
            if (b == "")
            {
                b = "hardfloat";
            }

            gomips64 = b;

            b = os.Getenv("GOPPC64");
            if (b == "")
            {
                b = "power8";
            }

            goppc64 = b;

            {
                var p = pathf("%s/src/all.bash", goroot);

                if (!isfile(p))
                {
                    fatalf("$GOROOT is not set correctly or not exported\n" + "\tGOROOT=%s\n" + "\t%s does not exist", goroot, p);
                }

            }


            b = os.Getenv("GOHOSTARCH");
            if (b != "")
            {
                gohostarch = b;
            }

            if (find(gohostarch, okgoarch) < 0L)
            {
                fatalf("unknown $GOHOSTARCH %s", gohostarch);
            }

            b = os.Getenv("GOARCH");
            if (b == "")
            {
                b = gohostarch;
            }

            goarch = b;
            if (find(goarch, okgoarch) < 0L)
            {
                fatalf("unknown $GOARCH %s", goarch);
            }

            b = os.Getenv("GO_EXTLINK_ENABLED");
            if (b != "")
            {
                if (b != "0" && b != "1")
                {
                    fatalf("unknown $GO_EXTLINK_ENABLED %s", b);
                }

                goextlinkenabled = b;

            }

            gogcflags = os.Getenv("BOOT_GO_GCFLAGS");
            goldflags = os.Getenv("BOOT_GO_LDFLAGS");

            @string cc = "gcc";
            @string cxx = "g++";
            if (defaultclang)
            {
                cc = "clang";
                cxx = "clang++";

            }

            defaultcc = compilerEnv("CC", cc);
            defaultcxx = compilerEnv("CXX", cxx);

            defaultcflags = os.Getenv("CFLAGS");
            defaultldflags = os.Getenv("LDFLAGS");

            b = os.Getenv("PKG_CONFIG");
            if (b == "")
            {
                b = "pkg-config";
            }

            defaultpkgconfig = b;

            defaultldso = os.Getenv("GO_LDSO"); 

            // For tools being invoked but also for os.ExpandEnv.
            os.Setenv("GO386", go386);
            os.Setenv("GOARCH", goarch);
            os.Setenv("GOARM", goarm);
            os.Setenv("GOHOSTARCH", gohostarch);
            os.Setenv("GOHOSTOS", gohostos);
            os.Setenv("GOOS", goos);
            os.Setenv("GOMIPS", gomips);
            os.Setenv("GOMIPS64", gomips64);
            os.Setenv("GOPPC64", goppc64);
            os.Setenv("GOROOT", goroot);
            os.Setenv("GOROOT_FINAL", goroot_final); 

            // Use a build cache separate from the default user one.
            // Also one that will be wiped out during startup, so that
            // make.bash really does start from a clean slate.
            os.Setenv("GOCACHE", pathf("%s/pkg/obj/go-build", goroot)); 

            // Make the environment more predictable.
            os.Setenv("LANG", "C");
            os.Setenv("LANGUAGE", "en_US.UTF8");

            workdir = xworkdir();
            xatexit(rmworkdir);

            tooldir = pathf("%s/pkg/tool/%s_%s", goroot, gohostos, gohostarch);

        }

        // compilerEnv returns a map from "goos/goarch" to the
        // compiler setting to use for that platform.
        // The entry for key "" covers any goos/goarch not explicitly set in the map.
        // For example, compilerEnv("CC", "gcc") returns the C compiler settings
        // read from $CC, defaulting to gcc.
        //
        // The result is a map because additional environment variables
        // can be set to change the compiler based on goos/goarch settings.
        // The following applies to all envNames but CC is assumed to simplify
        // the presentation.
        //
        // If no environment variables are set, we use def for all goos/goarch.
        // $CC, if set, applies to all goos/goarch but is overridden by the following.
        // $CC_FOR_TARGET, if set, applies to all goos/goarch except gohostos/gohostarch,
        // but is overridden by the following.
        // If gohostos=goos and gohostarch=goarch, then $CC_FOR_TARGET applies even for gohostos/gohostarch.
        // $CC_FOR_goos_goarch, if set, applies only to goos/goarch.
        private static map<@string, @string> compilerEnv(@string envName, @string def)
        {
            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"":def};

            {
                var env__prev1 = env;

                var env = os.Getenv(envName);

                if (env != "")
                {
                    m[""] = env;
                }

                env = env__prev1;

            }

            {
                var env__prev1 = env;

                env = os.Getenv(envName + "_FOR_TARGET");

                if (env != "")
                {
                    if (gohostos != goos || gohostarch != goarch)
                    {
                        m[gohostos + "/" + gohostarch] = m[""];
                    }

                    m[""] = env;

                }

                env = env__prev1;

            }


            foreach (var (_, goos) in okgoos)
            {
                foreach (var (_, goarch) in okgoarch)
                {
                    {
                        var env__prev1 = env;

                        env = os.Getenv(envName + "_FOR_" + goos + "_" + goarch);

                        if (env != "")
                        {
                            m[goos + "/" + goarch] = env;
                        }

                        env = env__prev1;

                    }

                }

            }
            return m;

        }

        // compilerEnvLookup returns the compiler settings for goos/goarch in map m.
        private static @string compilerEnvLookup(map<@string, @string> m, @string goos, @string goarch)
        {
            {
                var cc = m[goos + "/" + goarch];

                if (cc != "")
                {
                    return cc;
                }

            }

            return m[""];

        }

        // rmworkdir deletes the work directory.
        private static void rmworkdir()
        {
            if (vflag > 1L)
            {
                errprintf("rm -rf %s\n", workdir);
            }

            xremoveall(workdir);

        }

        // Remove trailing spaces.
        private static @string chomp(@string s)
        {
            return strings.TrimRight(s, " \t\r\n");
        }

        private static (@string, bool) branchtag(@string branch)
        {
            @string tag = default;
            bool precise = default;

            var log = run(goroot, CheckExit, "git", "log", "--decorate=full", "--format=format:%d", "master.." + branch);
            tag = branch;
            foreach (var (row, line) in strings.Split(log, "\n"))
            { 
                // Each line is either blank, or looks like
                //      (tag: refs/tags/go1.4rc2, refs/remotes/origin/release-branch.go1.4, refs/heads/release-branch.go1.4)
                // We need to find an element starting with refs/tags/.
                const @string s = (@string)" refs/tags/";

                var i = strings.Index(line, s);
                if (i < 0L)
                {
                    continue;
                } 
                // Trim off known prefix.
                line = line[i + len(s)..]; 
                // The tag name ends at a comma or paren.
                var j = strings.IndexAny(line, ",)");
                if (j < 0L)
                {
                    continue; // malformed line; ignore it
                }

                tag = line[..j];
                if (row == 0L)
                {
                    precise = true; // tag denotes HEAD
                }

                break;

            }
            return ;

        }

        // findgoversion determines the Go version to use in the version string.
        private static @string findgoversion()
        { 
            // The $GOROOT/VERSION file takes priority, for distributions
            // without the source repo.
            var path = pathf("%s/VERSION", goroot);
            if (isfile(path))
            {
                var b = chomp(readfile(path)); 
                // Commands such as "dist version > VERSION" will cause
                // the shell to create an empty VERSION file and set dist's
                // stdout to its fd. dist in turn looks at VERSION and uses
                // its content if available, which is empty at this point.
                // Only use the VERSION file if it is non-empty.
                if (b != "")
                { 
                    // Some builders cross-compile the toolchain on linux-amd64
                    // and then copy the toolchain to the target builder (say, linux-arm)
                    // for use there. But on non-release (devel) branches, the compiler
                    // used on linux-amd64 will be an amd64 binary, and the compiler
                    // shipped to linux-arm will be an arm binary, so they will have different
                    // content IDs (they are binaries for different architectures) and so the
                    // packages compiled by the running-on-amd64 compiler will appear
                    // stale relative to the running-on-arm compiler. Avoid this by setting
                    // the version string to something that doesn't begin with devel.
                    // Then the version string will be used in place of the content ID,
                    // and the packages will look up-to-date.
                    // TODO(rsc): Really the builders could be writing out a better VERSION file instead,
                    // but it is easier to change cmd/dist than to try to make changes to
                    // the builder while Brad is away.
                    if (strings.HasPrefix(b, "devel"))
                    {
                        {
                            var hostType = os.Getenv("META_BUILDLET_HOST_TYPE");

                            if (strings.Contains(hostType, "-cross"))
                            {
                                fmt.Fprintf(os.Stderr, "warning: changing VERSION from %q to %q\n", b, "builder " + hostType);
                                b = "builder " + hostType;
                            }

                        }

                    }

                    return b;

                }

            } 

            // The $GOROOT/VERSION.cache file is a cache to avoid invoking
            // git every time we run this command. Unlike VERSION, it gets
            // deleted by the clean command.
            path = pathf("%s/VERSION.cache", goroot);
            if (isfile(path))
            {
                return chomp(readfile(path));
            } 

            // Show a nicer error message if this isn't a Git repo.
            if (!isGitRepo())
            {
                fatalf("FAILED: not a Git repo; must put a VERSION file in $GOROOT");
            } 

            // Otherwise, use Git.
            // What is the current branch?
            var branch = chomp(run(goroot, CheckExit, "git", "rev-parse", "--abbrev-ref", "HEAD")); 

            // What are the tags along the current branch?
            @string tag = "devel";
            var precise = false; 

            // If we're on a release branch, use the closest matching tag
            // that is on the release branch (and not on the master branch).
            if (strings.HasPrefix(branch, "release-branch."))
            {
                tag, precise = branchtag(branch);
            }

            if (!precise)
            { 
                // Tag does not point at HEAD; add hash and date to version.
                tag += chomp(run(goroot, CheckExit, "git", "log", "-n", "1", "--format=format: +%h %cd", "HEAD"));

            } 

            // Cache version.
            writefile(tag, path, 0L);

            return tag;

        }

        // isGitRepo reports whether the working directory is inside a Git repository.
        private static bool isGitRepo()
        { 
            // NB: simply checking the exit code of `git rev-parse --git-dir` would
            // suffice here, but that requires deviating from the infrastructure
            // provided by `run`.
            var gitDir = chomp(run(goroot, 0L, "git", "rev-parse", "--git-dir"));
            if (!filepath.IsAbs(gitDir))
            {
                gitDir = filepath.Join(goroot, gitDir);
            }

            return isdir(gitDir);

        }

        /*
         * Initial tree setup.
         */

        // The old tools that no longer live in $GOBIN or $GOROOT/bin.
        private static @string oldtool = new slice<@string>(new @string[] { "5a", "5c", "5g", "5l", "6a", "6c", "6g", "6l", "8a", "8c", "8g", "8l", "9a", "9c", "9g", "9l", "6cov", "6nm", "6prof", "cgo", "ebnflint", "goapi", "gofix", "goinstall", "gomake", "gopack", "gopprof", "gotest", "gotype", "govet", "goyacc", "quietgcc" });

        // Unreleased directories (relative to $GOROOT) that should
        // not be in release branches.
        private static @string unreleased = new slice<@string>(new @string[] { "src/cmd/newlink", "src/cmd/objwriter", "src/debug/goobj", "src/old" });

        // setup sets up the tree for the initial build.
        private static void setup()
        { 
            // Create bin directory.
            {
                var p__prev1 = p;

                var p = pathf("%s/bin", goroot);

                if (!isdir(p))
                {
                    xmkdir(p);
                } 

                // Create package directory.

                p = p__prev1;

            } 

            // Create package directory.
            {
                var p__prev1 = p;

                p = pathf("%s/pkg", goroot);

                if (!isdir(p))
                {
                    xmkdir(p);
                }

                p = p__prev1;

            }


            p = pathf("%s/pkg/%s_%s", goroot, gohostos, gohostarch);
            if (rebuildall)
            {
                xremoveall(p);
            }

            xmkdirall(p);

            if (goos != gohostos || goarch != gohostarch)
            {
                p = pathf("%s/pkg/%s_%s", goroot, goos, goarch);
                if (rebuildall)
                {
                    xremoveall(p);
                }

                xmkdirall(p);

            } 

            // Create object directory.
            // We used to use it for C objects.
            // Now we use it for the build cache, to separate dist's cache
            // from any other cache the user might have.
            p = pathf("%s/pkg/obj/go-build", goroot);
            if (rebuildall)
            {
                xremoveall(p);
            }

            xmkdirall(p);
            xatexit(() =>
            {
                xremoveall(p);
            }); 

            // Create tool directory.
            // We keep it in pkg/, just like the object directory above.
            if (rebuildall)
            {
                xremoveall(tooldir);
            }

            xmkdirall(tooldir); 

            // Remove tool binaries from before the tool/gohostos_gohostarch
            xremoveall(pathf("%s/bin/tool", goroot)); 

            // Remove old pre-tool binaries.
            {
                var old__prev1 = old;

                foreach (var (_, __old) in oldtool)
                {
                    old = __old;
                    xremove(pathf("%s/bin/%s", goroot, old));
                } 

                // If $GOBIN is set and has a Go compiler, it must be cleaned.

                old = old__prev1;
            }

            foreach (var (_, char) in "56789")
            {
                if (isfile(pathf("%s/%c%s", gobin, char, "g")))
                {
                    {
                        var old__prev2 = old;

                        foreach (var (_, __old) in oldtool)
                        {
                            old = __old;
                            xremove(pathf("%s/%s", gobin, old));
                        }

                        old = old__prev2;
                    }

                    break;

                }

            } 

            // For release, make sure excluded things are excluded.
            var goversion = findgoversion();
            if (strings.HasPrefix(goversion, "release.") || (strings.HasPrefix(goversion, "go") && !strings.Contains(goversion, "beta")))
            {
                foreach (var (_, dir) in unreleased)
                {
                    {
                        var p__prev2 = p;

                        p = pathf("%s/%s", goroot, dir);

                        if (isdir(p))
                        {
                            fatalf("%s should not exist in release build", p);
                        }

                        p = p__prev2;

                    }

                }

            }

        }

        /*
         * Tool building
         */

        // deptab lists changes to the default dependencies for a given prefix.
        // deps ending in /* read the whole directory; deps beginning with -
        // exclude files with that prefix.
        // Note that this table applies only to the build of cmd/go,
        // after the main compiler bootstrap.


        // depsuffix records the allowed suffixes for source files.
        private static @string depsuffix = new slice<@string>(new @string[] { ".s", ".go" });

        // gentab records how to generate some trivial files.


        // installed maps from a dir name (as given to install) to a chan
        // closed when the dir's package is installed.
        private static var installed = make_map<@string, channel<object>>();
        private static sync.Mutex installedMu = default;

        private static void install(@string dir)
        {
            startInstall(dir).Receive();
        }

        private static channel<object> startInstall(@string dir)
        {
            installedMu.Lock();
            var ch = installed[dir];
            if (ch == null)
            {
                ch = make_channel<object>();
                installed[dir] = ch;
                go_(() => runInstall(dir, ch));
            }

            installedMu.Unlock();
            return ch;

        }

        // runInstall installs the library, package, or binary associated with dir,
        // which is relative to $GOROOT/src.
        private static void runInstall(@string pkg, channel<object> ch) => func((defer, _, __) =>
        {
            if (pkg == "net" || pkg == "os/user" || pkg == "crypto/x509")
            {
                fatalf("go_bootstrap cannot depend on cgo package %s", pkg);
            }

            defer(close(ch));

            if (pkg == "unsafe")
            {
                return ;
            }

            if (vflag > 0L)
            {
                if (goos != gohostos || goarch != gohostarch)
                {
                    errprintf("%s (%s/%s)\n", pkg, goos, goarch);
                }
                else
                {
                    errprintf("%s\n", pkg);
                }

            }

            var workdir = pathf("%s/%s", workdir, pkg);
            xmkdirall(workdir);

            slice<@string> clean = default;
            defer(() =>
            {
                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in clean)
                    {
                        name = __name;
                        xremove(name);
                    }

                    name = name__prev1;
                }
            }()); 

            // dir = full path to pkg.
            var dir = pathf("%s/src/%s", goroot, pkg);
            var name = filepath.Base(dir); 

            // ispkg predicts whether the package should be linked as a binary, based
            // on the name. There should be no "main" packages in vendor, since
            // 'go mod vendor' will only copy imported packages there.
            var ispkg = !strings.HasPrefix(pkg, "cmd/") || strings.Contains(pkg, "/internal/") || strings.Contains(pkg, "/vendor/"); 

            // Start final link command line.
            // Note: code below knows that link.p[targ] is the target.
            slice<@string> link = default;            long targ = default;            bool ispackcmd = default;
            if (ispkg)
            { 
                // Go library (package).
                ispackcmd = true;
                link = new slice<@string>(new @string[] { "pack", packagefile(pkg) });
                targ = len(link) - 1L;
                xmkdirall(filepath.Dir(link[targ]));

            }
            else
            { 
                // Go command.
                var elem = name;
                if (elem == "go")
                {
                    elem = "go_bootstrap";
                }

                link = new slice<@string>(new @string[] { pathf("%s/link",tooldir) });
                if (goos == "android")
                {
                    link = append(link, "-buildmode=pie");
                }

                if (goldflags != "")
                {
                    link = append(link, goldflags);
                }

                link = append(link, "-extld=" + compilerEnvLookup(defaultcc, goos, goarch));
                link = append(link, "-o", pathf("%s/%s%s", tooldir, elem, exe));
                targ = len(link) - 1L;

            }

            var ttarg = mtime(link[targ]); 

            // Gather files that are sources for this target.
            // Everything in that directory, and any target-specific
            // additions.
            var files = xreaddir(dir); 

            // Remove files beginning with . or _,
            // which are likely to be editor temporary files.
            // This is the same heuristic build.ScanDir uses.
            // There do exist real C files beginning with _,
            // so limit that check to just Go files.
            files = filter(files, p =>
            {
                return !strings.HasPrefix(p, ".") && (!strings.HasPrefix(p, "_") || !strings.HasSuffix(p, ".go"));
            });

            foreach (var (_, dt) in deptab)
            {
                if (pkg == dt.prefix || strings.HasSuffix(dt.prefix, "/") && strings.HasPrefix(pkg, dt.prefix))
                {
                    {
                        var p__prev2 = p;

                        foreach (var (_, __p) in dt.dep)
                        {
                            p = __p;
                            p = os.ExpandEnv(p);
                            files = append(files, p);
                        }

                        p = p__prev2;
                    }
                }

            }
            files = uniq(files); 

            // Convert to absolute paths.
            {
                var i__prev1 = i;
                var p__prev1 = p;

                foreach (var (__i, __p) in files)
                {
                    i = __i;
                    p = __p;
                    if (!filepath.IsAbs(p))
                    {
                        files[i] = pathf("%s/%s", dir, p);
                    }

                } 

                // Is the target up-to-date?

                i = i__prev1;
                p = p__prev1;
            }

            slice<@string> gofiles = default;            slice<@string> sfiles = default;            slice<@string> missing = default;

            var stale = rebuildall;
            files = filter(files, p =>
            {
                foreach (var (_, suf) in depsuffix)
                {
                    if (strings.HasSuffix(p, suf))
                    {
                        goto ok;
                    }

                }
                return false;
ok:
                var t = mtime(p);
                if (!t.IsZero() && !strings.HasSuffix(p, ".a") && !shouldbuild(p, pkg))
                {
                    return false;
                }

                if (strings.HasSuffix(p, ".go"))
                {
                    gofiles = append(gofiles, p);
                }
                else if (strings.HasSuffix(p, ".s"))
                {
                    sfiles = append(sfiles, p);
                }

                if (t.After(ttarg))
                {
                    stale = true;
                }

                if (t.IsZero())
                {
                    missing = append(missing, p);
                }

                return true;

            }); 

            // If there are no files to compile, we're done.
            if (len(files) == 0L)
            {
                return ;
            }

            if (!stale)
            {
                return ;
            } 

            // For package runtime, copy some files into the work space.
            if (pkg == "runtime")
            {
                xmkdirall(pathf("%s/pkg/include", goroot)); 
                // For use by assembly and C files.
                copyfile(pathf("%s/pkg/include/textflag.h", goroot), pathf("%s/src/runtime/textflag.h", goroot), 0L);
                copyfile(pathf("%s/pkg/include/funcdata.h", goroot), pathf("%s/src/runtime/funcdata.h", goroot), 0L);
                copyfile(pathf("%s/pkg/include/asm_ppc64x.h", goroot), pathf("%s/src/runtime/asm_ppc64x.h", goroot), 0L);

            } 

            // Generate any missing files; regenerate existing ones.
            {
                var p__prev1 = p;

                foreach (var (_, __p) in files)
                {
                    p = __p;
                    elem = filepath.Base(p);
                    {
                        var gt__prev2 = gt;

                        foreach (var (_, __gt) in gentab)
                        {
                            gt = __gt;
                            if (gt.gen == null)
                            {
                                continue;
                            }

                            if (strings.HasPrefix(elem, gt.nameprefix))
                            {
                                if (vflag > 1L)
                                {
                                    errprintf("generate %s\n", p);
                                }

                                gt.gen(dir, p); 
                                // Do not add generated file to clean list.
                                // In runtime, we want to be able to
                                // build the package with the go tool,
                                // and it assumes these generated files already
                                // exist (it does not know how to build them).
                                // The 'clean' command can remove
                                // the generated files.
                                goto built;

                            }

                        } 
                        // Did not rebuild p.

                        gt = gt__prev2;
                    }

                    if (find(p, missing) >= 0L)
                    {
                        fatalf("missing file %s", p);
                    }

built:
                    var importMap = make_map<@string, @string>();
                    {
                        var p__prev2 = p;

                        foreach (var (_, __p) in gofiles)
                        {
                            p = __p;
                            {
                                var imp__prev3 = imp;

                                foreach (var (_, __imp) in readimports(p))
                                {
                                    imp = __imp;
                                    importMap[imp] = resolveVendor(imp, dir);
                                }

                                imp = imp__prev3;
                            }
                        }

                        p = p__prev2;
                    }

                    var sortedImports = make_slice<@string>(0L, len(importMap));
                    {
                        var imp__prev2 = imp;

                        foreach (var (__imp) in importMap)
                        {
                            imp = __imp;
                            sortedImports = append(sortedImports, imp);
                        }

                        imp = imp__prev2;
                    }

                    sort.Strings(sortedImports);

                    {
                        var dep__prev2 = dep;

                        foreach (var (_, __dep) in importMap)
                        {
                            dep = __dep;
                            startInstall(dep);
                        }

                        dep = dep__prev2;
                    }

                    {
                        var dep__prev2 = dep;

                        foreach (var (_, __dep) in importMap)
                        {
                            dep = __dep;
                            install(dep);
                        }

                        dep = dep__prev2;
                    }

                    if (goos != gohostos || goarch != gohostarch)
                    { 
                        // We've generated the right files; the go command can do the build.
                        if (vflag > 1L)
                        {
                            errprintf("skip build for cross-compile %s\n", pkg);
                        }

                        return ;

                    }

                    @string asmArgs = new slice<@string>(new @string[] { pathf("%s/asm",tooldir), "-I", workdir, "-I", pathf("%s/pkg/include",goroot), "-D", "GOOS_"+goos, "-D", "GOARCH_"+goarch, "-D", "GOOS_GOARCH_"+goos+"_"+goarch });
                    if (goarch == "mips" || goarch == "mipsle")
                    { 
                        // Define GOMIPS_value from gomips.
                        asmArgs = append(asmArgs, "-D", "GOMIPS_" + gomips);

                    }

                    if (goarch == "mips64" || goarch == "mips64le")
                    { 
                        // Define GOMIPS64_value from gomips64.
                        asmArgs = append(asmArgs, "-D", "GOMIPS64_" + gomips64);

                    }

                    var goasmh = pathf("%s/go_asm.h", workdir); 

                    // Collect symabis from assembly code.
                    @string symabis = default;
                    if (len(sfiles) > 0L)
                    {
                        symabis = pathf("%s/symabis", workdir);
                        ref sync.WaitGroup wg = ref heap(out ptr<sync.WaitGroup> _addr_wg);
                        var asmabis = append(asmArgs.slice(-1, len(asmArgs), len(asmArgs)), "-gensymabis", "-o", symabis);
                        asmabis = append(asmabis, sfiles);
                        {
                            var err__prev2 = err;

                            var err = ioutil.WriteFile(goasmh, null, 0666L);

                            if (err != null)
                            {
                                fatalf("cannot write empty go_asm.h: %s", err);
                            }

                            err = err__prev2;

                        }

                        bgrun(_addr_wg, dir, asmabis);
                        bgwait(_addr_wg);

                    } 

                    // Build an importcfg file for the compiler.
                    ptr<bytes.Buffer> buf = addr(new bytes.Buffer());
                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in sortedImports)
                        {
                            imp = __imp;
                            if (imp == "unsafe")
                            {
                                continue;
                            }

                            var dep = importMap[imp];
                            if (imp != dep)
                            {
                                fmt.Fprintf(buf, "importmap %s=%s\n", imp, dep);
                            }

                            fmt.Fprintf(buf, "packagefile %s=%s\n", dep, packagefile(dep));

                        }

                        imp = imp__prev2;
                    }

                    var importcfg = pathf("%s/importcfg", workdir);
                    {
                        var err__prev1 = err;

                        err = ioutil.WriteFile(importcfg, buf.Bytes(), 0666L);

                        if (err != null)
                        {
                            fatalf("cannot write importcfg file: %v", err);
                        }

                        err = err__prev1;

                    }


                    @string archive = default; 
                    // The next loop will compile individual non-Go files.
                    // Hand the Go files to the compiler en masse.
                    // For packages containing assembly, this writes go_asm.h, which
                    // the assembly files will need.
                    var pkgName = pkg;
                    if (strings.HasPrefix(pkg, "cmd/") && strings.Count(pkg, "/") == 1L)
                    {
                        pkgName = "main";
                    }

                    var b = pathf("%s/_go_.a", workdir);
                    clean = append(clean, b);
                    if (!ispackcmd)
                    {
                        link = append(link, b);
                    }
                    else
                    {
                        archive = b;
                    } 

                    // Compile Go code.
                    @string compile = new slice<@string>(new @string[] { pathf("%s/compile",tooldir), "-std", "-pack", "-o", b, "-p", pkgName, "-importcfg", importcfg });
                    if (gogcflags != "")
                    {
                        compile = append(compile, strings.Fields(gogcflags));
                    }

                    if (pkg == "runtime")
                    {
                        compile = append(compile, "-+");
                    }

                    if (len(sfiles) > 0L)
                    {
                        compile = append(compile, "-asmhdr", goasmh);
                    }

                    if (symabis != "")
                    {
                        compile = append(compile, "-symabis", symabis);
                    }

                    if (goos == "android")
                    {
                        compile = append(compile, "-shared");
                    }

                    compile = append(compile, gofiles);
                    wg = default; 
                    // We use bgrun and immediately wait for it instead of calling run() synchronously.
                    // This executes all jobs through the bgwork channel and allows the process
                    // to exit cleanly in case an error occurs.
                    bgrun(_addr_wg, dir, compile);
                    bgwait(_addr_wg); 

                    // Compile the files.
                    {
                        var p__prev2 = p;

                        foreach (var (_, __p) in sfiles)
                        {
                            p = __p; 
                            // Assembly file for a Go package.
                            compile = asmArgs.slice(-1, len(asmArgs), len(asmArgs));

                            var doclean = true;
                            b = pathf("%s/%s", workdir, filepath.Base(p)); 

                            // Change the last character of the output file (which was c or s).
                            b = b[..len(b) - 1L] + "o";
                            compile = append(compile, "-o", b, p);
                            bgrun(_addr_wg, dir, compile);

                            link = append(link, b);
                            if (doclean)
                            {
                                clean = append(clean, b);
                            }

                        }

                        p = p__prev2;
                    }

                    bgwait(_addr_wg);

                    if (ispackcmd)
                    {
                        xremove(link[targ]);
                        dopack(link[targ], archive, link[targ + 1L..]);
                        return ;
                    } 

                    // Remove target before writing it.
                    xremove(link[targ]);
                    bgrun(_addr_wg, "", link);
                    bgwait(_addr_wg);

                }

                // packagefile returns the path to a compiled .a file for the given package
                // path. Paths may need to be resolved with resolveVendor first.

                p = p__prev1;
            }

            string;

            {
                return pathf("%s/pkg/%s_%s/%s.a", goroot, goos, goarch, pkg);
            }

            // matchfield reports whether the field (x,y,z) matches this build.
            // all the elements in the field must be satisfied.
            bool;

            {
                foreach (var (_, tag) in strings.Split(f, ","))
                {
                    if (!matchtag(tag))
                    {
                        return false;
                    }

                }
                return true;

            }

            // matchtag reports whether the tag (x or !x) matches this build.
            bool;

            {
                if (tag == "")
                {
                    return false;
                }

                if (tag[0L] == '!')
                {
                    if (len(tag) == 1L || tag[1L] == '!')
                    {
                        return false;
                    }

                    return !matchtag(tag[1L..]);

                }

                return tag == "gc" || tag == goos || tag == goarch || tag == "cmd_go_bootstrap" || tag == "go1.1" || (goos == "android" && tag == "linux") || (goos == "illumos" && tag == "solaris");

            }

            // shouldbuild reports whether we should build this file.
            // It applies the same rules that are used with context tags
            // in package go/build, except it's less picky about the order
            // of GOOS and GOARCH.
            // We also allow the special tag cmd_go_bootstrap.
            // See ../go/bootstrap.go and package go/build.
            pkg;

            bool;

            { 
                // Check file name for GOOS or GOARCH.
                name = filepath.Base(file);
                Func<slice<@string>, @string, bool> excluded = (list, ok) =>
                {
                    foreach (var (_, x) in list)
                    {
                        if (x == ok || (ok == "android" && x == "linux") || (ok == "illumos" && x == "solaris"))
                        {
                            continue;
                        }

                        var i = strings.Index(name, x);
                        if (i <= 0L || name[i - 1L] != '_')
                        {
                            continue;
                        }

                        i += len(x);
                        if (i == len(name) || name[i] == '.' || name[i] == '_')
                        {
                            return true;
                        }

                    }
                    return false;

                }
;
                if (excluded(okgoos, goos) || excluded(okgoarch, goarch))
                {
                    return false;
                } 

                // Omit test files.
                if (strings.Contains(name, "_test"))
                {
                    return false;
                } 

                // Check file contents for // +build lines.
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in strings.Split(readfile(file), "\n"))
                    {
                        p = __p;
                        p = strings.TrimSpace(p);
                        if (p == "")
                        {
                            continue;
                        }

                        var code = p;
                        i = strings.Index(code, "//");
                        if (i > 0L)
                        {
                            code = strings.TrimSpace(code[..i]);
                        }

                        if (code == "package documentation")
                        {
                            return false;
                        }

                        if (code == "package main" && pkg != "cmd/go" && pkg != "cmd/cgo")
                        {
                            return false;
                        }

                        if (!strings.HasPrefix(p, "//"))
                        {
                            break;
                        }

                        if (!strings.Contains(p, "+build"))
                        {
                            continue;
                        }

                        var fields = strings.Fields(p[2L..]);
                        if (len(fields) < 1L || fields[0L] != "+build")
                        {
                            continue;
                        }

                        {
                            var p__prev2 = p;

                            foreach (var (_, __p) in fields[1L..])
                            {
                                p = __p;
                                if (matchfield(p))
                                {
                                    goto fieldmatch;
                                }

                            }

                            p = p__prev2;
                        }

                        return false;
fieldmatch:
                        return true;

                    }

                    // copy copies the file src to dst, via memory (so only good for small files).

                    p = p__prev1;
                }

                src;

                flag;

                {
                    if (vflag > 1L)
                    {
                        errprintf("cp %s %s\n", src, dst);
                    }

                    writefile(readfile(src), dst, flag);

                }

                // dopack copies the package src to dst,
                // appending the files listed in extra.
                // The archive format is the traditional Unix ar format.
                src;

                string <missing '='> extra;

                {
                    var bdst = bytes.NewBufferString(readfile(src));
                    foreach (var (_, file) in extra)
                    {
                        b = readfile(file); 
                        // find last path element for archive member name
                        i = strings.LastIndex(file, "/") + 1L;
                        var j = strings.LastIndex(file, "\\") + 1L;
                        if (i < j)
                        {
                            i = j;
                        }

                        fmt.Fprintf(bdst, "%-16.16s%-12d%-6d%-6d%-8o%-10d`\n", file[i..], 0L, 0L, 0L, 0644L, len(b));
                        bdst.WriteString(b);
                        if (len(b) & 1L != 0L)
                        {
                            bdst.WriteByte(0L);
                        }

                    }
                    writefile(bdst.String(), dst, 0L);

                }
                @string runtimegen = new slice<@string>(new @string[] { "zaexperiment.h", "zversion.go" });

                // cleanlist is a list of packages with generated files and commands.
                @string cleanlist = new slice<@string>(new @string[] { "runtime/internal/sys", "cmd/cgo", "cmd/go/internal/cfg", "go/build" });

                clean();

                {
                    {
                        var name__prev1 = name;

                        foreach (var (_, __name) in cleanlist)
                        {
                            name = __name;
                            var path = pathf("%s/src/%s", goroot, name); 
                            // Remove generated files.
                            {
                                var elem__prev2 = elem;

                                foreach (var (_, __elem) in xreaddir(path))
                                {
                                    elem = __elem;
                                    {
                                        var gt__prev3 = gt;

                                        foreach (var (_, __gt) in gentab)
                                        {
                                            gt = __gt;
                                            if (strings.HasPrefix(elem, gt.nameprefix))
                                            {
                                                xremove(pathf("%s/%s", path, elem));
                                            }

                                        }

                                        gt = gt__prev3;
                                    }
                                } 
                                // Remove generated binary named for directory.

                                elem = elem__prev2;
                            }

                            if (strings.HasPrefix(name, "cmd/"))
                            {
                                xremove(pathf("%s/%s", path, name[4L..]));
                            }

                        } 

                        // remove runtimegen files.

                        name = name__prev1;
                    }

                    path = pathf("%s/src/runtime", goroot);
                    {
                        var elem__prev1 = elem;

                        foreach (var (_, __elem) in runtimegen)
                        {
                            elem = __elem;
                            xremove(pathf("%s/%s", path, elem));
                        }

                        elem = elem__prev1;
                    }

                    if (rebuildall)
                    { 
                        // Remove object tree.
                        xremoveall(pathf("%s/pkg/obj/%s_%s", goroot, gohostos, gohostarch)); 

                        // Remove installed packages and tools.
                        xremoveall(pathf("%s/pkg/%s_%s", goroot, gohostos, gohostarch));
                        xremoveall(pathf("%s/pkg/%s_%s", goroot, goos, goarch));
                        xremoveall(pathf("%s/pkg/%s_%s_race", goroot, gohostos, gohostarch));
                        xremoveall(pathf("%s/pkg/%s_%s_race", goroot, goos, goarch));
                        xremoveall(tooldir); 

                        // Remove cached version info.
                        xremove(pathf("%s/VERSION.cache", goroot));

                    }

                }

                /*
                 * command implementations
                 */

                // The env command prints the default environment.
                cmdenv();

                {
                    path = flag.Bool("p", false, "emit updated PATH");
                    var plan9 = flag.Bool("9", false, "emit plan 9 syntax");
                    var windows = flag.Bool("w", false, "emit windows syntax");
                    xflagparse(0L);

                    @string format = "%s=\"%s\"\n";

                    if (plan9.val) 
                        format = "%s='%s'\n";
                    else if (windows.val) 
                        format = "set %s=%s\r\n";
                                        xprintf(format, "GOARCH", goarch);
                    xprintf(format, "GOBIN", gobin);
                    xprintf(format, "GOCACHE", os.Getenv("GOCACHE"));
                    xprintf(format, "GODEBUG", os.Getenv("GODEBUG"));
                    xprintf(format, "GOHOSTARCH", gohostarch);
                    xprintf(format, "GOHOSTOS", gohostos);
                    xprintf(format, "GOOS", goos);
                    xprintf(format, "GOPROXY", os.Getenv("GOPROXY"));
                    xprintf(format, "GOROOT", goroot);
                    xprintf(format, "GOTMPDIR", os.Getenv("GOTMPDIR"));
                    xprintf(format, "GOTOOLDIR", tooldir);
                    if (goarch == "arm")
                    {
                        xprintf(format, "GOARM", goarm);
                    }

                    if (goarch == "386")
                    {
                        xprintf(format, "GO386", go386);
                    }

                    if (goarch == "mips" || goarch == "mipsle")
                    {
                        xprintf(format, "GOMIPS", gomips);
                    }

                    if (goarch == "mips64" || goarch == "mips64le")
                    {
                        xprintf(format, "GOMIPS64", gomips64);
                    }

                    if (goarch == "ppc64" || goarch == "ppc64le")
                    {
                        xprintf(format, "GOPPC64", goppc64);
                    }

                    if (path.val)
                    {
                        @string sep = ":";
                        if (gohostos == "windows")
                        {
                            sep = ";";
                        }

                        xprintf(format, "PATH", fmt.Sprintf("%s%s%s", gobin, sep, os.Getenv("PATH")));

                    }

                }
                var timeLogEnabled = os.Getenv("GOBUILDTIMELOGFILE") != "";                sync.Mutex timeLogMu = default;                ptr<os.File> timeLogFile;                time.Time timeLogStart = default;

                name;

                {
                    if (!timeLogEnabled)
                    {
                        return ;
                    }

                    timeLogMu.Lock();
                    defer(timeLogMu.Unlock());
                    if (timeLogFile == null)
                    {
                        var (f, err) = os.OpenFile(os.Getenv("GOBUILDTIMELOGFILE"), os.O_RDWR | os.O_APPEND, 0666L);
                        if (err != null)
                        {
                            log.Fatal(err);
                        }

                        buf = make_slice<byte>(100L);
                        var (n, _) = f.Read(buf);
                        var s = string(buf[..n]);
                        {
                            var i__prev2 = i;

                            i = strings.Index(s, "\n");

                            if (i >= 0L)
                            {
                                s = s[..i];
                            }

                            i = i__prev2;

                        }

                        i = strings.Index(s, " start");
                        if (i < 0L)
                        {
                            log.Fatalf("time log %s does not begin with start line", os.Getenv("GOBULDTIMELOGFILE"));
                        }

                        var (t, err) = time.Parse(time.UnixDate, s[..i]);
                        if (err != null)
                        {
                            log.Fatalf("cannot parse time log line %q: %v", s, err);
                        }

                        timeLogStart = t;
                        timeLogFile = f;

                    }

                    t = time.Now();
                    fmt.Fprintf(timeLogFile, "%s %+.1fs %s %s\n", t.Format(time.UnixDate), t.Sub(timeLogStart).Seconds(), op, name);

                }
                @string toolchain = new slice<@string>(new @string[] { "cmd/asm", "cmd/cgo", "cmd/compile", "cmd/link" });

                // The bootstrap command runs a build from scratch,
                // stopping at having installed the go_bootstrap command.
                //
                // WARNING: This command runs after cmd/dist is built with Go 1.4.
                // It rebuilds and installs cmd/dist with the new toolchain, so other
                // commands (like "go tool dist test" in run.bash) can rely on bug fixes
                // made since Go 1.4, but this function cannot. In particular, the uses
                // of os/exec in this function cannot assume that
                //    cmd.Env = append(os.Environ(), "X=Y")
                // sets $X to Y in the command's environment. That guarantee was
                // added after Go 1.4, and in fact in Go 1.4 it was typically the opposite:
                // if $X was already present in os.Environ(), most systems preferred
                // that setting, not the new one.
                cmdbootstrap();

                {
                    timelog("start", "dist bootstrap");
                    defer(timelog("end", "dist bootstrap"));

                    ref bool noBanner = ref heap(out ptr<bool> _addr_noBanner);
                    ref bool debug = ref heap(out ptr<bool> _addr_debug);
                    flag.BoolVar(_addr_rebuildall, "a", rebuildall, "rebuild all");
                    flag.BoolVar(_addr_debug, "d", debug, "enable debugging of bootstrap process");
                    flag.BoolVar(_addr_noBanner, "no-banner", noBanner, "do not print banner");

                    xflagparse(0L); 

                    // Set GOPATH to an internal directory. We shouldn't actually
                    // need to store files here, since the toolchain won't
                    // depend on modules outside of vendor directories, but if
                    // GOPATH points somewhere else (e.g., to GOROOT), the
                    // go tool may complain.
                    os.Setenv("GOPATH", pathf("%s/pkg/obj/gopath", goroot));

                    if (debug)
                    { 
                        // cmd/buildid is used in debug mode.
                        toolchain = append(toolchain, "cmd/buildid");

                    }

                    if (isdir(pathf("%s/src/pkg", goroot)))
                    {
                        fatalf("\n\n" + "The Go package sources have moved to $GOROOT/src.\n" + "*** %s still exists. ***\n" + "It probably contains stale files that may confuse the build.\n" + "Please (check what's there and) remove it and try again.\n" + "See https://golang.org/s/go14nopkg\n", pathf("%s/src/pkg", goroot));
                    }

                    if (rebuildall)
                    {
                        clean();
                    }

                    setup();

                    timelog("build", "toolchain1");
                    checkCC();
                    bootstrapBuildTools(); 

                    // Remember old content of $GOROOT/bin for comparison below.
                    var (oldBinFiles, _) = filepath.Glob(pathf("%s/bin/*", goroot)); 

                    // For the main bootstrap, building for host os/arch.
                    oldgoos = goos;
                    oldgoarch = goarch;
                    goos = gohostos;
                    goarch = gohostarch;
                    os.Setenv("GOHOSTARCH", gohostarch);
                    os.Setenv("GOHOSTOS", gohostos);
                    os.Setenv("GOARCH", goarch);
                    os.Setenv("GOOS", goos);

                    timelog("build", "go_bootstrap");
                    xprintf("Building Go bootstrap cmd/go (go_bootstrap) using Go toolchain1.\n");
                    install("runtime"); // dependency not visible in sources; also sets up textflag.h
                    install("cmd/go");
                    if (vflag > 0L)
                    {
                        xprintf("\n");
                    }

                    gogcflags = os.Getenv("GO_GCFLAGS"); // we were using $BOOT_GO_GCFLAGS until now
                    goldflags = os.Getenv("GO_LDFLAGS"); // we were using $BOOT_GO_LDFLAGS until now
                    var goBootstrap = pathf("%s/go_bootstrap", tooldir);
                    var cmdGo = pathf("%s/go", gobin);
                    if (debug)
                    {
                        run("", ShowOutput | CheckExit, pathf("%s/compile", tooldir), "-V=full");
                        copyfile(pathf("%s/compile1", tooldir), pathf("%s/compile", tooldir), writeExec);
                    } 

                    // To recap, so far we have built the new toolchain
                    // (cmd/asm, cmd/cgo, cmd/compile, cmd/link)
                    // using Go 1.4's toolchain and go command.
                    // Then we built the new go command (as go_bootstrap)
                    // using the new toolchain and our own build logic (above).
                    //
                    //    toolchain1 = mk(new toolchain, go1.4 toolchain, go1.4 cmd/go)
                    //    go_bootstrap = mk(new cmd/go, toolchain1, cmd/dist)
                    //
                    // The toolchain1 we built earlier is built from the new sources,
                    // but because it was built using cmd/go it has no build IDs.
                    // The eventually installed toolchain needs build IDs, so we need
                    // to do another round:
                    //
                    //    toolchain2 = mk(new toolchain, toolchain1, go_bootstrap)
                    //
                    timelog("build", "toolchain2");
                    if (vflag > 0L)
                    {
                        xprintf("\n");
                    }

                    xprintf("Building Go toolchain2 using go_bootstrap and Go toolchain1.\n");
                    os.Setenv("CC", compilerEnvLookup(defaultcc, goos, goarch));
                    goInstall(goBootstrap, append(new slice<@string>(new @string[] { "-i" }), toolchain));
                    if (debug)
                    {
                        run("", ShowOutput | CheckExit, pathf("%s/compile", tooldir), "-V=full");
                        run("", ShowOutput | CheckExit, pathf("%s/buildid", tooldir), pathf("%s/pkg/%s_%s/runtime/internal/sys.a", goroot, goos, goarch));
                        copyfile(pathf("%s/compile2", tooldir), pathf("%s/compile", tooldir), writeExec);
                    } 

                    // Toolchain2 should be semantically equivalent to toolchain1,
                    // but it was built using the new compilers instead of the Go 1.4 compilers,
                    // so it should at the least run faster. Also, toolchain1 had no build IDs
                    // in the binaries, while toolchain2 does. In non-release builds, the
                    // toolchain's build IDs feed into constructing the build IDs of built targets,
                    // so in non-release builds, everything now looks out-of-date due to
                    // toolchain2 having build IDs - that is, due to the go command seeing
                    // that there are new compilers. In release builds, the toolchain's reported
                    // version is used in place of the build ID, and the go command does not
                    // see that change from toolchain1 to toolchain2, so in release builds,
                    // nothing looks out of date.
                    // To keep the behavior the same in both non-release and release builds,
                    // we force-install everything here.
                    //
                    //    toolchain3 = mk(new toolchain, toolchain2, go_bootstrap)
                    //
                    timelog("build", "toolchain3");
                    if (vflag > 0L)
                    {
                        xprintf("\n");
                    }

                    xprintf("Building Go toolchain3 using go_bootstrap and Go toolchain2.\n");
                    goInstall(goBootstrap, append(new slice<@string>(new @string[] { "-a", "-i" }), toolchain));
                    if (debug)
                    {
                        run("", ShowOutput | CheckExit, pathf("%s/compile", tooldir), "-V=full");
                        run("", ShowOutput | CheckExit, pathf("%s/buildid", tooldir), pathf("%s/pkg/%s_%s/runtime/internal/sys.a", goroot, goos, goarch));
                        copyfile(pathf("%s/compile3", tooldir), pathf("%s/compile", tooldir), writeExec);
                    }

                    checkNotStale(goBootstrap, append(toolchain, "runtime/internal/sys"));

                    if (goos == oldgoos && goarch == oldgoarch)
                    { 
                        // Common case - not setting up for cross-compilation.
                        timelog("build", "toolchain");
                        if (vflag > 0L)
                        {
                            xprintf("\n");
                        }

                        xprintf("Building packages and commands for %s/%s.\n", goos, goarch);

                    }
                    else
                    { 
                        // GOOS/GOARCH does not match GOHOSTOS/GOHOSTARCH.
                        // Finish GOHOSTOS/GOHOSTARCH installation and then
                        // run GOOS/GOARCH installation.
                        timelog("build", "host toolchain");
                        if (vflag > 0L)
                        {
                            xprintf("\n");
                        }

                        xprintf("Building packages and commands for host, %s/%s.\n", goos, goarch);
                        goInstall(goBootstrap, "std", "cmd");
                        checkNotStale(goBootstrap, "std", "cmd");
                        checkNotStale(cmdGo, "std", "cmd");

                        timelog("build", "target toolchain");
                        if (vflag > 0L)
                        {
                            xprintf("\n");
                        }

                        goos = oldgoos;
                        goarch = oldgoarch;
                        os.Setenv("GOOS", goos);
                        os.Setenv("GOARCH", goarch);
                        os.Setenv("CC", compilerEnvLookup(defaultcc, goos, goarch));
                        xprintf("Building packages and commands for target, %s/%s.\n", goos, goarch);

                    }

                    @string targets = new slice<@string>(new @string[] { "std", "cmd" });
                    if (goos == "js" && goarch == "wasm")
                    { 
                        // Skip the cmd tools for js/wasm. They're not usable.
                        targets = targets[..1L];

                    }

                    goInstall(goBootstrap, targets);
                    checkNotStale(goBootstrap, targets);
                    checkNotStale(cmdGo, targets);
                    if (debug)
                    {
                        run("", ShowOutput | CheckExit, pathf("%s/compile", tooldir), "-V=full");
                        run("", ShowOutput | CheckExit, pathf("%s/buildid", tooldir), pathf("%s/pkg/%s_%s/runtime/internal/sys.a", goroot, goos, goarch));
                        checkNotStale(goBootstrap, append(toolchain, "runtime/internal/sys"));
                        copyfile(pathf("%s/compile4", tooldir), pathf("%s/compile", tooldir), writeExec);
                    } 

                    // Check that there are no new files in $GOROOT/bin other than
                    // go and gofmt and $GOOS_$GOARCH (target bin when cross-compiling).
                    var (binFiles, _) = filepath.Glob(pathf("%s/bin/*", goroot));
                    map ok = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in oldBinFiles)
                        {
                            f = __f;
                            ok[f] = true;
                        }

                        f = f__prev1;
                    }

                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in binFiles)
                        {
                            f = __f;
                            elem = strings.TrimSuffix(filepath.Base(f), ".exe");
                            if (!ok[f] && elem != "go" && elem != "gofmt" && elem != goos + "_" + goarch)
                            {
                                fatalf("unexpected new file in $GOROOT/bin: %s", elem);
                            }

                        } 

                        // Remove go_bootstrap now that we're done.

                        f = f__prev1;
                    }

                    xremove(pathf("%s/go_bootstrap", tooldir));

                    if (goos == "android")
                    { 
                        // Make sure the exec wrapper will sync a fresh $GOROOT to the device.
                        xremove(pathf("%s/go_android_exec-adb-sync-status", os.TempDir()));

                    }

                    {
                        var wrapperPath = wrapperPathFor(goos, goarch);

                        if (wrapperPath != "")
                        {
                            var oldcc = os.Getenv("CC");
                            os.Setenv("GOOS", gohostos);
                            os.Setenv("GOARCH", gohostarch);
                            os.Setenv("CC", compilerEnvLookup(defaultcc, gohostos, gohostarch));
                            goCmd(cmdGo, "build", "-o", pathf("%s/go_%s_%s_exec%s", gobin, goos, goarch, exe), wrapperPath); 
                            // Restore environment.
                            // TODO(elias.naur): support environment variables in goCmd?
                            os.Setenv("GOOS", goos);
                            os.Setenv("GOARCH", goarch);
                            os.Setenv("CC", oldcc);

                        } 

                        // Print trailing banner unless instructed otherwise.

                    } 

                    // Print trailing banner unless instructed otherwise.
                    if (!noBanner)
                    {
                        banner();
                    }

                }
                goarch;

                string;

                {

                    if (goos == "android") 
                        if (gohostos != "android")
                        {
                            return pathf("%s/misc/android/go_android_exec.go", goroot);
                        }

                    else if (goos == "darwin" && goarch == "arm64") 
                        if (gohostos != "darwin" || gohostarch != "arm64")
                        {
                            return pathf("%s/misc/ios/go_darwin_arm_exec.go", goroot);
                        }

                                        return "";

                }
                {
                    goCmd(goBinary, "install", args);
                }
                cmd;

                {
                    @string goCmd = new slice<@string>(new @string[] { goBinary, cmd, "-gcflags=all="+gogcflags, "-ldflags=all="+goldflags });
                    if (vflag > 0L)
                    {
                        goCmd = append(goCmd, "-v");
                    } 

                    // Force only one process at a time on vx32 emulation.
                    if (gohostos == "plan9" && os.Getenv("sysname") == "vx32")
                    {
                        goCmd = append(goCmd, "-p=1");
                    }

                    run(goroot, ShowOutput | CheckExit, append(goCmd, args));

                }
                {
                    var @out = run(goroot, CheckExit, append(new slice<@string>(new @string[] { goBinary, "list", "-gcflags=all="+gogcflags, "-ldflags=all="+goldflags, "-f={{if .Stale}}\tSTALE {{.ImportPath}}: {{.StaleReason}}{{end}}" }), targets));
                    if (strings.Contains(out, "\tSTALE "))
                    {
                        os.Setenv("GODEBUG", "gocachehash=1");
                        foreach (var (_, target) in new slice<@string>(new @string[] { "runtime/internal/sys", "cmd/dist", "cmd/link" }))
                        {
                            if (strings.Contains(out, "STALE " + target))
                            {
                                run(goroot, ShowOutput | CheckExit, goBinary, "list", "-f={{.ImportPath}} {{.Stale}}", target);
                                break;
                            }

                        }
                        fatalf("unexpected stale targets reported by %s list -gcflags=\"%s\" -ldflags=\"%s\" for %v:\n%s", goBinary, gogcflags, goldflags, targets, out);

                    }

                }

                // Cannot use go/build directly because cmd/dist for a new release
                // builds against an old release's go/build, which may be out of sync.
                // To reduce duplication, we generate the list for go/build from this.
                //
                // We list all supported platforms in this list, so that this is the
                // single point of truth for supported platforms. This list is used
                // by 'go tool dist list'.
                map cgoEnabled = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"aix/ppc64":true,"darwin/amd64":true,"darwin/arm64":true,"dragonfly/amd64":true,"freebsd/386":true,"freebsd/amd64":true,"freebsd/arm":true,"freebsd/arm64":true,"illumos/amd64":true,"linux/386":true,"linux/amd64":true,"linux/arm":true,"linux/arm64":true,"linux/ppc64":false,"linux/ppc64le":true,"linux/mips":true,"linux/mipsle":true,"linux/mips64":true,"linux/mips64le":true,"linux/riscv64":false,"linux/s390x":true,"linux/sparc64":true,"android/386":true,"android/amd64":true,"android/arm":true,"android/arm64":true,"js/wasm":false,"netbsd/386":true,"netbsd/amd64":true,"netbsd/arm":true,"netbsd/arm64":true,"openbsd/386":true,"openbsd/amd64":true,"openbsd/arm":true,"openbsd/arm64":true,"plan9/386":false,"plan9/amd64":false,"plan9/arm":false,"solaris/amd64":true,"windows/386":true,"windows/amd64":true,"windows/arm":false,};

                // List of platforms which are supported but not complete yet. These get
                // filtered out of cgoEnabled for 'dist list'. See golang.org/issue/28944
                map incomplete = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"linux/sparc64":true,};

                needCC();

                bool;

                {
                    switch (os.Getenv("CGO_ENABLED"))
                    {
                        case "1": 
                            return true;
                            break;
                        case "0": 
                            return false;
                            break;
                    }
                    return cgoEnabled[gohostos + "/" + gohostarch];

                }
                checkCC();

                {
                    if (!needCC())
                    {
                        return ;
                    }

                    {
                        var err__prev1 = err;

                        var (output, err) = exec.Command(defaultcc[""], "--help").CombinedOutput();

                        if (err != null)
                        {
                            @string outputHdr = "";
                            if (len(output) > 0L)
                            {
                                outputHdr = "\nCommand output:\n\n";
                            }

                            fatalf("cannot invoke C compiler %q: %v\n\n" + "Go needs a system C compiler for use with cgo.\n" + "To set a C compiler, set CC=the-compiler.\n" + "To disable cgo, set CGO_ENABLED=0.\n%s%s", defaultcc[""], err, outputHdr, output);

                        }

                        err = err__prev1;

                    }

                }
                {
                    while (true)
                    {
                        {
                            var err__prev1 = err;

                            var (fi, err) = os.Stat(filepath.Join(dir, "go.mod"));

                            if (err == null && !fi.IsDir())
                            {
                                return dir;
                            }

                            err = err__prev1;

                        }

                        var d = filepath.Dir(dir);
                        if (d == dir)
                        {
                            break;
                        }

                        dir = d;

                    }

                    return "";

                }
                defaulttarg();

                string;

                { 
                    // xgetwd might return a path with symlinks fully resolved, and if
                    // there happens to be symlinks in goroot, then the hasprefix test
                    // will never succeed. Instead, we use xrealwd to get a canonical
                    // goroot/src before the comparison to avoid this problem.
                    var pwd = xgetwd();
                    var src = pathf("%s/src/", goroot);
                    var real_src = xrealwd(src);
                    if (!strings.HasPrefix(pwd, real_src))
                    {
                        fatalf("current directory %s is not under %s", pwd, real_src);
                    }

                    pwd = pwd[len(real_src)..]; 
                    // guard against xrealwd returning the directory without the trailing /
                    pwd = strings.TrimPrefix(pwd, "/");

                    return pwd;

                }

                // Install installs the list of packages named on the command line.
                cmdinstall();

                {
                    xflagparse(-1L);

                    if (flag.NArg() == 0L)
                    {
                        install(defaulttarg());
                    }

                    foreach (var (_, arg) in flag.Args())
                    {
                        install(arg);
                    }

                }

                // Clean deletes temporary objects.
                cmdclean();

                {
                    xflagparse(0L);
                    clean();
                }

                // Banner prints the 'now you've installed Go' banner.
                cmdbanner();

                {
                    xflagparse(0L);
                    banner();
                }
                banner();

                {
                    if (vflag > 0L)
                    {
                        xprintf("\n");
                    }

                    xprintf("---\n");
                    xprintf("Installed Go for %s/%s in %s\n", goos, goarch, goroot);
                    xprintf("Installed commands in %s\n", gobin);

                    if (!xsamefile(goroot_final, goroot))
                    { 
                        // If the files are to be moved, don't check that gobin
                        // is on PATH; assume they know what they are doing.
                    }
                    else if (gohostos == "plan9")
                    { 
                        // Check that gobin is bound before /bin.
                        var pid = strings.Replace(readfile("#c/pid"), " ", "", -1L);
                        var ns = fmt.Sprintf("/proc/%s/ns", pid);
                        if (!strings.Contains(readfile(ns), fmt.Sprintf("bind -b %s /bin", gobin)))
                        {
                            xprintf("*** You need to bind %s before /bin.\n", gobin);
                        }

                    }
                    else
                    { 
                        // Check that gobin appears in $PATH.
                        @string pathsep = ":";
                        if (gohostos == "windows")
                        {
                            pathsep = ";";
                        }

                        if (!strings.Contains(pathsep + os.Getenv("PATH") + pathsep, pathsep + gobin + pathsep))
                        {
                            xprintf("*** You need to add %s to your PATH.\n", gobin);
                        }

                    }

                    if (!xsamefile(goroot_final, goroot))
                    {
                        xprintf("\n" + "The binaries expect %s to be copied or moved to %s\n", goroot, goroot_final);
                    }

                }

                // Version prints the Go version.
                cmdversion();

                {
                    xflagparse(0L);
                    xprintf("%s\n", findgoversion());
                }

                // cmdlist lists all supported platforms.
                cmdlist();

                {
                    var jsonFlag = flag.Bool("json", false, "produce JSON output");
                    xflagparse(0L);

                    slice<@string> plats = default;
                    {
                        var p__prev1 = p;

                        foreach (var (__p) in cgoEnabled)
                        {
                            p = __p;
                            if (incomplete[p])
                            {
                                continue;
                            }

                            plats = append(plats, p);

                        }

                        p = p__prev1;
                    }

                    sort.Strings(plats);

                    if (!jsonFlag.val)
                    {
                        {
                            var p__prev1 = p;

                            foreach (var (_, __p) in plats)
                            {
                                p = __p;
                                xprintf("%s\n", p);
                            }

                            p = p__prev1;
                        }

                        return ;

                    }

                    private partial struct jsonResult
                    {
                        public @string GOOS;
                        public @string GOARCH;
                        public bool CgoSupported;
                    }
                    slice<jsonResult> results = default;
                    {
                        var p__prev1 = p;

                        foreach (var (_, __p) in plats)
                        {
                            p = __p;
                            fields = strings.Split(p, "/");
                            results = append(results, new jsonResult(GOOS:fields[0],GOARCH:fields[1],CgoSupported:cgoEnabled[p]));
                        }

                        p = p__prev1;
                    }

                    var (out, err) = json.MarshalIndent(results, "", "\t");
                    if (err != null)
                    {
                        fatalf("json marshal error: %v", err);
                    }

                    {
                        var err__prev1 = err;

                        var (_, err) = os.Stdout.Write(out);

                        if (err != null)
                        {
                            fatalf("write failed: %v", err);
                        }

                        err = err__prev1;

                    }

                }

            }

        });
    }
}
