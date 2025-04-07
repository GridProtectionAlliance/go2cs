//******************************************************************************************************
//  GoRecvAttribute.cs - Gbtc
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
//  01/15/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go;

/// <summary>
/// Marks a reference based receiver method for automatic code generation.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate a pointer receiver method that
/// references a <see cref="ж{T}"/> type which calls this receiver method
/// declared with a <see langword="ref" /> type.
/// </para>
/// <para>
/// See the <c>RecvGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
/// <param name="options">Receiver options string.</param>
[AttributeUsage(AttributeTargets.Method)]
public class GoRecvAttribute(string options = "") : Attribute
{
    /// <summary>
    /// Gets the receiver options string.
    /// </summary>
    public string Options => options;
}
