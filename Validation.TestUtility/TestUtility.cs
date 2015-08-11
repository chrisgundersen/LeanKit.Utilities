using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Html;
using Validation.Html.Tests;
using Validation.Web;
using CommandLine;
using RestSharp;
using RestSharp.Validation;


namespace Validation.TestUtility
{
    class TestUtility
    {
        static void Main(string[] args)
        {
            TextReader tr;
            TextWriter tw;
            string defaultEndpoint = "http://localhost:53033";
            string ep;
            RestClient client = new RestClient();

            //handle options
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.inputFile != null)
                {
                    tr = new StreamReader(options.inputFile);
                    Console.WriteLine(String.Format("Input file is set as {0}", tr.ToString()));
                }
                else
                {
                    tr = new StreamReader(@"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test.txt");
                    Console.WriteLine(@"Input file is set as C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test.txt");
                }
                if (options.outputFile != null)
                {
                    tw = new StreamWriter(options.outputFile);
                    Console.WriteLine(String.Format("Output file is set as {0}", tw.ToString()));
                }
                else
                {
                    tw = new StreamWriter(@"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test2.txt");
                    Console.WriteLine(@"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test2.txt");
                }
                if (options.url != null)
                {
                    string url = options.url;
                    Console.WriteLine(String.Format(@"The url which will be scrapped is {0}", url));
                }
                if (options.endpoint != null)
                {
                    ep = options.endpoint;
                }
                else
                {
                    
                    ep = defaultEndpoint;
                    Console.WriteLine(String.Format(@"The api endpoint set is{0}", defaultEndpoint));
                }

            }
            else
            {
                string url;
                tr = new StreamReader(@"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test.txt");
                tw = new StreamWriter(@"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\test2.txt");
                ep = defaultEndpoint;

            }

            ////////////////////////////////////////

            if (options.url != null)
            {

                IRestResponse resp =scrapper(options.url);
                tw.WriteLine(resp.Content);

                if (resp.Content != null)
                {   
                    IRestResponse apiresp = ApiRequest(resp.Content, ep);
                    if (apiresp != null)
                    {
                        string s = apiresp.Content;
                        tw.WriteLine(s);      
                    }
                }
                else { Console.WriteLine("No conent returned by requested URL"); }
            }
            else
            {
                string xssrequest;

                while ((xssrequest = tr.ReadLine()) != null)
                {
                    IRestResponse resp = ApiRequest(xssrequest, ep);

                    if (resp != null)
                    {
                        tw.WriteLine(resp.Content);
                    }

                }
            }

            tr.Close();
            tw.Close();

            Console.ReadLine();

        }

        public static IRestResponse scrapper(string url)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(url);
            var request = new RestRequest("", Method.GET);
            IRestResponse response = client.Execute(request);

            return response;
        }

        public static IRestResponse ApiRequest(string req, string c)
        {
            try
            {
                Console.WriteLine("see here");

                var client = new RestClient(c);
                var request = new RestRequest("/validate/html", Method.GET);
                //request.Resource = "validate/html";
                //request.AddBody(req);

                IRestResponse response = client.Execute(request);
                return response;
            }
            catch(Exception ex)
            {
                var i = 0;
                return null;
            }

        }



    }
    public class Options
    {
        [Option('i', "inputFile", Required = false,
            HelpText = "Input file to test against")]
        public string inputFile { get; set; }

        [Option('o', "outputFile", Required = false,
            HelpText = "Output file to write results to")]
        public string outputFile { get; set; }

        [Option('u', "url", Required = false,
            HelpText = "Url to scrape html from for testing")]
        public string url { get; set; }

        [Option('e', "endpoint", Required = false,
            HelpText = "API endpoint to hit. Defaults to localhost:53033")]
        public string endpoint { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("TestUtility 1.0");
            usage.AppendLine("-i|--inputFile -- Input file to test against");
            usage.AppendLine("-o|--outputFile -- Output file to write results to");
            usage.AppendLine("-u|--url-- Url to scrape html from for testing");
            usage.AppendLine("-e| --endpoint -- API endpoint to hit. Defaults to 127.0.0.1:53033");
            return usage.ToString();
        }
    }
}
