//******************************************************************************************************
//  GoManualConversionAttribute.cs - Gbtc
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
//  03/14/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go;

/// <summary>
/// Mark a file as having been manually converted from Go to C#.
/// </summary>
/// <remarks>
/// <para>
/// This attribute should be applied to files that have been manually converted from Go to C#.
/// When encountered, it indicates that the file should not be automatically converted again.
/// </para>
/// <para>
/// Note that the module-level scope is used as a pseudo file-level marker for this attribute.
/// </para>
/// </remarks>
// Note: This attribute uses 'module' scope, which technically applies at the compiled module
// level. Since typical builds produce only a single module from multiple source files, this
// attribute acts effectively as a file-level marker to indicate this file has been manually
// converted from Go to C#. Multiple instances across different source files within the same
// module are allowed by the compiler without conflict, serving the intended purpose. The go2cs
// conversion tool detects this attribute by scanning the target source code file directly, thus
// identifying when to skip re-conversion. Note that actual module-level metadata stored in the
// assembly will likely serialize only a single instance of this attribute, even when multiple
// instances are defined across various files. As a result, reflection-based detection of this
// attribute only confirms that at least one file in the module was manually converted, not
// identifying individual files.
[AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
public class GoManualConversionAttribute : Attribute
{
}
