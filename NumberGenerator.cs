using System;

namespace Complex_Systems_Lab1
{
    public static class NumberGenerator
    {
        private static readonly Random rand = new Random();

        public static double DoubleInRange(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static void FillVector(double[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = DoubleInRange(0.0, 1000.0);
            }
        }

        public static void FillMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = DoubleInRange(0.0, 1000.0);
                }
            }
        }
    }
}
