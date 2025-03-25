using System;
using System.Security.Cryptography;
using System.Text;

public class HashManagement {
    public HashManagement(){

    }

    private static string GenerateString()
    {
        // Create a byte array to hold 128 bits (16 bytes)
        byte[] randomBytes = new byte[16];

        // Fill the array with random bytes
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        // Convert the byte array to a hexadecimal string
        StringBuilder hexStringBuilder = new StringBuilder();
        foreach (byte b in randomBytes)
        {
            hexStringBuilder.Append(b.ToString("x2"));  // Format as two hexadecimal digits
        }
        
        return hexStringBuilder.ToString();
    }

    public static string[] ComputeSha1Hash()
    {
        int numberOfHashes = 40000;
        string[] hashes = new string[numberOfHashes];

        using (SHA1 sha1 = SHA1.Create())  // Use SHA1 to create the hash
        {
            for (int i = 0; i < numberOfHashes; i++)
            {
            string input = GenerateString();
            // Convert the input string to a byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            // Compute the hash (returns a byte array)
            byte[] hashBytes = sha1.ComputeHash(inputBytes);
            // Convert the byte array to a hexadecimal string
            StringBuilder hashStringBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashStringBuilder.Append(b.ToString("x2")); // Format bytes as hexadecimal
            }
                hashes[i] = hashStringBuilder.ToString();
            }

            return hashes;
        }
    }
}