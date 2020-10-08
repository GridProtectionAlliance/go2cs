// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package loader -- go2cs converted at 2020 October 08 04:37:52 UTC
// import "cmd/link/internal/loader" ==> using loader = go.cmd.link.@internal.loader_package
// Original source: C:\Go\src\cmd\link\internal\loader\loader.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using bits = go.math.bits_package;
using os = go.os_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
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
            public slice<goobj2.Reloc> rs;
            public long li; // local index of symbol whose relocs we're examining
            public ptr<oReader> r; // object reader for containing package
            public ptr<Loader> l; // loader
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

        // ExtReloc contains the payload for an external relocation.
        public partial struct ExtReloc
        {
            public long Idx; // index of the original relocation
            public Sym Xsym;
            public long Xadd;
        }

        // ExtRelocView is a view of an external relocation.
        // It is intended to be constructed on the fly, such as ExtRelocs.At.
        // It is not the data structure used to store the payload internally.
        public partial struct ExtRelocView
        {
            public ref Reloc2 Reloc2 => ref Reloc2_val;
            public ref ptr<ExtReloc> ptr<ExtReloc> => ref ptr<ExtReloc>_ptr;
        }

        // Reloc2 holds a "handle" to access a relocation record from an
        // object file.
        public partial struct Reloc2
        {
            public ref ptr<goobj2.Reloc> Reloc> => ref Reloc>_ptr;
            public ptr<oReader> r;
            public ptr<Loader> l; // External reloc types may not fit into a uint8 which the Go object file uses.
// Store it here, instead of in the byte of goobj2.Reloc2.
// For Go symbols this will always be zero.
// goobj2.Reloc2.Type() + typ is always the right type, for both Go and external
// symbols.
            public objabi.RelocType typ;
        }

        public static objabi.RelocType Type(this Reloc2 rel)
        {
            return objabi.RelocType(rel.Reloc.Type()) + rel.typ;
        }
        public static Sym Sym(this Reloc2 rel)
        {
            return rel.l.resolve(rel.r, rel.Reloc.Sym());
        }
        public static void SetSym(this Reloc2 rel, Sym s)
        {
            rel.Reloc.SetSym(new goobj2.SymRef(PkgIdx:0,SymIdx:uint32(s)));
        }

        public static void SetType(this Reloc2 rel, objabi.RelocType t) => func((_, panic, __) =>
        {
            if (t != objabi.RelocType(uint8(t)))
            {
                panic("SetType: type doesn't fit into Reloc2");
            }

            rel.Reloc.SetType(uint8(t));
            if (rel.typ != 0L)
            { 
                // should use SymbolBuilder.SetRelocType
                panic("wrong method to set reloc type");

            }

        });

        // Aux2 holds a "handle" to access an aux symbol record from an
        // object file.
        public partial struct Aux2
        {
            public ref ptr<goobj2.Aux> Aux> => ref Aux>_ptr;
            public ptr<oReader> r;
            public ptr<Loader> l;
        }

        public static Sym Sym(this Aux2 a)
        {
            return a.l.resolve(a.r, a.Aux.Sym());
        }

        // oReader is a wrapper type of obj.Reader, along with some
        // extra information.
        // TODO: rename to objReader once the old one is gone?
        private partial struct oReader
        {
            public ref ptr<goobj2.Reader> Reader> => ref Reader>_ptr;
            public ptr<sym.CompilationUnit> unit;
            public long version; // version of static symbol
            public uint flags; // read from object file
            public @string pkgprefix;
            public slice<Sym> syms; // Sym's global index, indexed by local index
            public long ndef; // cache goobj2.Reader.NSym()
            public uint objidx; // index of this reader in the objs slice
        }

        private partial struct objIdx
        {
            public ptr<oReader> r;
            public Sym i; // start index
        }

        // objSym represents a symbol in an object file. It is a tuple of
        // the object and the symbol's local index.
        // For external symbols, r is l.extReader, s is its index into the
        // payload array.
        // {nil, 0} represents the nil symbol.
        private partial struct objSym
        {
            public ptr<oReader> r;
            public long s; // local index
        }

        private partial struct nameVer
        {
            public @string name;
            public long v;
        }

        public partial struct Bitmap // : slice<uint>
        {
        }

        // set the i-th bit.
        public static void Set(this Bitmap bm, Sym i)
        {
            var n = uint(i) / 32L;
            var r = uint(i) % 32L;
            bm[n] |= 1L << (int)(r);

        }

        // unset the i-th bit.
        public static void Unset(this Bitmap bm, Sym i)
        {
            var n = uint(i) / 32L;
            var r = uint(i) % 32L;
            bm[n] &= (1L << (int)(r));

        }

        // whether the i-th bit is set.
        public static bool Has(this Bitmap bm, Sym i)
        {
            var n = uint(i) / 32L;
            var r = uint(i) % 32L;
            return bm[n] & (1L << (int)(r)) != 0L;

        }

        // return current length of bitmap in bits.
        public static long Len(this Bitmap bm)
        {
            return len(bm) * 32L;
        }

        // return the number of bits set.
        public static long Count(this Bitmap bm)
        {
            long s = 0L;
            foreach (var (_, x) in bm)
            {
                s += bits.OnesCount32(x);
            }
            return s;

        }

        public static Bitmap MakeBitmap(long n)
        {
            return make(Bitmap, (n + 31L) / 32L);
        }

        // growBitmap insures that the specified bitmap has enough capacity,
        // reallocating (doubling the size) if needed.
        private static Bitmap growBitmap(long reqLen, Bitmap b)
        {
            var curLen = b.Len();
            if (reqLen > curLen)
            {
                b = append(b, MakeBitmap(reqLen + 1L - curLen));
            }

            return b;

        }

        // A Loader loads new object files and resolves indexed symbol references.
        //
        // Notes on the layout of global symbol index space:
        //
        // - Go object files are read before host object files; each Go object
        //   read adds its defined package symbols to the global index space.
        //   Nonpackage symbols are not yet added.
        //
        // - In loader.LoadNonpkgSyms, add non-package defined symbols and
        //   references in all object files to the global index space.
        //
        // - Host object file loading happens; the host object loader does a
        //   name/version lookup for each symbol it finds; this can wind up
        //   extending the external symbol index space range. The host object
        //   loader stores symbol payloads in loader.payloads using SymbolBuilder.
        //
        // - For now, in loader.LoadFull we convert all symbols (Go + external)
        //   to sym.Symbols.
        //
        // - At some point (when the wayfront is pushed through all of the
        //   linker), all external symbols will be payload-based, and we can
        //   get rid of the loader.Syms array.
        //
        // - Each symbol gets a unique global index. For duplicated and
        //   overwriting/overwritten symbols, the second (or later) appearance
        //   of the symbol gets the same global index as the first appearance.
        public partial struct Loader
        {
            public map<ptr<oReader>, Sym> start; // map from object file to its start index
            public slice<objIdx> objs; // sorted by start index (i.e. objIdx.i)
            public Sym extStart; // from this index on, the symbols are externally defined
            public slice<Sym> builtinSyms; // global index of builtin symbols

            public slice<objSym> objSyms; // global index mapping to local index

            public array<map<@string, Sym>> symsByName; // map symbol name to index, two maps are for ABI0 and ABIInternal
            public map<nameVer, Sym> extStaticSyms; // externally defined static symbols, keyed by name

            public ptr<oReader> extReader; // a dummy oReader, for external symbols
            public slice<extSymPayload> payloadBatch;
            public slice<ptr<extSymPayload>> payloads; // contents of linker-materialized external syms
            public slice<long> values; // symbol values, indexed by global sym index

            public slice<ptr<sym.Section>> sects; // sections
            public slice<ushort> symSects; // symbol's section, index to sects array

            public slice<byte> align; // symbol 2^N alignment, indexed by global index

            public slice<slice<byte>> outdata; // symbol's data in the output buffer
            public slice<slice<ExtReloc>> extRelocs; // symbol's external relocations

            public map<Sym, bool> deferReturnTramp; // whether the symbol is a trampoline of a deferreturn call

            public map<@string, ptr<oReader>> objByPkg; // map package path to its Go object reader

            public slice<ptr<sym.Symbol>> Syms; // indexed symbols. XXX we still make sym.Symbol for now.
            public slice<sym.Symbol> symBatch; // batch of symbols.

            public long anonVersion; // most recently assigned ext static sym pseudo-version

// Bitmaps and other side structures used to store data used to store
// symbol flags/attributes; these are to be accessed via the
// corresponding loader "AttrXXX" and "SetAttrXXX" methods. Please
// visit the comments on these methods for more details on the
// semantics / interpretation of the specific flags or attribute.
            public Bitmap attrReachable; // reachable symbols, indexed by global index
            public Bitmap attrOnList; // "on list" symbols, indexed by global index
            public Bitmap attrLocal; // "local" symbols, indexed by global index
            public Bitmap attrNotInSymbolTable; // "not in symtab" symbols, indexed by glob idx
            public Bitmap attrVisibilityHidden; // hidden symbols, indexed by ext sym index
            public Bitmap attrDuplicateOK; // dupOK symbols, indexed by ext sym index
            public Bitmap attrShared; // shared symbols, indexed by ext sym index
            public Bitmap attrExternal; // external symbols, indexed by ext sym index

            public map<Sym, bool> attrReadOnly; // readonly data for this sym
            public map<Sym, Sym> outer;
            public map<Sym, Sym> sub;
            public map<Sym, @string> dynimplib; // stores Dynimplib symbol attribute
            public map<Sym, @string> dynimpvers; // stores Dynimpvers symbol attribute
            public map<Sym, byte> localentry; // stores Localentry symbol attribute
            public map<Sym, @string> extname; // stores Extname symbol attribute
            public map<Sym, elf.SymType> elfType; // stores elf type symbol property
            public map<Sym, int> elfSym; // stores elf sym symbol property
            public map<Sym, int> localElfSym; // stores "local" elf sym symbol property
            public map<Sym, @string> symPkg; // stores package for symbol, or library for shlib-derived syms
            public map<Sym, int> plt; // stores dynimport for pe objects
            public map<Sym, int> got; // stores got for pe objects
            public map<Sym, int> dynid; // stores Dynid for symbol

            public map<relocId, sym.RelocVariant> relocVariant; // stores variant relocs

// Used to implement field tracking; created during deadcode if
// field tracking is enabled. Reachparent[K] contains the index of
// the symbol that triggered the marking of symbol K as live.
            public slice<Sym> Reachparent;
            public slice<sym.Reloc> relocBatch; // for bulk allocation of relocations
            public slice<sym.RelocExt> relocExtBatch; // for bulk allocation of relocations

            public uint flags;
            public long strictDupMsgs; // number of strict-dup warning/errors, when FlagStrictDups is enabled

            public elfsetstringFunc elfsetstring;
            public ptr<ErrorReporter> errorReporter;
            public Func<@string, long, ptr<sym.Symbol>> SymLookup;
        }

        private static readonly var pkgDef = (var)iota;
        private static readonly var nonPkgDef = (var)0;
        private static readonly var nonPkgRef = (var)1;


        public delegate void elfsetstringFunc(ptr<sym.Symbol>, @string, long);

        // extSymPayload holds the payload (data + relocations) for linker-synthesized
        // external symbols (note that symbol value is stored in a separate slice).
        private partial struct extSymPayload
        {
            public @string name; // TODO: would this be better as offset into str table?
            public long size;
            public long ver;
            public sym.SymKind kind;
            public uint objidx; // index of original object if sym made by cloneToExternal
            public Sym gotype; // Gotype (0 if not present)
            public slice<goobj2.Reloc> relocs;
            public slice<objabi.RelocType> reltypes; // relocation types
            public slice<byte> data;
            public slice<goobj2.Aux> auxs;
        }

 
        // Loader.flags
        public static readonly long FlagStrictDups = (long)1L << (int)(iota);


        public static ptr<Loader> NewLoader(uint flags, elfsetstringFunc elfsetstring, ptr<ErrorReporter> _addr_reporter)
        {
            ref ErrorReporter reporter = ref _addr_reporter.val;

            var nbuiltin = goobj2.NBuiltin();
            ptr<Loader> ldr = addr(new Loader(start:make(map[*oReader]Sym),objs:[]objIdx{{}},objSyms:[]objSym{{}},extReader:&oReader{},symsByName:[2]map[string]Sym{make(map[string]Sym,100000),make(map[string]Sym,50000)},objByPkg:make(map[string]*oReader),outer:make(map[Sym]Sym),sub:make(map[Sym]Sym),dynimplib:make(map[Sym]string),dynimpvers:make(map[Sym]string),localentry:make(map[Sym]uint8),extname:make(map[Sym]string),attrReadOnly:make(map[Sym]bool),elfType:make(map[Sym]elf.SymType),elfSym:make(map[Sym]int32),localElfSym:make(map[Sym]int32),symPkg:make(map[Sym]string),plt:make(map[Sym]int32),got:make(map[Sym]int32),dynid:make(map[Sym]int32),attrTopFrame:make(map[Sym]struct{}),attrSpecial:make(map[Sym]struct{}),attrCgoExportDynamic:make(map[Sym]struct{}),attrCgoExportStatic:make(map[Sym]struct{}),itablink:make(map[Sym]struct{}),deferReturnTramp:make(map[Sym]bool),extStaticSyms:make(map[nameVer]Sym),builtinSyms:make([]Sym,nbuiltin),flags:flags,elfsetstring:elfsetstring,errorReporter:reporter,sects:[]*sym.Section{nil},));
            reporter.ldr = ldr;
            return _addr_ldr!;
        }

        // Add object file r, return the start index.
        private static Sym addObj(this ptr<Loader> _addr_l, @string pkg, ptr<oReader> _addr_r) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            {
                var (_, ok) = l.start[r];

                if (ok)
                {
                    panic("already added");
                }

            }

            pkg = objabi.PathToPrefix(pkg); // the object file contains escaped package path
            {
                (_, ok) = l.objByPkg[pkg];

                if (!ok)
                {
                    l.objByPkg[pkg] = r;
                }

            }

            var i = Sym(len(l.objSyms));
            l.start[r] = i;
            l.objs = append(l.objs, new objIdx(r,i));
            return i;

        });

        // Add a symbol from an object file, return the global index and whether it is added.
        // If the symbol already exist, it returns the index of that symbol.
        private static (Sym, bool) AddSym(this ptr<Loader> _addr_l, @string name, long ver, ptr<oReader> _addr_r, long li, long kind, bool dupok, sym.SymKind typ) => func((_, panic, __) =>
        {
            Sym _p0 = default;
            bool _p0 = default;
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            if (l.extStart != 0L)
            {
                panic("AddSym called after external symbol is created");
            }

            var i = Sym(len(l.objSyms));
            Action addToGlobal = () =>
            {
                l.objSyms = append(l.objSyms, new objSym(r,li));
            }
;
            if (name == "")
            {
                addToGlobal();
                return (i, true); // unnamed aux symbol
            }

            if (ver == r.version)
            { 
                // Static symbol. Add its global index but don't
                // add to name lookup table, as it cannot be
                // referenced by name.
                addToGlobal();
                return (i, true);

            }

            if (kind == pkgDef)
            { 
                // Defined package symbols cannot be dup to each other.
                // We load all the package symbols first, so we don't need
                // to check dup here.
                // We still add it to the lookup table, as it may still be
                // referenced by name (e.g. through linkname).
                l.symsByName[ver][name] = i;
                addToGlobal();
                return (i, true);

            } 

            // Non-package (named) symbol. Check if it already exists.
            var (oldi, existed) = l.symsByName[ver][name];
            if (!existed)
            {
                l.symsByName[ver][name] = i;
                addToGlobal();
                return (i, true);
            } 
            // symbol already exists
            if (dupok)
            {
                if (l.flags & FlagStrictDups != 0L)
                {
                    l.checkdup(name, r, li, oldi);
                }

                return (oldi, false);

            }

            var (oldr, oldli) = l.toLocal(oldi);
            var oldsym = oldr.Sym(oldli);
            if (oldsym.Dupok())
            {
                return (oldi, false);
            }

            var overwrite = r.DataSize(li) != 0L;
            if (overwrite)
            { 
                // new symbol overwrites old symbol.
                var oldtyp = sym.AbiSymKindToSymKind[objabi.SymKind(oldsym.Type())];
                if (!(oldtyp.IsData() && oldr.DataSize(oldli) == 0L))
                {
                    log.Fatalf("duplicated definition of symbol " + name);
                }

                l.objSyms[oldi] = new objSym(r,li);

            }
            else
            { 
                // old symbol overwrites new symbol.
                if (!typ.IsData())
                { // only allow overwriting data symbol
                    log.Fatalf("duplicated definition of symbol " + name);

                }

            }

            return (oldi, true);

        });

        // newExtSym creates a new external sym with the specified
        // name/version.
        private static Sym newExtSym(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            var i = Sym(len(l.objSyms));
            if (l.extStart == 0L)
            {
                l.extStart = i;
            }

            l.growSyms(int(i));
            var pi = l.newPayload(name, ver);
            l.objSyms = append(l.objSyms, new objSym(l.extReader,int(pi)));
            l.extReader.syms = append(l.extReader.syms, i);
            return i;

        }

        // LookupOrCreateSym looks up the symbol with the specified name/version,
        // returning its Sym index if found. If the lookup fails, a new external
        // Sym will be created, entered into the lookup tables, and returned.
        private static Sym LookupOrCreateSym(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            var i = l.Lookup(name, ver);
            if (i != 0L)
            {
                return i;
            }

            i = l.newExtSym(name, ver);
            var @static = ver >= sym.SymVerStatic || ver < 0L;
            if (static)
            {
                l.extStaticSyms[new nameVer(name,ver)] = i;
            }
            else
            {
                l.symsByName[ver][name] = i;
            }

            return i;

        }

        private static bool IsExternal(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (r, _) = l.toLocal(i);
            return l.isExtReader(r);
        }

        private static bool isExtReader(this ptr<Loader> _addr_l, ptr<oReader> _addr_r)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            return r == l.extReader;
        }

        // For external symbol, return its index in the payloads array.
        // XXX result is actually not a global index. We (ab)use the Sym type
        // so we don't need conversion for accessing bitmaps.
        private static Sym extIndex(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (_, li) = l.toLocal(i);
            return Sym(li);
        }

        // Get a new payload for external symbol, return its index in
        // the payloads array.
        private static long newPayload(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            var pi = len(l.payloads);
            var pp = l.allocPayload();
            pp.name = name;
            pp.ver = ver;
            l.payloads = append(l.payloads, pp);
            l.growExtAttrBitmaps();
            return pi;
        }

        // getPayload returns a pointer to the extSymPayload struct for an
        // external symbol if the symbol has a payload. Will panic if the
        // symbol in question is bogus (zero or not an external sym).
        private static ptr<extSymPayload> getPayload(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                panic(fmt.Sprintf("bogus symbol index %d in getPayload", i));
            }

            var pi = l.extIndex(i);
            return _addr_l.payloads[pi]!;

        });

        // allocPayload allocates a new payload.
        private static ptr<extSymPayload> allocPayload(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            var batch = l.payloadBatch;
            if (len(batch) == 0L)
            {
                batch = make_slice<extSymPayload>(1000L);
            }

            var p = _addr_batch[0L];
            l.payloadBatch = batch[1L..];
            return _addr_p!;

        }

        private static void Grow(this ptr<extSymPayload> _addr_ms, long siz)
        {
            ref extSymPayload ms = ref _addr_ms.val;

            if (int64(int(siz)) != siz)
            {
                log.Fatalf("symgrow size %d too long", siz);
            }

            if (int64(len(ms.data)) >= siz)
            {
                return ;
            }

            if (cap(ms.data) < int(siz))
            {
                var cl = len(ms.data);
                ms.data = append(ms.data, make_slice<byte>(int(siz) + 1L - cl));
                ms.data = ms.data[0L..cl];
            }

            ms.data = ms.data[..siz];

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
            l.growValues(int(i) + 1L);
            l.growAttrBitmaps(int(i) + 1L);

        }

        // Convert a local index to a global index.
        private static Sym toGlobal(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, long i)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            return r.syms[i];
        }

        // Convert a global index to a local index.
        private static (ptr<oReader>, long) toLocal(this ptr<Loader> _addr_l, Sym i)
        {
            ptr<oReader> _p0 = default!;
            long _p0 = default;
            ref Loader l = ref _addr_l.val;

            return (_addr_l.objSyms[i].r!, int(l.objSyms[i].s));
        }

        // Resolve a local symbol reference. Return global index.
        private static Sym resolve(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, goobj2.SymRef s) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            ptr<oReader> rr;
            {
                var p = s.PkgIdx;


                if (p == goobj2.PkgIdxInvalid) 
                    // {0, X} with non-zero X is never a valid sym reference from a Go object.
                    // We steal this space for symbol references from external objects.
                    // In this case, X is just the global index.
                    if (l.isExtReader(r))
                    {
                        return Sym(s.SymIdx);
                    }

                    if (s.SymIdx != 0L)
                    {
                        panic("bad sym ref");
                    }

                    return 0L;
                else if (p == goobj2.PkgIdxNone) 
                    var i = int(s.SymIdx) + r.ndef;
                    return r.syms[i];
                else if (p == goobj2.PkgIdxBuiltin) 
                    return l.builtinSyms[s.SymIdx];
                else if (p == goobj2.PkgIdxSelf) 
                    rr = r;
                else 
                    var pkg = r.Pkg(int(p));
                    bool ok = default;
                    rr, ok = l.objByPkg[pkg];
                    if (!ok)
                    {
                        log.Fatalf("reference of nonexisted package %s, from %v", pkg, r.unit.Lib);
                    }


            }
            return l.toGlobal(rr, int(s.SymIdx));

        });

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

        // Check that duplicate symbols have same contents.
        private static void checkdup(this ptr<Loader> _addr_l, @string name, ptr<oReader> _addr_r, long li, Sym dup)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            var p = r.Data(li);
            var (rdup, ldup) = l.toLocal(dup);
            var pdup = rdup.Data(ldup);
            if (bytes.Equal(p, pdup))
            {
                return ;
            }

            @string reason = "same length but different contents";
            if (len(p) != len(pdup))
            {
                reason = fmt.Sprintf("new length %d != old length %d", len(p), len(pdup));
            }

            fmt.Fprintf(os.Stderr, "cmd/link: while reading object for '%v': duplicate symbol '%s', previous def at '%v', with mismatched payload: %s\n", r.unit.Lib, name, rdup.unit.Lib, reason); 

            // For the moment, allow DWARF subprogram DIEs for
            // auto-generated wrapper functions. What seems to happen
            // here is that we get different line numbers on formal
            // params; I am guessing that the pos is being inherited
            // from the spot where the wrapper is needed.
            var allowed = strings.HasPrefix(name, "go.info.go.interface") || strings.HasPrefix(name, "go.info.go.builtin") || strings.HasPrefix(name, "go.debuglines");
            if (!allowed)
            {
                l.strictDupMsgs++;
            }

        }

        private static long NStrictDupMsgs(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return l.strictDupMsgs;
        }

        // Number of total symbols.
        private static long NSym(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return len(l.objSyms);
        }

        // Number of defined Go symbols.
        private static long NDef(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return int(l.extStart);
        }

        // Number of reachable symbols.
        private static long NReachableSym(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return l.attrReachable.Count();
        }

        // Returns the raw (unpatched) name of the i-th symbol.
        private static @string RawSymName(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                return pp.name;
            }

            var (r, li) = l.toLocal(i);
            return r.Sym(li).Name(r.Reader);

        }

        // Returns the (patched) name of the i-th symbol.
        private static @string SymName(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                return pp.name;
            }

            var (r, li) = l.toLocal(i);
            return strings.Replace(r.Sym(li).Name(r.Reader), "\"\".", r.pkgprefix, -1L);

        }

        // Returns the version of the i-th symbol.
        private static long SymVersion(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                return pp.ver;
            }

            var (r, li) = l.toLocal(i);
            return int(abiToVer(r.Sym(li).ABI(), r.version));

        }

        // Returns the type of the i-th symbol.
        private static sym.SymKind SymType(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp != null)
                {
                    return pp.kind;
                }

                return 0L;

            }

            var (r, li) = l.toLocal(i);
            return sym.AbiSymKindToSymKind[objabi.SymKind(r.Sym(li).Type())];

        }

        // Returns the attributes of the i-th symbol.
        private static byte SymAttr(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            { 
                // TODO: do something? External symbols have different representation of attributes.
                // For now, ReflectMethod, NoSplit, GoType, and Typelink are used and they cannot be
                // set by external symbol.
                return 0L;

            }

            var (r, li) = l.toLocal(i);
            return r.Sym(li).Flag();

        }

        // Returns the size of the i-th symbol.
        private static long SymSize(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                return pp.size;
            }

            var (r, li) = l.toLocal(i);
            return int64(r.Sym(li).Siz());

        }

        // AttrReachable returns true for symbols that are transitively
        // referenced from the entry points. Unreachable symbols are not
        // written to the output.
        private static bool AttrReachable(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.attrReachable.Has(i);
        }

        // SetAttrReachable sets the reachability property for a symbol (see
        // AttrReachable).
        private static void SetAttrReachable(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrReachable.Set(i);
            }
            else
            {
                l.attrReachable.Unset(i);
            }

        }

        // AttrOnList returns true for symbols that are on some list (such as
        // the list of all text symbols, or one of the lists of data symbols)
        // and is consulted to avoid bugs where a symbol is put on a list
        // twice.
        private static bool AttrOnList(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.attrOnList.Has(i);
        }

        // SetAttrOnList sets the "on list" property for a symbol (see
        // AttrOnList).
        private static void SetAttrOnList(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrOnList.Set(i);
            }
            else
            {
                l.attrOnList.Unset(i);
            }

        }

        // AttrLocal returns true for symbols that are only visible within the
        // module (executable or shared library) being linked. This attribute
        // is applied to thunks and certain other linker-generated symbols.
        private static bool AttrLocal(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.attrLocal.Has(i);
        }

        // SetAttrLocal the "local" property for a symbol (see AttrLocal above).
        private static void SetAttrLocal(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrLocal.Set(i);
            }
            else
            {
                l.attrLocal.Unset(i);
            }

        }

        // SymAddr checks that a symbol is reachable, and returns its value.
        private static long SymAddr(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.AttrReachable(i))
            {
                panic("unreachable symbol in symaddr");
            }

            return l.values[i];

        });

        // AttrNotInSymbolTable returns true for symbols that should not be
        // added to the symbol table of the final generated load module.
        private static bool AttrNotInSymbolTable(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.attrNotInSymbolTable.Has(i);
        }

        // SetAttrNotInSymbolTable the "not in symtab" property for a symbol
        // (see AttrNotInSymbolTable above).
        private static void SetAttrNotInSymbolTable(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrNotInSymbolTable.Set(i);
            }
            else
            {
                l.attrNotInSymbolTable.Unset(i);
            }

        }

        // AttrVisibilityHidden symbols returns true for ELF symbols with
        // visibility set to STV_HIDDEN. They become local symbols in
        // the final executable. Only relevant when internally linking
        // on an ELF platform.
        private static bool AttrVisibilityHidden(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                return false;
            }

            return l.attrVisibilityHidden.Has(l.extIndex(i));

        }

        // SetAttrVisibilityHidden sets the "hidden visibility" property for a
        // symbol (see AttrVisibilityHidden).
        private static void SetAttrVisibilityHidden(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                panic("tried to set visibility attr on non-external symbol");
            }

            if (v)
            {
                l.attrVisibilityHidden.Set(l.extIndex(i));
            }
            else
            {
                l.attrVisibilityHidden.Unset(l.extIndex(i));
            }

        });

        // AttrDuplicateOK returns true for a symbol that can be present in
        // multiple object files.
        private static bool AttrDuplicateOK(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            { 
                // TODO: if this path winds up being taken frequently, it
                // might make more sense to copy the flag value out of the object
                // into a larger bitmap during preload.
                var (r, li) = l.toLocal(i);
                return r.Sym(li).Dupok();

            }

            return l.attrDuplicateOK.Has(l.extIndex(i));

        }

        // SetAttrDuplicateOK sets the "duplicate OK" property for an external
        // symbol (see AttrDuplicateOK).
        private static void SetAttrDuplicateOK(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                panic("tried to set dupok attr on non-external symbol");
            }

            if (v)
            {
                l.attrDuplicateOK.Set(l.extIndex(i));
            }
            else
            {
                l.attrDuplicateOK.Unset(l.extIndex(i));
            }

        });

        // AttrShared returns true for symbols compiled with the -shared option.
        private static bool AttrShared(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            { 
                // TODO: if this path winds up being taken frequently, it
                // might make more sense to copy the flag value out of the
                // object into a larger bitmap during preload.
                var (r, _) = l.toLocal(i);
                return (r.Flags() & goobj2.ObjFlagShared) != 0L;

            }

            return l.attrShared.Has(l.extIndex(i));

        }

        // SetAttrShared sets the "shared" property for an external
        // symbol (see AttrShared).
        private static void SetAttrShared(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                panic(fmt.Sprintf("tried to set shared attr on non-external symbol %d %s", i, l.SymName(i)));
            }

            if (v)
            {
                l.attrShared.Set(l.extIndex(i));
            }
            else
            {
                l.attrShared.Unset(l.extIndex(i));
            }

        });

        // AttrExternal returns true for function symbols loaded from host
        // object files.
        private static bool AttrExternal(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                return false;
            }

            return l.attrExternal.Has(l.extIndex(i));

        }

        // SetAttrExternal sets the "external" property for an host object
        // symbol (see AttrExternal).
        private static void SetAttrExternal(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(i))
            {
                panic(fmt.Sprintf("tried to set external attr on non-external symbol %q", l.RawSymName(i)));
            }

            if (v)
            {
                l.attrExternal.Set(l.extIndex(i));
            }
            else
            {
                l.attrExternal.Unset(l.extIndex(i));
            }

        });

        // AttrTopFrame returns true for a function symbol that is an entry
        // point, meaning that unwinders should stop when they hit this
        // function.
        private static bool AttrTopFrame(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (_, ok) = l.attrTopFrame[i];
            return ok;
        }

        // SetAttrTopFrame sets the "top frame" property for a symbol (see
        // AttrTopFrame).
        private static void SetAttrTopFrame(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrTopFrame[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(l.attrTopFrame, i);
            }

        }

        // AttrSpecial returns true for a symbols that do not have their
        // address (i.e. Value) computed by the usual mechanism of
        // data.go:dodata() & data.go:address().
        private static bool AttrSpecial(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (_, ok) = l.attrSpecial[i];
            return ok;
        }

        // SetAttrSpecial sets the "special" property for a symbol (see
        // AttrSpecial).
        private static void SetAttrSpecial(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrSpecial[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(l.attrSpecial, i);
            }

        }

        // AttrCgoExportDynamic returns true for a symbol that has been
        // specially marked via the "cgo_export_dynamic" compiler directive
        // written by cgo (in response to //export directives in the source).
        private static bool AttrCgoExportDynamic(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (_, ok) = l.attrCgoExportDynamic[i];
            return ok;
        }

        // SetAttrCgoExportDynamic sets the "cgo_export_dynamic" for a symbol
        // (see AttrCgoExportDynamic).
        private static void SetAttrCgoExportDynamic(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrCgoExportDynamic[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(l.attrCgoExportDynamic, i);
            }

        }

        // AttrCgoExportStatic returns true for a symbol that has been
        // specially marked via the "cgo_export_static" directive
        // written by cgo.
        private static bool AttrCgoExportStatic(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            var (_, ok) = l.attrCgoExportStatic[i];
            return ok;
        }

        // SetAttrCgoExportStatic sets the "cgo_export_static" for a symbol
        // (see AttrCgoExportStatic).
        private static void SetAttrCgoExportStatic(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            if (v)
            {
                l.attrCgoExportStatic[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(l.attrCgoExportStatic, i);
            }

        }

        private static bool AttrCgoExport(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.AttrCgoExportDynamic(i) || l.AttrCgoExportStatic(i);
        }

        // AttrReadOnly returns true for a symbol whose underlying data
        // is stored via a read-only mmap.
        private static bool AttrReadOnly(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (v, ok) = l.attrReadOnly[i];

                if (ok)
                {
                    return v;
                }

            }

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp.objidx != 0L)
                {
                    return l.objs[pp.objidx].r.ReadOnly();
                }

                return false;

            }

            var (r, _) = l.toLocal(i);
            return r.ReadOnly();

        }

        // SetAttrReadOnly sets the "data is read only" property for a symbol
        // (see AttrReadOnly).
        private static void SetAttrReadOnly(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            l.attrReadOnly[i] = v;
        }

        // AttrSubSymbol returns true for symbols that are listed as a
        // sub-symbol of some other outer symbol. The sub/outer mechanism is
        // used when loading host objects (sections from the host object
        // become regular linker symbols and symbols go on the Sub list of
        // their section) and for constructing the global offset table when
        // internally linking a dynamic executable.
        //
        // Note that in later stages of the linker, we set Outer(S) to some
        // container symbol C, but don't set Sub(C). Thus we have two
        // distinct scenarios:
        //
        // - Outer symbol covers the address ranges of its sub-symbols.
        //   Outer.Sub is set in this case.
        // - Outer symbol doesn't conver the address ranges. It is zero-sized
        //   and doesn't have sub-symbols. In the case, the inner symbol is
        //   not actually a "SubSymbol". (Tricky!)
        //
        // This method returns TRUE only for sub-symbols in the first scenario.
        //
        // FIXME: would be better to do away with this and have a better way
        // to represent container symbols.

        private static bool AttrSubSymbol(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;
 
            // we don't explicitly store this attribute any more -- return
            // a value based on the sub-symbol setting.
            var o = l.OuterSym(i);
            if (o == 0L)
            {
                return false;
            }

            return l.SubSym(o) != 0L;

        }

        // Note that we don't have a 'SetAttrSubSymbol' method in the loader;
        // clients should instead use the PrependSub method to establish
        // outer/sub relationships for host object symbols.

        // Returns whether the i-th symbol has ReflectMethod attribute set.
        private static bool IsReflectMethod(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.SymAttr(i) & goobj2.SymFlagReflectMethod != 0L;
        }

        // Returns whether the i-th symbol is nosplit.
        private static bool IsNoSplit(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.SymAttr(i) & goobj2.SymFlagNoSplit != 0L;
        }

        // Returns whether this is a Go type symbol.
        private static bool IsGoType(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.SymAttr(i) & goobj2.SymFlagGoType != 0L;
        }

        // Returns whether this symbol should be included in typelink.
        private static bool IsTypelink(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.SymAttr(i) & goobj2.SymFlagTypelink != 0L;
        }

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

        // Return whether this is a trampoline of a deferreturn call.
        private static bool IsDeferReturnTramp(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.deferReturnTramp[i];
        }

        // Set that i is a trampoline of a deferreturn call.
        private static void SetIsDeferReturnTramp(this ptr<Loader> _addr_l, Sym i, bool v)
        {
            ref Loader l = ref _addr_l.val;

            l.deferReturnTramp[i] = v;
        }

        // growValues grows the slice used to store symbol values.
        private static void growValues(this ptr<Loader> _addr_l, long reqLen)
        {
            ref Loader l = ref _addr_l.val;

            var curLen = len(l.values);
            if (reqLen > curLen)
            {
                l.values = append(l.values, make_slice<long>(reqLen + 1L - curLen));
            }

        }

        // SymValue returns the value of the i-th symbol. i is global index.
        private static long SymValue(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.values[i];
        }

        // SetSymValue sets the value of the i-th symbol. i is global index.
        private static void SetSymValue(this ptr<Loader> _addr_l, Sym i, long val)
        {
            ref Loader l = ref _addr_l.val;

            l.values[i] = val;
        }

        // AddToSymValue adds to the value of the i-th symbol. i is the global index.
        private static void AddToSymValue(this ptr<Loader> _addr_l, Sym i, long val)
        {
            ref Loader l = ref _addr_l.val;

            l.values[i] += val;
        }

        // Returns the symbol content of the i-th symbol. i is global index.
        private static slice<byte> Data(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp != null)
                {
                    return pp.data;
                }

                return null;

            }

            var (r, li) = l.toLocal(i);
            return r.Data(li);

        }

        // Returns the data of the i-th symbol in the output buffer.
        private static slice<byte> OutData(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (int(i) < len(l.outdata) && l.outdata[i] != null)
            {
                return l.outdata[i];
            }

            return l.Data(i);

        }

        // SetOutData sets the position of the data of the i-th symbol in the output buffer.
        // i is global index.
        private static void SetOutData(this ptr<Loader> _addr_l, Sym i, slice<byte> data)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp != null)
                {
                    pp.data = data;
                    return ;
                }

            }

            l.outdata[i] = data;

        }

        // InitOutData initializes the slice used to store symbol output data.
        private static void InitOutData(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            l.outdata = make_slice<slice<byte>>(l.extStart);
        }

        // SetExtRelocs sets the external relocations of the i-th symbol. i is global index.
        private static void SetExtRelocs(this ptr<Loader> _addr_l, Sym i, slice<ExtReloc> relocs)
        {
            ref Loader l = ref _addr_l.val;

            l.extRelocs[i] = relocs;
        }

        // InitExtRelocs initialize the slice used to store external relocations.
        private static void InitExtRelocs(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            l.extRelocs = make_slice<slice<ExtReloc>>(l.NSym());
        }

        // SymAlign returns the alignment for a symbol.
        private static int SymAlign(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (int(i) >= len(l.align))
            { 
                // align is extended lazily -- it the sym in question is
                // outside the range of the existing slice, then we assume its
                // alignment has not yet been set.
                return 0L;

            } 
            // TODO: would it make sense to return an arch-specific
            // alignment depending on section type? E.g. STEXT => 32,
            // SDATA => 1, etc?
            var abits = l.align[i];
            if (abits == 0L)
            {
                return 0L;
            }

            return int32(1L << (int)((abits - 1L)));

        }

        // SetSymAlign sets the alignment for a symbol.
        private static void SetSymAlign(this ptr<Loader> _addr_l, Sym i, int align) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // Reject nonsense alignments.
            if (align < 0L || align & (align - 1L) != 0L)
            {
                panic("bad alignment value");
            }

            if (int(i) >= len(l.align))
            {
                l.align = append(l.align, make_slice<byte>(l.NSym() - len(l.align)));
            }

            if (align == 0L)
            {
                l.align[i] = 0L;
            }

            l.align[i] = uint8(bits.Len32(uint32(align)));

        });

        // SymValue returns the section of the i-th symbol. i is global index.
        private static ptr<sym.Section> SymSect(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (int(i) >= len(l.symSects))
            { 
                // symSects is extended lazily -- it the sym in question is
                // outside the range of the existing slice, then we assume its
                // section has not yet been set.
                return _addr_null!;

            }

            return _addr_l.sects[l.symSects[i]]!;

        }

        // SetSymValue sets the section of the i-th symbol. i is global index.
        private static void SetSymSect(this ptr<Loader> _addr_l, Sym i, ptr<sym.Section> _addr_sect)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Section sect = ref _addr_sect.val;

            if (int(i) >= len(l.symSects))
            {
                l.symSects = append(l.symSects, make_slice<ushort>(l.NSym() - len(l.symSects)));
            }

            l.symSects[i] = sect.Index;

        }

        // growSects grows the slice used to store symbol sections.
        private static void growSects(this ptr<Loader> _addr_l, long reqLen)
        {
            ref Loader l = ref _addr_l.val;

            var curLen = len(l.symSects);
            if (reqLen > curLen)
            {
                l.symSects = append(l.symSects, make_slice<ushort>(reqLen + 1L - curLen));
            }

        }

        // NewSection creates a new (output) section.
        private static ptr<sym.Section> NewSection(this ptr<Loader> _addr_l) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            ptr<sym.Section> sect = @new<sym.Section>();
            var idx = len(l.sects);
            if (idx != int(uint16(idx)))
            {
                panic("too many sections created");
            }

            sect.Index = uint16(idx);
            l.sects = append(l.sects, sect);
            return _addr_sect!;

        });

        // SymDynImplib returns the "dynimplib" attribute for the specified
        // symbol, making up a portion of the info for a symbol specified
        // on a "cgo_import_dynamic" compiler directive.
        private static @string SymDynimplib(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.dynimplib[i];
        }

        // SetSymDynimplib sets the "dynimplib" attribute for a symbol.
        private static void SetSymDynimplib(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetDynimplib");
            }

            if (value == "")
            {
                delete(l.dynimplib, i);
            }
            else
            {
                l.dynimplib[i] = value;
            }

        });

        // SymDynimpvers returns the "dynimpvers" attribute for the specified
        // symbol, making up a portion of the info for a symbol specified
        // on a "cgo_import_dynamic" compiler directive.
        private static @string SymDynimpvers(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.dynimpvers[i];
        }

        // SetSymDynimpvers sets the "dynimpvers" attribute for a symbol.
        private static void SetSymDynimpvers(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetDynimpvers");
            }

            if (value == "")
            {
                delete(l.dynimpvers, i);
            }
            else
            {
                l.dynimpvers[i] = value;
            }

        });

        // SymExtname returns the "extname" value for the specified
        // symbol.
        private static @string SymExtname(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (s, ok) = l.extname[i];

                if (ok)
                {
                    return s;
                }

            }

            return l.SymName(i);

        }

        // SetSymExtname sets the  "extname" attribute for a symbol.
        private static void SetSymExtname(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetExtname");
            }

            if (value == "")
            {
                delete(l.extname, i);
            }
            else
            {
                l.extname[i] = value;
            }

        });

        // SymElfType returns the previously recorded ELF type for a symbol
        // (used only for symbols read from shared libraries by ldshlibsyms).
        // It is not set for symbols defined by the packages being linked or
        // by symbols read by ldelf (and so is left as elf.STT_NOTYPE).
        private static elf.SymType SymElfType(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (et, ok) = l.elfType[i];

                if (ok)
                {
                    return et;
                }

            }

            return elf.STT_NOTYPE;

        }

        // SetSymElfType sets the elf type attribute for a symbol.
        private static void SetSymElfType(this ptr<Loader> _addr_l, Sym i, elf.SymType et) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetSymElfType");
            }

            if (et == elf.STT_NOTYPE)
            {
                delete(l.elfType, i);
            }
            else
            {
                l.elfType[i] = et;
            }

        });

        // SymElfSym returns the ELF symbol index for a given loader
        // symbol, assigned during ELF symtab generation.
        private static int SymElfSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.elfSym[i];
        }

        // SetSymElfSym sets the elf symbol index for a symbol.
        private static void SetSymElfSym(this ptr<Loader> _addr_l, Sym i, int es) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (i == 0L)
            {
                panic("bad sym index");
            }

            if (es == 0L)
            {
                delete(l.elfSym, i);
            }
            else
            {
                l.elfSym[i] = es;
            }

        });

        // SymLocalElfSym returns the "local" ELF symbol index for a given loader
        // symbol, assigned during ELF symtab generation.
        private static int SymLocalElfSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.localElfSym[i];
        }

        // SetSymLocalElfSym sets the "local" elf symbol index for a symbol.
        private static void SetSymLocalElfSym(this ptr<Loader> _addr_l, Sym i, int es) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (i == 0L)
            {
                panic("bad sym index");
            }

            if (es == 0L)
            {
                delete(l.localElfSym, i);
            }
            else
            {
                l.localElfSym[i] = es;
            }

        });

        // SymPlt returns the plt value for pe symbols.
        private static int SymPlt(this ptr<Loader> _addr_l, Sym s)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (v, ok) = l.plt[s];

                if (ok)
                {
                    return v;
                }

            }

            return -1L;

        }

        // SetPlt sets the plt value for pe symbols.
        private static void SetPlt(this ptr<Loader> _addr_l, Sym i, int v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol for SetPlt");
            }

            if (v == -1L)
            {
                delete(l.plt, i);
            }
            else
            {
                l.plt[i] = v;
            }

        });

        // SymGot returns the got value for pe symbols.
        private static int SymGot(this ptr<Loader> _addr_l, Sym s)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (v, ok) = l.got[s];

                if (ok)
                {
                    return v;
                }

            }

            return -1L;

        }

        // SetGot sets the got value for pe symbols.
        private static void SetGot(this ptr<Loader> _addr_l, Sym i, int v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol for SetGot");
            }

            if (v == -1L)
            {
                delete(l.got, i);
            }
            else
            {
                l.got[i] = v;
            }

        });

        // SymDynid returns the "dynid" property for the specified symbol.
        private static int SymDynid(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (s, ok) = l.dynid[i];

                if (ok)
                {
                    return s;
                }

            }

            return -1L;

        }

        // SetSymDynid sets the "dynid" property for a symbol.
        private static void SetSymDynid(this ptr<Loader> _addr_l, Sym i, int val) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetSymDynid");
            }

            if (val == -1L)
            {
                delete(l.dynid, i);
            }
            else
            {
                l.dynid[i] = val;
            }

        });

        // DynIdSyms returns the set of symbols for which dynID is set to an
        // interesting (non-default) value. This is expected to be a fairly
        // small set.
        private static slice<Sym> DynidSyms(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            var sl = make_slice<Sym>(0L, len(l.dynid));
            foreach (var (s) in l.dynid)
            {
                sl = append(sl, s);
            }
            sort.Slice(sl, (i, j) => sl[i] < sl[j]);
            return sl;

        }

        // SymGoType returns the 'Gotype' property for a given symbol (set by
        // the Go compiler for variable symbols). This version relies on
        // reading aux symbols for the target sym -- it could be that a faster
        // approach would be to check for gotype during preload and copy the
        // results in to a map (might want to try this at some point and see
        // if it helps speed things up).
        private static Sym SymGoType(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                return pp.gotype;
            }

            var (r, li) = l.toLocal(i);
            var auxs = r.Auxs(li);
            foreach (var (j) in auxs)
            {
                var a = _addr_auxs[j];

                if (a.Type() == goobj2.AuxGotype) 
                    return l.resolve(r, a.Sym());
                
            }
            return 0L;

        }

        // SymUnit returns the compilation unit for a given symbol (which will
        // typically be nil for external or linker-manufactured symbols).
        private static ptr<sym.CompilationUnit> SymUnit(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp.objidx != 0L)
                {
                    var r = l.objs[pp.objidx].r;
                    return _addr_r.unit!;
                }

                return _addr_null!;

            }

            var (r, _) = l.toLocal(i);
            return _addr_r.unit!;

        }

        // SymPkg returns the package where the symbol came from (for
        // regular compiler-generated Go symbols), but in the case of
        // building with "-linkshared" (when a symbol is read from a
        // shared library), will hold the library name.
        // NOTE: this correspondes to sym.Symbol.File field.
        private static @string SymPkg(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            {
                var (f, ok) = l.symPkg[i];

                if (ok)
                {
                    return f;
                }

            }

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp.objidx != 0L)
                {
                    var r = l.objs[pp.objidx].r;
                    return r.unit.Lib.Pkg;
                }

                return "";

            }

            var (r, _) = l.toLocal(i);
            return r.unit.Lib.Pkg;

        }

        // SetSymPkg sets the package/library for a symbol. This is
        // needed mainly for external symbols, specifically those imported
        // from shared libraries.
        private static void SetSymPkg(this ptr<Loader> _addr_l, Sym i, @string pkg) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetSymPkg");
            }

            l.symPkg[i] = pkg;

        });

        // SymLocalentry returns the "local entry" value for the specified
        // symbol.
        private static byte SymLocalentry(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return l.localentry[i];
        }

        // SetSymLocalentry sets the "local entry" attribute for a symbol.
        private static void SetSymLocalentry(this ptr<Loader> _addr_l, Sym i, byte value) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // reject bad symbols
            if (i >= Sym(len(l.objSyms)) || i == 0L)
            {
                panic("bad symbol index in SetSymLocalentry");
            }

            if (value == 0L)
            {
                delete(l.localentry, i);
            }
            else
            {
                l.localentry[i] = value;
            }

        });

        // Returns the number of aux symbols given a global index.
        private static long NAux(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                return 0L;
            }

            var (r, li) = l.toLocal(i);
            return r.NAux(li);

        }

        // Returns the "handle" to the j-th aux symbol of the i-th symbol.
        private static Aux2 Aux2(this ptr<Loader> _addr_l, Sym i, long j)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                return new Aux2();
            }

            var (r, li) = l.toLocal(i);
            if (j >= r.NAux(li))
            {
                return new Aux2();
            }

            return new Aux2(r.Aux(li,j),r,l);

        }

        // GetFuncDwarfAuxSyms collects and returns the auxiliary DWARF
        // symbols associated with a given function symbol.  Prior to the
        // introduction of the loader, this was done purely using name
        // lookups, e.f. for function with name XYZ we would then look up
        // go.info.XYZ, etc.
        // FIXME: once all of dwarfgen is converted over to the loader,
        // it would save some space to make these aux symbols nameless.
        private static (Sym, Sym, Sym, Sym) GetFuncDwarfAuxSyms(this ptr<Loader> _addr_l, Sym fnSymIdx) => func((_, panic, __) =>
        {
            Sym auxDwarfInfo = default;
            Sym auxDwarfLoc = default;
            Sym auxDwarfRanges = default;
            Sym auxDwarfLines = default;
            ref Loader l = ref _addr_l.val;

            if (l.SymType(fnSymIdx) != sym.STEXT)
            {
                log.Fatalf("error: non-function sym %d/%s t=%s passed to GetFuncDwarfAuxSyms", fnSymIdx, l.SymName(fnSymIdx), l.SymType(fnSymIdx).String());
            }

            if (l.IsExternal(fnSymIdx))
            { 
                // Current expectation is that any external function will
                // not have auxsyms.
                return ;

            }

            var (r, li) = l.toLocal(fnSymIdx);
            var auxs = r.Auxs(li);
            foreach (var (i) in auxs)
            {
                var a = _addr_auxs[i];

                if (a.Type() == goobj2.AuxDwarfInfo) 
                    auxDwarfInfo = l.resolve(r, a.Sym());
                    if (l.SymType(auxDwarfInfo) != sym.SDWARFINFO)
                    {
                        panic("aux dwarf info sym with wrong type");
                    }

                else if (a.Type() == goobj2.AuxDwarfLoc) 
                    auxDwarfLoc = l.resolve(r, a.Sym());
                    if (l.SymType(auxDwarfLoc) != sym.SDWARFLOC)
                    {
                        panic("aux dwarf loc sym with wrong type");
                    }

                else if (a.Type() == goobj2.AuxDwarfRanges) 
                    auxDwarfRanges = l.resolve(r, a.Sym());
                    if (l.SymType(auxDwarfRanges) != sym.SDWARFRANGE)
                    {
                        panic("aux dwarf ranges sym with wrong type");
                    }

                else if (a.Type() == goobj2.AuxDwarfLines) 
                    auxDwarfLines = l.resolve(r, a.Sym());
                    if (l.SymType(auxDwarfLines) != sym.SDWARFLINES)
                    {
                        panic("aux dwarf lines sym with wrong type");
                    }

                            }
            return ;

        });

        // PrependSub prepends 'sub' onto the sub list for outer symbol 'outer'.
        // Will panic if 'sub' already has an outer sym or sub sym.
        // FIXME: should this be instead a method on SymbolBuilder?
        private static void PrependSub(this ptr<Loader> _addr_l, Sym outer, Sym sub) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // NB: this presupposes that an outer sym can't be a sub symbol of
            // some other outer-outer sym (I'm assuming this is true, but I
            // haven't tested exhaustively).
            if (l.OuterSym(outer) != 0L)
            {
                panic("outer has outer itself");
            }

            if (l.SubSym(sub) != 0L)
            {
                panic("sub set for subsym");
            }

            if (l.OuterSym(sub) != 0L)
            {
                panic("outer already set for subsym");
            }

            l.sub[sub] = l.sub[outer];
            l.sub[outer] = sub;
            l.outer[sub] = outer;

        });

        // OuterSym gets the outer symbol for host object loaded symbols.
        private static Sym OuterSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;
 
            // FIXME: add check for isExternal?
            return l.outer[i];

        }

        // SubSym gets the subsymbol for host object loaded symbols.
        private static Sym SubSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;
 
            // NB: note -- no check for l.isExternal(), since I am pretty sure
            // that later phases in the linker set subsym for "type." syms
            return l.sub[i];

        }

        // SetOuterSym sets the outer symbol of i to o (without setting
        // sub symbols).
        private static void SetOuterSym(this ptr<Loader> _addr_l, Sym i, Sym o)
        {
            ref Loader l = ref _addr_l.val;

            if (o != 0L)
            {
                l.outer[i] = o;
            }
            else
            {
                delete(l.outer, i);
            }

        }

        // Initialize Reachable bitmap and its siblings for running deadcode pass.
        private static void InitReachable(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            l.growAttrBitmaps(l.NSym() + 1L);
        }

        private partial struct symWithVal
        {
            public Sym s;
            public long v;
        }
        private partial struct bySymValue // : slice<symWithVal>
        {
        }

        private static long Len(this bySymValue s)
        {
            return len(s);
        }
        private static void Swap(this bySymValue s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }
        private static bool Less(this bySymValue s, long i, long j)
        {
            return s[i].v < s[j].v;
        }

        // SortSub walks through the sub-symbols for 's' and sorts them
        // in place by increasing value. Return value is the new
        // sub symbol for the specified outer symbol.
        private static Sym SortSub(this ptr<Loader> _addr_l, Sym s)
        {
            ref Loader l = ref _addr_l.val;

            if (s == 0L || l.sub[s] == 0L)
            {
                return s;
            } 

            // Sort symbols using a slice first. Use a stable sort on the off
            // chance that there's more than once symbol with the same value,
            // so as to preserve reproducible builds.
            symWithVal sl = new slice<symWithVal>(new symWithVal[] {  });
            {
                var ss = l.sub[s];

                while (ss != 0L)
                {
                    sl = append(sl, new symWithVal(s:ss,v:l.SymValue(ss)));
                    ss = l.sub[ss];
                }

            }
            sort.Stable(bySymValue(sl)); 

            // Then apply any changes needed to the sub map.
            var ns = Sym(0L);
            for (var i = len(sl) - 1L; i >= 0L; i--)
            {
                var s = sl[i].s;
                l.sub[s] = ns;
                ns = s;
            } 

            // Update sub for outer symbol, then return
 

            // Update sub for outer symbol, then return
            l.sub[s] = sl[0L].s;
            return sl[0L].s;

        }

        // Insure that reachable bitmap and its siblings have enough size.
        private static void growAttrBitmaps(this ptr<Loader> _addr_l, long reqLen)
        {
            ref Loader l = ref _addr_l.val;

            if (reqLen > l.attrReachable.Len())
            { 
                // These are indexed by global symbol
                l.attrReachable = growBitmap(reqLen, l.attrReachable);
                l.attrOnList = growBitmap(reqLen, l.attrOnList);
                l.attrLocal = growBitmap(reqLen, l.attrLocal);
                l.attrNotInSymbolTable = growBitmap(reqLen, l.attrNotInSymbolTable);

            }

            l.growExtAttrBitmaps();

        }

        private static void growExtAttrBitmaps(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;
 
            // These are indexed by external symbol index (e.g. l.extIndex(i))
            var extReqLen = len(l.payloads);
            if (extReqLen > l.attrVisibilityHidden.Len())
            {
                l.attrVisibilityHidden = growBitmap(extReqLen, l.attrVisibilityHidden);
                l.attrDuplicateOK = growBitmap(extReqLen, l.attrDuplicateOK);
                l.attrShared = growBitmap(extReqLen, l.attrShared);
                l.attrExternal = growBitmap(extReqLen, l.attrExternal);
            }

        }

        private static long Count(this ptr<Relocs> _addr_relocs)
        {
            ref Relocs relocs = ref _addr_relocs.val;

            return len(relocs.rs);
        }

        // At2 returns the j-th reloc for a global symbol.
        private static Reloc2 At2(this ptr<Relocs> _addr_relocs, long j)
        {
            ref Relocs relocs = ref _addr_relocs.val;

            if (relocs.l.isExtReader(relocs.r))
            {
                var pp = relocs.l.payloads[relocs.li];
                return new Reloc2(&relocs.rs[j],relocs.r,relocs.l,pp.reltypes[j]);
            }

            return new Reloc2(&relocs.rs[j],relocs.r,relocs.l,0);

        }

        // Relocs returns a Relocs object for the given global sym.
        private static Relocs Relocs(this ptr<Loader> _addr_l, Sym i) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            var (r, li) = l.toLocal(i);
            if (r == null)
            {
                panic(fmt.Sprintf("trying to get oreader for invalid sym %d\n\n", i));
            }

            return l.relocs(r, li);

        });

        // Relocs returns a Relocs object given a local sym index and reader.
        private static Relocs relocs(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, long li)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            slice<goobj2.Reloc> rs = default;
            if (l.isExtReader(r))
            {
                var pp = l.payloads[li];
                rs = pp.relocs;
            }
            else
            {
                rs = r.Relocs(li);
            }

            return new Relocs(rs:rs,li:li,r:r,l:l,);

        }

        // ExtRelocs returns the external relocations of the i-th symbol.
        private static ExtRelocs ExtRelocs(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            return new ExtRelocs(l.Relocs(i),l.extRelocs[i]);
        }

        // ExtRelocs represents the set of external relocations of a symbol.
        public partial struct ExtRelocs
        {
            public Relocs rs;
            public slice<ExtReloc> es;
        }

        public static long Count(this ExtRelocs ers)
        {
            return len(ers.es);
        }

        public static ExtRelocView At(this ExtRelocs ers, long j)
        {
            var i = ers.es[j].Idx;
            return new ExtRelocView(ers.rs.At2(i),&ers.es[j]);
        }

        // RelocByOff implements sort.Interface for sorting relocations by offset.

        public partial struct RelocByOff // : slice<Reloc>
        {
        }

        public static long Len(this RelocByOff x)
        {
            return len(x);
        }
        public static void Swap(this RelocByOff x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];
        }
        public static bool Less(this RelocByOff x, long i, long j)
        {
            return x[i].Off < x[j].Off;
        }

        // FuncInfo provides hooks to access goobj2.FuncInfo in the objects.
        public partial struct FuncInfo
        {
            public ptr<Loader> l;
            public ptr<oReader> r;
            public slice<byte> data;
            public slice<goobj2.Aux> auxs;
            public goobj2.FuncInfoLengths lengths;
        }

        private static bool Valid(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            return fi.r != null;
        }

        private static long Args(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            return int((goobj2.FuncInfo.val)(null).ReadArgs(fi.data));
        }

        private static long Locals(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            return int((goobj2.FuncInfo.val)(null).ReadLocals(fi.data));
        }

        private static slice<byte> Pcsp(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            var (pcsp, end) = (goobj2.FuncInfo.val)(null).ReadPcsp(fi.data);
            return fi.r.BytesAt(fi.r.PcdataBase() + pcsp, int(end - pcsp));
        }

        private static slice<byte> Pcfile(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            var (pcf, end) = (goobj2.FuncInfo.val)(null).ReadPcfile(fi.data);
            return fi.r.BytesAt(fi.r.PcdataBase() + pcf, int(end - pcf));
        }

        private static slice<byte> Pcline(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            var (pcln, end) = (goobj2.FuncInfo.val)(null).ReadPcline(fi.data);
            return fi.r.BytesAt(fi.r.PcdataBase() + pcln, int(end - pcln));
        }

        // Preload has to be called prior to invoking the various methods
        // below related to pcdata, funcdataoff, files, and inltree nodes.
        private static void Preload(this ptr<FuncInfo> _addr_fi)
        {
            ref FuncInfo fi = ref _addr_fi.val;

            fi.lengths = (goobj2.FuncInfo.val)(null).ReadFuncInfoLengths(fi.data);
        }

        private static slice<byte> Pcinline(this ptr<FuncInfo> _addr_fi) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            var (pcinl, end) = (goobj2.FuncInfo.val)(null).ReadPcinline(fi.data, fi.lengths.PcdataOff);
            return fi.r.BytesAt(fi.r.PcdataBase() + pcinl, int(end - pcinl));

        });

        private static uint NumPcdata(this ptr<FuncInfo> _addr_fi) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            return fi.lengths.NumPcdata;

        });

        private static slice<byte> Pcdata(this ptr<FuncInfo> _addr_fi, long k) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            var (pcdat, end) = (goobj2.FuncInfo.val)(null).ReadPcdata(fi.data, fi.lengths.PcdataOff, uint32(k));
            return fi.r.BytesAt(fi.r.PcdataBase() + pcdat, int(end - pcdat));

        });

        private static uint NumFuncdataoff(this ptr<FuncInfo> _addr_fi) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            return fi.lengths.NumFuncdataoff;

        });

        private static long Funcdataoff(this ptr<FuncInfo> _addr_fi, long k) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            return (goobj2.FuncInfo.val)(null).ReadFuncdataoff(fi.data, fi.lengths.FuncdataoffOff, uint32(k));

        });

        private static slice<Sym> Funcdata(this ptr<FuncInfo> _addr_fi, slice<Sym> syms) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            if (int(fi.lengths.NumFuncdataoff) > cap(syms))
            {
                syms = make_slice<Sym>(0L, fi.lengths.NumFuncdataoff);
            }
            else
            {
                syms = syms[..0L];
            }

            foreach (var (j) in fi.auxs)
            {
                var a = _addr_fi.auxs[j];
                if (a.Type() == goobj2.AuxFuncdata)
                {
                    syms = append(syms, fi.l.resolve(fi.r, a.Sym()));
                }

            }
            return syms;

        });

        private static uint NumFile(this ptr<FuncInfo> _addr_fi) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            return fi.lengths.NumFile;

        });

        private static Sym File(this ptr<FuncInfo> _addr_fi, long k) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            var sr = (goobj2.FuncInfo.val)(null).ReadFile(fi.data, fi.lengths.FileOff, uint32(k));
            return fi.l.resolve(fi.r, sr);

        });

        public partial struct InlTreeNode
        {
            public int Parent;
            public Sym File;
            public int Line;
            public Sym Func;
            public int ParentPC;
        }

        private static uint NumInlTree(this ptr<FuncInfo> _addr_fi) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            return fi.lengths.NumInlTree;

        });

        private static InlTreeNode InlTree(this ptr<FuncInfo> _addr_fi, long k) => func((_, panic, __) =>
        {
            ref FuncInfo fi = ref _addr_fi.val;

            if (!fi.lengths.Initialized)
            {
                panic("need to call Preload first");
            }

            var node = (goobj2.FuncInfo.val)(null).ReadInlTree(fi.data, fi.lengths.InlTreeOff, uint32(k));
            return new InlTreeNode(Parent:node.Parent,File:fi.l.resolve(fi.r,node.File),Line:node.Line,Func:fi.l.resolve(fi.r,node.Func),ParentPC:node.ParentPC,);

        });

        private static FuncInfo FuncInfo(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            ptr<oReader> r;
            slice<goobj2.Aux> auxs = default;
            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                if (pp.objidx == 0L)
                {
                    return new FuncInfo();
                }

                r = l.objs[pp.objidx].r;
                auxs = pp.auxs;

            }
            else
            {
                long li = default;
                r, li = l.toLocal(i);
                auxs = r.Auxs(li);
            }

            foreach (var (j) in auxs)
            {
                var a = _addr_auxs[j];
                if (a.Type() == goobj2.AuxFuncInfo)
                {
                    var b = r.Data(int(a.Sym().SymIdx));
                    return new FuncInfo(l,r,b,auxs,goobj2.FuncInfoLengths{});
                }

            }
            return new FuncInfo();

        }

        // Preload a package: add autolibs, add defined package symbols to the symbol table.
        // Does not add non-package symbols yet, which will be done in LoadNonpkgSyms.
        // Does not read symbol data.
        // Returns the fingerprint of the object.
        private static goobj2.FingerprintType Preload(this ptr<Loader> _addr_l, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, ptr<sym.CompilationUnit> _addr_unit, long length) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;
            ref sym.Library lib = ref _addr_lib.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var (roObject, readonly, err) = f.Slice(uint64(length)); // TODO: no need to map blocks that are for tools only (e.g. RefName)
            if (err != null)
            {
                log.Fatal("cannot read object file:", err);
            }

            var r = goobj2.NewReaderFromBytes(roObject, readonly);
            if (r == null)
            {
                if (len(roObject) >= 8L && bytes.Equal(roObject[..8L], (slice<byte>)"\x00go114ld"))
                {
                    log.Fatalf("found object file %s in old format, but -go115newobj is true\nset -go115newobj consistently in all -gcflags, -asmflags, and -ldflags", f.File().Name());
                }

                panic("cannot read object file");

            }

            var localSymVersion = syms.IncVersion();
            var pkgprefix = objabi.PathToPrefix(lib.Pkg) + ".";
            var ndef = r.NSym();
            var nnonpkgdef = r.NNonpkgdef();
            ptr<oReader> or = addr(new oReader(r,unit,localSymVersion,r.Flags(),pkgprefix,make([]Sym,ndef+nnonpkgdef+r.NNonpkgref()),ndef,uint32(len(l.objs)))); 

            // Autolib
            lib.Autolib = append(lib.Autolib, r.Autolib()); 

            // DWARF file table
            var nfile = r.NDwarfFile();
            unit.DWARFFileTable = make_slice<@string>(nfile);
            foreach (var (i) in unit.DWARFFileTable)
            {
                unit.DWARFFileTable[i] = r.DwarfFile(i);
            }
            l.addObj(lib.Pkg, or);
            l.preloadSyms(or, pkgDef); 

            // The caller expects us consuming all the data
            f.MustSeek(length, os.SEEK_CUR);

            return r.Fingerprint();

        });

        // Preload symbols of given kind from an object.
        private static void preloadSyms(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, long kind) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            var ndef = r.NSym();
            var nnonpkgdef = r.NNonpkgdef();
            long start = default;            long end = default;


            if (kind == pkgDef) 
                start = 0L;
                end = ndef;
            else if (kind == nonPkgDef) 
                start = ndef;
                end = ndef + nnonpkgdef;
            else 
                panic("preloadSyms: bad kind");
                        l.growSyms(len(l.objSyms) + end - start);
            l.growAttrBitmaps(len(l.objSyms) + end - start);
            for (var i = start; i < end; i++)
            {
                var osym = r.Sym(i);
                var name = strings.Replace(osym.Name(r.Reader), "\"\".", r.pkgprefix, -1L);
                var v = abiToVer(osym.ABI(), r.version);
                var dupok = osym.Dupok();
                var (gi, added) = l.AddSym(name, v, r, i, kind, dupok, sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())]);
                r.syms[i] = gi;
                if (!added)
                {
                    continue;
                }

                if (osym.TopFrame())
                {
                    l.SetAttrTopFrame(gi, true);
                }

                if (osym.Local())
                {
                    l.SetAttrLocal(gi, true);
                }

                if (strings.HasPrefix(name, "go.itablink."))
                {
                    l.itablink[gi] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                }

                if (strings.HasPrefix(name, "runtime."))
                {
                    {
                        var bi = goobj2.BuiltinIdx(name, v);

                        if (bi != -1L)
                        { 
                            // This is a definition of a builtin symbol. Record where it is.
                            l.builtinSyms[bi] = gi;

                        }

                    }

                }

                if (strings.HasPrefix(name, "go.string.") || strings.HasPrefix(name, "gclocals·") || strings.HasPrefix(name, "runtime.gcbits."))
                {
                    l.SetAttrNotInSymbolTable(gi, true);
                }

                {
                    var a = osym.Align();

                    if (a != 0L)
                    {
                        l.SetSymAlign(gi, int32(a));
                    }

                }

            }


        });

        // Add non-package symbols and references to external symbols (which are always
        // named).
        private static void LoadNonpkgSyms(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch)
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;

            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    l.preloadSyms(o.r, nonPkgDef);
                }

                o = o__prev1;
            }

            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    loadObjRefs(_addr_l, _addr_o.r, _addr_arch);
                }

                o = o__prev1;
            }
        }

        private static void loadObjRefs(ptr<Loader> _addr_l, ptr<oReader> _addr_r, ptr<sys.Arch> _addr_arch)
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var ndef = r.NSym() + r.NNonpkgdef();
            for (long i = 0L;
            var n = r.NNonpkgref(); i < n; i++)
            {
                var osym = r.Sym(ndef + i);
                var name = strings.Replace(osym.Name(r.Reader), "\"\".", r.pkgprefix, -1L);
                var v = abiToVer(osym.ABI(), r.version);
                r.syms[ndef + i] = l.LookupOrCreateSym(name, v);
                var gi = r.syms[ndef + i];
                if (osym.Local())
                {
                    l.SetAttrLocal(gi, true);
                }

                l.preprocess(arch, gi, name);

            }


        }

        private static long abiToVer(ushort abi, long localSymVersion)
        {
            long v = default;
            if (abi == goobj2.SymABIstatic)
            { 
                // Static
                v = localSymVersion;

            }            {
                var abiver = sym.ABIToVersion(obj.ABI(abi));


                else if (abiver != -1L)
                { 
                    // Note that data symbols are "ABI0", which maps to version 0.
                    v = abiver;

                }
                else
                {
                    log.Fatalf("invalid symbol ABI: %d", abi);
                }

            }

            return v;

        }

        // preprocess looks for integer/floating point constant symbols whose
        // content is encoded into the symbol name, and promotes them into
        // real symbols with RODATA type and a payload that matches the
        // encoded content.
        private static void preprocess(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch, Sym s, @string name)
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;

            if (name != "" && name[0L] == '$' && len(name) > 5L && l.SymType(s) == 0L && len(l.Data(s)) == 0L)
            {
                var (x, err) = strconv.ParseUint(name[5L..], 16L, 64L);
                if (err != null)
                {
                    log.Panicf("failed to parse $-symbol %s: %v", name, err);
                }

                var su = l.MakeSymbolUpdater(s);
                su.SetType(sym.SRODATA);
                su.SetLocal(true);
                switch (name[..5L])
                {
                    case "$f32.": 
                        if (uint64(uint32(x)) != x)
                        {
                            log.Panicf("$-symbol %s too large: %d", name, x);
                        }

                        su.AddUint32(arch, uint32(x));
                        break;
                    case "$f64.": 

                    case "$i64.": 
                        su.AddUint64(arch, x);
                        break;
                    default: 
                        log.Panicf("unrecognized $-symbol: %s", name);
                        break;
                }

            }

        }

        // Load full contents.
        private static void LoadFull(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, bool needReloc, bool needExtReloc) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
 
            // create all Symbols first.
            l.growSyms(l.NSym());
            l.growSects(l.NSym());

            if (needReloc && len(l.extRelocs) != 0L)
            { 
                // If needReloc is true, we are going to convert the loader's
                // "internal" relocations to sym.Relocs. In this case, external
                // relocations shouldn't be used.
                panic("phase error");

            }

            long nr = 0L; // total number of sym.Reloc's we'll need
            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    nr += loadObjSyms(_addr_l, _addr_syms, _addr_o.r, needReloc, needExtReloc);
                } 

                // Make a first pass through the external symbols, making
                // sure that each external symbol has a non-nil entry in
                // l.Syms (note that relocations and symbol content will
                // be copied in a later loop).

                o = o__prev1;
            }

            var toConvert = make_slice<Sym>(0L, len(l.payloads));
            {
                var i__prev1 = i;

                foreach (var (_, __i) in l.extReader.syms)
                {
                    i = __i;
                    if (!l.attrReachable.Has(i))
                    {
                        continue;
                    }

                    var pp = l.getPayload(i);
                    if (needReloc)
                    {
                        nr += len(pp.relocs);
                    }

                    if (needExtReloc && int(i) < len(l.extRelocs))
                    {
                        nr += len(l.extRelocs[i]);
                    } 
                    // create and install the sym.Symbol here so that l.Syms will
                    // be fully populated when we do relocation processing and
                    // outer/sub processing below. Note that once we do this,
                    // we'll need to get at the payload for a symbol with direct
                    // reference to l.payloads[] as opposed to calling l.getPayload().
                    var s = l.allocSym(pp.name, 0L);
                    l.installSym(i, s);
                    toConvert = append(toConvert, i);

                } 

                // allocate a single large slab of relocations for all live symbols

                i = i__prev1;
            }

            if (nr != 0L)
            {
                l.relocBatch = make_slice<sym.Reloc>(nr);
                if (needExtReloc)
                {
                    l.relocExtBatch = make_slice<sym.RelocExt>(nr);
                }

            } 

            // convert payload-based external symbols into sym.Symbol-based
            {
                var i__prev1 = i;

                foreach (var (_, __i) in toConvert)
                {
                    i = __i;
                    // Copy kind/size/value etc.
                    pp = l.payloads[l.extIndex(i)];
                    s = l.Syms[i];
                    s.Version = int16(pp.ver);
                    s.Type = pp.kind;
                    s.Size = pp.size; 

                    // Copy relocations
                    if (needReloc)
                    {
                        var batch = l.relocBatch;
                        s.R = batch.slice(-1, len(pp.relocs), len(pp.relocs));
                        l.relocBatch = batch[len(pp.relocs)..];
                        ref var relocs = ref heap(l.Relocs(i), out ptr<var> _addr_relocs);
                        l.convertRelocations(i, _addr_relocs, s, false);
                    }

                    if (needExtReloc)
                    {
                        l.convertExtRelocs(s, i);
                    } 

                    // Copy data
                    s.P = pp.data; 

                    // Transfer over attributes.
                    l.migrateAttributes(i, s);

                } 

                // load contents of defined symbols

                i = i__prev1;
            }

            {
                var o__prev1 = o;

                foreach (var (_, __o) in l.objs[1L..])
                {
                    o = __o;
                    loadObjFull(_addr_l, _addr_o.r, needReloc, needExtReloc);
                } 

                // Sanity check: we should have consumed all batched allocations.

                o = o__prev1;
            }

            if (len(l.relocBatch) != 0L || len(l.relocExtBatch) != 0L)
            {
                panic("batch allocation mismatch");
            } 

            // Note: resolution of ABI aliases is now also handled in
            // loader.convertRelocations, so once the host object loaders move
            // completely to loader.Sym, we can remove the code below.

            // Resolve ABI aliases for external symbols. This is only
            // needed for internal cgo linking.
            if (needReloc)
            {
                {
                    var i__prev1 = i;

                    foreach (var (_, __i) in l.extReader.syms)
                    {
                        i = __i;
                        {
                            var s__prev2 = s;

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

                            s = s__prev2;

                        }

                    }

                    i = i__prev1;
                }
            } 

            // Free some memory.
            // At this point we still need basic index mapping, and some fields of
            // external symbol payloads, but not much else.
            l.values = null;
            l.symSects = null;
            l.outdata = null;
            l.itablink = null;
            l.attrOnList = null;
            l.attrLocal = null;
            l.attrNotInSymbolTable = null;
            l.attrVisibilityHidden = null;
            l.attrDuplicateOK = null;
            l.attrShared = null;
            l.attrExternal = null;
            l.attrReadOnly = null;
            l.attrTopFrame = null;
            l.attrSpecial = null;
            l.attrCgoExportDynamic = null;
            l.attrCgoExportStatic = null;
            l.outer = null;
            l.align = null;
            l.dynimplib = null;
            l.dynimpvers = null;
            l.localentry = null;
            l.extname = null;
            l.elfType = null;
            l.plt = null;
            l.got = null;
            l.dynid = null;
            if (needExtReloc)
            { // converted to sym.Relocs, drop loader references
                l.relocVariant = null;
                l.extRelocs = null;

            } 

            // Drop fields that are no longer needed.
            {
                var i__prev1 = i;

                foreach (var (_, __i) in l.extReader.syms)
                {
                    i = __i;
                    pp = l.getPayload(i);
                    pp.name = "";
                    pp.auxs = null;
                    pp.data = null;
                    if (needExtReloc)
                    {
                        pp.relocs = null;
                        pp.reltypes = null;
                    }

                }

                i = i__prev1;
            }
        });

        // ResolveABIAlias given a symbol returns the ABI alias target of that
        // symbol. If the sym in question is not an alias, the sym itself is
        // returned.
        private static Sym ResolveABIAlias(this ptr<Loader> _addr_l, Sym s) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (s == 0L)
            {
                return 0L;
            }

            if (l.SymType(s) != sym.SABIALIAS)
            {
                return s;
            }

            var relocs = l.Relocs(s);
            var target = relocs.At2(0L).Sym();
            if (l.SymType(target) == sym.SABIALIAS)
            {
                panic(fmt.Sprintf("ABI alias %s references another ABI alias %s", l.SymName(s), l.SymName(target)));
            }

            return target;

        });

        // PropagateSymbolChangesBackToLoader is a temporary shim function
        // that copies over a given sym.Symbol into the equivalent representation
        // in the loader world. The intent is to enable converting a given
        // linker phase/pass from dealing with sym.Symbol's to a modernized
        // pass that works with loader.Sym, in cases where the "loader.Sym
        // wavefront" has not yet reached the pass in question. For such work
        // the recipe is to first call PropagateSymbolChangesBackToLoader(),
        // then exexute the pass working with the loader, then call
        // PropagateLoaderChangesToSymbols to copy the changes made by the
        // pass back to the sym.Symbol world.
        private static void PropagateSymbolChangesBackToLoader(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            // For the moment we only copy symbol values, and we don't touch
            // any new sym.Symbols created since loadlibfull() was run. This
            // seems to be what's needed for DWARF gen.
            for (var i = Sym(1L); i < Sym(len(l.objSyms)); i++)
            {
                var s = l.Syms[i];
                if (s != null)
                {
                    if (s.Value != l.SymValue(i))
                    {
                        l.SetSymValue(i, s.Value);
                    }

                }

            }


        }

        // PropagateLoaderChangesToSymbols is a temporary shim function that
        // takes a list of loader.Sym symbols and works to copy their contents
        // and attributes over to a corresponding sym.Symbol. The parameter
        // anonVerReplacement specifies a version number for any new anonymous
        // symbols encountered on the list, when creating sym.Symbols for them
        // (or zero if we don't expect to encounter any new anon symbols). See
        // the PropagateSymbolChangesBackToLoader header comment for more
        // info.
        //
        // WARNING: this function is brittle and depends heavily on loader
        // implementation. A key problem with doing this is that as things
        // stand at the moment, some sym.Symbol contents/attributes are
        // populated only when converting from loader.Sym to sym.Symbol in
        // loadlibfull, meaning we may wipe out some information when copying
        // back.

        private static slice<ptr<sym.Symbol>> PropagateLoaderChangesToSymbols(this ptr<Loader> _addr_l, slice<Sym> toconvert, long anonVerReplacement) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            ptr<sym.Symbol> result = new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] {  });
            Sym relocfixup = new slice<Sym>(new Sym[] {  }); 

            // Note: this loop needs to allow for the possibility that we may
            // see "new" symbols on the 'toconvert' list that come from object
            // files (for example, DWARF location lists), as opposed to just
            // newly manufactured symbols (ex: DWARF section symbols such as
            // ".debug_info").  This means that we have to be careful not to
            // stomp on sym.Symbol attributes/content that was set up in
            // in loadlibfull().

            // Also note that in order for the relocation fixup to work, we
            // have to do this in two passes -- one pass to create the symbols,
            // and then a second fix up the relocations once all necessary
            // sym.Symbols are created.

            // First pass, symbol creation and symbol data fixup.
            {
                var cand__prev1 = cand;

                foreach (var (_, __cand) in toconvert)
                {
                    cand = __cand;
                    var sn = l.SymName(cand);
                    var sv = l.SymVersion(cand);
                    var st = l.SymType(cand);
                    if (sv < 0L)
                    {
                        if (anonVerReplacement == 0L)
                        {
                            panic("expected valid anon version replacement");
                        }

                        sv = anonVerReplacement;

                    }

                    var s = l.Syms[cand];

                    var isnew = false;
                    if (sn == "")
                    { 
                        // Don't install anonymous symbols in the lookup tab.
                        if (s == null)
                        {
                            s = l.allocSym(sn, sv);
                            l.installSym(cand, s);
                        }

                        isnew = true;

                    }
                    else
                    {
                        if (s != null)
                        { 
                            // Already have a symbol for this -- it must be
                            // something that was previously processed by
                            // loadObjFull. Note that the symbol in question may
                            // or may not be in the name lookup map.
                        }
                        else
                        {
                            isnew = true;
                            s = l.SymLookup(sn, sv);
                        }

                    }

                    result = append(result, s); 

                    // Always copy these from new to old.
                    s.Value = l.SymValue(cand);
                    s.Type = st; 

                    // If the data for a symbol has increased in size, make sure
                    // we bring the new content across.
                    var relfix = isnew;
                    if (isnew || len(l.Data(cand)) > len(s.P))
                    {
                        s.P = l.Data(cand);
                        s.Size = int64(len(s.P));
                        relfix = true;
                    } 

                    // For 'new' symbols, copy other content.
                    if (relfix)
                    {
                        relocfixup = append(relocfixup, cand);
                    } 

                    // If new symbol, call a helper to migrate attributes.
                    // Otherwise touch only not-in-symbol-table, since there are
                    // some attrs that are only set up at the point where we
                    // convert loader.Sym to sym.Symbol.
                    if (isnew)
                    {
                        l.migrateAttributes(cand, s);
                    }
                    else
                    {
                        if (l.AttrNotInSymbolTable(cand))
                        {
                            s.Attr.Set(sym.AttrNotInSymbolTable, true);
                        }

                    }

                } 

                // Second pass to fix up relocations.

                cand = cand__prev1;
            }

            {
                var cand__prev1 = cand;

                foreach (var (_, __cand) in relocfixup)
                {
                    cand = __cand;
                    s = l.Syms[cand];
                    ref var relocs = ref heap(l.Relocs(cand), out ptr<var> _addr_relocs);
                    if (len(s.R) != relocs.Count())
                    {
                        s.R = make_slice<sym.Reloc>(relocs.Count());
                    }

                    l.convertRelocations(cand, _addr_relocs, s, true);

                }

                cand = cand__prev1;
            }

            return result;

        });

        // ExtractSymbols grabs the symbols out of the loader for work that hasn't been
        // ported to the new symbol type.
        private static void ExtractSymbols(this ptr<Loader> _addr_l, ptr<sym.Symbols> _addr_syms)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
 
            // Add symbols to the ctxt.Syms lookup table. This explicitly skips things
            // created via loader.Create (marked with versions less than zero), since
            // if we tried to add these we'd wind up with collisions. We do, however,
            // add these symbols to the list of global symbols so that other future
            // steps (like pclntab generation) can find these symbols if neceassary.
            // Along the way, update the version from the negative anon version to
            // something larger than sym.SymVerStatic (needed so that
            // sym.symbol.IsFileLocal() works properly).
            var anonVerReplacement = syms.IncVersion();
            {
                var s__prev1 = s;

                foreach (var (_, __s) in l.Syms)
                {
                    s = __s;
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.Version < 0L)
                    {
                        s.Version = int16(anonVerReplacement);
                    }

                } 

                // Provide lookup functions for sym.Symbols.

                s = s__prev1;
            }

            l.SymLookup = (name, ver) =>
            {
                var i = l.LookupOrCreateSym(name, ver);
                {
                    var s__prev1 = s;

                    var s = l.Syms[i];

                    if (s != null)
                    {
                        return s;
                    }

                    s = s__prev1;

                }

                s = l.allocSym(name, ver);
                l.installSym(i, s);
                return s;

            }
