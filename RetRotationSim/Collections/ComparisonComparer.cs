//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Chippydip.Collections
{
    // From: http://stackoverflow.com/questions/504645/binary-search-of-a-c-list-using-delegate-condition
    public sealed class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer (Comparison<T> comparison)
        {
            Contract.Requires(comparison != null);

            _comparison = comparison;
        }

        public int Compare (T x, T y)
        {
            return _comparison(x, y);
        }
    }
}
