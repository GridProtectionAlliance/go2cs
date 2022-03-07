// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mvs implements Minimal Version Selection.
// See https://research.swtch.com/vgo-mvs.
// package mvs -- go2cs converted at 2022 March 06 23:18:02 UTC
// import "cmd/go/internal/mvs" ==> using mvs = go.cmd.go.@internal.mvs_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\mvs\mvs.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using sync = go.sync_package;

using par = go.cmd.go.@internal.par_package;

using module = go.golang.org.x.mod.module_package;
using System;


namespace go.cmd.go.@internal;

public static partial class mvs_package {

    // A Reqs is the requirement graph on which Minimal Version Selection (MVS) operates.
    //
    // The version strings are opaque except for the special version "none"
    // (see the documentation for module.Version). In particular, MVS does not
    // assume that the version strings are semantic versions; instead, the Max method
    // gives access to the comparison operation.
    //
    // It must be safe to call methods on a Reqs from multiple goroutines simultaneously.
    // Because a Reqs may read the underlying graph from the network on demand,
    // the MVS algorithms parallelize the traversal to overlap network delays.
public partial interface Reqs {
    @string Required(module.Version m); // Max returns the maximum of v1 and v2 (it returns either v1 or v2).
//
// For all versions v, Max(v, "none") must be v,
// and for the target passed as the first argument to MVS functions,
// Max(target, v) must be target.
//
// Note that v1 < v2 can be written Max(v1, v2) != v1
// and similarly v1 <= v2 can be written Max(v1, v2) == v2.
    @string Max(@string v1, @string v2);
}

// An UpgradeReqs is a Reqs that can also identify available upgrades.
public partial interface UpgradeReqs {
    (module.Version, error) Upgrade(module.Version m);
}

// A DowngradeReqs is a Reqs that can also identify available downgrades.
public partial interface DowngradeReqs {
    (module.Version, error) Previous(module.Version m);
}

// BuildList returns the build list for the target module.
//
// target is the root vertex of a module requirement graph. For cmd/go, this is
// typically the main module, but note that this algorithm is not intended to
// be Go-specific: module paths and versions are treated as opaque values.
//
// reqs describes the module requirement graph and provides an opaque method
// for comparing versions.
//
// BuildList traverses the graph and returns a list containing the highest
// version for each visited module. The first element of the returned list is
// target itself; reqs.Max requires target.Version to compare higher than all
// other versions, so no other version can be selected. The remaining elements
// of the list are sorted by path.
//
// See https://research.swtch.com/vgo-mvs for details.
public static (slice<module.Version>, error) BuildList(module.Version target, Reqs reqs) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;

    return buildList(target, reqs, null);
}

