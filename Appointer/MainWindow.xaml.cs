using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Appointer.Properties;

namespace Appointer
{

	public class Person
	{
		public string Family;
		public string Name;
		public string FatherName;
		public string All;
		public bool Regular;

		public static Person CreatePerson(string personinfo, bool regular = true)
		{
			var person = new Person();
			var match = Regex.Match(personinfo, @"(?<fam>[А-Я]+)\s+(?<nam>[А-Я])\.(?<fna>[А-Я])\.");
			if (!match.Success)
			{
				match = Regex.Match(personinfo, @"(?<fam>[А-Я]+)\s+(?<nam>[А-Яа-я]+)\s+(?<fna>[А-Яа-я]+),");
			}
			if (!match.Success) return null;
			person.Family = match.Groups["fam"].Value;
			person.Name = match.Groups["nam"].Value;
			person.FatherName = match.Groups["fna"].Value;
			person.All = personinfo;
			person.Regular = regular;
			return person;
		}

		public override string ToString()
		{
			return All;
		}
	}

	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent(); 
			var ass = Assembly.GetExecutingAssembly().FullName;
			Title += " " + ass.Split(',')[1].Split('=')[1];
		}

		private void Appoint(object sender, RoutedEventArgs e)
		{
			try
			{
				var persons = Settings.Default.Persons.Cast<string>().ToList();
				var comendas = Settings.Default.Comendas.Cast<string>().ToList();
				var outfits = Settings.Default.Outfits.Cast<string>().ToList();

				var all = (from string personinfo in Settings.Default.Persons select Person.CreatePerson(personinfo, true)).ToList();
				all.AddRange(from string personinfo in Settings.Default.Comendas select Person.CreatePerson(personinfo, false));

				var appointpers = new StringBuilder("1. В наряд охраны назначить:\r\n");
				var appointcoms = new StringBuilder("2. Заступить на службу:\r\n");

				if (Clipboard.ContainsText())
				{
					var pasted = Clipboard.GetText();
					var lines = pasted.ToUpper().Split('\n').Where(s => s != "").ToArray();
					char lett = 'а';
					foreach (var line in lines)
					{
						var fields = line.Replace("\r", "").Split('\t');
						if (fields.Length <= outfits.Count + 1)
						{
							var day = SelectCalendar.DisplayDate.AddDays(-SelectCalendar.DisplayDate.Day);
							var dd = 1;
							int.TryParse(fields[0], out dd);
							day = day.AddDays(dd);
							var fdate = day.Month == day.AddDays(1).Month ? day.Day.ToString() : (day.Year == day.AddDays(1).Year ? day.ToString("d MMMM") : day.ToString("d MMMM yyyy")+" г.");
							var unity = day.Day == 2? "со" : "с";
							var date = string.Format("{0}) {3} {1} на {2} г.:", lett, fdate, day.AddDays(1).ToString("d MMMM yyyy"),unity);
							appointpers.AppendLine(date);
							appointcoms.AppendLine(date);

							for (int i = 1; i < fields.Length; i++)
							{
								var field = fields[i];
								if (string.IsNullOrWhiteSpace(field))
								{
									continue;
								}
								var personinfo = Person.CreatePerson(field);
								while (personinfo != null && personinfo.Family.Length > 0)
								{
									var candidates =
										all.Where(
											person =>
												person.Family.StartsWith(personinfo.Family) && 
												person.Name.StartsWith(personinfo.Name) &&
												person.FatherName.StartsWith(personinfo.FatherName)).ToArray();
									if (candidates.Length == 1)
									{
										var add = candidates[0];
										if (add.Regular)
											appointpers.AppendLine("- " + outfits[i - 1] + " – " + add.All);
										else
											appointcoms.AppendLine("- " + outfits[i - 1] + " – " + add.All);
										break;
									}
									if (candidates.Length > 1)
									{
										var fixer = new Fixer();
										fixer.DateBlock.Text = date;
										fixer.NarBlock.Text = outfits[i - 1];
										fixer.PersonBox.ItemsSource = candidates;
										fixer.ShowDialog();
										if (fixer.PersonBox.SelectedItem != null)
										{
											var add = (Person)(fixer.PersonBox.SelectedItem);
											if (add.Regular)
												appointpers.AppendLine("- " + outfits[i - 1] + " – " + add.All);
											else
												appointcoms.AppendLine("- " + outfits[i - 1] + " – " + add.All);
											break;
										}
									}
									personinfo.Family = personinfo.Family.Remove(personinfo.Family.Length - 1);
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

		private void SetOutfits(object sender, RoutedEventArgs e)
		{
			StatusLabel.Text = "Скопируйте часть графика в буфер обмена и нажмите кнопку \"Назначить\"";
			var setter = new ConfigList(Settings.Default.Outfits);
			setter.Description.Text = "Названия нарядов в приказе, в том же порядке что и в графике нарядов (отвечает на вопрос 'Кем заступает?')";
			setter.ShowDialog();
			if (setter.Saved == true)
			{
				Settings.Default.Outfits = setter.OutCollection;
				Settings.Default.Save();
			}
		}

		private void SetComendas(object sender, RoutedEventArgs e)
		{
			StatusLabel.Text = "Скопируйте часть графика в буфер обмена и нажмите кнопку \"Назначить\"";
			var setter = new ConfigList(Settings.Default.Comendas);
			setter.Description.Text = "Сотрудники комендантского отделения в формате - \nзвание ФАМИЛИЯ ИНИЦИАЛЫ, должность (отвечает на вопрос 'Кому заступить на службу?')";
			setter.ShowDialog();
			if (setter.Saved == true)
			{
				Settings.Default.Comendas = setter.OutCollection;
				Settings.Default.Save();
			}
		}

		private void SetPersons(object sender, RoutedEventArgs e)
		{
			StatusLabel.Text = "Скопируйте часть графика в буфер обмена и нажмите кнопку \"Назначить\"";
			var setter = new ConfigList(Settings.Default.Persons);
			setter.Description.Text = "Сотрудники не из комендантского отделения в формате - \nзвание ФАМИЛИЯ ИНИЦИАЛЫ, должность (отвечает на вопрос 'Кого назначить в наряд?')";
			setter.ShowDialog();
			if (setter.Saved == true)
			{
				Settings.Default.Persons = setter.OutCollection;
				Settings.Default.Save();
			}
		}
	}
}
