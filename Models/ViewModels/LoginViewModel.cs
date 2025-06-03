using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "Cần nhập Email.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Cần nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Lưu tài khoản?")]
    public bool RememberMe { get; set; }
}
