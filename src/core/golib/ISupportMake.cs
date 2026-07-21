//******************************************************************************************************
//  ISupportMake.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/27/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace go;

/// <summary>
/// Defines an interface to support 'make' function parameters.
/// </summary>
public interface ISupportMake<out T>
{
    /// <summary>
    /// Initializes type with make size and capacity parameters.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    static abstract T Make(nint p1, nint p2);
}
