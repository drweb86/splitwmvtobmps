﻿using System.Drawing.Imaging;
using HDE.Platform.Logging;

namespace HDE.IpCamEmu.Core.Source
{
    abstract class BaseSource: ISource
    {
        #region Fields

        protected readonly string _name;
        protected readonly ImageFormat _format;
        protected readonly ILog _log;

        #endregion

        public virtual ISourceServerCache PrepareSourceServerCache()
        {
            // Know nothing about server source cache.
            return null;
        }

        /// <summary>
        /// Sets cache.
        /// </summary>
        /// <param name="cache">Cache</param>
        public virtual void SetSourceServerCache(ISourceServerCache cache)
        {
            // Don't need it.
        }

        public abstract void Dispose();
        public abstract bool IsSourceEnded { get; }
        public abstract bool Reset();
        public abstract byte[] GetNextFrame();
        
        protected BaseSource(ILog ILog, string name, ImageFormat format)
        {
            _log = ILog;
            _name = name;
            _format = format;
        }
    }
}