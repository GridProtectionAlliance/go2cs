// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package errors implements functions to manipulate errors.
// package errors -- go2cs converted at 2018 July 21 15:02:29 UTC
// import "errors" ==> using errors = go.errors_package
// Original source: C:\Go\src\errors\errors.go

// New returns an error that formats as the given text.

using static go.builtin;

namespace go
{
    public static partial class errors_package
    {
        // New returns an error that formats as the given text.
        public static error New(@string text)
        {
            return error_cast(new errorString(text));
        }

        // errorString is a trivial implementation of error.
        internal partial struct errorString
        {
            public @string s;
        }

        private static @string Error(this ref errorString e)
        {
            return e.s;
        }
    }
}
