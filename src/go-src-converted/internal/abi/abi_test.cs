// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = go.@internal.abi_package;
using testenv = go.@internal.testenv_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using exec = os.exec_package;
using go.@internal;
using os;
using path;

partial class abi_test_package {

public static void TestFuncPC(ж<testing.T> Ꮡt) {
    // Test that FuncPC* can get correct function PC.
    var pcFromAsm = abi.FuncPCTestFnAddr;
    // Test FuncPC for locally defined function
    var pcFromGo = abi.FuncPCTest();
    if (pcFromGo != pcFromAsm) {
        Ꮡt.Errorf("FuncPC returns wrong PC, want %x, got %x"u8, pcFromAsm, pcFromGo);
    }
    // Test FuncPC for imported function
    pcFromGo = abi.FuncPCABI0(abi.FuncPCTestFn);
    if (pcFromGo != pcFromAsm) {
        Ꮡt.Errorf("FuncPC returns wrong PC, want %x, got %x"u8, pcFromAsm, pcFromGo);
    }
}

public static void TestFuncPCCompileError(ж<testing.T> Ꮡt) {
    // Test that FuncPC* on a function of a mismatched ABI is rejected.
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // We want to test internal package, which we cannot normally import.
    // Run the assembler and compiler manually.
    @string tmpdir = Ꮡt.TempDir();
    @string asmSrc = filepath.Join("testdata"u8, "x.s");
    @string goSrc = filepath.Join("testdata"u8, "x.go");
    @string symabi = filepath.Join(tmpdir, "symabi");
    @string obj = filepath.Join(tmpdir, "x.o");
    // Write an importcfg file for the dependencies of the package.
    @string importcfgfile = filepath.Join(tmpdir, "hello.importcfg");
    testenv.WriteImportcfg(new testing_TжTB(Ꮡt), importcfgfile, default!, "internal/abi"u8);
    // parse assembly code for symabi.
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.GoToolPath(new testing_TжTB(Ꮡt)), "tool"u8, "asm", "-p=p", "-gensymabis", "-o", symabi, asmSrc);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go tool asm -gensymabis failed: %v\n%s"u8, err, @out);
    }
    // compile go code.
    cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.GoToolPath(new testing_TжTB(Ꮡt)), "tool"u8, "compile", "-importcfg=" + importcfgfile, "-p=p", "-symabis", symabi, "-o", obj, goSrc);
    (@out, err) = cmd.CombinedOutput();
    if (err == default!) {
        Ꮡt.Fatalf("go tool compile did not fail"u8);
    }
    // Expect errors in line 17, 18, 20, no errors on other lines.
    var want = new @string[]{"x.go:17", "x.go:18", "x.go:20"}.slice();
    var got = strings.Split(((@string)@out), "\n"u8);
    if (got[len(got) - 1] == "") {
        got = got[..(int)(len(got) - 1)];
    }
    // remove last empty line
    foreach (var (i, s) in got) {
        if (!strings.Contains(s, want[i])) {
            Ꮡt.Errorf("did not error on line %s"u8, want[i]);
        }
    }
    if (len(got) != len(want)) {
        Ꮡt.Errorf("unexpected number of errors, want %d, got %d"u8, len(want), len(got));
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Logf("output:\n%s"u8, ((@string)@out));
    }
}

} // end abi_test_package
