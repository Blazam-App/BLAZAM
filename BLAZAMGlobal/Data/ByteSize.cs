namespace BLAZAM.Global.Data
{
    public class ByteSize
    {
        private readonly double _bytes;

        public ByteSize(double bytes)
        {
            _bytes = bytes;
        }

        public override string ToString()
        {
            double size = _bytes;
            string suffix = "B";

            if (size >= 1024)
            {
                suffix = "KB";
                size /= 1024.0;
            }

            if (size >= 1024)
            {
                suffix = "MB";
                size /= 1024.0;
            }

            if (size >= 1024)
            {
                suffix = "GB";
                size /= 1024.0;
            }

            return $"{size:N2} {suffix}";
        }
    }
}
