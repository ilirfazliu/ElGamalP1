using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace P1ElGamal
{
    public class ElGamalDecrypt : ElGamalCipher
    {
        public ElGamalDecrypt(ElGamalKeyStruct p_struct) : base(p_struct)
        {
            // set the default block size to be ciphertext
            block_size = ciphertext_blocksize;
        }

        protected override byte[] ProcessDataBlock(byte[] plaintext_data_block)
        {
            // extract the byte arrays that represent A and B
            byte[] a_bytes = new byte[ciphertext_blocksize / 2];
            Array.Copy(plaintext_data_block, 0, a_bytes, 0, a_bytes.Length);
            
            byte[] b_bytes = new Byte[ciphertext_blocksize / 2];
            Array.Copy(plaintext_data_block, a_bytes.Length, b_bytes, 0, b_bytes.Length);

            // create big integers from the byte arrays
            BigInteger A = new BigInteger(a_bytes);
            BigInteger B = new BigInteger(b_bytes);

            BigInteger M = (B * A.modPow(current_key.X, current_key.P).modInverse(current_key.P)) % current_key.P;

            // return the result - take care to ensure that we create a result which is the correct length
            byte[] m_bytes = M.getBytes();

            if (m_bytes.Length < plaintext_blocksize)
            {
                byte[] full_block_result = new byte[plaintext_blocksize];
                Array.Copy(m_bytes, 0, full_block_result, plaintext_blocksize - m_bytes.Length, m_bytes.Length);

                m_bytes = full_block_result;
            }
            return m_bytes;
        }

        protected override byte[] ProcessFinalDataBlock(byte[] plaintext_final_block)
        {
            if (plaintext_final_block.Length > 0)
            {
                return ProcessDataBlock(plaintext_final_block);
            }
            else
            {
                return new byte[0];
            }
        }
    }
}
