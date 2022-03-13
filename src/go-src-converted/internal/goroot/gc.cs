// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc
// +build gc

// package goroot -- go2cs converted at 2022 March 13 05:52:27 UTC
// import "internal/goroot" ==> using goroot = go.@internal.goroot_package
// Original source: C:\Program Files\Go\src\internal\goroot\gc.go
namespace go.@internal;

using exec = @internal.execabs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;


// IsStandardPackage reports whether path is a standard package,
// given goroot and compiler.

public static partial class goroot_package {

public static bool IsStandardPackage(@string goroot, @string compiler, @string path) => func((_, panic, _) => {
    switch (compiler) {
        case "gc": 
            var dir = filepath.Join(goroot, "src", path);
            var (_, err) = os.Stat(dir);
            return err == null;
            break;
        case "gccgo": 
            return gccgoSearch.isStandard(path);
            break;
        default: 
            panic("unknown compiler " + compiler);
            break;
    }
});

// gccgoSearch holds the gccgo search directories.
private partial struct gccgoDirs {
    public sync.Once once;
    public slice<@string> dirs;
}

// gccgoSearch is used to check whether a gccgo package exists in the
// standard library.
private static gccgoDirs gccgoSearch = default;

// init finds the gccgo search directories. If this fails it leaves dirs == nil.
private static void init(this ptr<gccgoDirs> _addr_gd) {
    ref gccgoDirs gd = ref _addr_gd.val;

    var gccgo = os.Getenv("GCCGO");
    if (gccgo == "") {
        gccgo = "gccgo";
    }
    var (bin, err) = exec.LookPath(gccgo);
    if (err != null) {
        return ;
    }
    var (allDirs, err) = exec.Command(bin, "-print-search-dirs").Output();
    if (err != null) {
        return ;
    }
    var (versionB, err) = exec.Command(bin, "-dumpversion").Output();
    if (err != null) {
        return ;
    }
    var version = strings.TrimSpace(string(versionB));
    var (machineB, err) = exec.Command(bin, "-dumpmachine").Output();
    if (err != null) {
        return ;
    }
    var machine = strings.TrimSpace(string(machineB));

    var dirsEntries = strings.Split(string(allDirs), "\n");
    const @string prefix = "libraries: =";

    slice<@string> dirs = default;
    foreach (var (_, dirEntry) in dirsEntries) {
        if (strings.HasPrefix(dirEntry, prefix)) {
            dirs = filepath.SplitList(strings.TrimPrefix(dirEntry, prefix));
            break;
        }
    }    if (len(dirs) == 0) {
        return ;
    }
    slice<@string> lastDirs = default;
    foreach (var (_, dir) in dirs) {
        var goDir = filepath.Join(dir, "go", version);
        {
            var fi__prev1 = fi;

            var (fi, err) = os.Stat(goDir);

            if (err == null && fi.IsDir()) {
                gd.dirs = append(gd.dirs, goDir);
                goDir = filepath.Join(goDir, machine);
                fi, err = os.Stat(goDir);

                if (err == null && fi.IsDir()) {
                    gd.dirs = append(gd.dirs, goDir);
                }
            }

            fi = fi__prev1;

        }
        {
            var fi__prev1 = fi;

            (fi, err) = os.Stat(dir);

            if (err == null && fi.IsDir()) {
                lastDirs = append(lastDirs, dir);
            }

            fi = fi__prev1;

        }
    }    gd.dirs = append(gd.dirs, lastDirs);
}

// isStandard reports whether path is a standard library for gccgo.
private static bool isStandard(this ptr<gccgoDirs> _addr_gd, @string path) {
    ref gccgoDirs gd = ref _addr_gd.val;
 
    // Quick check: if the first path component has a '.', it's not
    // in the standard library. This skips most GOPATH directories.
    var i = strings.Index(path, "/");
    if (i < 0) {
        i = len(path);
    }
    if (strings.Contains(path[..(int)i], ".")) {
        return false;
    }
    if (path == "unsafe") { 
        // Special case.
        return true;
    }
    gd.once.Do(gd.init);
    if (gd.dirs == null) { 
        // We couldn't find the gccgo search directories.
        // Best guess, since the first component did not contain
        // '.', is that this is a standard library package.
        return true;
    }
    foreach (var (_, dir) in gd.dirs) {
        var full = filepath.Join(dir, path) + ".gox";
        {
            var (fi, err) = os.Stat(full);

            if (err == null && !fi.IsDir()) {
                return true;
            }

        }
    }    return false;
}

} // end goroot_package
