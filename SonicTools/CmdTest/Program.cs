﻿using System;
using System.IO;
using DimpsSonicLib.IO;
using System.Threading;

namespace CmdTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments given.");
            }
            else
            {
                try
                {

                    ZNOReader.ReadZNO(args[0]);
                    //SettingTest.ReadLTS(args[1]);
                    //SettingTest.ReadMFS(args[2]);
                    //zlibTest.zlibFunc(args[0]);

                }
                catch (Exception ex) { Console.WriteLine("Exception message: {0}\n\nException: {1}", ex.Message, ex); }

                Console.WriteLine("\n\nPress enter to exit."); Console.ReadLine();
                //Console.WriteLine("\n\nExiting in 2 seconds."); Thread.Sleep(2000);
            }
        }    
    }
}
