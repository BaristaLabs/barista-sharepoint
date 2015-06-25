using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.DDay.iCal
{
    public class UIDFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
