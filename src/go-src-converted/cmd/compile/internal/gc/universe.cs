// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO(gri) This file should probably become part of package types.

// package gc -- go2cs converted at 2020 October 09 05:43:38 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\universe.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // builtinpkg is a fake package that declares the universe block.
        private static ptr<types.Pkg> builtinpkg;







        // isBuiltinFuncName reports whether name matches a builtin function
        // name.
        private static bool isBuiltinFuncName(@string name)
        {
            foreach (var (_, fn) in _addr_builtinFuncs)
            {
                if (fn.name == name)
                {
                    return true;
                }

            }
            return false;

        }



        // initUniverse initializes the universe block.
        private static void initUniverse()
        {
            lexinit();
            typeinit();
            lexinit1();
        }

        // lexinit initializes known symbols and the basic types.
        private static void lexinit()
        {
            {
                var s__prev1 = s;

                foreach (var (_, __s) in _addr_basicTypes)
                {
                    s = __s;
                    var etype = s.etype;
                    if (int(etype) >= len(types.Types))
                    {
                        Fatalf("lexinit: %s bad etype", s.name);
                    }

                    var s2 = builtinpkg.Lookup(s.name);
                    var t = types.Types[etype];
                    if (t == null)
                    {
                        t = types.New(etype);
                        t.Sym = s2;
                        if (etype != TANY && etype != TSTRING)
                        {
                            dowidth(t);
                        }

                        types.Types[etype] = t;

                    }

                    s2.Def = asTypesNode(typenod(t));
                    asNode(s2.Def).Name;

                    @new<Name>();

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in _addr_builtinFuncs)
                {
                    s = __s;
                    s2 = builtinpkg.Lookup(s.name);
                    s2.Def = asTypesNode(newname(s2));
                    asNode(s2.Def).SetSubOp(s.op);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in _addr_unsafeFuncs)
                {
                    s = __s;
                    s2 = unsafepkg.Lookup(s.name);
                    s2.Def = asTypesNode(newname(s2));
                    asNode(s2.Def).SetSubOp(s.op);
                }

                s = s__prev1;
            }

            types.Idealstring = types.New(TSTRING);
            types.Idealbool = types.New(TBOOL);
            types.Types[TANY] = types.New(TANY);

            var s = builtinpkg.Lookup("true");
            s.Def = asTypesNode(nodbool(true));
            asNode(s.Def).Sym;

            lookup("true");
            asNode(s.Def).Name;

            @new<Name>();
            asNode(s.Def).Type;

            types.Idealbool;

            s = builtinpkg.Lookup("false");
            s.Def = asTypesNode(nodbool(false));
            asNode(s.Def).Sym;

            lookup("false");
            asNode(s.Def).Name;

            @new<Name>();
            asNode(s.Def).Type;

            types.Idealbool;

            s = lookup("_");
            s.Block = -100L;
            s.Def = asTypesNode(newname(s));
            types.Types[TBLANK] = types.New(TBLANK);
            asNode(s.Def).Type;

            types.Types[TBLANK];
            nblank = asNode(s.Def);

            s = builtinpkg.Lookup("_");
            s.Block = -100L;
            s.Def = asTypesNode(newname(s));
            types.Types[TBLANK] = types.New(TBLANK);
            asNode(s.Def).Type;

            types.Types[TBLANK];

            types.Types[TNIL] = types.New(TNIL);
            s = builtinpkg.Lookup("nil");
            Val v = default;
            v.U = @new<NilVal>();
            s.Def = asTypesNode(nodlit(v));
            asNode(s.Def).Sym;

            s;
            asNode(s.Def).Name;

            @new<Name>();

            s = builtinpkg.Lookup("iota");
            s.Def = asTypesNode(nod(OIOTA, null, null));
            asNode(s.Def).Sym;

            s;
            asNode(s.Def).Name;

            @new<Name>();

        }

        private static void typeinit()
        {
            if (Widthptr == 0L)
            {
                Fatalf("typeinit before betypeinit");
            }

            {
                var et__prev1 = et;

                for (var et = types.EType(0L); et < NTYPE; et++)
                {
                    simtype[et] = et;
                }


                et = et__prev1;
            }

            types.Types[TPTR] = types.New(TPTR);
            dowidth(types.Types[TPTR]);

            var t = types.New(TUNSAFEPTR);
            types.Types[TUNSAFEPTR] = t;
            t.Sym = unsafepkg.Lookup("Pointer");
            t.Sym.Def = asTypesNode(typenod(t));
            asNode(t.Sym.Def).Name;

            @new<Name>();
            dowidth(types.Types[TUNSAFEPTR]);

            {
                var et__prev1 = et;

                for (et = TINT8; et <= TUINT64; et++)
                {
                    isInt[et] = true;
                }


                et = et__prev1;
            }
            isInt[TINT] = true;
            isInt[TUINT] = true;
            isInt[TUINTPTR] = true;

            isFloat[TFLOAT32] = true;
            isFloat[TFLOAT64] = true;

            isComplex[TCOMPLEX64] = true;
            isComplex[TCOMPLEX128] = true; 

            // initialize okfor
            {
                var et__prev1 = et;

                for (et = types.EType(0L); et < NTYPE; et++)
                {
                    if (isInt[et] || et == TIDEAL)
                    {
                        okforeq[et] = true;
                        okforcmp[et] = true;
                        okforarith[et] = true;
                        okforadd[et] = true;
                        okforand[et] = true;
                        okforconst[et] = true;
                        issimple[et] = true;
                        minintval[et] = @new<Mpint>();
                        maxintval[et] = @new<Mpint>();
                    }

                    if (isFloat[et])
                    {
                        okforeq[et] = true;
                        okforcmp[et] = true;
                        okforadd[et] = true;
                        okforarith[et] = true;
                        okforconst[et] = true;
                        issimple[et] = true;
                        minfltval[et] = newMpflt();
                        maxfltval[et] = newMpflt();
                    }

                    if (isComplex[et])
                    {
                        okforeq[et] = true;
                        okforadd[et] = true;
                        okforarith[et] = true;
                        okforconst[et] = true;
                        issimple[et] = true;
                    }

                }


                et = et__prev1;
            }

            issimple[TBOOL] = true;

            okforadd[TSTRING] = true;

            okforbool[TBOOL] = true;

            okforcap[TARRAY] = true;
            okforcap[TCHAN] = true;
            okforcap[TSLICE] = true;

            okforconst[TBOOL] = true;
            okforconst[TSTRING] = true;

            okforlen[TARRAY] = true;
            okforlen[TCHAN] = true;
            okforlen[TMAP] = true;
            okforlen[TSLICE] = true;
            okforlen[TSTRING] = true;

            okforeq[TPTR] = true;
            okforeq[TUNSAFEPTR] = true;
            okforeq[TINTER] = true;
            okforeq[TCHAN] = true;
            okforeq[TSTRING] = true;
            okforeq[TBOOL] = true;
            okforeq[TMAP] = true; // nil only; refined in typecheck
            okforeq[TFUNC] = true; // nil only; refined in typecheck
            okforeq[TSLICE] = true; // nil only; refined in typecheck
            okforeq[TARRAY] = true; // only if element type is comparable; refined in typecheck
            okforeq[TSTRUCT] = true; // only if all struct fields are comparable; refined in typecheck

            okforcmp[TSTRING] = true;

            long i = default;
            for (i = 0L; i < len(okfor); i++)
            {
                okfor[i] = okfornone[..];
            } 

            // binary
 

            // binary
            okfor[OADD] = okforadd[..];
            okfor[OAND] = okforand[..];
            okfor[OANDAND] = okforbool[..];
            okfor[OANDNOT] = okforand[..];
            okfor[ODIV] = okforarith[..];
            okfor[OEQ] = okforeq[..];
            okfor[OGE] = okforcmp[..];
            okfor[OGT] = okforcmp[..];
            okfor[OLE] = okforcmp[..];
            okfor[OLT] = okforcmp[..];
            okfor[OMOD] = okforand[..];
            okfor[OMUL] = okforarith[..];
            okfor[ONE] = okforeq[..];
            okfor[OOR] = okforand[..];
            okfor[OOROR] = okforbool[..];
            okfor[OSUB] = okforarith[..];
            okfor[OXOR] = okforand[..];
            okfor[OLSH] = okforand[..];
            okfor[ORSH] = okforand[..]; 

            // unary
            okfor[OBITNOT] = okforand[..];
            okfor[ONEG] = okforarith[..];
            okfor[ONOT] = okforbool[..];
            okfor[OPLUS] = okforarith[..]; 

            // special
            okfor[OCAP] = okforcap[..];
            okfor[OLEN] = okforlen[..]; 

            // comparison
            iscmp[OLT] = true;
            iscmp[OGT] = true;
            iscmp[OGE] = true;
            iscmp[OLE] = true;
            iscmp[OEQ] = true;
            iscmp[ONE] = true;

            maxintval[TINT8].SetString("0x7f");
            minintval[TINT8].SetString("-0x80");
            maxintval[TINT16].SetString("0x7fff");
            minintval[TINT16].SetString("-0x8000");
            maxintval[TINT32].SetString("0x7fffffff");
            minintval[TINT32].SetString("-0x80000000");
            maxintval[TINT64].SetString("0x7fffffffffffffff");
            minintval[TINT64].SetString("-0x8000000000000000");

            maxintval[TUINT8].SetString("0xff");
            maxintval[TUINT16].SetString("0xffff");
            maxintval[TUINT32].SetString("0xffffffff");
            maxintval[TUINT64].SetString("0xffffffffffffffff"); 

            // f is valid float if min < f < max.  (min and max are not themselves valid.)
            maxfltval[TFLOAT32].SetString("33554431p103"); // 2^24-1 p (127-23) + 1/2 ulp
            minfltval[TFLOAT32].SetString("-33554431p103");
            maxfltval[TFLOAT64].SetString("18014398509481983p970"); // 2^53-1 p (1023-52) + 1/2 ulp
            minfltval[TFLOAT64].SetString("-18014398509481983p970");

            maxfltval[TCOMPLEX64] = maxfltval[TFLOAT32];
            minfltval[TCOMPLEX64] = minfltval[TFLOAT32];
            maxfltval[TCOMPLEX128] = maxfltval[TFLOAT64];
            minfltval[TCOMPLEX128] = minfltval[TFLOAT64];

            types.Types[TINTER] = types.New(TINTER); // empty interface

            // simple aliases
            simtype[TMAP] = TPTR;
            simtype[TCHAN] = TPTR;
            simtype[TFUNC] = TPTR;
            simtype[TUNSAFEPTR] = TPTR;

            slicePtrOffset = 0L;
            sliceLenOffset = Rnd(slicePtrOffset + int64(Widthptr), int64(Widthptr));
            sliceCapOffset = Rnd(sliceLenOffset + int64(Widthptr), int64(Widthptr));
            sizeofSlice = Rnd(sliceCapOffset + int64(Widthptr), int64(Widthptr)); 

            // string is same as slice wo the cap
            sizeofString = Rnd(sliceLenOffset + int64(Widthptr), int64(Widthptr));

            dowidth(types.Types[TSTRING]);
            dowidth(types.Idealstring);

        }

        private static ptr<types.Type> makeErrorInterface()
        {
            var field = types.NewField();
            field.Type = types.Types[TSTRING];
            var f = functypefield(fakeRecvField(), null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { field }));

            field = types.NewField();
            field.Sym = lookup("Error");
            field.Type = f;

            var t = types.New(TINTER);
            t.SetInterface(new slice<ptr<types.Field>>(new ptr<types.Field>[] { field }));
            return _addr_t!;
        }

        private static void lexinit1()
        { 
            // error type
            var s = builtinpkg.Lookup("error");
            types.Errortype = makeErrorInterface();
            types.Errortype.Sym = s;
            types.Errortype.Orig = makeErrorInterface();
            s.Def = asTypesNode(typenod(types.Errortype));
            dowidth(types.Errortype); 

            // We create separate byte and rune types for better error messages
            // rather than just creating type alias *types.Sym's for the uint8 and
            // int32 types. Hence, (bytetype|runtype).Sym.isAlias() is false.
            // TODO(gri) Should we get rid of this special case (at the cost
            // of less informative error messages involving bytes and runes)?
            // (Alternatively, we could introduce an OTALIAS node representing
            // type aliases, albeit at the cost of having to deal with it everywhere).

            // byte alias
            s = builtinpkg.Lookup("byte");
            types.Bytetype = types.New(TUINT8);
            types.Bytetype.Sym = s;
            s.Def = asTypesNode(typenod(types.Bytetype));
            asNode(s.Def).Name;

            @new<Name>();
            dowidth(types.Bytetype); 

            // rune alias
            s = builtinpkg.Lookup("rune");
            types.Runetype = types.New(TINT32);
            types.Runetype.Sym = s;
            s.Def = asTypesNode(typenod(types.Runetype));
            asNode(s.Def).Name;

            @new<Name>();
            dowidth(types.Runetype); 

            // backend-dependent builtin types (e.g. int).
            {
                var s__prev1 = s;

                foreach (var (_, __s) in _addr_typedefs)
                {
                    s = __s;
                    var s1 = builtinpkg.Lookup(s.name);

                    var sameas = s.sameas32;
                    if (Widthptr == 8L)
                    {
                        sameas = s.sameas64;
                    }

                    simtype[s.etype] = sameas;
                    minfltval[s.etype] = minfltval[sameas];
                    maxfltval[s.etype] = maxfltval[sameas];
                    minintval[s.etype] = minintval[sameas];
                    maxintval[s.etype] = maxintval[sameas];

                    var t = types.New(s.etype);
                    t.Sym = s1;
                    types.Types[s.etype] = t;
                    s1.Def = asTypesNode(typenod(t));
                    asNode(s1.Def).Name;

                    @new<Name>();
                    s1.Origpkg = builtinpkg;

                    dowidth(t);

                }

                s = s__prev1;
            }
        }

        // finishUniverse makes the universe block visible within the current package.
        private static void finishUniverse()
        { 
            // Operationally, this is similar to a dot import of builtinpkg, except
            // that we silently skip symbols that are already declared in the
            // package block rather than emitting a redeclared symbol error.

            foreach (var (_, s) in builtinpkg.Syms)
            {
                if (s.Def == null)
                {
                    continue;
                }

                var s1 = lookup(s.Name);
                if (s1.Def != null)
                {
                    continue;
                }

                s1.Def = s.Def;
                s1.Block = s.Block;

            }
            nodfp = newname(lookup(".fp"));
            nodfp.Type = types.Types[TINT32];
            nodfp.SetClass(PPARAM);
            nodfp.Name.SetUsed(true);

        }
    }
}}}}
