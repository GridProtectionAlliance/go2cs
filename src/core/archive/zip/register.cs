// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using flate = compress.flate_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using compress;

partial class zip_package {

public delegate (io.WriteCloser, error) Compressor(io.Writer w);

public delegate io.ReadCloser Decompressor(io.Reader r);

internal static sync.Pool flateWriterPool;

internal static io.WriteCloser newFlateWriter(io.Writer w) {
    var (fw, ok) = flateWriterPool.Get()._<ж<flate.Writer>>(ᐧ);
    if (ok){
        fw.Reset(w);
    } else {
        (fw, _) = flate.NewWriter(w, 5);
    }
    return new pooledFlateWriter(fw: fw);
}

[GoType] partial struct pooledFlateWriter {
    internal sync_package.Mutex mu; // guards Close and Write
    internal ж<compress.flate_package.Writer> fw;
}

[GoRecv] internal static (nint n, error err) Write(this ref pooledFlateWriter w, slice<byte> p) => func((defer, _) => {
    nint n = default!;
    error err = default!;

    w.mu.Lock();
    defer(w.mu.Unlock);
    if (w.fw == nil) {
        return (0, errors.New("Write after Close"u8));
    }
    return w.fw.Write(p);
});

[GoRecv] internal static error Close(this ref pooledFlateWriter w) => func((defer, _) => {
    w.mu.Lock();
    defer(w.mu.Unlock);
    error err = default!;
    if (w.fw != nil) {
        err = w.fw.Close();
        flateWriterPool.Put(w.fw);
        w.fw = default!;
    }
    return err;
});

internal static sync.Pool flateReaderPool;

internal static io.ReadCloser newFlateReader(io.Reader r) {
    var (fr, ok) = flateReaderPool.Get()._<io.ReadCloser>(ᐧ);
    if (ok){
        fr._<flate.Resetter>().Reset(r, default!);
    } else {
        fr = flate.NewReader(r);
    }
    return new pooledFlateReader(fr: fr);
}

[GoType] partial struct pooledFlateReader {
    internal sync_package.Mutex mu; // guards Close and Read
    internal io_package.ReadCloser fr;
}

[GoRecv] internal static (nint n, error err) Read(this ref pooledFlateReader r, slice<byte> p) => func((defer, _) => {
    nint n = default!;
    error err = default!;

    r.mu.Lock();
    defer(r.mu.Unlock);
    if (r.fr == default!) {
        return (0, errors.New("Read after Close"u8));
    }
    return r.fr.Read(p);
});

[GoRecv] internal static error Close(this ref pooledFlateReader r) => func((defer, _) => {
    r.mu.Lock();
    defer(r.mu.Unlock);
    error err = default!;
    if (r.fr != default!) {
        err = r.fr.Close();
        flateReaderPool.Put(r.fr);
        r.fr = default!;
    }
    return err;
});

internal static sync.Map compressors; // map[uint16]Compressor
internal static sync.Map decompressors; // map[uint16]Decompressor

[GoInit] internal static void init() {
    compressors.Store(Store, ((Compressor)((io.Writer w) => (Ꮡ(new nopCloser(w)), default!))));
    compressors.Store(Deflate, ((Compressor)((io.Writer w) => (newFlateWriter(w), default!))));
    decompressors.Store(Store, ((Decompressor)io.NopCloser));
    decompressors.Store(Deflate, ((Decompressor)newFlateReader));
}

// RegisterDecompressor allows custom decompressors for a specified method ID.
// The common methods [Store] and [Deflate] are built in.
public static void RegisterDecompressor(uint16 method, Decompressor dcomp) {
    {
        var (_, dup) = decompressors.LoadOrStore(method, dcomp); if (dup) {
            throw panic("decompressor already registered");
        }
    }
}

// RegisterCompressor registers custom compressors for a specified method ID.
// The common methods [Store] and [Deflate] are built in.
public static void RegisterCompressor(uint16 method, Compressor comp) {
    {
        var (_, dup) = compressors.LoadOrStore(method, comp); if (dup) {
            throw panic("compressor already registered");
        }
    }
}

internal static Compressor compressor(uint16 method) {
    var (ci, ok) = compressors.Load(method);
    if (!ok) {
        return default!;
    }
    return ci._<Compressor>();
}

internal static Decompressor decompressor(uint16 method) {
    var (di, ok) = decompressors.Load(method);
    if (!ok) {
        return default!;
    }
    return di._<Decompressor>();
}

} // end zip_package
