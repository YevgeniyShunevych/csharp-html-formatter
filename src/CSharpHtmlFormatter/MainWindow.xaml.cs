using System;
using System.Windows;

namespace CSharpHtmlFormatter
{
    public partial class MainWindow : Window
    {
        private readonly HtmlFormatter htmlFormatter = new HtmlFormatter();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = null;

            string source = Clipboard.GetText();
            string formattedHtml = null;

            try
            {
                formattedHtml = htmlFormatter.Transform(source);
            }
            catch (Exception exception)
            {
                ResultTextBox.Text = "Failed to format HTML:" + Environment.NewLine + exception.ToString(); ;
                return;
            }

            ResultTextBox.Text = formattedHtml;
            Clipboard.SetText(formattedHtml);
        }
    }
}
