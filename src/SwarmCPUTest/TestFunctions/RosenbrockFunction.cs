using SwarmCPU.Interfaces;
using System;

namespace SwarmCPUTest.TestFunctions
{
    public class RosenbrockFunction : IFitnessFunction
    {
        public double Evaluate(double[] position)
        {
            double sum = 0.0;
            for (int i = 0; i < position.Length - 1; i++)
            {
                double term1 = position[i + 1] - (position[i] * position[i]);
                double term2 = position[i] - 1.0;
                sum += (100.0 * (term1 * term1)) + (term2 * term2);
            }
            return sum;
        }
    }
}