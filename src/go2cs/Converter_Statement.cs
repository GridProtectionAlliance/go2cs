//******************************************************************************************************
//  Converter_Statement.cs - Gbtc
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
//  05/04/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using go2cs.Metadata;

namespace go2cs
{
    public partial class Converter
    {
        private readonly Stack<StringBuilder> m_blocks = new Stack<StringBuilder>();

        public const string FunctionLiteralParametersMarker = ">>MARKER:FUNCTIONLITERAL_PARAMETERS<<";

        private void PushBlock()
        {
            m_blocks.Push(m_targetFile);
            m_targetFile = new StringBuilder();
        }

        private void PopBlock()
        {
            StringBuilder lastTarget = m_blocks.Pop();
            lastTarget.Append(m_targetFile);
            m_targetFile = lastTarget;
        }

        public override void EnterBlock(GolangParser.BlockContext context)
        {
            PushBlock();
            m_targetFile.AppendLine($"{Spacing()}{{");

            IndentLevel++;
        }

        public override void ExitBlock(GolangParser.BlockContext context)
        {
            IndentLevel--;

            m_targetFile.Append($"{Spacing()}}}");
            PopBlock();
        }

        public override void EnterFunctionLit(GolangParser.FunctionLitContext context)
        {
            m_targetFile.AppendLine($"{FunctionLiteralParametersMarker} =>");
        }

        public override void ExitFunctionLit(GolangParser.FunctionLitContext context)
        {
            string parametersSignature = "()";

            if (Signatures.TryGetValue(context.function()?.signature(), out Signature signature))
            {
                parametersSignature = signature.GenerateParameterNameList();

                if (signature.Parameters.Length != 1)
                    parametersSignature = $"({parametersSignature})";
            }
            else
            {
                AddWarning(context, $"Failed to find signature for function literal inside \"{m_currentFunctionName}\" function");
            }

            // Replace marker for function literal
            m_targetFile.Replace(FunctionLiteralParametersMarker, parametersSignature);
        }

        public override void ExitExpressionStmt(GolangParser.ExpressionStmtContext context)
        {
            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}{expression};{CheckForCommentsRight(context)}");

                if (!WroteCommentWithLineFeed)
                    m_targetFile.AppendLine();
            }
        }

        public override void EnterReturnStmt(GolangParser.ReturnStmtContext context)
        {
            m_targetFile.Append($"{Spacing()}return ");
            PushBlock();
        }

        public override void ExitReturnStmt(GolangParser.ReturnStmtContext context)
        {
            PopBlock();
            m_targetFile.Append($";{CheckForCommentsRight(context)}");

            if (!WroteCommentWithLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitGoStmt(GolangParser.GoStmtContext context)
        {
            Expressions.TryGetValue(context.expression(), out string expression);
            RequiredUsings.Add("System.Threading");
            m_targetFile.Append($"{Spacing()}ThreadPool.QueueUserWorkItem(state => {expression});");
        }
    }
}
