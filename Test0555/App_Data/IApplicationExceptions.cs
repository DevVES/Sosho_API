using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarageXAPINEW
{
    interface IApplicationExceptions
    {
        void AddApplicationError(string message, string stack);
        void AddError(string message, string stack, string userId);
    }
}
