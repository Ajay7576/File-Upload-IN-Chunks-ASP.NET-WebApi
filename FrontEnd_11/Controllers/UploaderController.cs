using Microsoft.AspNetCore.Mvc;

namespace FrontEnd_11.Controllers
{
    public class UploaderController : Controller
    {
        public IActionResult ChunkUpload()
        {
            return View();
        }
    }
}
