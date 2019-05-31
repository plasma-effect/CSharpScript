using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Script
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Output(Properties.Settings.Default.DefautCode, this.sourceEditor);
        }

        bool flag = false;
        private void SystemMessage(string str)
        {
            this.programMessageBox.Text = str;
        }
        private void ScriptMessage(string str)
        {
            this.scriptMessageBox.Text = str;
        }
        private void Output(string str, RichTextBox richTextBox)
        {
            var flowDoc = new FlowDocument();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(str);
            flowDoc.Blocks.Add(paragraph);
            richTextBox.Document = flowDoc;
        }
        public class Command
        {
            Func<string> input;
            Action<string> cout;
            Action<string> coutEndl;
            Action<string> message;

            public Command(Func<string> input, Action<string> cout, Action<string> coutEndl, Action<string> message)
            {
                this.input = input;
                this.cout = cout;
                this.coutEndl = coutEndl;
                this.message = message;
            }
            public void Write(object obj)
            {
                this.cout(obj.ToString());
            }
            public void WriteLine()
            {
                this.coutEndl("");
            }
            public void WriteLine(string str)
            {
                this.coutEndl(str);
            }
            public void WriteLine(object obj)
            {
                this.coutEndl(obj.ToString());
            }
            public void WriteLine<T>(IEnumerable<T> ts)
            {
                using(var ite = ts.GetEnumerator())
                {
                    if (ite.MoveNext())
                    {
                        Write(ite.Current.ToString());
                        while (ite.MoveNext())
                        {
                            Write(" ");
                            Write(ite.Current.ToString());
                        }
                    }
                }
                this.coutEndl("");
            }
            public void Message(string str)
            {
                this.message(str);
            }
            public string ReadLine()
            {
                return this.input();
            }
            public T[] ReadArray<T>(Func<string,T> parser)
            {
                return this.input().Split(' ').Select(parser).ToArray();
            }
            public IEnumerable<int> Range(int min, int max, int step = 1)
            {
                for (var i = min; i < max; i += step)
                {
                    yield return i;
                }
            }
            public IEnumerable<long> Range(long min, long max, long step = 1)
            {
                for(var i = min; i < max; i += step)
                {
                    yield return i;
                }
            }
        }

        private string GetString(RichTextBox richTextBox)
        {
            var rng = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return rng.Text;
        }

        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.flag)
            {
                this.flag = true;
                var stopwatch = new Stopwatch();
                var state = "";
                try
                {
                    var sstream = new StringReader(GetString(this.standardInputEditor));
                    var builder = new StringBuilder();
                    CSharpScript.RunAsync(GetString(this.sourceEditor), globals: new Command(
                        () => sstream.ReadLine(),
                        s => builder.Append(s),
                        s => builder.AppendLine(s),
                        ScriptMessage)).Wait();
                    Output(builder.ToString(), this.standardOutput);
                    state = "正常に終了しました。";
                }
                catch (Exception exp)
                {
                    Output(exp.Message, this.standardOutput);
                    state = "異常終了しました。";
                }
                finally
                {
                    stopwatch.Stop();
                    this.flag = false;

                    var sec = stopwatch.ElapsedMilliseconds / 1000;
                    var mill = stopwatch.ElapsedMilliseconds % 1000;
                    SystemMessage($"{state}実行時間:{sec}.{mill:000}ms");
                }
            }
            else
            {
                SystemMessage("プログラムを実行中です。");
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                DefaultExt = ".cs",
                Filter = "CSharp File(.cs)|*.cs"
            };
            if (sfd.ShowDialog() == true)
            {
                var filename = sfd.FileName;
                try
                {
                    using (var fs = new StreamWriter(filename))
                    {
                        fs.Write(GetString(this.sourceEditor));
                    }
                    SystemMessage($"{filename}に保存しました。");
                }
                catch(Exception exp)
                {
                    SystemMessage(exp.Message);
                }
            }
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                DefaultExt = ".cs",
                Filter = "CSharp File(.cs)|*.cs"
            };
            if (ofd.ShowDialog() == true)
            {
                var filename = ofd.FileName;
                try
                {
                    using(var fs=new StreamReader(filename))
                    {
                        Output(fs.ReadToEnd(), this.sourceEditor);
                    }
                    SystemMessage($"{filename}を開きました。");
                }
                catch(Exception exp)
                {
                    SystemMessage(exp.Message);
                }
            }
        }

        private void DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("デフォルトのソースコードにしますか？", "確認", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.DefautCode = GetString(this.sourceEditor);
            }
        }
    }
}
