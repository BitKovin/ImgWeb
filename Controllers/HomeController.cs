using ImgWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace ImgWeb
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public static List<Image> Images = new List<Image>();
        static string imageDataPath = "Data/images.xml";

        public static bool Saving = false;

        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Image(int id)
        {
            CheckLogin();
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            if(HttpContext.Session.Keys.Contains("key"))
                ViewData["isAdmin"] = AccountController.GetUserByKey(HttpContext.Session.GetString("key")).isAdmin;
            foreach (Image img in Images)
            {
                if(img.id == id)
                {
                    return View(img);
                }
            }

            return RedirectToAction("Index");
            
        }


        public ActionResult PrevPage(int page, string author)
        {
            return RedirectToAction("Index", "Home",new {page = page-1, author = author });
        }

        public ActionResult NextPage(int page, string author)
        {
            Console.WriteLine(author);
            return RedirectToAction("Index", "Home", new { page = page + 1, author = author });
        }

        public void CheckLogin()
        {
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            User user = AccountController.GetUserByIp(ip);

            if (user!=null)
            {
                HttpContext.Session.SetString("key",user.key);
            }
        }

        public ActionResult Index(int page = 0, string author = "")
        {

            CheckLogin();
            MainModel model = new MainModel();
            if (page<0) page = 0;



            List<Image> images = new List<Image>();

            foreach(Image img in Images)
            {
                images.Insert(0, img);
            }
           

            if(author!="")
            {
                //images = new List<Image>();

                foreach(Image image in Images)
                {
                    if(image.Author.ToLower()!=author.ToLower()) images.Remove(image);
                }

            }

            if (page >= (images.Count-1) / 5)
                page = ((images.Count-1) / 5);

            // Add some sample images
            model.images = images;
            model.page = page;
            model.pages = images.Count / 5;
            model.search = author;

            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");

            return View(model);
        }

        public ActionResult Upload()
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            if (HttpContext.Session.Keys.Contains("key"))
            if (HttpContext.Session.GetString("key").Length > 0)
            {
                return View();
            }
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Like(int imageIndex)
        {
            // Increment the likes for the selected image
            Images[imageIndex].Likes++;


            SaveData();

            // Return a JSON object with the updated image data
            return Json(new { likes = Images[imageIndex].Likes, dislikes = Images[imageIndex].Dislikes });
        }

        public ActionResult Dislike(int imageIndex)
        {
            // Increment the dislikes for the selected image
            Images[imageIndex].Dislikes++;

            SaveData();

            // Return a JSON object with the updated image data
            return Json(new { likes = Images[imageIndex].Likes, dislikes = Images[imageIndex].Dislikes });
        }


        [HttpPost]
        public IActionResult Upload(List<IFormFile> images, string name)
        {

            if(name.Length>50)
                name.Remove(50);

            if (images != null && images.Count > 0)
            {
                // Get the path to the Images directory in the wwwroot folder
                string webRootPath = _hostingEnvironment.WebRootPath;
                string imagesPath = Path.Combine(webRootPath, "Images");

                // Create the Images directory if it doesn't already exist
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Save each uploaded image to the Images directory and create a new Image instance for each file
                foreach (var image in images)
                {
                    if (image != null && image.Length > 0)
                    {
                        // Generate a unique filename for the uploaded image
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                        // Save the uploaded image to the Images directory
                        string filePath = Path.Combine(imagesPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        string author = AccountController.GetUserByKey(HttpContext.Session.GetString("key")).Username;

                        // Create a new Image instance and add it to the MainModel
                        Images.Add(new Image { Path = "/Images/" + fileName, Author = author, id = Images.Count, Name = name});

                        SaveData();
                        break;
                    }
                }

            }

            // Redirect back to the Index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddComment(int imageIndex, string comment, List<IFormFile> images)
        {
            if (comment == null) comment = "";
            if (comment.Length < 1 && images.Count == 0)
                return RedirectToAction("Image",new {id = imageIndex });

            if (comment.Length > 3000)
                comment.Remove(3000);


            // Retrieve the image from the database or wherever you are storing it
            Image image = Images[imageIndex];

            string path = "";

            if (images != null && images.Count > 0)
            {
                // Get the path to the Images directory in the wwwroot folder
                string webRootPath = _hostingEnvironment.WebRootPath;
                string imagesPath = Path.Combine(webRootPath, "Images");

                // Create the Images directory if it doesn't already exist
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Save each uploaded image to the Images directory and create a new Image instance for each file
                foreach (var CommentImage in images)
                {
                    if (CommentImage != null && CommentImage.Length > 0)
                    {
                        // Generate a unique filename for the uploaded image
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(CommentImage.FileName);

                        // Save the uploaded image to the Images directory
                        string filePath = Path.Combine(imagesPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            CommentImage.CopyTo(stream);
                        }

                        string author = AccountController.GetUserByKey(HttpContext.Session.GetString("key")).Username;

                        path = "/Images/" + fileName;

                        break;
                    }
                }
            }


                // Create the new comment object
            Image newComment = new Image();
            newComment.Author = AccountController.GetUserByKey(HttpContext.Session.GetString("key")).Username;
            newComment.Path = path;
            newComment.Name = comment;
            newComment.Likes = 0;
            newComment.Dislikes = 0;
            newComment.id = image.Сomments.Count; // assign an ID to the new comment

            // Add the comment to the image's comments list
            image.Сomments.Insert(0,newComment);
            SaveData();
            // Redirect the user back to the original image view
            return RedirectToAction("Image", new { id = imageIndex });
        }

        [HttpPost]
        public IActionResult DeleteComment(int img ,int id)
        {
            try
            {
                foreach (Image image in Images)
                {
                    if (image.id == img)
                    {
                        foreach (Image comment in image.Сomments)
                        {
                            if (comment.id == id)
                            {
                                image.Сomments.Remove(comment);
                                SaveData();
                            }
                            return Ok();
                        }
                    }
                }
                return NotFound();
            }
            catch (Exception ex) { return NotFound(); }
            
        }

        [HttpPost]
        public IActionResult DeleteImage(int img)
        {
            foreach (Image image in Images)
            {
                if (image.id == img)
                {
                    Images.Remove(image);
                    SaveData();
                    return Ok();
                }
            }
            return NotFound();
        }

            public static void SaveData()
        {

            if(Saving) return;
            Saving = true;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<Image>));

                if (!System.IO.File.Exists(imageDataPath))
                {
                    System.IO.File.Create(imageDataPath);
                }

                TextWriter writer = new StreamWriter(imageDataPath);
                ser.Serialize(writer, Images);
                writer.Close();
                
            }
            catch { }
            Saving = false;
        }

        public static void LoadData()
        {

                XmlSerializer ser = new XmlSerializer(typeof(List<Image>));
                using (XmlReader reader = XmlReader.Create(imageDataPath))
                {
                    Images = ser.Deserialize(reader) as List<Image>;

                    foreach(Image image in Images)
                    {
                        if (image.id < 0)
                            image.id = Images.IndexOf(image);
                    }

                }


        }

        public static List<Image> GetImagesFromUser(string name)
        {
            List<Image> images = new List<Image>();

            foreach (Image image in Images)
            {
                if(image.Author == name)
                    images.Add(image);
            }

            return images;
        }

        public IActionResult Privacy()
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["LoggedIn"] = HttpContext.Session.Keys.Contains("key");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}