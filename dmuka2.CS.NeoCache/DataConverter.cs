using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// This class usually is used for cast to byte array or for cast from byte array.
    /// </summary>
    internal static class DataConverter
    {
        #region ToByteArray()
        public static byte[] ToByteArrayDC(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] ToByteArrayDC(this byte value)
        {
            return new byte[] { value };
        }

        public static byte[] ToByteArrayDC(this sbyte value)
        {
            byte v = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v = p[0];
            }
            return new byte[] { v };
        }

        public static byte[] ToByteArrayDC(this short value)
        {
            byte v1 = 0;
            byte v2 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
            }
            return new byte[] { v1, v2 };
        }

        public static byte[] ToByteArrayDC(this ushort value)
        {
            byte v1 = 0;
            byte v2 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
            }
            return new byte[] { v1, v2 };
        }

        public static byte[] ToByteArrayDC(this int value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
            }

            return new byte[] { v1, v2, v3, v4 };
        }

        public static byte[] ToByteArrayDC(this uint value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
            }

            return new byte[] { v1, v2, v3, v4 };
        }

        public static byte[] ToByteArrayDC(this long value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            byte v5 = 0;
            byte v6 = 0;
            byte v7 = 0;
            byte v8 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
                v5 = p[4];
                v6 = p[5];
                v7 = p[6];
                v8 = p[7];
            }
            return new byte[] { v1, v2, v3, v4, v5, v6, v7, v8 };
        }

        public static byte[] ToByteArrayDC(this ulong value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            byte v5 = 0;
            byte v6 = 0;
            byte v7 = 0;
            byte v8 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
                v5 = p[4];
                v6 = p[5];
                v7 = p[6];
                v8 = p[7];
            }
            return new byte[] { v1, v2, v3, v4, v5, v6, v7, v8 };
        }

        public static byte[] ToByteArrayDC(this float value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
            }

            return new byte[] { v1, v2, v3, v4 };
        }

        public static byte[] ToByteArrayDC(this double value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            byte v5 = 0;
            byte v6 = 0;
            byte v7 = 0;
            byte v8 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
                v5 = p[4];
                v6 = p[5];
                v7 = p[6];
                v8 = p[7];
            }
            return new byte[] { v1, v2, v3, v4, v5, v6, v7, v8 };
        }

        public static byte[] ToByteArrayDC(this decimal value)
        {
            byte v1 = 0;
            byte v2 = 0;
            byte v3 = 0;
            byte v4 = 0;
            byte v5 = 0;
            byte v6 = 0;
            byte v7 = 0;
            byte v8 = 0;
            byte v9 = 0;
            byte v10 = 0;
            byte v11 = 0;
            byte v12 = 0;
            byte v13 = 0;
            byte v14 = 0;
            byte v15 = 0;
            byte v16 = 0;
            unsafe
            {
                byte* p = (byte*)&value;
                v1 = p[0];
                v2 = p[1];
                v3 = p[2];
                v4 = p[3];
                v5 = p[4];
                v6 = p[5];
                v7 = p[6];
                v8 = p[7];
                v9 = p[8];
                v10 = p[9];
                v11 = p[10];
                v12 = p[11];
                v13 = p[12];
                v14 = p[13];
                v15 = p[14];
                v16 = p[15];
            }
            return new byte[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16 };
        }
        #endregion

        #region ToTypeDC
        public static string ToStringDC(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static byte ToByteDC(this byte[] value)
        {
            return value[0];
        }

        public static sbyte ToSByteDC(this byte[] value)
        {
            sbyte v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
            }

            return v;
        }

        public static short ToInt16DC(this byte[] value)
        {
            short v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
            }

            return v;
        }

        public static ushort ToUInt16DC(this byte[] value)
        {
            ushort v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
            }

            return v;
        }

        public static int ToInt32DC(this byte[] value)
        {
            int v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
            }

            return v;
        }

        public static uint ToUInt32DC(this byte[] value)
        {
            uint v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
            }

            return v;
        }

        public static long ToInt64DC(this byte[] value)
        {
            long v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
                p[4] = value[4];
                p[5] = value[5];
                p[6] = value[6];
                p[7] = value[7];
            }

            return v;
        }

        public static ulong ToUInt64DC(this byte[] value)
        {
            ulong v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
                p[4] = value[4];
                p[5] = value[5];
                p[6] = value[6];
                p[7] = value[7];
            }

            return v;
        }

        public static float ToSingleDC(this byte[] value)
        {
            float v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
            }

            return v;
        }

        public static double ToDoubleDC(this byte[] value)
        {
            double v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
                p[4] = value[4];
                p[5] = value[5];
                p[6] = value[6];
                p[7] = value[7];
            }

            return v;
        }

        public static decimal ToDecimalDC(this byte[] value)
        {
            decimal v = 0;

            unsafe
            {
                byte* p = (byte*)&v;
                p[0] = value[0];
                p[1] = value[1];
                p[2] = value[2];
                p[3] = value[3];
                p[4] = value[4];
                p[5] = value[5];
                p[6] = value[6];
                p[7] = value[7];
                p[8] = value[8];
                p[9] = value[9];
                p[10] = value[10];
                p[11] = value[11];
                p[12] = value[12];
                p[13] = value[13];
                p[14] = value[14];
                p[15] = value[15];
            }

            return v;
        }
        #endregion
    }
}
