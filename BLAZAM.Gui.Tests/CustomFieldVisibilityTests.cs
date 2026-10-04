using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Gui.UI;
using Moq;

namespace BLAZAM.Gui.Tests;

public class CustomFieldVisibilityTests
{
    public static IEnumerable<object[]> ObjectTypes => Enum.GetValues<ActiveDirectoryObjectType>()
        .Where(type => type != ActiveDirectoryObjectType.All)
        .Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(ObjectTypes))]
    public void EachObjectTypeOnlyShowsItsConfiguredFields(ActiveDirectoryObjectType objectType)
    {
        var fields = Enum.GetValues<ActiveDirectoryObjectType>()
            .Where(type => type != ActiveDirectoryObjectType.All)
            .Select(type => Field(type)).ToList();
        var view = new TestEntryView();
        view.LoadFields(fields);
        view.DirectoryEntry = Entry(objectType);

        var visibleField = Assert.Single(view.VisibleFields);
        Assert.Same(fields.Single(field => field.ObjectTypes.Single().ObjectType == objectType), visibleField);
    }

    [Fact]
    public void FieldsFollowConfiguredObjectTypesWhenDisplayedEntryChanges()
    {
        var userField = Field(ActiveDirectoryObjectType.User);
        var groupField = Field(ActiveDirectoryObjectType.Group);
        var sharedField = Field(ActiveDirectoryObjectType.User, ActiveDirectoryObjectType.Group);
        var view = new TestEntryView();
        view.LoadFields([userField, groupField, sharedField]);

        view.DirectoryEntry = Entry(ActiveDirectoryObjectType.User);
        Assert.Collection(view.VisibleFields,
            field => Assert.Same(userField, field),
            field => Assert.Same(sharedField, field));

        view.DirectoryEntry = Entry(ActiveDirectoryObjectType.Group);
        Assert.Collection(view.VisibleFields,
            field => Assert.Same(groupField, field),
            field => Assert.Same(sharedField, field));

        view.DirectoryEntry = Entry(ActiveDirectoryObjectType.Computer);
        Assert.Empty(view.VisibleFields);
    }

    private static CustomActiveDirectoryField Field(params ActiveDirectoryObjectType[] types) => new()
    {
        ObjectTypes = types.Select(type => new ActiveDirectoryFieldObjectType { ObjectType = type }).ToList()
    };

    private static IDirectoryEntryAdapter Entry(ActiveDirectoryObjectType type)
    {
        var entry = new Mock<IDirectoryEntryAdapter>();
        entry.SetupGet(model => model.ObjectType).Returns(type);
        return entry.Object;
    }

    private sealed class TestEntryView : DirectoryEntryViewBase
    {
        public IList<CustomActiveDirectoryField> VisibleFields => CustomFields;
        public void LoadFields(IList<CustomActiveDirectoryField> fields) => CustomFields = fields;
    }
}
