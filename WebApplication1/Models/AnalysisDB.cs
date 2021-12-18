using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
	public class AnalysisDB
	{
		
		public String id { get; set; }
		public String nameDb { get; set; }
		public String nameUser { get; set; }
		public String comments { get; set; }
		public DateTime date { get; set; }
		public String type { get; set; }
		//public String type { get; set; }

		public override String ToString()
		{
			return String.Format("id:{0}\nname_db:{1}\nname_user:{2}\ncomments:{3}\ndate:{4}\ntype:{5}", id, nameDb, nameUser, comments, date, type);
		}
	}
}
