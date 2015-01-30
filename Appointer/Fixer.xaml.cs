using System;
using System.Collections.Generic;
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
	/// Логика взаимодействия для Fixer.xaml
	/// </summary>
	public partial class Fixer : Window
	{
		public Fixer()
		{
			InitializeComponent();
		}

		private void PersonBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Close();
		}
	}
}
