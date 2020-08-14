//******************************************************************************************************
//  Converter_MethodDecl.cs - Gbtc
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
using System;
using System.Collections.Generic;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public override void EnterMethodDecl(GoParser.MethodDeclContext context)
        {
            m_inFunction = true; // May need to scope certain objects, like consts, to current function
            m_originalFunctionName = context.IDENTIFIER().GetText();
            m_currentFunctionName = SanitizedIdentifier(m_originalFunctionName);
            m_variableIdentifiers.Clear();
            m_variableTypes.Clear();

            string functionSignature = FunctionSignature.Generate(m_originalFunctionName);

            if (!Metadata.Functions.TryGetValue(functionSignature, out m_currentFunction))
                throw new InvalidOperationException($"Failed to find metadata for method function \"{functionSignature}\".");

            // Function signature containing result type and parameters have not been visited yet,
            // so we mark their desired positions and replace once the visit has occurred
            m_functionResultTypeMarker = string.Format(FunctionResultTypeMarker, m_currentFunctionName);
            m_functionParametersMarker = string.Format(FunctionParametersMarker, m_currentFunctionName);
            m_functionExecContextMarker = string.Format(FunctionExecContextMarker, m_currentFunctionName);

            m_targetFile.AppendLine($"{Spacing()}{m_functionResultTypeMarker} {m_currentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");
        }

        public override void ExitMethodDecl(GoParser.MethodDeclContext context)
        {
            if (!(m_currentFunction.Signature is MethodSignature method))
                throw new InvalidOperationException($"Failed to find signature metadata for method function \"{m_currentFunctionName}\".");

            bool hasDefer = m_currentFunction.HasDefer;
            bool hasPanic = m_currentFunction.HasPanic;
            bool hasRecover = m_currentFunction.HasRecover;
            bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
            Signature signature = method.Signature;
            string receiverParametersSignature = method.GenerateReceiverParametersSignature(useFuncExecutionContext);
            string parametersSignature = signature.GenerateParametersSignature(useFuncExecutionContext);
            string resultSignature = signature.GenerateResultSignature();

            if (signature.Parameters.Length == 0)
                parametersSignature = $"({receiverParametersSignature})";
            else
                parametersSignature = $"({receiverParametersSignature}, {parametersSignature})";

            // Scope of an extension function is based on scope of the receiver type
            string scope = char.IsUpper(method.ReceiverParameters[0].Type.TypeName[0]) ? "public" : "private";
            resultSignature = $"{scope} static {resultSignature}";

            // Replace function markers
            m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
            m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

            if (useFuncExecutionContext)
            {
                List<string> funcExecContextByRefParams = new List<string>(method.GetByRefReceiverParameters(false));
                Stack<string> unusedNames = new Stack<string>(new[] { "__", "_" });

                funcExecContextByRefParams.AddRange(signature.GetByRefParameters(false));

                if (funcExecContextByRefParams.Count > 0)
                {
                    List<string> lambdaByRefParameters = new List<string>(method.GetByRefReceiverParameters(true));

                    lambdaByRefParameters.AddRange(signature.GetByRefParameters(true));

                    m_targetFile.Replace(m_functionExecContextMarker, $" => func({string.Join(", ", funcExecContextByRefParams)}, ({string.Join(", ", lambdaByRefParameters)}, Defer {(hasDefer ? "defer" : unusedNames.Pop())}, Panic {(hasPanic ? "panic" : unusedNames.Pop())}, Recover {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
                }
                else
                {
                    m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : unusedNames.Pop())}, {(hasPanic ? "panic" : unusedNames.Pop())}, {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
                }
            }
            else
            {
                m_targetFile.Replace(m_functionExecContextMarker, "");
            }

            m_currentFunction = null;
            m_currentFunctionName = null;
            m_originalFunctionName = null;
            m_inFunction = false;

            if (useFuncExecutionContext)
                m_targetFile.Append(");");

            m_targetFile.Append(CheckForCommentsRight(context));
        }
    }
}
