namespace ImgWeb
{
    public static class Utils
    {

        public static string RandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        public static bool isLoggedIn(ISession session)
        {
           return session.Keys.Contains("key");
        }

    }
}
