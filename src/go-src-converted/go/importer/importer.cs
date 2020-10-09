// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package importer provides access to export data importers.
// package importer -- go2cs converted at 2020 October 09 06:02:39 UTC
// import "go/importer" ==> using importer = go.go.importer_package
// Original source: C:\Go\src\go\importer\importer.go
using build = go.go.build_package;
using gccgoimporter = go.go.@internal.gccgoimporter_package;
using gcimporter = go.go.@internal.gcimporter_package;
using srcimporter = go.go.@internal.srcimporter_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class importer_package
    {
        // A Lookup function returns a reader to access package data for
        // a given import path, or an error if no matching package is found.
        public delegate  error) Lookup(@string,  (io.ReadCloser);

        // ForCompiler returns an Importer for importing from installed packages
        // for the compilers "gc" and "gccgo", or for importing directly
        // from the source if the compiler argument is "source". In this
        // latter case, importing may fail under circumstances where the
        // exported API is not entirely defined in pure Go source code
        // (if the package API depends on cgo-defined entities, the type
        // checker won't have access to those).
        //
        // The lookup function is called each time the resulting importer needs
        // to resolve an import path. In this mode the importer can only be
        // invoked with canonical import paths (not relative or absolute ones);
        // it is assumed that the translation to canonical import paths is being
        // done by the client of the importer.
        //
        // A lookup function must be provided for correct module-aware operation.
        // Deprecated: If lookup is nil, for backwards-compatibility, the importer
        // will attempt to resolve imports in the $GOPATH workspace.
        public static types.Importer ForCompiler(ptr<token.FileSet> _addr_fset, @string compiler, Lookup lookup) => func((_, panic, __) =>
        {
            ref token.FileSet fset = ref _addr_fset.val;

            switch (compiler)
            {
                case "gc": 
                    return addr(new gcimports(fset:fset,packages:make(map[string]*types.Package),lookup:lookup,));
                    break;
                case "gccgo": 
                    gccgoimporter.GccgoInstallation inst = default;
                    {
                        var err = inst.InitFromDriver("gccgo");

                        if (err != null)
                        {
                            return null;
                        }

                    }

                    return addr(new gccgoimports(packages:make(map[string]*types.Package),importer:inst.GetImporter(nil,nil),lookup:lookup,));
                    break;
                case "source": 
                    if (lookup != null)
                    {
                        panic("source importer for custom import path lookup not supported (issue #13847).");
                    }

                    return srcimporter.New(_addr_build.Default, fset, make_map<@string, ptr<types.Package>>());
                    break;
            } 

            // compiler not supported
            return null;

        });

        // For calls ForCompiler with a new FileSet.
        //
        // Deprecated: Use ForCompiler, which populates a FileSet
        // with the positions of objects created by the importer.
        public static types.Importer For(@string compiler, Lookup lookup)
        {
            return ForCompiler(_addr_token.NewFileSet(), compiler, lookup);
        }

        // Default returns an Importer for the compiler that built the running binary.
        // If available, the result implements types.ImporterFrom.
        public static types.Importer Default()
        {
            return For(runtime.Compiler, null);
        }

        // gc importer

        private partial struct gcimports
        {
            public ptr<token.FileSet> fset;
            public map<@string, ptr<types.Package>> packages;
            public Lookup lookup;
        }

        private static (ptr<types.Package>, error) Import(this ptr<gcimports> _addr_m, @string path)
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;
            ref gcimports m = ref _addr_m.val;

            return _addr_m.ImportFrom(path, "", 0L)!;
        }

        private static (ptr<types.Package>, error) ImportFrom(this ptr<gcimports> _addr_m, @string path, @string srcDir, types.ImportMode mode) => func((_, panic, __) =>
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;
            ref gcimports m = ref _addr_m.val;

            if (mode != 0L)
            {
                panic("mode must be 0");
            }

            return _addr_gcimporter.Import(m.fset, m.packages, path, srcDir, m.lookup)!;

        });

        // gccgo importer

        private partial struct gccgoimports
        {
            public map<@string, ptr<types.Package>> packages;
            public gccgoimporter.Importer importer;
            public Lookup lookup;
        }

        private static (ptr<types.Package>, error) Import(this ptr<gccgoimports> _addr_m, @string path)
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;
            ref gccgoimports m = ref _addr_m.val;

            return _addr_m.ImportFrom(path, "", 0L)!;
        }

        private static (ptr<types.Package>, error) ImportFrom(this ptr<gccgoimports> _addr_m, @string path, @string srcDir, types.ImportMode mode) => func((_, panic, __) =>
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;
            ref gccgoimports m = ref _addr_m.val;

            if (mode != 0L)
            {
                panic("mode must be 0");
            }

            return _addr_m.importer(m.packages, path, srcDir, m.lookup)!;

        });
    }
}}
