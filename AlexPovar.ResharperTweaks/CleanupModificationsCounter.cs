namespace AlexPovar.ResharperTweaks
{
  public class CleanupModificationsCounter
  {
    public int Count { get; private set; }

    public void Increment() => Count++;
  }
}