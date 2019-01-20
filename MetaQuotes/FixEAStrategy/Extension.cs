using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FixEAStrategy
{
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

        #region 数据转换

        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime ToDateTime(this long unixTime)
        {
            return DateTime.FromBinary(unixTime * 10000000 + 621355968000000000).ToLocalTime();
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

        #endregion

    }
}
