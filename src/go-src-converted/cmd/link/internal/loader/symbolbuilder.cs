// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package loader -- go2cs converted at 2022 March 13 06:33:32 UTC
// import "cmd/link/internal/loader" ==> using loader = go.cmd.link.@internal.loader_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\loader\symbolbuilder.go
namespace go.cmd.link.@internal;

using goobj = cmd.@internal.goobj_package;
using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using sym = cmd.link.@internal.sym_package;
using sort = sort_package;


// SymbolBuilder is a helper designed to help with the construction
// of new symbol contents.

using System;
public static partial class loader_package {

public partial struct SymbolBuilder {
    public ref ptr<extSymPayload> ptr<extSymPayload> => ref ptr<extSymPayload>_ptr; // points to payload being updated
    public Sym symIdx; // index of symbol being updated/constructed
    public ptr<Loader> l; // loader
}

// MakeSymbolBuilder creates a symbol builder for use in constructing
// an entirely new symbol.
private static ptr<SymbolBuilder> MakeSymbolBuilder(this ptr<Loader> _addr_l, @string name) {
    ref Loader l = ref _addr_l.val;
 
    // for now assume that any new sym is intended to be static
    var symIdx = l.CreateStaticSym(name);
    ptr<SymbolBuilder> sb = addr(new SymbolBuilder(l:l,symIdx:symIdx));
    sb.extSymPayload = l.getPayload(symIdx);
    return _addr_sb!;
}

// MakeSymbolUpdater creates a symbol builder helper for an existing
// symbol 'symIdx'. If 'symIdx' is not an external symbol, then create
// a clone of it (copy name, properties, etc) fix things up so that
// the lookup tables and caches point to the new version, not the old
// version.
private static ptr<SymbolBuilder> MakeSymbolUpdater(this ptr<Loader> _addr_l, Sym symIdx) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (symIdx == 0) {
        panic("can't update the null symbol");
    }
    if (!l.IsExternal(symIdx)) { 
        // Create a clone with the same name/version/kind etc.
        l.cloneToExternal(symIdx);
    }
    ptr<SymbolBuilder> sb = addr(new SymbolBuilder(l:l,symIdx:symIdx));
    sb.extSymPayload = l.getPayload(symIdx);
    return _addr_sb!;
});

// CreateSymForUpdate creates a symbol with given name and version,
// returns a CreateSymForUpdate for update. If the symbol already
// exists, it will update in-place.
private static ptr<SymbolBuilder> CreateSymForUpdate(this ptr<Loader> _addr_l, @string name, nint version) {
    ref Loader l = ref _addr_l.val;

    var s = l.LookupOrCreateSym(name, version);
    l.SetAttrReachable(s, true);
    return _addr_l.MakeSymbolUpdater(s)!;
}

// Getters for properties of the symbol we're working on.

private static Sym Sym(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.symIdx;
}
private static @string Name(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.name;
}
private static nint Version(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.ver;
}
private static sym.SymKind Type(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.kind;
}
private static long Size(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.size;
}
private static slice<byte> Data(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.data;
}
private static long Value(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymValue(sb.symIdx);
}
private static int Align(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymAlign(sb.symIdx);
}
private static byte Localentry(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymLocalentry(sb.symIdx);
}
private static bool OnList(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrOnList(sb.symIdx);
}
private static bool External(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrExternal(sb.symIdx);
}
private static @string Extname(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymExtname(sb.symIdx);
}
private static bool CgoExportDynamic(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrCgoExportDynamic(sb.symIdx);
}
private static @string Dynimplib(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymDynimplib(sb.symIdx);
}
private static @string Dynimpvers(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymDynimpvers(sb.symIdx);
}
private static Sym SubSym(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SubSym(sb.symIdx);
}
private static Sym GoType(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SymGoType(sb.symIdx);
}
private static bool VisibilityHidden(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrVisibilityHidden(sb.symIdx);
}
private static ptr<sym.Section> Sect(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return _addr_sb.l.SymSect(sb.symIdx)!;
}

// Setters for symbol properties.

