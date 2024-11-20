using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Forms;


class Program
{
    static string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
    static string exeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
    [STAThread]
    static void Main(string[] args)
    {
       
#if DEBUG
        if (Debugger.IsAttached)
        {
            Console.WriteLine("Debugger is attached.");
        }
        else
        {
            Console.WriteLine("No debugger attached. Waiting...");
            Console.WriteLine("Atache debugger, after that, hit enter key to continue...");
            Console.ReadLine(); 
        }
#endif

        if (args.Length == 0)
        {
            Console.WriteLine("No parameter.");
            return;
        }

        string urlScheme = args[0];
        Console.WriteLine($"Got param: {urlScheme}");

        try
        {
            (string filePath, string programName) = ParseUrlScheme(urlScheme);
            Console.WriteLine($"Decoded filepath: {filePath}");
            Console.WriteLine($"Program to execute: {programName}");

            string programPath = GetProgramPath(programName);
            Console.WriteLine($"Program path: {programPath}");
            
            if (filePath.StartsWith("file://")){
                filePath = filePath.Substring("file://".Length);
            }
            else {
                filePath = GetFileFromURL(filePath);
            }
            StartProgramWithFile(programPath, filePath);


            Console.WriteLine("Program completed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine($"Error: {ex.Message}");
            
        }
    }

    
    static (string? argParam, string programName) ParseUrlScheme(string urlScheme)
    {
        // delete scheme part
        int index = urlScheme.IndexOf(':');
        string payload = urlScheme.Substring(index+1);

        // split parameters
        string[] parts = payload.Split(';');
        string filePart = null;
        string execPart = null;

        foreach (string part in parts)
        {
            if (part.StartsWith("file="))
            {
                filePart = part.Substring("file=".Length);
                filePart = Uri.UnescapeDataString(filePart); 
            }
            else if (part.StartsWith("exec="))
            {
                execPart = part.Substring("exec=".Length);
                execPart = Uri.UnescapeDataString(execPart); 
            }
        }

        if (string.IsNullOrEmpty(execPart))
            throw new ArgumentException("No parameter: exec");

        return (filePart, execPart);
    }


    static string GetFileFromURL(string urlString) {
        
        string fileName   = System.IO.Path.GetFileName(urlString);
        //string extension  = System.IO.Path.GetExtension(urlString);
        //string outputFilePath = Path.Combine(Path.GetTempPath(), fileName);

        Uri uri = new Uri(urlString);
        Console.WriteLine("Fetching image from: " + uri);

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // サーバーから画像を同期的に取得
                HttpResponseMessage response = client.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();

                // ファイル名を作る
                MediaTypeHeaderValue contentType = response.Content.Headers.ContentType;
                string? mimeType = contentType?.MediaType;
                string extension = GetFileExtensionFromMimeType(mimeType);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string outputFilePath = Path.Combine(Path.GetTempPath(), "FromBrowser" + timestamp +  extension);


                string tempFilePath = "";
                if (response.IsSuccessStatusCode)
                {
                    // レスポンスの内容を取得
                    byte[] imageData = response.Content.ReadAsByteArrayAsync().Result;

                    // ローカル一時ファイルに保存
                    tempFilePath = outputFilePath;
                    File.WriteAllBytes(tempFilePath, imageData);
                }
                else
                {
                    Console.WriteLine("Failed to fetch image. HTTP Status Code: " + response.StatusCode);
                }
                    return tempFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return "";
            }
        }
    }

static string GetFileExtensionFromMimeType(string mimeType)
{
    // MIMEタイプから拡張子をマッピング
    return mimeType switch
    {
        // 画像ファイル
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        "image/bmp" => ".bmp",
        "image/tiff" => ".tiff",
        "image/svg+xml" => ".svg",
        "image/x-icon" => ".ico",

        // 動画ファイル
        "video/mp4" => ".mp4",
        "video/x-msvideo" => ".avi",
        "video/x-matroska" => ".mkv",
        "video/webm" => ".webm",
        "video/quicktime" => ".mov",
        "video/x-flv" => ".flv",
        "video/mpeg" => ".mpeg",

        // 音声ファイル
        "audio/mpeg" => ".mp3",
        "audio/ogg" => ".ogg",
        "audio/wav" => ".wav",
        "audio/webm" => ".weba",
        "audio/flac" => ".flac",
        "audio/aac" => ".aac",

        // 文書ファイル
        "application/pdf" => ".pdf",
        "application/msword" => ".doc",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
        "application/vnd.ms-excel" => ".xls",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
        "application/vnd.ms-powerpoint" => ".ppt",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",

        // 圧縮ファイル
        "application/zip" => ".zip",
        "application/x-7z-compressed" => ".7z",
        "application/x-rar-compressed" => ".rar",
        "application/x-tar" => ".tar",
        "application/gzip" => ".gz",

        // テキストファイル
        "text/plain" => ".txt",
        "text/html" => ".html",
        "text/css" => ".css",
        "text/javascript" => ".js",
        "application/json" => ".json",
        "application/xml" => ".xml",
        "text/markdown" => ".md",

        // その他
        "application/octet-stream" => ".bin", // 汎用バイナリファイル
        "application/x-shockwave-flash" => ".swf",
        _ => "" // サポートされていない場合は空文字列を返す
    };
}





    static string GetProgramPath(string programName)
    {
        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Irid", "URLSchemeLancher", "config.json");
        //const string configFilePath = "config.json";

        if (!File.Exists(configFilePath))
            throw new FileNotFoundException($"Coudn't find config file.: {configFilePath}");

        // read JSON file
        string jsonString = File.ReadAllText(configFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // No care caractor case.
        };
        var config = JsonSerializer.Deserialize<Config>(jsonString, options);

        if (config == null || config.Programs == null)
            throw new ArgumentException("Invailed config file.");

        // Get program path
        var program = config.Programs.Find(p => p.Name.Equals(programName, StringComparison.OrdinalIgnoreCase));
        if (program == null)
            throw new ArgumentException($"No program on config.: {programName}");

        return program.Path;
    }

   
    static void StartProgramWithFile(string programPath, string? filePath)
    {
        filePath = filePath.Replace("://", ":/"); // to Windows format
        //if (!File.Exists(programPath))
        //    throw new FileNotFoundException($"Couldn't find program file.: {programPath}");


        ProcessStartInfo  startInfo = new ProcessStartInfo
        {
            FileName = programPath,
            Arguments = $"\"{filePath}\"",
            UseShellExecute = true
        };

        using (Process process = Process.Start(startInfo) )
        {
            //process.WaitForExit();
        }
    }
}

public class Config
{
    public List<ProgramConfig> Programs { get; set; } 
}

public class ProgramConfig
{
    public string Name { get; set; }
    public string Path { get; set; }
}