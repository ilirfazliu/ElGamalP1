using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace P1ElGamal
{
    public class ElGamalParameters : AsymmetricAlgorithm
    {
            public byte[] P { get; set; }
            public byte[] G { get; set; }
            public byte[] Y { get; set; }
            public byte[] X { get; set; }

    }
}
