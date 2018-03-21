using System;

namespace ReachableTree.Server
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            using (var server = new AsyncHttpServer())
            {
                try
                {
                    server.Start("http://+:8080/");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.ReadKey(true);
            }
        }
    }
}
