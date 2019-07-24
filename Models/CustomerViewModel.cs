using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceClientPortal.Models
{
    public class CustomerViewModel
    {
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Application Date")]
        public DateTime AppDate { get; set; }

        public string InsuranceType { get; set; } 

        public double Amount { get; set; }

        public double Premium { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Insurance End Date")]
        public DateTime EndDate { get; set; }

        public IFormFile Image { get; set; }
    }
}
