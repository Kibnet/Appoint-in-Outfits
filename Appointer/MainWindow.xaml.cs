using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Appointer.Properties;

namespace Appointer
{

	public class Person
	{
		public string Family = "";
		public string Name = "";
		public string FatherName = "";
		public string All = "";
		public bool Regular;

		public static Person CreatePerson(string personinfo, bool regular = true)
		{
			var person = new Person();
			var match = Regex.Match(personinfo, @"(?<fam>[А-ЯЁ]+)\s+(?<nam>[А-ЯЁ])\.(?<fna>[А-ЯЁ])\.");
			if (!match.Success)
			{
				match = Regex.Match(personinfo, @"(?<fam>[А-ЯЁ]+)\s+(?<nam>[А-ЯЁа-яё]+)\s+(?<fna>[А-ЯЁа-яё]+),");
			}
			if (!match.Success)
				return null;
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
		const string outfitspath = "Наряды.txt";
		const string datelpath = "Дательный.txt";
		const string roditpath = "Родительный.txt";

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

				if (!File.Exists(outfitspath))
					File.Create(outfitspath).Close();

				if (!File.Exists(datelpath))
					File.Create(datelpath).Close();

				if (!File.Exists(roditpath))
					File.Create(roditpath).Close();

				var outfits = File.ReadAllLines(outfitspath);
				var all = File.ReadAllLines(roditpath).Select(personinfo => Person.CreatePerson(personinfo, true)).Where(person => person != null).ToList();
				all.AddRange(File.ReadAllLines(datelpath).Select(personinfo => Person.CreatePerson(personinfo, false)).Where(person => person != null));

				//var outfits = Settings.Default.Outfits.Cast<string>().ToList();
				//var all = Settings.Default.Persons.Cast<string>().Select(personinfo => Person.CreatePerson(personinfo, true)).Where(person => person != null).ToList();
				//all.AddRange(Settings.Default.Comendas.Cast<string>().Select(personinfo => Person.CreatePerson(personinfo, false)).Where(person => person != null));

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
						if (fields.Length == outfits.Length + 1)
						{
							var day = SelectCalendar.DisplayDate.AddDays(-SelectCalendar.DisplayDate.Day);
							var dd = 1;
							int.TryParse(fields[0], out dd);
							day = day.AddDays(dd);
							var fdate = day.Month == day.AddDays(1).Month ? day.Day.ToString() : (day.Year == day.AddDays(1).Year ? day.ToString("d MMMM") : day.ToString("d MMMM yyyy") + " г.");
							var unity = day.Day == 2 ? "со" : "с";
							var date = string.Format("{0}) {3} {1} на {2} г.:", lett, fdate, day.AddDays(1).ToString("d MMMM yyyy"), unity);
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
								if (personinfo == null)
								{
									MessageBox.Show(date + "\n" + outfits[i - 1] + "\nСотрудник " + field + " не найден.", "Неверные данные");
									continue;	
								}
								var candids =
										all.Where(
											person => person.Name.StartsWith(personinfo.Name) &&
												person.FatherName.StartsWith(personinfo.FatherName)).ToArray();
								if (candids.Length == 0)
								{
									MessageBox.Show(date + "\n" + outfits[i - 1] + "\nСотрудник с инициалами " + personinfo.Name + "." + personinfo.FatherName + ". не найден.", "Неверные данные");
									return;
								}
								var del = 0;
								while (personinfo.Family.Length > 0)
								{
									var candidates = candids.Where(person => person.Family.StartsWith(personinfo.Family)).ToArray();
									if (candidates.Length == 1)
									{
										var add = candidates[0];
										if (add.Regular)
											appointpers.AppendLine(string.Format("- {0} – {1};", outfits[i - 1], add.All));
										else
											appointcoms.AppendLine(string.Format("- {0} – {1};", outfits[i - 1], add.All));
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
												appointpers.AppendLine(string.Format("- {0} – {1};", outfits[i - 1], add.All));
											else
												appointcoms.AppendLine(string.Format("- {0} – {1};", outfits[i - 1], add.All));
											break;
										}
									}
									personinfo.Family = personinfo.Family.Remove(personinfo.Family.Length - 1);
									del++;
									if (del > 4)
									{
										MessageBox.Show(date + "\n" + outfits[i - 1] + "\nСотрудник " + field + " не найден.", "Неверные данные");
										return;
									}
								}
							}
						}
						else
						{
							MessageBox.Show("В настройках задано " + outfits.Length + " нарядов, следовательно надо скопировать в буфер обмена столькоже нарядов + первый столбец с числом месяца. А сейчас скопировано " + fields.Length + " столбцов.", "Неверные данные");
							return;
						}
						lett++;
					}

					var osn = "Основание: график наряда охраны на " + SelectCalendar.DisplayDate.ToString("MMMM yyyy").ToLower() + " года.";
					appointpers = appointpers.Replace(';', '.', appointpers.Length - 10, 10);
					appointcoms = appointcoms.Replace(';', '.', appointcoms.Length - 10, 10);
					appointpers.AppendLine(osn);
					appointcoms.AppendLine(osn);
					appointpers.Append(appointcoms);
					Clipboard.Clear();
					Clipboard.SetText(appointpers.ToString());
					StatusLabel.Text = "Текст приказа о назначении нарядов скопирован в буфер обмена, вставляйте в Word.";
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message + "\n" + exception.StackTrace, "Исключительный случай");
			}
		}
		
		private void SetList(object sender, string path, string desc)
		{
			if (!File.Exists(path))
				File.Create(path).Close();
			StatusLabel.Text = "Скопируйте часть графика в буфер обмена и нажмите кнопку \"Назначить\"";
			var setter = new ConfigList(File.ReadAllLines(path));
			setter.Title = ((Button) sender).Content as string;
			setter.Description.Text = desc;
			setter.ShowDialog();
			if (setter.Saved == true)
			{
				File.WriteAllLines(path, setter.OutCollection);
			}
		}

		private void SetOutfits(object sender, RoutedEventArgs e)
		{
			SetList(sender, outfitspath, "Названия нарядов в приказе, в том же порядке что и в графике нарядов (отвечает на вопрос 'Кем заступает?')");
		}

		private void SetComendas(object sender, RoutedEventArgs e)
		{
			SetList(sender, datelpath, "Сотрудники в дательном падеже в формате - \nзвание ФАМИЛИЯ Имя Отчество (или ИНИЦИАЛЫ), должность (отвечает на вопрос 'Кому заступить на службу?')");
		}

		private void SetPersons(object sender, RoutedEventArgs e)
		{
			SetList(sender, roditpath, "Сотрудники в родительном падеже в формате - \nзвание ФАМИЛИЯ Имя Отчество (или ИНИЦИАЛЫ), должность (отвечает на вопрос 'Кого назначить в наряд?')");
		}
	}
}
