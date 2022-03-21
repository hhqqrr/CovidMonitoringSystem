using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class TraceTogetherToken
    {
        //Attributes
        public string SerialNo {get; set;}
        public string CollectionLocation {get; set;}
        public DateTime ExpiryDate {get; set;}

        //Constructors
        public TraceTogetherToken(){ } //Non-parameteralised constructor

        public TraceTogetherToken(string sn, string cl, DateTime ed)
        {
            SerialNo = sn;
            CollectionLocation = cl;
            ExpiryDate = ed;
        }

        //Methods
        public bool IsEligibleForReplacement()
        {
            DateTime currentDate = DateTime.Now;
            
            int monthCurrentDate = currentDate.Month;
            int monthExpiryDate = ExpiryDate.Month;
            if (monthExpiryDate - monthCurrentDate <= 1)
            {
                return true; //return true if resident is eligible for replacement
            }
            return false;                
        }

        public void ReplaceToken(string newSerialNo, string newCollectionLocation)
        {
            SerialNo = newSerialNo;
            CollectionLocation = newCollectionLocation;
            DateTime replacementDate = DateTime.Now; //get date for replacement
            ExpiryDate = replacementDate.AddMonths(6); //Expiry Date
        }

        public override string ToString()
        {
            return $"Serial No.: {SerialNo} \t Collection Location: {CollectionLocation} \t Expiry Date: {ExpiryDate}";
        }

    }
}
