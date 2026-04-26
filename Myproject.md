# Myproject — Source overview and component map

Project: `Test Api`  
Root namespace: `Test_Api`  
Target: .NET 8, C# 12

This document describes project structure, data model, DB contexts, controllers/endpoints, how components link, and quick notes for maintenance.

---

## Project structure (important files / folders)
- `Program.cs` — app startup, registers two DbContexts and Swagger, maps controllers.
- `Context\UserDbContext.cs` — user / role / group domain DbContext.
- `Context\CookingDbContext.cs` — cooking app domain DbContext (accounts, categories, dishes, approvals, comments, favorites).
- `Controllers\AppController.cs` — main cooking-related API (register / login / dishes / categories / comments / favorites / image serve / approvals).
- `Controllers\UserController.cs` — user management (CRUD, assign user to role(s), group membership endpoints).
- `Controllers\RoleController.cs` — role CRUD endpoints.
- `Controllers\GroupController.cs` — group management (group CRUD and user membership) — model present; controller expected.
- `Models\*` — domain models: `User`, `Role`, `RoleUser`, `Group`, `GroupUser`, `Account`, `Category`, `Dish`, `DishApproval`, `Comment`, `Favorite`, DTOs (RegisterModel, LoginModel, ChangePasswordModel, Dish DTOs, Category DTOs, etc.).

---

## Data models (summary)

- `User` (in `Models\User.cs`)
  - Fields: `Id`, `ten`, `tuoi`
  - Navigation: `IList<RoleUser> RoleUser`, `IList<GroupUser> GroupUser`
  - Used by `UserDbContext`.

- `Role`
  - Fields: `Id`, `Rolename`
  - Navigation: `IList<RoleUser> RoleUser` (JsonIgnore)

- `RoleUser`
  - Composite join: `(UserId, RoleId)`
  - Nav: `user`, `role` (JsonIgnore on nav props)

- `Group`
  - Fields: `Id`, `Name`
  - Navigation: `IList<GroupUser> GroupUser` (JsonIgnore)

- `GroupUser`
  - Composite join: `(GroupId, UserId)`
  - Nav: `group`, `user`

- `Account` (Cooking domain)
  - Fields: `Id`, `Email`, `Name`, `Password`, `Role`
  - Navigation: `ICollection<Dish> Dishes`, `ICollection<Favorite> Favorites`

- `Category`
  - Key `Id`, `Name`, `ImageUrl`
  - Navigation: `ICollection<Dish> Dishes`

- `Dish`
  - Fields: `Id`, `Name`, `CategoryId`, `ImageUrl`, `Ingredients`, `Description`, `AccountId`
  - Navigation: `Account Account`, `Category Category`

- `DishApproval`
  - Tracks approve flow: `DishId`, `Status`, `ApprovedBy`, `Reason`, `UpdatedAt`, `CreatedAt`
  - Navigation: `Dish Dish`

- `Comment`
  - `DishId`, `UserId`, `CommentText`, `Timestamp`, nav to `Account`, `Dish`

- `Favorite`
  - `AccountId`, `DishId`, `CreatedAt`, nav to `Account` and `Dish`

---

## DbContexts and relationships

- `UserDbContext`
  - DbSets: `Users`, `Roles`, `RoleUsers`, `Groups`, `GroupUsers`.
  - Maps many-to-many via `RoleUser` and `GroupUser` with composite keys.
  - Note: `OnModelCreating` contains duplicated / repeated configuration for `GroupUser` keys and relations — safe but redundant.

- `CookingDbContext`
  - DbSets: `Users` (mapped to `Account` entity), `Categories`, `Dishes`, `DishApprovals`, `Comments`, `Favorites`.
  - Relationships:
    - `Dish` -> `Category` (many-to-one) cascade delete.
    - `Dish` -> `Account` (many-to-one) cascade delete.
    - `DishApproval` -> `Dish` (one-to-many-ish) cascade delete.
    - `Comment` -> `Account`, `Comment` -> `Dish` (cascade delete).
    - `Favorite` -> `Account` (with collection), `Favorite` -> `Dish`.

---

## Controllers & key endpoints (summary)

