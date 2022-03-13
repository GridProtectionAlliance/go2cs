// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:22 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\buildlist.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using mvs = cmd.go.@internal.mvs_package;
using par = cmd.go.@internal.par_package;
using context = context_package;
using fmt = fmt_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using debug = runtime.debug_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// capVersionSlice returns s with its cap reduced to its length.

using System;
public static partial class modload_package {

private static slice<module.Version> capVersionSlice(slice<module.Version> s) {
    return s.slice(-1, len(s), len(s));
}

// A Requirements represents a logically-immutable set of root module requirements.
public partial struct Requirements {
    public modDepth depth; // rootModules is the set of module versions explicitly required by the main
// module, sorted and capped to length. It may contain duplicates, and may
// contain multiple versions for a given module path.
    public slice<module.Version> rootModules;
    public map<@string, @string> maxRootVersion; // direct is the set of module paths for which we believe the module provides
// a package directly imported by a package or test in the main module.
//
// The "direct" map controls which modules are annotated with "// indirect"
// comments in the go.mod file, and may impact which modules are listed as
// explicit roots (vs. indirect-only dependencies). However, it should not
// have a semantic effect on the build list overall.
//
// The initial direct map is populated from the existing "// indirect"
// comments (or lack thereof) in the go.mod file. It is updated by the
// package loader: dependencies may be promoted to direct if new
// direct imports are observed, and may be demoted to indirect during
// 'go mod tidy' or 'go mod vendor'.
//
// The direct map is keyed by module paths, not module versions. When a
// module's selected version changes, we assume that it remains direct if the
// previous version was a direct dependency. That assumption might not hold in
// rare cases (such as if a dependency splits out a nested module, or merges a
// nested module back into a parent module).
    public map<@string, bool> direct;
    public sync.Once graphOnce; // guards writes to (but not reads from) graph
    public atomic.Value graph; // cachedGraph
}

// A cachedGraph is a non-nil *ModuleGraph, together with any error discovered
// while loading that graph.
private partial struct cachedGraph {
    public ptr<ModuleGraph> mg;
    public error err; // If err is non-nil, mg may be incomplete (but must still be non-nil).
}

// requirements is the requirement graph for the main module.
//
// It is always non-nil if the main module's go.mod file has been loaded.
//
// This variable should only be read from the loadModFile function, and should
// only be written in the loadModFile and commitRequirements functions.
// All other functions that need or produce a *Requirements should
// accept and/or return an explicit parameter.
private static ptr<Requirements> requirements;

// newRequirements returns a new requirement set with the given root modules.
// The dependencies of the roots will be loaded lazily at the first call to the
// Graph method.
//
// The rootModules slice must be sorted according to module.Sort.
// The caller must not modify the rootModules slice or direct map after passing
// them to newRequirements.
//
// If vendoring is in effect, the caller must invoke initVendor on the returned
// *Requirements before any other method.
private static ptr<Requirements> newRequirements(modDepth depth, slice<module.Version> rootModules, map<@string, bool> direct) => func((_, panic, _) => {
    {
        var m__prev1 = m;

        foreach (var (__i, __m) in rootModules) {
            i = __i;
            m = __m;
            if (m == Target) {
                panic(fmt.Sprintf("newRequirements called with untrimmed build list: rootModules[%v] is Target", i));
            }
            if (m.Path == "" || m.Version == "") {
                panic(fmt.Sprintf("bad requirement: rootModules[%v] = %v", i, m));
            }
            if (i > 0) {
                var prev = rootModules[i - 1];
                if (prev.Path > m.Path || (prev.Path == m.Path && semver.Compare(prev.Version, m.Version) > 0)) {
                    panic(fmt.Sprintf("newRequirements called with unsorted roots: %v", rootModules));
                }
            }
        }
        m = m__prev1;
    }

    ptr<Requirements> rs = addr(new Requirements(depth:depth,rootModules:capVersionSlice(rootModules),maxRootVersion:make(map[string]string,len(rootModules)),direct:direct,));

    {
        var m__prev1 = m;

        foreach (var (_, __m) in rootModules) {
            m = __m;
            {
                var (v, ok) = rs.maxRootVersion[m.Path];

                if (ok && cmpVersion(v, m.Version) >= 0) {
                    continue;
                }

            }
            rs.maxRootVersion[m.Path] = m.Version;
        }
        m = m__prev1;
    }

    return _addr_rs!;
});

// initVendor initializes rs.graph from the given list of vendored module
// dependencies, overriding the graph that would normally be loaded from module
// requirements.
private static void initVendor(this ptr<Requirements> _addr_rs, slice<module.Version> vendorList) {
    ref Requirements rs = ref _addr_rs.val;

    rs.graphOnce.Do(() => {
        ptr<ModuleGraph> mg = addr(new ModuleGraph(g:mvs.NewGraph(cmpVersion,[]module.Version{Target}),));

        if (rs.depth == lazy) { 
            // The roots of a lazy module should already include every module in the
            // vendor list, because the vendored modules are the same as those
            // maintained as roots by the lazy loading “import invariant”.
            //
            // Just to be sure, we'll double-check that here.
            var inconsistent = false;
            foreach (var (_, m) in vendorList) {
                {
                    var (v, ok) = rs.rootSelected(m.Path);

                    if (!ok || v != m.Version) {
                        @base.Errorf("go: vendored module %v should be required explicitly in go.mod", m);
                        inconsistent = true;
                    }

                }
            }
        else
            if (inconsistent) {
                @base.Fatalf("go: %v", errGoModDirty);
            } 

            // Now we can treat the rest of the module graph as effectively “pruned
            // out”, like a more aggressive version of lazy loading: in vendor mode,
            // the root requirements *are* the complete module graph.
            mg.g.Require(Target, rs.rootModules);
        } { 
            // The transitive requirements of the main module are not in general available
            // from the vendor directory, and we don't actually know how we got from
            // the roots to the final build list.
            //
            // Instead, we'll inject a fake "vendor/modules.txt" module that provides
            // those transitive dependencies, and mark it as a dependency of the main
            // module. That allows us to elide the actual structure of the module
            // graph, but still distinguishes between direct and indirect
            // dependencies.
            module.Version vendorMod = new module.Version(Path:"vendor/modules.txt",Version:"");
            mg.g.Require(Target, append(rs.rootModules, vendorMod));
            mg.g.Require(vendorMod, vendorList);
        }
        rs.graph.Store(new cachedGraph(mg,nil));
    });
}

// rootSelected returns the version of the root dependency with the given module
// path, or the zero module.Version and ok=false if the module is not a root
// dependency.
private static (@string, bool) rootSelected(this ptr<Requirements> _addr_rs, @string path) {
    @string version = default;
    bool ok = default;
    ref Requirements rs = ref _addr_rs.val;

    if (path == Target.Path) {
        return (Target.Version, true);
    }
    {
        var (v, ok) = rs.maxRootVersion[path];

        if (ok) {
            return (v, true);
        }
    }
    return ("", false);
}

// hasRedundantRoot returns true if the root list contains multiple requirements
// of the same module or a requirement on any version of the main module.
// Redundant requirements should be pruned, but they may influence version
// selection.
private static bool hasRedundantRoot(this ptr<Requirements> _addr_rs) {
    ref Requirements rs = ref _addr_rs.val;

    foreach (var (i, m) in rs.rootModules) {
        if (m.Path == Target.Path || (i > 0 && m.Path == rs.rootModules[i - 1].Path)) {
            return true;
        }
    }    return false;
}

// Graph returns the graph of module requirements loaded from the current
// root modules (as reported by RootModules).
//
// Graph always makes a best effort to load the requirement graph despite any
// errors, and always returns a non-nil *ModuleGraph.
//
// If the requirements of any relevant module fail to load, Graph also
// returns a non-nil error of type *mvs.BuildListError.
private static (ptr<ModuleGraph>, error) Graph(this ptr<Requirements> _addr_rs, context.Context ctx) {
    ptr<ModuleGraph> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    rs.graphOnce.Do(() => {
        var (mg, mgErr) = readModGraph(ctx, rs.depth, rs.rootModules);
        rs.graph.Store(new cachedGraph(mg,mgErr));
    });
    cachedGraph cached = rs.graph.Load()._<cachedGraph>();
    return (_addr_cached.mg!, error.As(cached.err)!);
}

// IsDirect returns whether the given module provides a package directly
// imported by a package or test in the main module.
private static bool IsDirect(this ptr<Requirements> _addr_rs, @string path) {
    ref Requirements rs = ref _addr_rs.val;

    return rs.direct[path];
}

// A ModuleGraph represents the complete graph of module dependencies
// of a main module.
//
// If the main module is lazily loaded, the graph does not include
// transitive dependencies of non-root (implicit) dependencies.
public partial struct ModuleGraph {
    public ptr<mvs.Graph> g;
    public par.Cache loadCache; // module.Version → summaryError

