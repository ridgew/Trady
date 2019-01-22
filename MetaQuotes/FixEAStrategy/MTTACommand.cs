namespace FixEAStrategy
{
    public abstract class MTTACommand : MetaTraderCommand
    {
        public MTTACommand(string symbol, string period, int barShift)
        {
            CommandName = "MT-TA";

            Symbol = symbol;
            PeriodString = period;
            BarShift = barShift;
        }

        public string Symbol { get; set; }

        public string PeriodString { get; set; }

        public int BarShift { get; set; }

        public abstract string SubCommandName { get; }

        public override string GetParameterJson()
        {
            return string.Concat("{",
                string.Format("\"Fn\":\"{3}\", \"Period\":\"{0}\", \"BarShift\":{1}, \"Symbol\":\"{2}\"", PeriodString, BarShift, Symbol, SubCommandName),
                "}");
        }
    }
}
