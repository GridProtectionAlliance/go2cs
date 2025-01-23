//******************************************************************************************************
//  GoTestMatchingConsoleOutputAttribute.cs - Gbtc
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
//  01/22/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go;

/// <summary>
/// Mark a package as suitable for comparing console output of Go and converted C# during testing.
/// </summary>
/// <remarks>
/// This attribute should be applied to converted packages that have deterministic console output.
/// In these cases, the output of the Go package should match the output of the converted C# code.
/// Applying this attribute enables automated testing of the console output to ensure consistency
/// between Go and C# for improved regression testing.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class GoTestMatchingConsoleOutputAttribute : Attribute
{
}
