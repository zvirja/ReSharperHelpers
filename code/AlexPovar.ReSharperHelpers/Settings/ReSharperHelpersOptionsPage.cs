using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.UI.RichText;
using FontStyle = System.Drawing.FontStyle;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [OptionsPage(PID, "Alex Povar ReSharper Helpers", typeof(MainThemedIcons.HelpersContextAction), ParentId = ToolsPage.PID)]
  public class ReSharperHelpersOptionsPage : BeSimpleOptionsPage, IOptionsPage
  {
    // ReSharper disable once InconsistentNaming
    private const string PID = "AlexPovarReSharperHelpers";

    public ReSharperHelpersOptionsPage(Lifetime lifetime, [NotNull] OptionsPageContext optionsPageContext, [NotNull] OptionsSettingsSmartContext settingsSmart) :
      base(lifetime, optionsPageContext, settingsSmart)
    {
      var projectNameProperty = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestsProjectName);
      this.AddHeader("Test project name");
      this.AddControl(projectNameProperty.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .Append("This setting is used to explicitly specify single project in solution where all unit tests are located.\r\n")
        .Append("The setting has less priority than ")
        .Append("[assembly: AssemblyMetadata(\"ReSharperHelpers.TestProject\", \"<name>\")]) ", new TextStyle(FontStyle.Italic))
        .Append("project attribute.\r\n", TextStyle.Default)
        .Append("Ensure to save this setting to solution specific layer.", new TextStyle(FontStyle.Bold)));

      this.AddSpacer();

      var testClassNameSuffix = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.TestClassNameSuffix);
      this.AddHeader("Test class name suffix");
      this.AddControl(testClassNameSuffix.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .Append("This setting specifies test class name suffix that should be used for the new tests.\r\n")
        .Append("Consider saving this setting to solution specific layer.", new TextStyle(FontStyle.Italic)));
 
      this.AddSpacer();

      var validTestClassNameSuffixes = settingsSmart.GetValueProperty(lifetime, (ReSharperHelperSettings settings) => settings.ValidTestClassNameSuffixes);
      this.AddHeader("Valid test class name suffixes");
      this.AddControl(validTestClassNameSuffixes.GetBeTextBox(lifetime));
      this.AddRichText(new RichText()
        .Append("This setting defines additional test suffixes valid for discovering the existing test classes.\r\n")
        .Append("Suffixes should be delimited by comma (e.g. value1,value2).\r\n")
        .Append("Consider saving this setting to solution specific layer.", new TextStyle(FontStyle.Italic)));
    }
  }
}