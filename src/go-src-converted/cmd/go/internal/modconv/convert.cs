// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2020 October 09 05:46:39 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Go\src\cmd\go\internal\modconv\convert.go
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using par = go.cmd.go.@internal.par_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modconv_package
    {
        // ConvertLegacyConfig converts legacy config to modfile.
        // The file argument is slash-delimited.
        public static error ConvertLegacyConfig(ptr<modfile.File> _addr_f, @string file, slice<byte> data)
        {
            ref modfile.File f = ref _addr_f.val;

            var i = strings.LastIndex(file, "/");
            long j = -2L;
            if (i >= 0L)
            {
                j = strings.LastIndex(file[..i], "/");
            }
            var convert = Converters[file[i + 1L..]];
            if (convert == null && j != -2L)
            {
                convert = Converters[file[j + 1L..]];
            }
            if (convert == null)
            {
                return error.As(fmt.Errorf("unknown legacy config file %s", file))!;
            }
            var (mf, err) = convert(file, data);
            if (err != null)
            {
                return error.As(fmt.Errorf("parsing %s: %v", file, err))!;
            }
            par.Work work = default;            sync.Mutex mu = default;            var need = make_map<@string, @string>();            var replace = make_map<@string, ptr<modfile.Replace>>();

            {
                var r__prev1 = r;

                foreach (var (_, __r) in mf.Replace)
                {
                    r = __r;
                    replace[r.New.Path] = r;
                    replace[r.Old.Path] = r;
                }
                r = r__prev1;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in mf.Require)
                {
                    r = __r;
                    var m = r.Mod;
                    if (m.Path == "")
                    {
                        continue;
                    }
                    {
                        var re__prev1 = re;

                        var (re, ok) = replace[m.Path];

                        if (ok)
                        {
                            work.Add(re.New);
                            continue;
                        }
                        re = re__prev1;

                    }

                    work.Add(r.Mod);

                }
                r = r__prev1;
            }

            work.Do(10L, item =>
            {
                module.Version r = item._<module.Version>();
                var (repo, info, err) = modfetch.ImportRepoRev(r.Path, r.Version);
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go: converting %s: stat %s@%s: %v\n", @base.ShortPath(file), r.Path, r.Version, err);
                    return ;
                }
                mu.Lock();
                var path = repo.ModulePath(); 
                // Don't use semver.Max here; need to preserve +incompatible suffix.
                {
                    var (v, ok) = need[path];

                    if (!ok || semver.Compare(v, info.Version) < 0L)
                    {
                        need[path] = info.Version;
                    }
                }

                mu.Unlock();

            });

            slice<@string> paths = default;
            {
                var path__prev1 = path;

                foreach (var (__path) in need)
                {
                    path = __path;
                    paths = append(paths, path);
                }
                path = path__prev1;
            }

            sort.Strings(paths);
            {
                var path__prev1 = path;

                foreach (var (_, __path) in paths)
                {
                    path = __path;
                    {
                        var re__prev1 = re;

                        (re, ok) = replace[path];

                        if (ok)
                        {
                            var err = f.AddReplace(re.Old.Path, re.Old.Version, path, need[path]);
                            if (err != null)
                            {
                                return error.As(fmt.Errorf("add replace: %v", err))!;
                            }
                        }
                        re = re__prev1;

                    }

                    f.AddNewRequire(path, need[path], false);

                }
                path = path__prev1;
            }

            f.Cleanup();
            return error.As(null!)!;

        }
    }
}}}}
