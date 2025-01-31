global using static System.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;

if (args.Length <= 1)
{
    WriteLine("Usage: " + "[-tscopy] [-move <directory>] [-rgx <pattern>] [-ext <output_extension>] ".Colorize(StringColors.Gray) + "<input_directory|input_file>".Colorize(StringColors.Cyan) + " <ffmpeg_params>");
    WriteLine();
    WriteLine("""
    Parameters:
    -tscopy: Copy timestamps from input to output file
    -move: Move original input files to specified directory after processing
    -rgx <pattern>: Regular expression pattern to filter input files
    -ext <extension>: File extension for output filename (if not provided, no extension is added)
    input_directory: If directory provided, all files in the directory are processed (not considering subdirectories)
    ffmpeg_params: FFmpeg parameters passed to the process (input and output are skipped)
    """.Colorize(StringColors.Gray));
    return;
}

try
{
    // PROCESS ARGUMENTS
    var input_files = ProcessArguments(out bool tscopy, out string ext, out string move_dir, out string ffparams);

    ffparams = CleanFFmpegParams(ffparams);

    // CONFIRM FILES
    WriteLine($"Confirm processing of following {input_files.Count} file{(input_files.Count > 1 ? "s" : "")}:".Colorize(StringColors.Yellow));
    foreach (var f in input_files)
        WriteLine("  - " + f.Colorize(StringColors.Yellow));

    WriteLine("\nOptions:\n" +
    "  - Copy timestamps: " + (tscopy ? "Yes".Colorize(StringColors.Green) : "No".Colorize(StringColors.Red)) + "\n" +
    "  - Move directory: " + (string.IsNullOrEmpty(move_dir) ? "None".Colorize(StringColors.Red) : move_dir.Colorize(StringColors.Cyan)) + "\n" +
    "  - Output extension: " + (string.IsNullOrEmpty(ext) ? "None".Colorize(StringColors.Red) : ext.Colorize(StringColors.Cyan)) + "\n" +
    "  - FFmpeg parameters: " + "-i [input] ".Colorize(StringColors.Gray) + ffparams.Colorize(StringColors.Cyan) + $" -y [output]{ext}".Colorize(StringColors.Gray));
    
    Write("\nProceed? (y/n): ");
    var choice = ReadLine()?.ToLower() ?? "n";
    if (choice != "y")
    {
        WriteLine("Operation cancelled.");
        return;
    }

    // PROCESS FILES
    WriteLine("\nProcessing files:");
    foreach (var f in input_files)
    {
        WriteLine();

        var dir = Path.GetDirectoryName(f) ?? "";
        var name = Path.GetFileNameWithoutExtension(f);
        var output = Path.Combine(dir, name + ext);
        var counter = 1;
        while (File.Exists(output))
            output = Path.Combine(dir, name + $"_{counter++}" + ext);

        var cmd = $"-hide_banner -i \"{f}\" {ffparams} -y \"{output}\"";

        Write(StatusUpdate(f, output, "STARTING    ".Colorize(StringColors.Gray)));

        if (!File.Exists(f))
        {
            Write(StatusUpdate(f, output, "NOT FOUND    ".Colorize(StringColors.Red)));
            continue;
        }

        var ts_created = File.GetCreationTime(f);
        var ts_modified = File.GetLastWriteTime(f);
        var ts_accessed = File.GetLastAccessTime(f);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = cmd,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
        {
            throw new Exception("Failed to start FFmpeg, make sure it is installed and accessible from current location (add to PATH if necessary)");
        }

        var log = "";
        process.ErrorDataReceived += (_, b) =>
        {
            if (b.Data == null) 
                return;

            log += "\n" + b.Data;
        };  
        process.BeginErrorReadLine();

        var sw = Stopwatch.StartNew();

        Write(StatusUpdate(f, output, "PROCESSING ".Colorize(StringColors.Yellow)));
        process.WaitForExit();

        sw.Stop();

        if (process.ExitCode != 0)
        {
            try
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
            catch { }

            Write(StatusUpdate(f, output, "ERROR       ".Colorize(StringColors.Red)));

            // make every line of log padded with spaces
            log = log.Replace("\n", "\n    ");
            WriteLine(log.Colorize(StringColors.GetForegroundColor(245, 124, 66)));
        }
        else
        {
            if (tscopy)
            {
                try
                {
                    if (File.Exists(output))
                    {
                        File.SetCreationTime(output, ts_created);
                        File.SetLastWriteTime(output, ts_modified);
                        File.SetLastAccessTime(output, ts_accessed);
                    }
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(move_dir) && Directory.Exists(move_dir))
            {
                try
                {
                    var move_file = Path.Combine(move_dir, Path.GetFileName(f));
                    File.Move(f, move_file, true);
                }
                catch { }
            }

            var seconds = sw.Elapsed.TotalSeconds;
            var time = seconds < 60 ? $"{seconds:0.0}s" : $"{seconds / 60:0.0}m";
            Write(StatusUpdate(f, output, $"OK [{time}]       ".Colorize(StringColors.Green)));
        }
    }

    WriteLine("\nAll files processed.");
}
catch (Exception ex)
{
    WriteLine($"\nError: {ex.Message}".Colorize(StringColors.Red) + "\n");
}

string StatusUpdate(string input, string output, string status)
{
    const int MAX_NAME_LENGTH = 42;
    return "\r  - " + PadTrim(input.Colorize(StringColors.Cyan), MAX_NAME_LENGTH) + " -> " +
                     PadTrim(output.Colorize(StringColors.Cyan), MAX_NAME_LENGTH) + ": " +
                     status;
}

string PadTrim(string name, int maxLength)
{
    // if below length, pad it with space
    if (name.Length < maxLength)
    {
        var pad = new string(' ', maxLength - name.Length);
        return name + pad;
    }

    // if above length, trim it (account for 3 dots)
    var half = maxLength / 2;
    return name[..half] + "..." + name[^(maxLength - half - 3)..];
}

string CleanFFmpegParams(string ffparams)
{
    // NOTE: we should keep the input and output file parameters in case someone introduces multiple inputs or outputs

    // any other cleanup necessary?
    return ffparams;
}

List<string> ProcessArguments(out bool tscopy, out string ext, out string move_dir, out string ffparams)
{
    // PROCESS ARGUMENTS
    ext = "";
    ffparams = "";
    move_dir = "";
    tscopy = false;
    string? rgx = null;
    string? input = null;
    for (int i = 0; i < args.Length; i++)
    {
        var a = args[i];
        if (input == null)
        {
            if (a == "-tscopy") tscopy = true;
            else if (a == "-rgx")
            {
                if (args.Length <= i + 1)
                    throw new ArgumentException("Missing argument for -rgx");

                rgx = args[++i];
            }
            else if (a == "-ext")
            {
                if (args.Length <= i + 1)
                    throw new ArgumentException("Missing argument for -ext");

                ext = args[++i];
                if (!ext.StartsWith("."))
                    ext = "." + ext;
            }
            else if (a == "-move")
            {
                if (args.Length <= i + 1)
                    throw new ArgumentException("Missing argument for -move");

                move_dir = args[++i];
                if (!Directory.Exists(move_dir))
                    throw new ArgumentException("Move directory does not exist");
            }
            else
            {
                if (string.IsNullOrEmpty(a))
                    throw new ArgumentException("Input file or directory is missing");

                input = a;
            }
        }
        else
        {
            ffparams = string.Join(" ", args[i..]);
            break;
        }
    }

    // COLLECT INPUT FILES
    List<string> input_files = new();
    if (File.Exists(input))
    {
        input_files.Add(input);
    }
    else if (Directory.Exists(input))
    {
        var files = Directory.GetFiles(input);
        if (rgx != null) files = files.Where(f => Regex.IsMatch(f, rgx)).ToArray();
        input_files.AddRange(files);
    }
    else
    {
        throw new ArgumentException("Input file or directory does not exist");
    }

    if (input_files.Count == 0)
    {
        throw new ArgumentException("No input files found");
    }

    return input_files;
}



