using Framework.DependencyInversion;

public interface ILongTask<TResultData>
{
    void BeginTask();
}
public class SelectionController : DependencyInversionScriptableObjectNode
{
    public void BeginTask()
    {

    }
}