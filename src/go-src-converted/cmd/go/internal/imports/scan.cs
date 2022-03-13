// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package imports -- go2cs converted at 2022 March 13 06:30:12 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\imports\scan.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;
using fs = io.fs_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;

using fsys = cmd.go.@internal.fsys_package;

public static partial class imports_package {

public static (slice<@string>, slice<@string>, error) ScanDir(@string dir, map<@string, bool> tags) {
    slice<@string> _p0 = default;
    slice<@string> _p0 = default;
    error _p0 = default!;

    var (infos, err) = fsys.ReadDir(dir);
    if (err != null) {
        return (null, null, error.As(err)!);
    }
    slice<@string> files = default;
    foreach (var (_, info) in infos) {
        var name = info.Name(); 

        // If the directory entry is a symlink, stat it to obtain the info for the
        // link target instead of the link itself.
        if (info.Mode() & fs.ModeSymlink != 0) {
            info, err = fsys.Stat(filepath.Join(dir, name));
            if (err != null) {
                continue; // Ignore broken symlinks.
            }
        }
        if (info.Mode().IsRegular() && !strings.HasPrefix(name, "_") && !strings.HasPrefix(name, ".") && strings.HasSuffix(name, ".go") && MatchFile(name, tags)) {
            files = append(files, filepath.Join(dir, name));
        }
    }    return scanFiles(files, tags, false);
}

public static (slice<@string>, slice<@string>, error) ScanFiles(slice<@string> files, map<@string, bool> tags) {
    slice<@string> _p0 = default;
    slice<@string> _p0 = default;
    error _p0 = default!;

    return scanFiles(files, tags, true);
}

private static (slice<@string>, slice<@string>, error) scanFiles(slice<@string> files, map<@string, bool> tags, bool explicitFiles) {
    slice<@string> _p0 = default;
    slice<@string> _p0 = default;
    error _p0 = default!;

    var imports = make_map<@string, bool>();
    var testImports = make_map<@string, bool>();
    nint numFiles = 0;
Files:
    foreach (var (_, name) in files) {
        var (r, err) = fsys.Open(name);
        if (err != null) {
            return (null, null, error.As(err)!);
        }
        ref slice<@string> list = ref heap(out ptr<slice<@string>> _addr_list);
        var (data, err) = ReadImports(r, false, _addr_list);
        r.Close();
        if (err != null) {
            return (null, null, error.As(fmt.Errorf("reading %s: %v", name, err))!);
        }
        foreach (var (_, path) in list) {
            if (path == "\"C\"" && !tags["cgo"] && !tags["*"]) {
                _continueFiles = true;
                break;
            }
        }        if (!explicitFiles && !ShouldBuild(data, tags)) {
            continue;
        }
        numFiles++;
        var m = imports;
        if (strings.HasSuffix(name, "_test.go")) {
            m = testImports;
        }
        foreach (var (_, p) in list) {
            var (q, err) = strconv.Unquote(p);
            if (err != null) {
                continue;
            }
            m[q] = true;
        }
    }    if (numFiles == 0) {
        return (null, null, error.As(ErrNoGo)!);
    }
    return (keys(imports), keys(testImports), error.As(null!)!);
}

public static var ErrNoGo = fmt.Errorf("no Go source files");

private static slice<@string> keys(map<@string, bool> m) {
    slice<@string> list = default;
    foreach (var (k) in m) {
        list = append(list, k);
    }    sort.Strings(list);
    return list;
}

} // end imports_package
