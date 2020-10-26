using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BugReport
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputStream = Console.OpenStandardInput();
            Console.SetIn(new StreamReader(inputStream));

            ListReference();

            Console.WriteLine("Please enter something first:");
            var line = Console.ReadLine();
            Console.WriteLine($"read first: {line}");

            Console.WriteLine("Please enter something second:");
            line = ReadInput(inputStream);
            Console.WriteLine($"read second: {line}");
        }

        private static void ListReference()
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = GetShellPath(),
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                },
            };

            var command = "git show-ref --head -d";
            process.StartInfo.Arguments = GetShellCommand(command);
            process.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "repo");
            process.Start();

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Data);
            };

            process.OutputDataReceived += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }

        /// <summary>
        /// Returns the name of the shell to the operating system.
        /// </summary>
        /// <returns>The name of the shell.</returns>
        private static string GetShellPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "cmd.exe";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/bin/bash";
            }

            throw new NotSupportedException("The operating system does not support executing the command line.");
        }

        /// <summary>
        /// Returns the shell command argument.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The shell command.</returns>
        private static string GetShellCommand(string command)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"/c \"{command}\"";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"-c \"{command}\"";
            }

            throw new NotSupportedException("The operating system does not support executing the command line.");
        }

        /// <summary>
        /// Read the user input from <paramref name="inputStream"/>.
        /// </summary>
        private static string ReadInput(Stream inputStream, Encoding encoding = null)
        {
            string ret = null;
            encoding ??= Encoding.UTF8;

            // We only correct the position when we allow seek.
            long position = 0;
            if (inputStream.CanSeek)
            {
                position = inputStream.Position;
            }

            using (var reader = new StreamReader(inputStream, encoding,
                true, 128, true))
            {
                ret = reader.ReadLine();

                // We skip this step directly when it is not allowed,
                // because there will be no bugs even if you do not
                // use position correction under the console program.
                // It contains a potential problem: !5
                if (inputStream.CanSeek && !(ret is null))
                {
                    inputStream.Seek(
                        position + encoding.GetByteCount(ret.ToString()) + encoding.GetByteCount(Environment.NewLine),
                        SeekOrigin.Begin);
                }
            }

            return ret;
        }
    }
}
