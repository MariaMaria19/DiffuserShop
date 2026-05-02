using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffuserShop.Shared.Dtos
{
    public class DiffuserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Scent { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int InStock { get; set; }

    }
}
