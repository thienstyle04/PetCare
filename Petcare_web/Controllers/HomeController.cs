using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models;
using Petcare_web.Models.DTO;
using System.Diagnostics;

namespace Petcare_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var services = new List<DichVuDTO>();
            
            try
            {
                var apiBaseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{apiBaseUrl}/api/DichVu/alldichvu");
                
                if (response.IsSuccessStatusCode)
                {
                    services = await response.Content.ReadFromJsonAsync<List<DichVuDTO>>() ?? new List<DichVuDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services for homepage");
            }

            return View(services);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
