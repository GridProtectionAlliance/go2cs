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
// package binutils -- go2cs converted at 2022 March 06 23:23:18 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\binutils.go
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;

using elfexec = go.github.com.google.pprof.@internal.elfexec_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class binutils_package {

    // A Binutils implements plugin.ObjTool by invoking the GNU binutils.
public partial struct Binutils {
    public sync.Mutex mu;
    public ptr<binrep> rep;
}

private static var objdumpLLVMVerRE = regexp.MustCompile("LLVM version (?:(\\d*)\\.(\\d*)\\.(\\d*)|.*(trunk).*)");private static var elfOpen = elf.Open;

// binrep is an immutable representation for Binutils.  It is atomically
// replaced on every mutation to provide thread-safe access.
private partial struct binrep {
    public @string llvmSymbolizer;
    public bool llvmSymbolizerFound;
    public @string addr2line;
    public bool addr2lineFound;
    public @string nm;
    public bool nmFound;
    public @string objdump;
    public bool objdumpFound;
    public bool isLLVMObjdump; // if fast, perform symbolization using nm (symbol names only),
// instead of file-line detail from the slower addr2line.
    public bool fast;
}

// get returns the current representation for bu, initializing it if necessary.
private static ptr<binrep> get(this ptr<Binutils> _addr_bu) {
    ref Binutils bu = ref _addr_bu.val;

    bu.mu.Lock();
    var r = bu.rep;
    if (r == null) {
        r = addr(new binrep());
        initTools(_addr_r, "");
        bu.rep = r;
    }
    bu.mu.Unlock();
    return _addr_r!;

}

// update modifies the rep for bu via the supplied function.
private static void update(this ptr<Binutils> _addr_bu, Action<ptr<binrep>> fn) => func((defer, _, _) => {
    ref Binutils bu = ref _addr_bu.val;

    ptr<binrep> r = addr(new binrep());
    bu.mu.Lock();
    defer(bu.mu.Unlock());
    if (bu.rep == null) {
        initTools(r, "");
    }
    else
 {
        r.val = bu.rep.val;
    }
    fn(r);
    bu.rep = r;

});

// String returns string representation of the binutils state for debug logging.
private static @string String(this ptr<Binutils> _addr_bu) {
    ref Binutils bu = ref _addr_bu.val;

    var r = bu.get();
    @string llvmSymbolizer = default;    @string addr2line = default;    @string nm = default;    @string objdump = default;

    if (r.llvmSymbolizerFound) {
        llvmSymbolizer = r.llvmSymbolizer;
    }
    if (r.addr2lineFound) {
        addr2line = r.addr2line;
    }
    if (r.nmFound) {
        nm = r.nm;
    }
    if (r.objdumpFound) {
        objdump = r.objdump;
    }
    return fmt.Sprintf("llvm-symbolizer=%q addr2line=%q nm=%q objdump=%q fast=%t", llvmSymbolizer, addr2line, nm, objdump, r.fast);

}

// SetFastSymbolization sets a toggle that makes binutils use fast
// symbolization (using nm), which is much faster than addr2line but
// provides only symbol name information (no file/line).
private static void SetFastSymbolization(this ptr<Binutils> _addr_bu, bool fast) {
    ref Binutils bu = ref _addr_bu.val;

    bu.update(r => {
        r.fast = fast;
    });

}

// SetTools processes the contents of the tools option. It
// expects a set of entries separated by commas; each entry is a pair
// of the form t:path, where cmd will be used to look only for the
// tool named t. If t is not specified, the path is searched for all
// tools.
private static void SetTools(this ptr<Binutils> _addr_bu, @string config) {
    ref Binutils bu = ref _addr_bu.val;

    bu.update(r => {
        initTools(_addr_r, config);
    });

}

private static void initTools(ptr<binrep> _addr_b, @string config) {
    ref binrep b = ref _addr_b.val;
 
    // paths collect paths per tool; Key "" contains the default.
    var paths = make_map<@string, slice<@string>>();
    foreach (var (_, t) in strings.Split(config, ",")) {
        @string name = "";
        var path = t;
        {
            var ct = strings.SplitN(t, ":", 2);

            if (len(ct) == 2) {
                (name, path) = (ct[0], ct[1]);
            }

        }

        paths[name] = append(paths[name], path);

    }    var defaultPath = paths[""];
    b.llvmSymbolizer, b.llvmSymbolizerFound = chooseExe(new slice<@string>(new @string[] { "llvm-symbolizer" }), new slice<@string>(new @string[] {  }), append(paths["llvm-symbolizer"], defaultPath));
    b.addr2line, b.addr2lineFound = chooseExe(new slice<@string>(new @string[] { "addr2line" }), new slice<@string>(new @string[] { "gaddr2line" }), append(paths["addr2line"], defaultPath)); 
    // The "-n" option is supported by LLVM since 2011. The output of llvm-nm
    // and GNU nm with "-n" option is interchangeable for our purposes, so we do
    // not need to differrentiate them.
    b.nm, b.nmFound = chooseExe(new slice<@string>(new @string[] { "llvm-nm", "nm" }), new slice<@string>(new @string[] { "gnm" }), append(paths["nm"], defaultPath));
    b.objdump, b.objdumpFound, b.isLLVMObjdump = findObjdump(append(paths["objdump"], defaultPath));

}

// findObjdump finds and returns path to preferred objdump binary.
// Order of preference is: llvm-objdump, objdump.
// On MacOS only, also looks for gobjdump with least preference.
// Accepts a list of paths and returns:
// a string with path to the preferred objdump binary if found,
// or an empty string if not found;
// a boolean if any acceptable objdump was found;
// a boolean indicating if it is an LLVM objdump.
private static (@string, bool, bool) findObjdump(slice<@string> paths) {
    @string _p0 = default;
    bool _p0 = default;
    bool _p0 = default;

    @string objdumpNames = new slice<@string>(new @string[] { "llvm-objdump", "objdump" });
    if (runtime.GOOS == "darwin") {
        objdumpNames = append(objdumpNames, "gobjdump");
    }
    foreach (var (_, objdumpName) in objdumpNames) {
        {
            var (objdump, objdumpFound) = findExe(objdumpName, paths);

            if (objdumpFound) {
                var (cmdOut, err) = exec.Command(objdump, "--version").Output();
                if (err != null) {
                    continue;
                }
                if (isLLVMObjdump(string(cmdOut))) {
                    return (objdump, true, true);
                }
                if (isBuObjdump(string(cmdOut))) {
                    return (objdump, true, false);
                }
            }

        }

    }    return ("", false, false);

}

// chooseExe finds and returns path to preferred binary. names is a list of
// names to search on both Linux and OSX. osxNames is a list of names specific
// to OSX. names always has a higher priority than osxNames. The order of
// the name within each list decides its priority (e.g. the first name has a
// higher priority than the second name in the list).
//
// It returns a string with path to the binary and a boolean indicating if any
// acceptable binary was found.
private static (@string, bool) chooseExe(slice<@string> names, slice<@string> osxNames, slice<@string> paths) {
    @string _p0 = default;
    bool _p0 = default;

    if (runtime.GOOS == "darwin") {
        names = append(names, osxNames);
    }
    foreach (var (_, name) in names) {
        {
            var (binary, found) = findExe(name, paths);

            if (found) {
                return (binary, true);
            }

        }

    }    return ("", false);

}

// isLLVMObjdump accepts a string with path to an objdump binary,
// and returns a boolean indicating if the given binary is an LLVM
// objdump binary of an acceptable version.
private static bool isLLVMObjdump(@string output) {
    var fields = objdumpLLVMVerRE.FindStringSubmatch(output);
    if (len(fields) != 5) {
        return false;
    }
    if (fields[4] == "trunk") {
        return true;
    }
    var (verMajor, err) = strconv.Atoi(fields[1]);
    if (err != null) {
        return false;
    }
    var (verPatch, err) = strconv.Atoi(fields[3]);
    if (err != null) {
        return false;
    }
    if (runtime.GOOS == "linux" && verMajor >= 8) { 
        // Ensure LLVM objdump is at least version 8.0 on Linux.
        // Some flags, like --demangle, and double dashes for options are
        // not supported by previous versions.
        return true;

    }
    if (runtime.GOOS == "darwin") { 
        // Ensure LLVM objdump is at least version 10.0.1 on MacOS.
        return verMajor > 10 || (verMajor == 10 && verPatch >= 1);

    }
    return false;

}

// isBuObjdump accepts a string with path to an objdump binary,
// and returns a boolean indicating if the given binary is a GNU
// binutils objdump binary. No version check is performed.
private static bool isBuObjdump(@string output) {
    return strings.Contains(output, "GNU objdump");
}

// findExe looks for an executable command on a set of paths.
// If it cannot find it, returns cmd.
private static (@string, bool) findExe(@string cmd, slice<@string> paths) {
    @string _p0 = default;
    bool _p0 = default;

    foreach (var (_, p) in paths) {
        var cp = filepath.Join(p, cmd);
        {
            var (c, err) = exec.LookPath(cp);

            if (err == null) {
                return (c, true);
            }

        }

    }    return (cmd, false);

}

// Disasm returns the assembly instructions for the specified address range
// of a binary.
private static (slice<plugin.Inst>, error) Disasm(this ptr<Binutils> _addr_bu, @string file, ulong start, ulong end, bool intelSyntax) {
    slice<plugin.Inst> _p0 = default;
    error _p0 = default!;
    ref Binutils bu = ref _addr_bu.val;

    var b = bu.get();
    if (!b.objdumpFound) {
        return (null, error.As(errors.New("cannot disasm: no objdump tool available"))!);
    }
    @string args = new slice<@string>(new @string[] { "--disassemble", "--demangle", "--no-show-raw-insn", "--line-numbers", fmt.Sprintf("--start-address=%#x",start), fmt.Sprintf("--stop-address=%#x",end) });

    if (intelSyntax) {
        if (b.isLLVMObjdump) {
            args = append(args, "--x86-asm-syntax=intel");
        }
        else
 {
            args = append(args, "-M", "intel");
        }
    }
    args = append(args, file);
    var cmd = exec.Command(b.objdump, args);
    var (out, err) = cmd.Output();
    if (err != null) {
        return (null, error.As(fmt.Errorf("%v: %v", cmd.Args, err))!);
    }
    return disassemble(out);

}

// Open satisfies the plugin.ObjTool interface.
private static (plugin.ObjFile, error) Open(this ptr<Binutils> _addr_bu, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, _) => {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref Binutils bu = ref _addr_bu.val;

    var b = bu.get(); 

    // Make sure file is a supported executable.
    // This uses magic numbers, mainly to provide better error messages but
    // it should also help speed.

    {
        var (_, err) = os.Stat(name);

        if (err != null) { 
            // For testing, do not require file name to exist.
            if (strings.Contains(b.addr2line, "testdata/")) {
                return (addr(new fileAddr2Line(file:file{b:b,name:name})), error.As(null!)!);
            }

            return (null, error.As(err)!);

        }
    } 

    // Read the first 4 bytes of the file.

    var (f, err) = os.Open(name);
    if (err != null) {
        return (null, error.As(fmt.Errorf("error opening %s: %v", name, err))!);
    }
    defer(f.Close());

    array<byte> header = new array<byte>(4);
    _, err = io.ReadFull(f, header[..]);

    if (err != null) {
        return (null, error.As(fmt.Errorf("error reading magic number from %s: %v", name, err))!);
    }
    var elfMagic = string(header[..]); 

    // Match against supported file types.
    if (elfMagic == elf.ELFMAG) {
        (f, err) = b.openELF(name, start, limit, offset);
        if (err != null) {
            return (null, error.As(fmt.Errorf("error reading ELF file %s: %v", name, err))!);
        }
        return (f, error.As(null!)!);

    }
    var machoMagicLittle = binary.LittleEndian.Uint32(header[..]);
    var machoMagicBig = binary.BigEndian.Uint32(header[..]);

    if (machoMagicLittle == macho.Magic32 || machoMagicLittle == macho.Magic64 || machoMagicBig == macho.Magic32 || machoMagicBig == macho.Magic64) {
        (f, err) = b.openMachO(name, start, limit, offset);
        if (err != null) {
            return (null, error.As(fmt.Errorf("error reading Mach-O file %s: %v", name, err))!);
        }
        return (f, error.As(null!)!);

    }
    if (machoMagicLittle == macho.MagicFat || machoMagicBig == macho.MagicFat) {
        (f, err) = b.openFatMachO(name, start, limit, offset);
        if (err != null) {
            return (null, error.As(fmt.Errorf("error reading fat Mach-O file %s: %v", name, err))!);
        }
        return (f, error.As(null!)!);

    }
    var peMagic = string(header[..(int)2]);
    if (peMagic == "MZ") {
        (f, err) = b.openPE(name, start, limit, offset);
        if (err != null) {
            return (null, error.As(fmt.Errorf("error reading PE file %s: %v", name, err))!);
        }
        return (f, error.As(null!)!);

    }
    return (null, error.As(fmt.Errorf("unrecognized binary format: %s", name))!);

});

