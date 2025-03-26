using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYLibs.gyutils.random
{
    public class MersenneTwister
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0df; // constant vector a
        private const uint UPPER_MASK = 0x80000000; // most significant w-r bits
        private const uint LOWER_MASK = 0x7fffffff; // least significant r bits

        private uint[] mt = new uint[N]; // the array for the state vector
        private int mti = N + 1; // mti==N+1 means mt[N] is not initialized

        public MersenneTwister(uint seed)
        {
            InitGenrand(seed);
        }

        private void InitGenrand(uint s)
        {
            mt[0] = s & 0xffffffff;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (uint)mti);
                mt[mti] &= 0xffffffff; // for >32 bit machines
            }
        }

        public uint GenrandInt32()
        {
            uint y;
            uint[] mag01 = new uint[] { 0, MATRIX_A };

            if (mti >= N)
            {
                for (int kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                for (int kk = N - M; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[0] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];
                mti = 0;
            }

            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);
            return y & 0xffffffff;
        }

        public int Next(int min, int max)
        {
            return (int)(GenrandInt32() % (max - min)) + min;
        }
    }
}
