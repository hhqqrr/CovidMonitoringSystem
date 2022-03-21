using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class Visitor:Person
    {
        public string PassportNo { get; set; }
        public string Nationality { get; set; }

        public Visitor(string n, string pn,string nation) : base(n)
        {
            PassportNo = pn;
            Nationality = nation;
        }

        public override double CalculateSHNCharges()
        {
            foreach(TravelEntry i in TravelEntryList)
            {
                double cost = 200;
                if ((i.LastCountryOfEmbarkation == "New Zealand") || (i.LastCountryOfEmbarkation == "Vietnam")||(i.LastCountryOfEmbarkation=="Macao SAR"))
                {
                    cost += 80;//transport cost
                }
                else//14 day SHN at SDF
                {
                    cost += i.ShnStay.CalculateTravelCost(i.EntryMode, i.EntryDate);//calculate the transport cost
                    cost += 2000;
                }
                return cost * 1.07;
            }
            return -1;//indicate error
        }//end of calculate SHN charges for visitors

        public override string ToString()
        {
            return base.ToString() + $"Passport No.: {PassportNo} \t Nationality: {Nationality}";
        }//end of tostring
    }//end of class
}
