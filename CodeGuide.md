This is a comprehensive guide to the **SwarmCPU** library, a C\# implementation of the Particle Swarm Optimization (PSO) algorithm. This guide will walk you through the architecture of the library, explain how each component works, and provide instructions on how to use it for your own optimization problems.

## Introduction to SwarmCPU

**SwarmCPU** is a lightweight, thread-safe .NET library designed to solve optimization problems using Particle Swarm Optimization. PSO is a computational method that optimizes a problem by iteratively trying to improve a candidate solution with regard to a given measure of quality. It's inspired by the social behavior of bird flocking or fish schooling. The library is built targeting `.NET 9.0`, making use of modern C\# features for performance and robustness.

The core idea is to have a "swarm" of "particles" in a search space. Each particle represents a potential solution to the problem. The particles move through the search space, influenced by their own best-known position and the best-known position of the entire swarm. Over time, the particles tend to converge towards the best solution.

This library is particularly useful for problems where the search space is large, complex, and the objective function is non-differentiable.

-----

## Core Components

The library is logically divided into several key components:

  * **`VectorMath<T>`:** A generic static class providing basic vector operations.
  * **`IFitnessFunction`:** An interface that defines the objective function to be optimized.
  * **`Particle`:** A class representing a single particle in the swarm.
  * **`Swarm`:** A class that manages the collection of particles.
  * **`SwarmOptimizer`:** The main engine that orchestrates the optimization process.

Let's dive into each of these components.

-----

### `VectorMath<T>`

This is a generic helper class located in `SwarmCPU.Helpers` that provides fundamental mathematical operations for vectors (represented as arrays `T[]`). It uses the `IFloatingPointIeee754<T>` interface, which means it can work with any standard floating-point type like `float`, `double`, or `Half`.

#### How It Works

The class provides static methods for:

  * **`Add(vec1, vec2)`**: Performs element-wise addition of two vectors.
  * **`Subtract(vec1, vec2)`**: Performs element-wise subtraction.
  * **`Multiply(vec, scalar)`**: Multiplies each element of a vector by a scalar value.
  * **`DotProduct(vec1, vec2)`**: Calculates the dot product of two vectors.
  * **`Magnitude(vec)`**: Computes the Euclidean norm or magnitude of a vector.
  * **`Copy(vec)`**: Creates a deep copy of a vector.

All methods include null and dimension-mismatch checks to ensure robustness, returning an empty array or `T.Zero` in case of invalid input.

```csharp
// Example Usage
double[] v1 = { 1.0, 2.0, 3.0 };
double[] v2 = { 4.0, 5.0, 6.0 };

double[] sum = VectorMath<double>.Add(v1, v2); // Result: { 5.0, 7.0, 9.0 }
double[] scaled = VectorMath<double>.Multiply(v1, 2.0); // Result: { 2.0, 4.0, 6.0 }
```

-----

### `IFitnessFunction`

This interface, found in `SwarmCPU.Interfaces`, is the cornerstone of any optimization problem you want to solve. It defines the contract for your objective function.

#### How It Works

The interface has a single method:

  * **`Evaluate(double[] position)`**: This method takes a particle's position (an array of `double`) as input and returns a `double` representing the "fitness" of that position.

In the context of PSO, "fitness" is a measure of how good a potential solution is. The goal of the optimizer is to **minimize** this value. Therefore, a lower return value from `Evaluate` means a better solution.

#### How to Use It

You must create your own class that implements this interface. This is where you define the specific problem you want to solve.

For example, to find the minimum of the Sphere function ($f(x, y) = x^2 + y^2$), you would implement it like this:

```csharp
using SwarmCPU.Interfaces;
using System;

public class SphereFunction : IFitnessFunction
{
    public double Evaluate(double[] position)
    {
        if (position == null || position.Length != 2)
        {
            throw new ArgumentException("Position must have 2 dimensions.");
        }
        double x = position[0];
        double y = position[1];
        return x * x + y * y; // f(x, y) = x^2 + y^2
    }
}
```

-----

### `Particle`

The `Particle` class in `SwarmCPU.Models` represents an individual agent in the search space.

#### How It Works

Each particle maintains its state, which includes:

  * **`Position`**: Its current coordinates in the search space (`double[]`).
  * **`Velocity`**: The direction and speed of its movement (`double[]`).
  * **`PersonalBestPosition`**: The best position it has found so far (`double[]`).
  * **`PersonalBestValue`**: The fitness value at its personal best position.

The constructor `Particle(int dimensions, double[] minPosition, double[] maxPosition, Random random)` initializes the particle with a random position and a small random velocity within the specified boundaries. It includes robust checks to handle invalid parameters (e.g., zero dimensions or mismatched boundary arrays) by creating an "empty" particle.

The `UpdateState(double[] newPosition, double[] newVelocity)` method is called by the `SwarmOptimizer` during each iteration to update the particle's position and velocity.

-----

### `Swarm`

The `Swarm` class in `SwarmCPU.Models` is a container for a collection of `Particle` objects.

#### How It Works

Its primary role is to initialize and hold the population of particles. The constructor `Swarm(int numberOfParticles, int dimensions, ...)` creates the specified number of particles, each initialized according to the provided dimensions and search space boundaries.

