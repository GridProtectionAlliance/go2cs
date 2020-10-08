// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:41:50 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\pcln.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private static int ftabaddstring(ptr<sym.Symbol> _addr_ftab, @string s)
        {
            ref sym.Symbol ftab = ref _addr_ftab.val;

            var start = len(ftab.P);
            ftab.Grow(int64(start + len(s) + 1L)); // make room for s plus trailing NUL
            copy(ftab.P[start..], s);
            return int32(start);

        }

        // numberfile assigns a file number to the file if it hasn't been assigned already.
        private static void numberfile(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_file)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol file = ref _addr_file.val;

            if (file.Type != sym.SFILEPATH)
            {
                ctxt.Filesyms = append(ctxt.Filesyms, file);
                file.Value = int64(len(ctxt.Filesyms));
                file.Type = sym.SFILEPATH;
                var path = file.Name[len(src.FileSymPrefix)..];
                file.Name = expandGoroot(path);
            }

        }

        private static void renumberfiles(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> files, ptr<sym.Pcdata> _addr_d)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Pcdata d = ref _addr_d.val;
 
            // Give files numbers.
            foreach (var (_, f) in files)
            {
                numberfile(_addr_ctxt, _addr_f);
            }
            var buf = make_slice<byte>(binary.MaxVarintLen32);
            var newval = int32(-1L);
            sym.Pcdata @out = default;
            var it = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
            it.Init(d.P);

            while (!it.Done)
            { 
                // value delta
                var oldval = it.Value;

                int val = default;
                if (oldval == -1L)
                {
                    val = -1L;
                it.Next();
                }
                else
                {
                    if (oldval < 0L || oldval >= int32(len(files)))
                    {
                        log.Fatalf("bad pcdata %d", oldval);
                    }

                    val = int32(files[oldval].Value);

                }

                var dv = val - newval;
                newval = val; 

                // value
                var n = binary.PutVarint(buf, int64(dv));
                @out.P = append(@out.P, buf[..n]); 

                // pc delta
                var pc = (it.NextPC - it.PC) / it.PCScale;
                n = binary.PutUvarint(buf, uint64(pc));
                @out.P = append(@out.P, buf[..n]);

            } 

            // terminating value delta
            // we want to write varint-encoded 0, which is just 0
 

            // terminating value delta
            // we want to write varint-encoded 0, which is just 0
            @out.P = append(@out.P, 0L);

            d = out;

        }

        // onlycsymbol reports whether this is a symbol that is referenced by C code.
        private static bool onlycsymbol(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            switch (s.Name)
            {
                case "_cgo_topofstack": 

                case "__cgo_topofstack": 

                case "_cgo_panic": 

                case "crosscall2": 
                    return true;
                    break;
            }
            if (strings.HasPrefix(s.Name, "_cgoexp_"))
            {
                return true;
            }

            return false;

        }

        private static bool emitPcln(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s == null)
            {
                return true;
            }

            if (ctxt.BuildMode == BuildModePlugin && ctxt.HeadType == objabi.Hdarwin && onlycsymbol(_addr_s))
            {
                return false;
            } 
            // We want to generate func table entries only for the "lowest level" symbols,
            // not containers of subsymbols.
            return !s.Attr.Container();

        }

        // pclntab initializes the pclntab symbol with
        // runtime function and file name information.

        private static sym.FuncInfo pclntabZpcln = default;

        // These variables are used to initialize runtime.firstmoduledata, see symtab.go:symtab.
        private static int pclntabNfunc = default;
        private static int pclntabFiletabOffset = default;
        private static int pclntabPclntabOffset = default;
        private static ptr<sym.Symbol> pclntabFirstFunc;
        private static ptr<sym.Symbol> pclntabLastFunc;

        private static void pclntab(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var funcdataBytes = int64(0L);
            var ftab = ctxt.Syms.Lookup("runtime.pclntab", 0L);
            ftab.Type = sym.SPCLNTAB;
            ftab.Attr |= sym.AttrReachable; 

            // See golang.org/s/go12symtab for the format. Briefly:
            //    8-byte header
            //    nfunc [thearch.ptrsize bytes]
            //    function table, alternating PC and offset to func struct [each entry thearch.ptrsize bytes]
            //    end PC [thearch.ptrsize bytes]
            //    offset to file table [4 bytes]

            // Find container symbols and mark them as such.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (s.Outer != null)
                    {
                        s.Outer.Attr |= sym.AttrContainer;
                    }

                } 

                // Gather some basic stats and info.

                s = s__prev1;
            }

            int nfunc = default;
            var prevSect = ctxt.Textp[0L].Sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (!emitPcln(_addr_ctxt, _addr_s))
                    {
                        continue;
                    }

                    nfunc++;
                    if (pclntabFirstFunc == null)
                    {
                        pclntabFirstFunc = s;
                    }

                    if (s.Sect != prevSect)
                    { 
                        // With multiple text sections, the external linker may insert functions
                        // between the sections, which are not known by Go. This leaves holes in
                        // the PC range covered by the func table. We need to generate an entry
                        // to mark the hole.
                        nfunc++;
                        prevSect = s.Sect;

                    }

                }

                s = s__prev1;
            }

            pclntabNfunc = nfunc;
            ftab.Grow(8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize) + 4L);
            ftab.SetUint32(ctxt.Arch, 0L, 0xfffffffbUL);
            ftab.SetUint8(ctxt.Arch, 6L, uint8(ctxt.Arch.MinLC));
            ftab.SetUint8(ctxt.Arch, 7L, uint8(ctxt.Arch.PtrSize));
            ftab.SetUint(ctxt.Arch, 8L, uint64(nfunc));
            pclntabPclntabOffset = int32(8L + ctxt.Arch.PtrSize);

            var funcnameoff = make_map<@string, int>();
            Func<@string, int> nameToOffset = name =>
            {
                var (nameoff, ok) = funcnameoff[name];
                if (!ok)
                {
                    nameoff = ftabaddstring(_addr_ftab, name);
                    funcnameoff[name] = nameoff;
                }

                return nameoff;

            }
