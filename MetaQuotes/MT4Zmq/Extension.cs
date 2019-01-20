using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core;

namespace MT4ZmqSingal
{
    public struct TickRate
    {
        public DateTime TickTime;
        public decimal Bid;
        public decimal Ask;
        public decimal Volume;
    }

    public static class Extension
    {
        public static DateTime ClearSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }

        public static DateTime TrimSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime ConvertFromMtTime(int time)
        {
            DateTime tmpTime = new DateTime(1970, 1, 1);
            return new DateTime(tmpTime.Ticks + (time * 0x989680L));
        }

        public static int ConvertToMtTime(DateTime? time)
        {
            int result = 0;
            if (time != null && time != DateTime.MinValue)
            {
                DateTime tmpTime = new DateTime(1970, 1, 1);
                result = (int)((time.Value.Ticks - tmpTime.Ticks) / 0x989680L);
            }
            return result;
        }

        public static Color ConvertFromMtColor(int color)
        {
            return Color.FromArgb((byte)(color), (byte)(color >> 8), (byte)(color >> 16));
        }

        public static int ConvertToMtColor(Color? color)
        {
            return color == null || color == Color.Empty ? 0xffffff : (Color.FromArgb(color.Value.B, color.Value.G, color.Value.R).ToArgb() & 0xffffff);
        }

        public static Candle MerginTickData(this Candle candle, TickRate tick)
        {
            decimal myPrice = Convert.ToDecimal(tick.Bid);
            candle.Close = myPrice;
            candle.High = Math.Max(candle.High, myPrice);
            candle.Low = Math.Min(candle.Low, myPrice);
            candle.Volume += 1;
            return candle;
        }

        /// <summary>
        /// 使用字符串替换控制台的当前行
        /// </summary>
        /// <param name="lineStr"></param>
        public static void ConsoleLineReplace(this string lineStr)
        {
            ClearCurrentConsoleLine();
            Console.Write(lineStr);
        }

        public static void ClearCurrentConsoleLine()
        {
            try
            {
                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new String(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
            }
            catch (System.IO.IOException)
            {

            }
        }

    }
}
