using System;

public class ThresholdTrigger<T> where T : IComparable<T> {
  private readonly T threshold;
  private readonly Action action;
  private bool triggered = false;
  private bool wasBelow = true;

  public ThresholdTrigger(T threshold, Action action) {
    this.threshold = threshold;
    this.action = action;
  }

  public void Update(T value) {
    bool isAbove = value.CompareTo(threshold) > 0;
    if (wasBelow && isAbove && !triggered) {
      triggered = true;
      action();
    }

    wasBelow = !isAbove;
  }

  public void Reset() {
    triggered = false;
    wasBelow = true;
  }
}
