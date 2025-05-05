// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Pipe adapter to connect code expecting an io.Reader
// with code expecting an io.Writer.
namespace go;

using errors = errors_package;
using sync = sync_package;

partial class io_package {

// onceError is an object that will only store an error once.
[GoType] partial struct onceError {
    public partial ref sync_package.Mutex Mutex { get; } // guards following
    internal error err;
}

[GoRecv] internal static void Store(this ref onceError a, error err) => func((defer, _) => {
    a.Lock();
    defer(a.Unlock);
    if (a.err != default!) {
        return;
    }
    a.err = err;
});

[GoRecv] internal static error Load(this ref onceError a) => func((defer, _) => {
    a.Lock();
    defer(a.Unlock);
    return a.err;
});

// ErrClosedPipe is the error used for read or write operations on a closed pipe.
public static error ErrClosedPipe = errors.New("io: read/write on closed pipe"u8);

// A pipe is the shared pipe structure underlying PipeReader and PipeWriter.
[GoType] partial struct pipe {
    internal sync_package.Mutex wrMu; // Serializes Write operations
    internal channel<slice<byte>> wrCh;
    internal channel<nint> rdCh;
    internal sync_package.Once once; // Protects closing done
    internal channel<struct{}> done;
    internal onceError rerr;
    internal onceError werr;
}

[GoRecv] internal static (nint n, error err) read(this ref pipe p, slice<byte> b) {
    nint n = default!;
    error err = default!;

    switch (ᐧ) {
    case ᐧ when p.done.ꟷᐳ(out _): {
        return (0, p.readCloseError());
    }
    default: {
    }}
    switch (select(ᐸꟷ(p.wrCh, ꓸꓸꓸ), ᐸꟷ(p.done, ꓸꓸꓸ))) {
    case 0 when p.wrCh.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdCh.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when p.done.ꟷᐳ(out _): {
        return (0, p.readCloseError());
    }}
}

[GoRecv] internal static error closeRead(this ref pipe p, error err) {
    if (err == default!) {
        err = ErrClosedPipe;
    }
    p.rerr.Store(err);
    p.once.Do(() => {
        close(p.done);
    });
    return default!;
}

[GoRecv] internal static (nint n, error err) write(this ref pipe p, slice<byte> b) => func((defer, _) => {
    nint n = default!;
    error err = default!;

    switch (ᐧ) {
    case ᐧ when p.done.ꟷᐳ(out _): {
        return (0, p.writeCloseError());
    }
    default: {
        p.wrMu.Lock();
        defer(p.wrMu.Unlock);
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
            return (n, p.writeCloseError());
        }}
    }
    return (n, default!);
});

[GoRecv] internal static error closeWrite(this ref pipe p, error err) {
    if (err == default!) {
        err = EOF;
    }
    p.werr.Store(err);
    p.once.Do(() => {
        close(p.done);
    });
    return default!;
}

// readCloseError is considered internal to the pipe type.
[GoRecv] internal static error readCloseError(this ref pipe p) {
    var rerr = p.rerr.Load();
    {
        var werr = p.werr.Load(); if (rerr == default! && werr != default!) {
            return werr;
        }
    }
    return ErrClosedPipe;
}

// writeCloseError is considered internal to the pipe type.
[GoRecv] internal static error writeCloseError(this ref pipe p) {
    var werr = p.werr.Load();
    {
        var rerr = p.rerr.Load(); if (werr == default! && rerr != default!) {
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
[GoRecv] public static (nint n, error err) Read(this ref PipeReader r, slice<byte> data) {
    nint n = default!;
    error err = default!;

    return r.pipe.read(data);
}

// Close closes the reader; subsequent writes to the
// write half of the pipe will return the error [ErrClosedPipe].
[GoRecv] public static error Close(this ref PipeReader r) {
    return r.CloseWithError(default!);
}

// CloseWithError closes the reader; subsequent writes
// to the write half of the pipe will return the error err.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
[GoRecv] public static error CloseWithError(this ref PipeReader r, error err) {
    return r.pipe.closeRead(err);
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
[GoRecv] public static (nint n, error err) Write(this ref PipeWriter w, slice<byte> data) {
    nint n = default!;
    error err = default!;

    return w.r.pipe.write(data);
}

// Close closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and EOF.
[GoRecv] public static error Close(this ref PipeWriter w) {
    return w.CloseWithError(default!);
}

// CloseWithError closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and the error err,
// or EOF if err is nil.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
[GoRecv] public static error CloseWithError(this ref PipeWriter w, error err) {
    return w.r.pipe.closeWrite(err);
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
        done: new channel<struct{}>(1)
    )
    )
    ));
    return (Ꮡ((~pw).r), pw);
}

} // end io_package