;
            syms.Lookup = l.SymLookup;
            syms.ROLookup = (name, ver) =>
            {
                i = l.Lookup(name, ver);
                return l.Syms[i];
            }
;

        }

        // allocSym allocates a new symbol backing.
        private static ptr<sym.Symbol> allocSym(this ptr<Loader> _addr_l, @string name, long version)
        {
            ref Loader l = ref _addr_l.val;

            var batch = l.symBatch;
            if (len(batch) == 0L)
            {
                batch = make_slice<sym.Symbol>(1000L);
            }

            var s = _addr_batch[0L];
            l.symBatch = batch[1L..];

            s.Dynid = -1L;
            s.Name = name;
            s.Version = int16(version);

            return _addr_s!;

        }

        // installSym sets the underlying sym.Symbol for the specified sym index.
        private static void installSym(this ptr<Loader> _addr_l, Sym i, ptr<sym.Symbol> _addr_s) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s == null)
            {
                panic("installSym nil symbol");
            }

            if (l.Syms[i] != null)
            {
                panic("sym already present in installSym");
            }

            l.Syms[i] = s;
            s.SymIdx = sym.LoaderSym(i);

        });

        // addNewSym adds a new sym.Symbol to the i-th index in the list of symbols.
        private static ptr<sym.Symbol> addNewSym(this ptr<Loader> _addr_l, Sym i, @string name, long ver, ptr<sym.CompilationUnit> _addr_unit, sym.SymKind t) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var s = l.allocSym(name, ver);
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
            l.growSyms(int(i));
            l.installSym(i, s);
            return _addr_s!;

        });

        // TopLevelSym tests a symbol (by name and kind) to determine whether
        // the symbol first class sym (participating in the link) or is an
        // anonymous aux or sub-symbol containing some sub-part or payload of
        // another symbol.
        private static bool TopLevelSym(this ptr<Loader> _addr_l, Sym s)
        {
            ref Loader l = ref _addr_l.val;

            return topLevelSym(l.RawSymName(s), l.SymType(s));
        }

        // topLevelSym tests a symbol name and kind to determine whether
        // the symbol first class sym (participating in the link) or is an
        // anonymous aux or sub-symbol containing some sub-part or payload of
        // another symbol.
        private static bool topLevelSym(@string sname, sym.SymKind skind)
        {
            if (sname != "")
            {
                return true;
            }


            if (skind == sym.SDWARFINFO || skind == sym.SDWARFRANGE || skind == sym.SDWARFLOC || skind == sym.SDWARFLINES || skind == sym.SGOFUNC) 
                return true;
            else 
                return false;
            
        }

        // loadObjSyms creates sym.Symbol objects for the live Syms in the
        // object corresponding to object reader "r". Return value is the
        // number of sym.Reloc entries required for all the new symbols.
        private static long loadObjSyms(ptr<Loader> _addr_l, ptr<sym.Symbols> _addr_syms, ptr<oReader> _addr_r, bool needReloc, bool needExtReloc)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref oReader r = ref _addr_r.val;

            long nr = 0L;
            for (long i = 0L;
            var n = r.NSym() + r.NNonpkgdef(); i < n; i++)
            {
                var gi = r.syms[i];
                {
                    var (r2, i2) = l.toLocal(gi);

                    if (r2 != r || i2 != i)
                    {
                        continue; // come from a different object
                    }

                }

                var osym = r.Sym(i);
                var name = strings.Replace(osym.Name(r.Reader), "\"\".", r.pkgprefix, -1L);
                var t = sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())]; 

                // Skip non-dwarf anonymous symbols (e.g. funcdata),
                // since they will never be turned into sym.Symbols.
                if (!topLevelSym(name, t))
                {
                    continue;
                }

                var ver = abiToVer(osym.ABI(), r.version);
                if (t == sym.SXREF)
                {
                    log.Fatalf("bad sxref");
                }

                if (t == 0L)
                {
                    log.Fatalf("missing type for %s in %s", name, r.unit.Lib);
                }

                if (!l.attrReachable.Has(gi) && name != "runtime.addmoduledata" && name != "runtime.lastmoduledatap")
                { 
                    // No need to load unreachable symbols.
                    // XXX reference to runtime.addmoduledata may be generated later by the linker in plugin mode.
                    continue;

                }

                l.addNewSym(gi, name, ver, r.unit, t);
                if (needReloc)
                {
                    nr += r.NReloc(i);
                }

                if (needExtReloc && int(gi) < len(l.extRelocs))
                {
                    nr += len(l.extRelocs[gi]);
                }

            }

            return nr;

        }

        // cloneToExternal takes the existing object file symbol (symIdx)
        // and creates a new external symbol payload that is a clone with
        // respect to name, version, type, relocations, etc. The idea here
        // is that if the linker decides it wants to update the contents of
        // a symbol originally discovered as part of an object file, it's
        // easier to do this if we make the updates to an external symbol
        // payload.
        // XXX maybe rename? makeExtPayload?
        private static void cloneToExternal(this ptr<Loader> _addr_l, Sym symIdx) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(symIdx))
            {
                panic("sym is already external, no need for clone");
            }

            l.growSyms(int(symIdx)); 

            // Read the particulars from object.
            var (r, li) = l.toLocal(symIdx);
            var osym = r.Sym(li);
            var sname = strings.Replace(osym.Name(r.Reader), "\"\".", r.pkgprefix, -1L);
            var sver = abiToVer(osym.ABI(), r.version);
            var skind = sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())]; 

            // Create new symbol, update version and kind.
            var pi = l.newPayload(sname, sver);
            var pp = l.payloads[pi];
            pp.kind = skind;
            pp.ver = sver;
            pp.size = int64(osym.Siz());
            pp.objidx = r.objidx; 

            // If this is a def, then copy the guts. We expect this case
            // to be very rare (one case it may come up is with -X).
            if (li < (r.NSym() + r.NNonpkgdef()))
            {
                // Copy relocations
                var relocs = l.Relocs(symIdx);
                pp.relocs = make_slice<goobj2.Reloc>(relocs.Count());
                pp.reltypes = make_slice<objabi.RelocType>(relocs.Count());
                foreach (var (i) in pp.relocs)
                { 
                    // Copy the relocs slice.
                    // Convert local reference to global reference.
                    var rel = relocs.At2(i);
                    pp.relocs[i].Set(rel.Off(), rel.Siz(), 0L, rel.Add(), new goobj2.SymRef(PkgIdx:0,SymIdx:uint32(rel.Sym())));
                    pp.reltypes[i] = rel.Type();

                } 

                // Copy data
                pp.data = r.Data(li);

            } 

            // If we're overriding a data symbol, collect the associated
            // Gotype, so as to propagate it to the new symbol.
            var auxs = r.Auxs(li);
            pp.auxs = auxs;
