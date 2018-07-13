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
using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string IfElseMarker = ">>MARKER:IFELSE_LEVEL_{0}<<";
        public const string IfExpressionMarker = ">>MARKER:IFEXPR_LEVEL_{0}<<";

        private int m_ifExpressionLevel;

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

            if (Expressions.TryGetValue(context.expression(0), out string channel) && Expressions.TryGetValue(context.expression(1), out string value))
            {
                m_targetFile.Append($"{Spacing()}{channel}.Send({value});{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find both channel and value expression for send statement: {context.GetText()}");
            }
        }

        public override void ExitExpressionStmt(GolangParser.ExpressionStmtContext context)
        {
            // expressionStmt
            //     : expression

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}{expression};{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
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

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}{expression}{context.children[1].GetText()};{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for inc/dec statement: {context.GetText()}");
            }
        }

        public override void ExitAssignment(GolangParser.AssignmentContext context)
        {
            // assignment
            //     : expressionList assign_op expressionList

            if (ExpressionLists.TryGetValue(context.expressionList(0), out string[] leftOperands) && ExpressionLists.TryGetValue(context.expressionList(1), out string[] rightOperands))
            {
                if (leftOperands.Length != rightOperands.Length)
                    AddWarning(context, $"Encountered count mismatch for left and right operand expressions in assignment statement: {context.GetText()}");

                int length = Math.Min(leftOperands.Length, rightOperands.Length);
               
                for (int i = 0; i < length; i++)
                {
                    string assignOP = context.assign_op().GetText();
                    string notOP = "";

                    if (assignOP.Equals("&^=", StringComparison.Ordinal))
                    {
                        assignOP = " &= ";
                        notOP = "~";
                    }
                    else
                    {
                        assignOP = $" {assignOP} ";
                    }

                    m_targetFile.Append($"{Spacing()}{leftOperands[i]}{assignOP}{notOP}{rightOperands[i]};");

                    // Since multiple assignments can be on one line, only check for comments after last assignment
                    if (i < length - 1)
                    {
                        m_targetFile.AppendLine();
                    }
                    else
                    {
                        m_targetFile.Append($"{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                        if (!WroteLineFeed)
                            m_targetFile.AppendLine();
                    }
                }
            }
            else
            {
                AddWarning(context, $"Failed to find both left and right operand expressions for assignment statement: {context.GetText()}");
            }
        }

        public override void ExitShortVarDecl(GolangParser.ShortVarDeclContext context)
        {
            // shortVarDecl
            //     : identifierList ':=' expressionList

            if (Identifiers.TryGetValue(context.identifierList(), out string[] identifiers) && ExpressionLists.TryGetValue(context.expressionList(), out string[] expressions))
            {
                if (identifiers.Length != expressions.Length)
                    AddWarning(context, $"Encountered count mismatch for identifiers and expressions in short var declaration statement: {context.GetText()}");

                int length = Math.Min(identifiers.Length, expressions.Length);

                for (int i = 0; i < length; i++)
                {
                    m_targetFile.Append($"{Spacing()}var {identifiers[i]} = {expressions[i]};");

                    // Since multiple declarations can be on one line, only check for comments after last declaration
                    if (i < length - 1)
                    {
                        m_targetFile.AppendLine();
                    }
                    else
                    {
                        m_targetFile.Append($"{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                        if (!WroteLineFeed)
                            m_targetFile.AppendLine();
                    }
                }
            }
            else
            {
                AddWarning(context, $"Failed to find both identifier and expression lists for short var declaration statement: {context.GetText()}");
            }
        }

        public override void ExitGoStmt(GolangParser.GoStmtContext context)
        {
            // goStmt
            //     : 'go' expression

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                RequiredUsings.Add("System.Threading");
                m_targetFile.Append($"{Spacing()}ThreadPool.QueueUserWorkItem(state => {expression});{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
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

            m_targetFile.Append($";{CheckForCommentsRight(context, preserveLineFeeds: true)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitBreakStmt(GolangParser.BreakStmtContext context)
        {
            // breakStmt
            //     : 'break' IDENTIFIER ?

            string label = SanitizedIdentifier(context.IDENTIFIER().GetText());
        }

        public override void ExitContinueStmt(GolangParser.ContinueStmtContext context)
        {
            // continueStmt
            //     : 'continue' IDENTIFIER ?

            string label = SanitizedIdentifier(context.IDENTIFIER().GetText());
        }

        public override void ExitGotoStmt(GolangParser.GotoStmtContext context)
        {
            // gotoStmt
            //     : 'goto' IDENTIFIER

            m_targetFile.Append($"{Spacing()}goto {SanitizedIdentifier(context.IDENTIFIER().GetText())};{CheckForCommentsRight(context, preserveLineFeeds: true)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitFallthroughStmt(GolangParser.FallthroughStmtContext context)
        {
            // fallthroughStmt
            //     : 'fallthrough'
        }

        public override void EnterIfStmt(GolangParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if '(simpleStmt ';') ? expression block ( 'else' ( ifStmt | block ) ) ?

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Any declared variable will be scoped to if statement, so create a sub-block for it
                m_targetFile.AppendLine($"{Spacing()}{{");
                IndentLevel++;
            }

            m_ifExpressionLevel++;
            m_targetFile.AppendLine($"{Spacing()}{string.Format(IfElseMarker, m_ifExpressionLevel)}if ({string.Format(IfExpressionMarker, m_ifExpressionLevel)})");

            if (context.block().Length == 2)
            {
                PushBlockSuffix(Environment.NewLine);
                PushBlockSuffix($"{Environment.NewLine}{Spacing()}else{Environment.NewLine}");
            }
            else
            {
                PushBlockSuffix(Environment.NewLine);
            }
        }

        public override void ExitIfStmt(GolangParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if '(simpleStmt ';') ? expression block ( 'else' ( ifStmt | block ) ) ?

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                // Replace if statement markers
                m_targetFile.Replace(string.Format(IfExpressionMarker, m_ifExpressionLevel), expression);
                m_targetFile.Replace(string.Format(IfElseMarker, m_ifExpressionLevel), context.Parent is GolangParser.IfStmtContext ? "else " : "");
            }
            else
            {
                AddWarning(context, $"Failed to find expression for if statement: {context.GetText()}");
            }

            m_ifExpressionLevel--;

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Close any locally scoped declared variable sub-block
                IndentLevel--;
                m_targetFile.AppendLine($"{Spacing()}}}");
            }
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