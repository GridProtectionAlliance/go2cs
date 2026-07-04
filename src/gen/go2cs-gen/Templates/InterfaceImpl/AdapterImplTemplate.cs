//******************************************************************************************************
//  AdapterImplTemplate.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/03/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the POINTER-sourced interface implementation adapter for a
/// <c>GoImplement&lt;TStruct, TInterface&gt;(Pointer = true)</c> attribute.
/// </summary>
/// <remarks>
/// A Go interface value created from a pointer (<c>var s Iface = &amp;t</c>) holds the
/// <em>pointer</em> — every call through the interface mutates the original object, and a type
/// assert back to <c>*T</c> recovers that same pointer. The value-boxing partial-struct
/// implementation cannot model this (the C# interface box is a COPY, and direct-ж receiver
/// methods cannot bind from a struct's <c>this</c> at all), so the adapter wraps the receiver
/// box <c>ж&lt;T&gt;</c> itself: interface members forward to the box (binding the direct-ж or
/// RecvGenerator ж-overload forms), equality is box identity (Go pointer equality), and the
/// golib type-assert machinery unwraps adapters through <c>IжAdapter.Box</c>.
/// </remarks>
internal class AdapterImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required string AdapterName;
    public required string AdapterScope;
    public required List<MethodInfo> Methods;

    // Maps an interface member's simple name to its forwarding receiver expression:
    // "m_box" when the struct method binds on the box (direct-ж primary form, or a
    // [GoRecv] ref extension whose RecvGenerator ж-twin exists), or "m_box.Value" for
    // a plain value-receiver method (Go copies the value at the call, matching ref-return
    // property access semantics here).
    public required Dictionary<string, string> ForwardReceivers;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Pointer-sourced '{{GetSimpleName(InterfaceName)}}' implementation adapter for 'ж&lt;{{StructName}}&gt;' —
             /// the interface value aliases the wrapped receiver box exactly as Go's interface holds the '*T'.
             /// </summary>
             {{AdapterScope}} sealed class {{AdapterName}} : {{InterfaceName}}, IжAdapter
             {
                 private readonly ж<{{StructName}}> m_box;

                 public {{AdapterName}}(ж<{{StructName}}> box) => m_box = box;

                 public object? Box => m_box;

                 {{MethodsImplementation}}

                 // Go pointer-interface equality is pointer identity: two interface values created
                 // from the same '*T' compare equal, as does the pointer box itself.
                 public override bool Equals(object? obj) => obj switch
                 {
                     IжAdapter adapter => ReferenceEquals(adapter.Box, m_box),
                     ж<{{StructName}}> box => ReferenceEquals(box, m_box),
                     _ => ReferenceEquals(obj, this)
                 };

                 public override int GetHashCode() => m_box?.GetHashCode() ?? 0;

                 public override string? ToString() => m_box?.ToString();
             }
         """;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                if (!ForwardReceivers.TryGetValue(simpleMethodName, out string? receiver))
                    receiver = "m_box";

                result.Append($"{method.ReturnType} {method.GetSignature()} => {receiver}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
            }

            return result.ToString();
        }
    }
}
