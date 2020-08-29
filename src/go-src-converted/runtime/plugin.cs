// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:16 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\plugin.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:linkname plugin_lastmoduleinit plugin.lastmoduleinit
        private static (@string, object, @string) plugin_lastmoduleinit()
        {
            ref moduledata md = default;
            {
                var pmd__prev1 = pmd;

                var pmd = firstmoduledata.next;

                while (pmd != null)
                {
                    if (pmd.bad)
                    {
                        md = null; // we only want the last module
                        continue;
                    pmd = pmd.next;
                    }
                    md = pmd;
                }

                pmd = pmd__prev1;
            }
            if (md == null)
            {
                throw("runtime: no plugin module data");
            }
            if (md.pluginpath == "")
            {
                throw("runtime: plugin has empty pluginpath");
            }
            if (md.typemap != null)
            {
                return ("", null, "plugin already loaded");
            }
            {
                var pmd__prev1 = pmd;

                foreach (var (_, __pmd) in activeModules())
                {
                    pmd = __pmd;
                    if (pmd.pluginpath == md.pluginpath)
                    {
                        md.bad = true;
                        return ("", null, "plugin already loaded");
                    }
                    if (inRange(pmd.text, pmd.etext, md.text, md.etext) || inRange(pmd.bss, pmd.ebss, md.bss, md.ebss) || inRange(pmd.data, pmd.edata, md.data, md.edata) || inRange(pmd.types, pmd.etypes, md.types, md.etypes))
                    {
                        println("plugin: new module data overlaps with previous moduledata");
                        println("\tpmd.text-etext=", hex(pmd.text), "-", hex(pmd.etext));
                        println("\tpmd.bss-ebss=", hex(pmd.bss), "-", hex(pmd.ebss));
                        println("\tpmd.data-edata=", hex(pmd.data), "-", hex(pmd.edata));
                        println("\tpmd.types-etypes=", hex(pmd.types), "-", hex(pmd.etypes));
                        println("\tmd.text-etext=", hex(md.text), "-", hex(md.etext));
                        println("\tmd.bss-ebss=", hex(md.bss), "-", hex(md.ebss));
                        println("\tmd.data-edata=", hex(md.data), "-", hex(md.edata));
                        println("\tmd.types-etypes=", hex(md.types), "-", hex(md.etypes));
                        throw("plugin: new module data overlaps with previous moduledata");
                    }
                }
                pmd = pmd__prev1;
            }

            foreach (var (_, pkghash) in md.pkghashes)
            {
                if (pkghash.linktimehash != pkghash.runtimehash.Value)
                {
                    md.bad = true;
                    return ("", null, "plugin was built with a different version of package " + pkghash.modulename);
                }
            }            modulesinit();
            typelinksinit();

            pluginftabverify(md);
            moduledataverify1(md);

            lock(ref itabLock);
            foreach (var (_, i) in md.itablinks)
            {
                itabAdd(i);
            }            unlock(ref itabLock); 

            // Build a map of symbol names to symbols. Here in the runtime
            // we fill out the first word of the interface, the type. We
            // pass these zero value interfaces to the plugin package,
            // where the symbol value is filled in (usually via cgo).
            //
            // Because functions are handled specially in the plugin package,
            // function symbol names are prefixed here with '.' to avoid
            // a dependency on the reflect package.
            syms = make(len(md.ptab));
            foreach (var (_, ptab) in md.ptab)
            {
                var symName = resolveNameOff(@unsafe.Pointer(md.types), ptab.name);
                var t = (_type.Value)(@unsafe.Pointer(md.types)).typeOff(ptab.typ);
                var val = default;
                ref array<unsafe.Pointer> valp = new ptr<ref array<unsafe.Pointer>>(@unsafe.Pointer(ref val));
                (valp.Value)[0L] = @unsafe.Pointer(t);

                var name = symName.name();
                if (t.kind & kindMask == kindFunc)
                {
                    name = "." + name;
                }
                syms[name] = val;
            }            return (md.pluginpath, syms, "");
        }

        private static void pluginftabverify(ref moduledata md)
        {
            var badtable = false;
            for (long i = 0L; i < len(md.ftab); i++)
            {
                var entry = md.ftab[i].entry;
                if (md.minpc <= entry && entry <= md.maxpc)
                {
                    continue;
                }
                funcInfo f = new funcInfo((*_func)(unsafe.Pointer(&md.pclntable[md.ftab[i].funcoff])),md);
                var name = funcname(f); 

                // A common bug is f.entry has a relocation to a duplicate
                // function symbol, meaning if we search for its PC we get
                // a valid entry with a name that is useful for debugging.
                @string name2 = "none";
                var entry2 = uintptr(0L);
                var f2 = findfunc(entry);
                if (f2.valid())
                {
                    name2 = funcname(f2);
                    entry2 = f2.entry;
                }
                badtable = true;
                println("ftab entry outside pc range: ", hex(entry), "/", hex(entry2), ": ", name, "/", name2);
            }

            if (badtable)
            {
                throw("runtime: plugin has bad symbol table");
            }
        }

        // inRange reports whether v0 or v1 are in the range [r0, r1].
        private static bool inRange(System.UIntPtr r0, System.UIntPtr r1, System.UIntPtr v0, System.UIntPtr v1)
        {
            return (v0 >= r0 && v0 <= r1) || (v1 >= r0 && v1 <= r1);
        }

        // A ptabEntry is generated by the compiler for each exported function
        // and global variable in the main package of a plugin. It is used to
        // initialize the plugin module's symbol map.
        private partial struct ptabEntry
        {
            public nameOff name;
            public typeOff typ;
        }
    }
}
