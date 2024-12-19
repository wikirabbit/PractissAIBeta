namespace PractissWeb.Services
{
    public class LoginCodeService
    {
        public static Dictionary<string, string> codeCache = new Dictionary<string, string>();

        public static string GenerateCode(string email)
        {
            // create a 6 digit code
            var random = new Random();
            string code = random.Next(0, 999999).ToString("D6");

            // add it to the codeCache with the email as the key
            if (codeCache.ContainsKey(email))
            {
                codeCache[email] = code; // Update the code if the email already exists
            }
            else
            {
                codeCache.Add(email, code); // Add new entry if the email does not exist
            }

            return code;
        }

        public static bool ValidateCode(string email, string code)
        {
            // validate the code with the email key from the codeCache
            if (codeCache.TryGetValue(email, out string storedCode))
            {
                return storedCode == code;
            }

            return false;
        }
    }
}