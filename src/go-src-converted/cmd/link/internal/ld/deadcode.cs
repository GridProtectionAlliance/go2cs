// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:20:56 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\deadcode.go
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using unicode = go.unicode_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

private static var _ = fmt.Print;

private partial struct deadcodePass {
    public ptr<Link> ctxt;
    public ptr<loader.Loader> ldr;
    public heap wq; // work queue, using min-heap for better locality

    public map<methodsig, bool> ifaceMethod; // methods declared in reached interfaces
    public slice<methodref> markableMethods; // methods of reached types
    public bool reflectSeen; // whether we have seen a reflect method call
    public bool dynlink;
    public slice<methodsig> methodsigstmp; // scratch buffer for decoding method signatures
}

private static void init(this ptr<deadcodePass> _addr_d) {
    ref deadcodePass d = ref _addr_d.val;

    d.ldr.InitReachable();
    d.ifaceMethod = make_map<methodsig, bool>();
    if (buildcfg.Experiment.FieldTrack) {
        d.ldr.Reachparent = make_slice<loader.Sym>(d.ldr.NSym());
    }
    d.dynlink = d.ctxt.DynlinkingGo();

    if (d.ctxt.BuildMode == BuildModeShared) { 
        // Mark all symbols defined in this library as reachable when
        // building a shared library.
        var n = d.ldr.NDef();
        {
            nint i__prev1 = i;

            for (nint i = 1; i < n; i++) {
                var s = loader.Sym(i);
                d.mark(s, 0);
            }


            i = i__prev1;
        }
        return ;

    }
    slice<@string> names = default; 

    // In a normal binary, start at main.main and the init
    // functions and mark what is reachable from there.
    if (d.ctxt.linkShared && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE)) {
        names = append(names, "main.main", "main..inittask");
    }
    else
 { 
        // The external linker refers main symbol directly.
        if (d.ctxt.LinkMode == LinkExternal && (d.ctxt.BuildMode == BuildModeExe || d.ctxt.BuildMode == BuildModePIE)) {
            if (d.ctxt.HeadType == objabi.Hwindows && d.ctxt.Arch.Family == sys.I386) {
                flagEntrySymbol.val = "_main";
            }
            else
 {
                flagEntrySymbol.val = "main";
            }

        }
        names = append(names, flagEntrySymbol.val);

    }
    names = append(names, "runtime.unreachableMethod");
    if (!d.ctxt.linkShared && d.ctxt.BuildMode != BuildModePlugin) { 
        // runtime.buildVersion and runtime.modinfo are referenced in .go.buildinfo section
        // (see function buildinfo in data.go). They should normally be reachable from the
        // runtime. Just make it explicit, in case.
        names = append(names, "runtime.buildVersion", "runtime.modinfo");

    }
    if (d.ctxt.BuildMode == BuildModePlugin) {
        names = append(names, objabi.PathToPrefix(flagPluginPath.val) + "..inittask", objabi.PathToPrefix(flagPluginPath.val) + ".main", "go.plugin.tabs"); 

        // We don't keep the go.plugin.exports symbol,
        // but we do keep the symbols it refers to.
        var exportsIdx = d.ldr.Lookup("go.plugin.exports", 0);
        if (exportsIdx != 0) {
            var relocs = d.ldr.Relocs(exportsIdx);
            {
                nint i__prev1 = i;

                for (i = 0; i < relocs.Count(); i++) {
                    d.mark(relocs.At(i).Sym(), 0);
                }


                i = i__prev1;
            }

        }
    }
    if (d.ctxt.Debugvlog > 1) {
        d.ctxt.Logf("deadcode start names: %v\n", names);
    }
    foreach (var (_, name) in names) { 
        // Mark symbol as a data/ABI0 symbol.
        d.mark(d.ldr.Lookup(name, 0), 0); 
        // Also mark any Go functions (internal ABI).
        d.mark(d.ldr.Lookup(name, sym.SymVerABIInternal), 0);

    }    {
        var s__prev1 = s;

        foreach (var (_, __s) in d.ctxt.dynexp) {
            s = __s;
            if (d.ctxt.Debugvlog > 1) {
                d.ctxt.Logf("deadcode start dynexp: %s<%d>\n", d.ldr.SymName(s), d.ldr.SymVersion(s));
            }
            d.mark(s, 0);
        }
        s = s__prev1;
    }
}

