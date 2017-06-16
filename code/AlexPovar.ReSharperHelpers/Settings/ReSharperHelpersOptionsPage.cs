using AlexPovar.ReSharperHelpers.UI;
using JetBrains.Annotations;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.UIAutomation;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Feature.Services.OptionPages.CodeEditing;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [OptionsPage(PID, "Alex Povar ReSharper Helpers", typeof(MainThemedIcons.HelpersContextAction), ParentId = CodeEditingPage.PID)]
  public class ReSharperHelpersOptionsPage : IOptionsPage
  {
    // ReSharper disable once InconsistentNaming
    private const string PID = "AlexPovarReSharperHelpers";

    public ReSharperHelpersOptionsPage([NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext settingsSmart)
    {
      var control = new ReSharperHelpersOptionsPageMarkup
      {
        DataContext = new ReSharperHelpersOptionsPageViewModel(lifetime, settingsSmart)
      };

      this.Control = control;
    }

    public bool OnOk() => true;

    public bool ValidatePage() => true;

    public EitherControl Control { get; }

    public string Id => PID;
  }
}