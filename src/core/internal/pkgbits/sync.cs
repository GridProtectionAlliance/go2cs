// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class pkgbits_package {

// fmtFrames formats a backtrace for reporting reader/writer desyncs.
internal static slice<@string> fmtFrames(params ꓸꓸꓸuintptr pcsʗp) {
    var pcs = pcsʗp.slice();

    var res = new slice<@string>(0, len(pcs));
    walkFrames(pcs, 
    var resʗ1 = res;
    (@string file, nint line, @string name, uintptr offset) => {
        // Trim package from function name. It's just redundant noise.
        name = strings.TrimPrefix(name, "cmd/compile/internal/noder."u8);
        resʗ1 = append(resʗ1, fmt.Sprintf("%s:%v: %s +0x%v"u8, file, line, name, offset));
    });
    return res;
}

internal delegate void frameVisitor(@string file, nint line, @string name, uintptr offset);

// walkFrames calls visit for each call frame represented by pcs.
//
// pcs should be a slice of PCs, as returned by runtime.Callers.
internal static void walkFrames(slice<uintptr> pcs, frameVisitor visit) {
    if (len(pcs) == 0) {
        return;
    }
    var frames = runtime.CallersFrames(pcs);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        visit(frame.File, frame.Line, frame.Function, frame.PC - frame.Entry);
        if (!more) {
            return;
        }
    }
}

[GoType("num:nint")] partial struct SyncMarker;

//go:generate stringer -type=SyncMarker -trimprefix=Sync
internal static readonly SyncMarker Δ_ = /* iota */ 0;
// Public markers (known to go/types importers).
public static readonly SyncMarker SyncEOF = 1;
public static readonly SyncMarker SyncBool = 2;
public static readonly SyncMarker SyncInt64 = 3;
public static readonly SyncMarker SyncUint64 = 4;
public static readonly SyncMarker SyncString = 5;
public static readonly SyncMarker SyncValue = 6;
public static readonly SyncMarker SyncVal = 7;
public static readonly SyncMarker SyncRelocs = 8;
public static readonly SyncMarker SyncReloc = 9;
public static readonly SyncMarker SyncUseReloc = 10;
public static readonly SyncMarker SyncPublic = 11;
public static readonly SyncMarker SyncPos = 12;
public static readonly SyncMarker SyncPosBase = 13;
public static readonly SyncMarker SyncObject = 14;
public static readonly SyncMarker SyncObject1 = 15;
public static readonly SyncMarker SyncPkg = 16;
public static readonly SyncMarker SyncPkgDef = 17;
public static readonly SyncMarker SyncMethod = 18;
public static readonly SyncMarker SyncType = 19;
public static readonly SyncMarker SyncTypeIdx = 20;
public static readonly SyncMarker SyncTypeParamNames = 21;
public static readonly SyncMarker SyncSignature = 22;
public static readonly SyncMarker SyncParams = 23;
public static readonly SyncMarker SyncParam = 24;
public static readonly SyncMarker SyncCodeObj = 25;
public static readonly SyncMarker SyncSym = 26;
public static readonly SyncMarker SyncLocalIdent = 27;
public static readonly SyncMarker SyncSelector = 28;
public static readonly SyncMarker SyncPrivate = 29;
public static readonly SyncMarker SyncFuncExt = 30;
public static readonly SyncMarker SyncVarExt = 31;
public static readonly SyncMarker SyncTypeExt = 32;
public static readonly SyncMarker SyncPragma = 33;
public static readonly SyncMarker SyncExprList = 34;
public static readonly SyncMarker SyncExprs = 35;
public static readonly SyncMarker SyncExpr = 36;
public static readonly SyncMarker SyncExprType = 37;
public static readonly SyncMarker SyncAssign = 38;
public static readonly SyncMarker SyncOp = 39;
public static readonly SyncMarker SyncFuncLit = 40;
public static readonly SyncMarker SyncCompLit = 41;
public static readonly SyncMarker SyncDecl = 42;
public static readonly SyncMarker SyncFuncBody = 43;
public static readonly SyncMarker SyncOpenScope = 44;
public static readonly SyncMarker SyncCloseScope = 45;
public static readonly SyncMarker SyncCloseAnotherScope = 46;
public static readonly SyncMarker SyncDeclNames = 47;
public static readonly SyncMarker SyncDeclName = 48;
public static readonly SyncMarker SyncStmts = 49;
public static readonly SyncMarker SyncBlockStmt = 50;
public static readonly SyncMarker SyncIfStmt = 51;
public static readonly SyncMarker SyncForStmt = 52;
public static readonly SyncMarker SyncSwitchStmt = 53;
public static readonly SyncMarker SyncRangeStmt = 54;
public static readonly SyncMarker SyncCaseClause = 55;
public static readonly SyncMarker SyncCommClause = 56;
public static readonly SyncMarker SyncSelectStmt = 57;
public static readonly SyncMarker SyncDecls = 58;
public static readonly SyncMarker SyncLabeledStmt = 59;
public static readonly SyncMarker SyncUseObjLocal = 60;
public static readonly SyncMarker SyncAddLocal = 61;
public static readonly SyncMarker SyncLinkname = 62;
public static readonly SyncMarker SyncStmt1 = 63;
public static readonly SyncMarker SyncStmtsEnd = 64;
public static readonly SyncMarker SyncLabel = 65;
public static readonly SyncMarker SyncOptLabel = 66;
public static readonly SyncMarker SyncMultiExpr = 67;
public static readonly SyncMarker SyncRType = 68;
public static readonly SyncMarker SyncConvRTTI = 69;

} // end pkgbits_package
