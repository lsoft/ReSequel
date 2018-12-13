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

using System.Windows.Media;
using Main;
using Main.Inclusion;
using Main.Inclusion.Validated;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text.Tagging;

namespace Extension.Tagging.SqlQuery
{
    public delegate void TagStatusChangedDelegate();

    public class SqlQueryTag : ITag
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
