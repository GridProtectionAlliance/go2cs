//******************************************************************************************************
//  PreScanner_Signature.cs - Gbtc
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
//  05/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System.Collections.Generic;

namespace go2cs
{
    public partial class PreScanner
    {
        // Stack handlers:
        //  functionDecl (optional)
        //  function (required)
        //  methodDecl (optional)
        //  methodSpec (optional)
        //  functionType (required)
        private readonly ParseTreeValues<Signature> m_signatures = new ParseTreeValues<Signature>();
        private List<ParameterInfo> m_result;

        public override void EnterSignature(GolangParser.SignatureContext context)
        {
            m_result = new List<ParameterInfo>(new[] { new ParameterInfo
            {
                Name = "",
                Type = TypeInfo.VoidType,
                IsVariadic = false
            }});
        }

        public override void ExitSignature(GolangParser.SignatureContext context)
        {
            Parameters.TryGetValue(context.parameters(), out List<ParameterInfo> parameters);
            m_signatures[context] = new Signature
            {
                Parameters = parameters?.ToArray() ?? new ParameterInfo[0],
                Result = m_result.ToArray(),
            };
        }

        public override void ExitResult(GolangParser.ResultContext context)
        {
            //result
            //  : parameters
            //  | type
            if (!Parameters.TryGetValue(context.parameters(), out m_result))
            {
                if (Types.TryGetValue(context.type(), out TypeInfo typeInfo))
                {
                    m_result = new List<ParameterInfo>(new[] { new ParameterInfo
                    {
                        Name = "",
                        Type = typeInfo,
                        IsVariadic = false
                    }});
                }
            }
        }
    }
}
