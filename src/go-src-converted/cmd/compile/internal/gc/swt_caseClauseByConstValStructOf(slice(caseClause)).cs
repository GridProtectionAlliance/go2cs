//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 09:29:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct caseClauseByConstVal
        {
            // Value of the caseClauseByConstVal struct
            private readonly slice<caseClause> m_value;

            public caseClauseByConstVal(slice<caseClause> value) => m_value = value;

            // Enable implicit conversions between slice<caseClause> and caseClauseByConstVal struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator caseClauseByConstVal(slice<caseClause> value) => new caseClauseByConstVal(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<caseClause>(caseClauseByConstVal value) => value.m_value;
            
            // Enable comparisons between nil and caseClauseByConstVal struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(caseClauseByConstVal value, NilType nil) => value.Equals(default(caseClauseByConstVal));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(caseClauseByConstVal value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, caseClauseByConstVal value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, caseClauseByConstVal value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator caseClauseByConstVal(NilType nil) => default(caseClauseByConstVal);
        }
    }
}}}}