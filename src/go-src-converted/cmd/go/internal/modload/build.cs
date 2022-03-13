// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:29:49 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\build.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using context = context_package;
using hex = encoding.hex_package;
using errors = errors_package;
using fmt = fmt_package;
using goroot = @internal.goroot_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using modfetch = cmd.go.@internal.modfetch_package;
using modinfo = cmd.go.@internal.modinfo_package;
using search = cmd.go.@internal.search_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;
using System;

public static partial class modload_package {


private static bool isStandardImportPath(@string path) {
    return findStandardImportPath(path) != "";
}

private static @string findStandardImportPath(@string path) => func((_, panic, _) => {
    if (path == "") {
        panic("findStandardImportPath called with empty path");
    }
    if (search.IsStandardImportPath(path)) {
        if (goroot.IsStandardPackage(cfg.GOROOT, cfg.BuildContext.Compiler, path)) {
            return filepath.Join(cfg.GOROOT, "src", path);
        }
    }
    return "";
});

// PackageModuleInfo returns information about the module that provides
// a given package. If modules are not enabled or if the package is in the
// standard library or if the package was not successfully loaded with
// LoadPackages or ImportFromFiles, nil is returned.
public static ptr<modinfo.ModulePublic> PackageModuleInfo(context.Context ctx, @string pkgpath) {
    if (isStandardImportPath(pkgpath) || !Enabled()) {
        return _addr_null!;
    }
    var (m, ok) = findModule(_addr_loaded, pkgpath);
    if (!ok) {
        return _addr_null!;
    }
    var rs = LoadModFile(ctx);
    return _addr_moduleInfo(ctx, _addr_rs, m, 0)!;
}

public static ptr<modinfo.ModulePublic> ModuleInfo(context.Context ctx, @string path) {
    if (!Enabled()) {
        return _addr_null!;
    }
    {
        var i = strings.Index(path, "@");

        if (i >= 0) {
            module.Version m = new module.Version(Path:path[:i],Version:path[i+1:]);
            return _addr_moduleInfo(ctx, _addr_null, m, 0)!;
        }
    }

    var rs = LoadModFile(ctx);

    @string v = default;    bool ok = default;
    if (rs.depth == lazy) {
        v, ok = rs.rootSelected(path);
    }
    if (!ok) {
        var (mg, err) = rs.Graph(ctx);
        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
        v = mg.Selected(path);
    }
    if (v == "none") {
        return addr(new modinfo.ModulePublic(Path:path,Error:&modinfo.ModuleError{Err:"module not in current build",},));
    }
    return _addr_moduleInfo(ctx, _addr_rs, new module.Version(Path:path,Version:v), 0)!;
}

// addUpdate fills in m.Update if an updated version is available.
private static void addUpdate(context.Context ctx, ptr<modinfo.ModulePublic> _addr_m) {
    ref modinfo.ModulePublic m = ref _addr_m.val;

    if (m.Version == "") {
        return ;
    }
    var (info, err) = Query(ctx, m.Path, "upgrade", m.Version, CheckAllowed);
    ptr<NoMatchingVersionError> noVersionErr;
    if (errors.Is(err, fs.ErrNotExist) || errors.As(err, _addr_noVersionErr)) { 
        // Ignore "not found" and "no matching version" errors.
        // This means the proxy has no matching version or no versions at all.
        //
        // We should report other errors though. An attacker that controls the
        // network shouldn't be able to hide versions by interfering with
        // the HTTPS connection. An attacker that controls the proxy may still
        // hide versions, since the "list" and "latest" endpoints are not
        // authenticated.
        return ;
    }
    else if (err != null) {
        if (m.Error == null) {
            m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
        }
        return ;
    }
    if (semver.Compare(info.Version, m.Version) > 0) {
        m.Update = addr(new modinfo.ModulePublic(Path:m.Path,Version:info.Version,Time:&info.Time,));
    }
}

// addVersions fills in m.Versions with the list of known versions.
// Excluded versions will be omitted. If listRetracted is false, retracted
// versions will also be omitted.
private static void addVersions(context.Context ctx, ptr<modinfo.ModulePublic> _addr_m, bool listRetracted) {
    ref modinfo.ModulePublic m = ref _addr_m.val;

    var allowed = CheckAllowed;
    if (listRetracted) {
        allowed = CheckExclusions;
    }
    error err = default!;
    m.Versions, err = versions(ctx, m.Path, allowed);
    if (err != null && m.Error == null) {
        m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
    }
}

// addRetraction fills in m.Retracted if the module was retracted by its author.
// m.Error is set if there's an error loading retraction information.
private static void addRetraction(context.Context ctx, ptr<modinfo.ModulePublic> _addr_m) {
    ref modinfo.ModulePublic m = ref _addr_m.val;

    if (m.Version == "") {
        return ;
    }
    var err = CheckRetractions(ctx, new module.Version(Path:m.Path,Version:m.Version));
    ptr<NoMatchingVersionError> noVersionErr;
    ptr<ModuleRetractedError> retractErr;
    if (err == null || errors.Is(err, fs.ErrNotExist) || errors.As(err, _addr_noVersionErr)) { 
        // Ignore "not found" and "no matching version" errors.
        // This means the proxy has no matching version or no versions at all.
        //
        // We should report other errors though. An attacker that controls the
        // network shouldn't be able to hide versions by interfering with
        // the HTTPS connection. An attacker that controls the proxy may still
        // hide versions, since the "list" and "latest" endpoints are not
        // authenticated.
        return ;
    }
    else if (errors.As(err, _addr_retractErr)) {
        if (len(retractErr.Rationale) == 0) {
            m.Retracted = new slice<@string>(new @string[] { "retracted by module author" });
        }
        else
 {
            m.Retracted = retractErr.Rationale;
        }
    }
    else if (m.Error == null) {
        m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
    }
}

// addDeprecation fills in m.Deprecated if the module was deprecated by its
// author. m.Error is set if there's an error loading deprecation information.
private static void addDeprecation(context.Context ctx, ptr<modinfo.ModulePublic> _addr_m) {
    ref modinfo.ModulePublic m = ref _addr_m.val;

    var (deprecation, err) = CheckDeprecation(ctx, new module.Version(Path:m.Path,Version:m.Version));
    ptr<NoMatchingVersionError> noVersionErr;
    if (errors.Is(err, fs.ErrNotExist) || errors.As(err, _addr_noVersionErr)) { 
        // Ignore "not found" and "no matching version" errors.
        // This means the proxy has no matching version or no versions at all.
        //
        // We should report other errors though. An attacker that controls the
        // network shouldn't be able to hide versions by interfering with
        // the HTTPS connection. An attacker that controls the proxy may still
        // hide versions, since the "list" and "latest" endpoints are not
        // authenticated.
        return ;
    }
    if (err != null) {
        if (m.Error == null) {
            m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
        }
        return ;
    }
    m.Deprecated = deprecation;
}

// moduleInfo returns information about module m, loaded from the requirements
// in rs (which may be nil to indicate that m was not loaded from a requirement
// graph).
private static ptr<modinfo.ModulePublic> moduleInfo(context.Context ctx, ptr<Requirements> _addr_rs, module.Version m, ListMode mode) => func((_, panic, _) => {
    ref Requirements rs = ref _addr_rs.val;

    if (m == Target) {
        ptr<modinfo.ModulePublic> info = addr(new modinfo.ModulePublic(Path:m.Path,Version:m.Version,Main:true,));
        {
            var v__prev2 = v;

            var (v, ok) = rawGoVersion.Load(Target);

            if (ok) {
                info.GoVersion = v._<@string>();
            }
            else
 {
                panic("internal error: GoVersion not set for main module");
            }

            v = v__prev2;

        }
        if (HasModRoot()) {
            info.Dir = ModRoot();
            info.GoMod = ModFilePath();
        }
        return _addr_info!;
    }
    info = addr(new modinfo.ModulePublic(Path:m.Path,Version:m.Version,Indirect:rs!=nil&&!rs.direct[m.Path],));
    {
        var v__prev1 = v;

        (v, ok) = rawGoVersion.Load(m);

        if (ok) {
            info.GoVersion = v._<@string>();
        }
        v = v__prev1;

    } 

    // completeFromModCache fills in the extra fields in m using the module cache.
    Action<ptr<modinfo.ModulePublic>> completeFromModCache = m => {
        Func<@string, bool> checksumOk = suffix => _addr_rs == null || m.Version == "" || cfg.BuildMod == "mod" || modfetch.HaveSum(new module.Version(Path:m.Path,Version:m.Version+suffix))!;

        if (m.Version != "") {
            {
                var (q, err) = Query(ctx, m.Path, m.Version, "", null);

                if (err != null) {
                    m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
                }
                else
 {
                    m.Version = q.Version;
                    m.Time = _addr_q.Time;
                }

            }
        }
        module.Version mod = new module.Version(Path:m.Path,Version:m.Version);

        if (m.GoVersion == "" && checksumOk("/go.mod")) { 
            // Load the go.mod file to determine the Go version, since it hasn't
            // already been populated from rawGoVersion.
            {
                var (summary, err) = rawGoModSummary(mod);

                if (err == null && summary.goVersion != "") {
                    m.GoVersion = summary.goVersion;
                }

            }
        }
        if (m.Version != "") {
            if (checksumOk("/go.mod")) {
                var (gomod, err) = modfetch.CachePath(mod, "mod");
                if (err == null) {
                    {
                        ptr<modinfo.ModulePublic> info__prev4 = info;

                        var (info, err) = os.Stat(gomod);

                        if (err == null && info.Mode().IsRegular()) {
                            m.GoMod = gomod;
                        }

                        info = info__prev4;

                    }
                }
            }
            if (checksumOk("")) {
                var (dir, err) = modfetch.DownloadDir(mod);
                if (err == null) {
                    m.Dir = dir;
                }
            }
            if (mode & ListRetracted != 0) {
                addRetraction(ctx, _addr_m);
            }
        }
    };

    if (rs == null) { 
        // If this was an explicitly-versioned argument to 'go mod download' or
        // 'go list -m', report the actual requested version, not its replacement.
        completeFromModCache(info); // Will set m.Error in vendor mode.
        return _addr_info!;
    }
    var r = Replacement(m);
    if (r.Path == "") {
        if (cfg.BuildMod == "vendor") { 
            // It's tempting to fill in the "Dir" field to point within the vendor
            // directory, but that would be misleading: the vendor directory contains
            // a flattened package tree, not complete modules, and it can even
            // interleave packages from different modules if one module path is a
            // prefix of the other.
        }
        else
 {
            completeFromModCache(info);
        }
        return _addr_info!;
    }
    info.Replace = addr(new modinfo.ModulePublic(Path:r.Path,Version:r.Version,));
    {
        var v__prev1 = v;

        (v, ok) = rawGoVersion.Load(m);

        if (ok) {
            info.Replace.GoVersion = v._<@string>();
        }
        v = v__prev1;

    }
    if (r.Version == "") {
        if (filepath.IsAbs(r.Path)) {
            info.Replace.Dir = r.Path;
        }
        else
 {
            info.Replace.Dir = filepath.Join(ModRoot(), r.Path);
        }
        info.Replace.GoMod = filepath.Join(info.Replace.Dir, "go.mod");
    }
    if (cfg.BuildMod != "vendor") {
        completeFromModCache(info.Replace);
        info.Dir = info.Replace.Dir;
        info.GoMod = info.Replace.GoMod;
        info.Retracted = info.Replace.Retracted;
    }
    info.GoVersion = info.Replace.GoVersion;
    return _addr_info!;
});

// PackageBuildInfo returns a string containing module version information
// for modules providing packages named by path and deps. path and deps must
// name packages that were resolved successfully with LoadPackages.
public static @string PackageBuildInfo(@string path, slice<@string> deps) {
    if (isStandardImportPath(path) || !Enabled()) {
        return "";
    }
    var target = mustFindModule(_addr_loaded, path, path);
    var mdeps = make_map<module.Version, bool>();
    foreach (var (_, dep) in deps) {
        if (!isStandardImportPath(dep)) {
            mdeps[mustFindModule(_addr_loaded, path, dep)] = true;
        }
    }    slice<module.Version> mods = default;
    delete(mdeps, target);
    {
        var mod__prev1 = mod;

        foreach (var (__mod) in mdeps) {
            mod = __mod;
            mods = append(mods, mod);
        }
        mod = mod__prev1;
    }

    module.Sort(mods);

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    fmt.Fprintf(_addr_buf, "path\t%s\n", path);

    Action<@string, module.Version> writeEntry = (token, m) => {
        var mv = m.Version;
        if (mv == "") {
            mv = "(devel)";
        }
        fmt.Fprintf(_addr_buf, "%s\t%s\t%s", token, m.Path, mv);
        {
            var r = Replacement(m);

            if (r.Path == "") {
                fmt.Fprintf(_addr_buf, "\t%s\n", modfetch.Sum(m));
            }
            else
 {
                fmt.Fprintf(_addr_buf, "\n=>\t%s\t%s\t%s\n", r.Path, r.Version, modfetch.Sum(r));
            }

        }
    };

    writeEntry("mod", target);
    {
        var mod__prev1 = mod;

        foreach (var (_, __mod) in mods) {
            mod = __mod;
            writeEntry("dep", mod);
        }
        mod = mod__prev1;
    }

    return buf.String();
}

// mustFindModule is like findModule, but it calls base.Fatalf if the
// module can't be found.
//
// TODO(jayconrod): remove this. Callers should use findModule and return
// errors instead of relying on base.Fatalf.
private static module.Version mustFindModule(ptr<loader> _addr_ld, @string target, @string path) => func((_, panic, _) => {
    ref loader ld = ref _addr_ld.val;

    ptr<loadPkg> (pkg, ok) = ld.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (ok) {
        if (pkg.err != null) {
            @base.Fatalf("build %v: cannot load %v: %v", target, path, pkg.err);
        }
        return pkg.mod;
    }
    if (path == "command-line-arguments") {
        return Target;
    }
    @base.Fatalf("build %v: cannot find module for path %v", target, path);
    panic("unreachable");
});

// findModule searches for the module that contains the package at path.
// If the package was loaded, its containing module and true are returned.
// Otherwise, module.Version{} and false are returend.
private static (module.Version, bool) findModule(ptr<loader> _addr_ld, @string path) {
    module.Version _p0 = default;
    bool _p0 = default;
    ref loader ld = ref _addr_ld.val;

    {
        ptr<loadPkg> (pkg, ok) = ld.pkgCache.Get(path)._<ptr<loadPkg>>();

        if (ok) {
            return (pkg.mod, pkg.mod != new module.Version());
        }
    }
    if (path == "command-line-arguments") {
        return (Target, true);
    }
    return (new module.Version(), false);
}

public static slice<byte> ModInfoProg(@string info, bool isgccgo) { 
    // Inject a variable with the debug information as runtime.modinfo,
    // but compile it in package main so that it is specific to the binary.
    // The variable must be a literal so that it will have the correct value
    // before the initializer for package main runs.
    //
    // The runtime startup code refers to the variable, which keeps it live
    // in all binaries.
    //
    // Note: we use an alternate recipe below for gccgo (based on an
    // init function) due to the fact that gccgo does not support
    // applying a "//go:linkname" directive to a variable. This has
    // drawbacks in that other packages may want to look at the module
    // info in their init functions (see issue 29628), which won't
    // work for gccgo. See also issue 30344.

    if (!isgccgo) {
        return (slice<byte>)fmt.Sprintf("package main\nimport _ \"unsafe\"\n//go:linkname __debug_modinfo__ runtime.modinfo\nva" +
    "r __debug_modinfo__ = %q\n", string(infoStart) + info + string(infoEnd));
    }
    else
 {
        return (slice<byte>)fmt.Sprintf("package main\nimport _ \"unsafe\"\n//go:linkname __set_debug_modinfo__ runtime.setmod" +
    "info\nfunc __set_debug_modinfo__(string)\nfunc init() { __set_debug_modinfo__(%q) " +
    "}\n", string(infoStart) + info + string(infoEnd));
    }
}

} // end modload_package
