// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:37 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\package.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;

public static partial class ir_package {

// A Package holds information about the package being compiled.
public partial struct Package {
    public slice<ptr<types.Pkg>> Imports; // Init functions, listed in source order.
    public slice<ptr<Func>> Inits; // Top-level declarations.
    public slice<Node> Decls; // Extern (package global) declarations.
    public slice<Node> Externs; // Assembly function declarations.
    public slice<ptr<Name>> Asms; // Cgo directives.
    public slice<slice<@string>> CgoPragmas; // Variables with //go:embed lines.
    public slice<ptr<Name>> Embeds; // Exported (or re-exported) symbols.
    public slice<ptr<Name>> Exports; // Map from function names of stencils to already-created stencils.
    public map<ptr<types.Sym>, ptr<Func>> Stencils;
}

} // end ir_package
