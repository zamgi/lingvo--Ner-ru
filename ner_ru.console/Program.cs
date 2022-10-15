using System;

using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class Program
    {
        private static void Main( string[] args )
        {
            try
            {
                var text = @"Сергей Собянин напомнил, что в 2011 году в Москве были 143 млрд. руб. приняты масштабные программы развития города, в том числе программа ""Безопасный город"" на пять лет, на которую будет выделено финансирование в размере 143 млрд. рублей.";
                //---var text = System.IO.File.ReadAllText( @"C:\1.txt" );

                ProcessText( text );
            }
            catch ( Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( ex );
                Console.ResetColor();
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine( "  [.......finita fusking comedy.......]" );
            Console.ReadLine();
        }

        private static void ProcessText( string text )
        {
            using var env = NerEnvironment.Create( LanguageTypeEnum.Ru );

            using ( var nerProcessor = env.CreateNerProcessor() )
            {
                Console.WriteLine( "\r\n-------------------------------------------------\r\n text: '" + text + '\'' );

                var result = nerProcessor.Run( text, splitBySmiles: true );

                Console.WriteLine( "-------------------------------------------------\r\n ner-entity-count: " + result.Count + Environment.NewLine );
                foreach ( var word in result )
                {
                    Console.WriteLine( word );
                }
                Console.WriteLine();
                
                Console.WriteLine( "-------------------------------------------------\r\n" );
            }
        }
    }
}
