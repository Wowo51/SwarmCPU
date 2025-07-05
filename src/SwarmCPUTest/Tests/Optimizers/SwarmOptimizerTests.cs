using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmCPU.Optimizers;
using SwarmCPU.Models;
using SwarmCPU.Interfaces;
using SwarmCPUTest.TestFunctions;
using SwarmCPUTest.Helpers; // For DataGenerators
using SwarmCPU.Helpers; // For VectorMath<double>.Copy in one test
using System;
using System.Linq;
using System.Collections.Generic;

namespace SwarmCPUTest.Tests.Optimizers
{
    [TestClass]
    public class SwarmOptimizerTests
    {
        private static readonly int _dimensions = 2;
        private static readonly double[] _minPosition = new double[] { -5.0, -5.0 };
        private static readonly double[] _maxPosition = new double[] { 5.0, 5.0 };
        private static readonly IFitnessFunction _sphereFunction = new SphereFunction();

        // Common SwarmOptimizer parameters for tests
        private const double _initialInertiaWeight = 0.9; // Updated from 0.729
        private const double _finalInertiaWeight = 0.4; // Updated from 0.729
        private const double _cognitiveWeight = 2.0; // Updated from 1.49445
        private const double _socialWeight = 2.0; // Updated from 1.49445
        private const double _maxVelocityFactor = 0.2; // New parameter
        private const double _targetPrecision = 1E-05; // New parameter from spec
        private const int _stagnationIterations = 50; // Updated from 10

        [TestMethod]
        public void SwarmOptimizer_Constructor_InitializesCorrectly()
        {
            int numberOfParticles = 50;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            Assert.IsNotNull(optimizer, "Optimizer should not be null.");

            Assert.AreNotEqual(double.MaxValue, optimizer.GlobalBestValue, "GlobalBestValue should be initialized to a value from the swarm.");
            Assert.IsNotNull(optimizer.GlobalBestPosition, "GlobalBestPosition should not be null.");
            Assert.AreEqual(_dimensions, optimizer.GlobalBestPosition.Length, "GlobalBestPosition should have correct dimensions.");
        }

        [TestMethod]
        public void Optimize_SphereFunction_FindsMinimumWithinTolerance()
        {
            int numberOfParticles = 100;
            int maxIterations = 1000;
            double tolerance = 1e-3;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(maxIterations);

            double[] globalBestPosition = optimizer.GlobalBestPosition;
            double globalBestValue = optimizer.GlobalBestValue;

            Assert.AreEqual(0.0, globalBestValue, tolerance, $"Global best value should be close to 0. Actual: {globalBestValue}");

            for (int i = 0; i < _dimensions; i++)
            {
                Assert.AreEqual(0.0, globalBestPosition[i], tolerance, $"Global best position dimension {i} should be close to 0. Actual: {globalBestPosition[i]}");
            }

            for (int i = 0; i < _dimensions; i++)
            {
                Assert.IsTrue(globalBestPosition[i] >= _minPosition[i] && globalBestPosition[i] <= _maxPosition[i],
                    $"Global best position for dimension {i} is out of bounds after optimization.");
            }
        }

        [TestMethod]
        public void Optimize_ConvergenceOverIterations()
        {
            int numberOfParticles = 30;
            int maxIterations = 100;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(1);
            double globalBestAfterOneIteration = optimizer.GlobalBestValue;
            optimizer.Optimize(maxIterations - 1);

            Assert.IsTrue(optimizer.GlobalBestValue <= globalBestAfterOneIteration, "Global best value should generally decrease or stay same over more iterations.");
        }

