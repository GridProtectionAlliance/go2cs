// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using strconv = strconv_package;
using strings = strings_package;
using @event = @internal.trace.event_package;
using version = @internal.trace.version_package;

partial class raw_package {

// Event is a simple representation of a trace event.
//
// Note that this typically includes much more than just
// timestamped events, and it also represents parts of the
// trace format's framing. (But not interpreted.)
[GoType] partial struct Event {
    public @internal.trace.version_package.Version Version;
    public @internal.trace.event_package.Type Ev;
    public slice<uint64> Args;
    public slice<byte> Data;
}

// String returns the canonical string representation of the event.
//
// This format is the same format that is parsed by the TextReader
// and emitted by the TextWriter.
[GoRecv] public static @string String(this ref Event e) {
    var spec = e.Version.Specs()[e.Ev];
    strings.Builder s = default!;
    s.WriteString(spec.Name);
    foreach (var (i, _) in spec.Args) {
        s.WriteString(" "u8);
        s.WriteString(spec.Args[i]);
        s.WriteString("="u8);
        s.WriteString(strconv.FormatUint(e.Args[i], 10));
    }
    if (spec.IsStack) {
        var frames = e.Args[(int)(len(spec.Args))..];
        for (nint i = 0; i < len(frames); i++) {
            if (i % 4 == 0){
                s.WriteString("\n\t"u8);
            } else {
                s.WriteString(" "u8);
            }
            s.WriteString(frameFields[i % 4]);
            s.WriteString("="u8);
            s.WriteString(strconv.FormatUint(frames[i], 10));
        }
    }
    if (e.Data != default!) {
        s.WriteString("\n\tdata="u8);
        s.WriteString(strconv.Quote(((@string)e.Data)));
    }
    return s.String();
}

} // end raw_package
