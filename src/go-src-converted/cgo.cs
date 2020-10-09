// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cgo handles cgo preprocessing of files containing `import "C"`.
//
// DESIGN
//
// The approach taken is to run the cgo processor on the package's
// CgoFiles and parse the output, faking the filenames of the
// resulting ASTs so that the synthetic file containing the C types is
// called "C" (e.g. "~/go/src/net/C") and the preprocessed files
// have their original names (e.g. "~/go/src/net/cgo_unix.go"),
// not the names of the actual temporary files.
//
// The advantage of this approach is its fidelity to 'go build'.  The
// downside is that the token.Position.Offset for each AST node is
// incorrect, being an offset within the temporary file.  Line numbers
// should still be correct because of the //line comments.
//
// The logic of this file is mostly plundered from the 'go build'
// tool, which also invokes the cgo preprocessor.
//
//
// REJECTED ALTERNATIVE
//
// An alternative approach that we explored is to extend go/types'
// Importer mechanism to provide the identity of the importing package
// so that each time `import "C"` appears it resolves to a different
// synthetic package containing just the objects needed in that case.
// The loader would invoke cgo but parse only the cgo_types.go file
// defining the package-level objects, discarding the other files
// resulting from preprocessing.
//
// The benefit of this approach would have been that source-level
// syntax information would correspond exactly to the original cgo
// file, with no preprocessing involved, making source tools like
// godoc, guru, and eg happy.  However, the approach was rejected
// due to the additional complexity it would impose on go/types.  (It
// made for a beautiful demo, though.)
//
// cgo files, despite their *.go extension, are not legal Go source
// files per the specification since they may refer to unexported
// members of package "C" such as C.int.  Also, a function such as
// C.getpwent has in effect two types, one matching its C type and one
// which additionally returns (errno C.int).  The cgo preprocessor
// uses name mangling to distinguish these two functions in the
// processed code, but go/types would need to duplicate this logic in
// its handling of function calls, analogous to the treatment of map
// lookups in which y=m[k] and y,ok=m[k] are both legal.