private static (plugin.ObjFile, error) openMachOCommon(this ptr<binrep> _addr_b, @string name, ptr<macho.File> _addr_of, ulong start, ulong limit, ulong offset) {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref binrep b = ref _addr_b.val;
    ref macho.File of = ref _addr_of.val;

    // Subtract the load address of the __TEXT section. Usually 0 for shared
    // libraries or 0x100000000 for executables. You can check this value by
    // running `objdump -private-headers <file>`.

    var textSegment = of.Segment("__TEXT");
    if (textSegment == null) {
        return (null, error.As(fmt.Errorf("could not identify base for %s: no __TEXT segment", name))!);
    }
    if (textSegment.Addr > start) {
        return (null, error.As(fmt.Errorf("could not identify base for %s: __TEXT segment address (0x%x) > mapping start address (0x%x)", name, textSegment.Addr, start))!);
    }
    var @base = start - textSegment.Addr;

    if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound)) {
        return (addr(new fileNM(file:file{b:b,name:name,base:base})), error.As(null!)!);
    }
    return (addr(new fileAddr2Line(file:file{b:b,name:name,base:base})), error.As(null!)!);

}

private static (plugin.ObjFile, error) openFatMachO(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, _) => {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref binrep b = ref _addr_b.val;

    var (of, err) = macho.OpenFat(name);
    if (err != null) {
        return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
    }
    defer(of.Close());

    if (len(of.Arches) == 0) {
        return (null, error.As(fmt.Errorf("empty fat Mach-O file: %s", name))!);
    }
    macho.Cpu arch = default; 
    // Use the host architecture.
    // TODO: This is not ideal because the host architecture may not be the one
    // that was profiled. E.g. an amd64 host can profile a 386 program.
    switch (runtime.GOARCH) {
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
    foreach (var (i) in of.Arches) {
        if (of.Arches[i].Cpu == arch) {
            return b.openMachOCommon(name, of.Arches[i].File, start, limit, offset);
        }
    }    return (null, error.As(fmt.Errorf("architecture not found in %s: %s", name, runtime.GOARCH))!);

});

