// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 13 06:34:22 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\errors.go
namespace go.cmd.link.@internal;

using obj = cmd.@internal.obj_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using sync = sync_package;
using System;

public static partial class ld_package {

private partial struct unresolvedSymKey {
    public loader.Sym from; // Symbol that referenced unresolved "to"
    public loader.Sym to; // Unresolved symbol referenced by "from"
}

public delegate  @string symNameFn(loader.Sym);

// ErrorReporter is used to make error reporting thread safe.
public partial struct ErrorReporter {
    public ref loader.ErrorReporter ErrorReporter => ref ErrorReporter_val;
    public sync.Once unresOnce;
    public map<unresolvedSymKey, bool> unresSyms;
    public sync.Mutex unresMutex;
    public symNameFn SymName;
}

// errorUnresolved prints unresolved symbol error for rs that is referenced from s.
private static void errorUnresolved(this ptr<ErrorReporter> _addr_reporter, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.Sym rs) => func((defer, _, _) => {
    ref ErrorReporter reporter = ref _addr_reporter.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    reporter.unresOnce.Do(() => {
        reporter.unresSyms = make_map<unresolvedSymKey, bool>();
    });

    unresolvedSymKey k = new unresolvedSymKey(from:s,to:rs);
    reporter.unresMutex.Lock();
    defer(reporter.unresMutex.Unlock());
    if (!reporter.unresSyms[k]) {
        reporter.unresSyms[k] = true;
        var name = ldr.SymName(rs); 

        // Try to find symbol under another ABI.
        obj.ABI reqABI = default;        obj.ABI haveABI = default;

        haveABI = ~obj.ABI(0);
        var (reqABI, ok) = sym.VersionToABI(ldr.SymVersion(rs));
        if (ok) {
            for (var abi = obj.ABI(0); abi < obj.ABICount; abi++) {
                var v = sym.ABIToVersion(abi);
                if (v == -1) {
                    continue;
                }
                {
                    var rs1 = ldr.Lookup(name, v);

                    if (rs1 != 0 && ldr.SymType(rs1) != sym.Sxxx && ldr.SymType(rs1) != sym.SXREF) {
                        haveABI = abi;
                    }

                }
            }
        }
        if (name == "main.main") {
            reporter.Errorf(s, "function main is undeclared in the main package");
        }
        else if (haveABI != ~obj.ABI(0)) {
            reporter.Errorf(s, "relocation target %s not defined for %s (but is defined for %s)", name, reqABI, haveABI);
        }
        else
 {
            reporter.Errorf(s, "relocation target %s not defined", name);
        }
    }
});

} // end ld_package
