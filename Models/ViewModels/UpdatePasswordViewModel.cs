using System.ComponentModel.DataAnnotations;

public class UpdatePasswordViewModel
{
    [Required] public string OldPassword { get; set; }
    [Required, MinLength(6)] public string NewPassword { get; set; }
    [Required, Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmNewPassword { get; set; }
}
