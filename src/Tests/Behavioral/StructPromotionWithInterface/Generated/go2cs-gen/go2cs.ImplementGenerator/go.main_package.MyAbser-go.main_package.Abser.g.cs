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
    partial struct MyAbser : go.main_package.Abser
    {
        // 'Abser.Abs()' explicit implementation mapped to direct struct receiver method:
        double go.main_package.Abser.Abs() => this.Abs();

        // Handle comparisons between struct 'MyAbser' and interface 'Abser'
        public static bool operator ==(MyAbser src, go.main_package.Abser iface) => iface is MyAbser val && val == src;
        
        public static bool operator !=(MyAbser src, go.main_package.Abser iface) => !(src == iface);
        
        public static bool operator ==(go.main_package.Abser iface, MyAbser src) => iface is MyAbser val && val == src;
        
        public static bool operator !=(go.main_package.Abser iface, MyAbser src) => !(iface == src);
    }
}
