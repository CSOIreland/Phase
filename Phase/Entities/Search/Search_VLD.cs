using FluentValidation;
namespace Phase
{
    public class Search_VLD: AbstractValidator<Search_DTO>
    {
        public Search_VLD() 
        {
            RuleFor(x => x.LngIsoCode).NotEmpty().Length(2);
            RuleFor(x => x.Search).NotNull().NotEmpty();
        }
    }
}
