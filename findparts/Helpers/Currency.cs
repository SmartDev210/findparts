using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Currency
{
	public Currency()
	{
	}

	public static string GetSymbol(string currency)
	{
		switch (currency)
		{
			case "GBP":
				return "£";
			case "EUR":
				return "€";
			case "USD":
			default:
				return "$";
		}
	}
}