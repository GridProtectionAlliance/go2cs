// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mvs implements Minimal Version Selection.
// See https://research.swtch.com/vgo-mvs.
// package mvs -- go2cs converted at 2020 October 08 04:34:06 UTC
// import "cmd/go/internal/mvs" ==> using mvs = go.cmd.go.@internal.mvs_package
// Original source: C:\Go\src\cmd\go\internal\mvs\mvs.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;

using par = go.cmd.go.@internal.par_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class mvs_package
    {
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
        public partial interface Reqs
        {
            (module.Version, error) Required(module.Version m); // Max returns the maximum of v1 and v2 (it returns either v1 or v2).
//
// For all versions v, Max(v, "none") must be v,
// and for the target passed as the first argument to MVS functions,
// Max(target, v) must be target.
//
// Note that v1 < v2 can be written Max(v1, v2) != v1
// and similarly v1 <= v2 can be written Max(v1, v2) == v2.
            (module.Version, error) Max(@string v1, @string v2); // Upgrade returns the upgraded version of m,
// for use during an UpgradeAll operation.
// If m should be kept as is, Upgrade returns m.
// If m is not yet used in the build, then m.Version will be "none".
// More typically, m.Version will be the version required
// by some other module in the build.
//
// If no module version is available for the given path,
// Upgrade returns a non-nil error.
// TODO(rsc): Upgrade must be able to return errors,
// but should "no latest version" just return m instead?
            (module.Version, error) Upgrade(module.Version m); // Previous returns the version of m.Path immediately prior to m.Version,
// or "none" if no such version is known.
            (module.Version, error) Previous(module.Version m);
        }

        // BuildListError decorates an error that occurred gathering requirements
        // while constructing a build list. BuildListError prints the chain
        // of requirements to the module where the error occurred.
        public partial struct BuildListError
        {
            public error Err;
            public slice<buildListErrorElem> stack;
        }

        private partial struct buildListErrorElem
        {
            public module.Version m; // nextReason is the reason this module depends on the next module in the
// stack. Typically either "requires", or "upgraded to".
            public @string nextReason;
        }

        // Module returns the module where the error occurred. If the module stack
        // is empty, this returns a zero value.
        private static module.Version Module(this ptr<BuildListError> _addr_e)
        {
            ref BuildListError e = ref _addr_e.val;

            if (len(e.stack) == 0L)
            {
                return new module.Version();
            }

            return e.stack[0L].m;

        }

        private static @string Error(this ptr<BuildListError> _addr_e)
        {
            ref BuildListError e = ref _addr_e.val;

            ptr<strings.Builder> b = addr(new strings.Builder());
            var stack = e.stack; 

            // Don't print modules at the beginning of the chain without a
            // version. These always seem to be the main module or a
            // synthetic module ("target@").
            while (len(stack) > 0L && stack[len(stack) - 1L].m.Version == "")
            {
                stack = stack[..len(stack) - 1L];
            }


            for (var i = len(stack) - 1L; i >= 1L; i--)
            {
                fmt.Fprintf(b, "%s@%s %s\n\t", stack[i].m.Path, stack[i].m.Version, stack[i].nextReason);
            }

            if (len(stack) == 0L)
            {
                b.WriteString(e.Err.Error());
            }
            else
            { 
                // Ensure that the final module path and version are included as part of the
                // error message.
                {
                    ptr<module.ModuleError> (_, ok) = e.Err._<ptr<module.ModuleError>>();

                    if (ok)
                    {
                        fmt.Fprintf(b, "%v", e.Err);
                    }
                    else
                    {
                        fmt.Fprintf(b, "%v", module.VersionError(stack[0L].m, e.Err));
                    }

                }

            }

            return b.String();

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
        public static (slice<module.Version>, error) BuildList(module.Version target, Reqs reqs)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;

            return buildList(target, reqs, null);
        }

        private static (slice<module.Version>, error) buildList(module.Version target, Reqs reqs, Func<module.Version, (module.Version, error)> upgrade) => func((_, panic, __) =>
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
 
            // Explore work graph in parallel in case reqs.Required
            // does high-latency network operations.
            private partial struct modGraphNode
            {
                public module.Version m;
                public slice<module.Version> required;
                public module.Version upgrade;
                public error err;
            }
            sync.Mutex mu = default;            map modGraph = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, ptr<modGraphNode>>{};            map min = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};            ref int haveErr = ref heap(out ptr<int> _addr_haveErr);
            Action<ptr<modGraphNode>, error> setErr = (n, err) =>
            {
                n.err = err;
                atomic.StoreInt32(_addr_haveErr, 1L);
            }
;

            par.Work work = default;
            work.Add(target);
            work.Do(10L, item =>
            {
                module.Version m = item._<module.Version>();

                ptr<modGraphNode> node = addr(new modGraphNode(m:m));
                mu.Lock();
                modGraph[m] = node;
                {
                    var v__prev1 = v;

                    var (v, ok) = min[m.Path];

                    if (!ok || reqs.Max(v, m.Version) != v)
                    {
                        min[m.Path] = m.Version;
                    }

                    v = v__prev1;

                }

                mu.Unlock();

                var (required, err) = reqs.Required(m);
                if (err != null)
                {
                    setErr(node, err);
                    return ;
                }

                node.required = required;
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in node.required)
                    {
                        r = __r;
                        work.Add(r);
                    }

                    r = r__prev1;
                }

                if (upgrade != null)
                {
                    var (u, err) = upgrade(m);
                    if (err != null)
                    {
                        setErr(node, err);
                        return ;
                    }

                    if (u != m)
                    {
                        node.upgrade = u;
                        work.Add(u);
                    }

                }

            }); 

            // If there was an error, find the shortest path from the target to the
            // node where the error occurred so we can report a useful error message.
            if (haveErr != 0L)
            { 
                // neededBy[a] = b means a was added to the module graph by b.
                var neededBy = make_map<ptr<modGraphNode>, ptr<modGraphNode>>();
                var q = make_slice<ptr<modGraphNode>>(0L, len(modGraph));
                q = append(q, modGraph[target]);
                while (len(q) > 0L)
                {
                    node = q[0L];
                    q = q[1L..];

                    if (node.err != null)
                    {
                        ptr<BuildListError> err = addr(new BuildListError(Err:node.err,stack:[]buildListErrorElem{{m:node.m}},));
                        {
                            var n__prev2 = n;

                            var n = neededBy[node];
                            var prev = node;

                            while (n != null)
                            {
                                @string reason = "requires";
                                if (n.upgrade == prev.m)
                                {
                                    reason = "updating to";
                                n = neededBy[n];
                            prev = n;
                                }

                                err.stack = append(err.stack, new buildListErrorElem(m:n.m,nextReason:reason));

                            }


                            n = n__prev2;
                        }
                        return (null, error.As(err)!);

                    }

                    var neighbors = node.required;
                    if (node.upgrade.Path != "")
                    {
                        neighbors = append(neighbors, node.upgrade);
                    }

                    foreach (var (_, neighbor) in neighbors)
                    {
                        var nn = modGraph[neighbor];
                        if (neededBy[nn] != null)
                        {
                            continue;
                        }

                        neededBy[nn] = node;
                        q = append(q, nn);

                    }

                }


            } 

            // The final list is the minimum version of each module found in the graph.
            {
                var v__prev1 = v;

                var v = min[target.Path];

                if (v != target.Version)
                { 
                    // target.Version will be "" for modload, the main client of MVS.
                    // "" denotes the main module, which has no version. However, MVS treats
                    // version strings as opaque, so "" is not a special value here.
                    // See golang.org/issue/31491, golang.org/issue/29773.
                    panic(fmt.Sprintf("mistake: chose version %q instead of target %+v", v, target)); // TODO: Don't panic.
                }

                v = v__prev1;

            }


            module.Version list = new slice<module.Version>(new module.Version[] { target });
            foreach (var (path, vers) in min)
            {
                if (path != target.Path)
                {
                    list = append(list, new module.Version(Path:path,Version:vers));
                }

                n = modGraph[new module.Version(Path:path,Version:vers)];
                var required = n.required;
                {
                    var r__prev2 = r;

                    foreach (var (_, __r) in required)
                    {
                        r = __r;
                        v = min[r.Path];
                        if (r.Path != target.Path && reqs.Max(v, r.Version) != v)
                        {
                            panic(fmt.Sprintf("mistake: version %q does not satisfy requirement %+v", v, r)); // TODO: Don't panic.
                        }

                    }

                    r = r__prev2;
                }
            }
            var tail = list[1L..];
            sort.Slice(tail, (i, j) =>
            {
                return tail[i].Path < tail[j].Path;
            });
            return (list, error.As(null!)!);

        });

        // Req returns the minimal requirement list for the target module,
        // with the constraint that all module paths listed in base must
        // appear in the returned list.
        public static (slice<module.Version>, error) Req(module.Version target, slice<@string> @base, Reqs reqs)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;

            var (list, err) = BuildList(target, reqs);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Note: Not running in parallel because we assume
            // that list came from a previous operation that paged
            // in all the requirements, so there's no I/O to overlap now.

            // Compute postorder, cache requirements.
            slice<module.Version> postorder = default;
            map reqCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, slice<module.Version>>{};
            reqCache[target] = null;
            Func<module.Version, error> walk = default;
            walk = m =>
            {
                var (_, ok) = reqCache[m];
                if (ok)
                {
                    return null;
                }

                var (required, err) = reqs.Required(m);
                if (err != null)
                {
                    return err;
                }

                reqCache[m] = required;
                {
                    var m1__prev1 = m1;

                    foreach (var (_, __m1) in required)
                    {
                        m1 = __m1;
                        {
                            var err__prev1 = err;

                            var err = walk(m1);

                            if (err != null)
                            {
                                return err;
                            }

                            err = err__prev1;

                        }

                    }

                    m1 = m1__prev1;
                }

                postorder = append(postorder, m);
                return null;

            }
