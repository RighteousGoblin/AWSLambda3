//==================================================================================
// Raw DataGrabber application 
//
// Author: Hansen Zhang
// 
// Date: 1/27/2020
//
// Note: Hardcode to input a url for Json file and extract date from the file.
//==================================================================================
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using Amazon.Lambda;
using System.Net;

namespace DataGrabber
{
    class Program
    {
        //-----------------------------------------------------------------------------------
        // Extract the lines that including certain string/suffix into an array of string
        //-----------------------------------------------------------------------------------
        public static string[] Readjson(string key, string full_content)
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
        }
        //-----------------------------------------------------------------------------------
        // Extract valid date into an array of string
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

            for (int i = 0; i < 300; i++)
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

        static void Main(string[] args)
        {

            string url_content;
            using (WebClient client = new WebClient())
            {
                url_content = client.DownloadString("https://raw.githubusercontent.com/RighteousGoblin/Hello-World/master/test1.json");
            }

            string[] TextLine_list = new string[300];
            TextLine_list = Program.Readjson("TextLine", url_content);
            string[] date_list = Program.ExtractDate(TextLine_list);
            string result = "";
            foreach (string i in date_list)
            {
                if (i != null)
                    result = result + i + "\n";
            }
            Console.WriteLine(result);
        }
    }
}
