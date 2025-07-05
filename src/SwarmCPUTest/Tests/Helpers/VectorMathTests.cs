using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmCPU.Helpers;
using SwarmCPUTest.Helpers; // For DataGenerators
using System;

namespace SwarmCPUTest.Tests.Helpers
{
    [TestClass]
    public class VectorMathTests
    {
        [TestMethod]
        public void Add_TwoVectors_ReturnsCorrectSum()
        {
            double[] vec1 = new double[] { 1.0, 2.0, 3.0 };
            double[] vec2 = new double[] { 4.0, 5.0, 6.0 };
            double[] expected = new double[] { 5.0, 7.0, 9.0 };

            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Vectors were not added correctly.");
        }

        [TestMethod]
        public void Add_ZeroVectors_ReturnsZeroVector()
        {
            double[] vec1 = new double[] { 0.0, 0.0 };
            double[] vec2 = new double[] { 0.0, 0.0 };
            double[] expected = new double[] { 0.0, 0.0 };

            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Adding zero vectors did not return zero vector.");
        }

        [TestMethod]
        public void Add_MixedPositiveNegative_ReturnsCorrectSum()
        {
            double[] vec1 = new double[] { 1.0, -2.0, 3.0 };
            double[] vec2 = new double[] { -4.0, 5.0, -6.0 };
            double[] expected = new double[] { -3.0, 3.0, -3.0 };

            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Adding mixed positive/negative vectors failed.");
        }

        [TestMethod]
        public void Add_Then_Subtract_ReturnsOriginalVector()
        {
            double[] vec1 = new double[] { 1.0, 2.0, 3.0 };
            double[] vec2 = new double[] { 4.0, 5.0, 6.0 };
            
            double[] sum = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(sum);
            double[] result = VectorMath<double>.Subtract(sum, vec2);
            Assert.IsNotNull(result);

            CollectionAssert.AreEqual(vec1, result, "Add followed by Subtract did not return original vector.");
        }

        [TestMethod]
        public void Subtract_TwoVectors_ReturnsCorrectDifference()
        {
            double[] vec1 = new double[] { 5.0, 7.0, 9.0 };
            double[] vec2 = new double[] { 4.0, 5.0, 6.0 };
            double[] expected = new double[] { 1.0, 2.0, 3.0 };

            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Vectors were not subtracted correctly.");
        }

        [TestMethod]
        public void Subtract_IdenticalVectors_ReturnsZeroVector()
        {
            double[] vec1 = new double[] { 1.0, 2.0, 3.0 };
            double[] vec2 = new double[] { 1.0, 2.0, 3.0 };
            double[] expected = new double[] { 0.0, 0.0, 0.0 };

            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Subtracting identical vectors did not return zero vector.");
        }

        [TestMethod]
        public void Multiply_VectorByPositiveScalar_ReturnsCorrectProduct()
        {
            double[] vec = new double[] { 1.0, 2.0, 3.0 };
            double scalar = 2.0;
            double[] expected = new double[] { 2.0, 4.0, 6.0 };

            double[] actual = VectorMath<double>.Multiply(vec, scalar);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Vector was not multiplied by positive scalar correctly.");
        }

        [TestMethod]
        public void Multiply_VectorByNegativeScalar_ReturnsCorrectProduct()
        {
            double[] vec = new double[] { 1.0, 2.0, 3.0 };
            double scalar = -2.0;
            double[] expected = new double[] { -2.0, -4.0, -6.0 };

            double[] actual = VectorMath<double>.Multiply(vec, scalar);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Vector was not multiplied by negative scalar correctly.");
        }

        [TestMethod]
        public void Multiply_VectorByZeroScalar_ReturnsZeroVector()
        {
            double[] vec = new double[] { 1.0, 2.0, 3.0 };
            double scalar = 0.0;
            double[] expected = new double[] { 0.0, 0.0, 0.0 };

            double[] actual = VectorMath<double>.Multiply(vec, scalar);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expected, actual, "Vector was not multiplied by zero scalar correctly.");
        }

        [TestMethod]
        public void Copy_Vector_ReturnsNewInstanceWithSameContent()
        {
            double[] original = new double[] { 1.0, 2.0, 3.0 };

            double[] copy = VectorMath<double>.Copy(original);

            Assert.IsNotNull(copy);
            Assert.AreNotSame(original, copy, "Copy should return a new array instance.");
            CollectionAssert.AreEqual(original, copy, "Copied array content mismatch.");
        }