    public sync.Once buildListOnce;
    public slice<module.Version> buildList;
}

// A summaryError is either a non-nil modFileSummary or a non-nil error
// encountered while reading or parsing that summary.
private partial struct summaryError {
    public ptr<modFileSummary> summary;
    public error err;
}

private static sync.Once readModGraphDebugOnce = default;

// readModGraph reads and returns the module dependency graph starting at the
// given roots.
//
// Unlike LoadModGraph, readModGraph does not attempt to diagnose or update
// inconsistent roots.
private static (ptr<ModuleGraph>, error) readModGraph(context.Context ctx, modDepth depth, slice<module.Version> roots) {
    ptr<ModuleGraph> _p0 = default!;
    error _p0 = default!;

    if (depth == lazy) {
        readModGraphDebugOnce.Do(() => {
            foreach (var (_, f) in strings.Split(os.Getenv("GODEBUG"), ",")) {
                switch (f) {
                    case "lazymod=log": 
                        debug.PrintStack();
                        fmt.Fprintf(os.Stderr, "go: read full module graph.\n");
                        break;
                    case "lazymod=strict": 
                        debug.PrintStack();
                        @base.Fatalf("go: read full module graph (forbidden by GODEBUG=lazymod=strict).");
                        break;
                }
            }
        });
    }
    sync.Mutex mu = default;    bool hasError = default;    ptr<ModuleGraph> mg = addr(new ModuleGraph(g:mvs.NewGraph(cmpVersion,[]module.Version{Target}),));
    mg.g.Require(Target, roots);

    var loadQueue = par.NewQueue(runtime.GOMAXPROCS(0));    sync.Map loadingEager = default; 

    // loadOne synchronously loads the explicit requirements for module m.
    // It does not load the transitive requirements of m even if the go version in
    // m's go.mod file indicates eager loading.
    Func<module.Version, (ptr<modFileSummary>, error)> loadOne = m => {
        summaryError cached = mg.loadCache.Do(m, () => {
            var (summary, err) = goModSummary(m);

            mu.Lock();
            if (err == null) {
                mg.g.Require(m, summary.require);
            }
            else
 {
                hasError = true;
            }
            mu.Unlock();

            return _addr_new summaryError(summary,err)!;
        })._<summaryError>();

        return (_addr_cached.summary!, error.As(cached.err)!);
    };

    Action<module.Version, modDepth> enqueue = default;
    enqueue = (m, depth) => {
        if (m.Version == "none") {
            return ;
        }
        if (depth == eager) {
            {
                var (_, dup) = loadingEager.LoadOrStore(m, null);

                if (dup) { 
                    // m has already been enqueued for loading. Since eager loading may
                    // follow cycles in the the requirement graph, we need to return early
                    // to avoid making the load queue infinitely long.
                    return ;
                }

            }
        }
        loadQueue.Add(() => {
            (summary, err) = loadOne(m);
            if (err != null) {
                return ; // findError will report the error later.
            } 

            // If the version in m's go.mod file implies eager loading, then we cannot
            // assume that the explicit requirements of m (added by loadOne) are
            // sufficient to build the packages it contains. We must load its full
            // transitive dependency graph to be sure that we see all relevant
            // dependencies.
            if (depth == eager || summary.depth == eager) {
                foreach (var (_, r) in summary.require) {
                    enqueue(r, eager);
                }
            }
        });
    };

    foreach (var (_, m) in roots) {
        enqueue(m, depth);
    }    loadQueue.Idle().Receive();

    if (hasError) {
        return (_addr_mg!, error.As(mg.findError())!);
    }
    return (_addr_mg!, error.As(null!)!);
}

// RequiredBy returns the dependencies required by module m in the graph,
// or ok=false if module m's dependencies are not relevant (such as if they
// are pruned out by lazy loading).
//
// The caller must not modify the returned slice, but may safely append to it
// and may rely on it not to be modified.
private static (slice<module.Version>, bool) RequiredBy(this ptr<ModuleGraph> _addr_mg, module.Version m) {
    slice<module.Version> reqs = default;
    bool ok = default;
    ref ModuleGraph mg = ref _addr_mg.val;

    return mg.g.RequiredBy(m);
}

// Selected returns the selected version of the module with the given path.
//
// If no version is selected, Selected returns version "none".
private static @string Selected(this ptr<ModuleGraph> _addr_mg, @string path) {
    @string version = default;
    ref ModuleGraph mg = ref _addr_mg.val;

    return mg.g.Selected(path);
}

// WalkBreadthFirst invokes f once, in breadth-first order, for each module
// version other than "none" that appears in the graph, regardless of whether
// that version is selected.
private static void WalkBreadthFirst(this ptr<ModuleGraph> _addr_mg, Action<module.Version> f) {
    ref ModuleGraph mg = ref _addr_mg.val;

    mg.g.WalkBreadthFirst(f);
}

// BuildList returns the selected versions of all modules present in the graph,
// beginning with Target.
//
// The order of the remaining elements in the list is deterministic
// but arbitrary.
//
// The caller must not modify the returned list, but may safely append to it
// and may rely on it not to be modified.
private static slice<module.Version> BuildList(this ptr<ModuleGraph> _addr_mg) {
    ref ModuleGraph mg = ref _addr_mg.val;

    mg.buildListOnce.Do(() => {
        mg.buildList = capVersionSlice(mg.g.BuildList());
    });
    return mg.buildList;
}

private static error findError(this ptr<ModuleGraph> _addr_mg) {
    ref ModuleGraph mg = ref _addr_mg.val;

    var errStack = mg.g.FindPath(m => {
        var cached = mg.loadCache.Get(m);
        return error.As(cached != null && cached._<summaryError>().err != null)!;
    });
    if (len(errStack) > 0) {
        summaryError err = mg.loadCache.Get(errStack[len(errStack) - 1])._<summaryError>().err;
        Func<module.Version, module.Version, bool> noUpgrade = default;
        return error.As(mvs.NewBuildListError(err, errStack, noUpgrade))!;
    }
    return error.As(null!)!;
}

private static bool allRootsSelected(this ptr<ModuleGraph> _addr_mg) {
    ref ModuleGraph mg = ref _addr_mg.val;

    var (roots, _) = mg.g.RequiredBy(Target);
    foreach (var (_, m) in roots) {
        if (mg.Selected(m.Path) != m.Version) {
            return false;
        }
    }    return true;
}

// LoadModGraph loads and returns the graph of module dependencies of the main module,
// without loading any packages.
//
// If the goVersion string is non-empty, the returned graph is the graph
// as interpreted by the given Go version (instead of the version indicated
// in the go.mod file).
//
// Modules are loaded automatically (and lazily) in LoadPackages:
// LoadModGraph need only be called if LoadPackages is not,
// typically in commands that care about modules but no particular package.
public static ptr<ModuleGraph> LoadModGraph(context.Context ctx, @string goVersion) {
    var rs = LoadModFile(ctx);

    if (goVersion != "") {
        var depth = modDepthFromGoVersion(goVersion);
        if (depth == eager && rs.depth != eager) { 
            // Use newRequirements instead of convertDepth because convertDepth
            // also updates roots; here, we want to report the unmodified roots
            // even though they may seem inconsistent.
            rs = newRequirements(eager, rs.rootModules, rs.direct);
        }
        var (mg, err) = rs.Graph(ctx);
        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
        return _addr_mg!;
    }
    var (rs, mg, err) = expandGraph(ctx, _addr_rs);
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    commitRequirements(ctx, modFileGoVersion(), rs);
    return _addr_mg!;
}

// expandGraph loads the complete module graph from rs.
//
// If the complete graph reveals that some root of rs is not actually the
// selected version of its path, expandGraph computes a new set of roots that
// are consistent. (When lazy loading is implemented, this may result in
// upgrades to other modules due to requirements that were previously pruned
// out.)
//
// expandGraph returns the updated roots, along with the module graph loaded
// from those roots and any error encountered while loading that graph.
// expandGraph returns non-nil requirements and a non-nil graph regardless of
// errors. On error, the roots might not be updated to be consistent.
private static (ptr<Requirements>, ptr<ModuleGraph>, error) expandGraph(context.Context ctx, ptr<Requirements> _addr_rs) {
    ptr<Requirements> _p0 = default!;
    ptr<ModuleGraph> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    var (mg, mgErr) = rs.Graph(ctx);
    if (mgErr != null) { 
        // Without the graph, we can't update the roots: we don't know which
        // versions of transitive dependencies would be selected.
        return (_addr_rs!, _addr_mg!, error.As(mgErr)!);
    }
    if (!mg.allRootsSelected()) { 
        // The roots of rs are not consistent with the rest of the graph. Update
        // them. In an eager module this is a no-op for the build list as a whole —
        // it just promotes what were previously transitive requirements to be
        // roots — but in a lazy module it may pull in previously-irrelevant
        // transitive dependencies.

        var (newRS, rsErr) = updateRoots(ctx, rs.direct, _addr_rs, null, null, false);
        if (rsErr != null) { 
            // Failed to update roots, perhaps because of an error in a transitive
            // dependency needed for the update. Return the original Requirements
            // instead.
            return (_addr_rs!, _addr_mg!, error.As(rsErr)!);
        }
        rs = newRS;
        mg, mgErr = rs.Graph(ctx);
    }
    return (_addr_rs!, _addr_mg!, error.As(mgErr)!);
}

// EditBuildList edits the global build list by first adding every module in add
// to the existing build list, then adjusting versions (and adding or removing
// requirements as needed) until every module in mustSelect is selected at the
// given version.
//
// (Note that the newly-added modules might not be selected in the resulting
// build list: they could be lower than existing requirements or conflict with
// versions in mustSelect.)
//
// If the versions listed in mustSelect are mutually incompatible (due to one of
// the listed modules requiring a higher version of another), EditBuildList
// returns a *ConstraintError and leaves the build list in its previous state.
//
// On success, EditBuildList reports whether the selected version of any module
// in the build list may have been changed (possibly to or from "none") as a
// result.
public static (bool, error) EditBuildList(context.Context ctx, slice<module.Version> add, slice<module.Version> mustSelect) {
    bool changed = default;
    error err = default!;

    var (rs, changed, err) = editRequirements(ctx, LoadModFile(ctx), add, mustSelect);
    if (err != null) {
        return (false, error.As(err)!);
    }
    commitRequirements(ctx, modFileGoVersion(), rs);
    return (changed, error.As(err)!);
}

// A ConstraintError describes inconsistent constraints in EditBuildList
public partial struct ConstraintError {
    public slice<Conflict> Conflicts;
}

private static @string Error(this ptr<ConstraintError> _addr_e) {
    ref ConstraintError e = ref _addr_e.val;

    ptr<object> b = @new<strings.Builder>();
    b.WriteString("version constraints conflict:");
    foreach (var (_, c) in e.Conflicts) {
        fmt.Fprintf(b, "\n\t%v requires %v, but %v is requested", c.Source, c.Dep, c.Constraint);
    }    return b.String();
}

// A Conflict documents that Source requires Dep, which conflicts with Constraint.
// (That is, Dep has the same module path as Constraint but a higher version.)
public partial struct Conflict {
    public module.Version Source;
    public module.Version Dep;
    public module.Version Constraint;
}

// tidyRoots trims the root dependencies to the minimal requirements needed to
// both retain the same versions of all packages in pkgs and satisfy the
// lazy loading invariants (if applicable).
private static (ptr<Requirements>, error) tidyRoots(context.Context ctx, ptr<Requirements> _addr_rs, slice<ptr<loadPkg>> pkgs) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    if (rs.depth == eager) {
        return _addr_tidyEagerRoots(ctx, rs.direct, pkgs)!;
    }
    return _addr_tidyLazyRoots(ctx, rs.direct, pkgs)!;
}

