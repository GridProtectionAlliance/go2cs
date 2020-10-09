// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2020 October 09 06:05:07 UTC
// import "cmd/vendor/golang.org/x/xerrors" ==> using xerrors = go.cmd.vendor.golang.org.x.xerrors_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\xerrors\format.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x
{
    public static partial class xerrors_package
    {
        // A Formatter formats error messages.
        public partial interface Formatter : error
        {
            error FormatError(Printer p);
        }

        // A Printer formats error messages.
        //
        // The most common implementation of Printer is the one provided by package fmt
        // during Printf (as of Go 1.13). Localization packages such as golang.org/x/text/message
        // typically provide their own implementations.
        public partial interface Printer
        {
            bool Print(params object[] args); // Printf writes a formatted string.
            bool Printf(@string format, params object[] args); // Detail reports whether error detail is requested.
// After the first call to Detail, all text written to the Printer
// is formatted as additional detail, or ignored when
// detail has not been requested.
// If Detail returns false, the caller can avoid printing the detail at all.
            bool Detail();
        }
    }
}}}}}
