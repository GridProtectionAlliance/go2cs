// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modinfo -- go2cs converted at 2022 March 06 23:16:44 UTC
// import "cmd/go/internal/modinfo" ==> using modinfo = go.cmd.go.@internal.modinfo_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modinfo\info.go
using time = go.time_package;
using System.ComponentModel;
using System;


namespace go.cmd.go.@internal;

public static partial class modinfo_package {

    // Note that these structs are publicly visible (part of go list's API)
    // and the fields are documented in the help text in ../list/list.go
public partial struct ModulePublic {
    [Description("json:\",omitempty\"")]
    public @string Path; // module path
    [Description("json:\",omitempty\"")]
    public @string Version; // module version
    [Description("json:\",omitempty\"")]
    public slice<@string> Versions; // available module versions
    [Description("json:\",omitempty\"")]
    public ptr<ModulePublic> Replace; // replaced by this module
    [Description("json:\",omitempty\"")]
    public ptr<time.Time> Time; // time version was created
    [Description("json:\",omitempty\"")]
    public ptr<ModulePublic> Update; // available update (with -u)
    [Description("json:\",omitempty\"")]
    public bool Main; // is this the main module?
    [Description("json:\",omitempty\"")]
    public bool Indirect; // module is only indirectly needed by main module
    [Description("json:\",omitempty\"")]
    public @string Dir; // directory holding local copy of files, if any
    [Description("json:\",omitempty\"")]
    public @string GoMod; // path to go.mod file describing module, if any
    [Description("json:\",omitempty\"")]
    public @string GoVersion; // go version used in module
    [Description("json:\",omitempty\"")]
    public slice<@string> Retracted; // retraction information, if any (with -retracted or -u)
    [Description("json:\",omitempty\"")]
    public @string Deprecated; // deprecation message, if any (with -u)
    [Description("json:\",omitempty\"")]
    public ptr<ModuleError> Error; // error loading module
}

public partial struct ModuleError {
    public @string Err; // error text
}

private static @string String(this ptr<ModulePublic> _addr_m) {
    ref ModulePublic m = ref _addr_m.val;

    var s = m.Path;
    Func<ptr<ModulePublic>, @string> versionString = mm => {
        var v = mm.Version;
        if (len(mm.Retracted) == 0) {
            return v;
        }
        return v + " (retracted)";

    };

    if (m.Version != "") {
        s += " " + versionString(m);
        if (m.Update != null) {
            s += " [" + versionString(m.Update) + "]";
        }
    }
    if (m.Deprecated != "") {
        s += " (deprecated)";
    }
    if (m.Replace != null) {
        s += " => " + m.Replace.Path;
        if (m.Replace.Version != "") {
            s += " " + versionString(m.Replace);
            if (m.Replace.Update != null) {
                s += " [" + versionString(m.Replace.Update) + "]";
            }
        }
        if (m.Replace.Deprecated != "") {
            s += " (deprecated)";
        }
    }
    return s;

}

} // end modinfo_package
