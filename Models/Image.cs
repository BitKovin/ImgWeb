namespace ImgWeb.Models
{
    public class Image
    {
        public string Path { get; set; }
        public string Author { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }

        public int id { get; set; } = -1;

        public string Name { get; set; } = "";

        public List<Image> Сomments { get; set;} = new List<Image>();

    }
}
