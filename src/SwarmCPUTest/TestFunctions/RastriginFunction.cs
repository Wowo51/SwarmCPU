using SwarmCPU.Interfaces;
using System;

namespace SwarmCPUTest.TestFunctions
{
    public class RastriginFunction : IFitnessFunction
    {
        public double Evaluate(double[] position)
        {
            double sum = 0.0;
            double n = (double)position.Length;

            for (int i = 0; i < position.Length; i++)
            {
                sum += (position[i] * position[i]) - (10.0 * Math.Cos(2.0 * Math.PI * position[i]));
            }
            return (10.0 * n) + sum;
        }
    }
}