using SwarmCPU.Interfaces;

namespace SwarmCPUTest.TestFunctions
{
    public class SphereFunction : IFitnessFunction
    {
        /// <summary>
        /// The Sphere function is a simple benchmark function,
        /// where f(x) = sum(x_i^2) for i=1 to n.
        /// The global minimum is at x = (0, 0, ..., 0), with f(x) = 0.
        /// </summary>
        /// <param name="position">The input position vector.</param>
        /// <returns>The calculated fitness value.</returns>
        public double Evaluate(double[] position)
        {
            double sum = 0.0;
            for (int i = 0; i < position.Length; i++)
            {
                sum += position[i] * position[i];
            }
            return sum;
        }
    }
}