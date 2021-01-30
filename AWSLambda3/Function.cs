//==================================================================================
// Lambda DataGrabber application 
//
// Author: Hansen Zhang
// 
// Date: 1/27/2020
//
// Note: Input a url for Json file and extract date from the file.
//==================================================================================
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Amazon.Lambda.Core;
using System.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda3
{
    public class Test_Input
    {
        public string key1, key2, key3;
    }

    public class Test_Input2
    {
        public string TextLine { get; set; }
        public string BoundingBox { get; set; }
        public int PointY { get; set; }
        public int PointX { get; set; }
        public int Length { get; set; }
        public int Height { get; set; }
        public bool IsProcess { get; set; }
    }

    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        //-----------------------------------------------------------------------------------
        // Extract the lines that including certain string/suffix into an array of string
        //-----------------------------------------------------------------------------------
        /*public static string[] Readjson(string key, string full_content)
        {
            int counter = 0;
            string line;
            string[] temp_data_list = new string[300];
            using (System.IO.StringReader reader = new System.IO.StringReader(full_content))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(key))
                    {
                        string temp = line;
                        temp = temp.Remove(0, 17);
                        temp = temp.Replace("\",", "");
                        temp_data_list[counter++] = temp;
                    }
                }
            }

            return temp_data_list;
        }*/

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
            for (int i = 0; i < temp_list.Length; i++)
            {
                string temp = "";
                // The special detect for "Date: MM/dd/yyyy"
                if (temp_list[i] != null)
                {
                    if (temp_list[i].Contains("Date: "))
                    {
                        temp = temp_testline_list[i].Remove(0, 6);
                    }
                    else temp = temp_testline_list[i];

                    foreach (string dateStringFormat in formats)
                    {
                        //Detect Date format and convert to a united format
                        if (DateTime.TryParseExact(temp, dateStringFormat,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out dDate))
                        {
                            string_date[counter++] = dDate.ToString("MM'/'dd'/'yyyy");
                            break;
                        }
                    }
                }
            }
            return string_date;
        }
        
        
        public string FunctionHandler(Object input, ILambdaContext context)
        {

            //string temp_input = "test";

            // for each Json object do below, then push into the result.
            string plain_txt = input.ToString();

            string[] TextLine_list = new string[300];
            string[] date_list;

            string this_line = "";
            string next_line = "";
            var result = "";
            string date_result = "";
            
            System.IO.StringReader reader = new System.IO.StringReader(plain_txt);
            try
            {
                next_line = reader.ReadLine();
                this_line = next_line;
            }
            catch (Exception e)
            {
                Console.WriteLine("Didn't detect correct JSON Format with '['");
            }
            
            next_line = reader.ReadLine();
            this_line = next_line;

            int index = 0;

            while (true)
            {
                if (next_line.Equals("]"))
                {
                    try
                    {
                        next_line = reader.ReadLine();
                        if (next_line.Equals("["))
                        {
                            next_line = reader.ReadLine();
                            continue;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }

                }
                else
                {
                    string JSON_Object = "";
                    result = "";
                    for (int i = 0; i < 9; i++)
                    {
                        this_line = next_line;
                        if (this_line.Contains("},"))
                            this_line = "}";
                        JSON_Object += this_line;

                        next_line = reader.ReadLine();
                    }
                    result += JSON_Object;

                    Test_Input2 object1 = JsonConvert.DeserializeObject<Test_Input2>(result);
                    TextLine_list[index++] = object1.TextLine;
                }
            }
            
            date_list = ExtractDate(TextLine_list);


            //Test of content
            foreach (string i in date_list)
            {
                if (i != null)
                    date_result = date_result + i + "\n";
            }
            
            return (date_result);
        }

    }
}