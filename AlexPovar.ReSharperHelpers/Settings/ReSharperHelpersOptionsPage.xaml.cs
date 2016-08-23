using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Options;
using JetBrains.UI.Options.OptionPages.ToolsPages;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [OptionsPage(PID, "Alex Povar ReSharper Helpers", typeof(MainThemedIcons.HelpersContextAction), ParentId = ToolsPage.PID)]
  public partial class ReSharperHelpersOptionsPage : IOptionsPage
  {
    // ReSharper disable once InconsistentNaming
    private const string PID = "AlexPovarReSharperHelpers";

    public ReSharperHelpersOptionsPage([NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext settingsSmart)
    {
      this.InitializeComponent();

      this.DataContext = new ReSharperHelpersOptionsPageViewModel(lifetime, settingsSmart);

      this.Control = this;
    }

    public bool OnOk() => true;

    public bool ValidatePage() => true;

    public EitherControl Control { get; }

    public string Id => PID;
  }
}