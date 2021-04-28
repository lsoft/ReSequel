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
using Extension.Tagging.SqlQuery;
using Main.Helper;
using Main.Inclusion.Validated;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Extension.Tagging.Extractor
{
    public static class TagDescriptorHelper
    {
        public static ITagSpan<SqlQueryTag> CreateTagSpan(
            this IValidatedSqlInclusion inclusion,
            ITextSnapshot currentSnapshot
            )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            if (currentSnapshot == null)
            {
                throw new ArgumentNullException(nameof(currentSnapshot));
            }


            (int start, int end) = inclusion.Inclusion.TargetSyntax.GetStartEnd();

            var length = end - start;

            var snapshotSpan = new SnapshotSpan(
                 currentSnapshot,
                 new Span(start, length)
                 );

            return
                new TagSpan<SqlQueryTag>(
                    snapshotSpan,
                    new SqlQueryTag(
                        inclusion,
                        (start, end)
                        )
                    );
        }

    }

}
