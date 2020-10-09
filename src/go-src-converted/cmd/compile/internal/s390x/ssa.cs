// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2020 October 09 05:40:09 UTC
// import "cmd/compile/internal/s390x" ==> using s390x = go.cmd.compile.@internal.s390x_package
// Original source: C:\Go\src\cmd\compile\internal\s390x\ssa.go
using math = go.math_package;

using gc = go.cmd.compile.@internal.gc_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class s390x_package
    {
        // markMoves marks any MOVXconst ops that need to avoid clobbering flags.
        private static void ssaMarkMoves(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Block> _addr_b)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;

            var flive = b.FlagsLiveAtEnd;
            foreach (var (_, c) in b.ControlValues())
            {
                flive = c.Type.IsFlags() || flive;
            }            for (var i = len(b.Values) - 1L; i >= 0L; i--)
            {
                var v = b.Values[i];
                if (flive && v.Op == ssa.OpS390XMOVDconst)
                { 
                    // The "mark" is any non-nil Aux value.
                    v.Aux = v;

                }
                if (v.Type.IsFlags())
                {
                    flive = false;
                }
                foreach (var (_, a) in v.Args)
                {
                    if (a.Type.IsFlags())
                    {
                        flive = true;
                    }
                }
            }

        }

        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return s390x.AFMOVS;
                        break;
                    case 8L: 
                        return s390x.AFMOVD;
                        break;
                }

            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVB;
                        }
                        else
                        {
                            return s390x.AMOVBZ;
                        }

                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVH;
                        }
                        else
                        {
                            return s390x.AMOVHZ;
                        }

                        break;
                    case 4L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVW;
                        }
                        else
                        {
                            return s390x.AMOVWZ;
                        }

                        break;
                    case 8L: 
                        return s390x.AMOVD;
                        break;
                }

            }

            panic("bad load type");

        });

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            var width = t.Size();
            if (t.IsFloat())
            {
                switch (width)
                {
                    case 4L: 
                        return s390x.AFMOVS;
                        break;
                    case 8L: 
                        return s390x.AFMOVD;
                        break;
                }

            }
            else
            {
                switch (width)
                {
                    case 1L: 
                        return s390x.AMOVB;
                        break;
                    case 2L: 
                        return s390x.AMOVH;
                        break;
                    case 4L: 
                        return s390x.AMOVW;
                        break;
                    case 8L: 
                        return s390x.AMOVD;
                        break;
                }

            }

            panic("bad store type");

        });

        // moveByType returns the reg->reg move instruction of the given type.
        private static obj.As moveByType(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                return s390x.AFMOVD;
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVB;
                        }
                        else
                        {
                            return s390x.AMOVBZ;
                        }

                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVH;
                        }
                        else
                        {
                            return s390x.AMOVHZ;
                        }

                        break;
                    case 4L: 
                        if (t.IsSigned())
                        {
                            return s390x.AMOVW;
                        }
                        else
                        {
                            return s390x.AMOVWZ;
                        }

                        break;
                    case 8L: 
                        return s390x.AMOVD;
                        break;
                }

            }

            panic("bad load type");

        });

        // opregreg emits instructions for
        //     dest := dest(To) op src(From)
        // and also returns the created obj.Prog so it
        // may be further adjusted (offset, scale, etc).
        private static ptr<obj.Prog> opregreg(ptr<gc.SSAGenState> _addr_s, obj.As op, short dest, short src)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = dest;
            p.From.Reg = src;
            return _addr_p!;
        }

        // opregregimm emits instructions for
        //    dest := src(From) op off
        // and also returns the created obj.Prog so it
        // may be further adjusted (offset, scale, etc).
        private static ptr<obj.Prog> opregregimm(ptr<gc.SSAGenState> _addr_s, obj.As op, short dest, short src, long off)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(op);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = off;
            p.Reg = src;
            p.To.Reg = dest;
            p.To.Type = obj.TYPE_REG;
            return _addr_p!;
        }

        private static void ssaGenValue(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpS390XSLD || v.Op == ssa.OpS390XSLW || v.Op == ssa.OpS390XSRD || v.Op == ssa.OpS390XSRW || v.Op == ssa.OpS390XSRAD || v.Op == ssa.OpS390XSRAW || v.Op == ssa.OpS390XRLLG || v.Op == ssa.OpS390XRLL) 
                var r = v.Reg();
                var r1 = v.Args[0L].Reg();
                var r2 = v.Args[1L].Reg();
                if (r2 == s390x.REG_R0)
                {
                    v.Fatalf("cannot use R0 as shift value %s", v.LongString());
                }

                var p = opregreg(_addr_s, v.Op.Asm(), r, r2);
                if (r != r1)
                {
                    p.Reg = r1;
                }

            else if (v.Op == ssa.OpS390XRXSBG) 
                r1 = v.Reg();
                if (r1 != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                r2 = v.Args[1L].Reg();
                s390x.RotateParams i = v.Aux._<s390x.RotateParams>();
                p = s.Prog(v.Op.Asm());
                p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(i.Start));
                p.RestArgs = new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:int64(i.End)}, {Type:obj.TYPE_CONST,Offset:int64(i.Amount)}, {Type:obj.TYPE_REG,Reg:r2} });
                p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:r1);
            else if (v.Op == ssa.OpS390XADD || v.Op == ssa.OpS390XADDW || v.Op == ssa.OpS390XSUB || v.Op == ssa.OpS390XSUBW || v.Op == ssa.OpS390XAND || v.Op == ssa.OpS390XANDW || v.Op == ssa.OpS390XOR || v.Op == ssa.OpS390XORW || v.Op == ssa.OpS390XXOR || v.Op == ssa.OpS390XXORW) 
                r = v.Reg();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                p = opregreg(_addr_s, v.Op.Asm(), r, r2);
                if (r != r1)
                {
                    p.Reg = r1;
                }

            else if (v.Op == ssa.OpS390XADDC) 
                r1 = v.Reg0();
                r2 = v.Args[0L].Reg();
                var r3 = v.Args[1L].Reg();
                if (r1 == r2)
                {
                    r2 = r3;
                    r3 = r2;

                }

                p = opregreg(_addr_s, v.Op.Asm(), r1, r2);
                if (r3 != r1)
                {
                    p.Reg = r3;
                }

            else if (v.Op == ssa.OpS390XSUBC) 
                r1 = v.Reg0();
                r2 = v.Args[0L].Reg();
                r3 = v.Args[1L].Reg();
                p = opregreg(_addr_s, v.Op.Asm(), r1, r3);
                if (r1 != r2)
                {
                    p.Reg = r2;
                }

            else if (v.Op == ssa.OpS390XADDE || v.Op == ssa.OpS390XSUBE) 
                r1 = v.Reg0();
                if (r1 != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                r2 = v.Args[1L].Reg();
                opregreg(_addr_s, v.Op.Asm(), r1, r2);
            else if (v.Op == ssa.OpS390XADDCconst) 
                r1 = v.Reg0();
                r3 = v.Args[0L].Reg();
                var i2 = int64(int16(v.AuxInt));
                opregregimm(_addr_s, v.Op.Asm(), r1, r3, i2); 
                // 2-address opcode arithmetic
            else if (v.Op == ssa.OpS390XMULLD || v.Op == ssa.OpS390XMULLW || v.Op == ssa.OpS390XMULHD || v.Op == ssa.OpS390XMULHDU || v.Op == ssa.OpS390XFMULS || v.Op == ssa.OpS390XFMUL || v.Op == ssa.OpS390XFDIVS || v.Op == ssa.OpS390XFDIV) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                opregreg(_addr_s, v.Op.Asm(), r, v.Args[1L].Reg());
            else if (v.Op == ssa.OpS390XFSUBS || v.Op == ssa.OpS390XFSUB || v.Op == ssa.OpS390XFADDS || v.Op == ssa.OpS390XFADD) 
                r = v.Reg0();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                opregreg(_addr_s, v.Op.Asm(), r, v.Args[1L].Reg());
            else if (v.Op == ssa.OpS390XMLGR) 
                // MLGR Rx R3 -> R2:R3
                var r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                if (r1 != s390x.REG_R3)
                {
                    v.Fatalf("We require the multiplcand to be stored in R3 for MLGR %s", v.LongString());
                }

                p = s.Prog(s390x.AMLGR);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r0;
                p.To.Reg = s390x.REG_R2;
                p.To.Type = obj.TYPE_REG;
            else if (v.Op == ssa.OpS390XFMADD || v.Op == ssa.OpS390XFMADDS || v.Op == ssa.OpS390XFMSUB || v.Op == ssa.OpS390XFMSUBS) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                r1 = v.Args[1L].Reg();
                r2 = v.Args[2L].Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.Reg = r2;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XFIDBR) 
                switch (v.AuxInt)
                {
                    case 0L: 

                    case 1L: 

                    case 3L: 

                    case 4L: 

                    case 5L: 

                    case 6L: 

                    case 7L: 
                        opregregimm(_addr_s, v.Op.Asm(), v.Reg(), v.Args[0L].Reg(), v.AuxInt);
                        break;
                    default: 
                        v.Fatalf("invalid FIDBR mask: %v", v.AuxInt);
                        break;
                }
            else if (v.Op == ssa.OpS390XCPSDR) 
                p = opregreg(_addr_s, v.Op.Asm(), v.Reg(), v.Args[1L].Reg());
                p.Reg = v.Args[0L].Reg();
            else if (v.Op == ssa.OpS390XDIVD || v.Op == ssa.OpS390XDIVW || v.Op == ssa.OpS390XDIVDU || v.Op == ssa.OpS390XDIVWU || v.Op == ssa.OpS390XMODD || v.Op == ssa.OpS390XMODW || v.Op == ssa.OpS390XMODDU || v.Op == ssa.OpS390XMODWU) 

                // TODO(mundaym): use the temp registers every time like x86 does with AX?
                var dividend = v.Args[0L].Reg();
                var divisor = v.Args[1L].Reg(); 

                // CPU faults upon signed overflow, which occurs when most
                // negative int is divided by -1.
                ptr<obj.Prog> j;
                if (v.Op == ssa.OpS390XDIVD || v.Op == ssa.OpS390XDIVW || v.Op == ssa.OpS390XMODD || v.Op == ssa.OpS390XMODW)
                {
                    ptr<obj.Prog> c;
                    c = s.Prog(s390x.ACMP);
                    j = s.Prog(s390x.ABEQ);

                    c.From.Type = obj.TYPE_REG;
                    c.From.Reg = divisor;
                    c.To.Type = obj.TYPE_CONST;
                    c.To.Offset = -1L;

                    j.To.Type = obj.TYPE_BRANCH;
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = divisor;
                p.Reg = 0L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = dividend; 

                // signed division, rest of the check for -1 case
                if (j != null)
                {
                    var j2 = s.Prog(s390x.ABR);
                    j2.To.Type = obj.TYPE_BRANCH;

                    ptr<obj.Prog> n;
                    if (v.Op == ssa.OpS390XDIVD || v.Op == ssa.OpS390XDIVW)
                    { 
                        // n * -1 = -n
                        n = s.Prog(s390x.ANEG);
                        n.To.Type = obj.TYPE_REG;
                        n.To.Reg = dividend;

                    }
                    else
                    { 
                        // n % -1 == 0
                        n = s.Prog(s390x.AXOR);
                        n.From.Type = obj.TYPE_REG;
                        n.From.Reg = dividend;
                        n.To.Type = obj.TYPE_REG;
                        n.To.Reg = dividend;

                    }

                    j.To.Val = n;
                    j2.To.Val = s.Pc();

                }

            else if (v.Op == ssa.OpS390XADDconst || v.Op == ssa.OpS390XADDWconst) 
                opregregimm(_addr_s, v.Op.Asm(), v.Reg(), v.Args[0L].Reg(), v.AuxInt);
            else if (v.Op == ssa.OpS390XMULLDconst || v.Op == ssa.OpS390XMULLWconst || v.Op == ssa.OpS390XSUBconst || v.Op == ssa.OpS390XSUBWconst || v.Op == ssa.OpS390XANDconst || v.Op == ssa.OpS390XANDWconst || v.Op == ssa.OpS390XORconst || v.Op == ssa.OpS390XORWconst || v.Op == ssa.OpS390XXORconst || v.Op == ssa.OpS390XXORWconst) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XSLDconst || v.Op == ssa.OpS390XSLWconst || v.Op == ssa.OpS390XSRDconst || v.Op == ssa.OpS390XSRWconst || v.Op == ssa.OpS390XSRADconst || v.Op == ssa.OpS390XSRAWconst || v.Op == ssa.OpS390XRLLGconst || v.Op == ssa.OpS390XRLLconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                r = v.Reg();
                r1 = v.Args[0L].Reg();
                if (r != r1)
                {
                    p.Reg = r1;
                }

                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XMOVDaddridx) 
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();
                p = s.Prog(s390x.AMOVD);
                p.From.Scale = 1L;
                if (i == s390x.REGSP)
                {
                    r = i;
                    i = r;

                }

                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = r;
                p.From.Index = i;
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XMOVDaddr) 
                p = s.Prog(s390x.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XCMP || v.Op == ssa.OpS390XCMPW || v.Op == ssa.OpS390XCMPU || v.Op == ssa.OpS390XCMPWU) 
                opregreg(_addr_s, v.Op.Asm(), v.Args[1L].Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpS390XFCMPS || v.Op == ssa.OpS390XFCMP) 
                opregreg(_addr_s, v.Op.Asm(), v.Args[1L].Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpS390XCMPconst || v.Op == ssa.OpS390XCMPWconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = v.AuxInt;
            else if (v.Op == ssa.OpS390XCMPUconst || v.Op == ssa.OpS390XCMPWUconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = int64(uint32(v.AuxInt));
            else if (v.Op == ssa.OpS390XMOVDconst) 
                var x = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = x;
            else if (v.Op == ssa.OpS390XFMOVSconst || v.Op == ssa.OpS390XFMOVDconst) 
                x = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = x;
            else if (v.Op == ssa.OpS390XADDWload || v.Op == ssa.OpS390XADDload || v.Op == ssa.OpS390XMULLWload || v.Op == ssa.OpS390XMULLDload || v.Op == ssa.OpS390XSUBWload || v.Op == ssa.OpS390XSUBload || v.Op == ssa.OpS390XANDWload || v.Op == ssa.OpS390XANDload || v.Op == ssa.OpS390XORWload || v.Op == ssa.OpS390XORload || v.Op == ssa.OpS390XXORWload || v.Op == ssa.OpS390XXORload) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[1L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XMOVDload || v.Op == ssa.OpS390XMOVWZload || v.Op == ssa.OpS390XMOVHZload || v.Op == ssa.OpS390XMOVBZload || v.Op == ssa.OpS390XMOVDBRload || v.Op == ssa.OpS390XMOVWBRload || v.Op == ssa.OpS390XMOVHBRload || v.Op == ssa.OpS390XMOVBload || v.Op == ssa.OpS390XMOVHload || v.Op == ssa.OpS390XMOVWload || v.Op == ssa.OpS390XFMOVSload || v.Op == ssa.OpS390XFMOVDload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XMOVBZloadidx || v.Op == ssa.OpS390XMOVHZloadidx || v.Op == ssa.OpS390XMOVWZloadidx || v.Op == ssa.OpS390XMOVBloadidx || v.Op == ssa.OpS390XMOVHloadidx || v.Op == ssa.OpS390XMOVWloadidx || v.Op == ssa.OpS390XMOVDloadidx || v.Op == ssa.OpS390XMOVHBRloadidx || v.Op == ssa.OpS390XMOVWBRloadidx || v.Op == ssa.OpS390XMOVDBRloadidx || v.Op == ssa.OpS390XFMOVSloadidx || v.Op == ssa.OpS390XFMOVDloadidx) 
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();
                if (i == s390x.REGSP)
                {
                    r = i;
                    i = r;

                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r;
                p.From.Scale = 1L;
                p.From.Index = i;
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XMOVBstore || v.Op == ssa.OpS390XMOVHstore || v.Op == ssa.OpS390XMOVWstore || v.Op == ssa.OpS390XMOVDstore || v.Op == ssa.OpS390XMOVHBRstore || v.Op == ssa.OpS390XMOVWBRstore || v.Op == ssa.OpS390XMOVDBRstore || v.Op == ssa.OpS390XFMOVSstore || v.Op == ssa.OpS390XFMOVDstore) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XMOVBstoreidx || v.Op == ssa.OpS390XMOVHstoreidx || v.Op == ssa.OpS390XMOVWstoreidx || v.Op == ssa.OpS390XMOVDstoreidx || v.Op == ssa.OpS390XMOVHBRstoreidx || v.Op == ssa.OpS390XMOVWBRstoreidx || v.Op == ssa.OpS390XMOVDBRstoreidx || v.Op == ssa.OpS390XFMOVSstoreidx || v.Op == ssa.OpS390XFMOVDstoreidx) 
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();
                if (i == s390x.REGSP)
                {
                    r = i;
                    i = r;

                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = r;
                p.To.Scale = 1L;
                p.To.Index = i;
                gc.AddAux(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XMOVDstoreconst || v.Op == ssa.OpS390XMOVWstoreconst || v.Op == ssa.OpS390XMOVHstoreconst || v.Op == ssa.OpS390XMOVBstoreconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                var sc = v.AuxValAndOff();
                p.From.Offset = sc.Val();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux2(_addr_p.To, v, sc.Off());
            else if (v.Op == ssa.OpS390XMOVBreg || v.Op == ssa.OpS390XMOVHreg || v.Op == ssa.OpS390XMOVWreg || v.Op == ssa.OpS390XMOVBZreg || v.Op == ssa.OpS390XMOVHZreg || v.Op == ssa.OpS390XMOVWZreg || v.Op == ssa.OpS390XLDGR || v.Op == ssa.OpS390XLGDR || v.Op == ssa.OpS390XCEFBRA || v.Op == ssa.OpS390XCDFBRA || v.Op == ssa.OpS390XCEGBRA || v.Op == ssa.OpS390XCDGBRA || v.Op == ssa.OpS390XCFEBRA || v.Op == ssa.OpS390XCFDBRA || v.Op == ssa.OpS390XCGEBRA || v.Op == ssa.OpS390XCGDBRA || v.Op == ssa.OpS390XCELFBR || v.Op == ssa.OpS390XCDLFBR || v.Op == ssa.OpS390XCELGBR || v.Op == ssa.OpS390XCDLGBR || v.Op == ssa.OpS390XCLFEBR || v.Op == ssa.OpS390XCLFDBR || v.Op == ssa.OpS390XCLGEBR || v.Op == ssa.OpS390XCLGDBR || v.Op == ssa.OpS390XLDEBR || v.Op == ssa.OpS390XLEDBR || v.Op == ssa.OpS390XFNEG || v.Op == ssa.OpS390XFNEGS || v.Op == ssa.OpS390XLPDFR || v.Op == ssa.OpS390XLNDFR) 
                opregreg(_addr_s, v.Op.Asm(), v.Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpS390XCLEAR) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                sc = v.AuxValAndOff();
                p.From.Offset = sc.Val();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux2(_addr_p.To, v, sc.Off());
            else if (v.Op == ssa.OpCopy) 
                if (v.Type.IsMemory())
                {
                    return ;
                }

                x = v.Args[0L].Reg();
                var y = v.Reg();
                if (x != y)
                {
                    opregreg(_addr_s, moveByType(_addr_v.Type), y, x);
                }

            else if (v.Op == ssa.OpLoadReg) 
                if (v.Type.IsFlags())
                {
                    v.Fatalf("load flags not implemented: %v", v.LongString());
                    return ;
                }

                p = s.Prog(loadByType(_addr_v.Type));
                gc.AddrAuto(_addr_p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpStoreReg) 
                if (v.Type.IsFlags())
                {
                    v.Fatalf("store flags not implemented: %v", v.LongString());
                    return ;
                }

                p = s.Prog(storeByType(_addr_v.Type));
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddrAuto(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XLoweredGetClosurePtr) 
                // Closure pointer is R12 (already)
                gc.CheckLoweredGetClosurePtr(v);
            else if (v.Op == ssa.OpS390XLoweredRound32F || v.Op == ssa.OpS390XLoweredRound64F)             else if (v.Op == ssa.OpS390XLoweredGetG) 
                r = v.Reg();
                p = s.Prog(s390x.AMOVD);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = s390x.REGG;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XLoweredGetCallerSP) 
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(s390x.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XLoweredGetCallerPC) 
                p = s.Prog(obj.AGETCALLERPC);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XCALLstatic || v.Op == ssa.OpS390XCALLclosure || v.Op == ssa.OpS390XCALLinter) 
                s.Call(v);
            else if (v.Op == ssa.OpS390XLoweredWB) 
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = v.Aux._<ptr<obj.LSym>>();
            else if (v.Op == ssa.OpS390XLoweredPanicBoundsA || v.Op == ssa.OpS390XLoweredPanicBoundsB || v.Op == ssa.OpS390XLoweredPanicBoundsC) 
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.BoundsCheckFunc[v.AuxInt];
                s.UseArgs(16L); // space used in callee args area by assembly stubs
            else if (v.Op == ssa.OpS390XFLOGR || v.Op == ssa.OpS390XPOPCNT || v.Op == ssa.OpS390XNEG || v.Op == ssa.OpS390XNEGW || v.Op == ssa.OpS390XMOVWBR || v.Op == ssa.OpS390XMOVDBR) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XNOT || v.Op == ssa.OpS390XNOTW) 
                v.Fatalf("NOT/NOTW generated %s", v.LongString());
            else if (v.Op == ssa.OpS390XSumBytes2 || v.Op == ssa.OpS390XSumBytes4 || v.Op == ssa.OpS390XSumBytes8) 
                v.Fatalf("SumBytes generated %s", v.LongString());
            else if (v.Op == ssa.OpS390XLOCGR) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(v.Aux._<s390x.CCMask>());
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpS390XFSQRT) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpS390XLTDBR || v.Op == ssa.OpS390XLTEBR) 
                opregreg(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpS390XInvertFlags) 
                v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpS390XFlagEQ || v.Op == ssa.OpS390XFlagLT || v.Op == ssa.OpS390XFlagGT || v.Op == ssa.OpS390XFlagOV) 
                v.Fatalf("Flag* ops should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpS390XAddTupleFirst32 || v.Op == ssa.OpS390XAddTupleFirst64) 
                v.Fatalf("AddTupleFirst* should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpS390XLoweredNilCheck) 
                // Issue a load which will fault if the input is nil.
                p = s.Prog(s390x.AMOVBZ);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = s390x.REGTMP;
                if (logopt.Enabled())
                {
                    logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
                }

                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");

                }

            else if (v.Op == ssa.OpS390XMVC) 
                var vo = v.AuxValAndOff();
                p = s.Prog(s390x.AMVC);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = vo.Val();
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[1].Reg(),Offset:vo.Off(),));
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Offset = vo.Off();
            else if (v.Op == ssa.OpS390XSTMG2 || v.Op == ssa.OpS390XSTMG3 || v.Op == ssa.OpS390XSTMG4 || v.Op == ssa.OpS390XSTM2 || v.Op == ssa.OpS390XSTM3 || v.Op == ssa.OpS390XSTM4) 
                {
                    s390x.RotateParams i__prev1 = i;

                    for (i = 2L; i < len(v.Args) - 1L; i++)
                    {
                        if (v.Args[i].Reg() != v.Args[i - 1L].Reg() + 1L)
                        {
                            v.Fatalf("invalid store multiple %s", v.LongString());
                        }

                    }


                    i = i__prev1;
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[len(v.Args) - 2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XLoweredMove) 
                // Inputs must be valid pointers to memory,
                // so adjust arg0 and arg1 as part of the expansion.
                // arg2 should be src+size,
                //
                // mvc: MVC  $256, 0(R2), 0(R1)
                //      MOVD $256(R1), R1
                //      MOVD $256(R2), R2
                //      CMP  R2, Rarg2
                //      BNE  mvc
                //      MVC  $rem, 0(R2), 0(R1) // if rem > 0
                // arg2 is the last address to move in the loop + 256
                var mvc = s.Prog(s390x.AMVC);
                mvc.From.Type = obj.TYPE_CONST;
                mvc.From.Offset = 256L;
                mvc.SetFrom3(new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[1].Reg()));
                mvc.To.Type = obj.TYPE_MEM;
                mvc.To.Reg = v.Args[0L].Reg();

                {
                    s390x.RotateParams i__prev1 = i;

                    for (i = 0L; i < 2L; i++)
                    {
                        var movd = s.Prog(s390x.AMOVD);
                        movd.From.Type = obj.TYPE_ADDR;
                        movd.From.Reg = v.Args[i].Reg();
                        movd.From.Offset = 256L;
                        movd.To.Type = obj.TYPE_REG;
                        movd.To.Reg = v.Args[i].Reg();
                    }


                    i = i__prev1;
                }

                var cmpu = s.Prog(s390x.ACMPU);
                cmpu.From.Reg = v.Args[1L].Reg();
                cmpu.From.Type = obj.TYPE_REG;
                cmpu.To.Reg = v.Args[2L].Reg();
                cmpu.To.Type = obj.TYPE_REG;

                var bne = s.Prog(s390x.ABLT);
                bne.To.Type = obj.TYPE_BRANCH;
                gc.Patch(bne, mvc);

                if (v.AuxInt > 0L)
                {
                    mvc = s.Prog(s390x.AMVC);
                    mvc.From.Type = obj.TYPE_CONST;
                    mvc.From.Offset = v.AuxInt;
                    mvc.SetFrom3(new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[1].Reg()));
                    mvc.To.Type = obj.TYPE_MEM;
                    mvc.To.Reg = v.Args[0L].Reg();
                }

            else if (v.Op == ssa.OpS390XLoweredZero) 
                // Input must be valid pointers to memory,
                // so adjust arg0 as part of the expansion.
                // arg1 should be src+size,
                //
                // clear: CLEAR $256, 0(R1)
                //        MOVD  $256(R1), R1
                //        CMP   R1, Rarg1
                //        BNE   clear
                //        CLEAR $rem, 0(R1) // if rem > 0
                // arg1 is the last address to zero in the loop + 256
                var clear = s.Prog(s390x.ACLEAR);
                clear.From.Type = obj.TYPE_CONST;
                clear.From.Offset = 256L;
                clear.To.Type = obj.TYPE_MEM;
                clear.To.Reg = v.Args[0L].Reg();

                movd = s.Prog(s390x.AMOVD);
                movd.From.Type = obj.TYPE_ADDR;
                movd.From.Reg = v.Args[0L].Reg();
                movd.From.Offset = 256L;
                movd.To.Type = obj.TYPE_REG;
                movd.To.Reg = v.Args[0L].Reg();

                cmpu = s.Prog(s390x.ACMPU);
                cmpu.From.Reg = v.Args[0L].Reg();
                cmpu.From.Type = obj.TYPE_REG;
                cmpu.To.Reg = v.Args[1L].Reg();
                cmpu.To.Type = obj.TYPE_REG;

                bne = s.Prog(s390x.ABLT);
                bne.To.Type = obj.TYPE_BRANCH;
                gc.Patch(bne, clear);

                if (v.AuxInt > 0L)
                {
                    clear = s.Prog(s390x.ACLEAR);
                    clear.From.Type = obj.TYPE_CONST;
                    clear.From.Offset = v.AuxInt;
                    clear.To.Type = obj.TYPE_MEM;
                    clear.To.Reg = v.Args[0L].Reg();
                }

            else if (v.Op == ssa.OpS390XMOVBZatomicload || v.Op == ssa.OpS390XMOVWZatomicload || v.Op == ssa.OpS390XMOVDatomicload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
            else if (v.Op == ssa.OpS390XMOVBatomicstore || v.Op == ssa.OpS390XMOVWatomicstore || v.Op == ssa.OpS390XMOVDatomicstore) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XLANfloor || v.Op == ssa.OpS390XLAOfloor) 
                r = v.Args[0L].Reg(); // clobbered, assumed R1 in comments

                // Round ptr down to nearest multiple of 4.
                // ANDW $~3, R1
                var ptr = s.Prog(s390x.AANDW);
                ptr.From.Type = obj.TYPE_CONST;
                ptr.From.Offset = 0xfffffffcUL;
                ptr.To.Type = obj.TYPE_REG;
                ptr.To.Reg = r; 

                // Redirect output of LA(N|O) into R1 since it is clobbered anyway.
                // LA(N|O) Rx, R1, 0(R1)
                var op = s.Prog(v.Op.Asm());
                op.From.Type = obj.TYPE_REG;
                op.From.Reg = v.Args[1L].Reg();
                op.Reg = r;
                op.To.Type = obj.TYPE_MEM;
                op.To.Reg = r;
            else if (v.Op == ssa.OpS390XLAA || v.Op == ssa.OpS390XLAAG) 
                p = s.Prog(v.Op.Asm());
                p.Reg = v.Reg0();
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
            else if (v.Op == ssa.OpS390XLoweredAtomicCas32 || v.Op == ssa.OpS390XLoweredAtomicCas64) 
                // Convert the flags output of CS{,G} into a bool.
                //    CS{,G} arg1, arg2, arg0
                //    MOVD   $0, ret
                //    BNE    2(PC)
                //    MOVD   $1, ret
                //    NOP (so the BNE has somewhere to land)

                // CS{,G} arg1, arg2, arg0
                var cs = s.Prog(v.Op.Asm());
                cs.From.Type = obj.TYPE_REG;
                cs.From.Reg = v.Args[1L].Reg(); // old
                cs.Reg = v.Args[2L].Reg(); // new
                cs.To.Type = obj.TYPE_MEM;
                cs.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_cs.To, v); 

                // MOVD $0, ret
                movd = s.Prog(s390x.AMOVD);
                movd.From.Type = obj.TYPE_CONST;
                movd.From.Offset = 0L;
                movd.To.Type = obj.TYPE_REG;
                movd.To.Reg = v.Reg0(); 

                // BNE 2(PC)
                bne = s.Prog(s390x.ABNE);
                bne.To.Type = obj.TYPE_BRANCH; 

                // MOVD $1, ret
                movd = s.Prog(s390x.AMOVD);
                movd.From.Type = obj.TYPE_CONST;
                movd.From.Offset = 1L;
                movd.To.Type = obj.TYPE_REG;
                movd.To.Reg = v.Reg0(); 

                // NOP (so the BNE has somewhere to land)
                var nop = s.Prog(obj.ANOP);
                gc.Patch(bne, nop);
            else if (v.Op == ssa.OpS390XLoweredAtomicExchange32 || v.Op == ssa.OpS390XLoweredAtomicExchange64) 
                // Loop until the CS{,G} succeeds.
                //     MOV{WZ,D} arg0, ret
                // cs: CS{,G}    ret, arg1, arg0
                //     BNE       cs

                // MOV{WZ,D} arg0, ret
                var load = s.Prog(loadByType(_addr_v.Type.FieldType(0L)));
                load.From.Type = obj.TYPE_MEM;
                load.From.Reg = v.Args[0L].Reg();
                load.To.Type = obj.TYPE_REG;
                load.To.Reg = v.Reg0();
                gc.AddAux(_addr_load.From, v); 

                // CS{,G} ret, arg1, arg0
                cs = s.Prog(v.Op.Asm());
                cs.From.Type = obj.TYPE_REG;
                cs.From.Reg = v.Reg0(); // old
                cs.Reg = v.Args[1L].Reg(); // new
                cs.To.Type = obj.TYPE_MEM;
                cs.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_cs.To, v); 

                // BNE cs
                bne = s.Prog(s390x.ABNE);
                bne.To.Type = obj.TYPE_BRANCH;
                gc.Patch(bne, cs);
            else if (v.Op == ssa.OpS390XSYNC) 
                s.Prog(s390x.ASYNC);
            else if (v.Op == ssa.OpClobber)             else 
                v.Fatalf("genValue not implemented: %s", v.LongString());
            
        }

        private static obj.As blockAsm(ptr<ssa.Block> _addr_b) => func((_, panic, __) =>
        {
            ref ssa.Block b = ref _addr_b.val;


            if (b.Kind == ssa.BlockS390XBRC) 
                return s390x.ABRC;
            else if (b.Kind == ssa.BlockS390XCRJ) 
                return s390x.ACRJ;
            else if (b.Kind == ssa.BlockS390XCGRJ) 
                return s390x.ACGRJ;
            else if (b.Kind == ssa.BlockS390XCLRJ) 
                return s390x.ACLRJ;
            else if (b.Kind == ssa.BlockS390XCLGRJ) 
                return s390x.ACLGRJ;
            else if (b.Kind == ssa.BlockS390XCIJ) 
                return s390x.ACIJ;
            else if (b.Kind == ssa.BlockS390XCGIJ) 
                return s390x.ACGIJ;
            else if (b.Kind == ssa.BlockS390XCLIJ) 
                return s390x.ACLIJ;
            else if (b.Kind == ssa.BlockS390XCLGIJ) 
                return s390x.ACLGIJ;
                        b.Fatalf("blockAsm not implemented: %s", b.LongString());
            panic("unreachable");

        });

        private static void ssaGenBlock(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref ssa.Block next = ref _addr_next.val;
 
            // Handle generic blocks first.

            if (b.Kind == ssa.BlockPlain) 
                if (b.Succs[0L].Block() != next)
                {
                    var p = s.Prog(s390x.ABR);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }

                return ;
            else if (b.Kind == ssa.BlockDefer) 
                // defer returns in R3:
                // 0 if we should continue executing
                // 1 if we should jump to deferreturn call
                p = s.Br(s390x.ACIJ, b.Succs[1L].Block());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(s390x.NotEqual & s390x.NotUnordered); // unordered is not possible
                p.Reg = s390x.REG_R3;
                p.RestArgs = new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:0} });
                if (b.Succs[0L].Block() != next)
                {
                    s.Br(s390x.ABR, b.Succs[0L].Block());
                }

                return ;
            else if (b.Kind == ssa.BlockExit) 
                return ;
            else if (b.Kind == ssa.BlockRet) 
                s.Prog(obj.ARET);
                return ;
            else if (b.Kind == ssa.BlockRetJmp) 
                p = s.Prog(s390x.ABR);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = b.Aux._<ptr<obj.LSym>>();
                return ;
            // Handle s390x-specific blocks. These blocks all have a
            // condition code mask in the Aux value and 2 successors.
            array<ptr<ssa.Block>> succs = new array<ptr<ssa.Block>>(new ptr<ssa.Block>[] { b.Succs[0].Block(), b.Succs[1].Block() });
            s390x.CCMask mask = b.Aux._<s390x.CCMask>(); 

            // TODO: take into account Likely property for forward/backward
            // branches. We currently can't do this because we don't know
            // whether a block has already been emitted. In general forward
            // branches are assumed 'not taken' and backward branches are
            // assumed 'taken'.
            if (next == succs[0L])
            {
                succs[0L] = succs[1L];
                succs[1L] = succs[0L];
                mask = mask.Inverse();

            }

            p = s.Br(blockAsm(_addr_b), succs[0L]);

            if (b.Kind == ssa.BlockS390XBRC) 
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(mask);
            else if (b.Kind == ssa.BlockS390XCGRJ || b.Kind == ssa.BlockS390XCRJ || b.Kind == ssa.BlockS390XCLGRJ || b.Kind == ssa.BlockS390XCLRJ) 
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(mask & s390x.NotUnordered); // unordered is not possible
                p.Reg = b.Controls[0L].Reg();
                p.RestArgs = new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_REG,Reg:b.Controls[1].Reg()} });
            else if (b.Kind == ssa.BlockS390XCGIJ || b.Kind == ssa.BlockS390XCIJ) 
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(mask & s390x.NotUnordered); // unordered is not possible
                p.Reg = b.Controls[0L].Reg();
                p.RestArgs = new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:int64(int8(b.AuxInt))} });
            else if (b.Kind == ssa.BlockS390XCLGIJ || b.Kind == ssa.BlockS390XCLIJ) 
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(mask & s390x.NotUnordered); // unordered is not possible
                p.Reg = b.Controls[0L].Reg();
                p.RestArgs = new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:int64(uint8(b.AuxInt))} });
            else 
                b.Fatalf("branch not implemented: %s", b.LongString());
                        if (next != succs[1L])
            {
                s.Br(s390x.ABR, succs[1L]);
            }

        }
    }
}}}}
