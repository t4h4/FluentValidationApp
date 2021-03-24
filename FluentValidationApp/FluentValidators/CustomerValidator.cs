using FluentValidation;
using FluentValidationApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentValidationApp.FluentValidators
{
    //servis olarak startup'a eklememiz gerek.
    //AbstractValidator generic sınıf. generic olacak neyle uğraşacağını belirtmemiz gerek. 
    public class CustomerValidator : AbstractValidator <Customer>
    {
        public string NotEmptyMessage { get; } = "{PropertyName} alani boş olamaz.";
        public CustomerValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(NotEmptyMessage);

            RuleFor(x => x.Email).NotEmpty().WithMessage(NotEmptyMessage)
                .EmailAddress().WithMessage("Email dogru formatta olmalidir.");

            RuleFor(x => x.Age).NotEmpty().WithMessage(NotEmptyMessage).InclusiveBetween(18, 60)
                .WithMessage("Age alani 18 ile 60 arasinda olmalidir.");

            //custom validator kullanmak için must metodunu kullan.
            //custom hatalar client tarafta görülmez. mutlaka verinin server tarafa gitmesi gerekir.
            //x girilen değeri alıyor.
            RuleFor(x => x.Birthday).NotEmpty().WithMessage(NotEmptyMessage).Must(x =>
            {
                return DateTime.Now.AddYears(-18) >= x; //şu anki yıldan 18 yıl geriye git, x'ten büyük veya eşitse true yoksa false. False ise 18 yaşından küçüktür. 
            }).WithMessage("Yaşınız 18 yaşından büyük olmalıdır");

            RuleForEach(x => x.Addresses).SetValidator(new AddressValidator());

            RuleFor(x => x.Gender).IsInEnum().WithMessage("{PropertyName} alani Erkek=1, Bayan=2 olmalidir.");
        }
    }
}
