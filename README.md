# FFmpegBatch

A simple CLI tool to batch convert files using FFmpeg.

## Why
I don't want to write a script every time, and I want a nice overview of all files that are being processed

## Features
- Convert multiple files at once **with single command**
- **Filter files** based with Regex pattern
- Move original files to another location after conversion
- **Copy timestamps** of original file to converted file

## Usage
```
[-tscopy] [-move <directory>] [-rgx <pattern>] [-ext <output_extension>] <input_directory|input_file> <ffmpeg_params>
```
- `tscopy` flag enables copying timestamps of original file to converted file
- `move` flag defines the directory where original files will be moved after conversion
- `rgx` flag defines the regex pattern to filter files in given directory
- `ext` flag defines the output extension of the converted files

### Examples
- `ffmpegBatch.exe -ext mp3 .`
    - converts all files in current directory to MP3 files
- `ffmpegBatch.exe -tscopy -rgx ".*?\.wav" -ext opus . -b:a 128k`
    - converts all WAV files in current directory to OPUS files with 128kbps audio bitrate
- `ffmpegBatch.exe -move "C://original" -ext webm "C://videos" -c:v libsvtav1 -crf 40 -vf scale=-1:1080`
    - converts all files in `C://videos` directory to WEBM files with AV1 codec and 1080p resolution, and moves original files to `C://original` directory

## Publish
You can manually publish the project to `out` directory by running:
```
dotnet publish -o out
```
By specifying a runtime identifier, you can publish to different platforms, e.g.:
```
dotnet publish -o out -r linux-x64
```
Other specifiers:
- `win-x64`
- `osx-x64`
- `linux-x64`
- `linux-arm`
- `linux-arm64`
- `osx-arm64`
- ...

## Screenshots

![image](https://assets.cryshana.me/WjVcYRFNyYSg.png)
![image](https://assets.cryshana.me/b40p6IrZTxW2.png)

### Error example
In case of non-zero FFmpeg exit code, the output is fully displayed under the line like so:

![image](https://assets.cryshana.me/VHbKxzkrFfo0.png)
