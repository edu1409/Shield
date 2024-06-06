using Microsoft.Extensions.Options;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.IO.MemoryMappedFiles;
using System.Reflection;

namespace Shield.Common.Services
{
    public class SharedMemoryService : ISharedMemoryService
    {
        private readonly MemoryMappedFile _sharedMemory;

        public SharedMemoryService(IOptions<SharedMemoryOptions> options)
        {
            FileMode fileMode;
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, Constants.SHARED_MEMORY_FILE);

            switch (options.Value.Source)
            {
                case SharedMemorySource.Server:
                    if (File.Exists(filePath)) File.Delete(filePath);
                    fileMode = FileMode.CreateNew;
                    break;
                case SharedMemorySource.Client:
                    fileMode = FileMode.Open;
                    break;
                default:
                    throw new ArgumentException(Constants.SHARED_MEMORY_INVALID_SOURCE);
            }

            //Set MemoryMappedFile
            var file = File.Open(filePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
            _sharedMemory = MemoryMappedFile.CreateFromFile(file, null, 1,
                MemoryMappedFileAccess.ReadWriteExecute, HandleInheritability.Inheritable, false);
        }

        public void Write(DisplayBacklightStatus value)
        {
            using var stream = _sharedMemory.CreateViewStream();
            var writer = new BinaryWriter(stream);
            writer.Write((byte)value);
        }

        public DisplayBacklightStatus Read()
        {
            DisplayBacklightStatus result;

            using (MemoryMappedViewStream stream = _sharedMemory.CreateViewStream())
            {
                var reader = new BinaryReader(stream);
                result = (DisplayBacklightStatus)reader.ReadByte();
            }

            return result;
        }
    }
}
