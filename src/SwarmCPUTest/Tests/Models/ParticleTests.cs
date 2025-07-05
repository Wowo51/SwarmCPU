using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmCPU.Models;
using SwarmCPU.Helpers;
using System;

namespace SwarmCPUTest.Tests.Models
{
    [TestClass]
    public class ParticleTests
    {
        private static readonly int _dimensions = 3;
        private static readonly double[] _minPosition = new double[] { -10.0, -10.0, -10.0 };
        private static readonly double[] _maxPosition = new double[] { 10.0, 10.0, 10.0 };

        [TestMethod]
        public void Particle_Constructor_InitializesCorrectly()
        {
            Random random = new Random(123); 
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);

            Assert.IsNotNull(particle.Position, "Position should not be null.");
            Assert.AreEqual(_dimensions, particle.Position.Length, "Position should have correct dimensions.");
            
            Assert.IsNotNull(particle.Velocity, "Velocity should not be null.");
            Assert.AreEqual(_dimensions, particle.Velocity.Length, "Velocity should have correct dimensions.");

            Assert.IsNotNull(particle.PersonalBestPosition, "PersonalBestPosition should not be null.");
            Assert.AreEqual(_dimensions, particle.PersonalBestPosition.Length, "PersonalBestPosition should have correct dimensions.");
            
            Assert.AreEqual(double.MaxValue, particle.PersonalBestValue, "PersonalBestValue should be initialized to MaxValue.");

            for (int i = 0; i < _dimensions; i++)
            {
                Assert.IsTrue(particle.Position[i] >= _minPosition[i] && particle.Position[i] <= _maxPosition[i], 
                    $"Initial position {particle.Position[i]} for dimension {i} is out of bounds [{_minPosition[i]}, {_maxPosition[i]}].");
            }

            CollectionAssert.AreEqual(particle.Position, particle.PersonalBestPosition, "PersonalBestPosition should be initialized to current position's content.");
            Assert.AreNotSame(particle.Position, particle.PersonalBestPosition, "PersonalBestPosition array should be a different instance than Position array.");
        }