;
            {
                var m__prev1 = m;

                foreach (var (_, __m) in list)
                {
                    m = __m;
                    {
                        var err__prev1 = err;

                        err = walk(m);

                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                } 

                // Walk modules in reverse post-order, only adding those not implied already.

                m = m__prev1;
            }

            map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};
            walk = m =>
            {
                if (have[m])
                {
                    return null;
                }

                have[m] = true;
                {
                    var m1__prev1 = m1;

                    foreach (var (_, __m1) in reqCache[m])
                    {
                        m1 = __m1;
                        walk(m1);
                    }

                    m1 = m1__prev1;
                }

                return null;

            }
;
            map max = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
            {
                var m__prev1 = m;

                foreach (var (_, __m) in list)
                {
                    m = __m;
                    {
                        var (v, ok) = max[m.Path];

                        if (ok)
                        {
                            max[m.Path] = reqs.Max(m.Version, v);
                        }
                        else
                        {
                            max[m.Path] = m.Version;
                        }

                    }

                } 
                // First walk the base modules that must be listed.

                m = m__prev1;
            }

            slice<module.Version> min = default;
            foreach (var (_, path) in base)
            {
                module.Version m = new module.Version(Path:path,Version:max[path]);
                min = append(min, m);
                walk(m);
            } 
            // Now the reverse postorder to bring in anything else.
            for (var i = len(postorder) - 1L; i >= 0L; i--)
            {
                m = postorder[i];
                if (max[m.Path] != m.Version)
                { 
                    // Older version.
                    continue;

                }

                if (!have[m])
                {
                    min = append(min, m);
                    walk(m);
                }

            }

            sort.Slice(min, (i, j) =>
            {
                return min[i].Path < min[j].Path;
            });
            return (min, error.As(null!)!);

        }

        // UpgradeAll returns a build list for the target module
        // in which every module is upgraded to its latest version.
        public static (slice<module.Version>, error) UpgradeAll(module.Version target, Reqs reqs)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;

            return buildList(target, reqs, m =>
            {
                if (m.Path == target.Path)
                {
                    return (target, error.As(null!)!);
                }

                return reqs.Upgrade(m);

            });

        }

        // Upgrade returns a build list for the target module
        // in which the given additional modules are upgraded.
        public static (slice<module.Version>, error) Upgrade(module.Version target, Reqs reqs, params module.Version[] upgrade)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            upgrade = upgrade.Clone();

            var (list, err) = reqs.Required(target);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 
            // TODO: Maybe if an error is given,
            // rerun with BuildList(upgrade[0], reqs) etc
            // to find which ones are the buggy ones.
            list = append((slice<module.Version>)null, list);
            list = append(list, upgrade);
            return BuildList(target, addr(new override(target,list,reqs)));

        }

        // Downgrade returns a build list for the target module
        // in which the given additional modules are downgraded.
        //
        // The versions to be downgraded may be unreachable from reqs.Latest and
        // reqs.Previous, but the methods of reqs must otherwise handle such versions
        // correctly.
        public static (slice<module.Version>, error) Downgrade(module.Version target, Reqs reqs, params module.Version[] downgrade)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            downgrade = downgrade.Clone();

            var (list, err) = reqs.Required(target);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var max = make_map<@string, @string>();
            {
                var r__prev1 = r;

                foreach (var (_, __r) in list)
                {
                    r = __r;
                    max[r.Path] = r.Version;
                }

                r = r__prev1;
            }

            foreach (var (_, d) in downgrade)
            {
                {
                    var v__prev1 = v;

                    var (v, ok) = max[d.Path];

                    if (!ok || reqs.Max(v, d.Version) != d.Version)
                    {
                        max[d.Path] = d.Version;
                    }

                    v = v__prev1;

                }

            }
            var added = make_map<module.Version, bool>();            var rdeps = make_map<module.Version, slice<module.Version>>();            var excluded = make_map<module.Version, bool>();
            Action<module.Version> exclude = default;
            exclude = m =>
            {
                if (excluded[m])
                {
                    return ;
                }

                excluded[m] = true;
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in rdeps[m])
                    {
                        p = __p;
                        exclude(p);
                    }

                    p = p__prev1;
                }
            }
