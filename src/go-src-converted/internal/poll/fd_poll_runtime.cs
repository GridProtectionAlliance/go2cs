// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows || wasip1
namespace go.@internal;

using errors = errors_package;
using sync = sync_package;
using Δsyscall = syscall_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class poll_package {

// runtimeNano returns the current value of the runtime clock in nanoseconds.
//
//go:linkname runtimeNano runtime.nanotime
internal static partial int64 runtimeNano();

internal static partial void runtime_pollServerInit();

internal static partial (uintptr, nint) runtime_pollOpen(uintptr fd);

internal static partial void runtime_pollClose(uintptr ctx);

internal static partial nint runtime_pollWait(uintptr ctx, nint mode);

internal static partial void runtime_pollWaitCanceled(uintptr ctx, nint mode);

internal static partial nint runtime_pollReset(uintptr ctx, nint mode);

internal static partial void runtime_pollSetDeadline(uintptr ctx, int64 d, nint mode);

internal static partial void runtime_pollUnblock(uintptr ctx);

internal static partial bool runtime_isPollServerDescriptor(uintptr fd);

[GoType] partial struct pollDesc {
    internal uintptr runtimeCtx;
}

internal static ж<sync.Once> ᏑserverInit = new(default(sync.Once));
internal static ref sync.Once serverInit => ref ᏑserverInit.Value;

[GoRecv] internal static error init(this ref pollDesc pd, ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    ᏑserverInit.Do(runtime_pollServerInit);
    var (ctx, errno) = runtime_pollOpen((uintptr)fd.Sysfd);
    if (errno != 0) {
        return errnoErr(((Δsyscall.Errno)(uintptr)errno));
    }
    pd.runtimeCtx = ctx;
    return default!;
}

[GoRecv] internal static void close(this ref pollDesc pd) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollClose(pd.runtimeCtx);
    pd.runtimeCtx = 0;
}

// Evict evicts fd from the pending list, unblocking any I/O running on fd.
[GoRecv] internal static void evict(this ref pollDesc pd) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollUnblock(pd.runtimeCtx);
}

[GoRecv] internal static error prepare(this ref pollDesc pd, nint mode, bool isFile) {
    if (pd.runtimeCtx == 0) {
        return default!;
    }
    nint res = runtime_pollReset(pd.runtimeCtx, mode);
    return convertErr(res, isFile);
}

[GoRecv] internal static error prepareRead(this ref pollDesc pd, bool isFile) {
    return pd.prepare((rune)'r', isFile);
}

[GoRecv] internal static error prepareWrite(this ref pollDesc pd, bool isFile) {
    return pd.prepare((rune)'w', isFile);
}

[GoRecv] internal static error wait(this ref pollDesc pd, nint mode, bool isFile) {
    if (pd.runtimeCtx == 0) {
        return errors.New("waiting for unsupported file type"u8);
    }
    nint res = runtime_pollWait(pd.runtimeCtx, mode);
    return convertErr(res, isFile);
}

[GoRecv] internal static error waitRead(this ref pollDesc pd, bool isFile) {
    return pd.wait((rune)'r', isFile);
}

[GoRecv] internal static error waitWrite(this ref pollDesc pd, bool isFile) {
    return pd.wait((rune)'w', isFile);
}

[GoRecv] internal static void waitCanceled(this ref pollDesc pd, nint mode) {
    if (pd.runtimeCtx == 0) {
        return;
    }
    runtime_pollWaitCanceled(pd.runtimeCtx, mode);
}

[GoRecv] internal static bool pollable(this ref pollDesc pd) {
    return pd.runtimeCtx != 0;
}

// Error values returned by runtime_pollReset and runtime_pollWait.
// These must match the values in runtime/netpoll.go.
internal static readonly UntypedInt pollNoError = 0;

internal static readonly UntypedInt pollErrClosing = 1;

internal static readonly UntypedInt pollErrTimeout = 2;

internal static readonly UntypedInt pollErrNotPollable = 3;

internal static error convertErr(nint res, bool isFile) {
    var exprᴛ1 = res;
    if (exprᴛ1 == pollNoError) {
        return default!;
    }
    if (exprᴛ1 == pollErrClosing) {
        return errClosing(isFile);
    }
    if (exprᴛ1 == pollErrTimeout) {
        return ErrDeadlineExceeded;
    }
    if (exprᴛ1 == pollErrNotPollable) {
        return ErrNotPollable;
    }

    println("unreachable: ", res);
    throw panic("unreachable");
}

// SetDeadline sets the read and write deadlines associated with fd.
public static error SetDeadline(this ж<FD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return setDeadlineImpl(Ꮡfd, t, (rune)'r' + (rune)'w');
}

// SetReadDeadline sets the read deadline associated with fd.
public static error SetReadDeadline(this ж<FD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return setDeadlineImpl(Ꮡfd, t, (rune)'r');
}

// SetWriteDeadline sets the write deadline associated with fd.
public static error SetWriteDeadline(this ж<FD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return setDeadlineImpl(Ꮡfd, t, (rune)'w');
}

internal static error setDeadlineImpl(ж<FD> Ꮡfd, time.Time t, nint mode) => func<error>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    int64 d = default!;
    if (!t.IsZero()) {
        d = (int64)time.Until(t);
        if (d == 0) {
            d = -1;
        }
    }
    // don't confuse deadline right now with no deadline
    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    if (fd.pd.runtimeCtx == 0) {
        return ErrNoDeadline;
    }
    runtime_pollSetDeadline(fd.pd.runtimeCtx, d, mode);
    return default!;
});

// IsPollDescriptor reports whether fd is the descriptor being used by the poller.
// This is only used for testing.
//
// IsPollDescriptor should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/opencontainers/runc
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname IsPollDescriptor
public static bool IsPollDescriptor(uintptr fd) {
    return runtime_isPollServerDescriptor(fd);
}

} // end poll_package
