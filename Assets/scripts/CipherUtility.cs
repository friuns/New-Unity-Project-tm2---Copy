using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class CipherUtility
{
    private static string Decode(string s)
    {
        var cs = s.ToCharArray();
        string k = "fptasdjklfjekljfkajsdfdjfkldsjfklsdfjdskljfklesa";
        for (int i = 0; i < cs.Length; i++)
            cs[i] ^= k[i];
        return Encoding.UTF8.GetString(Convert.FromBase64CharArray(cs, 0, cs.Length));
    }
	public static string Encrypt<T>(byte[] value, string password, string salt)
		  where T : SymmetricAlgorithm, new()
	{
		DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

		SymmetricAlgorithm algorithm = new T();

		byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
		byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

		ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

		using (MemoryStream buffer = new MemoryStream())
		{
			using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
			{
                stream.Write(value,0,value.Length);
			}

			return Convert.ToBase64String(buffer.ToArray());
		}
	}

    public static Stream Decrypt<T>(string text, string password, string salt)
	   where T : SymmetricAlgorithm, new()
	{
		DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

		SymmetricAlgorithm algorithm = new T();

		byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
		byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

		ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

		using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(text)))
		{
			using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
			{
			    return stream;
			}
		}
	}
}

