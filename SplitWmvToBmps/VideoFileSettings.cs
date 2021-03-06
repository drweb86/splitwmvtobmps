﻿using System;
using System.IO;
using HDE.Platform.Logging;

namespace HDE.IpCamEmu.Core.Source
{
    public class VideoFileSettings : SourceSettings
    {
        #region Properties

        public string File { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public TimeSpan TimeStep { get; set; }
        public bool RotateY { get; set; }
        public bool Grayscale { get; set;  }

        #endregion

        internal override ISource Create(ILog log)
        {
            return new VideoFileSource(log, Name, Format, new FileInfo(File).FullName, TimeStart, TimeEnd, TimeStep, RotateY, Grayscale);
        }
    }
}