// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package composite -- go2cs converted at 2020 October 08 04:57:53 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/composite" ==> using composite = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.composite_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\composite\whitelist.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class composite_package
    {
        // unkeyedLiteral is a white list of types in the standard packages
        // that are used with unkeyed literals we deem to be acceptable.
        private static map unkeyedLiteral = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"image/color.Alpha16":true,"image/color.Alpha":true,"image/color.CMYK":true,"image/color.Gray16":true,"image/color.Gray":true,"image/color.NRGBA64":true,"image/color.NRGBA":true,"image/color.NYCbCrA":true,"image/color.RGBA64":true,"image/color.RGBA":true,"image/color.YCbCr":true,"image.Point":true,"image.Rectangle":true,"image.Uniform":true,"unicode.Range16":true,"unicode.Range32":true,"testing.InternalBenchmark":true,"testing.InternalExample":true,"testing.InternalTest":true,};
    }
}}}}}}}}}
