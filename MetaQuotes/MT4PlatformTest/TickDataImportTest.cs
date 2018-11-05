using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickDataImporter;
using Trady.Core.Period;
using Trady.Core;

namespace MT4PlatformTest
{
    [TestClass]
    public class TickDataImportTest
    {
        [TestMethod]
        public void TestMethod()
        {
            // Use case
            var importer = new TickDataFileImporter("TickData", "(?<symbol>[\\w]+)_Tick_(?<date>\\d{4}.\\d{2}.\\d{2})(?<ext>.dat)");
            var candle = importer.ImportAsync("XAUUSD", DateTime.Parse("2018-10-11 00:00:00").Date,
                DateTime.Parse("2018-10-11 23:59:59").Date,
                PeriodOption.PerMinute)
                .Result.First();
            Assert.AreEqual(candle.Open, 1194.58m);
        }

		//https://github.com/lppkarl/Trady#CaptureSignalByRules

        [TestMethod]
        public void TransferTest()
        {
            // Transform the series for computation, downcast is forbidden
            // Supported period: PerSecond, PerMinute, Per15Minutes, Per30Minutes, Hourly, BiHourly, Daily, Weekly, Monthly
            var importer = new TickDataFileImporter("TickData", "(?<symbol>[\\w]+)_Tick_(?<date>\\d{4}.\\d{2}.\\d{2})(?<ext>.dat)");
            var candles = importer.ImportAsync("XAUUSD", DateTime.Parse("2018-10-11 00:00:00").Date,
                DateTime.Parse("2018-10-11 23:59:59").Date,
                PeriodOption.PerMinute)
                .Result;

            var m5Candles = candles.Transform<PerMinute, Per5Minute>();
            var m10Candles = candles.Transform<PerMinute, Per10Minute>();
            var m15Candles = candles.Transform<PerMinute, Per15Minute>();
        }
    }
}
