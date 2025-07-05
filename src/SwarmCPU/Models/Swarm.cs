using SwarmCPU.Helpers;
using System.Collections.Generic;
using System; // For Random

namespace SwarmCPU.Models
{
    public class Swarm
    {
        private List<Particle> _particles;
        private int _dimensions;
        private double[] _minPosition;
        private double[] _maxPosition;
        private Random _random; // Each swarm instance gets its own random for particle initialization

        public Swarm(int numberOfParticles, int dimensions, double[] minPosition, double[] maxPosition, Random random)
        {
            _dimensions = Math.Max(0, dimensions); // Clamp dimensions to non-negative
            int actualNumberOfParticles = Math.Max(0, numberOfParticles); // Clamp number of particles to non-negative

            if (random is null)
            {
                _random = new Random(); // Provide a default random if null is passed
            }
            else
            {
                _random = random;
            }

            // Ensure min/max position arrays are valid for the clamped dimensions
            if (_dimensions > 0 && (minPosition is null || maxPosition is null || minPosition.Length != _dimensions || maxPosition.Length != _dimensions))
            {
                // If bounding arrays are invalid for non-zero dimensions, set dimensions to zero
                // This will result in particles being initialized with zero dimensions.
                _dimensions = 0; 
                _minPosition = Array.Empty<double>();
                _maxPosition = Array.Empty<double>();
            }
            else
            {
                _minPosition = minPosition;
                _maxPosition = maxPosition;
            }

            _particles = new List<Particle>(actualNumberOfParticles);

            InitializeParticles(actualNumberOfParticles);
        }

        public IReadOnlyList<Particle> Particles
        {
            get { return _particles; }
        }

        private void InitializeParticles(int numberOfParticles)
        {
            for (int i = 0; i < numberOfParticles; i++)
            {
                _particles.Add(new Particle(_dimensions, _minPosition, _maxPosition, _random));
            }
        }
    }
}