private static void SetType(this ptr<SymbolBuilder> _addr_sb, sym.SymKind kind) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.kind = kind;
}
private static void SetSize(this ptr<SymbolBuilder> _addr_sb, long size) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.size = size;
}
private static void SetData(this ptr<SymbolBuilder> _addr_sb, slice<byte> data) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.data = data;
}
private static void SetOnList(this ptr<SymbolBuilder> _addr_sb, bool v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrOnList(sb.symIdx, v);
}
private static void SetExternal(this ptr<SymbolBuilder> _addr_sb, bool v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrExternal(sb.symIdx, v);
}
private static void SetValue(this ptr<SymbolBuilder> _addr_sb, long v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymValue(sb.symIdx, v);
}
private static void SetAlign(this ptr<SymbolBuilder> _addr_sb, int align) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymAlign(sb.symIdx, align);
}
private static void SetLocalentry(this ptr<SymbolBuilder> _addr_sb, byte value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymLocalentry(sb.symIdx, value);
}
private static void SetExtname(this ptr<SymbolBuilder> _addr_sb, @string value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymExtname(sb.symIdx, value);
}
private static void SetDynimplib(this ptr<SymbolBuilder> _addr_sb, @string value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymDynimplib(sb.symIdx, value);
}
private static void SetDynimpvers(this ptr<SymbolBuilder> _addr_sb, @string value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetSymDynimpvers(sb.symIdx, value);
}
private static void SetPlt(this ptr<SymbolBuilder> _addr_sb, int value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetPlt(sb.symIdx, value);
}
private static void SetGot(this ptr<SymbolBuilder> _addr_sb, int value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetGot(sb.symIdx, value);
}
private static void SetSpecial(this ptr<SymbolBuilder> _addr_sb, bool value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrSpecial(sb.symIdx, value);
}
private static void SetLocal(this ptr<SymbolBuilder> _addr_sb, bool value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrLocal(sb.symIdx, value);
}
private static void SetVisibilityHidden(this ptr<SymbolBuilder> _addr_sb, bool value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrVisibilityHidden(sb.symIdx, value);
}
private static void SetNotInSymbolTable(this ptr<SymbolBuilder> _addr_sb, bool value) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrNotInSymbolTable(sb.symIdx, value);
}
private static void SetSect(this ptr<SymbolBuilder> _addr_sb, ptr<sym.Section> _addr_sect) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sym.Section sect = ref _addr_sect.val;

    sb.l.SetSymSect(sb.symIdx, sect);
}

private static void AddBytes(this ptr<SymbolBuilder> _addr_sb, slice<byte> data) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    if (sb.kind == 0) {
        sb.kind = sym.SDATA;
    }
    sb.data = append(sb.data, data);
    sb.size = int64(len(sb.data));
}

private static Relocs Relocs(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.Relocs(sb.symIdx);
}

// ResetRelocs removes all relocations on this symbol.
private static void ResetRelocs(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.relocs = sb.relocs[..(int)0];
}

// SetRelocType sets the type of the 'i'-th relocation on this sym to 't'
private static void SetRelocType(this ptr<SymbolBuilder> _addr_sb, nint i, objabi.RelocType t) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.relocs[i].SetType(uint16(t));
}

// SetRelocSym sets the target sym of the 'i'-th relocation on this sym to 's'
private static void SetRelocSym(this ptr<SymbolBuilder> _addr_sb, nint i, Sym tgt) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.relocs[i].SetSym(new goobj.SymRef(PkgIdx:0,SymIdx:uint32(tgt)));
}

// SetRelocAdd sets the addend of the 'i'-th relocation on this sym to 'a'
private static void SetRelocAdd(this ptr<SymbolBuilder> _addr_sb, nint i, long a) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.relocs[i].SetAdd(a);
}

// Add n relocations, return a handle to the relocations.
private static Relocs AddRelocs(this ptr<SymbolBuilder> _addr_sb, nint n) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.relocs = append(sb.relocs, make_slice<goobj.Reloc>(n));
    return sb.l.Relocs(sb.symIdx);
}

// Add a relocation with given type, return its handle and index
// (to set other fields).
private static (Reloc, nint) AddRel(this ptr<SymbolBuilder> _addr_sb, objabi.RelocType typ) {
    Reloc _p0 = default;
    nint _p0 = default;
    ref SymbolBuilder sb = ref _addr_sb.val;

    var j = len(sb.relocs);
    sb.relocs = append(sb.relocs, new goobj.Reloc());
    sb.relocs[j].SetType(uint16(typ));
    var relocs = sb.Relocs();
    return (relocs.At(j), j);
}

// Sort relocations by offset.
private static void SortRelocs(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sort.Sort((relocsByOff.val)(sb.extSymPayload));
}

// Implement sort.Interface
private partial struct relocsByOff { // : extSymPayload
}

