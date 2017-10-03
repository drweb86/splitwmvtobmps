using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using HDE.IpCamEmu.Core.Source;
using HDE.Platform.FileIO;
using HDE.Platform.Logging;

namespace SplitWmvToBmps
{
    class Program
    {
        #region Fields

        private static LogBase _log;
        private static Dictionary<string, ImageFormat> _imageFormats;

        #endregion

        #region Private Methods

        private static void Init()
        {
            _imageFormats = new Dictionary<string, ImageFormat>
                                {
                                    // key must be lowercase
                                    {"bmp",     ImageFormat.Bmp},
                                    {"emf",     ImageFormat.Emf},
                                    {"exif",    ImageFormat.Exif},
                                    {"gif",     ImageFormat.Gif},
                                    {"ico",     ImageFormat.Icon},
                                    {"jpg",     ImageFormat.Jpeg},
                                    {"png",     ImageFormat.Png},
                                    {"tiff",    ImageFormat.Tiff},
                                    {"wmf",     ImageFormat.Wmf},
                                };

            _log = new QueueLog(
                new SimpleFileLog(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HDE.SplitWmvToBmps")),
                new ConsoleLog());
            _log.Open();
        }

        static void Help(ILog log)
        {
            log.Info(@"Command line arguments:
""-input=path to video file""
    AVI, WMV input file. Relative paths are discovered relative to current dir
""-format=output video format"" (default: BMP)
    Can be one of the following: {0}.
""-rotate=true or false"" (default: false)
    Rotates image
""-start=0:00:00:00.0000"" (default: 0:00:00:00.0000)
    Video time from which to begin extract frames in the format: days:hours:minutes:seconds.msec
""-end=0:00:00:00.0000"" (default: 10675199:02:48:05.0000)
    Video time when finish extract frames in the format: days:hours:minutes:seconds.msec
""-step=0:00:00:00.0000"" (default: 0:00:00:00.0040)
    Time step between frames in the format: days:hours:minutes:seconds.msec
""-gray=true or false"" (default: false)
    When grayscale is true, images will be grayscaled
""-output=output directory""
    Output directory for images. When directory doesn't exists it will be created. Relative paths are discovered relative to current dir",
                string.Join(", ", _imageFormats.Keys));
        }

        static void Main(string[] args)
        {
            Init();

            _log.Info("Ver. 4 (HDE.Platform Ver. {0}) (web-site http://splitwmvtobmps.codeplex.com/)", HDE.Platform.HdePlatformVersion.Version);

            if (args.Length == 0)
            {
                Help(_log);
                return;
            }

            _log.Info("Passing arguments...");
            var argumentsDictionary = args
                .Select(item => item.Split(new[] {"="}, StringSplitOptions.None))
                .ToDictionary(item => item[0], item => item[1]);

            VideoFileSettings settings = new VideoFileSettings();

            if (!argumentsDictionary.ContainsKey("-input"))
            {
                _log.Error("-input argument was not specified.");
                Help(_log);
                return;
            }
            else
            {
                settings.File = RelativePathDiscovery.ResolveRelativePath(argumentsDictionary["-input"], Directory.GetCurrentDirectory());
                if (!File.Exists(settings.File))
                {
                    _log.Error("Input video file was not found: {0}", settings.File);
                    return;
                }

                if (!settings.File.EndsWith("wmv") &&
                    !settings.File.EndsWith("avi"))
                {
                    _log.Error("Only WMV and AVI files are accepted: {0}", settings.File);
                    return;
                }
            }

            // format
            var outputFormatExtension = "bmp";
            if (!argumentsDictionary.ContainsKey("-format"))
            {
                settings.Format = ImageFormat.Bmp;
            }
            else
            {
                var keyLowercase = argumentsDictionary["-format"].ToLowerInvariant();
                if (_imageFormats.ContainsKey(keyLowercase))
                {
                    settings.Format = _imageFormats[keyLowercase];
                    outputFormatExtension = keyLowercase;
                }
                else
                {
                    _log.Error("Could not parse -format setting {0}", argumentsDictionary["-format"]);
                    Help(_log);
                    return;
                }
            }

            // rotate
            if (!argumentsDictionary.ContainsKey("-rotate"))
            {
                settings.RotateY = false;
            }
            else
            {
                try
                {
                    settings.RotateY = bool.Parse(argumentsDictionary["-rotate"]);
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse -rotate setting {0}", argumentsDictionary["-rotate"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            // start
            if (!argumentsDictionary.ContainsKey("-start"))
            {
                settings.TimeStart = new TimeSpan();
            }
            else
            {
                try
                {
                    settings.TimeStart = TimeSpan.Parse(argumentsDictionary["-start"], CultureInfo.InvariantCulture);
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse -start setting {0}", argumentsDictionary["-start"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            // end
            if (!argumentsDictionary.ContainsKey("-end"))
            {
                settings.TimeEnd = TimeSpan.MaxValue;
            }
            else
            {
                try
                {
                    settings.TimeEnd = TimeSpan.Parse(argumentsDictionary["-end"], CultureInfo.InvariantCulture);
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse -end setting {0}", argumentsDictionary["-end"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            // step
            if (!argumentsDictionary.ContainsKey("-step"))
            {
                settings.TimeStep = new TimeSpan(0,0,0,0,40);
            }
            else
            {
                try
                {
                    settings.TimeStep = TimeSpan.Parse(argumentsDictionary["-step"], CultureInfo.InvariantCulture);
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse -step setting {0}", argumentsDictionary["-step"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            // gray
            if (!argumentsDictionary.ContainsKey("-gray"))
            {
                settings.Grayscale = false;
            }
            else
            {
                try
                {
                    settings.Grayscale = bool.Parse(argumentsDictionary["-gray"]);
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse -gray setting {0}", argumentsDictionary["-gray"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            string outputFolder = null;
            // output
            if (!argumentsDictionary.ContainsKey("-output"))
            {
                _log.Error("-output argument was not specified.");
                Help(_log);
                return;
            }
            else
            {
                try
                {
                    outputFolder = RelativePathDiscovery.ResolveRelativePath(argumentsDictionary["-output"], Directory.GetCurrentDirectory());
                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                    }
                }
                catch (Exception unhandledException)
                {
                    _log.Error("Could not parse or create -output setting {0}", argumentsDictionary["-output"]);
                    _log.Error(unhandledException);
                    Help(_log);
                    return;
                }
            }

            settings.Name = "Converter";
            try
            {
                var src = settings.Create(_log);
                var cache = src.PrepareSourceServerCache();
                src.SetSourceServerCache(cache);
                src.Reset();

                _log.Warning("Processing...");
                int frameNo = 0;
                while (!src.IsSourceEnded)
                {
                    var data = src.GetNextFrame();
                    if (data != null)
                    {
                        frameNo++;
                        var outputFile = Path.Combine(outputFolder, string.Format("{0:D6}.{1}", frameNo, outputFormatExtension));
                        if (File.Exists(outputFile))
                        {
                            File.Delete(outputFile);
                        }
                        File.WriteAllBytes(outputFile, data);
                    }
                    else
                    {
                        break;
                    }
                }
                _log.Info("Done.");
            }
            catch(Exception e)
            {
                _log.Error(e);
                Environment.ExitCode = -1;
            }
            Console.Beep();
        }

        #endregion
    }
}
