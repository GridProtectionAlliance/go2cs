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
    partial struct fakeReader : go.main_package.WithInlineField_R
    {
        // 'WithInlineField_R.Read()' explicit implementation mapped to direct struct receiver method:
        (nint, go.error) go.main_package.WithInlineField_R.Read(go.slice<byte> _) => this.Read(_);
    }
}
