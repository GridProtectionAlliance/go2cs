// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Pipe adapter to connect code expecting an io.Reader
// with code expecting an io.Writer.
namespace go;

using errors = errors_package;
using Δsync = sync_package;

partial class io_package {

// onceError is an object that will only store an error once.
[GoType] partial struct onceError {
    public partial ref sync_package.Mutex Mutex { get; } // guards following
    internal error err;
}

internal static void Store(this ж<onceError> Ꮡa, error err) => func((defer, recover) => {
    ref var a = ref Ꮡa.Value;

    Ꮡa.of(onceError.ᏑMutex).Lock();
    defer(Ꮡa.of(onceError.ᏑMutex).Unlock);
    if (a.err != default!) {
        return;
    }
    a.err = err;
});

internal static error Load(this ж<onceError> Ꮡa) => func((defer, recover) => {
    ref var a = ref Ꮡa.Value;

    Ꮡa.of(onceError.ᏑMutex).Lock();
    defer(Ꮡa.of(onceError.ᏑMutex).Unlock);
    return a.err;
});

// ErrClosedPipe is the error used for read or write operations on a closed pipe.
public static error ErrClosedPipe = errors.New("io: read/write on closed pipe"u8);

// A pipe is the shared pipe structure underlying PipeReader and PipeWriter.
[GoType] partial struct pipe {
    internal Δsync.Mutex wrMu; // Serializes Write operations
    internal channel<slice<byte>> wrCh;
    internal channel<nint> rdCh;
    internal Δsync.Once once; // Protects closing done
    internal channel<EmptyStruct> done;
    internal onceError rerr;
    internal onceError werr;
}

internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;

    ref var p = ref Ꮡp.Value;
    switch (ᐧ) {
    case ᐧ when p.done.ꟷᐳ(out _): {
        return (0, Ꮡp.readCloseError());
    }
    default: {
        break;
    }}
    switch (select(ᐸꟷ(p.wrCh, ꓸꓸꓸ), ᐸꟷ(p.done, ꓸꓸꓸ))) {
    case 0 when p.wrCh.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdCh.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when p.done.ꟷᐳ(out _): {
        return (0, Ꮡp.readCloseError());
    }}
    return default!;
}

internal static error closeRead(this ж<pipe> Ꮡp, error err) {
    ref var p = ref Ꮡp.Value;

    if (err == default!) {
        err = ErrClosedPipe;
    }
    Ꮡp.of(pipe.Ꮡrerr).Store(err);
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        close(Ꮡp.Value.done);
    });
    return default!;
}

internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        switch (ᐧ) {
        case ᐧ when p.done.ꟷᐳ(out _): {
            (n, err) = (0, Ꮡp.writeCloseError()); return;
        }
        default: {
            Ꮡp.of(pipe.ᏑwrMu).Lock();
            defer(Ꮡp.of(pipe.ᏑwrMu).Unlock);
            break;
        }}
        for (var once = true; once || len(b) > 0; once = false) {
            switch (select(p.wrCh.ᐸꟷ(b, ꓸꓸꓸ), ᐸꟷ(p.done, ꓸꓸꓸ))) {
            case 0: {
                nint nw = ᐸꟷ(p.rdCh);
                b = b[(int)(nw)..];
                n += nw;
                break;
            }
            case 1 when p.done.ꟷᐳ(out _): {
                (n, err) = (n, Ꮡp.writeCloseError()); return;
            }}
        }
        (n, err) = (n, default!);
    });
    return (n, err);
}

internal static error closeWrite(this ж<pipe> Ꮡp, error err) {
    ref var p = ref Ꮡp.Value;

    if (err == default!) {
        err = EOF;
    }
    Ꮡp.of(pipe.Ꮡwerr).Store(err);
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        close(Ꮡp.Value.done);
    });
    return default!;
}

// readCloseError is considered internal to the pipe type.
internal static error readCloseError(this ж<pipe> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    var rerr = Ꮡp.of(pipe.Ꮡrerr).Load();
    {
        var werr = Ꮡp.of(pipe.Ꮡwerr).Load(); if (rerr == default! && werr != default!) {
            return werr;
        }
    }
    return ErrClosedPipe;
}