private static (slice<module.Version>, error) buildList(module.Version target, Reqs reqs, Func<module.Version, (module.Version, error)> upgrade) => func((_, panic, _) => {
    slice<module.Version> _p0 = default;
    error _p0 = default!;

    Func<@string, @string, nint> cmp = (v1, v2) => {
        if (reqs.Max(v1, v2) != v1) {
            return -1;
        }
        if (reqs.Max(v2, v1) != v2) {
            return 1;
        }
        return 0;

    };

    sync.Mutex mu = default;    var g = NewGraph(cmp, new slice<module.Version>(new module.Version[] { target }));    map upgrades = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, module.Version>{};    map errs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, error>{}; 

    // Explore work graph in parallel in case reqs.Required
    // does high-latency network operations.
    par.Work work = default;
    work.Add(target);
    work.Do(10, item => {
        module.Version m = item._<module.Version>();

        slice<module.Version> required = default;
        error err = default!;
        if (m.Version != "none") {
            required, err = reqs.Required(m);
        }
        var u = m;
        if (upgrade != null) {
            var (upgradeTo, upErr) = upgrade(m);
            if (upErr == null) {
                u = upgradeTo;
            }
            else if (err == null) {
                err = error.As(upErr)!;
            }

        }
        mu.Lock();
        if (err != null) {
            errs[m] = err;
        }
        if (u != m) {
            upgrades[m] = u;
            required = append(new slice<module.Version>(new module.Version[] { u }), required);
        }
        g.Require(m, required);
        mu.Unlock();

        foreach (var (_, r) in required) {
            work.Add(r);
        }
    }); 

    // If there was an error, find the shortest path from the target to the
    // node where the error occurred so we can report a useful error message.
    if (len(errs) > 0) {
        var errPath = g.FindPath(m => {
            return errs[m] != null;
        });
        if (len(errPath) == 0) {
            panic("internal error: could not reconstruct path to module with error");
        }
        err = errs[errPath[len(errPath) - 1]];
        Func<module.Version, module.Version, bool> isUpgrade = (from, to) => {
            {
                var u__prev2 = u;

                var (u, ok) = upgrades[from];

                if (ok) {
                    return u == to;
                }

                u = u__prev2;

            }

            return false;

        };
        return (null, error.As(NewBuildListError(err._<error>(), errPath, isUpgrade))!);

    }
    var list = g.BuildList();
    {
        var v = list[0];

        if (v != target) { 
            // target.Version will be "" for modload, the main client of MVS.
            // "" denotes the main module, which has no version. However, MVS treats
            // version strings as opaque, so "" is not a special value here.
            // See golang.org/issue/31491, golang.org/issue/29773.
            panic(fmt.Sprintf("mistake: chose version %q instead of target %+v", v, target));

        }
    }

    return (list, error.As(null!)!);

});

// Req returns the minimal requirement list for the target module,
// with the constraint that all module paths listed in base must
// appear in the returned list.
public static (slice<module.Version>, error) Req(module.Version target, slice<@string> @base, Reqs reqs) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;

    var (list, err) = BuildList(target, reqs);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<module.Version> postorder = default;
    map reqCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, slice<module.Version>>{};
    reqCache[target] = null;
    Func<module.Version, error> walk = default;
    walk = m => {
        var (_, ok) = reqCache[m];
        if (ok) {
            return null;
        }
        var (required, err) = reqs.Required(m);
        if (err != null) {
            return err;
        }
        reqCache[m] = required;
        {
            var m1__prev1 = m1;

            foreach (var (_, __m1) in required) {
                m1 = __m1;
                {
                    var err__prev1 = err;

                    var err = walk(m1);

                    if (err != null) {
                        return err;
                    }

                    err = err__prev1;

                }

            }

            m1 = m1__prev1;
        }

        postorder = append(postorder, m);
        return null;

    };
    {
        var m__prev1 = m;

        foreach (var (_, __m) in list) {
            m = __m;
            {
                var err__prev1 = err;

                err = walk(m);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

        }
        m = m__prev1;
    }

    map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};
    walk = m => {
        if (have[m]) {
            return null;
        }
        have[m] = true;
        {
            var m1__prev1 = m1;

            foreach (var (_, __m1) in reqCache[m]) {
                m1 = __m1;
                walk(m1);
            }

            m1 = m1__prev1;
        }

        return null;

    };
    map max = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
    {
        var m__prev1 = m;

        foreach (var (_, __m) in list) {
            m = __m;
            {
                var (v, ok) = max[m.Path];

                if (ok) {
                    max[m.Path] = reqs.Max(m.Version, v);
                }
                else
 {
                    max[m.Path] = m.Version;
                }

            }

        }
        m = m__prev1;
    }

    slice<module.Version> min = default;
    map haveBase = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    foreach (var (_, path) in base) {
        if (haveBase[path]) {
            continue;
        }
        module.Version m = new module.Version(Path:path,Version:max[path]);
        min = append(min, m);
        walk(m);
        haveBase[path] = true;

    }    for (var i = len(postorder) - 1; i >= 0; i--) {
        m = postorder[i];
        if (max[m.Path] != m.Version) { 
            // Older version.
            continue;

        }
        if (!have[m]) {
            min = append(min, m);
            walk(m);
        }
    }
    sort.Slice(min, (i, j) => {
        return min[i].Path < min[j].Path;
    });
    return (min, error.As(null!)!);

}

