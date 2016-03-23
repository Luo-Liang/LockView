using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InfoView.LockViewSpecificImageHandlers
{
    public interface ILockViewSpecificImageHandler
    {
        MemoryStream RequestImage(string parameters);
    }

    public abstract class LockViewSpecificHandlerBase : ILockViewSpecificImageHandler
    {
        public abstract MemoryStream RequestImage(string parameters);
    }

    public class LiveEarthImageHandler : LockViewSpecificHandlerBase
    {
        public override MemoryStream RequestImage(string parameters)
        {
            //already stripped lockview://fixedwallpaper/himawari-8
            //...//
            //http://epic.gsfc.nasa.gov/epic-archive/jpg/epic_1b_20160317094118_00.jpg
            var argumentKeyValue = new Dictionary<string, string>();
            var wc = new WebClient();
            MemoryStream memStream;
            foreach (var item in parameters.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('=');
                argumentKeyValue.Add(parts[0], parts.Length == 1 ? "" : parts[1]);
            }
            var geoPreference = argumentKeyValue["location"];
            if (geoPreference == "neutral") geoPreference = "eastern";
            if (geoPreference == "western")
            {
                //show EPIC
                var docHtml = wc.DownloadString($"http://epic.gsfc.nasa.gov/api/images.php?date={DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).ToString("yyyy-MM-dd")}");
                dynamic jArray = JsonConvert.DeserializeObject(docHtml);
                var imageSource = (jArray as IEnumerable<dynamic>).Select(o => new { Source = $"http://epic.gsfc.nasa.gov/epic-archive/jpg/{o.image}.jpg", Date = DateTime.Parse(o.date.ToString()) }).Aggregate((a, b) => Math.Abs((a.Date - DateTime.Now).TotalSeconds) < Math.Abs((b.Date - DateTime.Now).TotalSeconds) ? a : b).Source;
                memStream = new MemoryStream(wc.DownloadData(imageSource));
                memStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var now = DateTime.UtcNow - TimeSpan.FromMinutes(30);
                now = now - TimeSpan.FromMinutes(now.Minute % 10);
                now = now - TimeSpan.FromSeconds(now.Second);
                var width = 550;
                var level = "4d";
                var numBlocks = 4;
                var time = now.ToString("HHmmss");
                var year = now.ToString("yyyy");
                var month = now.ToString("MM");
                var day = now.ToString("dd");
                var url = $"http://himawari8-dl.nict.go.jp/himawari8/img/D531106/{level}/{width}/{year}/{month}/{day}/{time}";
                var image = new System.Drawing.Bitmap(width * numBlocks, width * numBlocks);
                var graphics = Graphics.FromImage(image);
                graphics.Clear(System.Drawing.Color.Black);
                for (int y = 0; y < numBlocks; y++)
                    for (int x = 0; x < numBlocks; x++)
                    {
                        var currUrl = $"{url}_{x}_{y}.png";
                        using (var response = WebRequest.Create(currUrl).GetResponse())
                        {
                            using (var imgBlock = Image.FromStream(response.GetResponseStream()))
                            {
                                graphics.DrawImage(imgBlock, x * width, y * width, width, width);
                            }
                        }
                    }
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Save();
                graphics.Dispose();
                memStream = new MemoryStream();
                image.Save(memStream, ImageFormat.Jpeg);
                memStream.Seek(0, SeekOrigin.Begin);
            }
            return memStream;
        }
    }
}
