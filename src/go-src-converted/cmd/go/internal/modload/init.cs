// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:32 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\init.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using context = context_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using build = go.build_package;
using lazyregexp = @internal.lazyregexp_package;
using os = os_package;
using path = path_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using lockedfile = cmd.go.@internal.lockedfile_package;
using modconv = cmd.go.@internal.modconv_package;
using modfetch = cmd.go.@internal.modfetch_package;
using search = cmd.go.@internal.search_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// Variables set by other packages.
//
// TODO(#40775): See if these can be plumbed as explicit parameters.

using System;
public static partial class modload_package {

 
// RootMode determines whether a module root is needed.
public static Root RootMode = default;public static bool ForceUseModules = default;private static bool allowMissingModuleImports = default;

// Variables set in Init.
private static bool initialized = default;private static @string modRoot = default;private static @string gopath = default;

// Variables set in initTarget (during {Load,Create}ModFile).
public static module.Version Target = default;private static @string targetPrefix = default;private static bool targetInGorootSrc = default;

public partial struct Root { // : nint
}

 
// AutoRoot is the default for most commands. modload.Init will look for
// a go.mod file in the current directory or any parent. If none is found,
// modules may be disabled (GO111MODULE=auto) or commands may run in a
// limited module mode.
public static readonly Root AutoRoot = iota; 

// NoRoot is used for commands that run in module mode and ignore any go.mod
// file the current directory or in parent directories.
public static readonly var NoRoot = 0; 

// NeedRoot is used for commands that must run in module mode and don't
// make sense without a main module.
public static readonly var NeedRoot = 1;

// ModFile returns the parsed go.mod file.
//
// Note that after calling LoadPackages or LoadModGraph,
// the require statements in the modfile.File are no longer
// the source of truth and will be ignored: edits made directly
// will be lost at the next call to WriteGoMod.
// To make permanent changes to the require statements
// in go.mod, edit it before loading.
public static ptr<modfile.File> ModFile() {
    Init();
    if (modFile == null) {
        die();
    }
    return _addr_modFile!;
}

public static @string BinDir() {
    Init();
    return filepath.Join(gopath, "bin");
}

// Init determines whether module mode is enabled, locates the root of the
// current module (if any), sets environment variables for Git subprocesses, and
// configures the cfg, codehost, load, modfetch, and search packages for use
// with modules.
public static void Init() {
    if (initialized) {
        return ;
    }
    initialized = true; 

    // Keep in sync with WillBeEnabled. We perform extra validation here, and
    // there are lots of diagnostics and side effects, so we can't use
    // WillBeEnabled directly.
    bool mustUseModules = default;
    var env = cfg.Getenv("GO111MODULE");
    switch (env) {
        case "auto": 
            mustUseModules = ForceUseModules;
            break;
        case "on": 

        case "": 
            mustUseModules = true;
            break;
        case "off": 
            if (ForceUseModules) {
                @base.Fatalf("go: modules disabled by GO111MODULE=off; see 'go help modules'");
            }
            mustUseModules = false;
            return ;
            break;
        default: 
            @base.Fatalf("go: unknown environment setting GO111MODULE=%s", env);
            break;
    }

    {
        var err = fsys.Init(@base.Cwd());

        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
    } 

    // Disable any prompting for passwords by Git.
    // Only has an effect for 2.3.0 or later, but avoiding
    // the prompt in earlier versions is just too hard.
    // If user has explicitly set GIT_TERMINAL_PROMPT=1, keep
    // prompting.
    // See golang.org/issue/9341 and golang.org/issue/12706.
    if (os.Getenv("GIT_TERMINAL_PROMPT") == "") {
        os.Setenv("GIT_TERMINAL_PROMPT", "0");
    }
    if (os.Getenv("GIT_SSH") == "" && os.Getenv("GIT_SSH_COMMAND") == "") {
        os.Setenv("GIT_SSH_COMMAND", "ssh -o ControlMaster=no -o BatchMode=yes");
    }
    if (os.Getenv("GCM_INTERACTIVE") == "") {
        os.Setenv("GCM_INTERACTIVE", "never");
    }
    if (modRoot != "") { 
        // modRoot set before Init was called ("go mod init" does this).
        // No need to search for go.mod.
    }
    else if (RootMode == NoRoot) {
        if (cfg.ModFile != "" && !@base.InGOFLAGS("-modfile")) {
            @base.Fatalf("go: -modfile cannot be used with commands that ignore the current module");
        }
        modRoot = "";
    }
    else
 {
        modRoot = findModuleRoot(@base.Cwd());
        if (modRoot == "") {
            if (cfg.ModFile != "") {
                @base.Fatalf("go: cannot find main module, but -modfile was set.\n\t-modfile cannot be used to set the module root directory.");
            }
            if (RootMode == NeedRoot) {
                @base.Fatalf("go: %v", ErrNoModRoot);
            }
            if (!mustUseModules) { 
                // GO111MODULE is 'auto', and we can't find a module root.
                // Stay in GOPATH mode.
                return ;
            }
        }
        else if (search.InDir(modRoot, os.TempDir()) == ".") { 
            // If you create /tmp/go.mod for experimenting,
            // then any tests that create work directories under /tmp
            // will find it and get modules when they're not expecting them.
            // It's a bit of a peculiar thing to disallow but quite mysterious
            // when it happens. See golang.org/issue/26708.
            modRoot = "";
            fmt.Fprintf(os.Stderr, "go: warning: ignoring go.mod in system temp root %v\n", os.TempDir());
            if (!mustUseModules) {
                return ;
            }
        }
    }
    if (cfg.ModFile != "" && !strings.HasSuffix(cfg.ModFile, ".mod")) {
        @base.Fatalf("go: -modfile=%s: file does not have .mod extension", cfg.ModFile);
    }
    cfg.ModulesEnabled = true;
    setDefaultBuildMod();
    var list = filepath.SplitList(cfg.BuildContext.GOPATH);
    if (len(list) == 0 || list[0] == "") {
        @base.Fatalf("missing $GOPATH");
    }
    gopath = list[0];
    {
        var (_, err) = fsys.Stat(filepath.Join(gopath, "go.mod"));

        if (err == null) {
            @base.Fatalf("$GOPATH/go.mod exists but should not");
        }
    }

    if (modRoot == "") { 
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

// WillBeEnabled checks whether modules should be enabled but does not
// initialize modules by installing hooks. If Init has already been called,
// WillBeEnabled returns the same result as Enabled.
//
// This function is needed to break a cycle. The main package needs to know
// whether modules are enabled in order to install the module or GOPATH version
// of 'go get', but Init reads the -modfile flag in 'go get', so it shouldn't
// be called until the command is installed and flags are parsed. Instead of
// calling Init and Enabled, the main package can call this function.
public static bool WillBeEnabled() {
    if (modRoot != "" || cfg.ModulesEnabled) { 
        // Already enabled.
        return true;
    }
    if (initialized) { 
        // Initialized, not enabled.
        return false;
    }
    var env = cfg.Getenv("GO111MODULE");
    switch (env) {
        case "on": 

        case "": 
            return true;
            break;
        case "auto": 
            break;
            break;
        default: 
            return false;
            break;
    }

    {
        var modRoot = findModuleRoot(@base.Cwd());

        if (modRoot == "") { 
            // GO111MODULE is 'auto', and we can't find a module root.
            // Stay in GOPATH mode.
            return false;
        }
        else if (search.InDir(modRoot, os.TempDir()) == ".") { 
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
public static bool Enabled() {
    Init();
    return modRoot != "" || cfg.ModulesEnabled;
}

// ModRoot returns the root of the main module.
// It calls base.Fatalf if there is no main module.
public static @string ModRoot() {
    if (!HasModRoot()) {
        die();
    }
    return modRoot;
}

// HasModRoot reports whether a main module is present.
// HasModRoot may return false even if Enabled returns true: for example, 'get'
// does not require a main module.
public static bool HasModRoot() {
    Init();
    return modRoot != "";
}

// ModFilePath returns the effective path of the go.mod file. Normally, this
// "go.mod" in the directory returned by ModRoot, but the -modfile flag may
// change its location. ModFilePath calls base.Fatalf if there is no main
// module, even if -modfile is set.
public static @string ModFilePath() {
    if (!HasModRoot()) {
        die();
    }
    if (cfg.ModFile != "") {
        return cfg.ModFile;
    }
    return filepath.Join(modRoot, "go.mod");
}

private static void die() {
    if (cfg.Getenv("GO111MODULE") == "off") {
        @base.Fatalf("go: modules disabled by GO111MODULE=off; see 'go help modules'");
    }
    {
        var (dir, name) = findAltConfig(@base.Cwd());

        if (dir != "") {
            var (rel, err) = filepath.Rel(@base.Cwd(), dir);
            if (err != null) {
                rel = dir;
            }
            @string cdCmd = "";
            if (rel != ".") {
                cdCmd = fmt.Sprintf("cd %s && ", rel);
            }
            @base.Fatalf("go: cannot find main module, but found %s in %s\n\tto create a module there, run:\n\t%sgo mod init", name, dir, cdCmd);
        }
    }
    @base.Fatalf("go: %v", ErrNoModRoot);
}

public static var ErrNoModRoot = errors.New("go.mod file not found in current directory or any parent directory; see 'go help modules'");

private partial struct goModDirtyError {
}

private static @string Error(this goModDirtyError _p0) {
    if (cfg.BuildModExplicit) {
        return fmt.Sprintf("updates to go.mod needed, disabled by -mod=%v; to update it:\n\tgo mod tidy", cfg.BuildMod);
    }
    if (cfg.BuildModReason != "") {
        return fmt.Sprintf("updates to go.mod needed, disabled by -mod=%s\n\t(%s)\n\tto update it:\n\tgo mod tidy", cfg.BuildMod, cfg.BuildModReason);
    }
    return "updates to go.mod needed; to update it:\n\tgo mod tidy";
}

private static error errGoModDirty = error.As(new goModDirtyError())!;

// LoadModFile sets Target and, if there is a main module, parses the initial
// build list from its go.mod file.
//
// LoadModFile may make changes in memory, like adding a go directive and
// ensuring requirements are consistent, and will write those changes back to
// disk unless DisallowWriteGoMod is in effect.
//
// As a side-effect, LoadModFile may change cfg.BuildMod to "vendor" if
// -mod wasn't set explicitly and automatic vendoring should be enabled.
//
// If LoadModFile or CreateModFile has already been called, LoadModFile returns
// the existing in-memory requirements (rather than re-reading them from disk).
//
// LoadModFile checks the roots of the module graph for consistency with each
// other, but unlike LoadModGraph does not load the full module graph or check
// it for global consistency. Most callers outside of the modload package should
// use LoadModGraph instead.
public static ptr<Requirements> LoadModFile(context.Context ctx) {
    var (rs, needCommit) = loadModFile(ctx);
    if (needCommit) {
        commitRequirements(ctx, modFileGoVersion(), _addr_rs);
    }
    return _addr_rs!;
}

// loadModFile is like LoadModFile, but does not implicitly commit the
// requirements back to disk after fixing inconsistencies.
//
// If needCommit is true, after the caller makes any other needed changes to the
// returned requirements they should invoke commitRequirements to fix any
// inconsistencies that may be present in the on-disk go.mod file.
private static (ptr<Requirements>, bool) loadModFile(context.Context ctx) {
    ptr<Requirements> rs = default!;
    bool needCommit = default;

    if (requirements != null) {
        return (_addr_requirements!, false);
    }
    Init();
    if (modRoot == "") {
        Target = new module.Version(Path:"command-line-arguments");
        targetPrefix = "command-line-arguments";
        var goVersion = LatestGoVersion();
        rawGoVersion.Store(Target, goVersion);
        requirements = newRequirements(modDepthFromGoVersion(goVersion), null, null);
        return (_addr_requirements!, false);
    }
    var gomod = ModFilePath();
    slice<byte> data = default;
    error err = default!;
    {
        var (gomodActual, ok) = fsys.OverlayPath(gomod);

        if (ok) { 
            // Don't lock go.mod if it's part of the overlay.
            // On Plan 9, locking requires chmod, and we don't want to modify any file
            // in the overlay. See #44700.
            data, err = os.ReadFile(gomodActual);
        }
        else
 {
            data, err = lockedfile.Read(gomodActual);
        }
    }
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    ref bool @fixed = ref heap(out ptr<bool> _addr_@fixed);
    var (f, err) = modfile.Parse(gomod, data, fixVersion(ctx, _addr_fixed));
    if (err != null) { 
        // Errors returned by modfile.Parse begin with file:line.
        @base.Fatalf("go: errors parsing go.mod:\n%s\n", err);
    }
    if (f.Module == null) { 
        // No module declaration. Must add module path.
        @base.Fatalf("go: no module declaration in go.mod. To specify the module path:\n\tgo mod edit -module=example.com/mod");
    }
    modFile = f;
    initTarget(f.Module.Mod);
    index = indexModFile(data, f, fixed);

    {
        error err__prev1 = err;

        err = module.CheckImportPath(f.Module.Mod.Path);

        if (err != null) {
            {
                ptr<module.InvalidPathError> (pathErr, ok) = err._<ptr<module.InvalidPathError>>();

                if (ok) {
                    pathErr.Kind = "module";
                }

            }
            @base.Fatalf("go: %v", err);
        }
        err = err__prev1;

    }

    setDefaultBuildMod(); // possibly enable automatic vendoring
    rs = requirementsFromModFile();
    if (cfg.BuildMod == "vendor") {
        readVendorList();
        checkVendorConsistency();
        rs.initVendor(vendorList);
    }
    if (rs.hasRedundantRoot()) { 
        // If any module path appears more than once in the roots, we know that the
        // go.mod file needs to be updated even though we have not yet loaded any
        // transitive dependencies.
        rs, err = updateRoots(ctx, rs.direct, rs, null, null, false);
        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
    }
    if (index.goVersionV == "") { 
        // TODO(#45551): Do something more principled instead of checking
        // cfg.CmdName directly here.
        if (cfg.BuildMod == "mod" && cfg.CmdName != "mod graph" && cfg.CmdName != "mod why") {
            addGoStmt(LatestGoVersion());
            if (go117EnableLazyLoading) { 
                // We need to add a 'go' version to the go.mod file, but we must assume
                // that its existing contents match something between Go 1.11 and 1.16.
                // Go 1.11 through 1.16 have eager requirements, but the latest Go
                // version uses lazy requirements instead — so we need to cnvert the
                // requirements to be lazy.
                rs, err = convertDepth(ctx, rs, lazy);
                if (err != null) {
                    @base.Fatalf("go: %v", err);
                }
            }
        }
        else
 {
            rawGoVersion.Store(Target, modFileGoVersion());
        }
    }
    requirements = rs;
    return (_addr_requirements!, true);
}

// CreateModFile initializes a new module by creating a go.mod file.
//
// If modPath is empty, CreateModFile will attempt to infer the path from the
// directory location within GOPATH.
//
// If a vendoring configuration file is present, CreateModFile will attempt to
// translate it to go.mod directives. The resulting build list may not be
// exactly the same as in the legacy configuration (for example, we can't get
// packages at multiple versions from the same module).
public static void CreateModFile(context.Context ctx, @string modPath) {
    modRoot = @base.Cwd();
    Init();
    var modFilePath = ModFilePath();
    {
        error err__prev1 = err;

        var (_, err) = fsys.Stat(modFilePath);

        if (err == null) {
            @base.Fatalf("go: %s already exists", modFilePath);
        }
        err = err__prev1;

    }

    if (modPath == "") {
        error err = default!;
        modPath, err = findModulePath(modRoot);
        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
    }    {
        error err__prev2 = err;

        err = module.CheckImportPath(modPath);


        else if (err != null) {
            {
                ptr<module.InvalidPathError> (pathErr, ok) = err._<ptr<module.InvalidPathError>>();

                if (ok) {
                    pathErr.Kind = "module"; 
                    // Same as build.IsLocalPath()
                    if (pathErr.Path == "." || pathErr.Path == ".." || strings.HasPrefix(pathErr.Path, "./") || strings.HasPrefix(pathErr.Path, "../")) {
                        pathErr.Err = errors.New("is a local import path");
                    }
                }

            }
            @base.Fatalf("go: %v", err);
        }
        err = err__prev2;

    }

    fmt.Fprintf(os.Stderr, "go: creating new go.mod: module %s\n", modPath);
    modFile = @new<modfile.File>();
    modFile.AddModuleStmt(modPath);
    initTarget(modFile.Module.Mod);
    addGoStmt(LatestGoVersion()); // Add the go directive before converted module requirements.

    var (convertedFrom, err) = convertLegacyConfig(modPath);
    if (convertedFrom != "") {
        fmt.Fprintf(os.Stderr, "go: copying requirements from %s\n", @base.ShortPath(convertedFrom));
    }
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    var rs = requirementsFromModFile();
    rs, err = updateRoots(ctx, rs.direct, rs, null, null, false);
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    commitRequirements(ctx, modFileGoVersion(), _addr_rs); 

    // Suggest running 'go mod tidy' unless the project is empty. Even if we
    // imported all the correct requirements above, we're probably missing
    // some sums, so the next build command in -mod=readonly will likely fail.
    //
    // We look for non-hidden .go files or subdirectories to determine whether
    // this is an existing project. Walking the tree for packages would be more
    // accurate, but could take much longer.
    var empty = true;
    var (files, _) = os.ReadDir(modRoot);
    foreach (var (_, f) in files) {
        var name = f.Name();
        if (strings.HasPrefix(name, ".") || strings.HasPrefix(name, "_")) {
            continue;
        }
        if (strings.HasSuffix(name, ".go") || f.IsDir()) {
            empty = false;
            break;
        }
    }    if (!empty) {
        fmt.Fprintf(os.Stderr, "go: to add module requirements and sums:\n\tgo mod tidy\n");
    }
}

// fixVersion returns a modfile.VersionFixer implemented using the Query function.
//
// It resolves commit hashes and branch names to versions,
// canonicalizes versions that appeared in early vgo drafts,
// and does nothing for versions that already appear to be canonical.
//
// The VersionFixer sets 'fixed' if it ever returns a non-canonical version.
private static modfile.VersionFixer fixVersion(context.Context ctx, ptr<bool> _addr_@fixed) => func((defer, _, _) => {
    ref bool @fixed = ref _addr_@fixed.val;

    return (path, vers) => {
        defer(() => {
            if (err == null && resolved != vers) {
                fixed.val = true;
            }
        }()); 

        // Special case: remove the old -gopkgin- hack.
        if (strings.HasPrefix(path, "gopkg.in/") && strings.Contains(vers, "-gopkgin-")) {
            vers = vers[(int)strings.Index(vers, "-gopkgin-") + len("-gopkgin-")..];
        }
        var (_, pathMajor, ok) = module.SplitPathVersion(path);
        if (!ok) {
            return ("", addr(new module.ModuleError(Path:path,Err:&module.InvalidVersionError{Version:vers,Err:fmt.Errorf("malformed module path %q",path),},)));
        }
        if (vers != "" && module.CanonicalVersion(vers) == vers) {
            {
                var err = module.CheckPathMajor(vers, pathMajor);

                if (err != null) {
                    return ("", module.VersionError(new module.Version(Path:path,Version:vers), err));
                }

            }
            return (vers, null);
        }
        var (info, err) = Query(ctx, path, vers, "", null);
        if (err != null) {
            return ("", err);
        }
        return (info.Version, null);
    };
});

// AllowMissingModuleImports allows import paths to be resolved to modules
// when there is no module root. Normally, this is forbidden because it's slow
// and there's no way to make the result reproducible, but some commands
// like 'go get' are expected to do this.
//
// This function affects the default cfg.BuildMod when outside of a module,
// so it can only be called prior to Init.
public static void AllowMissingModuleImports() => func((_, panic, _) => {
    if (initialized) {
        panic("AllowMissingModuleImports after Init");
    }
    allowMissingModuleImports = true;
});

// initTarget sets Target and associated variables according to modFile,
private static void initTarget(module.Version m) {
    Target = m;
    targetPrefix = m.Path;

    {
        var rel = search.InDir(@base.Cwd(), cfg.GOROOTsrc);

        if (rel != "") {
            targetInGorootSrc = true;
            if (m.Path == "std") { 
                // The "std" module in GOROOT/src is the Go standard library. Unlike other
                // modules, the packages in the "std" module have no import-path prefix.
                //
                // Modules named "std" outside of GOROOT/src do not receive this special
                // treatment, so it is possible to run 'go test .' in other GOROOTs to
                // test individual packages using a combination of the modified package
                // and the ordinary standard library.
                // (See https://golang.org/issue/30756.)
                targetPrefix = "";
            }
        }
    }
}

// requirementsFromModFile returns the set of non-excluded requirements from
// the global modFile.
private static ptr<Requirements> requirementsFromModFile() {
    var roots = make_slice<module.Version>(0, len(modFile.Require));
    map direct = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    foreach (var (_, r) in modFile.Require) {
        if (index != null && index.exclude[r.Mod]) {
            if (cfg.BuildMod == "mod") {
                fmt.Fprintf(os.Stderr, "go: dropping requirement on excluded version %s %s\n", r.Mod.Path, r.Mod.Version);
            }
            else
 {
                fmt.Fprintf(os.Stderr, "go: ignoring requirement on excluded version %s %s\n", r.Mod.Path, r.Mod.Version);
            }
            continue;
        }
        roots = append(roots, r.Mod);
        if (!r.Indirect) {
            direct[r.Mod.Path] = true;
        }
    }    module.Sort(roots);
    var rs = newRequirements(modDepthFromGoVersion(modFileGoVersion()), roots, direct);
    return _addr_rs!;
}

// setDefaultBuildMod sets a default value for cfg.BuildMod if the -mod flag
// wasn't provided. setDefaultBuildMod may be called multiple times.
private static void setDefaultBuildMod() {
    if (cfg.BuildModExplicit) { 
        // Don't override an explicit '-mod=' argument.
        return ;
    }
    if (cfg.CmdName == "get" || strings.HasPrefix(cfg.CmdName, "mod ")) { 
        // 'get' and 'go mod' commands may update go.mod automatically.
        // TODO(jayconrod): should this narrower? Should 'go mod download' or
        // 'go mod graph' update go.mod by default?
        cfg.BuildMod = "mod";
        return ;
    }
    if (modRoot == "") {
        if (allowMissingModuleImports) {
            cfg.BuildMod = "mod";
        }
        else
 {
            cfg.BuildMod = "readonly";
        }
        return ;
    }
    {
        var (fi, err) = fsys.Stat(filepath.Join(modRoot, "vendor"));

        if (err == null && fi.IsDir()) {
            @string modGo = "unspecified";
            if (index != null && index.goVersionV != "") {
                if (semver.Compare(index.goVersionV, "v1.14") >= 0) { 
                    // The Go version is at least 1.14, and a vendor directory exists.
                    // Set -mod=vendor by default.
                    cfg.BuildMod = "vendor";
                    cfg.BuildModReason = "Go version in go.mod is at least 1.14 and vendor directory exists.";
                    return ;
                }
                else
 {
                    modGo = index.goVersionV[(int)1..];
                }
            } 

            // Since a vendor directory exists, we should record why we didn't use it.
            // This message won't normally be shown, but it may appear with import errors.
            cfg.BuildModReason = fmt.Sprintf("Go version in go.mod is %s, so vendor directory was not used.", modGo);
        }
    }

    cfg.BuildMod = "readonly";
}

// convertLegacyConfig imports module requirements from a legacy vendoring
// configuration file, if one is present.
private static (@string, error) convertLegacyConfig(@string modPath) {
    @string from = default;
    error err = default!;

    Func<@string, @string> noneSelected = path => "none";
    Func<@string, @string, (module.Version, error)> queryPackage = (path, rev) => {
        var (pkgMods, modOnly, err) = QueryPattern(context.Background(), path, rev, noneSelected, null);
        if (err != null) {
            return (new module.Version(), error.As(err)!);
        }
        if (len(pkgMods) > 0) {
            return (pkgMods[0].Mod, error.As(null!)!);
        }
        return (modOnly.Mod, error.As(null!)!);
    };
    foreach (var (_, name) in altConfigs) {
        var cfg = filepath.Join(modRoot, name);
        var (data, err) = os.ReadFile(cfg);
        if (err == null) {
            var convert = modconv.Converters[name];
            if (convert == null) {
                return ("", error.As(null!)!);
            }
            cfg = filepath.ToSlash(cfg);
            var err = modconv.ConvertLegacyConfig(modFile, cfg, data, queryPackage);
            return (name, error.As(err)!);
        }
    }    return ("", error.As(null!)!);
}

// addGoStmt adds a go directive to the go.mod file if it does not already
// include one. The 'go' version added, if any, is the latest version supported
// by this toolchain.
private static void addGoStmt(@string v) {
    if (modFile.Go != null && modFile.Go.Version != "") {
        return ;
    }
    {
        var err = modFile.AddGoStmt(v);

        if (err != null) {
            @base.Fatalf("go: internal error: %v", err);
        }
    }
    rawGoVersion.Store(Target, v);
}

// LatestGoVersion returns the latest version of the Go language supported by
// this toolchain, like "1.17".
public static @string LatestGoVersion() {
    var tags = build.Default.ReleaseTags;
    var version = tags[len(tags) - 1];
    if (!strings.HasPrefix(version, "go") || !modfile.GoVersionRE.MatchString(version[(int)2..])) {
        @base.Fatalf("go: internal error: unrecognized default version %q", version);
    }
    return version[(int)2..];
}

// priorGoVersion returns the Go major release immediately preceding v,
// or v itself if v is the first Go major release (1.0) or not a supported
// Go version.
private static @string priorGoVersion(@string v) {
    @string vTag = "go" + v;
    var tags = build.Default.ReleaseTags;
    foreach (var (i, tag) in tags) {
        if (tag == vTag) {
            if (i == 0) {
                return v;
            }
            var version = tags[i - 1];
            if (!strings.HasPrefix(version, "go") || !modfile.GoVersionRE.MatchString(version[(int)2..])) {
                @base.Fatalf("go: internal error: unrecognized version %q", version);
            }
            return version[(int)2..];
        }
    }    return v;
}

private static @string altConfigs = new slice<@string>(new @string[] { "Gopkg.lock", "GLOCKFILE", "Godeps/Godeps.json", "dependencies.tsv", "glide.lock", "vendor.conf", "vendor.yml", "vendor/manifest", "vendor/vendor.json", ".git/config" });

private static @string findModuleRoot(@string dir) => func((_, panic, _) => {
    @string root = default;

    if (dir == "") {
        panic("dir not set");
    }
    dir = filepath.Clean(dir); 

    // Look for enclosing go.mod.
    while (true) {
        {
            var (fi, err) = fsys.Stat(filepath.Join(dir, "go.mod"));

            if (err == null && !fi.IsDir()) {
                return dir;
            }

        }
        var d = filepath.Dir(dir);
        if (d == dir) {
            break;
        }
        dir = d;
    }
    return "";
});

private static (@string, @string) findAltConfig(@string dir) => func((_, panic, _) => {
    @string root = default;
    @string name = default;

    if (dir == "") {
        panic("dir not set");
    }
    dir = filepath.Clean(dir);
    {
        var rel = search.InDir(dir, cfg.BuildContext.GOROOT);

        if (rel != "") { 
            // Don't suggest creating a module from $GOROOT/.git/config
            // or a config file found in any parent of $GOROOT (see #34191).
            return ("", "");
        }
    }
    while (true) {
        foreach (var (_, name) in altConfigs) {
            {
                var (fi, err) = fsys.Stat(filepath.Join(dir, name));

                if (err == null && !fi.IsDir()) {
                    return (dir, name);
                }

            }
        }        var d = filepath.Dir(dir);
        if (d == dir) {
            break;
        }
        dir = d;
    }
    return ("", "");
});

private static (@string, error) findModulePath(@string dir) {
    @string _p0 = default;
    error _p0 = default!;
 
    // TODO(bcmills): once we have located a plausible module path, we should
    // query version control (if available) to verify that it matches the major
    // version of the most recent tag.
    // See https://golang.org/issue/29433, https://golang.org/issue/27009, and
    // https://golang.org/issue/31549.

    // Cast about for import comments,
    // first in top-level directory, then in subdirectories.
    var (list, _) = os.ReadDir(dir);
    foreach (var (_, info) in list) {
        if (info.Type().IsRegular() && strings.HasSuffix(info.Name(), ".go")) {
            {
                var com__prev2 = com;

                var com = findImportComment(filepath.Join(dir, info.Name()));

                if (com != "") {
                    return (com, error.As(null!)!);
                }

                com = com__prev2;

            }
        }
    }    foreach (var (_, info1) in list) {
        if (info1.IsDir()) {
            var (files, _) = os.ReadDir(filepath.Join(dir, info1.Name()));
            foreach (var (_, info2) in files) {
                if (info2.Type().IsRegular() && strings.HasSuffix(info2.Name(), ".go")) {
                    {
                        var com__prev3 = com;

                        com = findImportComment(filepath.Join(dir, info1.Name(), info2.Name()));

                        if (com != "") {
                            return (path.Dir(com), error.As(null!)!);
                        }

                        com = com__prev3;

                    }
                }
            }
        }
    }    var (data, _) = os.ReadFile(filepath.Join(dir, "Godeps/Godeps.json"));
    ref var cfg1 = ref heap(out ptr<var> _addr_cfg1);
    json.Unmarshal(data, _addr_cfg1);
    if (cfg1.ImportPath != "") {
        return (cfg1.ImportPath, error.As(null!)!);
    }
    data, _ = os.ReadFile(filepath.Join(dir, "vendor/vendor.json"));
    ref var cfg2 = ref heap(out ptr<var> _addr_cfg2);
    json.Unmarshal(data, _addr_cfg2);
    if (cfg2.RootPath != "") {
        return (cfg2.RootPath, error.As(null!)!);
    }
    error badPathErr = default!;
    foreach (var (_, gpdir) in filepath.SplitList(cfg.BuildContext.GOPATH)) {
        if (gpdir == "") {
            continue;
        }
        {
            var rel = search.InDir(dir, filepath.Join(gpdir, "src"));

            if (rel != "" && rel != ".") {
                var path = filepath.ToSlash(rel); 
                // gorelease will alert users publishing their modules to fix their paths.
                {
                    var err = module.CheckImportPath(path);

                    if (err != null) {
                        badPathErr = error.As(err)!;
                        break;
                    }

                }
                return (path, error.As(null!)!);
            }

        }
    }    @string reason = "outside GOPATH, module path must be specified";
    if (badPathErr != null) { 
        // return a different error message if the module was in GOPATH, but
        // the module path determined above would be an invalid path.
        reason = fmt.Sprintf("bad module path inferred from directory in GOPATH: %v", badPathErr);
    }
    @string msg = "cannot determine module path for source directory %s (%s)\n\nExample usage:\n\t\'go mo" +
    "d init example.com/m\' to initialize a v0 or v1 module\n\t\'go mod init example.com/" +
    "m/v2\' to initialize a v2 module\n\nRun \'go help mod init\' for more information.\n";
    return ("", error.As(fmt.Errorf(msg, dir, reason))!);
}

private static var importCommentRE = lazyregexp.New("(?m)^package[ \\t]+[^ \\t\\r\\n/]+[ \\t]+//[ \\t]+import[ \\t]+(\\\"[^\"]+\\\")[ \\t]*\\r?\\n");

private static @string findImportComment(@string file) {
    var (data, err) = os.ReadFile(file);
    if (err != null) {
        return "";
    }
    var m = importCommentRE.FindSubmatch(data);
    if (m == null) {
        return "";
    }
    var (path, err) = strconv.Unquote(string(m[1]));
    if (err != null) {
        return "";
    }
    return path;
}

private static var allowWriteGoMod = true;

// DisallowWriteGoMod causes future calls to WriteGoMod to do nothing at all.
public static void DisallowWriteGoMod() {
    allowWriteGoMod = false;
}

// AllowWriteGoMod undoes the effect of DisallowWriteGoMod:
// future calls to WriteGoMod will update go.mod if needed.
// Note that any past calls have been discarded, so typically
// a call to AlowWriteGoMod should be followed by a call to WriteGoMod.
public static void AllowWriteGoMod() {
    allowWriteGoMod = true;
}

// WriteGoMod writes the current build list back to go.mod.
public static void WriteGoMod(context.Context ctx) => func((_, panic, _) => {
    if (!allowWriteGoMod) {
        panic("WriteGoMod called while disallowed");
    }
    commitRequirements(ctx, modFileGoVersion(), _addr_LoadModFile(ctx));
});

// commitRequirements writes sets the global requirements variable to rs and
// writes its contents back to the go.mod file on disk.
private static void commitRequirements(context.Context ctx, @string goVersion, ptr<Requirements> _addr_rs) => func((defer, _, _) => {
    ref Requirements rs = ref _addr_rs.val;

    requirements = rs;

    if (!allowWriteGoMod) { 
        // Some package outside of modload promised to update the go.mod file later.
        return ;
    }
    if (modRoot == "") { 
        // We aren't in a module, so we don't have anywhere to write a go.mod file.
        return ;
    }
    slice<ptr<modfile.Require>> list = default;
    foreach (var (_, m) in rs.rootModules) {
        list = append(list, addr(new modfile.Require(Mod:m,Indirect:!rs.direct[m.Path],)));
    }    if (goVersion != "") {
        modFile.AddGoStmt(goVersion);
    }
    if (semver.Compare("v" + modFileGoVersion(), separateIndirectVersionV) < 0) {
        modFile.SetRequire(list);
    }
    else
 {
        modFile.SetRequireSeparateIndirect(list);
    }
    modFile.Cleanup();

    var dirty = index.modFileIsDirty(modFile);
    if (dirty && cfg.BuildMod != "mod") { 
        // If we're about to fail due to -mod=readonly,
        // prefer to report a dirty go.mod over a dirty go.sum
        @base.Fatalf("go: %v", errGoModDirty);
    }
    if (!dirty && cfg.CmdName != "mod tidy") { 
        // The go.mod file has the same semantic content that it had before
        // (but not necessarily the same exact bytes).
        // Don't write go.mod, but write go.sum in case we added or trimmed sums.
        // 'go mod init' shouldn't write go.sum, since it will be incomplete.
        if (cfg.CmdName != "mod init") {
            modfetch.WriteGoSum(keepSums(ctx, _addr_loaded, _addr_rs, addBuildListZipSums));
        }
        return ;
    }
    var gomod = ModFilePath();
    {
        var (_, ok) = fsys.OverlayPath(gomod);

        if (ok) {
            if (dirty) {
                @base.Fatalf("go: updates to go.mod needed, but go.mod is part of the overlay specified with -overlay");
            }
            return ;
        }
    }

    var (new, err) = modFile.Format();
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    defer(() => { 
        // At this point we have determined to make the go.mod file on disk equal to new.
        index = indexModFile(new, modFile, false); 

        // Update go.sum after releasing the side lock and refreshing the index.
        // 'go mod init' shouldn't write go.sum, since it will be incomplete.
        if (cfg.CmdName != "mod init") {
            modfetch.WriteGoSum(keepSums(ctx, _addr_loaded, _addr_rs, addBuildListZipSums));
        }
    }()); 

    // Make a best-effort attempt to acquire the side lock, only to exclude
    // previous versions of the 'go' command from making simultaneous edits.
    {
        var (unlock, err) = modfetch.SideLock();

        if (err == null) {
            defer(unlock());
        }
    }

    var errNoChange = errors.New("no update needed");

    err = lockedfile.Transform(ModFilePath(), old => {
        if (bytes.Equal(old, new)) { 
            // The go.mod file is already equal to new, possibly as the result of some
            // other process.
            return (null, errNoChange);
        }
        if (index != null && !bytes.Equal(old, index.data)) { 
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

    if (err != null && err != errNoChange) {
        @base.Fatalf("go: updating go.mod: %v", err);
    }
});

// keepSums returns the set of modules (and go.mod file entries) for which
// checksums would be needed in order to reload the same set of packages
// loaded by the most recent call to LoadPackages or ImportFromFiles,
// including any go.mod files needed to reconstruct the MVS result,
// in addition to the checksums for every module in keepMods.
private static map<module.Version, bool> keepSums(context.Context ctx, ptr<loader> _addr_ld, ptr<Requirements> _addr_rs, whichSums which) {
    ref loader ld = ref _addr_ld.val;
    ref Requirements rs = ref _addr_rs.val;
 
    // Every module in the full module graph contributes its requirements,
    // so in order to ensure that the build list itself is reproducible,
    // we need sums for every go.mod in the graph (regardless of whether
    // that version is selected).
    var keep = make_map<module.Version, bool>(); 

    // Add entries for modules in the build list with paths that are prefixes of
    // paths of loaded packages. We need to retain sums for all of these modules —
    // not just the modules containing the actual packages — in order to rule out
    // ambiguous import errors the next time we load the package.
    if (ld != null) {
        foreach (var (_, pkg) in ld.pkgs) { 
            // We check pkg.mod.Path here instead of pkg.inStd because the
            // pseudo-package "C" is not in std, but not provided by any module (and
            // shouldn't force loading the whole module graph).
            if (pkg.testOf != null || (pkg.mod.Path == "" && pkg.err == null) || module.CheckImportPath(pkg.path) != null) {
                continue;
            }
            if (rs.depth == lazy && pkg.mod.Path != "") {
                {
                    var v__prev3 = v;

                    var (v, ok) = rs.rootSelected(pkg.mod.Path);

                    if (ok && v == pkg.mod.Version) { 
                        // pkg was loaded from a root module, and because the main module is
                        // lazy we do not check non-root modules for conflicts for packages
                        // that can be found in roots. So we only need the checksums for the
                        // root modules that may contain pkg, not all possible modules.
                        {
                            var prefix__prev2 = prefix;

                            var prefix = pkg.path;

                            while (prefix != ".") {
                                {
                                    var v__prev4 = v;

                                    (v, ok) = rs.rootSelected(prefix);

                                    if (ok && v != "none") {
                                        module.Version m = new module.Version(Path:prefix,Version:v);
                                        keep[resolveReplacement(m)] = true;
                                prefix = path.Dir(prefix);
                                    }

                                    v = v__prev4;

                                }
                            }


                            prefix = prefix__prev2;
                        }
                        continue;
                    }

                    v = v__prev3;

                }
            }
            var (mg, _) = rs.Graph(ctx);
            {
                var prefix__prev2 = prefix;

                prefix = pkg.path;

                while (prefix != ".") {
                    {
                        var v__prev2 = v;

                        var v = mg.Selected(prefix);

                        if (v != "none") {
                            m = new module.Version(Path:prefix,Version:v);
                            keep[resolveReplacement(m)] = true;
                    prefix = path.Dir(prefix);
                        }

                        v = v__prev2;

                    }
                }


                prefix = prefix__prev2;
            }
        }
    }
    if (rs.graph.Load() == null) { 
        // The module graph was not loaded, possibly because the main module is lazy
        // or possibly because we haven't needed to load the graph yet.
        // Save sums for the root modules (or their replacements), but don't
        // incur the cost of loading the graph just to find and retain the sums.
        {
            module.Version m__prev1 = m;

            foreach (var (_, __m) in rs.rootModules) {
                m = __m;
                var r = resolveReplacement(m);
                keep[modkey(r)] = true;
                if (which == addBuildListZipSums) {
                    keep[r] = true;
                }
            }
    else

            m = m__prev1;
        }
    } {
        (mg, _) = rs.Graph(ctx);
        mg.WalkBreadthFirst(m => {
            {
                var (_, ok) = mg.RequiredBy(m);

                if (ok) { 
                    // The requirements from m's go.mod file are present in the module graph,
                    // so they are relevant to the MVS result regardless of whether m was
                    // actually selected.
                    keep[modkey(resolveReplacement(m))] = true;
                }

            }
        });

        if (which == addBuildListZipSums) {
            {
                module.Version m__prev1 = m;

                foreach (var (_, __m) in mg.BuildList()) {
                    m = __m;
                    keep[resolveReplacement(m)] = true;
                }

                m = m__prev1;
            }
        }
    }
    return keep;
}

private partial struct whichSums { // : sbyte
}

private static readonly var loadedZipSumsOnly = whichSums(iota);
private static readonly var addBuildListZipSums = 0;

// modKey returns the module.Version under which the checksum for m's go.mod
// file is stored in the go.sum file.
private static module.Version modkey(module.Version m) {
    return new module.Version(Path:m.Path,Version:m.Version+"/go.mod");
}

} // end modload_package
