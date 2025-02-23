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

using System;

namespace go;

/// <summary>
/// Represents the C# implementation of the Go built-in <c>comparable</c> constraint.
/// </summary>
public interface comparable<T> : IEquatable<T> where T : comparable<T>
{
    public static virtual bool operator ==(T left, T right)
    {
        return left.Equals(right);
    }

    public static virtual bool operator !=(T left, T right)
    {
        return !left.Equals(right);
    }
}
