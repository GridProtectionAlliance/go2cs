// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:38:18 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\deadcode.go
using bytes = go.bytes_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using heap = go.container.heap_package;
using fmt = go.fmt_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static var _ = fmt.Print;

        private partial struct workQueue // : slice<loader.Sym>
        {
        }

        // Implement container/heap.Interface.
        private static long Len(this ptr<workQueue> _addr_q)
        {
            ref workQueue q = ref _addr_q.val;

            return len(q.val);
        }
        private static bool Less(this ptr<workQueue> _addr_q, long i, long j)
        {
            ref workQueue q = ref _addr_q.val;

            return (q.val)[i] < (q.val)[j];
        }
        private static void Swap(this ptr<workQueue> _addr_q, long i, long j)
        {
            ref workQueue q = ref _addr_q.val;

            (q.val)[i] = (q.val)[j];
            (q.val)[j] = (q.val)[i];
        }
        private static void Push(this ptr<workQueue> _addr_q, object i)
        {
            ref workQueue q = ref _addr_q.val;

            q.val = append(q.val, i._<loader.Sym>());
        }
        private static void Pop(this ptr<workQueue> _addr_q)
        {
            ref workQueue q = ref _addr_q.val;

            var i = (q.val)[len(q.val) - 1L];

            q.val = (q.val)[..len(q.val) - 1L];

            return i;
        }

        // Functions for deadcode pass to use.
        // Deadcode pass should call push/pop, not Push/Pop.
        private static void push(this ptr<workQueue> _addr_q, loader.Sym i)
        {
            ref workQueue q = ref _addr_q.val;

            heap.Push(q, i);
        }
        private static loader.Sym pop(this ptr<workQueue> _addr_q)
        {
            ref workQueue q = ref _addr_q.val;

            return heap.Pop(q)._<loader.Sym>();
        }
        private static bool empty(this ptr<workQueue> _addr_q)
        {
            ref workQueue q = ref _addr_q.val;

            return len(q.val) == 0L;
        }

        private partial struct deadcodePass
        {
            public ptr<Link> ctxt;
            public ptr<loader.Loader> ldr;
            public workQueue wq;
            public map<methodsig, bool> ifaceMethod; // methods declared in reached interfaces
            public slice<methodref> markableMethods; // methods of reached types
            public bool reflectSeen; // whether we have seen a reflect method call
        }

        private static void init(this ptr<deadcodePass> _addr_d)
        {
            ref deadcodePass d = ref _addr_d.val;

            d.ldr.InitReachable();
            d.ifaceMethod = make_map<methodsig, bool>();
            if (objabi.Fieldtrack_enabled != 0L)
            {
                d.ldr.Reachparent = make_slice<loader.Sym>(d.ldr.NSym());
            }

            heap.Init(_addr_d.wq);

            if (d.ctxt.BuildMode == BuildModeShared)
            { 
                // Mark all symbols defined in this library as reachable when
                // building a shared library.
                var n = d.ldr.NDef();
                {
                    long i__prev1 = i;

                    for (long i = 1L; i < n; i++)
                    {
                        var s = loader.Sym(i);
                        d.mark(s, 0L);
                    }


                    i = i__prev1;
                }
                return ;

            }

            slice<@string> names = default; 

            // In a normal binary, start at main.main and the init
            // functions and mark what is reachable from there.
            if (d.ctxt.linkShared && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE))
            {
                names = append(names, "main.main", "main..inittask");
            }
            else
            { 
                // The external linker refers main symbol directly.
                if (d.ctxt.LinkMode == LinkExternal && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE))
                {
                    if (d.ctxt.HeadType == objabi.Hwindows && d.ctxt.Arch.Family == sys.I386)
                    {
                        flagEntrySymbol.val = "_main";
                    }
                    else
                    {
                        flagEntrySymbol.val = "main";
                    }

                }

                names = append(names, flagEntrySymbol.val);
                if (d.ctxt.BuildMode == BuildModePlugin)
                {
                    names = append(names, objabi.PathToPrefix(flagPluginPath.val) + "..inittask", objabi.PathToPrefix(flagPluginPath.val) + ".main", "go.plugin.tabs"); 

                    // We don't keep the go.plugin.exports symbol,
                    // but we do keep the symbols it refers to.
                    var exportsIdx = d.ldr.Lookup("go.plugin.exports", 0L);
                    if (exportsIdx != 0L)
                    {
                        var relocs = d.ldr.Relocs(exportsIdx);
                        {
                            long i__prev1 = i;

                            for (i = 0L; i < relocs.Count(); i++)
                            {
                                d.mark(relocs.At2(i).Sym(), 0L);
                            }


                            i = i__prev1;
                        }

                    }

                }

            }

            var dynexpMap = d.ctxt.cgo_export_dynamic;
            if (d.ctxt.LinkMode == LinkExternal)
            {
                dynexpMap = d.ctxt.cgo_export_static;
            }

            foreach (var (exp) in dynexpMap)
            {
                names = append(names, exp);
            }
            foreach (var (_, name) in names)
            { 
                // Mark symbol as a data/ABI0 symbol.
                d.mark(d.ldr.Lookup(name, 0L), 0L); 
                // Also mark any Go functions (internal ABI).
                d.mark(d.ldr.Lookup(name, sym.SymVerABIInternal), 0L);

            }

        }

        private static void flood(this ptr<deadcodePass> _addr_d) => func((_, panic, __) =>
        {
            ref deadcodePass d = ref _addr_d.val;

            while (!d.wq.empty())
            {
                var symIdx = d.wq.pop();

                d.reflectSeen = d.reflectSeen || d.ldr.IsReflectMethod(symIdx);

                var isgotype = d.ldr.IsGoType(symIdx);
                ref var relocs = ref heap(d.ldr.Relocs(symIdx), out ptr<var> _addr_relocs);

                if (isgotype)
                {
                    var p = d.ldr.Data(symIdx);
                    if (len(p) != 0L && decodetypeKind(d.ctxt.Arch, p) & kindMask == kindInterface)
                    {
                        foreach (var (_, sig) in d.decodeIfaceMethods(d.ldr, d.ctxt.Arch, symIdx, _addr_relocs))
                        {
                            if (d.ctxt.Debugvlog > 1L)
                            {
                                d.ctxt.Logf("reached iface method: %s\n", sig);
                            }

                            d.ifaceMethod[sig] = true;

                        }

                    }

                }

                slice<methodref> methods = default;
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < relocs.Count(); i++)
                    {
                        var r = relocs.At2(i);
                        var t = r.Type();
                        if (t == objabi.R_WEAKADDROFF)
                        {
                            continue;
                        }

                        if (t == objabi.R_METHODOFF)
                        {
                            if (i + 2L >= relocs.Count())
                            {
                                panic("expect three consecutive R_METHODOFF relocs");
                            }

                            methods = append(methods, new methodref(src:symIdx,r:i));
                            i += 2L;
                            continue;

                        }

                        if (t == objabi.R_USETYPE)
                        { 
                            // type symbol used for DWARF. we need to load the symbol but it may not
                            // be otherwise reachable in the program.
                            // do nothing for now as we still load all type symbols.
                            continue;

                        }

                        d.mark(r.Sym(), symIdx);

                    }


                    i = i__prev2;
                }
                var naux = d.ldr.NAux(symIdx);
                {
                    long i__prev2 = i;

                    for (i = 0L; i < naux; i++)
                    {
                        var a = d.ldr.Aux2(symIdx, i);
                        if (a.Type() == goobj2.AuxGotype && !d.ctxt.linkShared)
                        { 
                            // A symbol being reachable doesn't imply we need its
                            // type descriptor. Don't mark it.
                            // TODO: when -linkshared, the GCProg generation code
                            // seems to need it. I'm not sure why. I think it could
                            // just reach to the type descriptor's data without
                            // requiring to mark it reachable.
                            continue;

                        }

                        d.mark(a.Sym(), symIdx);

                    } 
                    // Some host object symbols have an outer object, which acts like a
                    // "carrier" symbol, or it holds all the symbols for a particular
                    // section. We need to mark all "referenced" symbols from that carrier,
                    // so we make sure we're pulling in all outer symbols, and their sub
                    // symbols. This is not ideal, and these carrier/section symbols could
                    // be removed.


                    i = i__prev2;
                } 
                // Some host object symbols have an outer object, which acts like a
                // "carrier" symbol, or it holds all the symbols for a particular
                // section. We need to mark all "referenced" symbols from that carrier,
                // so we make sure we're pulling in all outer symbols, and their sub
                // symbols. This is not ideal, and these carrier/section symbols could
                // be removed.
                if (d.ldr.IsExternal(symIdx))
                {
                    d.mark(d.ldr.OuterSym(symIdx), symIdx);
                    d.mark(d.ldr.SubSym(symIdx), symIdx);
                }

                if (len(methods) != 0L)
                {
                    if (!isgotype)
                    {
                        panic("method found on non-type symbol");
                    } 
                    // Decode runtime type information for type methods
                    // to help work out which methods can be called
                    // dynamically via interfaces.
                    var methodsigs = d.decodetypeMethods(d.ldr, d.ctxt.Arch, symIdx, _addr_relocs);
                    if (len(methods) != len(methodsigs))
                    {
                        panic(fmt.Sprintf("%q has %d method relocations for %d methods", d.ldr.SymName(symIdx), len(methods), len(methodsigs)));
                    }

                    {
                        long i__prev2 = i;

                        foreach (var (__i, __m) in methodsigs)
                        {
                            i = __i;
                            m = __m;
                            methods[i].m = m;
                        }

                        i = i__prev2;
                    }

                    d.markableMethods = append(d.markableMethods, methods);

                }

            }


        });

        private static void mark(this ptr<deadcodePass> _addr_d, loader.Sym symIdx, loader.Sym parent)
        {
            ref deadcodePass d = ref _addr_d.val;

            if (symIdx != 0L && !d.ldr.AttrReachable(symIdx))
            {
                d.wq.push(symIdx);
                d.ldr.SetAttrReachable(symIdx, true);
                if (objabi.Fieldtrack_enabled != 0L)
                {
                    d.ldr.Reachparent[symIdx] = parent;
                }

                if (flagDumpDep.val)
                {
                    var to = d.ldr.SymName(symIdx);
                    if (to != "")
                    {
                        @string from = "_";
                        if (parent != 0L)
                        {
                            from = d.ldr.SymName(parent);
                        }

                        fmt.Printf("%s -> %s\n", from, to);

                    }

                }

            }

        }

        private static void markMethod(this ptr<deadcodePass> _addr_d, methodref m)
        {
            ref deadcodePass d = ref _addr_d.val;

            var relocs = d.ldr.Relocs(m.src);
            d.mark(relocs.At2(m.r).Sym(), m.src);
            d.mark(relocs.At2(m.r + 1L).Sym(), m.src);
            d.mark(relocs.At2(m.r + 2L).Sym(), m.src);
        }

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
        //    3. reflect.Value.Method (or MethodByName), or reflect.Type.Method
        //       (or MethodByName)
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
        //    - reflect.Value.Method or MethodByName is reachable
        //     - reflect.Type.Method or MethodByName is called (through the
        //       REFLECTMETHOD attribute marked by the compiler).
        // If any of these happen, all bets are off and all exported methods
        // of reachable types are marked reachable.
        //
        // Any unreached text symbols are removed from ctxt.Textp.
        private static void deadcode(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            deadcodePass d = new deadcodePass(ctxt:ctxt,ldr:ldr);
            d.init();
            d.flood();

            var methSym = ldr.Lookup("reflect.Value.Method", sym.SymVerABIInternal);
            var methByNameSym = ldr.Lookup("reflect.Value.MethodByName", sym.SymVerABIInternal);

            if (ctxt.DynlinkingGo())
            { 
                // Exported methods may satisfy interfaces we don't know
                // about yet when dynamically linking.
                d.reflectSeen = true;

            }

            while (true)
            { 
                // Methods might be called via reflection. Give up on
                // static analysis, mark all exported methods of
                // all reachable types as reachable.
                d.reflectSeen = d.reflectSeen || (methSym != 0L && ldr.AttrReachable(methSym)) || (methByNameSym != 0L && ldr.AttrReachable(methByNameSym)); 

                // Mark all methods that could satisfy a discovered
                // interface as reachable. We recheck old marked interfaces
                // as new types (with new methods) may have been discovered
                // in the last pass.
                var rem = d.markableMethods[..0L];
                foreach (var (_, m) in d.markableMethods)
                {
                    if ((d.reflectSeen && m.isExported()) || d.ifaceMethod[m.m])
                    {
                        d.markMethod(m);
                    }
                    else
                    {
                        rem = append(rem, m);
                    }

                }
                d.markableMethods = rem;

                if (d.wq.empty())
                { 
                    // No new work was discovered. Done.
                    break;

                }

                d.flood();

            }


            var n = ldr.NSym();

            if (ctxt.BuildMode != BuildModeShared)
            { 
                // Keep a itablink if the symbol it points at is being kept.
                // (When BuildModeShared, always keep itablinks.)
                for (long i = 1L; i < n; i++)
                {
                    var s = loader.Sym(i);
                    if (ldr.IsItabLink(s))
                    {
                        var relocs = ldr.Relocs(s);
                        if (relocs.Count() > 0L && ldr.AttrReachable(relocs.At2(0L).Sym()))
                        {
                            ldr.SetAttrReachable(s, true);
                        }

                    }

                }


            }

        }

        // methodref holds the relocations from a receiver type symbol to its
        // method. There are three relocations, one for each of the fields in
        // the reflect.method struct: mtyp, ifn, and tfn.
        private partial struct methodref
        {
            public methodsig m;
            public loader.Sym src; // receiver type symbol
            public long r; // the index of R_METHODOFF relocations
        }

        private static bool isExported(this methodref m) => func((_, panic, __) =>
        {
            foreach (var (_, r) in m.m)
            {
                return unicode.IsUpper(r);
            }
            panic("methodref has no signature");

        });

        // decodeMethodSig2 decodes an array of method signature information.
        // Each element of the array is size bytes. The first 4 bytes is a
        // nameOff for the method name, and the next 4 bytes is a typeOff for
        // the function type.
        //
        // Conveniently this is the layout of both runtime.method and runtime.imethod.
        private static slice<methodsig> decodeMethodSig(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, long off, long size, long count)
        {
            ref deadcodePass d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            bytes.Buffer buf = default;
            slice<methodsig> methods = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < count; i++)
                {
                    buf.WriteString(decodetypeName(ldr, symIdx, relocs, off));
                    var mtypSym = decodeRelocSym(ldr, symIdx, relocs, int32(off + 4L)); 
                    // FIXME: add some sort of caching here, since we may see some of the
                    // same symbols over time for param types.
                    ref var mrelocs = ref heap(ldr.Relocs(mtypSym), out ptr<var> _addr_mrelocs);
                    var mp = ldr.Data(mtypSym);

                    buf.WriteRune('(');
                    var inCount = decodetypeFuncInCount(arch, mp);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < inCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }

                            var a = decodetypeFuncInType(ldr, arch, mtypSym, _addr_mrelocs, i);
                            buf.WriteString(ldr.SymName(a));

                        }


                        i = i__prev2;
                    }
                    buf.WriteString(") (");
                    var outCount = decodetypeFuncOutCount(arch, mp);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < outCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }

                            a = decodetypeFuncOutType(ldr, arch, mtypSym, _addr_mrelocs, i);
                            buf.WriteString(ldr.SymName(a));

                        }


                        i = i__prev2;
                    }
                    buf.WriteRune(')');

                    off += size;
                    methods = append(methods, methodsig(buf.String()));
                    buf.Reset();

                }


                i = i__prev1;
            }
            return methods;

        }

        private static slice<methodsig> decodeIfaceMethods(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs) => func((_, panic, __) =>
        {
            ref deadcodePass d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            var p = ldr.Data(symIdx);
            if (decodetypeKind(arch, p) & kindMask != kindInterface)
            {
                panic(fmt.Sprintf("symbol %q is not an interface", ldr.SymName(symIdx)));
            }

            var rel = decodeReloc(ldr, symIdx, relocs, int32(commonsize(arch) + arch.PtrSize));
            var s = rel.Sym();
            if (s == 0L)
            {
                return null;
            }

            if (s != symIdx)
            {
                panic(fmt.Sprintf("imethod slice pointer in %q leads to a different symbol", ldr.SymName(symIdx)));
            }

            var off = int(rel.Add()); // array of reflect.imethod values
            var numMethods = int(decodetypeIfaceMethodCount(arch, p));
            long sizeofIMethod = 4L + 4L;
            return d.decodeMethodSig(ldr, arch, symIdx, relocs, off, sizeofIMethod, numMethods);

        });

        private static slice<methodsig> decodetypeMethods(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs) => func((_, panic, __) =>
        {
            ref deadcodePass d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            var p = ldr.Data(symIdx);
            if (!decodetypeHasUncommon(arch, p))
            {
                panic(fmt.Sprintf("no methods on %q", ldr.SymName(symIdx)));
            }

            var off = commonsize(arch); // reflect.rtype

            if (decodetypeKind(arch, p) & kindMask == kindStruct) // reflect.structType
                off += 4L * arch.PtrSize;
            else if (decodetypeKind(arch, p) & kindMask == kindPtr) // reflect.ptrType
                off += arch.PtrSize;
            else if (decodetypeKind(arch, p) & kindMask == kindFunc) // reflect.funcType
                off += arch.PtrSize; // 4 bytes, pointer aligned
            else if (decodetypeKind(arch, p) & kindMask == kindSlice) // reflect.sliceType
                off += arch.PtrSize;
            else if (decodetypeKind(arch, p) & kindMask == kindArray) // reflect.arrayType
                off += 3L * arch.PtrSize;
            else if (decodetypeKind(arch, p) & kindMask == kindChan) // reflect.chanType
                off += 2L * arch.PtrSize;
            else if (decodetypeKind(arch, p) & kindMask == kindMap) // reflect.mapType
                off += 4L * arch.PtrSize + 8L;
            else if (decodetypeKind(arch, p) & kindMask == kindInterface) // reflect.interfaceType
                off += 3L * arch.PtrSize;
            else                         var mcount = int(decodeInuxi(arch, p[off + 4L..], 2L));
            var moff = int(decodeInuxi(arch, p[off + 4L + 2L + 2L..], 4L));
            off += moff; // offset to array of reflect.method values
            const long sizeofMethod = (long)4L * 4L; // sizeof reflect.method in program
 // sizeof reflect.method in program
            return d.decodeMethodSig(ldr, arch, symIdx, relocs, off, sizeofMethod, mcount);

        });
    }
}}}}
