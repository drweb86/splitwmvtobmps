using System;
using System.IO;
using System.Reflection;
using HDE.Platform.FileIO;
using HDE.Platform.Logging;
using SplitWmvToBmps;

namespace btgs
{
    class Program
    {
        #region Fields

        private static LogBase _log;

        #endregion

        #region Private Methods

        private static void Init()
        {
            _log = new QueueLog(
                new SimpleFileLog(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HDE.BitmapToGrayscale")),
                new ConsoleLog());
            _log.Open();
        }

        static void Main(string[] args)
        {
            Init();
            _log.Info("Ver. 4 (HDE.Platform Ver. {0}) (web-site http://splitwmvtobmps.codeplex.com/)", HDE.Platform.HdePlatformVersion.Version);
            if (args == null || args.Length !=2)
            {
                _log.Error("Improper usage.");
                _log.Info(@"Usage:
    first argument - input directory with bmp files (subdirectories will be taken into account also);
    second - output directory. When directory doesn't exists it will be created");
                return;
            }

            _log.Warning("Processing...");
            
            try
            {
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                var inputDir = RelativePathDiscovery.ResolveRelativePath(args[0], location);
                var outDir = RelativePathDiscovery.ResolveRelativePath(args[1], location);

                Directory.CreateDirectory(outDir);

                var files = Directory.GetFiles(inputDir, "*.bmp", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var targetFile = file.Substring(inputDir.Length+1);
                    ImageHelper.MakeGrayscale(file, RelativePathDiscovery.ResolveRelativePath(targetFile, outDir));
                }

                _log.Info("{0} files processed.", files.Length);
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
                Environment.ExitCode = -1;
            }
        }

        #endregion
    }
}
