using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackerOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly DataTransferringService _dataService;

        public DataController(DataTransferringService dataService)
        {
            _dataService = dataService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _dataService.GetSpreadsheet(id);

            if(file == null)
            {
                return NotFound();
            }

            return File(file.SheetData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Filename);
        }


    }
}
