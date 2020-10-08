// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 08 04:35:36 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\modfile.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        private static ptr<modfile.File> modFile;

        // A modFileIndex is an index of data corresponding to a modFile
        // at a specific point in time.
        private partial struct modFileIndex
        {
            public slice<byte> data;
            public bool dataNeedsFix; // true if fixVersion applied a change while parsing data
            public module.Version module;
            public @string goVersion;
            public map<module.Version, requireMeta> require;
            public map<module.Version, module.Version> replace;
            public map<module.Version, bool> exclude;
        }

        // index is the index of the go.mod file as of when it was last read or written.
        private static ptr<modFileIndex> index;

        private partial struct requireMeta
        {
            public bool indirect;
        }

        // Allowed reports whether module m is allowed (not excluded) by the main module's go.mod.
        public static bool Allowed(module.Version m)
        {
            return index == null || !index.exclude[m];
        }

        // Replacement returns the replacement for mod, if any, from go.mod.
        // If there is no replacement for mod, Replacement returns
        // a module.Version with Path == "".
        public static module.Version Replacement(module.Version mod)
        {
            if (index != null)
            {
                {
                    var r__prev2 = r;

                    var (r, ok) = index.replace[mod];

                    if (ok)
                    {
                        return r;
                    }

                    r = r__prev2;

                }

                {
                    var r__prev2 = r;

                    (r, ok) = index.replace[new module.Version(Path:mod.Path)];

                    if (ok)
                    {
                        return r;
                    }

                    r = r__prev2;

                }

            }

            return new module.Version();

        }

        // indexModFile rebuilds the index of modFile.
        // If modFile has been changed since it was first read,
        // modFile.Cleanup must be called before indexModFile.
        private static ptr<modFileIndex> indexModFile(slice<byte> data, ptr<modfile.File> _addr_modFile, bool needsFix)
        {
            ref modfile.File modFile = ref _addr_modFile.val;

            ptr<modFileIndex> i = @new<modFileIndex>();
            i.data = data;
            i.dataNeedsFix = needsFix;

            i.module = new module.Version();
            if (modFile.Module != null)
            {
                i.module = modFile.Module.Mod;
            }

            i.goVersion = "";
            if (modFile.Go != null)
            {
                i.goVersion = modFile.Go.Version;
            }

            i.require = make_map<module.Version, requireMeta>(len(modFile.Require));
            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Require)
                {
                    r = __r;
                    i.require[r.Mod] = new requireMeta(indirect:r.Indirect);
                }

                r = r__prev1;
            }

            i.replace = make_map<module.Version, module.Version>(len(modFile.Replace));
            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Replace)
                {
                    r = __r;
                    {
                        var (prev, dup) = i.replace[r.Old];

                        if (dup && prev != r.New)
                        {
                            @base.Fatalf("go: conflicting replacements for %v:\n\t%v\n\t%v", r.Old, prev, r.New);
                        }

                    }

                    i.replace[r.Old] = r.New;

                }

                r = r__prev1;
            }

            i.exclude = make_map<module.Version, bool>(len(modFile.Exclude));
            foreach (var (_, x) in modFile.Exclude)
            {
                i.exclude[x.Mod] = true;
            }
            return _addr_i!;

        }

        // modFileIsDirty reports whether the go.mod file differs meaningfully
        // from what was indexed.
        // If modFile has been changed (even cosmetically) since it was first read,
        // modFile.Cleanup must be called before modFileIsDirty.
        private static bool modFileIsDirty(this ptr<modFileIndex> _addr_i, ptr<modfile.File> _addr_modFile)
        {
            ref modFileIndex i = ref _addr_i.val;
            ref modfile.File modFile = ref _addr_modFile.val;

            if (i == null)
            {
                return modFile != null;
            }

            if (i.dataNeedsFix)
            {
                return true;
            }

            if (modFile.Module == null)
            {
                if (i.module != (new module.Version()))
                {
                    return true;
                }

            }
            else if (modFile.Module.Mod != i.module)
            {
                return true;
            }

            if (modFile.Go == null)
            {
                if (i.goVersion != "")
                {
                    return true;
                }

            }
            else if (modFile.Go.Version != i.goVersion)
            {
                if (i.goVersion == "" && cfg.BuildMod == "readonly")
                { 
                    // go.mod files did not always require a 'go' version, so do not error out
                    // if one is missing — we may be inside an older module in the module
                    // cache, and should bias toward providing useful behavior.
                }
                else
                {
                    return true;
                }

            }

            if (len(modFile.Require) != len(i.require) || len(modFile.Replace) != len(i.replace) || len(modFile.Exclude) != len(i.exclude))
            {
                return true;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Require)
                {
                    r = __r;
                    {
                        var (meta, ok) = i.require[r.Mod];

                        if (!ok)
                        {
                            return true;
                        }
                        else if (r.Indirect != meta.indirect)
                        {
                            if (cfg.BuildMod == "readonly")
                            { 
                                // The module's requirements are consistent; only the "// indirect"
                                // comments that are wrong. But those are only guaranteed to be accurate
                                // after a "go mod tidy" — it's a good idea to run those before
                                // committing a change, but it's certainly not mandatory.
                            }
                            else
                            {
                                return true;
                            }

                        }


                    }

                }

                r = r__prev1;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Replace)
                {
                    r = __r;
                    if (r.New != i.replace[r.Old])
                    {
                        return true;
                    }

                }

                r = r__prev1;
            }

            foreach (var (_, x) in modFile.Exclude)
            {
                if (!i.exclude[x.Mod])
                {
                    return true;
                }

            }
            return false;

        }
    }
}}}}
