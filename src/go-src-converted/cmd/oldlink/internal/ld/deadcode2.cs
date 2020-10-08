// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:40:48 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\deadcode2.go
using bytes = go.bytes_package;
using dwarf = go.cmd.@internal.dwarf_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using heap = go.container.heap_package;
using fmt = go.fmt_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
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

        private partial struct deadcodePass2
        {
            public ptr<Link> ctxt;
            public ptr<loader.Loader> ldr;
            public workQueue wq;
            public slice<loader.Reloc> rtmp;
            public map<methodsig, bool> ifaceMethod; // methods declared in reached interfaces
            public slice<methodref2> markableMethods; // methods of reached types
            public bool reflectSeen; // whether we have seen a reflect method call
        }

        private static void init(this ptr<deadcodePass2> _addr_d)
        {
            ref deadcodePass2 d = ref _addr_d.val;

            d.ldr.InitReachable();
            d.ifaceMethod = make_map<methodsig, bool>();
            if (d.ctxt.Reachparent != null)
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
                        if (!d.ldr.IsDup(s))
                        {
                            d.mark(s, 0L);
                        }

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
                        d.ReadRelocs(exportsIdx);
                        {
                            long i__prev1 = i;

                            for (i = 0L; i < len(d.rtmp); i++)
                            {
                                d.mark(d.rtmp[i].Sym, 0L);
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

            // DWARF constant DIE symbols are not referenced, but needed by
            // the dwarf pass.
            if (!FlagW.val)
            {
                foreach (var (_, lib) in d.ctxt.Library)
                {
                    names = append(names, dwarf.ConstInfoPrefix + lib.Pkg);
                }

            }

            foreach (var (_, name) in names)
            { 
                // Mark symbol as a data/ABI0 symbol.
                d.mark(d.ldr.Lookup(name, 0L), 0L); 
                // Also mark any Go functions (internal ABI).
                d.mark(d.ldr.Lookup(name, sym.SymVerABIInternal), 0L);

            }

        }

        private static void flood(this ptr<deadcodePass2> _addr_d) => func((_, panic, __) =>
        {
            ref deadcodePass2 d = ref _addr_d.val;

            loader.Reloc symRelocs = new slice<loader.Reloc>(new loader.Reloc[] {  });
            loader.Sym auxSyms = new slice<loader.Sym>(new loader.Sym[] {  });
            while (!d.wq.empty())
            {
                var symIdx = d.wq.pop();

                d.reflectSeen = d.reflectSeen || d.ldr.IsReflectMethod(symIdx);

                var relocs = d.ldr.Relocs(symIdx);
                symRelocs = relocs.ReadAll(symRelocs);

                if (d.ldr.IsGoType(symIdx))
                {
                    var p = d.ldr.Data(symIdx);
                    if (len(p) != 0L && decodetypeKind(d.ctxt.Arch, p) & kindMask == kindInterface)
                    {
                        foreach (var (_, sig) in d.decodeIfaceMethods2(d.ldr, d.ctxt.Arch, symIdx, symRelocs))
                        {
                            if (d.ctxt.Debugvlog > 1L)
                            {
                                d.ctxt.Logf("reached iface method: %s\n", sig);
                            }

                            d.ifaceMethod[sig] = true;

                        }

                    }

                }

                slice<methodref2> methods = default;
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < relocs.Count; i++)
                    {
                        var r = symRelocs[i];
                        if (r.Type == objabi.R_WEAKADDROFF)
                        {
                            continue;
                        }

                        if (r.Type == objabi.R_METHODOFF)
                        {
                            if (i + 2L >= relocs.Count)
                            {
                                panic("expect three consecutive R_METHODOFF relocs");
                            }

                            methods = append(methods, new methodref2(src:symIdx,r:i));
                            i += 2L;
                            continue;

                        }

                        if (r.Type == objabi.R_USETYPE)
                        { 
                            // type symbol used for DWARF. we need to load the symbol but it may not
                            // be otherwise reachable in the program.
                            // do nothing for now as we still load all type symbols.
                            continue;

                        }

                        d.mark(r.Sym, symIdx);

                    }


                    i = i__prev2;
                }
                auxSyms = d.ldr.ReadAuxSyms(symIdx, auxSyms);
                {
                    long i__prev2 = i;

                    for (i = 0L; i < len(auxSyms); i++)
                    {
                        d.mark(auxSyms[i], symIdx);
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
                d.mark(d.ldr.OuterSym(symIdx), symIdx);
                d.mark(d.ldr.SubSym(symIdx), symIdx);

                if (len(methods) != 0L)
                { 
                    // Decode runtime type information for type methods
                    // to help work out which methods can be called
                    // dynamically via interfaces.
                    var methodsigs = d.decodetypeMethods2(d.ldr, d.ctxt.Arch, symIdx, symRelocs);
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

        private static void mark(this ptr<deadcodePass2> _addr_d, loader.Sym symIdx, loader.Sym parent)
        {
            ref deadcodePass2 d = ref _addr_d.val;

            if (symIdx != 0L && !d.ldr.Reachable.Has(symIdx))
            {
                d.wq.push(symIdx);
                d.ldr.Reachable.Set(symIdx);
                if (d.ctxt.Reachparent != null)
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

        private static void markMethod(this ptr<deadcodePass2> _addr_d, methodref2 m)
        {
            ref deadcodePass2 d = ref _addr_d.val;

            d.ReadRelocs(m.src);
            d.mark(d.rtmp[m.r].Sym, m.src);
            d.mark(d.rtmp[m.r + 1L].Sym, m.src);
            d.mark(d.rtmp[m.r + 2L].Sym, m.src);
        }

        private static void deadcode2(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            deadcodePass2 d = new deadcodePass2(ctxt:ctxt,ldr:ldr);
            d.init();
            d.flood();

            var callSym = ldr.Lookup("reflect.Value.Call", sym.SymVerABIInternal);
            var methSym = ldr.Lookup("reflect.Value.Method", sym.SymVerABIInternal);
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
                d.reflectSeen = d.reflectSeen || (callSym != 0L && ldr.Reachable.Has(callSym)) || (methSym != 0L && ldr.Reachable.Has(methSym)); 

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
                        if (relocs.Count > 0L && ldr.Reachable.Has(relocs.At(0L).Sym))
                        {
                            ldr.Reachable.Set(s);
                        }

                    }

                }


            }

        }

        // methodref2 holds the relocations from a receiver type symbol to its
        // method. There are three relocations, one for each of the fields in
        // the reflect.method struct: mtyp, ifn, and tfn.
        private partial struct methodref2
        {
            public methodsig m;
            public loader.Sym src; // receiver type symbol
            public long r; // the index of R_METHODOFF relocations
        }

        private static bool isExported(this methodref2 m) => func((_, panic, __) =>
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
        private static slice<methodsig> decodeMethodSig2(this ptr<deadcodePass2> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, slice<loader.Reloc> symRelocs, long off, long size, long count)
        {
            ref deadcodePass2 d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            bytes.Buffer buf = default;
            slice<methodsig> methods = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < count; i++)
                {
                    buf.WriteString(decodetypeName2(_addr_ldr, symIdx, symRelocs, off));
                    var mtypSym = decodeRelocSym2(_addr_ldr, symIdx, symRelocs, int32(off + 4L)); 
                    // FIXME: add some sort of caching here, since we may see some of the
                    // same symbols over time for param types.
                    d.ReadRelocs(mtypSym);
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

                            var a = d.decodetypeFuncInType2(ldr, arch, mtypSym, d.rtmp, i);
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

                            a = d.decodetypeFuncOutType2(ldr, arch, mtypSym, d.rtmp, i);
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

        private static slice<methodsig> decodeIfaceMethods2(this ptr<deadcodePass2> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, slice<loader.Reloc> symRelocs) => func((_, panic, __) =>
        {
            ref deadcodePass2 d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var p = ldr.Data(symIdx);
            if (decodetypeKind(arch, p) & kindMask != kindInterface)
            {
                panic(fmt.Sprintf("symbol %q is not an interface", ldr.SymName(symIdx)));
            }

            var rel = decodeReloc2(_addr_ldr, symIdx, symRelocs, int32(commonsize(arch) + arch.PtrSize));
            if (rel.Sym == 0L)
            {
                return null;
            }

            if (rel.Sym != symIdx)
            {
                panic(fmt.Sprintf("imethod slice pointer in %q leads to a different symbol", ldr.SymName(symIdx)));
            }

            var off = int(rel.Add); // array of reflect.imethod values
            var numMethods = int(decodetypeIfaceMethodCount(arch, p));
            long sizeofIMethod = 4L + 4L;
            return d.decodeMethodSig2(ldr, arch, symIdx, symRelocs, off, sizeofIMethod, numMethods);

        });

        private static slice<methodsig> decodetypeMethods2(this ptr<deadcodePass2> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, slice<loader.Reloc> symRelocs) => func((_, panic, __) =>
        {
            ref deadcodePass2 d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

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
            return d.decodeMethodSig2(ldr, arch, symIdx, symRelocs, off, sizeofMethod, mcount);

        });

        private static loader.Reloc decodeReloc2(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, slice<loader.Reloc> symRelocs, int off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;

            for (long j = 0L; j < len(symRelocs); j++)
            {
                var rel = symRelocs[j];
                if (rel.Off == off)
                {
                    return rel;
                }

            }

            return new loader.Reloc();

        }

        private static loader.Sym decodeRelocSym2(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, slice<loader.Reloc> symRelocs, int off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;

            return decodeReloc2(_addr_ldr, symIdx, symRelocs, off).Sym;
        }

        // decodetypeName2 decodes the name from a reflect.name.
        private static @string decodetypeName2(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, slice<loader.Reloc> symRelocs, long off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;

            var r = decodeRelocSym2(_addr_ldr, symIdx, symRelocs, int32(off));
            if (r == 0L)
            {
                return "";
            }

            var data = ldr.Data(r);
            var namelen = int(uint16(data[1L]) << (int)(8L) | uint16(data[2L]));
            return string(data[3L..3L + namelen]);

        }

        private static loader.Sym decodetypeFuncInType2(this ptr<deadcodePass2> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, slice<loader.Reloc> symRelocs, long i)
        {
            ref deadcodePass2 d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var uadd = commonsize(arch) + 4L;
            if (arch.PtrSize == 8L)
            {
                uadd += 4L;
            }

            if (decodetypeHasUncommon(arch, ldr.Data(symIdx)))
            {
                uadd += uncommonSize();
            }

            return decodeRelocSym2(_addr_ldr, symIdx, symRelocs, int32(uadd + i * arch.PtrSize));

        }

        private static loader.Sym decodetypeFuncOutType2(this ptr<deadcodePass2> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, slice<loader.Reloc> symRelocs, long i)
        {
            ref deadcodePass2 d = ref _addr_d.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return d.decodetypeFuncInType2(ldr, arch, symIdx, symRelocs, i + decodetypeFuncInCount(arch, ldr.Data(symIdx)));
        }

        // readRelocs reads the relocations for the specified symbol into the
        // deadcode relocs work array. Use with care, since the work array
        // is a singleton.
        private static void ReadRelocs(this ptr<deadcodePass2> _addr_d, loader.Sym symIdx)
        {
            ref deadcodePass2 d = ref _addr_d.val;

            var relocs = d.ldr.Relocs(symIdx);
            d.rtmp = relocs.ReadAll(d.rtmp);
        }
    }
}}}}
