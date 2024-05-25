using System.Security.Cryptography;

namespace PrivateApp.Resources.Entities
{
    public class RSAPublicKey
    {
        public RSAPublicKey(RSAParameters rsaParameters)
        {
            foreach (var b in rsaParameters.Modulus)
            {
                ModulusStr += Convert.ToInt32(b).ToString();
                ModulusStr += "-";
            }
            ModulusStr = ModulusStr.TrimEnd('-');
            foreach (var b in rsaParameters.Exponent)
            {
                ExponentStr += Convert.ToInt32(b).ToString();
                ExponentStr += "-";
            }
            ExponentStr = ExponentStr.TrimEnd('-');
        }
        public string ModulusStr { get; private set; }
        public string ExponentStr { get; private set; }
    }
}