;

            var pctaboff = make_map<@string, uint>();
            Func<int, slice<byte>, int> writepctab = (off, p) =>
            {
                var (start, ok) = pctaboff[string(p)];
                if (!ok)
                {
                    if (len(p) > 0L)
                    {
                        start = uint32(len(ftab.P));
                        ftab.AddBytes(p);
                    }

                    pctaboff[string(p)] = start;

                }

                var newoff = int32(ftab.SetUint32(ctxt.Arch, int64(off), start));
                return newoff;

            }
;

            nfunc = 0L; // repurpose nfunc as a running index
            var prevFunc = ctxt.Textp[0L];
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (!emitPcln(_addr_ctxt, _addr_s))
                    {
                        continue;
                    }

                    if (s.Sect != prevFunc.Sect)
                    { 
                        // With multiple text sections, there may be a hole here in the address
                        // space (see the comment above). We use an invalid funcoff value to
                        // mark the hole.
                        // See also runtime/symtab.go:findfunc
                        ftab.SetAddrPlus(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), prevFunc, prevFunc.Size);
                        ftab.SetUint(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), ~uint64(0L));
                        nfunc++;

                    }

                    prevFunc = s;

                    var pcln = s.FuncInfo;
                    if (pcln == null)
                    {
                        pcln = _addr_pclntabZpcln;
                    }

                    if (len(pcln.InlTree) > 0L)
                    {
                        if (len(pcln.Pcdata) <= objabi.PCDATA_InlTreeIndex)
                        { 
                            // Create inlining pcdata table.
                            var pcdata = make_slice<sym.Pcdata>(objabi.PCDATA_InlTreeIndex + 1L);
                            copy(pcdata, pcln.Pcdata);
                            pcln.Pcdata = pcdata;

                        }

                        if (len(pcln.Funcdataoff) <= objabi.FUNCDATA_InlTree)
                        { 
                            // Create inline tree funcdata.
                            var funcdata = make_slice<ptr<sym.Symbol>>(objabi.FUNCDATA_InlTree + 1L);
                            var funcdataoff = make_slice<long>(objabi.FUNCDATA_InlTree + 1L);
                            copy(funcdata, pcln.Funcdata);
                            copy(funcdataoff, pcln.Funcdataoff);
                            pcln.Funcdata = funcdata;
                            pcln.Funcdataoff = funcdataoff;

                        }

                    }

                    var funcstart = int32(len(ftab.P));
                    funcstart += int32(-len(ftab.P)) & (int32(ctxt.Arch.PtrSize) - 1L); // align to ptrsize

                    ftab.SetAddr(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), s);
                    ftab.SetUint(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), uint64(funcstart)); 

                    // Write runtime._func. Keep in sync with ../../../../runtime/runtime2.go:/_func
                    // and package debug/gosym.

                    // fixed size of struct, checked below
                    var off = funcstart;

                    var end = funcstart + int32(ctxt.Arch.PtrSize) + 3L * 4L + 5L * 4L + int32(len(pcln.Pcdata)) * 4L + int32(len(pcln.Funcdata)) * int32(ctxt.Arch.PtrSize);
                    if (len(pcln.Funcdata) > 0L && (end & int32(ctxt.Arch.PtrSize - 1L) != 0L))
                    {
                        end += 4L;
                    }

                    ftab.Grow(int64(end)); 

                    // entry uintptr
                    off = int32(ftab.SetAddr(ctxt.Arch, int64(off), s)); 

                    // name int32
                    var nameoff = nameToOffset(s.Name);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(nameoff))); 

                    // args int32
                    // TODO: Move into funcinfo.
                    var args = uint32(0L);
                    if (s.FuncInfo != null)
                    {
                        args = uint32(s.FuncInfo.Args);
                    }

                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), args)); 

                    // deferreturn
                    var deferreturn = uint32(0L);
                    var lastWasmAddr = uint32(0L);
                    foreach (var (_, r) in s.R)
                    {
                        if (ctxt.Arch.Family == sys.Wasm && r.Type == objabi.R_ADDR)
                        { 
                            // Wasm does not have a live variable set at the deferreturn
                            // call itself. Instead it has one identified by the
                            // resumption point immediately preceding the deferreturn.
                            // The wasm code has a R_ADDR relocation which is used to
                            // set the resumption point to PC_B.
                            lastWasmAddr = uint32(r.Add);

                        }

                        if (r.Type.IsDirectCall() && r.Sym != null && (r.Sym.Name == "runtime.deferreturn" || r.Sym.Attr.DeferReturnTramp()))
                        {
                            if (ctxt.Arch.Family == sys.Wasm)
                            {
                                deferreturn = lastWasmAddr - 1L;
                            }
                            else
                            { 
                                // Note: the relocation target is in the call instruction, but
                                // is not necessarily the whole instruction (for instance, on
                                // x86 the relocation applies to bytes [1:5] of the 5 byte call
                                // instruction).
                                deferreturn = uint32(r.Off);

                                if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
                                    deferreturn--;
                                else if (ctxt.Arch.Family == sys.PPC64 || ctxt.Arch.Family == sys.ARM || ctxt.Arch.Family == sys.ARM64 || ctxt.Arch.Family == sys.MIPS || ctxt.Arch.Family == sys.MIPS64)                                 else if (ctxt.Arch.Family == sys.RISCV64) 
                                    // TODO(jsing): The JALR instruction is marked with
                                    // R_CALLRISCV, whereas the actual reloc is currently
                                    // one instruction earlier starting with the AUIPC.
                                    deferreturn -= 4L;
                                else if (ctxt.Arch.Family == sys.S390X) 
                                    deferreturn -= 2L;
                                else 
                                    panic(fmt.Sprint("Unhandled architecture:", ctxt.Arch.Family));
                                
                            }

                            break; // only need one
                        }

                    }
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), deferreturn));

                    if (pcln != _addr_pclntabZpcln)
                    {
                        renumberfiles(_addr_ctxt, pcln.File, _addr_pcln.Pcfile);
                        if (false)
                        { 
                            // Sanity check the new numbering
                            var it = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
                            it.Init(pcln.Pcfile.P);

                            while (!it.Done)
                            {
                                if (it.Value < 1L || it.Value > int32(len(ctxt.Filesyms)))
                                {
                                    Errorf(s, "bad file number in pcfile: %d not in range [1, %d]\n", it.Value, len(ctxt.Filesyms));
                                    errorexit();
                                it.Next();
                                }

                            }


                        }

                    }

                    if (len(pcln.InlTree) > 0L)
                    {
                        var inlTreeSym = ctxt.Syms.Lookup("inltree." + s.Name, 0L);
                        inlTreeSym.Type = sym.SRODATA;
                        inlTreeSym.Attr |= sym.AttrReachable | sym.AttrDuplicateOK;

                        {
                            var i__prev2 = i;

                            foreach (var (__i, __call) in pcln.InlTree)
                            {
                                i = __i;
                                call = __call; 
                                // Usually, call.File is already numbered since the file
                                // shows up in the Pcfile table. However, two inlined calls
                                // might overlap exactly so that only the innermost file
                                // appears in the Pcfile table. In that case, this assigns
                                // the outer file a number.
                                numberfile(_addr_ctxt, _addr_call.File);
                                nameoff = nameToOffset(call.Func);

                                inlTreeSym.SetUint16(ctxt.Arch, int64(i * 20L + 0L), uint16(call.Parent));
                                inlTreeSym.SetUint8(ctxt.Arch, int64(i * 20L + 2L), uint8(objabi.GetFuncID(call.Func, ""))); 
                                // byte 3 is unused
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 20L + 4L), uint32(call.File.Value));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 20L + 8L), uint32(call.Line));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 20L + 12L), uint32(nameoff));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 20L + 16L), uint32(call.ParentPC));

                            }

                            i = i__prev2;
                        }

                        pcln.Funcdata[objabi.FUNCDATA_InlTree] = inlTreeSym;
                        pcln.Pcdata[objabi.PCDATA_InlTreeIndex] = pcln.Pcinline;

                    } 

                    // pcdata
                    off = writepctab(off, pcln.Pcsp.P);
                    off = writepctab(off, pcln.Pcfile.P);
                    off = writepctab(off, pcln.Pcline.P);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(len(pcln.Pcdata)))); 

                    // funcID uint8
                    @string file = default;
                    if (s.FuncInfo != null && len(s.FuncInfo.File) > 0L)
                    {
                        file = s.FuncInfo.File[0L].Name;
                    }

                    var funcID = objabi.GetFuncID(s.Name, file);

                    off = int32(ftab.SetUint8(ctxt.Arch, int64(off), uint8(funcID))); 

                    // unused
                    off += 2L; 

                    // nfuncdata must be the final entry.
                    off = int32(ftab.SetUint8(ctxt.Arch, int64(off), uint8(len(pcln.Funcdata))));
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in pcln.Pcdata)
                        {
                            i = __i;
                            off = writepctab(off, pcln.Pcdata[i].P);
                        } 

                        // funcdata, must be pointer-aligned and we're only int32-aligned.
                        // Missing funcdata will be 0 (nil pointer).

                        i = i__prev2;
                    }

                    if (len(pcln.Funcdata) > 0L)
                    {
                        if (off & int32(ctxt.Arch.PtrSize - 1L) != 0L)
                        {
                            off += 4L;
                        }

                        {
                            var i__prev2 = i;

                            foreach (var (__i) in pcln.Funcdata)
                            {
                                i = __i;
                                var dataoff = int64(off) + int64(ctxt.Arch.PtrSize) * int64(i);
                                if (pcln.Funcdata[i] == null)
                                {
                                    ftab.SetUint(ctxt.Arch, dataoff, uint64(pcln.Funcdataoff[i]));
                                    continue;
                                } 
                                // TODO: Dedup.
                                funcdataBytes += pcln.Funcdata[i].Size;
                                ftab.SetAddrPlus(ctxt.Arch, dataoff, pcln.Funcdata[i], pcln.Funcdataoff[i]);

                            }

                            i = i__prev2;
                        }

                        off += int32(len(pcln.Funcdata)) * int32(ctxt.Arch.PtrSize);

                    }

                    if (off != end)
                    {
                        Errorf(s, "bad math in functab: funcstart=%d off=%d but end=%d (npcdata=%d nfuncdata=%d ptrsize=%d)", funcstart, off, end, len(pcln.Pcdata), len(pcln.Funcdata), ctxt.Arch.PtrSize);
                        errorexit();
                    }

                    nfunc++;

                }

                s = s__prev1;
            }

            var last = ctxt.Textp[len(ctxt.Textp) - 1L];
            pclntabLastFunc = last; 
            // Final entry of table is just end pc.
            ftab.SetAddrPlus(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), last, last.Size); 

            // Start file table.
            var start = int32(len(ftab.P));

            start += int32(-len(ftab.P)) & (int32(ctxt.Arch.PtrSize) - 1L);
            pclntabFiletabOffset = start;
            ftab.SetUint32(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), uint32(start));

            ftab.Grow(int64(start) + (int64(len(ctxt.Filesyms)) + 1L) * 4L);
            ftab.SetUint32(ctxt.Arch, int64(start), uint32(len(ctxt.Filesyms) + 1L));
            {
                var i__prev1 = i;

                for (var i = len(ctxt.Filesyms) - 1L; i >= 0L; i--)
                {
                    var s = ctxt.Filesyms[i];
                    ftab.SetUint32(ctxt.Arch, int64(start) + s.Value * 4L, uint32(ftabaddstring(_addr_ftab, s.Name)));
                }


                i = i__prev1;
            }

            ftab.Size = int64(len(ftab.P));

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("pclntab=%d bytes, funcdata total %d bytes\n", ftab.Size, funcdataBytes);
            }

        });

        private static @string gorootFinal()
        {
            var root = objabi.GOROOT;
            {
                var final = os.Getenv("GOROOT_FINAL");

                if (final != "")
                {
                    root = final;
                }

            }

            return root;

        }

        private static @string expandGoroot(@string s)
        {
            const var n = (var)len("$GOROOT");

            if (len(s) >= n + 1L && s[..n] == "$GOROOT" && (s[n] == '/' || s[n] == '\\'))
            {
                return filepath.ToSlash(filepath.Join(gorootFinal(), s[n..]));
            }

            return s;

        }

        public static readonly long BUCKETSIZE = (long)256L * MINFUNC;
        public static readonly long SUBBUCKETS = (long)16L;
        public static readonly var SUBBUCKETSIZE = (var)BUCKETSIZE / SUBBUCKETS;
        public static readonly ulong NOIDX = (ulong)0x7fffffffUL;


        // findfunctab generates a lookup table to quickly find the containing
        // function for a pc. See src/runtime/symtab.go:findfunc for details.
        private static void findfunctab(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var t = ctxt.Syms.Lookup("runtime.findfunctab", 0L);
            t.Type = sym.SRODATA;
            t.Attr |= sym.AttrReachable;
            t.Attr |= sym.AttrLocal; 

            // find min and max address
            var min = ctxt.Textp[0L].Value;
            var lastp = ctxt.Textp[len(ctxt.Textp) - 1L];
            var max = lastp.Value + lastp.Size; 

            // for each subbucket, compute the minimum of all symbol indexes
            // that map to that subbucket.
            var n = int32((max - min + SUBBUCKETSIZE - 1L) / SUBBUCKETSIZE);

            var indexes = make_slice<int>(n);
            {
                var i__prev1 = i;

                for (var i = int32(0L); i < n; i++)
                {
                    indexes[i] = NOIDX;
                }


                i = i__prev1;
            }
            var idx = int32(0L);
            {
                var i__prev1 = i;

                foreach (var (__i, __s) in ctxt.Textp)
                {
                    i = __i;
                    s = __s;
                    if (!emitPcln(_addr_ctxt, _addr_s))
                    {
                        continue;
                    }

                    var p = s.Value;
                    ptr<sym.Symbol> e;
                    i++;
                    if (i < len(ctxt.Textp))
                    {
                        e = ctxt.Textp[i];
                    }

                    while (!emitPcln(_addr_ctxt, e) && i < len(ctxt.Textp))
                    {
                        e = ctxt.Textp[i];
                        i++;
                    }

                    var q = max;
                    if (e != null)
                    {
                        q = e.Value;
                    } 

                    //print("%d: [%lld %lld] %s\n", idx, p, q, s->name);
                    while (p < q)
                    {
                        i = int((p - min) / SUBBUCKETSIZE);
                        if (indexes[i] > idx)
                        {
                            indexes[i] = idx;
                        p += SUBBUCKETSIZE;
                        }

                    }


                    i = int((q - 1L - min) / SUBBUCKETSIZE);
                    if (indexes[i] > idx)
                    {
                        indexes[i] = idx;
                    }

                    idx++;

                } 

                // allocate table

                i = i__prev1;
            }

            var nbuckets = int32((max - min + BUCKETSIZE - 1L) / BUCKETSIZE);

            t.Grow(4L * int64(nbuckets) + int64(n)); 

            // fill in table
            {
                var i__prev1 = i;

                for (i = int32(0L); i < nbuckets; i++)
                {
                    var @base = indexes[i * SUBBUCKETS];
                    if (base == NOIDX)
                    {
                        Errorf(null, "hole in findfunctab");
                    }

                    t.SetUint32(ctxt.Arch, int64(i) * (4L + SUBBUCKETS), uint32(base));
                    for (var j = int32(0L); j < SUBBUCKETS && i * SUBBUCKETS + j < n; j++)
                    {
                        idx = indexes[i * SUBBUCKETS + j];
                        if (idx == NOIDX)
                        {
                            Errorf(null, "hole in findfunctab");
                        }

                        if (idx - base >= 256L)
                        {
                            Errorf(null, "too many functions in a findfunc bucket! %d/%d %d %d", i, nbuckets, j, idx - base);
                        }

                        t.SetUint8(ctxt.Arch, int64(i) * (4L + SUBBUCKETS) + 4L + int64(j), uint8(idx - base));

                    }


                }


                i = i__prev1;
            }

        }
    }
}}}}
