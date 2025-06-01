namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }


        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel{ BookId = bookId, ShowRentalInput = book.IsAvilableForRental };
            return PartialView("Form", viewModel);
        }
        
        [HttpPost,ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel FormViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(FormViewModel.BookId);
            if (book is null)
                return NotFound();

            BookCopy copy = new()
            {
                BookId = book.Id,
                EditionNumber = FormViewModel.EditionNumber,
                IsAvilableForRental = book.IsAvilableForRental && FormViewModel.IsAvilableForRental,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            _context.BookCopies.Add(copy);
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);   
        }


        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var bookCopy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
            if (bookCopy is null)
                return NotFound();
            var formViewModel = _mapper.Map<BookCopyFormViewModel>(bookCopy);
            formViewModel.ShowRentalInput = bookCopy.Book!.IsAvilableForRental;
            return PartialView("Form", formViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel formViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var bookCopy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == formViewModel.Id);
            if (bookCopy is null)
                return NotFound();

            bookCopy.EditionNumber = formViewModel.EditionNumber;
            bookCopy.IsAvilableForRental = bookCopy.Book!.IsAvilableForRental && formViewModel.IsAvilableForRental;
            bookCopy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            bookCopy.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(bookCopy);
            return PartialView("_BookCopyRow", viewModel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            copy.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }
    }
}
