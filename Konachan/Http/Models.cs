using Konachan.Helper;

namespace Konachan.Http
{
    class PostPic
    {
        private string fileurl = string.Empty;
        private string jpegurl = string.Empty;
        private string created_at = string.Empty;
        private string url_preview = string.Empty;
        public string Author { get; set; }
        public string File_size { get; set; }
        public string File_url
        {
            get { return "http:" + fileurl; }
            set { fileurl = value; }
        }
        public string Id { get; set; }
        public string Preview_url
        {
            get { return "http:" + url_preview; }
            set { url_preview = value; }
        }
        public string Jpeg_url
        {
            get { return "http:" + jpegurl; }
            set { jpegurl = value; }
        }
        public string Score { get; set; }
        public string Rating { get; set; }
        public string Tags { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Source { get; set; }
        public string MD5 { get; set; }
        public string Created_at
        {
            get { return Methods.LinuxToData(created_at); }
            set { created_at = value; }
        }
    }
}