private static (plugin.ObjFile, error) openMachO(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, _) => {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref binrep b = ref _addr_b.val;

    var (of, err) = macho.Open(name);
    if (err != null) {
        return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
    }
    defer(of.Close());

    return b.openMachOCommon(name, of, start, limit, offset);

});

private static (plugin.ObjFile, error) openELF(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, _) => {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref binrep b = ref _addr_b.val;

    var (ef, err) = elfOpen(name);
    if (err != null) {
        return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
    }
    defer(ef.Close());

    @string buildID = "";
    {
        var (f, err) = os.Open(name);

        if (err == null) {
            {
                var (id, err) = elfexec.GetBuildID(f);

                if (err == null) {
                    buildID = fmt.Sprintf("%x", id);
                }

            }

        }
    }


    ptr<ulong> stextOffset;    Func<ulong, bool> pageAligned = addr => addr % 4096 == 0;
    if (strings.Contains(name, "vmlinux") || !pageAligned(start) || !pageAligned(limit) || !pageAligned(offset)) { 
        // Reading all Symbols is expensive, and we only rarely need it so
        // we don't want to do it every time. But if _stext happens to be
        // page-aligned but isn't the same as Vaddr, we would symbolize
        // wrong. So if the name the addresses aren't page aligned, or if
        // the name is "vmlinux" we read _stext. We can be wrong if: (1)
        // someone passes a kernel path that doesn't contain "vmlinux" AND
        // (2) _stext is page-aligned AND (3) _stext is not at Vaddr
        var (symbols, err) = ef.Symbols();
        if (err != null && err != elf.ErrNoSymbols) {
            return (null, error.As(err)!);
        }
        foreach (var (_, s) in symbols) {
            if (s.Name == "_stext") { 
                // The kernel may use _stext as the mapping start address.
                stextOffset = _addr_s.Value;
                break;

            }

        }
    }
    {
        var (_, err) = elfexec.GetBase(_addr_ef.FileHeader, elfexec.FindTextProgHeader(ef), stextOffset, start, limit, offset);

        if (err != null) {
            return (null, error.As(fmt.Errorf("could not identify base for %s: %v", name, err))!);
        }
    }


    if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound)) {
        return (addr(new fileNM(file:file{b:b,name:name,buildID:buildID,m:&elfMapping{start:start,limit:limit,offset:offset,stextOffset:stextOffset},})), error.As(null!)!);
    }
    return (addr(new fileAddr2Line(file:file{b:b,name:name,buildID:buildID,m:&elfMapping{start:start,limit:limit,offset:offset,stextOffset:stextOffset},})), error.As(null!)!);

});