private static (ptr<Requirements>, error) updateRoots(context.Context ctx, map<@string, bool> direct, ptr<Requirements> _addr_rs, slice<ptr<loadPkg>> pkgs, slice<module.Version> add, bool rootsImported) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    if (rs.depth == eager) {
        return _addr_updateEagerRoots(ctx, direct, _addr_rs, add)!;
    }
    return _addr_updateLazyRoots(ctx, direct, _addr_rs, pkgs, add, rootsImported)!;
}

// tidyLazyRoots returns a minimal set of root requirements that maintains the
// "lazy loading" invariants of the go.mod file for the given packages:
//
//     1. For each package marked with pkgInAll, the module path that provided that
//        package is included as a root.
//     2. For all packages, the module that provided that package either remains
//        selected at the same version or is upgraded by the dependencies of a
//        root.
//
// If any module that provided a package has been upgraded above its previous,
// version, the caller may need to reload and recompute the package graph.
//
// To ensure that the loading process eventually converges, the caller should
// add any needed roots from the tidy root set (without removing existing untidy
// roots) until the set of roots has converged.
private static (ptr<Requirements>, error) tidyLazyRoots(context.Context ctx, map<@string, bool> direct, slice<ptr<loadPkg>> pkgs) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;

    slice<module.Version> roots = default;    map pathIncluded = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{Target.Path:true}; 
    // We start by adding roots for every package in "all".
    //
    // Once that is done, we may still need to add more roots to cover upgraded or
    // otherwise-missing test dependencies for packages in "all". For those test
    // dependencies, we prefer to add roots for packages with shorter import
    // stacks first, on the theory that the module requirements for those will
    // tend to fill in the requirements for their transitive imports (which have
    // deeper import stacks). So we add the missing dependencies for one depth at
    // a time, starting with the packages actually in "all" and expanding outwards
    // until we have scanned every package that was loaded.
    slice<ptr<loadPkg>> queue = default;    map queued = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<loadPkg>, bool>{};
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            if (!pkg.flags.has(pkgInAll)) {
                continue;
            }
            if (pkg.fromExternalModule() && !pathIncluded[pkg.mod.Path]) {
                roots = append(roots, pkg.mod);
                pathIncluded[pkg.mod.Path] = true;
            }
            queue = append(queue, pkg);
            queued[pkg] = true;
        }
        pkg = pkg__prev1;
    }

    module.Sort(roots);
    var tidy = newRequirements(lazy, roots, direct);

    while (len(queue) > 0) {
        roots = tidy.rootModules;
        var (mg, err) = tidy.Graph(ctx);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        var prevQueue = queue;
        queue = null;
        {
            var pkg__prev2 = pkg;

            foreach (var (_, __pkg) in prevQueue) {
                pkg = __pkg;
                var m = pkg.mod;
                if (m.Path == "") {
                    continue;
                }
                foreach (var (_, dep) in pkg.imports) {
                    if (!queued[dep]) {
                        queue = append(queue, dep);
                        queued[dep] = true;
                    }
                }
                if (pkg.test != null && !queued[pkg.test]) {
                    queue = append(queue, pkg.test);
                    queued[pkg.test] = true;
                }
                if (!pathIncluded[m.Path]) {
                    {
                        var s = mg.Selected(m.Path);

                        if (cmpVersion(s, m.Version) < 0) {
                            roots = append(roots, m);
                        }

                    }
                    pathIncluded[m.Path] = true;
                }
            }

            pkg = pkg__prev2;
        }

        if (len(roots) > len(tidy.rootModules)) {
            module.Sort(roots);
            tidy = newRequirements(lazy, roots, tidy.direct);
        }
    }

    var (_, err) = tidy.Graph(ctx);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_tidy!, error.As(null!)!);
}

