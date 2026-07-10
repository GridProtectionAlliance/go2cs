// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

//go:linkname plugin_lastmoduleinit plugin.lastmoduleinit
internal static (@string path, map<@string, any> syms, slice<ж<initTask>> initTasks, @string errstr) plugin_lastmoduleinit() {
    @string path = default!;
    map<@string, any> syms = default!;
    slice<ж<initTask>> initTasks = default!;
    @string errstr = default!;

    ж<moduledata> md = default!;
    for (var pmd = firstmoduledata.next; pmd != nil; pmd = pmd.Value.next) {
        if ((~pmd).bad) {
            md = default!;
            // we only want the last module
            continue;
        }
        md = pmd;
    }
    if (md == nil) {
        @throw("runtime: no plugin module data"u8);
    }
    if ((~md).pluginpath == ""u8) {
        @throw("runtime: plugin has empty pluginpath"u8);
    }
    if ((~md).typemap != default!) {
        return ("", default!, default!, "plugin already loaded");
    }
    foreach (var (_, pmd) in activeModules()) {
        if ((~pmd).pluginpath == (~md).pluginpath) {
            md.Value.bad = true;
            return ("", default!, default!, "plugin already loaded");
        }
        if (inRange((~pmd).text, (~pmd).etext, (~md).text, (~md).etext) || inRange((~pmd).bss, (~pmd).ebss, (~md).bss, (~md).ebss) || inRange((~pmd).data, (~pmd).edata, (~md).data, (~md).edata) || inRange((~pmd).types, (~pmd).etypes, (~md).types, (~md).etypes)) {
            println("plugin: new module data overlaps with previous moduledata");
            println("\tpmd.text-etext=", ((Δhex)(uint64)(~pmd).text), "-", ((Δhex)(uint64)(~pmd).etext));
            println("\tpmd.bss-ebss=", ((Δhex)(uint64)(~pmd).bss), "-", ((Δhex)(uint64)(~pmd).ebss));
            println("\tpmd.data-edata=", ((Δhex)(uint64)(~pmd).data), "-", ((Δhex)(uint64)(~pmd).edata));
            println("\tpmd.types-etypes=", ((Δhex)(uint64)(~pmd).types), "-", ((Δhex)(uint64)(~pmd).etypes));
            println("\tmd.text-etext=", ((Δhex)(uint64)(~md).text), "-", ((Δhex)(uint64)(~md).etext));
            println("\tmd.bss-ebss=", ((Δhex)(uint64)(~md).bss), "-", ((Δhex)(uint64)(~md).ebss));
            println("\tmd.data-edata=", ((Δhex)(uint64)(~md).data), "-", ((Δhex)(uint64)(~md).edata));
            println("\tmd.types-etypes=", ((Δhex)(uint64)(~md).types), "-", ((Δhex)(uint64)(~md).etypes));
            @throw("plugin: new module data overlaps with previous moduledata"u8);
        }
    }
    foreach (var (_, pkghash) in (~md).pkghashes) {
        if (pkghash.linktimehash != pkghash.runtimehash.Value) {
            md.Value.bad = true;
            return ("", default!, default!, "plugin was built with a different version of package " + pkghash.modulename);
        }
    }
    // Initialize the freshly loaded module.
    modulesinit();
    typelinksinit();
    pluginftabverify(md);
    moduledataverify1(md);
    @lock(ᏑitabLock);
    foreach (var (_, i) in (~md).itablinks) {
        itabAdd(i);
    }
    unlock(ᏑitabLock);
    // Build a map of symbol names to symbols. Here in the runtime
    // we fill out the first word of the interface, the type. We
    // pass these zero value interfaces to the plugin package,
    // where the symbol value is filled in (usually via cgo).
    //
    // Because functions are handled specially in the plugin package,
    // function symbol names are prefixed here with '.' to avoid
    // a dependency on the reflect package.
    syms = new map<@string, any>(len((~md).ptab));
    foreach (var (_, ptab) in (~md).ptab) {
        var symName = resolveNameOff((@unsafe.Pointer)(~md).types, ptab.name);
        var t = toRType((ж<_type>)(uintptr)((@unsafe.Pointer)(~md).types)).typeOff(ptab.typ);
        // TODO can this stack of conversions be simpler?
        ref var val = ref heap<any>(out var Ꮡval);
        var valp = (ж<array<@unsafe.Pointer>>)(uintptr)(new @unsafe.Pointer(Ꮡval));
        (valp.Value)[0] = new @unsafe.Pointer(t);
        @string name = symName.Name();
        if ((abiꓸKind)((~t).Kind_ & abi.KindMask) == abi.Func) {
            name = "."u8 + name;
        }
        syms[name] = val;
    }
    return ((~md).pluginpath, syms, (~md).inittasks, "");
}

internal static void pluginftabverify(ж<moduledata> Ꮡmd) {
    ref var md = ref Ꮡmd.Value;

    var badtable = false;
    for (nint i = 0; i < len(md.ftab); i++) {
        var entry = md.textAddr(md.ftab[i].entryoff);
        if (md.minpc <= entry && entry <= md.maxpc) {
            continue;
        }
        var f = new ΔfuncInfo((ж<_func>)(uintptr)(new @unsafe.Pointer(Ꮡ(md.pclntable, (int)(md.ftab[i].funcoff)))), Ꮡmd);
        @string name = funcname(f);
        // A common bug is f.entry has a relocation to a duplicate
        // function symbol, meaning if we search for its PC we get
        // a valid entry with a name that is useful for debugging.
        @string name2 = "none"u8;
        var entry2 = (uintptr)0;
        var f2 = findfunc(entry);
        if (f2.valid()) {
            name2 = funcname(f2);
            entry2 = f2.entry();
        }
        badtable = true;
        println("ftab entry", ((Δhex)(uint64)entry), "/", ((Δhex)(uint64)entry2), ": ",
            name, "/", name2, "outside pc range:[", ((Δhex)(uint64)md.minpc), ",", ((Δhex)(uint64)md.maxpc), "], modulename=", md.modulename, ", pluginpath=", md.pluginpath);
    }
    if (badtable) {
        @throw("runtime: plugin has bad symbol table"u8);
    }
}

// inRange reports whether v0 or v1 are in the range [r0, r1].
internal static bool inRange(uintptr r0, uintptr r1, uintptr v0, uintptr v1) {
    return (v0 >= r0 && v0 <= r1) || (v1 >= r0 && v1 <= r1);
}

// A ptabEntry is generated by the compiler for each exported function
// and global variable in the main package of a plugin. It is used to
// initialize the plugin module's symbol map.
[GoType] partial struct ptabEntry {
    internal nameOff name;
    internal typeOff typ;
}

} // end runtime_package
