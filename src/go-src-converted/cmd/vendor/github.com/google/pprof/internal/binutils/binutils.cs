// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Package binutils provides access to the GNU binutils.
// package binutils -- go2cs converted at 2020 October 08 04:42:51 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\binutils.go
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using sync = go.sync_package;

using elfexec = go.github.com.google.pprof.@internal.elfexec_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class binutils_package
    {
        // A Binutils implements plugin.ObjTool by invoking the GNU binutils.
        public partial struct Binutils
        {
            public sync.Mutex mu;
            public ptr<binrep> rep;
        }

        // binrep is an immutable representation for Binutils.  It is atomically
        // replaced on every mutation to provide thread-safe access.
        private partial struct binrep
        {
            public @string llvmSymbolizer;
            public bool llvmSymbolizerFound;
            public @string addr2line;
            public bool addr2lineFound;
            public @string nm;
            public bool nmFound;
            public @string objdump;
            public bool objdumpFound; // if fast, perform symbolization using nm (symbol names only),
// instead of file-line detail from the slower addr2line.
            public bool fast;
        }

        // get returns the current representation for bu, initializing it if necessary.
        private static ptr<binrep> get(this ptr<Binutils> _addr_bu)
        {
            ref Binutils bu = ref _addr_bu.val;

            bu.mu.Lock();
            var r = bu.rep;
            if (r == null)
            {
                r = addr(new binrep());
                initTools(_addr_r, "");
                bu.rep = r;
            }

            bu.mu.Unlock();
            return _addr_r!;

        }

        // update modifies the rep for bu via the supplied function.
        private static void update(this ptr<Binutils> _addr_bu, Action<ptr<binrep>> fn) => func((defer, _, __) =>
        {
            ref Binutils bu = ref _addr_bu.val;

            ptr<binrep> r = addr(new binrep());
            bu.mu.Lock();
            defer(bu.mu.Unlock());
            if (bu.rep == null)
            {
                initTools(_addr_r, "");
            }
            else
            {
                r.val = bu.rep.val;
            }

            fn(r);
            bu.rep = r;

        });

        // String returns string representation of the binutils state for debug logging.
        private static @string String(this ptr<Binutils> _addr_bu)
        {
            ref Binutils bu = ref _addr_bu.val;

            var r = bu.get();
            @string llvmSymbolizer = default;            @string addr2line = default;            @string nm = default;            @string objdump = default;

            if (r.llvmSymbolizerFound)
            {
                llvmSymbolizer = r.llvmSymbolizer;
            }

            if (r.addr2lineFound)
            {
                addr2line = r.addr2line;
            }

            if (r.nmFound)
            {
                nm = r.nm;
            }

            if (r.objdumpFound)
            {
                objdump = r.objdump;
            }

            return fmt.Sprintf("llvm-symbolizer=%q addr2line=%q nm=%q objdump=%q fast=%t", llvmSymbolizer, addr2line, nm, objdump, r.fast);

        }

        // SetFastSymbolization sets a toggle that makes binutils use fast
        // symbolization (using nm), which is much faster than addr2line but
        // provides only symbol name information (no file/line).
        private static void SetFastSymbolization(this ptr<Binutils> _addr_bu, bool fast)
        {
            ref Binutils bu = ref _addr_bu.val;

            bu.update(r =>
            {
                r.fast = fast;
            });

        }

        // SetTools processes the contents of the tools option. It
        // expects a set of entries separated by commas; each entry is a pair
        // of the form t:path, where cmd will be used to look only for the
        // tool named t. If t is not specified, the path is searched for all
        // tools.
        private static void SetTools(this ptr<Binutils> _addr_bu, @string config)
        {
            ref Binutils bu = ref _addr_bu.val;

            bu.update(r =>
            {
                initTools(_addr_r, config);
            });

        }

        private static void initTools(ptr<binrep> _addr_b, @string config)
        {
            ref binrep b = ref _addr_b.val;
 
            // paths collect paths per tool; Key "" contains the default.
            var paths = make_map<@string, slice<@string>>();
            foreach (var (_, t) in strings.Split(config, ","))
            {
                @string name = "";
                var path = t;
                {
                    var ct = strings.SplitN(t, ":", 2L);

                    if (len(ct) == 2L)
                    {
                        name = ct[0L];
                        path = ct[1L];

                    }

                }

                paths[name] = append(paths[name], path);

            }
            var defaultPath = paths[""];
            b.llvmSymbolizer, b.llvmSymbolizerFound = findExe("llvm-symbolizer", append(paths["llvm-symbolizer"], defaultPath));
            b.addr2line, b.addr2lineFound = findExe("addr2line", append(paths["addr2line"], defaultPath));
            if (!b.addr2lineFound)
            { 
                // On MacOS, brew installs addr2line under gaddr2line name, so search for
                // that if the tool is not found by its default name.
                b.addr2line, b.addr2lineFound = findExe("gaddr2line", append(paths["addr2line"], defaultPath));

            }

            b.nm, b.nmFound = findExe("nm", append(paths["nm"], defaultPath));
            b.objdump, b.objdumpFound = findExe("objdump", append(paths["objdump"], defaultPath));

        }

        // findExe looks for an executable command on a set of paths.
        // If it cannot find it, returns cmd.
        private static (@string, bool) findExe(@string cmd, slice<@string> paths)
        {
            @string _p0 = default;
            bool _p0 = default;

            foreach (var (_, p) in paths)
            {
                var cp = filepath.Join(p, cmd);
                {
                    var (c, err) = exec.LookPath(cp);

                    if (err == null)
                    {
                        return (c, true);
                    }

                }

            }
            return (cmd, false);

        }

        // Disasm returns the assembly instructions for the specified address range
        // of a binary.
        private static (slice<plugin.Inst>, error) Disasm(this ptr<Binutils> _addr_bu, @string file, ulong start, ulong end)
        {
            slice<plugin.Inst> _p0 = default;
            error _p0 = default!;
            ref Binutils bu = ref _addr_bu.val;

            var b = bu.get();
            var cmd = exec.Command(b.objdump, "-d", "-C", "--no-show-raw-insn", "-l", fmt.Sprintf("--start-address=%#x", start), fmt.Sprintf("--stop-address=%#x", end), file);
            var (out, err) = cmd.Output();
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("%v: %v", cmd.Args, err))!);
            }

            return disassemble(out);

        }

        // Open satisfies the plugin.ObjTool interface.
        private static (plugin.ObjFile, error) Open(this ptr<Binutils> _addr_bu, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, __) =>
        {
            plugin.ObjFile _p0 = default;
            error _p0 = default!;
            ref Binutils bu = ref _addr_bu.val;

            var b = bu.get(); 

            // Make sure file is a supported executable.
            // This uses magic numbers, mainly to provide better error messages but
            // it should also help speed.

            {
                var (_, err) = os.Stat(name);

                if (err != null)
                { 
                    // For testing, do not require file name to exist.
                    if (strings.Contains(b.addr2line, "testdata/"))
                    {
                        return (addr(new fileAddr2Line(file:file{b:b,name:name})), error.As(null!)!);
                    }

                    return (null, error.As(err)!);

                } 

                // Read the first 4 bytes of the file.

            } 

            // Read the first 4 bytes of the file.

            var (f, err) = os.Open(name);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("error opening %s: %v", name, err))!);
            }

            defer(f.Close());

            array<byte> header = new array<byte>(4L);
            _, err = io.ReadFull(f, header[..]);

            if (err != null)
            {
                return (null, error.As(fmt.Errorf("error reading magic number from %s: %v", name, err))!);
            }

            var elfMagic = string(header[..]); 

            // Match against supported file types.
            if (elfMagic == elf.ELFMAG)
            {
                (f, err) = b.openELF(name, start, limit, offset);
                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("error reading ELF file %s: %v", name, err))!);
                }

                return (f, error.As(null!)!);

            } 

            // Mach-O magic numbers can be big or little endian.
            var machoMagicLittle = binary.LittleEndian.Uint32(header[..]);
            var machoMagicBig = binary.BigEndian.Uint32(header[..]);

            if (machoMagicLittle == macho.Magic32 || machoMagicLittle == macho.Magic64 || machoMagicBig == macho.Magic32 || machoMagicBig == macho.Magic64)
            {
                (f, err) = b.openMachO(name, start, limit, offset);
                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("error reading Mach-O file %s: %v", name, err))!);
                }

                return (f, error.As(null!)!);

            }

            if (machoMagicLittle == macho.MagicFat || machoMagicBig == macho.MagicFat)
            {
                (f, err) = b.openFatMachO(name, start, limit, offset);
                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("error reading fat Mach-O file %s: %v", name, err))!);
                }

                return (f, error.As(null!)!);

            }

            return (null, error.As(fmt.Errorf("unrecognized binary format: %s", name))!);

        });

        private static (plugin.ObjFile, error) openMachOCommon(this ptr<binrep> _addr_b, @string name, ptr<macho.File> _addr_of, ulong start, ulong limit, ulong offset)
        {
            plugin.ObjFile _p0 = default;
            error _p0 = default!;
            ref binrep b = ref _addr_b.val;
            ref macho.File of = ref _addr_of.val;

            // Subtract the load address of the __TEXT section. Usually 0 for shared
            // libraries or 0x100000000 for executables. You can check this value by
            // running `objdump -private-headers <file>`.

            var textSegment = of.Segment("__TEXT");
            if (textSegment == null)
            {
                return (null, error.As(fmt.Errorf("could not identify base for %s: no __TEXT segment", name))!);
            }

            if (textSegment.Addr > start)
            {
                return (null, error.As(fmt.Errorf("could not identify base for %s: __TEXT segment address (0x%x) > mapping start address (0x%x)", name, textSegment.Addr, start))!);
            }

            var @base = start - textSegment.Addr;

            if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound))
            {
                return (addr(new fileNM(file:file{b:b,name:name,base:base})), error.As(null!)!);
            }

            return (addr(new fileAddr2Line(file:file{b:b,name:name,base:base})), error.As(null!)!);

        }

        private static (plugin.ObjFile, error) openFatMachO(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, __) =>
        {
            plugin.ObjFile _p0 = default;
            error _p0 = default!;
            ref binrep b = ref _addr_b.val;

            var (of, err) = macho.OpenFat(name);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
            }

            defer(of.Close());

            if (len(of.Arches) == 0L)
            {
                return (null, error.As(fmt.Errorf("empty fat Mach-O file: %s", name))!);
            }

            macho.Cpu arch = default; 
            // Use the host architecture.
            // TODO: This is not ideal because the host architecture may not be the one
            // that was profiled. E.g. an amd64 host can profile a 386 program.
            switch (runtime.GOARCH)
            {
                case "386": 
                    arch = macho.Cpu386;
                    break;
                case "amd64": 

                case "amd64p32": 
                    arch = macho.CpuAmd64;
                    break;
                case "arm": 

                case "armbe": 

                case "arm64": 

                case "arm64be": 
                    arch = macho.CpuArm;
                    break;
                case "ppc": 
                    arch = macho.CpuPpc;
                    break;
                case "ppc64": 

                case "ppc64le": 
                    arch = macho.CpuPpc64;
                    break;
                default: 
                    return (null, error.As(fmt.Errorf("unsupported host architecture for %s: %s", name, runtime.GOARCH))!);
                    break;
            }
            foreach (var (i) in of.Arches)
            {
                if (of.Arches[i].Cpu == arch)
                {
                    return b.openMachOCommon(name, of.Arches[i].File, start, limit, offset);
                }

            }
            return (null, error.As(fmt.Errorf("architecture not found in %s: %s", name, runtime.GOARCH))!);

        });

        private static (plugin.ObjFile, error) openMachO(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, __) =>
        {
            plugin.ObjFile _p0 = default;
            error _p0 = default!;
            ref binrep b = ref _addr_b.val;

            var (of, err) = macho.Open(name);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
            }

            defer(of.Close());

            return b.openMachOCommon(name, of, start, limit, offset);

        });

        private static (plugin.ObjFile, error) openELF(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, __) =>
        {
            plugin.ObjFile _p0 = default;
            error _p0 = default!;
            ref binrep b = ref _addr_b.val;

            var (ef, err) = elf.Open(name);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
            }

            defer(ef.Close());

            ptr<ulong> stextOffset;
            Func<ulong, bool> pageAligned = addr => addr % 4096L == 0L;
            if (strings.Contains(name, "vmlinux") || !pageAligned(start) || !pageAligned(limit) || !pageAligned(offset))
            { 
                // Reading all Symbols is expensive, and we only rarely need it so
                // we don't want to do it every time. But if _stext happens to be
                // page-aligned but isn't the same as Vaddr, we would symbolize
                // wrong. So if the name the addresses aren't page aligned, or if
                // the name is "vmlinux" we read _stext. We can be wrong if: (1)
                // someone passes a kernel path that doesn't contain "vmlinux" AND
                // (2) _stext is page-aligned AND (3) _stext is not at Vaddr
                var (symbols, err) = ef.Symbols();
                if (err != null && err != elf.ErrNoSymbols)
                {
                    return (null, error.As(err)!);
                }

                foreach (var (_, s) in symbols)
                {
                    if (s.Name == "_stext")
                    { 
                        // The kernel may use _stext as the mapping start address.
                        stextOffset = _addr_s.Value;
                        break;

                    }

                }

            }

            var (base, err) = elfexec.GetBase(_addr_ef.FileHeader, elfexec.FindTextProgHeader(ef), stextOffset, start, limit, offset);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("could not identify base for %s: %v", name, err))!);
            }

            @string buildID = "";
            {
                var (f, err) = os.Open(name);

                if (err == null)
                {
                    {
                        var (id, err) = elfexec.GetBuildID(f);

                        if (err == null)
                        {
                            buildID = fmt.Sprintf("%x", id);
                        }

                    }

                }

            }

            if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound))
            {
                return (addr(new fileNM(file:file{b,name,base,buildID})), error.As(null!)!);
            }

            return (addr(new fileAddr2Line(file:file{b,name,base,buildID})), error.As(null!)!);

        });

        // file implements the binutils.ObjFile interface.
        private partial struct file
        {
            public ptr<binrep> b;
            public @string name;
            public ulong @base;
            public @string buildID;
        }

        private static @string Name(this ptr<file> _addr_f)
        {
            ref file f = ref _addr_f.val;

            return f.name;
        }

        private static ulong Base(this ptr<file> _addr_f)
        {
            ref file f = ref _addr_f.val;

            return f.@base;
        }

        private static @string BuildID(this ptr<file> _addr_f)
        {
            ref file f = ref _addr_f.val;

            return f.buildID;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ptr<file> _addr_f, ulong addr)
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref file f = ref _addr_f.val;

            return (new slice<plugin.Frame>(new plugin.Frame[] {  }), error.As(null!)!);
        }

        private static error Close(this ptr<file> _addr_f)
        {
            ref file f = ref _addr_f.val;

            return error.As(null!)!;
        }

        private static (slice<ptr<plugin.Sym>>, error) Symbols(this ptr<file> _addr_f, ptr<regexp.Regexp> _addr_r, ulong addr)
        {
            slice<ptr<plugin.Sym>> _p0 = default;
            error _p0 = default!;
            ref file f = ref _addr_f.val;
            ref regexp.Regexp r = ref _addr_r.val;
 
            // Get from nm a list of symbols sorted by address.
            var cmd = exec.Command(f.b.nm, "-n", f.name);
            var (out, err) = cmd.Output();
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("%v: %v", cmd.Args, err))!);
            }

            return findSymbols(out, f.name, r, addr);

        }

        // fileNM implements the binutils.ObjFile interface, using 'nm' to map
        // addresses to symbols (without file/line number information). It is
        // faster than fileAddr2Line.
        private partial struct fileNM
        {
            public ref file file => ref file_val;
            public ptr<addr2LinerNM> addr2linernm;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ptr<fileNM> _addr_f, ulong addr)
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref fileNM f = ref _addr_f.val;

            if (f.addr2linernm == null)
            {
                var (addr2liner, err) = newAddr2LinerNM(f.b.nm, f.name, f.@base);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                f.addr2linernm = addr2liner;

            }

            return f.addr2linernm.addrInfo(addr);

        }

        // fileAddr2Line implements the binutils.ObjFile interface, using
        // llvm-symbolizer, if that's available, or addr2line to map addresses to
        // symbols (with file/line number information). It can be slow for large
        // binaries with debug information.
        private partial struct fileAddr2Line
        {
            public sync.Once once;
            public ref file file => ref file_val;
            public ptr<addr2Liner> addr2liner;
            public ptr<llvmSymbolizer> llvmSymbolizer;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ptr<fileAddr2Line> _addr_f, ulong addr)
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref fileAddr2Line f = ref _addr_f.val;

            f.once.Do(f.init);
            if (f.llvmSymbolizer != null)
            {
                return f.llvmSymbolizer.addrInfo(addr);
            }

            if (f.addr2liner != null)
            {
                return f.addr2liner.addrInfo(addr);
            }

            return (null, error.As(fmt.Errorf("could not find local addr2liner"))!);

        }

        private static void init(this ptr<fileAddr2Line> _addr_f)
        {
            ref fileAddr2Line f = ref _addr_f.val;

            {
                var (llvmSymbolizer, err) = newLLVMSymbolizer(f.b.llvmSymbolizer, f.name, f.@base);

                if (err == null)
                {
                    f.llvmSymbolizer = llvmSymbolizer;
                    return ;
                }

            }


            {
                var (addr2liner, err) = newAddr2Liner(f.b.addr2line, f.name, f.@base);

                if (err == null)
                {
                    f.addr2liner = addr2liner; 

                    // When addr2line encounters some gcc compiled binaries, it
                    // drops interesting parts of names in anonymous namespaces.
                    // Fallback to NM for better function names.
                    {
                        var (nm, err) = newAddr2LinerNM(f.b.nm, f.name, f.@base);

                        if (err == null)
                        {
                            f.addr2liner.nm = nm;
                        }

                    }

                }

            }

        }

        private static error Close(this ptr<fileAddr2Line> _addr_f)
        {
            ref fileAddr2Line f = ref _addr_f.val;

            if (f.llvmSymbolizer != null)
            {
                f.llvmSymbolizer.rw.close();
                f.llvmSymbolizer = null;
            }

            if (f.addr2liner != null)
            {
                f.addr2liner.rw.close();
                f.addr2liner = null;
            }

            return error.As(null!)!;

        }
    }
}}}}}}}