// UpgradeAll returns a build list for the target module
// in which every module is upgraded to its latest version.
public static (slice<module.Version>, error) UpgradeAll(module.Version target, UpgradeReqs reqs) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;

    return buildList(target, reqs, m => {
        if (m.Path == target.Path) {
            return (target, error.As(null!)!);
        }
        return reqs.Upgrade(m);

    });

}

// Upgrade returns a build list for the target module
// in which the given additional modules are upgraded.
public static (slice<module.Version>, error) Upgrade(module.Version target, UpgradeReqs reqs, params module.Version[] upgrade) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;
    upgrade = upgrade.Clone();

    var (list, err) = reqs.Required(target);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var pathInList = make_map<@string, bool>(len(list));
    foreach (var (_, m) in list) {
        pathInList[m.Path] = true;
    }    list = append((slice<module.Version>)null, list);

    var upgradeTo = make_map<@string, @string>(len(upgrade));
    foreach (var (_, u) in upgrade) {
        if (!pathInList[u.Path]) {
            list = append(list, new module.Version(Path:u.Path,Version:"none"));
        }
        {
            var (prev, dup) = upgradeTo[u.Path];

            if (dup) {
                upgradeTo[u.Path] = reqs.Max(prev, u.Version);
            }
            else
 {
                upgradeTo[u.Path] = u.Version;
            }

        }

    }    return buildList(target, addr(new override(target,list,reqs)), m => {
        {
            var (v, ok) = upgradeTo[m.Path];

            if (ok) {
                return (new module.Version(Path:m.Path,Version:v), error.As(null!)!);
            }

        }

        return (m, error.As(null!)!);

    });

}

// Downgrade returns a build list for the target module
// in which the given additional modules are downgraded,
// potentially overriding the requirements of the target.
//
// The versions to be downgraded may be unreachable from reqs.Latest and
// reqs.Previous, but the methods of reqs must otherwise handle such versions
// correctly.
public static (slice<module.Version>, error) Downgrade(module.Version target, DowngradeReqs reqs, params module.Version[] downgrade) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;
    downgrade = downgrade.Clone();
 
    // Per https://research.swtch.com/vgo-mvs#algorithm_4:
    // “To avoid an unnecessary downgrade to E 1.1, we must also add a new
    // requirement on E 1.2. We can apply Algorithm R to find the minimal set of
    // new requirements to write to go.mod.”
    //
    // In order to generate those new requirements, we need to identify versions
    // for every module in the build list — not just reqs.Required(target).
    var (list, err) = BuildList(target, reqs);
    if (err != null) {
        return (null, error.As(err)!);
    }
    list = list[(int)1..]; // remove target

    var max = make_map<@string, @string>();
    {
        var r__prev1 = r;

        foreach (var (_, __r) in list) {
            r = __r;
            max[r.Path] = r.Version;
        }
        r = r__prev1;
    }

    foreach (var (_, d) in downgrade) {
        {
            var v__prev1 = v;

            var (v, ok) = max[d.Path];

            if (!ok || reqs.Max(v, d.Version) != d.Version) {
                max[d.Path] = d.Version;
            }

            v = v__prev1;

        }

    }    var added = make_map<module.Version, bool>();    var rdeps = make_map<module.Version, slice<module.Version>>();    var excluded = make_map<module.Version, bool>();
    Action<module.Version> exclude = default;
    exclude = m => {
        if (excluded[m]) {
            return ;
        }
        excluded[m] = true;
        {
            var p__prev1 = p;

            foreach (var (_, __p) in rdeps[m]) {
                p = __p;
                exclude(p);
            }

            p = p__prev1;
        }
    };
    Action<module.Version> add = default;
    add = m => {
        if (added[m]) {
            return ;
        }
        added[m] = true;
        {
            var v__prev1 = v;

            (v, ok) = max[m.Path];

            if (ok && reqs.Max(m.Version, v) != v) { 
                // m would upgrade an existing dependency — it is not a strict downgrade,
                // and because it was already present as a dependency, it could affect the
                // behavior of other relevant packages.
                exclude(m);
                return ;

            }

            v = v__prev1;

        }

        (list, err) = reqs.Required(m);
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
            exclude(m);
            return ;

        }
        {
            var r__prev1 = r;

            foreach (var (_, __r) in list) {
                r = __r;
                add(r);
                if (excluded[r]) {
                    exclude(m);
                    return ;
                }
                rdeps[r] = append(rdeps[r], m);
            }

            r = r__prev1;
        }
    };

    var downgraded = make_slice<module.Version>(0, len(list) + 1);
    downgraded = append(downgraded, target);
