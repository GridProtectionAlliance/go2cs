// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package types declares the data types and implements
// the algorithms for type-checking of Go packages. Use
// Config.Check to invoke the type checker for a package.
// Alternatively, create a new type checker with NewChecker
// and invoke it incrementally by calling Checker.Files.
//
// Type-checking consists of several interdependent phases:
//
// Name resolution maps each identifier (ast.Ident) in the program to the
// language object (Object) it denotes.
// Use Info.{Defs,Uses,Implicits} for the results of name resolution.
//
// Constant folding computes the exact constant value (constant.Value)
// for every expression (ast.Expr) that is a compile-time constant.
// Use Info.Types[expr].Value for the results of constant folding.
//
// Type inference computes the type (Type) of every expression (ast.Expr)
// and checks for compliance with the language specification.
// Use Info.Types[expr].Type for the results of type inference.
//
// For a tutorial, see https://golang.org/s/types-tutorial.
//
// package types -- go2cs converted at 2020 August 29 08:46:54 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\api.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // An Error describes a type-checking error; it implements the error interface.
        // A "soft" error is an error that still permits a valid interpretation of a
        // package (such as "unused variable"); "hard" errors may lead to unpredictable
        // behavior if ignored.
        public partial struct Error
        {
            public ptr<token.FileSet> Fset; // file set for interpretation of Pos
            public token.Pos Pos; // error position
            public @string Msg; // error message
            public bool Soft; // if set, error is "soft"
        }

        // Error returns an error string formatted as follows:
        // filename:line:column: message
        public static @string Error(this Error err)
        {
            return fmt.Sprintf("%s: %s", err.Fset.Position(err.Pos), err.Msg);
        }

        // An Importer resolves import paths to Packages.
        //
        // CAUTION: This interface does not support the import of locally
        // vendored packages. See https://golang.org/s/go15vendor.
        // If possible, external implementations should implement ImporterFrom.
        public partial interface Importer
        {
            (ref Package, error) Import(@string path);
        }

        // ImportMode is reserved for future use.
        public partial struct ImportMode // : long
        {
        }

        // An ImporterFrom resolves import paths to packages; it
        // supports vendoring per https://golang.org/s/go15vendor.
        // Use go/importer to obtain an ImporterFrom implementation.
        public partial interface ImporterFrom : Importer
        {
            (ref Package, error) ImportFrom(@string path, @string dir, ImportMode mode);
        }

        // A Config specifies the configuration for type checking.
        // The zero value for Config is a ready-to-use default configuration.
        public partial struct Config
        {
            public bool IgnoreFuncBodies; // If FakeImportC is set, `import "C"` (for packages requiring Cgo)
// declares an empty "C" package and errors are omitted for qualified
// identifiers referring to package C (which won't find an object).
// This feature is intended for the standard library cmd/api tool.
//
// Caution: Effects may be unpredictable due to follow-on errors.
//          Do not use casually!
            public bool FakeImportC; // If Error != nil, it is called with each error found
// during type checking; err has dynamic type Error.
// Secondary errors (for instance, to enumerate all types
// involved in an invalid recursive type declaration) have
// error strings that start with a '\t' character.
// If Error == nil, type-checking stops with the first
// error found.
            public Action<error> Error; // An importer is used to import packages referred to from
// import declarations.
// If the installed importer implements ImporterFrom, the type
// checker calls ImportFrom instead of Import.
// The type checker reports an error if an importer is needed
// but none was installed.
            public Importer Importer; // If Sizes != nil, it provides the sizing functions for package unsafe.
// Otherwise SizesFor("gc", "amd64") is used instead.
            public Sizes Sizes; // If DisableUnusedImportCheck is set, packages are not checked
// for unused imports.
            public bool DisableUnusedImportCheck;
        }

        // Info holds result type information for a type-checked package.
        // Only the information for which a map is provided is collected.
        // If the package has type errors, the collected information may
        // be incomplete.
        public partial struct Info
        {
            public map<ast.Expr, TypeAndValue> Types; // Defs maps identifiers to the objects they define (including
// package names, dots "." of dot-imports, and blank "_" identifiers).
// For identifiers that do not denote objects (e.g., the package name
// in package clauses, or symbolic variables t in t := x.(type) of
// type switch headers), the corresponding objects are nil.
//
// For an anonymous field, Defs returns the field *Var it defines.
//
// Invariant: Defs[id] == nil || Defs[id].Pos() == id.Pos()
            public map<ref ast.Ident, Object> Defs; // Uses maps identifiers to the objects they denote.
//
// For an anonymous field, Uses returns the *TypeName it denotes.
//
// Invariant: Uses[id].Pos() != id.Pos()
            public map<ref ast.Ident, Object> Uses; // Implicits maps nodes to their implicitly declared objects, if any.
// The following node and object types may appear:
//
//     node               declared object
//
//     *ast.ImportSpec    *PkgName for imports without renames
//     *ast.CaseClause    type-specific *Var for each type switch case clause (incl. default)
//     *ast.Field         anonymous parameter *Var
//
            public map<ast.Node, Object> Implicits; // Selections maps selector expressions (excluding qualified identifiers)
// to their corresponding selections.
            public map<ref ast.SelectorExpr, ref Selection> Selections; // Scopes maps ast.Nodes to the scopes they define. Package scopes are not
// associated with a specific node but with all files belonging to a package.
// Thus, the package scope can be found in the type-checked Package object.
// Scopes nest, with the Universe scope being the outermost scope, enclosing
// the package scope, which contains (one or more) files scopes, which enclose
// function scopes which in turn enclose statement and function literal scopes.
// Note that even though package-level functions are declared in the package
// scope, the function scopes are embedded in the file scope of the file
// containing the function declaration.
//
// The following node types may appear in Scopes:
//
//     *ast.File
//     *ast.FuncType
//     *ast.BlockStmt
//     *ast.IfStmt
//     *ast.SwitchStmt
//     *ast.TypeSwitchStmt
//     *ast.CaseClause
//     *ast.CommClause
//     *ast.ForStmt
//     *ast.RangeStmt
//
            public map<ast.Node, ref Scope> Scopes; // InitOrder is the list of package-level initializers in the order in which
// they must be executed. Initializers referring to variables related by an
// initialization dependency appear in topological order, the others appear
// in source order. Variables without an initialization expression do not
// appear in this list.
            public slice<ref Initializer> InitOrder;
        }

        // TypeOf returns the type of expression e, or nil if not found.
        // Precondition: the Types, Uses and Defs maps are populated.
        //
        private static Type TypeOf(this ref Info info, ast.Expr e)
        {
            {
                var (t, ok) = info.Types[e];

                if (ok)
                {
                    return t.Type;
                }

            }
            {
                ref ast.Ident (id, _) = e._<ref ast.Ident>();

                if (id != null)
                {
                    {
                        var obj = info.ObjectOf(id);

                        if (obj != null)
                        {
                            return obj.Type();
                        }

                    }
                }

            }
            return null;
        }

        // ObjectOf returns the object denoted by the specified id,
        // or nil if not found.
        //
        // If id is an anonymous struct field, ObjectOf returns the field (*Var)
        // it uses, not the type (*TypeName) it defines.
        //
        // Precondition: the Uses and Defs maps are populated.
        //
        private static Object ObjectOf(this ref Info info, ref ast.Ident id)
        {
            {
                var obj = info.Defs[id];

                if (obj != null)
                {
                    return obj;
                }

            }
            return info.Uses[id];
        }

        // TypeAndValue reports the type and value (for constants)
        // of the corresponding expression.
        public partial struct TypeAndValue
        {
            public operandMode mode;
            public Type Type;
            public constant.Value Value;
        }

        // TODO(gri) Consider eliminating the IsVoid predicate. Instead, report
        // "void" values as regular values but with the empty tuple type.

        // IsVoid reports whether the corresponding expression
        // is a function call without results.
        public static bool IsVoid(this TypeAndValue tv)
        {
            return tv.mode == novalue;
        }

        // IsType reports whether the corresponding expression specifies a type.
        public static bool IsType(this TypeAndValue tv)
        {
            return tv.mode == typexpr;
        }

        // IsBuiltin reports whether the corresponding expression denotes
        // a (possibly parenthesized) built-in function.
        public static bool IsBuiltin(this TypeAndValue tv)
        {
            return tv.mode == builtin;
        }

        // IsValue reports whether the corresponding expression is a value.
        // Builtins are not considered values. Constant values have a non-
        // nil Value.
        public static bool IsValue(this TypeAndValue tv)
        {

            if (tv.mode == constant_ || tv.mode == variable || tv.mode == mapindex || tv.mode == value || tv.mode == commaok) 
                return true;
                        return false;
        }

        // IsNil reports whether the corresponding expression denotes the
        // predeclared value nil.
        public static bool IsNil(this TypeAndValue tv)
        {
            return tv.mode == value && tv.Type == Typ[UntypedNil];
        }

        // Addressable reports whether the corresponding expression
        // is addressable (https://golang.org/ref/spec#Address_operators).
        public static bool Addressable(this TypeAndValue tv)
        {
            return tv.mode == variable;
        }

        // Assignable reports whether the corresponding expression
        // is assignable to (provided a value of the right type).
        public static bool Assignable(this TypeAndValue tv)
        {
            return tv.mode == variable || tv.mode == mapindex;
        }

        // HasOk reports whether the corresponding expression may be
        // used on the lhs of a comma-ok assignment.
        public static bool HasOk(this TypeAndValue tv)
        {
            return tv.mode == commaok || tv.mode == mapindex;
        }

        // An Initializer describes a package-level variable, or a list of variables in case
        // of a multi-valued initialization expression, and the corresponding initialization
        // expression.
        public partial struct Initializer
        {
            public slice<ref Var> Lhs; // var Lhs = Rhs
            public ast.Expr Rhs;
        }

        private static @string String(this ref Initializer init)
        {
            bytes.Buffer buf = default;
            foreach (var (i, lhs) in init.Lhs)
            {
                if (i > 0L)
                {
                    buf.WriteString(", ");
                }
                buf.WriteString(lhs.Name());
            }
            buf.WriteString(" = ");
            WriteExpr(ref buf, init.Rhs);
            return buf.String();
        }

        // Check type-checks a package and returns the resulting package object and
        // the first error if any. Additionally, if info != nil, Check populates each
        // of the non-nil maps in the Info struct.
        //
        // The package is marked as complete if no errors occurred, otherwise it is
        // incomplete. See Config.Error for controlling behavior in the presence of
        // errors.
        //
        // The package is specified by a list of *ast.Files and corresponding
        // file set, and the package path the package is identified with.
        // The clean path must not be empty or dot (".").
        private static (ref Package, error) Check(this ref Config conf, @string path, ref token.FileSet fset, slice<ref ast.File> files, ref Info info)
        {
            var pkg = NewPackage(path, "");
            return (pkg, NewChecker(conf, fset, pkg, info).Files(files));
        }

        // AssertableTo reports whether a value of type V can be asserted to have type T.
        public static bool AssertableTo(ref Interface V, Type T)
        {
            var (m, _) = assertableTo(V, T);
            return m == null;
        }

        // AssignableTo reports whether a value of type V is assignable to a variable of type T.
        public static bool AssignableTo(Type V, Type T)
        {
            operand x = new operand(mode:value,typ:V);
            return x.assignableTo(null, T, null); // config not needed for non-constant x
        }

        // ConvertibleTo reports whether a value of type V is convertible to a value of type T.
        public static bool ConvertibleTo(Type V, Type T)
        {
            operand x = new operand(mode:value,typ:V);
            return x.convertibleTo(null, T); // config not needed for non-constant x
        }

        // Implements reports whether type V implements interface T.
        public static bool Implements(Type V, ref Interface T)
        {
            var (f, _) = MissingMethod(V, T, true);
            return f == null;
        }
    }
}}
