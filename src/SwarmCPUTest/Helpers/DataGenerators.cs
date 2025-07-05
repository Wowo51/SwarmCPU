using System;

namespace SwarmCPUTest.Helpers
{
    public static class DataGenerators
    {
        public static double[] GenerateRandomVector(int dimensions, double minValue, double maxValue, Random rng)
        {
            double[] vector = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                vector[i] = minValue + (rng.NextDouble() * (maxValue - minValue));
            }
            return vector;
        }

        public static double[][] GenerateRandomVectors(int count, int dimensions, double minValue, double maxValue, Random rng)
        {
            double[][] vectors = new double[count][];
            for (int i = 0; i < count; i++)
            {
                vectors[i] = GenerateRandomVector(dimensions, minValue, maxValue, rng);
            }
            return vectors;
        }
        
        public static double[] CreateVector(params double[] values)
        {
            return values;
        }
    }
}