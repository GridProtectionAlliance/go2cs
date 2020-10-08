// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of Go intermediate object files and archives.

// package objfile -- go2cs converted at 2020 October 08 03:50:04 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\goobj.go
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using dwarf = go.debug.dwarf_package;
using gosym = go.debug.gosym_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        private partial struct goobjFile
        {
            public ptr<goobj.Package> goobj;
            public ptr<os.File> f; // the underlying .o or .a file
        }

        private static (ptr<File>, error) openGoFile(ptr<os.File> _addr_r)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;
            ref os.File r = ref _addr_r.val;

            var (f, err) = goobj.Parse(r, "\"\"");
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ptr<goobjFile> rf = addr(new goobjFile(goobj:f,f:r));
            if (len(f.Native) == 0L)
            {
                return (addr(new File(r,[]*Entry{{raw:rf}})), error.As(null!)!);
            }

            var entries = make_slice<ptr<Entry>>(len(f.Native) + 1L);
            entries[0L] = addr(new Entry(raw:rf,));
L:
            foreach (var (i, nr) in f.Native)
            {
                foreach (var (_, try) in openers)
                {
                    {
                        var (raw, err) = try(nr);

                        if (err == null)
                        {
                            entries[i + 1L] = addr(new Entry(name:nr.Name,raw:raw,));
                            _continueL = true;
                            break;
                        }

                    }

                }
                return (_addr_null!, error.As(fmt.Errorf("open %s: unrecognized archive member %s", r.Name(), nr.Name))!);

            }
            return (addr(new File(r,entries)), error.As(null!)!);

        }

        private static @string goobjName(goobj.SymID id)
        {
            if (id.Version == 0L)
            {
                return id.Name;
            }

            return fmt.Sprintf("%s<%d>", id.Name, id.Version);

        }

        private static (slice<Sym>, error) symbols(this ptr<goobjFile> _addr_f)
        {
            slice<Sym> _p0 = default;
            error _p0 = default!;
            ref goobjFile f = ref _addr_f.val;

            var seen = make_map<goobj.SymID, bool>();

            slice<Sym> syms = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in f.goobj.Syms)
                {
                    s = __s;
                    seen[s.SymID] = true;
                    Sym sym = new Sym(Addr:uint64(s.Data.Offset),Name:goobjName(s.SymID),Size:s.Size,Type:s.Type.Name,Code:'?');

                    if (s.Kind == objabi.STEXT) 
                        sym.Code = 'T';
                    else if (s.Kind == objabi.SRODATA) 
                        sym.Code = 'R';
                    else if (s.Kind == objabi.SDATA) 
                        sym.Code = 'D';
                    else if (s.Kind == objabi.SBSS || s.Kind == objabi.SNOPTRBSS || s.Kind == objabi.STLSBSS) 
                        sym.Code = 'B';
                                        if (s.Version != 0L)
                    {
                        sym.Code += 'a' - 'A';
                    }

                    {
                        var r__prev2 = r;

                        foreach (var (__i, __r) in s.Reloc)
                        {
                            i = __i;
                            r = __r;
                            sym.Relocs = append(sym.Relocs, new Reloc(Addr:uint64(s.Data.Offset)+uint64(r.Offset),Size:uint64(r.Size),Stringer:&s.Reloc[i]));
                        }

                        r = r__prev2;
                    }

                    syms = append(syms, sym);

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in f.goobj.Syms)
                {
                    s = __s;
                    {
                        var r__prev2 = r;

                        foreach (var (_, __r) in s.Reloc)
                        {
                            r = __r;
                            if (!seen[r.Sym])
                            {
                                seen[r.Sym] = true;
                                sym = new Sym(Name:goobjName(r.Sym),Code:'U');
                                if (s.Version != 0L)
                                { 
                                    // should not happen but handle anyway
                                    sym.Code = 'u';

                                }

                                syms = append(syms, sym);

                            }

                        }

                        r = r__prev2;
                    }
                }

                s = s__prev1;
            }

            return (syms, error.As(null!)!);

        }

        private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<goobjFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> symtab = default;
            slice<byte> pclntab = default;
            error err = default!;
            ref goobjFile f = ref _addr_f.val;
 
            // Should never be called. We implement Liner below, callers
            // should use that instead.
            return (0L, null, null, error.As(fmt.Errorf("pcln not available in go object file"))!);

        }

        // Find returns the file name, line, and function data for the given pc.
        // Returns "",0,nil if unknown.
        // This function implements the Liner interface in preference to pcln() above.
        private static (@string, long, ptr<gosym.Func>) PCToLine(this ptr<goobjFile> _addr_f, ulong pc)
        {
            @string _p0 = default;
            long _p0 = default;
            ptr<gosym.Func> _p0 = default!;
            ref goobjFile f = ref _addr_f.val;
 
            // TODO: this is really inefficient. Binary search? Memoize last result?
            ptr<sys.Arch> arch;
            foreach (var (_, a) in sys.Archs)
            {
                if (a.Name == f.goobj.Arch)
                {
                    arch = a;
                    break;
                }

            }
            if (arch == null)
            {
                return ("", 0L, _addr_null!);
            }

            foreach (var (_, s) in f.goobj.Syms)
            {
                if (pc < uint64(s.Data.Offset) || pc >= uint64(s.Data.Offset + s.Data.Size))
                {
                    continue;
                }

                if (s.Func == null)
                {
                    return ("", 0L, _addr_null!);
                }

                var pcfile = make_slice<byte>(s.Func.PCFile.Size);
                var (_, err) = f.f.ReadAt(pcfile, s.Func.PCFile.Offset);
                if (err != null)
                {
                    return ("", 0L, _addr_null!);
                }

                var fileID = int(pcValue(pcfile, pc - uint64(s.Data.Offset), arch));
                var fileName = s.Func.File[fileID];
                var pcline = make_slice<byte>(s.Func.PCLine.Size);
                _, err = f.f.ReadAt(pcline, s.Func.PCLine.Offset);
                if (err != null)
                {
                    return ("", 0L, _addr_null!);
                }

                var line = int(pcValue(pcline, pc - uint64(s.Data.Offset), arch)); 
                // Note: we provide only the name in the Func structure.
                // We could provide more if needed.
                return (fileName, line, addr(new gosym.Func(Sym:&gosym.Sym{Name:s.Name})));

            }
            return ("", 0L, _addr_null!);

        }

        // pcValue looks up the given PC in a pc value table. target is the
        // offset of the pc from the entry point.
        private static int pcValue(slice<byte> tab, ulong target, ptr<sys.Arch> _addr_arch)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            ref var val = ref heap(int32(-1L), out ptr<var> _addr_val);
            ref ulong pc = ref heap(out ptr<ulong> _addr_pc);
            while (step(_addr_tab, _addr_pc, _addr_val, pc == 0L, _addr_arch))
            {
                if (target < pc)
                {
                    return val;
                }

            }

            return -1L;

        }

        // step advances to the next pc, value pair in the encoded table.
        private static bool step(ptr<slice<byte>> _addr_p, ptr<ulong> _addr_pc, ptr<int> _addr_val, bool first, ptr<sys.Arch> _addr_arch)
        {
            ref slice<byte> p = ref _addr_p.val;
            ref ulong pc = ref _addr_pc.val;
            ref int val = ref _addr_val.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var uvdelta = readvarint(_addr_p);
            if (uvdelta == 0L && !first)
            {
                return false;
            }

            if (uvdelta & 1L != 0L)
            {
                uvdelta = ~(uvdelta >> (int)(1L));
            }
            else
            {
                uvdelta >>= 1L;
            }

            var vdelta = int32(uvdelta);
            var pcdelta = readvarint(_addr_p) * uint32(arch.MinLC);
            pc += uint64(pcdelta);
            val += vdelta;
            return true;

        }

        // readvarint reads, removes, and returns a varint from *p.
        private static uint readvarint(ptr<slice<byte>> _addr_p)
        {
            ref slice<byte> p = ref _addr_p.val;

            uint v = default;            uint shift = default;

            slice<byte> s = p;
            shift = 0L;

            while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
            {
                var b = s[0L];
                s = s[1L..];
                v |= (uint32(b) & 0x7FUL) << (int)(shift);
                if (b & 0x80UL == 0L)
                {
                    break;
                shift += 7L;
                }

            }

            p = s;
            return v;

        }

        // We treat the whole object file as the text section.
        private static (ulong, slice<byte>, error) text(this ptr<goobjFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> text = default;
            error err = default!;
            ref goobjFile f = ref _addr_f.val;

            os.FileInfo info = default;
            info, err = f.f.Stat();
            if (err != null)
            {
                return ;
            }

            text = make_slice<byte>(info.Size());
            _, err = f.f.ReadAt(text, 0L);
            return ;

        }

        private static @string goarch(this ptr<goobjFile> _addr_f)
        {
            ref goobjFile f = ref _addr_f.val;

            return f.goobj.Arch;
        }

        private static (ulong, error) loadAddress(this ptr<goobjFile> _addr_f)
        {
            ulong _p0 = default;
            error _p0 = default!;
            ref goobjFile f = ref _addr_f.val;

            return (0L, error.As(fmt.Errorf("unknown load address"))!);
        }

        private static (ptr<dwarf.Data>, error) dwarf(this ptr<goobjFile> _addr_f)
        {
            ptr<dwarf.Data> _p0 = default!;
            error _p0 = default!;
            ref goobjFile f = ref _addr_f.val;

            return (_addr_null!, error.As(errors.New("no DWARF data in go object file"))!);
        }
    }
}}}
