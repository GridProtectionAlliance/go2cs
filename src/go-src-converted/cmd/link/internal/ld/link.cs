// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package ld -- go2cs converted at 2022 March 06 23:21:48 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\link.go
using bufio = go.bufio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

public partial struct Shlib {
    public @string Path;
    public slice<byte> Hash;
    public slice<@string> Deps;
    public ptr<elf.File> File;
}

// Link holds the context for writing object code from a compiler
// or for reading that input into the linker.
public partial struct Link {
    public ref Target Target => ref Target_val;
    public ref ErrorReporter ErrorReporter => ref ErrorReporter_val;
    public ref ArchSyms ArchSyms => ref ArchSyms_val;
    public channel<nint> outSem; // limits the number of output writers
    public ptr<OutBuf> Out;
    public nint version; // current version number for static/file-local symbols

    public nint Debugvlog;
    public ptr<bufio.Writer> Bso;
    public bool Loaded; // set after all inputs have been loaded as symbols

    public bool compressDWARF;
    public slice<@string> Libdir;
    public slice<ptr<sym.Library>> Library;
    public map<@string, ptr<sym.Library>> LibraryByPkg;
    public slice<Shlib> Shlibs;
    public slice<loader.Sym> Textp;
    public loader.Sym Moduledata;
    public map<@string, @string> PackageFile;
    public map<@string, @string> PackageShlib;
    public slice<loader.Sym> tramps; // trampolines

    public slice<ptr<sym.CompilationUnit>> compUnits; // DWARF compilation units
    public ptr<sym.CompilationUnit> runtimeCU; // One of the runtime CUs, the last one seen.

    public ptr<loader.Loader> loader;
    public slice<cgodata> cgodata; // cgo directives to load, three strings are args for loadcgo

    public slice<loader.Sym> datap;
    public slice<loader.Sym> dynexp; // Elf symtab variables.
    public nint numelfsym; // starts at 0, 1 is reserved

// These are symbols that created and written by the linker.
// Rather than creating a symbol, and writing all its data into the heap,
// you can create a symbol, and just a generation function will be called
// after the symbol's been created in the output mmap.
    public map<loader.Sym, generatorFunc> generatorSyms;
}

private partial struct cgodata {
    public @string file;
    public @string pkg;
    public slice<slice<@string>> directives;
}

// The smallest possible offset from the hardware stack pointer to a local
// variable on the stack. Architectures that use a link register save its value
// on the stack in the function prologue and so always have a pointer between
// the hardware stack pointer and the local variable area.
private static long FixedFrameSize(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;


    if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
        return 0;
    else if (ctxt.Arch.Family == sys.PPC64) 
        // PIC code on ppc64le requires 32 bytes of stack, and it's easier to
        // just use that much stack always on ppc64x.
        return int64(4 * ctxt.Arch.PtrSize);
    else 
        return int64(ctxt.Arch.PtrSize);
    
}

private static void Logf(this ptr<Link> _addr_ctxt, @string format, params object[] args) {
    args = args.Clone();
    ref Link ctxt = ref _addr_ctxt.val;

    fmt.Fprintf(ctxt.Bso, format, args);
    ctxt.Bso.Flush();
}

private static void addImports(ptr<Link> _addr_ctxt, ptr<sym.Library> _addr_l, @string pn) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref sym.Library l = ref _addr_l.val;

    var pkg = objabi.PathToPrefix(l.Pkg);
    foreach (var (_, imp) in l.Autolib) {
        var lib = addlib(ctxt, pkg, pn, imp.Pkg, imp.Fingerprint);
        if (lib != null) {
            l.Imports = append(l.Imports, lib);
        }
    }    l.Autolib = null;

}

// Allocate a new version (i.e. symbol namespace).
private static nint IncVersion(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.version++;
    return ctxt.version - 1;
}

// returns the maximum version number
private static nint MaxVersion(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return ctxt.version;
}

// generatorFunc is a convenience type.
// Linker created symbols that are large, and shouldn't really live in the
// heap can define a generator function, and their bytes can be generated
// directly in the output mmap.
//
// Generator symbols shouldn't grow the symbol size, and might be called in
// parallel in the future.
//
// Generator Symbols have their Data set to the mmapped area when the
// generator is called.
public delegate void generatorFunc(ptr<Link>, loader.Sym);

// createGeneratorSymbol is a convenience method for creating a generator
// symbol.
private static loader.Sym createGeneratorSymbol(this ptr<Link> _addr_ctxt, @string name, nint version, sym.SymKind t, long size, generatorFunc gen) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var s = ldr.LookupOrCreateSym(name, version);
    ldr.SetIsGeneratedSym(s, true);
    var sb = ldr.MakeSymbolUpdater(s);
    sb.SetType(t);
    sb.SetSize(size);
    ctxt.generatorSyms[s] = gen;
    return s;
}

} // end ld_package
