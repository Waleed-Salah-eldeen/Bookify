using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles =AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IEmailSender email, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModel);
        }

        [HttpGet,AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserFormViewModel
            {
                Roles = await _roleManager.Roles.Select(r => 
                                                        new SelectListItem 
                                                        { 
                                                            Text = r.Name, Value = r.Name 
                                                        
                                                        }).ToListAsync()           
            };
            return PartialView("_Form" , viewModel);
        }
        
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel formViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = new ApplicationUser
            {
                FullName = formViewModel.FullName,
                UserName = formViewModel.UserName,
                Email = formViewModel.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            var result = await _userManager.CreateAsync(user, formViewModel.Password);
            if(result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, formViewModel.SelectedRoles);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code },
                    protocol: Request.Scheme);

                var placeholders = new Dictionary<string, string>()
                {
                    { "header", $"Hey {user.FullName}, thanks for joining us!" },
                    { "body", "please confirm your email" },
                    { "url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" },
                    { "linkTitle", "Active Account!" }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var viewModel = _mapper.Map<UserFormViewModel>(user);
            viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
            viewModel.Roles = await _roleManager.Roles.Select(r => 
                                    new SelectListItem { Text = r.Name, Value = r.Name }).ToListAsync();
            
            return PartialView("_Form", viewModel);

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel formViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByIdAsync(formViewModel.Id);
            if (user is null)
                return NotFound();

            user = _mapper.Map(formViewModel, user);
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesUpdated = !currentRoles.SequenceEqual(formViewModel.SelectedRoles);
                if(rolesUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, formViewModel.SelectedRoles);
                }

				await _userManager.UpdateSecurityStampAsync(user);

				var userViewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", userViewModel);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [HttpGet,AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswordFormViewModel { Id = id };
            return PartialView("ResetPasswordForm", viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user is null)
                return NotFound();
            var oldPasswordHash = user.PasswordHash;
            
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, viewModel.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;
                await _userManager.UpdateAsync(user);

                var userViewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", userViewModel);
            }
            
            user.PasswordHash = oldPasswordHash;
            await _userManager.UpdateAsync(user);
            return BadRequest(string.Join(',', result.Errors.Select(r => r.Description)));
        }

        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);
            if (user.IsDeleted)
               await _userManager.UpdateSecurityStampAsync(user);
            return Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
               await _userManager.SetLockoutEndDateAsync(user, null);
            return Ok();
        }


        public async Task<IActionResult> AllowUserName(UserFormViewModel formViewModel)
        {
            var user = await _userManager.FindByNameAsync(formViewModel.UserName);
            var allow = user is null || user.Id.Equals(formViewModel.Id);
            return Json(allow);
        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel formViewModel)
        {
            var user = await _userManager.FindByEmailAsync(formViewModel.Email);
            var allow = user is null || user.Id.Equals(formViewModel.Id);
            return Json(allow);
        }
    }
}