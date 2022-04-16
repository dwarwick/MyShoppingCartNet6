﻿using System;

namespace PayoutsJobConsoleApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        
        public decimal Price { get; set; }

        public int ProductId { get; set; }        
        

        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }
        public string Name { get; set; }

        public int PayoutId { get; set; }


    }
}
