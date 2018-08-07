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
        public const string IfElseBreakMarker = ">>MARKER:IFELSEBREAK_LEVEL_{0}<<";
        public const string IfExpressionMarker = ">>MARKER:IFEXPR_LEVEL_{0}<<";
        public const string IfStatementMarker = ">>MARKER:IFSTATEMENT_LEVEL_{0}<<";
        public const string ExprSwitchExpressionMarker = ">>MARKER:EXPRSWITCH_LEVEL_{0}<<";
        public const string ExprSwitchCaseTypeMarker = ">>MARKER:EXPRSWITCHCASE_LEVEL_{0}<<";
        public const string ExprSwitchStatementMarker = ">>MARKER:EXPRSWITCHSTATEMENT_LEVEL_{0}<<";
        public const string TypeSwitchExpressionMarker = ">>MARKER:TYPESWITCH_LEVEL_{0}<<";
        public const string TypeSwitchCaseTypeMarker = ">>MARKER:TYPESWITCHCASE_LEVEL_{0}<<";
        public const string TypeSwitchStatementMarker = ">>MARKER:TYPESWITCHSTATEMENT_LEVEL_{0}<<";
        public const string ForExpressionMarker = ">>MARKER:FOREXPRESSION_LEVEL_{0}<<";
        public const string ForInitStatementMarker = ">>MARKER:FORINITSTATEMENT_LEVEL_{0}<<";
        public const string ForPostStatementMarker = ">>MARKER:FORPOSTSTATEMENT_LEVEL_{0}<<";
        public const string ForRangeExpressionsMarker = ">>MARKER:FORRANGEEXPRESSIONS_LEVEL_{0}<<";

        private readonly ParseTreeValues<string> m_simpleStatements = new ParseTreeValues<string>();
        private readonly Dictionary<string, bool> m_labels = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Stack<HashSet<string>> m_blockLabeledContinues = new Stack<HashSet<string>>();
        private readonly Stack<HashSet<string>> m_blockLabeledBreaks = new Stack<HashSet<string>>();
        private readonly Stack<StringBuilder> m_exprSwitchDefaultCase = new Stack<StringBuilder>();
        private readonly Stack<StringBuilder> m_typeSwitchDefaultCase = new Stack<StringBuilder>();
        private int m_ifExpressionLevel;
        private int m_exprSwitchExpressionLevel;
        private int m_typeSwitchExpressionLevel;
        private int m_forExpressionLevel;
        private bool m_fallThrough;

        public override void ExitStatement(GolangParser.StatementContext context)
        {
            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Append($"{statement}{(LineTerminatorAhead(context.simpleStmt()) ? "" : Environment.NewLine)}");
                else
                    AddWarning(context, $"Failed to find simple statement for: {context.simpleStmt().GetText()}");
            }
        }

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

            m_targetFile.Append($"{label}:{CheckForBodyCommentsRight(context)}");

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
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{channel}.Send({value});{CheckForBodyCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
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
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{expression};{CheckForBodyCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for expression statement: {context.GetText()}");
            }
        }

        public override void ExitIncDecStmt(GolangParser.IncDecStmtContext context)
        {
            // incDecStmt
            //     : expression('++' | '--')

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{expression}{context.children[1].GetText()};{CheckForBodyCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
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
                StringBuilder statement = new StringBuilder();

                if (leftOperands.Length != rightOperands.Length)
                {
                    if (leftOperands.Length > rightOperands.Length && rightOperands.Length == 1)
                        leftOperands = new[] { $"({string.Join(", ", leftOperands)})" };
                    else
                        AddWarning(context, $"Encountered count mismatch for left and right operand expressions in assignment statement: {context.GetText()}");
                }

                int length = Math.Min(leftOperands.Length, rightOperands.Length);
               
                for (int i = 0; i < length; i++)
                {
                    string assignOP = context.assign_op().GetText();

                    if (assignOP.Equals("<<=") || assignOP.Equals(">>="))
                    {
                        // TODO: Need expression evaluation - cast not needed for int expressions
                        if (!int.TryParse(rightOperands[i], out int _))
                            rightOperands[i] = $"(int)({rightOperands[i]})";
                    }

                    if (assignOP.Equals("&^="))
                    {
                        assignOP = " &= ";
                        rightOperands[i] = $"~({rightOperands[i]})";
                    }
                    else
                    {
                        assignOP = $" {assignOP} ";
                    }

                    statement.Append($"{Spacing()}{leftOperands[i]}{assignOP}{rightOperands[i]};");

                    // Since multiple assignments can be on one line, only check for comments after last assignment
                    if (i < length - 1)
                    {
                        statement.AppendLine();
                    }
                    else
                    {
                        statement.Append(CheckForBodyCommentsRight(context));

                        if (!WroteLineFeed)
                            statement.AppendLine();
                    }

                    m_simpleStatements[context.Parent] = statement.ToString();
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
                StringBuilder statement = new StringBuilder();

                if (identifiers.Length != expressions.Length)
                {
                    if (identifiers.Length > expressions.Length && expressions.Length == 1)
                        identifiers = new[] { $"({string.Join(", ", identifiers)})" };
                    else
                        AddWarning(context, $"Encountered count mismatch for identifiers and expressions in short var declaration statement: {context.GetText()}");
                }

                int length = Math.Min(identifiers.Length, expressions.Length);

                for (int i = 0; i < length; i++)
                {
                    statement.Append($"{Spacing()}var {identifiers[i]} = {expressions[i]};");

                    // Since multiple declarations can be on one line, only check for comments after last declaration
                    if (i < length - 1)
                    {
                        statement.AppendLine();
                    }
                    else
                    {
                        statement.Append(CheckForBodyCommentsRight(context));

                        if (!WroteLineFeed)
                            statement.AppendLine();
                    }
                }

                m_simpleStatements[context.Parent] = statement.ToString();
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
                m_targetFile.Append($"{Spacing()}ThreadPool.QueueUserWorkItem(state => {expression});{CheckForBodyCommentsRight(context)}");

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

            m_targetFile.Append($";{CheckForBodyCommentsRight(context)}");

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

                    m_targetFile.Append($"_break{label} = true;{CheckForBodyCommentsRight(context)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine("break;");
                }
            }

            if (!breakHandled)
            {
                m_targetFile.Append($"break;{CheckForBodyCommentsRight(context)}");

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

                    m_targetFile.Append($"_continue{label} = true;{CheckForBodyCommentsRight(context)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine("break;");
                }
            }

            if (!continueHandled)
            {
                m_targetFile.Append($"continue;{CheckForBodyCommentsRight(context)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
        }

        public override void ExitGotoStmt(GolangParser.GotoStmtContext context)
        {
            // gotoStmt
            //     : 'goto' IDENTIFIER

            m_targetFile.Append($"{Spacing()}goto {SanitizedIdentifier(context.IDENTIFIER().GetText())};{CheckForBodyCommentsRight(context)}");

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

            m_ifExpressionLevel++;

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                // Any declared variable will be scoped to if statement, so create a sub-block for it
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(IfStatementMarker, m_ifExpressionLevel));
            }

            m_targetFile.AppendLine($"{string.Format(IfElseBreakMarker, m_ifExpressionLevel)}{Spacing()}{string.Format(IfElseMarker, m_ifExpressionLevel)}if ({string.Format(IfExpressionMarker, m_ifExpressionLevel)})");

            if (context.block().Length == 2)
            {
                PushOuterBlockSuffix(null);  // For current block
                PushOuterBlockSuffix($"{Environment.NewLine}{Spacing()}else{(LineTerminatorAhead(context.block(0)) ? "" : Environment.NewLine)}");
            }
            else
            {
                PushOuterBlockSuffix(null);  // For current block
            }
        }

        public override void ExitIfStmt(GolangParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if '(simpleStmt ';') ? expression block ( 'else' ( ifStmt | block ) ) ?

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                bool isElseIf = context.Parent is GolangParser.IfStmtContext;

                // Replace if markers
                m_targetFile.Replace(string.Format(IfExpressionMarker, m_ifExpressionLevel), expression);
                m_targetFile.Replace(string.Format(IfElseBreakMarker, m_ifExpressionLevel), isElseIf ? Environment.NewLine : "");
                m_targetFile.Replace(string.Format(IfElseMarker, m_ifExpressionLevel), isElseIf ? "else " : "");
            }
            else
            {
                AddWarning(context, $"Failed to find expression for if statement: {context.GetText()}");
            }

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(IfStatementMarker, m_ifExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for if statement: {context.simpleStmt().GetText()}");
                
                // Close any locally scoped declared variable sub-block
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    IndentLevel--;
                    m_targetFile.AppendLine($"{Spacing()}}}");
                }
            }

            m_ifExpressionLevel--;
        }

        public override void EnterExprSwitchStmt(GolangParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            m_exprSwitchExpressionLevel++;

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel));
            }

            if (context.expression() != null)
                m_targetFile.Append($"{Spacing()}Switch({string.Format(ExprSwitchExpressionMarker, m_exprSwitchExpressionLevel)})");
            else
                m_targetFile.Append($"{Spacing()}Switch()");

            m_exprSwitchDefaultCase.Push(new StringBuilder());
        }

        public override void EnterExprCaseClause(GolangParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            if (context.exprSwitchCase().expressionList() == null)
                m_exprSwitchDefaultCase.Peek().Append($"{Environment.NewLine}{Spacing()}.Default(() =>{Environment.NewLine}{Spacing()}{{{CheckForBodyCommentsLeft(context.statementList(), 1)}");
            else
                m_targetFile.Append($"{Environment.NewLine}{Spacing()}.Case({string.Format(ExprSwitchCaseTypeMarker, m_exprSwitchExpressionLevel)})(() =>{Environment.NewLine}{Spacing()}{{{CheckForBodyCommentsLeft(context.statementList(), 1)}");

            IndentLevel++;

            PushBlock();
        }

        public override void ExitExprCaseClause(GolangParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            IndentLevel--;

            GolangParser.ExpressionListContext expressionList = context.exprSwitchCase().expressionList();

            if (expressionList == null)
            {
                m_exprSwitchDefaultCase.Peek().Append($"{PopBlock(false)}{Spacing()}}})");
            }
            else
            {
                PopBlock();
                m_targetFile.Append($"{Spacing()}}}{(m_fallThrough ? ", fallthrough" : "")})");

                if (!ExpressionLists.TryGetValue(expressionList, out string[] expressions))
                    AddWarning(expressionList, $"Failed to find expression list for switch case statement: {context.exprSwitchCase().GetText()}");

                // Replace switch expression case type marker
                m_targetFile.Replace(string.Format(ExprSwitchCaseTypeMarker, m_exprSwitchExpressionLevel), string.Join(", ", expressions ?? new string[0]));
            }

            // Reset fallthrough flag at the end of each case clause
            m_fallThrough = false;
        }

        public override void ExitExprSwitchStmt(GolangParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // Default case always needs to be last case clause in SwitchExpression - Go allows its declaration anywhere
            m_targetFile.Append($"{m_exprSwitchDefaultCase.Pop()};");

            if (context.expression() != null)
            {
                if (Expressions.TryGetValue(context.expression(), out string expression))
                {
                    // Replace switch expression marker
                    m_targetFile.Replace(string.Format(ExprSwitchExpressionMarker, m_exprSwitchExpressionLevel), expression);
                }
                else
                {
                    AddWarning(context, $"Failed to find expression for switch statement: {context.expression().GetText()}");
                }
            }

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for expression based switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    IndentLevel--;
                    m_targetFile.AppendLine($"{Spacing()}}}");
                }
            }

            m_exprSwitchExpressionLevel--;

            m_targetFile.Append(CheckForBodyCommentsRight(context));
        }

        public override void EnterTypeSwitchStmt(GolangParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch' (simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            m_typeSwitchExpressionLevel++;

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel));
            }

            if (context.typeSwitchGuard().IDENTIFIER() != null)
            {
                string identifier = SanitizedIdentifier(context.typeSwitchGuard().IDENTIFIER().GetText());

                m_targetFile.AppendLine($"{Spacing()}var {identifier} = {string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)};");
                m_targetFile.AppendLine();
                m_targetFile.Append($"{Spacing()}Switch({identifier})");
            }
            else
            {
                m_targetFile.Append($"{Spacing()}Switch({string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)})");
            }

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
                m_typeSwitchDefaultCase.Peek().Append($"{Environment.NewLine}{Spacing()}.Default(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            else
                m_targetFile.Append($"{Environment.NewLine}{Spacing()}.Case({string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel)})(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            
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
                m_typeSwitchDefaultCase.Peek().Append($"{PopBlock(false)}{Spacing()}}})");
            }
            else
            {
                PopBlock();
                m_targetFile.Append($"{Spacing()}}}{(m_fallThrough ? ", fallthrough" : "")})");

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

            if (context.simpleStmt() != null && context.simpleStmt().emptyStmt() == null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for type switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (context.simpleStmt().shortVarDecl() != null)
                {
                    IndentLevel--;
                    m_targetFile.AppendLine($"{Spacing()}}}");
                }
            }

            m_typeSwitchExpressionLevel--;

            m_targetFile.Append(CheckForBodyCommentsRight(context));
        }

        public override void ExitSelectStmt(GolangParser.SelectStmtContext context)
        {
            // selectStmt
            //     : 'select' '{' commClause * '}'
        }

        public override void EnterForStmt(GolangParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause) ? block

            m_forExpressionLevel++;

            GolangParser.ForClauseContext forClause = context.forClause();

            if (context.expression() != null || forClause != null && forClause.simpleStmt()?.Length == 0)
            {
                // Handle while style statement
                m_targetFile.AppendLine($"{Spacing()}while({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
            }
            else if (forClause != null)
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                if (HasInitialStatement(forClause, out GolangParser.SimpleStmtContext simpleStatement))
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    if (simpleStatement.shortVarDecl() != null)
                    {
                        m_targetFile.AppendLine($"{Spacing()}{{");
                        IndentLevel++;
                    }

                    m_targetFile.Append(string.Format(ForInitStatementMarker, m_forExpressionLevel));
                }

                if (HasPostStatement(forClause, out _))
                    PushInnerBlockSuffix($"{Spacing(indentLevel: 1)}{string.Format(ForPostStatementMarker, m_forExpressionLevel)}");

                // Handle for loop style statement - since Go's initial and post statements are more
                // free-form, a while loop is used instead

                m_targetFile.AppendLine($"{Spacing()}while({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
            }
            else
            {
                // rangeClause
                //     : (expressionList '=' | identifierList ':=' )? 'range' expression

                if (context.rangeClause().identifierList() != null)
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                // Handle item iteration style statement - since Go's iteration variables are mutable and
                // can be pre-declared, a standard for loop is used instead of a foreach. The exception
                // is for an index and rune iteration over a string which in C# a foreach works fine because
                // the inferred tuple can be readonly since it would not be referenced by Go code.
            }
        }

        public override void ExitForStmt(GolangParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause) ? block

            GolangParser.ForClauseContext forClause = context.forClause();

            if (context.expression() != null || forClause != null && forClause.simpleStmt()?.Length == 0)
            {
                Expressions.TryGetValue(context.expression() ?? forClause.expression(), out string expression);
                m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression ?? "true");
                m_targetFile.Append(CheckForBodyCommentsRight(context));
            }
            else if (forClause != null)
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                if (Expressions.TryGetValue(forClause.expression(), out string expression))
                    m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression);
                else
                    AddWarning(context, $"Failed to find expression in for statement: {context.GetText()}");

                m_targetFile.Append(CheckForBodyCommentsRight(context));

                if (HasInitialStatement(forClause, out GolangParser.SimpleStmtContext simpleStatement))
                {
                    // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                    if (m_simpleStatements.TryGetValue(simpleStatement, out string statement))
                        m_targetFile.Replace(string.Format(ForInitStatementMarker, m_forExpressionLevel), statement + Environment.NewLine);
                    else
                        AddWarning(context, $"Failed to find simple initial statement in for clause: {simpleStatement.GetText()}");

                    // Close any locally scoped declared variable sub-block
                    if (simpleStatement.shortVarDecl() != null)
                    {
                        IndentLevel--;
                        m_targetFile.AppendLine($"{Spacing()}}}");
                    }
                }

                if (HasPostStatement(forClause, out simpleStatement))
                {
                    if (m_simpleStatements.TryGetValue(simpleStatement, out string statement))
                        m_targetFile.Replace(string.Format(ForPostStatementMarker, m_forExpressionLevel), statement);
                    else
                        AddWarning(context, $"Failed to find simple post statement in for clause: {simpleStatement.GetText()}");
                }
            }
            else
            {
                // rangeClause
                //     : (expressionList '=' | identifierList ':=' )? 'range' expression

                m_targetFile.Append(CheckForBodyCommentsRight(context));

                if (context.rangeClause().identifierList() != null)
                {
                    // Close any locally scoped declared variable sub-block
                    IndentLevel--;
                    m_targetFile.AppendLine($"{Spacing()}}}");
                }
            }

            m_forExpressionLevel--;
        }

        private bool HasInitialStatement(GolangParser.ForClauseContext forClause, out GolangParser.SimpleStmtContext simpleStatement)
        {
            simpleStatement = null;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            return
                simpleStatement != null &&
                simpleStatement.emptyStmt() == null &&
                forClause.children[forClause.children.Count - 1] != simpleStatement;
        }

        private bool HasPostStatement(GolangParser.ForClauseContext forClause, out GolangParser.SimpleStmtContext simpleStatement)
        {
            simpleStatement = null;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            if (forClause.simpleStmt().Length == 1)
                return
                    simpleStatement != null &&
                    simpleStatement.emptyStmt() == null &&
                    forClause.children[forClause.children.Count - 1] == simpleStatement;

            simpleStatement = forClause.simpleStmt(1);

            return
                simpleStatement != null &&
                simpleStatement.emptyStmt() == null &&
                forClause.children[forClause.children.Count - 1] == simpleStatement;
        }

        public override void ExitDeferStmt(GolangParser.DeferStmtContext context)
        {
            // deferStmt
            //     : 'defer' expression

            if (Expressions.TryGetValue(context.expression(), out string expression))
            {
                m_targetFile.Append($"{Spacing()}defer({expression});{CheckForBodyCommentsRight(context)}");

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