private static nint Len(this ptr<relocsByOff> _addr_p) {
    ref relocsByOff p = ref _addr_p.val;

    return len(p.relocs);
}
private static bool Less(this ptr<relocsByOff> _addr_p, nint i, nint j) {
    ref relocsByOff p = ref _addr_p.val;

    return p.relocs[i].Off() < p.relocs[j].Off();
}
private static void Swap(this ptr<relocsByOff> _addr_p, nint i, nint j) {
    ref relocsByOff p = ref _addr_p.val;

    (p.relocs[i], p.relocs[j]) = (p.relocs[j], p.relocs[i]);
}

private static bool Reachable(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrReachable(sb.symIdx);
}

private static void SetReachable(this ptr<SymbolBuilder> _addr_sb, bool v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrReachable(sb.symIdx, v);
}

private static void setReachable(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.SetReachable(true);
}

private static bool ReadOnly(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrReadOnly(sb.symIdx);
}

private static void SetReadOnly(this ptr<SymbolBuilder> _addr_sb, bool v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrReadOnly(sb.symIdx, v);
}

private static bool DuplicateOK(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.AttrDuplicateOK(sb.symIdx);
}

private static void SetDuplicateOK(this ptr<SymbolBuilder> _addr_sb, bool v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SetAttrDuplicateOK(sb.symIdx, v);
}

private static Sym Outer(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.OuterSym(sb.symIdx);
}

private static Sym Sub(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    return sb.l.SubSym(sb.symIdx);
}

private static void SortSub(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.SortSub(sb.symIdx);
}

private static void AddInteriorSym(this ptr<SymbolBuilder> _addr_sb, Sym sub) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    sb.l.AddInteriorSym(sb.symIdx, sub);
}

private static long AddUint8(this ptr<SymbolBuilder> _addr_sb, byte v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    var off = sb.size;
    if (sb.kind == 0) {
        sb.kind = sym.SDATA;
    }
    sb.size++;
    sb.data = append(sb.data, v);
    return off;
}

private static long AddUintXX(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, ulong v, nint wid) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var off = sb.size;
    sb.setUintXX(arch, off, v, int64(wid));
    return off;
}

private static long setUintXX(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long off, ulong v, long wid) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    if (sb.kind == 0) {
        sb.kind = sym.SDATA;
    }
    if (sb.size < off + wid) {
        sb.size = off + wid;
        sb.Grow(sb.size);
    }
    switch (wid) {
        case 1: 
            sb.data[off] = uint8(v);
            break;
        case 2: 
            arch.ByteOrder.PutUint16(sb.data[(int)off..], uint16(v));
            break;
        case 4: 
            arch.ByteOrder.PutUint32(sb.data[(int)off..], uint32(v));
            break;
        case 8: 
            arch.ByteOrder.PutUint64(sb.data[(int)off..], v);
            break;
    }

    return off + wid;
}

private static long AddUint16(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, ushort v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.AddUintXX(arch, uint64(v), 2);
}

private static long AddUint32(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, uint v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.AddUintXX(arch, uint64(v), 4);
}

private static long AddUint64(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, ulong v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.AddUintXX(arch, v, 8);
}

private static long AddUint(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, ulong v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.AddUintXX(arch, v, arch.PtrSize);
}

private static long SetUint8(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long r, byte v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.setUintXX(arch, r, uint64(v), 1);
}

private static long SetUint16(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long r, ushort v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.setUintXX(arch, r, uint64(v), 2);
}

private static long SetUint32(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long r, uint v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.setUintXX(arch, r, uint64(v), 4);
}

private static long SetUint(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long r, ulong v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.setUintXX(arch, r, v, int64(arch.PtrSize));
}

private static long SetUintptr(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long r, System.UIntPtr v) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.setUintXX(arch, r, uint64(v), int64(arch.PtrSize));
}

private static long SetAddrPlus(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long off, Sym tgt, long add) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    if (sb.Type() == 0) {
        sb.SetType(sym.SDATA);
    }
    if (off + int64(arch.PtrSize) > sb.size) {
        sb.size = off + int64(arch.PtrSize);
        sb.Grow(sb.size);
    }
    var (r, _) = sb.AddRel(objabi.R_ADDR);
    r.SetSym(tgt);
    r.SetOff(int32(off));
    r.SetSiz(uint8(arch.PtrSize));
    r.SetAdd(add);
    return off + int64(r.Siz());
}

private static long SetAddr(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, long off, Sym tgt) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.SetAddrPlus(arch, off, tgt, 0);
}

private static long AddStringAt(this ptr<SymbolBuilder> _addr_sb, long off, @string str) => func((_, panic, _) => {
    ref SymbolBuilder sb = ref _addr_sb.val;

    var strLen = int64(len(str));
    if (off + strLen + 1 > int64(len(sb.data))) {
        panic("attempt to write past end of buffer");
    }
    copy(sb.data[(int)off..(int)off + strLen], str);
    sb.data[off + strLen] = 0;
    return off + strLen + 1;
});

