using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Test_Api.Context;

namespace Test_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GroupUserController : ControllerBase
    {
        private readonly UserDbContext _dBContext;

        public GroupUserController(UserDbContext dbContext)
        {
            _dBContext = dbContext;
        }
        [HttpGet("GetGroupUser")]
        public async Task<IEnumerable<object>> GetGroupUser()
        {
            var groups = await _dBContext.Groups
                .Include(g => g.GroupUser)
                .ThenInclude(gu => gu.user)
                .Select(g => new
                {
                     g.Id,
                     g.Name,
                    Users = g.GroupUser
                        .Where(gu => gu.user != null) 
                        .Select(gu => new
                        {
                            UserId = gu.UserId,
                            UserName = gu.user.ten
                        })
                        .ToList()
                })
                .ToListAsync();

            return groups;
        }












    }
}
