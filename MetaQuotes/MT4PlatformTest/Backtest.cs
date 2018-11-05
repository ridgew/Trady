using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickDataImporter;
using Trady.Analysis;
using Trady.Analysis.Backtest;
using Trady.Core.Period;
using Trady.Analysis.Extension;
using System.Linq;

namespace MT4PlatformTest
{
    [TestClass]
    public class Backtest
    {
        public async Task BackTestingTaskAsync()
        {
            // Import your candles
            var importer = new TickDataFileImporter("TickData", "(?<symbol>[\\w]+)_Tick_(?<date>\\d{4}.\\d{2}.\\d{2})(?<ext>.dat)");
            var fb = await importer.ImportAsync("XAUUSD", DateTime.Parse("2018-10-11 00:00:00").Date,
                DateTime.Parse("2018-10-11 23:59:59").Date,
                PeriodOption.PerMinute);


            // Build buy rule & sell rule based on various patterns
            var buyRule = Rule.Create(c => c.IsFullStoBullishCross(14, 3, 3))
                .And(c => c.IsMacdOscBullish(12, 26, 9))
                .And(c => c.IsSmaOscBullish(10, 30))
                .And(c => c.IsAccumDistBullish());

            var sellRule = Rule.Create(c => c.IsFullStoBearishCross(14, 3, 3))
                .Or(c => c.IsMacdBearishCross(12, 24, 9))
                .Or(c => c.IsSmaBearishCross(10, 30));

            // Create portfolio instance by using PortfolioBuilder
            var runner = new Builder()
                .Add(fb, 10)
                .Buy(buyRule)
                .Sell(sellRule)
                .BuyWithAllAvailableCash()
                .FlatExchangeFeeRate(0.001m)
                .Premium(1)
                .Build();

            // Start backtesting with the portfolio
            var result = await runner.RunAsync(10000);

            // Get backtest result for the portfolio
            //Console.WriteLine(string.Format("Transaction count: {0:#.##}, P/L ratio: {1:0.##}%, Principal: {2:#}, Total: {3:#}",
            //    result.Transactions.Count(),
            //    result.CorrectedProfitLoss * 100M,
            //    result.Principal,
            //    result.CorrectedBalance));

        }

        [TestMethod]
        public void BackTestingMethod()
        {
            

        }
    }
}
