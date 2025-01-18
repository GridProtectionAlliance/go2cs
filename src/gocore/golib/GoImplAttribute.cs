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
/// </para>
/// See the <c>ImplGenerator</c> in the go2cs code generators for details.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoImplAttribute<TStruct, TInterface> : Attribute
{
    /// <summary>
    /// Gets the type of target that implements interface.
    /// </summary>
    public Type TargetType => typeof(TStruct);

    /// <summary>
    /// Gets the type of interface to implement.
    /// </summary>
    public Type InterfaceType => typeof(TInterface);
}
