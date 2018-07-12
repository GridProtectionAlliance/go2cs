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
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public override void EnterLabeledStmt(GolangParser.LabeledStmtContext context)
        {
            PushBlock();
        }

        public override void ExitLabeledStmt(GolangParser.LabeledStmtContext context)
        {
            // labeledStmt
            //     : IDENTIFIER ':' statement

            string label = SanitizedIdentifier(context.IDENTIFIER().GetText());
            string statement = PopBlock(false);

            m_targetFile.Append($"{label}:{Environment.NewLine}{Environment.NewLine}{statement}");
        }

        public override void ExitSendStmt(GolangParser.SendStmtContext context)
        {
            // sendStmt
            //     : expression '<-' expression
        }

        public override void ExitExpressionStmt(GolangParser.ExpressionStmtContext context)
        {
            // expressionStmt
            //     : expression

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}{expression};{CheckForCommentsRight(context)}");

                if (!WroteCommentWithLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for expression statement: {context.GetText()}");
            }
        }

        public override void EnterIncDecStmt(GolangParser.IncDecStmtContext context)
        {
            // incDecStmt
            //     : expression('++' | '--')
        }

        public override void ExitAssignment(GolangParser.AssignmentContext context)
        {
            // assignment
            //     : expressionList assign_op expressionList
        }

        public override void ExitShortVarDecl(GolangParser.ShortVarDeclContext context)
        {
            // shortVarDecl
            //     : identifierList ':=' expressionList
        }

        public override void ExitGoStmt(GolangParser.GoStmtContext context)
        {
            // goStmt
            //     : 'go' expression

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                RequiredUsings.Add("System.Threading");
                m_targetFile.Append($"{Spacing()}ThreadPool.QueueUserWorkItem(state => {expression});{CheckForCommentsRight(context)}");

                if (!WroteCommentWithLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for go statement: {context.GetText()}");
            }
        }

        public override void ExitReturnStmt(GolangParser.ReturnStmtContext context)
        {
            // returnStmt
            //     : 'return' expressionList?

            m_targetFile.Append($"{Spacing()}return");

            if (ExpressionLists.TryGetValue(context.expressionList(), out string[] expressions))
            {
                if (expressions.Length > 1)
                    m_targetFile.Append($" ({string.Join(", ", expressions)})");
                else
                    m_targetFile.Append($" {expressions[0]}");
            }

            m_targetFile.Append($";{CheckForCommentsRight(context)}");

            if (!WroteCommentWithLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitBreakStmt(GolangParser.BreakStmtContext context)
        {
            // breakStmt
            //     : 'break' IDENTIFIER ?
        }

        public override void ExitContinueStmt(GolangParser.ContinueStmtContext context)
        {
            // continueStmt
            //     : 'continue' IDENTIFIER ?
        }

        public override void ExitGotoStmt(GolangParser.GotoStmtContext context)
        {
            // gotoStmt
            //     : 'goto' IDENTIFIER
        }

        public override void ExitFallthroughStmt(GolangParser.FallthroughStmtContext context)
        {
            // fallthroughStmt
            //     : 'fallthrough'
        }

        public override void ExitIfStmt(GolangParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if'(simpleStmt ';') ? expression block('else'(ifStmt | block)) ?
        }

        public override void ExitSwitchStmt(GolangParser.SwitchStmtContext context)
        {
            // switchStmt
            //     : exprSwitchStmt | typeSwitchStmt
        }

        public override void ExitSelectStmt(GolangParser.SelectStmtContext context)
        {
            // selectStmt
            //     : 'select' '{' commClause * '}'
        }

        public override void ExitForStmt(GolangParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for'(expression | forClause | rangeClause) ? block
        }

        public override void ExitDeferStmt(GolangParser.DeferStmtContext context)
        {
            // deferStmt
            //     : 'defer' expression
        }
    }
}