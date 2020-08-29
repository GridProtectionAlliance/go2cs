// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package comment.
// package pkg -- go2cs converted at 2020 August 29 10:00:08 UTC
// import "cmd/doc.pkg" ==> using pkg = go.cmd.doc.pkg_package
// Original source: C:\Go\src\cmd\doc\testdata\pkg.go

using static go.builtin;

namespace go {
namespace cmd
{
    public static partial class pkg_package
    {
        // Constants

        // Comment about exported constant.
        public static readonly long ExportedConstant = 1L;

        // Comment about internal constant.


        // Comment about internal constant.
        private static readonly long internalConstant = 2L;

        // Comment about block of constants.


        // Comment about block of constants.
 
        // Comment before ConstOne.
        public static readonly long ConstOne = 1L;
        public static readonly long ConstTwo = 2L; // Comment on line with ConstTwo.
        private static readonly long constThree = 3L; // Comment on line with constThree.

        // Const block where first entry is unexported.
        private static readonly var constFour = iota;
        public static readonly var ConstFive = 0;
        public static readonly var ConstSix = 1;

        // Variables

        // Comment about exported variable.
        public static long ExportedVariable = 1L;

        // Comment about internal variable.
        private static long internalVariable = 2L;

        // Comment about block of variables.
 
        // Comment before VarOne.
        public static long VarOne = 1L;        public static long VarTwo = 2L;        private static long varThree = 3L;

        // Var block where first entry is unexported.
        private static long varFour = 4L;        public static long VarFive = 5L;        private static long varSix = 6L;

        // Comment about exported function.
        public static bool ExportedFunc(long a)
;

        // Comment about internal function.
        private static bool internalFunc(long a)
;

        // Comment about exported type.
        public partial struct ExportedType : error
        {
            public long ExportedField; // Comment on line with exported field.
            public long unexportedField; // Comment on line with unexported field.
            public ref ExportedEmbeddedType ExportedEmbeddedType => ref ExportedEmbeddedType_val; // Comment on line with exported embedded field.
            public ref ExportedEmbeddedType ExportedEmbeddedType => ref ExportedEmbeddedType_ptr; // Comment on line with exported embedded *field.
            public ref qualified.ExportedEmbeddedType ExportedEmbeddedType => ref ExportedEmbeddedType_ptr; // Comment on line with exported embedded *selector.field.
            public ref unexportedType unexportedType => ref unexportedType_val; // Comment on line with unexported embedded field.
            public ref unexportedType unexportedType => ref unexportedType_ptr; // Comment on line with unexported embedded *field.
            public ref io.Reader Reader => ref Reader_val; // Comment on line with embedded Reader.
            public error error; // Comment on line with embedded error.
        }

        // Comment about exported method.
        public static bool ExportedMethod(this ExportedType _p0, long a)
        {>>MARKER:FUNCTION_internalFunc_BLOCK_PREFIX<<
            return true;
        }

        // Comment about unexported method.
        public static bool unexportedMethod(this ExportedType _p0, long a)
        {>>MARKER:FUNCTION_ExportedFunc_BLOCK_PREFIX<<
            return true;
        }

        public partial struct ExportedStructOneField
        {
            public long OnlyField; // the only field
        }

        // Constants tied to ExportedType. (The type is a struct so this isn't valid Go,
        // but it parses and that's all we need.)
        public static readonly ExportedType ExportedTypedConstant = iota;

        // Comment about constructor for exported type.
        public static ref ExportedType ExportedTypeConstructor()
        {
            return null;
        }

        private static readonly ExportedType unexportedTypedConstant = 1L; // In a separate section to test -u.

        // Comment about exported interface.
 // In a separate section to test -u.

        // Comment about exported interface.
        public partial interface ExportedInterface : io.Reader, error
        {
            void ExportedMethod(); // Comment on line with exported method.
            void unexportedMethod(); // Comment on line with unexported method.
        }

        // Comment about unexported type.
        private partial struct unexportedType // : long
        {
        }

        private static bool ExportedMethod(this unexportedType _p0)
        {
            return true;
        }

        private static bool unexportedMethod(this unexportedType _p0)
        {
            return true;
        }

        // Constants tied to unexportedType.
        public static readonly unexportedType ExportedTypedConstant_unexported = iota;

        private static readonly unexportedType unexportedTypedConstant = 1L; // In a separate section to test -u.

        // For case matching.
 // In a separate section to test -u.

        // For case matching.
        public static readonly long CaseMatch = 1L;

        public static readonly long Casematch = 2L;



        public static unexportedType ReturnUnexported()
        {
            return 0L;
        }
        public static ExportedType ReturnExported()
        {
            return new ExportedType();
        }

        public static readonly @string MultiLineConst = "\n\tMultiLineString1\n\tMultiLineString2\n\tMultiLineString3\n";



        public static object MultiLineFunc(object x)
        {
            return r;
        }



        private static readonly ulong _ = 2L * iota;
        private static readonly ulong _ = 1L << (int)(iota);
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



        private static void newLongLine(params @string[] ss)
;

        public static var LongLine = newLongLine("someArgument1", "someArgument2", "someArgument3", "someArgument4", "someArgument5", "someArgument6", "someArgument7", "someArgument8");

        public partial struct T2 // : long
        {
        }

        public partial struct T1 // : T2
        {
        }

        public static readonly var Duplicate = iota;
        private static readonly var duplicate = 0;
    }
}}