private static (plugin.ObjFile, error) openPE(this ptr<binrep> _addr_b, @string name, ulong start, ulong limit, ulong offset) => func((defer, _, _) => {
    plugin.ObjFile _p0 = default;
    error _p0 = default!;
    ref binrep b = ref _addr_b.val;

    var (pf, err) = pe.Open(name);
    if (err != null) {
        return (null, error.As(fmt.Errorf("error parsing %s: %v", name, err))!);
    }
    defer(pf.Close());

    ulong imageBase = default;
    switch (pf.OptionalHeader.type()) {
        case ptr<pe.OptionalHeader32> h:
            imageBase = uint64(h.ImageBase);
            break;
        case ptr<pe.OptionalHeader64> h:
            imageBase = uint64(h.ImageBase);
            break;
        default:
        {
            var h = pf.OptionalHeader.type();
            return (null, error.As(fmt.Errorf("unknown OptionalHeader %T", pf.OptionalHeader))!);
            break;
        }

    }

    ulong @base = default;
    if (start > 0) {
        base = start - imageBase;
    }
    if (b.fast || (!b.addr2lineFound && !b.llvmSymbolizerFound)) {
        return (addr(new fileNM(file:file{b:b,name:name,base:base})), error.As(null!)!);
    }
    return (addr(new fileAddr2Line(file:file{b:b,name:name,base:base})), error.As(null!)!);

});

