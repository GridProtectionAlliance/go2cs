// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:55 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\vendor.go
namespace go.cmd.go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;

using @base = cmd.go.@internal.@base_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;
using System;

public static partial class modload_package {

private static sync.Once vendorOnce = default;private static slice<module.Version> vendorList = default;private static slice<module.Version> vendorReplaced = default;private static map<@string, @string> vendorVersion = default;private static map<@string, module.Version> vendorPkgModule = default;private static map<module.Version, vendorMetadata> vendorMeta = default;

private partial struct vendorMetadata {
    public bool Explicit;
    public module.Version Replacement;
    public @string GoVersion;
}

// readVendorList reads the list of vendored modules from vendor/modules.txt.
private static void readVendorList() {
    vendorOnce.Do(() => {
        vendorList = null;
        vendorPkgModule = make_map<@string, module.Version>();
        vendorVersion = make_map<@string, @string>();
        vendorMeta = make_map<module.Version, vendorMetadata>();
        var (data, err) = os.ReadFile(filepath.Join(ModRoot(), "vendor/modules.txt"));
        if (err != null) {
            if (!errors.Is(err, fs.ErrNotExist)) {
                @base.Fatalf("go: %s", err);
            }
            return ;
        }
        module.Version mod = default;
        foreach (var (_, line) in strings.Split(string(data), "\n")) {
            if (strings.HasPrefix(line, "# ")) {
                var f = strings.Fields(line);

                if (len(f) < 3) {
                    continue;
                }
                if (semver.IsValid(f[2])) { 
                    // A module, but we don't yet know whether it is in the build list or
                    // only included to indicate a replacement.
                    mod = new module.Version(Path:f[1],Version:f[2]);
                    f = f[(int)3..];
                }
                else if (f[2] == "=>") { 
                    // A wildcard replacement found in the main module's go.mod file.
                    mod = new module.Version(Path:f[1]);
                    f = f[(int)2..];
                }
                else
 { 
                    // Not a version or a wildcard replacement.
                    // We don't know how to interpret this module line, so ignore it.
                    mod = new module.Version();
                    continue;
                }
                if (len(f) >= 2 && f[0] == "=>") {
                    var meta = vendorMeta[mod];
                    if (len(f) == 2) { 
                        // File replacement.
                        meta.Replacement = new module.Version(Path:f[1]);
                        vendorReplaced = append(vendorReplaced, mod);
                    }
                    else if (len(f) == 3 && semver.IsValid(f[2])) { 
                        // Path and version replacement.
                        meta.Replacement = new module.Version(Path:f[1],Version:f[2]);
                        vendorReplaced = append(vendorReplaced, mod);
                    }
                    else
 { 
                        // We don't understand this replacement. Ignore it.
                    }
                    vendorMeta[mod] = meta;
                }
                continue;
            } 

            // Not a module line. Must be a package within a module or a metadata
            // directive, either of which requires a preceding module line.
            if (mod.Path == "") {
                continue;
            }
            if (strings.HasPrefix(line, "## ")) { 
                // Metadata. Take the union of annotations across multiple lines, if present.
                meta = vendorMeta[mod];
                foreach (var (_, entry) in strings.Split(strings.TrimPrefix(line, "## "), ";")) {
                    entry = strings.TrimSpace(entry);
                    if (entry == "explicit") {
                        meta.Explicit = true;
                    }
                    if (strings.HasPrefix(entry, "go ")) {
                        meta.GoVersion = strings.TrimPrefix(entry, "go ");
                        rawGoVersion.Store(mod, meta.GoVersion);
                    } 
                    // All other tokens are reserved for future use.
                }
                vendorMeta[mod] = meta;
                continue;
            }
            {
                var f__prev1 = f;

                f = strings.Fields(line);

                if (len(f) == 1 && module.CheckImportPath(f[0]) == null) { 
                    // A package within the current module.
                    vendorPkgModule[f[0]] = mod; 

                    // Since this module provides a package for the build, we know that it
                    // is in the build list and is the selected version of its path.
                    // If this information is new, record it.
                    {
                        var (v, ok) = vendorVersion[mod.Path];

                        if (!ok || semver.Compare(v, mod.Version) < 0) {
                            vendorList = append(vendorList, mod);
                            vendorVersion[mod.Path] = mod.Version;
                        }

                    }
                }

                f = f__prev1;

            }
        }
    });
}

// checkVendorConsistency verifies that the vendor/modules.txt file matches (if
// go 1.14) or at least does not contradict (go 1.13 or earlier) the
// requirements and replacements listed in the main module's go.mod file.
private static void checkVendorConsistency() {
    readVendorList();

    var pre114 = false;
    if (semver.Compare(index.goVersionV, "v1.14") < 0) { 
        // Go versions before 1.14 did not include enough information in
        // vendor/modules.txt to check for consistency.
        // If we know that we're on an earlier version, relax the consistency check.
        pre114 = true;
    }
    ptr<object> vendErrors = @new<strings.Builder>();
    Action<module.Version, @string, object[]> vendErrorf = (mod, format, args) => {
        var detail = fmt.Sprintf(format, args);
        if (mod.Version == "") {
            fmt.Fprintf(vendErrors, "\n\t%s: %s", mod.Path, detail);
        }
        else
 {
            fmt.Fprintf(vendErrors, "\n\t%s@%s: %s", mod.Path, mod.Version, detail);
        }
    }; 

    // Iterate over the Require directives in their original (not indexed) order
    // so that the errors match the original file.
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Require) {
            r = __r;
            if (!vendorMeta[r.Mod].Explicit) {
                if (pre114) { 
                    // Before 1.14, modules.txt did not indicate whether modules were listed
                    // explicitly in the main module's go.mod file.
                    // However, we can at least detect a version mismatch if packages were
                    // vendored from a non-matching version.
                    {
                        var (vv, ok) = vendorVersion[r.Mod.Path];

                        if (ok && vv != r.Mod.Version) {
                            vendErrorf(r.Mod, fmt.Sprintf("is explicitly required in go.mod, but vendor/modules.txt indicates %s@%s", r.Mod.Path, vv));
                        }

                    }
                }
                else
 {
                    vendErrorf(r.Mod, "is explicitly required in go.mod, but not marked as explicit in vendor/modules.txt");
                }
            }
        }
        r = r__prev1;
    }

    Func<module.Version, @string> describe = m => {
        if (m.Version == "") {
            return m.Path;
        }
        return m.Path + "@" + m.Version;
    }; 

    // We need to verify *all* replacements that occur in modfile: even if they
    // don't directly apply to any module in the vendor list, the replacement
    // go.mod file can affect the selected versions of other (transitive)
    // dependencies
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Replace) {
            r = __r;
            var vr = vendorMeta[r.Old].Replacement;
            if (vr == (new module.Version())) {
                if (pre114 && (r.Old.Version == "" || vendorVersion[r.Old.Path] != r.Old.Version)) { 
                    // Before 1.14, modules.txt omitted wildcard replacements and
                    // replacements for modules that did not have any packages to vendor.
                }
                else
 {
                    vendErrorf(r.Old, "is replaced in go.mod, but not marked as replaced in vendor/modules.txt");
                }
            }
            else if (vr != r.New) {
                vendErrorf(r.Old, "is replaced by %s in go.mod, but marked as replaced by %s in vendor/modules.txt", describe(r.New), describe(vr));
            }
        }
        r = r__prev1;
    }

    {
        var mod__prev1 = mod;

        foreach (var (_, __mod) in vendorList) {
            mod = __mod;
            var meta = vendorMeta[mod];
            if (meta.Explicit) {
                {
                    var (_, inGoMod) = index.require[mod];

                    if (!inGoMod) {
                        vendErrorf(mod, "is marked as explicit in vendor/modules.txt, but not explicitly required in go.mod");
                    }

                }
            }
        }
        mod = mod__prev1;
    }

    {
        var mod__prev1 = mod;

        foreach (var (_, __mod) in vendorReplaced) {
            mod = __mod;
            var r = Replacement(mod);
            if (r == (new module.Version())) {
                vendErrorf(mod, "is marked as replaced in vendor/modules.txt, but not replaced in go.mod");
                continue;
            }
            {
                var meta__prev1 = meta;

                meta = vendorMeta[mod];

                if (r != meta.Replacement) {
                    vendErrorf(mod, "is marked as replaced by %s in vendor/modules.txt, but replaced by %s in go.mod", describe(meta.Replacement), describe(r));
                }

                meta = meta__prev1;

            }
        }
        mod = mod__prev1;
    }

    if (vendErrors.Len() > 0) {
        @base.Fatalf("go: inconsistent vendoring in %s:%s\n\n\tTo ignore the vendor directory, use -mod=readonly or -mod=mod.\n\tTo sync the vendor directory, run:\n\t\tgo mod vendor", modRoot, vendErrors);
    }
}

} // end modload_package
