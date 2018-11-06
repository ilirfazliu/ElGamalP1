using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace P1ElGamal
{
    public class ElGamalEncrypt : ElGamalCipher
    {
        Random random_number;

        public ElGamalEncrypt(ElGamalKeyStruct p_struct) : base(p_struct)
        {
            random_number = new Random();
        }

        //PROCESS_DATA_METHOD
        protected override byte[] ProcessDataBlock(byte[] plaintext_data_block)
        {
            // the random number, K
            BigInteger K;
            // create K, which is the random number        
            do
            {
                K = new BigInteger();
                K.genRandomBits(current_key.P.bitCount() - 1, random_number);
            } while (K.gcd(current_key.P - 1) != 1);

            // compute the values  A = G exp K mod P
            // and B = ((Y exp K mod P) * plain_data_block) / P
            BigInteger A = current_key.G.modPow(K, current_key.P);
            BigInteger B = (current_key.Y.modPow(K, current_key.P) * new BigInteger(plaintext_data_block)) % (current_key.P);

            // ciphertext
            byte[] cipher_result = new byte[ciphertext_blocksize];

            // copy the bytes from A and B into the result array
            byte[] a_bytes = A.getBytes();
            Array.Copy(a_bytes, 0, cipher_result, ciphertext_blocksize / 2 - a_bytes.Length, a_bytes.Length);

            byte[] b_bytes = B.getBytes();
            Array.Copy(b_bytes, 0, cipher_result, ciphertext_blocksize - b_bytes.Length, b_bytes.Length);

            // return the result array after merging A and B
            return cipher_result;
        }

        //PROCESS_FINAL_BLOCK_DATA_METHOD
        protected override byte[] ProcessFinalDataBlock(byte[] final_block)
        {
            if (final_block.Length > 0)
            {
                if (final_block.Length < block_size)
                {
                    // create a fullsize block which contains the
                    // data to encrypt followed by trailing zeros
                    byte[] padded_block = new byte[block_size];
                    Array.Copy(final_block, 0, padded_block, 0, final_block.Length);
                    return ProcessDataBlock(padded_block);
                }
                else
                {
                    return ProcessDataBlock(final_block);
                }
            }
            else
            {
                return new byte[0];
            }
        }
    }
}
