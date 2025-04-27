//******************************************************************************************************
//  RuntimeErrorPanic.cs - Gbtc
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
//  12/27/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace go.runtime;

/// <summary>
/// Represents common runtime error messages thrown in Go environment.
/// </summary>
public static class RuntimeErrorPanic
{
    private const string RuntimeErrorMessage = "runtime error: ";

    private const string NilPointerDereferenceMessage = $"{RuntimeErrorMessage}invalid memory address or nil pointer dereference";
    public static PanicException NilPointerDereference()
    {
        return new PanicException(NilPointerDereferenceMessage);
    }

    private const string IndexOutOfRangeMessage = $"{RuntimeErrorMessage}index out of range [{{0}}] with length {{1}}";
    public static PanicException IndexOutOfRange(int64 index, int64 length)
    {
        return new PanicException(string.Format(IndexOutOfRangeMessage, index, length));
    }
}
