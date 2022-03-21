using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class SafeEntry
    {
        //attributes
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public BusinessLocation Location { get; set; }

        //Constructors
        public SafeEntry() { } //non-parameteralised constructor

        public SafeEntry(DateTime ci, BusinessLocation l) //parameteralised constructor
        {
            CheckIn = ci;
            Location = l;
            
        }

        public void PerformCheckOut() //Method to perform checkout
        {

            CheckOut = DateTime.Now;
        }

        public override string ToString() //ToString Method
        {
            return $"CheckIn Time: {CheckIn} \t CheckOut Time: {CheckOut} \t Business Location: {Location}";
        }


    }
}
