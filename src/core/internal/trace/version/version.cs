// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using fmt = fmt_package;
using io = io_package;
using @event = @internal.trace.event_package;
using go122 = @internal.trace.@event.go122_package;
using @internal.trace.@event;

partial class version_package {

[GoType("num:uint32")] partial struct Version;

public static readonly Version Go111 = 11;
public static readonly Version Go119 = 19;
public static readonly Version Go121 = 21;
public static readonly Version Go122 = 22;
public static readonly Version Go123 = 23;
public static readonly Version Current = /* Go123 */ 23;

// Go 1.11–1.21 use a different parser and are only set here for the sake of
// Version.Valid.
// Go 1.23 adds backwards-incompatible events, but
// traces produced by Go 1.22 are also always valid
// Go 1.23 traces.
internal static @event.Spec versions = new map<Version, slice<@event.Spec>>{
    [Go111] = default!,
    [Go119] = default!,
    [Go121] = default!,
    [Go122] = go122.Specs(),
    [Go123] = go122.Specs()
};

// Specs returns the set of event.Specs for this version.
public static slice<@event.Spec> Specs(this Version v) {
    return versions[v];
}

public static bool Valid(this Version v) {
    var _ = versions[v];
    var ok = versions[v];
    return ok;
}

// headerFmt is the format of the header of all Go execution traces.
internal static readonly @string headerFmt = "go 1.%d trace\x00\x00\x00"u8;

// ReadHeader reads the version of the trace out of the trace file's
// header, whose prefix must be present in v.
public static (Version, error) ReadHeader(io.Reader r) {
    ref var v = ref heap(new Version(), out var Ꮡv);
    var (_, err) = fmt.Fscanf(r, headerFmt, Ꮡv);
    if (err != default!) {
        return (v, fmt.Errorf("bad file format: not a Go execution trace?"u8));
    }
    if (!v.Valid()) {
        return (v, fmt.Errorf("unknown or unsupported trace version go 1.%d"u8, v));
    }
    return (v, default!);
}

// WriteHeader writes a header for a trace version v to w.
public static (nint, error) WriteHeader(io.Writer w, Version v) {
    return fmt.Fprintf(w, headerFmt, v);
}

} // end version_package
