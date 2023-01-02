using Microsoft.AspNetCore.Mvc;

namespace ChunckTask_webApi.Controllers
{

    public class ResponseContext
    {
        public dynamic Data { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class UploadFileController : Controller
    {
        private IConfiguration configuration;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ResponseContext _responseData;
        public long chunkSize;
        private string tempFolder;
        public UploadFileController(IConfiguration configuration, ILogger<WeatherForecastController> logger)
        {
            this.configuration = configuration;
            _logger = logger;
            chunkSize = 30000000 * Convert.ToInt32(configuration["ChunkSize"]);
            tempFolder = configuration["TargetFolder"];
            _responseData = new ResponseContext();
        }


        

        [HttpPost("UploadChunks")]
        [RequestSizeLimit(100000000)]

        public async Task<IActionResult> UploadChunks(string id, string fileName)
        {
            try
            {
                var chunkNumber = id;
                string newpath = Path.Combine(Directory.GetCurrentDirectory()+ "/Temp", fileName + chunkNumber);
                using (FileStream fs = System.IO.File.Create(newpath))
                {
                    byte[] bytes = new byte[chunkSize];
                    long bytesRead = 0;
                    while ((bytesRead = await Request.Body.ReadAsync(bytes, 0, bytes.Length)) > 0)
                    {
                        fs.Write(bytes, 0, (int)bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }


        [HttpPost("UploadComplete")]
        public IActionResult UploadComplete(string fileName)
        {
            try
            {
                string tempPath = Directory.GetCurrentDirectory() + "/Temp";
                string newPath = Path.Combine(tempPath, fileName);
                string[] filePaths = Directory.GetFiles(tempPath).Where(p => p.Contains(fileName)).OrderBy(p => Int32.Parse(p.Replace(fileName, "$").Split('$')[1])).ToArray();
                foreach (string filePath in filePaths)
                {
                    MergeChunks(newPath, filePath);
                }
                System.IO.File.Move(Path.Combine(tempPath, fileName), Path.Combine(tempFolder, fileName));
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }
        private static void MergeChunks(string chunk1, string chunk2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = System.IO.File.Open(chunk1, FileMode.Append);
                fs2 = System.IO.File.Open(chunk2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                if (fs1 != null) fs1.Close();
                if (fs2 != null) fs2.Close();
                System.IO.File.Delete(chunk2);
            }
        }
    }
}


