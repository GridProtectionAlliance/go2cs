// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mvs -- go2cs converted at 2022 March 06 23:18:00 UTC
// import "cmd/go/internal/mvs" ==> using mvs = go.cmd.go.@internal.mvs_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\mvs\errors.go
using fmt = go.fmt_package;
using strings = go.strings_package;

using module = go.golang.org.x.mod.module_package;
using System;


namespace go.cmd.go.@internal;

public static partial class mvs_package {

    // BuildListError decorates an error that occurred gathering requirements
    // while constructing a build list. BuildListError prints the chain
    // of requirements to the module where the error occurred.
public partial struct BuildListError {
    public error Err;
    public slice<buildListErrorElem> stack;
}

private partial struct buildListErrorElem {
    public module.Version m; // nextReason is the reason this module depends on the next module in the
// stack. Typically either "requires", or "updating to".
    public @string nextReason;
}

// NewBuildListError returns a new BuildListError wrapping an error that
// occurred at a module found along the given path of requirements and/or
// upgrades, which must be non-empty.
//
// The isVersionChange function reports whether a path step is due to an
// explicit upgrade or downgrade (as opposed to an existing requirement in a
// go.mod file). A nil isVersionChange function indicates that none of the path
// steps are due to explicit version changes.
public static ptr<BuildListError> NewBuildListError(error err, slice<module.Version> path, Func<module.Version, module.Version, bool> isVersionChange) {
    var stack = make_slice<buildListErrorElem>(0, len(path));
    while (len(path) > 1) {
        @string reason = "requires";
        if (isVersionChange != null && isVersionChange(path[0], path[1])) {
            reason = "updating to";
        }
        stack = append(stack, new buildListErrorElem(m:path[0],nextReason:reason,));
        path = path[(int)1..];

    }
    stack = append(stack, new buildListErrorElem(m:path[0]));

    return addr(new BuildListError(Err:err,stack:stack,));

}

// Module returns the module where the error occurred. If the module stack
// is empty, this returns a zero value.
private static module.Version Module(this ptr<BuildListError> _addr_e) {
    ref BuildListError e = ref _addr_e.val;

    if (len(e.stack) == 0) {
        return new module.Version();
    }
    return e.stack[len(e.stack) - 1].m;

}

private static @string Error(this ptr<BuildListError> _addr_e) {
    ref BuildListError e = ref _addr_e.val;

    ptr<strings.Builder> b = addr(new strings.Builder());
    var stack = e.stack; 

    // Don't print modules at the beginning of the chain without a
    // version. These always seem to be the main module or a
    // synthetic module ("target@").
    while (len(stack) > 0 && stack[0].m.Version == "") {
        stack = stack[(int)1..];
    }

    if (len(stack) == 0) {
        b.WriteString(e.Err.Error());
    }
    else
 {
        foreach (var (_, elem) in stack[..(int)len(stack) - 1]) {
            fmt.Fprintf(b, "%s %s\n\t", elem.m, elem.nextReason);
        }        var m = stack[len(stack) - 1].m;
        {
            ptr<module.ModuleError> (mErr, ok) = e.Err._<ptr<module.ModuleError>>();

            if (ok) {
                module.Version actual = new module.Version(Path:mErr.Path,Version:mErr.Version);
                {
                    ptr<module.InvalidVersionError> (v, ok) = mErr.Err._<ptr<module.InvalidVersionError>>();

                    if (ok) {
                        actual.Version = v.Version;
                    }

                }

                if (actual == m) {
                    fmt.Fprintf(b, "%v", e.Err);
                }
                else
 {
                    fmt.Fprintf(b, "%s (replaced by %s): %v", m, actual, mErr.Err);
                }

            }
            else
 {
                fmt.Fprintf(b, "%v", module.VersionError(m, e.Err));
            }

        }

    }
    return b.String();

}

} // end mvs_package
