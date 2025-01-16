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
/// Represents a Go interface implementation attribute.
/// </summary>
/// <typeparam name="TInterface">Type of interface to implement.</typeparam>
/// <typeparam name="TTarget">Type of target that implements interface.</typeparam>
/// <remarks>
/// This attribute is used to auto-generate backend C# code needed to implement
/// an interface in a Go-like manner. See the <c>InterfaceGenerator</c> in the go2cs
/// code generators for operational details.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoImplAttribute<TInterface, TTarget> : Attribute
{
    /// <summary>
    /// Gets the type of interface to implement.
    /// </summary>
    public Type InterfaceType => typeof(TInterface);

    /// <summary>
    /// Gets the type of target that implements interface.
    /// </summary>
    public Type TargetType => typeof(TTarget);
}
