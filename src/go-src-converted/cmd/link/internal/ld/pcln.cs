// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:50:17 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\pcln.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
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
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // pclnState holds state information used during pclntab generation.
        // Here 'ldr' is just a pointer to the context's loader, 'container'
        // is a bitmap holding whether a given symbol index is an outer or
        // container symbol, 'deferReturnSym' is the index for the symbol
        // "runtime.deferreturn", 'nameToOffset' is a helper function for
        // capturing function names, 'numberedFiles' records the file number
        // assigned to a given file symbol, 'filepaths' is a slice of
        // expanded paths (indexed by file number).
        private partial struct pclnState
        {
            public ptr<loader.Loader> ldr;
            public loader.Bitmap container;
            public loader.Sym deferReturnSym;
            public Func<@string, int> nameToOffset;
            public map<loader.Sym, long> numberedFiles;
            public slice<@string> filepaths;
        }

        private static pclnState makepclnState(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            var drs = ldr.Lookup("runtime.deferreturn", sym.SymVerABIInternal);
            return new pclnState(container:loader.MakeBitmap(ldr.NSym()),ldr:ldr,deferReturnSym:drs,numberedFiles:make(map[loader.Sym]int64),filepaths:[]string{""},);
        }

        private static int ftabaddstring(this ptr<pclnState> _addr_state, ptr<loader.SymbolBuilder> _addr_ftab, @string s)
        {
            ref pclnState state = ref _addr_state.val;
            ref loader.SymbolBuilder ftab = ref _addr_ftab.val;

            var start = len(ftab.Data());
            ftab.Grow(int64(start + len(s) + 1L)); // make room for s plus trailing NUL
            var ftd = ftab.Data();
            copy(ftd[start..], s);
            return int32(start);

        }

        // numberfile assigns a file number to the file if it hasn't been assigned already.
        private static long numberfile(this ptr<pclnState> _addr_state, loader.Sym file)
        {
            ref pclnState state = ref _addr_state.val;

            {
                var val__prev1 = val;

                var (val, ok) = state.numberedFiles[file];

                if (ok)
                {
                    return val;
                }

                val = val__prev1;

            }

            var sn = state.ldr.SymName(file);
            var path = sn[len(src.FileSymPrefix)..];
            var val = int64(len(state.filepaths));
            state.numberedFiles[file] = val;
            state.filepaths = append(state.filepaths, expandGoroot(path));
            return val;

        }

        private static long fileVal(this ptr<pclnState> _addr_state, loader.Sym file) => func((_, panic, __) =>
        {
            ref pclnState state = ref _addr_state.val;

            {
                var (val, ok) = state.numberedFiles[file];

                if (ok)
                {
                    return val;
                }

            }

            panic("should have been numbered first");

        });

        private static void renumberfiles(this ptr<pclnState> _addr_state, ptr<Link> _addr_ctxt, loader.FuncInfo fi, ptr<sym.Pcdata> _addr_d)
        {
            ref pclnState state = ref _addr_state.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Pcdata d = ref _addr_d.val;
 
            // Give files numbers.
            var nf = fi.NumFile();
            for (var i = uint32(0L); i < nf; i++)
            {
                state.numberfile(fi.File(int(i)));
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
                    if (oldval < 0L || oldval >= int32(nf))
                    {
                        log.Fatalf("bad pcdata %d", oldval);
                    }

                    val = int32(state.fileVal(fi.File(int(oldval))));

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

        // onlycsymbol looks at a symbol's name to report whether this is a
        // symbol that is referenced by C code
        private static bool onlycsymbol(@string sname)
        {
            switch (sname)
            {
                case "_cgo_topofstack": 

                case "__cgo_topofstack": 

                case "_cgo_panic": 

                case "crosscall2": 
                    return true;
                    break;
            }
            if (strings.HasPrefix(sname, "_cgoexp_"))
            {
                return true;
            }

            return false;

        }

        private static bool emitPcln(ptr<Link> _addr_ctxt, loader.Sym s, loader.Bitmap container)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.BuildMode == BuildModePlugin && ctxt.HeadType == objabi.Hdarwin && onlycsymbol(ctxt.loader.SymName(s)))
            {
                return false;
            } 
            // We want to generate func table entries only for the "lowest
            // level" symbols, not containers of subsymbols.
            return !container.Has(s);

        }

        private static uint computeDeferReturn(this ptr<pclnState> _addr_state, ptr<Target> _addr_target, loader.Sym s) => func((_, panic, __) =>
        {
            ref pclnState state = ref _addr_state.val;
            ref Target target = ref _addr_target.val;

            var deferreturn = uint32(0L);
            var lastWasmAddr = uint32(0L);

            var relocs = state.ldr.Relocs(s);
            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                if (target.IsWasm() && r.Type() == objabi.R_ADDR)
                { 
                    // Wasm does not have a live variable set at the deferreturn
                    // call itself. Instead it has one identified by the
                    // resumption point immediately preceding the deferreturn.
                    // The wasm code has a R_ADDR relocation which is used to
                    // set the resumption point to PC_B.
                    lastWasmAddr = uint32(r.Add());

                }

                if (r.Type().IsDirectCall() && (r.Sym() == state.deferReturnSym || state.ldr.IsDeferReturnTramp(r.Sym())))
                {
                    if (target.IsWasm())
                    {
                        deferreturn = lastWasmAddr - 1L;
                    }
                    else
                    { 
                        // Note: the relocation target is in the call instruction, but
                        // is not necessarily the whole instruction (for instance, on
                        // x86 the relocation applies to bytes [1:5] of the 5 byte call
                        // instruction).
                        deferreturn = uint32(r.Off());

                        if (target.Arch.Family == sys.AMD64 || target.Arch.Family == sys.I386) 
                            deferreturn--;
                        else if (target.Arch.Family == sys.PPC64 || target.Arch.Family == sys.ARM || target.Arch.Family == sys.ARM64 || target.Arch.Family == sys.MIPS || target.Arch.Family == sys.MIPS64)                         else if (target.Arch.Family == sys.RISCV64) 
                            // TODO(jsing): The JALR instruction is marked with
                            // R_CALLRISCV, whereas the actual reloc is currently
                            // one instruction earlier starting with the AUIPC.
                            deferreturn -= 4L;
                        else if (target.Arch.Family == sys.S390X) 
                            deferreturn -= 2L;
                        else 
                            panic(fmt.Sprint("Unhandled architecture:", target.Arch.Family));
                        
                    }

                    break; // only need one
                }

            }

            return deferreturn;

        });

        // genInlTreeSym generates the InlTree sym for a function with the
        // specified FuncInfo.
        private static loader.Sym genInlTreeSym(this ptr<pclnState> _addr_state, loader.FuncInfo fi, ptr<sys.Arch> _addr_arch)
        {
            ref pclnState state = ref _addr_state.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var ldr = state.ldr;
            var its = ldr.CreateExtSym("", 0L);
            var inlTreeSym = ldr.MakeSymbolUpdater(its); 
            // Note: the generated symbol is given a type of sym.SGOFUNC, as a
            // signal to the symtab() phase that it needs to be grouped in with
            // other similar symbols (gcdata, etc); the dodata() phase will
            // eventually switch the type back to SRODATA.
            inlTreeSym.SetType(sym.SGOFUNC);
            ldr.SetAttrReachable(its, true);
            var ninl = fi.NumInlTree();
            for (long i = 0L; i < int(ninl); i++)
            {
                var call = fi.InlTree(i); 
                // Usually, call.File is already numbered since the file
                // shows up in the Pcfile table. However, two inlined calls
                // might overlap exactly so that only the innermost file
                // appears in the Pcfile table. In that case, this assigns
                // the outer file a number.
                var val = state.numberfile(call.File);
                var fn = ldr.SymName(call.Func);
                var nameoff = state.nameToOffset(fn);

                inlTreeSym.SetUint16(arch, int64(i * 20L + 0L), uint16(call.Parent));
                inlTreeSym.SetUint8(arch, int64(i * 20L + 2L), uint8(objabi.GetFuncID(fn, ""))); 
                // byte 3 is unused
                inlTreeSym.SetUint32(arch, int64(i * 20L + 4L), uint32(val));
                inlTreeSym.SetUint32(arch, int64(i * 20L + 8L), uint32(call.Line));
                inlTreeSym.SetUint32(arch, int64(i * 20L + 12L), uint32(nameoff));
                inlTreeSym.SetUint32(arch, int64(i * 20L + 16L), uint32(call.ParentPC));

            }

            return its;

        }

        // pclntab initializes the pclntab symbol with
        // runtime function and file name information.

        // These variables are used to initialize runtime.firstmoduledata, see symtab.go:symtab.
        private static int pclntabNfunc = default;
        private static int pclntabFiletabOffset = default;
        private static int pclntabPclntabOffset = default;
        private static loader.Sym pclntabFirstFunc = default;
        private static loader.Sym pclntabLastFunc = default;

        // pclntab generates the pcln table for the link output. Return value
        // is a bitmap indexed by global symbol that marks 'container' text
        // symbols, e.g. the set of all symbols X such that Outer(S) = X for
        // some other text symbol S.
        private static loader.Bitmap pclntab(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var funcdataBytes = int64(0L);
            var ldr = ctxt.loader;
            var ftabsym = ldr.LookupOrCreateSym("runtime.pclntab", 0L);
            var ftab = ldr.MakeSymbolUpdater(ftabsym);
            ftab.SetType(sym.SPCLNTAB);
            ldr.SetAttrReachable(ftabsym, true);

            var state = makepclnState(_addr_ctxt); 

            // See golang.org/s/go12symtab for the format. Briefly:
            //    8-byte header
            //    nfunc [thearch.ptrsize bytes]
            //    function table, alternating PC and offset to func struct [each entry thearch.ptrsize bytes]
            //    end PC [thearch.ptrsize bytes]
            //    offset to file table [4 bytes]

            // Find container symbols and mark them as such.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    var outer = ldr.OuterSym(s);
                    if (outer != 0L)
                    {
                        state.container.Set(outer);
                    }

                } 

                // Gather some basic stats and info.

                s = s__prev1;
            }

            int nfunc = default;
            var prevSect = ldr.SymSect(ctxt.Textp2[0L]);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    if (!emitPcln(_addr_ctxt, s, state.container))
                    {
                        continue;
                    }

                    nfunc++;
                    if (pclntabFirstFunc == 0L)
                    {
                        pclntabFirstFunc = s;
                    }

                    var ss = ldr.SymSect(s);
                    if (ss != prevSect)
                    { 
                        // With multiple text sections, the external linker may
                        // insert functions between the sections, which are not
                        // known by Go. This leaves holes in the PC range covered
                        // by the func table. We need to generate an entry to mark
                        // the hole.
                        nfunc++;
                        prevSect = ss;

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

            var szHint = len(ctxt.Textp2) * 2L;
            var funcnameoff = make_map<@string, int>(szHint);
            Func<@string, int> nameToOffset = name =>
            {
                var (nameoff, ok) = funcnameoff[name];
                if (!ok)
                {
                    nameoff = state.ftabaddstring(ftab, name);
                    funcnameoff[name] = nameoff;
                }

                return nameoff;

            }
;
            state.nameToOffset = nameToOffset;

            var pctaboff = make_map<@string, uint>(szHint);
            Func<int, slice<byte>, int> writepctab = (off, p) =>
            {
                var (start, ok) = pctaboff[string(p)];
                if (!ok)
                {
                    if (len(p) > 0L)
                    {
                        start = uint32(len(ftab.Data()));
                        ftab.AddBytes(p);
                    }

                    pctaboff[string(p)] = start;

                }

                var newoff = int32(ftab.SetUint32(ctxt.Arch, int64(off), start));
                return newoff;

            }
;

            object setAddr = ptr<loader.SymbolBuilder>;
            if (ctxt.IsExe() && ctxt.IsInternal() && !ctxt.DynlinkingGo())
            { 
                // Internal linking static executable. At this point the function
                // addresses are known, so we can just use them instead of emitting
                // relocations.
                // For other cases we are generating a relocatable binary so we
                // still need to emit relocations.
                //
                // Also not do this optimization when using plugins (DynlinkingGo),
                // as on darwin it does weird things with runtime.etext symbol.
                // TODO: remove the weird thing and remove this condition.
                setAddr = (s, arch, off, tgt, add) =>
                {
                    {
                        var v = ldr.SymValue(tgt);

                        if (v != 0L)
                        {
                            return s.SetUint(arch, off, uint64(v + add));
                        }

                    }

                    return s.SetAddrPlus(arch, off, tgt, add);

                }
;

            }

            sym.Pcdata pcsp = new sym.Pcdata();
            ref sym.Pcdata pcfile = ref heap(new sym.Pcdata(), out ptr<sym.Pcdata> _addr_pcfile);
            sym.Pcdata pcline = new sym.Pcdata();
            sym.Pcdata pcdata = new slice<sym.Pcdata>(new sym.Pcdata[] {  });
            loader.Sym funcdata = new slice<loader.Sym>(new loader.Sym[] {  });
            long funcdataoff = new slice<long>(new long[] {  });

            nfunc = 0L; // repurpose nfunc as a running index
            var prevFunc = ctxt.Textp2[0L];
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    if (!emitPcln(_addr_ctxt, s, state.container))
                    {
                        continue;
                    }

                    var thisSect = ldr.SymSect(s);
                    prevSect = ldr.SymSect(prevFunc);
                    if (thisSect != prevSect)
                    { 
                        // With multiple text sections, there may be a hole here
                        // in the address space (see the comment above). We use an
                        // invalid funcoff value to mark the hole. See also
                        // runtime/symtab.go:findfunc
                        var prevFuncSize = int64(ldr.SymSize(prevFunc));
                        setAddr(ftab, ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), prevFunc, prevFuncSize);
                        ftab.SetUint(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), ~uint64(0L));
                        nfunc++;

                    }

                    prevFunc = s;

                    pcsp.P = pcsp.P[..0L];
                    pcline.P = pcline.P[..0L];
                    pcfile.P = pcfile.P[..0L];
                    pcdata = pcdata[..0L];
                    funcdataoff = funcdataoff[..0L];
                    funcdata = funcdata[..0L];
                    var fi = ldr.FuncInfo(s);
                    if (fi.Valid())
                    {
                        fi.Preload();
                        var npc = fi.NumPcdata();
                        {
                            var i__prev2 = i;

                            for (var i = uint32(0L); i < npc; i++)
                            {
                                pcdata = append(pcdata, new sym.Pcdata(P:fi.Pcdata(int(i))));
                            }


                            i = i__prev2;
                        }
                        var nfd = fi.NumFuncdataoff();
                        {
                            var i__prev2 = i;

                            for (i = uint32(0L); i < nfd; i++)
                            {
                                funcdataoff = append(funcdataoff, fi.Funcdataoff(int(i)));
                            }


                            i = i__prev2;
                        }
                        funcdata = fi.Funcdata(funcdata);

                    }

                    if (fi.Valid() && fi.NumInlTree() > 0L)
                    {
                        if (len(pcdata) <= objabi.PCDATA_InlTreeIndex)
                        { 
                            // Create inlining pcdata table.
                            var newpcdata = make_slice<sym.Pcdata>(objabi.PCDATA_InlTreeIndex + 1L);
                            copy(newpcdata, pcdata);
                            pcdata = newpcdata;

                        }

                        if (len(funcdataoff) <= objabi.FUNCDATA_InlTree)
                        { 
                            // Create inline tree funcdata.
                            var newfuncdata = make_slice<loader.Sym>(objabi.FUNCDATA_InlTree + 1L);
                            var newfuncdataoff = make_slice<long>(objabi.FUNCDATA_InlTree + 1L);
                            copy(newfuncdata, funcdata);
                            copy(newfuncdataoff, funcdataoff);
                            funcdata = newfuncdata;
                            funcdataoff = newfuncdataoff;

                        }

                    }

                    var dSize = len(ftab.Data());
                    var funcstart = int32(dSize);
                    funcstart += int32(-dSize) & (int32(ctxt.Arch.PtrSize) - 1L); // align to ptrsize

                    setAddr(ftab, ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), s, 0L);
                    ftab.SetUint(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), uint64(funcstart)); 

                    // Write runtime._func. Keep in sync with ../../../../runtime/runtime2.go:/_func
                    // and package debug/gosym.

                    // fixed size of struct, checked below
                    var off = funcstart;

                    var end = funcstart + int32(ctxt.Arch.PtrSize) + 3L * 4L + 5L * 4L + int32(len(pcdata)) * 4L + int32(len(funcdata)) * int32(ctxt.Arch.PtrSize);
                    if (len(funcdata) > 0L && (end & int32(ctxt.Arch.PtrSize - 1L) != 0L))
                    {
                        end += 4L;
                    }

                    ftab.Grow(int64(end)); 

                    // entry uintptr
                    off = int32(setAddr(ftab, ctxt.Arch, int64(off), s, 0L)); 

                    // name int32
                    var sn = ldr.SymName(s);
                    var nameoff = nameToOffset(sn);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(nameoff))); 

                    // args int32
                    // TODO: Move into funcinfo.
                    var args = uint32(0L);
                    if (fi.Valid())
                    {
                        args = uint32(fi.Args());
                    }

                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), args)); 

                    // deferreturn
                    var deferreturn = state.computeDeferReturn(_addr_ctxt.Target, s);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), deferreturn));

                    if (fi.Valid())
                    {
                        pcsp = new sym.Pcdata(P:fi.Pcsp());
                        pcfile = new sym.Pcdata(P:fi.Pcfile());
                        pcline = new sym.Pcdata(P:fi.Pcline());
                        state.renumberfiles(ctxt, fi, _addr_pcfile);
                        if (false)
                        { 
                            // Sanity check the new numbering
                            var it = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
                            it.Init(pcfile.P);

                            while (!it.Done)
                            {
                                if (it.Value < 1L || it.Value > int32(len(state.numberedFiles)))
                                {
                                    ctxt.Errorf(s, "bad file number in pcfile: %d not in range [1, %d]\n", it.Value, len(state.numberedFiles));
                                    errorexit();
                                it.Next();
                                }

                            }


                        }

                    }

                    if (fi.Valid() && fi.NumInlTree() > 0L)
                    {
                        var its = state.genInlTreeSym(fi, ctxt.Arch);
                        funcdata[objabi.FUNCDATA_InlTree] = its;
                        pcdata[objabi.PCDATA_InlTreeIndex] = new sym.Pcdata(P:fi.Pcinline());
                    } 

                    // pcdata
                    off = writepctab(off, pcsp.P);
                    off = writepctab(off, pcfile.P);
                    off = writepctab(off, pcline.P);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(len(pcdata)))); 

                    // funcID uint8
                    @string file = default;
                    if (fi.Valid() && fi.NumFile() > 0L)
                    {
                        var filesymname = ldr.SymName(fi.File(0L));
                        file = filesymname[len(src.FileSymPrefix)..];
                    }

                    var funcID = objabi.GetFuncID(sn, file);

                    off = int32(ftab.SetUint8(ctxt.Arch, int64(off), uint8(funcID))); 

                    // unused
                    off += 2L; 

                    // nfuncdata must be the final entry.
                    off = int32(ftab.SetUint8(ctxt.Arch, int64(off), uint8(len(funcdata))));
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in pcdata)
                        {
                            i = __i;
                            off = writepctab(off, pcdata[i].P);
                        } 

                        // funcdata, must be pointer-aligned and we're only int32-aligned.
                        // Missing funcdata will be 0 (nil pointer).

                        i = i__prev2;
                    }

                    if (len(funcdata) > 0L)
                    {
                        if (off & int32(ctxt.Arch.PtrSize - 1L) != 0L)
                        {
                            off += 4L;
                        }

                        {
                            var i__prev2 = i;

                            foreach (var (__i) in funcdata)
                            {
                                i = __i;
                                var dataoff = int64(off) + int64(ctxt.Arch.PtrSize) * int64(i);
                                if (funcdata[i] == 0L)
                                {
                                    ftab.SetUint(ctxt.Arch, dataoff, uint64(funcdataoff[i]));
                                    continue;
                                } 
                                // TODO: Dedup.
                                funcdataBytes += int64(len(ldr.Data(funcdata[i])));
                                setAddr(ftab, ctxt.Arch, dataoff, funcdata[i], funcdataoff[i]);

                            }

                            i = i__prev2;
                        }

                        off += int32(len(funcdata)) * int32(ctxt.Arch.PtrSize);

                    }

                    if (off != end)
                    {
                        ctxt.Errorf(s, "bad math in functab: funcstart=%d off=%d but end=%d (npcdata=%d nfuncdata=%d ptrsize=%d)", funcstart, off, end, len(pcdata), len(funcdata), ctxt.Arch.PtrSize);
                        errorexit();
                    }

                    nfunc++;

                }

                s = s__prev1;
            }

            var last = ctxt.Textp2[len(ctxt.Textp2) - 1L];
            pclntabLastFunc = last; 
            // Final entry of table is just end pc.
            setAddr(ftab, ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize), last, ldr.SymSize(last)); 

            // Start file table.
            dSize = len(ftab.Data());
            var start = int32(dSize);
            start += int32(-dSize) & (int32(ctxt.Arch.PtrSize) - 1L);
            pclntabFiletabOffset = start;
            ftab.SetUint32(ctxt.Arch, 8L + int64(ctxt.Arch.PtrSize) + int64(nfunc) * 2L * int64(ctxt.Arch.PtrSize) + int64(ctxt.Arch.PtrSize), uint32(start));

            var nf = len(state.numberedFiles);
            ftab.Grow(int64(start) + int64((nf + 1L) * 4L));
            ftab.SetUint32(ctxt.Arch, int64(start), uint32(nf + 1L));
            {
                var i__prev1 = i;

                for (i = nf; i > 0L; i--)
                {
                    var path = state.filepaths[i];
                    var val = int64(i);
                    ftab.SetUint32(ctxt.Arch, int64(start) + val * 4L, uint32(state.ftabaddstring(ftab, path)));
                }


                i = i__prev1;
            }

            ftab.SetSize(int64(len(ftab.Data())));

            ctxt.NumFilesyms = len(state.numberedFiles);

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("pclntab=%d bytes, funcdata total %d bytes\n", ftab.Size(), funcdataBytes);
            }

            return state.container;

        }

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
            const var n = len("$GOROOT");

            if (len(s) >= n + 1L && s[..n] == "$GOROOT" && (s[n] == '/' || s[n] == '\\'))
            {
                return filepath.ToSlash(filepath.Join(gorootFinal(), s[n..]));
            }

            return s;

        }

        public static readonly long BUCKETSIZE = (long)256L * MINFUNC;
        public static readonly long SUBBUCKETS = (long)16L;
        public static readonly var SUBBUCKETSIZE = BUCKETSIZE / SUBBUCKETS;
        public static readonly ulong NOIDX = (ulong)0x7fffffffUL;


        // findfunctab generates a lookup table to quickly find the containing
        // function for a pc. See src/runtime/symtab.go:findfunc for details.
        // 'container' is a bitmap indexed by global symbol holding whether
        // a given text symbols is a container (outer sym).
        private static void findfunctab(this ptr<Link> _addr_ctxt, loader.Bitmap container)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            var tsym = ldr.LookupOrCreateSym("runtime.findfunctab", 0L);
            var t = ldr.MakeSymbolUpdater(tsym);
            t.SetType(sym.SRODATA);
            ldr.SetAttrReachable(tsym, true);
            ldr.SetAttrLocal(tsym, true); 

            // find min and max address
            var min = ldr.SymValue(ctxt.Textp2[0L]);
            var lastp = ctxt.Textp2[len(ctxt.Textp2) - 1L];
            var max = ldr.SymValue(lastp) + ldr.SymSize(lastp); 

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

                foreach (var (__i, __s) in ctxt.Textp2)
                {
                    i = __i;
                    s = __s;
                    if (!emitPcln(_addr_ctxt, s, container))
                    {
                        continue;
                    }

                    var p = ldr.SymValue(s);
                    loader.Sym e = default;
                    i++;
                    if (i < len(ctxt.Textp2))
                    {
                        e = ctxt.Textp2[i];
                    }

                    while (e != 0L && !emitPcln(_addr_ctxt, e, container) && i < len(ctxt.Textp2))
                    {
                        e = ctxt.Textp2[i];
                        i++;
                    }

                    var q = max;
                    if (e != 0L)
                    {
                        q = ldr.SymValue(e);
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
