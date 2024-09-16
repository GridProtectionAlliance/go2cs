﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommandLine.CSharpx;

using CommandLine.ErrorHandling;

namespace CommandLine.Infrastructure
{
    static class ResultExtensions
    {
        public static IEnumerable<TMessage> SuccessfulMessages<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
        {
            if (result.Tag == ResultType.Ok)
            {
                var ok = (Ok<TSuccess, TMessage>)result;
                return ok.Messages;
            }
            return [];
        }

        public static Maybe<TSuccess> ToMaybe<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
        {
            if (result.Tag == ResultType.Ok)
            {
                var ok = (Ok<TSuccess, TMessage>)result;
                return Maybe.Just(ok.Success);
            }
            return Maybe.Nothing<TSuccess>();
        }
    }
}