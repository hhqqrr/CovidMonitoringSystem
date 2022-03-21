using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class Resident:Person
    {
        public string Address { get; set; }
        public DateTime LastLeftCountry { get; set; }
        public TraceTogetherToken Token { get; set; }

        public Resident(string n,string addr,DateTime lastLeft) : base(n)
        {
            Address = addr;
            LastLeftCountry = lastLeft;
        }

        public override double CalculateSHNCharges()
        {
            foreach(TravelEntry te in TravelEntryList)
            {
                double cost = 200;
                //if last country of embark is New Zealand or Vietnam, the cost will only be 200 with no transport cost, so there is no need to code it
                if ((te.LastCountryOfEmbarkation == "New Zealand") || (te.LastCountryOfEmbarkation == "Vietnam"))// no transport cost
                {
                    cost = 200;
                }
                else if(te.LastCountryOfEmbarkation == "Macao SAR")
                {
                    cost += 20;//transport cost
                }
                else//14 day SHN at SDF
                {
                    cost += 20;//transport cost
                    cost += 1000;//SDF charge
                }
                return cost * 1.07;
            }
            return -1;//indicate there is error in the method
        }//end of CalculateSHNCharges

        public override string ToString()
        {
            return base.ToString() + $"Address: {Address} \t Last left country: {LastLeftCountry} \t Trace Together Token: {Token}";
        }//end of tostring
    }//end of class
}
