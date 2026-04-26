using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Api.Context;
using Test_Api.Models;

namespace Test_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleUserController : ControllerBase
    {
        private readonly UserDbContext _dBContext;

        public RoleUserController(UserDbContext dbContext)
        {
            _dBContext = dbContext;
        }

        [HttpGet("GetRoleUser")]
        
        public async Task<IActionResult> GetRoleWithUsers()
        {
            var roles = await _dBContext.Roles
                .Include(r => r.RoleUser)
                .ThenInclude(ru => ru.user)  
                .Select(r => new
                {
                     r.Id,
                     r.Rolename,
                    Users = r.RoleUser.Select(ru => new
                    {
                        ru.UserId,
                        UserName = ru.user.ten
                    }).ToList()
                })
                .ToListAsync();

            return Ok(roles);
        }


        
    }
}
