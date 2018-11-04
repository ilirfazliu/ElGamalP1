using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Security.Cryptography;


namespace P1ElGamal
{
    public class ImplementationClass : ImpExpElGamalParameters
    {
        public struct ElGamalKeyStruct
        {
            public BigInteger P;
            public BigInteger G;
            public BigInteger Y;
            public BigInteger X;
        }

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

            KeySizeValue = 1024;
            // set the range of legal keys
            LegalKeySizesValue = new[] { new KeySizes(384, 1088, 8) };
        }

        //public override string KeyExchangeAlgorithm => "ElGamal";

        private void CreateKeyPair(int key_strength)
        {
            // create the random number generator
            Random random_number = new Random();

            // create the large prime number, P
            current_key.P = BigInteger.genPseudoPrime(key_strength,
                16, random_number);

            // create the two random numbers, which are smaller than P
            current_key.X = new BigInteger();
            current_key.X.genRandomBits(key_strength - 1, random_number);
            current_key.G = new BigInteger();
            current_key.G.genRandomBits(key_strength - 1, random_number);

            // compute Y
            current_key.Y = current_key.G.modPow(current_key.X, current_key.P);
        }

        private bool NeedToGenerateKey()
        {
            return current_key.P == 0 && current_key.G == 0 && current_key.Y == 0;
        }

        public ElGamalKeyStruct KeyStruct
        {
            get
            {
                if (NeedToGenerateKey())
                {
                    CreateKeyPair(KeySizeValue);
                }
                return current_key;
            }
            set
            {
                current_key = value;
            }

        }

        public override void ImportParameters(ElGamalParameters p_parameters)
        {
            // obtain the  big integer values from the byte parameter values
            current_key.P = new BigInteger(p_parameters.P);
            current_key.G = new BigInteger(p_parameters.G);
            current_key.Y = new BigInteger(p_parameters.Y);
            if (p_parameters.X != null && p_parameters.X.Length > 0)
            {
                current_key.X = new BigInteger(p_parameters.X);
            }
            // set the length of the key based on the import
            KeySizeValue = current_key.P.bitCount();
        }

        public override ElGamalParameters ExportParameters(bool include_private_params)
        {

            if (NeedToGenerateKey())
            {
                // we need to create a new key before we can export 
                CreateKeyPair(KeySizeValue);
            }

            ElGamalParameters elgamal_params = new ElGamalParameters();
            // set the public values of the parameters
            elgamal_params.P = current_key.P.getBytes();
            elgamal_params.G = current_key.G.getBytes();
            elgamal_params.Y = current_key.Y.getBytes();

            // if required, include the private value, X
            if (include_private_params)
            {
                elgamal_params.X = current_key.X.getBytes();
            }
            else
            {
                // ensure that we zero the value
                elgamal_params.X = new byte[1];
            }
            // return the parameter set
            return elgamal_params;
        }

        public override byte[] EncryptData(byte[] data)
        {
            if (NeedToGenerateKey())
            {
                // we need to create a new key before we can export 
                CreateKeyPair(KeySizeValue);
            }
            // encrypt the data
            ElGamalEncrypt encrypt_data = new ElGamalEncrypt(current_key);
            return encrypt_data.ProcessData(data);
        }

        public override byte[] DecryptData(byte[] p_data)
        {
            if (NeedToGenerateKey())
            {
                // we need to create a new key before we can export 
                CreateKeyPair(KeySizeValue);
            }
            // decrypt the data
            ElGamalDecrypt decrypt_data = new ElGamalDecrypt(current_key);
            return decrypt_data.ProcessData(p_data);
        }
    }

}


