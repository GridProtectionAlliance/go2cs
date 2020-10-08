// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goobj -- go2cs converted at 2020 October 08 03:50:17 UTC
// import "cmd/internal/goobj" ==> using goobj = go.cmd.@internal.goobj_package
// Original source: C:\Go\src\cmd\internal\goobj\readnew.go
using goobj2 = go.cmd.@internal.goobj2_package;
using objabi = go.cmd.@internal.objabi_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class goobj_package
    {
        // Read object file in new format. For now we still fill
        // the data to the current goobj API.
        private static void readNew(this ptr<objReader> _addr_r) => func((_, panic, __) =>
        {
            ref objReader r = ref _addr_r.val;

            var start = uint32(r.offset);

            var length = r.limit - r.offset;
            var objbytes = make_slice<byte>(length);
            r.readFull(objbytes);
            var rr = goobj2.NewReaderFromBytes(objbytes, false);
            if (rr == null)
            {
                panic("cannot read object file");
            }
            var autolib = rr.Autolib();
            {
                var p__prev1 = p;

                foreach (var (_, __p) in autolib)
                {
                    p = __p;
                    r.p.Imports = append(r.p.Imports, p.Pkg); 
                    // Ignore fingerprint (for tools like objdump which only reads one object).
                }
                p = p__prev1;
            }

            var nrefName = rr.NRefName();
            var refNames = make_map<goobj2.SymRef, @string>(nrefName);
            {
                long i__prev1 = i;

                for (long i = 0L; i < nrefName; i++)
                {
                    var rn = rr.RefName(i);
                    refNames[rn.Sym()] = rn.Name(rr);
                }

                i = i__prev1;
            }

            Func<ushort, long> abiToVer = abi =>
            {
                long vers = default;
                if (abi == goobj2.SymABIstatic)
                { 
                    // Static symbol
                    vers = r.p.MaxVersion;

                }
                return vers;

            };

            Func<goobj2.SymRef, SymID> resolveSymRef = s =>
            {
                i = default;
                {
                    var p__prev1 = p;

                    var p = s.PkgIdx;


                    if (p == goobj2.PkgIdxInvalid) 
                        if (s.SymIdx != 0L)
                        {
                            panic("bad sym ref");
                        }
                        return new SymID();
                    else if (p == goobj2.PkgIdxNone) 
                        i = int(s.SymIdx) + rr.NSym();
                    else if (p == goobj2.PkgIdxBuiltin) 
                        var (name, abi) = goobj2.BuiltinName(int(s.SymIdx));
                        return new SymID(name,int64(abi));
                    else if (p == goobj2.PkgIdxSelf) 
                        i = int(s.SymIdx);
                    else 
                        return new SymID(refNames[s],0);


                    p = p__prev1;
                }
                ref var sym = ref heap(rr.Sym(i), out ptr<var> _addr_sym);
                return new SymID(sym.Name(rr),abiToVer(sym.ABI()));

            }; 

            // Read things for the current goobj API for now.

            // Symbols
            var pcdataBase = start + rr.PcdataBase();
            var n = rr.NSym() + rr.NNonpkgdef() + rr.NNonpkgref();
            var ndef = rr.NSym() + rr.NNonpkgdef();
            {
                long i__prev1 = i;

                for (i = 0L; i < n; i++)
                {
                    var osym = rr.Sym(i);
                    if (osym.Name(rr) == "")
                    {
                        continue; // not a real symbol
                    }
                    var name = strings.ReplaceAll(osym.Name(rr), "\"\".", r.pkgprefix);
                    SymID symID = new SymID(Name:name,Version:abiToVer(osym.ABI()));
                    r.p.SymRefs = append(r.p.SymRefs, symID);

                    if (i >= ndef)
                    {
                        continue; // not a defined symbol from here
                    }
                    var dataOff = rr.DataOff(i);
                    var siz = int64(rr.DataSize(i));

                    sym = new Sym(SymID:symID,Kind:objabi.SymKind(osym.Type()),DupOK:osym.Dupok(),Size:int64(osym.Siz()),Data:Data{int64(start+dataOff),siz},);
                    r.p.Syms = append(r.p.Syms, _addr_sym); 

                    // Reloc
                    var relocs = rr.Relocs(i);
                    sym.Reloc = make_slice<Reloc>(len(relocs));
                    {
                        var j__prev2 = j;

                        foreach (var (__j) in relocs)
                        {
                            j = __j;
                            var rel = _addr_relocs[j];
                            sym.Reloc[j] = new Reloc(Offset:int64(rel.Off()),Size:int64(rel.Siz()),Type:objabi.RelocType(rel.Type()),Add:rel.Add(),Sym:resolveSymRef(rel.Sym()),);
                        }
                        j = j__prev2;
                    }

                    long isym = -1L;
                    var funcdata = make_slice<goobj2.SymRef>(0L, 4L);
                    var auxs = rr.Auxs(i);
                    {
                        var j__prev2 = j;

                        foreach (var (__j) in auxs)
                        {
                            j = __j;
                            var a = _addr_auxs[j];

                            if (a.Type() == goobj2.AuxGotype) 
                                sym.Type = resolveSymRef(a.Sym());
                            else if (a.Type() == goobj2.AuxFuncInfo) 
                                if (a.Sym().PkgIdx != goobj2.PkgIdxSelf)
                                {
                                    panic("funcinfo symbol not defined in current package");
                                }
                                isym = int(a.Sym().SymIdx);
                            else if (a.Type() == goobj2.AuxFuncdata) 
                                funcdata = append(funcdata, a.Sym());
                            else if (a.Type() == goobj2.AuxDwarfInfo || a.Type() == goobj2.AuxDwarfLoc || a.Type() == goobj2.AuxDwarfRanges || a.Type() == goobj2.AuxDwarfLines)                             else 
                                panic("unknown aux type");
                            
                        }
                        j = j__prev2;
                    }

                    if (isym == -1L)
                    {
                        continue;
                    }
                    var b = rr.BytesAt(rr.DataOff(isym), rr.DataSize(isym));
                    goobj2.FuncInfo info = new goobj2.FuncInfo();
                    info.Read(b);

                    info.Pcdata = append(info.Pcdata, info.PcdataEnd); // for the ease of knowing where it ends
                    ptr<Func> f = addr(new Func(Args:int64(info.Args),Frame:int64(info.Locals),NoSplit:osym.NoSplit(),Leaf:osym.Leaf(),TopFrame:osym.TopFrame(),PCSP:Data{int64(pcdataBase+info.Pcsp),int64(info.Pcfile-info.Pcsp)},PCFile:Data{int64(pcdataBase+info.Pcfile),int64(info.Pcline-info.Pcfile)},PCLine:Data{int64(pcdataBase+info.Pcline),int64(info.Pcinline-info.Pcline)},PCInline:Data{int64(pcdataBase+info.Pcinline),int64(info.Pcdata[0]-info.Pcinline)},PCData:make([]Data,len(info.Pcdata)-1),FuncData:make([]FuncData,len(info.Funcdataoff)),File:make([]string,len(info.File)),InlTree:make([]InlinedCall,len(info.InlTree)),));
                    sym.Func = f;
                    {
                        var k__prev2 = k;

                        foreach (var (__k) in f.PCData)
                        {
                            k = __k;
                            f.PCData[k] = new Data(int64(pcdataBase+info.Pcdata[k]),int64(info.Pcdata[k+1]-info.Pcdata[k]));
                        }
                        k = k__prev2;
                    }

                    {
                        var k__prev2 = k;

                        foreach (var (__k) in f.FuncData)
                        {
                            k = __k;
                            symID = resolveSymRef(funcdata[k]);
                            f.FuncData[k] = new FuncData(symID,int64(info.Funcdataoff[k]));
                        }
                        k = k__prev2;
                    }

                    {
                        var k__prev2 = k;

                        foreach (var (__k) in f.File)
                        {
                            k = __k;
                            symID = resolveSymRef(info.File[k]);
                            f.File[k] = symID.Name;
                        }
                        k = k__prev2;
                    }

                    {
                        var k__prev2 = k;

                        foreach (var (__k) in f.InlTree)
                        {
                            k = __k;
                            var inl = _addr_info.InlTree[k];
                            f.InlTree[k] = new InlinedCall(Parent:int64(inl.Parent),File:resolveSymRef(inl.File).Name,Line:int64(inl.Line),Func:resolveSymRef(inl.Func),ParentPC:int64(inl.ParentPC),);
                        }
                        k = k__prev2;
                    }
                }

                i = i__prev1;
            }

        });
    }
}}}
