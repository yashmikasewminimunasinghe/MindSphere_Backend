using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.DTOs;
using MindSphereAuthAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MindSphereAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CounsellorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CounsellorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Counsellors
        [HttpGet]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<IEnumerable<CounsellorReadDto>>> GetCounsellors()
        {
            var counsellors = await _context.Counsellors.ToListAsync();

            var result = counsellors.Select(c => new CounsellorReadDto
            {
                Id = c.Id,
                Name = c.Name,
                Specialty = c.Specialty,
                Rating = c.Rating,
                PhotoUrl = c.PhotoUrl,
                AvailableSlots = c.AvailableSlots
            }).ToList();

            return Ok(result);
        }

        // GET: api/Counsellors/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<CounsellorReadDto>> GetCounsellor(int id)
        {
            var counsellor = await _context.Counsellors.FindAsync(id);
            if (counsellor == null)
                return NotFound();

            var dto = new CounsellorReadDto
            {
                Id = counsellor.Id,
                Name = counsellor.Name,
                Specialty = counsellor.Specialty,
                Rating = counsellor.Rating,
                PhotoUrl = counsellor.PhotoUrl,
                AvailableSlots = counsellor.AvailableSlots
            };

            return Ok(dto);
        }

        // POST: api/Counsellors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<CounsellorReadDto>> PostCounsellor([FromForm] CounsellorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loggedInUserId = User.FindFirstValue("id");
            string userId = "";
            if (string.IsNullOrEmpty(loggedInUserId))
                return Unauthorized("No user ID in token");

            var formattedSlots = dto.AvailableSlots?
                .Select(slot =>
                {
                    if (DateTime.TryParse(slot, out var parsed))
                        return parsed.ToString("yyyy-MM-dd HH:mm");
                    return slot;
                })
                .ToList();

            var counsellorProfile = _context.Users.FirstOrDefault(a => a.Email == dto.Email);
            if (counsellorProfile != null)
            {
                userId = counsellorProfile.Id;
            }
            else
            {
                return NotFound("Entered user has not been registered");
            }

            var counsellor = new Counsellor
            {
                Name = dto.Name,
                Specialty = dto.Specialty,
                Rating = dto.Rating,
                AvailableSlots = formattedSlots ?? new List<string>(),
                UserId = userId
            };

            if (dto.Photo != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Photo.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                counsellor.PhotoUrl = $"/Uploads/{uniqueFileName}";
            }
            else
            {
                counsellor.PhotoUrl = "/images/default-counsellor.jpg";
            }

            _context.Counsellors.Add(counsellor);
            await _context.SaveChangesAsync();

            var createdDto = new CounsellorReadDto
            {
                Id = counsellor.Id,
                Name = counsellor.Name,
                Specialty = counsellor.Specialty,
                Rating = counsellor.Rating,
                PhotoUrl = counsellor.PhotoUrl,
                AvailableSlots = counsellor.AvailableSlots
            };

            return CreatedAtAction(nameof(GetCounsellor), new { id = counsellor.Id }, createdDto);
        }

        // PUT: api/Counsellors/{id} - update WITHOUT photo change
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCounsellor(int id, [FromBody] CounsellorUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var counsellor = await _context.Counsellors.FindAsync(id);
            if (counsellor == null)
                return NotFound();

            counsellor.Name = dto.Name;
            counsellor.Specialty = dto.Specialty;
            counsellor.Rating = dto.Rating;

            var formattedSlots = dto.AvailableSlots?
                .Select(slot =>
                {
                    if (DateTime.TryParse(slot, out var parsed))
                        return parsed.ToString("yyyy-MM-dd HH:mm");
                    return slot;
                })
                .ToList();

            counsellor.AvailableSlots = formattedSlots ?? new List<string>();

            _context.Entry(counsellor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CounsellorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Counsellors/{id}/photo - update WITH photo change
        [HttpPut("{id}/photo")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UpdateCounsellorPhoto(int id, [FromForm] CounsellorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var counsellor = await _context.Counsellors.FindAsync(id);
            if (counsellor == null)
                return NotFound();

            // Update other fields
            counsellor.Name = dto.Name;
            counsellor.Specialty = dto.Specialty;
            counsellor.Rating = dto.Rating;

            var formattedSlots = dto.AvailableSlots?
                .Select(slot =>
                {
                    if (DateTime.TryParse(slot, out var parsed))
                        return parsed.ToString("yyyy-MM-dd HH:mm");
                    return slot;
                })
                .ToList();

            counsellor.AvailableSlots = formattedSlots ?? new List<string>();

            if (dto.Photo != null)
            {
                // Delete old photo if exists and not default
                if (!string.IsNullOrEmpty(counsellor.PhotoUrl) && !counsellor.PhotoUrl.Contains("default-counsellor.jpg"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), counsellor.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Photo.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                counsellor.PhotoUrl = $"/Uploads/{uniqueFileName}";
            }

            _context.Entry(counsellor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CounsellorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool CounsellorExists(int id)
        {
            return _context.Counsellors.Any(e => e.Id == id);
        }
    }
}
