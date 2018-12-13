using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;
using System.Threading;

namespace Extension.Tagging.SqlBorder
{
    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/SqlBorderFormatDefinition")]
    [UserVisible(true)]
    internal sealed class SqlBorderFormatDefinition : MarkerFormatDefinition
    {
        public SqlBorderFormatDefinition()
        {
            var color = Brushes.DarkBlue.Clone();
            color.Opacity = 0.5;

            this.Fill = color;

            this.Border = new Pen(Brushes.Blue, 1.0)
            {
                DashStyle = new DashStyle(new[] { 2.0, 6.0 }, 1)
            };

            this.DisplayName = "Found SQL";
            this.ZOrder = 5;
        }
    }

}
