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
using System.Collections.Generic;
using System.Diagnostics;
using Extension.Tagging.Extractor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Ninject;

namespace Extension.Tagging.SqlQuery
{
    public class SqlQueryTagger : ITagger<SqlQueryTag>, ITagUpdater
    {
        private ITextBuffer _buffer;
        private readonly ITagExtractor _tagExtractor;

        public SqlQueryTagger(
            ITextBuffer buffer
            )
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _tagExtractor = CompositionRoot.Root.CurrentRoot.Kernel.Get<ITagExtractor>();
            
            _buffer = buffer;
            _buffer.Changed += (sender, args) => HandleBufferChanged(args);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;


        public IEnumerable<ITagSpan<SqlQueryTag>> GetTags(
              NormalizedSnapshotSpanCollection spans
            )
        {
            var result = _tagExtractor.GetTags(
                this,
                _buffer
                );
            
            return result;
        }

        public void Raise(
            ITextSnapshot afterSnapshot,
            int start,
            int end
            )
        {
            if (afterSnapshot == null)
            {
                return;
            }

            var temp = TagsChanged;

            if (temp == null)
            {
                return;
            }


            try
            {
                // Combine all changes into a single span so that 
                // the ITagger<>.TagsChanged event can be raised just once for a compound edit 
                // with many parts. 

                SnapshotSpan totalAffectedSpan = new SnapshotSpan(
                    afterSnapshot,
                    start,
                    end - start
                    );

                temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            //using (var task = new System.Threading.Tasks.Task(
            //    async () =>
            //    {
            //        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //        try
            //        {
            //            // Combine all changes into a single span so that 
            //            // the ITagger<>.TagsChanged event can be raised just once for a compound edit 
            //            // with many parts. 

            //            SnapshotSpan totalAffectedSpan = new SnapshotSpan(
            //                afterSnapshot,
            //                start,
            //                end - start
            //                );

            //            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
            //        }
            //        catch (Exception excp)
            //        {
            //            Debug.WriteLine(excp.Message);
            //            Debug.WriteLine(excp.StackTrace);
            //        }
            //    }))
            //{
            //    task.Start();
            //    task.Wait();
            //}
        }

        // <summary> 
        /// Handle buffer changes. The default implementation expands changes to full lines and sends out 
        /// a <see cref="TagsChanged"/> event for these lines. 
        /// </summary> 
        /// <param name="args">The buffer change arguments.</param> 
        protected virtual void HandleBufferChanged(TextContentChangedEventArgs args)
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
                snapshot.GetLineFromPosition(end).End
                );

            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
        }
    }
}
