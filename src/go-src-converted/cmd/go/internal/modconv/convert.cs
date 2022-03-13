// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:33 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\convert.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// ConvertLegacyConfig converts legacy config to modfile.
// The file argument is slash-delimited.

using System;
using System.Threading;
public static partial class modconv_package {

public static error ConvertLegacyConfig(ptr<modfile.File> _addr_f, @string file, slice<byte> data, Func<@string, @string, (module.Version, error)> queryPackage) => func((defer, _, _) => {
    ref modfile.File f = ref _addr_f.val;

    var i = strings.LastIndex(file, "/");
    nint j = -2;
    if (i >= 0) {
        j = strings.LastIndex(file[..(int)i], "/");
    }
    var convert = Converters[file[(int)i + 1..]];
    if (convert == null && j != -2) {
        convert = Converters[file[(int)j + 1..]];
    }
    if (convert == null) {
        return error.As(fmt.Errorf("unknown legacy config file %s", file))!;
    }
    var (mf, err) = convert(file, data);
    if (err != null) {
        return error.As(fmt.Errorf("parsing %s: %v", file, err))!;
    }
    var versions = make_slice<module.Version>(len(mf.Require));
    var replace = make_map<@string, ptr<modfile.Replace>>();

    {
        var r__prev1 = r;

        foreach (var (_, __r) in mf.Replace) {
            r = __r;
            replace[r.New.Path] = r;
            replace[r.Old.Path] = r;
        }
        r = r__prev1;
    }

    private partial struct token {
    }
    var sem = make_channel<token>(runtime.GOMAXPROCS(0));
    {
        var i__prev1 = i;
        var r__prev1 = r;

        foreach (var (__i, __r) in mf.Require) {
            i = __i;
            r = __r;
            var m = r.Mod;
            if (m.Path == "") {
                continue;
            }
            {
                var re__prev1 = re;

                var (re, ok) = replace[m.Path];

                if (ok) {
                    m = re.New;
                }
                re = re__prev1;

            }
            sem.Send(new token());
            go_(() => (i, m) => {
                defer(() => {
                    sem.Receive();
                }());
                var (version, err) = queryPackage(m.Path, m.Version);
                if (err != null) {
                    fmt.Fprintf(os.Stderr, "go: converting %s: stat %s@%s: %v\n", @base.ShortPath(file), m.Path, m.Version, err);
                    return ;
                }
                versions[i] = version;
            }(i, m));
        }
        i = i__prev1;
        r = r__prev1;
    }

    for (var n = cap(sem); n > 0; n--) {
        sem.Send(new token());
    }

    map need = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
    foreach (var (_, v) in versions) {
        if (v.Path == "") {
            continue;
        }
        {
            var (needv, ok) = need[v.Path];

            if (!ok || semver.Compare(needv, v.Version) < 0) {
                need[v.Path] = v.Version;
            }
        }
    }    var paths = make_slice<@string>(0, len(need));
    {
        var path__prev1 = path;

        foreach (var (__path) in need) {
            path = __path;
            paths = append(paths, path);
        }
        path = path__prev1;
    }

    sort.Strings(paths);
    {
        var path__prev1 = path;

        foreach (var (_, __path) in paths) {
            path = __path;
            {
                var re__prev1 = re;

                (re, ok) = replace[path];

                if (ok) {
                    var err = f.AddReplace(re.Old.Path, re.Old.Version, path, need[path]);
                    if (err != null) {
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
});

} // end modconv_package
