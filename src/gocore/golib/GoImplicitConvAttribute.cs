//******************************************************************************************************
//  GoImplicitConvAttribute.cs - Gbtc
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
//  02/05/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go;

/// <summary>
/// Marks an assembly with an implicit conversion mapping.
/// </summary>
/// <typeparam name="TSource">Source type for implicit conversion.</typeparam>
/// <typeparam name="TTarget">Target type for implicit conversion.</typeparam>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# code needed to implement
/// an implicit conversion from a source type to a target type.
/// </para>
/// <para>
/// See the <c>ImplicitConvGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoImplicitConvAttribute<TSource, TTarget> : Attribute
{
    /// <summary>
    /// Gets or sets flag that determines if operator conversion order should be inverted.
    /// </summary>
    public bool Inverted { get; set; }
}
