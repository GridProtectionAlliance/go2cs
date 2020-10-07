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
            base.EnterMethodDecl(context);

            m_variableIdentifiers.Clear();
            m_variableTypes.Clear();


            if (CurrentFunction is null)
                throw new InvalidOperationException($"Failed to find metadata for method function \"{CurrentFunctionName}\".");

            // Function signature containing result type and parameters have not been visited yet,
            // so we mark their desired positions and replace once the visit has occurred
            m_functionResultTypeMarker = string.Format(FunctionResultTypeMarker, CurrentFunctionName);
            m_functionParametersMarker = string.Format(FunctionParametersMarker, CurrentFunctionName);
            m_functionExecContextMarker = string.Format(FunctionExecContextMarker, CurrentFunctionName);

            m_targetFile.AppendLine($"{Spacing()}{m_functionResultTypeMarker} {CurrentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");
        }

        public override void ExitMethodDecl(GoParser.MethodDeclContext context)
        {
            if (!(CurrentFunction.Signature is MethodSignature method))
                throw new InvalidOperationException($"Failed to find signature metadata for method function \"{CurrentFunctionName}\".");

            bool hasDefer = CurrentFunction.HasDefer;
            bool hasPanic = CurrentFunction.HasPanic;
            bool hasRecover = CurrentFunction.HasRecover;
            bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
            Signature signature = method.Signature;
            string receiverParametersSignature = method.GenerateReceiverParametersSignature();
            string parametersSignature = signature.GenerateParametersSignature();
            string resultSignature = signature.GenerateResultSignature();

            if (signature.Parameters.Length == 0)
                parametersSignature = $"({receiverParametersSignature})";
            else
                parametersSignature = $"({receiverParametersSignature}, {parametersSignature})";

            // Scope of an extension function is based on scope of the receiver type
            string receiverType = method.ReceiverParameters?.Length > 0 ? method.ReceiverParameters[0].Type.TypeName : "object";
            string scope = char.IsUpper(receiverType[0]) ? "public" : "private";
            resultSignature = $"{scope} static {resultSignature}";

            // Replace function markers
            m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
            m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

            if (useFuncExecutionContext)
            {
                Stack<string> unusedNames = new Stack<string>(new[] { "__", "_" });
                m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : unusedNames.Pop())}, {(hasPanic ? "panic" : unusedNames.Pop())}, {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
            }
            else
            {
                m_targetFile.Replace(m_functionExecContextMarker, "");
            }

            if (useFuncExecutionContext)
                m_targetFile.Append(");");

            m_targetFile.Append(CheckForCommentsRight(context));

            base.ExitMethodDecl(context);
        }
    }
}
