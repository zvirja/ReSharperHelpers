using System.Diagnostics;
using AlexPovar;
using AlexPovar.ReSharperHelpers;
using AlexPovar.ReSharperHelpers.Settings;
using AlexPovar.ReSharperHelpers.VisualStudio;
using AlexPovar.ReSharperHelpers.VisualStudio.Settings;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;

namespace AlexPovar.ReSharperHelpers.VisualStudio.Settings
{
  [OptionsPage(PID, "ReSharper Helpers", typeof(MainThemedIcons.HelpersContextAction), ParentId = ToolsPage.PID)]
  public class ReSharperHelpersOptionsPage : BeSimpleOptionsPage, IOptionsPage
  {
    // ReSharper disable once InconsistentNaming
    private const string PID = "AlexPovarReSharperHelpers";

    public ReSharperHelpersOptionsPage(Lifetime lifetime, [NotNull] OptionsPageContext optionsPageContext, [NotNull] OptionsSettingsSmartContext settingsSmart) :
      base(lifetime, optionsPageContext, settingsSmart)
    {
      AddControl(
        BeControls
          .GetGrid(
            GridOrientation.Vertical,
            (BeControls.GetSpacer(), BeSizingType.Fit),
            (BeControls.GetRichText("It's recommended to save project specific settings to .editorconfig file instead of configuring them here."), BeSizingType.Fit),
            // Temporarily remove for 2024.1.x due to breaking change: https://youtrack.jetbrains.com/issue/RSRP-497631
            // (BeControls.GetLinkButton("Read documentation", lifetime, () => Process.Start("https://github.com/zvirja/ReSharperHelpers#editor-config")), BeSizingType.Fit),
            (BeControls.GetSpacer(), BeSizingType.Fit)
          )
          .WithColor(JetRgbaColors.LightYellow)
          .WithTitledBorder("⚠ Use Editor Config", 5, BeMargins.Create(5))
      );
      AddSpacer();

      var projectNameProperty = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestsProjectName);
      AddHeader("Test project name [deprecated, use .editorconfig]");
      AddControl(projectNameProperty.GetBeTextBox(lifetime));
      AddRichText(new RichText()
        .AppendLine("This setting is used to explicitly specify single project in solution where all unit tests are located.")
        .Append("Save it to .editorconfig file instead", new TextStyle(JetFontStyles.Bold)).AppendLine()
      );
      AddSpacer();

      var testClassNameSuffix = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestClassNameSuffix);
      AddHeader("New test class name suffix");
      AddControl(testClassNameSuffix.GetBeTextBox(lifetime));
      AddRichText(new RichText()
        .AppendLine("This setting specifies test class name suffix that should be used for the new tests.")
        .Append("Consider saving this setting to .editorconfig file or solution specific layer.", new TextStyle(JetFontStyles.Italic)));

      AddSpacer();

      var validTestClassNameSuffixes = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.ValidTestClassNameSuffixes);
      AddHeader("Existing test class name suffixes");
      AddControl(validTestClassNameSuffixes.GetBeTextBox(lifetime));
      AddRichText(new RichText()
        .AppendLine("This setting defines additional test suffixes valid for discovering the existing test classes.")
        .AppendLine("Suffixes should be delimited by comma (e.g. value1,value2).")
        .Append("Consider saving this setting to .editorconfig file or solution specific layer.", new TextStyle(JetFontStyles.Italic)));
    }
  }
}
