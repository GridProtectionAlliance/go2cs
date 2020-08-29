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
// package binutils -- go2cs converted at 2020 August 29 10:05:12 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\binutils.go
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
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
        private static ref binrep get(this ref Binutils bu)
        {
            bu.mu.Lock();
            var r = bu.rep;
            if (r == null)
            {
                r = ref new binrep();
                initTools(r, "");
                bu.rep = r;
            }
            bu.mu.Unlock();
            return r;
        }

        // update modifies the rep for bu via the supplied function.
        private static void update(this ref Binutils _bu, Action<ref binrep> fn) => func(_bu, (ref Binutils bu, Defer defer, Panic _, Recover __) =>
        {
            binrep r = ref new binrep();
            bu.mu.Lock();
            defer(bu.mu.Unlock());
            if (bu.rep == null)
            {
                initTools(r, "");
            }
            else
            {
                r.Value = bu.rep.Value;
            }
            fn(r);
            bu.rep = r;
        });

        // SetFastSymbolization sets a toggle that makes binutils use fast
        // symbolization (using nm), which is much faster than addr2line but
        // provides only symbol name information (no file/line).
        private static void SetFastSymbolization(this ref Binutils bu, bool fast)
        {
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
        private static void SetTools(this ref Binutils bu, @string config)
        {
            bu.update(r =>
            {
                initTools(r, config);

            });
        }

        private static void initTools(ref binrep b, @string config)
        { 
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
            b.nm, b.nmFound = findExe("nm", append(paths["nm"], defaultPath));
            b.objdump, b.objdumpFound = findExe("objdump", append(paths["objdump"], defaultPath));
        }

        // findExe looks for an executable command on a set of paths.
        // If it cannot find it, returns cmd.
        private static (@string, bool) findExe(@string cmd, slice<@string> paths)
        {
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
        private static (slice<plugin.Inst>, error) Disasm(this ref Binutils bu, @string file, ulong start, ulong end)
        {
            var b = bu.get();
            var cmd = exec.Command(b.objdump, "-d", "-C", "--no-show-raw-insn", "-l", fmt.Sprintf("--start-address=%#x", start), fmt.Sprintf("--stop-address=%#x", end), file);
            var (out, err) = cmd.Output();
            if (err != null)
            {
                return (null, fmt.Errorf("%v: %v", cmd.Args, err));
            }
            return disassemble(out);
        }

        // Open satisfies the plugin.ObjTool interface.
        private static (plugin.ObjFile, error) Open(this ref Binutils bu, @string name, ulong start, ulong limit, ulong offset)
        {
            var b = bu.get(); 

            // Make sure file is a supported executable.
            // The pprof driver uses Open to sniff the difference
            // between an executable and a profile.
            // For now, only ELF is supported.
            // Could read the first few bytes of the file and
            // use a table of prefixes if we need to support other
            // systems at some point.

            {
                var (_, err) = os.Stat(name);

                if (err != null)
                { 
                    // For testing, do not require file name to exist.
                    if (strings.Contains(b.addr2line, "testdata/"))
                    {
                        return (ref new fileAddr2Line(file:file{b:b,name:name}), null);
                    }
                    return (null, err);
                }

            }

            {
                var f__prev1 = f;

                var (f, err) = b.openELF(name, start, limit, offset);

                if (err == null)
                {
                    return (f, null);
                }

                f = f__prev1;

            }
            {
                var f__prev1 = f;

                (f, err) = b.openMachO(name, start, limit, offset);

                if (err == null)
                {
                    return (f, null);
                }

                f = f__prev1;

            }
            return (null, fmt.Errorf("unrecognized binary: %s", name));
        }

        private static (plugin.ObjFile, error) openMachO(this ref binrep _b, @string name, ulong start, ulong limit, ulong offset) => func(_b, (ref binrep b, Defer defer, Panic _, Recover __) =>
        {
            var (of, err) = macho.Open(name);
            if (err != null)
            {
                return (null, fmt.Errorf("Parsing %s: %v", name, err));
            }
            defer(of.Close());

            if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound))
            {
                return (ref new fileNM(file:file{b:b,name:name}), null);
            }
            return (ref new fileAddr2Line(file:file{b:b,name:name}), null);
        });

        private static (plugin.ObjFile, error) openELF(this ref binrep _b, @string name, ulong start, ulong limit, ulong offset) => func(_b, (ref binrep b, Defer defer, Panic _, Recover __) =>
        {
            var (ef, err) = elf.Open(name);
            if (err != null)
            {
                return (null, fmt.Errorf("Parsing %s: %v", name, err));
            }
            defer(ef.Close());

            ref ulong stextOffset = default;
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
                if (err != null)
                {
                    return (null, err);
                }
                foreach (var (_, s) in symbols)
                {
                    if (s.Name == "_stext")
                    { 
                        // The kernel may use _stext as the mapping start address.
                        stextOffset = ref s.Value;
                        break;
                    }
                }
            }
            var (base, err) = elfexec.GetBase(ref ef.FileHeader, null, stextOffset, start, limit, offset);
            if (err != null)
            {
                return (null, fmt.Errorf("Could not identify base for %s: %v", name, err));
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
                return (ref new fileNM(file:file{b,name,base,buildID}), null);
            }
            return (ref new fileAddr2Line(file:file{b,name,base,buildID}), null);
        });

        // file implements the binutils.ObjFile interface.
        private partial struct file
        {
            public ptr<binrep> b;
            public @string name;
            public ulong @base;
            public @string buildID;
        }

        private static @string Name(this ref file f)
        {
            return f.name;
        }

        private static ulong Base(this ref file f)
        {
            return f.@base;
        }

        private static @string BuildID(this ref file f)
        {
            return f.buildID;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ref file f, ulong addr)
        {
            return (new slice<plugin.Frame>(new plugin.Frame[] {  }), null);
        }

        private static error Close(this ref file f)
        {
            return error.As(null);
        }

        private static (slice<ref plugin.Sym>, error) Symbols(this ref file f, ref regexp.Regexp r, ulong addr)
        { 
            // Get from nm a list of symbols sorted by address.
            var cmd = exec.Command(f.b.nm, "-n", f.name);
            var (out, err) = cmd.Output();
            if (err != null)
            {
                return (null, fmt.Errorf("%v: %v", cmd.Args, err));
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

        private static (slice<plugin.Frame>, error) SourceLine(this ref fileNM f, ulong addr)
        {
            if (f.addr2linernm == null)
            {
                var (addr2liner, err) = newAddr2LinerNM(f.b.nm, f.name, f.@base);
                if (err != null)
                {
                    return (null, err);
                }
                f.addr2linernm = addr2liner;
            }
            return f.addr2linernm.addrInfo(addr);
        }

        // fileAddr2Line implements the binutils.ObjFile interface, using
        // 'addr2line' to map addresses to symbols (with file/line number
        // information). It can be slow for large binaries with debug
        // information.
        private partial struct fileAddr2Line
        {
            public sync.Once once;
            public ref file file => ref file_val;
            public ptr<addr2Liner> addr2liner;
            public ptr<llvmSymbolizer> llvmSymbolizer;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ref fileAddr2Line f, ulong addr)
        {
            f.once.Do(f.init);
            if (f.llvmSymbolizer != null)
            {
                return f.llvmSymbolizer.addrInfo(addr);
            }
            if (f.addr2liner != null)
            {
                return f.addr2liner.addrInfo(addr);
            }
            return (null, fmt.Errorf("could not find local addr2liner"));
        }

        private static void init(this ref fileAddr2Line f)
        {
            {
                var (llvmSymbolizer, err) = newLLVMSymbolizer(f.b.llvmSymbolizer, f.name, f.@base);

                if (err == null)
                {
                    f.llvmSymbolizer = llvmSymbolizer;
                    return;
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

        private static error Close(this ref fileAddr2Line f)
        {
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
            return error.As(null);
        }
    }
}}}}}}}
