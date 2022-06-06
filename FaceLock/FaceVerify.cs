using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Management.Automation;
using System.Windows;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading;
using Windows.Media.MediaProperties;
using System.Runtime.InteropServices;

namespace SystemTrayApp.WPF
{
    internal class FaceVerify
    {
        public static bool enabled = true;
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
        public static string user_direc = Environment.GetEnvironmentVariable("USERPROFILE");
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
        public static string lock_path = user_direc + "/.cache/truth.jpg";
        public static string photo_path = user_direc + "/.cache/photo.jpeg";
        public static bool running = false;
        public static int time_span = 10000;
        public static string true_pass = "123456";
        public static string pass_path = user_direc + "/.cache/userpass.txt";
        [DllImport("user32")]
        public static extern void LockWorkStation();
        public FaceVerify()
        {
            init();
        }
        public async static void lock_computer(){
            await Task.Delay(10000);
            if(!running)
            LockWorkStation();
            Console.WriteLine(running);
        }
        public async void init()
        {

            using (var result = await PowerShell.Create().AddScript($"cat {pass_path}").InvokeAsync())
            {
                if (result.Count != 0)
                {
                    foreach(var i in result)
                    {
                        Console.Write(i);
                    }
                    true_pass = result[0].ToString();
                }
            }

        }
        public void run()
        {
            if (!running)
            {
                running = true;
                Verify();
            }

        }
        private async void Verify()
        {
            int error_times = 0;
            while (enabled == true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var capture = new MediaCapture();
                await capture.InitializeAsync();
                var fileUri = new Uri(photo_path);
                string uri = fileUri.LocalPath;
                int end = uri.LastIndexOf('\\');
                uri = uri.Substring(0, end + 1);
                string name = Path.GetFileName(fileUri.LocalPath);
                var folder = await StorageFolder.GetFolderFromPathAsync(uri);
                IStorageFile file = await folder.GetFileAsync(name);
                await capture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                Console.WriteLine("Photo taken");
                if (enabled && File.Exists(photo_path)&&File.Exists(lock_path))
                {
                    using (var result = await PowerShell.Create().AddScript($"deepface verify -img1_path {lock_path} -img2_path {photo_path} -model_name=SFace -distance_metric=euclidean_l2").InvokeAsync())
                    {
                        foreach(var i in result)
                        {
                            Console.WriteLine(i);
                        }
                        if (result.Count==0 || result[0].ToString().IndexOf("true")==-1)
                        {
                            Console.WriteLine("face errored");
                            time_span = 0;
                            error_times++;
                        }
                        else 
                            { error_times = 0; }
                        }
                }
                watch.Stop();
                if(error_times >= 3)
                {
                    running = false;
                    var task = Task.Run(lock_computer);
                    System.Windows.Forms.MessageBox.Show(new Form { TopMost = true }, "Face verify failed, please input password or computer will lock in 5 seconds!");
                    string password = (string)PromptDialog.Dialog.Prompt("Password", "Inconsistent face detected, please input your password", inputType: PromptDialog.Dialog.InputType.Password);
                    running = true;
                        Console.WriteLine(password);
                        if (password==null||!password.Equals(true_pass))
                        {
                            LockWorkStation();
                        }
                        else
                        {
                            time_span = 60000;
                        }
                }
                var ellap= time_span-watch.ElapsedMilliseconds;
                if(ellap > 0)await Task.Delay((int)ellap);
            }
            running = false;
        }
    }
}
