﻿//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using go;

#nullable enable

namespace go.@internal;

public static partial class cpu_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public partial struct X86ᴛ1
    {
        // Promoted Struct References
        // -- X86ᴛ1 has no promoted structs

        // Field References
        public static ref global::go.@internal.cpu_package.CacheLinePad Ꮡ_(ref X86ᴛ1 instance) => ref instance._;
        internal static ref bool ᏑHasAES(ref X86ᴛ1 instance) => ref instance.HasAES;
        internal static ref bool ᏑHasADX(ref X86ᴛ1 instance) => ref instance.HasADX;
        internal static ref bool ᏑHasAVX(ref X86ᴛ1 instance) => ref instance.HasAVX;
        internal static ref bool ᏑHasAVX2(ref X86ᴛ1 instance) => ref instance.HasAVX2;
        internal static ref bool ᏑHasAVX512F(ref X86ᴛ1 instance) => ref instance.HasAVX512F;
        internal static ref bool ᏑHasAVX512BW(ref X86ᴛ1 instance) => ref instance.HasAVX512BW;
        internal static ref bool ᏑHasAVX512VL(ref X86ᴛ1 instance) => ref instance.HasAVX512VL;
        internal static ref bool ᏑHasBMI1(ref X86ᴛ1 instance) => ref instance.HasBMI1;
        internal static ref bool ᏑHasBMI2(ref X86ᴛ1 instance) => ref instance.HasBMI2;
        internal static ref bool ᏑHasERMS(ref X86ᴛ1 instance) => ref instance.HasERMS;
        internal static ref bool ᏑHasFMA(ref X86ᴛ1 instance) => ref instance.HasFMA;
        internal static ref bool ᏑHasOSXSAVE(ref X86ᴛ1 instance) => ref instance.HasOSXSAVE;
        internal static ref bool ᏑHasPCLMULQDQ(ref X86ᴛ1 instance) => ref instance.HasPCLMULQDQ;
        internal static ref bool ᏑHasPOPCNT(ref X86ᴛ1 instance) => ref instance.HasPOPCNT;
        internal static ref bool ᏑHasRDTSCP(ref X86ᴛ1 instance) => ref instance.HasRDTSCP;
        internal static ref bool ᏑHasSHA(ref X86ᴛ1 instance) => ref instance.HasSHA;
        internal static ref bool ᏑHasSSE3(ref X86ᴛ1 instance) => ref instance.HasSSE3;
        internal static ref bool ᏑHasSSSE3(ref X86ᴛ1 instance) => ref instance.HasSSSE3;
        internal static ref bool ᏑHasSSE41(ref X86ᴛ1 instance) => ref instance.HasSSE41;
        internal static ref bool ᏑHasSSE42(ref X86ᴛ1 instance) => ref instance.HasSSE42;
        public static ref global::go.@internal.cpu_package.CacheLinePad Ꮡ__(ref X86ᴛ1 instance) => ref instance.__;
        
        // Constructors
        public X86ᴛ1(NilType _)
        {
            this._ = default!;
            this.HasAES = default!;
            this.HasADX = default!;
            this.HasAVX = default!;
            this.HasAVX2 = default!;
            this.HasAVX512F = default!;
            this.HasAVX512BW = default!;
            this.HasAVX512VL = default!;
            this.HasBMI1 = default!;
            this.HasBMI2 = default!;
            this.HasERMS = default!;
            this.HasFMA = default!;
            this.HasOSXSAVE = default!;
            this.HasPCLMULQDQ = default!;
            this.HasPOPCNT = default!;
            this.HasRDTSCP = default!;
            this.HasSHA = default!;
            this.HasSSE3 = default!;
            this.HasSSSE3 = default!;
            this.HasSSE41 = default!;
            this.HasSSE42 = default!;
            this.__ = default!;
        }

        public X86ᴛ1(global::go.@internal.cpu_package.CacheLinePad _ = default!, bool HasAES = default!, bool HasADX = default!, bool HasAVX = default!, bool HasAVX2 = default!, bool HasAVX512F = default!, bool HasAVX512BW = default!, bool HasAVX512VL = default!, bool HasBMI1 = default!, bool HasBMI2 = default!, bool HasERMS = default!, bool HasFMA = default!, bool HasOSXSAVE = default!, bool HasPCLMULQDQ = default!, bool HasPOPCNT = default!, bool HasRDTSCP = default!, bool HasSHA = default!, bool HasSSE3 = default!, bool HasSSSE3 = default!, bool HasSSE41 = default!, bool HasSSE42 = default!, global::go.@internal.cpu_package.CacheLinePad __ = default!)
        {
            this._ = _;
            this.HasAES = HasAES;
            this.HasADX = HasADX;
            this.HasAVX = HasAVX;
            this.HasAVX2 = HasAVX2;
            this.HasAVX512F = HasAVX512F;
            this.HasAVX512BW = HasAVX512BW;
            this.HasAVX512VL = HasAVX512VL;
            this.HasBMI1 = HasBMI1;
            this.HasBMI2 = HasBMI2;
            this.HasERMS = HasERMS;
            this.HasFMA = HasFMA;
            this.HasOSXSAVE = HasOSXSAVE;
            this.HasPCLMULQDQ = HasPCLMULQDQ;
            this.HasPOPCNT = HasPOPCNT;
            this.HasRDTSCP = HasRDTSCP;
            this.HasSHA = HasSHA;
            this.HasSSE3 = HasSSE3;
            this.HasSSSE3 = HasSSSE3;
            this.HasSSE41 = HasSSE41;
            this.HasSSE42 = HasSSE42;
            this.__ = __;
        }
        
        // Handle comparisons between struct 'X86ᴛ1' instances
        public bool Equals(X86ᴛ1 other) =>
            _ == other._ &&
            HasAES == other.HasAES &&
            HasADX == other.HasADX &&
            HasAVX == other.HasAVX &&
            HasAVX2 == other.HasAVX2 &&
            HasAVX512F == other.HasAVX512F &&
            HasAVX512BW == other.HasAVX512BW &&
            HasAVX512VL == other.HasAVX512VL &&
            HasBMI1 == other.HasBMI1 &&
            HasBMI2 == other.HasBMI2 &&
            HasERMS == other.HasERMS &&
            HasFMA == other.HasFMA &&
            HasOSXSAVE == other.HasOSXSAVE &&
            HasPCLMULQDQ == other.HasPCLMULQDQ &&
            HasPOPCNT == other.HasPOPCNT &&
            HasRDTSCP == other.HasRDTSCP &&
            HasSHA == other.HasSHA &&
            HasSSE3 == other.HasSSE3 &&
            HasSSSE3 == other.HasSSSE3 &&
            HasSSE41 == other.HasSSE41 &&
            HasSSE42 == other.HasSSE42 &&
            __ == other.__;
        
        public override bool Equals(object? obj) => obj is X86ᴛ1 other && Equals(other);
        
        public override int GetHashCode() => runtime.HashCode.Combine(
            _,
            HasAES,
            HasADX,
            HasAVX,
            HasAVX2,
            HasAVX512F,
            HasAVX512BW,
            HasAVX512VL,
            HasBMI1,
            HasBMI2,
            HasERMS,
            HasFMA,
            HasOSXSAVE,
            HasPCLMULQDQ,
            HasPOPCNT,
            HasRDTSCP,
            HasSHA,
            HasSSE3,
            HasSSSE3,
            HasSSE41,
            HasSSE42,
            __);
        
        public static bool operator ==(X86ᴛ1 left, X86ᴛ1 right) => left.Equals(right);
        
        public static bool operator !=(X86ᴛ1 left, X86ᴛ1 right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'X86ᴛ1'
        public static bool operator ==(X86ᴛ1 value, NilType nil) => value.Equals(default(X86ᴛ1));

        public static bool operator !=(X86ᴛ1 value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, X86ᴛ1 value) => value == nil;

        public static bool operator !=(NilType nil, X86ᴛ1 value) => value != nil;

        public static implicit operator X86ᴛ1(NilType nil) => default(X86ᴛ1);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            _.ToString(),
            HasAES.ToString(),
            HasADX.ToString(),
            HasAVX.ToString(),
            HasAVX2.ToString(),
            HasAVX512F.ToString(),
            HasAVX512BW.ToString(),
            HasAVX512VL.ToString(),
            HasBMI1.ToString(),
            HasBMI2.ToString(),
            HasERMS.ToString(),
            HasFMA.ToString(),
            HasOSXSAVE.ToString(),
            HasPCLMULQDQ.ToString(),
            HasPOPCNT.ToString(),
            HasRDTSCP.ToString(),
            HasSHA.ToString(),
            HasSSE3.ToString(),
            HasSSSE3.ToString(),
            HasSSE41.ToString(),
            HasSSE42.ToString(),
            __.ToString()
        ]), "}");
    }
}
