using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Mocks; // Assuming MockDirectoryEntry is in this namespace
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Mocks // Or your preferred testing namespace
{
    public class MockActiveDirectoryContext : IActiveDirectoryContext
    {
        private readonly List<IDirectoryEntryAdapter> _entries = new List<IDirectoryEntryAdapter>();
        private readonly Dictionary<string, IDirectoryEntry> _directoryEntries = new Dictionary<string, IDirectoryEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly MockDirectoryEntry _rootDg;
        private readonly MockDirectoryEntry _appRootDg;
        private readonly MockDirectoryEntry _deletedObjectsDg;

        public IAppDatabaseFactory Factory { get; set; } // Mock or set as needed
        public bool PortOpen { get; set; } = true;
        public DirectoryConnectionStatus Status { get; set; } = DirectoryConnectionStatus.OK;
        public int FailedConnectionAttempts { get; set; }
        public IDirectoryEntry? AppRootDirectoryEntry => _appRootDg; // Simplified

        public IADOUSearcher OUs { get; set; } // Needs a mock implementation
        public IADGroupSearcher Groups { get; set; } // Needs a mock implementation
        public IADUserSearcher Users { get; set; } // Needs a mock implementation
        public IADContactSearcher Contacts { get; set; } // Needs a mock implementation
        public IADPrinterSearcher Printers { get; set; } // Needs a mock implementation
        public IADComputerSearcher Computers { get; set; } // Needs a mock implementation
        public IADBitLockerSearcher BitLocker { get; set; } // Needs a mock implementation

        public AppDelegate<DirectoryConnectionStatus>? OnStatusChanged { get; set; }
        public ADSettings? ConnectionSettings { get; set; }
        public ActiveDirectoryUserState? CurrentUser { get; set; }
        public WindowsImpersonation Impersonation { get; private set; } // May need a mock if used
        public List<DomainController> DomainControllers { get; } = new List<DomainController>();
        public DomainControllerEventLogReader EventLogReader { get; private set; } // May need a mock
        public Exception? ConnectionException { get; set; }

        public MockActiveDirectoryContext(IAppDatabaseFactory factory = null, ADSettings settings = null)
        {
            Factory = factory; // Assign if provided, otherwise needs mocking/setting
            ConnectionSettings = settings ?? new ADSettings // Default settings if none provided
            {
                FQDN = "example.com",
                ApplicationBaseDN = "OU=AppRoot,DC=example,DC=com",
                ServerAddress = "mockserver",
                ServerPort = 389,
                Username = "mockadmin",
                Password = "encryptedPassword"
                // Password would be handled by a mock encryption service if needed
            };

            _rootDg = new MockDirectoryEntry("DC=example,DC=com", $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}/DC=example,DC=com") { SchemaClassName = "domainDNS" };
            _appRootDg = new MockDirectoryEntry("OU=AppRoot", $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}/{ConnectionSettings.ApplicationBaseDN}") { SchemaClassName = "organizationalUnit", Parent = _rootDg };
            _deletedObjectsDg = new MockDirectoryEntry("CN=Deleted Objects", $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}/CN=Deleted Objects,DC=example,DC=com") { SchemaClassName = "container", Parent = _rootDg };

            _directoryEntries[_rootDg.Path] = _rootDg;
            _directoryEntries[_appRootDg.Path] = _appRootDg;
            _directoryEntries[_deletedObjectsDg.Path] = _deletedObjectsDg;

            // Initialize mock searchers
            Users = new MockADUserSearcher(this);
            Groups = new MockADGroupSearcher(this);
            OUs = new MockADOUSearcher(this);
            Computers = new MockADComputerSearcher(this, null); // Pass a mock WmiFactory if needed for specific tests
            Contacts = new MockADContactSearcher(this);
            Printers = new MockADPrinterSearcher(this);
            BitLocker = new MockADBitLockerSearcher(this);


            // Mock impersonation and event log reader if complex interactions are needed
            // For basic scenarios, they might not need full mock implementations.
            // Impersonation = new MockWindowsImpersonation();
            // EventLogReader = new MockDomainControllerEventLogReader(this);


            PopulateWithRealisticData();
        }
        private void PopulateWithRealisticData(int approximateCount = 400)
        {
            if (ConnectionSettings == null)
            {
                Console.WriteLine("Cannot populate data: ConnectionSettings is null.");
                return;
            }

            var random = new Random();
            string baseDomainPath = $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}";
            string domainDN = ConnectionSettings.FQDN.FqdnToDN(); // Assumes FQDN is set like "example.com"

            // --- Sample Data ---
            string[] firstNames = {
    // Original names
    "Alice", "Bob", "Charlie", "David", "Eve", "Fiona", "George", "Hannah", "Ian", "Julia", "Kevin", "Laura",
    // Additional names
    "Aaron", "Abigail", "Adam", "Adrian", "Adriana", "Alan", "Albert", "Alex", "Alexa", "Alexander",
    "Alexandra", "Alexis", "Alicia", "Alvin", "Alyssa", "Amanda", "Amber", "Amelia", "Amy", "Andrea",
    "Andrew", "Angela", "Angelina", "Ann", "Anna", "Anthony", "Antonio", "April", "Ariana", "Arthur",
    "Ashley", "Audrey", "Austin", "Autumn", "Ava", "Avery", "Bella", "Benjamin", "Bernadette", "Bethany",
    "Betty", "Beverly", "Billy", "Blake", "Bradley", "Brandon", "Brandy", "Brenda", "Brent", "Brett",
    "Brian", "Brianna", "Brittany", "Brooke", "Bruce", "Bryan", "Caleb", "Cameron", "Camila", "Carl",
    "Carla", "Carlos", "Carmen", "Carol", "Caroline", "Carrie", "Carter", "Casey", "Cassandra", "Catherine",
    "Cathy", "Cecilia", "Chad", "Charlene", "Chase", "Chelsea", "Cheryl", "Chloe", "Chris", "Christian",
    "Christina", "Christine", "Christopher", "Cindy", "Claire", "Clara", "Clarence", "Clayton", "Cody", "Colin",
    "Colleen", "Connor", "Constance", "Corey", "Courtney", "Craig", "Crystal", "Curtis", "Cynthia", "Daisy",
    "Dakota", "Dale", "Damian", "Dana", "Daniel", "Danielle", "Danny", "Darrell", "Darren", "Dave",
    "Dawn", "Dean", "Deanna", "Deborah", "Debra", "Denise", "Dennis", "Derek", "Desiree", "Destiny",
    "Devin", "Diana", "Diane", "Dominic", "Don", "Donald", "Donna", "Doris", "Dorothy", "Douglas",
    "Drew", "Duane", "Dustin", "Dwayne", "Dylan", "Ebony", "Eddie", "Edgar", "Edith", "Edward",
    "Edwin", "Eileen", "Elaine", "Eleanor", "Elena", "Eli", "Elijah", "Eliza", "Elizabeth", "Ella",
    "Ellen", "Ellie", "Elliot", "Emily", "Emma", "Enrique", "Eric", "Erica", "Erika", "Erin",
    "Ernest", "Esmeralda", "Esther", "Ethan", "Eugene", "Eva", "Evan", "Evelyn", "Faith", "Felicia",
    "Felix", "Fernando", "Flora", "Florence", "Frances", "Francis", "Francisco", "Frank", "Fred", "Gabriel",
    "Gabriella", "Gail", "Garrett", "Gary", "Gavin", "Gemma", "Gene", "Geoffrey", "Gerald", "Geraldine",
    "Gilbert", "Gillian", "Gina", "Ginger", "Gladys", "Glen", "Glenn", "Gloria", "Gordon", "Grace",
    "Graham", "Grant", "Greg", "Gregory", "Gretchen", "Guadalupe", "Guy", "Hailey", "Harold", "Harry",
    "Harvey", "Hayden", "Hazel", "Heather", "Hector", "Heidi", "Helen", "Henry", "Herbert", "Holly",
    "Hope", "Howard", "Hugh", "Hunter", "Ingrid", "Irene", "Isaac", "Isabel", "Isabella", "Isaiah",
    "Ivan", "Ivy", "Jack", "Jackie", "Jackson", "Jacob", "Jacqueline", "Jade", "Jaime", "Jake",
    "James", "Jamie", "Jane", "Janet", "Janice", "Jared", "Jasmin", "Jason", "Javier", "Jay",
    "Jean", "Jeanette", "Jeff", "Jeffery", "Jeffrey", "Jenna", "Jennifer", "Jeremiah", "Jeremy", "Jerome",
    "Jerry", "Jesse", "Jessica", "Jesus", "Jill", "Jim", "Jo", "Joan", "Joann", "Joanna",
    "Joanne", "Jocelyn", "Jodi", "Joe", "Joel", "Joey", "John", "Johnny", "Jolene", "Jon",
    "Jonathan", "Jordan", "Jorge", "Jose", "Joseph", "Josephine", "Joshua", "Josiah", "Joy", "Joyce",
    "Juan", "Juana", "Judith", "Judy", "Julian", "Juliana", "Julie", "Julio", "Justin", "Kaden",
    "Kaitlyn", "Karen", "Karina", "Karl", "Karla", "Kate", "Katherine", "Kathleen", "Kathryn", "Kathy",
    "Katie", "Katrina", "Kay", "Kayla", "Keith", "Kelly", "Kelsey", "Ken", "Kendra", "Kenneth",
    "Kenny", "Kent", "Kerry", "Kim", "Kimberly", "Kirk", "Kristen", "Kristin", "Kristina", "Kristine",
    "Kristy", "Kyle", "Kylie", "Lacey", "Landon", "Larry", "Latoya", "Lauren", "Lawrence", "Leah",
    "Lee", "Leila", "Lena", "Leo", "Leon", "Leonard", "Leonardo", "Leroy", "Leslie", "Levi",
    "Lewis", "Liam", "Lillian", "Lily", "Linda", "Lindsay", "Lindsey", "Lisa", "Lloyd", "Logan",
    "Lois", "Lola", "Lorraine", "Louis", "Louise", "Lucas", "Lucia", "Lucille", "Lucy", "Luis",
    "Luke", "Luz", "Lydia", "Lynn", "Mabel", "Mackenzie", "Madeline", "Madison", "Makayla", "Malcolm",
    "Mallory", "Mandy", "Manuel", "Marc", "Marcia", "Marco", "Marcus", "Margaret", "Maria", "Marian",
    "Marie", "Marilyn", "Mario", "Marion", "Marisa", "Marjorie", "Mark", "Marlene", "Marsha", "Martha",
    "Martin", "Martina", "Marvin", "Mary", "Mason", "Mathew", "Matthew", "Maureen", "Max", "Maxine",
    "Maxwell", "Maya", "Megan", "Melanie", "Melinda", "Melissa", "Melody", "Melvin", "Mercedes", "Meredith",
    "Mia", "Micah", "Michael", "Micheal", "Michele", "Michelle", "Miguel", "Mike", "Mildred", "Miles",
    "Milton", "Mindy", "Miranda", "Miriam", "Misty", "Mitchell", "Molly", "Monica", "Monique", "Morgan",
    "Morris", "Myron", "Nancy", "Naomi", "Natalia", "Natalie", "Natasha", "Nathan", "Nathaniel", "Neil",
    "Nelson", "Nevaeh", "Nicholas", "Nick", "Nicole", "Nina", "Noah", "Noel", "Nolan", "Nora",
    "Norma", "Norman", "Olivia", "Omar", "Oscar", "Owen", "Paige", "Pam", "Pamela", "Paola",
    "Patricia", "Patrick", "Patty", "Paul", "Paula", "Pauline", "Pedro", "Peggy", "Penny", "Percy",
    "Perry", "Peter", "Peyton", "Philip", "Phillip", "Phyllis", "Priscilla", "Quentin", "Quinn", "Rachel",
    "Rafael", "Ralph", "Ramon", "Ramona", "Randall", "Randolph", "Randy", "Raul", "Ray", "Raymond",
    "Rebecca", "Regina", "Reginald", "Renee", "Rex", "Rhonda", "Ricardo", "Richard", "Rick", "Ricky",
    "Riley", "Rita", "Rob", "Robert", "Roberta", "Roberto", "Robin", "Rochelle", "Rocky", "Rodney",
    "Roger", "Roland", "Roman", "Ron", "Ronald", "Ronnie", "Rory", "Rosa", "Rose", "Rosemary",
    "Ross", "Roxanne", "Roy", "Ruben", "Ruby", "Rudolph", "Russell", "Ruth", "Ryan", "Sabrina",
    "Sadie", "Sally", "Salvador", "Sam", "Samantha", "Samuel", "Sandra", "Sandy", "Santiago", "Sara",
    "Sarah", "Saul", "Savannah", "Scott", "Sean", "Sebastian", "Selena", "Serena", "Sergio", "Seth",
    "Shaun", "Shawn", "Shawna", "Sheila", "Shelby", "Shelia", "Shelley", "Sheri", "Sherman", "Sherri",
    "Sherry", "Shirley", "Sidney", "Silvia", "Simon", "Skylar", "Sofia", "Sonia", "Sophia", "Sophie",
    "Spencer", "Stacey", "Stacy", "Stanley", "Stella", "Stephanie", "Stephen", "Steve", "Steven", "Stuart",
    "Sue", "Summer", "Susan", "Suzanne", "Sydney", "Sylvia", "Tabitha", "Tamara", "Tami", "Tammy",
    "Tanya", "Tara", "Taylor", "Ted", "Teresa", "Terrence", "Terri", "Terry", "Tessa", "Theodore",
    "Theresa", "Thomas", "Tia", "Tiffany", "Tim", "Timothy", "Tina", "Toby", "Todd", "Tom",
    "Tomas", "Tommy", "Toni", "Tony", "Tonya", "Tracey", "Traci", "Tracy", "Travis", "Trent",
    "Trevor", "Tricia", "Trinity", "Trisha", "Troy", "Tyler", "Tyrone", "Valerie", "Vanessa", "Vernon",
    "Veronica", "Vicki", "Vickie", "Victor", "Victoria", "Vincent", "Viola", "Violet", "Virgil", "Virginia",
    "Vivian", "Walter", "Wanda", "Warren", "Wayne", "Wendy", "Wesley", "Whitney", "Wilbert", "Wilbur",
    "Willard", "William", "Willie", "Wilma", "Wyatt", "Xavier", "Yadira", "Yasmin", "Yolanda", "Yvette",
    "Yvonne", "Zachary", "Zoe"
};

            string[] lastNames = {
    // Original names
    "Smith", "Jones", "Williams", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas",
    // Additional names
    "Adams", "Alexander", "Allen", "Alvarez", "Armstrong", "Arnold", "Austin", "Bailey", "Baker", "Baldwin",
    "Ball", "Banks", "Barber", "Barker", "Barnes", "Barnett", "Barrett", "Barton", "Bass", "Bates",
    "Beck", "Becker", "Bell", "Bennett", "Berry", "Bishop", "Black", "Blair", "Blake", "Boone",
    "Bowen", "Bowers", "Bowman", "Boyd", "Bradley", "Brady", "Brewer", "Bridges", "Briggs", "Brooks",
    "Bryant", "Burke", "Burns", "Burton", "Bush", "Butler", "Byrd", "Cain", "Calderon", "Caldwell",
    "Campbell", "Campos", "Cannon", "Carlson", "Carpenter", "Carr", "Carroll", "Carson", "Carter", "Case",
    "Castillo", "Castro", "Chambers", "Chandler", "Chapman", "Chavez", "Chen", "Cheng", "Cherry", "Christensen",
    "Chung", "Clark", "Clarke", "Clay", "Clayton", "Cobb", "Cohen", "Cole", "Coleman", "Collins",
    "Colon", "Combs", "Compton", "Conner", "Contreras", "Cook", "Cooper", "Copeland", "Cortez", "Cox",
    "Craig", "Crawford", "Cross", "Cruz", "Cummings", "Cunningham", "Curry", "Curtis", "Daniel", "Daniels",
    "Davidson", "Dawson", "Day", "Dean", "Delgado", "Dennis", "Diaz", "Dickson", "Dixon", "Dominguez",
    "Donovan", "Douglas", "Doyle", "Drake", "Dudley", "Duffy", "Duke", "Duncan", "Dunn", "Durham",
    "Dyer", "Eaton", "Edwards", "Elliott", "Ellis", "Erickson", "Espinoza", "Estrada", "Evans", "Farmer",
    "Farrell", "Ferguson", "Fernandez", "Fields", "Figueroa", "Fisher", "Fitzgerald", "Fleming", "Fletcher", "Flores",
    "Flowers", "Floyd", "Ford", "Foster", "Fowler", "Fox", "Francis", "Franco", "Frank", "Franklin",
    "Frazier", "Freeman", "French", "Frost", "Fuentes", "Fuller", "Fung", "Gaines", "Gallagher", "Gallegos",
    "Garcia", "Gardner", "Garner", "Garrett", "Garza", "Gates", "Gay", "George", "Gibbs", "Gibson",
    "Gilbert", "Giles", "Gill", "Gillespie", "Glover", "Gomez", "Gonzales", "Gonzalez", "Goodman", "Goodwin",
    "Gordon", "Graham", "Grant", "Graves", "Gray", "Green", "Greene", "Greer", "Gregory", "Griffin",
    "Griffith", "Grimes", "Gross", "Guerrero", "Gutierrez", "Guzman", "Hahn", "Hale", "Hall", "Hamilton",
    "Hammond", "Hampton", "Hancock", "Hansen", "Hanson", "Hardy", "Harmon", "Harper", "Harrell", "Harrington",
    "Harris", "Harrison", "Hart", "Harvey", "Hawkins", "Hayes", "Haynes", "Henderson", "Henry", "Hensley",
    "Hernandez", "Herrera", "Hess", "Hickman", "Hicks", "Higgins", "Hill", "Hinton", "Ho", "Hobbs",
    "Hodge", "Hodges", "Hoffman", "Hogan", "Holland", "Holloway", "Holmes", "Holt", "Hood", "Hooper",
    "Hoover", "Hopkins", "Horn", "Horne", "Horton", "Houston", "Howard", "Howell", "Huang", "Hubbard",
    "Hudson", "Huff", "Huffman", "Hughes", "Humphrey", "Hunt", "Hunter", "Hurst", "Hutchinson", "Huynh",
    "Ingram", "Jackson", "Jacobs", "James", "Jaramillo", "Jarvis", "Jenkins", "Jennings", "Jensen", "Jimenez",
    "Johns", "Johnson", "Johnston", "Jordan", "Joseph", "Juarez", "Kaiser", "Kane", "Katz", "Kaufman",
    "Keen", "Keller", "Kelley", "Kelly", "Kemp", "Kennedy", "Kent", "Kerr", "Key", "Khan",
    "Kidd", "Kim", "King", "Kirby", "Kirk", "Klein", "Knight", "Knox", "Koch", "Kramer",
    "Krueger", "Lam", "Lamb", "Lambert", "Lane", "Lang", "Larsen", "Larson", "Lawrence", "Lawson",
    "Le", "Leach", "Lee", "Leon", "Leonard", "Lester", "Levine", "Levy", "Lewis", "Li",
    "Lin", "Lindsey", "Little", "Liu", "Lloyd", "Logan", "Long", "Lopez", "Lowe", "Lowery",
    "Lucas", "Luna", "Lynch", "Lyons", "Ma", "Macdonald", "Mack", "Madden", "Maddox", "Mahoney",
    "Maldonado", "Malone", "Mann", "Manning", "Marks", "Marsh", "Marshall", "Martin", "Martinez", "Mason",
    "Massey", "Mathews", "Mathis", "Matthews", "Maxwell", "May", "Mayer", "Maynard", "Mayo", "Mcbride",
    "Mccall", "Mccarthy", "Mccarty", "Mcclain", "Mcclure", "Mccormick", "Mccoy", "Mccullough", "Mcdaniel", "Mcdonald",
    "Mcdowell", "Mcgee", "Mcguire", "Mcintosh", "Mcintyre", "Mckay", "Mckee", "Mckenzie", "Mckinney", "Mclaughlin",
    "Mclean", "Mcmahon", "Mcmillan", "Mcneil", "Mcpherson", "Meadows", "Medina", "Mejia", "Melton", "Mendez",
    "Mendoza", "Mercado", "Mercer", "Merritt", "Meyer", "Meyers", "Michael", "Middleton", "Miles", "Mills",
    "Mitchell", "Molina", "Monroe", "Montgomery", "Montoya", "Moody", "Moon", "Mooney", "Morales", "Moran",
    "Moreno", "Morgan", "Morris", "Morrison", "Morse", "Morton", "Moses", "Moss", "Mueller", "Mullen",
    "Mullins", "Munoz", "Murphy", "Murray", "Myers", "Nash", "Navarro", "Neal", "Nelson", "Newman",
    "Newton", "Nguyen", "Nichols", "Nicholson", "Nielsen", "Nixon", "Noble", "Nolan", "Norman", "Norris",
    "Norton", "Nunez", "O'brien", "O'connell", "O'connor", "O'donnell", "O'neal", "O'neill", "Oliver", "Olsen",
    "Olson", "Ortega", "Ortiz", "Osborne", "Owen", "Owens", "Pace", "Padilla", "Page", "Palmer",
    "Park", "Parker", "Parks", "Parrish", "Parsons", "Pate", "Patel", "Patrick", "Patterson", "Patton",
    "Paul", "Payne", "Pearson", "Peck", "Pena", "Pennington", "Perez", "Perkins", "Perry", "Peters",
    "Petersen", "Peterson", "Petty", "Pham", "Phelps", "Phillips", "Pierce", "Pittman", "Pitts", "Pollard",
    "Poole", "Pope", "Porter", "Potter", "Potts", "Powell", "Powers", "Pratt", "Preston", "Price",
    "Prince", "Proctor", "Pruitt", "Pugh", "Quinn", "Ramirez", "Ramos", "Ramsey", "Randall", "Randolph",
    "Rangel", "Rasmussen", "Ray", "Raymond", "Reed", "Reese", "Reeves", "Reid", "Reilly", "Reyes",
    "Reynolds", "Rhodes", "Rice", "Rich", "Richard", "Richards", "Richardson", "Richmond", "Riddle", "Riggs",
    "Riley", "Rios", "Rivas", "Rivera", "Roach", "Robbins", "Roberson", "Roberts", "Robertson", "Robinson",
    "Robles", "Rocha", "Rodgers", "Rodriguez", "Rodriquez", "Rogers", "Rojas", "Rollins", "Roman", "Romero",
    "Rose", "Ross", "Roth", "Rowe", "Rowland", "Roy", "Ruiz", "Rush", "Russell", "Russo",
    "Ryan", "Salas", "Salazar", "Salinas", "Sampson", "Sanchez", "Sanders", "Sandoval", "Sanford", "Santana",
    "Santiago", "Santos", "Sargent", "Saunders", "Savage", "Sawyer", "Schaefer", "Schmidt", "Schmitt", "Schneider",
    "Schroeder", "Schultz", "Schwartz", "Scott", "Sellers", "Serrano", "Sexton", "Shaffer", "Shannon", "Sharp",
    "Sharpe", "Shaw", "Shelton", "Shepard", "Shepherd", "Sherman", "Shields", "Short", "Silva", "Simmons",
    "Simon", "Simpson", "Sims", "Singleton", "Skinner", "Slater", "Sloan", "Small", "Snow", "Snyder",
    "Solis", "Solomon", "Sosa", "Soto", "Sparks", "Spears", "Spence", "Spencer", "Stafford", "Stanley",
    "Stanton", "Stark", "Steele", "Stephens", "Stephenson", "Stevens", "Stevenson", "Stewart", "Stokes", "Stone",
    "Stout", "Strickland", "Strong", "Stuart", "Suarez", "Sullivan", "Summers", "Sutton", "Swanson", "Sweeney",
    "Tanner", "Tate", "Terrell", "Terry", "Tran", "Travis", "Trevino", "Trujillo", "Tucker", "Turner",
    "Tyler", "Tyson", "Underwood", "Valdez", "Valencia", "Valenzuela", "Vance", "Vang", "Vargas", "Vasquez",
    "Vaughan", "Vaughn", "Vazquez", "Vega", "Velez", "Villa", "Villanueva", "Villarreal", "Vincent", "Wagner",
    "Walker", "Wall", "Wallace", "Waller", "Walls", "Walsh", "Walter", "Walters", "Walton", "Wang",
    "Ward", "Ware", "Warner", "Warren", "Washington", "Waters", "Watkins", "Watson", "Watts", "Weaver",
    "Webb", "Weber", "Webster", "Weeks", "Weiss", "Welch", "Wells", "West", "Wheeler", "Whitaker",
    "White", "Whitehead", "Whitfield", "Whitley", "Wiggins", "Wilcox", "Wilder", "Wiley", "Wilkerson", "Wilkins",
    "Wilkinson", "Williamson", "Willis", "Winters", "Wise", "Wolf", "Wolfe", "Wong", "Wood", "Woodard",
    "Woods", "Woodward", "Workman", "Wright", "Wu", "Wyatt", "Yang", "Yates", "Yi", "Yoder",
    "York", "Young", "Yu", "Zamora", "Zavala", "Zhang", "Zimmerman", "Zuniga"
};
            string[] departments = { "Sales", "Marketing", "Engineering", "HR", "Finance", "Support", "Research", "IT" };
            string[] ouNames = { "Employees", "Departments", "Servers", "Workstations", "Printers", "ServiceAccounts", "Groups" };
            string[] groupPrefixes = { "DL_", "SEC_", "APP_" };
            string[] groupPurposes = { "Users", "Admins", "ReadOnly", "FullAccess", "ProjectX", "DepartmentY" };
            string[] computerPrefixes = { "WS", "SRV", "LAPTOP", "TESTPC" };
            string[] osVersions = { "Windows Server 2022", "Windows Server 2019", "Windows 11 Pro", "Windows 10 Enterprise", "Ubuntu Linux" };
            string[] printerModels = { "HP LaserJet Pro M404dn", "Brother HL-L2390DW", "Canon imageCLASS MF743Cdw", "Epson EcoTank ET-2720" };
            string[] locations = { "Main Office", "Branch A", "Floor 1", "Floor 2", "Remote" };


            // --- Create Root OUs ---
            var rootOUs = new List<MockDirectoryEntry>();
            foreach (var ouName in ouNames)
            {
                if (_entries.Any(e => e is IADOrganizationalUnit ouAd && ouAd.Name == ouName && e.DN.EndsWith(domainDN))) continue; // Skip if already exists

                var ouDN = $"OU={ouName},{domainDN}";
                var ouPath = $"{baseDomainPath}/{ouDN}";
                var ouEntry = new MockDirectoryEntry(ouName, ouPath)
                {
                    SchemaClassName = "organizationalUnit",
                    Parent = _rootDg // Assuming _rootDg is your domain root MockDirectoryEntry
                };
                ouEntry.SetPropertyValue("objectClass", new List<object> { "top", "organizationalUnit" });
                ouEntry.SetPropertyValue("ou", ouName); // For OUs, 'ou' attribute is often same as name
                ouEntry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, ouDN);
                ouEntry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(30, 365)));


                AddMockDirectoryEntry(ouEntry, ActiveDirectoryObjectType.OU);
                rootOUs.Add(ouEntry);
            }
            if (!rootOUs.Any())
            {
                // If all OUs existed, grab them for parenting child objects
                rootOUs.AddRange(_entries.OfType<IADOrganizationalUnit>()
                                         .Where(ou => ou.DN.Split(',').Length == domainDN.Split(',').Length + 1 && ou.DN.EndsWith(domainDN))
                                         .Select(ou => ou.DirectoryEntry as MockDirectoryEntry).Where(mde => mde != null));
            }
            if (!rootOUs.Any())
            {
                Console.WriteLine("Could not find or create root OUs to populate data into.");
                // Fallback to using the AppRoot if no other OUs are available
                if (_appRootDg != null) rootOUs.Add(_appRootDg);
                else return; // Cannot proceed
            }


            // --- Create Nested OUs (Optional, for more depth) ---
            var nestedOUs = new List<MockDirectoryEntry>(rootOUs);
            int departmentOuCount = 0;
            var departmentsOu = rootOUs.FirstOrDefault(ou => ou.Name == "Departments");
            if (departmentsOu != null)
            {
                foreach (var deptName in departments)
                {
                    if (departmentOuCount >= 3) break; // Limit nested OUs for simplicity

                    var deptOuDN = $"OU={deptName},{departmentsOu.GetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName)}";
                    var deptOuPath = $"{baseDomainPath}/{deptOuDN}";
                    if (_entries.Any(e => e.DN == deptOuDN)) continue;

                    var deptOuEntry = new MockDirectoryEntry(deptName, deptOuPath)
                    {
                        SchemaClassName = "organizationalUnit",
                        Parent = departmentsOu
                    };
                    deptOuEntry.SetPropertyValue("objectClass", new List<object> { "top", "organizationalUnit" });
                    deptOuEntry.SetPropertyValue("ou", deptName);
                    deptOuEntry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, deptOuDN);
                    deptOuEntry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(0, 90)));

                    AddMockDirectoryEntry(deptOuEntry, ActiveDirectoryObjectType.OU);
                    nestedOUs.Add(deptOuEntry);
                    departmentOuCount++;
                }
            }
            if (!nestedOUs.Any()) nestedOUs.AddRange(rootOUs); // Ensure we have OUs to parent objects


            // --- Create Objects ---
            var createdGroups = new List<IADGroup>();
            int i = 0;
            for (i = 0; i < approximateCount; i++)
            {
                var parentOU = nestedOUs[random.Next(nestedOUs.Count)];
                var parentOuDN = parentOU.GetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName) as string;
                if (string.IsNullOrEmpty(parentOuDN)) continue;

                ActiveDirectoryObjectType typeToCreate = (ActiveDirectoryObjectType)random.Next(8); // User, Group, Computer, Contact, Printer

                string commonName;
                string samAccountName = null;
                MockDirectoryEntry entry;

                switch (typeToCreate)
                {
                    case ActiveDirectoryObjectType.User:
                        var firstName = firstNames[random.Next(firstNames.Length)];
                        var lastName = lastNames[random.Next(lastNames.Length)];
                        commonName = $"{firstName} {lastName}";
                        samAccountName = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(1, 99)}";
                        if (_entries.Any(e => e is IAccountDirectoryAdapter ada && ada.SAMAccountName == samAccountName)) samAccountName += random.Next(100, 199);

                        var userDN = $"CN={commonName},{parentOuDN}";
                        var userPath = $"{baseDomainPath}/{userDN}";

                        entry = new MockDirectoryEntry(commonName, userPath) { SchemaClassName = "user", Parent = parentOU };
                        entry.SetPropertyValue("objectClass", new List<object> { "top", "person", "organizationalPerson", "user" });
                        entry.SetPropertyValue(ActiveDirectoryFields.SAMAccountName.FieldName, samAccountName);
                        entry.SetPropertyValue(ActiveDirectoryFields.DisplayName.FieldName, commonName);
                        entry.SetPropertyValue(ActiveDirectoryFields.GivenName.FieldName, firstName);
                        entry.SetPropertyValue(ActiveDirectoryFields.SN.FieldName, lastName);
                        entry.SetPropertyValue(ActiveDirectoryFields.UserPrincipalName.FieldName, $"{samAccountName}@{ConnectionSettings.FQDN}");
                        entry.SetPropertyValue(ActiveDirectoryFields.Mail.FieldName, $"{samAccountName}@example.com"); // Use example.com for mocks
                        entry.SetPropertyValue(ActiveDirectoryFields.Department.FieldName, departments[random.Next(departments.Length)]);
                        entry.SetPropertyValue(ActiveDirectoryFields.Description.FieldName, $"Mock user account for {commonName}");
                        entry.SetPropertyValue("userAccountControl", 512); // Enabled, Normal Account
                        entry.SetPropertyValue(ActiveDirectoryFields.ObjectSID.FieldName, ("S-1").ToSidByteArray()); // Simplistic SID
                        entry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, userDN);
                        entry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(1, 90)));
                        AddMockDirectoryEntry(entry, ActiveDirectoryObjectType.User);
                        break;

                    case ActiveDirectoryObjectType.Group:
                        var groupNamePart = groupPurposes[random.Next(groupPurposes.Length)];
                        commonName = $"{groupPrefixes[random.Next(groupPrefixes.Length)]}{groupNamePart.Replace(" ", "")}";
                        samAccountName = commonName.ToLower() + random.Next(1, 50);
                        if (_entries.Any(e => e is IAccountDirectoryAdapter ada && ada.SAMAccountName == samAccountName)) samAccountName += random.Next(100, 199);


                        var groupDN = $"CN={commonName},{parentOuDN}";
                        var groupPath = $"{baseDomainPath}/{groupDN}";

                        entry = new MockDirectoryEntry(commonName, groupPath) { SchemaClassName = "group", Parent = parentOU };
                        entry.SetPropertyValue("objectClass", new List<object> { "top", "group" });
                        entry.SetPropertyValue(ActiveDirectoryFields.SAMAccountName.FieldName, samAccountName);
                        entry.SetPropertyValue(ActiveDirectoryFields.DisplayName.FieldName, commonName); // Groups often use CN as DisplayName
                        entry.SetPropertyValue("name", commonName);
                        entry.SetPropertyValue("groupType", random.Next(0, 2) == 0 ? -2147483640 : -2147483644); // Security Global or Universal Group
                        entry.SetPropertyValue(ActiveDirectoryFields.Description.FieldName, $"Mock group: {groupNamePart}");
                        entry.SetPropertyValue(ActiveDirectoryFields.ObjectSID.FieldName, new SecurityIdentifier(WellKnownSidType.WorldSid, null).Translate(typeof(NTAccount)).Value + "-" + random.Next(1000000, 9999999));
                        entry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, groupDN);
                        entry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(1, 90)));
                        AddMockDirectoryEntry(entry, ActiveDirectoryObjectType.Group);
                        var groupAdapter = _entries.LastOrDefault(e => e.DN == groupDN) as IADGroup;
                        if (groupAdapter != null) createdGroups.Add(groupAdapter);
                        break;

                    case ActiveDirectoryObjectType.Computer:
                        commonName = $"{computerPrefixes[random.Next(computerPrefixes.Length)]}{random.Next(1000, 9999)}";
                        samAccountName = $"{commonName}$"; // Computers have $ at the end of SAMAccountName
                        if (_entries.Any(e => e is IAccountDirectoryAdapter ada && ada.SAMAccountName == samAccountName)) samAccountName = $"{commonName}{random.Next(1, 9)}$";

                        var computerDN = $"CN={commonName},{parentOuDN}";
                        var computerPath = $"{baseDomainPath}/{computerDN}";

                        entry = new MockDirectoryEntry(commonName, computerPath) { SchemaClassName = "computer", Parent = parentOU };
                        entry.SetPropertyValue("objectClass", new List<object> { "top", "person", "organizationalPerson", "user", "computer" });
                        entry.SetPropertyValue(ActiveDirectoryFields.SAMAccountName.FieldName, samAccountName);
                        entry.SetPropertyValue(ActiveDirectoryFields.DisplayName.FieldName, commonName); // Usually CN
                        entry.SetPropertyValue("cn", commonName);
                        entry.SetPropertyValue(ActiveDirectoryFields.OperatingSystem.FieldName, osVersions[random.Next(osVersions.Length)]);
                        entry.SetPropertyValue(ActiveDirectoryFields.Description.FieldName, $"Mock computer: {commonName}");
                        entry.SetPropertyValue("userAccountControl", 4096); // Workstation/Server (Enabled)
                        entry.SetPropertyValue(ActiveDirectoryFields.ObjectSID.FieldName, new SecurityIdentifier(WellKnownSidType.WorldSid, null).Translate(typeof(NTAccount)).Value + "-" + random.Next(1000000, 9999999));
                        entry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, computerDN);
                        entry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(1, 90)));
                        AddMockDirectoryEntry(entry, ActiveDirectoryObjectType.Computer);
                        break;

                    case ActiveDirectoryObjectType.Contact:
                        var contactFirstName = firstNames[random.Next(firstNames.Length)];
                        var contactLastName = lastNames[random.Next(lastNames.Length)];
                        commonName = $"{contactFirstName} {contactLastName} (Contact)";
                        var contactEmail = $"{contactFirstName.ToLower()}.{contactLastName.ToLower()}@external-example.com";
                        if (_entries.Any(e => e is IGroupableDirectoryAdapter groupable && groupable.DisplayName == commonName)) commonName += random.Next(100, 199);


                        var contactDN = $"CN={commonName},{parentOuDN}";
                        var contactPath = $"{baseDomainPath}/{contactDN}";

                        entry = new MockDirectoryEntry(commonName, contactPath) { SchemaClassName = "contact", Parent = parentOU };
                        entry.SetPropertyValue("objectClass", new List<object> { "top", "person", "organizationalPerson", "contact" });
                        entry.SetPropertyValue(ActiveDirectoryFields.DisplayName.FieldName, commonName);
                        entry.SetPropertyValue(ActiveDirectoryFields.GivenName.FieldName, contactFirstName);
                        entry.SetPropertyValue(ActiveDirectoryFields.SN.FieldName, contactLastName);
                        entry.SetPropertyValue(ActiveDirectoryFields.Mail.FieldName, contactEmail);
                        entry.SetPropertyValue(ActiveDirectoryFields.Description.FieldName, $"External contact: {commonName}");
                        entry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, contactDN);
                        entry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(1, 90)));
                        AddMockDirectoryEntry(entry, ActiveDirectoryObjectType.Contact);
                        break;

                    case ActiveDirectoryObjectType.Printer:
                        var printerLocation = locations[random.Next(locations.Length)];
                        commonName = $"{printerLocation.Replace(" ", "")}-{printerModels[random.Next(printerModels.Length)].Split(' ')[1]}{random.Next(10, 99)}";
                        if (_entries.Any(e => e is IGroupableDirectoryAdapter groupable && groupable.DisplayName == commonName)) commonName += random.Next(100, 199);

                        var printerUnc = $"\\\\{computerPrefixes[1]}{random.Next(10, 50)}\\{commonName.Split('-').Last()}"; // UNC on a mock server

                        var printerDN = $"CN={commonName},{parentOuDN}";
                        var printerPath = $"{baseDomainPath}/{printerDN}";

                        entry = new MockDirectoryEntry(commonName, printerPath) { SchemaClassName = "printQueue", Parent = parentOU };
                        entry.SetPropertyValue("objectClass", new List<object> { "top", "printQueue" });
                        entry.SetPropertyValue("cn", commonName); // Common Name
                        entry.SetPropertyValue("printerName", commonName); // Actual printer name attribute
                        entry.SetPropertyValue("uNCName", printerUnc);
                        entry.SetPropertyValue("location", printerLocation);
                        entry.SetPropertyValue("driverName", "Universal Print Driver");
                        entry.SetPropertyValue("portName", "IP_10.0.1." + random.Next(100, 200));
                        entry.SetPropertyValue(ActiveDirectoryFields.Description.FieldName, $"Mock printer: {printerModels[random.Next(printerModels.Length)]}");
                        entry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, printerDN);
                        entry.SetPropertyValue("whenCreated", DateTime.UtcNow.AddDays(-random.Next(1, 90)));
                        AddMockDirectoryEntry(entry, ActiveDirectoryObjectType.Printer);
                        break;
                }
            }

            // --- Add some users to groups ---
            var usersForGrouping = _entries.OfType<IADUser>().ToList();
            if (usersForGrouping.Count > 0 && createdGroups.Count > 0)
            {
                foreach (var user in usersForGrouping.Take(Math.Min(usersForGrouping.Count, createdGroups.Count * 2))) // Add up to 2 users per group
                {
                    var groupToJoin = createdGroups[random.Next(createdGroups.Count)];
                    if (groupToJoin.DirectoryEntry is MockDirectoryEntry groupMockEntry)
                    {
                        var members = groupMockEntry.GetPropertyValues("member")?.ToList() ?? new List<object?>();
                        if (!members.Contains(user.DN))
                        {
                            members.Add(user.DN);
                            groupMockEntry.SetPropertyValue("member", members); // Assuming SetPropertyValue handles list assignment correctly for multi-valued
                        }

                        // Also update user's memberOf (though this is often automatically handled by AD, mock needs it explicit)
                        if (user.DirectoryEntry is MockDirectoryEntry userMockEntry)
                        {
                            var memberOfs = userMockEntry.GetPropertyValues("memberOf")?.ToList() ?? new List<object?>();
                            if (!memberOfs.Contains(groupToJoin.DN))
                            {
                                memberOfs.Add(groupToJoin.DN);
                                userMockEntry.SetPropertyValue("memberOf", memberOfs);
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Populated Mock AD with approximately {i} entries.");
        }

        public void AddEntry(IDirectoryEntryAdapter entryAdapter)
        {
            if (entryAdapter != null && !_entries.Any(e => e.DN == entryAdapter.DN))
            {
                _entries.Add(entryAdapter);
                if (entryAdapter.DirectoryEntry != null && !_directoryEntries.ContainsKey(entryAdapter.DirectoryEntry.Path))
                {
                    _directoryEntries[entryAdapter.DirectoryEntry.Path] = entryAdapter.DirectoryEntry;
                }
            }
        }
        public void AddMockDirectoryEntry(MockDirectoryEntry mockEntry, ActiveDirectoryObjectType objectType)
        {
            DirectoryEntryAdapter adapter;
            switch (objectType)
            {
                case ActiveDirectoryObjectType.User:
                    adapter = new ADUser();
                    break;
                case ActiveDirectoryObjectType.Group:
                    adapter = new ADGroup();
                    break;
                case ActiveDirectoryObjectType.OU:
                    adapter = new ADOrganizationalUnit();
                    break;
                case ActiveDirectoryObjectType.Computer:
                    adapter = new ADComputer();
                    break;
                case ActiveDirectoryObjectType.Contact:
                    adapter = new ADContact();
                    break;
                case ActiveDirectoryObjectType.Printer:
                    adapter = new ADPrinter();
                    break;
                case ActiveDirectoryObjectType.BitLocker:
                    adapter = new ADBitLockerRecovery();
                    break;
                default:
                    adapter = new DirectoryEntryAdapter(); // Or throw an exception for unsupported types
                    break;
            }
            adapter.Parse(this, mockEntry);
            AddEntry(adapter);
        }


        public IDirectoryEntryAdapter? FindEntryBySid(string sid)
        {
            byte[] sidBytes;
            try
            {
                sidBytes = sid.ToSidByteArray();
            }
            catch
            {
                return null; // Invalid SID format
            }
            return _entries.FirstOrDefault(e => e.SID != null && e.SID.SequenceEqual(sidBytes));
        }

        public IDirectoryEntryAdapter? FindEntryBySID(byte[] sid)
        {
            return _entries.FirstOrDefault(e => e.SID != null && e.SID.SequenceEqual(sid));
        }

        public IDirectoryEntryAdapter? FindEntryByGuid(byte[] guid)
        {
            return _entries.FirstOrDefault(e => e.Guid != null && e.Guid.SequenceEqual(guid));
        }

        public IDirectoryEntryAdapter? FindEntryByGuid(string guid)
        {
            if (System.Guid.TryParse(guid, out var parsedGuid))
            {
                return FindEntryByGuid(parsedGuid.ToByteArray());
            }
            return null;
        }

        public IADUser? Authenticate(LoginRequest loginReq)
        {
            // Mock authentication: find user and "authenticate" if password matches a mock value or is not checked
            var user = Users.FindUserByUsername(loginReq.Username) as IADUser;
            if (user != null)
            {
                // In a real mock, you might check loginReq.Password against a stored mock password
                // For simplicity, we'll assume successful if user exists.
                // You could also check if the user is disabled here.
                if (user is AccountDirectoryAdapter adUser && adUser.Disabled)
                {
                    return null; // User is disabled
                }
                return user;
            }
            return null;
        }

        public void Connect()
        {
            Status = DirectoryConnectionStatus.OK; // Assume connection is always successful for mock
            OnStatusChanged?.Invoke(Status);
        }

        public Task ConnectAsync()
        {
            Connect();
            return Task.CompletedTask;
        }

        public Task CancelConnection()
        {
            Status = DirectoryConnectionStatus.Unconfigured; // Or some other status indicating cancellation
            OnStatusChanged?.Invoke(Status);
            return Task.CompletedTask;
        }


        public IDirectoryEntry GetDeleteObjectsEntry()
        {
            return _deletedObjectsDg;
        }

        public IDirectoryEntry GetDirectoryEntry(string? baseDN = null)
        {
            if (string.IsNullOrEmpty(baseDN))
            {
                return _appRootDg; // Default to AppRoot if no DN is specified
            }

            var fullPath = $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}/{baseDN}";
            if (_directoryEntries.TryGetValue(fullPath, out var de))
            {
                return de;
            }

            // If not found, create a new mock entry for it (useful for dynamic OU creation in tests)
            var newEntry = new MockDirectoryEntry(baseDN.Split(',')[0].Split('=')[1], fullPath);
            _directoryEntries[fullPath] = newEntry;
            return newEntry;
        }

        public bool RestoreTombstone(IDirectoryEntryAdapter model, IADOrganizationalUnit newOU)
        {
            // Mock restoration: Mark as not deleted and update OU (Path/DN)
            if (model is DirectoryEntryAdapter adapter && adapter.IsDeleted)
            {
                // adapter.IsDeleted = false; // This property is get-only in the base class
                // Need a way to "undelete" in the mock, perhaps by removing a "isDeleted" property if set in MockDirectoryEntry
                if (adapter.DirectoryEntry is MockDirectoryEntry mockEntry)
                {
                    mockEntry.SetPropertyValue("isDeleted", false); // Assuming isDeleted is a settable property in your mock
                }


                // Update the model's DN and Path to reflect the new OU.
                // This is a simplified representation.
                var cn = model.CanonicalName; // Or parse from DN
                var newDn = $"CN={cn},{newOU.DN}";
                var newPath = $"LDAP://{ConnectionSettings.ServerAddress}:{ConnectionSettings.ServerPort}/{newDn}";

                if (adapter.DirectoryEntry != null)
                {
                    adapter.DirectoryEntry.Path = newPath;
                    _directoryEntries.Remove(model.DN); // remove old DN if path is key
                    _directoryEntries[newPath] = adapter.DirectoryEntry;
                }
                adapter.DirectoryEntry.SetPropertyValue(ActiveDirectoryFields.DistinguishedName.FieldName, newDn);


                return true;
            }
            return false;
        }

        public IDirectoryEntryAdapter? GetDirectoryEntryByDN(string? dn)
        {
            if (string.IsNullOrEmpty(dn)) return null;
            return _entries.FirstOrDefault(e => string.Equals(e.DN, dn, StringComparison.OrdinalIgnoreCase));
        }

        // Helper method to get all stored adapter entries for test verification
        public IEnumerable<IDirectoryEntryAdapter> GetAllAdapters() => _entries.AsReadOnly();
        public IEnumerable<IDirectoryEntry> GetAllDirectoryEntries() => _directoryEntries.Values.ToList().AsReadOnly();


        public void Dispose()
        {
            // Cleanup mock resources if any
        }

        AppLdapConnection? IActiveDirectoryContext.Connect()
        {
            throw new NotImplementedException();
        }

        Task<AppLdapConnection?> IActiveDirectoryContext.ConnectAsync()
        {
            throw new NotImplementedException();
        }
    }
}