loop: 

            // Install new payload to global index space.
            // (This needs to happen at the end, as the accessors above
            // need to access the old symbol content.)
            foreach (var (j) in auxs)
            {
                var a = _addr_auxs[j];

                if (a.Type() == goobj2.AuxGotype) 
                    pp.gotype = l.resolve(r, a.Sym());
                    _breakloop = true;
                    break;
                else                 
            } 

            // Install new payload to global index space.
            // (This needs to happen at the end, as the accessors above
            // need to access the old symbol content.)
            l.objSyms[symIdx] = new objSym(l.extReader,pi);
            l.extReader.syms = append(l.extReader.syms, symIdx);

        });

        // Copy the payload of symbol src to dst. Both src and dst must be external
        // symbols.
        // The intended use case is that when building/linking against a shared library,
        // where we do symbol name mangling, the Go object file may have reference to
        // the original symbol name whereas the shared library provides a symbol with
        // the mangled name. When we do mangling, we copy payload of mangled to original.
        private static void CopySym(this ptr<Loader> _addr_l, Sym src, Sym dst) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            if (!l.IsExternal(dst))
            {
                panic("dst is not external"); //l.newExtSym(l.SymName(dst), l.SymVersion(dst))
            }

            if (!l.IsExternal(src))
            {
                panic("src is not external"); //l.cloneToExternal(src)
            }

            l.payloads[l.extIndex(dst)] = l.payloads[l.extIndex(src)];
            l.SetSymPkg(dst, l.SymPkg(src)); 
            // TODO: other attributes?
        });

        // CopyAttributes copies over all of the attributes of symbol 'src' to
        // symbol 'dst'.
        private static void CopyAttributes(this ptr<Loader> _addr_l, Sym src, Sym dst)
        {
            ref Loader l = ref _addr_l.val;

            l.SetAttrReachable(dst, l.AttrReachable(src));
            l.SetAttrOnList(dst, l.AttrOnList(src));
            l.SetAttrLocal(dst, l.AttrLocal(src));
            l.SetAttrNotInSymbolTable(dst, l.AttrNotInSymbolTable(src));
            if (l.IsExternal(dst))
            {
                l.SetAttrVisibilityHidden(dst, l.AttrVisibilityHidden(src));
                l.SetAttrDuplicateOK(dst, l.AttrDuplicateOK(src));
                l.SetAttrShared(dst, l.AttrShared(src));
                l.SetAttrExternal(dst, l.AttrExternal(src));
            }
            else
            { 
                // Some attributes are modifiable only for external symbols.
                // In such cases, don't try to transfer over the attribute
                // from the source even if there is a clash. This comes up
                // when copying attributes from a dupOK ABI wrapper symbol to
                // the real target symbol (which may not be marked dupOK).
            }

            l.SetAttrTopFrame(dst, l.AttrTopFrame(src));
            l.SetAttrSpecial(dst, l.AttrSpecial(src));
            l.SetAttrCgoExportDynamic(dst, l.AttrCgoExportDynamic(src));
            l.SetAttrCgoExportStatic(dst, l.AttrCgoExportStatic(src));
            l.SetAttrReadOnly(dst, l.AttrReadOnly(src));

        }

        // migrateAttributes copies over all of the attributes of symbol 'src' to
        // sym.Symbol 'dst'.
        private static void migrateAttributes(this ptr<Loader> _addr_l, Sym src, ptr<sym.Symbol> _addr_dst)
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbol dst = ref _addr_dst.val;

            dst.Value = l.SymValue(src);
            dst.Align = l.SymAlign(src);
            dst.Sect = l.SymSect(src);

            dst.Attr.Set(sym.AttrReachable, l.AttrReachable(src));
            dst.Attr.Set(sym.AttrOnList, l.AttrOnList(src));
            dst.Attr.Set(sym.AttrLocal, l.AttrLocal(src));
            dst.Attr.Set(sym.AttrNotInSymbolTable, l.AttrNotInSymbolTable(src));
            dst.Attr.Set(sym.AttrNoSplit, l.IsNoSplit(src));
            dst.Attr.Set(sym.AttrVisibilityHidden, l.AttrVisibilityHidden(src));
            dst.Attr.Set(sym.AttrDuplicateOK, l.AttrDuplicateOK(src));
            dst.Attr.Set(sym.AttrShared, l.AttrShared(src));
            dst.Attr.Set(sym.AttrExternal, l.AttrExternal(src));
            dst.Attr.Set(sym.AttrTopFrame, l.AttrTopFrame(src));
            dst.Attr.Set(sym.AttrSpecial, l.AttrSpecial(src));
            dst.Attr.Set(sym.AttrCgoExportDynamic, l.AttrCgoExportDynamic(src));
            dst.Attr.Set(sym.AttrCgoExportStatic, l.AttrCgoExportStatic(src));
            dst.Attr.Set(sym.AttrReadOnly, l.AttrReadOnly(src)); 

            // Convert outer relationship
            {
                var (outer, ok) = l.outer[src];

                if (ok)
                {
                    dst.Outer = l.Syms[outer];
                } 

                // Set sub-symbol attribute. See the comment on the AttrSubSymbol
                // method for more on this, there is some tricky stuff here.

            } 

            // Set sub-symbol attribute. See the comment on the AttrSubSymbol
            // method for more on this, there is some tricky stuff here.
            dst.Attr.Set(sym.AttrSubSymbol, l.outer[src] != 0L && l.sub[l.outer[src]] != 0L); 

            // Copy over dynimplib, dynimpvers, extname.
            {
                var (name, ok) = l.extname[src];

                if (ok)
                {
                    dst.SetExtname(name);
                }

            }

            if (l.SymDynimplib(src) != "")
            {
                dst.SetDynimplib(l.SymDynimplib(src));
            }

            if (l.SymDynimpvers(src) != "")
            {
                dst.SetDynimpvers(l.SymDynimpvers(src));
            } 

            // Copy ELF type if set.
            {
                var (et, ok) = l.elfType[src];

                if (ok)
                {
                    dst.SetElfType(et);
                } 

                // Copy pe objects values if set.

            } 

            // Copy pe objects values if set.
            {
                var (plt, ok) = l.plt[src];

                if (ok)
                {
                    dst.SetPlt(plt);
                }

            }

            {
                var (got, ok) = l.got[src];

                if (ok)
                {
                    dst.SetGot(got);
                } 

                // Copy dynid

            } 

            // Copy dynid
            {
                var (dynid, ok) = l.dynid[src];

                if (ok)
                {
                    dst.Dynid = dynid;
                }

            }

        }

        // CreateExtSym creates a new external symbol with the specified name
        // without adding it to any lookup tables, returning a Sym index for it.
        private static Sym CreateExtSym(this ptr<Loader> _addr_l, @string name, long ver)
        {
            ref Loader l = ref _addr_l.val;

            return l.newExtSym(name, ver);
        }

        // CreateStaticSym creates a new static symbol with the specified name
        // without adding it to any lookup tables, returning a Sym index for it.
        private static Sym CreateStaticSym(this ptr<Loader> _addr_l, @string name)
        {
            ref Loader l = ref _addr_l.val;
 
            // Assign a new unique negative version -- this is to mark the
            // symbol so that it can be skipped when ExtractSymbols is adding
            // ext syms to the sym.Symbols hash.
            l.anonVersion--;
            return l.newExtSym(name, l.anonVersion);

        }

        private static void FreeSym(this ptr<Loader> _addr_l, Sym i)
        {
            ref Loader l = ref _addr_l.val;

            if (l.IsExternal(i))
            {
                var pp = l.getPayload(i);
                pp.val = new extSymPayload();
            }

        }

        private static void loadObjFull(ptr<Loader> _addr_l, ptr<oReader> _addr_r, bool needReloc, bool needExtReloc) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref oReader r = ref _addr_r.val;

            for (long i = 0L;
            var n = r.NSym() + r.NNonpkgdef(); i < n; i++)
            { 
                // A symbol may be a dup or overwritten. In this case, its
                // content will actually be provided by a different object
                // (to which its global index points). Skip those symbols.
                var gi = l.toGlobal(r, i);
                {
                    var (r2, i2) = l.toLocal(gi);

                    if (r2 != r || i2 != i)
                    {
                        continue;
                    }

                }

                var s = l.Syms[gi];
                if (s == null)
                {
                    continue;
                }

                l.migrateAttributes(gi, s); 
                // Be careful not to overwrite attributes set by the linker.
                // Don't use the attributes from the object file.

                var osym = r.Sym(i);
                var size = osym.Siz(); 

                // Symbol data
                s.P = l.OutData(gi); 

                // Relocs
                if (needReloc)
                {
                    ref var relocs = ref heap(l.relocs(r, i), out ptr<var> _addr_relocs);
                    var batch = l.relocBatch;
                    s.R = batch.slice(-1, relocs.Count(), relocs.Count());
                    l.relocBatch = batch[relocs.Count()..];
                    l.convertRelocations(gi, _addr_relocs, s, false);
                }

                if (needExtReloc)
                {
                    l.convertExtRelocs(s, gi);
                } 

                // Aux symbol info
                var auxs = r.Auxs(i);
                foreach (var (j) in auxs)
                {
                    var a = _addr_auxs[j];

                    if (a.Type() == goobj2.AuxFuncInfo || a.Type() == goobj2.AuxFuncdata || a.Type() == goobj2.AuxGotype)                     else if (a.Type() == goobj2.AuxDwarfInfo || a.Type() == goobj2.AuxDwarfLoc || a.Type() == goobj2.AuxDwarfRanges || a.Type() == goobj2.AuxDwarfLines)                     else 
                        panic("unknown aux type");
                    
                }
                if (s.Size < int64(size))
                {
                    s.Size = int64(size);
                }

            }


        });

        // convertRelocations takes a vector of loader.Reloc relocations and
        // translates them into an equivalent set of sym.Reloc relocations on
        // the symbol "dst", performing fixups along the way for ABI aliases,
        // etc. It is assumed that the caller has pre-allocated the dst symbol
        // relocations slice. If 'strict' is set, then this method will
        // panic if it finds a relocation targeting a nil symbol.
        private static void convertRelocations(this ptr<Loader> _addr_l, Sym symIdx, ptr<Relocs> _addr_src, ptr<sym.Symbol> _addr_dst, bool strict) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref Relocs src = ref _addr_src.val;
            ref sym.Symbol dst = ref _addr_dst.val;

            foreach (var (j) in dst.R)
            {
                var r = src.At2(j);
                var rs = r.Sym();
                var sz = r.Siz();
                var rt = r.Type();
                if (rt == objabi.R_METHODOFF)
                {
                    if (l.attrReachable.Has(rs))
                    {
                        rt = objabi.R_ADDROFF;
                    }
                    else
                    {
                        sz = 0L;
                        rs = 0L;
                    }

                }

                if (rt == objabi.R_WEAKADDROFF && !l.attrReachable.Has(rs))
                {
                    rs = 0L;
                    sz = 0L;
                }

                if (rs != 0L && l.Syms[rs] != null && l.Syms[rs].Type == sym.SABIALIAS)
                {
                    var rsrelocs = l.Relocs(rs);
                    rs = rsrelocs.At2(0L).Sym();
                }

                if (strict && rs != 0L && l.Syms[rs] == null && rt != objabi.R_USETYPE)
                {
                    panic("nil reloc target in convertRelocations");
                }

                dst.R[j] = new sym.Reloc(Off:r.Off(),Siz:sz,Type:rt,Add:r.Add(),Sym:l.Syms[rs],);
                {
                    var rv = l.RelocVariant(symIdx, j);

                    if (rv != 0L)
                    {
                        dst.R[j].InitExt();
                        dst.R[j].Variant = rv;
                    }

                }

            }

        });

        // Convert external relocations to sym.Relocs on symbol dst.
        private static void convertExtRelocs(this ptr<Loader> _addr_l, ptr<sym.Symbol> _addr_dst, Sym src) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
            ref sym.Symbol dst = ref _addr_dst.val;

            if (int(src) >= len(l.extRelocs))
            {
                return ;
            }

            var extRelocs = l.extRelocs[src];
            if (len(extRelocs) == 0L)
            {
                return ;
            }

            if (len(dst.R) != 0L)
            {
                panic("bad");
            }

            var n = len(extRelocs);
            var batch = l.relocBatch;
            dst.R = batch.slice(-1, n, n);
            l.relocBatch = batch[n..];
            var relocs = l.Relocs(src);
            foreach (var (i) in dst.R)
            {
                var er = _addr_extRelocs[i];
                var sr = relocs.At2(er.Idx);
                var r = _addr_dst.R[i];
                r.RelocExt = _addr_l.relocExtBatch[0L];
                l.relocExtBatch = l.relocExtBatch[1L..];
                r.Off = sr.Off();
                r.Siz = sr.Siz();
                r.Type = sr.Type();
                r.Sym = l.Syms[l.ResolveABIAlias(sr.Sym())];
                r.Add = sr.Add();
                r.Xsym = l.Syms[er.Xsym];
                r.Xadd = er.Xadd;
                {
                    var rv = l.RelocVariant(src, er.Idx);

                    if (rv != 0L)
                    {
                        r.Variant = rv;
                    }

                }

            }

        });

        // relocId is essentially a <S,R> tuple identifying the Rth
        // relocation of symbol S.
        private partial struct relocId
        {
            public Sym sym;
            public long ridx;
        }

        // SetRelocVariant sets the 'variant' property of a relocation on
        // some specific symbol.
        private static void SetRelocVariant(this ptr<Loader> _addr_l, Sym s, long ri, sym.RelocVariant v) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;
 
            // sanity check
            {
                var relocs = l.Relocs(s);

                if (ri >= relocs.Count())
                {
                    panic("invalid relocation ID");
                }

            }

            if (l.relocVariant == null)
            {
                l.relocVariant = make_map<relocId, sym.RelocVariant>();
            }

            if (v != 0L)
            {
                l.relocVariant[new relocId(s,ri)] = v;
            }
            else
            {
                delete(l.relocVariant, new relocId(s,ri));
            }

        });

        // RelocVariant returns the 'variant' property of a relocation on
        // some specific symbol.
        private static sym.RelocVariant RelocVariant(this ptr<Loader> _addr_l, Sym s, long ri)
        {
            ref Loader l = ref _addr_l.val;

            return l.relocVariant[new relocId(s,ri)];
        }

        // UndefinedRelocTargets iterates through the global symbol index
        // space, looking for symbols with relocations targeting undefined
        // references. The linker's loadlib method uses this to determine if
        // there are unresolved references to functions in system libraries
        // (for example, libgcc.a), presumably due to CGO code. Return
        // value is a list of loader.Sym's corresponding to the undefined
        // cross-refs. The "limit" param controls the maximum number of
        // results returned; if "limit" is -1, then all undefs are returned.
        private static slice<Sym> UndefinedRelocTargets(this ptr<Loader> _addr_l, long limit)
        {
            ref Loader l = ref _addr_l.val;

            Sym result = new slice<Sym>(new Sym[] {  });
            for (var si = Sym(1L); si < Sym(len(l.objSyms)); si++)
            {
                var relocs = l.Relocs(si);
                for (long ri = 0L; ri < relocs.Count(); ri++)
                {
                    var r = relocs.At2(ri);
                    var rs = r.Sym();
                    if (rs != 0L && l.SymType(rs) == sym.SXREF && l.RawSymName(rs) != ".got")
                    {
                        result = append(result, rs);
                        if (limit != -1L && len(result) >= limit)
                        {
                            break;
                        }

                    }

                }


            }

            return result;

        }

        // AssignTextSymbolOrder populates the Textp2 slices within each
        // library and compilation unit, insuring that packages are laid down
        // in dependency order (internal first, then everything else). Return value
        // is a slice of all text syms.
        private static slice<Sym> AssignTextSymbolOrder(this ptr<Loader> _addr_l, slice<ptr<sym.Library>> libs, slice<bool> intlibs, slice<Sym> extsyms) => func((_, panic, __) =>
        {
            ref Loader l = ref _addr_l.val;

            // Library Textp2 lists should be empty at this point.
            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in libs)
                {
                    lib = __lib;
                    if (len(lib.Textp2) != 0L)
                    {
                        panic("expected empty Textp2 slice for library");
                    }

                    if (len(lib.DupTextSyms2) != 0L)
                    {
                        panic("expected empty DupTextSyms2 slice for library");
                    }

                } 

                // Used to record which dupok symbol we've assigned to a unit.
                // Can't use the onlist attribute here because it will need to
                // clear for the later assignment of the sym.Symbol to a unit.
                // NB: we can convert to using onList once we no longer have to
                // call the regular addToTextp.

                lib = lib__prev1;
            }

            var assignedToUnit = MakeBitmap(l.NSym() + 1L); 

            // Start off textp2 with reachable external syms.
            Sym textp2 = new slice<Sym>(new Sym[] {  });
            {
                var sym__prev1 = sym;

                foreach (var (_, __sym) in extsyms)
                {
                    sym = __sym;
                    if (!l.attrReachable.Has(sym))
                    {
                        continue;
                    }

                    textp2 = append(textp2, sym);

                } 

                // Walk through all text symbols from Go object files and append
                // them to their corresponding library's textp2 list.

                sym = sym__prev1;
            }

            foreach (var (_, o) in l.objs[1L..])
            {
                var r = o.r;
                var lib = r.unit.Lib;
                {
                    long i__prev2 = i;

                    for (long i = 0L;
                    var n = r.NSym() + r.NNonpkgdef(); i < n; i++)
                    {
                        var gi = l.toGlobal(r, i);
                        if (!l.attrReachable.Has(gi))
                        {
                            continue;
                        }

                        var osym = r.Sym(i);
                        var st = sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())];
                        if (st != sym.STEXT)
                        {
                            continue;
                        }

                        var dupok = osym.Dupok();
                        {
                            var (r2, i2) = l.toLocal(gi);

                            if (r2 != r || i2 != i)
                            { 
                                // A dupok text symbol is resolved to another package.
                                // We still need to record its presence in the current
                                // package, as the trampoline pass expects packages
                                // are laid out in dependency order.
                                lib.DupTextSyms2 = append(lib.DupTextSyms2, sym.LoaderSym(gi));
                                continue; // symbol in different object
                            }

                        }

                        if (dupok)
                        {
                            lib.DupTextSyms2 = append(lib.DupTextSyms2, sym.LoaderSym(gi));
                            continue;
                        }

                        lib.Textp2 = append(lib.Textp2, sym.LoaderSym(gi));

                    }


                    i = i__prev2;
                }

            } 

            // Now assemble global textp, and assign text symbols to units.
            foreach (var (_, doInternal) in new array<bool>(new bool[] { true, false }))
            {
                {
                    var lib__prev2 = lib;

                    foreach (var (__idx, __lib) in libs)
                    {
                        idx = __idx;
                        lib = __lib;
                        if (intlibs[idx] != doInternal)
                        {
                            continue;
                        }

                        array<slice<sym.LoaderSym>> lists = new array<slice<sym.LoaderSym>>(new slice<sym.LoaderSym>[] { lib.Textp2, lib.DupTextSyms2 });
                        {
                            long i__prev3 = i;

                            foreach (var (__i, __list) in lists)
                            {
                                i = __i;
                                list = __list;
                                foreach (var (_, s) in list)
                                {
                                    var sym = Sym(s);
                                    if (l.attrReachable.Has(sym) && !assignedToUnit.Has(sym))
                                    {
                                        textp2 = append(textp2, sym);
                                        var unit = l.SymUnit(sym);
                                        if (unit != null)
                                        {
                                            unit.Textp2 = append(unit.Textp2, s);
                                            assignedToUnit.Set(sym);
                                        } 
                                        // Dupok symbols may be defined in multiple packages; the
                                        // associated package for a dupok sym is chosen sort of
                                        // arbitrarily (the first containing package that the linker
                                        // loads). Canonicalizes its Pkg to the package with which
                                        // it will be laid down in text.
                                        if (i == 1L && l.SymPkg(sym) != lib.Pkg)
                                        {
                                            l.SetSymPkg(sym, lib.Pkg);
                                        }

                                    }

                                }

                            }

                            i = i__prev3;
                        }

                        lib.Textp2 = null;
                        lib.DupTextSyms2 = null;

                    }

                    lib = lib__prev2;
                }
            }
            return textp2;

        });

        // ErrorReporter is a helper class for reporting errors.
        public partial struct ErrorReporter
        {
            public ptr<Loader> ldr;
            public Action AfterErrorAction;
        }

        // Errorf method logs an error message.
        //
        // After each error, the error actions function will be invoked; this
        // will either terminate the link immediately (if -h option given)
        // or it will keep a count and exit if more than 20 errors have been printed.
        //
        // Logging an error means that on exit cmd/link will delete any
        // output file and return a non-zero error code.
        //
        private static void Errorf(this ptr<ErrorReporter> _addr_reporter, Sym s, @string format, params object[] args)
        {
            args = args.Clone();
            ref ErrorReporter reporter = ref _addr_reporter.val;

            if (s != 0L && reporter.ldr.SymName(s) != "")
            {
                format = reporter.ldr.SymName(s) + ": " + format;
            }
            else
            {
                format = fmt.Sprintf("sym %d: %s", s, format);
            }

            format += "\n";
            fmt.Fprintf(os.Stderr, format, args);
            reporter.AfterErrorAction();

        }

        // GetErrorReporter returns the loader's associated error reporter.
        private static ptr<ErrorReporter> GetErrorReporter(this ptr<Loader> _addr_l)
        {
            ref Loader l = ref _addr_l.val;

            return _addr_l.errorReporter!;
        }

        // Errorf method logs an error message. See ErrorReporter.Errorf for details.
        private static void Errorf(this ptr<Loader> _addr_l, Sym s, @string format, params object[] args)
        {
            args = args.Clone();
            ref Loader l = ref _addr_l.val;

            l.errorReporter.Errorf(s, format, args);
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
            fmt.Println("extStart:", l.extStart);
            fmt.Println("Nsyms:", len(l.objSyms));
            fmt.Println("syms");
            {
                var i__prev1 = i;

                for (var i = Sym(1L); i < Sym(len(l.objSyms)); i++)
                {
                    if (l.IsExternal(i))
                    {
                        pi = fmt.Sprintf("<ext %d>", l.extIndex(i));
                    }

                    ptr<sym.Symbol> s;
                    if (int(i) < len(l.Syms))
                    {
                        s = l.Syms[i];
                    }

                    if (s != null)
                    {
                        fmt.Println(i, s, s.Type, pi);
                    }
                    else
                    {
                        fmt.Println(i, l.SymName(i), "<not loaded>", pi);
                    }

                }


                i = i__prev1;
            }
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

            fmt.Println("payloads:");
            {
                var i__prev1 = i;

                foreach (var (__i) in l.payloads)
                {
                    i = __i;
                    var pp = l.payloads[i];
                    fmt.Println(i, pp.name, pp.ver, pp.kind);
                }

                i = i__prev1;
            }
        }
    }
}}}}
