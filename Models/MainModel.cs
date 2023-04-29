namespace ImgWeb.Models
{
    public class MainModel
    {
        public User user { get; set; }
        public List<Image> images { get; set; } = new List<Image>();

        public int page = 0;

        public int pageNum { get { return page + 1; } set { } }

        public int pages = 1;

        public string search;

    }
}
