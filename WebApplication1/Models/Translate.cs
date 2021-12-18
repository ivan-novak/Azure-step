using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Translation
    {
        public String text { get; set; }

        public String to { get; set; }
    }
    public class Translations
    {
        public Translation[] translations { get; set; }
    }
}
