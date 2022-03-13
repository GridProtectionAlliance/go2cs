// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2022 March 13 06:43:41 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Program Files\Go\src\image\format.go
namespace go;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using atomic = sync.atomic_package;


// ErrFormat indicates that decoding encountered an unknown format.

using System;
public static partial class image_package {

public static var ErrFormat = errors.New("image: unknown format");

// A format holds an image format's name, magic header and how to decode it.
private partial struct format {
    public @string name;
    public @string magic;
    public Func<io.Reader, (Image, error)> decode;
    public Func<io.Reader, (Config, error)> decodeConfig;
}

// Formats is the list of registered formats.
private static sync.Mutex formatsMu = default;private static atomic.Value atomicFormats = default;

// RegisterFormat registers an image format for use by Decode.
// Name is the name of the format, like "jpeg" or "png".
// Magic is the magic prefix that identifies the format's encoding. The magic
// string can contain "?" wildcards that each match any one byte.
// Decode is the function that decodes the encoded image.
// DecodeConfig is the function that decodes just its configuration.
public static (Config, error) RegisterFormat(@string name, @string magic, Func<io.Reader, (Image, error)> decode, Func<io.Reader, (Config, error)> decodeConfig) {
    Config _p0 = default;
    error _p0 = default!;

    formatsMu.Lock();
    slice<format> (formats, _) = atomicFormats.Load()._<slice<format>>();
    atomicFormats.Store(append(formats, new format(name,magic,decode,decodeConfig)));
    formatsMu.Unlock();
}

// A reader is an io.Reader that can also peek ahead.
private partial interface reader {
    (slice<byte>, error) Peek(nint _p0);
}

// asReader converts an io.Reader to a reader.
private static reader asReader(io.Reader r) {
    {
        reader (rr, ok) = reader.As(r._<reader>())!;

        if (ok) {
            return rr;
        }
    }
    return bufio.NewReader(r);
}

// Match reports whether magic matches b. Magic may contain "?" wildcards.
private static bool match(@string magic, slice<byte> b) {
    if (len(magic) != len(b)) {
        return false;
    }
    foreach (var (i, c) in b) {
        if (magic[i] != c && magic[i] != '?') {
            return false;
        }
    }    return true;
}

// Sniff determines the format of r's data.
private static format sniff(reader r) {
    slice<format> (formats, _) = atomicFormats.Load()._<slice<format>>();
    foreach (var (_, f) in formats) {
        var (b, err) = r.Peek(len(f.magic));
        if (err == null && match(f.magic, b)) {
            return f;
        }
    }    return new format();
}

// Decode decodes an image that has been encoded in a registered format.
// The string returned is the format name used during format registration.
// Format registration is typically done by an init function in the codec-
// specific package.
public static (Image, @string, error) Decode(io.Reader r) {
    Image _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    var rr = asReader(r);
    var f = sniff(rr);
    if (f.decode == null) {
        return (null, "", error.As(ErrFormat)!);
    }
    var (m, err) = f.decode(rr);
    return (m, f.name, error.As(err)!);
}

// DecodeConfig decodes the color model and dimensions of an image that has
// been encoded in a registered format. The string returned is the format name
// used during format registration. Format registration is typically done by
// an init function in the codec-specific package.
public static (Config, @string, error) DecodeConfig(io.Reader r) {
    Config _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    var rr = asReader(r);
    var f = sniff(rr);
    if (f.decodeConfig == null) {
        return (new Config(), "", error.As(ErrFormat)!);
    }
    var (c, err) = f.decodeConfig(rr);
    return (c, f.name, error.As(err)!);
}

} // end image_package
