using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TcpConnectors.TestClient
{
    class ConsoleUI
    {
        private static object _lockObject = new Object();



        public static void ShowMenu(string title, params Action[] actions)
        {
            int[] retcodes = new int[actions.Length + 1];
            string[] texts = new string[actions.Length + 1];

            retcodes[0] = 0;
            texts[0] = "Exit Menu";

            for (int i = 1; i < actions.Length + 1; i++)
            {
                retcodes[i] = i;
                texts[i] = CamelCaseToWords(actions[i - 1].Method.Name);
            }

            var index = ShowMenu(title, retcodes, texts);
            if (index == 0) return;
            actions[index - 1]();

        }

        public static string ShowMenu(string title, string[] texts)
        {
            int[] retcodes = new int[texts.Length + 1];
            string[] textsEx = new string[texts.Length + 1];

            retcodes[0] = 0;
            textsEx[0] = "Exit Menu";

            for (int i = 1; i < texts.Length + 1; i++)
            {
                retcodes[i] = i;
                textsEx[i] = texts[i - 1];
            }

            var index = ShowMenu(title, retcodes, textsEx);
            if (index == 0) return null;
            return texts[index - 1];
        }

        public static int ShowMenu(string title, int[] retcodes, string[] texts)
        {
            lock (_lockObject)
            {
                if (title != null && title.Length > 0)
                    Console.WriteLine(title);

                PrintMenu(retcodes, texts);
            }
            for (; ; )
            {
                string input = Console.ReadLine();
                if (input.Trim() == "") continue;
                if (input.Trim() == "?")
                {
                    PrintMenu(retcodes, texts);
                    continue;
                }
                int retVal;
                if (Int32.TryParse(input, out retVal))
                {
                    foreach (int val in retcodes)
                    {
                        if (val == retVal) return retVal;
                    }
                }
                WriteLineSync("Input Error");
            }
        }

        private static void PrintMenu(int[] retcodes, string[] texts)
        {
            lock (_lockObject)
            {
                Console.WriteLine("?) Show menu again");
                for (int i = 0; i < retcodes.Length; i++)
                {
                    Console.WriteLine("{0}) {1}", retcodes[i], texts[i]);
                }
            }
        }

        public static void WriteLineSync(string value)
        {
            lock (_lockObject)
            {
                Console.WriteLine(value);
            }
        }

        public static string CamelCaseToWords(string str)
        {
            return Regex.Replace(str, "(\\B[A-Z])", " $1");
        }

    }
}
