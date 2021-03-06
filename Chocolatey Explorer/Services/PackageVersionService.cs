﻿using Chocolatey.Explorer.Model;
using Chocolatey.Explorer.Powershell;
using log4net;

namespace Chocolatey.Explorer.Services
{
    public class PackageVersionService : IPackageVersionService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PackageVersionService));

        private readonly IRun _powershellAsync;
        private string _package;
        private PackageVersion _packageVersion;
        private readonly ISourceService _sourceService;

        public delegate void VersionResult(PackageVersion version);
        public event VersionResult VersionChanged;

        public PackageVersionService() : this(new RunAsync(), new SourceService())
        {
        }

        public PackageVersionService(IRun powershell, ISourceService sourceService)
        {
            _powershellAsync = new RunAsync();
            _sourceService = sourceService;
            _powershellAsync.OutputChanged += VersionHandler;
            _powershellAsync.RunFinished += RunFinished;
        } 

        public void PackageVersion(string package)
        {
            log.Info("Getting version of package: " + package);
            _packageVersion = new PackageVersion();
            _package = package;
            _powershellAsync.Run("cver " + package + " -source " + _sourceService.Source);
        }

        private void VersionHandler(string version)
        {
            _packageVersion.Name = _package;
            if (version.StartsWith("found") && !version.StartsWith("foundCompare"))
            {
                _packageVersion.CurrentVersion = version.Substring(5).Trim();
            }
            if(version.StartsWith("latest") && !version.StartsWith("latestCopmpare"))
            {
                _packageVersion.Serverversion = version.Substring(6).Trim();
            }
        }

        private void RunFinished()
        {
            OnVersionChanged(_packageVersion);
        }

        private void OnVersionChanged(PackageVersion version)
        {
            var handler = VersionChanged;
            if (handler != null) handler(version);
        }

    }
}