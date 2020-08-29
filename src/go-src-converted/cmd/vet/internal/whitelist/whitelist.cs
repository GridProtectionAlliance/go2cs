// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package whitelist defines exceptions for the vet tool.
// package whitelist -- go2cs converted at 2020 August 29 10:08:50 UTC
// import "cmd/vet/internal/whitelist" ==> using whitelist = go.cmd.vet.@internal.whitelist_package
// Original source: C:\Go\src\cmd\vet\internal\whitelist\whitelist.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace @internal
{
    public static partial class whitelist_package
    {
        // UnkeyedLiteral is a white list of types in the standard packages
        // that are used with unkeyed literals we deem to be acceptable.
        public static map UnkeyedLiteral = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"image/color.Alpha16":true,"image/color.Alpha":true,"image/color.CMYK":true,"image/color.Gray16":true,"image/color.Gray":true,"image/color.NRGBA64":true,"image/color.NRGBA":true,"image/color.NYCbCrA":true,"image/color.RGBA64":true,"image/color.RGBA":true,"image/color.YCbCr":true,"image.Point":true,"image.Rectangle":true,"image.Uniform":true,"unicode.Range16":true,};
    }
}}}}
