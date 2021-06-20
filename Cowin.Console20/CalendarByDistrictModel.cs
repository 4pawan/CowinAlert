using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Cowin.Console20
{
    public class CalendarByDistrictModel
    {
        static string apiUrl = string.Format(ConfigurationManager.AppSettings["apiUrl"], DateTime.Today.Date.ToString("d"));
        public static string Token = ConfigurationManager.AppSettings["token"];
        public static int TimeToSleepInMinute = Convert.ToInt32(ConfigurationManager.AppSettings["timeToSleepInMinute"]);
        public static string PinCodeToCheck = ConfigurationManager.AppSettings["pinCodeToCheck"];

        public List<Center> centers { get; set; }

        public static IEnumerable<Center> GetAvailableSlots()
        {
            string result;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            CalendarByDistrictModel slot = JsonConvert.DeserializeObject<CalendarByDistrictModel>(result);

            var list = slot.centers.FindAll(c => CalendarByDistrictModel.IsSlotAvailable(c));

            return list;
        }


        public static bool IsSlotAvailable(Center c)
        {
            List<Session> found = new List<Session>();

            if (string.IsNullOrWhiteSpace(PinCodeToCheck))
            {
                found = c.sessions.Where(s => s.available_capacity_dose1 > 0
                                               && s.min_age_limit < 30
                                               && (s.vaccine == "COVISHIELD" || s.vaccine == "COVAXIN")).ToList();
            }
            else
            {
                found = c.sessions.Where(s => PinCodeToCheck.Contains(Convert.ToString(c.pincode))
                                              && s.available_capacity_dose1 > 0
                                              && s.min_age_limit < 30
                                              && (s.vaccine == "COVISHIELD" || s.vaccine == "COVAXIN")).ToList();
            }

            return found.Any();
        }

        public class Session
        {
            public string session_id { get; set; }
            public string date { get; set; }
            public int available_capacity { get; set; }
            public int min_age_limit { get; set; }
            public string vaccine { get; set; }
            public List<string> slots { get; set; }
            public int available_capacity_dose1 { get; set; }
            public int available_capacity_dose2 { get; set; }
        }

        public class VaccineFee
        {
            public string vaccine { get; set; }
            public string fee { get; set; }
        }

        public class Center
        {
            public int center_id { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public string state_name { get; set; }
            public string district_name { get; set; }
            public string block_name { get; set; }
            public int pincode { get; set; }
            public int lat { get; set; }
            public int @long { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string fee_type { get; set; }
            public List<Session> sessions { get; set; }
            public List<VaccineFee> vaccine_fees { get; set; }
        }
    }
}