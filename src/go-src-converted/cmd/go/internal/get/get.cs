// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package get implements the ``go get'' command.
// package get -- go2cs converted at 2020 August 29 10:01:53 UTC
// import "cmd/go/internal/get" ==> using get = go.cmd.go.@internal.get_package
// Original source: C:\Go\src\cmd\go\internal\get\get.go
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using web = go.cmd.go.@internal.web_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class get_package
    {
        public static base.Command CmdGet = ref new base.Command(UsageLine:"get [-d] [-f] [-fix] [-insecure] [-t] [-u] [-v] [build flags] [packages]",Short:"download and install packages and dependencies",Long:`
Get downloads the packages named by the import paths, along with their
dependencies. It then installs the named packages, like 'go install'.

The -d flag instructs get to stop after downloading the packages; that is,
it instructs get not to install the packages.

The -f flag, valid only when -u is set, forces get -u not to verify that
each package has been checked out from the source control repository
implied by its import path. This can be useful if the source is a local fork
of the original.

The -fix flag instructs get to run the fix tool on the downloaded packages
before resolving dependencies or building the code.

The -insecure flag permits fetching from repositories and resolving
custom domains using insecure schemes such as HTTP. Use with caution.

The -t flag instructs get to also download the packages required to build
the tests for the specified packages.

The -u flag instructs get to use the network to update the named packages
and their dependencies. By default, get uses the network to check out
missing packages but does not use it to look for updates to existing packages.

The -v flag enables verbose progress and debug output.

Get also accepts build flags to control the installation. See 'go help build'.

When checking out a new package, get creates the target directory
GOPATH/src/<import-path>. If the GOPATH contains multiple entries,
get uses the first one. For more details see: 'go help gopath'.

When checking out or updating a package, get looks for a branch or tag
that matches the locally installed version of Go. The most important
rule is that if the local installation is running version "go1", get
searches for a branch or tag named "go1". If no such version exists
it retrieves the default branch of the package.

When go get checks out or updates a Git repository,
it also updates any git submodules referenced by the repository.

Get never checks out or updates code stored in vendor directories.

For more about specifying packages, see 'go help packages'.

For more about how 'go get' finds source code to
download, see 'go help importpath'.

See also: go build, go install, go clean.
	`,);

        private static var getD = CmdGet.Flag.Bool("d", false, "");
        private static var getF = CmdGet.Flag.Bool("f", false, "");
        private static var getT = CmdGet.Flag.Bool("t", false, "");
        private static var getU = CmdGet.Flag.Bool("u", false, "");
        private static var getFix = CmdGet.Flag.Bool("fix", false, "");
        private static var getInsecure = CmdGet.Flag.Bool("insecure", false, "");

        private static void init()
        {
            work.AddBuildFlags(CmdGet);
            CmdGet.Run = runGet; // break init loop
        }

        private static void runGet(ref base.Command cmd, slice<@string> args)
        {
            work.BuildInit();

            if (getF && !getU.Value.Value)
            {
                @base.Fatalf("go get: cannot use -f flag without -u");
            } 

            // Disable any prompting for passwords by Git.
            // Only has an effect for 2.3.0 or later, but avoiding
            // the prompt in earlier versions is just too hard.
            // If user has explicitly set GIT_TERMINAL_PROMPT=1, keep
            // prompting.
            // See golang.org/issue/9341 and golang.org/issue/12706.
            if (os.Getenv("GIT_TERMINAL_PROMPT") == "")
            {
                os.Setenv("GIT_TERMINAL_PROMPT", "0");
            } 

            // Disable any ssh connection pooling by Git.
            // If a Git subprocess forks a child into the background to cache a new connection,
            // that child keeps stdout/stderr open. After the Git subprocess exits,
            // os /exec expects to be able to read from the stdout/stderr pipe
            // until EOF to get all the data that the Git subprocess wrote before exiting.
            // The EOF doesn't come until the child exits too, because the child
            // is holding the write end of the pipe.
            // This is unfortunate, but it has come up at least twice
            // (see golang.org/issue/13453 and golang.org/issue/16104)
            // and confuses users when it does.
            // If the user has explicitly set GIT_SSH or GIT_SSH_COMMAND,
            // assume they know what they are doing and don't step on it.
            // But default to turning off ControlMaster.
            if (os.Getenv("GIT_SSH") == "" && os.Getenv("GIT_SSH_COMMAND") == "")
            {
                os.Setenv("GIT_SSH_COMMAND", "ssh -o ControlMaster=no");
            } 

            // Phase 1. Download/update.
            load.ImportStack stk = default;
            long mode = 0L;
            if (getT.Value)
            {
                mode |= load.GetTestDeps;
            }
            args = downloadPaths(args);
            foreach (var (_, arg) in args)
            {
                download(arg, null, ref stk, mode);
            }
            @base.ExitIfErrors(); 

            // Phase 2. Rescan packages and re-evaluate args list.

            // Code we downloaded and all code that depends on it
            // needs to be evicted from the package cache so that
            // the information will be recomputed. Instead of keeping
            // track of the reverse dependency information, evict
            // everything.
            load.ClearPackageCache(); 

            // In order to rebuild packages information completely,
            // we need to clear commands cache. Command packages are
            // referring to evicted packages from the package cache.
            // This leads to duplicated loads of the standard packages.
            load.ClearCmdCache();

            args = load.ImportPaths(args);
            load.PackagesForBuild(args); 

            // Phase 3. Install.
            if (getD.Value)
            { 
                // Download only.
                // Check delayed until now so that importPaths
                // and packagesForBuild have a chance to print errors.
                return;
            }
            work.InstallPackages(args, true);
        }

        // downloadPaths prepares the list of paths to pass to download.
        // It expands ... patterns that can be expanded. If there is no match
        // for a particular pattern, downloadPaths leaves it in the result list,
        // in the hope that we can figure out the repository from the
        // initial ...-free prefix.
        private static slice<@string> downloadPaths(slice<@string> args)
        {
            args = load.ImportPathsNoDotExpansion(args);
            slice<@string> @out = default;
            foreach (var (_, a) in args)
            {
                if (strings.Contains(a, "..."))
                {
                    slice<@string> expand = default; 
                    // Use matchPackagesInFS to avoid printing
                    // warnings. They will be printed by the
                    // eventual call to importPaths instead.
                    if (build.IsLocalImport(a))
                    {
                        expand = load.MatchPackagesInFS(a);
                    }
                    else
                    {
                        expand = load.MatchPackages(a);
                    }
                    if (len(expand) > 0L)
                    {
                        out = append(out, expand);
                        continue;
                    }
                }
                out = append(out, a);
            }
            return out;
        }

        // downloadCache records the import paths we have already
        // considered during the download, to avoid duplicate work when
        // there is more than one dependency sequence leading to
        // a particular package.
        private static map downloadCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};

        // downloadRootCache records the version control repository
        // root directories we have already considered during the download.
        // For example, all the packages in the github.com/google/codesearch repo
        // share the same root (the directory for that path), and we only need
        // to run the hg commands to consider each repository once.
        private static map downloadRootCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};

        // download runs the download half of the get command
        // for the package named by the argument.
        private static void download(@string arg, ref load.Package _parent, ref load.ImportStack _stk, long mode) => func(_parent, _stk, (ref load.Package parent, ref load.ImportStack stk, Defer _, Panic panic, Recover __) =>
        {
            if (mode & load.UseVendor != 0L)
            { 
                // Caller is responsible for expanding vendor paths.
                panic("internal error: download mode has useVendor set");
            }
            Func<@string, long, ref load.Package> load1 = (path, mode) =>
            {
                if (parent == null)
                {
                    return load.LoadPackage(path, stk);
                }
                return load.LoadImport(path, parent.Dir, parent, stk, null, mode);
            }
;

            var p = load1(arg, mode);
            if (p.Error != null && p.Error.Hard)
            {
                @base.Errorf("%s", p.Error);
                return;
            } 

            // loadPackage inferred the canonical ImportPath from arg.
            // Use that in the following to prevent hysteresis effects
            // in e.g. downloadCache and packageCache.
            // This allows invocations such as:
            //   mkdir -p $GOPATH/src/github.com/user
            //   cd $GOPATH/src/github.com/user
            //   go get ./foo
            // see: golang.org/issue/9767
            arg = p.ImportPath; 

            // There's nothing to do if this is a package in the standard library.
            if (p.Standard)
            {
                return;
            } 

            // Only process each package once.
            // (Unless we're fetching test dependencies for this package,
            // in which case we want to process it again.)
            if (downloadCache[arg] && mode & load.GetTestDeps == 0L)
            {
                return;
            }
            downloadCache[arg] = true;

            ref load.Package pkgs = new slice<ref load.Package>(new ref load.Package[] { p });
            var wildcardOkay = len(stk.Value) == 0L;
            var isWildcard = false; 

            // Download if the package is missing, or update if we're using -u.
            if (p.Dir == "" || getU.Value)
            { 
                // The actual download.
                stk.Push(arg);
                var err = downloadPackage(p);
                if (err != null)
                {
                    @base.Errorf("%s", ref new load.PackageError(ImportStack:stk.Copy(),Err:err.Error()));
                    stk.Pop();
                    return;
                }
                stk.Pop();

                @string args = new slice<@string>(new @string[] { arg }); 
                // If the argument has a wildcard in it, re-evaluate the wildcard.
                // We delay this until after reloadPackage so that the old entry
                // for p has been replaced in the package cache.
                if (wildcardOkay && strings.Contains(arg, "..."))
                {
                    if (build.IsLocalImport(arg))
                    {
                        args = load.MatchPackagesInFS(arg);
                    }
                    else
                    {
                        args = load.MatchPackages(arg);
                    }
                    isWildcard = true;
                } 

                // Clear all relevant package cache entries before
                // doing any new loads.
                load.ClearPackageCachePartial(args);

                pkgs = pkgs[..0L];
                foreach (var (_, arg) in args)
                { 
                    // Note: load calls loadPackage or loadImport,
                    // which push arg onto stk already.
                    // Do not push here too, or else stk will say arg imports arg.
                    p = load1(arg, mode);
                    if (p.Error != null)
                    {
                        @base.Errorf("%s", p.Error);
                        continue;
                    }
                    pkgs = append(pkgs, p);
                }
            } 

            // Process package, which might now be multiple packages
            // due to wildcard expansion.
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs)
                {
                    p = __p;
                    if (getFix.Value)
                    {
                        var files = @base.RelPaths(p.InternalAllGoFiles());
                        @base.Run(cfg.BuildToolexec, str.StringList(@base.Tool("fix"), files)); 

                        // The imports might have changed, so reload again.
                        p = load.ReloadPackage(arg, stk);
                        if (p.Error != null)
                        {
                            @base.Errorf("%s", p.Error);
                            return;
                        }
                    }
                    if (isWildcard)
                    { 
                        // Report both the real package and the
                        // wildcard in any error message.
                        stk.Push(p.ImportPath);
                    } 

                    // Process dependencies, now that we know what they are.
                    var imports = p.Imports;
                    if (mode & load.GetTestDeps != 0L)
                    { 
                        // Process test dependencies when -t is specified.
                        // (But don't get test dependencies for test dependencies:
                        // we always pass mode 0 to the recursive calls below.)
                        imports = str.StringList(imports, p.TestImports, p.XTestImports);
                    }
                    foreach (var (i, path) in imports)
                    {
                        if (path == "C")
                        {
                            continue;
                        } 
                        // Fail fast on import naming full vendor path.
                        // Otherwise expand path as needed for test imports.
                        // Note that p.Imports can have additional entries beyond p.Internal.Build.Imports.
                        var orig = path;
                        if (i < len(p.Internal.Build.Imports))
                        {
                            orig = p.Internal.Build.Imports[i];
                        }
                        {
                            var (j, ok) = load.FindVendor(orig);

                            if (ok)
                            {
                                stk.Push(path);
                                err = ref new load.PackageError(ImportStack:stk.Copy(),Err:"must be imported as "+path[j+len("vendor/"):],);
                                stk.Pop();
                                @base.Errorf("%s", err);
                                continue;
                            } 
                            // If this is a test import, apply vendor lookup now.
                            // We cannot pass useVendor to download, because
                            // download does caching based on the value of path,
                            // so it must be the fully qualified path already.

                        } 
                        // If this is a test import, apply vendor lookup now.
                        // We cannot pass useVendor to download, because
                        // download does caching based on the value of path,
                        // so it must be the fully qualified path already.
                        if (i >= len(p.Imports))
                        {
                            path = load.VendoredImportPath(p, path);
                        }
                        download(path, p, stk, 0L);
                    }
                    if (isWildcard)
                    {
                        stk.Pop();
                    }
                }

                p = p__prev1;
            }

        });

        // downloadPackage runs the create or download command
        // to make the first copy of or update a copy of the given package.
        private static error downloadPackage(ref load.Package p)
        {
            ref vcsCmd vcs = default;            @string repo = default;            @string rootPath = default;
            error err = default;

            var security = web.Secure;
            if (getInsecure.Value)
            {
                security = web.Insecure;
            }
            if (p.Internal.Build.SrcRoot != "")
            { 
                // Directory exists. Look for checkout along path to src.
                vcs, rootPath, err = vcsFromDir(p.Dir, p.Internal.Build.SrcRoot);
                if (err != null)
                {
                    return error.As(err);
                }
                repo = "<local>"; // should be unused; make distinctive

                // Double-check where it came from.
                if (getU && vcs.remoteRepo != null.Value)
                {
                    var dir = filepath.Join(p.Internal.Build.SrcRoot, filepath.FromSlash(rootPath));
                    var (remote, err) = vcs.remoteRepo(vcs, dir);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    repo = remote;
                    if (!getF.Value)
                    {
                        {
                            var rr__prev4 = rr;
                            error err__prev4 = err;

                            var (rr, err) = repoRootForImportPath(p.ImportPath, security);

                            if (err == null)
                            {
                                repo = rr.repo;
                                if (rr.vcs.resolveRepo != null)
                                {
                                    var (resolved, err) = rr.vcs.resolveRepo(rr.vcs, dir, repo);
                                    if (err == null)
                                    {
                                        repo = resolved;
                                    }
                                }
                                if (remote != repo && rr.isCustom)
                                {
                                    return error.As(fmt.Errorf("%s is a custom import path for %s, but %s is checked out from %s", rr.root, repo, dir, remote));
                                }
                            }

                            rr = rr__prev4;
                            err = err__prev4;

                        }
                    }
                }
            }
            else
            { 
                // Analyze the import path to determine the version control system,
                // repository, and the import path for the root of the repository.
                (rr, err) = repoRootForImportPath(p.ImportPath, security);
                if (err != null)
                {
                    return error.As(err);
                }
                vcs = rr.vcs;
                repo = rr.repo;
                rootPath = rr.root;
            }
            if (!vcs.isSecure(repo) && !getInsecure.Value)
            {
                return error.As(fmt.Errorf("cannot download, %v uses insecure protocol", repo));
            }
            if (p.Internal.Build.SrcRoot == "")
            { 
                // Package not found. Put in first directory of $GOPATH.
                var list = filepath.SplitList(cfg.BuildContext.GOPATH);
                if (len(list) == 0L)
                {
                    return error.As(fmt.Errorf("cannot download, $GOPATH not set. For more details see: 'go help gopath'"));
                } 
                // Guard against people setting GOPATH=$GOROOT.
                if (filepath.Clean(list[0L]) == filepath.Clean(cfg.GOROOT))
                {
                    return error.As(fmt.Errorf("cannot download, $GOPATH must not be set to $GOROOT. For more details see: 'go help gopath'"));
                }
                {
                    error err__prev2 = err;

                    var (_, err) = os.Stat(filepath.Join(list[0L], "src/cmd/go/alldocs.go"));

                    if (err == null)
                    {
                        return error.As(fmt.Errorf("cannot download, %s is a GOROOT, not a GOPATH. For more details see: 'go help gopath'", list[0L]));
                    }

                    err = err__prev2;

                }
                p.Internal.Build.Root = list[0L];
                p.Internal.Build.SrcRoot = filepath.Join(list[0L], "src");
                p.Internal.Build.PkgRoot = filepath.Join(list[0L], "pkg");
            }
            var root = filepath.Join(p.Internal.Build.SrcRoot, filepath.FromSlash(rootPath));

            {
                error err__prev1 = err;

                err = checkNestedVCS(vcs, root, p.Internal.Build.SrcRoot);

                if (err != null)
                {
                    return error.As(err);
                } 

                // If we've considered this repository already, don't do it again.

                err = err__prev1;

            } 

            // If we've considered this repository already, don't do it again.
            if (downloadRootCache[root])
            {
                return error.As(null);
            }
            downloadRootCache[root] = true;

            if (cfg.BuildV)
            {
                fmt.Fprintf(os.Stderr, "%s (download)\n", rootPath);
            } 

            // Check that this is an appropriate place for the repo to be checked out.
            // The target directory must either not exist or have a repo checked out already.
            var meta = filepath.Join(root, "." + vcs.cmd);
            {
                error err__prev1 = err;

                (_, err) = os.Stat(meta);

                if (err != null)
                { 
                    // Metadata file or directory does not exist. Prepare to checkout new copy.
                    // Some version control tools require the target directory not to exist.
                    // We require that too, just to avoid stepping on existing work.
                    {
                        error err__prev2 = err;

                        (_, err) = os.Stat(root);

                        if (err == null)
                        {
                            return error.As(fmt.Errorf("%s exists but %s does not - stale checkout?", root, meta));
                        }

                        err = err__prev2;

                    }

                    (_, err) = os.Stat(p.Internal.Build.Root);
                    var gopathExisted = err == null; 

                    // Some version control tools require the parent of the target to exist.
                    var (parent, _) = filepath.Split(root);
                    err = error.As(os.MkdirAll(parent, 0777L));

                    if (err != null)
                    {
                        return error.As(err);
                    }
                    if (cfg.BuildV && !gopathExisted && p.Internal.Build.Root == cfg.BuildContext.GOPATH)
                    {
                        fmt.Fprintf(os.Stderr, "created GOPATH=%s; see 'go help gopath'\n", p.Internal.Build.Root);
                    }
                    err = error.As(vcs.create(root, repo));

                    if (err != null)
                    {
                        return error.As(err);
                    }
                }
                else
                { 
                    // Metadata directory does exist; download incremental updates.
                    err = error.As(vcs.download(root));

                    if (err != null)
                    {
                        return error.As(err);
                    }
                }

                err = err__prev1;

            }

            if (cfg.BuildN)
            { 
                // Do not show tag sync in -n; it's noise more than anything,
                // and since we're not running commands, no tag will be found.
                // But avoid printing nothing.
                fmt.Fprintf(os.Stderr, "# cd %s; %s sync/update\n", root, vcs.cmd);
                return error.As(null);
            } 

            // Select and sync to appropriate version of the repository.
            var (tags, err) = vcs.tags(root);
            if (err != null)
            {
                return error.As(err);
            }
            var vers = runtime.Version();
            {
                var i = strings.Index(vers, " ");

                if (i >= 0L)
                {
                    vers = vers[..i];
                }

            }
            {
                error err__prev1 = err;

                err = vcs.tagSync(root, selectTag(vers, tags));

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            return error.As(null);
        }

        // selectTag returns the closest matching tag for a given version.
        // Closest means the latest one that is not after the current release.
        // Version "goX" (or "goX.Y" or "goX.Y.Z") matches tags of the same form.
        // Version "release.rN" matches tags of the form "go.rN" (N being a floating-point number).
        // Version "weekly.YYYY-MM-DD" matches tags like "go.weekly.YYYY-MM-DD".
        //
        // NOTE(rsc): Eventually we will need to decide on some logic here.
        // For now, there is only "go1". This matches the docs in go help get.
        private static @string selectTag(@string goVersion, slice<@string> tags)
        {
            foreach (var (_, t) in tags)
            {
                if (t == "go1")
                {
                    return "go1";
                }
            }
            return "";
        }
    }
}}}}
