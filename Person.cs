using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    abstract class Person
    {
        public string Name { get; set; }
        public List<SafeEntry> SafeEntryList { get; set; }
        public List<TravelEntry> TravelEntryList { get; set; }
        public bool Did { get; set; }

        public Person() 
        {
            SafeEntryList = new List<SafeEntry>();
            TravelEntryList = new List<TravelEntry>();
        }

        public Person(string n, bool d)
        {
            Name = n;
            SafeEntryList = new List<SafeEntry>();
            TravelEntryList = new List<TravelEntry>();
            Did = d;
        }

        public void AddTravelEntry(TravelEntry te)
        {
            TravelEntryList.Add(te);
        }

        public void AddSafeEntry(SafeEntry se)
        {
            SafeEntryList.Add(se);
        }

        public abstract double CalculateSHNCharges();

        public override string ToString()
        {
            string str = $"Name: {Name}\n\n";
            for(int i = 0; i < SafeEntryList.Count; i++)
            {
                str += $"---------------------------------Safe Entry details for {Name} - Record number: {i+1} out of {SafeEntryList.Count}---------------------------------\n";
                str += SafeEntryList[i].ToString() + "\n";
            }
            for(int i = 0; i < TravelEntryList.Count; i++)
            {
                str += $"---------------------------------Travel Entry details for {Name} - Record number: {i + 1} out of {TravelEntryList.Count}---------------------------------\n";
                str += TravelEntryList[i].ToString() + "\n";
            }
            return str;
        }//end of tostring
    }//end of class
}
