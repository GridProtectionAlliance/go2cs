// Code generated from gen/386splitload.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 October 09 05:26:47 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewrite386splitload.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static bool rewriteValue386splitload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Op == Op386CMPBconstload) 
                return rewriteValue386splitload_Op386CMPBconstload(_addr_v);
            else if (v.Op == Op386CMPBload) 
                return rewriteValue386splitload_Op386CMPBload(_addr_v);
            else if (v.Op == Op386CMPLconstload) 
                return rewriteValue386splitload_Op386CMPLconstload(_addr_v);
            else if (v.Op == Op386CMPLload) 
                return rewriteValue386splitload_Op386CMPLload(_addr_v);
            else if (v.Op == Op386CMPWconstload) 
                return rewriteValue386splitload_Op386CMPWconstload(_addr_v);
            else if (v.Op == Op386CMPWload) 
                return rewriteValue386splitload_Op386CMPWload(_addr_v);
                        return false;

        }
        private static bool rewriteValue386splitload_Op386CMPBconstload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPBconstload {sym} [vo] ptr mem)
            // result: (CMPBconst (MOVBload {sym} [vo.Off32()] ptr mem) [vo.Val8()])
            while (true)
            {
                var vo = auxIntToValAndOff(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var mem = v_1;
                v.reset(Op386CMPBconst);
                v.AuxInt = int8ToAuxInt(vo.Val8());
                var v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
                v0.AuxInt = int32ToAuxInt(vo.Off32());
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValue386splitload_Op386CMPBload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPBload {sym} [off] ptr x mem)
            // result: (CMPB (MOVBload {sym} [off] ptr mem) x)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var x = v_1;
                var mem = v_2;
                v.reset(Op386CMPB);
                var v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValue386splitload_Op386CMPLconstload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPLconstload {sym} [vo] ptr mem)
            // result: (CMPLconst (MOVLload {sym} [vo.Off32()] ptr mem) [vo.Val32()])
            while (true)
            {
                var vo = auxIntToValAndOff(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var mem = v_1;
                v.reset(Op386CMPLconst);
                v.AuxInt = int32ToAuxInt(vo.Val32());
                var v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(vo.Off32());
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValue386splitload_Op386CMPLload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPLload {sym} [off] ptr x mem)
            // result: (CMPL (MOVLload {sym} [off] ptr mem) x)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var x = v_1;
                var mem = v_2;
                v.reset(Op386CMPL);
                var v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValue386splitload_Op386CMPWconstload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPWconstload {sym} [vo] ptr mem)
            // result: (CMPWconst (MOVWload {sym} [vo.Off32()] ptr mem) [vo.Val16()])
            while (true)
            {
                var vo = auxIntToValAndOff(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var mem = v_1;
                v.reset(Op386CMPWconst);
                v.AuxInt = int16ToAuxInt(vo.Val16());
                var v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v0.AuxInt = int32ToAuxInt(vo.Off32());
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValue386splitload_Op386CMPWload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (CMPWload {sym} [off] ptr x mem)
            // result: (CMPW (MOVWload {sym} [off] ptr mem) x)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                var ptr = v_0;
                var x = v_1;
                var mem = v_2;
                v.reset(Op386CMPW);
                var v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(sym);
                v0.AddArg2(ptr, mem);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteBlock386splitload(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            switch (b.Kind)
            {
            }
            return false;

        }
    }
}}}}