List: 

    // The downgrades we computed above only downgrade to versions enumerated by
    // reqs.Previous. However, reqs.Previous omits some versions — such as
    // pseudo-versions and retracted versions — that may be selected as transitive
    // requirements of other modules.
    //
    // If one of those requirements pulls the version back up above the version
    // identified by reqs.Previous, then the transitive dependencies of that that
    // initially-downgraded version should no longer matter — in particular, we
    // should not add new dependencies on module paths that nothing else in the
    // updated module graph even requires.
    //
    // In order to eliminate those spurious dependencies, we recompute the build
    // list with the actual versions of the downgraded modules as selected by MVS,
    // instead of our initial downgrades.
    // (See the downhiddenartifact and downhiddencross test cases).
    {
        var r__prev1 = r;

        foreach (var (_, __r) in list) {
            r = __r;
            add(r);
            while (excluded[r]) {
                var (p, err) = reqs.Previous(r);
                if (err != null) { 
                    // This is likely a transient error reaching the repository,
                    // rather than a permanent error with the retrieved version.
                    //
                    // TODO(golang.org/issue/31730, golang.org/issue/30134):
                    // decode what to do based on the actual error.
                    return (null, error.As(err)!);

                } 
                // If the target version is a pseudo-version, it may not be
                // included when iterating over prior versions using reqs.Previous.
                // Insert it into the right place in the iteration.
                // If v is excluded, p should be returned again by reqs.Previous on the next iteration.
                {
                    var v__prev1 = v;

                    var v = max[r.Path];

                    if (reqs.Max(v, r.Version) != v && reqs.Max(p.Version, v) != p.Version) {
                        p.Version = v;
                    }

                    v = v__prev1;

                }

                if (p.Version == "none") {
                    _continueList = true;
                    break;
                }

                add(p);
                r = p;

            }

            downgraded = append(downgraded, r);

        }
        r = r__prev1;
    }
    var (actual, err) = BuildList(target, addr(new override(target:target,list:downgraded,Reqs:reqs,)));
    if (err != null) {
        return (null, error.As(err)!);
    }
    var actualVersion = make_map<@string, @string>(len(actual));
    {
        var m__prev1 = m;

        foreach (var (_, __m) in actual) {
            m = __m;
            actualVersion[m.Path] = m.Version;
        }
        m = m__prev1;
    }

    downgraded = downgraded[..(int)0];
    {
        var m__prev1 = m;

        foreach (var (_, __m) in list) {
            m = __m;
            {
                var v__prev1 = v;

                (v, ok) = actualVersion[m.Path];

                if (ok) {
                    downgraded = append(downgraded, new module.Version(Path:m.Path,Version:v));
                }

                v = v__prev1;

            }

        }
        m = m__prev1;
    }

    return BuildList(target, addr(new override(target:target,list:downgraded,Reqs:reqs,)));

}

private partial struct @override : Reqs {
    public module.Version target;
    public slice<module.Version> list;
    public Reqs Reqs;
}

private static (slice<module.Version>, error) Required(this ptr<override> _addr_r, module.Version m) {
    slice<module.Version> _p0 = default;
    error _p0 = default!;
    ref override r = ref _addr_r.val;

    if (m == r.target) {
        return (r.list, error.As(null!)!);
    }
    return r.Reqs.Required(m);

}

} // end mvs_package
