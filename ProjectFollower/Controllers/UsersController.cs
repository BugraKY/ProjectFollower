﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectFollower.DataAcces.IMainRepository;
using ProjectFollower.Models.DbModels;
using ProjectFollower.Models.ViewModels;
using static ProjectFollower.Utility.ProjectConstant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ProjectFollower.Utility;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Http;

namespace ProjectFollower.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _hostEnvironment;

        public UsersController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UsersController> logger,
        IUnitOfWork uow,
        IWebHostEnvironment hostEnvironment
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _uow = uow;
            _hostEnvironment = hostEnvironment;
        }
        [BindProperty]
        public UserRegisterVM Input { get; set; }

        [Route("users")]
        public IActionResult Index()
        {
            
            #region Authentication Index
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(i => i.Id == Claims.Value);
            }
            else
                return View("SignIn");
            #endregion Authentication Index
            
            return View();
        }
        [Route("new-user")]
        public IActionResult NewUser()
        {
            /*
            #region Authentication Index
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(i => i.Id == Claims.Value);
            }
            else
                return View("SignIn");
            #endregion Authentication Index
            */

            var ModalMessage = new ModalMessageVM()
            {
                Message = "",
                Icon = "",
                Status = false
            };
            var addUserStatus = new UserRegisterVM()
            {
                ModalMessage = ModalMessage
            };

            return View(addUserStatus);
        }

        [HttpPost("new-user")]
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.Personel))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Personel));
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;


                Console.WriteLine(files.Count.ToString());
                //System.Diagnostics.Debug.WriteLine(files.ToString());
                
                if (files.Count() > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, LocFilePaths.DIR_Users_Img);

                    #region Check Users Directories
                    if (!(Directory.Exists(LocFilePaths.RootAsset)))
                        Directory.CreateDirectory(LocFilePaths.RootAsset);
                    if (!(Directory.Exists(LocFilePaths.DIR_Users_Main)))
                        Directory.CreateDirectory(LocFilePaths.DIR_Users_Main);
                    if (!(Directory.Exists(LocFilePaths.DIR_Users_Img)))
                        Directory.CreateDirectory(LocFilePaths.DIR_Users_Img);
                    #endregion Check Users Directories


                    //var imageUrl = productVM.Product.ImageUrl;

                    var extension = Path.GetExtension(files[0].FileName);

                    /*
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }*/

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        var stream=files[0].OpenReadStream();
                        var image = Image.FromStream(stream);
                        var size = image.Size;
                        var width = (float)size.Width;
                        var height = (float)size.Height;

                        float rate = width / height;

                        if (width > 200 || height > 200)
                        {
                            ModelState.AddModelError(string.Empty, "Kullanıcı oluşturulamadı! Profil resmi 200px den fazla olamaz.");
                            return View("NewUser");
                        }
                        if (rate !=1)
                        {
                            ModelState.AddModelError(string.Empty, "Kullanıcı oluşturulamadı! Profil resmi 1:1 oranında olmalıdır. ");
                            return View("NewUser");
                        }

                        files[0].CopyTo(fileStreams);
                    }
                    Input.ImageUrl = @"\images\users\" + fileName + extension;
                }
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    Lastname = Input.Lastname,
                    DepartmentId=Input.DepartmentId,
                    EmailConfirmed=true,
                    ImageUrl=Input.ImageUrl
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kayıt işlemi yapıldı.");

                    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Personel))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Personel));
                    }

                    if (User.IsInRole(UserRoles.Admin))
                    {
                        await _userManager.AddToRoleAsync(user, UserRoles.Personel);
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {

                        var ModalMessage = new ModalMessageVM()
                        {
                            Message="Kullanıcı başarılı bir şekilde eklendi.",
                            Icon= "success",
                            Status=true
                        };
                        var addUserStatus = new UserRegisterVM()
                        {
                            ModalMessage=ModalMessage
                        };
                        return View("NewUser",addUserStatus);
                    }
                    else
                    {
                        return View();
                    }
                    
                }


                foreach (var error in result.Errors)
                {
                    if(error.Code== "DuplicateUserName")
                    {
                        var ModalMessage = new ModalMessageVM()
                        {
                            Message = "Doğrulama hatası",
                            Icon = "error",
                            Status = true
                        };
                        var addUserStatus = new UserRegisterVM()
                        {
                            ModalMessage = ModalMessage
                        };
                        return View("NewUser", addUserStatus);
                    }
                    //


                    //ModelState.AddModelError(string.Empty, error.Description);

                    return View("NewUser");

                }

            }
            // If we got this far, something failed, redisplay form
            foreach (var item in ModelState.Values)
            {
                if (item.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                {
                    ModelState.AddModelError(string.Empty, item.Errors[0].ErrorMessage);
                    var ModalMessage = new ModalMessageVM()
                    {
                        Message = item.Errors[0].ErrorMessage,
                        Icon = "warning",
                        Status = true
                    };
                    var addUserStatus = new UserRegisterVM()
                    {
                        ModalMessage = ModalMessage
                    };
                    return View("NewUser", addUserStatus);
                }
            }

            return View("NewUser");


        }

        #region API
        [HttpGet("jsonresult/getdepartmentsjson")]
        public JsonResult GetDepartments()
        {
            /*
            #region Authentication Index
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(i => i.Id == Claims.Value);
            }
            else
                return Json(StatusCode(404));
            #endregion Authentication Index
            */
            var Departments = _uow.Department.GetAll();
            return Json(Departments);
        }

        
        [HttpGet("jsonresult/getalluserjson")]
        public JsonResult GetAllUser()
        {
            List<Users> _Users = new List<Users>();

            
            #region Authentication Index
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var users = _uow.ApplicationUser.GetAll(includeProperties: "Department");
                foreach (var item in users)
                {
                    Users Useritem = new Users()
                    {
                        AppUserName = item.AppUserName,
                        DepartmentId = item.DepartmentId,
                        Department = item.Department,
                        FullName = item.FirstName+" "+ item.Lastname,
                        IdentityNumber = item.IdentityNumber,
                        ImageUrl = item.ImageUrl
                    };
                    _Users.Add(Useritem);
                }
            }
            else
                return Json(StatusCode(404));
            #endregion Authentication Index
            


            //_Users.AddRange((IEnumerable<Users>)(Users)users);


            return Json(_Users);
        }
        #endregion API
    }
}
