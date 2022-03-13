// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:26 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\edit.go
namespace go.cmd.go.@internal;

using mvs = cmd.go.@internal.mvs_package;
using context = context_package;
using reflect = reflect_package;
using sort = sort_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// editRequirements returns an edited version of rs such that:
//
//     1. Each module version in mustSelect is selected.
//
//     2. Each module version in tryUpgrade is upgraded toward the indicated
//        version as far as can be done without violating (1).
//
//     3. Each module version in rs.rootModules (or rs.graph, if rs.depth is eager)
//        is downgraded from its original version only to the extent needed to
//        satisfy (1), or upgraded only to the extent needed to satisfy (1) and
//        (2).
//
//     4. No module is upgraded above the maximum version of its path found in the
//        dependency graph of rs, the combined dependency graph of the versions in
//        mustSelect, or the dependencies of each individual module version in
//        tryUpgrade.
//
// Generally, the module versions in mustSelect are due to the module or a
// package within the module matching an explicit command line argument to 'go
// get', and the versions in tryUpgrade are transitive dependencies that are
// either being upgraded by 'go get -u' or being added to satisfy some
// otherwise-missing package import.

using System;
public static partial class modload_package {

private static (ptr<Requirements>, bool, error) editRequirements(context.Context ctx, ptr<Requirements> _addr_rs, slice<module.Version> tryUpgrade, slice<module.Version> mustSelect) {
    ptr<Requirements> edited = default!;
    bool changed = default;
    error err = default!;
    ref Requirements rs = ref _addr_rs.val;

    var (limiter, err) = limiterForEdit(ctx, _addr_rs, tryUpgrade, mustSelect);
    if (err != null) {
        return (_addr_rs!, false, error.As(err)!);
    }
    slice<Conflict> conflicts = default;
    {
        var m__prev1 = m;

        foreach (var (_, __m) in mustSelect) {
            m = __m;
            var (conflict, err) = limiter.Select(m);
            if (err != null) {
                return (_addr_rs!, false, error.As(err)!);
            }
            if (conflict.Path != "") {
                conflicts = append(conflicts, new Conflict(Source:m,Dep:conflict,Constraint:module.Version{Path:conflict.Path,Version:limiter.max[conflict.Path],},));
            }
        }
        m = m__prev1;
    }

    if (len(conflicts) > 0) {
        return (_addr_rs!, false, error.As(addr(new ConstraintError(Conflicts:conflicts))!)!);
    }
    var (mods, changed, err) = selectPotentiallyImportedModules(ctx, _addr_limiter, _addr_rs, tryUpgrade);
    if (err != null) {
        return (_addr_rs!, false, error.As(err)!);
    }
    slice<module.Version> roots = default;
    if (rs.depth == eager) { 
        // In an eager module, modules that provide packages imported by the main
        // module may either be explicit roots or implicit transitive dependencies.
        // We promote the modules in mustSelect to be explicit requirements.
        slice<@string> rootPaths = default;
        {
            var m__prev1 = m;

            foreach (var (_, __m) in mustSelect) {
                m = __m;
                if (m.Version != "none" && m.Path != Target.Path) {
                    rootPaths = append(rootPaths, m.Path);
                }
            }
    else

            m = m__prev1;
        }

        if (!changed && len(rootPaths) == 0) { 
            // The build list hasn't changed and we have no new roots to add.
            // We don't need to recompute the minimal roots for the module.
            return (_addr_rs!, false, error.As(null!)!);
        }
        {
            var m__prev1 = m;

            foreach (var (_, __m) in mods) {
                m = __m;
                {
                    var (v, ok) = rs.rootSelected(m.Path);

                    if (ok && (v == m.Version || rs.direct[m.Path])) { 
                        // m.Path was formerly a root, and either its version hasn't changed or
                        // we believe that it provides a package directly imported by a package
                        // or test in the main module. For now we'll assume that it is still
                        // relevant enough to remain a root. If we actually load all of the
                        // packages and tests in the main module (which we are not doing here),
                        // we can revise the explicit roots at that point.
                        rootPaths = append(rootPaths, m.Path);
                    }
                }
            }
            m = m__prev1;
        }

        roots, err = mvs.Req(Target, rootPaths, addr(new mvsReqs(roots:mods)));
        if (err != null) {
            return (_addr_null!, false, error.As(err)!);
        }
    } { 
        // In a lazy module, every module that provides a package imported by the
        // main module must be retained as a root.
        roots = mods;
        if (!changed) { 
            // Because the roots we just computed are unchanged, the entire graph must
            // be the same as it was before. Save the original rs, since we have
            // probably already loaded its requirement graph.
            return (_addr_rs!, false, error.As(null!)!);
        }
    }
    var direct = make_map<@string, bool>(len(rs.direct));
    {
        var m__prev1 = m;

        foreach (var (_, __m) in roots) {
            m = __m;
            if (rs.direct[m.Path]) {
                direct[m.Path] = true;
            }
        }
        m = m__prev1;
    }

    return (_addr_newRequirements(rs.depth, roots, direct)!, changed, error.As(null!)!);
}

// limiterForEdit returns a versionLimiter with its max versions set such that
// the max version for every module path in mustSelect is the version listed
// there, and the max version for every other module path is the maximum version
// of its path found in the dependency graph of rs, the combined dependency
// graph of the versions in mustSelect, or the dependencies of each individual
// module version in tryUpgrade.
private static (ptr<versionLimiter>, error) limiterForEdit(context.Context ctx, ptr<Requirements> _addr_rs, slice<module.Version> tryUpgrade, slice<module.Version> mustSelect) {
    ptr<versionLimiter> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    var (mg, err) = rs.Graph(ctx);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    map maxVersion = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{}; // module path → version
    Action<module.Version> restrictTo = m => {
        var (v, ok) = maxVersion[m.Path];
        if (!ok || cmpVersion(v, m.Version) > 0) {
            maxVersion[m.Path] = m.Version;
        }
    };

    if (rs.depth == eager) { 
        // Eager go.mod files don't indicate which transitive dependencies are
        // actually relevant to the main module, so we have to assume that any module
        // that could have provided any package — that is, any module whose selected
        // version was not "none" — may be relevant.
        {
            var m__prev1 = m;

            foreach (var (_, __m) in mg.BuildList()) {
                m = __m;
                restrictTo(m);
            }
    else

            m = m__prev1;
        }
    } { 
        // The go.mod file explicitly records every module that provides a package
        // imported by the main module.
        //
        // If we need to downgrade an existing root or a new root found in
        // tryUpgrade, we don't want to allow that downgrade to incidentally upgrade
        // a module imported by the main module to some arbitrary version.
        // However, we don't particularly care about arbitrary upgrades to modules
        // that are (at best) only providing packages imported by tests of
        // dependencies outside the main module.
        {
            var m__prev1 = m;

            foreach (var (_, __m) in rs.rootModules) {
                m = __m;
                restrictTo(new module.Version(Path:m.Path,Version:mg.Selected(m.Path),));
            }

            m = m__prev1;
        }
    }
    {
        var err = raiseLimitsForUpgrades(ctx, maxVersion, rs.depth, tryUpgrade, mustSelect);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    } 

    // The versions in mustSelect override whatever we would naively select —
    // we will downgrade other modules as needed in order to meet them.
    {
        var m__prev1 = m;

        foreach (var (_, __m) in mustSelect) {
            m = __m;
            restrictTo(m);
        }
        m = m__prev1;
    }

    return (_addr_newVersionLimiter(rs.depth, maxVersion)!, error.As(null!)!);
}

// raiseLimitsForUpgrades increases the module versions in maxVersions to the
// versions that would be needed to allow each of the modules in tryUpgrade
// (individually or in any combination) and all of the modules in mustSelect
// (simultaneously) to be added as roots.
//
// Versions not present in maxVersion are unrestricted, and it is assumed that
// they will not be promoted to root requirements (and thus will not contribute
// their own dependencies if the main module is lazy).
//
// These limits provide an upper bound on how far a module may be upgraded as
// part of an incidental downgrade, if downgrades are needed in order to select
// the versions in mustSelect.
private static error raiseLimitsForUpgrades(context.Context ctx, map<@string, @string> maxVersion, modDepth depth, slice<module.Version> tryUpgrade, slice<module.Version> mustSelect) { 
    // allow raises the limit for m.Path to at least m.Version.
    // If m.Path was already unrestricted, it remains unrestricted.
    Action<module.Version> allow = m => {
        var (v, ok) = maxVersion[m.Path];
        if (!ok) {
            return ; // m.Path is unrestricted.
        }
        if (cmpVersion(v, m.Version) < 0) {
            maxVersion[m.Path] = m.Version;
        }
    };

    slice<module.Version> eagerUpgrades = default;    map<@string, bool> isLazyRootPath = default;
    if (depth == eager) {
        eagerUpgrades = tryUpgrade;
    }
    else
 {
        isLazyRootPath = make_map<@string, bool>(len(maxVersion));
        foreach (var (p) in maxVersion) {
            isLazyRootPath[p] = true;
        }        {
            var m__prev1 = m;

            foreach (var (_, __m) in tryUpgrade) {
                m = __m;
                isLazyRootPath[m.Path] = true;
            }

            m = m__prev1;
        }

        {
            var m__prev1 = m;

            foreach (var (_, __m) in mustSelect) {
                m = __m;
                isLazyRootPath[m.Path] = true;
            }

            m = m__prev1;
        }

        map allowedRoot = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};

        Func<module.Version, error> allowRoot = default;
        allowRoot = m => {
            if (allowedRoot[m]) {
                return error.As(null!)!;
            }
            allowedRoot[m] = true;

            if (m.Path == Target.Path) { 
                // Target is already considered to be higher than any possible m, so we
                // won't be upgrading to it anyway and there is no point scanning its
                // dependencies.
                return error.As(null!)!;
            }
            allow(m);

            var (summary, err) = goModSummary(m);
            if (err != null) {
                return error.As(err)!;
            }
            if (summary.depth == eager) { 
                // For efficiency, we'll load all of the eager upgrades as one big
                // graph, rather than loading the (potentially-overlapping) subgraph for
                // each upgrade individually.
                eagerUpgrades = append(eagerUpgrades, m);
                return error.As(null!)!;
            }
            {
                var r__prev1 = r;

                foreach (var (_, __r) in summary.require) {
                    r = __r;
                    if (isLazyRootPath[r.Path]) { 
                        // r could become a root as the result of an upgrade or downgrade,
                        // in which case its dependencies will not be pruned out.
                        // We need to allow those dependencies to be upgraded too.
                        {
                            var err = allowRoot(r);

                            if (err != null) {
                                return error.As(err)!;
                            }

                        }
                    }
                    else
 { 
                        // r will not become a root, so its dependencies don't matter.
                        // Allow only r itself.
                        allow(r);
                    }
                }

                r = r__prev1;
            }

            return error.As(null!)!;
        };

        {
            var m__prev1 = m;

            foreach (var (_, __m) in tryUpgrade) {
                m = __m;
                allowRoot(m);
            }

            m = m__prev1;
        }
    }
    if (len(eagerUpgrades) > 0) { 
        // Compute the max versions for eager upgrades all together.
        // Since these modules are eager, we'll end up scanning all of their
        // transitive dependencies no matter which versions end up selected,
        // and since we have a large dependency graph to scan we might get
        // a significant benefit from not revisiting dependencies that are at
        // common versions among multiple upgrades.
        var (upgradeGraph, err) = readModGraph(ctx, eager, eagerUpgrades);
        if (err != null) {
            if (go117LazyTODO) { 
                // Compute the requirement path from a module path in tryUpgrade to the
                // error, and the requirement path (if any) from rs.rootModules to the
                // tryUpgrade module path. Return a *mvs.BuildListError showing the
                // concatenation of the paths (with an upgrade in the middle).
            }
            return error.As(err)!;
        }
        {
            var r__prev1 = r;

            foreach (var (_, __r) in upgradeGraph.BuildList()) {
                r = __r; 
                // Upgrading to m would upgrade to r, and the caller requested that we
                // try to upgrade to m, so it's ok to upgrade to r.
                allow(r);
            }

            r = r__prev1;
        }
    }
    var nextRoots = append((slice<module.Version>)null, mustSelect);
    while (nextRoots != null) {
        module.Sort(nextRoots);
        var rs = newRequirements(depth, nextRoots, null);
        nextRoots = null;

        var (rs, mustGraph, err) = expandGraph(ctx, rs);
        if (err != null) {
            return error.As(err)!;
        }
        {
            var r__prev2 = r;

            foreach (var (_, __r) in mustGraph.BuildList()) {
                r = __r; 
                // Some module in mustSelect requires r, so we must allow at least
                // r.Version (unless it conflicts with another entry in mustSelect, in
                // which case we will error out either way).
                allow(r);

                if (isLazyRootPath[r.Path]) {
                    {
                        var v__prev2 = v;

                        (v, ok) = rs.rootSelected(r.Path);

                        if (ok && r.Version == v) { 
                            // r is already a root, so its requirements are already included in
                            // the build list.
                            continue;
                        } 

                        // The dependencies in mustSelect may upgrade (or downgrade) an existing
                        // root to match r, which will remain as a root. However, since r is not
                        // a root of rs, its dependencies have been pruned out of this build
                        // list. We need to add it back explicitly so that we allow any
                        // transitive upgrades that r will pull in.

                        v = v__prev2;

                    } 

                    // The dependencies in mustSelect may upgrade (or downgrade) an existing
                    // root to match r, which will remain as a root. However, since r is not
                    // a root of rs, its dependencies have been pruned out of this build
                    // list. We need to add it back explicitly so that we allow any
                    // transitive upgrades that r will pull in.
                    if (nextRoots == null) {
                        nextRoots = rs.rootModules; // already capped
                    }
                    nextRoots = append(nextRoots, r);
                }
            }

            r = r__prev2;
        }
    }

