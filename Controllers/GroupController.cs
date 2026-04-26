using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Api.Context;
using Test_Api.Models;

namespace Test_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {

        private readonly UserDbContext _dBContext;

        public GroupController(UserDbContext dbContext)
        {
            _dBContext = dbContext;
        }

        [HttpGet("GetGroup")]
        public async Task<IEnumerable<Group>> GetGroup()
        {
            return await _dBContext.Groups.ToListAsync();
        }


        [HttpPost("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] AddGroupRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || request.UserIds == null)
                return BadRequest("Dữ liệu không hợp lệ!");

            var existingUserIds = await _dBContext.Users
            .Where(u => request.UserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

            var invalidUserIds = request.UserIds.Except(existingUserIds).ToList();
            if (invalidUserIds.Any())
            {
                return BadRequest($"Các UserId sau không hợp lệ: {string.Join(", ", invalidUserIds)}");
            }

            var group = new Group { Id = request.Id, Name = request.Name };
            _dBContext.Groups.Add(group);
            await _dBContext.SaveChangesAsync();
            foreach (var userId in existingUserIds)
            {
                var groupUser = new GroupUser { GroupId = group.Id, UserId = userId };
                _dBContext.GroupUsers.Add(groupUser);
            }

            
            _dBContext.Groups.Add(group);
            await _dBContext.SaveChangesAsync();
            return Ok("Nhóm đã được tạo!");
        }

        [HttpDelete("DeleteGroup/{groupId}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var group = await _dBContext.Groups
                .Include(g => g.GroupUser)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound("Nhóm không tồn tại!");

            _dBContext.GroupUsers.RemoveRange(group.GroupUser);

            _dBContext.Groups.Remove(group);
            await _dBContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UpdateGroup{groupId}")]
        public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupRequest request)
        {
            var group = await _dBContext.Groups
                .Include(g => g.GroupUser)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound("Group not found");
            }

            var existingUserIds = await _dBContext.Users
            .Where(u => request.UserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

            var invalidUserIds = request.UserIds.Except(existingUserIds).ToList();
            if (invalidUserIds.Any())
            {
                return BadRequest($"Các UserId sau không hợp lệ: {string.Join(", ", invalidUserIds)}");
            }

            group.Name = request.Name;
            var currentUserIds = group.GroupUser.Select(gu => gu.UserId).ToList();

            var usersToAdd = request.UserIds.Except(currentUserIds).ToList();
            foreach (var userId in usersToAdd)
            {
                group.GroupUser.Add(new GroupUser { GroupId = groupId, UserId = userId });
            }

            var usersToRemove = existingUserIds.Except(request.UserIds).ToList();
            group.GroupUser = group.GroupUser.Where(gu => !usersToRemove.Contains(gu.UserId)).ToList();

            await _dBContext.SaveChangesAsync();
            return Ok(group);
        }
    }
}
