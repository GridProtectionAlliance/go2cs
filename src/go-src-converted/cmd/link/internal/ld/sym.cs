// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/span.c
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

// package ld -- go2cs converted at 2020 August 29 10:04:35 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\sym.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using log = go.log_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static ref Link linknew(ref sys.Arch arch)
        {
            Link ctxt = ref new Link(Syms:sym.NewSymbols(),Out:&OutBuf{arch:arch},Arch:arch,LibraryByPkg:make(map[string]*sym.Library),);

            if (objabi.GOARCH != arch.Name)
            {
                log.Fatalf("invalid objabi.GOARCH %s (want %s)", objabi.GOARCH, arch.Name);
            }
            AtExit(() =>
            {
                if (nerrors > 0L && ctxt.Out.f != null)
                {
                    ctxt.Out.f.Close();
                    mayberemoveoutfile();
                }
            });

            return ctxt;
        }

        // computeTLSOffset records the thread-local storage offset.
        private static void computeTLSOffset(this ref Link ctxt)
        {

            if (ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hwindows) 
                break;

                /*
                         * ELF uses TLS offset negative from FS.
                         * Translate 0(FS) and 8(FS) into -16(FS) and -8(FS).
                         * Known to low-level assembly in package runtime and runtime/cgo.
                         */
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris) 
                if (objabi.GOOS == "android")
                {

                    if (ctxt.Arch.Family == sys.AMD64) 
                        // Android/amd64 constant - offset from 0(FS) to our TLS slot.
                        // Explained in src/runtime/cgo/gcc_android_*.c
                        ctxt.Tlsoffset = 0x1d0UL;
                    else if (ctxt.Arch.Family == sys.I386) 
                        // Android/386 constant - offset from 0(GS) to our TLS slot.
                        ctxt.Tlsoffset = 0xf8UL;
                    else 
                        ctxt.Tlsoffset = -1L * ctxt.Arch.PtrSize;
                                    }
                else
                {
                    ctxt.Tlsoffset = -1L * ctxt.Arch.PtrSize;
                }
            else if (ctxt.HeadType == objabi.Hnacl) 

                if (ctxt.Arch.Family == sys.ARM) 
                    ctxt.Tlsoffset = 0L;
                else if (ctxt.Arch.Family == sys.AMD64) 
                    ctxt.Tlsoffset = 0L;
                else if (ctxt.Arch.Family == sys.I386) 
                    ctxt.Tlsoffset = -8L;
                else 
                    log.Fatalf("unknown thread-local storage offset for nacl/%s", ctxt.Arch.Name);
                /*
                         * OS X system constants - offset from 0(GS) to our TLS.
                         * Explained in src/runtime/cgo/gcc_darwin_*.c.
                         */
            else if (ctxt.HeadType == objabi.Hdarwin) 

                if (ctxt.Arch.Family == sys.ARM) 
                    ctxt.Tlsoffset = 0L; // dummy value, not needed
                else if (ctxt.Arch.Family == sys.AMD64) 
                    ctxt.Tlsoffset = 0x8a0UL;
                else if (ctxt.Arch.Family == sys.ARM64) 
                    ctxt.Tlsoffset = 0L; // dummy value, not needed
                else if (ctxt.Arch.Family == sys.I386) 
                    ctxt.Tlsoffset = 0x468UL;
                else 
                    log.Fatalf("unknown thread-local storage offset for darwin/%s", ctxt.Arch.Name);
                            else 
                log.Fatalf("unknown thread-local storage offset for %v", ctxt.HeadType);
                    }
    }
}}}}
