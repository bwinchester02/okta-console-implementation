using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winchester.OktaPasswordRotation
{
    public class OktaConfiguration
    {
        public string BaseURL { get; set; } = string.Empty;
        public string APIToken { get; set; } = string.Empty;
    }
}