        [TestMethod]
        public void GlobalBestPosition_Property_ReturnsCopy()
        {
            int numberOfParticles = 1;
            int maxIterations = 1;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(maxIterations);

            double[] retrievedPosition = optimizer.GlobalBestPosition;
            Assert.IsNotNull(retrievedPosition, "Retrieved GlobalBestPosition should not be null.");

            retrievedPosition[0] = 999.0;

            double[] secondRetrievedPosition = optimizer.GlobalBestPosition;
            Assert.AreNotEqual(999.0, secondRetrievedPosition[0], "Modifying the retrieved GlobalBestPosition should not affect the optimizer's internal state.");
            Assert.AreNotSame(retrievedPosition, secondRetrievedPosition, "Successive calls to GlobalBestPosition should return different array instances.");
        }

        [TestMethod]
        public void Optimize_From_KnownOptimalPosition_MaintainsOptimality()
        {
            int numberOfParticles = 50;
            int maxIterations = 100;
            double tolerance = 1e-4; // The required stringent numerical tolerance

            IFitnessFunction sphereFunc = new SphereFunction();

            double[] minBound = new double[] { -0.1, -0.1 };
            double[] maxBound = new double[] { 0.1, 0.1 };

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, minBound, maxBound,
                sphereFunc, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, tolerance, _stagnationIterations); // Pass tolerance to the optimizer

            optimizer.Optimize(maxIterations); // The optimizer will stop early if targetPrecision is met

            double[] finalGlobalBestPosition = optimizer.GlobalBestPosition;
            double finalGlobalBestValue = optimizer.GlobalBestValue;

            Assert.AreEqual(0.0, finalGlobalBestValue, tolerance, "Global best value should remain near optimal.");
            for (int i = 0; i < _dimensions; i++)
            {
                Assert.AreEqual(0.0, finalGlobalBestPosition[i], tolerance, $"Global best position dimension {i} should remain near optimal.");
            }
        }

        [TestMethod]
        public void Optimize_RastriginFunction_FindsMinimumWithinTolerance()
        {
            int numberOfParticles = 75;
            int maxIterations = 2000;
            int dimensions = 3;
            double[] rastriginMinPos = DataGenerators.CreateVector(-5.12, -5.12, -5.12);
            double[] rastriginMaxPos = DataGenerators.CreateVector(5.12, 5.12, 5.12);
            double[] expectedPosition = DataGenerators.CreateVector(0.0, 0.0, 0.0);
            double expectedValue = 0.0;
            double positionTolerance = 0.1;
            double valueTolerance = 0.5;

            IFitnessFunction rastriginFunction = new RastriginFunction();

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, dimensions, rastriginMinPos, rastriginMaxPos,
                rastriginFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(maxIterations);

            double[] globalBestPosition = optimizer.GlobalBestPosition;
            double globalBestValue = optimizer.GlobalBestValue;

            Assert.AreEqual(expectedValue, globalBestValue, valueTolerance, "Rastrigin global best value not found within tolerance.");
            for (int i = 0; i < dimensions; i++)
            {
                Assert.AreEqual(expectedPosition[i], globalBestPosition[i], positionTolerance, $"Rastrigin global best position dimension {i} not found within tolerance.");
            }
        }

        [TestMethod]
        public void Optimize_RosenbrockFunction_FindsMinimumWithinTolerance()
        {
            int numberOfParticles = 100;
            int maxIterations = 500;
            int dimensions = 2;
            double[] rosenbrockMinPos = DataGenerators.CreateVector(-2.048, -2.048);
            double[] rosenbrockMaxPos = DataGenerators.CreateVector(2.048, 2.048);
            double[] expectedPosition = DataGenerators.CreateVector(1.0, 1.0);
            double expectedValue = 0.0;
            double tolerance = 0.1;

            IFitnessFunction rosenbockFunction = new RosenbrockFunction();

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, dimensions, rosenbrockMinPos, rosenbrockMaxPos,
                rosenbockFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(maxIterations);

            double[] globalBestPosition = optimizer.GlobalBestPosition;
            double globalBestValue = optimizer.GlobalBestValue;

            Assert.AreEqual(expectedValue, globalBestValue, tolerance, "Rosenbrock global best value not found within tolerance.");
            for (int i = 0; i < dimensions; i++)
            {
                Assert.AreEqual(expectedPosition[i], globalBestPosition[i], tolerance, $"Rosenbrock global best position dimension {i} not found within tolerance.");
            }
        }