        [TestMethod]
        public void Copy_EmptyVector_ReturnsEmptyVector()
        {
            double[] original = new double[] { };
            double[] expected = new double[] { };

            double[] copy = VectorMath<double>.Copy(original);

            Assert.IsNotNull(copy);
            Assert.AreNotSame(original, copy, "Copy should return a new array instance for empty vector.");
            CollectionAssert.AreEqual(expected, copy, "Copied empty array content mismatch.");
        }

        // Modified tests to reflect non-throwing behavior
        [TestMethod]
        public void Add_NullVector1_ReturnsEmptyArray()
        {
            double[] vec1 = null!;
            double[] vec2 = new double[] { 1.0, 2.0 };
            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Add with null vector1 should return an empty array.");
        }

        [TestMethod]
        public void Add_NullVector2_ReturnsEmptyArray()
        {
            double[] vec1 = new double[] { 1.0, 2.0 };
            double[] vec2 = null!;
            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Add with null vector2 should return an empty array.");
        }

        [TestMethod]
        public void Add_MismatchedLength_ReturnsEmptyArray()
        {
            double[] vec1 = new double[] { 1.0, 2.0 };
            double[] vec2 = new double[] { 1.0, 2.0, 3.0 };
            double[] actual = VectorMath<double>.Add(vec1, vec2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Add with mismatched length vectors should return an empty array.");
        }

        [TestMethod]
        public void Subtract_NullVector1_ReturnsEmptyArray()
        {
            double[] vec1 = null!;
            double[] vec2 = new double[] { 1.0, 2.0 };
            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Subtract with null vector1 should return an empty array.");
        }

        [TestMethod]
        public void Subtract_MismatchedLength_ReturnsEmptyArray()
        {
            double[] vec1 = new double[] { 1.0, 2.0 };
            double[] vec2 = new double[] { 1.0, 2.0, 3.0 };
            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Subtract with mismatched length vectors should return an empty array.");
        }

        [TestMethod]
        public void Multiply_NullVector_ReturnsEmptyArray()
        {
            double[] vec = null!;
            double[] actual = VectorMath<double>.Multiply(vec, 5.0);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Multiply with null vector should return an empty array.");
        }

        [TestMethod]
        public void Copy_NullVector_ReturnsEmptyArray()
        {
            double[] vec = null!;
            double[] actual = VectorMath<double>.Copy(vec);
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length, "Copy with null vector should return an empty array.");
        }

        [TestMethod]
        public void DotProduct_StandardVectors_ReturnsCorrectValue()
        {
            double[] vec1 = new double[] { 1.0, 2.0, 3.0 };
            double[] vec2 = new double[] { 4.0, 5.0, 6.0 };
            double expected = 1.0 * 4.0 + 2.0 * 5.0 + 3.0 * 6.0; // 4 + 10 + 18 = 32
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(expected, actual, 1e-9, "Dot product is incorrect.");
        }

        [TestMethod]
        public void DotProduct_OrthogonalVectors_ReturnsZero()
        {
            double[] vec1 = new double[] { 1.0, 0.0 };
            double[] vec2 = new double[] { 0.0, 1.0 };
            double expected = 0.0;
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(expected, actual, 1e-9, "Dot product of orthogonal vectors should be zero.");
        }

        [TestMethod]
        public void DotProduct_EmptyVectors_ReturnsZero()
        {
            double[] vec1 = new double[] { };
            double[] vec2 = new double[] { };
            double expected = 0.0;
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(expected, actual, 1e-9, "Dot product of empty vectors should be zero.");
        }

        [TestMethod]
        public void DotProduct_NullVector1_ReturnsZero()
        {
            double[] vec1 = null!;
            double[] vec2 = new double[] { 1.0, 2.0 };
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(0.0, actual, "Dot product with null vector1 should return zero.");
        }

        [TestMethod]
        public void DotProduct_MismatchedLength_ReturnsZero()
        {
            double[] vec1 = new double[] { 1.0, 2.0 };
            double[] vec2 = new double[] { 1.0, 2.0, 3.0 };
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(0.0, actual, "Dot product with mismatched length vectors should return zero.");
        }

        [TestMethod]
        public void Magnitude_StandardVector_ReturnsCorrectValue()
        {
            double[] vec = new double[] { 3.0, 4.0 };
            double expected = 5.0; // Sqrt(3*3 + 4*4) = Sqrt(9+16) = Sqrt(25) = 5
            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(expected, actual, 1e-9, "Magnitude is incorrect.");
        }

        [TestMethod]
        public void Magnitude_ZeroVector_ReturnsZero()
        {
            double[] vec = new double[] { 0.0, 0.0, 0.0 };
            double expected = 0.0;
            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(expected, actual, 1e-9, "Magnitude of zero vector should be zero.");
        }

