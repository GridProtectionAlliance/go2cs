// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package objfile reads Go object files for the Go linker, cmd/link.
//
// This package is similar to cmd/internal/objfile which also reads
// Go object files.
// package objfile -- go2cs converted at 2020 October 08 04:41:34 UTC
// import "cmd/oldlink/internal/objfile" ==> using objfile = go.cmd.oldlink.@internal.objfile_package
// Original source: C:\Go\src\cmd\oldlink\internal\objfile\objfile.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using dwarf = go.cmd.@internal.dwarf_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using fmt = go.fmt_package;
using unsafeheader = go.@internal.unsafeheader_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class objfile_package
    {
        private static readonly @string startmagic = (@string)"\x00go114ld";
        private static readonly @string endmagic = (@string)"\xffgo114ld";


        private static slice<byte> emptyPkg = (slice<byte>)"\"\".";

        // objReader reads Go object files.
        private partial struct objReader
        {
            public ptr<bio.Reader> rd;
            public ptr<sys.Arch> arch;
            public ptr<sym.Symbols> syms;
            public ptr<sym.Library> lib;
            public ptr<sym.CompilationUnit> unit;
            public @string pn;
            public ptr<sym.Symbol> dupSym;
            public long localSymVersion;
            public long flags;
            public long strictDupMsgs;
            public long dataSize; // rdBuf is used by readString and readSymName as scratch for reading strings.
            public slice<byte> rdBuf; // List of symbol references for the file being read.
            public slice<ptr<sym.Symbol>> refs;
            public slice<byte> data;
            public slice<sym.Reloc> reloc;
            public slice<sym.Pcdata> pcdata;
            public slice<ptr<sym.Symbol>> funcdata;
            public slice<long> funcdataoff;
            public slice<ptr<sym.Symbol>> file;
            public @string pkgpref; // objabi.PathToPrefix(r.lib.Pkg) + "."

            public slice<byte> roObject; // from read-only mmap of object file (may be nil)
            public long roOffset; // offset into readonly object data examined so far

            public bool dataReadOnly; // whether data is backed by read-only memory
        }

        // Flags to enable optional behavior during object loading/reading.

        public static readonly long NoFlag = (long)iota; 

        // Sanity-check duplicate symbol contents, issuing warning
        // when duplicates have different lengths or contents.
        public static readonly var StrictDupsWarnFlag = (var)0; 

        // Similar to StrictDupsWarnFlag, but issue fatal error.
        public static readonly var StrictDupsErrFlag = (var)1;


        // Load loads an object file f into library lib.
        // The symbols loaded are added to syms.
        public static long Load(ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, ptr<sym.CompilationUnit> _addr_unit, long length, @string pn, long flags)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;
            ref sym.Library lib = ref _addr_lib.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var start = f.Offset();
            var roObject = f.SliceRO(uint64(length));
            if (roObject != null)
            {
                f.MustSeek(int64(-length), os.SEEK_CUR);
            }

            ptr<objReader> r = addr(new objReader(rd:f,lib:lib,unit:unit,arch:arch,syms:syms,pn:pn,dupSym:&sym.Symbol{Name:".dup"},localSymVersion:syms.IncVersion(),flags:flags,roObject:roObject,pkgpref:objabi.PathToPrefix(lib.Pkg)+".",));
            r.loadObjFile();
            if (roObject != null)
            {
                if (r.roOffset != length)
                {
                    log.Fatalf("%s: unexpected end at %d, want %d", pn, r.roOffset, start + length);
                }

                r.rd.MustSeek(int64(length), os.SEEK_CUR);

            }
            else if (f.Offset() != start + length)
            {
                log.Fatalf("%s: unexpected end at %d, want %d", pn, f.Offset(), start + length);
            }

            return r.strictDupMsgs;

        }

        private static void loadObjFile(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;
 
            // Magic header
            array<byte> buf = new array<byte>(8L);
            r.readFull(buf[..]);
            if (string(buf[..]) != startmagic)
            {
                if (string(buf[..]) == goobj2.Magic)
                {
                    log.Fatalf("found object file %s in new format, but -go115newobj is false\nset -go115newobj consistently in all -gcflags, -asmflags, and -ldflags", r.pn);
                }

                log.Fatalf("%s: invalid file start %x %x %x %x %x %x %x %x", r.pn, buf[0L], buf[1L], buf[2L], buf[3L], buf[4L], buf[5L], buf[6L], buf[7L]);

            } 

            // Version
            var (c, err) = r.readByte();
            if (err != null || c != 1L)
            {
                log.Fatalf("%s: invalid file version number %d", r.pn, c);
            } 

            // Autolib
            while (true)
            {
                var lib = r.readString();
                if (lib == "")
                {
                    break;
                }

                r.lib.ImportStrings = append(r.lib.ImportStrings, lib);

            } 

            // DWARF strings
 

            // DWARF strings
            var count = r.readInt();
            r.unit.DWARFFileTable = make_slice<@string>(count);
            for (long i = 0L; i < count; i++)
            { 
                // TODO: This should probably be a call to mkROString.
                r.unit.DWARFFileTable[i] = r.readString();

            } 

            // Symbol references
 

            // Symbol references
            r.refs = new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] { nil }); // zeroth ref is nil
            while (true)
            {
                (c, err) = r.peek(1L);
                if (err != null)
                {
                    log.Fatalf("%s: peeking: %v", r.pn, err);
                }

                if (c[0L] == 0xffUL)
                {
                    r.readByte();
                    break;
                }

                r.readRef();

            } 

            // Lengths
 

            // Lengths
            r.readSlices(); 

            // Data section
            err = r.readDataSection();
            if (err != null)
            {
                log.Fatalf("%s: error reading %s", r.pn, err);
            } 

            // Defined symbols
            while (true)
            {
                (c, err) = r.peek(1L);
                if (err != null)
                {
                    log.Fatalf("%s: peeking: %v", r.pn, err);
                }

                if (c[0L] == 0xffUL)
                {
                    break;
                }

                r.readSym();

            } 

            // Magic footer
 

            // Magic footer
            buf = new array<byte>(new byte[] {  });
            r.readFull(buf[..]);
            if (string(buf[..]) != endmagic)
            {
                log.Fatalf("%s: invalid file end", r.pn);
            }

        }

        private static void readSlices(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            r.dataSize = r.readInt();
            var n = r.readInt();
            r.reloc = make_slice<sym.Reloc>(n);
            n = r.readInt();
            r.pcdata = make_slice<sym.Pcdata>(n);
            _ = r.readInt(); // TODO: remove on next object file rev (autom count)
            n = r.readInt();
            r.funcdata = make_slice<ptr<sym.Symbol>>(n);
            r.funcdataoff = make_slice<long>(n);
            n = r.readInt();
            r.file = make_slice<ptr<sym.Symbol>>(n);

        }

        private static error readDataSection(this ptr<objReader> _addr_r)
        {
            error err = default!;
            ref objReader r = ref _addr_r.val;

            if (r.roObject != null)
            {
                r.data = r.roObject[r.roOffset..r.roOffset + int64(r.dataSize)];
                r.dataReadOnly = true;
                err = null;
                r.roOffset += int64(r.dataSize);
                return ;

            }

            r.data, r.dataReadOnly, err = r.rd.Slice(uint64(r.dataSize));
            return ;

        }

        // Symbols are prefixed so their content doesn't get confused with the magic footer.
        private static readonly ulong symPrefix = (ulong)0xfeUL;



        private static void readSym(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            byte c = default;
            error err = default!;
            c, err = r.readByte();

            if (c != symPrefix || err != null)
            {
                log.Fatalln("readSym out of sync");
            }

            c, err = r.readByte();

            if (err != null)
            {
                log.Fatalln("error reading input: ", err);
            }

            var t = sym.AbiSymKindToSymKind[c];
            var s = r.readSymIndex();
            var flags = r.readInt();
            var dupok = flags & 1L != 0L;
            var local = flags & 2L != 0L;
            var makeTypelink = flags & 4L != 0L;
            var size = r.readInt();
            var typ = r.readSymIndex();
            var data = r.readData();
            var nreloc = r.readInt();
            var isdup = false;

            ptr<sym.Symbol> dup;
            if (s.Type != 0L && s.Type != sym.SXREF)
            {
                if ((t == sym.SDATA || t == sym.SBSS || t == sym.SNOPTRBSS) && len(data) == 0L && nreloc == 0L)
                {
                    if (s.Size < int64(size))
                    {
                        s.Size = int64(size);
                    }

                    if (typ != null && s.Gotype == null)
                    {
                        s.Gotype = typ;
                    }

                    return ;

                }

                if ((s.Type == sym.SDATA || s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS) && len(s.P) == 0L && len(s.R) == 0L)
                {
                    goto overwrite;
                }

                if (s.Type != sym.SBSS && s.Type != sym.SNOPTRBSS && !dupok && !s.Attr.DuplicateOK())
                {
                    log.Fatalf("duplicate symbol %s (types %d and %d) in %s and %s", s.Name, s.Type, t, s.File, r.pn);
                }

                if (len(s.P) > 0L)
                {
                    dup = s;
                    s = r.dupSym;
                    isdup = true;
                }

            }

overwrite:
            s.File = r.pkgpref[..len(r.pkgpref) - 1L];
            s.Unit = r.unit;
            if (dupok)
            {
                s.Attr |= sym.AttrDuplicateOK;
            }

            if (t == sym.SXREF)
            {
                log.Fatalf("bad sxref");
            }

            if (t == 0L)
            {
                log.Fatalf("missing type for %s in %s", s.Name, r.pn);
            }

            if (t == sym.SBSS && (s.Type == sym.SRODATA || s.Type == sym.SNOPTRBSS))
            {
                t = s.Type;
            }

            s.Type = t;
            if (s.Size < int64(size))
            {
                s.Size = int64(size);
            }

            s.Attr.Set(sym.AttrLocal, local);
            s.Attr.Set(sym.AttrMakeTypelink, makeTypelink);
            if (typ != null)
            {
                s.Gotype = typ;
            }

            if (isdup && typ != null)
            { // if bss sym defined multiple times, take type from any one def
                dup.Gotype = typ;

            }

            s.P = data;
            s.Attr.Set(sym.AttrReadOnly, r.dataReadOnly);
            if (nreloc > 0L)
            {
                s.R = r.reloc.slice(-1, nreloc, nreloc);
                if (!isdup)
                {
                    r.reloc = r.reloc[nreloc..];
                }

                {
                    long i__prev1 = i;

                    for (long i = 0L; i < nreloc; i++)
                    {
                        s.R[i] = new sym.Reloc(Off:r.readInt32(),Siz:r.readUint8(),Type:objabi.RelocType(r.readInt32()),Add:r.readInt64(),Sym:r.readSymIndex(),);
                    }


                    i = i__prev1;
                }

            }

            if (s.Type == sym.STEXT)
            {
                s.FuncInfo = @new<sym.FuncInfo>();
                var pc = s.FuncInfo;

                pc.Args = r.readInt32();
                pc.Locals = r.readInt32();
                s.Align = r.readInt32();
                if (r.readUint8() != 0L)
                {
                    s.Attr |= sym.AttrNoSplit;
                }

                flags = r.readInt();
                if (flags & (1L << (int)(2L)) != 0L)
                {
                    s.Attr |= sym.AttrReflectMethod;
                }

                if (flags & (1L << (int)(3L)) != 0L)
                {
                    s.Attr |= sym.AttrShared;
                }

                if (flags & (1L << (int)(4L)) != 0L)
                {
                    s.Attr |= sym.AttrTopFrame;
                }

                var n = r.readInt();
                if (n != 0L)
                {
                    log.Fatalf("stale object file: autom count nonzero");
                }

                pc.Pcsp.P = r.readData();
                pc.Pcfile.P = r.readData();
                pc.Pcline.P = r.readData();
                pc.Pcinline.P = r.readData();
                n = r.readInt();
                pc.Pcdata = r.pcdata.slice(-1, n, n);
                if (!isdup)
                {
                    r.pcdata = r.pcdata[n..];
                }

                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        pc.Pcdata[i].P = r.readData();
                    }


                    i = i__prev1;
                }
                n = r.readInt();
                pc.Funcdata = r.funcdata.slice(-1, n, n);
                pc.Funcdataoff = r.funcdataoff.slice(-1, n, n);
                if (!isdup)
                {
                    r.funcdata = r.funcdata[n..];
                    r.funcdataoff = r.funcdataoff[n..];
                }

                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        pc.Funcdata[i] = r.readSymIndex();
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        pc.Funcdataoff[i] = r.readInt64();
                    }


                    i = i__prev1;
                }
                n = r.readInt();
                pc.File = r.file.slice(-1, n, n);
                if (!isdup)
                {
                    r.file = r.file[n..];
                }

                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        pc.File[i] = r.readSymIndex();
                    }


                    i = i__prev1;
                }
                n = r.readInt();
                pc.InlTree = make_slice<sym.InlinedCall>(n);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        pc.InlTree[i].Parent = r.readInt32();
                        pc.InlTree[i].File = r.readSymIndex();
                        pc.InlTree[i].Line = r.readInt32();
                        pc.InlTree[i].Func = r.readSymIndex().Name;
                        pc.InlTree[i].ParentPC = r.readInt32();
                    }


                    i = i__prev1;
                }

                if (!dupok)
                {
                    if (s.Attr.OnList())
                    {
                        log.Fatalf("symbol %s listed multiple times", s.Name);
                    }

                    s.Attr |= sym.AttrOnList;
                    r.lib.Textp = append(r.lib.Textp, s);

                }
                else
                { 
                    // there may ba a dup in another package
                    // put into a temp list and add to text later
                    if (!isdup)
                    {
                        r.lib.DupTextSyms = append(r.lib.DupTextSyms, s);
                    }
                    else
                    {
                        r.lib.DupTextSyms = append(r.lib.DupTextSyms, dup);
                    }

                }

            }

            if (s.Type == sym.SDWARFINFO)
            {
                r.patchDWARFName(s);
            }

            if (isdup && r.flags & (StrictDupsWarnFlag | StrictDupsErrFlag) != 0L)
            { 
                // Compare the just-read symbol with the previously read
                // symbol of the same name, verifying that they have the same
                // payload. If not, issue a warning and possibly an error.
                if (!bytes.Equal(s.P, dup.P))
                {
                    @string reason = "same length but different contents";
                    if (len(s.P) != len(dup.P))
                    {
                        reason = fmt.Sprintf("new length %d != old length %d", len(data), len(dup.P));
                    }

                    fmt.Fprintf(os.Stderr, "cmd/link: while reading object for '%v': duplicate symbol '%s', previous def at '%v', with mismatched payload: %s\n", r.lib, dup, dup.Unit.Lib, reason); 

                    // For the moment, allow DWARF subprogram DIEs for
                    // auto-generated wrapper functions. What seems to happen
                    // here is that we get different line numbers on formal
                    // params; I am guessing that the pos is being inherited
                    // from the spot where the wrapper is needed.
                    var allowed = (strings.HasPrefix(dup.Name, "go.info.go.interface") || strings.HasPrefix(dup.Name, "go.info.go.builtin") || strings.HasPrefix(dup.Name, "go.isstmt.go.builtin") || strings.HasPrefix(dup.Name, "go.debuglines"));
                    if (!allowed)
                    {
                        r.strictDupMsgs++;
                    }

                }

            }

        }

        private static void patchDWARFName(this ptr<objReader> _addr_r, ptr<sym.Symbol> _addr_s)
        {
            ref objReader r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;
 
            // This is kind of ugly. Really the package name should not
            // even be included here.
            if (s.Size < 1L || s.P[0L] != dwarf.DW_ABRV_FUNCTION)
            {
                return ;
            }

            var e = bytes.IndexByte(s.P, 0L);
            if (e == -1L)
            {
                return ;
            }

            var p = bytes.Index(s.P[..e], emptyPkg);
            if (p == -1L)
            {
                return ;
            }

            slice<byte> pkgprefix = (slice<byte>)r.pkgpref;
            var patched = bytes.Replace(s.P[..e], emptyPkg, pkgprefix, -1L);

            s.P = append(patched, s.P[e..]);
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

        private static void readFull(this ptr<objReader> _addr_r, slice<byte> b)
        {
            ref objReader r = ref _addr_r.val;

            if (r.roObject != null)
            {
                copy(b, r.roObject[r.roOffset..]);
                r.roOffset += int64(len(b));
                return ;
            }

            var (_, err) = io.ReadFull(r.rd, b);
            if (err != null)
            {
                log.Fatalf("%s: error reading %s", r.pn, err);
            }

        }

        private static (byte, error) readByte(this ptr<objReader> _addr_r)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref objReader r = ref _addr_r.val;

            if (r.roObject != null)
            {
                var b = r.roObject[r.roOffset];
                r.roOffset++;
                return (b, error.As(null!)!);
            }

            return r.rd.ReadByte();

        }

        private static (slice<byte>, error) peek(this ptr<objReader> _addr_r, long n)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref objReader r = ref _addr_r.val;

            if (r.roObject != null)
            {
                return (r.roObject[r.roOffset..r.roOffset + int64(n)], error.As(null!)!);
            }

            return r.rd.Peek(n);

        }

        private static void readRef(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            {
                var (c, err) = r.readByte();

                if (c != symPrefix || err != null)
                {
                    log.Fatalf("readSym out of sync");
                }

            }

            var name = r.readSymName();
            long v = default;
            {
                var abi = r.readInt();

                if (abi == -1L)
                { 
                    // Static
                    v = r.localSymVersion;

                }                {
                    var abiver = sym.ABIToVersion(obj.ABI(abi));


                    else if (abiver != -1L)
                    { 
                        // Note that data symbols are "ABI0", which maps to version 0.
                        v = abiver;

                    }
                    else
                    {
                        log.Fatalf("invalid symbol ABI for %q: %d", name, abi);
                    }

                }


            }

            var s = r.syms.Lookup(name, v);
            r.refs = append(r.refs, s);

            if (s == null || v == r.localSymVersion)
            {
                return ;
            }

            if (s.Name[0L] == '$' && len(s.Name) > 5L && s.Type == 0L && len(s.P) == 0L)
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

                        s.AddUint32(r.arch, uint32(x));
                        break;
                    case "$f64.": 

                    case "$i64.": 
                        s.AddUint64(r.arch, x);
                        break;
                    default: 
                        log.Panicf("unrecognized $-symbol: %s", s.Name);
                        break;
                }
                s.Attr.Set(sym.AttrReachable, false);

            }

            if (strings.HasPrefix(s.Name, "runtime.gcbits."))
            {
                s.Attr |= sym.AttrLocal;
            }

        }

        private static long readInt64(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var uv = uint64(0L);
            {
                var shift = uint(0L);

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    if (shift >= 64L)
                    {
                        log.Fatalf("corrupt input");
                    shift += 7L;
                    }

                    var (c, err) = r.readByte();
                    if (err != null)
                    {
                        log.Fatalln("error reading input: ", err);
                    }

                    uv |= uint64(c & 0x7FUL) << (int)(shift);
                    if (c & 0x80UL == 0L)
                    {
                        break;
                    }

                }

            }

            return int64(uv >> (int)(1L)) ^ (int64(uv << (int)(63L)) >> (int)(63L));

        }

        private static long readInt(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt64();
            if (int64(int(n)) != n)
            {
                log.Panicf("%v out of range for int", n);
            }

            return int(n);

        }

        private static int readInt32(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt64();
            if (int64(int32(n)) != n)
            {
                log.Panicf("%v out of range for int32", n);
            }

            return int32(n);

        }

        private static short readInt16(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt64();
            if (int64(int16(n)) != n)
            {
                log.Panicf("%v out of range for int16", n);
            }

            return int16(n);

        }

        private static byte readUint8(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt64();
            if (int64(uint8(n)) != n)
            {
                log.Panicf("%v out of range for uint8", n);
            }

            return uint8(n);

        }

        private static @string readString(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt();
            if (cap(r.rdBuf) < n)
            {
                r.rdBuf = make_slice<byte>(2L * n);
            }

            r.readFull(r.rdBuf[..n]);
            return string(r.rdBuf[..n]);

        }

        private static slice<byte> readData(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt();
            var p = r.data.slice(-1, n, n);
            r.data = r.data[n..];
            return p;
        }

        private static @string mkROString(slice<byte> rodata)
        {
            if (len(rodata) == 0L)
            {
                return "";
            }

            ref @string s = ref heap(out ptr<@string> _addr_s);
            var hdr = (unsafeheader.String.val)(@unsafe.Pointer(_addr_s));
            hdr.Data = @unsafe.Pointer(_addr_rodata[0L]);
            hdr.Len = len(rodata);

            return s;

        }

        // readSymName reads a symbol name, replacing all "". with pkg.
        private static @string readSymName(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt();
            if (n == 0L)
            {
                r.readInt64();
                return "";
            }

            if (cap(r.rdBuf) < n)
            {
                r.rdBuf = make_slice<byte>(2L * n);
            }

            var sOffset = r.roOffset;
            var (origName, err) = r.peek(n);
            if (err == bufio.ErrBufferFull)
            { 
                // Long symbol names are rare but exist. One source is type
                // symbols for types with long string forms. See #15104.
                origName = make_slice<byte>(n);
                r.readFull(origName);

            }
            else if (err != null)
            {
                log.Fatalf("%s: error reading symbol: %v", r.pn, err);
            }

            var adjName = r.rdBuf[..0L];
            long nPkgRefs = 0L;
            while (true)
            {
                var i = bytes.Index(origName, emptyPkg);
                if (i == -1L)
                {
                    @string s = default;
                    if (r.roObject != null && nPkgRefs == 0L)
                    {
                        s = mkROString(r.roObject[sOffset..sOffset + int64(n)]);
                    }
                    else
                    {
                        s = string(append(adjName, origName));
                    } 
                    // Read past the peeked origName, now that we're done with it,
                    // using the rfBuf (also no longer used) as the scratch space.
                    // TODO: use bufio.Reader.Discard if available instead?
                    if (err == null)
                    {
                        r.readFull(r.rdBuf[..n]);
                    }

                    r.rdBuf = adjName[..0L]; // in case 2*n wasn't enough
                    return s;

                }

                nPkgRefs++;
                adjName = append(adjName, origName[..i]);
                adjName = append(adjName, r.pkgpref[..len(r.pkgpref) - 1L]);
                adjName = append(adjName, '.');
                origName = origName[i + len(emptyPkg)..];

            }


        }

        // Reads the index of a symbol reference and resolves it to a symbol
        private static ptr<sym.Symbol> readSymIndex(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var i = r.readInt();
            return _addr_r.refs[i]!;
        }
    }
}}}}