// updateLazyRoots returns a set of root requirements that maintains the “lazy
// loading” invariants of the go.mod file:
//
//     1. The selected version of the module providing each package marked with
//        either pkgInAll or pkgIsRoot is included as a root.
//        Note that certain root patterns (such as '...') may explode the root set
//        to contain every module that provides any package imported (or merely
//        required) by any other module.
//     2. Each root appears only once, at the selected version of its path
//        (if rs.graph is non-nil) or at the highest version otherwise present as a
//        root (otherwise).
//     3. Every module path that appears as a root in rs remains a root.
//     4. Every version in add is selected at its given version unless upgraded by
//        (the dependencies of) an existing root or another module in add.
//
// The packages in pkgs are assumed to have been loaded from either the roots of
// rs or the modules selected in the graph of rs.
//
// The above invariants together imply the “lazy loading” invariants for the
// go.mod file:
//
//     1. (The import invariant.) Every module that provides a package transitively
//        imported by any package or test in the main module is included as a root.
//        This follows by induction from (1) and (3) above. Transitively-imported
//        packages loaded during this invocation are marked with pkgInAll (1),
//        and by hypothesis any transitively-imported packages loaded in previous
//        invocations were already roots in rs (3).
//
//     2. (The argument invariant.) Every module that provides a package matching
//        an explicit package pattern is included as a root. This follows directly
//        from (1): packages matching explicit package patterns are marked with
//        pkgIsRoot.
//
//     3. (The completeness invariant.) Every module that contributed any package
//        to the build is required by either the main module or one of the modules
//        it requires explicitly. This invariant is left up to the caller, who must
//        not load packages from outside the module graph but may add roots to the
//        graph, but is facilited by (3). If the caller adds roots to the graph in
//        order to resolve missing packages, then updateLazyRoots will retain them,
//        the selected versions of those roots cannot regress, and they will
//        eventually be written back to the main module's go.mod file.
//
// (See https://golang.org/design/36460-lazy-module-loading#invariants for more
// detail.)
private static (ptr<Requirements>, error) updateLazyRoots(context.Context ctx, map<@string, bool> direct, ptr<Requirements> _addr_rs, slice<ptr<loadPkg>> pkgs, slice<module.Version> add, bool rootsImported) => func((_, panic, _) => {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    var roots = rs.rootModules;
    var rootsUpgraded = false;

    map spotCheckRoot = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{}; 

    // “The selected version of the module providing each package marked with
    // either pkgInAll or pkgIsRoot is included as a root.”
    var needSort = false;
    foreach (var (_, pkg) in pkgs) {
        if (!pkg.fromExternalModule()) { 
            // pkg was not loaded from a module dependency, so we don't need
            // to do anything special to maintain that dependency.
            continue;
        }

        if (pkg.flags.has(pkgInAll))         else if (rootsImported && pkg.flags.has(pkgFromRoot))         else if (pkg.flags.has(pkgIsRoot))         else 
            // pkg is a dependency of some other package outside of the main module.
            // As far as we know it's not relevant to the main module (and thus not
            // relevant to consumers of the main module either), and its dependencies
            // should already be in the module graph — included in the dependencies of
            // the package that imported it.
            continue;
                {
            var (_, ok) = rs.rootSelected(pkg.mod.Path);

            if (ok) { 
                // It is possible that the main module's go.mod file is incomplete or
                // otherwise erroneous — for example, perhaps the author forgot to 'git
                // add' their updated go.mod file after adding a new package import, or
                // perhaps they made an edit to the go.mod file using a third-party tool
                // ('git merge'?) that doesn't maintain consistency for module
                // dependencies. If that happens, ideally we want to detect the missing
                // requirements and fix them up here.
                //
                // However, we also need to be careful not to be too aggressive. For
                // transitive dependencies of external tests, the go.mod file for the
                // module containing the test itself is expected to provide all of the
                // relevant dependencies, and we explicitly don't want to pull in
                // requirements on *irrelevant* requirements that happen to occur in the
                // go.mod files for these transitive-test-only dependencies. (See the test
                // in mod_lazy_test_horizon.txt for a concrete example.
                //
                // The “goldilocks zone” seems to be to spot-check exactly the same
                // modules that we promote to explicit roots: namely, those that provide
                // packages transitively imported by the main module, and those that
                // provide roots of the package-import graph. That will catch erroneous
                // edits to the main module's go.mod file and inconsistent requirements in
                // dependencies that provide imported packages, but will ignore erroneous
                // or misleading requirements in dependencies that aren't obviously
                // relevant to the packages in the main module.
                spotCheckRoot[pkg.mod] = true;
            }
            else
 {
                roots = append(roots, pkg.mod);
                rootsUpgraded = true; 
                // The roots slice was initially sorted because rs.rootModules was sorted,
                // but the root we just added could be out of order.
                needSort = true;
            }

        }
    }    {
        var m__prev1 = m;

        foreach (var (_, __m) in add) {
            m = __m;
            {
                var v__prev1 = v;

                var (v, ok) = rs.rootSelected(m.Path);

                if (!ok || cmpVersion(v, m.Version) < 0) {
                    roots = append(roots, m);
                    rootsUpgraded = true;
                    needSort = true;
                }

                v = v__prev1;

            }
        }
        m = m__prev1;
    }

    if (needSort) {
        module.Sort(roots);
    }
    while (true) {
        ptr<ModuleGraph> mg;
        if (rootsUpgraded) { 
            // We've added or upgraded one or more roots, so load the full module
            // graph so that we can update those roots to be consistent with other
            // requirements.
            if (cfg.BuildMod != "mod") { 
                // Our changes to the roots may have moved dependencies into or out of
                // the lazy-loading horizon, which could in turn change the selected
                // versions of other modules. (Unlike for eager modules, for lazy
                // modules adding or removing an explicit root is a semantic change, not
                // just a cosmetic one.)
                return (_addr_rs!, error.As(errGoModDirty)!);
            }
            rs = newRequirements(lazy, roots, direct);
            error err = default!;
            mg, err = rs.Graph(ctx);
            if (err != null) {
                return (_addr_rs!, error.As(err)!);
            }
        }
        else
 { 
            // Since none of the roots have been upgraded, we have no reason to
            // suspect that they are inconsistent with the requirements of any other
            // roots. Only look at the full module graph if we've already loaded it;
            // otherwise, just spot-check the explicit requirements of the roots from
            // which we loaded packages.
            if (rs.graph.Load() != null) { 
                // We've already loaded the full module graph, which includes the
                // requirements of all of the root modules — even the transitive
                // requirements, if they are eager!
                mg, _ = rs.Graph(ctx);
            }
            else if (cfg.BuildMod == "vendor") { 
                // We can't spot-check the requirements of other modules because we
                // don't in general have their go.mod files available in the vendor
                // directory. (Fortunately this case is impossible, because mg.graph is
                // always non-nil in vendor mode!)
                panic("internal error: rs.graph is unexpectedly nil with -mod=vendor");
            }
            else if (!spotCheckRoots(ctx, _addr_rs, spotCheckRoot)) { 
                // We spot-checked the explicit requirements of the roots that are
                // relevant to the packages we've loaded. Unfortunately, they're
                // inconsistent in some way; we need to load the full module graph
                // so that we can fix the roots properly.
                err = default!;
                mg, err = rs.Graph(ctx);
                if (err != null) {
                    return (_addr_rs!, error.As(err)!);
                }
            }
        }
        roots = make_slice<module.Version>(0, len(rs.rootModules));
        rootsUpgraded = false;
        var inRootPaths = make_map<@string, bool>(len(rs.rootModules) + 1);
        inRootPaths[Target.Path] = true;
        {
            var m__prev2 = m;

            foreach (var (_, __m) in rs.rootModules) {
                m = __m;
                if (inRootPaths[m.Path]) { 
                    // This root specifies a redundant path. We already retained the
                    // selected version of this path when we saw it before, so omit the
                    // redundant copy regardless of its version.
                    //
                    // When we read the full module graph, we include the dependencies of
                    // every root even if that root is redundant. That better preserves
                    // reproducibility if, say, some automated tool adds a redundant
                    // 'require' line and then runs 'go mod tidy' to try to make everything
                    // consistent, since the requirements of the older version are carried
                    // over.
                    //
                    // So omitting a root that was previously present may *reduce* the
                    // selected versions of non-roots, but merely removing a requirement
                    // cannot *increase* the selected versions of other roots as a result —
                    // we don't need to mark this change as an upgrade. (This particular
                    // change cannot invalidate any other roots.)
                    continue;
                }
                @string v = default;
                if (mg == null) {
                    v, _ = rs.rootSelected(m.Path);
                }
                else
 {
                    v = mg.Selected(m.Path);
                }
                roots = append(roots, new module.Version(Path:m.Path,Version:v));
                inRootPaths[m.Path] = true;
                if (v != m.Version) {
                    rootsUpgraded = true;
                }
            } 
            // Note that rs.rootModules was already sorted by module path and version,
            // and we appended to the roots slice in the same order and guaranteed that
            // each path has only one version, so roots is also sorted by module path
            // and (trivially) version.

            m = m__prev2;
        }

        if (!rootsUpgraded) {
            if (cfg.BuildMod != "mod") { 
                // The only changes to the root set (if any) were to remove duplicates.
                // The requirements are consistent (if perhaps redundant), so keep the
                // original rs to preserve its ModuleGraph.
                return (_addr_rs!, error.As(null!)!);
            } 
            // The root set has converged: every root going into this iteration was
            // already at its selected version, although we have have removed other
            // (redundant) roots for the same path.
            break;
        }
    }

    if (rs.depth == lazy && reflect.DeepEqual(roots, rs.rootModules) && reflect.DeepEqual(direct, rs.direct)) { 
        // The root set is unchanged and rs was already lazy, so keep rs to
        // preserve its cached ModuleGraph (if any).
        return (_addr_rs!, error.As(null!)!);
    }
    return (_addr_newRequirements(lazy, roots, direct)!, error.As(null!)!);
});

