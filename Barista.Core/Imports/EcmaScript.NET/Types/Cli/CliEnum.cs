using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.EcmaScript.NET.Types.Cli
{
    public class CliEnum : CliObject
    {

        public override string ClassName
        {
            get
            {
                return "NativeCliEnum";
            }
        }

        public CliEnum (Enum enm)
        {
            base.Init (enm, enm.GetType ());
        }

    }
}
