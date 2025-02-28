//******************************************************************************************************
//  comparable.cs - Gbtc
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
//  11/26/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System.Numerics;

namespace go;

// TODO: Delete this, native types do not implement this interface - must use IEqualityOperators directly instead when comparable is encountered

/// <summary>
/// Represents the C# implementation of the Go built-in <c>comparable</c> constraint.
/// </summary>
/// <remarks>
/// Defines constraints for the `==`, `!=` operators.
/// </remarks>
/// <typeparam name="T">Constrained type.</typeparam>
public interface comparable<T> : 
    IEqualityOperators<T, T, bool> 
    where T : comparable<T>
{
}
