using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Security.Cryptography;

namespace P1ElGamal
{
    public abstract class ImplementationClass : ImpExpElGamalParameters
    {
        public struct ElGamalKeyStruct
        {
            public BigInteger P;
            public BigInteger G;
            public BigInteger Y;
            public BigInteger X;
        }

        public void s() { }   
     
        private ElGamalKeyStruct current_key;
        // maximum length of the BigInteger in uint (4 bytes)
        // change this to suit the required level of precision.
        private const int maxLength = 70;
        private uint[] data = null;            // stores bytes from the Big Integer
        public int dataLength;                 // number of actual chars used


        public ImplementationClass()
        {
            // create the key struct
            current_key = new ElGamalKeyStruct();
            // set all of the big integers to zero
            current_key.P = new BigInteger(0);
            current_key.G = new BigInteger(0);
            current_key.Y = new BigInteger(0);
            current_key.X = new BigInteger(0);

            /// Default constructor for BigInteger of value 0
            data = new uint[maxLength];
            dataLength = 1;
        }
        
         
        private void CreateKeyPair(int keyStrength)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                // create the large prime number, P
                current_key.P = current_key.P.genPseudoPrime(keyStrength, 16, rng);

                // create the two random numbers, which are smaller than P
                current_key.X = new BigInteger();
                current_key.X = current_key.X.genRandomBits(keyStrength - 1, rng);
                current_key.G = new BigInteger();
                current_key.G = current_key.G.genRandomBits(keyStrength - 1, rng);

                // compute Y
                current_key.Y = BigInteger.ModPow(current_key.G, current_key.X, current_key.P);
            }

        }

    }
}

