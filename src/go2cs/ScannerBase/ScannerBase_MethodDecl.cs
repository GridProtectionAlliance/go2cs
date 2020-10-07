//******************************************************************************************************
//  ScannerBase_MethodDecl.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  10/06/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using static go2cs.Common;

namespace go2cs
{
    public partial class ScannerBase
    {
        public override void EnterMethodDecl(GoParser.MethodDeclContext context)
        {
            InFunction = true;
            OriginalFunctionName = context.IDENTIFIER()?.GetText() ?? "_";
            CurrentFunctionName = SanitizedIdentifier(OriginalFunctionName);

            GoParser.ParameterDeclContext[] receiverParameters = context.receiver().parameters().parameterDecl();
            string receiverTypeName;

            if (receiverParameters.Length > 0)
            {
                receiverTypeName = receiverParameters[0].type_().GetText();

                // Receiver does not need to handle pointer-to-pointer look ups
                if (receiverTypeName.StartsWith("*"))
                    receiverTypeName = $"ptr<{receiverTypeName.Substring(1)}>";
            }
            else
            {
                receiverTypeName = "object";
            }

            string functionSignature = FunctionSignature.Generate(OriginalFunctionName, new[] { receiverTypeName });

            FunctionInfo currentFunction = null;
            Metadata?.Functions.TryGetValue(functionSignature, out currentFunction);
            CurrentFunction = currentFunction;
        }

        public override void ExitMethodDecl(GoParser.MethodDeclContext context)
        {
            CurrentFunction = null;
            CurrentFunctionName = null;
            OriginalFunctionName = null;
            InFunction = false;
        }
    }
}
