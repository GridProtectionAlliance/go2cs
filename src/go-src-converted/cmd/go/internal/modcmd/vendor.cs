// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modcmd -- go2cs converted at 2022 March 13 06:32:26 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\vendor.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using build = go.build_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;
using path = path_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using imports = cmd.go.@internal.imports_package;
using load = cmd.go.@internal.load_package;
using modload = cmd.go.@internal.modload_package;
using str = cmd.go.@internal.str_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;
using System;

public static partial class modcmd_package {

private static ptr<base.Command> cmdVendor = addr(new base.Command(UsageLine:"go mod vendor [-e] [-v]",Short:"make vendored copy of dependencies",Long:`
Vendor resets the main module's vendor directory to include all packages
needed to build and test all the main module's packages.
It does not include test code for vendored packages.

The -v flag causes vendor to print the names of vendored
modules and packages to standard error.

The -e flag causes vendor to attempt to proceed despite errors
encountered while loading packages.

See https://golang.org/ref/mod#go-mod-vendor for more about 'go mod vendor'.
	`,Run:runVendor,));

private static bool vendorE = default; // if true, report errors but proceed anyway

private static void init() {
    cmdVendor.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
    cmdVendor.Flag.BoolVar(_addr_vendorE, "e", false, "");
    @base.AddModCommonFlags(_addr_cmdVendor.Flag);
}

private static void runVendor(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) != 0) {
        @base.Fatalf("go mod vendor: vendor takes no arguments");
    }
    modload.ForceUseModules = true;
    modload.RootMode = modload.NeedRoot;

    modload.PackageOpts loadOpts = new modload.PackageOpts(Tags:imports.AnyTags(),VendorModulesInGOROOTSrc:true,ResolveMissingImports:true,UseVendorAll:true,AllowErrors:vendorE,SilenceMissingStdImports:true,);
    var (_, pkgs) = modload.LoadPackages(ctx, loadOpts, "all");

    var vdir = filepath.Join(modload.ModRoot(), "vendor");
    {
        var err__prev1 = err;

        var err = os.RemoveAll(vdir);

        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        err = err__prev1;

    }

    var modpkgs = make_map<module.Version, slice<@string>>();
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            var m = modload.PackageModule(pkg);
            if (m.Path == "" || m == modload.Target) {
                continue;
            }
            modpkgs[m] = append(modpkgs[m], pkg);
        }
        pkg = pkg__prev1;
    }

    var includeAllReplacements = false;
    var includeGoVersions = false;
    map isExplicit = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};
    {
        var gv = modload.ModFile().Go;

        if (gv != null) {
            if (semver.Compare("v" + gv.Version, "v1.14") >= 0) { 
                // If the Go version is at least 1.14, annotate all explicit 'require' and
                // 'replace' targets found in the go.mod file so that we can perform a
                // stronger consistency check when -mod=vendor is set.
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in modload.ModFile().Require) {
                        r = __r;
                        isExplicit[r.Mod] = true;
                    }

                    r = r__prev1;
                }

                includeAllReplacements = true;
            }
            if (semver.Compare("v" + gv.Version, "v1.17") >= 0) { 
                // If the Go version is at least 1.17, annotate all modules with their
                // 'go' version directives.
                includeGoVersions = true;
            }
        }
    }

    slice<module.Version> vendorMods = default;
    {
        var m__prev1 = m;

        foreach (var (__m) in isExplicit) {
            m = __m;
            vendorMods = append(vendorMods, m);
        }
        m = m__prev1;
    }

    {
        var m__prev1 = m;

        foreach (var (__m) in modpkgs) {
            m = __m;
            if (!isExplicit[m]) {
                vendorMods = append(vendorMods, m);
            }
        }
        m = m__prev1;
    }

    module.Sort(vendorMods);

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);    io.Writer w = _addr_buf;
    if (cfg.BuildV) {
        w = io.MultiWriter(_addr_buf, os.Stderr);
    }
    {
        var m__prev1 = m;

        foreach (var (_, __m) in vendorMods) {
            m = __m;
            var line = moduleLine(m, modload.Replacement(m));
            io.WriteString(w, line);

            @string goVersion = "";
            if (includeGoVersions) {
                goVersion = modload.ModuleInfo(ctx, m.Path).GoVersion;
            }

            if (isExplicit[m] && goVersion != "") 
                fmt.Fprintf(w, "## explicit; go %s\n", goVersion);
            else if (isExplicit[m]) 
                io.WriteString(w, "## explicit\n");
            else if (goVersion != "") 
                fmt.Fprintf(w, "## go %s\n", goVersion);
                        var pkgs = modpkgs[m];
            sort.Strings(pkgs);
            {
                var pkg__prev2 = pkg;

                foreach (var (_, __pkg) in pkgs) {
                    pkg = __pkg;
                    fmt.Fprintf(w, "%s\n", pkg);
                    vendorPkg(vdir, pkg);
                }

                pkg = pkg__prev2;
            }
        }
        m = m__prev1;
    }

    if (includeAllReplacements) { 
        // Record unused and wildcard replacements at the end of the modules.txt file:
        // without access to the complete build list, the consumer of the vendor
        // directory can't otherwise determine that those replacements had no effect.
        {
            var r__prev1 = r;

            foreach (var (_, __r) in modload.ModFile().Replace) {
                r = __r;
                if (len(modpkgs[r.Old]) > 0) { 
                    // We we already recorded this replacement in the entry for the replaced
                    // module with the packages it provides.
                    continue;
                }
                line = moduleLine(r.Old, r.New);
                buf.WriteString(line);
                if (cfg.BuildV) {
                    os.Stderr.WriteString(line);
                }
            }

            r = r__prev1;
        }
    }
    if (buf.Len() == 0) {
        fmt.Fprintf(os.Stderr, "go: no dependencies to vendor\n");
        return ;
    }
    {
        var err__prev1 = err;

        err = os.MkdirAll(vdir, 0777);

        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.WriteFile(filepath.Join(vdir, "modules.txt"), buf.Bytes(), 0666);

        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        err = err__prev1;

    }
}

