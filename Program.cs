using System;
using System.Text;
using System.Security.Cryptography;

namespace P1ElGamal
{
    class Program
    {
        static void Main()
        {
            // define the byte array that we will use as plaintext
            Console.Write("Type your PLAINTEXT: ");
            string _plaintext = Console.ReadLine();
            byte[] plaintext = Encoding.Default.GetBytes(_plaintext);

            // Create an instance of the algorithm and generate keys
            ImpExpElGamalParameters _parameters = new ImplementationClass();

            // set the key size
            _parameters.KeySize = 384;

            // extract and print the xml string
            string xml_string = _parameters.ToXmlString(true);
            Console.WriteLine("\n{0} \n", xml_string);

            ImpExpElGamalParameters encrypt = new ImplementationClass();
    
            encrypt.FromXmlString(_parameters.ToXmlString(false));
            byte[] ciphertext = _parameters.EncryptData(plaintext);
          

            // decrypt instance
            ImpExpElGamalParameters decrypt = new ImplementationClass();

            
            decrypt.FromXmlString(_parameters.ToXmlString(true));

            // decrypted ciphertext
            byte[] potential_plaintext = decrypt.DecryptData(ciphertext);
            

            Console.WriteLine("\n\nPLAINTEXT: {0} \n\nCIPHERTEXT: {1} \n\nDECRYPTED: {2} \n\n", Encoding.UTF8.GetString(plaintext),
                Encoding.UTF8.GetString(ciphertext), Encoding.UTF8.GetString(potential_plaintext));
            Console.ReadLine();
        }

        private static bool CompareArrays(byte[] p_arr1, byte[] p_arr2)
        {
            for (int i = 0; i < p_arr1.Length; i++)
            {
                if (p_arr1[i] != p_arr2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
    
}
