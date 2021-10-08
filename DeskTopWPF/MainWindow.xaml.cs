using DeskTopWPF.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace DeskTopWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Navigation> navigations = new List<Navigation>() { new Navigation { Name = "Home", Display = "Главное меню" } };
        private string path =System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private string filename ="settings.xml";
        private static Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            spMenu.Children.Clear();
            try
            {
                settings = Service.ReadSettings(System.IO.Path.Combine(path,filename));
            
                CreateMenu(settings.MenuItems);
            }
            catch (Exception ex)
            {
                showMessage(MessageTypes.Error, ex.Message);
            }
         
        }

        private void AddButtonBack()
        {
            Button button = new Button();
            button.Name = "goBack";
            button.Content = "<< Назад";
            button.Click += back_Click;
            button.Style = Resources["backButton"] as Style;
            spMenu.Children.Add(button);
        }

        private void AddNavigationPanel()
        {
            wpNavigation.Children.Clear();
            foreach (Navigation navigation in navigations)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Name = "tb" + navigation.Name;
                textBlock.Text = navigation.Display + " >> ";
                textBlock.Style = Resources["navigationFont"] as Style;
                wpNavigation.Children.Add(textBlock);
            }
            spMenu.Children.Add(wpNavigation);
        }

        private void CreateMenu(IEnumerable<Item> items)
        {
            spMenu.Children.Clear();
            //Visible
            if (navigations.Count() > 1)
            {
                AddButtonBack();

                AddNavigationPanel();

                wpNavigation.Visibility = Visibility.Visible;
                
            }
            else
            {
                wpNavigation.Visibility = Visibility.Collapsed;
            }

            foreach (Item item in items)
            {
                if ((bool)item.Visible) {
                    Button button = new Button();

                    button.Content = item.Display;
                    button.Name = item.Name;
                    button.Style = Resources["MenuButton"] as Style;
                    button.Click += item_Click;
                    spMenu.Children.Add(button);
                }
            }
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            var rootName = ((Button)sender).Name;

            var root = GetItemsByName(rootName);

            //Navigation
            navigations.Add(new Navigation { Name = rootName, Display = (string)((Button)sender).Content });
          

            CreateMenu(root);

            if (!root.Any())
            {
               Item item = GetItemByName(rootName);
               if (item.Command != null)
                {

                    try
                    {
                        var  commandPath = System.IO.Path.GetDirectoryName(item.Command.FileName);
                        if (String.IsNullOrEmpty(commandPath))
                        {
                            commandPath = System.IO.Path.Combine(path, "commands", item.Command.FileName);
                        }
                        else
                        {
                            commandPath = item.Command.FileName;
                        }
                        if (System.IO.File.Exists(commandPath))
                        {
                            if (item.Command.Timeout != 0)
                            {
                                try
                                {
                                    startCommand(commandPath, item.Command.Timeout);
                                }
                                catch (Exception ex)
                                {
                                    showMessage(MessageTypes.Error, ex.Message);
                                }
                            }
                            else
                            {
                                startCommand(commandPath);
                            }
                        }
                        else
                        {
                            showMessage(MessageTypes.Error, "Файл \"" + commandPath + "\" команды \"" + item.Display + "\" не найден!");
                        }
                    }
                    catch (Exception ex)
                    {
                        showMessage(MessageTypes.Error, ex.Message);
                    }
                    
                }
                else{

                    showMessage(MessageTypes.Error, "Файл команды \""+item.Display+"\" не задан!");
                }
               
            }
           
            
           
        }



        private void startCommand(string commandfile, int timeout)
        {

            var process = new Process();
            process.StartInfo = ProcessInfo(commandfile);

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

           
               
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        { 
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout))
                    {

                        // Process completed. Check process.ExitCode here.
                        if (process.ExitCode == 0)
                        {

                            showMessage(MessageTypes.Success, output.ToString());
                        }
                        else
                        {

                            showMessage(MessageTypes.Error, error.ToString());
                        }

                    }
                    else
                    {
                        // Timed out.
                        showMessage(MessageTypes.Error, "Время ожидания окончания операции истекло!");
                    }
                
            
        }

        private ProcessStartInfo ProcessInfo(string commandfile)
        {
            var ProcessInfo = new ProcessStartInfo(@"cmd.exe", "/c " + "\""+commandfile+ "\"");
            ProcessInfo.RedirectStandardOutput = true;
            ProcessInfo.UseShellExecute = false;
            ProcessInfo.CreateNoWindow = true;
            //ProcessInfo.WorkingDirectory = @"D:\";
            //ProcessInfo.Verb = "runas";
            ProcessInfo.StandardErrorEncoding = Encoding.GetEncoding(866);
            ProcessInfo.StandardOutputEncoding = Encoding.GetEncoding(866);

            //
            ProcessInfo.RedirectStandardOutput = true;
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardInput = true;

            return ProcessInfo;

        } 

        private void startCommand(string commandfile)
        {
            try
            {
                var process=Process.Start(ProcessInfo(commandfile));
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                  
                if (process.ExitCode == 0){

                    showMessage(MessageTypes.Success, output);
                }
                else{

                    showMessage(MessageTypes.Error, error);
                }
                process.Close();
            }
            catch (Exception e)
            {
                showMessage(MessageTypes.Error,e.Message);
            }
        }


    private void back_Click(object sender, RoutedEventArgs e)
        {
            navigations.Remove(navigations.Last());

            CreateMenu(GetItemsByName(navigations.Last().Name));

        }

        private IEnumerable<Item> GetItemsByName(string name)
        {
            
            if (name == navigations.First().Name)
                return settings.MenuItems;

            return settings.MenuItems.Concat(settings.MenuItems.SelectManyRecursive(parent => parent.MenuItems)).Where(w => w.Name == name).First().MenuItems;
            
        }

        private Item GetItemByName(string name)
        {
           return  settings.MenuItems.Concat(settings.MenuItems.SelectManyRecursive(parent => parent.MenuItems)).Where(w => w.Name == name).First();
        }

        private void showMessage(MessageTypes type, string message)
        {


            MessageControl messager = new MessageControl();
            if (type == MessageTypes.Error)
             {
                messager.Title = "Ошибка!";
                messager.Text = message;
                messager.Background = (Brush)new BrushConverter().ConvertFrom("#CB2400");
                messager.Image = "pack://application:,,,/Icons/error-icon.png";
                messager.Margin = new Thickness(2);

            }
             else
             {
                messager.Title = "Операция успешно выполнена!";
                messager.Text = message;
                messager.Background = (Brush)new BrushConverter().ConvertFrom("#009337");
                messager.Image = "pack://application:,,,/Icons/success-icon.png";
                messager.Margin = new Thickness(2);

            }

           
            
            
            spMenu.Children.Add(messager);
        }

    }


   
}