// elfMapping stores the parameters of a runtime mapping that are needed to
// identify the ELF segment associated with a mapping.
private partial struct elfMapping {
    public ulong start; // Offset of _stext symbol. Only defined for kernel images, nil otherwise.
    public ulong limit; // Offset of _stext symbol. Only defined for kernel images, nil otherwise.
    public ulong offset; // Offset of _stext symbol. Only defined for kernel images, nil otherwise.
    public ptr<ulong> stextOffset;
}

// file implements the binutils.ObjFile interface.
private partial struct file {
    public ptr<binrep> b;
    public @string name;
    public @string buildID;
    public sync.Once baseOnce; // Ensures the base, baseErr and isData are computed once.
    public ulong @base;
    public error baseErr; // Any eventual error while computing the base.
    public bool isData; // Mapping information. Relevant only for ELF files, nil otherwise.
    public ptr<elfMapping> m;
}

// computeBase computes the relocation base for the given binary file only if
// the elfMapping field is set. It populates the base and isData fields and
// returns an error.
private static error computeBase(this ptr<file> _addr_f, ulong addr) => func((defer, _, _) => {
    ref file f = ref _addr_f.val;

    if (f == null || f.m == null) {
        return error.As(null!)!;
    }
    if (addr < f.m.start || addr >= f.m.limit) {
        return error.As(fmt.Errorf("specified address %x is outside the mapping range [%x, %x] for file %q", addr, f.m.start, f.m.limit, f.name))!;
    }
    var (ef, err) = elfOpen(f.name);
    if (err != null) {
        return error.As(fmt.Errorf("error parsing %s: %v", f.name, err))!;
    }
    defer(ef.Close());

    ptr<elf.ProgHeader> ph; 
    // For user space executables, find the actual program segment that is
    // associated with the given mapping. Skip this search if limit <= start.
    // We cannot use just a check on the start address of the mapping to tell if
    // it's a kernel / .ko module mapping, because with quipper address remapping
    // enabled, the address would be in the lower half of the address space.
    if (f.m.stextOffset == null && f.m.start < f.m.limit && f.m.limit < (uint64(1) << 63)) { 
        // Get all program headers associated with the mapping.
        var (headers, hasLoadables) = elfexec.ProgramHeadersForMapping(ef, f.m.offset, f.m.limit - f.m.start); 

        // Some ELF files don't contain any loadable program segments, e.g. .ko
        // kernel modules. It's not an error to have no header in such cases.
        if (hasLoadables) {
            ph, err = matchUniqueHeader(headers, addr - f.m.start + f.m.offset);
            if (err != null) {
                return error.As(fmt.Errorf("failed to find program header for file %q, ELF mapping %#v, address %x: %v", f.name, f.m.val, addr, err))!;
            }
        }
    }
    else
 { 
        // For the kernel, find the program segment that includes the .text section.
        ph = elfexec.FindTextProgHeader(ef);

    }
    var (base, err) = elfexec.GetBase(_addr_ef.FileHeader, ph, f.m.stextOffset, f.m.start, f.m.limit, f.m.offset);
    if (err != null) {
        return error.As(err)!;
    }
    f.@base = base;
    f.isData = ph != null && ph.Flags & elf.PF_X == 0;
    return error.As(null!)!;

});

