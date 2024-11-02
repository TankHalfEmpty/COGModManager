using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;

namespace COGModManager
{
    internal class Program
    {
        private static string GameDirectory =
            @"C:\Program Files (x86)\Steam\steamapps\common\Slackers - Carts of Glory";

        private static string ExecutableName = "CartOfGlory.exe";
        private static string DataFileName = "COGModManagerData.cog";
        private static string PatchZipPath = "COGModManager.ModResources.PatchFiles.zip";
        private static string ContentFolderName = "Content";
        private static string WebURL = "https://cogmm.netlify.app/";

        private static string AppDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "COGModManager");

        private static string BackupFolder => Path.Combine(AppDataFolder, "Backup");
        private static string DataFilePath => Path.Combine(GameDirectory, DataFileName);
        private static Dictionary<string, ModData> InstalledMods = new Dictionary<string, ModData>();

        private static void WriteLineColored(string message, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }


        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Directory.CreateDirectory(AppDataFolder);


            if (!ValidateGameDirectory())
            {
                Console.WriteLine("⚠️ Game directory not found. Please enter the game directory path:");
                GameDirectory = Console.ReadLine() ?? string.Empty;
                if (!ValidateGameDirectory())
                {
                    WriteLineColored("❌ Invalid directory. Exiting...", ConsoleColor.Red);
                    return;
                }
            }

            if (IsFirstLaunch())
            {
                WriteLineColored("🛠️ First-time setup: Creating backup...", ConsoleColor.Yellow);
                Console.WriteLine();
                BackupGameDirectory();
                SaveFirstLaunchData();
                WriteLineColored("✅ Backup created successfully.", ConsoleColor.Gray);
                PromptReturn();
            }