private static @string moduleLine(module.Version m, module.Version r) {
    ptr<object> b = @new<strings.Builder>();
    b.WriteString("# ");
    b.WriteString(m.Path);
    if (m.Version != "") {
        b.WriteString(" ");
        b.WriteString(m.Version);
    }
    if (r.Path != "") {
        b.WriteString(" => ");
        b.WriteString(r.Path);
        if (r.Version != "") {
            b.WriteString(" ");
            b.WriteString(r.Version);
        }
    }
    b.WriteString("\n");
    return b.String();
}

private static void vendorPkg(@string vdir, @string pkg) { 
    // TODO(#42504): Instead of calling modload.ImportMap then build.ImportDir,
    // just call load.PackagesAndErrors. To do that, we need to add a good way
    // to ignore build constraints.
    var realPath = modload.ImportMap(pkg);
    if (realPath != pkg && modload.ImportMap(realPath) != "") {
        fmt.Fprintf(os.Stderr, "warning: %s imported as both %s and %s; making two copies.\n", realPath, realPath, pkg);
    }
    var copiedFiles = make_map<@string, bool>();
    var dst = filepath.Join(vdir, pkg);
    var src = modload.PackageDir(realPath);
    if (src == "") {
        fmt.Fprintf(os.Stderr, "internal error: no pkg for %s -> %s\n", pkg, realPath);
    }
    copyDir(dst, src, matchPotentialSourceFile, copiedFiles);
    {
        var m = modload.PackageModule(realPath);

        if (m.Path != "") {
            copyMetadata(m.Path, realPath, dst, src, copiedFiles);
        }
    }

    var ctx = build.Default;
    ctx.UseAllFiles = true;
    var (bp, err) = ctx.ImportDir(src, build.IgnoreVendor); 
    // Because UseAllFiles is set on the build.Context, it's possible ta get
    // a MultiplePackageError on an otherwise valid package: the package could
    // have different names for GOOS=windows and GOOS=mac for example. On the
    // other hand if there's a NoGoError, the package might have source files
    // specifying "// +build ignore" those packages should be skipped because
    // embeds from ignored files can't be used.
    // TODO(#42504): Find a better way to avoid errors from ImportDir. We'll
    // need to figure this out when we switch to PackagesAndErrors as per the
    // TODO above.
    ptr<build.MultiplePackageError> multiplePackageError;
    ptr<build.NoGoError> noGoError;
    if (err != null) {
        if (errors.As(err, _addr_noGoError)) {
            return ; // No source files in this package are built. Skip embeds in ignored files.
        }
        else if (!errors.As(err, _addr_multiplePackageError)) { // multiplePackgeErrors are okay, but others are not.
            @base.Fatalf("internal error: failed to find embedded files of %s: %v\n", pkg, err);
        }
    }
    var embedPatterns = str.StringList(bp.EmbedPatterns, bp.TestEmbedPatterns, bp.XTestEmbedPatterns);
    var (embeds, err) = load.ResolveEmbed(bp.Dir, embedPatterns);
    if (err != null) {
        @base.Fatalf("go mod vendor: %v", err);
    }
    foreach (var (_, embed) in embeds) {
        var embedDst = filepath.Join(dst, embed);
        if (copiedFiles[embedDst]) {
            continue;
        }
        var (r, err) = os.Open(filepath.Join(src, embed));
        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        {
            var err__prev1 = err;

            var err = os.MkdirAll(filepath.Dir(embedDst), 0777);

            if (err != null) {
                @base.Fatalf("go mod vendor: %v", err);
            }

            err = err__prev1;

        }
        var (w, err) = os.Create(embedDst);
        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        {
            var err__prev1 = err;

            var (_, err) = io.Copy(w, r);

            if (err != null) {
                @base.Fatalf("go mod vendor: %v", err);
            }

            err = err__prev1;

        }
        r.Close();
        {
            var err__prev1 = err;

            err = w.Close();

            if (err != null) {
                @base.Fatalf("go mod vendor: %v", err);
            }

            err = err__prev1;

        }
    }
}

