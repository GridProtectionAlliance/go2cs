// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 09 05:46:58 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\search.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using cfg = go.cmd.go.@internal.cfg_package;
using imports = go.cmd.go.@internal.imports_package;
using search = go.cmd.go.@internal.search_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        private partial struct stdFilter // : sbyte
        {
        }

        private static readonly var omitStd = stdFilter(iota);
        private static readonly var includeStd = 0;


        // matchPackages is like m.MatchPackages, but uses a local variable (rather than
        // a global) for tags, can include or exclude packages in the standard library,
        // and is restricted to the given list of modules.
        private static void matchPackages(ptr<search.Match> _addr_m, map<@string, bool> tags, stdFilter filter, slice<module.Version> modules)
        {
            ref search.Match m = ref _addr_m.val;

            m.Pkgs = new slice<@string>(new @string[] {  });

            Func<@string, bool> isMatch = _p0 => true;
            Func<@string, bool> treeCanMatch = _p0 => true;
            if (!m.IsMeta())
            {
                isMatch = search.MatchPattern(m.Pattern());
                treeCanMatch = search.TreeCanMatchPattern(m.Pattern());
            }

            map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"builtin":true,};
            if (!cfg.BuildContext.CgoEnabled)
            {
                have["runtime/cgo"] = true; // ignore during walk
            }

            private partial struct pruning // : sbyte
            {
            }
            const var pruneVendor = pruning(1L << (int)(iota));
            const var pruneGoMod = 0;


            Action<@string, @string, pruning> walkPkgs = (root, importPathRoot, prune) =>
            {
                root = filepath.Clean(root);
                var err = filepath.Walk(root, (path, fi, err) =>
                {
                    if (err != null)
                    {
                        m.AddError(err);
                        return null;
                    }

                    var want = true;
                    @string elem = ""; 

                    // Don't use GOROOT/src but do walk down into it.
                    if (path == root)
                    {
                        if (importPathRoot == "")
                        {
                            return null;
                        }

                    }
                    else
                    { 
                        // Avoid .foo, _foo, and testdata subdirectory trees.
                        _, elem = filepath.Split(path);
                        if (strings.HasPrefix(elem, ".") || strings.HasPrefix(elem, "_") || elem == "testdata")
                        {
                            want = false;
                        }

                    }

                    var name = importPathRoot + filepath.ToSlash(path[len(root)..]);
                    if (importPathRoot == "")
                    {
                        name = name[1L..]; // cut leading slash
                    }

                    if (!treeCanMatch(name))
                    {
                        want = false;
                    }

                    if (!fi.IsDir())
                    {
                        if (fi.Mode() & os.ModeSymlink != 0L && want)
                        {
                            {
                                var err__prev3 = err;

                                var (target, err) = os.Stat(path);

                                if (err == null && target.IsDir())
                                {
                                    fmt.Fprintf(os.Stderr, "warning: ignoring symlink %s\n", path);
                                }

                                err = err__prev3;

                            }

                        }

                        return null;

                    }

                    if (!want)
                    {
                        return filepath.SkipDir;
                    } 
                    // Stop at module boundaries.
                    if ((prune & pruneGoMod != 0L) && path != root)
                    {
                        {
                            var err__prev2 = err;

                            var (fi, err) = os.Stat(filepath.Join(path, "go.mod"));

                            if (err == null && !fi.IsDir())
                            {
                                return filepath.SkipDir;
                            }

                            err = err__prev2;

                        }

                    }

                    if (!have[name])
                    {
                        have[name] = true;
                        if (isMatch(name))
                        {
                            {
                                var err__prev3 = err;

                                var (_, _, err) = scanDir(path, tags);

                                if (err != imports.ErrNoGo)
                                {
                                    m.Pkgs = append(m.Pkgs, name);
                                }

                                err = err__prev3;

                            }

                        }

                    }

                    if (elem == "vendor" && (prune & pruneVendor != 0L))
                    {
                        return filepath.SkipDir;
                    }

                    return null;

                });
                if (err != null)
                {
                    m.AddError(err);
                }

            }
;

            if (filter == includeStd)
            {
                walkPkgs(cfg.GOROOTsrc, "", pruneGoMod);
                if (treeCanMatch("cmd"))
                {
                    walkPkgs(filepath.Join(cfg.GOROOTsrc, "cmd"), "cmd", pruneGoMod);
                }

            }

            if (cfg.BuildMod == "vendor")
            {
                if (HasModRoot())
                {
                    walkPkgs(ModRoot(), targetPrefix, pruneGoMod | pruneVendor);
                    walkPkgs(filepath.Join(ModRoot(), "vendor"), "", pruneVendor);
                }

                return ;

            }

            foreach (var (_, mod) in modules)
            {
                if (!treeCanMatch(mod.Path))
                {
                    continue;
                }

                @string root = default;                @string modPrefix = default;
                bool isLocal = default;
                if (mod == Target)
                {
                    if (!HasModRoot())
                    {
                        continue; // If there is no main module, we can't search in it.
                    }

                    root = ModRoot();
                    modPrefix = targetPrefix;
                    isLocal = true;

                }
                else
                {
                    err = default!;
                    root, isLocal, err = fetch(mod);
                    if (err != null)
                    {
                        m.AddError(err);
                        continue;
                    }

                    modPrefix = mod.Path;

                }

                var prune = pruneVendor;
                if (isLocal)
                {
                    prune |= pruneGoMod;
                }

                walkPkgs(root, modPrefix, prune);

            }
            return ;

        }
    }
}}}}
