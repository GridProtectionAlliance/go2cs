// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for untagged struct literals.

// package testdata -- go2cs converted at 2020 August 29 10:09:35 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\composite.go
using flag = go.flag_package;
using scanner = go.go.scanner_package;
using image = go.image_package;
using unicode = go.unicode_package;

using unknownpkg = go.path.to.unknownpkg_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static @string Okay1 = new slice<@string>(new @string[] { "Name", "Usage", "DefValue" });

        public static map Okay2 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Name":true,"Usage":true,"DefValue":true,};

        public static struct{XstringYstringZstring} Okay3 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{XstringYstringZstring}{"Name","Usage","DefValue",};



        public partial struct MyStruct
        {
            public @string X;
            public @string Y;
            public @string Z;
        }

        public static MyStruct Okay5 = ref new MyStruct("Name","Usage","DefValue",);

        public static MyStruct Okay6 = new slice<MyStruct>(new MyStruct[] { {"foo","bar","baz"}, {"aa","bb","cc"} });

        // Testing is awkward because we need to reference things from a separate package
        // to trigger the warnings.

        private static flag.Flag goodStructLiteral = new flag.Flag(Name:"Name",Usage:"Usage",);
        private static flag.Flag badStructLiteral = new flag.Flag("Name","Usage",nil,"DefValue",);

        // SpecialCase is a named slice of CaseRange to test issue 9171.
        private static unicode.SpecialCase goodNamedSliceLiteral = new unicode.SpecialCase({Lo:1,Hi:2},unicode.CaseRange{Lo:1,Hi:2},);
        private static unicode.SpecialCase badNamedSliceLiteral = new unicode.SpecialCase({1,2},unicode.CaseRange{1,2},);

        // ErrorList is a named slice, so no warnings should be emitted.
        private static scanner.ErrorList goodScannerErrorList = new scanner.ErrorList(&scanner.Error{Msg:"foobar"},);
        private static scanner.ErrorList badScannerErrorList = new scanner.ErrorList(&scanner.Error{"foobar"},);

        // Check whitelisted structs: if vet is run with --compositewhitelist=false,
        // this line triggers an error.
        private static image.Point whitelistedPoint = new image.Point(1,2);

        // Do not check type from unknown package.
        // See issue 15408.
        private static unknownpkg.Foobar unknownPkgVar = new unknownpkg.Foobar("foo","bar");
    }
}}}
