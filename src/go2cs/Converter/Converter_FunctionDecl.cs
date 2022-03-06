//******************************************************************************************************
//  Converter_FunctionDecl.cs - Gbtc
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

namespace go2cs;

public partial class Converter
{
    public const string FunctionResultTypeMarker = ">>MARKER:FUNCTION_{0}_RESULTTYPE<<";
    public const string FunctionParametersMarker = ">>MARKER:FUNCTION_{0}_PARAMETERS<<";
    public const string FunctionExecContextMarker = ">>MARKER:FUNCTION_{0}_EXEC_CONTEXT<<";
    public const string FunctionBlockPrefixMarker = ">>MARKER:FUNCTION_{0}_BLOCK_PREFIX<<";

    private string m_functionResultTypeMarker;
    private string m_functionParametersMarker;
    private string m_functionExecContextMarker;

    public override void ExitUnaryExpr(GoParser.UnaryExprContext context)
    {
        base.ExitUnaryExpr(context);

        if (!InFunction || context.primaryExpr() is not null && PrimaryExpressions.ContainsKey(context.primaryExpr()) 
                        || context.expression() is null || !Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            return;

        ParameterInfo[] parameters = CurrentFunction.Signature.Signature.Parameters;
        string unaryOP = context.children[0].GetText();

        if (!unaryOP.Equals("*", StringComparison.Ordinal))
            return;

        ParameterInfo pointerParam = parameters.FirstOrDefault(parameter => parameter.Name.Equals(expression.Text));
                        
        if (pointerParam is not null && pointerParam.Type is PointerTypeInfo pointer)
        {
            TypeInfo targetType = pointer.TargetTypeInfo.Clone();
            targetType.IsByRefPointer = true;

            string derefPointerExpression = expression.Text;

            // Handle pointer-to-pointer dereferencing
            int derefs = pointer.Name.Count(chr => chr == '*');

            if (derefs > 1)
            {
                for (int i = 1; i < derefs; i++)
                    derefPointerExpression += ".val";
            }

            // Implicitly dereference pointer parameter when dereference operator (*) is used
            UnaryExpressions[context] = new()
            {
                Text = derefPointerExpression,
                Type = targetType
            };
        }
    }

    public override void EnterFunctionDecl(GoParser.FunctionDeclContext context)
    {
        base.EnterFunctionDecl(context);

        m_variableIdentifiers.Clear();
        m_variableTypes.Clear();

        if (CurrentFunction is null)
            throw new InvalidOperationException($"Failed to find metadata for function \"{CurrentFunctionName}\".");

        FunctionSignature function = CurrentFunction.Signature;

        if (function is null)
            throw new InvalidOperationException($"Failed to find signature metadata for function \"{CurrentFunctionName}\".");

        string scope = char.IsUpper(OriginalFunctionName[0]) ? "public" : "private";

        // Handle Go "main" function as a special case, in C# this should be capitalized "Main"
        if (CurrentFunctionName.Equals("main"))
        {
            CurrentFunctionName = "Main";
                
            // Track file names that contain main function in main package
            if (Package.Equals("main"))
                s_mainPackageFiles.Add(TargetFileName);
        }

        // Function signature containing result type and parameters have not been visited yet,
        // so we mark their desired positions and replace once the visit has occurred
        m_functionResultTypeMarker = string.Format(FunctionResultTypeMarker, CurrentFunctionName);
        m_functionParametersMarker = string.Format(FunctionParametersMarker, CurrentFunctionName);
        m_functionExecContextMarker = string.Format(FunctionExecContextMarker, CurrentFunctionName);
        PushInnerBlockPrefix(string.Format(FunctionBlockPrefixMarker, CurrentFunctionName));

        m_targetFile.Append($"{Spacing()}{scope} static {m_functionResultTypeMarker} {CurrentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");

        if (Options.UseAnsiBraceStyle)
            m_targetFile.AppendLine();
    }

    public override void ExitFunctionDecl(GoParser.FunctionDeclContext context)
    {
        bool signatureOnly = context.block() is null;

        FunctionSignature function = CurrentFunction.Signature;
        bool hasDefer = CurrentFunction.HasDefer;
        bool hasPanic = CurrentFunction.HasPanic;
        bool hasRecover = CurrentFunction.HasRecover;
        bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
        Signature signature = function.Signature;
        string parametersSignature = $"({signature.GenerateParametersSignature()})";
        string resultSignature = signature.GenerateResultSignature();
        string blockPrefix = string.Empty;

        if (!signatureOnly)
        {
            StringBuilder resultParameters = new();
            StringBuilder arrayClones = new();
            StringBuilder implicitPointers = new();

            foreach (ParameterInfo parameter in signature.Result)
            {
                if (!string.IsNullOrEmpty(parameter.Name))
                    resultParameters.AppendLine($"{Spacing(1)}{parameter.Type.TypeName} {parameter.Name} = default{(parameter.Type is PointerTypeInfo || parameter.Type.TypeClass == TypeClass.Interface ? "!" : string.Empty)};");
            }

            foreach (ParameterInfo parameter in signature.Parameters)
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

                StringBuilder updatedSignature = new();
                bool initialParam = true;

                foreach (ParameterInfo parameter in signature.Parameters)
                {
                    if (!initialParam)
                        updatedSignature.Append(", ");

                    initialParam = false;
                    updatedSignature.Append($"{(parameter.IsVariadic ? "params " : string.Empty)}{parameter.Type.TypeName} ");

                    if (parameter.Type is PointerTypeInfo)
                        updatedSignature.Append(AddressPrefix);

                    updatedSignature.Append(parameter.Name);
                }

                parametersSignature = $"({updatedSignature})";
            }
        }

        // Replace function markers
        m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
        m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

        if (useFuncExecutionContext)
        {
            Stack<string> unusedNames = new(new[] { Options.WriteLegacyCompatibleCode ? "__" : "_", "_" });
            m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : unusedNames.Pop())}, {(hasPanic ? "panic" : unusedNames.Pop())}, {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
        }
        else
        {
            m_targetFile.Replace(m_functionExecContextMarker, string.Empty);
        }

        m_targetFile.Replace(string.Format(FunctionBlockPrefixMarker, CurrentFunctionName), blockPrefix);

        if (useFuncExecutionContext)
            m_targetFile.Append(");");
        else if (signatureOnly)
            m_targetFile.Append(";");

        m_targetFile.Append(CheckForCommentsRight(context));

        base.ExitFunctionDecl(context);
    }
}
