using System.ComponentModel.DataAnnotations;

namespace CoffeeShops.ViewModels;

public class RegisterUserViewModel
{

    [Required(ErrorMessage = "Поле логина не должно пыть пустым")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Длина пароля должна быть от 1 до 128 символов.")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Поле пароля не долдно быть пустым")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Длина пароля должна быть от 1 до 128 символов.")]
    public string Password { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Range(typeof(DateTime), "1900-01-01", "2025-01-01", ErrorMessage = "Некорректная дата рождения")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Поле почты не может быть пустым")]
    [StringLength(256, MinimumLength = 4, ErrorMessage = "Длина пароля должна быть от 1 до 128 символов.")]
    public string Email { get; set; }


}