- `AppController` (`[Route("api/[controller]")]`)
  - Auth / Account:
    - `POST api/app/register` — register `Account` (`RegisterModel`).
    - `POST api/app/login` — login, returns fake token and user details.
    - `POST api/app/changepassword` — change password for `Account`.
  - Category:
    - `GET api/app/GetListCategory` — list categories.
    - `POST api/app/addcategory` — add category with `IFormFile` image; saves file to `Image` folder.
  - Dish:
    - `GET api/app/GetListDish` — list dishes (includes category).
    - `POST api/app/AddDish` — add dish via form; stores image; creates `DishApproval` with status `Pending`.
    - `DELETE api/app/deleteDish/{id}` — delete dish and image.
    - `POST api/app/GetDishesByCategory` — list approved dishes by category.
    - `PUT api/app/editdish` and `PUT api/app/updatedish/{id}` — dish update endpoints.
    - `GET api/app/user/posts?userId={id}` — list posts by account with latest approval info.
  - Images:
    - `GET api/app/{file_name}` — serves image files from `Image` folder.
  - Approval:
    - `POST api/app/approveDish` — update `DishApproval`.
    - `GET api/app/GetPendingDishes` — list pending approvals.
  - Comments:
    - `POST api/app/AddComment` — add comment to dish.
    - `GET api/app/GetCommentsByDish/{dishId}` — list comments (includes account).
  - Search & Favorites:
    - `GET api/app/searchdish?keyword=...` — search approved dishes.
    - `POST api/app/addFavorite` — add favorite.
    - `DELETE api/app/removeFavorite` — remove favorite.
    - `GET api/app/getFavorites/{accountId}` — get favorites for account.

- `UserController` (`[Route("[controller]")]`)
  - `GET /User/GetListUser` — returns users with roles (RoleUser -> Role).
  - `POST /User/AddUser` — add `User` (internal user domain).
  - `DELETE /User/DeleteUser/{id}` — remove user.
  - `PUT /User/UpdateUser/{id}` — update user fields.
  - `POST /User/AddUserInRole` — assign role via `RoleUser` (expected).
  - Group related endpoints expected for managing groups and membership (model present).

- `RoleController` (`[Route("api/[controller]")]`)
  - `GET api/role/GetRole` — all roles.
  - `POST api/role/AddRole` — add role (check `Id` uniqueness).
  - `DELETE api/role/Deleterole/{id}` — delete role.
  - `PUT api/role/UpdateRole/{id}` — update role (current implementation removes and re-adds; likely should update in-place).

---

## How features link (data flow)
- User domain:
  - `User`, `Role` connected through `RoleUser` many-to-many.
  - `Group` membership tracked via `GroupUser` join table.
- Cooking domain:
  - `Account` owns `Dishes`. Creating a `Dish` triggers an initial `DishApproval` record (status `Pending`).
  - Approval flow: admins call `approveDish` to set `DishApproval.Status` to `Approved`/`Rejected`. Many query endpoints filter by latest approval status (via ordering by `UpdatedAt`).
  - `Comments` and `Favorites` reference `Account` and `Dish`; cascade deletes defined that remove dependent records when `Dish` or `Account` deleted.
- Images:
  - Image files are stored under runtime `Image` directory (content root) and served by `AppController`.

---

## Program.cs behavior
- Configures app to use URLs `http://0.0.0.0:5018`.
- Registers controllers, Swagger in Development.
- Adds two DbContexts using connection strings:
  - `InternConnection` -> `UserDbContext`
  - `CookingAppConnection` -> `CookingDbContext`

---

## Observations & quick recommendations
- `UserDbContext.OnModelCreating` contains duplicated calls (multiple `HasKey` and duplicated relationship mapping). Clean duplicates to reduce confusion.
- `RoleController.UpdateRole` removes then re-adds role — loses relationships and is risky. Prefer fetching entity and updating fields, then SaveChanges.
- `Account` DbSet named `Users` in `CookingDbContext` (table mapped to `"Account"`). This naming is workable but may be confusing; consider renaming DbSet to `Accounts` for clarity.
- Passwords are stored in plaintext (`Account.Password`); strongly recommend hashing and salting (e.g., using ASP.NET Core Identity or a secure hashing algorithm).
- Authentication is currently returning a "fake token". Replace with JWT or other secure auth as required.
- Ensure `Image` folder write permissions in hosting environment; validate and sanitize file uploads.

---

## Useful file map (paths)
- `Program.cs`
- `Context\UserDbContext.cs`
- `Context\CookingDbContext.cs`
- `Controllers\AppController.cs`
- `Controllers\UserController.cs`
- `Controllers\RoleController.cs`
- `Controllers\GroupController.cs` (expected)
- `Models\Account.cs`
- `Models\Category.cs`
- `Models\Dish.cs`
- `Models\DishApproval.cs`
- `Models\Comment.cs`
- `Models\Favorite.cs`
- `Models\User.cs`
- `Models\Role.cs`
- `Models\RoleUser.cs`
- `Models\Group.cs`
- `Models\GroupUser.cs`

---

If you want, I can:
- Generate a UML/mermaid diagram of model relationships.
- Produce a concise OpenAPI/Swagger summary of endpoints.
- Create a cleaned `Myproject.md` in a different format or translate to Vietnamese.
