﻿using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Database.Models.Database.Permissions
{
    public class ActionAccessMapping
    {
        public int ActionAccessMappingId { get; set; }
        public ActiveDirectoryObjectType ObjectType { get; set; }
        /// <summary>
        /// Allow/Deny flag. Has a value of true when action is allowed.
        /// Has a value of false when action is denied.
        /// </summary>
        public bool AllowOrDeny { get; set; }
        public ActionAccessFlag ObjectAction { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public override int GetHashCode()
        {
            return (ObjectType.ToString()+ ObjectAction.Name).GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is ActionAccessMapping mapping)
            {
                if(mapping.ObjectType== ObjectType && mapping.ObjectAction == ObjectAction)
                {
                    return true;
                }
            }
            return false;
        }
    }
}