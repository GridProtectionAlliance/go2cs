// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package errors implements functions to manipulate errors.
// package errors -- go2cs converted at 2020 August 29 08:16:03 UTC
// import "errors" ==> using errors = go.errors_package
// Original source: C:\Go\src\errors\errors.go

using static go.builtin;

namespace go
{
    public static partial class errors_package
    {
        // New returns an error that formats as the given text.
        public static error New(@string text)
        {
            return error.As(ref new errorString(text));
        }

        // errorString is a trivial implementation of error.
        private partial struct errorString
        {
            public @string s;
        }

        private static @string Error(this ref errorString e)
        {
            return e.s;
        }
    }
}
