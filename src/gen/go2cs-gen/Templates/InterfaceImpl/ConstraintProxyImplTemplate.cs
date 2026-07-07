//******************************************************************************************************
//  ConstraintProxyImplTemplate.cs - Gbtc
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
//  07/06/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the SELF-REFERENTIAL constraint proxy for a
/// <c>GoImplement&lt;TElement, TInterface&gt;(ConstraintProxy = true)</c> attribute.
/// </summary>
/// <remarks>
/// Go's <c>nistCurve[Point nistPoint[Point]]</c> instantiated with a pointer type
/// (<c>nistCurve[*P224Point]</c>) needs a C# type argument that NOMINALLY implements
/// <c>nistPoint&lt;itself&gt;</c>. The golib box <c>ж&lt;P224Point&gt;</c> can't (it's a sealed
/// golib type in another assembly, and Go's structural satisfaction has no C# nominal analog), so
/// this proxy wraps the box and implements the constraint interface over ITSELF. Implicit
/// <c>ж&lt;P224Point&gt;</c>↔proxy conversions marshal every self-typed (<c>T</c>) parameter and
/// result automatically, so each interface method just forwards to the box's extension method.
/// The proxy is substituted for the type argument at the constrained instantiation; a value
/// (<c>ж&lt;P224Point&gt;</c>) flowing into a <c>Point</c>-typed position converts implicitly.
/// </remarks>
internal class ConstraintProxyImplTemplate : TemplateBase
{
    // Template Parameters
    public required string ProxyName;       // e.g. "P224PointжnistPoint"
    public required string InterfaceRef;    // e.g. "global::go.crypto.elliptic_package.nistPoint<P224PointжnistPoint>"
    public required string ElementName;     // e.g. "global::go.crypto.@internal.nistec_package.P224Point"
    public required string AdapterScope;
    public required string MethodsImplementation;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Self-referential constraint proxy: '{{ElementName}}' satisfies its generic method-set
             /// constraint interface by wrapping the box 'ж&lt;{{ElementName}}&gt;' and implementing the
             /// interface over ITSELF, so it can serve as a constrained generic type argument.
             /// </summary>
             {{AdapterScope}} sealed class {{ProxyName}} : {{InterfaceRef}}, IжAdapter
             {
                 private readonly ж<{{ElementName}}> m_box;

                 public {{ProxyName}}(ж<{{ElementName}}> box) => m_box = box;

                 public object? Box => m_box;

                 // Implicit conversions marshal every self-typed (T) boundary automatically: a box
                 // flows INTO the proxy at the constrained instantiation, and the forwarders below
                 // unwrap T-typed arguments to the box and rewrap T-typed results back to the proxy.
                 public static implicit operator {{ProxyName}}(ж<{{ElementName}}> box) => new {{ProxyName}}(box);

                 public static implicit operator ж<{{ElementName}}>({{ProxyName}} proxy) => proxy.m_box;

                 {{MethodsImplementation}}

                 // Go pointer-interface equality is pointer identity: proxies over the same box compare equal.
                 public override bool Equals(object? obj) => obj switch
                 {
                     IжAdapter adapter => ReferenceEquals(adapter.Box, m_box),
                     ж<{{ElementName}}> box => ReferenceEquals(box, m_box),
                     _ => ReferenceEquals(obj, this)
                 };

                 public override int GetHashCode() => m_box?.GetHashCode() ?? 0;

                 public override string? ToString() => m_box?.ToString();
             }
         """;
}
