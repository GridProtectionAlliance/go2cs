// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/l.h
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

// package ld -- go2cs converted at 2020 August 29 10:04:10 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\link.go
using bufio = go.bufio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
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
            public map<ref sym.Symbol, ulong> gcdataAddresses;
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
            public ptr<sym.Symbol> Tlsg;
            public slice<@string> Libdir;
            public slice<ref sym.Library> Library;
            public map<@string, ref sym.Library> LibraryByPkg;
            public slice<Shlib> Shlibs;
            public long Tlsoffset;
            public slice<ref sym.Symbol> Textp;
            public slice<ref sym.Symbol> Filesyms;
            public ptr<sym.Symbol> Moduledata;
            public map<@string, @string> PackageFile;
            public map<@string, @string> PackageShlib;
            public slice<ref sym.Symbol> tramps; // trampolines
        }

        // The smallest possible offset from the hardware stack pointer to a local
        // variable on the stack. Architectures that use a link register save its value
        // on the stack in the function prologue and so always have a pointer between
        // the hardware stack pointer and the local variable area.
        private static long FixedFrameSize(this ref Link ctxt)
        {

            if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
                return 0L;
            else if (ctxt.Arch.Family == sys.PPC64) 
                // PIC code on ppc64le requires 32 bytes of stack, and it's easier to
                // just use that much stack always on ppc64x.
                return int64(4L * ctxt.Arch.PtrSize);
            else 
                return int64(ctxt.Arch.PtrSize);
                    }

        private static void Logf(this ref Link ctxt, @string format, params object[] args)
        {
            fmt.Fprintf(ctxt.Bso, format, args);
            ctxt.Bso.Flush();
        }

        private static void addImports(ref Link ctxt, ref sym.Library l, @string pn)
        {
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

        public partial struct Pciter
        {
            public sym.Pcdata d;
            public slice<byte> p;
            public uint pc;
            public uint nextpc;
            public uint pcscale;
            public int value;
            public long start;
            public long done;
        }
    }
}}}}
