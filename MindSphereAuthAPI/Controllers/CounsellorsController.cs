using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.DTOs;
using MindSphereAuthAPI.Models;
using System;
using System.Collections.Generic;
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
                Email = c.UserId != null ? _context.Users.FirstOrDefault(u => u.Id == c.UserId)?.Email : null,
                Specialty = c.Specialty,
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
                Email = counsellor.UserId != null ? _context.Users.FirstOrDefault(u => u.Id == counsellor.UserId)?.Email : null,
                Specialty = counsellor.Specialty,
                AvailableSlots = counsellor.AvailableSlots
            };

            return Ok(dto);
        }

        // POST: api/Counsellors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CounsellorReadDto>> PostCounsellor([FromBody] CounsellorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loggedInUserId = User.FindFirstValue("id");
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
            if (counsellorProfile == null)
                return NotFound("Entered user has not been registered");

            var counsellor = new Counsellor
            {
                Name = dto.Name,
                Specialty = dto.Specialty,
                AvailableSlots = formattedSlots ?? new List<string>(),
                UserId = counsellorProfile.Id
            };

            _context.Counsellors.Add(counsellor);
            await _context.SaveChangesAsync();

            var createdDto = new CounsellorReadDto
            {
                Id = counsellor.Id,
                Name = counsellor.Name,
                Email = dto.Email,
                Specialty = counsellor.Specialty,
                AvailableSlots = counsellor.AvailableSlots
            };

            return CreatedAtAction(nameof(GetCounsellor), new { id = counsellor.Id }, createdDto);
        }

        // PUT: api/Counsellors/{id}
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
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        private bool CounsellorExists(int id)
        {
            return _context.Counsellors.Any(e => e.Id == id);
        }
    }
}
