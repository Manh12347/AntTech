using Microsoft.AspNetCore.Mvc;

namespace AntTech.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", image.FileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                var url = Url.Content($"~/uploads/{image.FileName}");
                return Ok(new { url });
            }
            return BadRequest();
        }
    }
}
