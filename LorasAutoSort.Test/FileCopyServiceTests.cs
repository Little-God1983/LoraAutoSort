using FluentAssertions;
using Services.LoraAutoSort.Classes;
using Services.LoraAutoSort.Services;
using System.Collections.ObjectModel;

namespace LorasAutoSort.Test
{
    public class FileCopyServiceTests : IDisposable
    {
        private readonly string _testSourcePath;
        private readonly string _testTargetPath;
        private readonly FileCopyService _service;

        public FileCopyServiceTests()
        {
            _testSourcePath = Path.Combine(Path.GetTempPath(), "LoraAutoSortTests_Source");
            _testTargetPath = Path.Combine(Path.GetTempPath(), "LoraAutoSortTests_Target");
            _service = new FileCopyService();

            SetupTestDirectories();
        }

        private void SetupTestDirectories()
        {
            foreach (var path in new[] { _testSourcePath, _testTargetPath })
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
            }
        }

        [Fact]
        public void ProcessModelClasses_ShouldCopyFilesCorrectly()
        {
            // Arrange
            var modelFile = "test_model.safetensors";
            var modelFilePath = Path.Combine(_testSourcePath, modelFile);
            File.WriteAllText(modelFilePath, "test content");

            var models = new List<ModelClass>
            {
                new ModelClass
                {
                    ModelName = "test_model",
                    AssociatedFilesInfo = new List<FileInfo> { new FileInfo(modelFilePath) },
                    NoMetaData = false,
                    DiffusionBaseModel = "SD 1.5",
                    CivitaiCategory = CivitaiBaseCategories.CHARACTER,
                    ModelType = DiffusionTypes.LORA
                }
            };

            var options = new SelectedOptions
            {
                TargetPath = _testTargetPath,
                CreateBaseFolders = true,
                IsMoveOperation = false,
                OverrideFiles = true,
                UseCustomMappings = false
            };

            var progress = new Progress<ProgressReport>();
            var cts = new CancellationTokenSource();

            // Act
            bool result = _service.ProcessModelClasses(progress, cts.Token, models, options);

            // Assert
            result.Should().BeFalse(); // No errors
            var expectedPath = Path.Combine(_testTargetPath, "SD 1.5", "CHARACTER", modelFile);
            File.Exists(expectedPath).Should().BeTrue();
            // Original file should still exist because it was a copy operation
            File.Exists(modelFilePath).Should().BeTrue();
        }

        [Fact]
        public void ProcessModelClasses_ShouldMoveFilesCorrectly()
        {
            // Arrange
            var modelFile = "test_model.safetensors";
            var modelFilePath = Path.Combine(_testSourcePath, modelFile);
            File.WriteAllText(modelFilePath, "test content");

            var models = new List<ModelClass>
            {
                new ModelClass
                {
                    ModelName = "test_model",
                    AssociatedFilesInfo = new List<FileInfo> { new FileInfo(modelFilePath) },
                    NoMetaData = false,
                    DiffusionBaseModel = "SD 1.5",
                    CivitaiCategory = CivitaiBaseCategories.CHARACTER,
                    ModelType = DiffusionTypes.LORA
                }
            };

            var options = new SelectedOptions
            {
                TargetPath = _testTargetPath,
                CreateBaseFolders = true,
                IsMoveOperation = true,
                OverrideFiles = true,
                UseCustomMappings = false
            };

            var progress = new Progress<ProgressReport>();
            var cts = new CancellationTokenSource();

            // Act
            bool result = _service.ProcessModelClasses(progress, cts.Token, models, options);

            // Assert
            result.Should().BeFalse(); // No errors
            var expectedPath = Path.Combine(_testTargetPath, "SD 1.5", "CHARACTER", modelFile);
            File.Exists(expectedPath).Should().BeTrue();
            // Original file should not exist because it was a move operation
            File.Exists(modelFilePath).Should().BeFalse();
        }

        [Fact]
        public void ProcessModelClasses_WithCustomMapping_ShouldUseCorrectPath()
        {
            // Arrange
            var modelFile = "test_model.safetensors";
            var modelFilePath = Path.Combine(_testSourcePath, modelFile);
            File.WriteAllText(modelFilePath, "test content");

            var models = new List<ModelClass>
            {
                new ModelClass
                {
                    ModelName = "test_model",
                    AssociatedFilesInfo = new List<FileInfo> { new FileInfo(modelFilePath) },
                    NoMetaData = false,
                    DiffusionBaseModel = "SD 1.5",
                    CivitaiCategory = CivitaiBaseCategories.CHARACTER,
                    ModelType = DiffusionTypes.LORA,
                    Tags = new List<string> { "custom_tag" }
                }
            };

            // Mock the CustomTagMapXmlService by creating a test mapping file
            var customMapping = new CustomTagMap
            {
                LookForTag = new List<string> { "custom_tag" },
                MapToFolder = "CustomFolder",
                Priority = 0
            };

            var options = new SelectedOptions
            {
                TargetPath = _testTargetPath,
                CreateBaseFolders = true,
                IsMoveOperation = false,
                OverrideFiles = true,
                UseCustomMappings = true
            };

            var progress = new Progress<ProgressReport>();
            var cts = new CancellationTokenSource();

            // Create test mapping file
            var xmlService = new CustomTagMapXmlService();
            xmlService.SaveMappings(new ObservableCollection<CustomTagMap> { customMapping });

            // Act
            bool result = _service.ProcessModelClasses(progress, cts.Token, models, options);

            // Assert
            result.Should().BeFalse(); // No errors
            var expectedPath = Path.Combine(_testTargetPath, "SD 1.5", "CustomFolder", modelFile);
            File.Exists(expectedPath).Should().BeTrue();
        }

        [Fact]
        public void ProcessModelClasses_ShouldSkipNonLoraFiles()
        {
            // Arrange
            var modelFile = "test_model.safetensors";
            var modelFilePath = Path.Combine(_testSourcePath, modelFile);
            File.WriteAllText(modelFilePath, "test content");

            var models = new List<ModelClass>
            {
                new ModelClass
                {
                    ModelName = "test_model",
                    AssociatedFilesInfo = new List<FileInfo> { new FileInfo(modelFilePath) },
                    NoMetaData = false,
                    DiffusionBaseModel = "SD 1.5",
                    CivitaiCategory = CivitaiBaseCategories.CHARACTER,
                    ModelType = DiffusionTypes.EMBEDDING // Not a LORA
                }
            };

            var options = new SelectedOptions
            {
                TargetPath = _testTargetPath,
                CreateBaseFolders = true,
                IsMoveOperation = false,
                OverrideFiles = true,
                UseCustomMappings = false
            };

            var progress = new Progress<ProgressReport>();
            var cts = new CancellationTokenSource();

            // Act
            bool result = _service.ProcessModelClasses(progress, cts.Token, models, options);

            // Assert
            result.Should().BeTrue(); // Should have errors because file was skipped
            var expectedPath = Path.Combine(_testTargetPath, "SD 1.5", "CHARACTER", modelFile);
            File.Exists(expectedPath).Should().BeFalse();
            // Original file should still exist
            File.Exists(modelFilePath).Should().BeTrue();
        }

        void IDisposable.Dispose()
        {
            foreach (var path in new[] { _testSourcePath, _testTargetPath })
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }
    }
}