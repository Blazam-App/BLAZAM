namespace BLAZAM.ActiveDirectory.Data
{
    public class AttributeSchemaInfo
    {
        public string AtttributeName { get; set; }
        public string AttributeSyntax { get; set; } // e.g., "2.5.5.12"
        public int OMSyntax { get; set; }          // e.g., 64
        public string OMObjectClass { get; set; }   // Dotted OID string if OMSyntax is 127
        public byte[] OMObjectClassBytes { get; set; }
        public bool IsSingleValued { get; set; }
    }

}
