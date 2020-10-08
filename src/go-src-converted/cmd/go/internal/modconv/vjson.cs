// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2020 October 08 04:35:29 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Go\src\cmd\go\internal\modconv\vjson.go
using json = go.encoding.json_package;

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
        public static (ptr<modfile.File>, error) ParseVendorJSON(@string file, slice<byte> data)
        {
            ptr<modfile.File> _p0 = default!;
            error _p0 = default!;

            ref var cfg = ref heap(out ptr<var> _addr_cfg);
            {
                var err = json.Unmarshal(data, _addr_cfg);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }
            }

            ptr<modfile.File> mf = @new<modfile.File>();
            foreach (var (_, d) in cfg.Package)
            {
                mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:d.Path,Version:d.Revision})));
            }            return (_addr_mf!, error.As(null!)!);

        }
    }
}}}}
