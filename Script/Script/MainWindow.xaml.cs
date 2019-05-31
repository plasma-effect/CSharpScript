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
        }

        bool flag = false;
        private void SystemMessage(string str)
        {
            this.programMessageBox.Text = str;
        }
        private void Output(string str)
        {
            var flowDoc = new FlowDocument();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(str);
            flowDoc.Blocks.Add(paragraph);
            this.standardOutput.Document = flowDoc;
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
                    var range = new TextRange(this.standardInputEditor.Document.ContentStart, this.standardInputEditor.Document.ContentEnd);
                    var sstream = new StringReader(range.Text);
                    range = new TextRange(this.sourceEditor.Document.ContentStart, this.sourceEditor.Document.ContentEnd);
                    var builder = new StringBuilder();
                    CSharpScript.RunAsync(range.Text, globals: new Command(
                        () => sstream.ReadLine(),
                        s => builder.Append(s),
                        s => builder.AppendLine(s),
                        SystemMessage)).Wait();
                    Output(builder.ToString());
                    state = "正常に終了しました。";
                }
                catch (Exception exp)
                {
                    Output(exp.ToString());
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
    }
}
