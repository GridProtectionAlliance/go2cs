//******************************************************************************************************
//  Converter_Signature.cs - Gbtc
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

using System;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        // Stack handlers:
        //  functionDecl (optional)
        //  function (required)
        //  methodDecl (optional)
        //  methodSpec (optional)
        //  functionType (required)
        private readonly ParseTreeValues<(string parameters, string result)> m_signatures = new ParseTreeValues<(string, string)>();
        private string m_result;

        public override void ExitSignature(GolangParser.SignatureContext context)
        {
            m_parameters.TryGetValue(context.parameters(), out string parameters);
            m_signatures[context] = (parameters, m_result ?? "void");
        }

        public override void EnterResult(GolangParser.ResultContext context)
        {
            m_result = null;
        }

        public override void ExitResult(GolangParser.ResultContext context)
        {
            //result
            //  : parameters
            //  | type
            if (!m_parameters.TryGetValue(context.parameters(), out m_result))
                if (m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo))
                    m_result = typeInfo.PrimitiveName;

            if (string.IsNullOrWhiteSpace(m_result))
                m_result = "void";
        }

        private string ExtractFunctionSignature(string parameters)
        {
            parameters = RemoveSurrounding(parameters, "(", ")");

            if (string.IsNullOrWhiteSpace(parameters))
                return "";

            string[] parameterItems = parameters.Split(',');
            string[] signatureItems = new string[parameterItems.Length];

            for (int i = 0; i < parameterItems.Length; i++)
            {
                string[] parts = parameterItems[i].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                    signatureItems[i] = parts[0];
                else
                    signatureItems[i] = parameterItems[i];
            }

            return string.Join(", ", signatureItems);
        }
    }
}
