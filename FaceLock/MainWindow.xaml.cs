using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management.Automation;


namespace SystemTrayApp.WPF
{
    public partial class MainWindow : Window
    {
        PowerShell powerShell = PowerShell.Create();
        FaceVerify faceVerify = new FaceVerify();
        public MainWindow()
        {
            InitializeComponent();
            faceVerify.run();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var pass = (string)PromptDialog.Dialog.Prompt("Password", "Input your origin password", inputType: PromptDialog.Dialog.InputType.Password);
            if (pass.Equals(FaceVerify.true_pass))
            {
                pass = (string)PromptDialog.Dialog.Prompt("Password", "Input your changed password", inputType: PromptDialog.Dialog.InputType.Password);
                var result = await PowerShell.Create().AddScript($"echo {pass} > {FaceVerify.pass_path}").InvokeAsync();
                foreach (var i in result)
                {
                    Console.WriteLine(i);
                }
            }
            else
            {
                MessageBox.Show("Incorrect Password");
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                Console.WriteLine(openFileDlg.FileName);
                await powerShell.AddScript($"cp \"{openFileDlg.FileName}\"  -Destination {FaceVerify.lock_path} ").InvokeAsync();
                Console.WriteLine("Photo Reset!");
            }

        }
    }
}
