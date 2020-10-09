// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package goobj implements reading of Go object files and archives.
//
// TODO(rsc): Decide where this package should live. (golang.org/issue/6932)
// TODO(rsc): Decide the appropriate integer types for various fields.
// package goobj -- go2cs converted at 2020 October 09 05:08:46 UTC
// import "cmd/internal/goobj" ==> using goobj = go.cmd.@internal.goobj_package
// Original source: C:\Go\src\cmd\internal\goobj\read.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using objabi = go.cmd.@internal.objabi_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class goobj_package
    {
        // A Sym is a named symbol in an object file.
        public partial struct Sym
        {
            public ref SymID SymID => ref SymID_val; // symbol identifier (name and version)
            public objabi.SymKind Kind; // kind of symbol
            public bool DupOK; // are duplicate definitions okay?
            public long Size; // size of corresponding data
            public SymID Type; // symbol for Go type information
            public Data Data; // memory image of symbol
            public slice<Reloc> Reloc; // relocations to apply to Data
            public ptr<Func> Func; // additional data for functions
        }

        // A SymID - the combination of Name and Version - uniquely identifies
        // a symbol within a package.
        public partial struct SymID
        {
            public @string Name; // Version is zero for symbols with global visibility.
// Symbols with only file visibility (such as file-level static
// declarations in C) have a non-zero version distinguishing
// a symbol in one file from a symbol of the same name
// in another file
            public long Version;
        }

        public static @string String(this SymID s)
        {
            if (s.Version == 0L)
            {
                return s.Name;
            }

            return fmt.Sprintf("%s<%d>", s.Name, s.Version);

        }

        // A Data is a reference to data stored in an object file.
        // It records the offset and size of the data, so that a client can
        // read the data only if necessary.
        public partial struct Data
        {
            public long Offset;
            public long Size;
        }

        // A Reloc describes a relocation applied to a memory image to refer
        // to an address within a particular symbol.
        public partial struct Reloc
        {
            public long Offset;
            public long Size;
            public SymID Sym;
            public long Add; // The Type records the form of address expected in the bytes
// described by the previous fields: absolute, PC-relative, and so on.
// TODO(rsc): The interpretation of Type is not exposed by this package.
            public objabi.RelocType Type;
        }

        // A Var describes a variable in a function stack frame: a declared
        // local variable, an input argument, or an output result.
        public partial struct Var
        {
            public @string Name; // Name of variable.
            public long Kind; // TODO(rsc): Define meaning.
            public long Offset; // Frame offset. TODO(rsc): Define meaning.

            public SymID Type; // Go type for variable.
        }

        // Func contains additional per-symbol information specific to functions.
        public partial struct Func
        {
            public long Args; // size in bytes of argument frame: inputs and outputs
            public long Frame; // size in bytes of local variable frame
            public uint Align; // alignment requirement in bytes for the address of the function
            public bool Leaf; // function omits save of link register (ARM)
            public bool NoSplit; // function omits stack split prologue
            public bool TopFrame; // function is the top of the call stack
            public slice<Var> Var; // detail about local variables
            public Data PCSP; // PC → SP offset map
            public Data PCFile; // PC → file number map (index into File)
            public Data PCLine; // PC → line number map
            public Data PCInline; // PC → inline tree index map
            public slice<Data> PCData; // PC → runtime support data map
            public slice<FuncData> FuncData; // non-PC-specific runtime support data
            public slice<@string> File; // paths indexed by PCFile
            public slice<InlinedCall> InlTree;
        }

        // TODO: Add PCData []byte and PCDataIter (similar to liblink).

        // A FuncData is a single function-specific data value.
        public partial struct FuncData
        {
            public SymID Sym; // symbol holding data
            public long Offset; // offset into symbol for funcdata pointer
        }

        // An InlinedCall is a node in an InlTree.
        // See cmd/internal/obj.InlTree for details.
        public partial struct InlinedCall
        {
            public long Parent;
            public @string File;
            public long Line;
            public SymID Func;
            public long ParentPC;
        }

        // A Package is a parsed Go object file or archive defining a Go package.
        public partial struct Package
        {
            public @string ImportPath; // import path denoting this package
            public slice<@string> Imports; // packages imported by this package
            public slice<SymID> SymRefs; // list of symbol names and versions referred to by this pack
            public slice<ptr<Sym>> Syms; // symbols defined by this package
            public long MaxVersion; // maximum Version in any SymID in Syms
            public @string Arch; // architecture
            public slice<ptr<NativeReader>> Native; // native object data (e.g. ELF)
            public slice<@string> DWARFFileList; // List of files for the DWARF .debug_lines section
        }

        public partial struct NativeReader : io.ReaderAt
        {
            public @string Name;
            public ref io.ReaderAt ReaderAt => ref ReaderAt_val;
        }

        private static slice<byte> archiveHeader = (slice<byte>)"!<arch>\n";        private static slice<byte> archiveMagic = (slice<byte>)"`\n";        private static slice<byte> goobjHeader = (slice<byte>)"go objec";        private static var errCorruptArchive = errors.New("corrupt archive");        private static var errTruncatedArchive = errors.New("truncated archive");        private static var errCorruptObject = errors.New("corrupt object file");        private static var errNotObject = errors.New("unrecognized object file format");

        // An objReader is an object file reader.
        private partial struct objReader
        {
            public ptr<Package> p;
            public ptr<bufio.Reader> b;
            public ptr<os.File> f;
            public error err;
            public long offset;
            public long dataOffset;
            public long limit;
            public array<byte> tmp;
            public @string pkgprefix;
        }

        // init initializes r to read package p from f.
        private static void init(this ptr<objReader> _addr_r, ptr<os.File> _addr_f, ptr<Package> _addr_p)
        {
            ref objReader r = ref _addr_r.val;
            ref os.File f = ref _addr_f.val;
            ref Package p = ref _addr_p.val;

            r.f = f;
            r.p = p;
            r.offset, _ = f.Seek(0L, io.SeekCurrent);
            r.limit, _ = f.Seek(0L, io.SeekEnd);
            f.Seek(r.offset, io.SeekStart);
            r.b = bufio.NewReader(f);
            r.pkgprefix = objabi.PathToPrefix(p.ImportPath) + ".";
        }

        // error records that an error occurred.
        // It returns only the first error, so that an error
        // caused by an earlier error does not discard information
        // about the earlier error.
        private static error error(this ptr<objReader> _addr_r, error err)
        {
            ref objReader r = ref _addr_r.val;

            if (r.err == null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                r.err = err;

            } 
            // panic("corrupt") // useful for debugging
            return error.As(r.err)!;

        }

        // peek returns the next n bytes without advancing the reader.
        private static (slice<byte>, error) peek(this ptr<objReader> _addr_r, long n)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref objReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return (null, error.As(r.err)!);
            }

            if (r.offset >= r.limit)
            {
                r.error(io.ErrUnexpectedEOF);
                return (null, error.As(r.err)!);
            }

            var (b, err) = r.b.Peek(n);
            if (err != null)
            {
                if (err != bufio.ErrBufferFull)
                {
                    r.error(err);
                }

            }

            return (b, error.As(err)!);

        }

        // readByte reads and returns a byte from the input file.
        // On I/O error or EOF, it records the error but returns byte 0.
        // A sequence of 0 bytes will eventually terminate any
        // parsing state in the object file. In particular, it ends the
        // reading of a varint.
        private static byte readByte(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return 0L;
            }

            if (r.offset >= r.limit)
            {
                r.error(io.ErrUnexpectedEOF);
                return 0L;
            }

            var (b, err) = r.b.ReadByte();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                r.error(err);
                b = 0L;

            }
            else
            {
                r.offset++;
            }

            return b;

        }

        // read reads exactly len(b) bytes from the input file.
        // If an error occurs, read returns the error but also
        // records it, so it is safe for callers to ignore the result
        // as long as delaying the report is not a problem.
        private static error readFull(this ptr<objReader> _addr_r, slice<byte> b)
        {
            ref objReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return error.As(r.err)!;
            }

            if (r.offset + int64(len(b)) > r.limit)
            {
                return error.As(r.error(io.ErrUnexpectedEOF))!;
            }

            var (n, err) = io.ReadFull(r.b, b);
            r.offset += int64(n);
            if (err != null)
            {
                return error.As(r.error(err))!;
            }

            return error.As(null!)!;

        }

        // readInt reads a zigzag varint from the input file.
        private static long readInt(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            ulong u = default;

            {
                var shift = uint(0L);

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    if (shift >= 64L)
                    {
                        r.error(errCorruptObject);
                        return 0L;
                    shift += 7L;
                    }

                    var c = r.readByte();
                    u |= uint64(c & 0x7FUL) << (int)(shift);
                    if (c & 0x80UL == 0L)
                    {
                        break;
                    }

                }

            }

            return int64(u >> (int)(1L)) ^ (int64(u) << (int)(63L) >> (int)(63L));

        }

        // readString reads a length-delimited string from the input file.
        private static @string readString(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt();
            var buf = make_slice<byte>(n);
            r.readFull(buf);
            return string(buf);
        }

        // readSymID reads a SymID from the input file.
        private static SymID readSymID(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var i = r.readInt();
            return r.p.SymRefs[i];
        }

        private static void readRef(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var name = r.readString();
            var abiOrStatic = r.readInt(); 

            // In a symbol name in an object file, "". denotes the
            // prefix for the package in which the object file has been found.
            // Expand it.
            name = strings.ReplaceAll(name, "\"\".", r.pkgprefix); 

            // The ABI field records either the ABI or -1 for static symbols.
            //
            // To distinguish different static symbols with the same name,
            // we use the symbol "version". Version 0 corresponds to
            // global symbols, and each file has a unique version > 0 for
            // all of its static symbols. The version is incremented on
            // each call to parseObject.
            //
            // For global symbols, we currently ignore the ABI.
            //
            // TODO(austin): Record the ABI in SymID. Since this is a
            // public API, we'll have to keep Version as 0 and record the
            // ABI in a new field (which differs from how the linker does
            // this, but that's okay). Show the ABI in things like
            // objdump.
            long vers = default;
            if (abiOrStatic == -1L)
            { 
                // Static symbol
                vers = r.p.MaxVersion;

            }

            r.p.SymRefs = append(r.p.SymRefs, new SymID(name,vers));

        }

        // readData reads a data reference from the input file.
        private static Data readData(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            var n = r.readInt();
            Data d = new Data(Offset:r.dataOffset,Size:n);
            r.dataOffset += n;
            return d;
        }

        // skip skips n bytes in the input.
        private static void skip(this ptr<objReader> _addr_r, long n)
        {
            ref objReader r = ref _addr_r.val;

            if (n < 0L)
            {
                r.error(fmt.Errorf("debug/goobj: internal error: misuse of skip"));
            }

            if (n < int64(len(r.tmp)))
            { 
                // Since the data is so small, a just reading from the buffered
                // reader is better than flushing the buffer and seeking.
                r.readFull(r.tmp[..n]);

            }
            else if (n <= int64(r.b.Buffered()))
            { 
                // Even though the data is not small, it has already been read.
                // Advance the buffer instead of seeking.
                while (n > int64(len(r.tmp)))
                {
                    r.readFull(r.tmp[..]);
                    n -= int64(len(r.tmp));
                }
            else

                r.readFull(r.tmp[..n]);

            }            { 
                // Seek, giving up buffered data.
                var (_, err) = r.f.Seek(r.offset + n, io.SeekStart);
                if (err != null)
                {
                    r.error(err);
                }

                r.offset += n;
                r.b.Reset(r.f);

            }

        }

        // Parse parses an object file or archive from f,
        // assuming that its import path is pkgpath.
        public static (ptr<Package>, error) Parse(ptr<os.File> _addr_f, @string pkgpath)
        {
            ptr<Package> _p0 = default!;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            if (pkgpath == "")
            {
                pkgpath = "\"\"";
            }

            ptr<Package> p = @new<Package>();
            p.ImportPath = pkgpath;

            objReader rd = default;
            rd.init(f, p);
            var err = rd.readFull(rd.tmp[..8L]);
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                return (_addr_null!, error.As(err)!);

            }


            if (bytes.Equal(rd.tmp[..8L], archiveHeader)) 
                {
                    var err__prev1 = err;

                    err = rd.parseArchive();

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

            else if (bytes.Equal(rd.tmp[..8L], goobjHeader)) 
                {
                    var err__prev1 = err;

                    err = rd.parseObject(goobjHeader);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

            else 
                return (_addr_null!, error.As(errNotObject)!);
                        return (_addr_p!, error.As(null!)!);

        }

        // trimSpace removes trailing spaces from b and returns the corresponding string.
        // This effectively parses the form used in archive headers.
        private static @string trimSpace(slice<byte> b)
        {
            return string(bytes.TrimRight(b, " "));
        }

        // parseArchive parses a Unix archive of Go object files.
        private static error parseArchive(this ptr<objReader> _addr_r)
        {
            ref objReader r = ref _addr_r.val;

            while (r.offset < r.limit)
            {
                {
                    var err__prev1 = err;

                    var err = r.readFull(r.tmp[..60L]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }

                var data = r.tmp[..60L]; 

                // Each file is preceded by this text header (slice indices in first column):
                //     0:16    name
                //    16:28 date
                //    28:34 uid
                //    34:40 gid
                //    40:48 mode
                //    48:58 size
                //    58:60 magic - `\n
                // We only care about name, size, and magic.
                // The fields are space-padded on the right.
                // The size is in decimal.
                // The file data - size bytes - follows the header.
                // Headers are 2-byte aligned, so if size is odd, an extra padding
                // byte sits between the file data and the next header.
                // The file data that follows is padded to an even number of bytes:
                // if size is odd, an extra padding byte is inserted betw the next header.
                if (len(data) < 60L)
                {
                    return error.As(errTruncatedArchive)!;
                }

                if (!bytes.Equal(data[58L..60L], archiveMagic))
                {
                    return error.As(errCorruptArchive)!;
                }

                var name = trimSpace(data[0L..16L]);
                var (size, err) = strconv.ParseInt(trimSpace(data[48L..58L]), 10L, 64L);
                if (err != null)
                {
                    return error.As(errCorruptArchive)!;
                }

                data = data[60L..];
                var fsize = size + size & 1L;
                if (fsize < 0L || fsize < size)
                {
                    return error.As(errCorruptArchive)!;
                }

                switch (name)
                {
                    case "__.PKGDEF": 
                        r.skip(size);
                        break;
                    default: 
                        var oldLimit = r.limit;
                        r.limit = r.offset + size;

                        var (p, err) = r.peek(8L);
                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        if (bytes.Equal(p, goobjHeader))
                        {
                            {
                                var err__prev2 = err;

                                err = r.parseObject(null);

                                if (err != null)
                                {
                                    return error.As(fmt.Errorf("parsing archive member %q: %v", name, err))!;
                                }

                                err = err__prev2;

                            }

                        }
                        else
                        {
                            r.p.Native = append(r.p.Native, addr(new NativeReader(Name:name,ReaderAt:io.NewSectionReader(r.f,r.offset,size),)));
                        }

                        r.skip(r.limit - r.offset);
                        r.limit = oldLimit;
                        break;
                }
                if (size & 1L != 0L)
                {
                    r.skip(1L);
                }

            }

            return error.As(null!)!;

        }

        // parseObject parses a single Go object file.
        // The prefix is the bytes already read from the file,
        // typically in order to detect that this is an object file.
        // The object file consists of a textual header ending in "\n!\n"
        // and then the part we want to parse begins.
        // The format of that part is defined in a comment at the top
        // of src/liblink/objfile.c.
        private static error parseObject(this ptr<objReader> _addr_r, slice<byte> prefix)
        {
            ref objReader r = ref _addr_r.val;

            r.p.MaxVersion++;
            var h = make_slice<byte>(0L, 256L);
            h = append(h, prefix);
            byte c1 = default;            byte c2 = default;            byte c3 = default;

            while (true)
            {
                c1 = c2;
                c2 = c3;
                c3 = r.readByte();
                h = append(h, c3); 
                // The new export format can contain 0 bytes.
                // Don't consider them errors, only look for r.err != nil.
                if (r.err != null)
                {
                    return error.As(errCorruptObject)!;
                }

                if (c1 == '\n' && c2 == '!' && c3 == '\n')
                {
                    break;
                }

            }


            var hs = strings.Fields(string(h));
            if (len(hs) >= 4L)
            {
                r.p.Arch = hs[3L];
            } 
            // TODO: extract OS + build ID if/when we need it
            var (p, err) = r.peek(8L);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (bytes.Equal(p, (slice<byte>)goobj2.Magic))
            {
                r.readNew();
                return error.As(null!)!;
            }

            r.readFull(r.tmp[..8L]);
            if (!bytes.Equal(r.tmp[..8L], (slice<byte>)"\x00go114ld"))
            {
                return error.As(r.error(errCorruptObject))!;
            }

            var b = r.readByte();
            if (b != 1L)
            {
                return error.As(r.error(errCorruptObject))!;
            } 

            // Direct package dependencies.
            while (true)
            {
                var s = r.readString();
                if (s == "")
                {
                    break;
                }

                r.p.Imports = append(r.p.Imports, s);

            } 

            // Read filenames for dwarf info.
 

            // Read filenames for dwarf info.
            var count = r.readInt();
            {
                var i__prev1 = i;

                for (var i = int64(0L); i < count; i++)
                {
                    r.p.DWARFFileList = append(r.p.DWARFFileList, r.readString());
                }


                i = i__prev1;
            }

            r.p.SymRefs = new slice<SymID>(new SymID[] { {"",0} });
            while (true)
            {
                {
                    var b__prev1 = b;

                    b = r.readByte();

                    if (b != 0xfeUL)
                    {
                        if (b != 0xffUL)
                        {
                            return error.As(r.error(errCorruptObject))!;
                        }

                        break;

                    }

                    b = b__prev1;

                }


                r.readRef();

            }


            var dataLength = r.readInt();
            r.readInt(); // n relocations - ignore
            r.readInt(); // n pcdata - ignore
            r.readInt(); // n autom - ignore
            r.readInt(); // n funcdata - ignore
            r.readInt(); // n files - ignore

            r.dataOffset = r.offset;
            r.skip(dataLength); 

            // Symbols.
            while (true)
            {
                {
                    var b__prev1 = b;

                    b = r.readByte();

                    if (b != 0xfeUL)
                    {
                        if (b != 0xffUL)
                        {
                            return error.As(r.error(errCorruptObject))!;
                        }

                        break;

                    }

                    b = b__prev1;

                }


                var typ = r.readByte();
                s = addr(new Sym(SymID:r.readSymID()));
                r.p.Syms = append(r.p.Syms, s);
                s.Kind = objabi.SymKind(typ);
                var flags = r.readInt();
                s.DupOK = flags & 1L != 0L;
                s.Size = r.readInt();
                s.Type = r.readSymID();
                s.Data = r.readData();
                s.Reloc = make_slice<Reloc>(r.readInt());
                {
                    var i__prev2 = i;

                    foreach (var (__i) in s.Reloc)
                    {
                        i = __i;
                        var rel = _addr_s.Reloc[i];
                        rel.Offset = r.readInt();
                        rel.Size = r.readInt();
                        rel.Type = objabi.RelocType(r.readInt());
                        rel.Add = r.readInt();
                        rel.Sym = r.readSymID();
                    }

                    i = i__prev2;
                }

                if (s.Kind == objabi.STEXT)
                {
                    ptr<Func> f = @new<Func>();
                    s.Func = f;
                    f.Args = r.readInt();
                    f.Frame = r.readInt();
                    f.Align = uint32(r.readInt());
                    flags = r.readInt();
                    f.Leaf = flags & (1L << (int)(0L)) != 0L;
                    f.TopFrame = flags & (1L << (int)(4L)) != 0L;
                    f.NoSplit = r.readInt() != 0L;
                    f.Var = make_slice<Var>(r.readInt());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.Var)
                        {
                            i = __i;
                            var v = _addr_f.Var[i];
                            v.Name = r.readSymID().Name;
                            v.Offset = r.readInt();
                            v.Kind = r.readInt();
                            v.Type = r.readSymID();
                        }

                        i = i__prev2;
                    }

                    f.PCSP = r.readData();
                    f.PCFile = r.readData();
                    f.PCLine = r.readData();
                    f.PCInline = r.readData();
                    f.PCData = make_slice<Data>(r.readInt());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.PCData)
                        {
                            i = __i;
                            f.PCData[i] = r.readData();
                        }

                        i = i__prev2;
                    }

                    f.FuncData = make_slice<FuncData>(r.readInt());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.FuncData)
                        {
                            i = __i;
                            f.FuncData[i].Sym = r.readSymID();
                        }

                        i = i__prev2;
                    }

                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.FuncData)
                        {
                            i = __i;
                            f.FuncData[i].Offset = r.readInt(); // TODO
                        }

                        i = i__prev2;
                    }

                    f.File = make_slice<@string>(r.readInt());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.File)
                        {
                            i = __i;
                            f.File[i] = r.readSymID().Name;
                        }

                        i = i__prev2;
                    }

                    f.InlTree = make_slice<InlinedCall>(r.readInt());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in f.InlTree)
                        {
                            i = __i;
                            f.InlTree[i].Parent = r.readInt();
                            f.InlTree[i].File = r.readSymID().Name;
                            f.InlTree[i].Line = r.readInt();
                            f.InlTree[i].Func = r.readSymID();
                            f.InlTree[i].ParentPC = r.readInt();
                        }

                        i = i__prev2;
                    }
                }

            }


            r.readFull(r.tmp[..7L]);
            if (!bytes.Equal(r.tmp[..7L], (slice<byte>)"go114ld"))
            {
                return error.As(r.error(errCorruptObject))!;
            }

            return error.As(null!)!;

        }

        private static @string String(this ptr<Reloc> _addr_r, ulong insnOffset)
        {
            ref Reloc r = ref _addr_r.val;

            var delta = r.Offset - int64(insnOffset);
            var s = fmt.Sprintf("[%d:%d]%s", delta, delta + r.Size, r.Type);
            if (r.Sym.Name != "")
            {
                if (r.Add != 0L)
                {
                    return fmt.Sprintf("%s:%s+%d", s, r.Sym.Name, r.Add);
                }

                return fmt.Sprintf("%s:%s", s, r.Sym.Name);

            }

            if (r.Add != 0L)
            {
                return fmt.Sprintf("%s:%d", s, r.Add);
            }

            return s;

        }
    }
}}}
