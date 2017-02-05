using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeUtils.Models
{
    interface IFunctionProvider
    {
        string GetReplyMessage(string message);
    }
}
