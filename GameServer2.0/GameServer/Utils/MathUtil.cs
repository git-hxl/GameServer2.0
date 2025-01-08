
using System.Numerics;


namespace GameServer.Utils
{
    public static class MathUtil
    {
        public static double Magnitude(Vector3 a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public static double Angle(Vector3 a, Vector3 b)
        {
            float dot = Vector3.Dot(a, b);
            float magnitudeA = a.Length();
            float magnitudeB = b.Length();

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0;
            }
            return Math.Acos(dot / (magnitudeA * magnitudeB));
        }

        public static double Distance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        public static double Distance(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        {
            Vector3 aa = new Vector3(a.x, a.y, a.z);
            Vector3 bb = new Vector3(b.x, b.y, b.z);
            return Vector3.Distance(aa, bb);
        }
    }
}