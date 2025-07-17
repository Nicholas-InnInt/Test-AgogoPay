using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantBills
{
	public enum MerchantBillTypeEnum
	{
		//订单流水
		OrderIn = 1,
		//提现流水
		Withdraw = 2,
		//添加
		AddBill = 3,
		//扣除
		DeleteBill = 4
	}
}
