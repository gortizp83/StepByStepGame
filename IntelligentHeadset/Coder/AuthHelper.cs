
namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    /// <summary>
    /// Contains helper methods for authentication process.
    /// </summary>
    class AuthHelper
    {
        private const uint SharedSecret = 0xE47325F4;

        /// <summary>
        /// Calculates an authentication response (key) based on the given nonce value.
        /// Example: Received nonce: 0x2bd45fbb, authentication key: 0x7e7d3bd2
        /// </summary>
        /// <param name="nonce">The nonce value.</param>
        /// <returns>The calculated authentication response (key).</returns>
        public static uint CalculateAuthenticationKey(uint nonce)
        {
            ulong temp = (nonce ^ SharedSecret);

            int lowerFiveBitsOfNonce = (int)nonce & 0x1F;

            // Rotate left by the amount defined by the lower 5 bits of the nonce
            temp = LeftRotate(temp, (int)nonce & 0x1F);

            uint response = (uint)temp;

            /*string nonceAsHexString = string.Format("0x{0:X}", nonce);
            string answerAsHexString = string.Format("0x{0:X}", response);
            string lowerFiveBitsOfNonceAsHexString = string.Format("0x{0:X}", lowerFiveBitsOfNonce);

            System.Diagnostics.Debug.WriteLine("CalculateAuthenticationToken(): "
                + nonceAsHexString + " -> " + answerAsHexString
                + ", lower 5 bits of nonce: " + lowerFiveBitsOfNonceAsHexString);*/

            return response;
        }

        /// <summary>
        /// For testing.
        /// </summary>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public static ulong CalculateAuthenticationToken(string nonceAsHexString)
        {
            uint nonce = uint.Parse(nonceAsHexString, System.Globalization.NumberStyles.HexNumber);
            return CalculateAuthenticationKey(nonce);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static ulong LeftRotate(ulong value, int step)
        {
            ulong rotated = (value << step) | (value >> (32 - step));

            /*System.Diagnostics.Debug.WriteLine("LeftRotate(): "
                + string.Format("0x{0:X}", value) + " -> "
                + string.Format("0x{0:X}", rotated));*/

            return rotated;
        }
    }
}
