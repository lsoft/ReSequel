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
using Main;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Extension.Tagging.SqlQuery
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(SqlQueryTag))]
    public sealed class SqlQueryTaggerProvider : ITaggerProvider
    {
        [ImportingConstructor]
        public SqlQueryTaggerProvider(
            )
        {
        }


        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            return buffer.Properties.GetOrCreateSingletonProperty<SqlQueryTagger>(() => new SqlQueryTagger(buffer)) as ITagger<T>;
        }
    }
}
