//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:00:50 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using abi = go.cmd.compile.@internal.abi_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using buildcfg = go.@internal.buildcfg_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Types
        {
            // Constructors
            public Types(NilType _)
            {
                this.Bool = default;
                this.Int8 = default;
                this.Int16 = default;
                this.Int32 = default;
                this.Int64 = default;
                this.UInt8 = default;
                this.UInt16 = default;
                this.UInt32 = default;
                this.UInt64 = default;
                this.Int = default;
                this.Float32 = default;
                this.Float64 = default;
                this.UInt = default;
                this.Uintptr = default;
                this.String = default;
                this.BytePtr = default;
                this.Int32Ptr = default;
                this.UInt32Ptr = default;
                this.IntPtr = default;
                this.UintptrPtr = default;
                this.Float32Ptr = default;
                this.Float64Ptr = default;
                this.BytePtrPtr = default;
            }

            public Types(ref ptr<types.Type> Bool = default, ref ptr<types.Type> Int8 = default, ref ptr<types.Type> Int16 = default, ref ptr<types.Type> Int32 = default, ref ptr<types.Type> Int64 = default, ref ptr<types.Type> UInt8 = default, ref ptr<types.Type> UInt16 = default, ref ptr<types.Type> UInt32 = default, ref ptr<types.Type> UInt64 = default, ref ptr<types.Type> Int = default, ref ptr<types.Type> Float32 = default, ref ptr<types.Type> Float64 = default, ref ptr<types.Type> UInt = default, ref ptr<types.Type> Uintptr = default, ref ptr<types.Type> String = default, ref ptr<types.Type> BytePtr = default, ref ptr<types.Type> Int32Ptr = default, ref ptr<types.Type> UInt32Ptr = default, ref ptr<types.Type> IntPtr = default, ref ptr<types.Type> UintptrPtr = default, ref ptr<types.Type> Float32Ptr = default, ref ptr<types.Type> Float64Ptr = default, ref ptr<types.Type> BytePtrPtr = default)
            {
                this.Bool = Bool;
                this.Int8 = Int8;
                this.Int16 = Int16;
                this.Int32 = Int32;
                this.Int64 = Int64;
                this.UInt8 = UInt8;
                this.UInt16 = UInt16;
                this.UInt32 = UInt32;
                this.UInt64 = UInt64;
                this.Int = Int;
                this.Float32 = Float32;
                this.Float64 = Float64;
                this.UInt = UInt;
                this.Uintptr = Uintptr;
                this.String = String;
                this.BytePtr = BytePtr;
                this.Int32Ptr = Int32Ptr;
                this.UInt32Ptr = UInt32Ptr;
                this.IntPtr = IntPtr;
                this.UintptrPtr = UintptrPtr;
                this.Float32Ptr = Float32Ptr;
                this.Float64Ptr = Float64Ptr;
                this.BytePtrPtr = BytePtrPtr;
            }

            // Enable comparisons between nil and Types struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Types value, NilType nil) => value.Equals(default(Types));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Types value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Types value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Types value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Types(NilType nil) => default(Types);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Types Types_cast(dynamic value)
        {
            return new Types(ref value.Bool, ref value.Int8, ref value.Int16, ref value.Int32, ref value.Int64, ref value.UInt8, ref value.UInt16, ref value.UInt32, ref value.UInt64, ref value.Int, ref value.Float32, ref value.Float64, ref value.UInt, ref value.Uintptr, ref value.String, ref value.BytePtr, ref value.Int32Ptr, ref value.UInt32Ptr, ref value.IntPtr, ref value.UintptrPtr, ref value.Float32Ptr, ref value.Float64Ptr, ref value.BytePtrPtr);
        }
    }
}}}}