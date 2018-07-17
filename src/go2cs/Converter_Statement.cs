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

using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string IfElseMarker = ">>MARKER:IFELSE_LEVEL_{0}<<";
        public const string IfExpressionMarker = ">>MARKER:IFEXPR_LEVEL_{0}<<";
        public const string TypeSwitchExpressionMarker = ">>MARKER:TYPESWITCH_LEVEL_{0}<<";
        public const string TypeSwitchCaseTypeMarker = ">>MARKER:TYPESWITCHCASE_LEVEL_{0}<<";

        private readonly Dictionary<string, bool> m_labels = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Stack<HashSet<string>> m_blockLabeledContinues = new Stack<HashSet<string>>();
        private readonly Stack<HashSet<string>> m_blockLabeledBreaks = new Stack<HashSet<string>>();
        private readonly Stack<StringBuilder> m_exprSwitchDefaultCase = new Stack<StringBuilder>();
        private readonly Stack<StringBuilder> m_typeSwitchDefaultCase = new Stack<StringBuilder>();
        private int m_ifExpressionLevel;
        private int m_typeSwitchExpressionLevel;
        private bool m_fallThrough;

        public override void EnterLabeledStmt(GolangParser.LabeledStmtContext context)
        {
            // labeledStmt
            //     : IDENTIFIER ':' statement

            PushBlock();
            m_labels.Add(SanitizedIdentifier(context.IDENTIFIER().GetText()), false);

            // Check labeled continue in for loop
            // Check labeled break in for loop, select and switch
        }

        public override void ExitLabeledStmt(GolangParser.LabeledStmtContext context)
        {
            // labeledStmt
            //     : IDENTIFIER ':' statement

            string label = SanitizedIdentifier(context.IDENTIFIER().GetText());
            string statement = PopBlock(false);

            m_targetFile.Append($"{label}:{CheckForCommentsRight(context, preserveLineFeeds: true)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();

            m_targetFile.Append(statement);
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
                else if (expressions.Length > 0)
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

            bool breakHandled = false;

            if (context.IDENTIFIER() != null)
            {
                string label = SanitizedIdentifier(context.IDENTIFIER().GetText());

                if (m_labels.ContainsKey(label))
                {
                    breakHandled = true;

                    foreach (HashSet<string> blockBreaks in m_blockLabeledBreaks)
                        blockBreaks.Add(label);

                    m_targetFile.Append($"_break{label} = true;{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine("break;");
                }
            }

            if (!breakHandled)
            {
                m_targetFile.Append($"break;{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
        }

        public override void ExitContinueStmt(GolangParser.ContinueStmtContext context)
        {
            // continueStmt
            //     : 'continue' IDENTIFIER ?

            bool continueHandled = false;

            if (context.IDENTIFIER() != null)
            {
                string label = SanitizedIdentifier(context.IDENTIFIER().GetText());

                if (m_labels.ContainsKey(label))
                {
                    continueHandled = true;

                    foreach (HashSet<string> blockContinues in m_blockLabeledContinues)
                        blockContinues.Add(label);

                    m_targetFile.Append($"_continue{label} = true;{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine("break;");
                }
            }

            if (!continueHandled)
            {
                m_targetFile.Append($"continue;{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
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

            m_fallThrough = true;
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

        public override void EnterExprSwitchStmt(GolangParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Any declared variable will be scoped to switch statement, so create a sub-block for it
                m_targetFile.AppendLine($"{Spacing()}{{");
                IndentLevel++;
            }
        }

        public override void ExitExprSwitchStmt(GolangParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Close any locally scoped declared variable sub-block
                IndentLevel--;
                m_targetFile.AppendLine($"{Spacing()}}}");
            }
        }

        public override void EnterTypeSwitchStmt(GolangParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch'(simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Any declared variable will be scoped to switch statement, so create a sub-block for it
                m_targetFile.AppendLine($"{Spacing()}{{");
                IndentLevel++;
            }

            m_typeSwitchExpressionLevel++;

            if (context.typeSwitchGuard().IDENTIFIER() != null)
            {
                string identifier = SanitizedIdentifier(context.typeSwitchGuard().IDENTIFIER().GetText());

                m_targetFile.AppendLine($"{Spacing()}var {identifier} = {string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)};");
                m_targetFile.AppendLine();
                m_targetFile.AppendLine($"{Spacing()}Switch({identifier})");
            }
            else
            {
                m_targetFile.AppendLine($"{Spacing()}Switch({string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)})");
            }

            IndentLevel++;

            m_typeSwitchDefaultCase.Push(new StringBuilder());
        }

        public override void EnterTypeCaseClause(GolangParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            if (context.typeSwitchCase().typeList() == null)
                m_typeSwitchDefaultCase.Peek().AppendLine($"{Spacing()}.Default(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            else
                m_targetFile.AppendLine($"{Spacing()}.Case({string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel)})(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            
            IndentLevel++;

            PushBlock();
        }

        public override void ExitTypeCaseClause(GolangParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            IndentLevel--;

            if (context.typeSwitchCase().typeList() == null)
            {
                m_typeSwitchDefaultCase.Peek().AppendLine($"{PopBlock(false)}{Spacing()}}})");
            }
            else
            {
                PopBlock();
                m_targetFile.AppendLine($"{Spacing()}}}{(m_fallThrough ? ", fallthrough" : "")})");

                GolangParser.TypeListContext typeList = context.typeSwitchCase().typeList();
                List<string> types = new List<string>();

                for (int i = 0; i < typeList.type().Length; i++)
                {
                    if (Types.TryGetValue(typeList.type(i), out TypeInfo typeInfo))
                    {
                        string typeName = typeInfo.TypeName;

                        if (typeName.Equals("nil", StringComparison.Ordinal))
                            typeName = "NilType";

                        types.Add($"typeof({typeName})");
                    }
                    else
                    {
                        AddWarning(typeList, $"Failed to find type info for type switch case statement: {context.typeSwitchCase().GetText()}");
                    }
                }

                // Replace type switch expression marker
                m_targetFile.Replace(string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel), string.Join(", ", types));
            }

            // Reset fallthrough flag at the end of each case clause
            m_fallThrough = false;
        }

        public override void ExitTypeSwitchStmt(GolangParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch'(simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            IndentLevel--;

            // Default case always needs to be last case clause in SwitchExpression - Go allows its declaration anywhere
            m_targetFile.Append($"{m_typeSwitchDefaultCase.Pop()};");

            if (PrimaryExpressions.TryGetValue(context.typeSwitchGuard().primaryExpr(), out string expression))
            {
                // Replace type switch expression marker
                m_targetFile.Replace(string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel), expression);
            }
            else
            {
                AddWarning(context, $"Failed to find primary expression for type switch statement: {context.typeSwitchGuard().GetText()}");
            }

            m_typeSwitchExpressionLevel--;

            if (context.simpleStmt() != null && context.simpleStmt().shortVarDecl() != null)
            {
                // Close any locally scoped declared variable sub-block
                IndentLevel--;
                m_targetFile.AppendLine($"{Spacing()}}}");
            }
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

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}defer({expression});{CheckForCommentsRight(context, preserveLineFeeds: true)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for defer statement: {context.GetText()}");
            }
        }
    }
}