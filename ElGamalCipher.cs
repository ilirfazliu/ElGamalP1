using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace P1ElGamal
{
    public abstract class ElGamalCipher : ImplementationClass
    {
        protected int block_size;
        protected int plaintext_blocksize;
        protected int ciphertext_blocksize;
        protected ElGamalKeyStruct current_key;

        protected abstract byte[] ProcessDataBlock(byte[] plaintext_data_block);
        protected abstract byte[] ProcessFinalDataBlock(byte[] final_plaintext_block);

        public ElGamalCipher(ElGamalKeyStruct elg_current_key)
        {
            // set the key details
            current_key = elg_current_key;

            // calculate the blocksizes
            //remember that the length of the key modulus affects the amount of data that can be processed by the encryption function in one go
            plaintext_blocksize = (elg_current_key.P.bitCount() - 1) / 8;
            ciphertext_blocksize = ((elg_current_key.P.bitCount() + 7) / 8) * 2;

            // set the default block for plaintext, which is suitable for encryption
            block_size = plaintext_blocksize;
        }

        public byte[] ProcessData(byte[] data)
        {

            // create a stream backed by a memory array
            MemoryStream stream = new MemoryStream();
            // determine how many complete blocks there are
            int complete_blocks = data.Length / block_size;

            // create an array which will hold a block
            byte[] hold_data_block = new Byte[block_size];

            // run through and process the complete blocks
            int i = 0;

            for (; i < complete_blocks; i++)
            {
                // copy the block and create the big integer
                Array.Copy(data, i * block_size, hold_data_block, 0, block_size);
                // process the block
                byte[] result = ProcessDataBlock(hold_data_block);
                // write the processed data into the stream
                stream.Write(result, 0, result.Length);
            }

            // process the final block
            byte[] last_block = new Byte[data.Length - (complete_blocks * block_size)];

            Array.Copy(data, i * block_size, last_block, 0, last_block.Length);

            // process the final block
            byte[] final_result = ProcessFinalDataBlock(last_block);

            // write the final data bytes into the stream
            stream.Write(final_result, 0, final_result.Length);

            // return the contents of the stream as a byte array
            return stream.ToArray();
        }
    }
}
