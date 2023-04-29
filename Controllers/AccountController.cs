using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;

namespace ImgWeb
{

    public class AccountController : Controller
    {

        static List<User> users = new List<User>();

        static string userDataPath = "Data/users/users.xml";

        public static void Init()
        {
            LoadData();
        }

        public IActionResult Register()
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            if (ModelState.IsValid)
            {
                // Check if user already exists
                if (UserExists(user.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(user);
                }

                user.key = Utils.RandomString();

                users.Add(user);

                TempData["Message"] = "Registration successful!";

                SaveData();

                
                return RedirectToAction("Login");
            }
            
            return View(user);
        }

        public IActionResult Login()
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            return View();
        }

        public IActionResult logout()
        {
            User user = AccountController.GetUserByKey(HttpContext.Session.GetString("key"));
            if (user != null)
            {
                user.lastIp = "";
                SaveData();
            }

            HttpContext.Session.Remove("key");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            if (ModelState.IsValid)
            {
                // Check if user exists
                if (!UserExists(user.Username))
                {
                    ModelState.AddModelError("Username", "Username not found.");
                    return View(user);
                }

                // Check if password is correct
                if (!IsPasswordCorrect(user.Username, user.Password))
                {
                    ModelState.AddModelError("Password", "Incorrect password.");
                    return View(user);
                }

                User foundUser = FindUser(user.Username);

                HttpContext.Session.SetString("key", foundUser.key);
                TempData["Message"] = "Login successful!";
                return RedirectToAction("Account", foundUser);
            }

            return View();
        }

        public IActionResult Account(string username, string password)
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");

            return View(GetUserByKey(HttpContext.Session.GetString("key")));
        }

        private bool UserExists(string username)
        {
            foreach (User user in users)
            {
                if (user.Username == username)
                    return true;
            }

            return false;
        }

        private bool IsPasswordCorrect(string username, string password)
        {
            foreach (User user in users)
            {
                if (user.Username == username && user.Password == password)
                    return true;
            }

            return false;
        }

        public static void SaveData()
        {
            TextWriter writer = new StreamWriter(userDataPath);
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<User>));

                if (!System.IO.File.Exists(userDataPath))
                {
                    System.IO.File.Create(userDataPath);
                }
                ser.Serialize(writer, users);
            }catch(Exception ex) {}
            writer.Close();
        }

        public static void LoadData()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<User>));
                using (XmlReader reader = XmlReader.Create(userDataPath))
                {
                    users = ser.Deserialize(reader) as List<User>;
                }
            }catch (Exception ex)
            {
                throw ex;
            }

        }

        public static User FindUser(string username)
        {
            foreach (User user in users)
            {
                if (user.Username == username) return user;
            }
            return new User();
        }

        public static User GetUserByKey(string key)
        {
            foreach (User user in users)
            {
                if (user.key == key) return user;
            }
            return new User();
        }

        public static User GetUserByIp(string ip)
        {
            foreach (User user in users)
            {
                if (user.lastIp == ip) return user;
            }
            return null;
        }

    }
}


