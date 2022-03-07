// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:28 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\testmain.go
// CreateTestMainPackage synthesizes a main package that runs all the
// tests of the supplied packages.
// It is closely coupled to $GOROOT/src/cmd/go/test.go and $GOROOT/src/testing.
//
// TODO(adonovan): throws this all away now that x/tools/go/packages
// provides access to the actual synthetic test main files.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using types = go.go.types_package;
using log = go.log_package;
using os = go.os_package;
using strings = go.strings_package;
using template = go.text.template_package;

namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

    // FindTests returns the Test, Benchmark, and Example functions
    // (as defined by "go test") defined in the specified package,
    // and its TestMain function, if any.
    //
    // Deprecated: Use golang.org/x/tools/go/packages to access synthetic
    // testmain packages.
public static (slice<ptr<Function>>, slice<ptr<Function>>, slice<ptr<Function>>, ptr<Function>) FindTests(ptr<Package> _addr_pkg) {
    slice<ptr<Function>> tests = default;
    slice<ptr<Function>> benchmarks = default;
    slice<ptr<Function>> examples = default;
    ptr<Function> main = default!;
    ref Package pkg = ref _addr_pkg.val;

    var prog = pkg.Prog; 

    // The first two of these may be nil: if the program doesn't import "testing",
    // it can't contain any tests, but it may yet contain Examples.
    ptr<types.Signature> testSig; // func(*testing.T)
    ptr<types.Signature> benchmarkSig; // func(*testing.B)
    var exampleSig = types.NewSignature(null, null, null, false); // func()

    // Obtain the types from the parameters of testing.MainStart.
    {
        var testingPkg = prog.ImportedPackage("testing");

        if (testingPkg != null) {
            var mainStart = testingPkg.Func("MainStart");
            var @params = mainStart.Signature.Params();
            testSig = funcField(@params.At(1).Type());
            benchmarkSig = funcField(@params.At(2).Type()); 

            // Does the package define this function?
            //   func TestMain(*testing.M)
            {
                var f__prev2 = f;

                var f = pkg.Func("TestMain");

                if (f != null) {
                    ptr<types.Signature> sig = f.Type()._<ptr<types.Signature>>();
                    var starM = mainStart.Signature.Results().At(0).Type(); // *testing.M
                    if (sig.Results().Len() == 0 && sig.Params().Len() == 1 && types.Identical(sig.Params().At(0).Type(), starM)) {
                        main = f;
                    }
                }
                f = f__prev2;

            }

        }
    } 

    // TODO(adonovan): use a stable order, e.g. lexical.
    foreach (var (_, mem) in pkg.Members) {
        {
            var f__prev1 = f;

            ptr<Function> (f, ok) = mem._<ptr<Function>>();

            if (ok && ast.IsExported(f.Name()) && strings.HasSuffix(prog.Fset.Position(f.Pos()).Filename, "_test.go")) {

                if (testSig != null && isTestSig(_addr_f, "Test", testSig)) 
                    tests = append(tests, f);
                else if (benchmarkSig != null && isTestSig(_addr_f, "Benchmark", benchmarkSig)) 
                    benchmarks = append(benchmarks, f);
                else if (isTestSig(_addr_f, "Example", _addr_exampleSig)) 
                    examples = append(examples, f);
                else 
                    continue;
                
            }
            f = f__prev1;

        }

    }    return ;

}

// Like isTest, but checks the signature too.
private static bool isTestSig(ptr<Function> _addr_f, @string prefix, ptr<types.Signature> _addr_sig) {
    ref Function f = ref _addr_f.val;
    ref types.Signature sig = ref _addr_sig.val;

    return isTest(f.Name(), prefix) && types.Identical(f.Signature, sig);
}

// Given the type of one of the three slice parameters of testing.Main,
// returns the function type.
private static ptr<types.Signature> funcField(types.Type slice) {
    return slice._<ptr<types.Slice>>().Elem().Underlying()._<ptr<types.Struct>>().Field(1).Type()._<ptr<types.Signature>>();
}

// isTest tells whether name looks like a test (or benchmark, according to prefix).
// It is a Test (say) if there is a character after Test that is not a lower-case letter.
// We don't want TesticularCancer.
// Plundered from $GOROOT/src/cmd/go/test.go
private static bool isTest(@string name, @string prefix) {
    if (!strings.HasPrefix(name, prefix)) {
        return false;
    }
    if (len(name) == len(prefix)) { // "Test" is ok
        return true;

    }
    return ast.IsExported(name[(int)len(prefix)..]);

}

