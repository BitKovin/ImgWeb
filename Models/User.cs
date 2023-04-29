using System.Collections.Generic;

namespace ImgWeb
{
    public class User
    {

        public string Username { get; set; } = "null";
        public string Password { get; set; } = "null";

        public string key { get; set; } = "null";

        public string lastIp = "";

        public bool isAdmin = false;

    }
}
