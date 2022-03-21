using System;
using System.Collections.Generic;
using System.Text;

namespace COVIDMonitoringSystem
{
    class SHNFacility
    {
        public string FacilityName { get; set; }
        public int FacilityCapacity { get; set; }
        public int FacilityVacancy { get; set; }
        public double DistFromAirCheckpoint { get; set; }
        public double DistFromSeaCheckpoint { get; set; }
        public double DistFromLandCheckpoint { get; set; }

        public SHNFacility() { }

        public SHNFacility(string fn, int fc, double dac, double dsc, double dlc)
        {
            FacilityName = fn;
            FacilityCapacity = fc;
            DistFromAirCheckpoint = dac;
            DistFromSeaCheckpoint = dsc;
            DistFromLandCheckpoint = dlc;
        }

        public double CalculateTravelCost(string em,DateTime ed)
        {
            double bf = 50;
            double add;
            if(em == "Land")
            {
                add = DistFromLandCheckpoint * 0.22;
            }
            else if(em == "Sea")
            {
                add = DistFromSeaCheckpoint * 0.22;
            }
            else//By air
            {
                add = DistFromAirCheckpoint * 0.22;
            }
            bf += add;
            TimeSpan ts = ed.TimeOfDay;// time retrieved from file
            TimeSpan sixAm = new TimeSpan(6, 0, 0);
            TimeSpan eightFiveNineAm = new TimeSpan(8, 59, 0);
            TimeSpan sixPm = new TimeSpan(18, 0, 0);
            TimeSpan elevFiveNinePm = new TimeSpan(23, 59, 0);
            TimeSpan midnight = new TimeSpan(0, 0, 0);
            TimeSpan fiveFiveNineAm = new TimeSpan(5, 59, 0);
            if((ts>=sixAm && ts <= eightFiveNineAm) || (ts >= sixPm && ts <= elevFiveNinePm))
            {
                bf *= 1.25;
            }
            else if(ts>=midnight && ts <= fiveFiveNineAm)
            {
                bf *= 1.5;
            }
            return bf;
        }//end of Calculate Travel cost method

        public bool IsAvailable()
        {
            if (FacilityVacancy > 0)//available
            {
                Console.WriteLine($"There are {FacilityCapacity - FacilityVacancy} vacancy(s) left.");
                return true; //return true if there are facilities available
            }
            return false;//return false if there are no vacancies left
        }//end of IsAvailable method

        public override string ToString()
        {
            return $"Facility Name: {FacilityName} \t Facility Capacity: {FacilityCapacity} \t Facility Vacancy: {FacilityVacancy}" +
                $" \nDistance From Air Checkpoint: {DistFromAirCheckpoint} \t Distance From Sea Checkpoint: {DistFromSeaCheckpoint} \t Distance From Land Checkpoint: {DistFromLandCheckpoint}";
        }
    }//end of class
}
