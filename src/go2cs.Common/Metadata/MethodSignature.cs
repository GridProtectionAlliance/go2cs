//******************************************************************************************************
//  MethodSignature.cs - Gbtc
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
//  06/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace go2cs.Metadata
{
    [Serializable]
    public class MethodSignature : FunctionSignature
    {
        public ParameterInfo[] ReceiverParameters;

        public MethodSignature()
        {            
        }

        public MethodSignature(FunctionSignature functionSignature)
        {
            Name = functionSignature.Name;
            Signature = functionSignature.Signature;
            Comments = functionSignature.Comments;
            IsPromoted = functionSignature.IsPromoted;
        }

        // Go method declarations act overloadable, unlike function declarations
        public override string GenerateLookup() => Generate(Name, GetReceiverParameterTypeNames());

        public override string Generate()
        {
            IEnumerable<string> parameters = GetReceiverParameterTypeNames().Concat(Signature.Parameters.Select(parameter => parameter.Type.TypeName));
            return Generate(Name, parameters);
        }

        public IEnumerable<string> GetReceiverParameterTypeNames() => new[] { ReceiverParameters?.Length > 0 ? ReceiverParameters[0].Type.TypeName : "object" };

        public string GenerateReceiverParametersSignature() =>
            $"this {string.Join(", ", ReceiverParameters.Select(parameter => $"{parameter.Type.TypeName} {parameter.Name}"))}";
    }
}
