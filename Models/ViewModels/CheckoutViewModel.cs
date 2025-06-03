using System.ComponentModel.DataAnnotations;
using WebBanSach.Models.Entities;

namespace WebBanSach.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // Thông tin người nhận
        public string RecipientName { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string AddressDetail { get; set; }
        public string Notes { get; set; }

        // Chọn phương thức thanh toán
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")] 
        public int SelectedPaymentMethodId { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }

        // Giỏ hàng
        public List<CartViewModel> CartItems { get; set; }
    }
}
