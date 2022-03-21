using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class TravelEntry
    {
        public string LastCountryOfEmbarkation { get; set; }
        public string EntryMode { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ShnEndDate { get; set; }
        public SHNFacility ShnStay { get; set; }
        public bool IsPaid { get; set; }

        public TravelEntry() { }

        public TravelEntry(string le,string em,DateTime ed)
        {
            LastCountryOfEmbarkation = le;
            EntryMode = em;
            EntryDate = ed;
        }

        public void AssignSHNFacility(SHNFacility sf)
        {
            ShnStay = sf;
        }

        public void CalculateSHNDuration()
        {
            double dura;
            if(LastCountryOfEmbarkation == "New Zealand" || LastCountryOfEmbarkation == "Vietnam") // 0 day SHN
            {
                dura = 0;
            }
            else if(LastCountryOfEmbarkation == "Macao SAR")//7 day SHN with own accommodation
            {
                dura = 7;
            }
            else //14 day SHN at SHN dedicated facility (SDF)
            {
                dura = 14;
            }
            ShnEndDate = EntryDate.AddDays(dura);
        }//end of CalculateSHNDuration

        public override string ToString()
        {
            string isp;
            if (IsPaid == false)
            {
                isp = "Unpaid";
            }
            else//IsPaid == true
            {
                isp = "Paid";
            }
            return $"Last Left Country: {LastCountryOfEmbarkation} \t Entry Mode: {EntryMode} \t Entry Date: {EntryDate} \nSHN End Date: {ShnEndDate}" +
                $" \t SHN Stay: {ShnStay} \t Travel Entry Charges: {isp}";
        }

    }//end of class
}
