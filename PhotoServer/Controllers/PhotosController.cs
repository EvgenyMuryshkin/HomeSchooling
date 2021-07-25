using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PhotoServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(ILogger<PhotosController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public int Get()
        {
            return Process.GetCurrentProcess().Id;
        }

        [HttpPost]
        public async Task Post()
        {
            _logger.LogInformation($"Received {Request.ContentType}, size {Request.ContentLength}");
            Request.Headers.TryGetValue("FileName", out var values);
            var storePath = Settings.Location;
            var dateFolder = Path.Combine(storePath, DateTime.Now.ToString("yyyy_MM_dd"));

            if (!Directory.Exists(dateFolder))
                Directory.CreateDirectory(dateFolder);

            var fileName = values.FirstOrDefault() ?? $"{DateTime.Now.ToString("HH_mm_ss")}.jpg";

            var path = Path.Combine(dateFolder, fileName);

            _logger.LogInformation($"Stored in {path}");

            using (var file = System.IO.File.Create(path))
            {
                await Request.Body.CopyToAsync(file);
            }
        }
    }
}
