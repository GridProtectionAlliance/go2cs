// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:03:17 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\deadcode.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // deadcode marks all reachable symbols.
        //
        // The basis of the dead code elimination is a flood fill of symbols,
        // following their relocations, beginning at *flagEntrySymbol.
        //
        // This flood fill is wrapped in logic for pruning unused methods.
        // All methods are mentioned by relocations on their receiver's *rtype.
        // These relocations are specially defined as R_METHODOFF by the compiler
        // so we can detect and manipulated them here.
        //
        // There are three ways a method of a reachable type can be invoked:
        //
        //    1. direct call
        //    2. through a reachable interface type
        //    3. reflect.Value.Call, .Method, or reflect.Method.Func
        //
        // The first case is handled by the flood fill, a directly called method
        // is marked as reachable.
        //
        // The second case is handled by decomposing all reachable interface
        // types into method signatures. Each encountered method is compared
        // against the interface method signatures, if it matches it is marked
        // as reachable. This is extremely conservative, but easy and correct.
        //
        // The third case is handled by looking to see if any of:
        //    - reflect.Value.Call is reachable
        //    - reflect.Value.Method is reachable
        //     - reflect.Type.Method or MethodByName is called.
        // If any of these happen, all bets are off and all exported methods
        // of reachable types are marked reachable.
        //
        // Any unreached text symbols are removed from ctxt.Textp.
        private static void deadcode(ref Link ctxt)
        {
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f deadcode\n", Cputime());
            }
            deadcodepass d = ref new deadcodepass(ctxt:ctxt,ifaceMethod:make(map[methodsig]bool),); 

            // First, flood fill any symbols directly reachable in the call
            // graph from *flagEntrySymbol. Ignore all methods not directly called.
            d.init();
            d.flood();

            var callSym = ctxt.Syms.ROLookup("reflect.Value.Call", 0L);
            var methSym = ctxt.Syms.ROLookup("reflect.Value.Method", 0L);
            var reflectSeen = false;

            if (ctxt.DynlinkingGo())
            { 
                // Exported methods may satisfy interfaces we don't know
                // about yet when dynamically linking.
                reflectSeen = true;
            }
            while (true)
            {
                if (!reflectSeen)
                {
                    if (d.reflectMethod || (callSym != null && callSym.Attr.Reachable()) || (methSym != null && methSym.Attr.Reachable()))
                    { 
                        // Methods might be called via reflection. Give up on
                        // static analysis, mark all exported methods of
                        // all reachable types as reachable.
                        reflectSeen = true;
                    }
                }
                slice<methodref> rem = default;
                {
                    var m__prev2 = m;

                    foreach (var (_, __m) in d.markableMethods)
                    {
                        m = __m;
                        if ((reflectSeen && m.isExported()) || d.ifaceMethod[m.m])
                        {
                            d.markMethod(m);
                        }
                        else
                        {
                            rem = append(rem, m);
                        }
                    }
                    m = m__prev2;
                }

                d.markableMethods = rem;

                if (len(d.markQueue) == 0L)
                { 
                    // No new work was discovered. Done.
                    break;
                }
                d.flood();
            } 

            // Remove all remaining unreached R_METHODOFF relocations.
            {
                var m__prev1 = m;

                foreach (var (_, __m) in d.markableMethods)
                {
                    m = __m;
                    foreach (var (_, r) in m.r)
                    {
                        d.cleanupReloc(r);
                    }
                }
                m = m__prev1;
            }

            if (ctxt.BuildMode != BuildModeShared)
            { 
                // Keep a itablink if the symbol it points at is being kept.
                // (When BuildModeShared, always keep itablinks.)
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        if (strings.HasPrefix(s.Name, "go.itablink."))
                        {
                            s.Attr.Set(sym.AttrReachable, len(s.R) == 1L && s.R[0L].Sym.Attr.Reachable());
                        }
                    }
                    s = s__prev1;
                }

            }
            foreach (var (_, lib) in ctxt.Library)
            {
                lib.Textp = lib.Textp[..0L];
            }            var textp = make_slice<ref sym.Symbol>(0L, len(ctxt.Textp));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (s.Attr.Reachable())
                    {
                        if (s.Lib != null)
                        {
                            s.Lib.Textp = append(s.Lib.Textp, s);
                        }
                        textp = append(textp, s);
                    }
                }
                s = s__prev1;
            }

            ctxt.Textp = textp;
        }

        // methodref holds the relocations from a receiver type symbol to its
        // method. There are three relocations, one for each of the fields in
        // the reflect.method struct: mtyp, ifn, and tfn.
        private partial struct methodref
        {
            public methodsig m;
            public ptr<sym.Symbol> src; // receiver type symbol
            public array<ref sym.Reloc> r; // R_METHODOFF relocations to fields of runtime.method
        }

        private static ref sym.Symbol ifn(this methodref m)
        {
            return m.r[1L].Sym;
        }

        private static bool isExported(this methodref m) => func((_, panic, __) =>
        {
            foreach (var (_, r) in m.m)
            {
                return unicode.IsUpper(r);
            }
            panic("methodref has no signature");
        });

        // deadcodepass holds state for the deadcode flood fill.
        private partial struct deadcodepass
        {
            public ptr<Link> ctxt;
            public slice<ref sym.Symbol> markQueue; // symbols to flood fill in next pass
            public map<methodsig, bool> ifaceMethod; // methods declared in reached interfaces
            public slice<methodref> markableMethods; // methods of reached types
            public bool reflectMethod;
        }

        private static void cleanupReloc(this ref deadcodepass d, ref sym.Reloc r)
        {
            if (r.Sym.Attr.Reachable())
            {
                r.Type = objabi.R_ADDROFF;
            }
            else
            {
                if (d.ctxt.Debugvlog > 1L)
                {
                    d.ctxt.Logf("removing method %s\n", r.Sym.Name);
                }
                r.Sym = null;
                r.Siz = 0L;
            }
        }

        // mark appends a symbol to the mark queue for flood filling.
        private static void mark(this ref deadcodepass d, ref sym.Symbol s, ref sym.Symbol parent)
        {
            if (s == null || s.Attr.Reachable())
            {
                return;
            }
            if (s.Attr.ReflectMethod())
            {
                d.reflectMethod = true;
            }
            if (flagDumpDep.Value)
            {
                @string p = "_";
                if (parent != null)
                {
                    p = parent.Name;
                }
                fmt.Printf("%s -> %s\n", p, s.Name);
            }
            s.Attr |= sym.AttrReachable;
            s.Reachparent = parent;
            d.markQueue = append(d.markQueue, s);
        }

        // markMethod marks a method as reachable.
        private static void markMethod(this ref deadcodepass d, methodref m)
        {
            foreach (var (_, r) in m.r)
            {
                d.mark(r.Sym, m.src);
                r.Type = objabi.R_ADDROFF;
            }
        }

        // init marks all initial symbols as reachable.
        // In a typical binary, this is *flagEntrySymbol.
        private static void init(this ref deadcodepass d)
        {
            slice<@string> names = default;

            if (d.ctxt.Arch.Family == sys.ARM)
            { 
                // mark some functions that are only referenced after linker code editing
                names = append(names, "runtime.read_tls_fallback");
            }
            if (d.ctxt.BuildMode == BuildModeShared)
            { 
                // Mark all symbols defined in this library as reachable when
                // building a shared library.
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in d.ctxt.Syms.Allsym)
                    {
                        s = __s;
                        if (s.Type != 0L && s.Type != sym.SDYNIMPORT)
                        {
                            d.mark(s, null);
                        }
                    }
            else

                    s = s__prev1;
                }

            }            { 
                // In a normal binary, start at main.main and the init
                // functions and mark what is reachable from there.

                if (d.ctxt.linkShared && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE))
                {
                    names = append(names, "main.main", "main.init");
                }
                else
                { 
                    // The external linker refers main symbol directly.
                    if (d.ctxt.LinkMode == LinkExternal && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE))
                    {
                        if (d.ctxt.HeadType == objabi.Hwindows && d.ctxt.Arch.Family == sys.I386)
                        {
                            flagEntrySymbol.Value = "_main";
                        }
                        else
                        {
                            flagEntrySymbol.Value = "main";
                        }
                    }
                    names = append(names, flagEntrySymbol.Value);
                    if (d.ctxt.BuildMode == BuildModePlugin)
                    {
                        names = append(names, objabi.PathToPrefix(flagPluginPath.Value) + ".init", objabi.PathToPrefix(flagPluginPath.Value) + ".main", "go.plugin.tabs"); 

                        // We don't keep the go.plugin.exports symbol,
                        // but we do keep the symbols it refers to.
                        var exports = d.ctxt.Syms.ROLookup("go.plugin.exports", 0L);
                        if (exports != null)
                        {
                            foreach (var (_, r) in exports.R)
                            {
                                d.mark(r.Sym, null);
                            }
                        }
                    }
                }
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in dynexp)
                    {
                        s = __s;
                        d.mark(s, null);
                    }

                    s = s__prev1;
                }

            }
            foreach (var (_, name) in names)
            {
                d.mark(d.ctxt.Syms.ROLookup(name, 0L), null);
            }
        }

        // flood fills symbols reachable from the markQueue symbols.
        // As it goes, it collects methodref and interface method declarations.
        private static void flood(this ref deadcodepass _d) => func(_d, (ref deadcodepass d, Defer _, Panic panic, Recover __) =>
        {
            while (len(d.markQueue) > 0L)
            {
                var s = d.markQueue[0L];
                d.markQueue = d.markQueue[1L..];
                if (s.Type == sym.STEXT)
                {
                    if (d.ctxt.Debugvlog > 1L)
                    {
                        d.ctxt.Logf("marktext %s\n", s.Name);
                    }
                    if (s.FuncInfo != null)
                    {
                        foreach (var (_, a) in s.FuncInfo.Autom)
                        {
                            d.mark(a.Gotype, s);
                        }
                    }
                }
                if (strings.HasPrefix(s.Name, "type.") && s.Name[5L] != '.')
                {
                    if (len(s.P) == 0L)
                    { 
                        // Probably a bug. The undefined symbol check
                        // later will give a better error than deadcode.
                        continue;
                    }
                    if (decodetypeKind(d.ctxt.Arch, s) & kindMask == kindInterface)
                    {
                        foreach (var (_, sig) in decodeIfaceMethods(d.ctxt.Arch, s))
                        {
                            if (d.ctxt.Debugvlog > 1L)
                            {
                                d.ctxt.Logf("reached iface method: %s\n", sig);
                            }
                            d.ifaceMethod[sig] = true;
                        }
                    }
                }
                long mpos = 0L; // 0-3, the R_METHODOFF relocs of runtime.uncommontype
                slice<methodref> methods = default;
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < len(s.R); i++)
                    {
                        var r = ref s.R[i];
                        if (r.Sym == null)
                        {
                            continue;
                        }
                        if (r.Type == objabi.R_WEAKADDROFF)
                        { 
                            // An R_WEAKADDROFF relocation is not reason
                            // enough to mark the pointed-to symbol as
                            // reachable.
                            continue;
                        }
                        if (r.Type != objabi.R_METHODOFF)
                        {
                            d.mark(r.Sym, s);
                            continue;
                        } 
                        // Collect rtype pointers to methods for
                        // later processing in deadcode.
                        if (mpos == 0L)
                        {
                            methodref m = new methodref(src:s);
                            m.r[0L] = r;
                            methods = append(methods, m);
                        }
                        else
                        {
                            methods[len(methods) - 1L].r[mpos] = r;
                        }
                        mpos++;
                        if (mpos == len(new methodref().r))
                        {
                            mpos = 0L;
                        }
                    }


                    i = i__prev2;
                }
                if (len(methods) > 0L)
                { 
                    // Decode runtime type information for type methods
                    // to help work out which methods can be called
                    // dynamically via interfaces.
                    var methodsigs = decodetypeMethods(d.ctxt.Arch, s);
                    if (len(methods) != len(methodsigs))
                    {
                        panic(fmt.Sprintf("%q has %d method relocations for %d methods", s.Name, len(methods), len(methodsigs)));
                    }
                    {
                        long i__prev2 = i;
                        methodref m__prev2 = m;

                        foreach (var (__i, __m) in methodsigs)
                        {
                            i = __i;
                            m = __m;
                            var name = string(m);
                            name = name[..strings.Index(name, "(")];
                            if (!strings.HasSuffix(methods[i].ifn().Name, name))
                            {
                                panic(fmt.Sprintf("%q relocation for %q does not match method %q", s.Name, methods[i].ifn().Name, name));
                            }
                            methods[i].m = m;
                        }

                        i = i__prev2;
                        m = m__prev2;
                    }

                    d.markableMethods = append(d.markableMethods, methods);
                }
                if (s.FuncInfo != null)
                {
                    {
                        long i__prev2 = i;

                        foreach (var (__i) in s.FuncInfo.Funcdata)
                        {
                            i = __i;
                            d.mark(s.FuncInfo.Funcdata[i], s);
                        }

                        i = i__prev2;
                    }

                }
                d.mark(s.Gotype, s);
                d.mark(s.Sub, s);
                d.mark(s.Outer, s);
            }

        });
    }
}}}}
