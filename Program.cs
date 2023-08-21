using System;
using System.IO;
using System.Text;

struct ArchiveFile
{
    public int FileOffset;
    public int FileLength;
    public string Filename;
}

class ArchiveExtractor
{
    private static readonly string ExtractedFolder = "Extracted";

    public static void Main(string[] args)
    {
        Console.Write("============================================\n");
        Console.BackgroundColor = ConsoleColor.Red;
        Console.Write("Gorky 17 *.dat and *.kdt extractor tool V1.1\n");
        Console.ResetColor();
        Console.Write("Moddb: https://www.moddb.com/games/gorky-17\n");
        Console.Write("Discord:https://discord.gg/yzHTckxT\n");
        Console.Write("By: Krisztian Kispeti\n");
        Console.Write("============================================\n");
        Console.Write("Enter the name of the *.dat or *.kdt file: ");
        string archivePath = Console.ReadLine();

        if (!File.Exists(archivePath))
        {
            Console.WriteLine("File can not found.");
            return;
        }

        ArchiveFile[] files = ExtractFiles(archivePath);

        if (files != null && files.Length > 0)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Archive size: {new FileInfo(archivePath).Length} byte");
            Console.WriteLine($"Number of files in archive: {files.Length}\n");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("There are no files for extraction in this archive.");
            return;
        }

        while (true)
        {   
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Available Commands: list, extract, quit");
            Console.Write("Make your selection: ");
            Console.ResetColor();
            string command = Console.ReadLine();

            if (command == "list")
            {
                ListFilesWithSize(files);
            }
            else if (command == "extract")
            {
                if (!Directory.Exists(ExtractedFolder))
                {
                    Directory.CreateDirectory(ExtractedFolder);
                }
                ExtractAllFiles(archivePath, files);
                Console.WriteLine("Extraction successful!");
            }
            else if (command == "quit")
            {
                break; // Kilépés a programból
            }
            else
            {
                Console.WriteLine("Unknown command.");
            }
        }
    }

    public static ArchiveFile[] ExtractFiles(string archivePath)
    {
        ArchiveFile[] files = null;

        using (BinaryReader reader = new BinaryReader(File.Open(archivePath, FileMode.Open)))
        {
            int numberOfFiles = reader.ReadInt32();
            reader.BaseStream.Seek(28, SeekOrigin.Current);

            files = new ArchiveFile[numberOfFiles];

            for (int i = 0; i < numberOfFiles; i++)
            {
                files[i].FileOffset = reader.ReadInt32();
                files[i].FileLength = reader.ReadInt32();
                reader.BaseStream.Seek(8, SeekOrigin.Current);
                files[i].Filename = Encoding.ASCII.GetString(reader.ReadBytes(112)).TrimEnd('\0');

                files[i].Filename = RemoveInvalidPathChars(files[i].Filename);
            }
        }

        return files;
    }

    public static string RemoveInvalidPathChars(string filename)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            filename = filename.Replace(invalidChar.ToString(), "");
        }
        return filename;
    }

    public static void ListFilesWithSize(ArchiveFile[] files)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\nList of files with size:");
        Console.ResetColor();

        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {files[i].Filename} - Size: {files[i].FileLength} Byte");
        }
    }

    public static void ExtractAllFiles(string archivePath, ArchiveFile[] files)
    {
        using (BinaryReader reader = new BinaryReader(File.Open(archivePath, FileMode.Open)))
        {
            foreach (var file in files)
            {
                reader.BaseStream.Seek(file.FileOffset, SeekOrigin.Begin);
                byte[] fileData = reader.ReadBytes(file.FileLength);

                string outputPath = Path.Combine(ExtractedFolder, file.Filename);

                try
                {
                    File.WriteAllBytes(outputPath, fileData);
                    Console.WriteLine($"Extracted: {file.Filename}");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Could not extracted: {file.Filename} (Access denied)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not extracted: {file.Filename} ({ex.Message})");
                }
            }
        }
    }
}
