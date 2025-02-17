using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        private DateTime _orderDate = DateTime.UtcNow;
        
        [Required]
        public DateTime OrderDate
        {
            get => _orderDate;
            set => _orderDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; }

        [Required]
        [EmailAddress]
        public string GuestEmail { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // ✅ FIX: Ensure OrderDetails is never null
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}