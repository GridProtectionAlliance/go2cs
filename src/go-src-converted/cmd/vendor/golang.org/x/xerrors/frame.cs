// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2022 March 13 06:42:51 UTC
// import "cmd/vendor/golang.org/x/xerrors" ==> using xerrors = go.cmd.vendor.golang.org.x.xerrors_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\xerrors\frame.go
namespace go.cmd.vendor.golang.org.x;

using runtime = runtime_package;


// A Frame contains part of a call stack.

public static partial class xerrors_package {

public partial struct Frame {
    public array<System.UIntPtr> frames;
}

// Caller returns a Frame that describes a frame on the caller's stack.
// The argument skip is the number of frames to skip over.
// Caller(0) returns the frame for the caller of Caller.
public static Frame Caller(nint skip) {
    Frame s = default;
    runtime.Callers(skip + 1, s.frames[..]);
    return s;
}

// location reports the file, line, and function of a frame.
//
// The returned function may be "" even if file and line are not.
public static (@string, @string, nint) location(this Frame f) {
    @string function = default;
    @string file = default;
    nint line = default;

    var frames = runtime.CallersFrames(f.frames[..]);
    {
        var (_, ok) = frames.Next();

        if (!ok) {
            return ("", "", 0);
        }
    }
    var (fr, ok) = frames.Next();
    if (!ok) {
        return ("", "", 0);
    }
    return (fr.Function, fr.File, fr.Line);
}

// Format prints the stack as error detail.
// It should be called from an error's Format implementation
// after printing any other error detail.
public static void Format(this Frame f, Printer p) {
    if (p.Detail()) {
        var (function, file, line) = f.location();
        if (function != "") {
            p.Printf("%s\n    ", function);
        }
        if (file != "") {
            p.Printf("%s:%d\n", file, line);
        }
    }
}

} // end xerrors_package
