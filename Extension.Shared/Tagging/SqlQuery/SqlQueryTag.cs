using Main.Inclusion.Validated;
using Microsoft.VisualStudio.Text.Tagging;

namespace Extension.Tagging.SqlQuery
{
    public class SqlQueryTag : TextMarkerTag
    {
        public IValidatedSqlInclusion Inclusion
        {
            get;
        }

        public (int start, int end) StartEnd
        {
            get;
        }

        public event TagStatusChangedDelegate TagStatusEvent;

        public SqlQueryTag(
            IValidatedSqlInclusion inclusion,
            (int start, int end) startEnd
            )
            : base("MarkerFormatDefinition/SqlBorderFormatDefinition")
        {
            if (inclusion == null)
            {
                throw new System.ArgumentNullException(nameof(inclusion));
            }

            Inclusion = inclusion;

            StartEnd = startEnd;

            inclusion.InclusionStatusEvent += RaiseTagStatusEvent;
        }

        private void RaiseTagStatusEvent()
        {
            var t = TagStatusEvent;
            if(t!=null)
            {
                t();
            }
        }
    }
}
