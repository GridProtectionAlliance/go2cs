//******************************************************************************************************
//  GoImplementAttribute.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  01/13/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go;

/// <summary>
/// Marks an assembly with a structure interface implementation mapping.
/// </summary>
/// <typeparam name="TStruct">Struct type that implements interface.</typeparam>
/// <typeparam name="TInterface">Interface type to implement.</typeparam>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# code needed to implement
/// an interface for a structure using matching receiver methods.
/// </para>
/// <para>
/// See the <c>ImplementGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoImplementAttribute<TStruct, TInterface> : Attribute
{
    /// <summary>
    /// Gets or sets flag that determines if interface is a promoted
    /// field in the structure.
    /// </summary>
    public bool Promoted { get; set; }

    /// <summary>
    /// Gets or sets flag indicating the Go interface cast source was a POINTER to the struct
    /// (<c>var s Iface = &amp;t</c>). The generator then emits an <see cref="IжAdapter"/> wrapper
    /// class holding the receiver box <c>ж&lt;TStruct&gt;</c> — the interface value aliases the
    /// original box exactly as Go's interface holds the <c>*T</c> — instead of the value-boxing
    /// partial-struct implementation (which copies, and cannot bind direct-ж receiver methods).
    /// </summary>
    public bool Pointer { get; set; }
}

/// <summary>
/// Implemented by generated interface-implementation adapters that wrap the receiver box
/// <c>ж&lt;T&gt;</c> of a POINTER-sourced Go interface value.
/// </summary>
/// <remarks>
/// A type assert back to the Go pointer type (<c>s.(*T)</c>) targets the box itself, so the
/// assert machinery unwraps the adapter through <see cref="Box"/>. Adapter equality also
/// delegates to box identity, matching Go pointer-interface equality.
/// </remarks>
public interface IжAdapter
{
    /// <summary>
    /// Gets the wrapped receiver box (a <c>ж&lt;T&gt;</c>).
    /// </summary>
    object? Box { get; }
}
