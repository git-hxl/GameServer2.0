
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
    }
}
