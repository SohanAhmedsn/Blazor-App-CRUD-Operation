using BlazorApp1.Shared.Models;
using BlazorApp1.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly CarInformationDbContext db;
        private readonly IWebHostEnvironment env;
        public CarsController(CarInformationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDetail>>> GetCarDetails()
        {
            return await db.CarDetails.ToListAsync();
        }
        [HttpGet("Parts")]
        public async Task<ActionResult<IEnumerable<CarDetail>>> GetCarDetailsWithParts()
        {
            return await db.CarDetails.Include(x => x.PartsDetails).ToListAsync();
        }
        [HttpGet("Parts/{id}")]
        public async Task<ActionResult<CarDetail>> GetCarDetail(int id)
        {
            var d = await db.CarDetails.Include(x => x.PartsDetails).FirstOrDefaultAsync(x => x.CarDetailId == id);
            if (d == null) return NotFound();
            else return d;
        }
        [HttpPost]
        public async Task<ActionResult<CarDetail>> PostCarInfo(CarDetail car)
        {
            await db.CarDetails.AddAsync(car);
            await db.SaveChangesAsync();
            return car;
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CarDetail>> PutCarInfo(int id, CarDetail car)
        {
            if (id != car.CarDetailId) return BadRequest("Id mismatch");
            var existing = await db.CarDetails.Include(x => x.PartsDetails).FirstOrDefaultAsync(x => x.CarDetailId == id);
            if (existing == null) return NotFound();
            existing.CarName = car.CarName;
            existing.LaunchDate = car.LaunchDate;
            existing.Price = car.Price;
            existing.IsStock = car.IsStock;
            existing.Picture = car.Picture;
            db.PartsDetails.RemoveRange(existing.PartsDetails);
            foreach (var s in car.PartsDetails)
            {
                existing.PartsDetails.Add(new PartsDetail { PartName = s.PartName, PartsPrice = s.PartsPrice });
            }
            await db.SaveChangesAsync();
            return car;
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCarInfo(int id)
        {

            var existing = await db.CarDetails.Include(x => x.PartsDetails).FirstOrDefaultAsync(x => x.CarDetailId == id);
            if (existing == null) return NotFound();
            db.CarDetails.Remove(existing);
            await db.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("Upload")]
        public async Task<ImageUploadResponse> Upload(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var storedFileName = randomFileName + ext;
            using FileStream fs = new FileStream(Path.Combine(env.WebRootPath, "Pictures", storedFileName), FileMode.Create);
            await file.CopyToAsync(fs);
            fs.Close();
            return new ImageUploadResponse { FileName = file.FileName, StoredFileName = storedFileName };
        }
    }
}

