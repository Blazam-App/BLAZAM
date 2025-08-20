using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Mocks;
using BLAZAM.Common.Data;

namespace BLAZAMActiveDirectoryTests
{

    public class MockDirectoryTests
    {
        public static readonly MockActiveDirectoryContext Directory = new MockActiveDirectoryContext();

        [Fact]
        public void Initializes()
        {
            var settings = Directory.ConnectionSettings;
            var status = Directory.Status;
            var appRoot = Directory.AppRootDirectoryEntry;
            var newUsers = Directory.Users.FindNewUsers();
            var objType = newUsers?.First()?.ObjectType;


            Assert.True(settings.IsValid);
            Assert.True(status == DirectoryConnectionStatus.OK);
            Assert.True(objType == ActiveDirectoryObjectType.User);
        }
        [Fact]
        public void ComputerSearch()
        {
            
            var newComputers = Directory.Computers.FindNewComputers();
            var objType = newComputers.First()?.ObjectType;


            Assert.True(objType == ActiveDirectoryObjectType.Computer);
        }
        [Fact]
        public void ContactSearch()
        {
            
            var newContacts = Directory.Contacts.FindNewContacts();
            var objType = newContacts.First()?.ObjectType;


            Assert.True(objType == ActiveDirectoryObjectType.Contact);
        }
        [Fact]
        public void GroupSearch()
        {
            
            var newGroups = Directory.Groups.FindNewGroups();
            var objType = newGroups?.First()?.ObjectType;


            Assert.True(objType == ActiveDirectoryObjectType.Group);
        }
        [Fact]
        public void PrinterSearch()
        {
            
            var newPrinters = Directory.Printers.FindNewPrinters();
            var objType = newPrinters.First()?.ObjectType;


            Assert.True(objType == ActiveDirectoryObjectType.Printer);
        }
    }
}