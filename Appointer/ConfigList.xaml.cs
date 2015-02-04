using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Appointer
{
	/// <summary>
	/// Логика взаимодействия для ConfigList.xaml
	/// </summary>
	public partial class ConfigList : Window
	{
		public ConfigList(string[] outfits)
		{
			InitializeComponent();

			Saved = false;

			var sb = new StringBuilder();
			foreach (var outfit in outfits)
			{
				sb.AppendLine(outfit);
			}
			Box.Text = sb.ToString();
		}

		public bool Saved { get; set; }

		//public StringCollection OutCollection { get; set; }
		public string[] OutCollection { get; set; }

		private void Save(object sender, RoutedEventArgs e)
		{
			Saved = true;
			var lines = Box.Text.Replace("\r", "").Split('\n').Where(s => !String.IsNullOrWhiteSpace(s));
			OutCollection = lines.ToArray();
			Close();
		}
	}
}
