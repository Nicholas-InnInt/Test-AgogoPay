﻿using System;

namespace Neptune.NsPay.MultiTenancy.HostDashboard.Dto
{
    public class IncomeStastistic 
    {
        public string Label { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public IncomeStastistic()
        {


        }

        public IncomeStastistic( DateTime date)
        { 
            Date = date;
        }

        public IncomeStastistic(DateTime date, decimal amount)
        {
            Date = date;
            Amount = amount;
        }
    }
}