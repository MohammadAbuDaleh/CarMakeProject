using CarseerProject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Route("api/models")]
[ApiController]
public class CarModelsController : ControllerBase
{
    public IConfiguration Configuration { get; }
    private readonly string CarMakeApiUrl = "";
    private readonly string CarMakeCsvPath = "";
    public CarModelsController(IConfiguration configuration)
    {
        Configuration = configuration;
        CarMakeApiUrl=Configuration["CarMakeApiUrl"];
        CarMakeCsvPath = Configuration["CarMakeCsvPath"];
    }
  

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetCarModels([FromQuery] string make, [FromQuery] int modelYear)
    {
        if (string.IsNullOrEmpty(make) || modelYear <= 0)
        {
            return BadRequest("Make and model year are required parameters.");
        }

        string makeId = await GetMakeIdAsync(make);
        if (string.IsNullOrEmpty(makeId))
        {
            return NotFound($"Make '{make}' not found.");
        }

        List<string> models = await GetModelsForMakeIdYearAsync(makeId, modelYear);
        if (models == null)
        {
            return StatusCode(500, "Unable to retrieve models.");
        }

        return Ok(new { Models = models });
    }

    private async Task<string> GetMakeIdAsync(string make)
    {
        using (var reader = new StreamReader(CarMakeCsvPath))
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = line.Split(',');

                if (values.Length >= 2 && values[1].Trim().Equals(make, System.StringComparison.OrdinalIgnoreCase))
                {
                    return values[0].Trim();
                }
            }
        }

        return null;
    }

    private async Task<List<string>> GetModelsForMakeIdYearAsync(string makeId, int modelYear)
    {
        string apiUrl = CarMakeApiUrl.Replace("{makeId}", makeId).Replace("{modelYear}", modelYear.ToString());
        
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetStringAsync(apiUrl);

            var result = JsonConvert.DeserializeObject<CarModelsResponse>(response);
            var carModelsResponse = result;

            var modelNames = carModelsResponse.Results.Select(model => model.Model_Name).ToList();


            return modelNames;
        }
    }



}