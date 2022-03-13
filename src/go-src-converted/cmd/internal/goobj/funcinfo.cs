// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goobj -- go2cs converted at 2022 March 13 05:43:24 UTC
// import "cmd/internal/goobj" ==> using goobj = go.cmd.@internal.goobj_package
// Original source: C:\Program Files\Go\src\cmd\internal\goobj\funcinfo.go
namespace go.cmd.@internal;

using bytes = bytes_package;
using objabi = cmd.@internal.objabi_package;
using binary = encoding.binary_package;


// CUFileIndex is used to index the filenames that are stored in the
// per-package/per-CU FileList.

using System;
public static partial class goobj_package {

public partial struct CUFileIndex { // : uint
}

// FuncInfo is serialized as a symbol (aux symbol). The symbol data is
// the binary encoding of the struct below.
//
// TODO: make each pcdata a separate symbol?
public partial struct FuncInfo {
    public uint Args;
    public uint Locals;
    public objabi.FuncID FuncID;
    public objabi.FuncFlag FuncFlag;
    public SymRef Pcsp;
    public SymRef Pcfile;
    public SymRef Pcline;
    public SymRef Pcinline;
    public slice<SymRef> Pcdata;
    public slice<uint> Funcdataoff;
    public slice<CUFileIndex> File;
    public slice<InlTreeNode> InlTree;
}

private static void Write(this ptr<FuncInfo> _addr_a, ptr<bytes.Buffer> _addr_w) {
    ref FuncInfo a = ref _addr_a.val;
    ref bytes.Buffer w = ref _addr_w.val;

    Action<byte> writeUint8 = x => {
        w.WriteByte(x);
    };
    array<byte> b = new array<byte>(4);
    Action<uint> writeUint32 = x => {
        binary.LittleEndian.PutUint32(b[..], x);
        w.Write(b[..]);
    };
    Action<SymRef> writeSymRef = s => {
        writeUint32(s.PkgIdx);
        writeUint32(s.SymIdx);
    };

    writeUint32(a.Args);
    writeUint32(a.Locals);
    writeUint8(uint8(a.FuncID));
    writeUint8(uint8(a.FuncFlag));
    writeUint8(0); // pad to uint32 boundary
    writeUint8(0);
    writeSymRef(a.Pcsp);
    writeSymRef(a.Pcfile);
    writeSymRef(a.Pcline);
    writeSymRef(a.Pcinline);
    writeUint32(uint32(len(a.Pcdata)));
    foreach (var (_, sym) in a.Pcdata) {
        writeSymRef(sym);
    }    writeUint32(uint32(len(a.Funcdataoff)));
    foreach (var (_, x) in a.Funcdataoff) {
        writeUint32(x);
    }    writeUint32(uint32(len(a.File)));
    foreach (var (_, f) in a.File) {
        writeUint32(uint32(f));
    }    writeUint32(uint32(len(a.InlTree)));
    foreach (var (i) in a.InlTree) {
        a.InlTree[i].Write(w);
    }
}

// FuncInfoLengths is a cache containing a roadmap of offsets and
// lengths for things within a serialized FuncInfo. Each length field
// stores the number of items (e.g. files, inltree nodes, etc), and the
// corresponding "off" field stores the byte offset of the start of
// the items in question.
public partial struct FuncInfoLengths {
    public uint NumPcdata;
    public uint PcdataOff;
    public uint NumFuncdataoff;
    public uint FuncdataoffOff;
    public uint NumFile;
    public uint FileOff;
    public uint NumInlTree;
    public uint InlTreeOff;
    public bool Initialized;
}

private static FuncInfoLengths ReadFuncInfoLengths(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    FuncInfoLengths result = default; 

    // Offset to the number of pcdata values. This value is determined by counting
    // the number of bytes until we write pcdata to the file.
    const nint numpcdataOff = 44;

    result.NumPcdata = binary.LittleEndian.Uint32(b[(int)numpcdataOff..]);
    result.PcdataOff = numpcdataOff + 4;

    var numfuncdataoffOff = result.PcdataOff + 8 * result.NumPcdata;
    result.NumFuncdataoff = binary.LittleEndian.Uint32(b[(int)numfuncdataoffOff..]);
    result.FuncdataoffOff = numfuncdataoffOff + 4;

    var numfileOff = result.FuncdataoffOff + 4 * result.NumFuncdataoff;
    result.NumFile = binary.LittleEndian.Uint32(b[(int)numfileOff..]);
    result.FileOff = numfileOff + 4;

    var numinltreeOff = result.FileOff + 4 * result.NumFile;
    result.NumInlTree = binary.LittleEndian.Uint32(b[(int)numinltreeOff..]);
    result.InlTreeOff = numinltreeOff + 4;

    result.Initialized = true;

    return result;
}

private static uint ReadArgs(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return binary.LittleEndian.Uint32(b);
}

private static uint ReadLocals(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return binary.LittleEndian.Uint32(b[(int)4..]);
}

private static objabi.FuncID ReadFuncID(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return objabi.FuncID(b[8]);
}

private static objabi.FuncFlag ReadFuncFlag(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return objabi.FuncFlag(b[9]);
}

private static SymRef ReadPcsp(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return new SymRef(binary.LittleEndian.Uint32(b[12:]),binary.LittleEndian.Uint32(b[16:]));
}

private static SymRef ReadPcfile(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return new SymRef(binary.LittleEndian.Uint32(b[20:]),binary.LittleEndian.Uint32(b[24:]));
}

private static SymRef ReadPcline(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return new SymRef(binary.LittleEndian.Uint32(b[28:]),binary.LittleEndian.Uint32(b[32:]));
}

private static SymRef ReadPcinline(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return new SymRef(binary.LittleEndian.Uint32(b[36:]),binary.LittleEndian.Uint32(b[40:]));
}

private static slice<SymRef> ReadPcdata(this ptr<FuncInfo> _addr__p0, slice<byte> b) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    var syms = make_slice<SymRef>(binary.LittleEndian.Uint32(b[(int)44..]));
    foreach (var (i) in syms) {
        syms[i] = new SymRef(binary.LittleEndian.Uint32(b[48+i*8:]),binary.LittleEndian.Uint32(b[52+i*8:]));
    }    return syms;
}

