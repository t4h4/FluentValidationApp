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
        public CustomerValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name alani bos olamaz.");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email alani bos olamaz.")
                .EmailAddress().WithMessage("Email dogru formatta olmalidir.");

            RuleFor(x => x.Age).NotEmpty().WithMessage("Age alani bos olamaz.").InclusiveBetween(18, 60)
                .WithMessage("Age alani 18 ile 60 arasinda olmalidir.");

        }
    }
}
