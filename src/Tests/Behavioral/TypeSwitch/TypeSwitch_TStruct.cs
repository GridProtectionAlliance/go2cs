//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2018 August 14 00:22:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using static go.builtin;
using fmt = go.fmt_package;

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.1.0")]
        public partial struct T : EmptyInterface
        {
            // I.m function promotion
            private delegate @string mByVal(T value);
            private delegate @string mByRef(ref T value);

            private static readonly mByVal s_mByVal;
            private static readonly mByRef s_mByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public @string m() => s_mByRef?.Invoke(ref this) ?? s_mByVal?.Invoke(this) ?? I?.m() ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);
            
            [DebuggerStepperBoundary]
            static T()
            {
                Type targetType = typeof(T);
                MethodInfo extensionMethod;
                
                extensionMethod = targetType.GetExtensionMethodSearchingPromotions("m");

                if ((object)extensionMethod != null)
                {
                    s_mByRef = extensionMethod.CreateStaticDelegate(typeof(mByRef)) as mByRef;

                    if ((object)s_mByRef == null)
                        s_mByVal = extensionMethod.CreateStaticDelegate(typeof(mByVal)) as mByVal;
                }
            }

            // Constructors
            public T(NilType _)
            {
                this.name = default;
                this.I = default;
            }

            public T(@string name, I I)
            {
                this.name = name;
                this.I = I;
            }

            // Enable comparisons between nil and T struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(T value, NilType nil) => value.Equals(default(T));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(T value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, T value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, T value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T(NilType nil) => default(T);
        }

        [GeneratedCode("go2cs", "0.1.1.0")]
        public static T T_cast(dynamic value)
        {
            return new T(value.name, value.I);
        }
    }
}