// writeCloseError is considered internal to the pipe type.
internal static error writeCloseError(this ж<pipe> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    var werr = Ꮡp.of(pipe.Ꮡwerr).Load();
    {
        var rerr = Ꮡp.of(pipe.Ꮡrerr).Load(); if (werr == default! && rerr != default!) {
            return rerr;
        }
    }
    return ErrClosedPipe;
}

// A PipeReader is the read half of a pipe.
[GoType] partial struct PipeReader {
    internal partial ref pipe pipe { get; }
}

// Read implements the standard Read interface:
// it reads data from the pipe, blocking until a writer
// arrives or the write end is closed.
// If the write end is closed with an error, that error is
// returned as err; otherwise err is EOF.
public static (nint n, error err) Read(this ж<PipeReader> Ꮡr, slice<byte> data) {
    nint n = default!;
    error err = default!;

    ref var r = ref Ꮡr.Value;
    return Ꮡr.of(PipeReader.Ꮡpipe).read(data);
}

// Close closes the reader; subsequent writes to the
// write half of the pipe will return the error [ErrClosedPipe].
public static error Close(this ж<PipeReader> Ꮡr) {
    ref var r = ref Ꮡr.Value;

    return Ꮡr.CloseWithError(default!);
}

// CloseWithError closes the reader; subsequent writes
// to the write half of the pipe will return the error err.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
public static error CloseWithError(this ж<PipeReader> Ꮡr, error err) {
    ref var r = ref Ꮡr.Value;

    return Ꮡr.of(PipeReader.Ꮡpipe).closeRead(err);
}

// A PipeWriter is the write half of a pipe.
[GoType] partial struct PipeWriter {
    internal PipeReader r;
}

// Write implements the standard Write interface:
// it writes data to the pipe, blocking until one or more readers
// have consumed all the data or the read end is closed.
// If the read end is closed with an error, that err is
// returned as err; otherwise err is [ErrClosedPipe].
public static (nint n, error err) Write(this ж<PipeWriter> Ꮡw, slice<byte> data) {
    nint n = default!;
    error err = default!;

    ref var w = ref Ꮡw.Value;
    return Ꮡw.of(PipeWriter.Ꮡr).of(PipeReader.Ꮡpipe).write(data);
}

// Close closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and EOF.
public static error Close(this ж<PipeWriter> Ꮡw) {
    ref var w = ref Ꮡw.Value;

    return Ꮡw.CloseWithError(default!);
}

// CloseWithError closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and the error err,
// or EOF if err is nil.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
public static error CloseWithError(this ж<PipeWriter> Ꮡw, error err) {
    ref var w = ref Ꮡw.Value;

    return Ꮡw.of(PipeWriter.Ꮡr).of(PipeReader.Ꮡpipe).closeWrite(err);
}

// Pipe creates a synchronous in-memory pipe.
// It can be used to connect code expecting an [io.Reader]
// with code expecting an [io.Writer].
//
// Reads and Writes on the pipe are matched one to one
// except when multiple Reads are needed to consume a single Write.
// That is, each Write to the [PipeWriter] blocks until it has satisfied
// one or more Reads from the [PipeReader] that fully consume
// the written data.
// The data is copied directly from the Write to the corresponding
// Read (or Reads); there is no internal buffering.
//
// It is safe to call Read and Write in parallel with each other or with Close.
// Parallel calls to Read and parallel calls to Write are also safe:
// the individual calls will be gated sequentially.
public static (ж<PipeReader>, ж<PipeWriter>) Pipe() {
    var pw = Ꮡ(new PipeWriter(r: new PipeReader(pipe: new pipe(
        wrCh: new channel<slice<byte>>(1),
        rdCh: new channel<nint>(1),
        done: new channel<EmptyStruct>(1)
    )
    )
    ));
    return (pw.of(PipeWriter.Ꮡr), pw);
}

} // end io_package
