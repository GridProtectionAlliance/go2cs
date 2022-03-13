// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mvs -- go2cs converted at 2022 March 13 06:31:23 UTC
// import "cmd/go/internal/mvs" ==> using mvs = go.cmd.go.@internal.mvs_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\mvs\graph.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;

using module = golang.org.x.mod.module_package;


// Graph implements an incremental version of the MVS algorithm, with the
// requirements pushed by the caller instead of pulled by the MVS traversal.

using System;
public static partial class mvs_package {

public partial struct Graph {
    public Func<@string, @string, nint> cmp;
    public slice<module.Version> roots;
    public map<module.Version, slice<module.Version>> required;
    public map<module.Version, bool> isRoot; // contains true for roots and false for reachable non-roots
    public map<@string, @string> selected; // path → version
}

// NewGraph returns an incremental MVS graph containing only a set of root
// dependencies and using the given max function for version strings.
//
// The caller must ensure that the root slice is not modified while the Graph
// may be in use.
public static ptr<Graph> NewGraph(Func<@string, @string, nint> cmp, slice<module.Version> roots) {
    ptr<Graph> g = addr(new Graph(cmp:cmp,roots:roots[:len(roots):len(roots)],required:make(map[module.Version][]module.Version),isRoot:make(map[module.Version]bool),selected:make(map[string]string),));

    foreach (var (_, m) in roots) {
        g.isRoot[m] = true;
        if (g.cmp(g.Selected(m.Path), m.Version) < 0) {
            g.selected[m.Path] = m.Version;
        }
    }    return _addr_g!;
}

// Require adds the information that module m requires all modules in reqs.
// The reqs slice must not be modified after it is passed to Require.
//
// m must be reachable by some existing chain of requirements from g's target,
// and Require must not have been called for it already.
//
// If any of the modules in reqs has the same path as g's target,
// the target must have higher precedence than the version in req.
private static void Require(this ptr<Graph> _addr_g, module.Version m, slice<module.Version> reqs) => func((_, panic, _) => {
    ref Graph g = ref _addr_g.val;
 
    // To help catch disconnected-graph bugs, enforce that all required versions
    // are actually reachable from the roots (and therefore should affect the
    // selected versions of the modules they name).
    {
        var (_, reachable) = g.isRoot[m];

        if (!reachable) {
            panic(fmt.Sprintf("%v is not reachable from any root", m));
        }
    } 

    // Truncate reqs to its capacity to avoid aliasing bugs if it is later
    // returned from RequiredBy and appended to.
    reqs = reqs.slice(-1, len(reqs), len(reqs));

    {
        var (_, dup) = g.required[m];

        if (dup) {
            panic(fmt.Sprintf("requirements of %v have already been set", m));
        }
    }
    g.required[m] = reqs;

    foreach (var (_, dep) in reqs) { 
        // Mark dep reachable, regardless of whether it is selected.
        {
            var (_, ok) = g.isRoot[dep];

            if (!ok) {
                g.isRoot[dep] = false;
            }

        }

        if (g.cmp(g.Selected(dep.Path), dep.Version) < 0) {
            g.selected[dep.Path] = dep.Version;
        }
    }
});

// RequiredBy returns the slice of requirements passed to Require for m, if any,
// with its capacity reduced to its length.
// If Require has not been called for m, RequiredBy(m) returns ok=false.
//
// The caller must not modify the returned slice, but may safely append to it
// and may rely on it not to be modified.
private static (slice<module.Version>, bool) RequiredBy(this ptr<Graph> _addr_g, module.Version m) {
    slice<module.Version> reqs = default;
    bool ok = default;
    ref Graph g = ref _addr_g.val;

    reqs, ok = g.required[m];
    return (reqs, ok);
}

// Selected returns the selected version of the given module path.
//
// If no version is selected, Selected returns version "none".
private static @string Selected(this ptr<Graph> _addr_g, @string path) {
    @string version = default;
    ref Graph g = ref _addr_g.val;

    var (v, ok) = g.selected[path];
    if (!ok) {
        return "none";
    }
    return v;
}

// BuildList returns the selected versions of all modules present in the Graph,
// beginning with the selected versions of each module path in the roots of g.
//
// The order of the remaining elements in the list is deterministic
// but arbitrary.
private static slice<module.Version> BuildList(this ptr<Graph> _addr_g) {
    ref Graph g = ref _addr_g.val;

    var seenRoot = make_map<@string, bool>(len(g.roots));

    slice<module.Version> list = default;
    foreach (var (_, r) in g.roots) {
        if (seenRoot[r.Path]) { 
            // Multiple copies of the same root, with the same or different versions,
            // are a bit of a degenerate case: we will take the transitive
            // requirements of both roots into account, but only the higher one can
            // possibly be selected. However — especially given that we need the
            // seenRoot map for later anyway — it is simpler to support this
            // degenerate case than to forbid it.
            continue;
        }
        {
            var v = g.Selected(r.Path);

            if (v != "none") {
                list = append(list, new module.Version(Path:r.Path,Version:v));
            }

        }
        seenRoot[r.Path] = true;
    }    var uniqueRoots = list;

    foreach (var (path, version) in g.selected) {
        if (!seenRoot[path]) {
            list = append(list, new module.Version(Path:path,Version:version));
        }
    }    module.Sort(list[(int)len(uniqueRoots)..]);

    return list;
}

// WalkBreadthFirst invokes f once, in breadth-first order, for each module
// version other than "none" that appears in the graph, regardless of whether
// that version is selected.
private static void WalkBreadthFirst(this ptr<Graph> _addr_g, Action<module.Version> f) {
    ref Graph g = ref _addr_g.val;

    slice<module.Version> queue = default;
    var enqueued = make_map<module.Version, bool>();
    {
        var m__prev1 = m;

        foreach (var (_, __m) in g.roots) {
            m = __m;
            if (m.Version != "none") {
                queue = append(queue, m);
                enqueued[m] = true;
            }
        }
        m = m__prev1;
    }

    while (len(queue) > 0) {
        var m = queue[0];
        queue = queue[(int)1..];

        f(m);

        var (reqs, _) = g.RequiredBy(m);
        foreach (var (_, r) in reqs) {
            if (!enqueued[r] && r.Version != "none") {
                queue = append(queue, r);
                enqueued[r] = true;
            }
        }
    }
}

// FindPath reports a shortest requirement path starting at one of the roots of
// the graph and ending at a module version m for which f(m) returns true, or
// nil if no such path exists.
private static slice<module.Version> FindPath(this ptr<Graph> _addr_g, Func<module.Version, bool> f) {
    ref Graph g = ref _addr_g.val;
 
    // firstRequires[a] = b means that in a breadth-first traversal of the
    // requirement graph, the module version a was first required by b.
    var firstRequires = make_map<module.Version, module.Version>();

    var queue = g.roots;
    {
        var m__prev1 = m;

        foreach (var (_, __m) in g.roots) {
            m = __m;
            firstRequires[m] = new module.Version();
        }
        m = m__prev1;
    }

    while (len(queue) > 0) {
        var m = queue[0];
        queue = queue[(int)1..];

        if (f(m)) { 
            // Construct the path reversed (because we're starting from the far
            // endpoint), then reverse it.
            module.Version path = new slice<module.Version>(new module.Version[] { m });
            while (true) {
                m = firstRequires[m];
                if (m.Path == "") {
                    break;
                }
                path = append(path, m);
            }


            nint i = 0;
            var j = len(path) - 1;
            while (i < j) {
                (path[i], path[j]) = (path[j], path[i]);                i++;
                j--;
            }


            return path;
        }
        var (reqs, _) = g.RequiredBy(m);
        foreach (var (_, r) in reqs) {
            {
                var (_, seen) = firstRequires[r];

                if (!seen) {
                    queue = append(queue, r);
                    firstRequires[r] = m;
                }

            }
        }
    }

    return null;
}

} // end mvs_package
