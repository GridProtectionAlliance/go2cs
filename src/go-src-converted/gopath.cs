// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packagestest -- go2cs converted at 2020 October 09 06:02:31 UTC
// import "golang.org/x/tools/go/packages/packagestest" ==> using packagestest = go.golang.org.x.tools.go.packages.packagestest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\packagestest\gopath.go
using path = go.path_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace packages
{
    public static partial class packagestest_package
    {
        // GOPATH is the exporter that produces GOPATH layouts.
        // Each "module" is put in it's own GOPATH entry to help test complex cases.
        // Given the two files
        //     golang.org/repoa#a/a.go
        //     golang.org/repob#b/b.go
        // You would get the directory layout
        //     /sometemporarydirectory
        //     ├── repoa
        //     │   └── src
        //     │       └── golang.org
        //     │           └── repoa
        //     │               └── a
        //     │                   └── a.go
        //     └── repob
        //         └── src
        //             └── golang.org
        //                 └── repob
        //                     └── b
        //                         └── b.go
        // GOPATH would be set to
        //     /sometemporarydirectory/repoa;/sometemporarydirectory/repob
        // and the working directory would be
        //     /sometemporarydirectory/repoa/src
        public static gopath GOPATH = new gopath();

        private static void init()
        {
            All = append(All, GOPATH);
        }

        private partial struct gopath
        {
        }

        private static @string Name(this gopath _p0)
        {
            return "GOPATH";
        }

        private static @string Filename(this gopath _p0, ptr<Exported> _addr_exported, @string module, @string fragment)
        {
            ref Exported exported = ref _addr_exported.val;

            return filepath.Join(gopathDir(_addr_exported, module), "src", module, fragment);
        }

        private static error Finalize(this gopath _p0, ptr<Exported> _addr_exported)
        {
            ref Exported exported = ref _addr_exported.val;

            exported.Config.Env = append(exported.Config.Env, "GO111MODULE=off");
            @string gopath = "";
            foreach (var (module) in exported.written)
            {
                if (gopath != "")
                {
                    gopath += string(filepath.ListSeparator);
                }

                var dir = gopathDir(_addr_exported, module);
                gopath += dir;
                if (module == exported.primary)
                {
                    exported.Config.Dir = filepath.Join(dir, "src");
                }

            }
            exported.Config.Env = append(exported.Config.Env, "GOPATH=" + gopath);
            return error.As(null!)!;

        }

        private static @string gopathDir(ptr<Exported> _addr_exported, @string module)
        {
            ref Exported exported = ref _addr_exported.val;

            var dir = path.Base(module);
            if (versionSuffixRE.MatchString(dir))
            {
                dir = path.Base(path.Dir(module)) + "_" + dir;
            }

            return filepath.Join(exported.temp, dir);

        }
    }
}}}}}}