private static void flood(this ptr<deadcodePass> _addr_d) => func((_, panic, _) => {
    ref deadcodePass d = ref _addr_d.val;

    slice<methodref> methods = default;
    while (!d.wq.empty()) {
        var symIdx = d.wq.pop();

        d.reflectSeen = d.reflectSeen || d.ldr.IsReflectMethod(symIdx);

        var isgotype = d.ldr.IsGoType(symIdx);
        ref var relocs = ref heap(d.ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
        bool usedInIface = default;

        if (isgotype) {
            if (d.dynlink) { 
                // When dynamic linking, a type may be passed across DSO
                // boundary and get converted to interface at the other side.
                d.ldr.SetAttrUsedInIface(symIdx, true);

            }

            usedInIface = d.ldr.AttrUsedInIface(symIdx);

        }
        methods = methods[..(int)0];
        {
            nint i__prev2 = i;

            for (nint i = 0; i < relocs.Count(); i++) {
                var r = relocs.At(i); 
                // When build with "-linkshared", we can't tell if the interface
                // method in itab will be used or not. Ignore the weak attribute.
                if (r.Weak() && !(d.ctxt.linkShared && d.ldr.IsItab(symIdx))) {
                    continue;
                }

                var t = r.Type();

                if (t == objabi.R_METHODOFF) 
                    if (i + 2 >= relocs.Count()) {
                        panic("expect three consecutive R_METHODOFF relocs");
                    }
                    if (usedInIface) {
                        methods = append(methods, new methodref(src:symIdx,r:i)); 
                        // The method descriptor is itself a type descriptor, and
                        // it can be used to reach other types, e.g. by using
                        // reflect.Type.Method(i).Type.In(j). We need to traverse
                        // its child types with UsedInIface set. (See also the
                        // comment below.)
                        var rs = r.Sym();
                        if (!d.ldr.AttrUsedInIface(rs)) {
                            d.ldr.SetAttrUsedInIface(rs, true);
                            if (d.ldr.AttrReachable(rs)) {
                                d.ldr.SetAttrReachable(rs, false);
                                d.mark(rs, symIdx);
                            }
                        }

                    }

                    i += 2;
                    continue;
                else if (t == objabi.R_USETYPE) 
                    // type symbol used for DWARF. we need to load the symbol but it may not
                    // be otherwise reachable in the program.
                    // do nothing for now as we still load all type symbols.
                    continue;
                else if (t == objabi.R_USEIFACE) 
                    // R_USEIFACE is a marker relocation that tells the linker the type is
                    // converted to an interface, i.e. should have UsedInIface set. See the
                    // comment below for why we need to unset the Reachable bit and re-mark it.
                    rs = r.Sym();
                    if (!d.ldr.AttrUsedInIface(rs)) {
                        d.ldr.SetAttrUsedInIface(rs, true);
                        if (d.ldr.AttrReachable(rs)) {
                            d.ldr.SetAttrReachable(rs, false);
                            d.mark(rs, symIdx);
                        }
                    }
                    continue;
                else if (t == objabi.R_USEIFACEMETHOD) 
                    // R_USEIFACEMETHOD is a marker relocation that marks an interface
                    // method as used.
                    rs = r.Sym();
                    if (d.ctxt.linkShared && (d.ldr.SymType(rs) == sym.SDYNIMPORT || d.ldr.SymType(rs) == sym.Sxxx)) { 
                        // Don't decode symbol from shared library (we'll mark all exported methods anyway).
                        // We check for both SDYNIMPORT and Sxxx because name-mangled symbols haven't
                        // been resolved at this point.
                        continue;

                    }

                    var m = d.decodeIfaceMethod(d.ldr, d.ctxt.Arch, rs, r.Add());
                    if (d.ctxt.Debugvlog > 1) {
                        d.ctxt.Logf("reached iface method: %v\n", m);
                    }

                    d.ifaceMethod[m] = true;
                    continue;
                                rs = r.Sym();
                if (isgotype && usedInIface && d.ldr.IsGoType(rs) && !d.ldr.AttrUsedInIface(rs)) { 
                    // If a type is converted to an interface, it is possible to obtain an
                    // interface with a "child" type of it using reflection (e.g. obtain an
                    // interface of T from []chan T). We need to traverse its "child" types
                    // with UsedInIface attribute set.
                    // When visiting the child type (chan T in the example above), it will
                    // have UsedInIface set, so it in turn will mark and (re)visit its children
                    // (e.g. T above).
                    // We unset the reachable bit here, so if the child type is already visited,
                    // it will be visited again.
                    // Note that a type symbol can be visited at most twice, one without
                    // UsedInIface and one with. So termination is still guaranteed.
                    d.ldr.SetAttrUsedInIface(rs, true);
                    d.ldr.SetAttrReachable(rs, false);

                }

                d.mark(rs, symIdx);

            }


            i = i__prev2;
        }
        var naux = d.ldr.NAux(symIdx);
        {
            nint i__prev2 = i;

            for (i = 0; i < naux; i++) {
                var a = d.ldr.Aux(symIdx, i);
                if (a.Type() == goobj.AuxGotype) { 
                    // A symbol being reachable doesn't imply we need its
                    // type descriptor. Don't mark it.
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
        if (d.ldr.IsExternal(symIdx)) {
            d.mark(d.ldr.OuterSym(symIdx), symIdx);
            d.mark(d.ldr.SubSym(symIdx), symIdx);
        }
        if (len(methods) != 0) {
            if (!isgotype) {
                panic("method found on non-type symbol");
            } 
            // Decode runtime type information for type methods
            // to help work out which methods can be called
            // dynamically via interfaces.
            var methodsigs = d.decodetypeMethods(d.ldr, d.ctxt.Arch, symIdx, _addr_relocs);
            if (len(methods) != len(methodsigs)) {
                panic(fmt.Sprintf("%q has %d method relocations for %d methods", d.ldr.SymName(symIdx), len(methods), len(methodsigs)));
            }

            {
                nint i__prev2 = i;
                var m__prev2 = m;

                foreach (var (__i, __m) in methodsigs) {
                    i = __i;
                    m = __m;
                    methods[i].m = m;
                    if (d.ctxt.Debugvlog > 1) {
                        d.ctxt.Logf("markable method: %v of sym %v %s\n", m, symIdx, d.ldr.SymName(symIdx));
                    }
                }

                i = i__prev2;
                m = m__prev2;
            }

            d.markableMethods = append(d.markableMethods, methods);

        }
    }

});

private static void mark(this ptr<deadcodePass> _addr_d, loader.Sym symIdx, loader.Sym parent) {
    ref deadcodePass d = ref _addr_d.val;

    if (symIdx != 0 && !d.ldr.AttrReachable(symIdx)) {
        d.wq.push(symIdx);
        d.ldr.SetAttrReachable(symIdx, true);
        if (buildcfg.Experiment.FieldTrack && d.ldr.Reachparent[symIdx] == 0) {
            d.ldr.Reachparent[symIdx] = parent;
        }
        if (flagDumpDep.val) {
            var to = d.ldr.SymName(symIdx);
            if (to != "") {
                if (d.ldr.AttrUsedInIface(symIdx)) {
                    to += " <UsedInIface>";
                }
                @string from = "_";
                if (parent != 0) {
                    from = d.ldr.SymName(parent);
                    if (d.ldr.AttrUsedInIface(parent)) {
                        from += " <UsedInIface>";
                    }
                }
                fmt.Printf("%s -> %s\n", from, to);
            }
        }
    }
}

private static void markMethod(this ptr<deadcodePass> _addr_d, methodref m) {
    ref deadcodePass d = ref _addr_d.val;

    var relocs = d.ldr.Relocs(m.src);
    d.mark(relocs.At(m.r).Sym(), m.src);
    d.mark(relocs.At(m.r + 1).Sym(), m.src);
    d.mark(relocs.At(m.r + 2).Sym(), m.src);
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
private static void deadcode(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    deadcodePass d = new deadcodePass(ctxt:ctxt,ldr:ldr);
    d.init();
    d.flood();

    var methSym = ldr.Lookup("reflect.Value.Method", sym.SymVerABIInternal);
    var methByNameSym = ldr.Lookup("reflect.Value.MethodByName", sym.SymVerABIInternal);

    if (ctxt.DynlinkingGo()) { 
        // Exported methods may satisfy interfaces we don't know
        // about yet when dynamically linking.
        d.reflectSeen = true;

    }
    while (true) { 
        // Methods might be called via reflection. Give up on
        // static analysis, mark all exported methods of
        // all reachable types as reachable.
        d.reflectSeen = d.reflectSeen || (methSym != 0 && ldr.AttrReachable(methSym)) || (methByNameSym != 0 && ldr.AttrReachable(methByNameSym)); 

        // Mark all methods that could satisfy a discovered
        // interface as reachable. We recheck old marked interfaces
        // as new types (with new methods) may have been discovered
        // in the last pass.
        var rem = d.markableMethods[..(int)0];
        foreach (var (_, m) in d.markableMethods) {
            if ((d.reflectSeen && m.isExported()) || d.ifaceMethod[m.m]) {
                d.markMethod(m);
            }
            else
 {
                rem = append(rem, m);
            }

        }        d.markableMethods = rem;

        if (d.wq.empty()) { 
            // No new work was discovered. Done.
            break;

        }
        d.flood();

    }

}

// methodsig is a typed method signature (name + type).
private partial struct methodsig {
    public @string name;
    public loader.Sym typ; // type descriptor symbol of the function
}

// methodref holds the relocations from a receiver type symbol to its
// method. There are three relocations, one for each of the fields in
// the reflect.method struct: mtyp, ifn, and tfn.
private partial struct methodref {
    public methodsig m;
    public loader.Sym src; // receiver type symbol
    public nint r; // the index of R_METHODOFF relocations
}

private static bool isExported(this methodref m) => func((_, panic, _) => {
    foreach (var (_, r) in m.m.name) {
        return unicode.IsUpper(r);
    }    panic("methodref has no signature");
});

// decodeMethodSig decodes an array of method signature information.
// Each element of the array is size bytes. The first 4 bytes is a
// nameOff for the method name, and the next 4 bytes is a typeOff for
// the function type.
//
// Conveniently this is the layout of both runtime.method and runtime.imethod.
private static slice<methodsig> decodeMethodSig(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, nint off, nint size, nint count) {
    ref deadcodePass d = ref _addr_d.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    if (cap(d.methodsigstmp) < count) {
        d.methodsigstmp = append(d.methodsigstmp[..(int)0], make_slice<methodsig>(count));
    }
    var methods = d.methodsigstmp[..(int)count];
    for (nint i = 0; i < count; i++) {
        methods[i].name = decodetypeName(ldr, symIdx, relocs, off);
        methods[i].typ = decodeRelocSym(ldr, symIdx, relocs, int32(off + 4));
        off += size;
    }
    return methods;

}

// Decode the method of interface type symbol symIdx at offset off.
private static methodsig decodeIfaceMethod(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, long off) => func((_, panic, _) => {
    ref deadcodePass d = ref _addr_d.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var p = ldr.Data(symIdx);
    if (decodetypeKind(arch, p) & kindMask != kindInterface) {
        panic(fmt.Sprintf("symbol %q is not an interface", ldr.SymName(symIdx)));
    }
    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    methodsig m = default;
    m.name = decodetypeName(ldr, symIdx, _addr_relocs, int(off));
    m.typ = decodeRelocSym(ldr, symIdx, _addr_relocs, int32(off + 4));
    return m;

});

private static slice<methodsig> decodetypeMethods(this ptr<deadcodePass> _addr_d, ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs) => func((_, panic, _) => {
    ref deadcodePass d = ref _addr_d.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    var p = ldr.Data(symIdx);
    if (!decodetypeHasUncommon(arch, p)) {
        panic(fmt.Sprintf("no methods on %q", ldr.SymName(symIdx)));
    }
    var off = commonsize(arch); // reflect.rtype

    if (decodetypeKind(arch, p) & kindMask == kindStruct) // reflect.structType
        off += 4 * arch.PtrSize;
    else if (decodetypeKind(arch, p) & kindMask == kindPtr) // reflect.ptrType
        off += arch.PtrSize;
    else if (decodetypeKind(arch, p) & kindMask == kindFunc) // reflect.funcType
        off += arch.PtrSize; // 4 bytes, pointer aligned
    else if (decodetypeKind(arch, p) & kindMask == kindSlice) // reflect.sliceType
        off += arch.PtrSize;
    else if (decodetypeKind(arch, p) & kindMask == kindArray) // reflect.arrayType
        off += 3 * arch.PtrSize;
    else if (decodetypeKind(arch, p) & kindMask == kindChan) // reflect.chanType
        off += 2 * arch.PtrSize;
    else if (decodetypeKind(arch, p) & kindMask == kindMap) // reflect.mapType
        off += 4 * arch.PtrSize + 8;
    else if (decodetypeKind(arch, p) & kindMask == kindInterface) // reflect.interfaceType
        off += 3 * arch.PtrSize;
    else         var mcount = int(decodeInuxi(arch, p[(int)off + 4..], 2));
    var moff = int(decodeInuxi(arch, p[(int)off + 4 + 2 + 2..], 4));
    off += moff; // offset to array of reflect.method values
    const nint sizeofMethod = 4 * 4; // sizeof reflect.method in program
 // sizeof reflect.method in program
    return d.decodeMethodSig(ldr, arch, symIdx, relocs, off, sizeofMethod, mcount);

});

} // end ld_package
