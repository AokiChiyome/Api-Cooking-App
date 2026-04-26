using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Api.Context;
using Test_Api.Models;

namespace Test_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly UserDbContext _dBContext;

        public RoleController(UserDbContext dbContext)
        {
            _dBContext = dbContext;
        }


        [HttpGet("GetRole")]
        public async Task<IEnumerable<Role>> GetRole()
        {

            return await _dBContext.Roles.ToListAsync();
        }

        [HttpPost("AddRole")]
        public IActionResult AddRole([FromBody] Role role)
        {
            var existingUser = _dBContext.Roles.FirstOrDefault(u => u.Id == role.Id);
            if (role == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            if (existingUser != null)
            {
                return Conflict("ID đã tồn tại! Vui lòng chọn ID khác.");
            }


            _dBContext.Roles.Add(role);
            _dBContext.SaveChanges();
            var result = new
            {
                role.Id,
                role.Rolename
            };
            return Ok(result);
        }


        [HttpDelete("Deleterole/{id}")]
        public IActionResult DeleteRole(int id)
        {
            var checkId = _dBContext.Roles.FirstOrDefault(c => c.Id == id);
            if (checkId == null)
            {
                return NotFound();
            }
            _dBContext.Roles.Remove(checkId);
            _dBContext.SaveChanges();
            return Ok();
        }


        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role newRole)
        {
            var checkId = _dBContext.Roles.FirstOrDefault(c => c.Id == id);
            if (checkId == null)
            {
                return NotFound();
            }

            _dBContext.Roles.Remove(checkId);
            _dBContext.SaveChanges();

            _dBContext.Roles.Add(newRole);
            _dBContext.SaveChanges();


            checkId.Rolename = newRole.Rolename;
            await _dBContext.SaveChangesAsync();
            return Ok();
        }
    }
}
