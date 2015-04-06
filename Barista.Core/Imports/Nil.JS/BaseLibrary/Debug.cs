﻿using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.BaseLibrary
{
    public static class Debug
    {
        public static void writeln(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
            {
#if !PORTABLE
                if (i < args.length)
                    System.Diagnostics.Debug.Write(args[0]);
                else
#endif
                    System.Diagnostics.Debug.WriteLine(args[args.length - 1]);
            }
        }

        public static void write(Arguments args)
        {
#if PORTABLE
            for (var i = 0; i < args.length; i++)
                System.Diagnostics.Debug.WriteLine(args[0]);
#else
            for (var i = 0; i < args.length; i++)
                System.Diagnostics.Debug.Write(args[0]);
#endif
        }

        public static void assert(Arguments args)
        {
            System.Diagnostics.Debug.Assert((bool)args[0], args[0].ToString());
        }
    }
}