private partial struct metakey {
    public @string modPath;
    public @string dst;
}

private static var copiedMetadata = make_map<metakey, bool>();

// copyMetadata copies metadata files from parents of src to parents of dst,
// stopping after processing the src parent for modPath.
private static void copyMetadata(@string modPath, @string pkg, @string dst, @string src, map<@string, bool> copiedFiles) {
    for (nint parent = 0; ; parent++) {
        if (copiedMetadata[new metakey(modPath,dst)]) {
            break;
        }
        copiedMetadata[new metakey(modPath,dst)] = true;
        if (parent > 0) {
            copyDir(dst, src, matchMetadata, copiedFiles);
        }
        if (modPath == pkg) {
            break;
        }
        pkg = path.Dir(pkg);
        dst = filepath.Dir(dst);
        src = filepath.Dir(src);
    }
}

// metaPrefixes is the list of metadata file prefixes.
// Vendoring copies metadata files from parents of copied directories.
// Note that this list could be arbitrarily extended, and it is longer
// in other tools (such as godep or dep). By using this limited set of
// prefixes and also insisting on capitalized file names, we are trying
// to nudge people toward more agreement on the naming
// and also trying to avoid false positives.
private static @string metaPrefixes = new slice<@string>(new @string[] { "AUTHORS", "CONTRIBUTORS", "COPYLEFT", "COPYING", "COPYRIGHT", "LEGAL", "LICENSE", "NOTICE", "PATENTS" });

// matchMetadata reports whether info is a metadata file.
private static bool matchMetadata(@string dir, fs.DirEntry info) {
    var name = info.Name();
    foreach (var (_, p) in metaPrefixes) {
        if (strings.HasPrefix(name, p)) {
            return true;
        }
    }    return false;
}

// matchPotentialSourceFile reports whether info may be relevant to a build operation.
private static bool matchPotentialSourceFile(@string dir, fs.DirEntry info) => func((defer, _, _) => {
    if (strings.HasSuffix(info.Name(), "_test.go")) {
        return false;
    }
    if (info.Name() == "go.mod" || info.Name() == "go.sum") {
        {
            var gv = modload.ModFile().Go;

            if (gv != null && semver.Compare("v" + gv.Version, "v1.17") >= 0) { 
                // As of Go 1.17, we strip go.mod and go.sum files from dependency modules.
                // Otherwise, 'go' commands invoked within the vendor subtree may misidentify
                // an arbitrary directory within the vendor tree as a module root.
                // (See https://golang.org/issue/42970.)
                return false;
            }

        }
    }
    if (strings.HasSuffix(info.Name(), ".go")) {
        var (f, err) = fsys.Open(filepath.Join(dir, info.Name()));
        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        defer(f.Close());

        var (content, err) = imports.ReadImports(f, false, null);
        if (err == null && !imports.ShouldBuild(content, imports.AnyTags())) { 
            // The file is explicitly tagged "ignore", so it can't affect the build.
            // Leave it out.
            return false;
        }
        return true;
    }
    return true;
});

// copyDir copies all regular files satisfying match(info) from src to dst.
private static bool copyDir(@string dst, @string src, Func<@string, fs.DirEntry, bool> match, map<@string, bool> copiedFiles) {
    var (files, err) = os.ReadDir(src);
    if (err != null) {
        @base.Fatalf("go mod vendor: %v", err);
    }
    {
        var err__prev1 = err;

        var err = os.MkdirAll(dst, 0777);

        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        err = err__prev1;

    }
    foreach (var (_, file) in files) {
        if (file.IsDir() || !file.Type().IsRegular() || !match(src, file)) {
            continue;
        }
        copiedFiles[file.Name()] = true;
        var (r, err) = os.Open(filepath.Join(src, file.Name()));
        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        var dstPath = filepath.Join(dst, file.Name());
        copiedFiles[dstPath] = true;
        var (w, err) = os.Create(dstPath);
        if (err != null) {
            @base.Fatalf("go mod vendor: %v", err);
        }
        {
            var err__prev1 = err;

            var (_, err) = io.Copy(w, r);

            if (err != null) {
                @base.Fatalf("go mod vendor: %v", err);
            }

            err = err__prev1;

        }
        r.Close();
        {
            var err__prev1 = err;

            err = w.Close();

            if (err != null) {
                @base.Fatalf("go mod vendor: %v", err);
            }

            err = err__prev1;

        }
    }
}

} // end modcmd_package