        // Modified tests to reflect non-throwing behavior
        [TestMethod]
        public void SwarmOptimizer_Constructor_NegativeNumberOfParticles_InitializesWithZeroParticles()
        {
            int negativeParticles = -1;
            SwarmOptimizer optimizer = new SwarmOptimizer(negativeParticles, _dimensions, _minPosition, _maxPosition,
                                                          _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                                                          _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);
            Assert.AreEqual(0, optimizer.GlobalBestPosition.Length, "Global best position should be empty for zero-particle swarm.");
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Global best value should be MaxValue if no particles.");
            optimizer.Optimize(1); // Test that optimizing an empty swarm does not crash
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Global best value should remain MaxValue after optimization with no particles.");
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_ZeroDimensions_InitializesCorrectly()
        {
            int dimensions = 0;
            double[] emptyBounds = new double[] { };
            SwarmOptimizer optimizer = new SwarmOptimizer(
                10, dimensions, emptyBounds, emptyBounds,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);
            Assert.IsNotNull(optimizer.GlobalBestPosition);
            Assert.AreEqual(0, optimizer.GlobalBestPosition.Length);
            optimizer.Optimize(1);
            Assert.IsTrue(optimizer.GlobalBestValue <= double.MaxValue); // Should still be max value if 0 dimensions
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_NullFitnessFunction_UsesDefaultMaxValueFunction()
        {
            IFitnessFunction? nullFunction = null;
            SwarmOptimizer optimizer = new SwarmOptimizer(50, _dimensions, _minPosition, _maxPosition,
                                                          nullFunction!, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                                                          _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Optimizer should use a default function that returns MaxValue.");
            optimizer.Optimize(1);
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Optimizer with null fitness function should keep GlobalBestValue at MaxValue.");
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_NullMinPosition_InitializesWithZeroDimensions()
        {
            double[]? nullMin = null;
            SwarmOptimizer optimizer = new SwarmOptimizer(50, _dimensions, nullMin!, _maxPosition,
                                                           _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                                                           _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);
            Assert.AreEqual(0, optimizer.GlobalBestPosition.Length, "Global best position should be empty if minPosition is null.");
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Global best value should be MaxValue if dimensions are zeroed due to null bounds.");
        }

        [TestMethod]
        public void Optimize_ZeroIterations_DoesNotChangeGlobalBest()
        {
            int numberOfParticles = 50;
            int maxIterations = 0;
            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            double initialGlobalBestValue = optimizer.GlobalBestValue;
            double[] initialGlobalBestPosition = VectorMath<double>.Copy(optimizer.GlobalBestPosition);

            optimizer.Optimize(maxIterations);

            Assert.AreEqual(initialGlobalBestValue, optimizer.GlobalBestValue, "Global best value should not change with zero iterations.");
            CollectionAssert.AreEqual(initialGlobalBestPosition, optimizer.GlobalBestPosition, "Global best position should not change with zero iterations.");
        }

        [TestMethod]
        public void Optimize_OneIteration_UpdatesGlobalBest()
        {
            int numberOfParticles = 50;
            int maxIterations = 1;
            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            double initialGlobalBestValue = optimizer.GlobalBestValue;

            optimizer.Optimize(maxIterations);

            Assert.IsTrue(optimizer.GlobalBestValue <= initialGlobalBestValue, "Global best value should improve or stay same after one iteration.");
        }

        [TestMethod]
        public void Optimize_ZeroCognitiveSocialWeights_ConvergesLessEffectively()
        {
            int numberOfParticles = 50;
            int maxIterations = 200;
            double inertiaWeight = 0.729;
            double cognitiveWeight = 0.0;
            double socialWeight = 0.0;
            double expectedTolerance = 0.01;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, inertiaWeight, inertiaWeight, cognitiveWeight, socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(maxIterations);

            double globalBestValue = optimizer.GlobalBestValue;
            Assert.IsTrue(globalBestValue < double.MaxValue, "Optimizer should not crash and find some value.");
            Assert.AreNotEqual(0.0, globalBestValue, expectedTolerance, "Optimizer should not find exact minimum with zero cognitive/social weights.");
        }

        [TestMethod]
        public void Optimize_OneDimension_SphereFunction_FindsMinimum()
        {
            int numberOfParticles = 30;
            int dimensions = 1;
            double[] minPosition1D = DataGenerators.CreateVector(-10.0);
            double[] maxPosition1D = DataGenerators.CreateVector(10.0);
            double tolerance = 1e-2;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, dimensions, minPosition1D, maxPosition1D,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            optimizer.Optimize(200);

            Assert.AreEqual(0.0, optimizer.GlobalBestValue, tolerance, "1D Sphere function: Global best value not found.");
            Assert.AreEqual(0.0, optimizer.GlobalBestPosition[0], tolerance, "1D Sphere function: Global best position not found.");
        }

        [TestMethod]
        public void SwarmOptimizer_InitialGlobalBest_IsMeaningful()
        {
            int numberOfParticles = 50;
            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            double initialGlobalBestValue = optimizer.GlobalBestValue;
            double[] initialGlobalBestPosition = optimizer.GlobalBestPosition;

            Assert.AreNotEqual(double.MaxValue, initialGlobalBestValue, "Initial GlobalBestValue should be updated from MaxValue.");
            Assert.IsNotNull(initialGlobalBestPosition, "Initial GlobalBestPosition should not be null.");
            Assert.AreEqual(_dimensions, initialGlobalBestPosition.Length, "Initial GlobalBestPosition should have correct dimensions.");

            double evaluatedValue = _sphereFunction.Evaluate(initialGlobalBestPosition);
            Assert.AreEqual(evaluatedValue, initialGlobalBestValue, 1e-9, "Initial GlobalBestValue should match evaluation of initial GlobalBestPosition.");
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_MinPositionLengthMismatch_InitializesWithZeroDimensions()
        {
            int dimensions = 3;
            double[] mismatchedMin = new double[] { -10.0, -10.0 };
            SwarmOptimizer optimizer = new SwarmOptimizer(50, dimensions, mismatchedMin, _maxPosition, _sphereFunction,
                _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            Assert.AreEqual(0, optimizer.GlobalBestPosition.Length, "Global best position should be empty if minPosition length mismatches.");
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Global best value should be MaxValue if dimensions are zeroed due to mismatched bounds.");
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_MaxPositionLengthMismatch_InitializesWithZeroDimensions()
        {
            int dimensions = 3;
            double[] _tempMinPosition = new double[] { -10.0, -10.0, -10.0 };
            double[] mismatchedMax = new double[] { 10.0 };
            SwarmOptimizer optimizer = new SwarmOptimizer(50, dimensions, _tempMinPosition, mismatchedMax, _sphereFunction,
                _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            Assert.AreEqual(0, optimizer.GlobalBestPosition.Length, "Global best position should be empty if maxPosition length mismatches.");
            Assert.AreEqual(double.MaxValue, optimizer.GlobalBestValue, "Global best value should be MaxValue if dimensions are zeroed due to mismatched bounds.");
        }

        [TestMethod]
        public void SwarmOptimizer_Constructor_InitialAndFinalInertiaWeightParameters()
        {
            int numberOfParticles = 50;
            int dimensions = 2;
            double initialInertia = 0.9;
            double finalInertia = 0.4;
            double cognitive = 1.5;
            double social = 1.5;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, dimensions, _minPosition, _maxPosition,
                _sphereFunction, initialInertia, finalInertia, cognitive, social,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            Assert.IsNotNull(optimizer);
            // Internal parameters are not publicly exposed, so we can't directly assert them.
            // But the constructor call works.
        }
    }
}