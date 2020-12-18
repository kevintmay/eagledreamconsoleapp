using System;
using System.Configuration;
using System.Text;
using System.Data.SQLite;

namespace eagleDreamConsoleApp
{
    class Program
    {
        // stores the input menu option the user selecte=s
        public static string InputMenuOption { get; private set; }

        // stores the input menu option count
        public static int MenuOptionsCount { get; private set; }

        // stores the menu options that the user can select
        public static string[] MenuOptionsAll { get; private set; }

        // stores just the menu text that is visible to the user
        public static string[] MenuText { get; private set; }

        // stores the sql/view to call
        public static string[] MenuSQL { get; private set; }

        // stores whether the menu option is active or not (0/1)
        public static string[] MenuActive { get; private set; }

        // stores the column name/headers
        public static string[] ReportHeaders { get; private set; }

        // storess the report header count
        public static int ReportHeadersCount { get; private set; }

        //***********************************
        // BEGIN: Main
        //***********************************
        static void Main()
        {
    
            // initialize variables
            InitVariables();

            // first thing to do is load the menu to present the user(s)
            CreateMenu();

        } // Main





        //***********************************
        // BEGIN: InitVariables
        //***********************************
        public static void InitVariables()
        {

            // retreive the number of app settings within the config file that start with Option
            MenuOptionsCount = ConfigurationManager.AppSettings.Count;

            // declare the arrays here
            MenuOptionsAll = new string[MenuOptionsCount];
            MenuText = new string[MenuOptionsCount];
            MenuSQL = new string[MenuOptionsCount];
            MenuActive = new string[MenuOptionsCount];

            // loop through and set the menu options here
            for (int k = 0; k < MenuOptionsCount; k++)
            {
                string menuString = ConfigurationManager.AppSettings["Option" + (k+1).ToString()];
                string[] values = menuString.Split("|");

                // add the values
                MenuOptionsAll[k] = values[0];
                MenuText[k] = values[1];
                MenuSQL[k] = values[2];
                MenuActive[k] = values[3];            
            } // end for

        } // InitVariables





        //***********************************
        // BEGIN: CreateMenu
        //*********************************** 
        public static void CreateMenu()
        {

            // since we will need to present the user back to the menu often... it's best to put this all in a method
            Console.WriteLine("*****************************");
            Console.WriteLine("****** EagleDream Menu ******");
            Console.WriteLine("*****************************");
            Console.WriteLine("");

            // loop through the settings and populate the menu
            for (int i = 0; i < MenuOptionsCount; i++)
            {
                // only want menu items that are active to print to console. This can be changed within the app.config file
                // if the menu option is active
                if (int.Parse(MenuActive[i]) == 1)
                {
                       // print the selection to screen to present the user
                    Console.WriteLine(MenuOptionsAll[i] + " - " + MenuText[i]);
                } // endif
                
            } // for

            // now that the menu is created, go ahead and ask them what they would like to do
            GatherUserInput();

        } // CreateMenu





        //***********************************
        // BEGIN: GatherUserInput
        //*********************************** 
        public static void GatherUserInput()
        {
            // ask user for their selection
            Console.WriteLine("Please input a selection...");

            // grab the user's selction and save it
            InputMenuOption = Console.ReadLine();

            // set inputMenuOption to an integer
            int inputMenuOptionInt;

            try
            {

                // try to parse the user's selection into an int
                inputMenuOptionInt = int.Parse(InputMenuOption);

            }
            catch
            {
                // EE
                if (InputMenuOption.ToUpper() == "UUDDLRLRBA")
                {
                    // Jenny
                    inputMenuOptionInt = 8675309;

                } else {
                    // in case the user input something other than a number,
                    inputMenuOptionInt = 0;
                } // endif EE
                 
            } // try

            if (inputMenuOptionInt == 8675309)
            {
                // :)
                Console.WriteLine("You found me! Well done!\n- Kevin May :)");
            } else {
                // get the index of the user's selection to then check if their selection is in fact active
                // set index = -1, meaning that the user's seletion is not a valid option
                int index = -1;

                // loop through array and check if their selection is valid
                for (int i = 0; i < MenuOptionsAll.Length; i++)
                {
                    // if their selection is found, then grab the index so we can check if it's active
                    if (MenuOptionsAll[i].Equals(InputMenuOption))
                    {
                        // save the index
                        index = i;
                        break;

                    } // endif

                } // for

                // if the index is -1 then we didn't find a match
                if ((index != -1) && (index < MenuActive.Length) && (MenuActive[index] == "1"))
                {

                    // process the selection based on what the user selected
                    ProcessSelection(InputMenuOption);

                }
                else
                {

                    Console.WriteLine("Invalid selection. Please try again.");

                    GatherUserInput();

                } //endif
            } // endif     
        } // GatherUserInput