The class exposes a read-only list of its particles via the `Particles` property. This prevents direct modification of the particle list from outside the library's core logic, ensuring stability. Like the `Particle` class, it performs sanity checks on its constructor arguments to avoid runtime errors from invalid configurations.

-----

### `SwarmOptimizer`

This is the central class in `SwarmCPU.Optimizers` that brings everything together and runs the PSO algorithm.

#### How It Works

1.  **Initialization**: The `SwarmOptimizer` is initialized with the problem parameters: number of particles, dimensions, search space boundaries (`minPosition`, `maxPosition`), and an implementation of `IFitnessFunction`. It also sets several PSO-specific parameters:

      * **Inertia Weight**: Controls the influence of the particle's previous velocity. It linearly decreases from an `initial` to a `final` value, encouraging exploration at the start and exploitation towards the end.
      * **Cognitive Acceleration**: The "personal" learning factor, pulling the particle towards its own best-found position.
      * **Social Acceleration**: The "social" learning factor, pulling the particle towards the swarm's best-found position.
      * **Velocity Factor**: Restricts the maximum velocity of a particle to a fraction of the search space range, preventing particles from flying out of bounds. This also decreases over iterations.

    The optimizer provides two constructors: a simple one with recommended default parameters and a detailed one for full customization.

2.  **Global Best**: It keeps track of the `GlobalBestPosition` (the best position found by any particle in the swarm) and the corresponding `GlobalBestValue`.

3.  **Optimization Loop**: The `Optimize(int maxIterations)` method is the main entry point to run the algorithm. Inside this method, it iterates up to `maxIterations`:

      * **Parallel Processing**: It uses `Parallel.ForEach` to update each particle simultaneously, leveraging multiple CPU cores for significant speedup.
      * **Thread Safety**: It uses a `ThreadLocal<Random>` to ensure that random number generation is thread-safe and doesn't become a bottleneck. The update to the global best position is protected by a `lock` to prevent race conditions.
      * **Update Equations**: For each particle, it calculates a new velocity based on its current velocity (inertia), its personal best position (cognitive component), and the global best position (social component).
        $$v_{i}(t+1) = w \cdot v_{i}(t) + c_1 \cdot r_1 \cdot (p_{best,i} - x_i(t)) + c_2 \cdot r_2 \cdot (g_{best} - x_i(t))$$
        It then updates the particle's position:
        $$x_i(t+1) = x_i(t) + v_i(t+1)$$
      * **Boundary Clamping**: After calculating the new position and velocity, it clamps them to ensure they stay within the defined search space and do not exceed the maximum velocity.
      * **Fitness Evaluation**: It evaluates the fitness of the particle's new position using the provided `IFitnessFunction`.
      * **Best Updates**: It updates the particle's personal best and the swarm's global best if the new position is better.
      * **Convergence Check**: The optimizer includes a convergence criterion. If the global best value does not improve by a `_targetPrecision` for a specified number of `_stagnationIterations`, the optimization loop will terminate early.

#### How to Use It

Using the `SwarmOptimizer` involves three simple steps:

1.  **Implement the Fitness Function**: Create a class that implements `IFitnessFunction` for your specific problem.
2.  **Instantiate the Optimizer**: Create an instance of `SwarmOptimizer` and provide it with your fitness function and problem parameters.
3.  **Run the Optimization**: Call the `Optimize()` method.
4.  **Get the Results**: Retrieve the best solution from the `GlobalBestPosition` and `GlobalBestValue` properties.

### Complete Usage Example

Let's put it all together to find the minimum of the 2D Sphere function, which is at (0, 0) with a value of 0.

```csharp
using SwarmCPU.Interfaces;
using SwarmCPU.Optimizers;
using System;

// 1. Define the fitness function for the Sphere problem
public class SphereFunction : IFitnessFunction
{
    public double Evaluate(double[] position)
    {
        // f(x, y) = x^2 + y^2
        double x = position[0];
        double y = position[1];
        return x * x + y * y;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // 2. Set up the optimization problem
        int numberOfParticles = 100;
        int dimensions = 2;
        double[] minPosition = { -10.0, -10.0 }; // Search space from -10 to 10 in each dimension
        double[] maxPosition = { 10.0, 10.0 };
        IFitnessFunction fitnessFunction = new SphereFunction();

        // 3. Instantiate the SwarmOptimizer
        // Using the constructor with recommended default parameters
        var optimizer = new SwarmOptimizer(
            numberOfParticles,
            dimensions,
            minPosition,
            maxPosition,
            fitnessFunction
        );

        // 4. Run the optimization
        int maxIterations = 200;
        Console.WriteLine("Starting optimization...");
        optimizer.Optimize(maxIterations);
        Console.WriteLine("Optimization finished.");

        // 5. Retrieve and display the results
        double[] bestPosition = optimizer.GlobalBestPosition;
        double bestValue = optimizer.GlobalBestValue;

        Console.WriteLine("\n--- Results ---");
        Console.WriteLine($"Best Position Found: ({string.Join(", ", bestPosition)})");
        Console.WriteLine($"Best Fitness Value: {bestValue}");

        // Expected output will be very close to (0, 0) with a value near 0.
    }
}
```