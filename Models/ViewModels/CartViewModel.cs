using WebBanSach.Models.Entities;

namespace WebBanSach.Models.ViewModels
{
    public class CartViewModel
    {
        public Book book { get; set; }
        public int amount { get; set; }
        public decimal TotalMoney => amount * book.Price;
    }
}
