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
    public partial struct T1
    {
        // Field References
        public static ref global::go.@string Ꮡname(ref T1 instance) => ref instance.name;
        
        // Constructors
        public T1(NilType _)
        {
            this.name = default;
        }

        public T1(global::go.@string name = default)
        {
            this.name = name;
        }
        
        // Enable comparisons between nil and T1 struct
        public static bool operator ==(T1 value, NilType nil) => value.Equals(default(T1));

        public static bool operator !=(T1 value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, T1 value) => value == nil;

        public static bool operator !=(NilType nil, T1 value) => value != nil;

        public static implicit operator T1(NilType nil) => default(T1);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            name.ToString()
        ]), "}");
    }
}