            LoadInstalledMods();
            ShowMenu();
        }


        private static void ShowMenu()
        {
            while (true)
            {
                Console.Clear();
                WriteLineColored("🌌 COG Mod Manager by TankHalfEmpty", ConsoleColor.Cyan);
                WriteLineColored("1. 📜 Help", ConsoleColor.Green);
                WriteLineColored("2. 🔧 Patch", ConsoleColor.Green);
                WriteLineColored("3. 🧹 Unpatch", ConsoleColor.Green);
                WriteLineColored("4. ➕ Install", ConsoleColor.Green);
                WriteLineColored("5. ➖ Uninstall", ConsoleColor.Green);
                WriteLineColored("6. 🌐 Repository", ConsoleColor.Green);
                WriteLineColored("7. 🔄 Restore", ConsoleColor.Green);
                WriteLineColored("8. ❌ Exit", ConsoleColor.Red);

                string? input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.Clear();
                        ShowHelp();
                        break;
                    case "2":
                        Console.Clear();
                        Patch();
                        break;
                    case "3":
                        Console.Clear();
                        Unpatch();
                        break;
                    case "4":
                        Console.Clear();
                        Install();
                        break;
                    case "5":
                        Console.Clear();
                        Uninstall();
                        break;
                    case "6":
                        Console.Clear();
                        ShowRepository();
                        break;
                    case "7":
                        Console.Clear();
                        Restore();
                        break;
                    case "8":
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid option. Try again.");
                        PromptReturn();
                        break;
                }
            }
        }


        private static void ShowHelp()
        {
            Console.Clear();
            WriteLineColored("ℹ️ Help Menu:", ConsoleColor.Cyan);
            WriteLineColored("🔧 Patch - Applies the patch files from Unreal Mod Unlocker to the game.",
                ConsoleColor.Yellow);
            WriteLineColored("🧹 Unpatch - Removes patch files from the game.", ConsoleColor.Yellow);
            WriteLineColored("➕ Install - Installs a mod from a specified zip file or URL.", ConsoleColor.Yellow);
            WriteLineColored("➖ Uninstall - Uninstalls a mod by its name.", ConsoleColor.Yellow);
            WriteLineColored("🌐 Repository - Lists mods from the Online Repository.", ConsoleColor.Yellow);
            WriteLineColored("🔄 Restore - Restores the game files from the backup.", ConsoleColor.Yellow);
            PromptReturn();
        }

        private static void ShowRepository()
        {
            Console.Clear();
            WriteLineColored("🌐 Loading repository mods...", ConsoleColor.Cyan);

            string repoUrl = WebURL + "ModList.json";

            try
            {
                using WebClient client = new WebClient();
                string json = client.DownloadString(repoUrl);
                RepositoryData repoData = JsonConvert.DeserializeObject<RepositoryData>(json);

                if (repoData?.RepositoryMods == null || repoData.RepositoryMods.Count == 0)
                {
                    WriteLineColored("❌ No mods found in the repository.", ConsoleColor.Red);
                    PromptReturn();
                    return;
                }

                int index = 1;
                foreach (var mod in repoData.RepositoryMods)
                {
                    string modName = mod.ModName;
                    WriteLineColored($"{index++}. {modName} - {mod.ModDescription}", ConsoleColor.Green);
                }

                Console.WriteLine("Enter the number of the mod you want to download and install:");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 &&
                    choice <= repoData.RepositoryMods.Count)
                {
                    string selectedMod = repoData.RepositoryMods[choice - 1].FileName;
                    string modUrl = WebURL + "ModRepo/" + selectedMod;
                    DownloadAndInstallModFromUrl(modUrl);
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
            catch (Exception ex)
            {
                WriteLineColored($"❌ Error loading repository mods: {ex.Message}", ConsoleColor.Red);
            }

            PromptReturn();
        }


        private static bool ValidateGameDirectory() =>
            Directory.Exists(GameDirectory) && File.Exists(Path.Combine(GameDirectory, ExecutableName));

        private static bool IsFirstLaunch() =>
            !File.Exists(Path.Combine(AppDataFolder, "launchInfo.json"));

        private static void BackupGameDirectory()
        {
            Directory.CreateDirectory(BackupFolder);
            foreach (var dirPath in Directory.GetDirectories(GameDirectory, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(GameDirectory, BackupFolder));

            foreach (var filePath in Directory.GetFiles(GameDirectory, "*.*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(GameDirectory, BackupFolder), overwrite: true);
        }

        private static void SaveFirstLaunchData()
        {
            var launchInfo = new LaunchInfo
            {
                BackupCreationDate = DateTime.Now,
                ExecutableLastWriteTime = File.GetLastWriteTime(Path.Combine(GameDirectory, ExecutableName))
            };
            string json = JsonConvert.SerializeObject(launchInfo, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(AppDataFolder, "launchInfo.json"), json);
        }

        private static LaunchInfo LoadLaunchInfo()
        {
            string json = File.ReadAllText(Path.Combine(AppDataFolder, "launchInfo.json"));
            return JsonConvert.DeserializeObject<LaunchInfo>(json);
        }

        private static void Restore()
        {
            LaunchInfo launchInfo = LoadLaunchInfo();
            DateTime currentExeWriteTime = File.GetLastWriteTime(Path.Combine(GameDirectory, ExecutableName));

            if (currentExeWriteTime != launchInfo.ExecutableLastWriteTime)
            {
                WriteLineColored(
                    "⚠️ Warning: Game executable has been modified since the last backup. Restore anyway? (y/n)",
                    ConsoleColor.Yellow);
                if (Console.ReadLine()?.ToLower() != "y") return;
            }

            ClearDirectory(GameDirectory);
            Console.WriteLine("🔄 Restoring game files...");

            foreach (var dirPath in Directory.GetDirectories(BackupFolder, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(BackupFolder, GameDirectory));

            foreach (var filePath in Directory.GetFiles(BackupFolder, "*.*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(BackupFolder, GameDirectory), overwrite: true);

            WriteLineColored("✅ Game files restored successfully.", ConsoleColor.Green);
            PromptReturn();
        }

        private static void ClearDirectory(string directoryPath)
        {
            foreach (var filePath in Directory.GetFiles(directoryPath))
                File.Delete(filePath);

            foreach (var dirPath in Directory.GetDirectories(directoryPath))
                Directory.Delete(dirPath, true);
        }

        private static void Patch()
        {
            Console.WriteLine("🔧 Patching game...");
            string targetDir = Path.Combine(GameDirectory, "CartOfGlory", "Binaries", "Win64");
            Directory.CreateDirectory(targetDir);

            using Stream zipStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(PatchZipPath);
            if (zipStream == null)
            {
                WriteLineColored(("❌ Patch file not found in embedded resources."), ConsoleColor.Red);
                PromptReturn();
                return;
            }

            using (ZipArchive archive = new ZipArchive(zipStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(targetDir, entry.FullName);
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string? destinationDir = Path.GetDirectoryName(destinationPath);
                    if (destinationDir != null) Directory.CreateDirectory(destinationDir);

                    entry.ExtractToFile(destinationPath, overwrite: true);
                    Console.WriteLine($"✅ Extracted: {destinationPath}");
                }
            }

            WriteLineColored("✅ Patch applied successfully.", ConsoleColor.Green);
            PromptReturn();
        }

        private static void Unpatch()
        {
            Console.WriteLine("🧹 Unpatching game...");
            string targetDir = Path.Combine(GameDirectory, "CartOfGlory", "Binaries", "Win64");

            using Stream zipStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(PatchZipPath);
            if (zipStream == null)
            {
                WriteLineColored("❌ Patch file not found in embedded resources.", ConsoleColor.Red);
                PromptReturn();
                return;
            }

            using (ZipArchive archive = new ZipArchive(zipStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string filePath = Path.Combine(targetDir, entry.FullName);
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
            }

            WriteLineColored("✅ Unpatching complete.", ConsoleColor.Green);
            PromptReturn();
        }

        private static void Install()
        {
            Console.WriteLine("➕ Enter path to mod zip file or URL:");
            string zipPath = Console.ReadLine() ?? string.Empty;

            if (Uri.TryCreate(zipPath, UriKind.Absolute, out Uri? uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                Console.WriteLine("🌐 Downloading mod from URL...");
                string tempZipPath = Path.Combine(Path.GetTempPath(), "temp_mod.zip");

                try
                {
                    using (var webClient = new WebClient()) webClient.DownloadFile(zipPath, tempZipPath);
                    zipPath = tempZipPath;
                }
                catch (Exception ex)
                {
                    WriteLineColored($"❌ Failed to download mod: {ex.Message}", ConsoleColor.Red);
                    PromptReturn();
                    return;
                }
            }
            else if (!File.Exists(zipPath))
            {
                WriteLineColored("❌ File not found or path is invalid.", ConsoleColor.Red);
                PromptReturn();
                return;
            }

            Console.WriteLine($"🔎 Looking for mod at {zipPath}...");
            InstallModFromZip(zipPath);

            PromptReturn();
        }

        private static void Uninstall()
        {
            if (InstalledMods.Count == 0)
            {
                WriteLineColored("ℹ️ No mods currently installed.", ConsoleColor.Cyan);
                PromptReturn();
                return;
            }

            Console.WriteLine("Installed Mods:");
            int index = 1;
            var modList = InstalledMods.Keys.ToList();

            foreach (var modName in modList)
                Console.WriteLine($"{index++}. {modName}");

            Console.WriteLine("🔧 Enter the number of the mod to uninstall:");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= modList.Count)
            {
                string modName = modList[choice - 1];
                if (InstalledMods.TryGetValue(modName, out ModData? modData))
                {
                    UninstallModFiles(modName);
                    InstalledMods.Remove(modName);
                    SaveInstalledMods();
                    WriteLineColored($"✅ {modData.ModName} uninstalled successfully.", ConsoleColor.Green);
                }
            }
            else Console.WriteLine("Invalid selection.");

            PromptReturn();
        }

        private static void UninstallModFiles(string modName)
        {
            if (!InstalledMods.TryGetValue(modName, out ModData? modData))
            {
                WriteLineColored($"❌ Mod '{modName}' not found in installed mods.", ConsoleColor.Red);
                return;
            }

            Console.WriteLine($"🧹 Uninstalling '{modName}'...");

            foreach (string filePath in modData.InstalledFilePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    WriteLineColored($"❌ Removed: {filePath}", ConsoleColor.Red);
                }
                else
                {
                    WriteLineColored($"⚠️ File not found, skipping: {filePath}", ConsoleColor.Yellow);
                }
            }

            InstalledMods.Remove(modName);
            SaveInstalledMods();
        }

        private static void LoadInstalledMods()
        {
            if (File.Exists(DataFilePath))
            {
                string jsonData = File.ReadAllText(DataFilePath);
                InstalledMods = JsonConvert.DeserializeObject<Dictionary<string, ModData>>(jsonData) ?? new();
            }
        }

        private static void SaveInstalledMods()
        {
            string jsonData = JsonConvert.SerializeObject(InstalledMods, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(DataFilePath, jsonData);
        }

        private static void PromptReturn()
        {
            WriteLineColored("\nPress Enter to open the main menu.", ConsoleColor.Magenta);
            Console.ReadLine();
            Console.Clear();
        }

        private static void InstallModFromZip(string zipPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                ZipArchiveEntry? manifestEntry = archive.Entries.FirstOrDefault(e => e.FullName == "manifest.cog");
                if (manifestEntry == null)
                {
                    WriteLineColored("❌ Manifest file missing in zip.", ConsoleColor.Red);
                    return;
                }

                using (StreamReader reader = new StreamReader(manifestEntry.Open()))
                {
                    ModData modData = JsonConvert.DeserializeObject<ModData>(reader.ReadToEnd());
                    if (modData == null)
                    {
                        WriteLineColored("❌ Failed to parse manifest file.", ConsoleColor.Red);
                        return;
                    }

                    WriteLineColored(
                        "📋 Mod Identified: " + modData.ModName + " by " + modData.ModAuthor + " v" +
                        modData.ModVersion,
                        ConsoleColor.Cyan);

                    if (InstalledMods.TryGetValue(modData.ModName, out ModData? existingMod))
                    {
                        int versionComparison = string.Compare(modData.ModVersion, existingMod.ModVersion);
                        if (versionComparison == 0)
                        {
                            WriteLineColored($"ℹ️ Identical version of '{modData.ModName}' found. Re-Install? (y/n)",
                                ConsoleColor.Yellow);
                            if (Console.ReadLine()?.ToLower() != "y") return;
                        }
                        else if (versionComparison > 0)
                        {
                            Console.WriteLine(
                                $"🛠️ Upgrading mod '{modData.ModName}' from version {existingMod.ModVersion} to {modData.ModVersion}.");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"🛠️ Downgrading mod '{modData.ModName}' from version {existingMod.ModVersion} to {modData.ModVersion}.");
                        }

                        UninstallModFiles(existingMod.ModName);
                        InstalledMods.Remove(existingMod.ModName);
                    }

                    var chosenAddons = new HashSet<string>();
                    foreach (var addon in modData.OptionalAddons)
                    {
                        if (!existingMod?.OptionalAddons.Any(a => a.Directory == addon.Directory) ?? true)
                        {
                            WriteLineColored($"Optional addon '{addon.AddonName}'. Install? (y/n)",
                                ConsoleColor.Yellow);
                            if (Console.ReadLine()?.ToLower() == "y")
                            {
                                Console.WriteLine("Added addon directory: " + addon.Directory);
                                chosenAddons.Add(addon.Directory);
                            }
                        }
                    }

                    Console.WriteLine("📦 Installing mod files...");
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".cog")) continue;

                        bool belongsToAddon =
                            modData.OptionalAddons.Any(addon => entry.FullName.StartsWith(addon.Directory));
                        bool shouldInstall = !belongsToAddon || chosenAddons.Any(dir =>
                            entry.FullName.StartsWith(dir, StringComparison.OrdinalIgnoreCase));

                        if (!shouldInstall) continue;

                        string destinationPath = Path.Combine(GameDirectory, entry.FullName);
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        string? destinationDir = Path.GetDirectoryName(destinationPath);
                        if (destinationDir != null) Directory.CreateDirectory(destinationDir);

                        entry.ExtractToFile(destinationPath, overwrite: true);
                        Console.WriteLine($"📁 {entry.Name} installed.");

                        modData.InstalledFilePaths.Add(destinationPath);
                    }

                    modData.OptionalAddons =
                        modData.OptionalAddons.Where(a => chosenAddons.Contains(a.Directory)).ToList();
                    InstalledMods[modData.ModName] = modData;
                    SaveInstalledMods();
                    WriteLineColored($"✅ {modData.ModName} version {modData.ModVersion} installed successfully.",
                        ConsoleColor.Green);
                }
            }
        }


        private static bool IsValidZipFile(string filePath)
        {
            try
            {
                using ZipArchive archive = ZipFile.OpenRead(filePath);
                return true;
            }
            catch (InvalidDataException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating ZIP file: {ex.Message}");
                return false;
            }
        }


        private static void DownloadAndInstallModFromUrl(string modUrl)
        {
            string tempZipPath = Path.Combine(Path.GetTempPath(), "temp_mod.zip");

            try
            {
                using WebClient webClient = new WebClient();
                webClient.DownloadFile(modUrl, tempZipPath);
                Console.WriteLine($"🌐 Downloaded mod from repository: {modUrl}");

                if (!IsValidZipFile(tempZipPath))
                {
                    WriteLineColored("❌ The downloaded file is not a valid ZIP archive. Please try downloading again.",
                        ConsoleColor.Red);
                    return;
                }

                InstallModFromZip(tempZipPath);
            }
            catch (Exception ex)
            {
                WriteLineColored($"❌ Failed to download mod: {ex.Message}", ConsoleColor.Red);
                PromptReturn();
            }
        }
    }

    internal class ModData
    {
        public string ModName { get; set; }
        public string ModAuthor { get; set; }
        public string ModVersion { get; set; }
        public List<Addon> OptionalAddons { get; set; } = new List<Addon>();
        public List<string> InstalledFilePaths { get; set; } = new List<string>();
    }


    internal class Addon
    {
        public string AddonName { get; set; }
        public string Directory { get; set; }
    }

    internal class LaunchInfo
    {
        public DateTime BackupCreationDate { get; set; }
        public DateTime ExecutableLastWriteTime { get; set; }
    }

    internal class RepositoryData
    {
        public List<RepositoryMod> RepositoryMods { get; set; }
    }

    internal class RepositoryMod
    {
        public string FileName { get; set; }
        public string ModName { get; set; }
        public string ModDescription { get; set; }
    }
}