//******************************************************************************************************
//  GoTypeAliasAttribute.cs - Gbtc
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
/// Represents a Go type alias attribute.
/// </summary>
/// <param name="alias">Alias name.</param>
/// <param name="typeName">Type name of the alias.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoTypeAliasAttribute(string alias, string typeName) : Attribute
{
    /// <summary>
    /// Gets the alias name.
    /// </summary>
    public string Alias => alias;

    /// <summary>
    /// Gets the type name of the alias.
    /// </summary>
    public string TypeName => typeName;
}
