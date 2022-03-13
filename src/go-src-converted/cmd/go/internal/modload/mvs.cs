// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:46 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\mvs.go
namespace go.cmd.go.@internal;

using context = context_package;
using errors = errors_package;
using os = os_package;
using sort = sort_package;

using modfetch = cmd.go.@internal.modfetch_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// cmpVersion implements the comparison for versions in the module loader.
//
// It is consistent with semver.Compare except that as a special case,
// the version "" is considered higher than all other versions.
// The main module (also known as the target) has no version and must be chosen
// over other versions of the same module in the module dependency graph.

using System;
public static partial class modload_package {

private static nint cmpVersion(@string v1, @string v2) {
    if (v2 == "") {
        if (v1 == "") {
            return 0;
        }
        return -1;
    }
    if (v1 == "") {
        return 1;
    }
    return semver.Compare(v1, v2);
}

// mvsReqs implements mvs.Reqs for module semantic versions,
// with any exclusions or replacements applied internally.
private partial struct mvsReqs {
    public slice<module.Version> roots;
}

private static (slice<module.Version>, error) Required(this ptr<mvsReqs> _addr_r, module.Version mod) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;
    ref mvsReqs r = ref _addr_r.val;

    if (mod == Target) { 
        // Use the build list as it existed when r was constructed, not the current
        // global build list.
        return (r.roots, error.As(null!)!);
    }
    if (mod.Version == "none") {
        return (null, error.As(null!)!);
    }
    var (summary, err) = goModSummary(mod);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (summary.require, error.As(null!)!);
}

// Max returns the maximum of v1 and v2 according to semver.Compare.
//
// As a special case, the version "" is considered higher than all other
// versions. The main module (also known as the target) has no version and must
// be chosen over other versions of the same module in the module dependency
// graph.
private static @string Max(this ptr<mvsReqs> _addr__p0, @string v1, @string v2) {
    ref mvsReqs _p0 = ref _addr__p0.val;

    if (cmpVersion(v1, v2) < 0) {
        return v2;
    }
    return v1;
}

// Upgrade is a no-op, here to implement mvs.Reqs.
// The upgrade logic for go get -u is in ../modget/get.go.
private static (module.Version, error) Upgrade(this ptr<mvsReqs> _addr__p0, module.Version m) {
    module.Version _p0 = default;
    error _p0 = default!;
    ref mvsReqs _p0 = ref _addr__p0.val;

    return (m, error.As(null!)!);
}

private static (slice<@string>, error) versions(context.Context ctx, @string path, AllowedFunc allowed) {
    slice<@string> _p0 = default;
    error _p0 = default!;
 
    // Note: modfetch.Lookup and repo.Versions are cached,
    // so there's no need for us to add extra caching here.
    slice<@string> versions = default;
    err = modfetch.TryProxies(proxy => {
        var (repo, err) = lookupRepo(proxy, path);
        if (err != null) {
            return err;
        }
        var (allVersions, err) = repo.Versions("");
        if (err != null) {
            return err;
        }
        var allowedVersions = make_slice<@string>(0, len(allVersions));
        foreach (var (_, v) in allVersions) {
            {
                var err__prev1 = err;

                var err = allowed(ctx, new module.Version(Path:path,Version:v));

                if (err == null) {
                    allowedVersions = append(allowedVersions, v);
                }
                else if (!errors.Is(err, ErrDisallowed)) {
                    return err;
                }

                err = err__prev1;

            }
        }        versions = allowedVersions;
        return null;
    });
    return (versions, error.As(err)!);
}

// previousVersion returns the tagged version of m.Path immediately prior to
// m.Version, or version "none" if no prior version is tagged.
//
// Since the version of Target is not found in the version list,
// it has no previous version.
private static (module.Version, error) previousVersion(module.Version m) {
    module.Version _p0 = default;
    error _p0 = default!;
 
    // TODO(golang.org/issue/38714): thread tracing context through MVS.

    if (m == Target) {
        return (new module.Version(Path:m.Path,Version:"none"), error.As(null!)!);
    }
    var (list, err) = versions(context.TODO(), m.Path, CheckAllowed);
    if (err != null) {
        if (errors.Is(err, os.ErrNotExist)) {
            return (new module.Version(Path:m.Path,Version:"none"), error.As(null!)!);
        }
        return (new module.Version(), error.As(err)!);
    }
    var i = sort.Search(len(list), i => semver.Compare(list[i], m.Version) >= 0);
    if (i > 0) {
        return (new module.Version(Path:m.Path,Version:list[i-1]), error.As(null!)!);
    }
    return (new module.Version(Path:m.Path,Version:"none"), error.As(null!)!);
}

private static (module.Version, error) Previous(this ptr<mvsReqs> _addr__p0, module.Version m) {
    module.Version _p0 = default;
    error _p0 = default!;
    ref mvsReqs _p0 = ref _addr__p0.val;

    return previousVersion(m);
}

} // end modload_package
