using Microsoft.AspNetCore.Components.Forms;

namespace BLAZAM.Server.Helpers
{
    public  static class UIHelpers
    {
        public static async Task<byte[]?> ToByteArrayAsync(this IBrowserFile file, int maxReadBytes = 5000000)
        {
            byte[] fileBytes;
            using (var stream = file.OpenReadStream(5000000))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }
            return fileBytes;
        }
    }
}
