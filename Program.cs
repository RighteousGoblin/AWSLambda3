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
        // Extracts the lines that including certain string/suffix into an array of string.
        //-----------------------------------------------------------------------------------
        public static string[] Readjson(string full_content)
        {
            
            string[] TextLine_list = new string[300];
            int text_line_index = 0;

            string plain_txt = full_content.ToString();
            plain_txt = plain_txt.Remove(0, 1);
            string right_quote_seperator = "},";
            int location;
            while (true)
            {
                location = plain_txt.IndexOf(right_quote_seperator);
                if (location != -1)
                {
                    string JSON_string = plain_txt.Substring(0, location + 1);
                    plain_txt = plain_txt.Substring(location + 2); // cut string before "}," included

                    Test_Input2 object1 = JsonConvert.DeserializeObject<Test_Input2>(JSON_string);
                    TextLine_list[text_line_index++] = object1.TextLine;
                }
                else
                {
                    plain_txt = plain_txt.Replace("]", "");
                    Test_Input2 object1 = JsonConvert.DeserializeObject<Test_Input2>(plain_txt);
                    TextLine_list[text_line_index++] = object1.TextLine;
                    break;
                }
            }
            return TextLine_list;
        }
        /*
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


        //-----------------------------------------------------------------------------------
        // Extracts a Date array from a given string array by testifying the format.
        //-----------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------
        // Converts array of date to a single date string.
        //-----------------------------------------------------------------------------------
        public string GenerateResult(string[] temp_date_list)
        {
            string Date_result = "";
            foreach (string i in temp_date_list)
            {
                if (i != null)
                    Date_result = Date_result + i + "\n";
            }
            return (Date_result);
        }

        //-----------------------------------------------------------------------------------
        // Main handler function.
        //-----------------------------------------------------------------------------------
        public string FunctionHandler(Object input, ILambdaContext context)
        {
            //extract Testline from all the inputs.
            string[] TextLine_list = Readjson(input.ToString());

            //extract date with certain format from Testline array generated above.
            string[] Date_list = ExtractDate(TextLine_list);

            string result = GenerateResult(Date_list);
            
            return(result);
        }

    }
}




//The part below is for Visual Studio running only. since on the website, the \n char are all ignored, reader reads all the lines.
/*StringReader reader = new StringReader(plain_txt);
try
{
    next_line = reader.ReadLine();
    this_line = next_line;
}
catch (Exception e)
{
    if (next_line == null)
    {
        Console.WriteLine("NextLine Error.\n");
    }
}

next_line = reader.ReadLine();
this_line = next_line;
int text_line_index = 0;
next_line += "abc";

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
        TextLine_list[text_line_index++] = object1.TextLine;
    }
}
*/

