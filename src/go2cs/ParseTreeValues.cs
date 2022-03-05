//******************************************************************************************************
//  ParseTreeValues.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  05/13/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using Antlr4.Runtime.Tree;

namespace go2cs;

public class ParseTreeValues<T> : Dictionary<IParseTree, T>
{
    /// <summary>Gets the value associated with the specified key.</summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key,
    /// if the key is found; otherwise, the default value for the type of the
    /// <paramref name="value" /> parameter. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="Dictionary{TKey, TValue}" /> contains an element with the specified key; otherwise, <c>false</c>.
    /// </returns>
    public new bool TryGetValue(IParseTree key, out T value)
    {
        if (key is null)
        {
            value = default;
            return false;
        }

        return base.TryGetValue(key, out value);
    }
}