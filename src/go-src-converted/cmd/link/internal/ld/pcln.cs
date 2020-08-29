// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:26 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\pcln.go
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sym = go.cmd.link.@internal.sym_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // iteration over encoded pcdata tables.
        private static uint getvarint(ref slice<byte> pp)
        {
            var v = uint32(0L);
            var p = pp.Value;
            {
                long shift = 0L;

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    v |= uint32(p[0L] & 0x7FUL) << (int)(uint(shift));
                    var tmp4 = p;
                    p = p[1L..];
                    if (tmp4[0L] & 0x80UL == 0L)
                    {
                        break;
                    shift += 7L;
                    }
                }
            }

            pp.Value = p;
            return v;
        }

        private static void pciternext(ref Pciter it)
        {
            it.pc = it.nextpc;
            if (it.done != 0L)
            {
                return;
            }
            if (-cap(it.p) >= -cap(it.d.P[len(it.d.P)..]))
            {
                it.done = 1L;
                return;
            } 

            // value delta
            var v = getvarint(ref it.p);

            if (v == 0L && it.start == 0L)
            {
                it.done = 1L;
                return;
            }
            it.start = 0L;
            var dv = int32(v >> (int)(1L)) ^ (int32(v << (int)(31L)) >> (int)(31L));
            it.value += dv; 

            // pc delta
            v = getvarint(ref it.p);

            it.nextpc = it.pc + v * it.pcscale;
        }

        private static void pciterinit(ref Link ctxt, ref Pciter it, ref sym.Pcdata d)
        {
            it.d = d.Value;
            it.p = it.d.P;
            it.pc = 0L;
            it.nextpc = 0L;
            it.value = -1L;
            it.start = 1L;
            it.done = 0L;
            it.pcscale = uint32(ctxt.Arch.MinLC);
            pciternext(it);
        }

        private static void addvarint(ref sym.Pcdata d, uint val)
        {
            var n = int32(0L);
            {
                var v__prev1 = v;

                var v = val;

                while (v >= 0x80UL)
                {
                    n++;
                    v >>= 7L;
                }


                v = v__prev1;
            }
            n++;

            var old = len(d.P);
            while (cap(d.P) < len(d.P) + int(n))
            {
                d.P = append(d.P[..cap(d.P)], 0L);
            }

            d.P = d.P[..old + int(n)];

            var p = d.P[old..];
            v = default;
            v = val;

            while (v >= 0x80UL)
            {
                p[0L] = byte(v | 0x80UL);
                p = p[1L..];
                v >>= 7L;
            }

            p[0L] = byte(v);
        }

        private static int addpctab(ref Link ctxt, ref sym.Symbol ftab, int off, ref sym.Pcdata d)
        {
            int start = default;
            if (len(d.P) > 0L)
            {
                start = int32(len(ftab.P));
                ftab.AddBytes(d.P);
            }
            return int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(start)));
        }

        private static int ftabaddstring(ref Link ctxt, ref sym.Symbol ftab, @string s)
        {
            var n = int32(len(s)) + 1L;
            var start = int32(len(ftab.P));
            ftab.Grow(int64(start) + int64(n) + 1L);
            copy(ftab.P[start..], s);
            return start;
        }

        // numberfile assigns a file number to the file if it hasn't been assigned already.
        private static void numberfile(ref Link ctxt, ref sym.Symbol file)
        {
            if (file.Type != sym.SFILEPATH)
            {
                ctxt.Filesyms = append(ctxt.Filesyms, file);
                file.Value = int64(len(ctxt.Filesyms));
                file.Type = sym.SFILEPATH;
                var path = file.Name[len(src.FileSymPrefix)..];
                file.Name = expandGoroot(path);
            }
        }

        private static void renumberfiles(ref Link ctxt, slice<ref sym.Symbol> files, ref sym.Pcdata d)
        {
            ref sym.Symbol f = default; 

            // Give files numbers.
            for (long i = 0L; i < len(files); i++)
            {
                f = files[i];
                numberfile(ctxt, f);
            }


            var newval = int32(-1L);
            sym.Pcdata @out = default;
            Pciter it = default;
            pciterinit(ctxt, ref it, d);

            while (it.done == 0L)
            { 
                // value delta
                var oldval = it.value;

                int val = default;
                if (oldval == -1L)
                {
                    val = -1L;
                pciternext(ref it);
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
                var v = (uint32(dv) << (int)(1L)) ^ uint32(dv >> (int)(31L));
                addvarint(ref out, v); 

                // pc delta
                addvarint(ref out, (it.nextpc - it.pc) / it.pcscale);
            } 

            // terminating value delta
 

            // terminating value delta
            addvarint(ref out, 0L);

            d.Value = out;
        }

        // onlycsymbol reports whether this is a cgo symbol provided by the
        // runtime and only used from C code.
        private static bool onlycsymbol(ref sym.Symbol s)
        {
            switch (s.Name)
            {
                case "_cgo_topofstack": 

                case "_cgo_panic": 

                case "crosscall2": 
                    return true;
                    break;
            }
            return false;
        }

        private static bool emitPcln(ref Link ctxt, ref sym.Symbol s)
        {
            if (s == null)
            {
                return true;
            }
            if (ctxt.BuildMode == BuildModePlugin && ctxt.HeadType == objabi.Hdarwin && onlycsymbol(s))
            {
                return false;
            } 
            // We want to generate func table entries only for the "lowest level" symbols,
            // not containers of subsymbols.
            if (s.Attr.Container())
            {
                return true;
            }
            return true;
        }

        // pclntab initializes the pclntab symbol with
        // runtime function and file name information.

        private static sym.FuncInfo pclntabZpcln = default;

        // These variables are used to initialize runtime.firstmoduledata, see symtab.go:symtab.
        private static int pclntabNfunc = default;
        private static int pclntabFiletabOffset = default;
        private static int pclntabPclntabOffset = default;
        private static ref sym.Symbol pclntabFirstFunc = default;
        private static ref sym.Symbol pclntabLastFunc = default;

        private static void pclntab(this ref Link ctxt)
        {
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
            var nfunc = int32(0L); 

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

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (emitPcln(ctxt, s))
                    {
                        nfunc++;
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
                    nameoff = ftabaddstring(ctxt, ftab, name);
                    funcnameoff[name] = nameoff;
                }
                return nameoff;
            }
;

            nfunc = 0L;
            ref sym.Symbol last = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    last = s;
                    if (!emitPcln(ctxt, s))
                    {
                        continue;
                    }
                    var pcln = s.FuncInfo;
                    if (pcln == null)
                    {
                        pcln = ref pclntabZpcln;
                    }
                    if (pclntabFirstFunc == null)
                    {
                        pclntabFirstFunc = s;
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
                            var funcdata = make_slice<ref sym.Symbol>(objabi.FUNCDATA_InlTree + 1L);
                            var funcdataoff = make_slice<long>(objabi.FUNCDATA_InlTree + 1L);
                            copy(funcdata, pcln.Funcdata);
                            copy(funcdataoff, pcln.Funcdataoff);
                            pcln.Funcdata = funcdata;
                            pcln.Funcdataoff = funcdataoff;
                        }
                    }
                    var funcstart = int32(len(ftab.P));
                    funcstart += int32(-len(ftab.P)) & (int32(ctxt.Arch.PtrSize) - 1L);

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

                    // funcID uint32
                    var funcID = objabi.FuncID_normal;
                    switch (s.Name)
                    {
                        case "runtime.goexit": 
                            funcID = objabi.FuncID_goexit;
                            break;
                        case "runtime.jmpdefer": 
                            funcID = objabi.FuncID_jmpdefer;
                            break;
                        case "runtime.mcall": 
                            funcID = objabi.FuncID_mcall;
                            break;
                        case "runtime.morestack": 
                            funcID = objabi.FuncID_morestack;
                            break;
                        case "runtime.mstart": 
                            funcID = objabi.FuncID_mstart;
                            break;
                        case "runtime.rt0_go": 
                            funcID = objabi.FuncID_rt0_go;
                            break;
                        case "runtime.asmcgocall": 
                            funcID = objabi.FuncID_asmcgocall;
                            break;
                        case "runtime.sigpanic": 
                            funcID = objabi.FuncID_sigpanic;
                            break;
                        case "runtime.runfinq": 
                            funcID = objabi.FuncID_runfinq;
                            break;
                        case "runtime.bgsweep": 
                            funcID = objabi.FuncID_bgsweep;
                            break;
                        case "runtime.forcegchelper": 
                            funcID = objabi.FuncID_forcegchelper;
                            break;
                        case "runtime.timerproc": 
                            funcID = objabi.FuncID_timerproc;
                            break;
                        case "runtime.gcBgMarkWorker": 
                            funcID = objabi.FuncID_gcBgMarkWorker;
                            break;
                        case "runtime.systemstack_switch": 
                            funcID = objabi.FuncID_systemstack_switch;
                            break;
                        case "runtime.systemstack": 
                            funcID = objabi.FuncID_systemstack;
                            break;
                        case "runtime.cgocallback_gofunc": 
                            funcID = objabi.FuncID_cgocallback_gofunc;
                            break;
                        case "runtime.gogo": 
                            funcID = objabi.FuncID_gogo;
                            break;
                        case "runtime.externalthreadhandler": 
                            funcID = objabi.FuncID_externalthreadhandler;
                            break;
                    }
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(funcID)));

                    if (pcln != ref pclntabZpcln)
                    {
                        renumberfiles(ctxt, pcln.File, ref pcln.Pcfile);
                        if (false)
                        { 
                            // Sanity check the new numbering
                            Pciter it = default;
                            pciterinit(ctxt, ref it, ref pcln.Pcfile);

                            while (it.done == 0L)
                            {
                                if (it.value < 1L || it.value > int32(len(ctxt.Filesyms)))
                                {
                                    Errorf(s, "bad file number in pcfile: %d not in range [1, %d]\n", it.value, len(ctxt.Filesyms));
                                    errorexit();
                                pciternext(ref it);
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
                                numberfile(ctxt, call.File);
                                nameoff = nameToOffset(call.Func.Name);

                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 16L + 0L), uint32(call.Parent));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 16L + 4L), uint32(call.File.Value));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 16L + 8L), uint32(call.Line));
                                inlTreeSym.SetUint32(ctxt.Arch, int64(i * 16L + 12L), uint32(nameoff));
                            }

                            i = i__prev2;
                        }

                        pcln.Funcdata[objabi.FUNCDATA_InlTree] = inlTreeSym;
                        pcln.Pcdata[objabi.PCDATA_InlTreeIndex] = pcln.Pcinline;
                    } 

                    // pcdata
                    off = addpctab(ctxt, ftab, off, ref pcln.Pcsp);

                    off = addpctab(ctxt, ftab, off, ref pcln.Pcfile);
                    off = addpctab(ctxt, ftab, off, ref pcln.Pcline);
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(len(pcln.Pcdata))));
                    off = int32(ftab.SetUint32(ctxt.Arch, int64(off), uint32(len(pcln.Funcdata))));
                    {
                        var i__prev2 = i;

                        for (long i = 0L; i < len(pcln.Pcdata); i++)
                        {
                            off = addpctab(ctxt, ftab, off, ref pcln.Pcdata[i]);
                        } 

                        // funcdata, must be pointer-aligned and we're only int32-aligned.
                        // Missing funcdata will be 0 (nil pointer).


                        i = i__prev2;
                    } 

                    // funcdata, must be pointer-aligned and we're only int32-aligned.
                    // Missing funcdata will be 0 (nil pointer).
                    if (len(pcln.Funcdata) > 0L)
                    {
                        if (off & int32(ctxt.Arch.PtrSize - 1L) != 0L)
                        {
                            off += 4L;
                        }
                        {
                            var i__prev2 = i;

                            for (i = 0L; i < len(pcln.Funcdata); i++)
                            {
                                if (pcln.Funcdata[i] == null)
                                {
                                    ftab.SetUint(ctxt.Arch, int64(off) + int64(ctxt.Arch.PtrSize) * int64(i), uint64(pcln.Funcdataoff[i]));
                                }
                                else
                                { 
                                    // TODO: Dedup.
                                    funcdataBytes += pcln.Funcdata[i].Size;

                                    ftab.SetAddrPlus(ctxt.Arch, int64(off) + int64(ctxt.Arch.PtrSize) * int64(i), pcln.Funcdata[i], pcln.Funcdataoff[i]);
                                }
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

                for (i = len(ctxt.Filesyms) - 1L; i >= 0L; i--)
                {
                    var s = ctxt.Filesyms[i];
                    ftab.SetUint32(ctxt.Arch, int64(start) + s.Value * 4L, uint32(ftabaddstring(ctxt, ftab, s.Name)));
                }


                i = i__prev1;
            }

            ftab.Size = int64(len(ftab.P));

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f pclntab=%d bytes, funcdata total %d bytes\n", Cputime(), ftab.Size, funcdataBytes);
            }
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

        public static readonly long BUCKETSIZE = 256L * MINFUNC;
        public static readonly long SUBBUCKETS = 16L;
        public static readonly var SUBBUCKETSIZE = BUCKETSIZE / SUBBUCKETS;
        public static readonly ulong NOIDX = 0x7fffffffUL;

        // findfunctab generates a lookup table to quickly find the containing
        // function for a pc. See src/runtime/symtab.go:findfunc for details.
        private static void findfunctab(this ref Link ctxt)
        {
            var t = ctxt.Syms.Lookup("runtime.findfunctab", 0L);
            t.Type = sym.SRODATA;
            t.Attr |= sym.AttrReachable;
            t.Attr |= sym.AttrLocal; 

            // find min and max address
            var min = ctxt.Textp[0L].Value;
            var max = int64(0L);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    max = s.Value + s.Size;
                } 

                // for each subbucket, compute the minimum of all symbol indexes
                // that map to that subbucket.

                s = s__prev1;
            }

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
                var s__prev1 = s;

                foreach (var (__i, __s) in ctxt.Textp)
                {
                    i = __i;
                    s = __s;
                    if (!emitPcln(ctxt, s))
                    {
                        continue;
                    }
                    var p = s.Value;
                    ref sym.Symbol e = default;
                    i++;
                    if (i < len(ctxt.Textp))
                    {
                        e = ctxt.Textp[i];
                    }
                    while (!emitPcln(ctxt, e) && i < len(ctxt.Textp))
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
                s = s__prev1;
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