        //***********************************
        // BEGIN: ProcessSelection
        //***********************************
        private static void ProcessSelection(string inputMenuOption)
        {
            // create the connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();

            // read the data
            GetData(sqlite_conn, MenuSQL[(int.Parse(inputMenuOption) - 1)], MenuText[(int.Parse(inputMenuOption) - 1)]);

        } // ProcessSelection





        //***********************************
        // BEGIN: CreateConnection
        //***********************************
        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["EagleDreamConnection"].ConnectionString);

            // open the connection:
            try
            {
                sqlite_conn.Open();                
            } catch (Exception ex) {
                Console.WriteLine("Connection Error!\n\n" + ex.ToString());
            } // end try

            // return the connection
            return sqlite_conn;

        } // CreateConnection




        //***********************************
        // BEGIN: GetData
        //***********************************
        public static void GetData(SQLiteConnection conn, string sql, string menuText)
        {

            string sqlGetData = "SELECT * FROM " + sql;

            // First, lets get the header/column count for the results
            GetHeaders(conn, sql);

            // new string builder for header
            StringBuilder headers = new StringBuilder();

            // insert a blank line to space out data
            Console.WriteLine("");
            Console.WriteLine("* " + menuText + " *");

            // build the headers
            for (int km = 0; km < ReportHeadersCount; km++)
            {
                // if there are more than one headers/columns... print a delimitter
                if (km > 0)
                {
                    headers.Append(",");
                } //endif

                headers.Append(ReportHeaders[km]);
            } //end for

            // print the headers
            Console.WriteLine(headers);

            // get connection to db
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            // sql query to call the view
            sqlite_cmd.CommandText = sqlGetData;

            // execute the query
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            // loop through the results and build out the string for each line
            while (sqlite_datareader.Read())
            {
                // new stringbuilder
                StringBuilder sb = new StringBuilder();

                // loop through and add to the stringbuilder
                for (int x = 0; x < sqlite_datareader.FieldCount; x++)
                {

                    // if we are on the second column/header, add a delimiter
                    if (x > 0)
                    {
                        sb.Append(",");
                    } //endif

                    // append the value to the stringbuilder
                    sb.Append(sqlite_datareader.GetValue(sqlite_datareader.GetOrdinal(ReportHeaders[x])));
  
                } // end fo

                // write the line
                Console.WriteLine(sb);

            } // end while

            // close the connection
            conn.Close();

            Console.WriteLine("");

            // go back and create menu
            CreateMenu();

        } // end GetData





        //***********************************
        // BEGIN: GetHeaders
        //***********************************
        public static void GetHeaders(SQLiteConnection conn, string sql)
        {

            // declare sql here
            string sqlCount = "SELECT count(name) FROM PRAGMA_TABLE_INFO('" + sql + "')";
            string sqlGetNames = "SELECT name FROM PRAGMA_TABLE_INFO('" + sql + "') order by cid asc";

            // set y = 0 for the index for reportHeaders
            int y = 0;

            // get connection to db
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            // get the count of the columns query
            sqlite_cmd.CommandText = sqlCount;

            // execute the query
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            // read the results
            sqlite_datareader.Read();
            
            // set the variable of the header count
            ReportHeadersCount = sqlite_datareader.GetInt32(0);

            // create a new command to call
            sqlite_cmd = conn.CreateCommand();

            //  new query to get the column names
            sqlite_cmd.CommandText = sqlGetNames;

            // execute the query
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            // new variable to store the headers/columns so that we can refernece them
            ReportHeaders = new string[ReportHeadersCount];

            // loop through the results and add them to reportHeaders
            while (sqlite_datareader.Read())
            {
                // new sb to store headers
                StringBuilder sb = new StringBuilder();

                // since we are only getting back one column, this can be 0
                ReportHeaders[y] = sqlite_datareader.GetString(0);

                // increment y
                y++;

            } // end while
            
        } // GetHeaders

    }
}