// package cgo -- go2cs converted at 2020 October 09 06:03:52 UTC
// import "golang.org/x/tools/go/internal/cgo" ==> using cgo = go.golang.org.x.tools.go.@internal.cgo_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\cgo\cgo.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class cgo_package
    {
        // ProcessFiles invokes the cgo preprocessor on bp.CgoFiles, parses
        // the output and returns the resulting ASTs.
        //
        public static (slice<ptr<ast.File>>, error) ProcessFiles(ptr<build.Package> _addr_bp, ptr<token.FileSet> _addr_fset, Func<@string, @string> DisplayPath, parser.Mode mode) => func((defer, _, __) =>
        {
            slice<ptr<ast.File>> _p0 = default;
            error _p0 = default!;
            ref build.Package bp = ref _addr_bp.val;
            ref token.FileSet fset = ref _addr_fset.val;

            var (tmpdir, err) = ioutil.TempDir("", strings.Replace(bp.ImportPath, "/", "_", -1L) + "_C");
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            defer(os.RemoveAll(tmpdir));

            var pkgdir = bp.Dir;
            if (DisplayPath != null)
            {
                pkgdir = DisplayPath(pkgdir);
            }
            var (cgoFiles, cgoDisplayFiles, err) = Run(_addr_bp, pkgdir, tmpdir, false);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            slice<ptr<ast.File>> files = default;
            foreach (var (i) in cgoFiles)
            {
                var (rd, err) = os.Open(cgoFiles[i]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                var display = filepath.Join(bp.Dir, cgoDisplayFiles[i]);
                var (f, err) = parser.ParseFile(fset, display, rd, mode);
                rd.Close();
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                files = append(files, f);

            }            return (files, error.As(null!)!);

        });

        private static var cgoRe = regexp.MustCompile("[/\\\\:]");

        // Run invokes the cgo preprocessor on bp.CgoFiles and returns two
        // lists of files: the resulting processed files (in temporary
        // directory tmpdir) and the corresponding names of the unprocessed files.
        //
        // Run is adapted from (*builder).cgo in
        // $GOROOT/src/cmd/go/build.go, but these features are unsupported:
        // Objective C, CGOPKGPATH, CGO_FLAGS.
        //
        // If useabs is set to true, absolute paths of the bp.CgoFiles will be passed in
        // to the cgo preprocessor. This in turn will set the // line comments
        // referring to those files to use absolute paths. This is needed for
        // go/packages using the legacy go list support so it is able to find
        // the original files.
        public static (slice<@string>, slice<@string>, error) Run(ptr<build.Package> _addr_bp, @string pkgdir, @string tmpdir, bool useabs)
        {
            slice<@string> files = default;
            slice<@string> displayFiles = default;
            error err = default!;
            ref build.Package bp = ref _addr_bp.val;

            var (cgoCPPFLAGS, _, _, _) = cflags(_addr_bp, true);
            var (_, cgoexeCFLAGS, _, _) = cflags(_addr_bp, false);

            if (len(bp.CgoPkgConfig) > 0L)
            {
                var (pcCFLAGS, err) = pkgConfigFlags(bp);
                if (err != null)
                {
                    return (null, null, error.As(err)!);
                }

                cgoCPPFLAGS = append(cgoCPPFLAGS, pcCFLAGS);

            } 

            // Allows including _cgo_export.h from .[ch] files in the package.
            cgoCPPFLAGS = append(cgoCPPFLAGS, "-I", tmpdir); 

            // _cgo_gotypes.go (displayed "C") contains the type definitions.
            files = append(files, filepath.Join(tmpdir, "_cgo_gotypes.go"));
            displayFiles = append(displayFiles, "C");
            foreach (var (_, fn) in bp.CgoFiles)
            { 
                // "foo.cgo1.go" (displayed "foo.go") is the processed Go source.
                var f = cgoRe.ReplaceAllString(fn[..len(fn) - len("go")], "_");
                files = append(files, filepath.Join(tmpdir, f + "cgo1.go"));
                displayFiles = append(displayFiles, fn);

            }
            slice<@string> cgoflags = default;
            if (bp.Goroot && bp.ImportPath == "runtime/cgo")
            {
                cgoflags = append(cgoflags, "-import_runtime_cgo=false");
            }

            if (bp.Goroot && bp.ImportPath == "runtime/race" || bp.ImportPath == "runtime/cgo")
            {
                cgoflags = append(cgoflags, "-import_syscall=false");
            }

            slice<@string> cgoFiles = bp.CgoFiles;
            if (useabs)
            {
                cgoFiles = make_slice<@string>(len(bp.CgoFiles));
                foreach (var (i) in cgoFiles)
                {
                    cgoFiles[i] = filepath.Join(pkgdir, bp.CgoFiles[i]);
                }

            }

            var args = stringList("go", "tool", "cgo", "-objdir", tmpdir, cgoflags, "--", cgoCPPFLAGS, cgoexeCFLAGS, cgoFiles);
            if (false)
            {
                log.Printf("Running cgo for package %q: %s (dir=%s)", bp.ImportPath, args, pkgdir);
            }

            var cmd = exec.Command(args[0L], args[1L..]);
            cmd.Dir = pkgdir;
            cmd.Stdout = os.Stderr;
            cmd.Stderr = os.Stderr;
            {
                var err = cmd.Run();

                if (err != null)
                {
                    return (null, null, error.As(fmt.Errorf("cgo failed: %s: %s", args, err))!);
                }

            }


            return (files, displayFiles, error.As(null!)!);

        }

        // -- unmodified from 'go build' ---------------------------------------

        // Return the flags to use when invoking the C or C++ compilers, or cgo.
        private static (slice<@string>, slice<@string>, slice<@string>, slice<@string>) cflags(ptr<build.Package> _addr_p, bool def)
        {
            slice<@string> cppflags = default;
            slice<@string> cflags = default;
            slice<@string> cxxflags = default;
            slice<@string> ldflags = default;
            ref build.Package p = ref _addr_p.val;

            @string defaults = default;
            if (def)
            {
                defaults = "-g -O2";
            }

            cppflags = stringList(envList("CGO_CPPFLAGS", ""), p.CgoCPPFLAGS);
            cflags = stringList(envList("CGO_CFLAGS", defaults), p.CgoCFLAGS);
            cxxflags = stringList(envList("CGO_CXXFLAGS", defaults), p.CgoCXXFLAGS);
            ldflags = stringList(envList("CGO_LDFLAGS", defaults), p.CgoLDFLAGS);
            return ;

        }

        // envList returns the value of the given environment variable broken
        // into fields, using the default value when the variable is empty.
        private static slice<@string> envList(@string key, @string def)
        {
            var v = os.Getenv(key);
            if (v == "")
            {
                v = def;
            }

            return strings.Fields(v);

        }

        // stringList's arguments should be a sequence of string or []string values.
        // stringList flattens them into a single []string.
        private static slice<@string> stringList(params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            slice<@string> x = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in args)
                {
                    arg = __arg;
                    switch (arg.type())
                    {
                        case slice<@string> arg:
                            x = append(x, arg);
                            break;
                        case @string arg:
                            x = append(x, arg);
                            break;
                        default:
                        {
                            var arg = arg.type();
                            panic("stringList: invalid argument");
                            break;
                        }
                    }

                }

                arg = arg__prev1;
            }

            return x;

        });
    }
}}}}}}
