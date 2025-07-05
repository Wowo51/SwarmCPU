using SwarmCPU.Helpers;
using System;

namespace SwarmCPU.Models
{
    public class Particle
    {
        private double[] _position;
        private double[] _velocity;
        private double[] _personalBestPosition;
        private double _personalBestValue;
        private int _dimensions;

        public Particle(int dimensions, double[] minPosition, double[] maxPosition, Random random)
        {
            int effectiveDimensions = Math.Max(0, dimensions);

            // Check for invalid dimension or bounds configuration
            bool isValidConfiguration = effectiveDimensions > 0 &&
                                        minPosition is not null && maxPosition is not null &&
                                        minPosition.Length == effectiveDimensions && maxPosition.Length == effectiveDimensions;

            if (!isValidConfiguration)
            {
                // If configuration is invalid, set dimensions to 0 and all related arrays to empty.
                _dimensions = 0;
                _position = Array.Empty<double>();
                _velocity = Array.Empty<double>();
                _personalBestPosition = Array.Empty<double>();
                _personalBestValue = double.MaxValue; // Still initialized to max value for consistency
            }
            else
            {
                _dimensions = effectiveDimensions;
                _position = new double[_dimensions];
                _velocity = new double[_dimensions];
                _personalBestPosition = new double[_dimensions];
                _personalBestValue = double.MaxValue; // Initialize with a very high value for minimization problems
                
                // Bounds are guaranteed non-null by the isValidConfiguration check.
                // Using '!' to assert non-nullability for the compiler and resolve CS8604 warnings.
                InitializeRandom(_dimensions, minPosition!, maxPosition!, random);
            }
        }

        public double[] Position
        {
            get { return _position; }
        }

        public double[] Velocity
        {
            get { return _velocity; }
        }

        public double[] PersonalBestPosition
        {
            get { return VectorMath<double>.Copy(_personalBestPosition); } // Return a copy to prevent external modification
            set { _personalBestPosition = VectorMath<double>.Copy(value); } 
        }

        public double PersonalBestValue
        {
            get { return _personalBestValue; }
            set { _personalBestValue = value; }
        }

        // Method to update particle's position and velocity from the optimizer
        public void UpdateState(double[] newPosition, double[] newVelocity)
        {
            // Update only if _dimensions allows and inputs match.
            if (_dimensions > 0 && newPosition is not null && newVelocity is not null && 
                newPosition.Length == _dimensions && newVelocity.Length == _dimensions)
            {
                _position = VectorMath<double>.Copy(newPosition); 
                _velocity = VectorMath<double>.Copy(newVelocity); 
            }
        }

        private void InitializeRandom(int dimensions, double[] minPosition, double[] maxPosition, Random random)
        {
            // This method should only be called if dimensions are > 0 and bounds are valid.
            for (int i = 0; i < dimensions; i++)
            {
                double range = maxPosition[i] - minPosition[i];
                _position[i] = minPosition[i] + (random.NextDouble() * range);
                // Initial velocity: random value within a fraction of position range, can be negative
                _velocity[i] = (random.NextDouble() * (range / 10.0)) * (random.NextDouble() >= 0.5 ? 1.0 : -1.0); 
            }
            PersonalBestPosition = _position; // Initialize personal best to current position, copies array contents
        }
    }
}