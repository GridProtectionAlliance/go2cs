//******************************************************************************************************
//  EmptyStruct.cs - Gbtc
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
//  05/09/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace go;

/// <summary>
/// Represents an empty struct.
/// </summary>
public struct EmptyStruct
{
    public EmptyStruct(NilType _)
    {
    }

    public bool Equals(EmptyStruct other) => true;

    public override bool Equals(object? obj) => obj is EmptyStruct other && Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(EmptyStruct left, EmptyStruct right) => left.Equals(right);

    public static bool operator !=(EmptyStruct left, EmptyStruct right) => !(left == right);

    // Handle comparisons between 'nil' and struct 'EmptyStruct'
    public static bool operator ==(EmptyStruct value, NilType nil) => value.Equals(default(EmptyStruct));

    public static bool operator !=(EmptyStruct value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, EmptyStruct value) => value == nil;

    public static bool operator !=(NilType nil, EmptyStruct value) => value != nil;

    public static implicit operator EmptyStruct(NilType nil) => default(EmptyStruct);

    public override string ToString() => "{}";
}
