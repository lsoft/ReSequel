﻿//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************


namespace Extension.Tagging.Extractor
{
    public interface ITimeoutTagExtractor : ITagExtractor
    {
        //ITagUpdater TagUpdater
        //{
        //    get;
        //    set;
        //}

        void AsyncStart(
            );

        void SyncStop(
            );
    }
}