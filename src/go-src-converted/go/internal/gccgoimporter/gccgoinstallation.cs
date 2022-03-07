// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gccgoimporter -- go2cs converted at 2022 March 06 23:32:40 UTC
// import "go/internal/gccgoimporter" ==> using gccgoimporter = go.go.@internal.gccgoimporter_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\gccgoinstallation.go
using bufio = go.bufio_package;
using types = go.go.types_package;
using exec = go.@internal.execabs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

namespace go.go.@internal;

public static partial class gccgoimporter_package {

    // Information about a specific installation of gccgo.
public partial struct GccgoInstallation {
    public @string GccVersion; // Target triple (e.g. x86_64-unknown-linux-gnu).
    public @string TargetTriple; // Built-in library paths used by this installation.
    public slice<@string> LibPaths;
}

// Ask the driver at the given path for information for this GccgoInstallation.
// The given arguments are passed directly to the call of the driver.
private static error InitFromDriver(this ptr<GccgoInstallation> _addr_inst, @string gccgoPath, params @string[] args) {
    error err = default!;
    args = args.Clone();
    ref GccgoInstallation inst = ref _addr_inst.val;

    var argv = append(new slice<@string>(new @string[] { "-###", "-S", "-x", "go", "-" }), args);
    var cmd = exec.Command(gccgoPath, argv);
    var (stderr, err) = cmd.StderrPipe();
    if (err != null) {
        return ;
    }
    err = cmd.Start();
    if (err != null) {
        return ;
    }
    var scanner = bufio.NewScanner(stderr);
    while (scanner.Scan()) {
        var line = scanner.Text();

        if (strings.HasPrefix(line, "Target: ")) 
            inst.TargetTriple = line[(int)8..];
        else if (line[0] == ' ') 
            var args = strings.Fields(line);
            foreach (var (_, arg) in args[(int)1..]) {
                if (strings.HasPrefix(arg, "-L")) {
                    inst.LibPaths = append(inst.LibPaths, arg[(int)2..]);
                }
            }
        
    }

    argv = append(new slice<@string>(new @string[] { "-dumpversion" }), args);
    var (stdout, err) = exec.Command(gccgoPath, argv).Output();
    if (err != null) {
        return ;
    }
    inst.GccVersion = strings.TrimSpace(string(stdout));

    return ;

}

// Return the list of export search paths for this GccgoInstallation.
private static slice<@string> SearchPaths(this ptr<GccgoInstallation> _addr_inst) {
    slice<@string> paths = default;
    ref GccgoInstallation inst = ref _addr_inst.val;

    foreach (var (_, lpath) in inst.LibPaths) {
        var spath = filepath.Join(lpath, "go", inst.GccVersion);
        var (fi, err) = os.Stat(spath);
        if (err != null || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);

        spath = filepath.Join(spath, inst.TargetTriple);
        fi, err = os.Stat(spath);
        if (err != null || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);

    }    paths = append(paths, inst.LibPaths);

    return ;

}

// Return an importer that searches incpaths followed by the gcc installation's
// built-in search paths and the current directory.
private static Importer GetImporter(this ptr<GccgoInstallation> _addr_inst, slice<@string> incpaths, map<ptr<types.Package>, InitData> initmap) {
    ref GccgoInstallation inst = ref _addr_inst.val;

    return GetImporter(append(append(incpaths, inst.SearchPaths()), "."), initmap);
}

} // end gccgoimporter_package