    return error.As(null!)!;
}

// selectPotentiallyImportedModules increases the limiter-selected version of
// every module in rs that potentially provides a package imported (directly or
// indirectly) by the main module, and every module in tryUpgrade, toward the
// highest version seen in rs or tryUpgrade, but not above the maximums enforced
// by the limiter.
//
// It returns the list of module versions selected by the limiter, sorted by
// path, along with a boolean indicating whether that list is different from the
// list of modules read from rs.
private static (slice<module.Version>, bool, error) selectPotentiallyImportedModules(context.Context ctx, ptr<versionLimiter> _addr_limiter, ptr<Requirements> _addr_rs, slice<module.Version> tryUpgrade) {
    slice<module.Version> mods = default;
    bool changed = default;
    error err = default!;
    ref versionLimiter limiter = ref _addr_limiter.val;
    ref Requirements rs = ref _addr_rs.val;

    {
        var m__prev1 = m;

        foreach (var (_, __m) in tryUpgrade) {
            m = __m;
            {
                var err__prev1 = err;

                var err = limiter.UpgradeToward(ctx, m);

                if (err != null) {
                    return (null, false, error.As(err)!);
                }

                err = err__prev1;

            }
        }
        m = m__prev1;
    }

    slice<module.Version> initial = default;
    if (rs.depth == eager) {
        var (mg, err) = rs.Graph(ctx);
        if (err != null) {
            return (null, false, error.As(err)!);
        }
        initial = mg.BuildList()[(int)1..];
    }
    else
 {
        initial = rs.rootModules;
    }
    {
        var m__prev1 = m;

        foreach (var (_, __m) in initial) {
            m = __m;
            {
                var err__prev1 = err;

                err = limiter.UpgradeToward(ctx, m);

                if (err != null) {
                    return (null, false, error.As(err)!);
                }

                err = err__prev1;

            }
        }
        m = m__prev1;
    }

    mods = make_slice<module.Version>(0, len(limiter.selected));
    {
        var path__prev1 = path;
        var v__prev1 = v;

        foreach (var (__path, __v) in limiter.selected) {
            path = __path;
            v = __v;
            if (v != "none" && path != Target.Path) {
                mods = append(mods, new module.Version(Path:path,Version:v));
            }
        }
        path = path__prev1;
        v = v__prev1;
    }

    (mg, err) = readModGraph(ctx, rs.depth, mods);
    if (err != null) {
        return (null, false, error.As(err)!);
    }
    mods = make_slice<module.Version>(0, len(limiter.selected));
    {
        var path__prev1 = path;

        foreach (var (__path, _) in limiter.selected) {
            path = __path;
            if (path != Target.Path) {
                {
                    var v__prev2 = v;

                    var v = mg.Selected(path);

                    if (v != "none") {
                        mods = append(mods, new module.Version(Path:path,Version:v));
                    }

                    v = v__prev2;

                }
            }
        }
        path = path__prev1;
    }

    module.Sort(mods);

    changed = !reflect.DeepEqual(mods, initial);

    return (mods, changed, error.As(err)!);
}