// matchUniqueHeader attempts to identify a unique header from the given list,
// using the given file offset to disambiguate between multiple segments. It
// returns an error if the header list is empty or if it cannot identify a
// unique header.
private static (ptr<elf.ProgHeader>, error) matchUniqueHeader(slice<ptr<elf.ProgHeader>> headers, ulong fileOffset) {
    ptr<elf.ProgHeader> _p0 = default!;
    error _p0 = default!;

    if (len(headers) == 0) {
        return (_addr_null!, error.As(errors.New("no program header matches mapping info"))!);
    }
    if (len(headers) == 1) { 
        // Don't use the file offset if we already have a single header.
        return (_addr_headers[0]!, error.As(null!)!);

    }
    ptr<elf.ProgHeader> ph;
    foreach (var (_, h) in headers) {
        if (fileOffset >= h.Off && fileOffset < h.Off + h.Memsz) {
            if (ph != null) { 
                // Assuming no other bugs, this can only happen if we have two or
                // more small program segments that fit on the same page, and a
                // segment other than the last one includes uninitialized data.
                return (_addr_null!, error.As(fmt.Errorf("found second program header (%#v) that matches file offset %x, first program header is %#v. Does first program segment contain uninitialized data?", h.val, fileOffset, ph.val))!);

            }

            ph = h;

        }
    }    if (ph == null) {
        return (_addr_null!, error.As(fmt.Errorf("no program header matches file offset %x", fileOffset))!);
    }
    return (_addr_ph!, error.As(null!)!);

}

private static @string Name(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return f.name;
}

private static (ulong, error) ObjAddr(this ptr<file> _addr_f, ulong addr) {
    ulong _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;

    f.baseOnce.Do(() => {
        f.baseErr = f.computeBase(addr);
    });
    if (f.baseErr != null) {
        return (0, error.As(f.baseErr)!);
    }
    return (addr - f.@base, error.As(null!)!);

}

private static @string BuildID(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return f.buildID;
}

