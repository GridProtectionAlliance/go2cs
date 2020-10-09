// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package list -- go2cs converted at 2020 October 09 05:45:15 UTC
// import "cmd/go/internal/list" ==> using list = go.cmd.go.@internal.list_package
// Original source: C:\Go\src\cmd\go\internal\list\context.go
using build = go.go.build_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class list_package
    {
        public partial struct Context
        {
            [Description("json:\",omitempty\"")]
            public @string GOARCH; // target architecture
            [Description("json:\",omitempty\"")]
            public @string GOOS; // target operating system
            [Description("json:\",omitempty\"")]
            public @string GOROOT; // Go root
            [Description("json:\",omitempty\"")]
            public @string GOPATH; // Go path
            [Description("json:\",omitempty\"")]
            public bool CgoEnabled; // whether cgo can be used
            [Description("json:\",omitempty\"")]
            public bool UseAllFiles; // use files regardless of +build lines, file names
            [Description("json:\",omitempty\"")]
            public @string Compiler; // compiler to assume when computing target paths
            [Description("json:\",omitempty\"")]
            public slice<@string> BuildTags; // build constraints to match in +build lines
            [Description("json:\",omitempty\"")]
            public slice<@string> ReleaseTags; // releases the current release is compatible with
            [Description("json:\",omitempty\"")]
            public @string InstallSuffix; // suffix to use in the name of the install dir
        }

        private static ptr<Context> newContext(ptr<build.Context> _addr_c)
        {
            ref build.Context c = ref _addr_c.val;

            return addr(new Context(GOARCH:c.GOARCH,GOOS:c.GOOS,GOROOT:c.GOROOT,GOPATH:c.GOPATH,CgoEnabled:c.CgoEnabled,UseAllFiles:c.UseAllFiles,Compiler:c.Compiler,BuildTags:c.BuildTags,ReleaseTags:c.ReleaseTags,InstallSuffix:c.InstallSuffix,));
        }
    }
}}}}
