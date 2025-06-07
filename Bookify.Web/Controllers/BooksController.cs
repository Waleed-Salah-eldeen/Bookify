using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuGet.Packaging.Signing;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller 
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IImageService _imageService;

        public BooksController(ApplicationDbContext context, IMapper mapper, IOptions<CloudinarySettings> cloudinary, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;

            var account = new Account
            {
                Cloud = cloudinary.Value.CloudName,
                ApiKey = cloudinary.Value.APIKey,
                ApiSecret = cloudinary.Value.APISecret
            };

            _cloudinary = new Cloudinary(account);
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Details(int id)
        {
            var bookModel = _context.Books.Include(b => b.Author)
                                          .Include(b => b.Copies)
                                          .Include(b => b.Categories)
                                          .ThenInclude(c => c.Category)
                                          .SingleOrDefault(b => b.Id == id);

            if (bookModel is null)
                return NotFound();

            var bookViewModel = _mapper.Map<BookViewModel>(bookModel);
            return View(bookViewModel);
        }

        [HttpPost]
        public IActionResult GetBooks()
        {
            var skip = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];
            var sortColumnIndex = Request.Form["order[0][column]"];
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(searchValue))
                books = books.Where(b => b.Title.Contains(searchValue) || b.Author!.Name.Contains(searchValue));
            books = books.OrderBy($"{sortColumn} {sortColumnDirection}");

            var data = books.Skip(skip).Take(pageSize).ToList();
            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);
            var recordsTotal = books.Count();
        
            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData };
            return Ok(jsonData);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = PopulateViewModel(model);
                return View("Form", model);
            }

            var book = _mapper.Map<Book>(model);

            if(model.Image is not null)
            {

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var result = await _imageService.UploadAsync(model.Image, imageName, "/images/books", hasThumbnail: true);

                if(!result.isUploded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage: result.erorrMassage!);
                    return View("Form", PopulateViewModel(model));
                }

                book.ImageUrl = $"/images/books/{imageName}";
                book.ThumbnailImageUrl = $"/images/books/thumb/{imageName}";

                // cloudinary
                //using var stream = model.Image.OpenReadStream();
                //var imageParams = new ImageUploadParams()
                //{
                //  File = new FileDescription(imageName, stream),
                //UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);
                //book.ImageUrl = result.SecureUrl.ToString();
                //book.ThumbnailImageUrl = GetThumbnailUrl(book.ImageUrl);
                //book.ImagePublicId = result.PublicId;
            }

            book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            foreach (var categoryIdSelected in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = categoryIdSelected });

            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == id);
            if (model is null)
                return NotFound();

            var viewModel = _mapper.Map<BookFormViewModel>(model);

            viewModel = PopulateViewModel(viewModel);
            viewModel.SelectedCategories = model.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel = PopulateViewModel(viewModel);
                return View("Form", viewModel);
            }

            var book = _context.Books.Include(b => b.Categories)
                                     .Include(b => b.Copies)
                                     .SingleOrDefault(b => b.Id == viewModel.Id);

            if (book is null)
                return NotFound();

            //string imagePublicId = null;

            if (viewModel.Image is not null)
            {
                if(!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.ThumbnailImageUrl);

                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(viewModel.Image.FileName)}";
                var result = await _imageService.UploadAsync(viewModel.Image, imageName, "/images/books", hasThumbnail: true);

                if (!result.isUploded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage: result.erorrMassage!);
                    return View("Form", PopulateViewModel(viewModel));
                }

                viewModel.ImageUrl = $"/images/books/{imageName}";
                viewModel.ThumbnailImageUrl = $"/images/books/thumb/{imageName}";


                //cloudinary
                //using var stream = viewModel.Image.OpenReadStream();
                //var imageParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);
                //viewModel.ImageUrl = result.SecureUrl.ToString();
                //imagePublicId = result.PublicId;

            }
            else if(!string.IsNullOrEmpty(book.ImageUrl))
            {
                viewModel.ImageUrl = book.ImageUrl;
                viewModel.ThumbnailImageUrl = book.ThumbnailImageUrl;
            }

            book = _mapper.Map(viewModel, book);
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            book.LastUpdatedOn = DateTime.Now;
            foreach (var categoryIdSelected in viewModel.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = categoryIdSelected });

            if (!viewModel.IsAvilableForRental)
                foreach (var copy in book.Copies)
                    copy.IsAvilableForRental = false;

            //book.ThumbnailImageUrl = GetThumbnailUrl(book.ImageUrl!);
            //book.ImagePublicId = imagePublicId;

            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound();
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            book.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

        public IActionResult AllowItem(BookFormViewModel viewModel)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == viewModel.Title && b.AuthorId == viewModel.AuthorId);
            var isAllow = book is null || book.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            //var viewModel = model is null ? new BookFormViewModel() : model;     //Conditional operator

            var viewModel = model;
            viewModel ??= new BookFormViewModel();    //null-coalescing assignment operator

            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }

        private string GetThumbnailUrl(string imageUrl)
        {
            var thumbnailSection = "c_thumb,w_200,g_face/";
            var separator = "image/upload/";
            var urlParts = imageUrl.Split(separator);
            var thumbnailUrl = $"{urlParts[0]}{separator}{thumbnailSection}{urlParts[1]}";
            return thumbnailUrl;
        }
    }
}
