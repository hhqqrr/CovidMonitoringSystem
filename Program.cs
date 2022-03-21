using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using ISO3166;//this contains all the country names around the globe, for validation of users entering the countries
using System.Threading;//for using thread.sleep to make the program wait for a little

namespace COVIDMonitoringSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Country[] countryList = Country.List;//this list contains all the countries in the Country Object, from the downloaded ISO3166 extension

            //1) Load both CSV files and populate two lists
            string[] personData = File.ReadAllLines("Person.csv");
            string[] businessData = File.ReadAllLines("BusinessLocation.csv");
            List<Person> personList = new List<Person>();
            List<BusinessLocation> blList = new List<BusinessLocation>();

            //2) Call API and populate list
            List<SHNFacility> SHNFaciList = new List<SHNFacility>();//create a new list for SHNFacility

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://covidmonitoringapiprg2.azurewebsites.net");//get the base addr
                Task<HttpResponseMessage> responseTask = client.GetAsync("/facility");//put in the subdomains
                responseTask.Wait();

                HttpResponseMessage result = responseTask.Result;
                if (result.IsSuccessStatusCode)//if getting from web is success
                {
                    Task<string> readTask = result.Content.ReadAsStringAsync();//read as string
                    readTask.Wait();

                    string data = readTask.Result;
                    SHNFaciList = JsonConvert.DeserializeObject<List<SHNFacility>>(data);//place it into the list of SHNFacility
                }
            }//end of using HttpClient

            //calling the methods to populate the lists
            InitBusinessList(businessData, blList);
            InitPersonList(personData, personList,SHNFaciList);


            //Set the vacancy count to the facility capacity and update the vancacies based on whether a person have used a facility
            foreach (SHNFacility i in SHNFaciList)
            {
                i.FacilityVacancy = i.FacilityCapacity;
                foreach(Person p in personList)
                {
                    foreach(TravelEntry te in p.TravelEntryList)
                    {
                        if(te.ShnStay == i)//if the person's SHN facililty is the same as the one in the SHNFacilist
                        {
                            i.FacilityVacancy -= 1;
                        }
                    }
                }
            }//end of loop


            //Main menu
            bool brk = false;//the main condition to break the loop
            while (brk==false)
            {
                Console.WriteLine("\n-------------------Main Menu---------------------");
                string[] mainMenu = {"List All Visitors","List Person Details","SafeEntry/TraceTogether Menu","TravelEntry Menu","SwabTest"};
                DisplayMenu(mainMenu);
                Console.Write("Enter Option: ");
                string opt = Console.ReadLine();
                //Options
                if (opt == "0")//Exit
                {
                    Console.WriteLine("\nExit\n");
                    break;
                }//end of opt 0, Exit
                else if (opt == "1")//List all visitors===============================================================================================================
                {
                    Console.WriteLine("\n-----List all visitors-----\n");
                    Console.WriteLine("{0,-30} {1,-15} {2,-20}", "Name", "PassportNo.", "Nationality");
                    Console.WriteLine("-------------------------------------------------------------------");
                    foreach (Person p in personList)
                    {
                        if (p is Visitor)//check if the person is a visitor
                        {
                            Visitor vs = (Visitor)p;//downcast to visitor object
                            Console.WriteLine("{0,-30} {1,-15} {2,-20}", vs.Name, vs.PassportNo, vs.Nationality);
                        }
                    }
                }//end of opt 1, List all visitors
                else if (opt == "2")//List person details===============================================================================================================
                {
                    Console.WriteLine("\n-----List person details-----\n");
                    //1) Prompt for name
                    while (true)
                    {
                        Console.Write("Enter the name of person to search: ");
                        string nm = Console.ReadLine();
                        string name = ConvertFirstLtrUpper(nm);//this methods converts the first letter of the names to upper and rest of the letter to lower(for validation)
                        Person searched = SearchName(name, personList);//2) search for person
                        if (searched != null)//searched for the person and the person exists in the list
                        {
                            Console.WriteLine(searched);
                            break;
                        }
                        else//person is not found
                        {
                            Console.WriteLine("\nThe name entered is not found\nTry Again\n");
                        }
                    }//end of while loop
                }//end of opt 2, List person details
                else if(opt == "3")//safe entry menu==========================================================================================
                {
                    string[] SafeEntryMenu = {"Assign/Replace Trace Together Token","List all Business Locations","Edit Business Location Capacity"
                    ,"SafeEntry Check-in","SafeEntry Check-out", "Contact Tracing Report", "Return to Main Menu"};
                    while (true)
                    {
                        Console.WriteLine("\n----------------Safe Entry Menu------------------");
                        DisplayMenu(SafeEntryMenu);
                        Console.Write("Enter option for Safe Entry Menu: ");
                        string sfOpt = Console.ReadLine();
                        if (sfOpt == "0")//break the whole program, the option is writen in the DisplayMenu() method
                        {
                            brk = true;//for the main menu to break
                            break;//for the SafeEntry menu to break
                        }
                        else if (sfOpt == "7")
                        {
                            Console.WriteLine("\nExit to main menu\n");
                            break;//break safeEntry menu to return to main menu
                        }
                        else if(sfOpt == "1")//assign/replace trace tgt token
                        {
                            Console.WriteLine("\n-----Assign/Replace Trace Together Token-----\n");
                            AssignOrReplaceToken(personList);

                        }//end of option 1, assgining or replacing of trace tgt token
                        else if(sfOpt == "2")//List all business locations
                        {
                            Console.WriteLine("\n-----List All Business Locations-----\n");
                            ListAllBusinessLocation(blList);
                        }//end of option 2, listing of all the business locations
                        else if(sfOpt == "3")//edit business location capacity
                        {
                            Console.WriteLine("\n-----Edit Business Location Capacity-----\n");
                            EditBusinessLocationCapacity(blList);
                        }//end of option 3, edit business location capacity
                        else if (sfOpt == "4")//safe entry check IN
                        {
                            Console.WriteLine("\n------Safe Entry Check IN-----\n");
                            SafeEntryCheckIn(blList, personList);
                        }//end of option 4, safe entry check IN
                        else if(sfOpt == "5")//safe entry check OUT
                        {
                            Console.WriteLine("\n------Safe Entry Check OUT-----\n");
                            SafeEntryCheckOut(personList, blList);
                        }//end of option 5, safe entry check OUT
                        else if(sfOpt == "6")//ADV    contact tracing report
                        {
                            Console.WriteLine("\n-----Contact Tracing Report-----\n");
                            ContactTracingReport(blList, personList);
                        }//end of option 6, adv, contact tracing report
                        else//option is invalid
                        {
                            InvalidOpt();
                        }//end of if else statement
                    }//end of while loop
                }//end of opt 3, safe entry menu
                else if(opt == "4")//Travel entry menu===============================================================================================================
                {
                    string[] travelEntryMenu = {"List all SHN Facilities", "Create Visitor", "Create Travel Entry Record", "Calculate SHN Charges", "SHN Status Reporting","Return to Main Menu"};
                    while (true)
                    {
                        Console.WriteLine("\n---------------Travel Entry Menu-----------------");
                        DisplayMenu(travelEntryMenu);
                        Console.Write("Enter option for Travel Entry Menu: ");
                        string teOpt = Console.ReadLine();
                        if (teOpt == "0")//exit the whole thing-------------------------------------------------
                        {
                            Console.WriteLine("\nExit\n");
                            brk = true;//set the condition for the main menu to break it
                            break;//break within the travel entry menu
                        }
                        else if (teOpt == "6")//-------------------------------------------------------------------------
                        {
                            break;//to return to the main menu
                        }
                        else if(teOpt == "1")//List all the SHN facilities---------------------------------------------------
                        {
                            Console.WriteLine("\nList all SHN Facilities\n");
                            Console.WriteLine("{0,-20} {1,-10} {2,-10} {3,-35} {4,-35} {5,-40}", "Facility Name", "Capacity", "Vacancies",
                                "Distance From Air Checkpoint(km)", "Distance From Sea Checkpoint(km)", "Distance From Land Checkpoint(km)");
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------" +
                                "-----------------------------------------");
                            foreach(SHNFacility shn in SHNFaciList)
                            {
                                Console.WriteLine("{0,-20} {1,-10} {2,-10} {3,-35} {4,-35} {5,-40}", shn.FacilityName, shn.FacilityCapacity, shn.FacilityVacancy,
                                    shn.DistFromAirCheckpoint, shn.DistFromSeaCheckpoint, shn.DistFromLandCheckpoint);
                            }
                        }//end of teOpt 1, Listing of all SHN facilities
                        else if (teOpt == "2")//create visitor-----------------------------------------------------------
                        {
                            Console.WriteLine("\n-----Create Visitor-----\n");
                            Console.Write("Enter name of visitor (or enter 'e' to return to Travel Entry menu): ");//following options allows users to exit to the travel entry menu
                            string name = Console.ReadLine();
                            if (name.ToLower() == "e")//return to the travel entry menu
                            {
                                continue;
                            }
                            name = ConvertFirstLtrUpper(name);
                            Console.Write("Enter Passport No. for the visitor (or enter 'e' to return to Travel Entry menu): ");
                            string psNo = Console.ReadLine();
                            if (psNo.ToLower() == "e")
                            {
                                continue;//return to the travel entry menu
                            }
                            Console.Write("Enter nationality of visitor (or enter 'e' to return to Travel Entry menu): ");
                            string nation = Console.ReadLine();
                            if (psNo.ToLower() == "e")
                            {
                                continue;//return to the travel entry menu
                            }
                            personList.Add(new Visitor(name, psNo, nation));
                            Console.WriteLine($"New visitor {name} created.");
                        }
                        else if (teOpt == "3")//create travel entry record-----------------------------------------------------------
                        {
                            bool break3 = false;
                            while (break3 == false)
                            {
                                Console.WriteLine("\n-----Create TravelEntry Record-----\n");
                                Console.Write("Enter name of person (or enter 'e' to return to Travel Entry menu): ");
                                string name = Console.ReadLine();
                                if (name.ToLower() == "e")
                                {
                                    break3 = true;
                                    continue;//return to the travel entry menu
                                }
                                name = ConvertFirstLtrUpper(name);//put first letter to upper case
                                Person p = SearchName(name, personList);
                                if (p == null)//name does not exist
                                {
                                    Console.WriteLine("\nThe name entered is not found. Try again\n");
                                    continue;//skip the rest of the codes
                                }
                                bool brkInner = false;
                                while (brkInner==false)//loop for validations
                                {
                                    Console.WriteLine($"\nName of person: {name}\n");
                                    DisplayAllCountries(countryList);
                                    Console.Write("Enter number corresponding to last country of embarkation (or enter 'e' to return to Travel Entry menu): ");
                                    string lastEmbark = Console.ReadLine();
                                    if (lastEmbark.ToLower() == "e")
                                    {
                                        break3 = true;
                                        brkInner = true;
                                        continue;//return to the travel entry menu
                                    }
                                    try
                                    {
                                        int code = Convert.ToInt32(lastEmbark);
                                        if (code < 1 || code > countryList.Length)//lower than 1 or more than the numbers of countries to choose from
                                        {
                                            throw new Exception();
                                        }
                                        lastEmbark = countryList[code - 1].Name;
                                        Console.WriteLine($"\nYour chosen country is: {lastEmbark}");
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Input cannot be string.");

                                    }
                                    catch (Exception)
                                    {
                                        InvalidOpt();
                                        continue;
                                    }
                                    bool validOpt = false;
                                    while (validOpt == false)
                                    {
                                        Console.Write("Enter entry mode(Land/Air/Sea) (or enter 'e' to return to Travel Entry menu): ");//choosing the mode of travel
                                        string eMode = Console.ReadLine();
                                        if (eMode.ToLower() == "e")
                                        {
                                            break3 = true;
                                            brkInner = true;
                                            break;//return to the travel entry menu
                                        }
                                        eMode = ConvertFirstLtrUpper(eMode);//this method converts the first letter of the input to upper, rest to lower

                                        if (eMode != "Land" && eMode != "Sea" && eMode != "Air")//if the input is not in any of this
                                        {
                                            Console.WriteLine("\nPlease enter a valid entry mode.\n");//loop for users to try again
                                        }
                                        else//input is valid
                                        {

                                            DateTime entryDt = DateTime.Now;//default it to date of creation
                                            while (true)
                                            {
                                                Console.Write("\nEnter date (yyyy/mm/yy)of entry\nEnter 'd' to use current date\nEnter 'e' to exit\nEnter Option: ");
                                                string date = Console.ReadLine();
                                                if (date.ToLower() == "e")
                                                {
                                                    validOpt = true;
                                                    break3 = true;
                                                    brkInner = true;
                                                    break;
                                                }
                                                else if (date.ToLower() == "d")
                                                {
                                                    //dont have to assign since it was already default to current date
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        entryDt = Convert.ToDateTime(date);
                                                        Console.Write("Enter time of entry in 24 hour format '00:00' or 'e' to exit: ");
                                                        string t = Console.ReadLine();
                                                        if (t.ToLower() == "e")
                                                        {
                                                            break3 = true;
                                                            brkInner = true;//break all the loops to return
                                                            validOpt = true;
                                                            break;
                                                        }
                                                        TimeSpan ti = TimeSpan.Parse(t);
                                                        entryDt = entryDt.Add(ti);
                                                        
                                                    }
                                                    catch (Exception)
                                                    {
                                                        InvalidOpt();//if there is any error, print invalid option message and prompt again
                                                        continue;
                                                    }
                                                }

                                                if (lastEmbark == "Macao")//there are some differences in the country list and the given list from assignment
                                                {
                                                    lastEmbark = "Macao SAR";
                                                }
                                                else if (lastEmbark == "Viet Nam")
                                                {
                                                    lastEmbark = "Vietnam";
                                                }
                                                TravelEntry te = new TravelEntry(lastEmbark, eMode, entryDt);//create travel entry object
                                                Console.WriteLine($"\nTravel Entry Record Created\nLast Embarked Country: {lastEmbark} \t Entry Mode: {eMode} \t Date: {entryDt}");
                                                te.CalculateSHNDuration();
                                                if (te.ShnEndDate == te.EntryDate)
                                                {
                                                    Console.WriteLine("\nYou do not need to serve SHN\n");
                                                }
                                                else if (te.ShnEndDate.Subtract(te.EntryDate).Days == 7)//if the endDate minus the entry date is 7
                                                {
                                                    Console.WriteLine("\nYou need to serve 7 days of SHN at your own accommodation\n");
                                                }
                                                else//14 days SHN at SDF
                                                {
                                                    Console.WriteLine("\nYou need to serve 14 days of SHN at chosen SDF\n");
                                                    SHNFacility shn = SHNFacilitiesOption(SHNFaciList, eMode);//a method to display,validate and return SHNFacility obj
                                                                                                               //pass in the faclities list and the entry mode of the person
                                                    if (shn == null)//user chose to exit to the safe entry menu
                                                    {
                                                        break3 = true;//set the condition to break the main loop to true
                                                        brkInner = true;
                                                        validOpt = true;
                                                        break;//break the inner loop;
                                                    }
                                                    else//valid option
                                                    {
                                                        te.AssignSHNFacility(shn);
                                                        shn.FacilityVacancy -= 1;//reduce vacancy count
                                                        
                                                    }
                                                }
                                                te.IsPaid = false;//the user have not paid for the SHN facility, so set to false first
                                                p.AddTravelEntry(te);//add travel entry records to the person
                                                Console.WriteLine("\nTravel Entry Record added.\n");
                                                break3 = true;
                                                brkInner = true;
                                                validOpt = true;
                                                break;//exit
                                            }
                                        }//end of else statement
                                    }//end of validOpt while loop
                                }//end of while loop
                            }
                        }//end of teOpt 3, cration of travelEntry record

                        else if(teOpt == "4")//Calculate SHN Charges-----------------------------------------------------------------------------
                        {
                            bool break4 = false;//the condition for users to return to the safe entry menu
                            while (break4 == false)
                            {
                                Console.WriteLine("\n-----Calculate SHN Charges-----\n");
                                Console.Write("Enter name of person (or enter 'e' to return to Travel Entry menu): ");
                                string name = Console.ReadLine();
                                if(name.ToLower() == "e")
                                {
                                    break4 = true;//return to the travel entry menu
                                    continue;
                                }
                                name = ConvertFirstLtrUpper(name);
                                Person ps = SearchName(name, personList);
                                if (ps == null)//person does not exist
                                {
                                    Console.WriteLine("\nPerson with the name entered does not exists. Try again\n");
                                }
                                else//the person exists
                                {
                                    Console.WriteLine($"\nName of person: {name}\n");
                                    foreach (TravelEntry te in ps.TravelEntryList)
                                    {
                                        if (DateTime.Now > te.ShnEndDate && te.IsPaid == false)//SHN ended and no paid
                                        {
                                            double cost = ps.CalculateSHNCharges();//conditions and if else statements are in the classes already for part4 i. and ii.
                                            while (true)
                                            {
                                                Console.Write($"Confirm payment of {cost.ToString("$0.00")}, press 1 to continue (or enter 'e' to return to Travel Entry menu): ");
                                                string ans = Console.ReadLine();
                                                if (ans.ToLower() == "e")
                                                {
                                                    Console.WriteLine($"\nWarning! Your charges of {cost.ToString("$0.00")} are unpaid.\n\nReturning to the Travel Entry Menu.\n");
                                                    break4 = true;
                                                    break;
                                                }
                                                else if (ans != "1")//user has chosen to continue
                                                {
                                                    InvalidOpt();//display that the option entered is invalid
                                                    continue;//skips the other codes
                                                }
                                                Console.WriteLine("\nPayment was successfull\n");
                                                te.IsPaid = true;//change the boolean value to true
                                                break4 = true;
                                                break;
                                            }//end of while loop
                                        }//end of if statement for retrieving TravelEntry with SHN ended and unpaid
                                    }//end of foreach loop for the list of TravelEntry in each person
                                    if (break4 == true)
                                    {
                                        continue;//if payment was already successful or user has chosen to exit, break4 would already be set to true
                                    }
                                    Console.WriteLine("\nYou have no outstanding charges\n");//if the person has not charges payable currently
                                    break4 = true;
                                }
                            }
                        }//end of opt 4, calculation of SHN charges
                        else if (teOpt == "5")//ADV   SHN Status Reporting----------------------------------------------------------------------------------------
                        {
                            bool break5 = false;
                            while (break5 == false)
                            {
                                List<Person> serving = new List<Person>();//create a new list to store person obj, that are currently serving SHN
                                Console.WriteLine("\n-----SHN Status Reporting-----\n");
                                Console.Write("Enter date (yyyy/mm/dd) (or enter 'e' to return to Travel Entry menu): ");
                                string date = Console.ReadLine();
                                if (date.ToLower() == "e")
                                {
                                    break;//return to travel entry menu
                                }
                                try
                                {
                                    DateTime dt = Convert.ToDateTime(date);
                                    foreach (Person p in personList)
                                    {
                                        foreach (TravelEntry te in p.TravelEntryList)
                                        {
                                            if (te.EntryDate.Date <= dt.Date && te.ShnEndDate.Date >= dt.Date)//those currently serving SHN(need to use the .Date func to compare only the dates)
                                            {
                                                serving.Add(p);
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    InvalidOpt();
                                    continue;
                                }
                                bool apend = true;//default it to true, to append data to the original file or not
                                if (File.Exists("Serving.csv"))//checks if file exists, returns true if it exists and executes the following codes
                                {
                                    string[] data = File.ReadAllLines("Serving.csv");
                                    if (data.Length != 0)//file exists and have something in it(to filter out empty files)
                                    {
                                        while (true)
                                        {
                                            if (serving.Count == 0)//no records found 
                                            {//for the date, i chose the date (string) as input by user, if the format is wrong, the loop will not get to this code, so the format entered by user is definitely correct
                                                Console.WriteLine($"\n There are currently no one serving SHN on {date}\nIf you choose to append data, no new data will be created" +
                                                    $"\nIf you choose to overrride current data, the data will be overriden with empty data");
                                            }
                                            Console.WriteLine("\n-----There are existing data in the records-----\n[e] Return to Travel Entry Menu\n[1] Append new data" +
                                                "\n[2] Override the current data\n[3] Display existing data\n");
                                            Console.Write("Enter option: ");
                                            string op = Console.ReadLine();
                                            if (op.ToLower() == "e")
                                            {
                                                break5 = true;
                                                break;
                                            }
                                            else if (op == "1")//append the data
                                            {
                                                apend = true;
                                                break;
                                            }
                                            else if (op == "2")//override
                                            {
                                                Console.WriteLine("\n-----Warning! This option will remove all existing data and replace it with new data-----\n");
                                                Console.Write("[e] Back to Travel Entry Menu\n[1] Confirm override\n[2] Change option to append data: ");
                                                string ans = Console.ReadLine();
                                                if (ans.ToLower() == "e")//exit to travel entry menu
                                                {
                                                    break5 = true;
                                                    break;
                                                }
                                                else if (ans == "1")//confirm override data
                                                {
                                                    apend = false;
                                                    break;
                                                }
                                                else if(ans == "2")//append data
                                                {
                                                    break;//dont have to assign again since default is already assigned as true at the top
                                                }
                                                else//option is invalid and will bring user back to choosing again due to while loop
                                                {
                                                    InvalidOpt();
                                                }
                                            }
                                            else if (op == "3")//display existing data
                                            {
                                                Console.WriteLine("{0,-15} {1,-25} {2,-15}", "Name", "SHN end date", "Facility Name");
                                                Console.WriteLine("-------------------------------------------------------");
                                                foreach(string i in data)
                                                {
                                                    string[] line = i.Split(',');
                                                    Console.WriteLine("{0,-15} {1,-25} {2,-15}", line[0], line[1], line[2]);
                                                }
                                                continue;//loop so that they can choose again with the data
                                            }
                                            else//invalid opt
                                            {
                                                InvalidOpt();
                                            }
                                        }//end of while loop to prompt to append or override
                                    }//end of if file already exists and have data in it
                                }//end of if file already exists
                                if(break5 == true) { continue; }//skip the writing of file if users decide to exit
                                using (StreamWriter sw = new StreamWriter("Serving.csv", apend))//start to write data into file
                                {
                                    foreach(Person p in serving)
                                    {
                                        foreach(TravelEntry te in p.TravelEntryList)//can append multiple times for the same person if they got more than one entry details
                                        {
                                            sw.WriteLine($"{p.Name},{te.ShnEndDate},{te.ShnStay.FacilityName}");
                                        }
                                    }
                                }//end of stream writer
                                break5 = true;
                                Console.WriteLine("\nCSV Report Generated: Data in the file is updated\n");
                            }//end of while loop
                        }//end of teOpt 5, SHN Status Reporting
                        else//invalid option
                        {
                            InvalidOpt();
                        }
                    }//end of while loop within the travel entry menu
                }//end of Travel entry menu
                else if(opt == "5")
                {
                    
                    
                    PerformSwab(personList);
                }
                else//invalid option
                {
                    InvalidOpt();
                }
            }//end of main menu loop


        }//end of main method

        //this methods initialises data given
        static void InitPersonList(string[] personData, List<Person> personList,List<SHNFacility>SHNFaciList)
        {
            for (int i = 1; i < personData.Length; i++)//start i = 1 to remove the heading
            {
                TravelEntry te = null;
                TraceTogetherToken tt = null;
                string[] line = personData[i].Split(',');
                bool did = Convert.ToBoolean(line[15]);
                if (line[0] == "resident")
                {
                    Resident p = (new Resident(line[1], line[2], Convert.ToDateTime(line[3])));
                    if (line[9] != "" && line[10] != "" && line[11] != "" && line[12] != "" && line[13] != "")//for travel entry
                    {
                        te = new TravelEntry(line[9], line[10], Convert.ToDateTime(line[11]));
                        p.AddTravelEntry(te);
                        te.ShnEndDate = Convert.ToDateTime(line[12]);
                        te.IsPaid = Convert.ToBoolean(line[13]);
                        te.CalculateSHNDuration();
                    }
                    //for safe Entry
                    if (line[6] != "" && line[7] != "" && line[8] != "")
                    {
                        tt = new TraceTogetherToken(line[6], line[7], Convert.ToDateTime(line[8]));
                        p.Token = tt;
                    }
                    //personList.Add(r);
                    p.Did = did;
                    personList.Add(p);
                }
                else if (line[0] == "visitor")
                {
                    Visitor p = (new Visitor(line[1], line[4], line[5]));
                    if (line[9] != "" && line[10] != "" && line[11] != "" && line[12]!="" && line[13] != "")//for travel entry
                    {
                        te = new TravelEntry(line[9], line[10], Convert.ToDateTime(line[11]));
                        p.AddTravelEntry(te);
                        te.ShnEndDate = Convert.ToDateTime(line[12]);
                        te.IsPaid = Convert.ToBoolean(line[13]);
                        te.CalculateSHNDuration();
                    }
                    //for safe ENtry
                    p.Did = did;
                    personList.Add(p);
                }
                if (line[13] != "" && te != null)//if the travel entry is applicable (from above, if not it will remain null)
                {
                    te.IsPaid = Convert.ToBoolean(line[13]);//set the IsPaid of the travel entry to the value in the file
                }//end of assigning IsPaid to travel Entry
                if (line[14] != "" && te != null)//if the travel entry is applicable (from above, if not it will remain null)
                {
                    SHNFacility shn = SearchSHNFaci(SHNFaciList, line[14]);
                    if (shn != null)//only assign if it does not equal to null i.e. SHNFacility is not found
                    {
                        te.AssignSHNFacility(shn);
                    }
                }//end of assigning of shn faci to Travel entry
                
                

            }
        }//end of init person method


        //this method initializes business list
        static void InitBusinessList(string[] businessData, List<BusinessLocation> businessList)
        {
            
            for (int i = 1; i < businessData.Length; i++)
            {
                string[] businessLocationInfo = businessData[i].Split(",");   
                int maxCap = Convert.ToInt32(businessLocationInfo[2]);
                BusinessLocation blObject = new BusinessLocation(businessLocationInfo[0], businessLocationInfo[1], maxCap);
                businessList.Add(blObject);
            }
        
        }//end of init business list method


        //this method displays menu of options with an option at the back for users to exit, this method requires an array of options in string
        static void DisplayMenu(string[] menu)
        {
            for (int i = 0; i < menu.Length; i++)
            {
                Console.WriteLine($"[{i + 1}] {menu[i]}");
            }
            Console.WriteLine("[0] Exit the program\n--------------------------------------------------");//add exit option
        }//end of DisplayMenu method


        //this method searches for the name entered by a person and returns the Person object, or null if it the person is not found
        static Person SearchName(string name,List<Person>personList)
        {
            foreach(Person p in personList)
            {
                if (p.Name == name)
                {
                    return p;
                }
            }
            return null;//return null if the person is not found
        } //end of SearchName method
        
        //===SafeEntry/TraceTogether===

        //Search for BusinessLocation object
        static BusinessLocation SearchForBusinessLocation(string businessName, string branchCode, int maxCap, List<BusinessLocation> businessFile)
        {
            foreach (BusinessLocation b in businessFile)
            {
                if (businessName == b.BusinessName && branchCode == b.BranchCode && maxCap == b.MaximumCapacity)
                {
                    return b;
                }
            }
            return null; //return null if the BusinessLocation object is not found
        } //end of SearchForBusinessLocation method

       
        
        //following option for printing error messages
        static void InvalidOpt()
        {
            Console.WriteLine("\nInvalid option. Try again.\n");
        }


        //this option is to search for SHNFacility Object with the name of the facility passed in
        static SHNFacility SearchSHNFaci(List<SHNFacility> SHNFaciList,string faciName)
        {
            foreach(SHNFacility shn in SHNFaciList)
            {
                if(shn.FacilityName == faciName)
                {
                    return shn;//return the shn faci
                }
            }
            return null;//null if there is no matching faci name
        }//end of SearchSHNFaci

        //this method converts the first letter of strings passed to Uppercase and rest to lower case, if separated by spaces, it will also be in upper case
        /* E.g. "examPle sTring"  will be converted to "Example String"*/
        static string ConvertFirstLtrUpper(string input)//this method converts first letter to upper and rest to lower cases
        {
            if (input == "") { return ""; }//if user passed in an empty string
            string tgt = "";//make it blank first  
            string[] splt = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);//if user enters consecutive spaces, there will be errors so we need to ignore the spaces
            bool addSpace = false;
            if (splt.Length > 1) { addSpace = true; }//if the count of the array of strings, are more than 1, meaning got spaces in the user input
            foreach (string sp in splt)
            {
                List<char> chaList = new List<char>();//make a list of chars
                foreach (char i in sp)
                {
                    chaList.Add(i);//add each char in the input to a list
                }
                tgt += Convert.ToString(char.ToUpper(chaList[0]));//first letter to upper case
                for (int i = 1; i < chaList.Count; i++)
                {
                    string s = Convert.ToString(char.ToLower(chaList[i]));//make the rest of them to lower case
                    tgt += s;//string them up together
                }
                if (addSpace == true) { tgt += " "; }//if spaces are needed
            }
            return tgt;//return the final value;
        }//end of ConvertFirstLtrUpper method


        //option for users to select SHNFacilities and validates if there are enough spaces
        static SHNFacility SHNFacilitiesOption(List<SHNFacility> shnL,string em)//this method returns a SHNFacility
        {
            while (true)
            {
                Console.WriteLine("\nSelect SHN Facility\n");
                Console.WriteLine("{0,-2} {1,-20} {2,-20} {3,-20} {4,-30}", "No.", "Facility Name", "Max Capacity", "Vacancies", "DistFrom" + em + "Checkpoint(km)");
                double dist = 0;
                for (int i = 0; i < shnL.Count; i++)
                {
                    SHNFacility f = shnL[i];
                    if (em == "Air")
                    {
                        dist = f.DistFromAirCheckpoint;
                    }
                    else if(em == "Sea")
                    {
                        dist = f.DistFromSeaCheckpoint;
                    }
                    else//travelled by land
                    {
                        dist = f.DistFromLandCheckpoint;
                    }
                    Console.WriteLine("[{0,-1}]  {1,-20} {2,-20} {3,-20} {4,-30}",i+1, f.FacilityName, f.FacilityCapacity, f.FacilityVacancy, dist);

                }
                Console.WriteLine("[0]  Discard current progress and return to Safe Entry Menu");
                Console.Write("Enter Facility Number: ");
                string ans = Console.ReadLine();
                if (ans == "0")//exit
                {
                    return null;//return null and break the code
                }
                try
                {
                    int opt = Convert.ToInt32(ans);//if error occurs, the catch statement will catch it
                    if (opt < 0 || opt > shnL.Count || shnL[opt].FacilityVacancy == 0)
                    {
                        throw new Exception();//if got error, throw to catch statement
                    }
                    return shnL[opt-1];//return the SHN Facility and break

                }
                catch(Exception)
                {
                    Console.WriteLine("\nInvalid option. Try Again\n");
                }
            }
        }//end of SHN facilities option method

        //this method displays all the countries for users to choose from
        static void DisplayAllCountries(Country[] countries)
        {
            for (int i = 0; i < countries.Length - 1; i += 3)
            {
                Console.WriteLine("{0,-4}| {1,-47} {2,-4}| {3,-47} {4,-4}| {5,-47}", i + 1, countries[i].Name, i + 2, countries[i + 1].Name, i + 3, countries[i + 2].Name);
            }
        }//end of display all countries

        //this method searched for resident object based on the name entered
        static Resident SearchResident(string name, List<Person> pList)
        {
            foreach (Person p in pList)
            {
                if (name == p.Name && p is Resident)
                {
                    Resident r = (Resident)p; //downcast person object to resident object
                    return r;

                }
            }
            return null;
        }//end of search resident
        
        //this method assigns and/or replaces token
        static void AssignOrReplaceToken(List<Person> pList)
        {
            while (true)
            {
                bool newToken = false;//this is for skipping the replace token codes, if user is creating new token
                Console.Write("Please enter your name (or enter 'e' to exit): "); //prompt user for name
                string userName = Console.ReadLine(); //assign user input into string variable userName
                if (userName.ToLower() == "e") { break; }// break to safe entry menu
                userName = ConvertFirstLtrUpper(userName);//convert the first letter to upper case for validation
                Resident selectedResident = SearchResident(userName, pList); //Check if resident is in a list

                if(selectedResident != null)//resident exists
                {
                    if (selectedResident.Token == null)//the resident does not have existing token
                    {
                        newToken = true;
                        Console.WriteLine("\nYou do not have exisiting token\nCreate a new trace together token.\n");
                        string serialNo;
                        serialNo = GenerateToken(pList);
                        Console.WriteLine($"\nAssigned Serial No.: {serialNo}");
                         //made sure that the serial number is unique in the method GenerateToken() itself

                        Console.Write("Enter you collection location (or enter 'e' to exit): ");
                        string collectionLocation = Console.ReadLine();
                        if (collectionLocation.ToLower() == "e")
                        {
                            break;
                        }//return to break
                         //cannot use the ConvertFirstLtrUpper method since some of the names are not in the format
                        DateTime dateForAssignment = DateTime.Now; //get date for Assignment of TraceTogether Token
                        DateTime expiryDate = dateForAssignment.AddMonths(6); //Add 6 months to dateForAssignment to get expiryDate

                        selectedResident.Token = new TraceTogetherToken(serialNo, collectionLocation, expiryDate);
                        // Assign new TraceTogetherToken to the resident
                        Console.WriteLine("\nTrace Together token added successfully.\n");
                        Console.WriteLine($"Details\n---------\n{selectedResident.Token}");
                    }
                    else//resident has a trace together token
                    {
                        Console.WriteLine($"Name: {selectedResident.Name}");
                        Console.WriteLine(selectedResident.Token);
                    }
                    //code for replace token
                    if(newToken == true)//skip all other codes if user jus made a new token
                    {
                        Console.WriteLine("\nReturning to Safe Entry Menu.\n");
                        break;
                    }
                    if (selectedResident.Token.IsEligibleForReplacement() == false)
                    {
                        Console.WriteLine("\nYou are not eligible for replacement of token.\n");
                        Console.WriteLine("\nBack to Safe Entry Menu\n");
                        break;
                    }
                    else
                    {//resident is eligible (replace the token for the resident)
                        Console.WriteLine("\nYou are elgible for replacement of token.");
                        string sr = GenerateToken(pList);
                        Console.Write("Enter collection location  (or enter 'e' to exit): ");
                        string loca = Console.ReadLine();
                        if (loca.ToLower() == "e")
                        {
                            break;//exit
                        }
                        DateTime exp = DateTime.Now.AddMonths(6);
                        selectedResident.Token = new TraceTogetherToken(sr, loca, exp);
                        Console.WriteLine($"\nNew token with the following details added to {selectedResident.Name}\n{selectedResident.Token}\n");
                        Console.WriteLine("\nBack to Safe Entry Menu\n");
                        break;
                    }

                }//end of if statement
                else
                {
                    Console.WriteLine("\nResident is not found.\n");
                }
            }
        }//End of AssignOrReplaceToken method

        static string GenerateToken(List<Person> personList)//this method generates token and ensures that there are no repeats
        {
            string serialNo;
            while (true)
            {
                string firstLetter = "T";
                Random rnd = new Random();
                int num = rnd.Next(1000, 9999); //Generate random 4 digit number
                string fourDigitString = Convert.ToString(num);  //Convert int num  to string
                serialNo = firstLetter + fourDigitString; //create a serialNo for the person
                bool same = false;//assign having the same serial no. to false first
                foreach (Person p in personList)//this loop is to see if there are any same serial numbers in the list provided
                {
                    if (p is Resident)
                    {
                        Resident r = (Resident)p;//down cast to resident
                        if (r.Token != null)//to prevent residents without tokens to run the code
                        {
                            if (r.Token.SerialNo == serialNo)
                            {
                                same = true;
                                break;
                            }
                        }
                    }
                }//end of foreachloop
                if (same != true)//if the token does not have the same serial no.
                {
                    return serialNo;
                }
            }
        }
        
        
        //this method lists all the business locations
        static void ListAllBusinessLocation(List<BusinessLocation> businessList) //Method to list all business locations  
        {
            Console.WriteLine("{0,-25} {1,-15} {2,-20} {3,-20}", "Business Name", "Branch Code", "Maximum Capacity", "Visitors Now");
            Console.WriteLine("--------------------------------------------------------------------------------");
            foreach (BusinessLocation b in businessList)
            {
                Console.WriteLine("{0,-25} {1,-15} {2,-20} {3,-20}", b.BusinessName, b.BranchCode, b.MaximumCapacity, b.VisitorsNow);
            }
        }// End of ListAllBusinessLocation method


        //in this method, we prompted for all the details instead of just the businessname or the branchcode as the businessname or branchcode may not be unique
        //with all the details including old maximum capacity, we are able to identify a unique business
        static void EditBusinessLocationCapacity(List<BusinessLocation> businessFile) //Method to edit selected business location
        {
            string businessName;
            string branchCode;
            int maxCap;
            bool brk = false;
            while (brk == false)
            {
                while (true)
                {
                    Console.Write("Please enter business name of the business (or enter 'e' to exit): ");
                    businessName = Console.ReadLine();
                    if (businessName.ToLower() == "e") 
                    {
                        brk = true;
                        break; 
                    }
                    bool bName = BusinessNameExists(businessName, businessFile);
                    if (bName == false)
                    {
                        Console.WriteLine("\nThe business name entered does not exist. Try again.\n");
                    }
                    else
                    {
                        break;
                    }
                }//end of while loop
                if(brk == true) { break; }//prevent the code to execute when the user has enterd to exit the program
                while (true)
                {
                    Console.Write("Please enter branch code of the business (or enter 'e' to exit): ");
                    branchCode = Console.ReadLine();
                    if (branchCode.ToLower() == "e") 
                    {
                        brk = true;
                        break; 
                    }
                    bool branch = BusinessBranchExists(branchCode, businessFile);//returns true if the branch exists and false if the branch does not exist
                    if(branch == true) { break; }//the branch is found
                    Console.WriteLine("\nThe Branch Code entered does not exist. Try again.\n");//to reach this code, the branch code must be non-existent, if it exists, the loop would have been broken by the break statement above
                }
                if (brk == true) { break; }//prevent the code to execute when the user has enterd to exit the program
                bool brkInner3 = false;
                while (brkInner3 == false)
                {
                    Console.Write("Please enter the old maximum capacity of the business (or enter 'e' to exit): ");
                    string cap = Console.ReadLine();
                    if (cap.ToLower() == "e") 
                    {
                        brk = true;
                        break; 
                    }
                    try
                    {
                        maxCap = Convert.ToInt32(cap);// try to conver the input to int, if cannot convert, exception will execute
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nPlease enter a valid input\n");
                        continue;
                    }
                    //after validation of all data, find the location
                    BusinessLocation bl = SearchForBusinessLocation(businessName, branchCode, maxCap, businessFile);
                    if ( bl != null) //Method to 
                    {
                        while (true)
                        {
                            Console.Write("\nPlease enter the New maximum capacity of the business (or enter 'e' to exit): ");
                            string nw = Console.ReadLine();
                            if (nw.ToLower() == "e")
                            {
                                brkInner3 = true;
                                brk = true;//exit
                                break;
                            }
                            int editedMaxCap;
                            try
                            {
                                editedMaxCap = Convert.ToInt32(nw);//try to convert it to int
                            }
                            catch (Exception)
                            {
                                InvalidOpt();
                                continue;
                            }

                            bl.MaximumCapacity = editedMaxCap;
                            Console.WriteLine($"\nData updated.Below shows the latest data.\n{bl}");
                            brkInner3 = true;
                            brk = true;
                            break;
                        }//end of while loop
                    }
                    else
                    {
                        Console.Write("\nBusiness Location not valid (Not all the details entered match to one unique business location). Please try again.\n");
                        break;//break the brkInner3 loop for the main loop to run again and prompt user
                    }

                }//end of third inner while loop
            }//end of main while loop
        }//End of EditBusinessLocationCapacity Method

        //this method returns true or false(whether the business exists) for the name of the business entered
        static bool BusinessNameExists(string name, List<BusinessLocation> bList)
        {
            foreach(BusinessLocation i in bList)
            {
                if(i.BusinessName == name)
                {
                    return true;//return true once the location is found
                }
            }
            return false;//after loop, if the location does not exist, return false
        }//end of BusinessNameExists

        static bool BusinessBranchExists(string code, List<BusinessLocation> bList)
        {
            foreach(BusinessLocation i in bList)
            {
                if (i.BranchCode == code)
                {
                    return true;// once the code is found, return true
                }
            }
            return false;//after all iterations if the code is not found, return false
        }


        static void SafeEntryCheckIn(List<BusinessLocation> businessList, List<Person> personList) //Method to for user to check in to safe entry
        {
            string name;
            Person p = null;
            bool brk = false;
            while (brk == false)
            {
                while (true)
                {
                    Console.Write("Enter your name (or enter 'e' to exit): ");
                    name = Console.ReadLine();
                    if (name.ToLower() == "e")
                    {
                        brk = true;//break main
                        break;
                    }
                    name = ConvertFirstLtrUpper(name);//format the input
                    p = SearchName(name, personList);//returns null if the name is not found
                    if (p == null || p is Visitor)
                    {
                        Console.WriteLine("\nName entered could not be found. Try Again.\nNote that the person with name entered must be a resident to continue");
                    }
                    else
                    {
                        break;
                    }
                }//end of first inner loop
                if (brk == true) { break; }//break and skip the rest of the code if user chose to exit from the program
                while (true)
                {
                    Console.WriteLine("{0,2} {1,-25} {2,-15} {3,-20} {4,-20}", "No.", "Business Name", "Branch Code", "Maximum Capacity", "Visitors Now");
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    for(int i = 0; i < businessList.Count; i++)
                    {
                        Console.WriteLine("{0,2}) {1,-25} {2,-15} {3,-20} {4,-20}", i + 1, businessList[i].BusinessName, businessList[i].BranchCode, businessList[i].MaximumCapacity,
                            businessList[i].VisitorsNow);
                    }
                    Console.WriteLine(" e) Exit");//option to exit
                    Console.Write("\nEnter option: ");
                    string opt = Console.ReadLine();
                    if (opt.ToLower() == "e")
                    {
                        brk = true;//exit
                        break;
                    }
                    try
                    {
                        int ans = Convert.ToInt32(opt);//if unable to convert to int, throw exception
                        if(ans < 1 || ans > businessList.Count)//if the int entered is not in range given
                        {
                            throw new Exception();
                        }
                        BusinessLocation chosenB = businessList[ans-1];//assgin chosen location
                        bool InNotOut = CheckInSameLocation(p, chosenB);
                        if(InNotOut == true)
                        {
                            Console.WriteLine("\nYou have already checked in the same location but have not checked out.\nYou cannot check in again to the same location without checking out first\n");
                            continue;
                        }
                        if (chosenB.IsFull() == true)
                        {
                            Console.WriteLine("\nThe current location is already full. Try again.\n");
                            continue;//lets user choose locations again, and not exit directly
                        }
                        else//the location is not full
                        {
                            SafeEntry se = new SafeEntry(DateTime.Now, chosenB);//create new Safe Entry object with currrent time and the chosen location
                            chosenB.VisitorsNow += 1;
                            p.AddSafeEntry(se);
                            Console.WriteLine($"\nSafe Entry Record added for {p.Name}, Details are as follows\nCheck In Time: {se.CheckIn} \t Location: {chosenB.BusinessName}\n");
                            Console.WriteLine("\nReturning to Safe Entry Menu");
                            brk = true;
                            break;//exit the method
                        }
                    }
                    catch (Exception)
                    {
                        InvalidOpt();
                        continue;
                    }
                }//end of second inner loop
            }//end of main while loop
   
        }//end of SafeEntry CheckIn

        //this method checks if a person has checked in a location and not checked out yet (for validation)
        static bool CheckInSameLocation(Person p, BusinessLocation bl)
        {
            foreach(SafeEntry se in p.SafeEntryList)
            {
                if (se.Location == bl && se.CheckIn > se.CheckOut)//if it is the same locationa and the (defaulted) checkout time is earlier than checkin time
                {
                    return true;
                }
            }
            return false;//false after looping through
        }//end of ChckInSameLocation

        //method for safe entry check out
        static void SafeEntryCheckOut(List<Person> personList, List<BusinessLocation> businessFile)
        {
            bool brk = false;
            while(brk == false)
            {
                Person p = null;
                while (true)
                {
                    Console.Write("Enter name to Check Out (or enter 'e' to exit): ");
                    string name = Console.ReadLine();
                    if (name.ToLower() == "e")
                    {
                        brk = true;
                        break;//exit
                    }
                    name = ConvertFirstLtrUpper(name);
                    p = SearchName(name, personList);
                    if (p == null)//does not exist
                    {
                        Console.WriteLine("\nThe name entered does not exist. Try again");
                    }
                    else
                    {
                        break;//person exists
                    }
                }//end of first inner loop
                if (brk == true) { break; }//prevent code from running if user already exited
                if (HaveUnchecked(p) == false)
                {
                    Console.WriteLine("\nYou do not have any pending check outs.\n");
                    break;
                }
                while (true)
                {
                    List<SafeEntry> inNotOut = new List<SafeEntry>();//new list to add in all the safe entry records where the resident checked in, but not out
                    int count = 1;
                    foreach(SafeEntry se in p.SafeEntryList)
                    {
                        if (se.CheckOut <= se.CheckIn)//the default time set for the checkout is way earlier than the check in time, it is not possible for checkout time to be earlier than check in time
                        {//so we can use it as a condition to check if the use have already checkout
                            Console.WriteLine(count + ") {0,-25} {1,-25}", se.Location.BusinessName, se.CheckIn);
                            inNotOut.Add(se);//add safe entry record to list
                            count++;
                        }
                    }
                    Console.WriteLine("e) Exit");
                    Console.Write("\nEnter option: ");
                    string opt = Console.ReadLine();
                    if (opt.ToLower() == "e")
                    {
                        brk = true;
                        break;//exit
                    }
                    try
                    {
                        int chosen = Convert.ToInt32(opt);
                        if(chosen < 1 || chosen > inNotOut.Count)//not in range, throw to catch statemnent
                        {
                            throw new Exception();
                        }
                        SafeEntry ckOut = inNotOut[chosen - 1];//assign chosen safe entry record to safe entry variable
                        ckOut.PerformCheckOut();//check out
                        ckOut.Location.VisitorsNow -= 1;//reduce count by 1
                        Console.WriteLine("\nCheck Out successful");
                        Console.WriteLine($"\nDetails are as follows\n{ckOut}");
                        Console.WriteLine("\nReturning to safe entry menu.");
                        brk = true;
                        break;//exit
                    }
                    catch (Exception)
                    {
                        InvalidOpt();
                        continue;
                    }

                }//end of second inner loop
            }//end of main while loop
        }//End of SafeEntryCheckOut Method

        //this method checks if a person has any pending checkouts
        static bool HaveUnchecked(Person p)//returns true if person have any pending checkouts
        {
            foreach(SafeEntry se in p.SafeEntryList)
            {
                if (se.CheckOut == default(DateTime))
                {
                    return true;
                }
            }
            return false;//does not have any pendig checkouts
        }//end of HaveUnchecked method

        //ADVANCED ADDITIONAL Perform swab test for user
        static void PerformSwab(List<Person>pList)
        {
            bool brk = false;
            while (brk == false)
            {
                Console.Write("Enter name (or enter 'e' to exit): ");
                string name = Console.ReadLine();
                if (name.ToLower() == "e")
                {
                    break;//exit the loop
                }
                name = ConvertFirstLtrUpper(name);//call method to format the name (incase user enters weird format)
                Person p = SearchName(name,pList);//this method returns Person object if the person is found, and returns null if not found
                if (p == null)//if person does not exist
                {
                    Console.WriteLine("\nThe name entered is not found, Please register (i.e. Create visitor object) before proceeding with the swab test.\n");
                    break;
                }
                
                bool done = false;
                DateTime entryDate = DateTime.Now;//set the date time to now first as the default
                foreach (TravelEntry te in p.TravelEntryList)
                {
                    entryDate = te.EntryDate;//get the person's last travel entry object and assign the entry date
                    done = true;//declare that person have a travel entry record, therefore done a swab test before
                }
                bool innerBrk = false;//this is the condition for a while loop in later code
                if (done == true || p.Did==true)//if the person have done a swab test before
                {
                    while (true)
                    {
                        Console.WriteLine($"\nYou have already done a swab test before when you last entered the country on {entryDate}.");
                        Console.WriteLine("Do you wish to continue with the swab test?\n[e] Exit\n[1] Continue with swab test\n");
                        Console.Write("Enter option: ");
                        string opt = Console.ReadLine();
                        if (opt.ToLower() == "e")
                        {
                            brk = true;//break main while loop
                            innerBrk = true;// skip the bottom codes for the swab test
                            break;//break inner while loop
                        }
                        else if(opt == "1")
                        {
                            Console.WriteLine("\nContinue with swab test\n");
                            break;
                        }
                        else
                        {
                            InvalidOpt();
                        }
                    }//end of inner while loop
                }//end of if person already exists 
                else// else the person is found and does not have any tavel entry records
                {
                    Console.WriteLine("\nYou do not have any travel entry records.\nContinuing with the swab test.\n");
                }
                //code for swab tests
                while (innerBrk == false)
                {
                    //information taken and altered from https://www.mountelizabeth.com.sg/healthplus/article/covid-19-tests
                    Console.WriteLine("{0,-35} | {1,-35} | {2,-35}", "Type", "Pros", "Cons");
                    Console.WriteLine("===================================================================================================================");
                    Console.WriteLine("{0,-35} | {1,-35} | {2,-35}", "Autonomic Response Testing (ART)", "Quick test, Inexpensive", "Decreased accuracy");
                    Console.WriteLine("{0,-35} | {1,-35} | {2,-35}", "Polymerase Chain Reaction (PCR)", "Accurate results", "Slow test and costly");
                    Console.WriteLine("===================================================================================================================\n");
                    Console.WriteLine("[e] Exit\n[1] ART\n[2] PCR");
                    Console.Write("Enter option: ");
                    string opt = Console.ReadLine();
                    double cost;
                    Random rnd = new Random();
                    int rand;
                    if (opt.ToLower() == "e")
                    {
                        brk = true;//exit
                        break;
                    }
                    else if (opt == "1")
                    {
                        Console.WriteLine("You have chosen Autonomic Response Testing\nResults takes 18-24 hrs\nCost: $20");
                        cost = 20;
                        rand = rnd.Next(18, 25);//random int from 18 to 24 hrs
                    }
                    else if (opt == "2")
                    {
                        Console.WriteLine("You have chosen Polymerase Chain Reaction\nResults takes 2-4 days\nCost: $200");
                        cost = 200;
                        rand = rnd.Next(48, 97);//random int from 48 to 96 hours
                    }
                    else
                    {
                        InvalidOpt();
                        continue;
                    }
                    
                    while (true)
                    {
                        Console.WriteLine($"\nConfirm Payment of ${cost}\n[e] Exit\n[1] Confirm payment");
                        Console.Write("Enter option: ");
                        string ans = Console.ReadLine();
                        if (ans.ToLower() == "e")
                        {
                            innerBrk = true;
                            brk = true;
                            break;
                        }
                        else if(ans == "1")
                        {
                            Console.WriteLine("\nPayment successful\n");
                            Console.WriteLine("\n......Swab Test in Progress...\n");
                            Thread.Sleep(1000);//let the program wait for one second
                            Console.WriteLine("\nSwab test successful, Waiting for results\n");
                            Thread.Sleep(1000);//let the program wait for one second
                            Console.WriteLine($"\nAfter {rand} hours....\n");//use the random hours generated to display to users, since the hours depend on the type of test
                            int infected = rnd.Next(1, 101);//generate one number from 1 to 100
                            //int infected = 1;
                            if(infected == 1)//assume infection rate at 1%
                            {
                                Console.WriteLine("You have been tested positive for Covid-19\nPlease visit a hospital asap and take necessary precautions to prevent the spread of the virus");
                                Console.WriteLine("\nReturning to main menu\n");
                                brk = true;
                                innerBrk = true;
                                break;
                            }
                            else//not infected
                            {
                                Console.WriteLine("You tested negative for Covid-19");
                                Console.WriteLine("\nReturning to main menu\n");
                                brk = true;
                                innerBrk = true;
                                break;
                            }
                        }
                        else
                        {
                            InvalidOpt();
                            continue;
                        }
                    }//end of payment while loop
                }//end of while loop

            }//end of main while loop
        }//end of PerformSwab
        //Advanced Features Contact Tracing Report
        static void ContactTracingReport(List<BusinessLocation>bList, List<Person>pList)
        {
            bool brk = false;
            while (brk == false)
            {
                BusinessLocation loca = null;
                DateTime given;
                Console.Write("Enter date for contact tracing report(yyyy/mm/dd) or enter 'e' to exit: ");//prompt to enter date
                string date = Console.ReadLine();
                if (date.ToLower() == "e")
                {
                    break;//exit
                }
                try
                {
                    given = Convert.ToDateTime(date);
                    if (given > DateTime.Now)
                    {
                        Console.WriteLine("\nEntered date cannot be in the future, Try Again.\n");//dont allow future timings
                        continue;
                    }
                }
                catch (Exception)
                {
                    InvalidOpt();
                    continue;
                }
                while (true) 
                {
                    Console.Write("Enter time '00:00' format (or enter 'e' to exit): ");
                    string time = Console.ReadLine();
                    if (time.ToLower() == "e")
                    {
                        brk = true;
                        break;
                    }
                    try
                    {
                        TimeSpan ts = TimeSpan.Parse(time);
                        if (given.Add(ts) > DateTime.Now)
                        {
                            Console.WriteLine("\nDate and time entered cannot be in the future. Try again.\n");//dont allow future times
                            continue;
                        }
                        given = given.Add(ts);//cannot execute this code at the top first, if this code was before the validation, the date will be overstated since the date was added for multiple times if user has error
                        break;
                    }
                    catch (Exception)
                    {
                        InvalidOpt();
                        continue;
                    }
                }//end of first inner while loop
                if(brk == true) { break; }
                while (true)
                {
                    Console.WriteLine("  {0,-25} {1,-25}", "Business Name", "Branch Code");
                    Console.WriteLine("--------------------------------------------------");
                    for(int i = 0; i < bList.Count; i++)
                    {
                        Console.WriteLine((i + 1) + ") {0,-25} {1,-25}", bList[i].BusinessName, bList[i].BranchCode);
                    }
                    Console.WriteLine("e) Exit");
                    Console.Write("\nEnter option (choose location for contact tracing report): ");
                    string opt = Console.ReadLine();
                    if (opt.ToLower() == "e")
                    {
                        brk = true;
                        break;
                    }
                    try
                    {
                        int ans = Convert.ToInt32(opt);
                        if (ans < 0 || ans > bList.Count)
                        {
                            throw new Exception();//out of range
                        }
                        loca = bList[ans - 1];//loca is a businesslocation obj, declared above
                        break;
                    }
                    catch (Exception)
                    {
                        InvalidOpt();
                        break;
                    }
                }//end of second inner while loop
                if(brk == true) { break; }

                
                List<Resident> checkedIn = new List<Resident>();
                foreach (Person p in pList)
                {
                    if (p is Resident)
                    {
                        Resident r = (Resident)p;//down cast
                        foreach (SafeEntry se in r.SafeEntryList)
                        {
                            if ((se.CheckIn < given && se.CheckOut > given) || (se.CheckIn < given && se.CheckOut == default(DateTime)))
                            //condition 1, check in before given and checkout after given. Condition 2, check in before given and not checked out yet (default datetime by c#)
                            {
                                checkedIn.Add(r);//add the resident to the list
                            }
                        }
                    }
                }//end of foreach loop to get all the residents checked in at that time.
                List<string> generate = new List<string>();
                if (checkedIn.Count == 0)
                {
                    Console.WriteLine($"\nNo one was checked in to {loca.BusinessName} at {given}");

                }
                else
                {
                    Console.WriteLine($"\n-----List of Persons checked in at {loca.BusinessName} on {given}\n");
                    Console.WriteLine("{0,-15} {1,-30} {2,-30} {3,-25} {4,-25}", "Name", "Address", "Business Name", "Check In Date&Time", "Check Out Date&Time");
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------");
                    foreach (Resident r in checkedIn)
                    {
                        foreach (SafeEntry se in r.SafeEntryList)
                        {
                            string outTime = Convert.ToString(se.CheckOut);
                            if (se.CheckOut == default(DateTime))
                            {
                                outTime = "Nil";//user have not checked out
                            }
                            Console.WriteLine("{0,-15} {1,-30} {2,-30} {3,-25} {4,-25}", r.Name, r.Address, se.Location.BusinessName, se.CheckIn, outTime);
                            generate.Add($"{r.Name},{ r.Address},{se.Location.BusinessName},{se.CheckIn},{outTime}");//add them to a string list for generation of csv file
                        }

                    }//end of foreach
                }
                bool append = true;//default it to true first
                if (File.Exists("ContactTracing.csv"))
                {
                    string[] data = File.ReadAllLines("ContactTracing.csv");
                    if (data.Length > 0)
                    {
                        bool inerbrk = false;
                        while (inerbrk == false)
                        {
                            Console.WriteLine("\nThere was data that were already in the file\n[e] Exit\n[1] Append data\n[2] Override data\n[3] Show data in file");
                            Console.Write("Enter your option: ");
                            string opt = Console.ReadLine();
                            if (opt.ToLower() == "e")
                            {
                                brk = true;
                                break;
                            }
                            else if(opt == "1")//append
                            {//no need assign true as already assigned before
                                Console.WriteLine("\nAppend to current data.\n");
                                break;
                            }
                            else if (opt == "2")
                            {
                                while (true)
                                {
                                    Console.WriteLine("\nOverriding will remove all current data\n[e] Exit\n[1] Confirm Override\n[2] Append instead");
                                    Console.Write("Enter you option: ");
                                    string ans = Console.ReadLine();
                                    if (ans.ToLower() == "e")
                                    {
                                        brk = true;
                                        inerbrk = true;
                                        break;
                                    }
                                    else if(ans == "1")
                                    {
                                        Console.WriteLine("\nConfirm Override.\n");
                                        append = false;
                                    }
                                    else if(ans == "2")
                                    {
                                        Console.WriteLine("\nYou have chosen to append data instead.");
                                        inerbrk = true;
                                        break;
                                    }
                                    else
                                    {
                                        InvalidOpt();
                                        continue;
                                    }
                                }//end of small while loop
                            }
                            else if (opt == "3")
                            {//display existing data
                                ShowContactTracing();
                            }
                            else
                            {
                                InvalidOpt();
                                continue;
                            }
                        }//end of inner while
                    }
                }
                if(brk == true) { break; }//prevent code from executing
                bool cont = true;//condition for continuing the code
                if (generate.Count == 0)
                {//if there is nothing to generate
                    while (true)
                    {
                        Console.Write("There are no check in records at the location and time given\n[e] Exit\n[1] Generate Empty report\nEnter option: ");
                        string op = Console.ReadLine();
                        if (op.ToLower() == "e")
                        {
                            cont = false;
                            break;
                        }
                        else if (op == "1")
                        {
                            Console.WriteLine("\nGenerate/Append empty report.\n");
                            break;
                            //no need assign as the default is already true
                        }
                        else
                        {
                            InvalidOpt();
                        }
                    }//end of while loop
                }
                if(cont == false) { break; }
                using (StreamWriter sw = new StreamWriter("ContactTracing.csv", append))
                {
                    foreach(string i in generate)
                    {
                        sw.WriteLine(i);
                    }
                }
                Console.WriteLine("\ncsv Report generated successfully.\n");
                break;
            }//end of main while loop
        }//end of contact tracing report


        //this method reads the file for contact tracing
        static void ShowContactTracing()
        {
            if (File.Exists("ContactTracing.csv"))
            {
                Console.WriteLine("{0,-15} {1,-30} {2,-30} {3,-25} {4,-25}", "Name", "Address", "Business Name", "Check In Date&Time", "Check Out Date&Time");
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
                string[] data = File.ReadAllLines("ContactTracing.csv");
                foreach(string i in data)
                {
                    string[] line = i.Split(',');
                    Console.WriteLine("{0,-15} {1,-30} {2,-30} {3,-25} {4,-25}", line[0], line[1], line[2], line[3], line[4]);
                }
            }
            else
            {
                Console.WriteLine("\nFile does not exist.\n");
            }
        }//end of show contact tracing


    }//end of program class
}
