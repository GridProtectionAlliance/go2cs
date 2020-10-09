// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// package ld -- go2cs converted at 2020 October 09 05:49:38 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\errors.go
using obj = go.cmd.@internal.obj_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private partial struct unresolvedSymKey
        {
            public loader.Sym from; // Symbol that referenced unresolved "to"
            public loader.Sym to; // Unresolved symbol referenced by "from"
        }

        private partial struct unresolvedSymKey2
        {
            public ptr<sym.Symbol> from; // Symbol that referenced unresolved "to"
            public ptr<sym.Symbol> to; // Unresolved symbol referenced by "from"
        }

        public delegate  ptr<sym.Symbol> lookupFn(@string,  long);
        public delegate  @string symNameFn(loader.Sym);

        // ErrorReporter is used to make error reporting thread safe.
        public partial struct ErrorReporter
        {
            public ref loader.ErrorReporter ErrorReporter => ref ErrorReporter_val;
            public sync.Once unresOnce;
            public map<unresolvedSymKey, bool> unresSyms;
            public map<unresolvedSymKey2, bool> unresSyms2;
            public sync.Mutex unresMutex;
            public lookupFn lookup;
            public symNameFn SymName;
        }

        // errorUnresolved prints unresolved symbol error for rs that is referenced from s.
        private static void errorUnresolved(this ptr<ErrorReporter> _addr_reporter, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.Sym rs) => func((defer, _, __) =>
        {
            ref ErrorReporter reporter = ref _addr_reporter.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            reporter.unresOnce.Do(() =>
            {
                reporter.unresSyms = make_map<unresolvedSymKey, bool>();
            });

            unresolvedSymKey k = new unresolvedSymKey(from:s,to:rs);
            reporter.unresMutex.Lock();
            defer(reporter.unresMutex.Unlock());
            if (!reporter.unresSyms[k])
            {
                reporter.unresSyms[k] = true;
                var name = ldr.SymName(rs); 

                // Try to find symbol under another ABI.
                obj.ABI reqABI = default;                obj.ABI haveABI = default;

                haveABI = ~obj.ABI(0L);
                var (reqABI, ok) = sym.VersionToABI(ldr.SymVersion(rs));
                if (ok)
                {
                    for (var abi = obj.ABI(0L); abi < obj.ABICount; abi++)
                    {
                        var v = sym.ABIToVersion(abi);
                        if (v == -1L)
                        {
                            continue;
                        }

                        {
                            var rs1 = ldr.Lookup(name, v);

                            if (rs1 != 0L && ldr.SymType(rs1) != sym.Sxxx && ldr.SymType(rs1) != sym.SXREF)
                            {
                                haveABI = abi;
                            }

                        }

                    }


                } 

                // Give a special error message for main symbol (see #24809).
                if (name == "main.main")
                {
                    reporter.Errorf(s, "function main is undeclared in the main package");
                }
                else if (haveABI != ~obj.ABI(0L))
                {
                    reporter.Errorf(s, "relocation target %s not defined for %s (but is defined for %s)", name, reqABI, haveABI);
                }
                else
                {
                    reporter.Errorf(s, "relocation target %s not defined", name);
                }

            }

        });

        // errorUnresolved2 prints unresolved symbol error for r.Sym that is referenced from s.
        private static void errorUnresolved2(this ptr<ErrorReporter> _addr_reporter, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r) => func((defer, _, __) =>
        {
            ref ErrorReporter reporter = ref _addr_reporter.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            reporter.unresOnce.Do(() =>
            {
                reporter.unresSyms2 = make_map<unresolvedSymKey2, bool>();
            });

            unresolvedSymKey2 k = new unresolvedSymKey2(from:s,to:r.Sym);
            reporter.unresMutex.Lock();
            defer(reporter.unresMutex.Unlock());
            if (!reporter.unresSyms2[k])
            {
                reporter.unresSyms2[k] = true; 

                // Try to find symbol under another ABI.
                obj.ABI reqABI = default;                obj.ABI haveABI = default;

                haveABI = ~obj.ABI(0L);
                var (reqABI, ok) = sym.VersionToABI(int(r.Sym.Version));
                if (ok)
                {
                    for (var abi = obj.ABI(0L); abi < obj.ABICount; abi++)
                    {
                        var v = sym.ABIToVersion(abi);
                        if (v == -1L)
                        {
                            continue;
                        }

                        {
                            var rs = reporter.lookup(r.Sym.Name, v);

                            if (rs != null && rs.Type != sym.Sxxx && rs.Type != sym.SXREF)
                            {
                                haveABI = abi;
                            }

                        }

                    }


                } 

                // Give a special error message for main symbol (see #24809).
                if (r.Sym.Name == "main.main")
                {
                    Errorf(s, "function main is undeclared in the main package");
                }
                else if (haveABI != ~obj.ABI(0L))
                {
                    Errorf(s, "relocation target %s not defined for %s (but is defined for %s)", r.Sym.Name, reqABI, haveABI);
                }
                else
                {
                    Errorf(s, "relocation target %s not defined", r.Sym.Name);
                }

            }

        });
    }
}}}}
