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
using global::go;

#nullable enable

namespace go;

public static partial class main_package
{
    partial struct JavaProgrammer : go.main_package.Animal
    {
        // 'Animal.Speak()' explicit implementation mapped to direct struct receiver method:
        go.@string go.main_package.Animal.Speak() => this.Speak();

        // Handle comparisons between struct 'JavaProgrammer' and interface 'Animal'
        public static bool operator ==(JavaProgrammer src, go.main_package.Animal iface) => iface is JavaProgrammer val && val == src;
        
        public static bool operator !=(JavaProgrammer src, go.main_package.Animal iface) => !(src == iface);
        
        public static bool operator ==(go.main_package.Animal iface, JavaProgrammer src) => iface is JavaProgrammer val && val == src;
        
        public static bool operator !=(go.main_package.Animal iface, JavaProgrammer src) => !(iface == src);
    }
}
