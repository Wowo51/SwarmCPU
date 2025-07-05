using SwarmCPU.Models;
using SwarmCPU.Interfaces;
using SwarmCPU.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks; // For Parallel.ForEach
using System.Threading; // For lock, ThreadLocal
using System; // For Random, Guid

namespace SwarmCPU.Optimizers
{
    public class SwarmOptimizer
    {
        private Swarm _swarm;
        private IFitnessFunction _fitnessFunction;
        private int _dimensions;
        private double[] _minPosition;
        private double[] _maxPosition;

        // PSO Parameters
        private double _initialInertiaWeight;
        private double _finalInertiaWeight;
        private double _cognitiveAcceleration;
        private double _socialAcceleration;
        private double _initialMaxVelocityFactor; // Controls the maximum proportion of range for velocity clamping initially
        private double _finalMaxVelocityFactor;   // Controls the maximum proportion of range for velocity clamping finally

        private double[] _globalBestPosition;
        private double _globalBestValue;
        private readonly object _globalBestLock = new object();
        private Random _initializationRandom = new Random();
        private static ThreadLocal<Random> _threadRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode() ^ Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));

        // New fields for convergence criteria
        private double _targetPrecision;
        private int _stagnationIterations;

        /// <summary>
        /// Initializes a new instance of the SwarmOptimizer with recommended default PSO parameters
        /// for better convergence and exploration-exploitation balance, while maintaining configurability
        /// for problem-dependent parameters.
        /// </summary>
        /// <param name="numberOfParticles">The number of particles in the swarm.</param>
        /// <param name="dimensions">The number of dimensions for the optimization problem.</param>
        /// <param name="minPosition">An array defining the minimum bounds for each dimension.</param>
        /// <param name="maxPosition">An array defining the maximum bounds for each dimension.</param>
        /// <param name="fitnessFunction">The fitness function to evaluate particle positions.</param>
        public SwarmOptimizer(
            int numberOfParticles,
            int dimensions,
            double[] minPosition,
            double[] maxPosition,
            IFitnessFunction fitnessFunction)
            : this(
                numberOfParticles,
                dimensions,
                minPosition,
                maxPosition,
                fitnessFunction,
                0.9,                           // Recommended initial inertia weight
                0.4,                           // Recommended final inertia weight
                2.0,                           // Recommended cognitive acceleration
                2.0,                           // Recommended social acceleration
                0.5,                           // Initial max velocity factor (retaining current reasonable value)
                0.1,                           // Final max velocity factor (retaining current reasonable value)
                1E-05,                         // Target precision for convergence (from specification)
                50)                            // Recommended stagnation iterations
        {
        }

        public SwarmOptimizer(
            int numberOfParticles,
            int dimensions,
            double[] minPosition,
            double[] maxPosition,
            IFitnessFunction fitnessFunction,
            double initialInertiaWeight,
            double finalInertiaWeight,
            double cognitiveAcceleration,
            double socialAcceleration,
            double initialMaxVelocityFactor,
            double finalMaxVelocityFactor,
            double targetPrecision,
            int stagnationIterations)
        {
            _initialInertiaWeight = initialInertiaWeight;
            _finalInertiaWeight = finalInertiaWeight;
            _cognitiveAcceleration = cognitiveAcceleration;
            _socialAcceleration = socialAcceleration;
            _initialMaxVelocityFactor = initialMaxVelocityFactor;
            _finalMaxVelocityFactor = finalMaxVelocityFactor;
            _targetPrecision = targetPrecision;
            _stagnationIterations = stagnationIterations;

            _globalBestValue = double.MaxValue; // Always initialize to max value

            int effectiveDimensions = Math.Max(0, dimensions);
            int actualNumberOfParticles = Math.Max(0, numberOfParticles);

            // Validate fitness function
            if (fitnessFunction is null)
            {
                _fitnessFunction = new DelegateFitnessFunction((double[] pos) => double.MaxValue);
            }
            else
            {
                _fitnessFunction = fitnessFunction;
            }

            // Determine final _dimensions and bounds, handling invalid scenarios.
            if (effectiveDimensions > 0 &&
                (minPosition is null || maxPosition is null || minPosition.Length != effectiveDimensions || maxPosition.Length != effectiveDimensions))
            {
                // If dimensions are non-zero but bounds are invalid, reset _dimensions to 0.
                _dimensions = 0;
                _minPosition = Array.Empty<double>();
                _maxPosition = Array.Empty<double>();
            }
            else
            {
                // Otherwise, use effectiveDimensions and provided bounds.
                _dimensions = effectiveDimensions;
                _minPosition = minPosition;
                _maxPosition = maxPosition;

                // Ensure min/max positions are correctly ordered for each dimension.
                // This improves robustness if user provides inverted bounds.
                for (int i = 0; i < _dimensions; i++)
                {
                    if (_minPosition[i] > _maxPosition[i])
                    {
                        double temp = _minPosition[i];
                        _minPosition[i] = _maxPosition[i];
                        _maxPosition[i] = temp;
                    }
                }
            }

            // Initialize _globalBestPosition based on the FINAL _dimensions and number of particles.
            // If actualNumberOfParticles is 0, even with valid non-zero dimensions, there's no swarm to optimize.
            // In this case, global best should remain MaxValue and position be empty.
            if (_dimensions == 0 || actualNumberOfParticles == 0)
            {
                _globalBestPosition = Array.Empty<double>();
                // _globalBestValue remains double.MaxValue as set earlier.
            }
            else
            {
                _globalBestPosition = new double[_dimensions];
            }

            _swarm = new Swarm(actualNumberOfParticles, _dimensions, _minPosition, _maxPosition, _initializationRandom);

            InitializeGlobalBest();
        }

        public double[] GlobalBestPosition
        {
            get
            {
                return VectorMath<double>.Copy(_globalBestPosition);
            }
        }

        public double GlobalBestValue
        {
            get
            {
                return _globalBestValue;
            }
        }

        public void Optimize(int maxIterations)
        {
            if (maxIterations < 0)
            {
                return; // Do nothing if maxIterations is negative.
            }

            // If _dimensions is 0 or no particles, there is nothing to optimize.
            if (_dimensions == 0 || _swarm.Particles.Count == 0)
            {
                return;
            }

            // Initialize for convergence tracking
            double previousGlobalBestValue = double.MaxValue;
            int consecutiveStagnationCount = 0;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                double currentInertiaWeight;
                double currentMaxVelocityFactor;

                // Calculate current inertia weight and max velocity factor, assuming maxIterations is > 0
                if (maxIterations > 0)
                {
                    currentInertiaWeight = _initialInertiaWeight -
                                           (_initialInertiaWeight - _finalInertiaWeight) *
                                           ((double)iteration / maxIterations);

                    currentMaxVelocityFactor = _initialMaxVelocityFactor -
                                               (_initialMaxVelocityFactor - _finalMaxVelocityFactor) *
                                               ((double)iteration / maxIterations);
                }
                else // Avoid division by zero if maxIterations is 0
                {
                    currentInertiaWeight = _initialInertiaWeight;
                    currentMaxVelocityFactor = _initialMaxVelocityFactor;
                }

                // Parallel update of particles
                Parallel.ForEach(_swarm.Particles, (Particle particle) =>
                {
                    Random threadSafeRandom = _threadRandom.Value!;
                    double r1 = threadSafeRandom.NextDouble();
                    double r2 = threadSafeRandom.NextDouble();

                    // Calculate velocity components
                    double[] particleToPersonalBest = VectorMath<double>.Subtract(particle.PersonalBestPosition, particle.Position);
                    double[] cognitiveComponent = VectorMath<double>.Multiply(particleToPersonalBest, _cognitiveAcceleration * r1);

                    double[] particleToGlobalBest = VectorMath<double>.Subtract(_globalBestPosition, particle.Position);
                    double[] socialComponent = VectorMath<double>.Multiply(particleToGlobalBest, _socialAcceleration * r2);

                    double[] inertiaComponent = VectorMath<double>.Multiply(particle.Velocity, currentInertiaWeight);

                    double[] newVelocity = VectorMath<double>.Add(
                                            VectorMath<double>.Add(inertiaComponent, cognitiveComponent),
                                            socialComponent);

                    // Calculate new position
                    double[] newPosition = VectorMath<double>.Add(particle.Position, newVelocity);

                    // Apply position boundary constraints and velocity clamping
                    for (int i = 0; i < _dimensions; i++)
                    {
                        double dimensionRange = _maxPosition[i] - _minPosition[i];
                        // dimensionRange cannot be negative due to constructor preprocessing.

                        // Recalculate max velocity for each dimension based on its specific range and current decaying factor
                        double currentMaxVel = Math.Abs(dimensionRange) * currentMaxVelocityFactor;

                        // Clamp velocity
                        if (newVelocity[i] > currentMaxVel)
                        {
                            newVelocity[i] = currentMaxVel;
                        }
                        else if (newVelocity[i] < -currentMaxVel)
                        {
                            newVelocity[i] = -currentMaxVel;
                        }

                        // Clamp position
                        if (newPosition[i] < _minPosition[i])
                        {
                            newPosition[i] = _minPosition[i];
                        }
                        else if (newPosition[i] > _maxPosition[i])
                        {
                            newPosition[i] = _maxPosition[i];
                        }
                    }

                    // Update particle's state
                    particle.UpdateState(newPosition, newVelocity);
                    // Evaluate fitness
                    double currentFitness = _fitnessFunction.Evaluate(particle.Position);
                    // Update personal best
                    if (currentFitness < particle.PersonalBestValue)
                    {
                        particle.PersonalBestValue = currentFitness;
                        particle.PersonalBestPosition = particle.Position;
                    }

                    // Update global best (thread-safe)
                    UpdateGlobalBest(particle.Position, currentFitness);
                });

                // Check for convergence after all particles have been updated in this iteration
                lock (_globalBestLock)
                {
                    // Check if current improvement is less than target precision
                    if (Math.Abs(previousGlobalBestValue - _globalBestValue) < _targetPrecision)
                    {
                        consecutiveStagnationCount++;
                    }
                    else
                    {
                        consecutiveStagnationCount = 0; // Reset counter if significant improvement occurred
                    }

                    // Update previousGlobalBestValue for the next iteration's comparison
                    previousGlobalBestValue = _globalBestValue;
                }

                // Check for early exit due to convergence/stagnation
                if (consecutiveStagnationCount >= _stagnationIterations)
                {
                    break;
                }
            }
        }
        private void InitializeGlobalBest()
        {
            // If there are no dimensions to optimize or no particles in the swarm,
            // the global best should remain at its initial state (MaxValue for value, empty for position).
            if (_dimensions == 0 || _swarm.Particles.Count == 0)
            {
                _globalBestValue = double.MaxValue;
                _globalBestPosition = Array.Empty<double>();
                return;
            }

            _globalBestValue = double.MaxValue; // Reset for actual initialization from swarm
            foreach (Particle particle in _swarm.Particles)
            {
                double fitness = _fitnessFunction.Evaluate(particle.Position);
                particle.PersonalBestValue = fitness;
                particle.PersonalBestPosition = particle.Position;

                if (fitness < _globalBestValue)
                {
                    _globalBestValue = fitness;
                    _globalBestPosition = VectorMath<double>.Copy(particle.Position);
                }
            }
        }

        private void UpdateGlobalBest(double[] currentPosition, double currentFitness)
        {
            lock (_globalBestLock)
            {
                if (currentFitness < _globalBestValue)
                {
                    _globalBestValue = currentFitness;
                    _globalBestPosition = VectorMath<double>.Copy(currentPosition);
                }
            }
        }

        // Internal helper class for a default fitness function if none provided
        private class DelegateFitnessFunction : IFitnessFunction
        {
            private readonly Func<double[], double> _evaluateFunc;

            public DelegateFitnessFunction(Func<double[], double> evaluateFunc)
            {
                _evaluateFunc = evaluateFunc;
            }

            public double Evaluate(double[] position)
            {
                return _evaluateFunc(position);
            }
        }
    }
}