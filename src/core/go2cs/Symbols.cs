//******************************************************************************************************
//  Symbols.cs - Gbtc
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
//  04/20/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace go2cs;

public static class Symbols
{
    // Extended Unicode characters are being used to help avoid conflicts with Go identifiers for
    // symbols, markers, intermediate and temporary variables. These characters have to be valid
    // C# identifiers, i.e., Unicode letter characters, decimal digit characters, connecting
    // characters, combining characters, or formatting characters. Some character variants will
    // be better suited to different fonts or display environments. Defaults have been chosen
    // based on better appearance with common Visual Studio code fonts, e.g., "Cascadia Mono".
    // Note: keep constants in sync with go2cs transpiler and golib core.
    public const string PointerPrefix = "ж";
    public const string AddressPrefix = "Ꮡ";
    public const string ShadowVarMarker = "Δ";
    public const string TypeAliasDot = "ꓸ";
    public const string EllipsisOperator = "ꓸꓸꓸ";
    public const string CapturedVarMarker = "ʗ";
    public const string TempVarMarker = "ᴛ";
}
