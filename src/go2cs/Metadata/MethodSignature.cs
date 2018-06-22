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
            IsPromoted = functionSignature.IsPromoted;
        }

        public override string Generate()
        {
            IEnumerable<ParameterInfo> parameters = ReceiverParameters.Concat(Signature.Parameters);
            return Generate(Name, parameters);
        }

        public string GenerateReceiverParametersSignature(bool prefixByRef)
        {
            return $"this {string.Join(", ", GetReceiverParameters(prefixByRef))}";
        }

        public IEnumerable<String> GetReceiverParameters(bool prefixByRef)
        {
            return ReceiverParameters.Select(parameter => $"{parameter.Type.PrimitiveName} {(prefixByRef && parameter.Type.IsByRefPointer ? "_" : "")}{parameter.Name}");
        }

        public IEnumerable<String> GetByRefReceiverParameters(bool includeType)
        {
            return ReceiverParameters.Where(parameter => parameter.Type.IsByRefPointer).Select(parameter => $"{(includeType ? $"{parameter.Type.PrimitiveName} " : "_")}{parameter.Name}");
        }
    }
}