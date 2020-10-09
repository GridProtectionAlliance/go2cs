// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package loader -- go2cs converted at 2020 October 09 05:51:31 UTC
// import "cmd/oldlink/internal/loader" ==> using loader = go.cmd.oldlink.@internal.loader_package
// Original source: C:\Go\src\cmd\oldlink\internal\loader\loader.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using dwarf = go.cmd.@internal.dwarf_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class loader_package
    {
        private static var _ = fmt.Print;

        // Sym encapsulates a global symbol index, used to identify a specific
        // Go symbol. The 0-valued Sym is corresponds to an invalid symbol.
        public partial struct Sym // : long
        {
        }

        // Relocs encapsulates the set of relocations on a given symbol; an
        // instance of this type is returned by the Loader Relocs() method.
        public partial struct Relocs
        {
            public long Count; // number of relocs

            public long li; // local index of symbol whose relocs we're examining
            public ptr<oReader> r; // object reader for containing package
            public ptr<Loader> l; // loader

            public ptr<sym.Symbol> ext; // external symbol if not nil
        }

        // Reloc contains the payload for a specific relocation.
        // TODO: replace this with sym.Reloc, once we change the
        // relocation target from "*sym.Symbol" to "loader.Sym" in sym.Reloc.
        public partial struct Reloc
        {
            public int Off; // offset to rewrite
            public byte Size; // number of bytes to rewrite: 0, 1, 2, or 4
            public objabi.RelocType Type; // the relocation type
            public long Add; // addend
            public Sym Sym; // global index of symbol the reloc addresses
        }

        // oReader is a wrapper type of obj.Reader, along with some
        // extra information.
        // TODO: rename to objReader once the old one is gone?
        private partial struct oReader
        {
            public ptr<sym.CompilationUnit> unit;
            public long version; // version of static symbol
            public uint flags; // read from object file
            public @string pkgprefix;
            public slice<Sym> rcache; // cache mapping local PkgNone symbol to resolved Sym
        }

        private partial struct objIdx
        {
            public ptr<oReader> r;
            public Sym i; // start index
            public Sym e; // end index
        }

        private partial struct nameVer
        {
            public @string name;
            public long v;
        }

        private partial struct bitmap // : slice<uint>
        {
        }

        // set the i-th bit.
        private static void Set(this bitmap bm, Sym i)
        {
            var n = uint(i) / 32L;
            var r = uint(i) % 32L;
            bm[n] |= 1L << (int)(r);

        }

        // whether the i-th bit is set.
        private static bool Has(this bitmap bm, Sym i)
        {
            var n = uint(i) / 32L;
            var r = uint(i) % 32L;
            return bm[n] & (1L << (int)(r)) != 0L;

        }

        private static bitmap makeBitmap(long n)
        {
            return make(bitmap, (n + 31L) / 32L);
        }

        // A Loader loads new object files and resolves indexed symbol references.
        public partial struct Loader
        {
            public map<ptr<oReader>, Sym> start; // map from object file to its start index
            public slice<objIdx> objs; // sorted by start index (i.e. objIdx.i)
            public Sym max; // current max index
            public Sym extStart; // from this index on, the symbols are externally defined
            public slice<nameVer> extSyms; // externally defined symbols
            public slice<Sym> builtinSyms; // global index of builtin symbols
            public long ocache; // index (into 'objs') of most recent lookup

            public array<map<@string, Sym>> symsByName; // map symbol name to index, two maps are for ABI0 and ABIInternal
            public map<nameVer, Sym> extStaticSyms; // externally defined static symbols, keyed by name
            public map<Sym, Sym> overwrite; // overwrite[i]=j if symbol j overwrites symbol i

            public map<@string, ptr<oReader>> objByPkg; // map package path to its Go object reader

            public slice<ptr<sym.Symbol>> Syms; // indexed symbols. XXX we still make sym.Symbol for now.

            public long anonVersion; // most recently assigned ext static sym pseudo-version

            public bitmap Reachable; // bitmap of reachable symbols, indexed by global index

// Used to implement field tracking; created during deadcode if
// field tracking is enabled. Reachparent[K] contains the index of
// the symbol that triggered the marking of symbol K as live.
            public slice<Sym> Reachparent;
            public slice<sym.Reloc> relocBatch; // for bulk allocation of relocations

            public uint flags;
            public long strictDupMsgs; // number of strict-dup warning/errors, when FlagStrictDups is enabled
        }

 
        // Loader.flags
        public static readonly long FlagStrictDups = (long)1L << (int)(iota);


        public static ptr<Loader> NewLoader(uint flags) => func((_, panic, __) =>
        {
            log.Fatal("-newobj in oldlink should not be used");
            panic("unreachable");
        });

        // Return the start index in the global index space for a given object file.
        private static Sym startIndex(this ptr<Loader> _addr_l, ptr<oReader> _addr_r)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            return l.start[r];
        }

        // Add a symbol with a given index, return if it is added.
        private static bool AddSym(this ptr<Loader> _addr_l, @string name, long ver, Sym i, ptr<oReader> _addr_r, bool dupok, sym.SymKind typ) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            panic("unreachable");
        });

        // Add an external symbol (without index). Return the index of newly added
        // symbol, or 0 if not added.
        private static Sym AddExtSym(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            var @static = ver >= sym.SymVerStatic;
            if (static)
            {
                {
                    var (_, ok) = l.extStaticSyms[new nameVer(name,ver)];

                    if (ok)
                    {
                        return 0L;
                    }

                }

            }
            else
            {
                {
                    (_, ok) = l.symsByName[ver][name];

                    if (ok)
                    {
                        return 0L;
                    }

                }

            }

            var i = l.max + 1L;
            if (static)
            {
                l.extStaticSyms[new nameVer(name,ver)] = i;
            }
            else
            {
                l.symsByName[ver][name] = i;
            }

            l.max++;
            if (l.extStart == 0L)
            {
                l.extStart = i;
            }

            l.extSyms = append(l.extSyms, new nameVer(name,ver));
            l.growSyms(int(i));
            return i;

        }

        private static bool IsExternal(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.extStart != 0L && i >= l.extStart;
        }

        // Ensure Syms slice has enough space.
        private static void growSyms(this ptr<Loader> _addr_l, long i)
        {
            ref Loader l = ref _addr_l.val;

            var n = len(l.Syms);
            if (n > i)
            {
                return ;
            }

            l.Syms = append(l.Syms, make_slice<ptr<sym.Symbol>>(i + 1L - n));

        }

        // Convert a local index to a global index.
        private static Sym toGlobal(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, long i)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            var g = l.startIndex(r) + Sym(i);
            {
                var (ov, ok) = l.overwrite[g];

                if (ok)
                {
                    return ov;
                }

            }

            return g;

        }

        // Convert a global index to a local index.
        private static (ptr<oReader>, long) toLocal(this ptr<Loader> _addr_l, Sym i)
        {
            ptr<oReader> _p0 = default!;
            long _p0 = default;
            ref Loader l = ref _addr_l.val;

            {
                var (ov, ok) = l.overwrite[i];

                if (ok)
                {
                    i = ov;
                }

            }

            if (l.IsExternal(i))
            {
                return (_addr_null!, int(i - l.extStart));
            }

            var oc = l.ocache;
            if (oc != 0L && i >= l.objs[oc].i && i <= l.objs[oc].e)
            {
                return (_addr_l.objs[oc].r!, int(i - l.objs[oc].i));
            } 
            // Search for the local object holding index i.
            // Below k is the first one that has its start index > i,
            // so k-1 is the one we want.
            var k = sort.Search(len(l.objs), k =>
            {
                return _addr_l.objs[k].i > i!;
            });
            l.ocache = k - 1L;
            return (_addr_l.objs[k - 1L].r!, int(i - l.objs[k - 1L].i));

        }

        // Look up a symbol by name, return global index, or 0 if not found.
        // This is more like Syms.ROLookup than Lookup -- it doesn't create
        // new symbol.
        private static Sym Lookup(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            if (ver >= sym.SymVerStatic || ver < 0L)
            {
                return l.extStaticSyms[new nameVer(name,ver)];
            }

            return l.symsByName[ver][name];

        }

        // Returns whether i is a dup of another symbol, and i is not
        // "primary", i.e. Lookup i by name will not return i.
        private static bool IsDup(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Check that duplicate symbols have same contents.
        private static void checkdup(this ptr<Loader> _addr_l, @string name, Sym i, ptr<oReader> _addr_r, Sym dup) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            panic("unreachable");
        });

        private static long NStrictDupMsgs(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return l.strictDupMsgs;
        }

        // Number of total symbols.
        private static long NSym(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return int(l.max + 1L);
        }

        // Number of defined Go symbols.
        private static long NDef(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return int(l.extStart);
        }

        // Returns the raw (unpatched) name of the i-th symbol.
        private static @string RawSymName(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns the (patched) name of the i-th symbol.
        private static @string SymName(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns the type of the i-th symbol.
        private static sym.SymKind SymType(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns the attributes of the i-th symbol.
        private static byte SymAttr(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns whether the i-th symbol has ReflectMethod attribute set.
        private static bool IsReflectMethod(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns whether this is a Go type symbol.
        private static bool IsGoType(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns whether this is a "go.itablink.*" symbol.
        private static bool IsItabLink(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (_, ok) = l.itablink[i];

                if (ok)
                {
                    return true;
                }

            }

            return false;

        }

        // Returns the symbol content of the i-th symbol. i is global index.
        private static slice<byte> Data(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns the number of aux symbols given a global index.
        private static long NAux(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Returns the referred symbol of the j-th aux symbol of the i-th
        // symbol.
        private static Sym AuxSym(this ptr<Loader> _addr_l, Sym i, long j) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // ReadAuxSyms reads the aux symbol ids for the specified symbol into the
        // slice passed as a parameter. If the slice capacity is not large enough, a new
        // larger slice will be allocated. Final slice is returned.
        private static slice<Sym> ReadAuxSyms(this ptr<Loader> _addr_l, Sym symIdx, slice<Sym> dst) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // OuterSym gets the outer symbol for host object loaded symbols.
        private static Sym OuterSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var sym = l.Syms[i];
            if (sym != null && sym.Outer != null)
            {
                var outer = sym.Outer;
                return l.Lookup(outer.Name, int(outer.Version));
            }

            return 0L;

        }

        // SubSym gets the subsymbol for host object loaded symbols.
        private static Sym SubSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var sym = l.Syms[i];
            if (sym != null && sym.Sub != null)
            {
                var sub = sym.Sub;
                return l.Lookup(sub.Name, int(sub.Version));
            }

            return 0L;

        }

        // Initialize Reachable bitmap for running deadcode pass.
        private static void InitReachable(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            l.Reachable = makeBitmap(l.NSym());
        }

        // At method returns the j-th reloc for a global symbol.
        private static Reloc At(this ptr<Relocs> _addr_relocs, long j) => func((_, panic, __) =>
        {
            ref Relocs relocs = ref _addr_relocs.val;

            panic("unreachable");
        });

        // ReadAll method reads all relocations for a symbol into the
        // specified slice. If the slice capacity is not large enough, a new
        // larger slice will be allocated. Final slice is returned.
        private static slice<Reloc> ReadAll(this ptr<Relocs> _addr_relocs, slice<Reloc> dst) => func((_, panic, __) =>
        {
            ref Relocs relocs = ref _addr_relocs.val;

            panic("unreachable");
        });

        // Relocs returns a Relocs object for the given global sym.
        private static Relocs Relocs(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            panic("unreachable");
        });

        // Preload a package: add autolibs, add symbols to the symbol table.
        // Does not read symbol data yet.
        private static void Preload(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, ptr<sym.CompilationUnit> _addr_unit, long length, @string pn, long flags) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;
            ref sym.Library lib = ref _addr_lib.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            panic("unreachable");
        });

        // Make sure referenced symbols are added. Most of them should already be added.
        // This should only be needed for referenced external symbols.
        private static void LoadRefs(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms)
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;

            foreach (var (_, o) in l.objs[1L..])
            {
                loadObjRefs(_addr_l, _addr_o.r, _addr_arch, _addr_syms);
            }

        }

        private static void loadObjRefs(ptr<Loader> _addr_l, ptr<oReader> _addr_r, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;

            panic("unreachable");
        });

        private static long abiToVer(ushort abi, long localSymVersion) => func((_, panic, __) =>
        {
            panic("unreachable");
        });

        private static void preprocess(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Name != "" && s.Name[0L] == '$' && len(s.Name) > 5L && s.Type == 0L && len(s.P) == 0L)
            {
                var (x, err) = strconv.ParseUint(s.Name[5L..], 16L, 64L);
                if (err != null)
                {
                    log.Panicf("failed to parse $-symbol %s: %v", s.Name, err);
                }

                s.Type = sym.SRODATA;
                s.Attr |= sym.AttrLocal;
                switch (s.Name[..5L])
                {
                    case "$f32.": 
                        if (uint64(uint32(x)) != x)
                        {
                            log.Panicf("$-symbol %s too large: %d", s.Name, x);
                        }

                        s.AddUint32(arch, uint32(x));
                        break;
                    case "$f64.": 

                    case "$i64.": 
                        s.AddUint64(arch, x);
                        break;
                    default: 
                        log.Panicf("unrecognized $-symbol: %s", s.Name);
                        break;
                }

            }

        }

        // Load full contents.
        private static void LoadFull(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms)
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
 
            // create all Symbols first.
            l.growSyms(l.NSym());

            long nr = 0L; // total number of sym.Reloc's we'll need
            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    nr += loadObjSyms(_addr_l, _addr_syms, _addr_o.r);
                } 

                // allocate a single large slab of relocations for all live symbols

                o = o__prev1;
            }

            l.relocBatch = make_slice<sym.Reloc>(nr); 

            // external symbols
            {
                var i__prev1 = i;

                for (var i = l.extStart; i <= l.max; i++)
                {
                    {
                        var s__prev1 = s;

                        var s = l.Syms[i];

                        if (s != null)
                        {
                            s.Attr.Set(sym.AttrReachable, l.Reachable.Has(i));
                            continue; // already loaded from external object
                        }

                        s = s__prev1;

                    }

                    var nv = l.extSyms[i - l.extStart];
                    if (l.Reachable.Has(i) || strings.HasPrefix(nv.name, "gofile.."))
                    { // XXX file symbols are used but not marked
                        s = syms.Newsym(nv.name, nv.v);
                        preprocess(_addr_arch, _addr_s);
                        s.Attr.Set(sym.AttrReachable, l.Reachable.Has(i));
                        l.Syms[i] = s;

                    }

                } 

                // load contents of defined symbols


                i = i__prev1;
            } 

            // load contents of defined symbols
            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    loadObjFull(_addr_l, _addr_o.r);
                } 

                // Resolve ABI aliases for external symbols. This is only
                // needed for internal cgo linking.
                // (The old code does this in deadcode, but deadcode2 doesn't
                // do this.)

                o = o__prev1;
            }

            {
                var i__prev1 = i;

                for (i = l.extStart; i <= l.max; i++)
                {
                    {
                        var s__prev1 = s;

                        s = l.Syms[i];

                        if (s != null && s.Attr.Reachable())
                        {
                            foreach (var (ri) in s.R)
                            {
                                var r = _addr_s.R[ri];
                                if (r.Sym != null && r.Sym.Type == sym.SABIALIAS)
                                {
                                    r.Sym = r.Sym.R[0L].Sym;
                                }

                            }

                        }

                        s = s__prev1;

                    }

                }


                i = i__prev1;
            }

        }

        // ExtractSymbols grabs the symbols out of the loader for work that hasn't been
        // ported to the new symbol type.
        private static void ExtractSymbols(this ptr<Loader> _addr_l, ptr<sym.Symbols> _addr_syms)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
 
            // Nil out overwritten symbols.
            // Overwritten Go symbols aren't a problem (as they're lazy loaded), but
            // symbols loaded from host object loaders are fully loaded, and we might
            // have multiple symbols with the same name. This loop nils them out.
            foreach (var (oldI) in l.overwrite)
            {
                l.Syms[oldI] = null;
            } 

            // Add symbols to the ctxt.Syms lookup table. This explicitly
            // skips things created via loader.Create (marked with versions
            // less than zero), since if we tried to add these we'd wind up
            // with collisions. Along the way, update the version from the
            // negative anon version to something larger than sym.SymVerStatic
            // (needed so that sym.symbol.IsFileLocal() works properly).
            var anonVerReplacement = syms.IncVersion();
            foreach (var (_, s) in l.Syms)
            {
                if (s == null)
                {
                    continue;
                }

                if (s.Name != "" && s.Version >= 0L)
                {
                    syms.Add(s);
                }

                if (s.Version < 0L)
                {
                    s.Version = int16(anonVerReplacement);
                }

            }

        }

        // addNewSym adds a new sym.Symbol to the i-th index in the list of symbols.
        private static ptr<sym.Symbol> addNewSym(this ptr<Loader> _addr_l, Sym i, ptr<sym.Symbols> _addr_syms, @string name, long ver, ptr<sym.CompilationUnit> _addr_unit, sym.SymKind t) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var s = syms.Newsym(name, ver);
            if (s.Type != 0L && s.Type != sym.SXREF)
            {
                fmt.Println("symbol already processed:", unit.Lib, i, s);
                panic("symbol already processed");
            }

            if (t == sym.SBSS && (s.Type == sym.SRODATA || s.Type == sym.SNOPTRBSS))
            {
                t = s.Type;
            }

            s.Type = t;
            s.Unit = unit;
            l.growSyms(int(i));
            l.Syms[i] = s;
            return _addr_s!;

        });

        // loadObjSyms creates sym.Symbol objects for the live Syms in the
        // object corresponding to object reader "r". Return value is the
        // number of sym.Reloc entries required for all the new symbols.
        private static long loadObjSyms(ptr<Loader> _addr_l, ptr<sym.Symbols> _addr_syms, ptr<oReader> _addr_r) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref oReader r = ref _addr_r.val;

            panic("unreachable");
        });

        // LoadSymbol loads a single symbol by name.
        // This function should only be used by the host object loaders.
        // NB: This function does NOT set the symbol as reachable.
        private static ptr<sym.Symbol> LoadSymbol(this ptr<Loader> _addr_l, @string name, long version, ptr<sym.Symbols> _addr_syms) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;

            panic("unreachable");
        });

        // LookupOrCreate looks up a symbol by name, and creates one if not found.
        // Either way, it will also create a sym.Symbol for it, if not already.
        // This should only be called when interacting with parts of the linker
        // that still works on sym.Symbols (i.e. internal cgo linking, for now).
        private static ptr<sym.Symbol> LookupOrCreate(this ptr<Loader> _addr_l, @string name, long version, ptr<sym.Symbols> _addr_syms) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;

            var i = l.Lookup(name, version);
            if (i != 0L)
            { 
                // symbol exists
                if (int(i) < len(l.Syms) && l.Syms[i] != null)
                {
                    return _addr_l.Syms[i]!; // already loaded
                }

                if (l.IsExternal(i))
                {
                    panic("Can't load an external symbol.");
                }

                return _addr_l.LoadSymbol(name, version, syms)!;

            }

            i = l.AddExtSym(name, version);
            var s = syms.Newsym(name, version);
            l.Syms[i] = s;
            return _addr_s!;

        });

        // Create creates a symbol with the specified name, returning a
        // sym.Symbol object for it. This method is intended for static/hidden
        // symbols discovered while loading host objects. We can see more than
        // one instance of a given static symbol with the same name/version,
        // so we can't add them to the lookup tables "as is". Instead assign
        // them fictitious (unique) versions, starting at -1 and decreasing by
        // one for each newly created symbol, and record them in the
        // extStaticSyms hash.
        private static ptr<sym.Symbol> Create(this ptr<Loader> _addr_l, @string name, ptr<sym.Symbols> _addr_syms)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;

            var i = l.max + 1L;
            l.max++;
            if (l.extStart == 0L)
            {
                l.extStart = i;
            } 

            // Assign a new unique negative version -- this is to mark the
            // symbol so that it can be skipped when ExtractSymbols is adding
            // ext syms to the sym.Symbols hash.
            l.anonVersion--;
            var ver = l.anonVersion;
            l.extSyms = append(l.extSyms, new nameVer(name,ver));
            l.growSyms(int(i));
            var s = syms.Newsym(name, ver);
            l.Syms[i] = s;
            l.extStaticSyms[new nameVer(name,ver)] = i;

            return _addr_s!;

        }

        private static void loadObjFull(ptr<Loader> _addr_l, ptr<oReader> _addr_r) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            panic("unreachable");
        });

        private static slice<byte> emptyPkg = (slice<byte>)"\"\".";

        private static (slice<byte>, long) patchDWARFName1(slice<byte> p, ptr<oReader> _addr_r)
        {
            slice<byte> _p0 = default;
            long _p0 = default;
            ref oReader r = ref _addr_r.val;
 
            // This is kind of ugly. Really the package name should not
            // even be included here.
            if (len(p) < 1L || p[0L] != dwarf.DW_ABRV_FUNCTION)
            {
                return (p, -1L);
            }

            var e = bytes.IndexByte(p, 0L);
            if (e == -1L)
            {
                return (p, -1L);
            }

            if (!bytes.Contains(p[..e], emptyPkg))
            {
                return (p, -1L);
            }

            slice<byte> pkgprefix = (slice<byte>)r.pkgprefix;
            var patched = bytes.Replace(p[..e], emptyPkg, pkgprefix, -1L);
            return (append(patched, p[e..]), e);

        }

        private static void patchDWARFName(ptr<sym.Symbol> _addr_s, ptr<oReader> _addr_r)
        {
            ref sym.Symbol s = ref _addr_s.val;
            ref oReader r = ref _addr_r.val;

            var (patched, e) = patchDWARFName1(s.P, _addr_r);
            if (e == -1L)
            {
                return ;
            }

            s.P = patched;
            s.Attr.Set(sym.AttrReadOnly, false);
            var delta = int64(len(s.P)) - s.Size;
            s.Size = int64(len(s.P));
            foreach (var (i) in s.R)
            {
                var r = _addr_s.R[i];
                if (r.Off > int32(e))
                {
                    r.Off += int32(delta);
                }

            }

        }

        // For debugging.
        private static void Dump(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            fmt.Println("objs");
            foreach (var (_, obj) in l.objs)
            {
                if (obj.r != null)
                {
                    fmt.Println(obj.i, obj.r.unit.Lib);
                }

            }
            fmt.Println("syms");
            {
                var i__prev1 = i;

                foreach (var (__i, __s) in l.Syms)
                {
                    i = __i;
                    s = __s;
                    if (i == 0L)
                    {
                        continue;
                    }

                    if (s != null)
                    {
                        fmt.Println(i, s, s.Type);
                    }
                    else
                    {
                        fmt.Println(i, l.SymName(Sym(i)), "<not loaded>");
                    }

                }

                i = i__prev1;
            }

            fmt.Println("overwrite:", l.overwrite);
            fmt.Println("symsByName");
            {
                var name__prev1 = name;
                var i__prev1 = i;

                foreach (var (__name, __i) in l.symsByName[0L])
                {
                    name = __name;
                    i = __i;
                    fmt.Println(i, name, 0L);
                }

                name = name__prev1;
                i = i__prev1;
            }

            {
                var name__prev1 = name;
                var i__prev1 = i;

                foreach (var (__name, __i) in l.symsByName[1L])
                {
                    name = __name;
                    i = __i;
                    fmt.Println(i, name, 1L);
                }

                name = name__prev1;
                i = i__prev1;
            }
        }
    }
}}}}
