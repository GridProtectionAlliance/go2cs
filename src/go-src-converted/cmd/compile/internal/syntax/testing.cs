// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements testing support.

// package syntax -- go2cs converted at 2022 March 13 06:27:07 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\testing.go
namespace go.cmd.compile.@internal;

using io = io_package;
using regexp = regexp_package;
using strings = strings_package;


// CommentsDo parses the given source and calls the provided handler for each
// comment or error. If the text provided to handler starts with a '/' it is
// the comment text; otherwise it is the error message.

using System;
public static partial class syntax_package {

public static void CommentsDo(io.Reader src, Action<nuint, nuint, @string> handler) {
    scanner s = default;
    s.init(src, handler, comments);
    while (s.tok != _EOF) {
        s.next();
    }
}

// ERROR comments must start with text `ERROR "msg"` or `ERROR msg`.
// Space around "msg" or msg is ignored.
private static var errRx = regexp.MustCompile("^ *ERROR *\"?([^\"]*)\"?");

// ErrorMap collects all comments with comment text of the form
// `ERROR "msg"` or `ERROR msg` from the given src and returns them
// as []Error lists in a map indexed by line number. The position
// for each Error is the position of the token immediately preceding
// the comment, the Error message is the message msg extracted from
// the comment, with all errors that are on the same line collected
// in a slice, in source order. If there is no preceding token (the
// `ERROR` comment appears in the beginning of the file), then the
// recorded position is unknown (line, col = 0, 0). If there are no
// ERROR comments, the result is nil.
public static map<nuint, slice<Error>> ErrorMap(io.Reader src) {
    map<nuint, slice<Error>> errmap = default;
 
    // position of previous token
    ptr<PosBase> @base;
    var prev = default;

    scanner s = default;
    s.init(src, (_, _, text) => {
        if (text[0] != '/') {
            return ; // error, ignore
        }
        if (text[1] == '*') {
            text = text[..(int)len(text) - 2]; // strip trailing */
        }
        {
            scanner s__prev1 = s;

            s = errRx.FindStringSubmatch(text[(int)2..]);

            if (len(s) == 2) {
                var pos = MakePos(base, prev.line, prev.col);
                Error err = new Error(pos,strings.TrimSpace(s[1]));
                if (errmap == null) {
                    errmap = make_map<nuint, slice<Error>>();
                }
                errmap[prev.line] = append(errmap[prev.line], err);
            }

            s = s__prev1;

        }
    }, comments);

    while (s.tok != _EOF) {
        s.next();
        if (s.tok == _Semi && s.lit != "semicolon") {
            continue; // ignore automatically inserted semicolons
        }
        (prev.line, prev.col) = (s.line, s.col);
    }

    return ;
}

} // end syntax_package
