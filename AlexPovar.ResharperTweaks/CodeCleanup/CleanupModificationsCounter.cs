namespace AlexPovar.ResharperTweaks.CodeCleanup
{
  public class CleanupModificationsCounter
  {
    public int Count { get; private set; }

    public void Increment() => this.Count++;
  }
}