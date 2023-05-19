using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ConjHacks
{
    public sealed partial class MainPage : Page
    {
        private string html;
        private string script;
        private string spanishInput;
        private string currentWords;

        private bool running = false;

        private Dictionary<string, string> ansDictionary;

        public MainPage()
        {
            this.InitializeComponent();
            RunButton.IsEnabled = false;
            Status.Text = "Ready for load...";
        }

        // Answer table parser
        private void ParseTable()
        {
            Status.Text = "Parsing table.";
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            ansDictionary = new Dictionary<string, string>();

            var rows = doc.DocumentNode.SelectNodes("//tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");
                    if (cells != null && cells.Count >= 2)
                    {
                        string english = Regex.Replace(cells[0].InnerText.Trim(), @"^""?\d+\.\s*", string.Empty);
                        string spanish = Regex.Replace(cells[1].InnerText.Trim(), @"^""?\d+\.\s*", string.Empty);
                        ansDictionary.Add(english, spanish);
                    }
                }
            }
            Thread.Sleep(1000);
            Status.Text = "Ready!";
            RunButton.IsEnabled = true;
        }

        // Startup and loop
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(1000);
            Status.Text = "Running!";
            running = true;
            while(running)
            {
                // Script to find current question
                currentWords = await WebViewConj.ExecuteScriptAsync("document.getElementById(\"question-input\").innerHTML;");
                currentWords = currentWords.Replace("\"", "");
                // Try linking to keypair
                try { spanishInput = ansDictionary[currentWords]; } catch (Exception ex) { Debug.WriteLine(ex); }
                Thread.Sleep(200);
                // Execute input script
                await WebViewConj.CoreWebView2.ExecuteScriptAsync($"document.getElementById('assignment-answer-input').value = '{spanishInput}'; var enterEvent = new KeyboardEvent(\"keydown\", {{ keyCode: 13 }});\r\ntextbox.dispatchEvent(enterEvent);");   
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get table and parse
            html = InputBox.Text;
            ParseTable();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Stops loop
            Status.Text = "Stopped.";
            running = false;
        }
    }
}
