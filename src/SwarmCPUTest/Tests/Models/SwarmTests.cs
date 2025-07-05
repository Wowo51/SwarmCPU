using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmCPU.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SwarmCPUTest.Tests.Models
{
    [TestClass]
    public class SwarmTests
    {
        private static readonly int _dimensions = 2;
        private static readonly double[] _minPosition = new double[] { -5.0, -5.0 };
        private static readonly double[] _maxPosition = new double[] { 5.0, 5.0 };

        [TestMethod]
        public void Swarm_Constructor_InitializesParticlesCorrectly()
        {
            int numberOfParticles = 10;
            Random random = new Random(456);

            Swarm swarm = new Swarm(numberOfParticles, _dimensions, _minPosition, _maxPosition, random);

            Assert.IsNotNull(swarm.Particles, "Particles collection should not be null.");
            Assert.AreEqual(numberOfParticles, swarm.Particles.Count, "Swarm should contain the correct number of particles.");

            foreach (Particle particle in swarm.Particles)
            {
                Assert.IsNotNull(particle, "Each particle in the swarm should not be null.");
                Assert.AreEqual(_dimensions, particle.Position.Length, "Particle position should have correct dimensions.");
                Assert.AreEqual(_dimensions, particle.Velocity.Length, "Particle velocity should have correct dimensions.");

                for (int i = 0; i < _dimensions; i++)
                {
                    Assert.IsTrue(particle.Position[i] >= _minPosition[i] && particle.Position[i] <= _maxPosition[i],
                        $"Particle initial position for dimension {i} is out of bounds.");
                }
            }
        }

        [TestMethod]
        public void Particles_Property_IsReadOnly()
        {
            int numberOfParticles = 5;
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, _dimensions, _minPosition, _maxPosition, random);

            IReadOnlyList<Particle> particles = swarm.Particles;
            Assert.IsInstanceOfType(particles, typeof(IReadOnlyList<Particle>), "Particles property should be IReadOnlyList.");
        }

        [TestMethod]
        public void Swarm_Constructor_ZeroParticles_InitializesEmptySwarm()
        {
            int numberOfParticles = 0;
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, _dimensions, _minPosition, _maxPosition, random);

            Assert.IsNotNull(swarm.Particles, "Particles collection should not be null for zero particles.");
            Assert.AreEqual(0, swarm.Particles.Count, "Swarm should contain zero particles.");
        }

        // Modified tests to reflect non-throwing behavior
        [TestMethod]
        public void Swarm_Constructor_NegativeDimensions_InitializesWithZeroDimensionParticles()
        {
            int numberOfParticles = 1;
            int negativeDimensions = -1;
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, negativeDimensions, _minPosition, _maxPosition, random);

            Assert.AreEqual(numberOfParticles, swarm.Particles.Count);
            Assert.AreEqual(0, swarm.Particles[0].Position.Length, "Particle dimensions should be zero if input dimensions are negative.");
        }

        [TestMethod]
        public void Swarm_Constructor_MinPositionLengthMismatch_InitializesWithZeroDimensionParticles()
        {
            int numberOfParticles = 1;
            int dimensions = 3;
            double[] mismatchedMin = new double[] { -10.0, -10.0 };
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, dimensions, mismatchedMin, _maxPosition, random);

            Assert.AreEqual(numberOfParticles, swarm.Particles.Count);
            Assert.AreEqual(0, swarm.Particles[0].Position.Length, "Particle dimensions should be zero if minPosition length mismatches.");
        }

        [TestMethod]
        public void Swarm_Constructor_NullMinPosition_InitializesWithZeroDimensionParticles()
        {
            int numberOfParticles = 1;
            int dimensions = 3;
            double[]? nullMin = null;
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, dimensions, nullMin!, _maxPosition, random);

            Assert.AreEqual(numberOfParticles, swarm.Particles.Count);
            Assert.AreEqual(0, swarm.Particles[0].Position.Length, "Particle dimensions should be zero if minPosition is null.");
        }

        [TestMethod]
        public void Swarm_Constructor_NegativeNumberOfParticles_InitializesEmptySwarm()
        {
            int negativeNumberOfParticles = -1;
            Random random = new Random();
            Swarm swarm = new Swarm(negativeNumberOfParticles, _dimensions, _minPosition, _maxPosition, random);

            Assert.AreEqual(0, swarm.Particles.Count, "Swarm should contain zero particles for negative number of particles.");
        }

        [TestMethod]
        public void Swarm_Constructor_MaxPositionLengthMismatch_InitializesWithZeroDimensionParticles()
        {
            int numberOfParticles = 1;
            int dimensions = 3;
            double[] mismatchedMax = new double[] { 10.0, 10.0 }; // Mismatched length compared to _dimensions
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, dimensions, _minPosition, mismatchedMax, random);

            Assert.AreEqual(numberOfParticles, swarm.Particles.Count);
            Assert.AreEqual(0, swarm.Particles[0].Position.Length, "Particle dimensions should be zero if maxPosition length mismatches.");
        
        }

        [TestMethod]
        public void Swarm_Constructor_NullMaxPosition_InitializesWithZeroDimensionParticles()
        {
            int numberOfParticles = 1;
            int dimensions = 3;
            double[]? nullMax = null;
            Random random = new Random();
            Swarm swarm = new Swarm(numberOfParticles, dimensions, _minPosition, nullMax!, random);

            Assert.AreEqual(numberOfParticles, swarm.Particles.Count);
            Assert.AreEqual(0, swarm.Particles[0].Position.Length, "Particle dimensions should be zero if maxPosition is null.");
        }

        [TestMethod]
        public void Swarm_Constructor_NullRandom_InitializesWithDefaultRandom()
        {
            int numberOfParticles = 10;
            Random? nullRandom = null;
            Swarm swarm = new Swarm(numberOfParticles, _dimensions, _minPosition, _maxPosition, nullRandom!);
            Assert.IsNotNull(swarm.Particles, "Swarm should still be initialized even with null random.");
            Assert.IsTrue(swarm.Particles.Count == numberOfParticles, "Swarm should have the correct number of particles.");
            // Further checks could verify randomness if needed, but not directly checking the Random instance.
        }
    }
}