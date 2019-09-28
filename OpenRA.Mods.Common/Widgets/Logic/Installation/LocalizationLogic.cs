using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fluent.Net;
using OpenRA.FileSystem;
using OpenRA.Widgets;
using FS = OpenRA.FileSystem.FileSystem;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	class LocalizationLogic : ChromeLogic
	{
		readonly ButtonWidget quitButton;
		readonly LabelWidget desk;
		readonly LabelWidget title;
		readonly LabelWidget langLabel;
		readonly ModData modData;
		Dictionary<string, List<string>> dicc;
		readonly MessageContext ctx;

		[ObjectCreator.UseCtor]
		public LocalizationLogic(Widget widget, ModData modData, Action continueLoading)
		{
			var panel = widget.Get("LOCALIZATION_PANEL");
			this.modData = modData;

			if (modData.Manifest.Translations.Any())
			{
				List<MiniYamlNode> yaml = MiniYaml.Load(modData.ModFiles, modData.Manifest.Translations, null);
				dicc = new Dictionary<string, List<string>>();
				foreach (string tr in modData.Manifest.Translations)
				{
					string fileName = FS.ResolveAssemblyPath(tr, modData.Manifest, null);
					string key = Path.GetFileNameWithoutExtension(fileName);
					if (File.Exists(fileName) && (!string.IsNullOrEmpty(key)))
					{
						if (!dicc.ContainsKey(key))
						{
							dicc.Add(key, new List<string>());
						}

						dicc[key].Add(fileName);
					}
				}
			}

			ctx = FieldLoader.CreateContextUI("", true);

			var languageDropDownButton = panel.Get<DropDownButtonWidget>("LANGUAGE_DROPDOWNBUTTON");
			languageDropDownButton.OnMouseDown = _ => ShowLanguageDropdown(languageDropDownButton);
			languageDropDownButton.Text = FieldLoader.TranslateUI(Game.Settings.Graphics.DefaultLanguage);

			quitButton = panel.Get<ButtonWidget>("CONTINUE_BUTTON");
			quitButton.OnClick = () =>
			{
				Game.Settings.Save();
				continueLoading();
			};
			desk = panel.Get<LabelWidget>("LANGUAGE_DESC");
			title = panel.Get<LabelWidget>("TITLE");
			langLabel = panel.Get<LabelWidget>("LANGUAGE_LABEL");
		}

		public bool ShowLanguageDropdown(DropDownButtonWidget dropdown)
		{
			Func<System.Globalization.CultureInfo, ScrollItemWidget, ScrollItemWidget> setupItem = (o, itemTemplate) =>
			{
				string lang = o.EnglishName;
				var item = ScrollItemWidget.Setup(itemTemplate,
					() =>
					Game.Settings.Graphics.Language == lang,
					() =>
					{
						FieldLoader.LoadTranslationsUI(o.Name, dicc);
						dropdown.Text = lang;
						Game.Settings.Graphics.Language = o.Name;
						quitButton.Text = FieldLoader.TranslateUI("LOCALIZATION-CONTINUE-BUTTON");
						desk.Text = FieldLoader.TranslateUI("LOCALIZATION-DESC");
						title.Text = FieldLoader.TranslateUI("LOCALIZATION-TITLE");
						langLabel.Text = FieldLoader.TranslateUI("LOCALIZATION-LABEL");
						dropdown.Text = lang;
					});

				item.Get<LabelWidget>("LABEL").GetText = () => lang;
				return item;
			};

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 500, modData.Cultures.Values, setupItem);
			return true;
		}
	}
}
