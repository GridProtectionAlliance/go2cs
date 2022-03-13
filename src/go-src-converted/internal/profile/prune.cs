// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Implements methods to remove frames from profiles.

// package profile -- go2cs converted at 2022 March 13 05:38:48 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Program Files\Go\src\internal\profile\prune.go
namespace go.@internal;

using fmt = fmt_package;
using regexp = regexp_package;


// Prune removes all nodes beneath a node matching dropRx, and not
// matching keepRx. If the root node of a Sample matches, the sample
// will have an empty stack.

public static partial class profile_package {

private static void Prune(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_dropRx, ptr<regexp.Regexp> _addr_keepRx) {
    ref Profile p = ref _addr_p.val;
    ref regexp.Regexp dropRx = ref _addr_dropRx.val;
    ref regexp.Regexp keepRx = ref _addr_keepRx.val;

    var prune = make_map<ulong, bool>();
    var pruneBeneath = make_map<ulong, bool>();

    foreach (var (_, loc) in p.Location) {
        nint i = default;
        for (i = len(loc.Line) - 1; i >= 0; i--) {
            {
                var fn = loc.Line[i].Function;

                if (fn != null && fn.Name != "") {
                    var funcName = fn.Name; 
                    // Account for leading '.' on the PPC ELF v1 ABI.
                    if (funcName[0] == '.') {
                        funcName = funcName[(int)1..];
                    }
                    if (dropRx.MatchString(funcName)) {
                        if (keepRx == null || !keepRx.MatchString(funcName)) {
                            break;
                        }
                    }
                }
            }
        }

        if (i >= 0) { 
            // Found matching entry to prune.
            pruneBeneath[loc.ID] = true; 

            // Remove the matching location.
            if (i == len(loc.Line) - 1) { 
                // Matched the top entry: prune the whole location.
                prune[loc.ID] = true;
            }
            else
 {
                loc.Line = loc.Line[(int)i + 1..];
            }
        }
    }    foreach (var (_, sample) in p.Sample) { 
        // Scan from the root to the leaves to find the prune location.
        // Do not prune frames before the first user frame, to avoid
        // pruning everything.
        var foundUser = false;
        {
            nint i__prev2 = i;

            for (i = len(sample.Location) - 1; i >= 0; i--) {
                var id = sample.Location[i].ID;
                if (!prune[id] && !pruneBeneath[id]) {
                    foundUser = true;
                    continue;
                }
                if (!foundUser) {
                    continue;
                }
                if (prune[id]) {
                    sample.Location = sample.Location[(int)i + 1..];
                    break;
                }
                if (pruneBeneath[id]) {
                    sample.Location = sample.Location[(int)i..];
                    break;
                }
            }

            i = i__prev2;
        }
    }
}

// RemoveUninteresting prunes and elides profiles using built-in
// tables of uninteresting function names.
private static error RemoveUninteresting(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    ptr<regexp.Regexp> keep;    ptr<regexp.Regexp> drop;

    error err = default!;

    if (p.DropFrames != "") {
        drop, err = regexp.Compile("^(" + p.DropFrames + ")$");

        if (err != null) {
            return error.As(fmt.Errorf("failed to compile regexp %s: %v", p.DropFrames, err))!;
        }
        if (p.KeepFrames != "") {
            keep, err = regexp.Compile("^(" + p.KeepFrames + ")$");

            if (err != null) {
                return error.As(fmt.Errorf("failed to compile regexp %s: %v", p.KeepFrames, err))!;
            }
        }
        p.Prune(drop, keep);
    }
    return error.As(null!)!;
}

} // end profile_package
