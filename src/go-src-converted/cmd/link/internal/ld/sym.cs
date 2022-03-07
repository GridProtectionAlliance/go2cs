// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/span.c
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

// package ld -- go2cs converted at 2022 March 06 23:22:19 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\sym.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using buildcfg = go.@internal.buildcfg_package;
using log = go.log_package;
using runtime = go.runtime_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

private static ptr<Link> linknew(ptr<sys.Arch> _addr_arch) {
    ref sys.Arch arch = ref _addr_arch.val;

    loader.ErrorReporter ler = new loader.ErrorReporter(AfterErrorAction:afterErrorAction);
    ptr<Link> ctxt = addr(new Link(Target:Target{Arch:arch},version:sym.SymVerStatic,outSem:make(chanint,2*runtime.GOMAXPROCS(0)),Out:NewOutBuf(arch),LibraryByPkg:make(map[string]*sym.Library),numelfsym:1,ErrorReporter:ErrorReporter{ErrorReporter:ler},generatorSyms:make(map[loader.Sym]generatorFunc),));

    if (buildcfg.GOARCH != arch.Name) {
        log.Fatalf("invalid buildcfg.GOARCH %s (want %s)", buildcfg.GOARCH, arch.Name);
    }
    AtExit(() => {
        if (nerrors > 0) {
            ctxt.Out.Close();
            mayberemoveoutfile();
        }
    });

    return _addr_ctxt!;

}

// computeTLSOffset records the thread-local storage offset.
// Not used for Android where the TLS offset is determined at runtime.
private static void computeTLSOffset(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;


    if (ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hwindows || ctxt.HeadType == objabi.Hjs || ctxt.HeadType == objabi.Haix) 
        break;
    else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris) 
        /*
                 * ELF uses TLS offset negative from FS.
                 * Translate 0(FS) and 8(FS) into -16(FS) and -8(FS).
                 * Known to low-level assembly in package runtime and runtime/cgo.
                 */
        ctxt.Tlsoffset = -1 * ctxt.Arch.PtrSize;
    else if (ctxt.HeadType == objabi.Hdarwin) 
        /*
                 * OS X system constants - offset from 0(GS) to our TLS.
                 */

        if (ctxt.Arch.Family == sys.AMD64) 
            ctxt.Tlsoffset = 0x30;
        else if (ctxt.Arch.Family == sys.ARM64) 
            ctxt.Tlsoffset = 0; // dummy value, not needed
        else 
            log.Fatalf("unknown thread-local storage offset for darwin/%s", ctxt.Arch.Name);

            /*
                         * For x86, Apple has reserved a slot in the TLS for Go. See issue 23617.
                         * That slot is at offset 0x30 on amd64.
                         * The slot will hold the G pointer.
                         * These constants should match those in runtime/sys_darwin_amd64.s
                         * and runtime/cgo/gcc_darwin_amd64.c.
                         */
            else 
        log.Fatalf("unknown thread-local storage offset for %v", ctxt.HeadType);
    
}

} // end ld_package