// A versionLimiter tracks the versions that may be selected for each module
// subject to constraints on the maximum versions of transitive dependencies.
private partial struct versionLimiter {
    public modDepth depth; // max maps each module path to the maximum version that may be selected for
// that path.
//
// Paths with no entry are unrestricted, and we assume that they will not be
// promoted to root dependencies (so will not contribute dependencies if the
// main module is lazy).
    public map<@string, @string> max; // selected maps each module path to a version of that path (if known) whose
// transitive dependencies do not violate any max version. The version kept
// is the highest one found during any call to UpgradeToward for the given
// module path.
//
// If a higher acceptable version is found during a call to UpgradeToward for
// some *other* module path, that does not update the selected version.
// Ignoring those versions keeps the downgrades computed for two modules
// together close to the individual downgrades that would be computed for each
// module in isolation. (The only way one module can affect another is if the
// final downgraded version of the one module explicitly requires a higher
// version of the other.)
//
// Version "none" of every module is always known not to violate any max
// version, so paths at version "none" are omitted.
    public map<@string, @string> selected; // dqReason records whether and why each each encountered version is
// disqualified.
    public map<module.Version, dqState> dqReason; // requiring maps each not-yet-disqualified module version to the versions
// that directly require it. If that version becomes disqualified, the
// disqualification will be propagated to all of the versions in the list.
    public map<module.Version, slice<module.Version>> requiring;
}

