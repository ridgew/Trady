using System;
using System.IO;
using System.Text;
using Trady.Core;

namespace TickDataImporter
{
    public static class Pack
    {
        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime ToDateTime(this long unixTime)
        {
            return DateTime.FromBinary(unixTime * 10000000 + 621355968000000000).ToLocalTime();
        }

        public static DateTime ClearSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }

        public static DateTime ClearMinutes(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
        }

        public static MT4Candle ToCandle(this TickRate rate)
        {
            return new MT4Candle(new DateTimeOffset(rate.TickTime),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Volume));
        }

        public static Candle MerginTickData(this Candle candle, TickRate tick)
        {
            decimal myPrice = Convert.ToDecimal(tick.Bid);
            candle.Close = myPrice;
            candle.High = Math.Max(candle.High, myPrice);
            candle.Low = Math.Min(candle.Low, myPrice);
            return candle;
        }

        public static int ReadInt(this Stream fs, ref byte[] buf)
        {
            int total = fs.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public static int ReadIntReversed(this Stream fs, ref byte[] buf)
        {
            byte[] myBuf = new byte[4];
            int total = fs.Read(myBuf, 0, 4);
            return byteArr2Int(myBuf);
        }

        public static int byteArr2Int(byte[] arrB)
        {
            if (arrB == null || arrB.Length != 4)
            {
                return 0;
            }
            int i = (arrB[0] << 24) + (arrB[1] << 16) + (arrB[2] << 8) + arrB[3];
            return i;
        }

        public static byte[] int2ByteArr(int i)
        {
            byte[] arrB = new byte[4];
            arrB[0] = (byte)(i >> 24);
            arrB[1] = (byte)(i >> 16);
            arrB[2] = (byte)(i >> 8);
            arrB[3] = (byte)i;
            return arrB;
        }

        public static long ReadLong(this Stream fs, ref byte[] buf)
        {
            long total = fs.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public static double ReadDouble(this Stream fs, ref byte[] buf)
        {
            int total = fs.Read(buf, 0, 8);
            return BitConverter.ToDouble(buf, 0);
        }

        public static DateTime ReadTime(this Stream fs, ref byte[] buf)
        {
            long iIntVer = ReadLong(fs, ref buf);
            return ToDateTime(iIntVer);
        }

        public static string ReadString(this Stream fs, ref byte[] buf, int length)
        {
            int total = fs.Read(buf, 0, length);
            return Encoding.UTF8.GetString(buf, 0, total).TrimEnd('\0');
        }
    }
}
