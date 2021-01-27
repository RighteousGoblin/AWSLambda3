using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda3
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        public static string[] Readjson(string key, string full_content)
        {
            int counter = 0;
            string line;
            string[] temp_data_list = new string[300];
            //System.IO.StreamReader file = new System.IO.StreamReader(@temp_full_path);
            using (System.IO.StringReader reader = new System.IO.StringReader(full_content))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(key))
                    {
                        string temp = line;
                        temp = temp.Remove(0, 17);
                        temp = temp.Replace("\",", "");
                        //System.Console.WriteLine(temp);
                        temp_data_list[counter++] = temp;
                    }
                }
            }
            System.Console.WriteLine("There were {0} lines.", counter);
            return temp_data_list;
        }

        public static string[] ExtractDate(string[] temp_testline_list)
        {
            string[] formats = {
                            // Regular without clock time
                            "M/d/yyyy", "MM/dd/yyyy",
                            "d/M/yyyy", "dd/MM/yyyy",
                            "yyyy/M/d", "yyyy/MM/dd",
                            "M-d-yyyy", "MM-dd-yyyy",
                            "d-M-yyyy", "dd-MM-yyyy",
                            "yyyy-M-d", "yyyy-MM-dd",
                            "M.d.yyyy", "MM.dd.yyyy",
                            "d.M.yyyy", "dd.MM.yyyy",
                            "yyyy.M.d", "yyyy.MM.dd",
                            "M,d,yyyy", "MM,dd,yyyy",
                            "d,M,yyyy", "dd,MM,yyyy",
                            "yyyy,M,d", "yyyy,MM,dd",
                            "M d yyyy", "MM dd yyyy",
                            "d M yyyy", "dd MM yyyy",
                            "yyyy M d", "yyyy MM dd",
                            "MM/dd/y",
                            // Regular with clock time
                            "dddd, dd MMMM yyyy HH:mm:ss",
                            "MM/dd/yyyy HH:mm", "MM/dd/yyyy hh:mm tt",
                            "MM/dd/yyyy H:mm", "MM/dd/yyyy h:mm tt",
                            "MM/dd/yyyy HH:mm:ss",
                            //Other formats
                            "MMMM dd", "yyyy-MM-dd T HH:mm:ss.fffffffK",
                            "ddd, dd MMM yyy HH:mm:ss GMT", "yyyy-MM-dd THH:mm:ss",
                            "MM/dd/yyyy HH:mm:ss tt","dd-MMM-yyyy HH:mm:ss"
                           };

            string[] temp_list = temp_testline_list;
            string[] string_date = new string[10];
            int counter = 0;
            DateTime dDate;
            /*for(int i = 0; i < 300; i++)
            {
                DateTime dDate;

                if (DateTime.TryParse(temp_list[i], out dDate))
                {
                    
                    String.Format("{0:MM/d/yyyy}", dDate);
                    string_date = dDate.ToString();
                    Console.WriteLine("Noise data: " + temp_list[i]);
                    Console.WriteLine("Result: " + dDate);
                    */
            for (int i = 0; i < 300; i++)
            {
                //Console.WriteLine("Invalid" + temp_list[i]);
                if (temp_list[i] != null)
                {
                    if (temp_list[i].Contains("Date: "))
                    {
                        string temp = temp_testline_list[i].Remove(0, 6);
                        Console.WriteLine(temp);
                    }
                }

                foreach (string dateStringFormat in formats)
                {

                    if (DateTime.TryParseExact(temp_list[i], dateStringFormat,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out dDate))
                    //Console.WriteLine("Converted '{0}' to {1}.", dateStringFormat, dateValue.ToString("yyyy-MM-dd"));                
                    {
                        Console.WriteLine("Result: " + dDate);
                        string_date[counter++] = dDate.ToString("MM'/'dd'/'yyyy");
                        break;
                    }
                }
            }

            return string_date;
        }


        public string FunctionHandler(string input, ILambdaContext context)
        {

            string[] TextLine_list = new string[500];
            TextLine_list = Readjson("TextLine", input);
            string[] date_list = ExtractDate(TextLine_list);
            string result = "1";
            // Test of content
            foreach (string i in date_list)
            {
                if (i != null)
                {
                    result = result + "\n" + i;
                }
            }
            result += input;
            return result;
        }
    }
}
