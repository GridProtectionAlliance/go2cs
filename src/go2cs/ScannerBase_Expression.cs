//******************************************************************************************************
//  ScannerBase_Expression.cs - Gbtc
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

using Antlr4.Runtime.Misc;
using go2cs.Metadata;
using System.Collections.Generic;
using static go2cs.Common;

namespace go2cs
{
    public partial class ScannerBase
    {
        // Stack handlers:
        //  expressionList (required)
        //  expressionStmt (required)
        //  sendStmt (required)
        //  incDecStmt (required)
        //  defer (required)
        //  ifStmt (required)
        //  exprSwitchStmt (optional)
        //  recvStmt (required)
        //  forClause (optional)
        //  rangeClause (required)
        //  goStmt (required)
        //  arrayLength (required)
        //  operand (optional)
        //  key (optional)
        //  element (optional)
        //  index (required)
        //  slice (optional)
        //  expression (optional)
        //  conversion (required)
        protected readonly ParseTreeValues<string> Expressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> UnaryExpressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> PrimaryExpressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> Operands = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> Arguments = new ParseTreeValues<string>();

        // TODO: Focus on getting primaryExpr working, and build out from there
        public override void ExitPrimaryExpr(GolangParser.PrimaryExprContext context)
        {
            PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression);

            if (Operands.TryGetValue(context.operand(), out string operand))
            {
                PrimaryExpressions[context] = operand;
            }
            else if (Arguments.TryGetValue(context.arguments(), out string arguments))
            {
                PrimaryExpressions[context] = $"{primaryExpression}{arguments}";
            }
            else
            {
                // TODO: This is an expression fall back until all sub-tree paths are populated as needed
                PrimaryExpressions[context] = context.GetText();
            }
        }

        public override void ExitExpression(GolangParser.ExpressionContext context)
        {
            if (context.expression()?.Length == 2)
            {                
                string binaryOP = context.children[1].GetText();
                binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";
                Expressions[context] = $"{Expressions[context.expression(0)]}{binaryOP}{Expressions[context.expression(1)]}";
            }
            else
            {
                Expressions[context] = UnaryExpressions[context.unaryExpr()];
            }
        }

        public override void ExitUnaryExpr(GolangParser.UnaryExprContext context)
        {
            if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression))
            {
                UnaryExpressions[context] = primaryExpression;
            }
            else if (context.unaryExpr() != null)
            {
                string unaryOP = context.children[0].GetText();

                if (unaryOP.Equals("^"))
                {
                    unaryOP = "~";
                }
                else if (unaryOP.Equals("<-"))
                {
                    // TODO: Handle channel value access (update when channel class is created):
                    unaryOP = null;
                    UnaryExpressions[context] = $"{UnaryExpressions[context.unaryExpr()]}.Receive()";
                }
                else if (unaryOP.Equals("&"))
                {
                    // TODO: Handle pointer acquisition context - may need to augment pre-scan metadata for heap allocation notation
                }
                else if (unaryOP.Equals("*"))
                {
                    // TODO: Handle pointer dereference context
                }

                if ((object)unaryOP != null)
                    UnaryExpressions[context] = $"{unaryOP}{UnaryExpressions[context.unaryExpr()]}";
            }
            else if (!UnaryExpressions.ContainsKey(context))
            {
                // TODO: This is an expression fall back until all sub-tree paths are populated as needed
                UnaryExpressions[context] = context.GetText();
            }
        }

        public override void ExitArguments(GolangParser.ArgumentsContext context)
        {
            List<string> arguments = new List<string>();

            if (Types.TryGetValue(context.type(), out TypeInfo typeInfo))
                arguments.Add($"typeof({typeInfo.PrimitiveName})");

            if (ExpressionLists.TryGetValue(context.expressionList(), out string[] expressions))
                arguments.AddRange(expressions);

            Arguments[context] = $"({string.Join(", ", arguments)})";
        }

        public override void ExitOperand( GolangParser.OperandContext context)
        {
            if (Expressions.TryGetValue(context.expression(), out string expression))
                Operands[context] = $"({expression})"; ;
        }

        public override void ExitBasicLit(GolangParser.BasicLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from basic literal: \"{context.GetText()}\"");
                return;
            }

            string basicLiteral;

            //basicLit
            //    : INT_LIT
            //    | FLOAT_LIT
            //    | IMAGINARY_LIT
            //    | RUNE_LIT
            //    | STRING_LIT
            //    ;

            if (context.IMAGINARY_LIT() != null)
            {
                string value = context.IMAGINARY_LIT().GetText();
                basicLiteral = value.EndsWith("i") ? $"i({value.Substring(0, value.Length - 1)})" : value;
            }
            else if (context.RUNE_LIT() != null)
            {
                basicLiteral = ReplaceOctalBytes(context.RUNE_LIT().GetText());
            }
            else if (context.STRING_LIT() != null)
            {
                basicLiteral = ToStringLiteral(ReplaceOctalBytes(context.STRING_LIT().GetText()));
            }
            else
            {
                basicLiteral = context.GetText();
            }

            Operands[operandContext] = basicLiteral;
        }

        public override void ExitCompositeLit(GolangParser.CompositeLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from composite literal: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update to handle in-line type constructions
            Operands[operandContext] = context.GetText();
        }

        public override void ExitFunctionLit(GolangParser.FunctionLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from function literal: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update to handle in-line lambda declarations
            Operands[operandContext] = context.GetText();
        }

        public override void ExitOperandName(GolangParser.OperandNameContext context)
        {
            if (!(context.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from operand name: \"{context.GetText()}\"");
                return;
            }

            Operands[operandContext] = context.GetText();
        }

        public override void ExitMethodExpr([NotNull] GolangParser.MethodExprContext context)
        {
            if (!(context.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from method expression: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update type name pointer dereferences, e.g., (*typeName)
            Operands[operandContext] = context.GetText();
        }
    }
}