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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Appointer.Properties;

namespace Appointer
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Appoint(object sender, RoutedEventArgs e)
		{
			try
			{
				var persons = Settings.Default.Persons.Cast<string>().ToList();
				var comendas = Settings.Default.Comendas.Cast<string>().ToList();
				var outfits = Settings.Default.Outfits.Cast<string>().ToList();


				var appointpers = new StringBuilder("1. В наряд охраны назначить:\r\n");
				var appointcoms = new StringBuilder("2. Заступить на службу:\r\n");

				if (Clipboard.ContainsText())
				{
					var pasted = Clipboard.GetText();
					var lines = pasted.Split('\n').Where(s => s != "").ToArray();
					char lett = 'а';
					foreach (var line in lines)
					{
						var fields = line.Replace("\r", "").Split('\t');
						if (fields.Length == outfits.Count + 1)
						{
							var day = SelectCalendar.DisplayDate.AddDays(-SelectCalendar.DisplayDate.Day);
							var dd = 1;
							int.TryParse(fields[0], out dd);
							day = day.AddDays(dd);
							var date = string.Format("{0}) с {1} на {2} г.:", lett, day.Day, day.AddDays(1).ToString("d MMMM yyyy"));

							appointpers.AppendLine(date);
							appointcoms.AppendLine(date);

							for (int i = 1; i < fields.Length; i++)
							{
								var field = fields[i];
								var inic = field.Split(' ').LastOrDefault();
								var pinic = persons.Where(s => inic != null && s.Contains(inic)).ToArray();
								var cinic = comendas.Where(s => inic != null && s.Contains(inic)).ToArray();
								while (field.Length > 0)
								{
									var pers = pinic.Where(s => s.Contains(field)).ToArray();
									var coms = cinic.Where(s => s.Contains(field)).ToArray();
									if (pers.Any())
									{
										appointpers.AppendLine("- " + outfits[i - 1] + " – " + pers.FirstOrDefault());
										break;
									}
									if (coms.Any())
									{
										appointcoms.AppendLine("- " + outfits[i - 1] + " – " + coms.FirstOrDefault());
										break;
									}
									field = field.Remove(field.Length - 1);
								}
							}
						}
						lett++;
					}

					var osn = "Основание: график наряда охраны на " + SelectCalendar.DisplayDate.ToString("MMMM yyyy").ToLower() + " года.";
					appointpers.AppendLine(osn);
					appointcoms.AppendLine(osn);
					Clipboard.SetText(appointpers.ToString() + appointcoms.ToString());
					StatusLabel.Text = "Текст приказа о назначении нарядов скопирован в буфер обмена, вставляйте в Word.";
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Исключительный случай");
			}
		}
	}
}
