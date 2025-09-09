using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.PatientManagement.Models
{
    public class PatientName
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public PatientName(string firstName,string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
