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

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    internal partial struct fakeError
    {
        // Promoted Struct References
        // -- fakeError has no promoted structs

        // Field References
        // -- fakeError has no defined fields
        
        // Constructors
        public fakeError(NilType _)
        {
        }

        
        // Handle comparisons between struct 'fakeError' instances
        public bool Equals(fakeError other) =>
            true /* empty */;
        
        public override bool Equals(object? obj) => obj is fakeError other && Equals(other);
        
        public override int GetHashCode() => base.GetHashCode();
        
        public static bool operator ==(fakeError left, fakeError right) => left.Equals(right);
        
        public static bool operator !=(fakeError left, fakeError right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'fakeError'
        public static bool operator ==(fakeError value, NilType nil) => value.Equals(default(fakeError));

        public static bool operator !=(fakeError value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, fakeError value) => value == nil;

        public static bool operator !=(NilType nil, fakeError value) => value != nil;

        public static implicit operator fakeError(NilType nil) => default(fakeError);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            ""
        ]), "}");
    }
}
