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
    internal partial struct main_Person_Address
    {
        // Promoted Struct References
        // -- main_Person_Address has no promoted structs

        // Field References
        public static ref global::go.@string ᏑStreet(ref main_Person_Address instance) => ref instance.Street;
        public static ref global::go.@string ᏑCity(ref main_Person_Address instance) => ref instance.City;
        
        // Constructors
        public main_Person_Address(NilType _)
        {
            this.Street = default!;
            this.City = default!;
        }

        public main_Person_Address(global::go.@string Street = default!, global::go.@string City = default!)
        {
            this.Street = Street;
            this.City = City;
        }
        
        // Handle comparisons between struct 'main_Person_Address' instances
        public bool Equals(main_Person_Address other) =>
            Street == other.Street &&
            City == other.City;
        
        public override bool Equals(object? obj) => obj is main_Person_Address other && Equals(other);
        
        public override int GetHashCode() => runtime.HashCode.Combine(
            Street,
            City);
        
        public static bool operator ==(main_Person_Address left, main_Person_Address right) => left.Equals(right);
        
        public static bool operator !=(main_Person_Address left, main_Person_Address right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'main_Person_Address'
        public static bool operator ==(main_Person_Address value, NilType nil) => value.Equals(default(main_Person_Address));

        public static bool operator !=(main_Person_Address value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, main_Person_Address value) => value == nil;

        public static bool operator !=(NilType nil, main_Person_Address value) => value != nil;

        public static implicit operator main_Person_Address(NilType nil) => default(main_Person_Address);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            Street.ToString(),
            City.ToString()
        ]), "}");
    }
}
