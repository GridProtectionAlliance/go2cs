// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package term -- go2cs converted at 2022 March 06 23:31:05 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term_windows.go
using os = go.os_package;

using windows = go.golang.org.x.sys.windows_package;

namespace go.cmd.vendor.golang.org.x;

public static partial class term_package {

private partial struct state {
    public uint mode;
}

private static bool isTerminal(nint fd) {
    ref uint st = ref heap(out ptr<uint> _addr_st);
    var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);
    return err == null;
}

private static (ptr<State>, error) makeRaw(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    ref uint st = ref heap(out ptr<uint> _addr_st);
    {
        var err__prev1 = err;

        var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    var raw = st & ~(windows.ENABLE_ECHO_INPUT | windows.ENABLE_PROCESSED_INPUT | windows.ENABLE_LINE_INPUT | windows.ENABLE_PROCESSED_OUTPUT);
    {
        var err__prev1 = err;

        err = windows.SetConsoleMode(windows.Handle(fd), raw);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    return (addr(new State(state{st})), error.As(null!)!);

}

private static (ptr<State>, error) getState(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    ref uint st = ref heap(out ptr<uint> _addr_st);
    {
        var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    return (addr(new State(state{st})), error.As(null!)!);

}

private static error restore(nint fd, ptr<State> _addr_state) {
    ref State state = ref _addr_state.val;

    return error.As(windows.SetConsoleMode(windows.Handle(fd), state.mode))!;
}

private static (nint, nint, error) getSize(nint fd) {
    nint width = default;
    nint height = default;
    error err = default!;

    ref windows.ConsoleScreenBufferInfo info = ref heap(out ptr<windows.ConsoleScreenBufferInfo> _addr_info);
    {
        var err = windows.GetConsoleScreenBufferInfo(windows.Handle(fd), _addr_info);

        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }

    return (int(info.Window.Right - info.Window.Left + 1), int(info.Window.Bottom - info.Window.Top + 1), error.As(null!)!);

}

private static (slice<byte>, error) readPassword(nint fd) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    ref uint st = ref heap(out ptr<uint> _addr_st);
    {
        var err__prev1 = err;

        var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var old = st;

    st &= (windows.ENABLE_ECHO_INPUT | windows.ENABLE_LINE_INPUT);
    st |= (windows.ENABLE_PROCESSED_OUTPUT | windows.ENABLE_PROCESSED_INPUT);
    {
        var err__prev1 = err;

        err = windows.SetConsoleMode(windows.Handle(fd), st);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }


    defer(windows.SetConsoleMode(windows.Handle(fd), old));

    ref windows.Handle h = ref heap(out ptr<windows.Handle> _addr_h);
    var (p, _) = windows.GetCurrentProcess();
    {
        var err__prev1 = err;

        err = windows.DuplicateHandle(p, windows.Handle(fd), p, _addr_h, 0, false, windows.DUPLICATE_SAME_ACCESS);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }


    var f = os.NewFile(uintptr(h), "stdin");
    defer(f.Close());
    return readPasswordLine(f);

});

} // end term_package
