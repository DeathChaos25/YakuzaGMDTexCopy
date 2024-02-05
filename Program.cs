using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Linq;

namespace YakuzaGMDTexCopy
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("YakuzaGMDTexCopy:\nUsage:\nDrag and Drop a Yakuza game's .GMD model file into the program's exe");
                ExitDialog();
            }
            else
            {
                FileInfo arg0 = new FileInfo(args[0]);
                List<string> TextureNames = new List<string>();
                List<string> TextureNamesList = new List<string>();
                string texturesDir = "null";
                string commonDir = "null";
                string targetDir = "null";

                if (arg0.Name.ToLower().Contains(".gmd"))
                {
                    Console.WriteLine($"Attempting to parse {arg0.Name}");

                    using (EndianBinaryReader br = new EndianBinaryReader(File.OpenRead(arg0.FullName), true))
                    {
                        string magic = new string(br.ReadChars(4)); // GSGM
                        if (magic != "GSGM")
                        {
                            Console.WriteLine($"Not a valid GMD file - invalid magic: {magic}");
                            return;
                        }

                        int versionNum = br.ReadInt32();

                        if (versionNum == 0x102)
                        {
                            br.IsLittleEndian = false;
                            Console.WriteLine("Detected Big Endian GMD");
                        }

                        br.BaseStream.Seek(0x70, SeekOrigin.Begin);

                        int materialPointer = br.ReadInt32();
                        int materialCount = br.ReadInt32();
                        br.BaseStream.Seek(materialPointer, SeekOrigin.Begin);

                        Console.WriteLine("Listing Textures used by Model:\n");

                        for (int i = 0; i< materialCount; i++)
                        {
                            br.BaseStream.Seek(2, SeekOrigin.Current);
                            string materialName = new string(br.ReadChars(0x1E)).TrimEnd('\0');

                            Console.WriteLine($"{materialName}");

                            AddToListIfNotExist(TextureNames, materialName + ".dds");
                            AddToListIfNotExist(TextureNames, materialName + "_l.dds"); // Lost Judgment?

                            AddToListIfNotExist(TextureNamesList, materialName + ".dds"); // this is to write to txt
                        }
                    }

                    Console.WriteLine();

                    string iniFilePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "last_used_paths.ini");

                    DialogResult result;

                    if (File.Exists(iniFilePath) && MessageBox.Show("Use previously Saved paths?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        IniFile iniFileManager = new IniFile(iniFilePath);

                        texturesDir = iniFileManager.TexturesDir;
                        commonDir = iniFileManager.CommonDir;
                        targetDir = iniFileManager.TargetDir;

                        // Console.WriteLine($"ini TexturesDir is {texturesDir}");
                        // Console.WriteLine($"ini CommonDir is {commonDir}");
                        // Console.WriteLine($"ini TargetDir is {targetDir}");
                    }
                    else
                    {

                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                        folderBrowserDialog.Description = "Select the directory where the model's textures are located";

                        result = folderBrowserDialog.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            texturesDir = folderBrowserDialog.SelectedPath;
                        }
                        else
                        {
                            Console.WriteLine("No directory selected.");
                        }

                        folderBrowserDialog.Description = "Select the target directory to save textures to.";

                        result = folderBrowserDialog.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            targetDir = folderBrowserDialog.SelectedPath;
                        }

                        result = MessageBox.Show("Separate Common Textures?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                        if (result == DialogResult.Yes)
                        {
                            folderBrowserDialog.Description = "Select the directory where the target game's textures are located to compare for common textures.";

                            result = folderBrowserDialog.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                commonDir = folderBrowserDialog.SelectedPath;
                            }
                        }

                        File.WriteAllLines(iniFilePath, new[]
                        {
                            $"TexturesDir={texturesDir}",
                            $"CommonDir={commonDir}",
                            $"TargetDir={targetDir}"
                        });

                    }

                    Directory.CreateDirectory(targetDir);

                    if (commonDir != "null")
                    {
                        string outputDirCommon = Path.Join(targetDir, "common");
                        Directory.CreateDirectory(outputDirCommon);
                    }

                    List<string> TextureFailures = new List<string>();

                    // big mess here

                    List<string> FoundFiles = Directory.EnumerateFiles(texturesDir, "*.dds", SearchOption.AllDirectories).ToList();
                    List<string> FoundFilesCommon = new List<string>();

                    if (commonDir != "null")
                    {
                        FoundFilesCommon = Directory.EnumerateFiles(commonDir, "*.dds", SearchOption.AllDirectories).ToList();
                    }

                    FoundFiles.RemoveAll(a => !TextureNames.Any(b => Path.GetFileName(a) == b));
                    FoundFilesCommon.RemoveAll(a => !TextureNames.Any(b => Path.GetFileName(a) == b));

                    // Remove entries from texPaths that exist in texPathsCommon
                    FoundFiles = FoundFiles.Where(a => !FoundFilesCommon.Any(b => Path.GetFileName(a) == Path.GetFileName(b))).ToList();

                    string targetPath;

                    Console.WriteLine("Gathering Textures...");
                    foreach (string texture in FoundFiles)
                    {
                        targetPath = Path.Join(targetDir, Path.GetFileName(texture));

                        Console.WriteLine($"Copying {Path.GetFileName(texture)} to output folder");

                        File.Copy(texture, targetPath, true);
                    }

                    if (commonDir != "null") Console.WriteLine("Gathering Common Textures...");

                    foreach (string texture in FoundFilesCommon)
                    {
                        targetPath = Path.Combine(Path.Join(Path.Join(targetDir, "common"), Path.GetFileName(texture)));

                        Console.WriteLine($"Copying {Path.GetFileName(texture)} to output common folder");

                        File.Copy(texture, targetPath, true);
                    }

                    foreach(string texName in TextureNames)
                    {

                        // Check if the string is not found in either list
                        if (!FoundFiles.Any(a => Path.GetFileName(a) == texName) && !FoundFilesCommon.Any(b => Path.GetFileName(b) == texName))
                        {
                            if (!texName.Contains("_l", StringComparison.OrdinalIgnoreCase)) TextureFailures.Add(texName);
                        }

                    }

                    if (TextureFailures.Count > 0)
                    {
                        Console.WriteLine("\nThe following textures could not be found:");
                        TextureFailures.ForEach(i => Console.Write($"{i}\n"));
                    }

                    File.WriteAllLines(Path.Join(targetDir, arg0.Name + ".txt"), TextureNamesList);

                    // Console.WriteLine($"Current Paths:\nTextures Origin {texturesDir}\nCommon Textures Origin {commonDir}\nTarget Directory to save to {targetDir}");
                }
                else
                {
                    Console.WriteLine("YakuzaGMDTexCopy: This program only accepts .gmd files!\nUsage:\nDrag and Drop a Yakuza game's .GMD model file into the program's exe");
                    ExitDialog();
                }
                // else Console.WriteLine("https://youtu.be/yaAG8e2h0-k");
            }

            ExitDialog();
        }

        static void AddToListIfNotExist<T>(List<T> list, T newItem)
        {
            if (!list.Contains(newItem)) list.Add(newItem);
        }

        static void ExitDialog()
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}