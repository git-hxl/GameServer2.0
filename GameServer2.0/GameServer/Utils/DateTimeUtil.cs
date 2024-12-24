

using System;

namespace Utils
{
    public class DateTimeUtil
    {
        /// <summary>
        /// ʱ��������룩
        /// </summary>
        /// <returns></returns>
        public static long TimeStamp
        {
            get
            {
                System.DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (long)(System.DateTime.UtcNow - startTime).TotalMilliseconds;
            }
        }

        /// <summary>
        /// ת��ʱ���
        /// </summary>
        /// <param name="timeStamp">ʱ��������룩</param>
        /// <returns>UTC DateTime</returns>
        public static System.DateTime ConvertToDateTime(long timeStamp)
        {
            System.DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return startTime.AddMilliseconds(timeStamp);
        }
    }
}
