//******************************************************************************************************
//  HashCode.cs - Gbtc
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
//  04/03/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go.runtime;

/// <summary>
/// Provides a helper class for generating hash codes.
/// </summary>
public class HashCode
{
    /// <summary>
    /// Combines all values into a single hash code.
    /// </summary>
    /// <param name="objects">Source objects.</param>
    /// <returns>Combined hash code.</returns>
    /// <remarks>
    /// System hash code combine function combines up to 8 objects, this function works with
    /// any number by combing the first 8 then combining the remainder in groups of 8.
    /// </remarks>
    public static int Combine(params object[] objects)
    {
        switch (objects.Length)
        {
            case 0:
                return 0;
            case <= 8:
                // Use the built-in HashCode.Combine for 1-8 objects for optimal performance
                return objects.Length switch
                {
                    1 => System.HashCode.Combine(objects[0]),
                    2 => System.HashCode.Combine(objects[0], objects[1]),
                    3 => System.HashCode.Combine(objects[0], objects[1], objects[2]),
                    4 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3]),
                    5 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4]),
                    6 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5]),
                    7 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6]),
                    8 => System.HashCode.Combine(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6], objects[7]),
                    _ => 0 // Won't reach here due to the condition
                };
        }

        // For more than 8 objects, combine in groups of 8 and then combine the results
        int hashCode = 0;
    
        for (int i = 0; i < objects.Length; i += 8)
        {
            int remaining = Math.Min(8, objects.Length - i);
        
            int groupHash = remaining switch
            {
                1 => System.HashCode.Combine(objects[i]),
                2 => System.HashCode.Combine(objects[i], objects[i + 1]),
                3 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2]),
                4 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3]),
                5 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4]),
                6 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5]),
                7 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5], objects[i + 6]),
                8 => System.HashCode.Combine(objects[i], objects[i + 1], objects[i + 2], objects[i + 3], objects[i + 4], objects[i + 5], objects[i + 6], objects[i + 7]),
                _ => 0 // Won't reach here
            };

            hashCode = System.HashCode.Combine(hashCode, groupHash);
        }

        return hashCode;
    }
}
