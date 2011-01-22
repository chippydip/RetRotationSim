//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    public class Histogram
    {
        // _bins.Length == _counts.Length + 1
        // _counts[i] covers _bins[i] -> _bins[i+1]
        private readonly double[] _bins;
        private readonly int[] _counts;
        private int _maxCount;

        // on-line mean and variance calculations
        // see: http://en.wikipedia.org/wiki/Standard_deviation#Rapid_calculation_methods
        private int _i;
        private double _Ai;
        private double _Qi;

        public Histogram (double min, double max, int bins)
        {
            Contract.Requires(bins >= 1);
            Contract.Requires(min < max);

            _bins = new double[bins + 1];
            _counts = new int[bins];

            _bins[0] = min;
            _bins[bins] = max;

            double width = max - min;
            double frac = 1.0 / bins;
            for (int i = 1; i < bins; ++i)
                _bins[i] = min + (i * frac) * width;

            if (!CheckBins())
                throw new ArgumentException("min and max are too close to use that many bins");
        }

        public Histogram (double[] bins)
        {
            Contract.Requires(bins != null);

            _bins = new double[bins.Length];
            _counts = new int[_bins.Length - 1];
            
            bins.CopyTo(_bins, 0);

            if (!CheckBins())
                throw new ArgumentException("boundaries are not monotonically increasing", "bins");
        }

        // Make sure the bin boundaries are monatonically increasing
        private bool CheckBins ()
        {
            double last = _bins[0];
            int numBins = _counts.Length;
            for (int i = 1; i < numBins; ++i)
            {
                double value = _bins[i];
                if (value <= last)
                    return false;
                last = value;
            }
            return true;
        }

        public void Add (double value) 
        {
            // Update counts
            int bin = BinFor(value);
            ++_counts[bin];
            _maxCount = Math.Max(_maxCount, _counts[bin]);

            // Update Statistics: http://en.wikipedia.org/wiki/Standard_deviation#Rapid_calculation_methods
            ++_i;
            double nextAi = _Ai + (value - _Ai) / _i;
            _Qi += (value - _Ai) * (value - nextAi);
            _Ai = nextAi;
        }

        public void Clear ()
        {
            Array.Clear(_bins, 0, _bins.Length);
            _maxCount = 0;
            _i = 0;
            _Ai = 0;
            _Qi = 0;
        }

        public int BinCount { get { return _counts.Length; } }

        public double MinValue { get { return _bins[0]; } }

        public double MaxValue { get { return _bins[BinCount]; } }

        public int BinFor (double value)
        {
            Contract.Requires(value >= MinValue);
            Contract.Requires(value <= MaxValue);
            
            // TODO: guessing the bin number in a binary-search fasion might be faster?
            int length = _bins.Length;
            for (int i = 1; i < length; ++i)
                if (_bins[i] > value)
                    return i - 1;

            // Should only get here if value == MaxValue
            return length - 2;
        }

        public int this[int index] { get { return _counts[index]; } }

        public int LargestBinSize { get { return _maxCount; } }

        public int Count { get { return _i; } }

        public double Mean { get { return _Ai; } }

        // Use this if the histogram contains all data points
        public double PopulationVariance { get { return _Qi / _i; } }

        // Use this if the histogram contains only a sample of all data points
        // (makes sure that the variance is unbiased)
        public double SampleVariance { get { return _Qi / (_i - 1); } }

        public double PopulationStandardDeviation { get { return Math.Sqrt(PopulationVariance); } }

        public double SampleStandardDeviation { get { return Math.Sqrt(SampleVariance); } }
    }
}
