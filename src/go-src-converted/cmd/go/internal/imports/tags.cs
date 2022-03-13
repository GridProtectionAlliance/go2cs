// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package imports -- go2cs converted at 2022 March 13 06:30:13 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\imports\tags.go
namespace go.cmd.go.@internal;

using cfg = cmd.go.@internal.cfg_package;
using sync = sync_package;
using System;

public static partial class imports_package {

private static map<@string, bool> tags = default;private static sync.Once tagsOnce = default;

// Tags returns a set of build tags that are true for the target platform.
// It includes GOOS, GOARCH, the compiler, possibly "cgo",
// release tags like "go1.13", and user-specified build tags.
public static map<@string, bool> Tags() {
    tagsOnce.Do(() => {
        tags = loadTags();
    });
    return tags;
}

private static map<@string, bool> loadTags() {
    map tags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{cfg.BuildContext.GOOS:true,cfg.BuildContext.GOARCH:true,cfg.BuildContext.Compiler:true,};
    if (cfg.BuildContext.CgoEnabled) {
        tags["cgo"] = true;
    }
    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in cfg.BuildContext.BuildTags) {
            tag = __tag;
            tags[tag] = true;
        }
        tag = tag__prev1;
    }

    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in cfg.BuildContext.ReleaseTags) {
            tag = __tag;
            tags[tag] = true;
        }
        tag = tag__prev1;
    }

    return tags;
}

private static map<@string, bool> anyTags = default;private static sync.Once anyTagsOnce = default;

// AnyTags returns a special set of build tags that satisfy nearly all
// build tag expressions. Only "ignore" and malformed build tag requirements
// are considered false.
public static map<@string, bool> AnyTags() {
    anyTagsOnce.Do(() => {
        anyTags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"*":true};
    });
    return anyTags;
}

} // end imports_package
