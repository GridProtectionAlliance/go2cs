// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:42:52 UTC
// Original source: C:\Program Files\Go\src\cmd\vet\main.go
namespace go;

using objabi = cmd.@internal.objabi_package;

using unitchecker = golang.org.x.tools.go.analysis.unitchecker_package;

using asmdecl = golang.org.x.tools.go.analysis.passes.asmdecl_package;
using assign = golang.org.x.tools.go.analysis.passes.assign_package;
using atomic = golang.org.x.tools.go.analysis.passes.atomic_package;
using bools = golang.org.x.tools.go.analysis.passes.bools_package;
using buildtag = golang.org.x.tools.go.analysis.passes.buildtag_package;
using cgocall = golang.org.x.tools.go.analysis.passes.cgocall_package;
using composite = golang.org.x.tools.go.analysis.passes.composite_package;
using copylock = golang.org.x.tools.go.analysis.passes.copylock_package;
using errorsas = golang.org.x.tools.go.analysis.passes.errorsas_package;
using framepointer = golang.org.x.tools.go.analysis.passes.framepointer_package;
using httpresponse = golang.org.x.tools.go.analysis.passes.httpresponse_package;
using ifaceassert = golang.org.x.tools.go.analysis.passes.ifaceassert_package;
using loopclosure = golang.org.x.tools.go.analysis.passes.loopclosure_package;
using lostcancel = golang.org.x.tools.go.analysis.passes.lostcancel_package;
using nilfunc = golang.org.x.tools.go.analysis.passes.nilfunc_package;
using printf = golang.org.x.tools.go.analysis.passes.printf_package;
using shift = golang.org.x.tools.go.analysis.passes.shift_package;
using sigchanyzer = golang.org.x.tools.go.analysis.passes.sigchanyzer_package;
using stdmethods = golang.org.x.tools.go.analysis.passes.stdmethods_package;
using stringintconv = golang.org.x.tools.go.analysis.passes.stringintconv_package;
using structtag = golang.org.x.tools.go.analysis.passes.structtag_package;
using testinggoroutine = golang.org.x.tools.go.analysis.passes.testinggoroutine_package;
using tests = golang.org.x.tools.go.analysis.passes.tests_package;
using unmarshal = golang.org.x.tools.go.analysis.passes.unmarshal_package;
using unreachable = golang.org.x.tools.go.analysis.passes.unreachable_package;
using unsafeptr = golang.org.x.tools.go.analysis.passes.unsafeptr_package;
using unusedresult = golang.org.x.tools.go.analysis.passes.unusedresult_package;

public static partial class main_package {

private static void Main() {
    objabi.AddVersionFlag();

    unitchecker.Main(asmdecl.Analyzer, assign.Analyzer, atomic.Analyzer, bools.Analyzer, buildtag.Analyzer, cgocall.Analyzer, composite.Analyzer, copylock.Analyzer, errorsas.Analyzer, framepointer.Analyzer, httpresponse.Analyzer, ifaceassert.Analyzer, loopclosure.Analyzer, lostcancel.Analyzer, nilfunc.Analyzer, printf.Analyzer, shift.Analyzer, sigchanyzer.Analyzer, stdmethods.Analyzer, stringintconv.Analyzer, structtag.Analyzer, tests.Analyzer, testinggoroutine.Analyzer, unmarshal.Analyzer, unreachable.Analyzer, unsafeptr.Analyzer, unusedresult.Analyzer);
}

} // end main_package
