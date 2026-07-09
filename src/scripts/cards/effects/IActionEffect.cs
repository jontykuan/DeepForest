namespace DeepForest.Cards.Effects
{
    public interface IActionEffect
    {
        ActionResult Execute(ActionContext context);
        bool CanExecute(ActionContext context);
    }
}
