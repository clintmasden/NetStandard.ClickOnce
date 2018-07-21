using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageUINetStandard.Models
{
    public class Dependency
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Manifest { get; set; }

        public int Index { get; set; }

        public int Length { get; set; }
    }
}