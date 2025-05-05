// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains various functionality that is
// different between go/types and types2. Factoring
// out this code allows more of the rest of the code
// to be shared.
namespace go.go;

using ast = go.ast_package;
using constant = go.constant_package;
using token = go.token_package;

partial class types_package {

internal const bool isTypes2 = false;

// cmpPos compares the positions p and q and returns a result r as follows:
//
// r <  0: p is before q
// r == 0: p and q are the same position (but may not be identical)
// r >  0: p is after q
//
// If p and q are in different files, p is before q if the filename
// of p sorts lexicographically before the filename of q.
internal static nint cmpPos(token.Pos p, token.Pos q) {
    return ((nint)(p - q));
}

// hasDots reports whether the last argument in the call is followed by ...
internal static bool hasDots(ж<ast.CallExpr> Ꮡcall) {
    ref var call = ref Ꮡcall.val;

    return call.Ellipsis.IsValid();
}

// dddErrPos returns the positioner for reporting an invalid ... use in a call.
internal static positioner dddErrPos(ж<ast.CallExpr> Ꮡcall) {
    ref var call = ref Ꮡcall.val;

    return ((atPos)call.Ellipsis);
}

// argErrPos returns positioner for reporting an invalid argument count.
internal static positioner argErrPos(ж<ast.CallExpr> Ꮡcall) {
    ref var call = ref Ꮡcall.val;

    return inNode(~call, call.Rparen);
}

// startPos returns the start position of node n.
internal static tokenꓸPos startPos(ast.Node n) {
    return n.Pos();
}

// endPos returns the position of the first character immediately after node n.
internal static tokenꓸPos endPos(ast.Node n) {
    return n.End();
}

// makeFromLiteral returns the constant value for the given literal string and kind.
internal static constant.Value makeFromLiteral(@string lit, token.Token kind) {
    return constant.MakeFromLiteral(lit, kind, 0);
}

} // end types_package