private static (slice<plugin.Frame>, error) SourceLine(this ptr<file> _addr_f, ulong addr) {
    slice<plugin.Frame> _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;

    f.baseOnce.Do(() => {
        f.baseErr = f.computeBase(addr);
    });
    if (f.baseErr != null) {
        return (null, error.As(f.baseErr)!);
    }
    return (null, error.As(null!)!);

}

private static error Close(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return error.As(null!)!;
}

private static (slice<ptr<plugin.Sym>>, error) Symbols(this ptr<file> _addr_f, ptr<regexp.Regexp> _addr_r, ulong addr) {
    slice<ptr<plugin.Sym>> _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;
    ref regexp.Regexp r = ref _addr_r.val;
 
    // Get from nm a list of symbols sorted by address.
    var cmd = exec.Command(f.b.nm, "-n", f.name);
    var (out, err) = cmd.Output();
    if (err != null) {
        return (null, error.As(fmt.Errorf("%v: %v", cmd.Args, err))!);
    }
    return findSymbols(out, f.name, r, addr);

}

// fileNM implements the binutils.ObjFile interface, using 'nm' to map
// addresses to symbols (without file/line number information). It is
// faster than fileAddr2Line.
private partial struct fileNM {
    public ref file file => ref file_val;
    public ptr<addr2LinerNM> addr2linernm;
}

private static (slice<plugin.Frame>, error) SourceLine(this ptr<fileNM> _addr_f, ulong addr) {
    slice<plugin.Frame> _p0 = default;
    error _p0 = default!;
    ref fileNM f = ref _addr_f.val;

    f.baseOnce.Do(() => {
        f.baseErr = f.computeBase(addr);
    });
    if (f.baseErr != null) {
        return (null, error.As(f.baseErr)!);
    }
    if (f.addr2linernm == null) {
        var (addr2liner, err) = newAddr2LinerNM(f.b.nm, f.name, f.@base);
        if (err != null) {
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
private partial struct fileAddr2Line {
    public sync.Once once;
    public ref file file => ref file_val;
    public ptr<addr2Liner> addr2liner;
    public ptr<llvmSymbolizer> llvmSymbolizer;
    public bool isData;
}

private static (slice<plugin.Frame>, error) SourceLine(this ptr<fileAddr2Line> _addr_f, ulong addr) {
    slice<plugin.Frame> _p0 = default;
    error _p0 = default!;
    ref fileAddr2Line f = ref _addr_f.val;

    f.baseOnce.Do(() => {
        f.baseErr = f.computeBase(addr);
    });
    if (f.baseErr != null) {
        return (null, error.As(f.baseErr)!);
    }
    f.once.Do(f.init);
    if (f.llvmSymbolizer != null) {
        return f.llvmSymbolizer.addrInfo(addr);
    }
    if (f.addr2liner != null) {
        return f.addr2liner.addrInfo(addr);
    }
    return (null, error.As(fmt.Errorf("could not find local addr2liner"))!);

}

private static void init(this ptr<fileAddr2Line> _addr_f) {
    ref fileAddr2Line f = ref _addr_f.val;

    {
        var (llvmSymbolizer, err) = newLLVMSymbolizer(f.b.llvmSymbolizer, f.name, f.@base, f.isData);

        if (err == null) {
            f.llvmSymbolizer = llvmSymbolizer;
            return ;
        }
    }


    {
        var (addr2liner, err) = newAddr2Liner(f.b.addr2line, f.name, f.@base);

        if (err == null) {
            f.addr2liner = addr2liner; 

            // When addr2line encounters some gcc compiled binaries, it
            // drops interesting parts of names in anonymous namespaces.
            // Fallback to NM for better function names.
            {
                var (nm, err) = newAddr2LinerNM(f.b.nm, f.name, f.@base);

                if (err == null) {
                    f.addr2liner.nm = nm;
                }

            }

        }
    }

}

private static error Close(this ptr<fileAddr2Line> _addr_f) {
    ref fileAddr2Line f = ref _addr_f.val;

    if (f.llvmSymbolizer != null) {
        f.llvmSymbolizer.rw.close();
        f.llvmSymbolizer = null;
    }
    if (f.addr2liner != null) {
        f.addr2liner.rw.close();
        f.addr2liner = null;
    }
    return error.As(null!)!;

}

} // end binutils_package
