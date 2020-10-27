using System;
using System.IO;
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

            inputStream = Console.OpenStandardInput();
            Console.SetIn(new StreamReader(inputStream));

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
                    FileName = "git",
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                },
            };

            var command = "show-ref --head -d";
            process.StartInfo.Arguments = command;
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
            process.Close();
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