// spotCheckRoots reports whether the versions of the roots in rs satisfy the
// explicit requirements of the modules in mods.
private static bool spotCheckRoots(context.Context ctx, ptr<Requirements> _addr_rs, map<module.Version, bool> mods) => func((defer, _, _) => {
    ref Requirements rs = ref _addr_rs.val;

    var (ctx, cancel) = context.WithCancel(ctx);
    defer(cancel());

    var work = par.NewQueue(runtime.GOMAXPROCS(0));
    {
        var m__prev1 = m;

        foreach (var (__m) in mods) {
            m = __m;
            var m = m;
            work.Add(() => {
                if (ctx.Err() != null) {
                    return ;
                }
                var (summary, err) = goModSummary(m);
                if (err != null) {
                    cancel();
                    return ;
                }
                foreach (var (_, r) in summary.require) {
                    {
                        var (v, ok) = rs.rootSelected(r.Path);

                        if (ok && cmpVersion(v, r.Version) < 0) {
                            cancel();
                            return ;
                        }

                    }
                }
            });
        }
        m = m__prev1;
    }

    work.Idle().Receive();

    if (ctx.Err() != null) { 
        // Either we failed a spot-check, or the caller no longer cares about our
        // answer anyway.
        return false;
    }
    return true;
});

// tidyEagerRoots returns a minimal set of root requirements that maintains the
// selected version of every module that provided a package in pkgs, and
// includes the selected version of every such module in direct as a root.
private static (ptr<Requirements>, error) tidyEagerRoots(context.Context ctx, map<@string, bool> direct, slice<ptr<loadPkg>> pkgs) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;

    slice<module.Version> keep = default;    map keptPath = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    slice<@string> rootPaths = default;    map inRootPaths = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    foreach (var (_, pkg) in pkgs) {
        if (!pkg.fromExternalModule()) {
            continue;
        }
        {
            var m = pkg.mod;

            if (!keptPath[m.Path]) {
                keep = append(keep, m);
                keptPath[m.Path] = true;
                if (direct[m.Path] && !inRootPaths[m.Path]) {
                    rootPaths = append(rootPaths, m.Path);
                    inRootPaths[m.Path] = true;
                }
            }

        }
    }    var (min, err) = mvs.Req(Target, rootPaths, addr(new mvsReqs(roots:keep)));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newRequirements(eager, min, direct)!, error.As(null!)!);
}

