// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9
// +build plan9

// package poll -- go2cs converted at 2022 March 06 22:13:23 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\strconv.go


namespace go.@internal;

public static partial class poll_package {

    // stringsHasSuffix is strings.HasSuffix. It reports whether s ends in
    // suffix.
private static bool stringsHasSuffix(@string s, @string suffix) {
    return len(s) >= len(suffix) && s[(int)len(s) - len(suffix)..] == suffix;
}

} // end poll_package