// CreateTestMainPackage creates and returns a synthetic "testmain"
// package for the specified package if it defines tests, benchmarks or
// executable examples, or nil otherwise.  The new package is named
// "main" and provides a function named "main" that runs the tests,
// similar to the one that would be created by the 'go test' tool.
//
// Subsequent calls to prog.AllPackages include the new package.
// The package pkg must belong to the program prog.
//
// Deprecated: Use golang.org/x/tools/go/packages to access synthetic
// testmain packages.
private static ptr<Package> CreateTestMainPackage(this ptr<Program> _addr_prog, ptr<Package> _addr_pkg) {
    ref Program prog = ref _addr_prog.val;
    ref Package pkg = ref _addr_pkg.val;

    if (pkg.Prog != prog) {
        log.Fatal("Package does not belong to Program");
    }
    var data = default;
    data.Pkg = pkg; 

    // Enumerate tests.
    data.Tests, data.Benchmarks, data.Examples, data.Main = FindTests(_addr_pkg);
    if (data.Main == null && data.Tests == null && data.Benchmarks == null && data.Examples == null) {
        return _addr_null!;
    }
    var path = pkg.Pkg.Path() + "$testmain";
    var tmpl = testmainTmpl;
    {
        var testingPkg = prog.ImportedPackage("testing");

        if (testingPkg != null) { 
            // In Go 1.8, testing.MainStart's first argument is an interface, not a func.
            data.Go18 = types.IsInterface(testingPkg.Func("MainStart").Signature.Params().At(0).Type());

        }
        else
 { 
            // The program does not import "testing", but FindTests
            // returned non-nil, which must mean there were Examples
            // but no Test, Benchmark, or TestMain functions.

            // We'll simply call them from testmain.main; this will
            // ensure they don't panic, but will not check any
            // "Output:" comments.
            // (We should not execute an Example that has no
            // "Output:" comment, but it's impossible to tell here.)
            tmpl = examplesOnlyTmpl;

        }
    }

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    {
        var err = tmpl.Execute(_addr_buf, data);

        if (err != null) {
            log.Fatalf("internal error expanding template for %s: %v", path, err);
        }
    }

    if (false) { // debugging
        fmt.Fprintln(os.Stderr, buf.String());

    }
    var (f, err) = parser.ParseFile(prog.Fset, path + ".go", _addr_buf, parser.Mode(0));
    if (err != null) {
        log.Fatalf("internal error parsing %s: %v", path, err);
    }
    types.Config conf = new types.Config(DisableUnusedImportCheck:true,Importer:importer{pkg},);
    ptr<ast.File> files = new slice<ptr<ast.File>>(new ptr<ast.File>[] { f });
    ptr<types.Info> info = addr(new types.Info(Types:make(map[ast.Expr]types.TypeAndValue),Defs:make(map[*ast.Ident]types.Object),Uses:make(map[*ast.Ident]types.Object),Implicits:make(map[ast.Node]types.Object),Scopes:make(map[ast.Node]*types.Scope),Selections:make(map[*ast.SelectorExpr]*types.Selection),));
    var (testmainPkg, err) = conf.Check(path, prog.Fset, files, info);
    if (err != null) {
        log.Fatalf("internal error type-checking %s: %v", path, err);
    }
    var testmain = prog.CreatePackage(testmainPkg, files, info, false);
    testmain.SetDebugMode(false);
    testmain.Build();
    testmain.Func("main").Synthetic;

    "test main function";
    testmain.Func("init").Synthetic;

    "package initializer";
    return _addr_testmain!;

}

// An implementation of types.Importer for an already loaded SSA program.
private partial struct importer {
    public ptr<Package> pkg; // package under test; may be non-importable
}

private static (ptr<types.Package>, error) Import(this importer imp, @string path) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;

    {
        var p = imp.pkg.Prog.ImportedPackage(path);

        if (p != null) {
            return (_addr_p.Pkg!, error.As(null!)!);
        }
    }

    if (path == imp.pkg.Pkg.Path()) {
        return (_addr_imp.pkg.Pkg!, error.As(null!)!);
    }
    return (_addr_null!, error.As(fmt.Errorf("not found"))!); // can't happen
}

private static var testmainTmpl = template.Must(template.New("testmain").Parse(@"
package main

import ""io""
import ""os""
import ""testing""
import p {{printf ""%q"" .Pkg.Pkg.Path}}

{{if .Go18}}
type deps struct{}

func (deps) ImportPath() string { return """" }
func (deps) MatchString(pat, str string) (bool, error) { return true, nil }
func (deps) StartCPUProfile(io.Writer) error { return nil }
func (deps) StartTestLog(io.Writer) {}
func (deps) StopCPUProfile() {}
func (deps) StopTestLog() error { return nil }
func (deps) WriteHeapProfile(io.Writer) error { return nil }
func (deps) WriteProfileTo(string, io.Writer, int) error { return nil }

var match deps
{{else}}
func match(_, _ string) (bool, error) { return true, nil }
{{end}}

func main() {
	tests := []testing.InternalTest{
{{range .Tests}}
		{ {{printf ""%q"" .Name}}, p.{{.Name}} },
{{end}}
	}
	benchmarks := []testing.InternalBenchmark{
{{range .Benchmarks}}
		{ {{printf ""%q"" .Name}}, p.{{.Name}} },
{{end}}
	}
	examples := []testing.InternalExample{
{{range .Examples}}
		{Name: {{printf ""%q"" .Name}}, F: p.{{.Name}}},
{{end}}
	}
	m := testing.MainStart(match, tests, benchmarks, examples)
{{with .Main}}
	p.{{.Name}}(m)
{{else}}
	os.Exit(m.Run())
{{end}}
}

"));

private static var examplesOnlyTmpl = template.Must(template.New("examples").Parse("\npackage main\n\nimport p {{printf \"%q\" .Pkg.Pkg.Path}}\n\nfunc main() {\n{{range .Exa" +
    "mples}}\n\tp.{{.Name}}()\n{{end}}\n}\n"));

} // end ssa_package
