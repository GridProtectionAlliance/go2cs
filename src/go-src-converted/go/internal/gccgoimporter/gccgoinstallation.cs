// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using bufio = bufio_package;
using types = go.types_package;
using os = os_package;
using exec = os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using go;
using os;
using path;
using ꓸꓸꓸ@string = Span<@string>;

partial class gccgoimporter_package {

// Information about a specific installation of gccgo.
[GoType] partial struct GccgoInstallation {
    // Version of gcc (e.g. 4.8.0).
    public @string GccVersion;
    // Target triple (e.g. x86_64-unknown-linux-gnu).
    public @string TargetTriple;
    // Built-in library paths used by this installation.
    public slice<@string> LibPaths;
}

// Ask the driver at the given path for information for this GccgoInstallation.
// The given arguments are passed directly to the call of the driver.
[GoRecv] public static error /*err*/ InitFromDriver(this ref GccgoInstallation inst, @string gccgoPath, params ꓸꓸꓸ@string argsʗp) {
    error err = default!;
    var args = argsʗp.slice();

    var argv = append(new @string[]{"-###", "-S", "-x", "go", "-"}.slice(), args.ꓸꓸꓸ);
    var cmd = exec.Command(gccgoPath, argv.ꓸꓸꓸ);
    (stderr, err) = cmd.StderrPipe();
    if (err != default!) {
        return err;
    }
    err = cmd.Start();
    if (err != default!) {
        return err;
    }
    var scanner = bufio.NewScanner(stderr);
    while (scanner.Scan()) {
        @string line = scanner.Text();
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, "Target: "u8): {
            inst.TargetTriple = line[8..];
            break;
        }
        case {} when line[0] is (rune)' ': {
            var argsΔ2 = strings.Fields(line);
            foreach (var (_, arg) in argsΔ2[1..]) {
                if (strings.HasPrefix(arg, "-L"u8)) {
                    inst.LibPaths = append(inst.LibPaths, arg[2..]);
                }
            }
            break;
        }}

    }
    argv = append(new @string[]{"-dumpversion"}.slice(), args.ꓸꓸꓸ);
    (stdout, err) = exec.Command(gccgoPath, argv.ꓸꓸꓸ).Output();
    if (err != default!) {
        return err;
    }
    inst.GccVersion = strings.TrimSpace(((@string)stdout));
    return err;
}

// Return the list of export search paths for this GccgoInstallation.
[GoRecv] public static slice<@string> /*paths*/ SearchPaths(this ref GccgoInstallation inst) {
    slice<@string> paths = default!;

    foreach (var (_, lpath) in inst.LibPaths) {
        @string spath = filepath.Join(lpath, "go", inst.GccVersion);
        (fi, err) = os.Stat(spath);
        if (err != default! || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);
        spath = filepath.Join(spath, inst.TargetTriple);
        (fi, err) = os.Stat(spath);
        if (err != default! || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);
    }
    paths = append(paths, inst.LibPaths.ꓸꓸꓸ);
    return paths;
}

// Return an importer that searches incpaths followed by the gcc installation's
// built-in search paths and the current directory.
[GoRecv] public static Importer GetImporter(this ref GccgoInstallation inst, slice<@string> incpaths, types.Package>InitData initmap) {
    return GetImporter(append(append(incpaths, inst.SearchPaths().ꓸꓸꓸ), "."u8), initmap);
}

} // end gccgoimporter_package
