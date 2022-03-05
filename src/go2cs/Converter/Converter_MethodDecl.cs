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
using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs;

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
        PushInnerBlockPrefix(string.Format(FunctionBlockPrefixMarker, CurrentFunctionName));

        m_targetFile.Append($"{Spacing()}{m_functionResultTypeMarker} {CurrentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");

        if (Options.UseAnsiBraceStyle)
            m_targetFile.AppendLine();
    }

    public override void ExitMethodDecl(GoParser.MethodDeclContext context)
    {
        if (CurrentFunction.Signature is not MethodSignature method)
            throw new InvalidOperationException($"Failed to find signature metadata for method function \"{CurrentFunctionName}\".");

        bool hasDefer = CurrentFunction.HasDefer;
        bool hasPanic = CurrentFunction.HasPanic;
        bool hasRecover = CurrentFunction.HasRecover;
        bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
        Signature signature = method.Signature;
        string receiverParametersSignature = method.GenerateReceiverParametersSignature();
        string parametersSignature = signature.GenerateParametersSignature();
        string resultSignature = signature.GenerateResultSignature();
        ParameterInfo[] receiverParameters = method.ReceiverParameters ?? Array.Empty<ParameterInfo>();

        if (signature.Parameters.Length == 0)
            parametersSignature = $"({receiverParametersSignature})";
        else
            parametersSignature = $"({receiverParametersSignature}, {parametersSignature})";

        // Scope of an extension function is based on scope of the receiver type
        string receiverType = receiverParameters.Length > 0 ? receiverParameters[0].Type.TypeName : "object";
        string scope = char.IsUpper(receiverType[0]) ? "public" : "private";
        resultSignature = $"{scope} static {resultSignature}";
        string blockPrefix = string.Empty;

        StringBuilder resultParameters = new StringBuilder();
        StringBuilder arrayClones = new StringBuilder();
        StringBuilder implicitPointers = new StringBuilder();

        foreach (ParameterInfo parameter in signature.Result)
        {
            if (!string.IsNullOrEmpty(parameter.Name))
                resultParameters.AppendLine($"{Spacing(1)}{parameter.Type.TypeName} {parameter.Name} = default{(parameter.Type is PointerTypeInfo || parameter.Type.TypeClass == TypeClass.Interface ? "!" : string.Empty)};");
        }

        foreach (ParameterInfo parameter in receiverParameters.Concat(signature.Parameters))
        {
            // For any array parameters, Go copies the array by value
            if (parameter.Type.TypeClass == TypeClass.Array)
                arrayClones.AppendLine($"{Spacing(1)}{parameter.Name} = {parameter.Name}.Clone();");

            // All pointers in Go can be implicitly dereferenced, so setup a "local ref" instance to each
            if (parameter.Type is PointerTypeInfo pointer)
                implicitPointers.AppendLine($"{Spacing(1)}ref {pointer.TargetTypeInfo.TypeName} {parameter.Name} = ref {AddressPrefix}{parameter.Name}.val;");
        }

        if (resultParameters.Length > 0)
        {
            resultParameters.Insert(0, Environment.NewLine);
            blockPrefix += resultParameters.ToString();
        }

        if (arrayClones.Length > 0)
        {
            if (blockPrefix.Length == 0)
                arrayClones.Insert(0, Environment.NewLine);

            blockPrefix += arrayClones.ToString();
        }

        if (implicitPointers.Length > 0)
        {
            if (blockPrefix.Length == 0)
                implicitPointers.Insert(0, Environment.NewLine);

            blockPrefix += implicitPointers.ToString();

            StringBuilder updatedSignature = new StringBuilder();
            bool initialParam = true;

            foreach (ParameterInfo parameter in receiverParameters.Concat(signature.Parameters))
            {
                if (initialParam)
                    updatedSignature.Append("this ");
                else 
                    updatedSignature.Append(", ");

                initialParam = false;
                updatedSignature.Append($"{(parameter.IsVariadic ? "params " : string.Empty)}{parameter.Type.TypeName} ");

                if (parameter.Type is PointerTypeInfo)
                    updatedSignature.Append(AddressPrefix);

                updatedSignature.Append(parameter.Name);
            }

            parametersSignature = $"({updatedSignature})";
        }

        // Replace function markers
        m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
        m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

        if (useFuncExecutionContext)
        {
            Stack<string> unusedNames = new Stack<string>(new[] { Options.WriteLegacyCompatibleCode ? "__" : "_", "_" });
            m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : unusedNames.Pop())}, {(hasPanic ? "panic" : unusedNames.Pop())}, {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
        }
        else
        {
            m_targetFile.Replace(m_functionExecContextMarker, string.Empty);
        }

        m_targetFile.Replace(string.Format(FunctionBlockPrefixMarker, CurrentFunctionName), blockPrefix);

        if (useFuncExecutionContext)
            m_targetFile.Append(");");

        m_targetFile.Append(CheckForCommentsRight(context));

        base.ExitMethodDecl(context);
    }
}
