// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objfile -- go2cs converted at 2020 August 29 08:45:44 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\disasm.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using src = go.cmd.@internal.src_package;
using list = go.container.list_package;
using gosym = go.debug.gosym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strings = go.strings_package;
using tabwriter = go.text.tabwriter_package;

using armasm = go.golang.org.x.arch.arm.armasm_package;
using arm64asm = go.golang.org.x.arch.arm64.arm64asm_package;
using ppc64asm = go.golang.org.x.arch.ppc64.ppc64asm_package;
using x86asm = go.golang.org.x.arch.x86.x86asm_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        // Disasm is a disassembler for a given File.
        public partial struct Disasm
        {
            public slice<Sym> syms; //symbols in file, sorted by address
            public Liner pcln; // pcln table
            public slice<byte> text; // bytes of text segment (actual instructions)
            public ulong textStart; // start PC of text
            public ulong textEnd; // end PC of text
            public @string goarch; // GOARCH string
            public disasmFunc disasm; // disassembler function for goarch
            public binary.ByteOrder byteOrder; // byte order for goarch
        }

        // Disasm returns a disassembler for the file f.
        private static (ref Disasm, error) Disasm(this ref Entry e)
        {
            var (syms, err) = e.Symbols();
            if (err != null)
            {
                return (null, err);
            }
            var (pcln, err) = e.PCLineTable();
            if (err != null)
            {
                return (null, err);
            }
            var (textStart, textBytes, err) = e.Text();
            if (err != null)
            {
                return (null, err);
            }
            var goarch = e.GOARCH();
            var disasm = disasms[goarch];
            var byteOrder = byteOrders[goarch];
            if (disasm == null || byteOrder == null)
            {
                return (null, fmt.Errorf("unsupported architecture"));
            } 

            // Filter out section symbols, overwriting syms in place.
            var keep = syms[..0L];
            foreach (var (_, sym) in syms)
            {
                switch (sym.Name)
                {
                    case "runtime.text": 

                    case "text": 

                    case "_text": 

                    case "runtime.etext": 

                    case "etext": 

                    case "_etext": 
                        break;
                    default: 
                        keep = append(keep, sym);
                        break;
                }
            }
            syms = keep;
            Disasm d = ref new Disasm(syms:syms,pcln:pcln,text:textBytes,textStart:textStart,textEnd:textStart+uint64(len(textBytes)),goarch:goarch,disasm:disasm,byteOrder:byteOrder,);

            return (d, null);
        }

        // lookup finds the symbol name containing addr.
        private static (@string, ulong) lookup(this ref Disasm d, ulong addr)
        {
            var i = sort.Search(len(d.syms), i => addr < d.syms[i].Addr);
            if (i > 0L)
            {
                var s = d.syms[i - 1L];
                if (s.Addr != 0L && s.Addr <= addr && addr < s.Addr + uint64(s.Size))
                {
                    return (s.Name, s.Addr);
                }
            }
            return ("", 0L);
        }

        // base returns the final element in the path.
        // It works on both Windows and Unix paths,
        // regardless of host operating system.
        private static @string @base(@string path)
        {
            path = path[strings.LastIndex(path, "/") + 1L..];
            path = path[strings.LastIndex(path, "\\") + 1L..];
            return path;
        }

        // CachedFile contains the content of a file split into lines.
        public partial struct CachedFile
        {
            public @string FileName;
            public slice<slice<byte>> Lines;
        }

        // FileCache is a simple LRU cache of file contents.
        public partial struct FileCache
        {
            public ptr<list.List> files;
            public long maxLen;
        }

        // NewFileCache returns a FileCache which can contain up to maxLen cached file contents.
        public static ref FileCache NewFileCache(long maxLen)
        {
            return ref new FileCache(files:list.New(),maxLen:maxLen,);
        }

        // Line returns the source code line for the given file and line number.
        // If the file is not already cached, reads it , inserts it into the cache,
        // and removes the least recently used file if necessary.
        // If the file is in cache, moves it up to the front of the list.
        private static (slice<byte>, error) Line(this ref FileCache fc, @string filename, long line)
        {
            if (filepath.Ext(filename) != ".go")
            {
                return (null, null);
            } 

            // Clean filenames returned by src.Pos.SymFilename()
            // or src.PosBase.SymFilename() removing
            // the leading src.FileSymPrefix.
            if (strings.HasPrefix(filename, src.FileSymPrefix))
            {
                filename = filename[len(src.FileSymPrefix)..];
            } 

            // Expand literal "$GOROOT" rewrited by obj.AbsFile()
            filename = filepath.Clean(os.ExpandEnv(filename));

            ref CachedFile cf = default;
            ref list.Element e = default;

            e = fc.files.Front();

            while (e != null)
            {
                cf = e.Value._<ref CachedFile>();
                if (cf.FileName == filename)
                {
                    break;
                e = e.Next();
                }
            }


            if (e == null)
            {
                var (content, err) = ioutil.ReadFile(filename);
                if (err != null)
                {
                    return (null, err);
                }
                cf = ref new CachedFile(FileName:filename,Lines:bytes.Split(content,[]byte{'\n'}),);
                fc.files.PushFront(cf);

                if (fc.files.Len() >= fc.maxLen)
                {
                    fc.files.Remove(fc.files.Back());
                }
            }
            else
            {
                fc.files.MoveToFront(e);
            }
            return (cf.Lines[line - 1L], null);
        }

        // Print prints a disassembly of the file to w.
        // If filter is non-nil, the disassembly only includes functions with names matching filter.
        // If printCode is true, the disassembly includs corresponding source lines.
        // The disassembly only includes functions that overlap the range [start, end).
        private static void Print(this ref Disasm d, io.Writer w, ref regexp.Regexp filter, ulong start, ulong end, bool printCode)
        {
            if (start < d.textStart)
            {
                start = d.textStart;
            }
            if (end > d.textEnd)
            {
                end = d.textEnd;
            }
            var printed = false;
            var bw = bufio.NewWriter(w);

            ref FileCache fc = default;
            if (printCode)
            {
                fc = NewFileCache(8L);
            }
            foreach (var (_, sym) in d.syms)
            {
                var symStart = sym.Addr;
                var symEnd = sym.Addr + uint64(sym.Size);
                var relocs = sym.Relocs;
                if (sym.Code != 'T' && sym.Code != 't' || symStart < d.textStart || symEnd <= start || end <= symStart || filter != null && !filter.MatchString(sym.Name))
                {
                    continue;
                }
                if (printed)
                {
                    fmt.Fprintf(bw, "\n");
                }
                printed = true;

                var (file, _, _) = d.pcln.PCToLine(sym.Addr);
                fmt.Fprintf(bw, "TEXT %s(SB) %s\n", sym.Name, file);

                var tw = tabwriter.NewWriter(bw, 18L, 8L, 1L, '\t', tabwriter.StripEscape);
                if (symEnd > end)
                {
                    symEnd = end;
                }
                var code = d.text[..end - d.textStart];

                @string lastFile = default;
                long lastLine = default;

                d.Decode(symStart, symEnd, relocs, (pc, size, file, line, text) =>
                {
                    var i = pc - d.textStart;

                    if (printCode)
                    {
                        if (file != lastFile || line != lastLine)
                        {
                            {
                                var (srcLine, err) = fc.Line(file, line);

                                if (err == null)
                                {
                                    fmt.Fprintf(tw, "%s%s%s\n", new slice<byte>(new byte[] { tabwriter.Escape }), srcLine, new slice<byte>(new byte[] { tabwriter.Escape }));
                                }

                            }

                            lastFile = file;
                            lastLine = line;
                        }
                        fmt.Fprintf(tw, "  %#x\t", pc);
                    }
                    else
                    {
                        fmt.Fprintf(tw, "  %s:%d\t%#x\t", base(file), line, pc);
                    }
                    if (size % 4L != 0L || d.goarch == "386" || d.goarch == "amd64" || d.goarch == "amd64p32")
                    { 
                        // Print instruction as bytes.
                        fmt.Fprintf(tw, "%x", code[i..i + size]);
                    }
                    else
                    { 
                        // Print instruction as 32-bit words.
                        {
                            var j = uint64(0L);

                            while (j < size)
                            {
                                if (j > 0L)
                                {
                                    fmt.Fprintf(tw, " ");
                                j += 4L;
                                }
                                fmt.Fprintf(tw, "%08x", d.byteOrder.Uint32(code[i + j..]));
                            }

                        }
                    }
                    fmt.Fprintf(tw, "\t%s\n", text);
                });
                tw.Flush();
            }
            bw.Flush();
        }

        // Decode disassembles the text segment range [start, end), calling f for each instruction.
        private static void Decode(this ref Disasm d, ulong start, ulong end, slice<Reloc> relocs, Action<ulong, ulong, @string, long, @string> f)
        {
            if (start < d.textStart)
            {
                start = d.textStart;
            }
            if (end > d.textEnd)
            {
                end = d.textEnd;
            }
            var code = d.text[..end - d.textStart];
            var lookup = d.lookup;
            {
                var pc = start;

                while (pc < end)
                {
                    var i = pc - d.textStart;
                    var (text, size) = d.disasm(code[i..], pc, lookup, d.byteOrder);
                    var (file, line, _) = d.pcln.PCToLine(pc);
                    text += "\t";
                    var first = true;
                    while (len(relocs) > 0L && relocs[0L].Addr < i + uint64(size))
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            text += " ";
                        }
                        text += relocs[0L].Stringer.String(pc - start);
                        relocs = relocs[1L..];
                    }

                    f(pc, uint64(size), file, line, text);
                    pc += uint64(size);
                }

            }
        }

        public delegate  ulong) lookupFunc(ulong,  (@string);
        public delegate  long) disasmFunc(slice<byte>,  ulong,  lookupFunc,  binary.ByteOrder,  (@string);

        private static (@string, long) disasm_386(slice<byte> code, ulong pc, lookupFunc lookup, binary.ByteOrder _)
        {
            return disasm_x86(code, pc, lookup, 32L);
        }

        private static (@string, long) disasm_amd64(slice<byte> code, ulong pc, lookupFunc lookup, binary.ByteOrder _)
        {
            return disasm_x86(code, pc, lookup, 64L);
        }

        private static (@string, long) disasm_x86(slice<byte> code, ulong pc, lookupFunc lookup, long arch)
        {
            var (inst, err) = x86asm.Decode(code, arch);
            @string text = default;
            var size = inst.Len;
            if (err != null || size == 0L || inst.Op == 0L)
            {
                size = 1L;
                text = "?";
            }
            else
            {
                text = x86asm.GoSyntax(inst, pc, lookup);
            }
            return (text, size);
        }

        private partial struct textReader
        {
            public slice<byte> code;
            public ulong pc;
        }

        private static (long, error) ReadAt(this textReader r, slice<byte> data, long off)
        {
            if (off < 0L || uint64(off) < r.pc)
            {
                return (0L, io.EOF);
            }
            var d = uint64(off) - r.pc;
            if (d >= uint64(len(r.code)))
            {
                return (0L, io.EOF);
            }
            n = copy(data, r.code[d..]);
            if (n < len(data))
            {
                err = io.ErrUnexpectedEOF;
            }
            return;
        }

        private static (@string, long) disasm_arm(slice<byte> code, ulong pc, lookupFunc lookup, binary.ByteOrder _)
        {
            var (inst, err) = armasm.Decode(code, armasm.ModeARM);
            @string text = default;
            var size = inst.Len;
            if (err != null || size == 0L || inst.Op == 0L)
            {
                size = 4L;
                text = "?";
            }
            else
            {
                text = armasm.GoSyntax(inst, pc, lookup, new textReader(code,pc));
            }
            return (text, size);
        }

        private static (@string, long) disasm_arm64(slice<byte> code, ulong pc, lookupFunc lookup, binary.ByteOrder byteOrder)
        {
            var (inst, err) = arm64asm.Decode(code);
            @string text = default;
            if (err != null || inst.Op == 0L)
            {
                text = "?";
            }
            else
            {
                text = arm64asm.GoSyntax(inst, pc, lookup, new textReader(code,pc));
            }
            return (text, 4L);
        }

        private static (@string, long) disasm_ppc64(slice<byte> code, ulong pc, lookupFunc lookup, binary.ByteOrder byteOrder)
        {
            var (inst, err) = ppc64asm.Decode(code, byteOrder);
            @string text = default;
            var size = inst.Len;
            if (err != null || size == 0L || inst.Op == 0L)
            {
                size = 4L;
                text = "?";
            }
            else
            {
                text = ppc64asm.GoSyntax(inst, pc, lookup);
            }
            return (text, size);
        }

        private static map disasms = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, disasmFunc>{"386":disasm_386,"amd64":disasm_amd64,"amd64p32":disasm_amd64,"arm":disasm_arm,"arm64":disasm_arm64,"ppc64":disasm_ppc64,"ppc64le":disasm_ppc64,};

        private static map byteOrders = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, binary.ByteOrder>{"386":binary.LittleEndian,"amd64":binary.LittleEndian,"amd64p32":binary.LittleEndian,"arm":binary.LittleEndian,"arm64":binary.LittleEndian,"ppc64":binary.BigEndian,"ppc64le":binary.LittleEndian,"s390x":binary.BigEndian,};

        public partial interface Liner
        {
            (@string, long, ref gosym.Func) PCToLine(ulong _p0);
        }
    }
}}}
