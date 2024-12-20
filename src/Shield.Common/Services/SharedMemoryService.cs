﻿using Microsoft.Extensions.Options;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.IO.MemoryMappedFiles;
using System.Reflection;

namespace Shield.Common.Services
{
    /**************** Memory Map ****************
     * byte#               function             *
     *  1       ServiceStatus of Lcd.Primary    *
     *  2       ServiceStatus of Lcd.Secondary  *
     *  3       ServiceStatus of Fan.In         *
     *  4       ServiceStatus of Fan1.Out       *
     *******************************************/

    public class SharedMemoryService : ISharedMemoryService
    {
        private static readonly string _filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, Constants.SHARED_MEMORY_FILE);
        private readonly MemoryMappedFile? _sharedMemory;

        public SharedMemoryService(IOptions<SharedMemoryOptions> options)
        {
            FileStream file;

            switch (options.Value.Source)
            {
                case SharedMemorySource.Startup:
                    if (File.Exists(_filePath)) File.Delete(_filePath);
                    file = File.Open(_filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                    break;
                case SharedMemorySource.Command:
                    file = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    break;
                default:
                    throw new ArgumentException(Constants.SHARED_MEMORY_INVALID_SOURCE);
            }

            //Set MemoryMappedFile
            _sharedMemory ??= MemoryMappedFile.CreateFromFile(file, null, 16,
                MemoryMappedFileAccess.ReadWriteExecute, HandleInheritability.Inheritable, false);
        }

        public void Write(SharedMemoryByte offset, ServiceStatus serviceStatus)
        {
            using var acessor = _sharedMemory?.CreateViewAccessor((int)offset, 1, MemoryMappedFileAccess.Write);
            acessor!.Write(0, (byte)serviceStatus);
        }

        public ServiceStatus Read(SharedMemoryByte serviceStatus)
        {
            using var acessor = _sharedMemory?.CreateViewAccessor((int)serviceStatus, 1, MemoryMappedFileAccess.Read);
            acessor!.Read(0, out byte status);

            return (ServiceStatus)status;
        }
    }
}
