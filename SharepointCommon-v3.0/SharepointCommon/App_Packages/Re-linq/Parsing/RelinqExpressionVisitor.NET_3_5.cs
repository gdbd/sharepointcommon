﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;


#if !NET_4_5
namespace Remotion.Linq.Parsing
{
  public abstract partial class RelinqExpressionVisitor
  {
    protected override sealed Expression VisitUnknownNonExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      switch (expression.NodeType)
      {
        case SubQueryExpression.ExpressionType:
          return VisitSubQuery ((SubQueryExpression) expression);
        case QuerySourceReferenceExpression.ExpressionType:
          return VisitQuerySourceReference ((QuerySourceReferenceExpression) expression);
        default:
          return VisitRelinqUnknownNonExtension (expression);
      }
    }

    protected virtual Expression VisitRelinqUnknownNonExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return base.VisitUnknownNonExtension (expression);
    }
  }
}
#endif