using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Widgets;
using FS = OpenRA.FileSystem.FileSystem;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	class LocalizationLogic : ChromeLogic
	{
		readonly IEnumerable<string> languages;
		readonly List<MiniYamlNode> yaml;
		readonly ButtonWidget quitButton;
		readonly LabelWidget desk;
		readonly LabelWidget title;
		readonly LabelWidget langLabel;

		[ObjectCreator.UseCtor]
		public LocalizationLogic(Widget widget, ModData modData, Action continueLoading)
		{
			var panel = widget.Get("LOCALIZATION_PANEL");
			languages = modData.Languages;

			if (modData.Manifest.Translations.Any())
			{
				yaml = MiniYaml.Load(modData.ModFiles, modData.Manifest.Translations, null);
			}

			var languageDropDownButton = panel.Get<DropDownButtonWidget>("LANGUAGE_DROPDOWNBUTTON");
			languageDropDownButton.OnMouseDown = _ => ShowLanguageDropdown(languageDropDownButton);
			languageDropDownButton.Text = FieldLoader.Translate(Game.Settings.Graphics.DefaultLanguage);

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
			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (o, itemTemplate) =>
			{
				var item = ScrollItemWidget.Setup(itemTemplate,
					() => Game.Settings.Graphics.Language == o,
					() =>
					{
						Game.Settings.Graphics.Language = o;
						var selectedTranslations = new Dictionary<string, string>();
						var defaultTranslations = new Dictionary<string, string>();
						foreach (var y in yaml)
						{
							if (y.Key == Game.Settings.Graphics.Language)
								selectedTranslations = y.Value.ToDictionary(my => my.Value ?? "");
							else if (y.Key == Game.Settings.Graphics.DefaultLanguage)
								defaultTranslations = y.Value.ToDictionary(my => my.Value ?? "");
						}

						foreach (var tkv in defaultTranslations)
						{
							if (selectedTranslations.ContainsKey(tkv.Key))
								continue;
							selectedTranslations.Add(tkv.Key, tkv.Value);
						}

						quitButton.Text = selectedTranslations["LOCALIZATION_CONTINUE_BUTTON"];
						desk.Text = selectedTranslations["LOCALIZATION_DESC"].Replace("\\n", "\n");
						title.Text = selectedTranslations["LOCALIZATION_TITLE"];
						langLabel.Text = selectedTranslations["LOCALIZATION_LABEL"];
						dropdown.Text = selectedTranslations[o];
					});

				var text = FieldLoader.Translate(o);
				item.Get<LabelWidget>("LABEL").GetText = () => text;
				return item;
			};

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 500, languages, setupItem);
			return true;
		}
	}
}
