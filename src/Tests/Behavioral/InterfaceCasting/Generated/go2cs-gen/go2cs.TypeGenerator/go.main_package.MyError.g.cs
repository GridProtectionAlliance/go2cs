﻿//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System;

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public partial struct MyError
    {
        // Field References
        public static ref global::go.@string Ꮡdescription(ref MyError instance) => ref instance.description;
        
        // Constructors
        public MyError(NilType _)
        {
            this.description = default;
        }

        public MyError(global::go.@string description = default)
        {
            this.description = description;
        }
        
        // Enable comparisons between nil and MyError struct
        public static bool operator ==(MyError value, NilType nil) => value.Equals(default(MyError));

        public static bool operator !=(MyError value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, MyError value) => value == nil;

        public static bool operator !=(NilType nil, MyError value) => value != nil;

        public static implicit operator MyError(NilType nil) => default(MyError);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            description.ToString()
        ]), "}");
    }
}
