using System;
using System.Collections.Generic;
using Extension.Tagging.SqlQuery;
using Extension.Tagging.Support;
using Extension.Tagging.ValidateButton;
using Main.Inclusion.Scanner;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Ninject;

namespace Extension.Tagging.SqlQueryAdornment
{
    public sealed class SqlQueryAdornmentTagger : IntraTextAdornmentTagger<SqlQueryTag, ValidateButtonAdornment>
    {
        public static ITagger<IntraTextAdornmentTag> GetTagger(
            IWpfTextView view,
            Lazy<ITagAggregator<SqlQueryTag>> tagAggregator
            )
        {
            return
                view.Properties.GetOrCreateSingletonProperty<SqlQueryAdornmentTagger>(
                    () => new SqlQueryAdornmentTagger(view, tagAggregator.Value)
                    );
        }

        private readonly ITagAggregator<SqlQueryTag> TagAggregator;

        private SqlQueryAdornmentTagger(
            IWpfTextView view,
            ITagAggregator<SqlQueryTag> tagAggregator
            )
            : base(view)
        {
            if (tagAggregator == null)
            {
                throw new ArgumentNullException(nameof(tagAggregator));
            }

            this.TagAggregator = tagAggregator;

            this.TagAggregator.TagsChanged += TagsChangedHandler;
        }

        private void TagsChangedHandler(object sender, TagsChangedEventArgs e)
        {
            if (_view == null)
            {
                return;
            }

            NormalizedSnapshotSpanCollection spans = e.Span.GetSpans(_view.TextBuffer);
            if (spans == null)
            {
                return;
            }
            if (spans.Count == 0)
            {
                return;
            }

            SnapshotSpan span = spans[0];

            InvalidateSpans(new List<SnapshotSpan> { span });
        }

        public void Dispose()
        {
            this.TagAggregator.Dispose();

            base._view.Properties.RemoveProperty(typeof(SqlQueryAdornmentTagger));
        }

        // To produce adornments that don't obscure the text, the adornment tags
        // should have zero length spans. Overriding this method allows control
        // over the tag spans.
        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, SqlQueryTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;

            var tags = this.TagAggregator.GetTags(spans);

            foreach (IMappingTagSpan<SqlQueryTag> tag in tags)
            {
                NormalizedSnapshotSpanCollection tagSpans = tag.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (tagSpans.Count != 1)
                    continue;

                var adornmentSpan = new SnapshotSpan(tagSpans[0].End, 0);

                yield return Tuple.Create(adornmentSpan, (PositionAffinity?)PositionAffinity.Successor, tag.Tag);
            }
        }

        protected override ValidateButtonAdornment CreateAdornment(SqlQueryTag tag, SnapshotSpan span)
        {
            return new ValidateButtonAdornment(
                CompositionRoot.Root.CurrentRoot.Kernel.Get<ISolutionNameProvider>(),
                tag
                );
        }

        protected override bool UpdateAdornment(ValidateButtonAdornment adornment, SqlQueryTag tag)
        {
            adornment.UpdateTag(tag);
            return true;
        }
    }
}
