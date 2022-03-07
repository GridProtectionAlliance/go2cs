// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package execabs is a drop-in replacement for os/exec
// that requires PATH lookups to find absolute paths.
// That is, execabs.Command("cmd") runs the same PATH lookup
// as exec.Command("cmd"), but if the result is a path
// which is relative, the Run and Start methods will report
// an error instead of running the executable.
// package execabs -- go2cs converted at 2022 March 06 22:41:18 UTC
// import "internal/execabs" ==> using execabs = go.@internal.execabs_package
// Original source: C:\Program Files\Go\src\internal\execabs\execabs.go
using context = go.context_package;
using fmt = go.fmt_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class execabs_package {

public static var ErrNotFound = exec.ErrNotFound;

public partial struct Cmd { // : exec.Cmd
}
public partial struct Error { // : exec.Error
}
public partial struct ExitError { // : exec.ExitError
}
private static error relError(@string file, @string path) {
    return error.As(fmt.Errorf("%s resolves to executable relative to current directory (.%c%s)", file, filepath.Separator, path))!;
}

public static (@string, error) LookPath(@string file) {
    @string _p0 = default;
    error _p0 = default!;

    var (path, err) = exec.LookPath(file);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (filepath.Base(file) == file && !filepath.IsAbs(path)) {
        return ("", error.As(relError(file, path))!);
    }
    return (path, error.As(null!)!);

}

private static void fixCmd(@string name, ptr<exec.Cmd> _addr_cmd) {
    ref exec.Cmd cmd = ref _addr_cmd.val;

    if (filepath.Base(name) == name && !filepath.IsAbs(cmd.Path)) { 
        // exec.Command was called with a bare binary name and
        // exec.LookPath returned a path which is not absolute.
        // Set cmd.lookPathErr and clear cmd.Path so that it
        // cannot be run.
        var lookPathErr = (error.val)(@unsafe.Pointer(reflect.ValueOf(cmd).Elem().FieldByName("lookPathErr").Addr().Pointer()));
        if (lookPathErr == null.val) {
            lookPathErr.val = relError(name, cmd.Path);
        }
        cmd.Path = "";

    }
}

public static ptr<exec.Cmd> CommandContext(context.Context ctx, @string name, params @string[] arg) {
    arg = arg.Clone();

    var cmd = exec.CommandContext(ctx, name, arg);
    fixCmd(name, _addr_cmd);
    return _addr_cmd!;
}

public static ptr<exec.Cmd> Command(@string name, params @string[] arg) {
    arg = arg.Clone();

    var cmd = exec.Command(name, arg);
    fixCmd(name, _addr_cmd);
    return _addr_cmd!;
}

} // end execabs_package
