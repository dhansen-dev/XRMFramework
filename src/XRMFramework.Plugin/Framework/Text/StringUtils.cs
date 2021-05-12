using System.Security.Cryptography;
using System.Text;

namespace XRMFramework.Text
{
    public class StringUtils
    {
        /// <summary>
        /// Hashes a string using Sha256
        /// </summary>
        /// <param name="stringToHash">The string to hash</param>
        /// <param name="encoding">Encoding to use. Default is <see cref="Encoding.UTF8"/></param>
        /// <returns></returns>
        public static string HashString(string stringToHash, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            var stringAsBytes = encoding.GetBytes(stringToHash);

            var sha256 = new SHA256Managed();
            var stringBuilder = new StringBuilder();

            var hashedBytes = sha256.ComputeHash(stringAsBytes);

            foreach (var hashedByte in hashedBytes)
            {
                stringBuilder.Append(hashedByte.ToString("x"));
            }

            var hashedString = stringBuilder.ToString();

            return hashedString;
        }
    }
}