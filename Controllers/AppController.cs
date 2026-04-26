using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Api.Context;
using Test_Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace Test_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly CookingDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public AppController(CookingDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            
            if (_dbContext.Users.Any(u => u.Email == model.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new Account
            {
                Email = model.Email,
                Name = model.Name,
                Password = model.Password, 
                Role = model.Role
            };

            
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || user.Password != model.Password)
            {
                return Unauthorized("Invalid credentials.");
            }

           
            var fakeToken = $"token-{Guid.NewGuid()}"; 

            return Ok(new
            {
                token = fakeToken,
                userId = user.Id,
                email = user.Email,
                name = user.Name,
                role = user.Role
            });
        }
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            if (user.Password != model.OldPassword)
            {
                return BadRequest("Mật khẩu cũ không đúng.");
            }

            if (model.NewPassword == model.OldPassword)
            {
                return BadRequest("Mật khẩu mới không được giống mật khẩu cũ.");
            }

            user.Password = model.NewPassword;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpGet("GetListCategory")]
        public async Task<IEnumerable<Category>> GetCategory()
        {
            return await _dbContext.Categories.ToListAsync();
        }
        [HttpPost("addcategory")]
        public async Task<IActionResult> AddCategory([FromForm] CategoryCreateFormDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Tên món không được để trống.");

            if (model.ImageFile == null || model.ImageFile.Length == 0)
                return BadRequest("Ảnh món ăn không hợp lệ hoặc rỗng.");

            bool isExist = await _dbContext.Categories
                .AnyAsync(c => c.Id == model.Id);

            if (isExist)
                return Conflict("Id đã tồn tại. Vui lòng chọn Id khác.");

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
            var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Image");

            if (!Directory.Exists(imagePath))
                Directory.CreateDirectory(imagePath);

            var savePath = Path.Combine(imagePath, uniqueFileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            var category = new Category
            {
                Id = model.Id, 
                Name = model.Name,
                ImageUrl = uniqueFileName
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Thêm món thành công", category });
        }



        [HttpGet("GetListDish")]
        public async Task<IEnumerable<Dish>> GetDish()
        {
            return await _dbContext.Dishes
                .Include(d => d.Category) 
                .ToListAsync();
        }

        [HttpPost("AddDish")]
        public async Task<IActionResult> AddDish([FromForm] DishCreateFormDto model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return BadRequest("Ảnh món ăn không hợp lệ hoặc rỗng.");
            }

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
            var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Image");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            var savePath = Path.Combine(imagePath, uniqueFileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            var dish = new Dish
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                ImageUrl = uniqueFileName,
                Ingredients = model.Ingredients,
                Description = model.Description,
                AccountId = model.AccountId
            };

            _dbContext.Dishes.Add(dish);
            await _dbContext.SaveChangesAsync();
            var approval = new DishApproval
            {
                DishId = dish.Id,
                Status = "Pending",
                ApprovedBy = null,
                Reason = null,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.DishApprovals.Add(approval);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Thêm món ăn thành công, chờ duyệt!" });
        }
        [HttpDelete("deleteDish/{id}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var dish = await _dbContext.Dishes.FindAsync(id);

            if (dish == null)
            {
                return NotFound("Món ăn không tồn tại.");
            }

            var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Image", dish.ImageUrl);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _dbContext.Dishes.Remove(dish);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Món ăn và ảnh đã được xóa thành công." });
        }


        [HttpPost("GetDishesByCategory")]
        public async Task<ActionResult<IEnumerable<Dish>>> GetDishesByCategory([FromBody] CategoryRequest request)
        {
            var dishes = await _dbContext.Dishes
                .Where(d => d.CategoryId == request.Id)
                .Where(d => _dbContext.DishApprovals
                    .Where(a => a.DishId == d.Id)
                    .OrderByDescending(a => a.UpdatedAt)
                    .Select(a => a.Status)
                    .FirstOrDefault() == "Approved")
                .ToListAsync();

            if (!dishes.Any())
            {
                return NotFound("Không có món ăn nào đã được duyệt trong danh mục này.");
            }

            return Ok(dishes);
        }

        [HttpPut("editdish")]
        public async Task<IActionResult> EditDish([FromBody] DishEditDto dto)
        {
            var dish = await _dbContext.Dishes.FindAsync(dto.Id);

            if (dish == null)
            {
                return NotFound("Không tìm thấy món ăn.");
            }

            var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Danh mục không tồn tại.");
            }
            dish.Name = dto.Name;
            dish.CategoryId = dto.CategoryId;
            dish.ImageUrl = dto.ImageUrl;
            dish.Ingredients = dto.Ingredients;
            dish.Description = dto.Description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật món ăn thành công", dish });
        }
        [HttpPut("updatedish/{id}")]
        public async Task<IActionResult> UpdateDish(int id, [FromBody] DishEditDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var dish = await _dbContext.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound("Không tìm thấy món ăn.");
            }

            
            if (!string.IsNullOrWhiteSpace(dto.Name))
                dish.Name = dto.Name;

            if (dto.CategoryId != 0)  
                dish.CategoryId = dto.CategoryId;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                dish.ImageUrl = dto.ImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.Ingredients))
                dish.Ingredients = dto.Ingredients;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                dish.Description = dto.Description;

            _dbContext.Dishes.Update(dish);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật món ăn thành công." });
        }
        [HttpGet("user/posts")]
        public async Task<IActionResult> GetUserPosts([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("UserId không hợp lệ.");
            }

            var dishes = await _dbContext.Dishes
                .Where(d => d.AccountId == userId)
                .Include(d => d.Category)
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.Name,
                    CategoryId = d.CategoryId,
                    ImageUrl = d.ImageUrl,
                    Ingredients = d.Ingredients,
                    Description = d.Description,
                    CategoryName = d.Category.Name, 
                    Approval = _dbContext.DishApprovals
                                .Where(a => a.DishId == d.Id)
                                .OrderByDescending(a => a.UpdatedAt)
                                .Select(a => new
                                {
                                    Id = a.Id,
                                    DishId = a.DishId,
                                    Status = a.Status,
                                    ApprovedBy = a.ApprovedBy,
                                    Reason = a.Reason,
                                    UpdatedAt = a.UpdatedAt
                                })
                                .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(dishes);
        }

        [AllowAnonymous]
        [HttpGet("{file_name}")]
        public IActionResult Getipa(string file_name)
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Image", file_name);

            var stream = System.IO.File.OpenRead(path);
            stream.Position = 0;
            return File(stream, "image/png", file_name);
        }

        [HttpPost("approveDish")]
        public async Task<IActionResult> ApproveDish([FromBody] DishApprovalDto model)
        {
            if (model == null || model.DishId <= 0)
                return BadRequest("Dữ liệu duyệt không hợp lệ!");

            var approval = await _dbContext.DishApprovals.FirstOrDefaultAsync(a => a.DishId == model.DishId);

            if (approval == null)
            {
                return NotFound(new { message = $"Không tìm thấy bài viết có ID: {model.DishId}" });
            }

            approval.Status = model.Status ?? "Approved";
            approval.ApprovedBy = model.ApprovedBy;
            approval.Reason = model.Reason ?? "Duyệt thành công.";
            approval.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Bài viết ID: {model.DishId} đã được cập nhật trạng thái: {approval.Status}",
                updatedAt = approval.UpdatedAt
            });
        }





        [HttpGet("GetPendingDishes")]
        public async Task<IActionResult> GetPendingDishes()
        {
            var pendingDishes = await _dbContext.DishApprovals
            .Where(a => a.Status == "Pending")
            .Include(a => a.Dish)
            .Select(a => new
            {
                ApprovalId = a.Id,
                DishId = a.DishId,
                Name = a.Dish.Name,
                ImageUrl = a.Dish.ImageUrl,
                Ingredients = a.Dish.Ingredients,
                Description = a.Dish.Description,
                Status = a.Status,
                CreatedAt = a.CreatedAt,  
                UpdatedAt = a.UpdatedAt   
            })
            .ToListAsync();
            return Ok(pendingDishes);
        }
        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            var comment = new Comment
            {
                DishId = dto.DishId,
                UserId = dto.UserId,
                CommentText = dto.CommentText,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.DishId,
                comment.UserId,
                comment.CommentText,
                comment.Timestamp
            });
        }
        [HttpGet("GetCommentsByDish/{dishId}")]
        public async Task<IActionResult> GetCommentsByDish(int dishId)
        {
            var comments = await _dbContext.Comments
                .Where(c => c.DishId == dishId)
                .Include(c => c.Account)
                .OrderByDescending(c => c.Timestamp)
                .Select(c => new
                {
                    c.Id,
                    c.CommentText,
                    
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(c.Timestamp).DateTime,
                    Account = new
                    {
                        c.Account.Id,
                        c.Account.Name,
                        c.Account.Email 
                    }
                })
                .ToListAsync();

            return Ok(comments);
        }
        [HttpGet("searchdish")]
        public async Task<IActionResult> SearchDish([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Từ khóa không được để trống.");
            }

            var dishes = await _dbContext.Dishes
                .Include(d => d.Category)
                .Where(d => d.Name.ToLower().Contains(keyword.ToLower()))
                .Where(d => _dbContext.DishApprovals
                    .Where(a => a.DishId == d.Id)
                    .OrderByDescending(a => a.UpdatedAt)
                    .Select(a => a.Status)
                    .FirstOrDefault() == "Approved")
                .Select(d => new DishDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    CategoryId = d.CategoryId,
                    ImageUrl = d.ImageUrl,
                    Ingredients = d.Ingredients,
                    Description = d.Description,
                    CategoryName = d.Category.Name
                })
                .ToListAsync();

            if (dishes == null || dishes.Count == 0)
            {
                return NotFound("Không tìm thấy món ăn nào phù hợp.");
            }

            return Ok(dishes);
        }
        [HttpPost("addFavorite")]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteDto dto)
        {
            var exist = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.AccountId == dto.AccountId && f.DishId == dto.DishId); 

            if (exist != null)
            {
                return BadRequest("Món ăn đã được thêm vào yêu thích.");
            }

            var favorite = new Favorite
            {
                AccountId = dto.AccountId,  
                DishId = dto.DishId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Favorites.Add(favorite);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Đã thêm vào danh sách yêu thích." });
        }

        [HttpDelete("removeFavorite")]
        public async Task<IActionResult> RemoveFavorite([FromBody] FavoriteDto dto)
        {
            var favorite = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.AccountId == dto.AccountId && f.DishId == dto.DishId);  

            if (favorite == null)
            {
                return NotFound("Món ăn không tồn tại trong danh sách yêu thích.");
            }

            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Đã xoá khỏi danh sách yêu thích." });
        }

        [HttpGet("getFavorites/{accountId}")]  
        public async Task<IActionResult> GetFavorites(int accountId)
        {
            var favorites = await _dbContext.Favorites
                .Where(f => f.AccountId == accountId)  
                .Include(f => f.Dish)
                .Select(f => new
                {
                    f.Dish.Id,
                    f.Dish.Name,
                    f.Dish.ImageUrl,
                    f.Dish.Description,
                    f.Dish.Ingredients
                })
                .ToListAsync();

            return Ok(favorites);
        }
























    }


}

