// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package term -- go2cs converted at 2022 March 06 23:31:04 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term_plan9.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;

using plan9 = go.golang.org.x.sys.plan9_package;

namespace go.cmd.vendor.golang.org.x;

public static partial class term_package {

private partial struct state {
}

private static bool isTerminal(nint fd) {
    var (path, err) = plan9.Fd2path(fd);
    if (err != null) {
        return false;
    }
    return path == "/dev/cons" || path == "/mnt/term/dev/cons";

}

private static (ptr<State>, error) makeRaw(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(fmt.Errorf("terminal: MakeRaw not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
}

private static (ptr<State>, error) getState(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(fmt.Errorf("terminal: GetState not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
}

private static error restore(nint fd, ptr<State> _addr_state) {
    ref State state = ref _addr_state.val;

    return error.As(fmt.Errorf("terminal: Restore not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!;
}

private static (nint, nint, error) getSize(nint fd) {
    nint width = default;
    nint height = default;
    error err = default!;

    return (0, 0, error.As(fmt.Errorf("terminal: GetSize not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
}

private static (slice<byte>, error) readPassword(nint fd) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return (null, error.As(fmt.Errorf("terminal: ReadPassword not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
}

} // end term_package
