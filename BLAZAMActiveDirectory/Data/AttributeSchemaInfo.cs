namespace BLAZAM.ActiveDirectory.Data
{
    public class AttributeSchemaInfo
    {
        public string AtttributeName { get; set; }
        public int OMSyntax { get; set; }          // e.g., 64
        public bool IsSingleValued { get; set; }
    }

}