        [TestMethod]
        public void UpdateState_UpdatesPositionAndVelocityCorrectly()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);

            double[] newPosition = new double[] { 1.0, 2.0, 3.0 };
            double[] newVelocity = new double[] { 0.1, 0.2, 0.3 };

            particle.UpdateState(newPosition, newVelocity);

            CollectionAssert.AreEqual(newPosition, particle.Position, "Position was not updated correctly.");
            CollectionAssert.AreEqual(newVelocity, particle.Velocity, "Velocity was not updated correctly.");

            Assert.AreNotSame(newPosition, particle.Position, "Position array should be a different instance after update.");
            Assert.AreNotSame(newVelocity, particle.Velocity, "Velocity array should be a different instance after update.");
        }

        [TestMethod]
        public void PersonalBestPosition_Setter_CreatesCopy()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);

            double[] newPersonalBest = new double[] { 5.0, 5.0, 5.0 };
            particle.PersonalBestPosition = newPersonalBest;

            CollectionAssert.AreEqual(newPersonalBest, particle.PersonalBestPosition, "PersonalBestPosition content was not set correctly.");
            Assert.AreNotSame(newPersonalBest, particle.PersonalBestPosition, "PersonalBestPosition array instance should be a copy, not the original.");

            newPersonalBest[0] = 99.0;
            Assert.AreNotEqual(99.0, particle.PersonalBestPosition[0], "PersonalBestPosition should not be affected by changes to original array.");
        }

        [TestMethod]
        public void PersonalBestPosition_Getter_ReturnsCopy()
        {
            Random random = new Random(789);
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);
            
            double[] initialPersonalBest = new double[] { 1.0, 1.0, 1.0 };
            particle.PersonalBestPosition = initialPersonalBest;

            double[] retrievedPosition = particle.PersonalBestPosition;
            Assert.IsNotNull(retrievedPosition, "Retrieved PersonalBestPosition should not be null.");
            CollectionAssert.AreEqual(initialPersonalBest, retrievedPosition, "Retrieved position mismatch.");
            Assert.AreNotSame(initialPersonalBest, retrievedPosition, "Getter should return a copy, not the original instance.");

            retrievedPosition[0] = 99.0;
            double[] secondRetrievedPosition = particle.PersonalBestPosition;
            Assert.AreNotEqual(99.0, secondRetrievedPosition[0], "Modifying the retrieved array should not affect the particle's internal best position.");
        }


        [TestMethod]
        public void Particle_Constructor_ZeroDimensions_InitializesCorrectlyAsEmpty()
        {
            int zeroDimensions = 0;
            double[] emptyMinMax = new double[] { };
            Random random = new Random();

            Particle particle = new Particle(zeroDimensions, emptyMinMax, emptyMinMax, random);

            Assert.IsNotNull(particle.Position);
            Assert.AreEqual(0, particle.Position.Length);
            Assert.IsNotNull(particle.Velocity);
            Assert.AreEqual(0, particle.Velocity.Length);
            Assert.IsNotNull(particle.PersonalBestPosition);
            Assert.AreEqual(0, particle.PersonalBestPosition.Length);
            Assert.AreEqual(double.MaxValue, particle.PersonalBestValue);
        }

        // Modified tests to reflect non-throwing behavior
        [TestMethod]
        public void Particle_Constructor_NegativeDimensions_InitializesAsZeroDimensions()
        {
            int negativeDimensions = -1;
            Random random = new Random();
            Particle particle = new Particle(negativeDimensions, _minPosition, _maxPosition, random);

            Assert.AreEqual(0, particle.Position.Length, "Particle position should have 0 dimensions for negative input dimensions.");
            Assert.AreEqual(0, particle.Velocity.Length, "Particle velocity should have 0 dimensions for negative input dimensions.");
        }

        [TestMethod]
        public void Particle_Constructor_MinPositionLengthMismatch_InitializesAsZeroDimensions()
        {
            int dimensions = 3;
            double[] mismatchedMin = new double[] { -10.0, -10.0 };
            Random random = new Random();
            Particle particle = new Particle(dimensions, mismatchedMin, _maxPosition, random);

            Assert.AreEqual(0, particle.Position.Length, "Particle position should have 0 dimensions for minPosition length mismatch.");
        }

        [TestMethod]
        public void Particle_Constructor_MaxPositionLengthMismatch_InitializesAsZeroDimensions()
        {
            int dimensions = 3;
            double[] mismatchedMax = new double[] { 10.0 };
            Random random = new Random();
            Particle particle = new Particle(dimensions, _minPosition, mismatchedMax, random);

            Assert.AreEqual(0, particle.Position.Length, "Particle position should have 0 dimensions for maxPosition length mismatch.");
        }

        [TestMethod]
        public void Particle_Constructor_NullMinPosition_InitializesAsZeroDimensions()
        {
            int dimensions = 3;
            double[]? nullMin = null;
            Random random = new Random();
            Particle particle = new Particle(dimensions, nullMin!, _maxPosition, random);

            Assert.AreEqual(0, particle.Position.Length, "Particle position should have 0 dimensions for null minPosition.");
        }

        [TestMethod]
        public void Particle_Constructor_NullMaxPosition_InitializesAsZeroDimensions()
        {
            int dimensions = 3;
            double[]? nullMax = null;
            Random random = new Random();
            Particle particle = new Particle(dimensions, _minPosition, nullMax!, random);

            Assert.AreEqual(0, particle.Position.Length, "Particle position should have 0 dimensions for null maxPosition.");
        }

        [TestMethod]
        public void UpdateState_NullNewPosition_DoesNotUpdateState()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);
            double[] initialPosition = VectorMath<double>.Copy(particle.Position);
            double[] initialVelocity = VectorMath<double>.Copy(particle.Velocity);

            particle.UpdateState(null!, new double[_dimensions]);

            CollectionAssert.AreEqual(initialPosition, particle.Position, "Position should not be updated with null newPosition.");
            CollectionAssert.AreEqual(initialVelocity, particle.Velocity, "Velocity should not be updated when newPosition is null.");
        }

        [TestMethod]
        public void UpdateState_NullNewVelocity_DoesNotUpdateState()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);
            double[] initialPosition = VectorMath<double>.Copy(particle.Position);
            double[] initialVelocity = VectorMath<double>.Copy(particle.Velocity);

            particle.UpdateState(new double[_dimensions], null!);

            CollectionAssert.AreEqual(initialPosition, particle.Position, "Position should not be updated when newVelocity is null.");
            CollectionAssert.AreEqual(initialVelocity, particle.Velocity, "Velocity should not be updated with null newVelocity.");
        }

        [TestMethod]
        public void UpdateState_MismatchedPositionLength_DoesNotUpdateState()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);
            double[] initialPosition = VectorMath<double>.Copy(particle.Position);
            double[] initialVelocity = VectorMath<double>.Copy(particle.Velocity);

            particle.UpdateState(new double[_dimensions + 1], new double[_dimensions]);

            CollectionAssert.AreEqual(initialPosition, particle.Position, "Position should not be updated with mismatched newPosition length.");
            CollectionAssert.AreEqual(initialVelocity, particle.Velocity, "Velocity should not be updated with mismatched newPosition length.");
        }

        [TestMethod]
        public void UpdateState_MismatchedVelocityLength_DoesNotUpdateState()
        {
            Random random = new Random();
            Particle particle = new Particle(_dimensions, _minPosition, _maxPosition, random);
            double[] initialPosition = VectorMath<double>.Copy(particle.Position);
            double[] initialVelocity = VectorMath<double>.Copy(particle.Velocity);

            particle.UpdateState(new double[_dimensions], new double[_dimensions - 1]);

            CollectionAssert.AreEqual(initialPosition, particle.Position, "Position should not be updated with mismatched newVelocity length.");
            CollectionAssert.AreEqual(initialVelocity, particle.Velocity, "Velocity should not be updated with mismatched newVelocity length.");
        }
    }
}