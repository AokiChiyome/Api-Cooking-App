using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Api.Context;
using Test_Api.Models;

namespace Test_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _dBContext;

        public UserController(UserDbContext dbContext)
        {
            _dBContext = dbContext;
        }

        [HttpGet("GetListUser")]
        public async Task<IEnumerable<object>> GetUsers()
        {
            var users = await _dBContext.Users.Include(u => u.RoleUser).ThenInclude(ru => ru.role)
                .Select(u => new { u.Id, u.ten, u.tuoi, Roles = u.RoleUser.Select(ru => ru.role.Rolename).ToList() }).ToListAsync();
            return users;
        }

        [HttpPost("AddUser")]
        public IActionResult AddUser([FromBody] User user)
        {
            var existingUser = _dBContext.Users.FirstOrDefault(u => u.Id == user.Id);
            if (user == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            if (existingUser != null)
            {
                return Conflict("ID đã tồn tại! Vui lòng chọn ID khác.");
            }
            _dBContext.Users.Add(user);
            _dBContext.SaveChanges();
            return Ok("Thêm user thành công!");
        }


        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var checkId = _dBContext.Users.FirstOrDefault(c => c.Id == id);
            if (checkId == null)
            {
                return NotFound();
            }
            _dBContext.Users.Remove(checkId);
            _dBContext.SaveChanges();
            return Ok();
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User newuser)
        {
            var checkId = _dBContext.Users.FirstOrDefault(c => c.Id == id);
            if (checkId == null)
            {
                return NotFound();
            }
            checkId.ten = newuser.ten;
            checkId.tuoi = newuser.tuoi;
            await _dBContext.SaveChangesAsync();
            return Ok();
        }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("AddUserInRole")]
        public IActionResult AddUserInRole([FromBody] RoleUser roleUser)
        {
            if (roleUser == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            if (!_dBContext.Users.Any(u => u.Id == roleUser.UserId))
                return NotFound("Người dùng không tồn tại.");

            if (!_dBContext.Roles.Any(r => r.Id == roleUser.RoleId))
                return NotFound("Chức vụ không tồn tại.");

            if (_dBContext.RoleUsers.Any(Ru => Ru.UserId == roleUser.UserId && Ru.RoleId == roleUser.RoleId))
                return BadRequest("Người dùng đã có vai trò này.");

            _dBContext.RoleUsers.Add(new RoleUser { UserId = roleUser.UserId, RoleId = roleUser.RoleId });
            _dBContext.SaveChanges();

            return Ok("Thêm vai trò thành công.");


        }

        [HttpDelete("DeleteRoleInUser")]
        public IActionResult DeleteRoleInUser([FromBody] RoleUser roleUser)
        {
            var check = _dBContext.RoleUsers.FirstOrDefault(Ru => Ru.UserId == roleUser.UserId && Ru.RoleId == roleUser.RoleId);
            if (check == null)
            {
                return BadRequest("Người dùng Không có vai trò này");
            }
            _dBContext.RoleUsers.Remove(check);
            _dBContext.SaveChanges();
            return Ok();
        }

        [HttpPut("UpdateRoleInUser/{UserId}/{OldRoleId}")]
        public async Task<IActionResult> UpdateRoleInUser(int UserId, int OldRoleId, [FromBody] RoleUser roleUser)
        {
            if (roleUser == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            var userCheck = await _dBContext.Users.AnyAsync(u => u.Id == UserId);
            if (!userCheck)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            var roleCheck = await _dBContext.Roles.AnyAsync(r => r.Id == roleUser.RoleId);
            if (!roleCheck)
            {
                return NotFound("Vai trò mới không tồn tại.");
            }

            var CheckRoleUser = await _dBContext.RoleUsers.FirstOrDefaultAsync(ru => ru.UserId == UserId && ru.RoleId == OldRoleId);

            if (CheckRoleUser == null)
            {
                return NotFound("Vai trò cũ không tồn tại.");
            }

            _dBContext.RoleUsers.Remove(CheckRoleUser);
            await _dBContext.SaveChangesAsync();


            var newRoleUser = new RoleUser
            {
                UserId = UserId,
                RoleId = roleUser.RoleId
            };
            _dBContext.RoleUsers.Add(newRoleUser);
            await _dBContext.SaveChangesAsync();

            return Ok();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [HttpPost("AddUserInGroup")]
        public IActionResult AddUserInGroup([FromBody] AddUserToGroup groupuser)
        {
            if (groupuser == null)
                return NotFound("Dữ liệu không hợp lệ");
            
            if (!_dBContext.Users.Any(u => u.Id == groupuser.UserId))
                return NotFound("Người dùng không tồn tại.");

            if(!_dBContext.Groups.Any(g => g.Id == groupuser.GroupId))
                return NotFound("Group không tồn tại");

            if (_dBContext.GroupUsers.Any(Gu => Gu.UserId == groupuser.UserId && Gu.GroupId == groupuser.GroupId))
                return BadRequest("Người dùng đã có trong group này.");

            _dBContext.GroupUsers.Add(new GroupUser { UserId = groupuser.UserId, GroupId = groupuser.GroupId });
            _dBContext.SaveChanges();

            return Ok("Đã thêm thành công");

        }

        [HttpDelete("DeleteUserInGroup")]
        public IActionResult DeleteUserInGroup([FromBody] DeleteUserInGroup groupuser)
        {
            if (groupuser == null || groupuser.UserIds == null || !groupuser.UserIds.Any())
                return BadRequest("Dữ liệu không hợp lệ.");

            var usersToRemove = _dBContext.GroupUsers
            .Where(gu => groupuser.UserIds.Contains(gu.UserId) && gu.GroupId == groupuser.GroupId)
            .ToList();

            if (!usersToRemove.Any())
                return BadRequest("Không có người dùng hợp lệ để xóa khỏi nhóm.");

            _dBContext.GroupUsers.RemoveRange(usersToRemove);
            _dBContext.SaveChanges();
            return Ok();
        }










    }
}
