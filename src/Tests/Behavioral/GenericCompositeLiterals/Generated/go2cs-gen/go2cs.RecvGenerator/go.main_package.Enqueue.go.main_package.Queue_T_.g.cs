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
using System.Threading;

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public static void Enqueue<T>(this ж<go.main_package.Queue<T>> Ꮡq, T item)
        where T : new()
    {
        ref var q = ref Ꮡq.val;
        q.Enqueue(item);
    }
}