// A dqState indicates whether and why a module version is “disqualified” from
// being used in a way that would incorporate its requirements.
//
// The zero dqState indicates that the module version is not known to be
// disqualified, either because it is ok or because we are currently traversing
// a cycle that includes it.
private partial struct dqState {
    public error err; // if non-nil, disqualified because the requirements of the module could not be read
    public module.Version conflict; // disqualified because the module (transitively) requires dep, which exceeds the maximum version constraint for its path
}

private static bool isDisqualified(this dqState dq) {
    return dq != new dqState();
}

// newVersionLimiter returns a versionLimiter that restricts the module paths
// that appear as keys in max.
//
// max maps each module path to its maximum version; paths that are not present
// in the map are unrestricted. The limiter assumes that unrestricted paths will
// not be promoted to root dependencies.
//
// If depth is lazy, then if a module passed to UpgradeToward or Select is
// itself lazy, its unrestricted dependencies are skipped when scanning
// requirements.
private static ptr<versionLimiter> newVersionLimiter(modDepth depth, map<@string, @string> max) {
    return addr(new versionLimiter(depth:depth,max:max,selected:map[string]string{Target.Path:Target.Version},dqReason:map[module.Version]dqState{},requiring:map[module.Version][]module.Version{},));
}

// UpgradeToward attempts to upgrade the selected version of m.Path as close as
// possible to m.Version without violating l's maximum version limits.
//
// If depth is lazy and m itself is lazy, the the dependencies of unrestricted
// dependencies of m will not be followed.
private static error UpgradeToward(this ptr<versionLimiter> _addr_l, context.Context ctx, module.Version m) {
    ref versionLimiter l = ref _addr_l.val;

    var (selected, ok) = l.selected[m.Path];
    if (ok) {
        if (cmpVersion(selected, m.Version) >= 0) { 
            // The selected version is already at least m, so no upgrade is needed.
            return error.As(null!)!;
        }
    }
    else
 {
        selected = "none";
    }
    if (l.check(m, l.depth).isDisqualified()) {
        var (candidates, err) = versions(ctx, m.Path, CheckAllowed);
        if (err != null) { 
            // This is likely a transient error reaching the repository,
            // rather than a permanent error with the retrieved version.
            //
            // TODO(golang.org/issue/31730, golang.org/issue/30134):
            // decode what to do based on the actual error.
            return error.As(err)!;
        }
        var i = sort.Search(len(candidates), i => error.As(semver.Compare(candidates[i], m.Version) >= 0)!);
        candidates = candidates[..(int)i];

        while (l.check(m, l.depth).isDisqualified()) {
            var n = len(candidates);
            if (n == 0 || cmpVersion(selected, candidates[n - 1]) >= 0) { 
                // We couldn't find a suitable candidate above the already-selected version.
                // Retain that version unmodified.
                return error.As(null!)!;
            }
            (m.Version, candidates) = (candidates[n - 1], candidates[..(int)n - 1]);
        }
    }
    l.selected[m.Path] = m.Version;
    return error.As(null!)!;
}

