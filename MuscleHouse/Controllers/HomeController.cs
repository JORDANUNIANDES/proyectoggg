using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MuscleHouse.Models;
using MuscleHouse.Services;

namespace MuscleHouse.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStaffService _staffService;

        public HomeController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Personal()
        {
            var trainers = await _staffService.GetAllTrainersAsync();
            var receptionists = await _staffService.GetAllReceptionistsAsync();

            ViewBag.Trainers = trainers;
            ViewBag.Receptionists = receptionists;

            return View();
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
