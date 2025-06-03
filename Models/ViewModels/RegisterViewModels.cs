using System.ComponentModel.DataAnnotations;

namespace WebBanSach.Models.ViewModels
{
    public class RegisterViewModels
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Mật khẩu không khớp.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}
