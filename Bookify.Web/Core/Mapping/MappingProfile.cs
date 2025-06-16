using Bookify.Web.Core.ViewModels.UsersViewModels;

namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(des => des.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.Text, opt => opt.MapFrom(src => src.Name));


            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(des => des.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.Text, opt => opt.MapFrom(src => src.Name));

            //Books
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(b => b.Categories, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
                .ForMember(des => des.Author, opt => opt.MapFrom(src => src.Author!.Name))
                .ForMember(des => des.Categories,
                opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));


            //BookCopy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(des => des.BookTitle, opt => opt.MapFrom(src => src.Book!.Title));

            CreateMap<BookCopy, BookCopyFormViewModel>();

            // Users
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>()
                .ForMember(des => des.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(des => des.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ReverseMap();


			//Governorates & Areas
			CreateMap<Governorate, SelectListItem>()
				.ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

			CreateMap<Area, SelectListItem>()
				.ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

			//Subscribers
			CreateMap<Subscriber, SubscriberFormViewModel>().ReverseMap();

			CreateMap<Subscriber, SubscriberSearchResultViewModel>()
				.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

			CreateMap<Subscriber, SubscriberViewModel>()
				.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
				.ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
				.ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<Subscription, SubscriptionViewModel>();
        }
    }
}
