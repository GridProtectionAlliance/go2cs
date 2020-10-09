//******************************************************************************************************
//  FunctionSignature.cs - Gbtc
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
//  06/06/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace go2cs.Metadata
{
    public enum DerivedFunctionSignature
    {
        Method
    }

    [Serializable]
    public class FunctionSignature
    {
        public string Name;
        public Signature Signature;
        public string Comments;
        public bool IsPromoted;

        public virtual string GenerateLookup() => Generate(Name);

        public virtual string Generate() => Generate(Name, Signature.Parameters.Select(parameter => parameter.Type.TypeName));

        public string GetParameterNames() => string.Join(", ", Signature.Parameters.Select(parameter => parameter.Name));

        public string GetParameterTypeNames() => string.Join(", ", Signature.Parameters.Select(parameter => parameter.Type.TypeName));

        public static string Generate(string functionName, IEnumerable<string> parameterTypeNames = null)
        {
            if (parameterTypeNames is null)
                parameterTypeNames = Enumerable.Empty<string>();

            return $"{functionName}({string.Join(", ", parameterTypeNames)})";
        }
    }
}
