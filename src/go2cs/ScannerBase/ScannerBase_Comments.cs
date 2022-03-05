//******************************************************************************************************
//  ScannerBase_Comments.cs - Gbtc
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

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2cs;

public partial class ScannerBase
{
    public const int BaseSpacing = 4;

    protected int IndentLevel;
    protected bool WroteLineFeed;

    protected string Spacing(int offsetLevel = 0, int indentLevel = -1)
    {
        if (indentLevel < 0)
            indentLevel = IndentLevel;

        indentLevel += offsetLevel;

        if (indentLevel < 1)
            return string.Empty;

        return new(' ', BaseSpacing * indentLevel);
    }

    protected bool LineTerminatorAhead(ParserRuleContext context, int tokenOffset = 0)
    {
        if (context is null)
            return false;

        int tokenIndex = context.Stop.TokenIndex + tokenOffset;
        IList<IToken> hiddenChannel = TokenStream.GetHiddenTokensToRight(tokenIndex, TokenConstants.HiddenChannel);

        if (hiddenChannel?.Count > 0)
        {
            foreach (IToken token in hiddenChannel)
            {
                if (token.Text.IndexOf('\n') > -1)
                    return true;
            }
        }

        return false;
    }

    protected string CheckForCommentsLeft(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1)
    {
        if (context?.Start is null)
            return string.Empty;

        return CheckForComments(context.Start.TokenIndex, TokenStream.GetHiddenTokensToLeft, offsetLevel, indentLevel);
    }

    protected string CheckForCommentsRight(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1)
    {
        if (context?.Stop is null)
            return string.Empty;

        return CheckForComments(context.Stop.TokenIndex, TokenStream.GetHiddenTokensToRight, offsetLevel, indentLevel);
    }

    protected string CheckForCommentsLeft(IParseTree element, int offsetLevel = 0, int indentLevel = -1)
    {
        if (element is null)
            return string.Empty;

        return CheckForComments(element.SourceInterval.a, TokenStream.GetHiddenTokensToLeft, offsetLevel, indentLevel);
    }

    protected string CheckForCommentsRight(IParseTree element, int offsetLevel = 0, int indentLevel = -1)
    {
        if (element is null)
            return string.Empty;

        return CheckForComments(element.SourceInterval.b, TokenStream.GetHiddenTokensToRight, offsetLevel, indentLevel);
    }

    protected string CheckForEndOfLineComment(ParserRuleContext context)
    {
        if (context is null)
            return string.Empty;

        StringBuilder comments = new StringBuilder();
        IList<IToken> lineCommentChannel = TokenStream.GetHiddenTokensToRight(context.Stop.TokenIndex, TokenConstants.HiddenChannel);

        if (lineCommentChannel?.Count > 0)
        {
            IToken token = lineCommentChannel[0];
            string commentText = token.Text;

            if (commentText.Trim().StartsWith("//"))
            {
                if (!CommentOnNewLine(TokenStream.GetHiddenTokensToLeft(token.TokenIndex), token))
                {
                    string[] lines = commentText.Split(NewLineDelimeters, StringSplitOptions.RemoveEmptyEntries);

                    if (lines.Length > 0)
                        comments.Append(lines[0]);
                }
            }
        }

        return comments.ToString();
    }

    protected bool EndsWithLineFeed(string line)
    {
        if (string.IsNullOrEmpty(line))
            return false;

        while (line.Length > 0)
        {
            char lastChar = line[^1];

            if (char.IsWhiteSpace(lastChar) && lastChar != '\r' && lastChar != '\n')
                line = line.Substring(0, line.Length - 1);
            else
                break;
        }

        if (line.Length == 0)
            return false;

        int index = line.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);

        if (index == -1)
        {
            index = line.LastIndexOf('\n');

            if (index == -1)
                return false;

            if (index == line.Length - 1)
                return true;

            return false;
        }

        if (index == line.Length - 2)
            return true;

