using System;
using System.Collections.Generic;
using System.Text;

namespace P1ElGamal
{
    public class ElGamalParameters
    {
        [Serializable]
        public struct ElGamalParam
        {
            public byte[] P;
            public byte[] G;
            public byte[] Y;
            [NonSerialized] public byte[] X;
        }
    }
}
