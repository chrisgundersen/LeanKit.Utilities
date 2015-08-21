using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using CommandLine;
using System.Data.SqlClient;


namespace Validation.TestUtility
{
    class TestUtility
    {
        static void Main(string[] args)
        {
            TextReader tr;
            TextWriter tw;
            string defaultEndpoint = "http://localhost:53033/validate/html";
            string defaultReadfile = @"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\Validation.TestUtility\test.txt";
            string defaultWritefile = @"C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\Validation.TestUtility\test2.txt";
            string ep;
            string connectionString = null;
            string query = null;


            //handle options
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.inputFile != null)
                {
                    try
                    {
                        tr = new StreamReader(options.inputFile);
                        Console.WriteLine(String.Format("Input file is set as {0}", tr.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"Cannot locate this file. Setting input at default value.");
                        tr = new StreamReader(defaultReadfile);

                    }
                }
                else
                {

                    tr = new StreamReader(defaultReadfile);
                    Console.WriteLine(@"Input file is set as C:\Users\Administrator\Documents\GitHub\LeanKit.Utilities\Validation.TestUtility\test.txt");

                }
                if (options.outputFile != null)
                {
                    tw = new StreamWriter(options.outputFile);
                    Console.WriteLine(String.Format("Output file is set as {0}", tw.ToString()));
                }
                else
                {
                    tw = new StreamWriter(defaultWritefile);
                    Console.WriteLine(defaultWritefile);
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
                //Check and set database variables
                if (options.dbserver != null && options.dbname != null && options.dbuser != null && options.dbpassword != null && options.dbquery != null)
                {
                    connectionString = String.Format(@"Data Source = {0}; Initial Catalog ={1}; Persist Security Info = true; User ID={2};Password={3}", options.dbserver, options.dbname, options.dbuser, options.dbpassword);
                    query = options.dbquery;
                }

            }
            else
            {
                string url;
                tr = new StreamReader(defaultReadfile);
                tw = new StreamWriter(defaultWritefile);
                ep = defaultEndpoint;
                connectionString = null;

            }

            ////////////////////////////////////////

            if (options.url != null)        //if scraping a url
            {

                Task<String> resp = scraper(options.url);

                if (resp.Result != null)
                {
                    Task<String> apiresp = ApiRequest(resp.Result, ep);
                    if (apiresp.Result != null)
                    {
                        string s = apiresp.Result;
                        tw.WriteLine("\tResult:\t{0}\n", s);
                    }
                    else
                    {
                        tw.WriteLine("\tResult: Format accepted\n");
                    }
                }
                else
                {
                    Console.WriteLine("No conent returned by requested URL\n");
                }
            }
            else if (options.dbserver != null && options.dbname != null && options.dbuser != null && options.dbpassword != null && options.dbquery != null) //else if testing against database
            {
                sqlTester(connectionString, query, tw, ep);

            }
            else    //else Basic check against file
            {
                string xssrequest;
                while ((xssrequest = tr.ReadLine()) != null)
                {
                    Task<String> resp = ApiRequest(xssrequest, ep);

                    if (resp != null)
                    {
                        string s = resp.Result;
                        tw.WriteLine("Result:\t{0}\n", s);
                    }

                }
            }



            //close files
            if (tr != null)
            {
                tr.Close();
            }
            tw.Close();

            Console.ReadLine();

        }


        /* Web Scraper */
        public static async Task<String> scraper(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                var response = await client.GetAsync(url);
                var contents = response.Content.ReadAsStringAsync().Result;
                return contents;
            }
            catch (Exception ex) { return null; }
        }

        /* Api-Request method */
        public static async Task<String> ApiRequest(string req, string c)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(c);
                var response = await client.PostAsync(c, new StringContent(req));
                var contents = response.Content.ReadAsStringAsync().Result;
                return contents;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        /* Sql-test handler */
        public static void sqlTester(string connectionString, string query, TextWriter tw, string ep)
        {
            if (query == null)
            {
                Console.WriteLine("Error. No query was provided.");
                tw.WriteLine("Error. No query was provided.");
            }
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = query;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Connection = connection;

            connection.Open();

            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                Console.WriteLine("\t{0}\r\n", reader.GetName(0));
                tw.WriteLine("\t{0}\n", reader.GetName(0));
                int total = 0;
                int sanitized = 0;

                while (reader.Read())
                {
                    string req = reader.GetString(0);
                    Console.WriteLine("Request: {0}\n", req);
                    tw.WriteLine("Request: {0}\n", req);
                    Task<String> apiresp = ApiRequest(req, ep);
                    if (apiresp.Result != null)
                    {
                        Console.WriteLine("\tResponse: {0}", apiresp.Result);
                        tw.WriteLine("\tResponse: {0}", apiresp.Result);
                        sanitized++;
                    }


                    total++;
                }

                Console.WriteLine("Sanitized requests over total requests:\t {0}/{1}", sanitized, total);
                tw.WriteLine("Sanitized requests over total requests:\t {0}/{1}", sanitized, total);

            }
            else
            {
                Console.WriteLine("Reader has no rows.");
                tw.WriteLine("Reader has no rows");
            }

            connection.Close();
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

        [Option('s', "dbservername", Required = false,
            HelpText = "Name of the database server (optional)")]
        public string dbserver { get; set; }

        [Option('n', "dbname", Required = false,
            HelpText = "Name of the database itself")]
        public string dbname { get; set; }

        [Option('i', "dbuser", Required = false,
            HelpText = "Database User")]
        public string dbuser { get; set; }

        [Option('p', "dbpass", Required = false,
            HelpText = "Database Password, to be used with username")]
        public string dbpassword { get; set; }

        [Option('q', "dbquery", Required = false,
            HelpText = "Query to run against the database")]
        public string dbquery { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("TestUtility 1.0");
            usage.AppendLine("-i|--inputFile -- Input file to test against");
            usage.AppendLine("-o|--outputFile -- Output file to write results to");
            usage.AppendLine("-u|--url-- Url to scrape html from for testing");
            usage.AppendLine("-e| --endpoint -- API endpoint to hit. Defaults to 127.0.0.1:53033");
            usage.AppendLine("-s| --dbservername -- Server which DB is hosted (if using db to test data)");
            usage.AppendLine("-n| --dbname -- Name of Database (if using db to test data)");
            usage.AppendLine("-i| --dbuser -- DB username (if using db to test data)");
            usage.AppendLine("-p| --dbpass -- Password for DB user (if using db to test data)");
            usage.AppendLine("-q| --dbquery -- Query to run against the databse");
            return usage.ToString();
        }
    }
}
