namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.AssertParametersNotNullActionTests
{
  public class CorrectOverloadIsSelected
  {
    void Method(object ar{caret:Assert:parameter:is:not:null}g)
    {
    }
  }

  public static class Assert
  {
    public static void ArgumentNotNull(int value, string name);
    public static void ArgumentNotNull(string value, string name);
    public static void ArgumentNotNull(double value, string name);
    public static void ArgumentNotNull(object value, string name);
  }
}