// Package analysistest provides utilities for testing analyzers.
// package analysistest -- go2cs converted at 2020 October 09 06:01:20 UTC
// import "golang.org/x/tools/go/analysis/analysistest" ==> using analysistest = go.golang.org.x.tools.go.analysis.analysistest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\analysistest\analysistest.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using checker = go.golang.org.x.tools.go.analysis.@internal.checker_package;
using packages = go.golang.org.x.tools.go.packages_package;
using diff = go.golang.org.x.tools.@internal.lsp.diff_package;
using myers = go.golang.org.x.tools.@internal.lsp.diff.myers_package;
using span = go.golang.org.x.tools.@internal.span_package;
using testenv = go.golang.org.x.tools.@internal.testenv_package;
using txtar = go.golang.org.x.tools.txtar_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis
{
    public static partial class analysistest_package
    {
        // WriteFiles is a helper function that creates a temporary directory
        // and populates it with a GOPATH-style project using filemap (which
        // maps file names to contents). On success it returns the name of the
        // directory and a cleanup function to delete it.
        public static (@string, Action, error) WriteFiles(map<@string, @string> filemap)
        {
            @string dir = default;
            Action cleanup = default;
            error err = default!;

            var (gopath, err) = ioutil.TempDir("", "analysistest");
            if (err != null)
            {
                return ("", null, error.As(err)!);
            }
            cleanup = () =>
            {
                os.RemoveAll(gopath);
            };

            foreach (var (name, content) in filemap)
            {
                var filename = filepath.Join(gopath, "src", name);
                os.MkdirAll(filepath.Dir(filename), 0777L); // ignore error
                {
                    var err = ioutil.WriteFile(filename, (slice<byte>)content, 0666L);

                    if (err != null)
                    {
                        cleanup();
                        return ("", null, error.As(err)!);
                    }
                }

            }            return (gopath, cleanup, error.As(null!)!);

        }

        // TestData returns the effective filename of
        // the program's "testdata" directory.
        // This function may be overridden by projects using
        // an alternative build system (such as Blaze) that
        // does not run a test in its package directory.
        public static Func<@string> TestData = () =>
        {
            var (testdata, err) = filepath.Abs("testdata");
            if (err != null)
            {
                log.Fatal(err);
            }

            return testdata;

        };

        // Testing is an abstraction of a *testing.T.
        public partial interface Testing
        {
            void Errorf(@string format, params object[] args);
        }

        // RunWithSuggestedFixes behaves like Run, but additionally verifies suggested fixes.
        // It uses golden files placed alongside the source code under analysis:
        // suggested fixes for code in example.go will be compared against example.go.golden.
        //
        // Golden files can be formatted in one of two ways: as plain Go source code, or as txtar archives.
        // In the first case, all suggested fixes will be applied to the original source, which will then be compared against the golden file.
        // In the second case, suggested fixes will be grouped by their messages, and each set of fixes will be applied and tested separately.
        // Each section in the archive corresponds to a single message.
        //
        // A golden file using txtar may look like this:
        //     -- turn into single negation --
        //     package pkg
        //
        //     func fn(b1, b2 bool) {
        //         if !b1 { // want `negating a boolean twice`
        //             println()
        //         }
        //     }
        //
        //     -- remove double negation --
        //     package pkg
        //
        //     func fn(b1, b2 bool) {
        //         if b1 { // want `negating a boolean twice`
        //             println()
        //         }
        //     }
        public static slice<ptr<Result>> RunWithSuggestedFixes(Testing t, @string dir, ptr<analysis.Analyzer> _addr_a, params @string[] patterns)
        {
            patterns = patterns.Clone();
            ref analysis.Analyzer a = ref _addr_a.val;

            var r = Run(t, dir, _addr_a, patterns); 

            // file -> message -> edits
            var fileEdits = make_map<ptr<token.File>, map<@string, slice<diff.TextEdit>>>();
            var fileContents = make_map<ptr<token.File>, slice<byte>>(); 

            // Validate edits, prepare the fileEdits map and read the file contents.
            foreach (var (_, act) in r)
            {
                foreach (var (_, diag) in act.Diagnostics)
                {
                    {
                        var sf__prev3 = sf;

                        foreach (var (_, __sf) in diag.SuggestedFixes)
                        {
                            sf = __sf;
                            foreach (var (_, edit) in sf.TextEdits)
                            { 
                                // Validate the edit.
                                if (edit.Pos > edit.End)
                                {
                                    t.Errorf("diagnostic for analysis %v contains Suggested Fix with malformed edit: pos (%v) > end (%v)", act.Pass.Analyzer.Name, edit.Pos, edit.End);
                                    continue;
                                }

                                var file = act.Pass.Fset.File(edit.Pos);
                                var endfile = act.Pass.Fset.File(edit.End);
                                if (file == null || endfile == null || file != endfile)
                                {
                                    t.Errorf("diagnostic for analysis %v contains Suggested Fix with malformed spanning files %v and %v", act.Pass.Analyzer.Name, file.Name(), endfile.Name());
                                    continue;
                                }

                                {
                                    var (_, ok) = fileContents[file];

                                    if (!ok)
                                    {
                                        var (contents, err) = ioutil.ReadFile(file.Name());
                                        if (err != null)
                                        {
                                            t.Errorf("error reading %s: %v", file.Name(), err);
                                        }

                                        fileContents[file] = contents;

                                    }

                                }

                                var (spn, err) = span.NewRange(act.Pass.Fset, edit.Pos, edit.End).Span();
                                if (err != null)
                                {
                                    t.Errorf("error converting edit to span %s: %v", file.Name(), err);
                                }

                                {
                                    (_, ok) = fileEdits[file];

                                    if (!ok)
                                    {
                                        fileEdits[file] = make_map<@string, slice<diff.TextEdit>>();
                                    }

                                }

                                fileEdits[file][sf.Message] = append(fileEdits[file][sf.Message], new diff.TextEdit(Span:spn,NewText:string(edit.NewText),));

                            }

                        }

                        sf = sf__prev3;
                    }
                }

            }
            {
                var file__prev1 = file;

                foreach (var (__file, __fixes) in fileEdits)
                {
                    file = __file;
                    fixes = __fixes; 
                    // Get the original file contents.
                    var (orig, ok) = fileContents[file];
                    if (!ok)
                    {
                        t.Errorf("could not find file contents for %s", file.Name());
                        continue;
                    } 

                    // Get the golden file and read the contents.
                    var (ar, err) = txtar.ParseFile(file.Name() + ".golden");
                    if (err != null)
                    {
                        t.Errorf("error reading %s.golden: %v", file.Name(), err);
                        continue;
                    }

                    if (len(ar.Files) > 0L)
                    { 
                        // one virtual file per kind of suggested fix

                        if (len(ar.Comment) != 0L)
                        { 
                            // we allow either just the comment, or just virtual
                            // files, not both. it is not clear how "both" should
                            // behave.
                            t.Errorf("%s.golden has leading comment; we don't know what to do with it", file.Name());
                            continue;

                        }

                        {
                            var sf__prev2 = sf;
                            var edits__prev2 = edits;

                            foreach (var (__sf, __edits) in fixes)
                            {
                                sf = __sf;
                                edits = __edits;
                                var found = false;
                                foreach (var (_, vf) in ar.Files)
                                {
                                    if (vf.Name == sf)
                                    {
                                        found = true;
                                        var @out = diff.ApplyEdits(string(orig), edits); 
                                        // the file may contain multiple trailing
                                        // newlines if the user places empty lines
                                        // between files in the archive. normalize
                                        // this to a single newline.
                                        var want = string(bytes.TrimRight(vf.Data, "\n")) + "\n";
                                        var (formatted, err) = format.Source((slice<byte>)out);
                                        if (err != null)
                                        {
                                            continue;
                                        }

                                        if (want != string(formatted))
                                        {
                                            var d = myers.ComputeEdits("", want, string(formatted));
                                            t.Errorf("suggested fixes failed for %s:\n%s", file.Name(), diff.ToUnified(fmt.Sprintf("%s.golden [%s]", file.Name(), sf), "actual", want, d));
                                        }

                                        break;

                                    }

                                }
                    else
                                if (!found)
                                {
                                    t.Errorf("no section for suggested fix %q in %s.golden", sf, file.Name());
                                }

                            }

                            sf = sf__prev2;
                            edits = edits__prev2;
                        }
                    }                    { 
                        // all suggested fixes are represented by a single file

                        slice<diff.TextEdit> catchallEdits = default;
                        {
                            var edits__prev2 = edits;

                            foreach (var (_, __edits) in fixes)
                            {
                                edits = __edits;
                                catchallEdits = append(catchallEdits, edits);
                            }

                            edits = edits__prev2;
                        }

                        @out = diff.ApplyEdits(string(orig), catchallEdits);
                        want = string(ar.Comment);

                        (formatted, err) = format.Source((slice<byte>)out);
                        if (err != null)
                        {
                            continue;
                        }

                        if (want != string(formatted))
                        {
                            d = myers.ComputeEdits("", want, string(formatted));
                            t.Errorf("suggested fixes failed for %s:\n%s", file.Name(), diff.ToUnified(file.Name() + ".golden", "actual", want, d));
                        }

                    }

                }

                file = file__prev1;
            }

            return r;

        }

        // Run applies an analysis to the packages denoted by the "go list" patterns.
        //
        // It loads the packages from the specified GOPATH-style project
        // directory using golang.org/x/tools/go/packages, runs the analysis on
        // them, and checks that each analysis emits the expected diagnostics
        // and facts specified by the contents of '// want ...' comments in the
        // package's source files.
        //
        // An expectation of a Diagnostic is specified by a string literal
        // containing a regular expression that must match the diagnostic
        // message. For example:
        //
        //    fmt.Printf("%s", 1) // want `cannot provide int 1 to %s`
        //
        // An expectation of a Fact associated with an object is specified by
        // 'name:"pattern"', where name is the name of the object, which must be
        // declared on the same line as the comment, and pattern is a regular
        // expression that must match the string representation of the fact,
        // fmt.Sprint(fact). For example:
        //
        //    func panicf(format string, args interface{}) { // want panicf:"printfWrapper"
        //
        // Package facts are specified by the name "package" and appear on
        // line 1 of the first source file of the package.
        //
        // A single 'want' comment may contain a mixture of diagnostic and fact
        // expectations, including multiple facts about the same object:
        //
        //    // want "diag" "diag2" x:"fact1" x:"fact2" y:"fact3"
        //
        // Unexpected diagnostics and facts, and unmatched expectations, are
        // reported as errors to the Testing.
        //
        // Run reports an error to the Testing if loading or analysis failed.
        // Run also returns a Result for each package for which analysis was
        // attempted, even if unsuccessful. It is safe for a test to ignore all
        // the results, but a test may use it to perform additional checks.
        public static slice<ptr<Result>> Run(Testing t, @string dir, ptr<analysis.Analyzer> _addr_a, params @string[] patterns)
        {
            patterns = patterns.Clone();
            ref analysis.Analyzer a = ref _addr_a.val;

            {
                testenv.Testing (t, ok) = t._<testenv.Testing>();

                if (ok)
                {
                    testenv.NeedsGoPackages(t);
                }

            }


            var (pkgs, err) = loadPackages(dir, patterns);
            if (err != null)
            {
                t.Errorf("loading %s: %v", patterns, err);
                return null;
            }

            var results = checker.TestAnalyzer(a, pkgs);
            foreach (var (_, result) in results)
            {
                if (result.Err != null)
                {
                    t.Errorf("error analyzing %s: %v", result.Pass, result.Err);
                }
                else
                {
                    check(t, dir, _addr_result.Pass, result.Diagnostics, result.Facts);
                }

            }
            return results;

        }

        // A Result holds the result of applying an analyzer to a package.
        public partial struct Result // : checker.TestAnalyzerResult
        {
        }

        // loadPackages uses go/packages to load a specified packages (from source, with
        // dependencies) from dir, which is the root of a GOPATH-style project
        // tree. It returns an error if any package had an error, or the pattern
        // matched no packages.
        private static (slice<ptr<packages.Package>>, error) loadPackages(@string dir, params @string[] patterns)
        {
            slice<ptr<packages.Package>> _p0 = default;
            error _p0 = default!;
            patterns = patterns.Clone();
 
            // packages.Load loads the real standard library, not a minimal
            // fake version, which would be more efficient, especially if we
            // have many small tests that import, say, net/http.
            // However there is no easy way to make go/packages to consume
            // a list of packages we generate and then do the parsing and
            // typechecking, though this feature seems to be a recurring need.

            ptr<packages.Config> cfg = addr(new packages.Config(Mode:packages.LoadAllSyntax,Dir:dir,Tests:true,Env:append(os.Environ(),"GOPATH="+dir,"GO111MODULE=off","GOPROXY=off"),));
            var (pkgs, err) = packages.Load(cfg, patterns);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Print errors but do not stop:
            // some Analyzers may be disposed to RunDespiteErrors.
            packages.PrintErrors(pkgs);

            if (len(pkgs) == 0L)
            {
                return (null, error.As(fmt.Errorf("no packages matched %s", patterns))!);
            }

            return (pkgs, error.As(null!)!);

        }

        // check inspects an analysis pass on which the analysis has already
        // been run, and verifies that all reported diagnostics and facts match
        // specified by the contents of "// want ..." comments in the package's
        // source files, which must have been parsed with comments enabled.
        private static void check(Testing t, @string gopath, ptr<analysis.Pass> _addr_pass, slice<analysis.Diagnostic> diagnostics, map<types.Object, slice<analysis.Fact>> facts)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            private partial struct key
            {
                public @string file;
                public long line;
            }

            var want = make_map<key, slice<expectation>>(); 

            // processComment parses expectations out of comments.
            Action<@string, long, @string> processComment = (filename, linenum, text) =>
            {
                text = strings.TrimSpace(text); 

                // Any comment starting with "want" is treated
                // as an expectation, even without following whitespace.
                {
                    var rest = strings.TrimPrefix(text, "want");

                    if (rest != text)
                    {
                        var (expects, err) = parseExpectations(rest);
                        if (err != null)
                        {
                            t.Errorf("%s:%d: in 'want' comment: %s", filename, linenum, err);
                            return ;
                        }

                        if (expects != null)
                        {
                            want[new key(filename,linenum)] = expects;
                        }

                    }

                }

            } 

            // Extract 'want' comments from Go files.
; 

            // Extract 'want' comments from Go files.
            {
                var f__prev1 = f;

                foreach (var (_, __f) in pass.Files)
                {
                    f = __f;
                    foreach (var (_, cgroup) in f.Comments)
                    {
                        foreach (var (_, c) in cgroup.List)
                        {
                            var text = strings.TrimPrefix(c.Text, "//");
                            if (text == c.Text)
                            { // not a //-comment.
                                text = strings.TrimPrefix(text, "/*");
                                text = strings.TrimSuffix(text, "*/");

                            } 

                            // Hack: treat a comment of the form "//...// want..."
                            // or "/*...// want... */
                            // as if it starts at 'want'.
                            // This allows us to add comments on comments,
                            // as required when testing the buildtag analyzer.
                            {
                                var i__prev1 = i;

                                var i = strings.Index(text, "// want");

                                if (i >= 0L)
                                {
                                    text = text[i + len("// ")..];
                                } 

                                // It's tempting to compute the filename
                                // once outside the loop, but it's
                                // incorrect because it can change due
                                // to //line directives.

                                i = i__prev1;

                            } 

                            // It's tempting to compute the filename
                            // once outside the loop, but it's
                            // incorrect because it can change due
                            // to //line directives.
                            var posn = pass.Fset.Position(c.Pos());
                            var filename = sanitize(gopath, posn.Filename);
                            processComment(filename, posn.Line, text);

                        }

                    }

                } 

                // Extract 'want' comments from non-Go files.
                // TODO(adonovan): we may need to handle //line directives.

                f = f__prev1;
            }

            {
                var filename__prev1 = filename;

                foreach (var (_, __filename) in pass.OtherFiles)
                {
                    filename = __filename;
                    var (data, err) = ioutil.ReadFile(filename);
                    if (err != null)
                    {
                        t.Errorf("can't read '// want' comments from %s: %v", filename, err);
                        continue;
                    }

                    filename = sanitize(gopath, filename);
                    long linenum = 0L;
                    foreach (var (_, line) in strings.Split(string(data), "\n"))
                    {
                        linenum++;
                        {
                            var i__prev1 = i;

                            i = strings.Index(line, "//");

                            if (i >= 0L)
                            {
                                line = line[i + len("//")..];
                                processComment(filename, linenum, line);
                            }

                            i = i__prev1;

                        }

                    }

                }

                filename = filename__prev1;
            }

            Action<token.Position, @string, @string, @string> checkMessage = (posn, kind, name, message) =>
            {
                posn.Filename = sanitize(gopath, posn.Filename);
                key k = new key(posn.Filename,posn.Line);
                var expects = want[k];
                slice<@string> unmatched = default;
                {
                    var i__prev1 = i;
                    var exp__prev1 = exp;

                    foreach (var (__i, __exp) in expects)
                    {
                        i = __i;
                        exp = __exp;
                        if (exp.kind == kind && exp.name == name)
                        {
                            if (exp.rx.MatchString(message))
                            { 
                                // matched: remove the expectation.
                                expects[i] = expects[len(expects) - 1L];
                                expects = expects[..len(expects) - 1L];
                                want[k] = expects;
                                return ;

                            }

                            unmatched = append(unmatched, fmt.Sprintf("%q", exp.rx));

                        }

                    }

                    i = i__prev1;
                    exp = exp__prev1;
                }

                if (unmatched == null)
                {
                    t.Errorf("%v: unexpected %s: %v", posn, kind, message);
                }
                else
                {
                    t.Errorf("%v: %s %q does not match pattern %s", posn, kind, message, strings.Join(unmatched, " or "));
                }

            } 

            // Check the diagnostics match expectations.
; 

            // Check the diagnostics match expectations.
            {
                var f__prev1 = f;

                foreach (var (_, __f) in diagnostics)
                {
                    f = __f; 
                    // TODO(matloob): Support ranges in analysistest.
                    posn = pass.Fset.Position(f.Pos);
                    checkMessage(posn, "diagnostic", "", f.Message);

                } 

                // Check the facts match expectations.
                // Report errors in lexical order for determinism.
                // (It's only deterministic within each file, not across files,
                // because go/packages does not guarantee file.Pos is ascending
                // across the files of a single compilation unit.)

                f = f__prev1;
            }

            slice<types.Object> objects = default;
            {
                var obj__prev1 = obj;

                foreach (var (__obj) in facts)
                {
                    obj = __obj;
                    objects = append(objects, obj);
                }

                obj = obj__prev1;
            }

            sort.Slice(objects, (i, j) =>
            { 
                // Package facts compare less than object facts.
                var ip = objects[i] == null;
                var jp = objects[j] == null; // whether i, j is a package fact
                if (ip != jp)
                {
                    return ip && !jp;
                }

                return objects[i].Pos() < objects[j].Pos();

            });
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in objects)
                {
                    obj = __obj;
                    posn = default;
                    @string name = default;
                    if (obj != null)
                    { 
                        // Object facts are reported on the declaring line.
                        name = obj.Name();
                        posn = pass.Fset.Position(obj.Pos());

                    }
                    else
                    { 
                        // Package facts are reported at the start of the file.
                        name = "package";
                        posn = pass.Fset.Position(pass.Files[0L].Pos());
                        posn.Line = 1L;

                    }

                    foreach (var (_, fact) in facts[obj])
                    {
                        checkMessage(posn, "fact", name, fmt.Sprint(fact));
                    }

                } 

                // Reject surplus expectations.
                //
                // Sometimes an Analyzer reports two similar diagnostics on a
                // line with only one expectation. The reader may be confused by
                // the error message.
                // TODO(adonovan): print a better error:
                // "got 2 diagnostics here; each one needs its own expectation".

                obj = obj__prev1;
            }

            slice<@string> surplus = default;
            {
                var expects__prev1 = expects;

                foreach (var (__key, __expects) in want)
                {
                    key = __key;
                    expects = __expects;
                    {
                        var exp__prev2 = exp;

                        foreach (var (_, __exp) in expects)
                        {
                            exp = __exp;
                            var err = fmt.Sprintf("%s:%d: no %s was reported matching %q", key.file, key.line, exp.kind, exp.rx);
                            surplus = append(surplus, err);
                        }

                        exp = exp__prev2;
                    }
                }

                expects = expects__prev1;
            }

            sort.Strings(surplus);
            {
                var err__prev1 = err;

                foreach (var (_, __err) in surplus)
                {
                    err = __err;
                    t.Errorf("%s", err);
                }

                err = err__prev1;
            }
        }

        private partial struct expectation
        {
            public @string kind; // either "fact" or "diagnostic"
            public @string name; // name of object to which fact belongs, or "package" ("fact" only)
            public ptr<regexp.Regexp> rx;
        }

        private static @string String(this expectation ex)
        {
            return fmt.Sprintf("%s %s:%q", ex.kind, ex.name, ex.rx); // for debugging
        }

        // parseExpectations parses the content of a "// want ..." comment
        // and returns the expectations, a mixture of diagnostics ("rx") and
        // facts (name:"rx").
        private static (slice<expectation>, error) parseExpectations(@string text)
        {
            slice<expectation> _p0 = default;
            error _p0 = default!;

            @string scanErr = default;
            ptr<object> sc = @new<scanner.Scanner>().Init(strings.NewReader(text));
            sc.Error = (s, msg) =>
            {
                scanErr = msg; // e.g. bad string escape
            }
;
            sc.Mode = scanner.ScanIdents | scanner.ScanStrings | scanner.ScanRawStrings;

            Func<int, (ptr<regexp.Regexp>, error)> scanRegexp = tok =>
            {
                if (tok != scanner.String && tok != scanner.RawString)
                {
                    return (null, error.As(fmt.Errorf("got %s, want regular expression", scanner.TokenString(tok)))!);
                }

                var (pattern, _) = strconv.Unquote(sc.TokenText()); // can't fail
                return regexp.Compile(pattern);

            }
;

            slice<expectation> expects = default;
            while (true)
            {
                var tok = sc.Scan();

                if (tok == scanner.String || tok == scanner.RawString) 
                    var (rx, err) = scanRegexp(tok);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    expects = append(expects, new expectation("diagnostic","",rx));
                else if (tok == scanner.Ident) 
                    var name = sc.TokenText();
                    tok = sc.Scan();
                    if (tok != ':')
                    {
                        return (null, error.As(fmt.Errorf("got %s after %s, want ':'", scanner.TokenString(tok), name))!);
                    }

                    tok = sc.Scan();
                    (rx, err) = scanRegexp(tok);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    expects = append(expects, new expectation("fact",name,rx));
                else if (tok == scanner.EOF) 
                    if (scanErr != "")
                    {
                        return (null, error.As(fmt.Errorf("%s", scanErr))!);
                    }

                    return (expects, error.As(null!)!);
                else 
                    return (null, error.As(fmt.Errorf("unexpected %s", scanner.TokenString(tok)))!);
                
            }


        }

        // sanitize removes the GOPATH portion of the filename,
        // typically a gnarly /tmp directory, and returns the rest.
        private static @string sanitize(@string gopath, @string filename)
        {
            var prefix = gopath + string(os.PathSeparator) + "src" + string(os.PathSeparator);
            return filepath.ToSlash(strings.TrimPrefix(filename, prefix));
        }
    }
}}}}}}