private static long Addstring(this ptr<SymbolBuilder> _addr_sb, @string str) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    if (sb.kind == 0) {
        sb.kind = sym.SNOPTRDATA;
    }
    var r = sb.size;
    if (sb.name == ".shstrtab") { 
        // FIXME: find a better mechanism for this
        sb.l.elfsetstring(str, int(r));
    }
    sb.data = append(sb.data, str);
    sb.data = append(sb.data, 0);
    sb.size = int64(len(sb.data));
    return r;
}

private static long SetBytesAt(this ptr<SymbolBuilder> _addr_sb, long off, slice<byte> b) => func((_, panic, _) => {
    ref SymbolBuilder sb = ref _addr_sb.val;

    var datLen = int64(len(b));
    if (off + datLen > int64(len(sb.data))) {
        panic("attempt to write past end of buffer");
    }
    copy(sb.data[(int)off..(int)off + datLen], b);
    return off + datLen;
});

private static long addSymRef(this ptr<SymbolBuilder> _addr_sb, Sym tgt, long add, objabi.RelocType typ, nint rsize) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    if (sb.kind == 0) {
        sb.kind = sym.SDATA;
    }
    var i = sb.size;

    sb.size += int64(rsize);
    sb.Grow(sb.size);

    var (r, _) = sb.AddRel(typ);
    r.SetSym(tgt);
    r.SetOff(int32(i));
    r.SetSiz(uint8(rsize));
    r.SetAdd(add);

    return i + int64(rsize);
}

// Add a symbol reference (relocation) with given type, addend, and size
// (the most generic form).
private static long AddSymRef(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt, long add, objabi.RelocType typ, nint rsize) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, add, typ, rsize);
}

private static long AddAddrPlus(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt, long add) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, add, objabi.R_ADDR, arch.PtrSize);
}

private static long AddAddrPlus4(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt, long add) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, add, objabi.R_ADDR, 4);
}

private static long AddAddr(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.AddAddrPlus(arch, tgt, 0);
}

private static long AddPCRelPlus(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt, long add) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, add, objabi.R_PCREL, 4);
}

private static long AddCURelativeAddrPlus(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt, long add) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, add, objabi.R_ADDRCUOFF, arch.PtrSize);
}

private static long AddSize(this ptr<SymbolBuilder> _addr_sb, ptr<sys.Arch> _addr_arch, Sym tgt) {
    ref SymbolBuilder sb = ref _addr_sb.val;
    ref sys.Arch arch = ref _addr_arch.val;

    return sb.addSymRef(tgt, 0, objabi.R_SIZE, arch.PtrSize);
}

// GenAddAddrPlusFunc returns a function to be called when capturing
// a function symbol's address. In later stages of the link (when
// address assignment is done) when doing internal linking and
// targeting an executable, we can just emit the address of a function
// directly instead of generating a relocation. Clients can call
// this function (setting 'internalExec' based on build mode and target)
// and then invoke the returned function in roughly the same way that
// loader.*SymbolBuilder.AddAddrPlus would be used.
public static Func<ptr<SymbolBuilder>, ptr<sys.Arch>, Sym, long, long> GenAddAddrPlusFunc(bool internalExec) {
    if (internalExec) {
        return (s, arch, tgt, add) => {
            {
                var v = s.l.SymValue(tgt);

                if (v != 0) {
                    return s.AddUint(arch, uint64(v + add));
                }

            }
            return s.AddAddrPlus(arch, tgt, add);
        }
    else;
    } {
        return (SymbolBuilder.val).AddAddrPlus;
    }
}

private static void MakeWritable(this ptr<SymbolBuilder> _addr_sb) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    if (sb.ReadOnly()) {
        sb.data = append((slice<byte>)null, sb.data);
        sb.l.SetAttrReadOnly(sb.symIdx, false);
    }
}

private static void AddUleb(this ptr<SymbolBuilder> _addr_sb, ulong v) {
    ref SymbolBuilder sb = ref _addr_sb.val;

    if (v < 128) { // common case: 1 byte
        sb.AddUint8(uint8(v));
        return ;
    }
    while (true) {
        var c = uint8(v & 0x7f);
        v>>=7;
        if (v != 0) {
            c |= 0x80;
        }
        sb.AddUint8(c);
        if (c & 0x80 == 0) {
            break;
        }
    }
}

} // end loader_package
