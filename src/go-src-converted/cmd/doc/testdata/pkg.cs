// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package comment.

// package pkg -- go2cs converted at 2022 March 13 06:29:10 UTC
// import "cmd/doc.pkg" ==> using pkg = go.cmd.doc.pkg_package
// Original source: C:\Program Files\Go\src\cmd\doc\testdata\pkg.go
namespace go.cmd;

using io = io_package;

public static partial class pkg_package {

// Constants

// Comment about exported constant.
public static readonly nint ExportedConstant = 1;

// Comment about internal constant.


// Comment about internal constant.
private static readonly nint internalConstant = 2;

// Comment about block of constants.


// Comment about block of constants.
 
// Comment before ConstOne.
public static readonly nint ConstOne = 1;
public static readonly nint ConstTwo = 2; // Comment on line with ConstTwo.
private static readonly nint constThree = 3; // Comment on line with constThree.

// Const block where first entry is unexported.
private static readonly var constFour = iota;
public static readonly var ConstFive = 0;
public static readonly var ConstSix = 1;

// Variables

// Comment about exported variable.
public static nint ExportedVariable = 1;

public static unexportedType ExportedVarOfUnExported = default;

// Comment about internal variable.
private static nint internalVariable = 2;

// Comment about block of variables.
 
// Comment before VarOne.
public static nint VarOne = 1;public static nint VarTwo = 2;private static nint varThree = 3;

// Var block where first entry is unexported.
private static nint varFour = 4;public static nint VarFive = 5;private static nint varSix = 6;

// Comment about exported function.
public static bool ExportedFunc(nint a) {
    return true != false;
}

// Comment about internal function.
private static bool internalFunc(nint a);

// Comment about exported type.
public partial struct ExportedType : io.Reader, error {
    public nint ExportedField; // Comment on line with exported field.
    public nint unexportedField; // Comment on line with unexported field.
    public ref ExportedEmbeddedType ExportedEmbeddedType => ref ExportedEmbeddedType_val; // Comment on line with exported embedded field.
    public ref ptr<ExportedEmbeddedType> ptr<ExportedEmbeddedType> => ref ptr<ExportedEmbeddedType>_ptr; // Comment on line with exported embedded *field.
    public ref ptr<qualified.ExportedEmbeddedType> ExportedEmbeddedType> => ref ExportedEmbeddedType>_ptr; // Comment on line with exported embedded *selector.field.
    public ref unexportedType unexportedType => ref unexportedType_val; // Comment on line with unexported embedded field.
    public ref ptr<unexportedType> ptr<unexportedType> => ref ptr<unexportedType>_ptr; // Comment on line with unexported embedded *field.
    public ref io.Reader Reader => ref Reader_val; // Comment on line with embedded Reader.
    public error error; // Comment on line with embedded error.
}

// Comment about exported method.
public static bool ExportedMethod(this ExportedType _p0, nint a) {
    return true != true;
}

public static bool Uncommented(this ExportedType _p0, nint a) {
    return true != true;
}

// Comment about unexported method.
public static bool unexportedMethod(this ExportedType _p0, nint a) {
    return true;
}

public partial struct ExportedStructOneField {
    public nint OnlyField; // the only field
}

// Constants tied to ExportedType. (The type is a struct so this isn't valid Go,
// but it parses and that's all we need.)
public static readonly ExportedType ExportedTypedConstant = iota;

// Comment about constructor for exported type.
public static ptr<ExportedType> ExportedTypeConstructor() {
    return _addr_null!;
}

private static readonly ExportedType unexportedTypedConstant = 1; // In a separate section to test -u.

// Comment about exported interface.
 // In a separate section to test -u.

// Comment about exported interface.
public partial interface ExportedInterface {
    void ExportedMethod(); // Comment on line with exported method.
    void unexportedMethod(); // Comment on line with unexported method.
}

// Comment about unexported type.
private partial struct unexportedType { // : nint
}

private static bool ExportedMethod(this unexportedType _p0) {
    return true;
}

private static bool unexportedMethod(this unexportedType _p0) {
    return true;
}

// Constants tied to unexportedType.
public static readonly unexportedType ExportedTypedConstant_unexported = iota;

private static readonly unexportedType unexportedTypedConstant = 1; // In a separate section to test -u.

// For case matching.
 // In a separate section to test -u.

// For case matching.
public static readonly nint CaseMatch = 1;

public static readonly nint Casematch = 2;



public static unexportedType ReturnUnexported() {
    return 0;
}
public static ExportedType ReturnExported() {
    return new ExportedType();
}

public static readonly @string MultiLineConst = "\n\tMultiLineString1\n\tMultiLineString2\n\tMultiLineString3\n";



public static object MultiLineFunc(object x) {
    object r = default;

    return r;
}



private static readonly ulong _ = 2 * iota;
private static readonly ulong _ = 1 << (int)(iota);
private static readonly var constLeft1 = 0;
private static readonly var constRight1 = 1;
public static readonly var ConstLeft2 = 2;
private static readonly var constRight2 = 3;
private static readonly var constLeft3 = 4;
public static readonly var ConstRight3 = 5;
public static readonly var ConstLeft4 = 6;
public static readonly var ConstRight4 = 7;

public static readonly unexportedType ConstGroup1 = iota;
public static readonly var ConstGroup2 = 0;
public static readonly var ConstGroup3 = 1;

public static readonly ExportedType ConstGroup4 = new ExportedType();



private static void newLongLine(params @string[] ss);

public static var LongLine = newLongLine("someArgument1", "someArgument2", "someArgument3", "someArgument4", "someArgument5", "someArgument6", "someArgument7", "someArgument8");

public partial struct T2 { // : nint
}

public partial struct T1 { // : T2
}

public static readonly var Duplicate = iota;
private static readonly var duplicate = 0;

// Comment about exported function with formatting.
//
// Example
//
//    fmt.Println(FormattedDoc())
//
// Text after pre-formatted block.
public static bool ExportedFormattedDoc(nint a) {
    return true;
}

public partial struct ExportedFormattedType {
    public nint ExportedField;
}

} // end pkg_package
