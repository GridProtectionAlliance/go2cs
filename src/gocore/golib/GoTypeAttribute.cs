//******************************************************************************************************
//  GoTypeAttribute.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  09/15/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;

namespace go;

/// <summary>
/// Marks a struct or interface for automatic code generation.
/// </summary>
/// <param name="definition">Type definition string, if applicable.</param>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# type code needed to emulate
/// behaviour for a Go type definition.
/// </para>
/// <para>
/// See the <c>TypeGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
/// <param name="definition">Type definition string.</param>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Interface)]
public class GoTypeAttribute(string definition = "") : Attribute
{
    /// <summary>
    /// Gets the type definition string.
    /// </summary>
    public string Definition => definition;
}
