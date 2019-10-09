using System;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TestRunShell: MonoBehaviour
{
    // Opens the Internet Explorer application.
    void OpenApplication(string myFavoritesPath)
    {
        UnityEngine.Debug.Log("path:" + myFavoritesPath);
        // Start Internet Explorer. Defaults to the home page.
        // Display the contents of the favorites folder in the browser.
        // Process.Start(myFavoritesPath);
        // Process.Start("/");
        // Process.Start("/Applications/Unity/Hub/Editor/2018.4.4f1/Documentation/en/Manual/2DAnd3DModeSettings.html");
        Process.Start("/usr/local/bin/vim", "/Users/tugame/test.txt");
    }

    // Opens urls and .html documents using Internet Explorer.
    void OpenWithArguments()
    {
        // url's are not considered documents. They can only be opened
        // by passing them as arguments.
        // Process.Start("IExplore.exe", "www.northwindtraders.com");

        // Start a Web page using a browser associated with .html and .asp files.
        // Process.Start("IExplore.exe", "C:\\myPath\\myFile.htm");
        // Process.Start("IExplore.exe", "C:\\myPath\\myFile.asp");
        Process.Start("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" ,
            "/Applications/Unity/Hub/Editor/2018.4.4f1/Documentation/en/Manual/2DAnd3DModeSettings.html");
    }

    // Uses the ProcessStartInfo class to start new processes,
    // both in a minimized mode.
    void OpenWithStartInfo()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome");
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        Process.Start(startInfo);

        startInfo.Arguments = "www.northwindtraders.com";

        Process.Start(startInfo);
    }

    private void OutPutTest()
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "/bin/ls";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.Arguments = "/";
            process.Start();
            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();


            // Write the redirected output to this application's window.
            Console.WriteLine(output);
            UnityEngine.Debug.Log("output:" + output);

            process.WaitForExit();
        }
    }

    [MenuItem("Menu/Main")]
    static void Main()
    {
        // Get the path that stores favorite links.
        string myFavoritesPath =
            Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

        TestRunShell myProcess = new TestRunShell();

        // myProcess.OpenApplication(myFavoritesPath);
        // myProcess.OpenWithArguments();
        // myProcess.OpenWithStartInfo();
        myProcess.OutPutTest();
    }

    [MenuItem("Menu/Output")]
    static void FileTest()
    {
        // 获取指定父目录的所有子目录
        DirectoryInfo[] rootDirs = new DirectoryInfo(@"/").GetDirectories();
        UnityEngine.Debug.Log("rootDirs:");
        for(int i = 0; i < rootDirs.Length; ++i)
        {
            UnityEngine.Debug.Log(rootDirs[i]);
        }

        // Write each directory name to a file.
        // StreamWriter指定相对路径时，相对路径为当前工程文件目录
        using (StreamWriter sw = new StreamWriter("CDriveDirs.txt"))
        {
            foreach (DirectoryInfo dir in rootDirs)
            {
                sw.WriteLine(dir.Name);

            }
        }

        // Read and show each line from the file.
        // string line = "";
        // using (StreamReader sr = new StreamReader("CDriveDirs.txt"))
        // {
        //     while ((line = sr.ReadLine()) != null)
        //     {
        //         Console.WriteLine(line);
        //     }
        // }
    }
}