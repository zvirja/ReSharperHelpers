using System.Diagnostics;
using System.Drawing;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using FontStyle = System.Drawing.FontStyle;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [OptionsPage(PID, "ReSharper Helpers", typeof(MainThemedIcons.HelpersContextAction), ParentId = ToolsPage.PID)]
  public class ReSharperHelpersOptionsPage : BeSimpleOptionsPage, IOptionsPage
  {
    // ReSharper disable once InconsistentNaming
    private const string PID = "AlexPovarReSharperHelpers";

    public ReSharperHelpersOptionsPage(Lifetime lifetime, [NotNull] OptionsPageContext optionsPageContext, [NotNull] OptionsSettingsSmartContext settingsSmart) :
      base(lifetime, optionsPageContext, settingsSmart)
    {
      this.AddControl(
        BeControls
          .GetGrid(
            GridOrientation.Vertical,
            (BeControls.GetSpacer(), BeSizingType.Fit),
            (BeControls.GetRichText("It's recommended to save project specific settings to .editorconfig file instead of configuring them here."), BeSizingType.Fit),
            (BeControls.GetLinkButton("Read documentation", lifetime, () => Process.Start("https://github.com/zvirja/ReSharperHelpers#editor-config")), BeSizingType.Fit),
            (BeControls.GetSpacer(), BeSizingType.Fit)
          )
          .WithColor(Color.LightYellow)
          .InGroupBox("⚠ Use Editor Config")
      );
      this.AddSpacer();

      var projectNameProperty = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestsProjectName);
      this.AddHeader("Test project name [deprecated, use .editorconfig]");
      this.AddControl(projectNameProperty.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .AppendLine("This setting is used to explicitly specify single project in solution where all unit tests are located.")
        .Append("The setting has less priority than ")
        .Append("[assembly: AssemblyMetadata(\"ReSharperHelpers.TestProject\", \"<name>\")]) ", new TextStyle(FontStyle.Italic))
        .Append("project attribute.", TextStyle.Default).AppendLine()
        .Append("Save it to .editorconfig file instead", new TextStyle(FontStyle.Bold)).AppendLine()
      );
      this.AddSpacer();

      var testClassNameSuffix = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestClassNameSuffix);
      this.AddHeader("New test class name suffix");
      this.AddControl(testClassNameSuffix.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .AppendLine("This setting specifies test class name suffix that should be used for the new tests.")
        .Append("Consider saving this setting to .editorconfig file or solution specific layer.", new TextStyle(FontStyle.Italic)));

      this.AddSpacer();

      var validTestClassNameSuffixes = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.ValidTestClassNameSuffixes);
      this.AddHeader("Existing test class name suffixes");
      this.AddControl(validTestClassNameSuffixes.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .AppendLine("This setting defines additional test suffixes valid for discovering the existing test classes.")
        .AppendLine("Suffixes should be delimited by comma (e.g. value1,value2).")
        .Append("Consider saving this setting to .editorconfig file or solution specific layer.", new TextStyle(FontStyle.Italic)));
    }
  }
}