        return false;
    }

    protected string RemoveLastLineFeed(string line)
    {
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        int index = line.LastIndexOf("\r\n", StringComparison.Ordinal);

        if (index > -1 && index == line.Length - 2)
            return line.Substring(0, index);

        index = line.LastIndexOf('\n');

        if (index > -1 && index == line.Length - 1)
            line = line.Substring(0, index);

        return line;
    }

    protected string RemoveFirstLineFeed(string line)
    {
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        int index = line.IndexOf("\r\n", StringComparison.Ordinal);

        if (index == 0)
            return line.Substring(2);

        index = line.IndexOf('\n');

        if (index == 0)
            line = line.Substring(1);

        return line;
    }

    private string CheckForComments(int tokenIndex, Func<int, IList<IToken>> getHiddenTokens, int offsetLevel, int indentLevel)
    {
        IList<IToken> hiddenTokens = getHiddenTokens(tokenIndex) ?? new List<IToken>();
        StringBuilder comments = new StringBuilder();
        int firstComment = -1;
        string hiddenText, hiddenTrimmed;

        WroteLineFeed = false;

        for (int i = 0; i < hiddenTokens.Count; i++)
        {
            hiddenText = hiddenTokens[i].Text.Replace("\t", Spacing(indentLevel: 1));
            hiddenTrimmed = hiddenText.TrimStart();

            if (hiddenTrimmed.StartsWith("/*") || hiddenTrimmed.StartsWith("//"))
            {
                if (firstComment == -1)
                    firstComment = i;

                comments.Append(hiddenText);
            }
            else
            {
                hiddenText = new(Array.FindAll(hiddenText.ToCharArray(), c => c == '\r' || c == '\n'));

                if (hiddenText.Length > 0)
                    comments.Append(hiddenText);
            }
        }

        bool firstIsEOLComment = false;

        hiddenText = comments.ToString();

        // If there is no first comment, all hidden tokens are whitespace, so no need to fix forward spacing
        if (firstComment > -1)
        {
            hiddenTrimmed = hiddenText.TrimStart();

            if (hiddenTrimmed.StartsWith("//") || hiddenTrimmed.StartsWith("/*") && hiddenTrimmed.Count(c => c == '\n') < 2)
                firstIsEOLComment = firstComment == 0 || !hiddenTokens[firstComment - 1].Text.EndsWith("\n");

            hiddenText = FixForwardSpacing(hiddenText, offsetLevel, indentLevel, firstIsEOLComment);
        }

        if (hiddenText.Length > 0)
            WroteLineFeed = EndsWithLineFeed(hiddenText);

        return hiddenText;
    }

    protected (string, string) SplitEOLComment(string source)
    {
        string[] lines = source.Split(NewLineDelimeters, StringSplitOptions.None);

        if (lines.Length > 1)
            return (lines[0], string.Join(Environment.NewLine, lines.Skip(1)));

        if (lines.Length > 0)
            return (lines[0], string.Empty);
            
        return (string.Empty, string.Empty);
    }

    protected string FixForwardSpacing(string source, int offsetLevel = 0, int indentLevel = -1, bool firstIsEOLComment = false)
    {
        string forwardSpacing = Spacing(offsetLevel, indentLevel);
        string[] lines = source.Split(NewLineDelimeters, StringSplitOptions.None);
        List<string> fixedLines = new List<string>();
        string[] workLines;

        if (firstIsEOLComment)
        {
            fixedLines.Add($" {lines[0].TrimStart()}");
            workLines = lines.Skip(1).ToArray();
        }
        else
        {
            workLines = lines;
        }

        int commonIndex = -1;
        bool hasCommonWhiteSpace = true;

        while (hasCommonWhiteSpace)
        {
            char lastChar = char.MinValue;
            bool tested = false;
            commonIndex++;

            foreach (string line in workLines)
            {
                if (line.Length == 0 || commonIndex >= line.Length)
                    continue;

                tested = true;

                if (lastChar == char.MinValue)
                {
                    lastChar = line[commonIndex];

                    if (!char.IsWhiteSpace(lastChar))
                    {
                        hasCommonWhiteSpace = false;
                        break;
                    }
                }
                else if (line[commonIndex] != lastChar)
                {
                    hasCommonWhiteSpace = false;
                    break;
                }
            }

            if (!tested)
                break;
        }

        if (commonIndex > 0)
        {
            for (int i = 0; i < workLines.Length; i++)
            {
                if (workLines[i].Length > commonIndex)
                    workLines[i] = workLines[i].Substring(commonIndex);
            }
        }

        fixedLines.AddRange(workLines.Select(line => line.Trim().Length > 0 ? $"{forwardSpacing}{line}" : string.Empty));

        return string.Join(Environment.NewLine, fixedLines);
    }

    private bool CommentOnNewLine(IList<IToken> hiddenChannel, IToken testToken)
    {
        IToken priorToken = hiddenChannel?.FirstOrDefault(token => token.StopIndex == testToken.StartIndex - 1);
        return priorToken is not null && EndsWithLineFeed(priorToken.Text);
    }

    //protected bool IsEndOfLineCommentLeft(ParserRuleContext context)
    //{
    //    return IsEndOfLineComment(context.Start.TokenIndex, TokenStream.GetHiddenTokensToLeft);
    //}

    //protected bool IsEndOfLineCommentRight(ParserRuleContext context)
    //{
    //    return IsEndOfLineComment(context.Stop.TokenIndex, TokenStream.GetHiddenTokensToRight);
    //}

    //private bool IsEndOfLineComment(int tokenIndex, Func<int, int, IList<IToken>> getHiddenTokens)
    //{
    //    IList<IToken> lineCommentChannel = getHiddenTokens(tokenIndex, GoLexer.LineCommentChannel);

    //    if (lineCommentChannel?.Count > 0)
    //    {
    //        IToken token = lineCommentChannel[0];
    //        string commentText = token.Text;

    //        if (commentText.Trim().StartsWith("//"))
    //            return !CommentOnNewLine(TokenStream.GetHiddenTokensToLeft(token.TokenIndex), token);
    //    }

    //    return false;
    //}
}