        [TestMethod]
        public void Magnitude_EmptyVector_ReturnsZero()
        {
            double[] vec = new double[] { };
            double expected = 0.0;
            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(expected, actual, 1e-9, "Magnitude of empty vector should be zero.");
        }

        [TestMethod]
        public void Magnitude_NullVector_ReturnsZero()
        {
            double[] vec = null!;
            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(0.0, actual, "Magnitude with null vector should return zero.");
        }

        [TestMethod]
        public void Add_SingleElementVectors_ReturnsCorrectSum()
        {
            double[] vec1 = DataGenerators.CreateVector(5.0);
            double[] vec2 = DataGenerators.CreateVector(3.0);
            double[] expected = DataGenerators.CreateVector(8.0);
            double[] actual = VectorMath<double>.Add(vec1, vec2);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Subtract_SingleElementVectors_ReturnsCorrectDifference()
        {
            double[] vec1 = DataGenerators.CreateVector(5.0);
            double[] vec2 = DataGenerators.CreateVector(3.0);
            double[] expected = DataGenerators.CreateVector(2.0);
            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiply_SingleElementVector_ReturnsCorrectProduct()
        {
            double[] vec = DataGenerators.CreateVector(5.0);
            double scalar = 2.0;
            double[] expected = DataGenerators.CreateVector(10.0);
            double[] actual = VectorMath<double>.Multiply(vec, scalar);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DotProduct_SingleElementVectors_ReturnsCorrectValue()
        {
            double[] vec1 = DataGenerators.CreateVector(5.0);
            double[] vec2 = DataGenerators.CreateVector(3.0);
            double expected = 15.0;
            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Magnitude_SingleElementVector_ReturnsCorrectValue()
        {
            double[] vec = DataGenerators.CreateVector(-5.0);
            double expected = 5.0; 
            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Add_LargeDimensionVectors_ReturnsCorrectSum()
        {
            int dimensions = 1000;
            Random rng = new Random(1);
            double[] vec1 = DataGenerators.GenerateRandomVector(dimensions, -100.0, 100.0, rng);
            double[] vec2 = DataGenerators.GenerateRandomVector(dimensions, -100.0, 100.0, rng);
            double[] expected = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                expected[i] = vec1[i] + vec2[i];
            }

            double[] actual = VectorMath<double>.Add(vec1, vec2);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Subtract_LargeDimensionVectors_ReturnsCorrectDifference()
        {
            int dimensions = 1000;
            Random rng = new Random(2);
            double[] vec1 = DataGenerators.GenerateRandomVector(dimensions, -100.0, 100.0, rng);
            double[] vec2 = DataGenerators.GenerateRandomVector(dimensions, -100.0, 100.0, rng);
            double[] expected = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                expected[i] = vec1[i] - vec2[i];
            }

            double[] actual = VectorMath<double>.Subtract(vec1, vec2);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiply_LargeDimensionVector_ReturnsCorrectProduct()
        {
            int dimensions = 1000;
            Random rng = new Random(3);
            double[] vec = DataGenerators.GenerateRandomVector(dimensions, -100.0, 100.0, rng);
            double scalar = rng.NextDouble() * 10 - 5; 
            double[] expected = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                expected[i] = vec[i] * scalar;
            }

            double[] actual = VectorMath<double>.Multiply(vec, scalar);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DotProduct_LargeDimensionVectors_ReturnsCorrectValue()
        {
            int dimensions = 1000;
            Random rng = new Random(4);
            double[] vec1 = DataGenerators.GenerateRandomVector(dimensions, -1.0, 1.0, rng);
            double[] vec2 = DataGenerators.GenerateRandomVector(dimensions, -1.0, 1.0, rng);
            double expected = 0.0;
            for (int i = 0; i < dimensions; i++)
            {
                expected += vec1[i] * vec2[i];
            }

            double actual = VectorMath<double>.DotProduct(vec1, vec2);
            Assert.AreEqual(expected, actual, 1e-9);
        }

        [TestMethod]
        public void Magnitude_LargeDimensionVector_ReturnsCorrectValue()
        {
            int dimensions = 1000;
            Random rng = new Random(5);
            double[] vec = DataGenerators.GenerateRandomVector(dimensions, -1.0, 1.0, rng);
            double expectedSumOfSquares = 0.0;
            for (int i = 0; i < dimensions; i++)
            {
                expectedSumOfSquares += vec[i] * vec[i];
            }
            double expected = Math.Sqrt(expectedSumOfSquares);

            double actual = VectorMath<double>.Magnitude(vec);
            Assert.AreEqual(expected, actual, 1e-9);
        }
    }
}