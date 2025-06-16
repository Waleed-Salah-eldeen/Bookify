using Bookify.Web.Core.Models;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
	public class SubscribersController : Controller
	{
		private readonly IDataProtector _dataProtector;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscribersController(IDataProtectionProvider dataProtectionProvider,ApplicationDbContext context,
			IMapper mapper, IImageService imageService, 
			IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
		{
			_dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
            _context = context;
			_mapper = mapper;
			_imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

		public IActionResult Index()
		{
			return View();
		}


		public IActionResult Create()
		{
			var viewModel = PopulateViewModel();
			return View("Form", viewModel);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SubscriberFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", PopulateViewModel(model));

			var subscriber = _mapper.Map<Subscriber>(model);

			// Start working with the image
			var extension = Path.GetExtension(model.Image!.FileName);
			var imageName = $"{Guid.NewGuid()}{extension}";
			var imagePath = "/images/subscribers";
			var result = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

			if (!result.isUploded)
			{
				ModelState.AddModelError(nameof(model.Image), result.erorrMassage!);
				return View("Form", PopulateViewModel(model));
			}
			// End working with the image

			subscriber.ImageUrl = $"{imagePath}/{imageName}";
			subscriber.ThumbnailImageUrl = $"{imagePath}/thumb/{imageName}";
			subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

			subscriber.Subscriptions.Add(subscription);

            _context.Add(subscriber);
			_context.SaveChanges();

            //Send welcome email
            var placeholders = new Dictionary<string, string>()
            {
                { "header", $"Welcome {model.FirstName}," },
                { "body", "thanks for joining Bookify 🤩" }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

			BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
				model.Email,
				"Welcome to Bookify", body));
            
            //

            var key = _dataProtector.Protect(subscriber.Id.ToString());
			return RedirectToAction(nameof(Details), new { id = key });
		}

		[HttpGet]
		public IActionResult Edit(string id)
		{
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers.Find(subscriberId);

			if (subscriber is null)
				return NotFound();
			var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
			viewModel.Key = id;
			return View("Form", PopulateViewModel(viewModel));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SubscriberFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", PopulateViewModel(model));
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));

            var subscriber = _context.Subscribers.Find(subscriberId);

			if (subscriber is null)
				return NotFound();

			if (model.Image is not null)
			{
				if (!string.IsNullOrEmpty(subscriber.ImageUrl))
					//Detete the old image
					_imageService.Delete(subscriber.ImageUrl, subscriber.ThumbnailImageUrl);

				// start store the new image
				var extension = Path.GetExtension(model.Image!.FileName);
				var imageName = $"{Guid.NewGuid()}{extension}";
				var imagePath = "/images/subscribers";
				var result = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

				if (!result.isUploded)
				{
					ModelState.AddModelError(nameof(model.Image), result.erorrMassage!);
					return View("Form", PopulateViewModel(model));
				}
				// End store the new image

				model.ImageUrl = $"{imagePath}/{imageName}";
				model.ThumbnailImageUrl = $"{imagePath}/thumb/{imageName}";
			}
			else if(!string.IsNullOrEmpty(subscriber.ImageUrl))
			{
				model.ImageUrl = subscriber.ImageUrl;
				model.ThumbnailImageUrl = subscriber.ThumbnailImageUrl;
			}

			subscriber = _mapper.Map(model, subscriber);
			subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			subscriber.LastUpdatedOn = DateTime.Now;

			_context.SaveChanges();
			return RedirectToAction(nameof(Details), new { id = model.Key });
		}

		[HttpPost,ValidateAntiForgeryToken]
		public IActionResult Search(SearchFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var subscriber = _context.Subscribers
							.SingleOrDefault(s =>
									s.Email == model.Value
								|| s.NationalId == model.Value
								|| s.MobileNumber == model.Value);

			var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
			if (subscriber is not null)
				viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

                return PartialView("_Result", viewModel);
		}

		public IActionResult Details(string id)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(id));
			var subscriber = _context.Subscribers.Include(s => s.Governorate)
										  .Include(s => s.Area)
										  .Include(s => s.Subscriptions)
										  .SingleOrDefault(s => s.Id == subscriberId);

			if (subscriber is null)
				return NotFound();

			var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
			viewModel.Key = id;
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RenewSubscription(string sKey)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

			var subscriber = _context.Subscribers
										.Include(s => s.Subscriptions)
										.SingleOrDefault(s => s.Id == subscriberId);

			if (subscriber is null)
				return NotFound();

			if (subscriber.IsBlackListed)
				return BadRequest();

			var lastSubscription = subscriber.Subscriptions.Last();

			var startDate = lastSubscription.EndDate < DateTime.Today
							? DateTime.Today
							: lastSubscription.EndDate.AddDays(1);

			Subscription newSubscription = new()
			{
				CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
				CreatedOn = DateTime.Now,
				StartDate = startDate,
				EndDate = startDate.AddYears(1)
			};

			subscriber.Subscriptions.Add(newSubscription);

			_context.SaveChanges();

			//Send email and WhatsApp Message
			var placeholders = new Dictionary<string, string>()
			{
				{ "header", $"Hello {subscriber.FirstName}," },
				{ "body", $"your subscription has been renewed through {newSubscription.EndDate.ToString("d MMM, yyyy")} 🎉🎉" }
			};

			var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

			BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Bookify Subscription Renewal", body));

            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", viewModel);

        }

        [HttpGet]
		[AjaxOnly]
		public IActionResult GetAreas(int governorateId)
		{
			var areas = _context.Areas.Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
						.OrderBy(g => g.Name).ToList();

			return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
		}


		private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
		{
			var viewModel = model ?? new SubscriberFormViewModel();

			var governorates = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
			viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

			if (model?.GovernorateId > 0)
			{
				var areas = _context.Areas
					.Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
					.OrderBy(a => a.Name)
					.ToList();

				viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
			}

			return viewModel;
		}

		public IActionResult AllowEmail(SubscriberFormViewModel viewModel)
		{
			int subscriberid = 0;
			if (!string.IsNullOrEmpty(viewModel.Key))
				subscriberid = int.Parse(_dataProtector.Unprotect(viewModel.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.Email == viewModel.Email);
			var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberid);
			return Json(isAllowed);
		}

		public IActionResult AllowMobileNumber(SubscriberFormViewModel viewModel)
		{
            int subscriberid = 0;
            if (!string.IsNullOrEmpty(viewModel.Key))
                subscriberid = int.Parse(_dataProtector.Unprotect(viewModel.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == viewModel.MobileNumber);
			var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberid);
			return Json(isAllowed);
		}

		public IActionResult AllowNationalId(SubscriberFormViewModel viewModel)
		{
            int subscriberid = 0;
            if (!string.IsNullOrEmpty(viewModel.Key))
                subscriberid = int.Parse(_dataProtector.Unprotect(viewModel.Key));

            var Subscriber = _context.Subscribers.SingleOrDefault(b => b.NationalId == viewModel.NationalId);
			var isAllowed = Subscriber is null || Subscriber.Id.Equals(subscriberid);

			return Json(isAllowed);
		}
	}
}
