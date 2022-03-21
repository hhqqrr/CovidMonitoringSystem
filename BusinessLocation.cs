using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class BusinessLocation
    {
        public string BusinessName { get; set; }
        public string BranchCode { get; set; }
        public int MaximumCapacity { get; set; }
        public int VisitorsNow { get; set; }

        public BusinessLocation() { }

        public BusinessLocation(string bn,string bc,int cap)
        {
            BusinessName = bn;
            BranchCode = bc;
            MaximumCapacity = cap;
        }

        public bool IsFull()
        {
            if (VisitorsNow == MaximumCapacity)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"Business Name: {BusinessName} \t Branch Code: {BranchCode} \t Maximum Capacity: {MaximumCapacity} \t " +
                $"No. of Visitors Now: {VisitorsNow}";
        }
    }
}
