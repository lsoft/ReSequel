//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System;
using System.ComponentModel.Composition;
using Extension.Tagging.SqlQuery;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Extension.Tagging.SqlQueryAdornment
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [ContentType("projection")]
    [TagType(typeof(IntraTextAdornmentTag))]
    public sealed class SqlQueryAdornmentTaggerProvider : IViewTaggerProvider
    {

        private IBufferTagAggregatorFactoryService _bufferTagAggregatorFactoryService;

        [ImportingConstructor]
        public SqlQueryAdornmentTaggerProvider(
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

            if (textView == null)
                throw new ArgumentNullException("textView");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer != textView.TextBuffer)
                return null;

            return SqlQueryAdornmentTagger.GetTagger(
                (IWpfTextView)textView,
                new Lazy<ITagAggregator<SqlQueryTag>>(
                    () => _bufferTagAggregatorFactoryService.CreateTagAggregator<SqlQueryTag>(textView.TextBuffer)))
                as ITagger<T>;
        }
    }
}
