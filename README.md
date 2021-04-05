Separation of Concern (SoC) prensibini benimseyen .net property doğrulama kütüphanesidir. Doğrulama işlemlerini ayrı bir class üzerinden gerçekleştirmeye olanak verdiğinden dolayı SoC’ye uygundur. Property doğrulama işlemleri ayrı yerde yazdıldığından dolayı, unit test konusunda developer kişilere büyük kolaylık sağlar. Ayrıca bu kütüphane, attribute tarafında kullanılan doğrulama ifadelerine göre oldukça geniş bir hazır doğrulama yelpazesine sahiptir. Hazır doğrulama kütüphanesi dışında custom doğrulama metodları yazmayı da kolay kılan bir kütüphanedir, FluentValidation. MVC mimarisinde çift taraflı doğrulama mantığında çalışır. Client tarafta JavaScript ile doğrulama yaparken, server tarafında da doğrulama yapabilir. Kullanıcı JS’i kapatsa bile tarayıcısında, server taraflı doğrulama aktif olarak kalır. API mantığında sadece server taraflı doğrulama vardır.

Commit: [CustomerValidator sınıfı eklendi.](https://github.com/t4h4/FluentValidationApp/commit/9d617d0454b66a21feb1318dbaef72c5c8cddfb7)

Eklenen kurallar: 
* Name alani bos olamaz.
* Email alani bos olamaz.
* Email dogru formatta olmalidir.
* Age alani bos olamaz.
* Age alani 18 ile 60 arasinda olmalidir.

Yapılanlar:
* fluentvalidation.aspnetcore\9.5.3\ paketi yüklendi. 
* Ana dizine FluentValidators klasörü oluşturuldu. Bu klasörün altına CustomerValidator.cs class yapısı eklendi. Class yapısında kurallar oluşturuldu.
* Başlangıçta yalnızca customer entity'si validasyona tabii olacağı için sadece o startup.cs'de AddSingleton olarak eklendi. 

````c#
services.AddSingleton<IValidator<Customer>, CustomerValidator>();
// IValidator interface'i üzerinden generic olarak customer'ımı al, böyle bir interface örneğiyle karşılaşırsan
// CustomerValidator'dan nesne örneği al. 
````
* Daha sonrası için çoklu entity validasyonuna ihtiyaç duyulur diye, hepsini teker teker ekleme iş yükünden tasarruf olması amacıyla aşağıdaki koda refactor edildi. 
````c#
services.AddControllersWithViews().AddFluentValidation(options =>
{
    options.RegisterValidatorsFromAssemblyContaining<Startup>();
});        
````

***


Commit: [Özel validator eklendi.](https://github.com/t4h4/FluentValidationApp/commit/6f6900f58b71f447cb4408ab3a79a1190a0a0418)

Eklenen kural: 
* Yaşınız 18 yaşından büyük olmalıdır.

Yapılanlar:
* Entity yapısına DateTime türünde  Birthday props eklendi.
* Kural detay yapısı aşağıdaki şekildedir.
````c#
//custom validator kullanmak için must metodunu kullan.
//custom hatalar client tarafta görülmez. mutlaka verinin server tarafa gitmesi gerekir.
//x girilen değeri alıyor.
RuleFor(x => x.Birthday).NotEmpty().WithMessage(NotEmptyMessage).Must(x =>
{
    return DateTime.Now.AddYears(-18) >= x; //şu anki yıldan 18 yıl geriye git, x'ten büyük veya eşitse true yoksa false. False ise 18 
                                            //yaşından küçüktür. 
}).WithMessage("Yaşınız 18 yaşından büyük olmalıdır");
````
***
Commit: [IValidator interface validate() method kullanıldı.  ( if (ModelState.IsValid) yerine )](https://github.com/t4h4/FluentValidationApp/commit/611e30c6bce04992ed344a54cbe80db1dc84a389)

Yapılanlar: 
* Normalde fluent api model state'i kontrol edebiliyor. ona göre valid olma durumuna göre veri ekleniyor.
````c#
if (ModelState.IsValid)
{
    _context.Add(customer);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
return View(customer);       
````    
* Bunun yerine IValidator interface'ine ait validate() method kullanıldı. Bunun için controller içerisinde DI işlemi yapıldı. 
````c#
private readonly AppDbContext _context;
private readonly IValidator<Customer> _customerValidator;

public CustomersController(AppDbContext context, IValidator<Customer> customerValidator)
{
    _context = context;
    _customerValidator = customerValidator;
}     
````  
* DI sonrası aşağıdaki operasyonla beklenen sağlandı.
````c#
public async Task<IActionResult> Create([Bind("Name,Email,Age,Birthday")] Customer customer)
{
    var result = _customerValidator.Validate(customer);
    if (result.IsValid)
    {
        _context.Add(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(customer);
}
````
***
Commit: [One-Many entity üzerinde validation işlemi yapıldı. ( Bir costumer'ın birden fazla adresi olabilir. )](https://github.com/t4h4/FluentValidationApp/commit/a0033fe4073b2e938aff57a99f48ccf6607b26ec)

* Model içine yeni Address entity yapısı oluşturuldu ve Customer entity'si ile one to many ilişkisi kuruldu.
````c#
public class Address
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string Province { get; set; }
    public string PostCode { get; set; }
    public virtual Customer Customer { get; set; } //navigation property
}     
````
* Customer entity tarafı ise;
````c#
public class Customer
      {
          public int Id { get; set; }
          public string Name { get; set; }
          public string Email { get; set; }
          public int Age { get; set; }
          public DateTime? Birthday { get; set; }
          public IList<Address> Addresses { get; set; }
       }     
````
* FluentValidators adlı klasörün içinde AddressValidator.cs class yapısı oluşturuldu ve aşağıdaki kurallar eklendi.
````c#
public string NotEmptyMessage { get; } = "{PropertyName} alanı boş olamaz";

public AddressValidator()
{
    RuleFor(x => x.Content).NotEmpty().WithMessage(NotEmptyMessage);
    RuleFor(x => x.Province).NotEmpty().WithMessage(NotEmptyMessage);
    RuleFor(x => x.PostCode).NotEmpty().WithMessage(NotEmptyMessage).MaximumLength(5).WithMessage("{PropertyName} alanı en fazla {MaxLength} karakter olmalıdır. ");
}      
```` 
* CustomerValidator.cs tarafında ise aşağıdaki işlem yapılıp, bağlantı sağlandı.

````c#
RuleForEach(x => x.Addresses).SetValidator(new AddressValidator());
````

***

Commit: [API Controller, custom hata mesajı özelliği eklendi.](https://github.com/t4h4/FluentValidationApp/commit/8e5e79bda15a9d83385ccf0c03ae3547c0f4e8d7)

* Post method için error name ve property name alanlarının gösterildiği hata mesajını göndermek için yapı kurmak istedik.
* Öncelikle CustomersApiController.cs class yapısını oluşturduk daha sonra ilgili yeri aşağıdaki şekilde güncelledik. 
````c#

[HttpPost]
public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
{
    var result = _customerValidator.Validate(customer);

    if (!result.IsValid)
    {
        return BadRequest(result.Errors.Select(x => new
        {
            property = x.PropertyName,
            error = x.ErrorMessage
        }));   //isimsiz class oluşturup yaptık.
    }

    _context.Customers.Add(customer);
    await _context.SaveChangesAsync();

    return CreatedAtAction("GetCustomer", new { id = customer.Id }, customer);
}

````
* Daha sonra Startup.cs dosyasında aşağıdaki gerekli ayarlamayı yaptık. 
````c#
services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; //model state filtresini invalid et, engelle, custom hata mesajı gönderebilmek için.
}
````           
***
Commit: [IsInEnum() validator kullanıldı.](https://github.com/t4h4/FluentValidationApp/commit/c0e8b0db808f0d1a572a0408b3dbf00afb7d8b71)

* Model altında Gender.cs yapısında enum oluşturuldu. 
````c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentValidationApp.Models
{
    public enum Gender
    {
        Erkek = 1,
        Bayan = 2
    }
}
````
* Customer entity yapısına enum yapımızı eklendik. 
````c#
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public DateTime? Birthday { get; set; }
    public IList<Address> Addresses { get; set; }
    public Gender Gender { get; set; }
}
````           
* CustomerValidator.cs dosyasında gerekli enum kuralını ekledik.
````c#
RuleFor(x => x.Gender).IsInEnum().WithMessage("{PropertyName} alani Erkek=1, Bayan=2 olmalidir.");
````
