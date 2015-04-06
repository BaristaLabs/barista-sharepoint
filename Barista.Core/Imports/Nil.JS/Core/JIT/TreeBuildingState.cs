﻿#if !NET35

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Barista.NiL.JS.Core.JIT
{
    internal sealed class TreeBuildingState
    {
        public readonly Stack<LabelTarget> BreakLabels;
        public readonly Stack<LabelTarget> ContinueLabels;
        public Dictionary<string, LabelTarget> NamedBreakLabels;
        public Dictionary<string, LabelTarget> NamedContinueLabels;

        public TreeBuildingState()
        {
            BreakLabels = new Stack<LabelTarget>();
            ContinueLabels = new Stack<LabelTarget>();
            NamedBreakLabels = new Dictionary<string, LabelTarget>();
            NamedContinueLabels = new Dictionary<string, LabelTarget>();
        }
    }
}

#endif