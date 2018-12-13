using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.VisualStudio.Shell;
using Extension.Tagging.SqlQuery;

namespace Extension.Tagging.SqlBorder
{

    [Export]
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(SqlBorderTag))]
    public sealed class SqlBorderTaggerProvider : IViewTaggerProvider
    {
        private IBufferTagAggregatorFactoryService _bufferTagAggregatorFactoryService;

        [ImportingConstructor]
        public SqlBorderTaggerProvider(
            IBufferTagAggregatorFactoryService bufferTagAggregatorFactoryService
            )
        {
            if (bufferTagAggregatorFactoryService == null)
            {
                throw new ArgumentNullException(nameof(bufferTagAggregatorFactoryService));
            }

            _bufferTagAggregatorFactoryService = bufferTagAggregatorFactoryService;

        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (textView.TextBuffer != buffer)
            {
                return null;
            }

            return SqlBorderTagger.GetTagger(
                textView,
                buffer,
                new Lazy<ITagAggregator<SqlQueryTag>>(
                    () => _bufferTagAggregatorFactoryService.CreateTagAggregator<SqlQueryTag>(textView.TextBuffer)))
                as ITagger<T>;
        }
    }

}
