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

// package ld -- go2cs converted at 2020 October 09 05:52:13 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\link.go
using bufio = go.bufio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        public partial struct Shlib
        {
            public @string Path;
            public slice<byte> Hash;
            public slice<@string> Deps;
            public ptr<elf.File> File;
            public map<ptr<sym.Symbol>, ulong> gcdataAddresses;
        }

        // Link holds the context for writing object code from a compiler
        // or for reading that input into the linker.
        public partial struct Link
        {
            public ptr<OutBuf> Out;
            public ptr<sym.Symbols> Syms;
            public ptr<sys.Arch> Arch;
            public long Debugvlog;
            public ptr<bufio.Writer> Bso;
            public bool Loaded; // set after all inputs have been loaded as symbols

            public bool IsELF;
            public objabi.HeadType HeadType;
            public bool linkShared; // link against installed Go shared libraries
            public LinkMode LinkMode;
            public BuildMode BuildMode;
            public bool canUsePlugins; // initialized when Loaded is set to true
            public bool compressDWARF;
            public ptr<sym.Symbol> Tlsg;
            public slice<@string> Libdir;
            public slice<ptr<sym.Library>> Library;
            public map<@string, ptr<sym.Library>> LibraryByPkg;
            public slice<Shlib> Shlibs;
            public long Tlsoffset;
            public slice<ptr<sym.Symbol>> Textp;
            public slice<ptr<sym.Symbol>> Filesyms;
            public ptr<sym.Symbol> Moduledata;
            public map<@string, @string> PackageFile;
            public map<@string, @string> PackageShlib;
            public slice<ptr<sym.Symbol>> tramps; // trampolines

// unresolvedSymSet is a set of erroneous unresolved references.
// Used to avoid duplicated error messages.
            public map<unresolvedSymKey, bool> unresolvedSymSet; // Used to implement field tracking.
            public map<ptr<sym.Symbol>, ptr<sym.Symbol>> Reachparent;
            public slice<ptr<sym.CompilationUnit>> compUnits; // DWARF compilation units
            public ptr<sym.CompilationUnit> runtimeCU; // One of the runtime CUs, the last one seen.

            public slice<byte> relocbuf; // temporary buffer for applying relocations

            public ptr<loader.Loader> loader;
            public slice<cgodata> cgodata; // cgo directives to load, three strings are args for loadcgo

            public map<@string, bool> cgo_export_static;
            public map<@string, bool> cgo_export_dynamic;
        }

        private partial struct cgodata
        {
            public @string file;
            public @string pkg;
            public slice<slice<@string>> directives;
        }

        private partial struct unresolvedSymKey
        {
            public ptr<sym.Symbol> from; // Symbol that referenced unresolved "to"
            public ptr<sym.Symbol> to; // Unresolved symbol referenced by "from"
        }

        // ErrorUnresolved prints unresolved symbol error for r.Sym that is referenced from s.
        private static void ErrorUnresolved(this ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            if (ctxt.unresolvedSymSet == null)
            {
                ctxt.unresolvedSymSet = make_map<unresolvedSymKey, bool>();
            }

            unresolvedSymKey k = new unresolvedSymKey(from:s,to:r.Sym);
            if (!ctxt.unresolvedSymSet[k])
            {
                ctxt.unresolvedSymSet[k] = true; 

                // Try to find symbol under another ABI.
                obj.ABI reqABI = default;                obj.ABI haveABI = default;

                haveABI = ~obj.ABI(0L);
                var (reqABI, ok) = sym.VersionToABI(int(r.Sym.Version));
                if (ok)
                {
                    for (var abi = obj.ABI(0L); abi < obj.ABICount; abi++)
                    {
                        var v = sym.ABIToVersion(abi);
                        if (v == -1L)
                        {
                            continue;
                        }

                        {
                            var rs = ctxt.Syms.ROLookup(r.Sym.Name, v);

                            if (rs != null && rs.Type != sym.Sxxx && rs.Type != sym.SXREF)
                            {
                                haveABI = abi;
                            }

                        }

                    }


                } 

                // Give a special error message for main symbol (see #24809).
                if (r.Sym.Name == "main.main")
                {
                    Errorf(s, "function main is undeclared in the main package");
                }
                else if (haveABI != ~obj.ABI(0L))
                {
                    Errorf(s, "relocation target %s not defined for %s (but is defined for %s)", r.Sym.Name, reqABI, haveABI);
                }
                else
                {
                    Errorf(s, "relocation target %s not defined", r.Sym.Name);
                }

            }

        }

        // The smallest possible offset from the hardware stack pointer to a local
        // variable on the stack. Architectures that use a link register save its value
        // on the stack in the function prologue and so always have a pointer between
        // the hardware stack pointer and the local variable area.
        private static long FixedFrameSize(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
                return 0L;
            else if (ctxt.Arch.Family == sys.PPC64) 
                // PIC code on ppc64le requires 32 bytes of stack, and it's easier to
                // just use that much stack always on ppc64x.
                return int64(4L * ctxt.Arch.PtrSize);
            else 
                return int64(ctxt.Arch.PtrSize);
            
        }

        private static void Logf(this ptr<Link> _addr_ctxt, @string format, params object[] args)
        {
            args = args.Clone();
            ref Link ctxt = ref _addr_ctxt.val;

            fmt.Fprintf(ctxt.Bso, format, args);
            ctxt.Bso.Flush();
        }

        private static void addImports(ptr<Link> _addr_ctxt, ptr<sym.Library> _addr_l, @string pn)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Library l = ref _addr_l.val;

            var pkg = objabi.PathToPrefix(l.Pkg);
            foreach (var (_, importStr) in l.ImportStrings)
            {
                var lib = addlib(ctxt, pkg, pn, importStr);
                if (lib != null)
                {
                    l.Imports = append(l.Imports, lib);
                }

            }
            l.ImportStrings = null;

        }
    }
}}}}
