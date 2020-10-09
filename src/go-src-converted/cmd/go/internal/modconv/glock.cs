// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2020 October 09 05:46:47 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Go\src\cmd\go\internal\modconv\glock.go
using strings = go.strings_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modconv_package
    {
        public static (ptr<modfile.File>, error) ParseGLOCKFILE(@string file, slice<byte> data)
        {
            ptr<modfile.File> _p0 = default!;
            error _p0 = default!;

            ptr<modfile.File> mf = @new<modfile.File>();
            foreach (var (_, line) in strings.Split(string(data), "\n"))
            {
                var f = strings.Fields(line);
                if (len(f) >= 2L && f[0L] != "cmd")
                {
                    mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:f[0],Version:f[1]})));
                }
            }            return (_addr_mf!, error.As(null!)!);

        }
    }
}}}}