private static long ReadFuncdataoff(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint funcdataofffoff, uint k) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return int64(binary.LittleEndian.Uint32(b[(int)funcdataofffoff + 4 * k..]));
}

private static CUFileIndex ReadFile(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint filesoff, uint k) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    return CUFileIndex(binary.LittleEndian.Uint32(b[(int)filesoff + 4 * k..]));
}

private static InlTreeNode ReadInlTree(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint inltreeoff, uint k) {
    ref FuncInfo _p0 = ref _addr__p0.val;

    const nint inlTreeNodeSize = 4 * 6;

    InlTreeNode result = default;
    result.Read(b[(int)inltreeoff + k * inlTreeNodeSize..]);
    return result;
}

// InlTreeNode is the serialized form of FileInfo.InlTree.
public partial struct InlTreeNode {
    public int Parent;
    public CUFileIndex File;
    public int Line;
    public SymRef Func;
    public int ParentPC;
}

private static void Write(this ptr<InlTreeNode> _addr_inl, ptr<bytes.Buffer> _addr_w) {
    ref InlTreeNode inl = ref _addr_inl.val;
    ref bytes.Buffer w = ref _addr_w.val;

    array<byte> b = new array<byte>(4);
    Action<uint> writeUint32 = x => {
        binary.LittleEndian.PutUint32(b[..], x);
        w.Write(b[..]);
    };
    writeUint32(uint32(inl.Parent));
    writeUint32(uint32(inl.File));
    writeUint32(uint32(inl.Line));
    writeUint32(inl.Func.PkgIdx);
    writeUint32(inl.Func.SymIdx);
    writeUint32(uint32(inl.ParentPC));
}

// Read an InlTreeNode from b, return the remaining bytes.
private static slice<byte> Read(this ptr<InlTreeNode> _addr_inl, slice<byte> b) {
    ref InlTreeNode inl = ref _addr_inl.val;

    Func<uint> readUint32 = () => {
        var x = binary.LittleEndian.Uint32(b);
        b = b[(int)4..];
        return x;
    };
    inl.Parent = int32(readUint32());
    inl.File = CUFileIndex(readUint32());
    inl.Line = int32(readUint32());
    inl.Func = new SymRef(readUint32(),readUint32());
    inl.ParentPC = int32(readUint32());
    return b;
}

} // end goobj_package
