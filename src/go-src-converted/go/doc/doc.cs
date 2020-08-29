// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package doc extracts source code documentation from a Go AST.
// package doc -- go2cs converted at 2020 August 29 08:47:02 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\doc.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        // Package is the documentation for an entire package.
        public partial struct Package
        {
            public @string Doc;
            public @string Name;
            public @string ImportPath;
            public slice<@string> Imports;
            public slice<@string> Filenames;
            public map<@string, slice<ref Note>> Notes; // Deprecated: For backward compatibility Bugs is still populated,
// but all new code should use Notes instead.
            public slice<@string> Bugs; // declarations
            public slice<ref Value> Consts;
            public slice<ref Type> Types;
            public slice<ref Value> Vars;
            public slice<ref Func> Funcs;
        }

        // Value is the documentation for a (possibly grouped) var or const declaration.
        public partial struct Value
        {
            public @string Doc;
            public slice<@string> Names; // var or const names in declaration order
            public ptr<ast.GenDecl> Decl;
            public long order;
        }

        // Type is the documentation for a type declaration.
        public partial struct Type
        {
            public @string Doc;
            public @string Name;
            public ptr<ast.GenDecl> Decl; // associated declarations
            public slice<ref Value> Consts; // sorted list of constants of (mostly) this type
            public slice<ref Value> Vars; // sorted list of variables of (mostly) this type
            public slice<ref Func> Funcs; // sorted list of functions returning this type
            public slice<ref Func> Methods; // sorted list of methods (including embedded ones) of this type
        }

        // Func is the documentation for a func declaration.
        public partial struct Func
        {
            public @string Doc;
            public @string Name;
            public ptr<ast.FuncDecl> Decl; // methods
// (for functions, these fields have the respective zero value)
            public @string Recv; // actual   receiver "T" or "*T"
            public @string Orig; // original receiver "T" or "*T"
            public long Level; // embedding level; 0 means not embedded
        }

        // A Note represents a marked comment starting with "MARKER(uid): note body".
        // Any note with a marker of 2 or more upper case [A-Z] letters and a uid of
        // at least one character is recognized. The ":" following the uid is optional.
        // Notes are collected in the Package.Notes map indexed by the notes marker.
        public partial struct Note
        {
            public token.Pos Pos; // position range of the comment containing the marker
            public token.Pos End; // position range of the comment containing the marker
            public @string UID; // uid found with the marker
            public @string Body; // note body text
        }

        // Mode values control the operation of New.
        public partial struct Mode // : long
        {
        }

 
        // extract documentation for all package-level declarations,
        // not just exported ones
        public static readonly Mode AllDecls = 1L << (int)(iota); 

        // show all embedded methods, not just the ones of
        // invisible (unexported) anonymous fields
        public static readonly var AllMethods = 0;

        // New computes the package documentation for the given package AST.
        // New takes ownership of the AST pkg and may edit or overwrite it.
        //
        public static ref Package New(ref ast.Package pkg, @string importPath, Mode mode)
        {
            reader r = default;
            r.readPackage(pkg, mode);
            r.computeMethodSets();
            r.cleanupTypes();
            return ref new Package(Doc:r.doc,Name:pkg.Name,ImportPath:importPath,Imports:sortedKeys(r.imports),Filenames:r.filenames,Notes:r.notes,Bugs:noteBodies(r.notes["BUG"]),Consts:sortedValues(r.values,token.CONST),Types:sortedTypes(r.types,mode&AllMethods!=0),Vars:sortedValues(r.values,token.VAR),Funcs:sortedFuncs(r.funcs,true),);
        }
    }
}}
