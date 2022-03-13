// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:34 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\modconv.go
namespace go.cmd.go.@internal;

using modfile = golang.org.x.mod.modfile_package;
using System;

public static partial class modconv_package {

public static map Converters = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Func<@string, slice<byte>, (ptr<modfile.File>, error)>>{"GLOCKFILE":ParseGLOCKFILE,"Godeps/Godeps.json":ParseGodepsJSON,"Gopkg.lock":ParseGopkgLock,"dependencies.tsv":ParseDependenciesTSV,"glide.lock":ParseGlideLock,"vendor.conf":ParseVendorConf,"vendor.yml":ParseVendorYML,"vendor/manifest":ParseVendorManifest,"vendor/vendor.json":ParseVendorJSON,};

} // end modconv_package
