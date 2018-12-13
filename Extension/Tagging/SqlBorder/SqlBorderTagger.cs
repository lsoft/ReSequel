using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extension.Tagging.SqlQuery;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Extension.Tagging.SqlBorder
{
    public sealed class SqlBorderTagger : ITagger<SqlBorderTag>
    {
        private ITextView _textView;
        private ITextBuffer _buffer;

        public static ITagger<SqlBorderTag> GetTagger(ITextView textView, ITextBuffer buffer, Lazy<ITagAggregator<SqlQueryTag>> aggregator)
        {
            return textView.Properties.GetOrCreateSingletonProperty<SqlBorderTagger>(
                () => new SqlBorderTagger(textView, buffer, aggregator.Value)
                );
        }

        private readonly ITagAggregator<SqlQueryTag> _tagAggregator;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public SqlBorderTagger(
            ITextView textView,
            ITextBuffer buffer,
            ITagAggregator<SqlQueryTag> tagAggregator
            )
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (tagAggregator == null)
            {
                throw new ArgumentNullException(nameof(tagAggregator));
            }

            _textView = textView;
            _buffer = buffer;

            this._tagAggregator = tagAggregator;

            this._tagAggregator.TagsChanged += TagsChangedHandler;
            buffer.Changed += (sender, args) => HandleBufferChanged(args);
        }

        private void TagsChangedHandler(object sender, TagsChangedEventArgs e)
        {
            NormalizedSnapshotSpanCollection spans = e.Span.GetSpans(e.Span.AnchorBuffer);

            if (spans == null)
            {
                return;
            }

            if (spans.Count == 0)
            {
                return;
            }

            SnapshotSpan snapshotSpan = spans[0];

            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    RaiseTagsChanged(snapshotSpan);
                }
                catch (Exception excp)
                {
                    Debug.WriteLine(excp.Message);
                    Debug.WriteLine(excp.StackTrace);
                }
            });
        }

        /// <summary>
        /// Causes adornments to be updated synchronously.
        /// </summary>
        private void RaiseTagsChanged(SnapshotSpan span)
        {
            var handler = this.TagsChanged;
            if (handler != null)
                handler(this, new SnapshotSpanEventArgs(span));
        }

        public IEnumerable<ITagSpan<SqlBorderTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var currentSnapshot = _buffer.CurrentSnapshot;

            IEnumerable<IMappingTagSpan<SqlQueryTag>> tags = this._tagAggregator.GetTags(spans);

            foreach (IMappingTagSpan<SqlQueryTag> tagSpan in tags)
            {
                SqlQueryTag tag = tagSpan.Tag;
                NormalizedSnapshotSpanCollection tagSpans = tagSpan.Span.GetSpans(currentSnapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (tagSpans.Count != 1)
                {
                    continue;
                }

                var span = new SnapshotSpan(
                    currentSnapshot,
                    new Span(tag.StartEnd.start, tag.StartEnd.end - tag.StartEnd.start)
                    );

                yield return
                    new TagSpan<SqlBorderTag>(
                        span,
                        new SqlBorderTag()
                        );
            }
        }

        // <summary> 
        /// Handle buffer changes. The default implementation expands changes to full lines and sends out 
        /// a <see cref="TagsChanged"/> event for these lines. 
        /// </summary> 
        /// <param name="args">The buffer change arguments.</param> 
        private void HandleBufferChanged(TextContentChangedEventArgs args)
        {
            if (args.Changes.Count == 0)
                return;

            var temp = TagsChanged;

            if (temp == null)
                return;

            // Combine all changes into a single span so that 
            // the ITagger<>.TagsChanged event can be raised just once for a compound edit 
            // with many parts. 

            ITextSnapshot snapshot = args.After;

            int start = args.Changes[0].NewPosition;
            int end = args.Changes[args.Changes.Count - 1].NewEnd;

            SnapshotSpan totalAffectedSpan = new SnapshotSpan(
                snapshot.GetLineFromPosition(start).Start,
                snapshot.GetLineFromPosition(end).End);

            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
        }

    }

}
