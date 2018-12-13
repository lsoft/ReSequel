using Microsoft.VisualStudio.Text.Tagging;

namespace Extension.Tagging.SqlBorder
{
    public sealed class SqlBorderTag : TextMarkerTag
    {
        public SqlBorderTag()
           : base("MarkerFormatDefinition/SqlBorderFormatDefinition")
        {
        }
    }

}
