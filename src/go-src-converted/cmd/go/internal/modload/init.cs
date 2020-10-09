// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 09 05:46:38 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\init.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using lazyregexp = go.@internal.lazyregexp_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using debug = go.runtime.debug_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using modconv = go.cmd.go.@internal.modconv_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using mvs = go.cmd.go.@internal.mvs_package;
using search = go.cmd.go.@internal.search_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        private static var mustUseModules = false;        private static bool initialized = default;        private static @string modRoot = default;        public static module.Version Target = default;        private static @string targetPrefix = default;        private static bool targetInGorootSrc = default;        private static @string gopath = default;        public static bool CmdModInit = default;        public static @string CmdModModule = default;        private static bool allowMissingModuleImports = default;

        // ModFile returns the parsed go.mod file.
        //
        // Note that after calling ImportPaths or LoadBuildList,
        // the require statements in the modfile.File are no longer
        // the source of truth and will be ignored: edits made directly
        // will be lost at the next call to WriteGoMod.
        // To make permanent changes to the require statements
        // in go.mod, edit it before calling ImportPaths or LoadBuildList.
        public static ptr<modfile.File> ModFile()
        {
            Init();
            if (modFile == null)
            {
                die();
            }

            return _addr_modFile!;

        }

        public static @string BinDir()
        {
            Init();
            return filepath.Join(gopath, "bin");
        }

        // Init determines whether module mode is enabled, locates the root of the
        // current module (if any), sets environment variables for Git subprocesses, and
        // configures the cfg, codehost, load, modfetch, and search packages for use
        // with modules.
        public static void Init()
        {
            if (initialized)
            {
                return ;
            }

            initialized = true; 

            // Keep in sync with WillBeEnabled. We perform extra validation here, and
            // there are lots of diagnostics and side effects, so we can't use
            // WillBeEnabled directly.
            var env = cfg.Getenv("GO111MODULE");
            switch (env)
            {
                case "auto": 

                case "": 
                    mustUseModules = false;
                    break;
                case "on": 
                    mustUseModules = true;
                    break;
                case "off": 
                    mustUseModules = false;
                    return ;
                    break;
                default: 
                    @base.Fatalf("go: unknown environment setting GO111MODULE=%s", env);
                    break;
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

            if (CmdModInit)
            { 
                // Running 'go mod init': go.mod will be created in current directory.
                modRoot = @base.Cwd;

            }
            else
            {
                modRoot = findModuleRoot(@base.Cwd);
                if (modRoot == "")
                {
                    if (cfg.ModFile != "")
                    {
                        @base.Fatalf("go: cannot find main module, but -modfile was set.\n\t-modfile cannot be used to set the module root directory.");
                    }

                    if (!mustUseModules)
                    { 
                        // GO111MODULE is 'auto', and we can't find a module root.
                        // Stay in GOPATH mode.
                        return ;

                    }

                }
                else if (search.InDir(modRoot, os.TempDir()) == ".")
                { 
                    // If you create /tmp/go.mod for experimenting,
                    // then any tests that create work directories under /tmp
                    // will find it and get modules when they're not expecting them.
                    // It's a bit of a peculiar thing to disallow but quite mysterious
                    // when it happens. See golang.org/issue/26708.
                    modRoot = "";
                    fmt.Fprintf(os.Stderr, "go: warning: ignoring go.mod in system temp root %v\n", os.TempDir());

                }

            }

            if (cfg.ModFile != "" && !strings.HasSuffix(cfg.ModFile, ".mod"))
            {
                @base.Fatalf("go: -modfile=%s: file does not have .mod extension", cfg.ModFile);
            } 

            // We're in module mode. Install the hooks to make it work.
            {
                var c = cache.Default();

                if (c == null)
                { 
                    // With modules, there are no install locations for packages
                    // other than the build cache.
                    @base.Fatalf("go: cannot use modules with build cache disabled");

                }

            }


            var list = filepath.SplitList(cfg.BuildContext.GOPATH);
            if (len(list) == 0L || list[0L] == "")
            {
                @base.Fatalf("missing $GOPATH");
            }

            gopath = list[0L];
            {
                var (_, err) = os.Stat(filepath.Join(gopath, "go.mod"));

                if (err == null)
                {
                    @base.Fatalf("$GOPATH/go.mod exists but should not");
                }

            }


            cfg.ModulesEnabled = true;
            load.ModBinDir = BinDir;
            load.ModLookup = Lookup;
            load.ModPackageModuleInfo = PackageModuleInfo;
            load.ModImportPaths = ImportPaths;
            load.ModPackageBuildInfo = PackageBuildInfo;
            load.ModInfoProg = ModInfoProg;
            load.ModImportFromFiles = ImportFromFiles;
            load.ModDirImportPath = DirImportPath;

            if (modRoot == "")
            { 
                // We're in module mode, but not inside a module.
                //
                // Commands like 'go build', 'go run', 'go list' have no go.mod file to
                // read or write. They would need to find and download the latest versions
                // of a potentially large number of modules with no way to save version
                // information. We can succeed slowly (but not reproducibly), but that's
                // not usually a good experience.
                //
                // Instead, we forbid resolving import paths to modules other than std and
                // cmd. Users may still build packages specified with .go files on the
                // command line, but they'll see an error if those files import anything
                // outside std.
                //
                // This can be overridden by calling AllowMissingModuleImports.
                // For example, 'go get' does this, since it is expected to resolve paths.
                //
                // See golang.org/issue/32027.
            }
            else
            {
                modfetch.GoSumFile = strings.TrimSuffix(ModFilePath(), ".mod") + ".sum";
                search.SetModRoot(modRoot);
            }

        }

        private static void init()
        {
            load.ModInit = Init;
        }

        // WillBeEnabled checks whether modules should be enabled but does not
        // initialize modules by installing hooks. If Init has already been called,
        // WillBeEnabled returns the same result as Enabled.
        //
        // This function is needed to break a cycle. The main package needs to know
        // whether modules are enabled in order to install the module or GOPATH version
        // of 'go get', but Init reads the -modfile flag in 'go get', so it shouldn't
        // be called until the command is installed and flags are parsed. Instead of
        // calling Init and Enabled, the main package can call this function.
        public static bool WillBeEnabled()
        {
            if (modRoot != "" || mustUseModules)
            {
                return true;
            }

            if (initialized)
            {
                return false;
            } 

            // Keep in sync with Init. Init does extra validation and prints warnings or
            // exits, so it can't call this function directly.
            var env = cfg.Getenv("GO111MODULE");
            switch (env)
            {
                case "on": 
                    return true;
                    break;
                case "auto": 

                case "": 
                    break;
                    break;
                default: 
                    return false;
                    break;
            }

            if (CmdModInit)
            { 
                // Running 'go mod init': go.mod will be created in current directory.
                return true;

            }

            {
                var modRoot = findModuleRoot(@base.Cwd);

                if (modRoot == "")
                { 
                    // GO111MODULE is 'auto', and we can't find a module root.
                    // Stay in GOPATH mode.
                    return false;

                }
                else if (search.InDir(modRoot, os.TempDir()) == ".")
                { 
                    // If you create /tmp/go.mod for experimenting,
                    // then any tests that create work directories under /tmp
                    // will find it and get modules when they're not expecting them.
                    // It's a bit of a peculiar thing to disallow but quite mysterious
                    // when it happens. See golang.org/issue/26708.
                    return false;

                }


            }

            return true;

        }

        // Enabled reports whether modules are (or must be) enabled.
        // If modules are enabled but there is no main module, Enabled returns true
        // and then the first use of module information will call die
        // (usually through MustModRoot).
        public static bool Enabled()
        {
            Init();
            return modRoot != "" || mustUseModules;
        }

        // ModRoot returns the root of the main module.
        // It calls base.Fatalf if there is no main module.
        public static @string ModRoot()
        {
            if (!HasModRoot())
            {
                die();
            }

            return modRoot;

        }

        // HasModRoot reports whether a main module is present.
        // HasModRoot may return false even if Enabled returns true: for example, 'get'
        // does not require a main module.
        public static bool HasModRoot()
        {
            Init();
            return modRoot != "";
        }

        // ModFilePath returns the effective path of the go.mod file. Normally, this
        // "go.mod" in the directory returned by ModRoot, but the -modfile flag may
        // change its location. ModFilePath calls base.Fatalf if there is no main
        // module, even if -modfile is set.
        public static @string ModFilePath()
        {
            if (!HasModRoot())
            {
                die();
            }

            if (cfg.ModFile != "")
            {
                return cfg.ModFile;
            }

            return filepath.Join(modRoot, "go.mod");

        }

        // printStackInDie causes die to print a stack trace.
        //
        // It is enabled by the testgo tag, and helps to diagnose paths that
        // unexpectedly require a main module.
        private static var printStackInDie = false;

        private static void die()
        {
            if (printStackInDie)
            {
                debug.PrintStack();
            }

            if (cfg.Getenv("GO111MODULE") == "off")
            {
                @base.Fatalf("go: modules disabled by GO111MODULE=off; see 'go help modules'");
            }

            {
                var (dir, name) = findAltConfig(@base.Cwd);

                if (dir != "")
                {
                    var (rel, err) = filepath.Rel(@base.Cwd, dir);
                    if (err != null)
                    {
                        rel = dir;
                    }

                    @string cdCmd = "";
                    if (rel != ".")
                    {
                        cdCmd = fmt.Sprintf("cd %s && ", rel);
                    }

                    @base.Fatalf("go: cannot find main module, but found %s in %s\n\tto create a module there, run:\n\t%sgo mod init", name, dir, cdCmd);

                }

            }

            @base.Fatalf("go: cannot find main module; see 'go help modules'");

        }

        // InitMod sets Target and, if there is a main module, parses the initial build
        // list from its go.mod file, creating and populating that file if needed.
        //
        // As a side-effect, InitMod sets a default for cfg.BuildMod if it does not
        // already have an explicit value.
        public static void InitMod()
        {
            if (len(buildList) > 0L)
            {
                return ;
            }

            Init();
            if (modRoot == "")
            {
                Target = new module.Version(Path:"command-line-arguments");
                targetPrefix = "command-line-arguments";
                buildList = new slice<module.Version>(new module.Version[] { Target });
                return ;
            }

            if (CmdModInit)
            { 
                // Running go mod init: do legacy module conversion
                legacyModInit();
                modFileToBuildList();
                WriteGoMod();
                return ;

            }

            var gomod = ModFilePath();
            var (data, err) = lockedfile.Read(gomod);
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            ref bool @fixed = ref heap(out ptr<bool> _addr_@fixed);
            var (f, err) = modfile.Parse(gomod, data, fixVersion(_addr_fixed));
            if (err != null)
            { 
                // Errors returned by modfile.Parse begin with file:line.
                @base.Fatalf("go: errors parsing go.mod:\n%s\n", err);

            }

            modFile = f;
            index = indexModFile(data, f, fixed);

            if (len(f.Syntax.Stmt) == 0L || f.Module == null)
            { 
                // Empty mod file. Must add module path.
                var (path, err) = findModulePath(modRoot);
                if (err != null)
                {
                    @base.Fatalf("go: %v", err);
                }

                f.AddModuleStmt(path);

            }

            if (len(f.Syntax.Stmt) == 1L && f.Module != null)
            { 
                // Entire file is just a module statement.
                // Populate require if possible.
                legacyModInit();

            }

            modFileToBuildList();
            setDefaultBuildMod();
            if (cfg.BuildMod == "vendor")
            {
                readVendorList();
                checkVendorConsistency();
            }
            else
            { 
                // TODO(golang.org/issue/33326): if cfg.BuildMod != "readonly"?
                WriteGoMod();

            }

        }

        // fixVersion returns a modfile.VersionFixer implemented using the Query function.
        //
        // It resolves commit hashes and branch names to versions,
        // canonicalizes versions that appeared in early vgo drafts,
        // and does nothing for versions that already appear to be canonical.
        //
        // The VersionFixer sets 'fixed' if it ever returns a non-canonical version.
        private static modfile.VersionFixer fixVersion(ptr<bool> _addr_@fixed) => func((defer, _, __) =>
        {
            ref bool @fixed = ref _addr_@fixed.val;

            return (path, vers) =>
            {
                defer(() =>
                {
                    if (err == null && resolved != vers)
                    {
                        fixed.val = true;
                    }

                }()); 

                // Special case: remove the old -gopkgin- hack.
                if (strings.HasPrefix(path, "gopkg.in/") && strings.Contains(vers, "-gopkgin-"))
                {
                    vers = vers[strings.Index(vers, "-gopkgin-") + len("-gopkgin-")..];
                } 

                // fixVersion is called speculatively on every
                // module, version pair from every go.mod file.
                // Avoid the query if it looks OK.
                var (_, pathMajor, ok) = module.SplitPathVersion(path);
                if (!ok)
                {
                    return ("", addr(new module.ModuleError(Path:path,Err:&module.InvalidVersionError{Version:vers,Err:fmt.Errorf("malformed module path %q",path),},)));
                }

                if (vers != "" && module.CanonicalVersion(vers) == vers)
                {
                    {
                        var err = module.CheckPathMajor(vers, pathMajor);

                        if (err == null)
                        {
                            return (vers, null);
                        }

                    }

                }

                var (info, err) = Query(path, vers, "", null);
                if (err != null)
                {
                    return ("", err);
                }

                return (info.Version, null);

            };

        });

        // AllowMissingModuleImports allows import paths to be resolved to modules
        // when there is no module root. Normally, this is forbidden because it's slow
        // and there's no way to make the result reproducible, but some commands
        // like 'go get' are expected to do this.
        public static void AllowMissingModuleImports()
        {
            allowMissingModuleImports = true;
        }

        // modFileToBuildList initializes buildList from the modFile.
        private static void modFileToBuildList()
        {
            Target = modFile.Module.Mod;
            targetPrefix = Target.Path;
            {
                var rel = search.InDir(@base.Cwd, cfg.GOROOTsrc);

                if (rel != "")
                {
                    targetInGorootSrc = true;
                    if (Target.Path == "std")
                    {
                        targetPrefix = "";
                    }

                }

            }


            module.Version list = new slice<module.Version>(new module.Version[] { Target });
            foreach (var (_, r) in modFile.Require)
            {
                list = append(list, r.Mod);
            }
            buildList = list;

        }

        // setDefaultBuildMod sets a default value for cfg.BuildMod
        // if it is currently empty.
        private static void setDefaultBuildMod()
        {
            if (cfg.BuildMod != "")
            { 
                // Don't override an explicit '-mod=' argument.
                return ;

            }

            cfg.BuildMod = "mod";
            if (cfg.CmdName == "get" || strings.HasPrefix(cfg.CmdName, "mod "))
            { 
                // Don't set -mod implicitly for commands whose purpose is to
                // manipulate the build list.
                return ;

            }

            if (modRoot == "")
            {
                return ;
            }

            {
                var fi__prev1 = fi;

                var (fi, err) = os.Stat(filepath.Join(modRoot, "vendor"));

                if (err == null && fi.IsDir())
                {
                    @string modGo = "unspecified";
                    if (index.goVersion != "")
                    {
                        if (semver.Compare("v" + index.goVersion, "v1.14") >= 0L)
                        { 
                            // The Go version is at least 1.14, and a vendor directory exists.
                            // Set -mod=vendor by default.
                            cfg.BuildMod = "vendor";
                            cfg.BuildModReason = "Go version in go.mod is at least 1.14 and vendor directory exists.";
                            return ;

                        }
                        else
                        {
                            modGo = index.goVersion;
                        }

                    } 

                    // Since a vendor directory exists, we have a non-trivial reason for
                    // choosing -mod=mod, although it probably won't be used for anything.
                    // Record the reason anyway for consistency.
                    // It may be overridden if we switch to mod=readonly below.
                    cfg.BuildModReason = fmt.Sprintf("Go version in go.mod is %s.", modGo);

                }

                fi = fi__prev1;

            }


            var p = ModFilePath();
            {
                var fi__prev1 = fi;

                (fi, err) = os.Stat(p);

                if (err == null && !hasWritePerm(p, fi))
                {
                    cfg.BuildMod = "readonly";
                    cfg.BuildModReason = "go.mod file is read-only.";
                }

                fi = fi__prev1;

            }

        }

        private static void legacyModInit()
        {
            if (modFile == null)
            {
                var (path, err) = findModulePath(modRoot);
                if (err != null)
                {
                    @base.Fatalf("go: %v", err);
                }

                fmt.Fprintf(os.Stderr, "go: creating new go.mod: module %s\n", path);
                modFile = @new<modfile.File>();
                modFile.AddModuleStmt(path);
                addGoStmt(); // Add the go directive before converted module requirements.
            }

            foreach (var (_, name) in altConfigs)
            {
                var cfg = filepath.Join(modRoot, name);
                var (data, err) = ioutil.ReadFile(cfg);
                if (err == null)
                {
                    var convert = modconv.Converters[name];
                    if (convert == null)
                    {
                        return ;
                    }

                    fmt.Fprintf(os.Stderr, "go: copying requirements from %s\n", @base.ShortPath(cfg));
                    cfg = filepath.ToSlash(cfg);
                    {
                        var err = modconv.ConvertLegacyConfig(modFile, cfg, data);

                        if (err != null)
                        {
                            @base.Fatalf("go: %v", err);
                        }

                    }

                    if (len(modFile.Syntax.Stmt) == 1L)
                    { 
                        // Add comment to avoid re-converting every time it runs.
                        modFile.AddComment("// go: no requirements found in " + name);

                    }

                    return ;

                }

            }

        }

        // addGoStmt adds a go directive to the go.mod file if it does not already include one.
        // The 'go' version added, if any, is the latest version supported by this toolchain.
        private static void addGoStmt()
        {
            if (modFile.Go != null && modFile.Go.Version != "")
            {
                return ;
            }

            var tags = build.Default.ReleaseTags;
            var version = tags[len(tags) - 1L];
            if (!strings.HasPrefix(version, "go") || !modfile.GoVersionRE.MatchString(version[2L..]))
            {
                @base.Fatalf("go: unrecognized default version %q", version);
            }

            {
                var err = modFile.AddGoStmt(version[2L..]);

                if (err != null)
                {
                    @base.Fatalf("go: internal error: %v", err);
                }

            }

        }

        private static @string altConfigs = new slice<@string>(new @string[] { "Gopkg.lock", "GLOCKFILE", "Godeps/Godeps.json", "dependencies.tsv", "glide.lock", "vendor.conf", "vendor.yml", "vendor/manifest", "vendor/vendor.json", ".git/config" });

        private static @string findModuleRoot(@string dir) => func((_, panic, __) =>
        {
            @string root = default;

            if (dir == "")
            {
                panic("dir not set");
            }

            dir = filepath.Clean(dir); 

            // Look for enclosing go.mod.
            while (true)
            {
                {
                    var (fi, err) = os.Stat(filepath.Join(dir, "go.mod"));

                    if (err == null && !fi.IsDir())
                    {
                        return dir;
                    }

                }

                var d = filepath.Dir(dir);
                if (d == dir)
                {
                    break;
                }

                dir = d;

            }

            return "";

        });

        private static (@string, @string) findAltConfig(@string dir) => func((_, panic, __) =>
        {
            @string root = default;
            @string name = default;

            if (dir == "")
            {
                panic("dir not set");
            }

            dir = filepath.Clean(dir);
            {
                var rel = search.InDir(dir, cfg.BuildContext.GOROOT);

                if (rel != "")
                { 
                    // Don't suggest creating a module from $GOROOT/.git/config
                    // or a config file found in any parent of $GOROOT (see #34191).
                    return ("", "");

                }

            }

            while (true)
            {
                foreach (var (_, name) in altConfigs)
                {
                    {
                        var (fi, err) = os.Stat(filepath.Join(dir, name));

                        if (err == null && !fi.IsDir())
                        {
                            return (dir, name);
                        }

                    }

                }
                var d = filepath.Dir(dir);
                if (d == dir)
                {
                    break;
                }

                dir = d;

            }

            return ("", "");

        });

        private static (@string, error) findModulePath(@string dir)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (CmdModModule != "")
            { 
                // Running go mod init x/y/z; return x/y/z.
                {
                    var err = module.CheckImportPath(CmdModModule);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                }

                return (CmdModModule, error.As(null!)!);

            } 

            // TODO(bcmills): once we have located a plausible module path, we should
            // query version control (if available) to verify that it matches the major
            // version of the most recent tag.
            // See https://golang.org/issue/29433, https://golang.org/issue/27009, and
            // https://golang.org/issue/31549.

            // Cast about for import comments,
            // first in top-level directory, then in subdirectories.
            var (list, _) = ioutil.ReadDir(dir);
            foreach (var (_, info) in list)
            {
                if (info.Mode().IsRegular() && strings.HasSuffix(info.Name(), ".go"))
                {
                    {
                        var com__prev2 = com;

                        var com = findImportComment(filepath.Join(dir, info.Name()));

                        if (com != "")
                        {
                            return (com, error.As(null!)!);
                        }

                        com = com__prev2;

                    }

                }

            }
            foreach (var (_, info1) in list)
            {
                if (info1.IsDir())
                {
                    var (files, _) = ioutil.ReadDir(filepath.Join(dir, info1.Name()));
                    foreach (var (_, info2) in files)
                    {
                        if (info2.Mode().IsRegular() && strings.HasSuffix(info2.Name(), ".go"))
                        {
                            {
                                var com__prev3 = com;

                                com = findImportComment(filepath.Join(dir, info1.Name(), info2.Name()));

                                if (com != "")
                                {
                                    return (path.Dir(com), error.As(null!)!);
                                }

                                com = com__prev3;

                            }

                        }

                    }

                }

            } 

            // Look for Godeps.json declaring import path.
            var (data, _) = ioutil.ReadFile(filepath.Join(dir, "Godeps/Godeps.json"));
            ref var cfg1 = ref heap(out ptr<var> _addr_cfg1);
            json.Unmarshal(data, _addr_cfg1);
            if (cfg1.ImportPath != "")
            {
                return (cfg1.ImportPath, error.As(null!)!);
            } 

            // Look for vendor.json declaring import path.
            data, _ = ioutil.ReadFile(filepath.Join(dir, "vendor/vendor.json"));
            ref var cfg2 = ref heap(out ptr<var> _addr_cfg2);
            json.Unmarshal(data, _addr_cfg2);
            if (cfg2.RootPath != "")
            {
                return (cfg2.RootPath, error.As(null!)!);
            } 

            // Look for path in GOPATH.
            foreach (var (_, gpdir) in filepath.SplitList(cfg.BuildContext.GOPATH))
            {
                if (gpdir == "")
                {
                    continue;
                }

                {
                    var rel = search.InDir(dir, filepath.Join(gpdir, "src"));

                    if (rel != "" && rel != ".")
                    {
                        return (filepath.ToSlash(rel), error.As(null!)!);
                    }

                }

            }
            @string msg = @"cannot determine module path for source directory %s (outside GOPATH, module path must be specified)

Example usage:
	'go mod init example.com/m' to initialize a v0 or v1 module
	'go mod init example.com/m/v2' to initialize a v2 module

Run 'go help mod init' for more information.
";
            return ("", error.As(fmt.Errorf(msg, dir))!);

        }

        private static var importCommentRE = lazyregexp.New("(?m)^package[ \\t]+[^ \\t\\r\\n/]+[ \\t]+//[ \\t]+import[ \\t]+(\\\"[^\"]+\\\")[ \\t]*\\r?\\n");

        private static @string findImportComment(@string file)
        {
            var (data, err) = ioutil.ReadFile(file);
            if (err != null)
            {
                return "";
            }

            var m = importCommentRE.FindSubmatch(data);
            if (m == null)
            {
                return "";
            }

            var (path, err) = strconv.Unquote(string(m[1L]));
            if (err != null)
            {
                return "";
            }

            return path;

        }

        private static var allowWriteGoMod = true;

        // DisallowWriteGoMod causes future calls to WriteGoMod to do nothing at all.
        public static void DisallowWriteGoMod()
        {
            allowWriteGoMod = false;
        }

        // AllowWriteGoMod undoes the effect of DisallowWriteGoMod:
        // future calls to WriteGoMod will update go.mod if needed.
        // Note that any past calls have been discarded, so typically
        // a call to AlowWriteGoMod should be followed by a call to WriteGoMod.
        public static void AllowWriteGoMod()
        {
            allowWriteGoMod = true;
        }

        // MinReqs returns a Reqs with minimal additional dependencies of Target,
        // as will be written to go.mod.
        public static mvs.Reqs MinReqs()
        {
            slice<@string> retain = default;
            foreach (var (_, m) in buildList[1L..])
            {
                var (_, explicit) = index.require[m];
                if (explicit || loaded.direct[m.Path])
                {
                    retain = append(retain, m.Path);
                }

            }
            var (min, err) = mvs.Req(Target, retain, Reqs());
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            return addr(new mvsReqs(buildList:append([]module.Version{Target},min...)));

        }

        // WriteGoMod writes the current build list back to go.mod.
        public static void WriteGoMod() => func((defer, _, __) =>
        { 
            // If we're using -mod=vendor we basically ignored
            // go.mod, so definitely don't try to write back our
            // incomplete view of the world.
            if (!allowWriteGoMod || cfg.BuildMod == "vendor")
            {
                return ;
            } 

            // If we aren't in a module, we don't have anywhere to write a go.mod file.
            if (modRoot == "")
            {
                return ;
            }

            if (cfg.BuildMod != "readonly")
            {
                addGoStmt();
            }

            if (loaded != null)
            {
                var reqs = MinReqs();
                var (min, err) = reqs.Required(Target);
                if (err != null)
                {
                    @base.Fatalf("go: %v", err);
                }

                slice<ptr<modfile.Require>> list = default;
                foreach (var (_, m) in min)
                {
                    list = append(list, addr(new modfile.Require(Mod:m,Indirect:!loaded.direct[m.Path],)));
                }
                modFile.SetRequire(list);

            }

            modFile.Cleanup();

            var dirty = index.modFileIsDirty(modFile);
            if (dirty && cfg.BuildMod == "readonly")
            { 
                // If we're about to fail due to -mod=readonly,
                // prefer to report a dirty go.mod over a dirty go.sum
                if (cfg.BuildModReason != "")
                {
                    @base.Fatalf("go: updates to go.mod needed, disabled by -mod=readonly\n\t(%s)", cfg.BuildModReason);
                }
                else
                {
                    @base.Fatalf("go: updates to go.mod needed, disabled by -mod=readonly");
                }

            } 
            // Always update go.sum, even if we didn't change go.mod: we may have
            // downloaded modules that we didn't have before.
            modfetch.WriteGoSum();

            if (!dirty && cfg.CmdName != "mod tidy")
            { 
                // The go.mod file has the same semantic content that it had before
                // (but not necessarily the same exact bytes).
                // Ignore any intervening edits.
                return ;

            }

            var (new, err) = modFile.Format();
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            defer(() =>
            { 
                // At this point we have determined to make the go.mod file on disk equal to new.
                index = indexModFile(new, modFile, false);

            }()); 

            // Make a best-effort attempt to acquire the side lock, only to exclude
            // previous versions of the 'go' command from making simultaneous edits.
            {
                var (unlock, err) = modfetch.SideLock();

                if (err == null)
                {
                    defer(unlock());
                }

            }


            var errNoChange = errors.New("no update needed");

            err = lockedfile.Transform(ModFilePath(), old =>
            {
                if (bytes.Equal(old, new))
                { 
                    // The go.mod file is already equal to new, possibly as the result of some
                    // other process.
                    return (null, errNoChange);

                }

                if (index != null && !bytes.Equal(old, index.data))
                { 
                    // The contents of the go.mod file have changed. In theory we could add all
                    // of the new modules to the build list, recompute, and check whether any
                    // module in *our* build list got bumped to a different version, but that's
                    // a lot of work for marginal benefit. Instead, fail the command: if users
                    // want to run concurrent commands, they need to start with a complete,
                    // consistent module definition.
                    return (null, fmt.Errorf("existing contents have changed since last read"));

                }

                return (new, null);

            });

            if (err != null && err != errNoChange)
            {
                @base.Fatalf("go: updating go.mod: %v", err);
            }

        });
    }
}}}}
