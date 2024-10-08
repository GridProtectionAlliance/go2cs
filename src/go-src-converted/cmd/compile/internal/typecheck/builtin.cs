// Code generated by mkbuiltin.go. DO NOT EDIT.

// package typecheck -- go2cs converted at 2022 March 13 05:59:19 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\builtin.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;

public static partial class typecheck_package {



// Not inlining this function removes a significant chunk of init code.
//go:noinline
private static ptr<types.Type> newSig(slice<ptr<types.Field>> @params, slice<ptr<types.Field>> results) {
    return _addr_types.NewSignature(types.NoPkg, null, null, params, results)!;
}

private static slice<ptr<types.Field>> @params(params ptr<ptr<types.Type>>[] _addr_tlist) {
    tlist = tlist.Clone();
    ref types.Type tlist = ref _addr_tlist.val;

    var flist = make_slice<ptr<types.Field>>(len(tlist));
    foreach (var (i, typ) in tlist) {
        flist[i] = types.NewField(src.NoXPos, null, typ);
    }    return flist;
}

private static slice<ptr<types.Type>> runtimeTypes() {
    array<ptr<types.Type>> typs = new array<ptr<types.Type>>(145);
    typs[0] = types.ByteType;
    typs[1] = types.NewPtr(typs[0]);
    typs[2] = types.Types[types.TANY];
    typs[3] = types.NewPtr(typs[2]);
    typs[4] = newSig(params(_addr_typs[1]), params(_addr_typs[3]));
    typs[5] = types.Types[types.TUINTPTR];
    typs[6] = types.Types[types.TBOOL];
    typs[7] = types.Types[types.TUNSAFEPTR];
    typs[8] = newSig(params(_addr_typs[5], typs[1], typs[6]), params(_addr_typs[7]));
    typs[9] = newSig(null, null);
    typs[10] = types.Types[types.TINTER];
    typs[11] = newSig(params(_addr_typs[10]), null);
    typs[12] = types.Types[types.TINT32];
    typs[13] = types.NewPtr(typs[12]);
    typs[14] = newSig(params(_addr_typs[13]), params(_addr_typs[10]));
    typs[15] = types.Types[types.TINT];
    typs[16] = newSig(params(_addr_typs[15], typs[15]), null);
    typs[17] = types.Types[types.TUINT];
    typs[18] = newSig(params(_addr_typs[17], typs[15]), null);
    typs[19] = newSig(params(_addr_typs[6]), null);
    typs[20] = types.Types[types.TFLOAT64];
    typs[21] = newSig(params(_addr_typs[20]), null);
    typs[22] = types.Types[types.TINT64];
    typs[23] = newSig(params(_addr_typs[22]), null);
    typs[24] = types.Types[types.TUINT64];
    typs[25] = newSig(params(_addr_typs[24]), null);
    typs[26] = types.Types[types.TCOMPLEX128];
    typs[27] = newSig(params(_addr_typs[26]), null);
    typs[28] = types.Types[types.TSTRING];
    typs[29] = newSig(params(_addr_typs[28]), null);
    typs[30] = newSig(params(_addr_typs[2]), null);
    typs[31] = newSig(params(_addr_typs[5]), null);
    typs[32] = types.NewArray(typs[0], 32);
    typs[33] = types.NewPtr(typs[32]);
    typs[34] = newSig(params(_addr_typs[33], typs[28], typs[28]), params(_addr_typs[28]));
    typs[35] = newSig(params(_addr_typs[33], typs[28], typs[28], typs[28]), params(_addr_typs[28]));
    typs[36] = newSig(params(_addr_typs[33], typs[28], typs[28], typs[28], typs[28]), params(_addr_typs[28]));
    typs[37] = newSig(params(_addr_typs[33], typs[28], typs[28], typs[28], typs[28], typs[28]), params(_addr_typs[28]));
    typs[38] = types.NewSlice(typs[28]);
    typs[39] = newSig(params(_addr_typs[33], typs[38]), params(_addr_typs[28]));
    typs[40] = newSig(params(_addr_typs[28], typs[28]), params(_addr_typs[15]));
    typs[41] = types.NewArray(typs[0], 4);
    typs[42] = types.NewPtr(typs[41]);
    typs[43] = newSig(params(_addr_typs[42], typs[22]), params(_addr_typs[28]));
    typs[44] = newSig(params(_addr_typs[33], typs[1], typs[15]), params(_addr_typs[28]));
    typs[45] = newSig(params(_addr_typs[1], typs[15]), params(_addr_typs[28]));
    typs[46] = types.RuneType;
    typs[47] = types.NewSlice(typs[46]);
    typs[48] = newSig(params(_addr_typs[33], typs[47]), params(_addr_typs[28]));
    typs[49] = types.NewSlice(typs[0]);
    typs[50] = newSig(params(_addr_typs[33], typs[28]), params(_addr_typs[49]));
    typs[51] = types.NewArray(typs[46], 32);
    typs[52] = types.NewPtr(typs[51]);
    typs[53] = newSig(params(_addr_typs[52], typs[28]), params(_addr_typs[47]));
    typs[54] = newSig(params(_addr_typs[3], typs[15], typs[3], typs[15], typs[5]), params(_addr_typs[15]));
    typs[55] = newSig(params(_addr_typs[28], typs[15]), params(_addr_typs[46], typs[15]));
    typs[56] = newSig(params(_addr_typs[28]), params(_addr_typs[15]));
    typs[57] = newSig(params(_addr_typs[1], typs[2]), params(_addr_typs[2]));
    typs[58] = types.Types[types.TUINT16];
    typs[59] = newSig(params(_addr_typs[58]), params(_addr_typs[7]));
    typs[60] = types.Types[types.TUINT32];
    typs[61] = newSig(params(_addr_typs[60]), params(_addr_typs[7]));
    typs[62] = newSig(params(_addr_typs[24]), params(_addr_typs[7]));
    typs[63] = newSig(params(_addr_typs[28]), params(_addr_typs[7]));
    typs[64] = types.Types[types.TUINT8];
    typs[65] = types.NewSlice(typs[64]);
    typs[66] = newSig(params(_addr_typs[65]), params(_addr_typs[7]));
    typs[67] = newSig(params(_addr_typs[1], typs[3]), params(_addr_typs[2]));
    typs[68] = newSig(params(_addr_typs[1], typs[1]), params(_addr_typs[1]));
    typs[69] = newSig(params(_addr_typs[1], typs[1], typs[1]), null);
    typs[70] = newSig(params(_addr_typs[1]), null);
    typs[71] = types.NewPtr(typs[5]);
    typs[72] = newSig(params(_addr_typs[71], typs[7], typs[7]), params(_addr_typs[6]));
    typs[73] = newSig(null, params(_addr_typs[60]));
    typs[74] = types.NewMap(typs[2], typs[2]);
    typs[75] = newSig(params(_addr_typs[1], typs[22], typs[3]), params(_addr_typs[74]));
    typs[76] = newSig(params(_addr_typs[1], typs[15], typs[3]), params(_addr_typs[74]));
    typs[77] = newSig(null, params(_addr_typs[74]));
    typs[78] = newSig(params(_addr_typs[1], typs[74], typs[3]), params(_addr_typs[3]));
    typs[79] = newSig(params(_addr_typs[1], typs[74], typs[60]), params(_addr_typs[3]));
    typs[80] = newSig(params(_addr_typs[1], typs[74], typs[24]), params(_addr_typs[3]));
    typs[81] = newSig(params(_addr_typs[1], typs[74], typs[28]), params(_addr_typs[3]));
    typs[82] = newSig(params(_addr_typs[1], typs[74], typs[3], typs[1]), params(_addr_typs[3]));
    typs[83] = newSig(params(_addr_typs[1], typs[74], typs[3]), params(_addr_typs[3], typs[6]));
    typs[84] = newSig(params(_addr_typs[1], typs[74], typs[60]), params(_addr_typs[3], typs[6]));
    typs[85] = newSig(params(_addr_typs[1], typs[74], typs[24]), params(_addr_typs[3], typs[6]));
    typs[86] = newSig(params(_addr_typs[1], typs[74], typs[28]), params(_addr_typs[3], typs[6]));
    typs[87] = newSig(params(_addr_typs[1], typs[74], typs[3], typs[1]), params(_addr_typs[3], typs[6]));
    typs[88] = newSig(params(_addr_typs[1], typs[74], typs[7]), params(_addr_typs[3]));
    typs[89] = newSig(params(_addr_typs[1], typs[74], typs[3]), null);
    typs[90] = newSig(params(_addr_typs[1], typs[74], typs[60]), null);
    typs[91] = newSig(params(_addr_typs[1], typs[74], typs[24]), null);
    typs[92] = newSig(params(_addr_typs[1], typs[74], typs[28]), null);
    typs[93] = newSig(params(_addr_typs[3]), null);
    typs[94] = newSig(params(_addr_typs[1], typs[74]), null);
    typs[95] = types.NewChan(typs[2], types.Cboth);
    typs[96] = newSig(params(_addr_typs[1], typs[22]), params(_addr_typs[95]));
    typs[97] = newSig(params(_addr_typs[1], typs[15]), params(_addr_typs[95]));
    typs[98] = types.NewChan(typs[2], types.Crecv);
    typs[99] = newSig(params(_addr_typs[98], typs[3]), null);
    typs[100] = newSig(params(_addr_typs[98], typs[3]), params(_addr_typs[6]));
    typs[101] = types.NewChan(typs[2], types.Csend);
    typs[102] = newSig(params(_addr_typs[101], typs[3]), null);
    typs[103] = types.NewArray(typs[0], 3);
    typs[104] = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(src.NoXPos,Lookup("enabled"),typs[6]), types.NewField(src.NoXPos,Lookup("pad"),typs[103]), types.NewField(src.NoXPos,Lookup("needed"),typs[6]), types.NewField(src.NoXPos,Lookup("cgo"),typs[6]), types.NewField(src.NoXPos,Lookup("alignme"),typs[24]) }));
    typs[105] = newSig(params(_addr_typs[1], typs[3], typs[3]), null);
    typs[106] = newSig(params(_addr_typs[1], typs[3]), null);
    typs[107] = newSig(params(_addr_typs[1], typs[3], typs[15], typs[3], typs[15]), params(_addr_typs[15]));
    typs[108] = newSig(params(_addr_typs[101], typs[3]), params(_addr_typs[6]));
    typs[109] = newSig(params(_addr_typs[3], typs[98]), params(_addr_typs[6], typs[6]));
    typs[110] = newSig(params(_addr_typs[71]), null);
    typs[111] = newSig(params(_addr_typs[1], typs[1], typs[71], typs[15], typs[15], typs[6]), params(_addr_typs[15], typs[6]));
    typs[112] = newSig(params(_addr_typs[1], typs[15], typs[15]), params(_addr_typs[7]));
    typs[113] = newSig(params(_addr_typs[1], typs[22], typs[22]), params(_addr_typs[7]));
    typs[114] = newSig(params(_addr_typs[1], typs[15], typs[15], typs[7]), params(_addr_typs[7]));
    typs[115] = types.NewSlice(typs[2]);
    typs[116] = newSig(params(_addr_typs[1], typs[115], typs[15]), params(_addr_typs[115]));
    typs[117] = newSig(params(_addr_typs[1], typs[7], typs[15]), null);
    typs[118] = newSig(params(_addr_typs[1], typs[7], typs[22]), null);
    typs[119] = newSig(params(_addr_typs[3], typs[3], typs[5]), null);
    typs[120] = newSig(params(_addr_typs[7], typs[5]), null);
    typs[121] = newSig(params(_addr_typs[3], typs[3], typs[5]), params(_addr_typs[6]));
    typs[122] = newSig(params(_addr_typs[3], typs[3]), params(_addr_typs[6]));
    typs[123] = newSig(params(_addr_typs[7], typs[7]), params(_addr_typs[6]));
    typs[124] = newSig(params(_addr_typs[7], typs[5], typs[5]), params(_addr_typs[5]));
    typs[125] = newSig(params(_addr_typs[7], typs[5]), params(_addr_typs[5]));
    typs[126] = newSig(params(_addr_typs[22], typs[22]), params(_addr_typs[22]));
    typs[127] = newSig(params(_addr_typs[24], typs[24]), params(_addr_typs[24]));
    typs[128] = newSig(params(_addr_typs[20]), params(_addr_typs[22]));
    typs[129] = newSig(params(_addr_typs[20]), params(_addr_typs[24]));
    typs[130] = newSig(params(_addr_typs[20]), params(_addr_typs[60]));
    typs[131] = newSig(params(_addr_typs[22]), params(_addr_typs[20]));
    typs[132] = newSig(params(_addr_typs[24]), params(_addr_typs[20]));
    typs[133] = newSig(params(_addr_typs[60]), params(_addr_typs[20]));
    typs[134] = newSig(params(_addr_typs[26], typs[26]), params(_addr_typs[26]));
    typs[135] = newSig(null, params(_addr_typs[5]));
    typs[136] = newSig(params(_addr_typs[5], typs[5]), null);
    typs[137] = newSig(params(_addr_typs[5], typs[5], typs[5]), null);
    typs[138] = newSig(params(_addr_typs[7], typs[1], typs[5]), null);
    typs[139] = types.NewSlice(typs[7]);
    typs[140] = newSig(params(_addr_typs[7], typs[139]), null);
    typs[141] = newSig(params(_addr_typs[64], typs[64]), null);
    typs[142] = newSig(params(_addr_typs[58], typs[58]), null);
    typs[143] = newSig(params(_addr_typs[60], typs[60]), null);
    typs[144] = newSig(params(_addr_typs[24], typs[24]), null);
    return typs[..];
}

} // end typecheck_package
