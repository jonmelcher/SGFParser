// ***************************************
// Entrypoint - used for testing SGFParser
//
// Written by Jonathan Melcher
// Last updated Feb 04, 2015
// ***************************************

using System;
using System.Collections.Generic;


namespace SGFParser
{
    class Entrypoint
    {
        static void Main(string[] args)
        {
            ParserSGF reader = new ParserSGF("test4.sgf");
            List<string> gameProps = new List<string>{"BR", "WR", "PB", "PW", "GN", "RE"};
            List<string> moveProps = new List<string>{";B", ";W"};
            List<string> placementProps = new List<string> { "AB", "AW", "AE", "CR", "MA", "SQ", "TR" };
            TreeSGF parsedTree = reader.ParseFile(reader.Raw, gameProps, moveProps, placementProps);

            parsedTree.ResetTheIDS("A", true);
            Console.ReadKey();

        }
    }
}
