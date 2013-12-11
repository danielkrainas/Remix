namespace Atlana
{
    using System;
    
    /// <summary>
    /// The random number generator class for the mud.
    /// </summary>
    /// <remarks>
    /// The Randomizer class is a singleton class therefore cannot be instanced.
    /// </remarks>
    /// <example>
    /// Gain access to the singleton instance of the Randomizer class.
    /// <code>
    /// Atlana.Random.Randomizer randomizer = Atlana.Random.Randomizer.Instance;
    /// </code>
    /// Generate a random number between the values of 1 and 100.
    /// <code>
    /// int i = Atlana.Random.Randomizer.Range(1, 100);
    /// </code>
    /// </example>
    public sealed class Randomizer
    {
        /// <summary>
        /// The singleton instance of the <see cref="Atlana.Random.Randomizer">Randomizer class</see>.
        /// </summary>
        private static Randomizer instance = new Randomizer(DateTime.Now.Second);

        /// <summary>
        /// An array of unsigned integers containing 100 pre-rolled randomly generated numbers.
        /// </summary>
        private uint[] randBuf = new uint[100];

        /// <summary>
        /// The index of the last read number out of the <see cref="Atlana.Random.Randomizer.randBuf">randBuf</see> array.
        /// </summary>
        private uint index = 0;

        /// <summary>
        /// Original value of the seed used to initialize the number array.
        /// </summary>
        private uint origSeed;

        /// <summary>
        /// Initializes a new instance of the Randomizer class. Uses <paramref name="seed">seed</paramref> to initialize the number array.
        /// </summary>
        /// <param name="seed">Integer value used to seed the number array.</param>
        private Randomizer(int seed)
        {
            this.origSeed = (uint)seed;
            this.Initialize((uint)seed);
        }

        /// <summary>
        /// Gets the singleton instance of the Randomizer class.
        /// </summary>
        public static Randomizer Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Invokes the randomizer to refresh the list of random values.
        /// </summary>
        public void ReRoll()
        {
            this.Roll();
        }

        /// <summary>
        /// Generates a random unsigned integer number value between the specified range of <paramref name="min">min</paramref> and <paramref name="max">max</paramref>.
        /// </summary>
        /// <param name="min">An unsigned integer representing the minimum value of the return value.</param>
        /// <param name="max">An unsigned integer representing the maximum value of the return value.</param>
        /// <returns>An unsigned integer containing a number between <paramref name="min">min</paramref> and <paramref name="max">max</paramref>.</returns>
        public uint Range(uint min, uint max)
        {
            uint y = this.Random();
            if (y < min)
            {
                y = y * min;
            }

            if (y > max)
            {
                y = y % (max + 1);
            }

            return y;
        }

        /// <summary>
        /// Generates a random integer number value between the specified range of <paramref name="min">min</paramref> and <paramref name="max">max</paramref>.
        /// </summary>
        /// <param name="min">An integer representing the minimum value of the return value.</param>
        /// <param name="max">An integer representing the maximum value of the return value.</param>
        /// <returns>An integer containing a number between <paramref name="min">min</paramref> and <paramref name="max">max</paramref>.</returns>
        public int Range(int min, int max)
        {
            return Convert.ToInt32(this.Range(Convert.ToUInt32(min), Convert.ToUInt32(max)));
        }

        /// <summary>
        /// Generates a random number between 0 and 100.
        /// </summary>
        /// <returns>A randomly generated integer value between 0 and 100.</returns>
        public int Percent()
        {
            return this.Range(0, 100);
        }

        /// <summary>
        /// Retrieves a value from the random number array.
        /// </summary>
        /// <returns>A randomly generated unsigned integer</returns>
        private uint Random()
        {
            if (this.index == 0)
            {
                this.Roll();
            }

            uint y = this.randBuf[this.index];
            y = y ^ (y >> 11);
            y = y ^ ((y << 7) + 3794);
            y = y ^ ((y << 15) + 815);
            y = y ^ (y >> 18);
            this.index = (this.index + 1) % 100;
            return y;
        }

        /// <summary>
        /// Initializes the number array from a seed provided by <paramref name="seed">seed</paramref>.
        /// </summary>
        /// <param name="seed">Unsigned integer value used to seed the number array.</param>
        private void Initialize(uint seed)
        {
            this.randBuf[0] = seed;
            for (uint i = 1; i < 100; i++)
            {
                this.randBuf[i] = (uint)(this.randBuf[i - 1] >> 1) + i;
            }
        }

        /// <summary>
        /// Checks to prevent <see cref="System.ArithmeticException">ArithmeticException</see>.
        /// </summary>
        private void OverflowCheck()
        {
            foreach (uint u in this.randBuf)
            {
                if (u > (uint.MaxValue / 3794))
                {
                    this.Initialize(this.origSeed + this.index);
                    break;
                }
            }
        }

        /// <summary>
        /// Refreshes the list of values in the random number array.
        /// </summary>
        private void Roll()
        {
            this.OverflowCheck();
            for (uint i = 0; i < 99; i++)
            {
                uint y = this.randBuf[i + 1] * 3794U;
                this.randBuf[i] = (((y >> 10) + this.randBuf[i]) ^ this.randBuf[(i + 399) % 100]) + i;
                if ((this.randBuf[i] % 2) == 1)
                {
                    this.randBuf[i] = (this.randBuf[i + 1] << 21) ^ (this.randBuf[i + 1] * (this.randBuf[i + 1] & 30));
                }
            }
        }
    }
}