// Select attempts to set the selected version of m.Path to exactly m.Version.
private static (module.Version, error) Select(this ptr<versionLimiter> _addr_l, module.Version m) {
    module.Version conflict = default;
    error err = default!;
    ref versionLimiter l = ref _addr_l.val;

    var dq = l.check(m, l.depth);
    if (!dq.isDisqualified()) {
        l.selected[m.Path] = m.Version;
    }
    return (dq.conflict, error.As(dq.err)!);
}

// check determines whether m (or its transitive dependencies) would violate l's
// maximum version limits if added to the module requirement graph.
//
// If depth is lazy and m itself is lazy, then the dependencies of unrestricted
// dependencies of m will not be followed. If the lazy loading invariants hold
// for the main module up to this point, the packages in those modules are at
// best only imported by tests of dependencies that are themselves loaded from
// outside modules. Although we would like to keep 'go test all' as reproducible
// as is feasible, we don't want to retain test dependencies that are only
// marginally relevant at best.
private static dqState check(this ptr<versionLimiter> _addr_l, module.Version m, modDepth depth) {
    ref versionLimiter l = ref _addr_l.val;

    if (m.Version == "none" || m == Target) { 
        // version "none" has no requirements, and the dependencies of Target are
        // tautological.
        return new dqState();
    }
    {
        var dq__prev1 = dq;

        var (dq, seen) = l.dqReason[m];

        if (seen) {
            return dq;
        }
        dq = dq__prev1;

    }
    l.dqReason[m] = new dqState();

    {
        var (max, ok) = l.max[m.Path];

        if (ok && cmpVersion(m.Version, max) > 0) {
            return l.disqualify(m, new dqState(conflict:m));
        }
    }

    var (summary, err) = goModSummary(m);
    if (err != null) { 
        // If we can't load the requirements, we couldn't load the go.mod file.
        // There are a number of reasons this can happen, but this usually
        // means an older version of the module had a missing or invalid
        // go.mod file. For example, if example.com/mod released v2.0.0 before
        // migrating to modules (v2.0.0+incompatible), then added a valid go.mod
        // in v2.0.1, downgrading from v2.0.1 would cause this error.
        //
        // TODO(golang.org/issue/31730, golang.org/issue/30134): if the error
        // is transient (we couldn't download go.mod), return the error from
        // Downgrade. Currently, we can't tell what kind of error it is.
        return l.disqualify(m, new dqState(err:err));
    }
    if (summary.depth == eager) {
        depth = eager;
    }
    foreach (var (_, r) in summary.require) {
        if (depth == lazy) {
            {
                var (_, restricted) = l.max[r.Path];

                if (!restricted) { 
                    // r.Path is unrestricted, so we don't care at what version it is
                    // selected. We assume that r.Path will not become a root dependency, so
                    // since m is lazy, r's dependencies won't be followed.
                    continue;
                }

            }
        }
        {
            var dq__prev1 = dq;

            var dq = l.check(r, depth);

            if (dq.isDisqualified()) {
                return l.disqualify(m, dq);
            } 

            // r and its dependencies are (perhaps provisionally) ok.
            //
            // However, if there are cycles in the requirement graph, we may have only
            // checked a portion of the requirement graph so far, and r (and thus m) may
            // yet be disqualified by some path we have not yet visited. Remember this edge
            // so that we can disqualify m and its dependents if that occurs.

            dq = dq__prev1;

        } 

        // r and its dependencies are (perhaps provisionally) ok.
        //
        // However, if there are cycles in the requirement graph, we may have only
        // checked a portion of the requirement graph so far, and r (and thus m) may
        // yet be disqualified by some path we have not yet visited. Remember this edge
        // so that we can disqualify m and its dependents if that occurs.
        l.requiring[r] = append(l.requiring[r], m);
    }    return new dqState();
}

// disqualify records that m (or one of its transitive dependencies)
// violates l's maximum version limits.
private static dqState disqualify(this ptr<versionLimiter> _addr_l, module.Version m, dqState dq) {
    ref versionLimiter l = ref _addr_l.val;

    {
        var dq = l.dqReason[m];

        if (dq.isDisqualified()) {
            return dq;
        }
    }
    l.dqReason[m] = dq;

    foreach (var (_, p) in l.requiring[m]) {
        l.disqualify(p, new dqState(conflict:m));
    }    delete(l.requiring, m);
    return dq;
}

} // end modload_package
