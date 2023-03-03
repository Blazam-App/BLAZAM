# Permissions Abstract
Permission in Blazam differ from Active Directory in one major (and extremely helpful) way.

Active Directory ACL permissions are applied per OU. This means that for every OU you want a group to have
access to, you have to create the exact same ACL list in each OU for that group. This is time consuming and
leads to mistakes.

Blazam adds a layer of abstraction to this paradigm. By including an `Access Level` layer between the OU permissions and the group assinged,
you can create a single ACL rule an reuse it for as many groups on as many OU's as you'd like.

The `Access Level`'s you define can be reused or coombined to create exactly the configuration you desire.

!!! example

	A group `HR` could be given the `Access Level` `Read Users` (which allows only read access to usr demographics  fields)
	and the `Read Groups` `Access Level` to the OU's `Company/Marketing` and `Company/IT` while also receiving `Rename Users` for the `Company/Marketing` OU
	as well as the `Deny Group Read` `Access Level` for the `Company/IT` OU.

	This will result in a member of `HR` to be able to read user demographics in `Company/Marketing` and `Company/IT` while being able to read
	the groups a user is a member of, only if the group is under the `Company/Marketing` OU.

	They will also be able to rename users under `Company/Marketing`


!!! note

	Permissions that are applied inherit fully down the OU tree unless a `Deny` permission is set at a lower level.

## Groups
The core element of the permission system in Blazam is the Active Directory Group.

Any group added will allow the members of that group to log into the application.

Nested group members are counted.

## Access Levels
Access Levels improve upon the default permission system found in Active Directory.
### Name
You can name your Access Levels however you'd like.
### Object Permissions
Permissions are split between different Active Directory object types. You can set different permissions
for groups from users, computers, or OU's within the same OU, or any combination therein.
### Field Permissions
Under each object type allowed, you can choose which fields will be denied, readable, or editable.

## Mappings
Mapping permissions is similar to default Active Directory permissions, but utilizing the powereful `Acces Level`
component to ease and enhance the delegation process.

## Impersonation
As a super admin, you will be able to impersonate the application experience of other users.
This is extremely helpful when setting up permission to verify the access you intended.