namespace SwarmCPU.Helpers
{
    using System;
    using System.Numerics; // For IFloatingPointIeee754<T>

    public static class VectorMath<T> where T : IFloatingPointIeee754<T>
    {
        public static T[] Add(T[] vec1, T[] vec2)
        {
            if (vec1 is null || vec2 is null || vec1.Length != vec2.Length)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[vec1.Length];
            for (int i = 0; i < vec1.Length; i++)
            {
                result[i] = vec1[i] + vec2[i];
            }
            return result;
        }

        public static T[] Subtract(T[] vec1, T[] vec2)
        {
            if (vec1 is null || vec2 is null || vec1.Length != vec2.Length)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[vec1.Length];
            for (int i = 0; i < vec1.Length; i++)
            {
                result[i] = vec1[i] - vec2[i];
            }
            return result;
        }

        public static T[] Multiply(T[] vec, T scalar)
        {
            if (vec is null)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[vec.Length];
            for (int i = 0; i < vec.Length; i++)
            {
                result[i] = vec[i] * scalar;
            }
            return result;
        }

        public static T DotProduct(T[] vec1, T[] vec2)
        {
            if (vec1 is null || vec2 is null || vec1.Length != vec2.Length)
            {
                return T.Zero;
            }

            T dot = T.Zero;
            for (int i = 0; i < vec1.Length; i++)
            {
                dot += vec1[i] * vec2[i];
            }
            return dot;
        }

        public static T Magnitude(T[] vec)
        {
            if (vec is null)
            {
                return T.Zero;
            }

            T sumOfSquares = T.Zero;
            for (int i = 0; i < vec.Length; i++)
            {
                sumOfSquares += vec[i] * vec[i];
            }
            return T.Sqrt(sumOfSquares);
        }

        public static T[] Copy(T[] vec)
        {
            if (vec is null)
            {
                return Array.Empty<T>();
            }
            T[] result = new T[vec.Length];
            Array.Copy(vec, result, vec.Length);
            return result;
        }
    }
}