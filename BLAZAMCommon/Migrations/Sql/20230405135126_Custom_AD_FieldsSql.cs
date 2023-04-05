using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class CustomADFieldsSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "ObjectActionFlag",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FieldType",
                table: "ActiveDirectoryFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryFieldObjectMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFieldObjectMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveDirectoryFieldObjectMappings_ActiveDirectoryFields_ActiveDirectoryFieldId",
                        column: x => x.ActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType" },
                values: new object[,]
                {
                    { 100001, "Last Name", "sn", 0 },
                    { 100002, "First Name", "givenname", 0 },
                    { 100003, "Office", "physicalDeliveryOfficeName", 0 },
                    { 100004, "Employee ID", "employeeId", 0 },
                    { 100005, "Home Directory", "homeDirectory", 0 },
                    { 100006, "Logon Script Path", "scriptPath", 0 },
                    { 100007, "Profile Path", "profilePath", 0 },
                    { 100008, "Home Phone Number", "homePhone", 0 },
                    { 100009, "Street Address", "streetAddress", 0 },
                    { 100010, "City", "city", 0 },
                    { 100011, "State", "st", 0 },
                    { 100012, "Zip Code", "postalCode", 0 },
                    { 100013, "Site", "site", 0 },
                    { 100014, "Name", "name", 0 },
                    { 100015, "Username", "samaccountname", 0 },
                    { 100016, "SID", "objectSID", 0 },
                    { 100017, "E-Mail Address", "mail", 0 },
                    { 100018, "Description", "description", 0 },
                    { 100019, "Display Name", "displayName", 0 },
                    { 100020, "Distinguished Name", "distinguishedName", 0 },
                    { 100021, "Member Of", "memberOf", 0 },
                    { 100022, "Company", "company", 0 },
                    { 100023, "Title", "title", 0 },
                    { 100024, "User Principal Name", "userPrincipalName", 0 },
                    { 100025, "Telephone Number", "telephoneNumber", 0 },
                    { 100026, "PO Box", "postOfficeBox", 0 },
                    { 100027, "Canonical Name", "cn", 0 },
                    { 100028, "Home Drive", "homeDrive", 0 },
                    { 100029, "Department", "department", 0 },
                    { 100030, "Middle Name", "middleName", 0 },
                    { 100031, "Pager", "pager", 0 },
                    { 100032, "OS", "operatingSystemVersion", 0 },
                    { 100033, "Account Expiration", "accountExpires", 0 },
                    { 100034, "Manager", "manager", 0 }
                });

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Action",
                value: 4);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Action",
                value: 3);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Action",
                value: 8);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Action",
                value: 5);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Action",
                value: 6);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 6,
                column: "Action",
                value: 7);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 7,
                column: "Action",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 8,
                column: "Action",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 9,
                column: "Action",
                value: 1);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFieldObjectMappings",
                columns: new[] { "Id", "ActiveDirectoryFieldId", "ObjectType" },
                values: new object[,]
                {
                    { 100001, 100001, 0 },
                    { 100002, 100002, 0 },
                    { 100003, 100003, 0 },
                    { 100004, 100004, 0 },
                    { 100005, 100005, 0 },
                    { 100006, 100006, 0 },
                    { 100007, 100007, 0 },
                    { 100008, 100008, 0 },
                    { 100009, 100009, 0 },
                    { 100010, 100010, 0 },
                    { 100011, 100011, 0 },
                    { 100012, 100012, 0 },
                    { 100013, 100013, 1 },
                    { 100014, 100013, 2 },
                    { 100015, 100013, 0 },
                    { 100016, 100013, 3 },
                    { 100017, 100014, 0 },
                    { 100018, 100015, 0 },
                    { 100019, 100015, 1 },
                    { 100020, 100015, 2 },
                    { 100021, 100016, 0 },
                    { 100022, 100016, 1 },
                    { 100023, 100016, 2 },
                    { 100024, 100016, 3 },
                    { 100025, 100017, 0 },
                    { 100026, 100017, 1 },
                    { 100027, 100018, 0 },
                    { 100028, 100018, 1 },
                    { 100029, 100018, 2 },
                    { 100030, 100018, 3 },
                    { 100031, 100019, 0 },
                    { 100032, 100019, 1 },
                    { 100033, 100019, 2 },
                    { 100034, 100019, 3 },
                    { 100035, 100020, 0 },
                    { 100036, 100020, 1 },
                    { 100037, 100020, 2 },
                    { 100038, 100020, 3 },
                    { 100039, 100021, 0 },
                    { 100040, 100021, 1 },
                    { 100041, 100021, 2 },
                    { 100042, 100022, 0 },
                    { 100043, 100023, 0 },
                    { 100044, 100024, 0 },
                    { 100045, 100025, 0 },
                    { 100046, 100026, 0 },
                    { 100047, 100027, 0 },
                    { 100048, 100027, 1 },
                    { 100049, 100027, 2 },
                    { 100050, 100027, 3 },
                    { 100051, 100028, 0 },
                    { 100052, 100029, 0 },
                    { 100053, 100030, 0 },
                    { 100054, 100031, 0 },
                    { 100055, 100032, 2 },
                    { 100056, 100033, 0 },
                    { 100057, 100033, 2 },
                    { 100058, 100034, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryFieldObjectMappings_ActiveDirectoryFieldId",
                table: "ActiveDirectoryFieldObjectMappings",
                column: "ActiveDirectoryFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveDirectoryFieldObjectMappings");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100001);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100002);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100003);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100004);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100005);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100006);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100007);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100008);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100009);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100010);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100011);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100012);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100013);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100014);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100015);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100016);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100017);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100018);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100019);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100020);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100021);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100022);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100023);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100024);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100025);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100026);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100027);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100028);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100029);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100030);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100031);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100032);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100033);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 100034);

            migrationBuilder.DropColumn(
                name: "Action",
                table: "ObjectActionFlag");

            migrationBuilder.DropColumn(
                name: "FieldType",
                table: "ActiveDirectoryFields");

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName" },
                values: new object[,]
                {
                    { 1, "Last Name", "sn" },
                    { 2, "First Name", "givenname" },
                    { 3, "Office", "physicalDeliveryOfficeName" },
                    { 4, "Employee ID", "employeeId" },
                    { 5, "Home Directory", "homeDirectory" },
                    { 6, "Logon Script Path", "scriptPath" },
                    { 7, "Profile Path", "profilePath" },
                    { 8, "Home Phone Number", "homePhone" },
                    { 9, "Street Address", "streetAddress" },
                    { 10, "City", "city" },
                    { 11, "State", "st" },
                    { 12, "Zip Code", "postalCode" },
                    { 13, "Site", "site" },
                    { 14, "Name", "name" },
                    { 15, "Username", "samaccountname" },
                    { 16, "SID", "objectSID" },
                    { 17, "E-Mail Address", "mail" },
                    { 18, "Description", "description" },
                    { 19, "Display Name", "displayName" },
                    { 20, "Distinguished Name", "distinguishedName" },
                    { 21, "Member Of", "memberOf" },
                    { 22, "Company", "company" },
                    { 23, "Title", "title" },
                    { 24, "User Principal Name", "userPrincipalName" },
                    { 25, "Telephone Number", "telephoneNumber" },
                    { 26, "PO Box", "postOfficeBox" },
                    { 27, "Canonical Name", "cn" },
                    { 28, "Home Drive", "homeDrive" },
                    { 29, "Department", "department" },
                    { 30, "Middle Name", "middleName" },
                    { 31, "Pager", "pager" },
                    { 32, "OS", "operatingSystemVersion" },
                    { 33, "Account Expiration", "accountExpires" },
                    { 34, "Manager", "manager" }
                });
        }
    }
}