;
            Action<module.Version> add = default;
            add = m =>
            {
                if (added[m])
                {
                    return ;
                }

                added[m] = true;
                {
                    var v__prev1 = v;

                    (v, ok) = max[m.Path];

                    if (ok && reqs.Max(m.Version, v) != v)
                    {
                        exclude(m);
                        return ;
                    }

                    v = v__prev1;

                }

                (list, err) = reqs.Required(m);
                if (err != null)
                { 
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

                }

                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in list)
                    {
                        r = __r;
                        add(r);
                        if (excluded[r])
                        {
                            exclude(m);
                            return ;
                        }

                        rdeps[r] = append(rdeps[r], m);

                    }

                    r = r__prev1;
                }
            }
;

            slice<module.Version> @out = default;
            out = append(out, target);
List:

            {
                var r__prev1 = r;

                foreach (var (_, __r) in list)
                {
                    r = __r;
                    add(r);
                    while (excluded[r])
                    {
                        var (p, err) = reqs.Previous(r);
                        if (err != null)
                        { 
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

                            if (reqs.Max(v, r.Version) != v && reqs.Max(p.Version, v) != p.Version)
                            {
                                p.Version = v;
                            }

                            v = v__prev1;

                        }

                        if (p.Version == "none")
                        {
                            _continueList = true;
                            break;
                        }

                        add(p);
                        r = p;

                    }

                    out = append(out, r);

                }

                r = r__prev1;
            }
            return (out, error.As(null!)!);

        }

        private partial struct @override : Reqs
        {
            public module.Version target;
            public slice<module.Version> list;
            public Reqs Reqs;
        }

        private static (slice<module.Version>, error) Required(this ptr<override> _addr_r, module.Version m)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            ref override r = ref _addr_r.val;

            if (m == r.target)
            {
                return (r.list, error.As(null!)!);
            }

            return r.Reqs.Required(m);

        }
    }
}}}}
