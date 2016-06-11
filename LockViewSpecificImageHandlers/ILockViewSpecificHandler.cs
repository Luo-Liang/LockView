using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        TimeSpan GetExpirationDuration(string parameters);
    }

    public abstract class LockViewSpecificHandlerBase : ILockViewSpecificImageHandler
    {
        public abstract TimeSpan GetExpirationDuration(string parameters);

        public abstract MemoryStream RequestImage(string parameters);
    }

    public class LiveEarthImageHandler : LockViewSpecificHandlerBase
    {
        public override TimeSpan GetExpirationDuration(string parameters)
        {
            var argumentKeyValue = new Dictionary<string, string>();
            foreach (var item in parameters.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('=');
                argumentKeyValue.Add(parts[0], parts.Length == 1 ? "" : parts[1]);
            }
            if (argumentKeyValue.ContainsKey("location") == false) argumentKeyValue["location"] = "western";
            var geoPreference = argumentKeyValue["location"];
            if (geoPreference == "neutral") geoPreference = "eastern";
            return geoPreference == "eastern" ? TimeSpan.FromMinutes(10) : TimeSpan.FromHours(2);
        }

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
            if (argumentKeyValue.ContainsKey("location") == false) argumentKeyValue["location"] = "western";
            var geoPreference = argumentKeyValue["location"];
            if (geoPreference == "neutral") geoPreference = "eastern";
            if (geoPreference == "western")
            {
                //show EPIC
                var docHtml = wc.DownloadString($"http://epic.gsfc.nasa.gov/api/images.php?date={DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)).ToString("yyyy-MM-dd")}");
                dynamic jArray = JsonConvert.DeserializeObject(docHtml);
                var imageSource = (jArray as IEnumerable<dynamic>).Select(o => new { Source = $"http://epic.gsfc.nasa.gov/epic-archive/jpg/{o.image}.jpg", Date = DateTime.Parse(o.date.ToString()) }).Aggregate((a, b) => Math.Abs((a.Date - DateTime.Now).TotalSeconds % 86400) < Math.Abs((b.Date - DateTime.Now).TotalSeconds % 86400) ? a : b).Source;
                memStream = new MemoryStream(wc.DownloadData(imageSource));
            }
            else
            {
                var now = DateTime.UtcNow - TimeSpan.FromMinutes(30);
                now = now - TimeSpan.FromMinutes(now.Minute % 10);
                now = now - TimeSpan.FromSeconds(now.Second);
                var _width = 550;
                var level = "4d";
                var numBlocks = 4;
                var time = now.ToString("HHmmss");
                var year = now.ToString("yyyy");
                var month = now.ToString("MM");
                var day = now.ToString("dd");
                var url = $"http://himawari8-dl.nict.go.jp/himawari8/img/D531106/{level}/{_width}/{year}/{month}/{day}/{time}";
                var _image = new System.Drawing.Bitmap(_width * numBlocks, _width * numBlocks);
                var _graphics = Graphics.FromImage(_image);
                _graphics.Clear(System.Drawing.Color.Black);
                for (int y = 0; y < numBlocks; y++)
                    for (int x = 0; x < numBlocks; x++)
                    {
                        var currUrl = $"{url}_{x}_{y}.png";
                        using (var response = WebRequest.Create(currUrl).GetResponse())
                        {
                            using (var imgBlock = Image.FromStream(response.GetResponseStream()))
                            {
                                _graphics.DrawImage(imgBlock, x * _width, y * _width, _width, _width);
                            }
                        }
                    }
                _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                _graphics.Save();
                _graphics.Dispose();
                memStream = new MemoryStream();
                _image.Save(memStream, ImageFormat.Jpeg);
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
    }

    public class EditorChoiceImageHandler : LockViewSpecificHandlerBase
    {
        Random random = new Random();
        public override TimeSpan GetExpirationDuration(string parameters)
        {
            return TimeSpan.FromDays(1);
        }

        public override MemoryStream RequestImage(string parameters)
        {

            //we need to find some images either from greer.io or my 1drv.
            //for simplicity's sake let's just grab some.
            var directoryPath = AppDomain.CurrentDomain.BaseDirectory + "\\Assets";
            // Directory.Delete(directoryPath, true);
            var allFiles = Directory.GetFiles(directoryPath);
            var selectedFile = allFiles[random.Next() % allFiles.Length];
            MemoryStream resultStream = new MemoryStream();
            using (var stream = File.Open(selectedFile, FileMode.Open))
                stream.CopyTo(resultStream);
            resultStream.Seek(0, SeekOrigin.Begin);
            return resultStream;
        }
    }
}