// updateEagerRoots returns a set of root requirements that includes the selected
// version of every module path in direct as a root, and maintains the selected
// version of every module selected in the graph of rs.
//
// The roots are updated such that:
//
//     1. The selected version of every module path in direct is included as a root
//        (if it is not "none").
//     2. Each root is the selected version of its path. (We say that such a root
//        set is “consistent”.)
//     3. Every version selected in the graph of rs remains selected unless upgraded
//        by a dependency in add.
//     4. Every version in add is selected at its given version unless upgraded by
//        (the dependencies of) an existing root or another module in add.
private static (ptr<Requirements>, error) updateEagerRoots(context.Context ctx, map<@string, bool> direct, ptr<Requirements> _addr_rs, slice<module.Version> add) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    var (mg, err) = rs.Graph(ctx);
    if (err != null) { 
        // We can't ignore errors in the module graph even if the user passed the -e
        // flag to try to push past them. If we can't load the complete module
        // dependencies, then we can't reliably compute a minimal subset of them.
        return (_addr_rs!, error.As(err)!);
    }
    if (cfg.BuildMod != "mod") { 
        // Instead of actually updating the requirements, just check that no updates
        // are needed.
        if (rs == null) { 
            // We're being asked to reconstruct the requirements from scratch,
            // but we aren't even allowed to modify them.
            return (_addr_rs!, error.As(errGoModDirty)!);
        }
        {
            var m__prev1 = m;

            foreach (var (_, __m) in rs.rootModules) {
                m = __m;
                if (m.Version != mg.Selected(m.Path)) { 
                    // The root version v is misleading: the actual selected version is higher.
                    return (_addr_rs!, error.As(errGoModDirty)!);
                }
            }

            m = m__prev1;
        }

        {
            var m__prev1 = m;

            foreach (var (_, __m) in add) {
                m = __m;
                if (m.Version != mg.Selected(m.Path)) {
                    return (_addr_rs!, error.As(errGoModDirty)!);
                }
            }

            m = m__prev1;
        }

        foreach (var (mPath) in direct) {
            {
                var (_, ok) = rs.rootSelected(mPath);

                if (!ok) { 
                    // Module m is supposed to be listed explicitly, but isn't.
                    //
                    // Note that this condition is also detected (and logged with more
                    // detail) earlier during package loading, so it shouldn't actually be
                    // possible at this point — this is just a defense in depth.
                    return (_addr_rs!, error.As(errGoModDirty)!);
                }

            }
        }        return (_addr_rs!, error.As(null!)!);
    }
    slice<@string> rootPaths = default;    map inRootPaths = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    foreach (var (_, root) in rs.rootModules) { 
        // If the selected version of the root is the same as what was already
        // listed in the go.mod file, retain it as a root (even if redundant) to
        // avoid unnecessary churn. (See https://golang.org/issue/34822.)
        //
        // We do this even for indirect requirements, since we don't know why they
        // were added and they could become direct at any time.
        if (!inRootPaths[root.Path] && mg.Selected(root.Path) == root.Version) {
            rootPaths = append(rootPaths, root.Path);
            inRootPaths[root.Path] = true;
        }
    }    var keep = append(mg.BuildList()[(int)1..], add);
    {
        var m__prev1 = m;

        foreach (var (_, __m) in keep) {
            m = __m;
            if (direct[m.Path] && !inRootPaths[m.Path]) {
                rootPaths = append(rootPaths, m.Path);
                inRootPaths[m.Path] = true;
            }
        }
        m = m__prev1;
    }

    var (min, err) = mvs.Req(Target, rootPaths, addr(new mvsReqs(roots:keep)));
    if (err != null) {
        return (_addr_rs!, error.As(err)!);
    }
    if (rs.depth == eager && reflect.DeepEqual(min, rs.rootModules) && reflect.DeepEqual(direct, rs.direct)) { 
        // The root set is unchanged and rs was already eager, so keep rs to
        // preserve its cached ModuleGraph (if any).
        return (_addr_rs!, error.As(null!)!);
    }
    return (_addr_newRequirements(eager, min, direct)!, error.As(null!)!);
}

// convertDepth returns a version of rs with the given depth.
// If rs already has the given depth, convertDepth returns rs unmodified.
private static (ptr<Requirements>, error) convertDepth(context.Context ctx, ptr<Requirements> _addr_rs, modDepth depth) {
    ptr<Requirements> _p0 = default!;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    if (rs.depth == depth) {
        return (_addr_rs!, error.As(null!)!);
    }
    if (depth == eager) { 
        // We are converting a lazy module to an eager one. The roots of an eager
        // module graph are a superset of the roots of a lazy graph, so we don't
        // need to add any new roots — we just need to prune away the ones that are
        // redundant given eager loading, which is exactly what updateEagerRoots
        // does.
        return _addr_updateEagerRoots(ctx, rs.direct, _addr_rs, null)!;
    }
    var (mg, err) = rs.Graph(ctx);
    if (err != null) {
        return (_addr_rs!, error.As(err)!);
    }
    return (_addr_newRequirements(lazy, mg.BuildList()[(int)1..], rs.direct)!, error.As(null!)!);
}

} // end modload_package
