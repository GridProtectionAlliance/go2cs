// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goobj2 -- go2cs converted at 2020 October 08 03:50:09 UTC
// import "cmd/internal/goobj2" ==> using goobj2 = go.cmd.@internal.goobj2_package
// Original source: C:\Go\src\cmd\internal\goobj2\funcinfo.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class goobj2_package
    {
        // FuncInfo is serialized as a symbol (aux symbol). The symbol data is
        // the binary encoding of the struct below.
        //
        // TODO: make each pcdata a separate symbol?
        public partial struct FuncInfo
        {
            public uint Args;
            public uint Locals;
            public uint Pcsp;
            public uint Pcfile;
            public uint Pcline;
            public uint Pcinline;
            public slice<uint> Pcdata;
            public uint PcdataEnd;
            public slice<uint> Funcdataoff;
            public slice<SymRef> File; // TODO: just use string?

            public slice<InlTreeNode> InlTree;
        }

        private static void Write(this ptr<FuncInfo> _addr_a, ptr<bytes.Buffer> _addr_w)
        {
            ref FuncInfo a = ref _addr_a.val;
            ref bytes.Buffer w = ref _addr_w.val;

            array<byte> b = new array<byte>(4L);
            Action<uint> writeUint32 = x =>
            {
                binary.LittleEndian.PutUint32(b[..], x);
                w.Write(b[..]);
            }
;

            writeUint32(a.Args);
            writeUint32(a.Locals);

            writeUint32(a.Pcsp);
            writeUint32(a.Pcfile);
            writeUint32(a.Pcline);
            writeUint32(a.Pcinline);
            writeUint32(uint32(len(a.Pcdata)));
            {
                var x__prev1 = x;

                foreach (var (_, __x) in a.Pcdata)
                {
                    x = __x;
                    writeUint32(x);
                }

                x = x__prev1;
            }

            writeUint32(a.PcdataEnd);
            writeUint32(uint32(len(a.Funcdataoff)));
            {
                var x__prev1 = x;

                foreach (var (_, __x) in a.Funcdataoff)
                {
                    x = __x;
                    writeUint32(x);
                }

                x = x__prev1;
            }

            writeUint32(uint32(len(a.File)));
            foreach (var (_, f) in a.File)
            {
                writeUint32(f.PkgIdx);
                writeUint32(f.SymIdx);
            }
            writeUint32(uint32(len(a.InlTree)));
            foreach (var (i) in a.InlTree)
            {
                a.InlTree[i].Write(w);
            }

        }

        private static void Read(this ptr<FuncInfo> _addr_a, slice<byte> b)
        {
            ref FuncInfo a = ref _addr_a.val;

            Func<uint> readUint32 = () =>
            {
                var x = binary.LittleEndian.Uint32(b);
                b = b[4L..];
                return x;
            }
;

            a.Args = readUint32();
            a.Locals = readUint32();

            a.Pcsp = readUint32();
            a.Pcfile = readUint32();
            a.Pcline = readUint32();
            a.Pcinline = readUint32();
            var pcdatalen = readUint32();
            a.Pcdata = make_slice<uint>(pcdatalen);
            {
                var i__prev1 = i;

                foreach (var (__i) in a.Pcdata)
                {
                    i = __i;
                    a.Pcdata[i] = readUint32();
                }

                i = i__prev1;
            }

            a.PcdataEnd = readUint32();
            var funcdataofflen = readUint32();
            a.Funcdataoff = make_slice<uint>(funcdataofflen);
            {
                var i__prev1 = i;

                foreach (var (__i) in a.Funcdataoff)
                {
                    i = __i;
                    a.Funcdataoff[i] = readUint32();
                }

                i = i__prev1;
            }

            var filelen = readUint32();
            a.File = make_slice<SymRef>(filelen);
            {
                var i__prev1 = i;

                foreach (var (__i) in a.File)
                {
                    i = __i;
                    a.File[i] = new SymRef(readUint32(),readUint32());
                }

                i = i__prev1;
            }

            var inltreelen = readUint32();
            a.InlTree = make_slice<InlTreeNode>(inltreelen);
            {
                var i__prev1 = i;

                foreach (var (__i) in a.InlTree)
                {
                    i = __i;
                    b = a.InlTree[i].Read(b);
                }

                i = i__prev1;
            }
        }

        // FuncInfoLengths is a cache containing a roadmap of offsets and
        // lengths for things within a serialized FuncInfo. Each length field
        // stores the number of items (e.g. files, inltree nodes, etc), and the
        // corresponding "off" field stores the byte offset of the start of
        // the items in question.
        public partial struct FuncInfoLengths
        {
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

        private static FuncInfoLengths ReadFuncInfoLengths(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            FuncInfoLengths result = default;

            const long numpcdataOff = (long)24L;

            result.NumPcdata = binary.LittleEndian.Uint32(b[numpcdataOff..]);
            result.PcdataOff = numpcdataOff + 4L;

            var numfuncdataoffOff = result.PcdataOff + 4L * (result.NumPcdata + 1L);
            result.NumFuncdataoff = binary.LittleEndian.Uint32(b[numfuncdataoffOff..]);
            result.FuncdataoffOff = numfuncdataoffOff + 4L;

            var numfileOff = result.FuncdataoffOff + 4L * result.NumFuncdataoff;
            result.NumFile = binary.LittleEndian.Uint32(b[numfileOff..]);
            result.FileOff = numfileOff + 4L;

            const long symRefSize = (long)4L + 4L;

            var numinltreeOff = result.FileOff + symRefSize * result.NumFile;
            result.NumInlTree = binary.LittleEndian.Uint32(b[numinltreeOff..]);
            result.InlTreeOff = numinltreeOff + 4L;

            result.Initialized = true;

            return result;
        }

        private static uint ReadArgs(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            return binary.LittleEndian.Uint32(b);
        }

        private static uint ReadLocals(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            return binary.LittleEndian.Uint32(b[4L..]);
        }

        // return start and end offsets.
        private static (uint, uint) ReadPcsp(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            uint _p0 = default;
            uint _p0 = default;
            ref FuncInfo _p0 = ref _addr__p0.val;

            return (binary.LittleEndian.Uint32(b[8L..]), binary.LittleEndian.Uint32(b[12L..]));
        }

        // return start and end offsets.
        private static (uint, uint) ReadPcfile(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            uint _p0 = default;
            uint _p0 = default;
            ref FuncInfo _p0 = ref _addr__p0.val;

            return (binary.LittleEndian.Uint32(b[12L..]), binary.LittleEndian.Uint32(b[16L..]));
        }

        // return start and end offsets.
        private static (uint, uint) ReadPcline(this ptr<FuncInfo> _addr__p0, slice<byte> b)
        {
            uint _p0 = default;
            uint _p0 = default;
            ref FuncInfo _p0 = ref _addr__p0.val;

            return (binary.LittleEndian.Uint32(b[16L..]), binary.LittleEndian.Uint32(b[20L..]));
        }

        // return start and end offsets.
        private static (uint, uint) ReadPcinline(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint pcdataoffset)
        {
            uint _p0 = default;
            uint _p0 = default;
            ref FuncInfo _p0 = ref _addr__p0.val;

            return (binary.LittleEndian.Uint32(b[20L..]), binary.LittleEndian.Uint32(b[pcdataoffset..]));
        }

        // return start and end offsets.
        private static (uint, uint) ReadPcdata(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint pcdataoffset, uint k)
        {
            uint _p0 = default;
            uint _p0 = default;
            ref FuncInfo _p0 = ref _addr__p0.val;

            return (binary.LittleEndian.Uint32(b[pcdataoffset + 4L * k..]), binary.LittleEndian.Uint32(b[pcdataoffset + 4L + 4L * k..]));
        }

        private static long ReadFuncdataoff(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint funcdataofffoff, uint k)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            return int64(binary.LittleEndian.Uint32(b[funcdataofffoff + 4L * k..]));
        }

        private static SymRef ReadFile(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint filesoff, uint k)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            var p = binary.LittleEndian.Uint32(b[filesoff + 8L * k..]);
            var s = binary.LittleEndian.Uint32(b[filesoff + 4L + 8L * k..]);
            return new SymRef(p,s);
        }

        private static InlTreeNode ReadInlTree(this ptr<FuncInfo> _addr__p0, slice<byte> b, uint inltreeoff, uint k)
        {
            ref FuncInfo _p0 = ref _addr__p0.val;

            const long inlTreeNodeSize = (long)4L * 7L;

            InlTreeNode result = default;
            result.Read(b[inltreeoff + k * inlTreeNodeSize..]);
            return result;
        }

        // InlTreeNode is the serialized form of FileInfo.InlTree.
        public partial struct InlTreeNode
        {
            public int Parent;
            public SymRef File;
            public int Line;
            public SymRef Func;
            public int ParentPC;
        }

        private static void Write(this ptr<InlTreeNode> _addr_inl, ptr<bytes.Buffer> _addr_w)
        {
            ref InlTreeNode inl = ref _addr_inl.val;
            ref bytes.Buffer w = ref _addr_w.val;

            array<byte> b = new array<byte>(4L);
            Action<uint> writeUint32 = x =>
            {
                binary.LittleEndian.PutUint32(b[..], x);
                w.Write(b[..]);
            }
;
            writeUint32(uint32(inl.Parent));
            writeUint32(inl.File.PkgIdx);
            writeUint32(inl.File.SymIdx);
            writeUint32(uint32(inl.Line));
            writeUint32(inl.Func.PkgIdx);
            writeUint32(inl.Func.SymIdx);
            writeUint32(uint32(inl.ParentPC));

        }

        // Read an InlTreeNode from b, return the remaining bytes.
        private static slice<byte> Read(this ptr<InlTreeNode> _addr_inl, slice<byte> b)
        {
            ref InlTreeNode inl = ref _addr_inl.val;

            Func<uint> readUint32 = () =>
            {
                var x = binary.LittleEndian.Uint32(b);
                b = b[4L..];
                return x;
            }
;
            inl.Parent = int32(readUint32());
            inl.File = new SymRef(readUint32(),readUint32());
            inl.Line = int32(readUint32());
            inl.Func = new SymRef(readUint32(),readUint32());
            inl.ParentPC = int32(readUint32());
            return b;

        }
    }
}}}
