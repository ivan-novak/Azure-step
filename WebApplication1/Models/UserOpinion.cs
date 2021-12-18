using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
	public class UserOpinion
	{
		public String id { get; set; }
		public String name { get; set; }
		public String text { get; set; }
		public int? year { get; set; }
		public override string ToString()
		{
			return String.Format("{0}: {1} ({2}) [{3}]", name, text, year, id);
		}
	}
}
