// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2020 October 09 05:46:47 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Go\src\cmd\go\internal\modconv\modconv.go
using modfile = go.golang.org.x.mod.modfile_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modconv_package
    {
        public static map Converters = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Func<@string, slice<byte>, (ptr<modfile.File>, error)>>{"GLOCKFILE":ParseGLOCKFILE,"Godeps/Godeps.json":ParseGodepsJSON,"Gopkg.lock":ParseGopkgLock,"dependencies.tsv":ParseDependenciesTSV,"glide.lock":ParseGlideLock,"vendor.conf":ParseVendorConf,"vendor.yml":ParseVendorYML,"vendor/manifest":ParseVendorManifest,"vendor/vendor.json":ParseVendorJSON,};
    }
}}}}
