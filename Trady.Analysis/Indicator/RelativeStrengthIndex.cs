﻿using System;
using Trady.Core;

namespace Trady.Analysis.Indicator
{
    public partial class RelativeStrengthIndex : IndicatorBase
    {
        private const string RsiTag = "Rsi";

        private RelativeStrength _rsIndicator;

        public RelativeStrengthIndex(Equity equity, int periodCount) : base(equity, periodCount)
        {
            _rsIndicator = new RelativeStrength(equity, periodCount);
        }

        public int PeriodCount => Parameters[0];

        protected override IAnalyticResult<decimal> ComputeResultByIndex(int index)
        {
            var rsi = 100 - (100 / (1 + _rsIndicator.ComputeByIndex(index).Rs));
            return new IndicatorResult(Equity[index].DateTime, rsi);
        }

        public IndicatorResultTimeSeries<IndicatorResult> Compute(DateTime? startTime = null, DateTime? endTime = null)
            => new IndicatorResultTimeSeries<IndicatorResult>(Equity.Name, ComputeResults<IndicatorResult>(startTime, endTime), Equity.Period, Equity.MaxTickCount);

        public IndicatorResult ComputeByDateTime(DateTime dateTime)
            => ComputeResultByDateTime<IndicatorResult>(dateTime);

        public IndicatorResult ComputeByIndex(int index)
            => ComputeResultByIndex<IndicatorResult>(index);
    }
}