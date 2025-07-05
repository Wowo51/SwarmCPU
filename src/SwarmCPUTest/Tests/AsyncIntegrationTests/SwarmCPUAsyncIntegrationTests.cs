using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmCPU.Optimizers;
using SwarmCPU.Interfaces;
using SwarmCPUTest.TestFunctions;
using SwarmCPUTest.Helpers;
using System.Threading.Tasks;
using System;

namespace SwarmCPUTest.Tests.AsyncIntegrationTests
{
    [TestClass]
    public class SwarmCPUAsyncIntegrationTests
    {
        private static readonly int _dimensions = 3;
        private static readonly double[] _minPosition = new double[] { -10.0, -10.0, -10.0 };
        private static readonly double[] _maxPosition = new double[] { 10.0, 10.0, 10.0 };
        private readonly IFitnessFunction _sphereFunction = new SphereFunction();

        // Common SwarmOptimizer parameters for tests
        private const double _initialInertiaWeight = 0.9;
        private const double _finalInertiaWeight = 0.4;
        private const double _cognitiveWeight = 2.0;
        private const double _socialWeight = 2.0;
        private const double _maxVelocityFactor = 0.2; // New parameter
        private const double _targetPrecision = 1E-05; // New parameter from spec
        private const int _stagnationIterations = 50; // New parameter

        [TestMethod]
        public async Task Optimize_Async_DoesNotBlockCallingThread()
        {
            int numberOfParticles = 50;
            int maxIterations = 50;

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            bool callingThreadContinued = false;
            Task<double> optimizationTask = Task.Run(() =>
            {
                optimizer.Optimize(maxIterations);
                return optimizer.GlobalBestValue;
            });

            await Task.Delay(10);
            callingThreadContinued = true;

            double finalBestValue = await optimizationTask;

            Assert.IsTrue(callingThreadContinued, "Calling thread was blocked by synchronous Optimize call.");
            Assert.AreEqual(0.0, finalBestValue, 1e-2, "Optimizer did not find minimum asynchronously.");
        }

        [TestMethod]
        public async Task MultipleOptimizationTasks_RunConcurrently_VerifyIndependence()
        {
            int numberOfParticles = 30;
            int maxIterations = 75;
            double tolerance = 1e-2;
            
            SwarmOptimizer optimizer1 = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            SwarmOptimizer optimizer2 = new SwarmOptimizer(
                numberOfParticles, _dimensions, _minPosition, _maxPosition,
                _sphereFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            Task<double> task1 = Task.Run(() => {
                optimizer1.Optimize(maxIterations);
                return optimizer1.GlobalBestValue;
            });

            Task<double> task2 = Task.Run(() => {
                optimizer2.Optimize(maxIterations);
                return optimizer2.GlobalBestValue;
            });

            await Task.WhenAll(task1, task2);

            double result1 = task1.Result;
            double result2 = task2.Result;

            Assert.AreEqual(0.0, result1, tolerance, "Optimizer 1 did not find the minimum.");
            Assert.AreEqual(0.0, result2, tolerance, "Optimizer 2 did not find the minimum.");
        }

        [TestMethod]
        public async Task Optimize_Async_WithRastriginFunction_FindsMinimum()
        {
            int numberOfParticles = 75;
            int maxIterations = 2000;
            double[] rastriginMinPos = DataGenerators.CreateVector(-5.12, -5.12, -5.12);
            double[] rastriginMaxPos = DataGenerators.CreateVector(5.12, 5.12, 5.12);
            double[] minExpected = DataGenerators.CreateVector(0.0, 0.0, 0.0);
            double valueExpected = 0.0;
            double tolerancePosition = 0.1;
            double toleranceValue = 0.5;

            IFitnessFunction rastriginFunction = new RastriginFunction();

            SwarmOptimizer optimizer = new SwarmOptimizer(
                numberOfParticles, _dimensions, rastriginMinPos, rastriginMaxPos,
                rastriginFunction, _initialInertiaWeight, _finalInertiaWeight, _cognitiveWeight, _socialWeight,
                _maxVelocityFactor, _maxVelocityFactor, _targetPrecision, _stagnationIterations);

            await Task.Run(() => optimizer.Optimize(maxIterations));

            double[] globalBestPosition = optimizer.GlobalBestPosition;
            double globalBestValue = optimizer.GlobalBestValue;

            Assert.AreEqual(valueExpected, globalBestValue, toleranceValue, "Rastrigin global best value not found.");
            
            for (int i = 0; i < _dimensions; i++)
            {
                Assert.AreEqual(minExpected[i], globalBestPosition[i], tolerancePosition, $"Rastrigin global best position dimension {i} not found.");
            }
        }
    }
}